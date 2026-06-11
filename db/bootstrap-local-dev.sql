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

-- ── V003 — josyn.ErrorStore ─────────────────────────────────
-- No FKs to JobRegistry or SessionStore: error records are
-- archival and must outlive the entities they reference.
CREATE TABLE [josyn].[ErrorStore]
(
    [Id]               INT              NOT NULL IDENTITY(1,1),
    [UID]              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    [OccurredAt]       DATETIMEOFFSET   NOT NULL,
    [Causer]           NVARCHAR(256)    NOT NULL,
    [Message]          NVARCHAR(MAX)    NOT NULL,
    [CallStack]        NVARCHAR(MAX)    NULL,
    [ExceptionDetails] NVARCHAR(MAX)    NULL,
    [JobName]          NVARCHAR(256)    NULL,
    [SessionGuid]      UNIQUEIDENTIFIER NULL,

    CONSTRAINT [PK_ErrorStore]     PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_ErrorStore_UID] UNIQUE ([UID])
);
GO

CREATE INDEX [IX_ErrorStore_OccurredAt]
    ON [josyn].[ErrorStore] ([OccurredAt] DESC);
GO

CREATE INDEX [IX_ErrorStore_JobName]
    ON [josyn].[ErrorStore] ([JobName])
    WHERE [JobName] IS NOT NULL;
GO

CREATE INDEX [IX_ErrorStore_SessionGuid]
    ON [josyn].[ErrorStore] ([SessionGuid])
    WHERE [SessionGuid] IS NOT NULL;
GO

-- ── V004 — josyn.ConfigStore ────────────────────────────────
-- Runtime configuration for the JOSYN platform.
-- Key is the natural identifier; Value holds arbitrary string data
-- (connection strings, paths, flags, etc.).
-- This table is the built-in implementation of IConfigSource.
-- It can be replaced at runtime by a company-specific adapter (see ADR-009).
CREATE TABLE [josyn].[ConfigStore]
(
    [Id]    INT           NOT NULL IDENTITY(1,1),
    [Key]   NVARCHAR(256) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_ConfigStore]     PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_ConfigStore_Key] UNIQUE ([Key])
);
GO

-- ── Dev seed data ────────────────────────────────────────────
-- Registers the CLI demo job so bootstrap-local-dev is
-- immediately usable without a manual INSERT.
INSERT INTO [josyn].[JobRegistry] ([Name], [TechnicalUserName])
VALUES ('Contoso.DemoProduct.DemoJob', 'tu.josyn');
GO

INSERT INTO [josyn].[ConfigStore] ([Key], [Value])
VALUES ('RuntimeEnvironment', 'DEV');
GO
















