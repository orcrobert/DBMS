USE Metal;
GO

--------------------------------------------------------------------------------
-- Ensure ProcedureLog Table Exists (from previous exercise)
-- If running this in a completely new file/session, you might need it.
-- If it exists from the previous script, this will just be skipped or you can comment it out.
--------------------------------------------------------------------------------
IF OBJECT_ID('dbo.ProcedureLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcedureLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        ProcedureName VARCHAR(255) NOT NULL,
        ActionType VARCHAR(50), 
        Parameters VARCHAR(MAX) NULL,
        LogMessage VARCHAR(MAX) NOT NULL,
        ErrorNumber INT NULL,
        ErrorSeverity INT NULL,
        ErrorState INT NULL,
        ErrorLine INT NULL,
        LogTime DATETIME DEFAULT GETDATE()
    );
    PRINT 'dbo.ProcedureLog table created.';
END
ELSE
BEGIN
    PRINT 'dbo.ProcedureLog table already exists.';
END
GO

--------------------------------------------------------------------------------
-- Stored Procedure: usp_AddFanToBand_PartialRecovery
--------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_AddFanToBand_PartialRecovery
    @BandName VARCHAR(255),
    @FanName VARCHAR(255),
    @FanEmail VARCHAR(255),
    @ForceBandFail BIT = 0,     -- Parameter to simulate band processing failure
    @ForceFanFail BIT = 0,      -- Parameter to simulate fan processing failure
    @ForceLinkFail BIT = 0      -- Parameter to simulate link processing failure
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProcedureName VARCHAR(255) = 'usp_AddFanToBand_PartialRecovery';
    DECLARE @InputParameters VARCHAR(MAX) = CONCAT(
        '@BandName=', ISNULL(@BandName, 'NULL'),
        ', @FanName=', ISNULL(@FanName, 'NULL'),
        ', @FanEmail=', ISNULL(@FanEmail, 'NULL'),
        ', @ForceBandFail=', CAST(@ForceBandFail AS VARCHAR(1)),
        ', @ForceFanFail=', CAST(@ForceFanFail AS VARCHAR(1)),
        ', @ForceLinkFail=', CAST(@ForceLinkFail AS VARCHAR(1))
    );

    INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
    VALUES (@ProcedureName, 'Start', @InputParameters, 'Procedure execution started.');

    -- 1. Parameter Validation (same as previous procedure)
    IF @BandName IS NULL OR LTRIM(RTRIM(@BandName)) = '' BEGIN
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'ValidationError', @InputParameters, 'BandName cannot be empty.');
        RAISERROR('BandName cannot be empty.', 16, 1); RETURN; END
    IF @FanName IS NULL OR LTRIM(RTRIM(@FanName)) = '' BEGIN
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'ValidationError', @InputParameters, 'FanName cannot be empty.');
        RAISERROR('FanName cannot be empty.', 16, 1); RETURN; END
    IF @FanEmail IS NULL OR LTRIM(RTRIM(@FanEmail)) = '' BEGIN
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'ValidationError', @InputParameters, 'FanEmail cannot be empty.');
        RAISERROR('FanEmail cannot be empty.', 16, 1); RETURN; END

    BEGIN TRANSACTION MainTrx;
    INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'TXN Start', @InputParameters, 'MainTrx started.');

    DECLARE @CurrentBandId INT = NULL;
    DECLARE @CurrentFanId INT = NULL;
    DECLARE @LogMessage VARCHAR(MAX);
    
    DECLARE @BandProcessedSuccessfully BIT = 0;
    DECLARE @FanProcessedSuccessfully BIT = 0;
    DECLARE @LinkProcessedSuccessfully BIT = 0;

    -- Step 1: Process Band
    SAVE TRANSACTION SavePoint_Band;
    INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'SavePoint', @InputParameters, 'SavePoint_Band created.');
    BEGIN TRY
        IF @ForceBandFail = 1 RAISERROR('Simulated Band Processing Failure.', 16, 1);

        SELECT @CurrentBandId = BandId FROM dbo.Bands WHERE BandName = @BandName;
        IF @CurrentBandId IS NULL
        BEGIN
            INSERT INTO dbo.Bands (BandName) VALUES (@BandName);
            SET @CurrentBandId = SCOPE_IDENTITY();
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, CONCAT('New band created: ''', @BandName, ''' ID: ', @CurrentBandId));
        END ELSE BEGIN
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, CONCAT('Existing band found: ''', @BandName, ''' ID: ', @CurrentBandId));
        END
        SET @BandProcessedSuccessfully = 1;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() = -1 BEGIN -- Uncommittable transaction
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine)
            VALUES (@ProcedureName, 'FatalError', @InputParameters, CONCAT('Band Processing: Uncommittable TXN. Rolling back MainTrx. Error: ', ERROR_MESSAGE()), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE());
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION MainTrx;
            RAISERROR('Band processing failed critically. Main transaction rolled back.', 16, 1); RETURN; END
        
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION SavePoint_Band; -- Rollback only Band part
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine)
        VALUES (@ProcedureName, 'Error', @InputParameters, CONCAT('Band Processing Failed. Rolled back to SavePoint_Band. Error: ', ERROR_MESSAGE()), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE());
        SET @CurrentBandId = NULL; -- Mark as failed
    END CATCH

    -- Step 2: Process Fan (Proceed even if band processing failed, to see if fan can be created/found)
    IF XACT_STATE() <> -1 -- Only proceed if transaction is still active
    BEGIN
        SAVE TRANSACTION SavePoint_Fan;
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'SavePoint', @InputParameters, 'SavePoint_Fan created.');
        BEGIN TRY
            IF @ForceFanFail = 1 RAISERROR('Simulated Fan Processing Failure.', 16, 1);

            SELECT @CurrentFanId = FanId FROM dbo.Fans WHERE Email = @FanEmail;
            IF @CurrentFanId IS NULL
            BEGIN
                INSERT INTO dbo.Fans (FanName, Email) VALUES (@FanName, @FanEmail);
                SET @CurrentFanId = SCOPE_IDENTITY();
                INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, CONCAT('New fan created: ''', @FanName, ''' Email: ', @FanEmail, ' ID: ', @CurrentFanId));
            END ELSE BEGIN
                INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, CONCAT('Existing fan found: ''', @FanName, ''' Email: ', @FanEmail, ' ID: ', @CurrentFanId));
            END
            SET @FanProcessedSuccessfully = 1;
        END TRY
        BEGIN CATCH
            IF XACT_STATE() = -1 BEGIN
                INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine)
                VALUES (@ProcedureName, 'FatalError', @InputParameters, CONCAT('Fan Processing: Uncommittable TXN. Rolling back MainTrx. Error: ', ERROR_MESSAGE()), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE());
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION MainTrx;
                RAISERROR('Fan processing failed critically. Main transaction rolled back.', 16, 1); RETURN; END

            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION SavePoint_Fan; -- Rollback only Fan part
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine)
            VALUES (@ProcedureName, 'Error', @InputParameters, CONCAT('Fan Processing Failed. Rolled back to SavePoint_Fan. Error: ', ERROR_MESSAGE()), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE());
            SET @CurrentFanId = NULL; -- Mark as failed
        END CATCH
    END

    -- Step 3: Process Link
    IF XACT_STATE() <> -1 AND @CurrentBandId IS NOT NULL AND @CurrentFanId IS NOT NULL -- Only if both entities are available and TXN is active
    BEGIN
        SAVE TRANSACTION SavePoint_Link;
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'SavePoint', @InputParameters, 'SavePoint_Link created.');
        BEGIN TRY
            -- Simulate a link failure if @ForceLinkFail is true AND the link already exists (PK violation)
            -- Or a generic link failure if @ForceLinkFail is true and link doesn't exist (to test other errors)
            IF @ForceLinkFail = 1 
            BEGIN
                IF EXISTS(SELECT 1 FROM dbo.BandFans WHERE BandId = @CurrentBandId AND FanId = @CurrentFanId)
                    RAISERROR('Simulated Link Failure: Link already exists (PK violation).', 16, 1); 
                ELSE
                    RAISERROR('Simulated Link Failure: Generic error.', 16, 1);
            END

            INSERT INTO dbo.BandFans (BandId, FanId) VALUES (@CurrentBandId, @CurrentFanId);
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, CONCAT('Link created for BandId: ', @CurrentBandId, ', FanId: ', @CurrentFanId));
            SET @LinkProcessedSuccessfully = 1;
        END TRY
        BEGIN CATCH
            IF XACT_STATE() = -1 BEGIN
                INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine)
                VALUES (@ProcedureName, 'FatalError', @InputParameters, CONCAT('Link Processing: Uncommittable TXN. Rolling back MainTrx. Error: ', ERROR_MESSAGE()), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE());
                IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION MainTrx;
                RAISERROR('Link processing failed critically. Main transaction rolled back.', 16, 1); RETURN; END

            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION SavePoint_Link; -- Rollback only Link part
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine)
            VALUES (@ProcedureName, 'Error', @InputParameters, CONCAT('Link Processing Failed. Rolled back to SavePoint_Link. Error: ', ERROR_MESSAGE()), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE());
        END CATCH
    END
    ELSE IF XACT_STATE() <> -1 -- Log why link was skipped if not because TXN died
    BEGIN
        IF @CurrentBandId IS NULL INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, 'Link skipped: Band ID not available or band processing failed.');
        IF @CurrentFanId IS NULL INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) VALUES (@ProcedureName, 'Info', @InputParameters, 'Link skipped: Fan ID not available or fan processing failed.');
    END

    -- Final Commit or Rollback Decision for MainTrx
    IF @@TRANCOUNT > 0 -- If MainTrx is still active (wasn't rolled back by a fatal error CATCH)
    BEGIN
        IF XACT_STATE() = 1 -- And is committable
        BEGIN
            COMMIT TRANSACTION MainTrx;
            DECLARE @FinalStatus VARCHAR(500);
            SET @FinalStatus = CONCAT('MainTrx Committed. BandOK:', CAST(@BandProcessedSuccessfully AS CHAR(1)), 
                                    ', FanOK:', CAST(@FanProcessedSuccessfully AS CHAR(1)), 
                                    ', LinkOK:', CAST(@LinkProcessedSuccessfully AS CHAR(1)), '.');
            IF @CurrentBandId IS NULL AND @BandProcessedSuccessfully = 0 AND NOT (@ForceBandFail = 1 AND @BandProcessedSuccessfully = 0) SET @FinalStatus = CONCAT(@FinalStatus, ' Band processing may have encountered an issue not setting ID.'); -- Defensive
            IF @CurrentFanId IS NULL AND @FanProcessedSuccessfully = 0 AND NOT (@ForceFanFail = 1 AND @FanProcessedSuccessfully = 0) SET @FinalStatus = CONCAT(@FinalStatus, ' Fan processing may have encountered an issue not setting ID.');

            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
            VALUES (@ProcedureName, 'TXN Commit', @InputParameters, @FinalStatus);
            PRINT 'Procedure completed. Main Transaction Committed. Check logs for details.';
        END
        ELSE -- Uncommittable (XACT_STATE() = -1), this state should ideally be caught earlier
        BEGIN
            ROLLBACK TRANSACTION MainTrx;
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
            VALUES (@ProcedureName, 'TXN Rollback', @InputParameters, 'MainTrx Rolled Back at the end due to uncommittable state.');
            PRINT 'Procedure completed WITH ERRORS. Main Transaction Rolled Back. Check logs.';
        END
    END
    -- If @@TRANCOUNT = 0, it means a CATCH block already executed a full ROLLBACK of MainTrx.
END
GO

--------------------------------------------------------------------------------
-- Test Script for usp_AddFanToBand_PartialRecovery
--------------------------------------------------------------------------------
PRINT N'====================================================================';
PRINT N'CLEANING UP PREVIOUS TEST DATA FOR PartialRecovery';
PRINT N'====================================================================';
-- Clear the log table (or filter by ProcedureName if shared log)
DELETE FROM dbo.ProcedureLog WHERE ProcedureName = 'usp_AddFanToBand_PartialRecovery';
PRINT N'ProcedureLog table cleared for usp_AddFanToBand_PartialRecovery.';

-- Delete specific fans (use different names/emails for this test suite to avoid conflict if prev. tests not cleaned)
DELETE FROM dbo.Fans 
WHERE Email IN (
    'diana_prince@example.com', 
    'clark_kent@example.com', 
    'bruce_wayne@example.com',
    'peter_parker_fail@example.com' -- For fan fail test
);
PRINT N'Test fans for PartialRecovery deleted.';

-- Delete specific bands
DELETE FROM dbo.Bands 
WHERE BandName IN (
    'Justice League Band', 
    'Avengers Band',
    'X-Men Band Fail' -- For band fail test
);
PRINT N'Test bands for PartialRecovery deleted.';
PRINT N'Cleanup for PartialRecovery complete.';
GO

PRINT N'--------------------------------------------------------------------';
PRINT N'Starting Test Scenarios for usp_AddFanToBand_PartialRecovery';
PRINT N'--------------------------------------------------------------------';
GO

-- Scenario PR1: Full Success (New Band, New Fan, New Link)
PRINT N'Scenario PR1: Full Success';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand_PartialRecovery 
    @BandName = 'Justice League Band', 
    @FanName = 'Diana Prince', 
    @FanEmail = 'diana_prince@example.com';
SELECT 'Band PR1' AS Context, * FROM dbo.Bands WHERE BandName = 'Justice League Band';
SELECT 'Fan PR1' AS Context, * FROM dbo.Fans WHERE Email = 'diana_prince@example.com';
SELECT 'Link PR1' AS Context, B.BandName, FA.FanName FROM dbo.BandFans BF JOIN dbo.Bands B ON B.BandId = BF.BandId JOIN dbo.Fans FA ON FA.FanId = BF.FanId WHERE B.BandName = 'Justice League Band' AND FA.Email = 'diana_prince@example.com';
SELECT TOP 15 * FROM dbo.ProcedureLog WHERE ProcedureName = 'usp_AddFanToBand_PartialRecovery' ORDER BY LogId DESC;
GO

-- Scenario PR2: New Band, New Fan, Link Fails (e.g., PK Violation - simulate by running PR1 logic again with ForceLinkFail=0, then try to link again)
PRINT N'Scenario PR2.1: Setup - Create initial link for Justice League Band and Diana Prince (same as PR1)';
EXEC dbo.usp_AddFanToBand_PartialRecovery @BandName = 'Justice League Band', @FanName = 'Diana Prince', @FanEmail = 'diana_prince@example.com', @ForceBandFail=0, @ForceFanFail=0, @ForceLinkFail=0;
GO
PRINT N'Scenario PR2.2: New Band (Avengers), New Fan (Clark), Link Fails (Simulating by trying to link JL and Diana again, or use @ForceLinkFail on a new pair)';
PRINT N'Attempting to add Diana to Justice League Band again (will cause actual PK violation for link)';
PRINT N'-------------------------------------------------------------------------------------------';
EXEC dbo.usp_AddFanToBand_PartialRecovery 
    @BandName = 'Justice League Band',        -- Existing Band
    @FanName = 'Diana Prince',            -- Existing Fan
    @FanEmail = 'diana_prince@example.com', -- Existing Email
    @ForceLinkFail = 0;                   -- Let the PK violation on BandFans be the failure
-- Expected: Band 'Justice League Band' and Fan 'Diana Prince' REMAIN. No new link. Original link remains.
SELECT 'Band PR2' AS Context, * FROM dbo.Bands WHERE BandName = 'Justice League Band';
SELECT 'Fan PR2' AS Context, * FROM dbo.Fans WHERE Email = 'diana_prince@example.com';
SELECT 'Link Count PR2' AS Context, COUNT(*) as LinkCount FROM dbo.BandFans BF JOIN dbo.Bands B ON B.BandId = BF.BandId JOIN dbo.Fans FA ON FA.FanId = BF.FanId WHERE B.BandName = 'Justice League Band' AND FA.Email = 'diana_prince@example.com'; -- Should be 1
SELECT TOP 15 * FROM dbo.ProcedureLog WHERE ProcedureName = 'usp_AddFanToBand_PartialRecovery' ORDER BY LogId DESC;
GO

-- Scenario PR3: New Band, Fan Creation Fails (Simulated with @ForceFanFail)
PRINT N'Scenario PR3: New Band, Fan Creation Fails';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand_PartialRecovery 
    @BandName = 'Avengers Band',             -- New Band
    @FanName = 'Peter Parker Fail',          -- New Fan (intended)
    @FanEmail = 'peter_parker_fail@example.com', -- New Email (intended)
    @ForceFanFail = 1;
-- Expected: 'Avengers Band' IS CREATED and REMAINS. Fan 'Peter Parker Fail' IS NOT CREATED. No link.
SELECT 'Band PR3' AS Context, * FROM dbo.Bands WHERE BandName = 'Avengers Band'; -- Should exist
SELECT 'Fan PR3' AS Context, * FROM dbo.Fans WHERE Email = 'peter_parker_fail@example.com'; -- Should NOT exist
SELECT 'Link PR3' AS Context, B.BandName, FA.FanName FROM dbo.BandFans BF JOIN dbo.Bands B ON B.BandId = BF.BandId JOIN dbo.Fans FA ON FA.FanId = BF.FanId WHERE B.BandName = 'Avengers Band'; -- Should be empty
SELECT TOP 15 * FROM dbo.ProcedureLog WHERE ProcedureName = 'usp_AddFanToBand_PartialRecovery' ORDER BY LogId DESC;
GO

-- Scenario PR4: Band Creation Fails (Simulated with @ForceBandFail)
PRINT N'Scenario PR4: Band Creation Fails';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand_PartialRecovery 
    @BandName = 'X-Men Band Fail',           -- New Band (intended to fail)
    @FanName = 'Bruce Wayne',                -- New Fan (should not be created if band fails critically)
    @FanEmail = 'bruce_wayne@example.com',   -- New Email
    @ForceBandFail = 1;
-- Expected: 'X-Men Band Fail' IS NOT CREATED. Fan 'Bruce Wayne' IS NOT CREATED. No link. Entire main transaction rolled back.
SELECT 'Band PR4' AS Context, * FROM dbo.Bands WHERE BandName = 'X-Men Band Fail'; -- Should NOT exist
SELECT 'Fan PR4' AS Context, * FROM dbo.Fans WHERE Email = 'bruce_wayne@example.com'; -- Should NOT exist
SELECT TOP 15 * FROM dbo.ProcedureLog WHERE ProcedureName = 'usp_AddFanToBand_PartialRecovery' ORDER BY LogId DESC;
GO

-- Scenario PR5: Parameter Validation (e.g. empty FanName)
PRINT N'Scenario PR5: Parameter validation fail';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand_PartialRecovery 
    @BandName = 'Some Valid Band PR5', 
    @FanName = '',                           -- Invalid
    @FanEmail = 'valid_pr5@example.com';
SELECT TOP 15 * FROM dbo.ProcedureLog WHERE ProcedureName = 'usp_AddFanToBand_PartialRecovery' ORDER BY LogId DESC;
GO

PRINT N'--------------------------------------------------------------------';
PRINT N'Finished Test Scenarios for usp_AddFanToBand_PartialRecovery';
PRINT N'--------------------------------------------------------------------';
GO