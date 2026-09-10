-- Derivco Training Database Schema
-- Module 8: Production Reality — Tooling, Containers, and AI
--
-- Same schema as Module 6. Run this once against the SQL Server container
-- before starting the lab. It creates the database, so connect to master:
--   sqlcmd ... -d master -i /scripts/schema.sql

SET QUOTED_IDENTIFIER ON
GO

IF DB_ID('DerivcoTraining') IS NULL
    CREATE DATABASE DerivcoTraining;
GO

USE DerivcoTraining;
GO

DROP TABLE IF EXISTS Transactions;
DROP TABLE IF EXISTS Players;
GO

CREATE TABLE Players (
    PlayerId        INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50)      NOT NULL UNIQUE,
    Balance         DECIMAL(18,2)     NOT NULL DEFAULT 0.00,
    IsActive        BIT               NOT NULL DEFAULT 1,
    Region          NVARCHAR(10)      NOT NULL,
    CreatedAt       DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2         NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Transactions (
    TransactionId   INT IDENTITY(1,1) PRIMARY KEY,
    PlayerId        INT               NOT NULL REFERENCES Players(PlayerId),
    Amount          DECIMAL(18,2)     NOT NULL,
    TransactionType NVARCHAR(20)      NOT NULL,  -- 'Transfer', 'Deposit', 'Withdrawal'
    CreatedAt       DATETIME2         NOT NULL DEFAULT GETUTCDATE()
);

-- Indexes for common query patterns
CREATE INDEX IX_Transactions_PlayerId ON Transactions(PlayerId);
CREATE INDEX IX_Players_Region ON Players(Region) WHERE IsActive = 1;
GO
