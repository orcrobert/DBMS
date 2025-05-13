--------------------------------------------------------------------------------
-- Non-Repeatable Reads Demonstration
--
-- How to use:
-- 1. Run the "SETUP DATA" section once to ensure the test band exists.
-- 2. To demonstrate the issue:
--    a. Copy the "QUERY WINDOW 1 (TRANSACTION 1 - Demonstrate NRR)" script into SSMS Query Window 1.
--    b. Copy the "QUERY WINDOW 2 (TRANSACTION 2 - Modifier)" script into SSMS Query Window 2.
--    c. Execute T1 up to its first WAITFOR DELAY.
--    d. Execute T2 completely.
--    e. Allow T1 to complete. Observe the different values read.
-- 3. To demonstrate the fix:
--    a. Ensure the "SETUP DATA" section has reset the band name.
--    b. Copy the "QUERY WINDOW 1 (TRANSACTION 1 - FIXED)" script into SSMS Query Window 1.
--    c. Use "QUERY WINDOW 2 (TRANSACTION 2 - Modifier)" script in SSMS Query Window 2.
--    d. Execute T1 (FIXED) up to its WAITFOR DELAY.
--    e. Execute T2. Observe T2 waits.
--    f. Allow T1 (FIXED) to complete. Observe T1 reads the same value twice. Then T2 completes.
--------------------------------------------------------------------------------
USE Metal;
GO

--------------------------------------------------------------------------------
-- SETUP DATA for Non-Repeatable Read Test
--------------------------------------------------------------------------------
PRINT N'====================================================================';
PRINT N'SETUP DATA for Non-Repeatable Read Test';
PRINT N'====================================================================';

DECLARE @NrrTestBandName VARCHAR(255) = 'NRR Test Band';
DECLARE @NrrTestBandNameUpdated VARCHAR(255) = 'NRR Band - Updated by T2';
DECLARE @TestBandId_NRR INT;

-- Clean up from potential previous T2 update
IF EXISTS (SELECT 1 FROM dbo.Bands WHERE BandName = @NrrTestBandNameUpdated)
BEGIN
    DELETE FROM dbo.Bands WHERE BandName = @NrrTestBandNameUpdated;
    PRINT CONCAT('Cleaned up band: ', @NrrTestBandNameUpdated);
END

-- Find or create/reset the primary test band
SELECT @TestBandId_NRR = BandId FROM dbo.Bands WHERE BandName = @NrrTestBandName;

