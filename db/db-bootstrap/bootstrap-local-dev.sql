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

SET QUOTED_IDENTIFIER ON;
GO

CREATE USER [tu.josyn]
    FOR LOGIN [tu.josyn];
GO

ALTER ROLE [db_owner]
    ADD MEMBER [tu.josyn];
GO

CREATE SCHEMA [josyn];
GO

-- ── V001 + V005 — josyn.SessionStore ────────────────────────────────
CREATE TABLE [josyn].[SessionStore]
(
    [Id]                INT              NOT NULL IDENTITY(1,1),
    [UID]               UNIQUEIDENTIFIER NOT NULL,
    [JobTypeName]       NVARCHAR(256)    NOT NULL,
    [Arguments]         NVARCHAR(MAX)    NOT NULL,
    [Result]            NVARCHAR(MAX)    NOT NULL,
    [JobVersion]        VARCHAR(24)      NOT NULL,
    [UserName]          VARCHAR(64)      NOT NULL,
    [UserDomain]        VARCHAR(32)      NOT NULL,
    [ClientApplication] VARCHAR(128)     NOT NULL,
    [ClientMachine]     VARCHAR(64)      NOT NULL,
    [TecUser]           VARCHAR(64)      NULL,
    [Started]           DATETIME2        NOT NULL,
    [ExecutionStatus]   VARCHAR(32)      NOT NULL,
    [Progress]          VARCHAR(512)     NULL,
    [Finished]          DATETIME2        NULL,
    [JapServerProcessId] INT              NOT NULL CONSTRAINT [DF_Session_JapServerProcessId] DEFAULT ((0)),
    [JobHostProcessId]   INT              NOT NULL CONSTRAINT [DF_Session_JobHostProcessId]  DEFAULT ((0)),
    [LastWriteTime]      DATETIME2        NULL,
    [Host]              VARCHAR(64)      NULL,

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

-- ── V006 — josyn.ArgumentRecords ───────────────────────────────────
-- Named INI argument payloads per job. 0-to-n records per job.
-- "default" is the conventional name for the single-payload case (no enforcement).
-- Content is stored verbatim; parsing is the caller's responsibility.
-- See ADR-028.
CREATE TABLE [josyn].[ArgumentRecords]
(
    [JobName] NVARCHAR(256) NOT NULL,
    [Name]    NVARCHAR(128) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_ArgumentRecords] PRIMARY KEY CLUSTERED ([JobName], [Name]),
    CONSTRAINT [FK_ArgumentRecords_JobRegistry]
        FOREIGN KEY ([JobName]) REFERENCES [josyn].[JobRegistry] ([Name])
);
GO

-- ── V007 — josyn.JobSchedules ────────────────────────────────────────
-- Per-job scheduling header. One row per job that has any time-based schedule.
-- Jobs with no row here are never launched by TimeScheduler.
-- See ADR-027.
CREATE TABLE [josyn].[JobSchedules]
(
    [JobName]        NVARCHAR(256) NOT NULL,
    [Suspended]      BIT           NOT NULL CONSTRAINT [DF_JobSchedules_Suspended] DEFAULT (0),
    [SuspendedUntil] DATE          NULL,

    CONSTRAINT [PK_JobSchedules] PRIMARY KEY CLUSTERED ([JobName]),
    CONSTRAINT [FK_JobSchedules_JobRegistry]
        FOREIGN KEY ([JobName]) REFERENCES [josyn].[JobRegistry] ([Name])
);
GO

-- ── V008 — josyn.JobScheduleEntries ──────────────────────────────────
-- One row per independently scheduled invocation of a job.
-- Each entry pairs a ScheduleDefinition (ADR-026 JSONC) with a named argument record.
-- The FK to ArgumentRecords is logical only — enforced at the application layer.
-- See ADR-027.
CREATE TABLE [josyn].[JobScheduleEntries]
(
    [JobName]              NVARCHAR(256) NOT NULL,
    [ArgumentRecordName]   NVARCHAR(128) NOT NULL,
    [ScheduleDefinition]   NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_JobScheduleEntries] PRIMARY KEY CLUSTERED ([JobName], [ArgumentRecordName]),
    CONSTRAINT [FK_JobScheduleEntries_JobSchedules]
        FOREIGN KEY ([JobName]) REFERENCES [josyn].[JobSchedules] ([JobName])
);
GO


-- Registers the CLI demo job so bootstrap-local-dev is
-- immediately usable without a manual INSERT.
INSERT INTO [josyn].[JobRegistry] ([Name], [TechnicalUserName])
VALUES ('Contoso.DemoProduct.DemoJob', 'tu.job.default@LENOVO-LEGION');
GO

INSERT INTO [josyn].[ArgumentRecords] ([JobName], [Name], [Content])
VALUES ('Contoso.DemoProduct.DemoJob', 'default',
'; DemoArguments
Message=Hello contoso-demo-job
RepeatCount=0
ScheduledFor=19.06.2026
IsHighPriority=False
Budget=0,00');
GO

INSERT INTO [josyn].[ConfigStore] ([Key], [Value])
VALUES ('RuntimeEnvironment', 'DEV');
GO
















