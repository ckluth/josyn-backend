-- ============================================================
-- JOSYN Local Development Database Bootstrap
-- ============================================================
-- PURPOSE : Sets up a fresh local SQL Server instance for
--           JOSYN development. Creates the database, schema,
--           login, and all current tables.
--
-- AUDIENCE: Developers only. Never run in production.
-- USAGE   : Run once on a fresh local SQL Server instance.
--           For incremental updates to an existing dev DB,
--           apply the relevant migration(s) from migrations/.
-- ============================================================

USE [master];
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'josyn-db-local')
BEGIN
    ALTER DATABASE [josyn-db-local] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [josyn-db-local];
END
GO

CREATE DATABASE [josyn-db-local];
GO

IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'tu.josyn')
    DROP LOGIN [tu.josyn];
GO

CREATE LOGIN [tu.josyn]
    WITH PASSWORD     = 'josyn',
         CHECK_POLICY     = OFF,
         CHECK_EXPIRATION = OFF;
GO

USE [josyn-db-local];
GO

CREATE USER [tu.josyn]
    FOR LOGIN [tu.josyn];
GO

ALTER ROLE [db_owner]
    ADD MEMBER [tu.josyn];
GO

CREATE SCHEMA [josyn];
GO

-- ── V001 — josyn.SessionStore ────────────────────────────────
CREATE TABLE [josyn].[SessionStore]
(
    [Id]          INT              NOT NULL IDENTITY(1,1),
    [UID]         UNIQUEIDENTIFIER NOT NULL,
    [JobTypeName] NVARCHAR(256)    NOT NULL,
    [Arguments]   NVARCHAR(MAX)    NOT NULL,
    [Result]      NVARCHAR(MAX)    NOT NULL,

    CONSTRAINT [PK_SessionStore] PRIMARY KEY CLUSTERED ([Id])
);
GO

-- ── V002 — josyn.JobRegistry ────────────────────────────────
CREATE TABLE [josyn].[JobRegistry]
(
    [Id]                INT           NOT NULL IDENTITY(1,1),
    [Name]              NVARCHAR(256) NOT NULL,
    [TechnicalUserName] NVARCHAR(256) NOT NULL,

    CONSTRAINT [PK_JobRegistry]      PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_JobRegistry_Name] UNIQUE ([Name])
);
GO

-- FK: every session must reference a registered job
ALTER TABLE [josyn].[SessionStore]
    ADD CONSTRAINT [FK_SessionStore_JobRegistry]
    FOREIGN KEY ([JobTypeName]) REFERENCES [josyn].[JobRegistry] ([Name]);
GO
