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
