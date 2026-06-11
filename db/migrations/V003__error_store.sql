-- ============================================================
-- V003 — josyn.ErrorStore
-- ============================================================
-- Creates the error record table for the JOSYN Storage Realm.
-- See josyn-platform/repos/josyn-backend/decisions/ADR-006-error-handler.md
--
-- No FKs to JobRegistry or SessionStore: error records are
-- archival and must outlive the entities they reference.
-- ============================================================

SET QUOTED_IDENTIFIER ON;
GO

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
