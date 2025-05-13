--------------------------------------------------------------------------------
-- Dirty Reads Demonstration
--------------------------------------------------------------------------------
USE Metal;
GO

--------------------------------------------------------------------------------
-- SETUP DATA for Dirty Read Test
--------------------------------------------------------------------------------
PRINT N'====================================================================';
PRINT N'SETUP DATA for Dirty Read Test';
PRINT N'====================================================================';

DECLARE @DrTestBandName VARCHAR(255) = 'Dirty Read Test Band'; -- DR for Dirty Read
DECLARE @DrTestBandName_Dirty VARCHAR(255) = 'DR Band - Dirty Update';
DECLARE @TestBandId_DR INT;

-- Clean up from potential previous T2 update if it was committed (it shouldn't be for dirty read)
-- And also the original test band name
DELETE FROM dbo.Bands WHERE BandName = @DrTestBandName_Dirty;
DELETE FROM dbo.Bands WHERE BandName = @DrTestBandName;
PRINT CONCAT('Cleaned up bands: ', @DrTestBandName, ' and ', @DrTestBandName_Dirty, ' (if they existed).');

-- Insert the primary test band
INSERT INTO dbo.Bands (BandName) VALUES (@DrTestBandName);
SET @TestBandId_DR = SCOPE_IDENTITY();
PRINT CONCAT('Inserted band ''', @DrTestBandName, ''' for DR tests with BandId: ', @TestBandId_DR);
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 1 (TRANSACTION 1 - Demonstrate Dirty Read)
-- Instructions: Run this in Query Window 1.
-- Execute T2 up to its WAITFOR DELAY, then execute this script.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @DrTestBandName_T1 VARCHAR(255) = 'Dirty Read Test Band';
DECLARE @BandId_T1 INT;

SELECT @BandId_T1 = BandId FROM dbo.Bands WHERE BandName = @DrTestBandName_T1;

IF @BandId_T1 IS NULL
BEGIN
    PRINT 'Transaction 1: Test band not found. Please run SETUP DATA section.';
    RETURN;
END

-- Set isolation level to READ UNCOMMITTED to allow dirty reads
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
PRINT 'Transaction 1: Isolation Level set to READ UNCOMMITTED.';

BEGIN TRANSACTION;
PRINT 'Transaction 1: Transaction started.';

PRINT CONCAT('Transaction 1: Attempting FIRST read for BandId: ', @BandId_T1, ' (expecting dirty data if T2 has updated)...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1;
PRINT 'Transaction 1: First read complete.';

-- Keep this transaction open. T2 will rollback its change.
PRINT 'Transaction 1: Waiting for 15 seconds (T2 should rollback during this time)...';
WAITFOR DELAY '00:00:15';

PRINT CONCAT('Transaction 1: Attempting SECOND read for BandId: ', @BandId_T1, ' (after T2 rollback)...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1;
PRINT 'Transaction 1: Second read complete (should show original data).';

COMMIT TRANSACTION;
PRINT 'Transaction 1: Transaction committed.';

-- Reset isolation level to default (optional, good practice if session is reused)
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
PRINT 'Transaction 1: Isolation Level reset to READ COMMITTED.';
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 2 (TRANSACTION 2 - Modifier & Rollback)
-- Instructions: Run this in Query Window 2.
-- Execute up to the WAITFOR DELAY, then run T1, then let this script complete.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @DrTestBandName_T2 VARCHAR(255) = 'Dirty Read Test Band';
DECLARE @DrTestBandName_Dirty_T2 VARCHAR(255) = 'DR Band - Dirty Update';
DECLARE @BandId_T2 INT;

SELECT @BandId_T2 = BandId FROM dbo.Bands WHERE BandName = @DrTestBandName_T2;

IF @BandId_T2 IS NULL
BEGIN
    PRINT 'Transaction 2: Test band not found. Please run SETUP DATA section.';
    RETURN;
END

PRINT 'Transaction 2: Transaction starting...';
BEGIN TRANSACTION;
PRINT 'Transaction 2: Transaction started.';

-- Transaction 2 updates the BandName but DOES NOT COMMIT
UPDATE dbo.Bands
SET BandName = @DrTestBandName_Dirty_T2
WHERE BandId = @BandId_T2;
PRINT CONCAT('Transaction 2: BandName updated to ''', @DrTestBandName_Dirty_T2, ''' for BandId: ', @BandId_T2, ' (UNCOMMITTED).');

-- Wait for a moment to allow Transaction 1 to read the dirty data
PRINT 'Transaction 2: Waiting for 10 seconds to allow T1 to perform its first read...';
WAITFOR DELAY '00:00:10';

-- Transaction 2 rolls back its changes
PRINT 'Transaction 2: Rolling back changes...';
ROLLBACK TRANSACTION;
PRINT 'Transaction 2: Transaction rolled back.';

PRINT CONCAT('Transaction 2: Verifying BandName for BandId: ', @BandId_T2, ' after rollback...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T2; -- Should show original name
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 1 (TRANSACTION 1 - FIXED for Dirty Read)
-- Instructions: After resetting data with SETUP DATA, run this in Query Window 1.
-- Execute T2 up to its UPDATE (before WAITFOR), then execute this script. T1 will wait.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @DrTestBandName_T1_Fixed VARCHAR(255) = 'Dirty Read Test Band';
DECLARE @BandId_T1_Fixed INT;

SELECT @BandId_T1_Fixed = BandId FROM dbo.Bands WHERE BandName = @DrTestBandName_T1_Fixed;

IF @BandId_T1_Fixed IS NULL
BEGIN
    PRINT 'Transaction 1 (FIXED): Test band not found. Please run SETUP DATA section.';
    RETURN;
END

-- Set isolation level to READ COMMITTED (default, but explicit here)
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
PRINT 'Transaction 1 (FIXED): Isolation Level set to READ COMMITTED.';

BEGIN TRANSACTION;
PRINT 'Transaction 1 (FIXED): Transaction started.';

-- Transaction 1 attempts to read the data.
-- It will now WAIT if Transaction 2 has an uncommitted update on that row.
PRINT CONCAT('Transaction 1 (FIXED): Attempting to read BandName for BandId: ', @BandId_T1_Fixed, ' (will wait if T2 holds lock)...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1_Fixed;
PRINT 'Transaction 1 (FIXED): Read complete (should show original/committed data).';

COMMIT TRANSACTION;
PRINT 'Transaction 1 (FIXED): Transaction committed.';
GO
