USE master
GO
DROP DATABASE PublicData
GO
CREATE DATABASE PublicData
GO
USE PublicData
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SignedDocuments')
BEGIN
	CREATE TABLE dbo.SignedDocuments(
		Id uniqueidentifier NOT NULL PRIMARY KEY DEFAULT(newid()),
		DocumentId uniqueidentifier NOT NULL,
		Content nvarchar(max) NOT NULL,
		SignedAtUtc datetime2(7) NOT NULL DEFAULT(GETUTCDATE())
	) 
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UnsignedDocuments')
BEGIN
	CREATE TABLE dbo.UnsignedDocuments(
		Id uniqueidentifier NOT NULL PRIMARY KEY DEFAULT(newid()),
		Content nvarchar(max) NOT NULL,
		CreatedAtUtc datetime2(7) NOT NULL DEFAULT(GETUTCDATE())
	)
END
GO


USE master
GO
DROP DATABASE KeyStore
GO
CREATE DATABASE KeyStore
GO

USE KeyStore;
GO

-- Create the Keys table if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Keys')
BEGIN
    CREATE TABLE dbo.Keys(
        Id uniqueidentifier NOT NULL PRIMARY KEY DEFAULT(NEWID()),
        PublicKey nvarchar(max) NULL,
        PrivateKey nvarchar(max) NULL,
        IsLocked bit NOT NULL DEFAULT(0),
        ModifiedAtutc datetime2(7) NOT NULL DEFAULT(GETUTCDATE())
    );
END
GO
