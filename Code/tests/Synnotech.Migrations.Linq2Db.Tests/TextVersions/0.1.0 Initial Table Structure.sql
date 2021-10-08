﻿IF OBJECT_ID('dbo.MigrationInfos', 'U') IS NULL
BEGIN

CREATE TABLE dbo.MigrationInfos(
	Id INT IDENTITY(1, 1) CONSTRAINT PK_MigrationInfos PRIMARY KEY CLUSTERED,
	[Name] NVARCHAR(100) NOT NULL,
	[Version] NVARCHAR(20) CONSTRAINT U_MigrationInfos_Version NOT NULL,
	[AppliedAt] DATETIME2 NOT NULL
);

END;

CREATE TABLE dbo.MasterData(
	Id INT IDENTITY(1, 1) CONSTRAINT PK_MasterData PRIMARY KEY CLUSTERED,
	[Value] INT NOT NULL
);