IF @TestBandId_NRR IS NULL
BEGIN
    INSERT INTO dbo.Bands (BandName) VALUES (@NrrTestBandName);
    SET @TestBandId_NRR = SCOPE_IDENTITY();
    PRINT CONCAT('Inserted band ''', @NrrTestBandName, ''' for NRR tests with BandId: ', @TestBandId_NRR);
END
ELSE
BEGIN
    UPDATE dbo.Bands SET BandName = @NrrTestBandName WHERE BandId = @TestBandId_NRR; -- Ensure original name
    PRINT CONCAT('Reset band ''', @NrrTestBandName, ''' for NRR tests. BandId: ', @TestBandId_NRR);
END
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 1 (TRANSACTION 1 - Demonstrate Non-Repeatable Read)
-- Instructions: Run this in Query Window 1. Execute up to the first WAITFOR DELAY,
-- then run Transaction 2, then let this complete.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @NrrTestBandName_T1 VARCHAR(255) = 'NRR Test Band';
DECLARE @BandId_T1 INT;

SELECT @BandId_T1 = BandId FROM dbo.Bands WHERE BandName = @NrrTestBandName_T1;

IF @BandId_T1 IS NULL
BEGIN
    PRINT 'Transaction 1: Test band not found. Please run SETUP DATA section.';
    RETURN;
END

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
PRINT 'Transaction 1: Isolation Level set to READ COMMITTED.';

BEGIN TRANSACTION;
PRINT 'Transaction 1: Transaction started.';

PRINT CONCAT('Transaction 1: First read of BandName for BandId: ', @BandId_T1, '...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1;
PRINT 'Transaction 1: First read complete.';

PRINT 'Transaction 1: Waiting for 10 seconds to allow T2 to update and commit...';
WAITFOR DELAY '00:00:20';

PRINT CONCAT('Transaction 1: Second read of BandName for BandId: ', @BandId_T1, '...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1;
PRINT 'Transaction 1: Second read complete. (Value might be different!)';

COMMIT TRANSACTION;
PRINT 'Transaction 1: Transaction committed.';
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 2 (TRANSACTION 2 - Modifier)
-- Instructions: Run this in Query Window 2 AFTER Transaction 1 has done its first read and is waiting.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @NrrTestBandName_T2 VARCHAR(255) = 'NRR Test Band';
DECLARE @NrrTestBandNameUpdated_T2 VARCHAR(255) = 'NRR Band - Updated by T2';
DECLARE @BandId_T2 INT;

SELECT @BandId_T2 = BandId FROM dbo.Bands WHERE BandName = @NrrTestBandName_T2;

IF @BandId_T2 IS NULL
BEGIN
    PRINT 'Transaction 2: Test band not found. Please run SETUP DATA section.';
    RETURN;
END

PRINT 'Transaction 2: Transaction starting...';
BEGIN TRANSACTION;
PRINT 'Transaction 2: Transaction started.';

UPDATE dbo.Bands
SET BandName = @NrrTestBandNameUpdated_T2
WHERE BandId = @BandId_T2;
PRINT CONCAT('Transaction 2: BandName updated to ''', @NrrTestBandNameUpdated_T2, ''' for BandId: ', @BandId_T2);

COMMIT TRANSACTION;
PRINT 'Transaction 2: Transaction committed.';

PRINT CONCAT('Transaction 2: Verifying BandName for BandId: ', @BandId_T2, ' after commit...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T2;
GO

--------------------------------------------------------------------------------
-- QUERY WINDOW 1 (TRANSACTION 1 - FIXED with REPEATABLE READ)
-- Instructions: After resetting data with SETUP DATA, run this in Query Window 1.
-- Execute up to the WAITFOR DELAY, then run Transaction 2 (it should block), then let this complete.
--------------------------------------------------------------------------------

USE Metal;
GO

DECLARE @NrrTestBandName_T1_Fixed VARCHAR(255) = 'NRR Test Band';
DECLARE @BandId_T1_Fixed INT;

SELECT @BandId_T1_Fixed = BandId FROM dbo.Bands WHERE BandName = @NrrTestBandName_T1_Fixed;

IF @BandId_T1_Fixed IS NULL
BEGIN
    PRINT 'Transaction 1 (FIXED): Test band not found. Please run SETUP DATA section.';
    RETURN;
END

SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
PRINT 'Transaction 1 (FIXED): Isolation Level set to REPEATABLE READ.';

BEGIN TRANSACTION;
PRINT 'Transaction 1 (FIXED): Transaction started.';

PRINT CONCAT('Transaction 1 (FIXED): First read of BandName for BandId: ', @BandId_T1_Fixed, '...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1_Fixed;
PRINT 'Transaction 1 (FIXED): First read complete.';

PRINT 'Transaction 1 (FIXED): Waiting for 10 seconds. T2 will attempt to update now...';
WAITFOR DELAY '00:00:20';

PRINT CONCAT('Transaction 1 (FIXED): Second read of BandName for BandId: ', @BandId_T1_Fixed, '...');
SELECT BandId, BandName
FROM dbo.Bands
WHERE BandId = @BandId_T1_Fixed;
PRINT 'Transaction 1 (FIXED): Second read complete. (Value should be the same!)';

COMMIT TRANSACTION;
PRINT 'Transaction 1 (FIXED): Transaction committed.';

-- Reset isolation level to default for the session
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
PRINT 'Transaction 1 (FIXED): Isolation Level reset to READ COMMITTED.';
GO
