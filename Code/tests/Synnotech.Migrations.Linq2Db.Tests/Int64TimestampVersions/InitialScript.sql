IF OBJECT_ID('dbo.MigrationInfos', 'U') IS NULL
BEGIN

CREATE TABLE [dbo].[MigrationInfos] (
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [Version] BIGINT CONSTRAINT UQ_MigrationInfos UNIQUE NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [AppliedAt] DATETIME2 NOT NULL
);

END;

CREATE TABLE [dbo].[Contacts] (
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL
);