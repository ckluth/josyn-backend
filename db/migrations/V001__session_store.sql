-- ============================================================
-- V001 — josyn.SessionStore
-- ============================================================
-- Stores one record per job execution session.
-- Package: JOSYN.Backend.SessionStore
-- ============================================================

USE [josyn-db-local];
GO

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
