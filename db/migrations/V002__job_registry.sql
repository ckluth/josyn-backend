-- ============================================================
-- V002 — josyn.JobRegistry
-- ============================================================
-- Stores the JOSYN job registry: platform-wide unique job
-- names and the technical user identity for each job.
-- Package: JOSYN.Backend.JobRegistry
-- ============================================================

USE [josyn-db-local];
GO

CREATE TABLE [josyn].[JobRegistry]
(
    [Id]                INT           NOT NULL IDENTITY(1,1),
    [Name]              NVARCHAR(256) NOT NULL,
    [TechnicalUserName] NVARCHAR(256) NOT NULL,

    CONSTRAINT [PK_JobRegistry]      PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_JobRegistry_Name] UNIQUE ([Name])
);
GO

-- Every session must reference a registered job
ALTER TABLE [josyn].[SessionStore]
    ADD CONSTRAINT [FK_SessionStore_JobRegistry]
    FOREIGN KEY ([JobTypeName]) REFERENCES [josyn].[JobRegistry] ([Name]);
GO
