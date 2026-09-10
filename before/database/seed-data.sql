-- Derivco Training Seed Data
-- Module 8: Production Reality — Tooling, Containers, and AI
--
-- Same seed data as Module 6. Run after schema.sql.

SET QUOTED_IDENTIFIER ON
GO

USE DerivcoTraining;
GO

INSERT INTO Players (Username, Balance, IsActive, Region) VALUES
    ('player_za_001', 500.00,   1, 'ZA'),
    ('player_za_002', 1200.50,  1, 'ZA'),
    ('player_uk_001', 750.00,   1, 'UK'),
    ('player_uk_002', 0.00,     0, 'UK'),
    ('player_eu_001', 3000.00,  1, 'EU'),
    ('player_eu_002', 150.75,   1, 'EU');

INSERT INTO Transactions (PlayerId, Amount, TransactionType) VALUES
    (1, 500.00,   'Deposit'),
    (1, -50.00,   'Withdrawal'),
    (2, 1200.50,  'Deposit'),
    (3, 1000.00,  'Deposit'),
    (3, -250.00,  'Withdrawal'),
    (5, 3000.00,  'Deposit'),
    (6, 200.00,   'Deposit'),
    (6, -49.25,   'Withdrawal');
GO
