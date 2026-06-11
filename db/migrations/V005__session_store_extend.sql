-- ============================================================
-- V005 — josyn.SessionStore — Extended fields
-- ============================================================
-- Adds execution context, lifecycle, and audit columns.
-- Package: JOSYN.Backend.SessionStore
-- ============================================================

USE [josyn-db-local];
GO

ALTER TABLE [josyn].[SessionStore]
    ADD [JobVersion]        VARCHAR(24)  NOT NULL CONSTRAINT [DF_Session_JobVersion]        DEFAULT (''),
        [UserName]          VARCHAR(64)  NOT NULL CONSTRAINT [DF_Session_UserName]          DEFAULT (''),
        [UserDomain]        VARCHAR(32)  NOT NULL CONSTRAINT [DF_Session_UserDomain]        DEFAULT (''),
        [ClientApplication] VARCHAR(128) NOT NULL CONSTRAINT [DF_Session_ClientApplication] DEFAULT (''),
        [ClientMachine]     VARCHAR(64)  NOT NULL CONSTRAINT [DF_Session_ClientMachine]     DEFAULT (''),
        [TecUser]           VARCHAR(64)  NULL,
        [Started]           DATETIME2    NOT NULL CONSTRAINT [DF_Session_Started]           DEFAULT (GETUTCDATE()),
        [ExecutionStatus]   VARCHAR(32)  NOT NULL CONSTRAINT [DF_Session_ExecutionStatus]   DEFAULT ('pending'),
        [Progress]          VARCHAR(512) NULL,
        [Finished]          DATETIME2    NULL,
        [JapServerProcess]  INT          NOT NULL CONSTRAINT [DF_Session_JapServerProcess]  DEFAULT ((0)),
        [JobHostProcessId]  INT          NOT NULL CONSTRAINT [DF_Session_JobHostProcessId]  DEFAULT ((0)),
        [JapExitCode]       INT          NOT NULL CONSTRAINT [DF_Session_JapExitCode]       DEFAULT ((0)),
        [JobExitCode]       INT          NOT NULL CONSTRAINT [DF_Session_JobExitCode]       DEFAULT ((0)),
        [LastWriteTime]     DATETIME2    NULL,
        [WrittenBy]         VARCHAR(64)  NULL;
GO
