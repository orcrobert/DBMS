--------------------------------------------------------------------------------
-- Phantom Reads Demonstration
--------------------------------------------------------------------------------
USE Metal;
GO

--------------------------------------------------------------------------------
-- SETUP DATA for Phantom Read Test
--------------------------------------------------------------------------------
PRINT N'====================================================================';
PRINT N'SETUP DATA for Phantom Read Test';
PRINT N'====================================================================';

DECLARE @PhantomTestBandPrefix VARCHAR(255) = 'Phantom Band%';
DECLARE @InitialPhantomBandName VARCHAR(255) = 'Phantom Band Alpha';
DECLARE @NewPhantomBandNameT2 VARCHAR(255) = 'Phantom Band Beta (New)';

-- Clean up bands from previous phantom tests
DELETE FROM dbo.Bands WHERE BandName LIKE @PhantomTestBandPrefix;
PRINT CONCAT('Cleaned up bands with names like ''', @PhantomTestBandPrefix, ''' (if they existed).');

-- Insert an initial band that matches the query criteria
INSERT INTO dbo.Bands (BandName) VALUES (@InitialPhantomBandName);
PRINT CONCAT('Inserted initial band: ''', @InitialPhantomBandName, ''' for Phantom Read tests.');
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 1 (TRANSACTION 1 - Demonstrate Phantom Read)
-- Instructions: Run this in Query Window 1. Execute up to the WAITFOR DELAY,
-- then run Transaction 2, then let this script complete.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @PhantomTestBandPrefix_T1 VARCHAR(255) = 'Phantom Band%';

-- Set isolation level to REPEATABLE READ
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
PRINT 'Transaction 1: Isolation Level set to REPEATABLE READ.';

BEGIN TRANSACTION;
PRINT 'Transaction 1: Transaction started.';

PRINT 'Transaction 1: First query for bands LIKE ''Phantom Band%''...';
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandName LIKE @PhantomTestBandPrefix_T1;
PRINT 'Transaction 1: First query complete.';

PRINT 'Transaction 1: Waiting for 10 seconds to allow T2 to insert and commit...';
WAITFOR DELAY '00:00:10';

PRINT 'Transaction 1: Second query for bands LIKE ''Phantom Band%''...';
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandName LIKE @PhantomTestBandPrefix_T1;
PRINT 'Transaction 1: Second query complete. (A new "phantom" row might appear!)';

COMMIT TRANSACTION;
PRINT 'Transaction 1: Transaction committed.';

SET TRANSACTION ISOLATION LEVEL READ COMMITTED; -- Reset
PRINT 'Transaction 1: Isolation Level reset to READ COMMITTED.';
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 2 (TRANSACTION 2 - Inserter)
-- Instructions: Run this in Query Window 2 AFTER Transaction 1 has done its first read and is waiting.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @NewPhantomBandName_T2 VARCHAR(255) = 'Phantom Band Beta (New)';

PRINT 'Transaction 2: Transaction starting...';
BEGIN TRANSACTION;
PRINT 'Transaction 2: Transaction started.';

PRINT CONCAT('Transaction 2: Inserting new band: ''', @NewPhantomBandName_T2, '''...');
INSERT INTO dbo.Bands (BandName) VALUES (@NewPhantomBandName_T2);
PRINT 'Transaction 2: New band inserted.';

COMMIT TRANSACTION;
PRINT 'Transaction 2: Transaction committed.';

PRINT 'Transaction 2: Verifying new band insertion...';
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandName = @NewPhantomBandName_T2;
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 1 (TRANSACTION 1 - FIXED for Phantom Read with SERIALIZABLE)
-- Instructions: After resetting data with SETUP DATA, run this in Query Window 1.
-- Execute up to the WAITFOR DELAY, then run Transaction 2 (it should block), then let this complete.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @PhantomTestBandPrefix_T1_Fixed VARCHAR(255) = 'Phantom Band%';

-- Set isolation level to SERIALIZABLE to prevent phantom reads
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
PRINT 'Transaction 1 (FIXED): Isolation Level set to SERIALIZABLE.';

BEGIN TRANSACTION;
PRINT 'Transaction 1 (FIXED): Transaction started.';

PRINT 'Transaction 1 (FIXED): First query for bands LIKE ''Phantom Band%''...';
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandName LIKE @PhantomTestBandPrefix_T1_Fixed;
PRINT 'Transaction 1 (FIXED): First query complete.';

PRINT 'Transaction 1 (FIXED): Waiting for 10 seconds. T2 will attempt to insert now...';
WAITFOR DELAY '00:00:10';

PRINT 'Transaction 1 (FIXED): Second query for bands LIKE ''Phantom Band%''...';
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandName LIKE @PhantomTestBandPrefix_T1_Fixed;
PRINT 'Transaction 1 (FIXED): Second query complete. (No phantom rows should appear!)';

COMMIT TRANSACTION;
PRINT 'Transaction 1 (FIXED): Transaction committed.';

SET TRANSACTION ISOLATION LEVEL READ COMMITTED; -- Reset
PRINT 'Transaction 1 (FIXED): Isolation Level reset to READ COMMITTED.';
GO
