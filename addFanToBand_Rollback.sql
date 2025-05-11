USE Metal;
GO

-- Create Procedure Logs Table
CREATE TABLE dbo.ProcedureLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    ProcedureName VARCHAR(255) NOT NULL,
    ActionType VARCHAR(50), -- e.g., 'Start', 'Commit', 'Rollback', 'Info', 'Error'
    Parameters VARCHAR(MAX) NULL,
    LogMessage VARCHAR(MAX) NOT NULL,
    ErrorNumber INT NULL,
    ErrorSeverity INT NULL,
    ErrorState INT NULL,
    ErrorLine INT NULL,
    LogTime DATETIME DEFAULT GETDATE()
);
GO


-- Insert date to a m:n relationship
CREATE OR ALTER PROCEDURE dbo.usp_AddFanToBand
    @BandName VARCHAR(255),
    @FanName VARCHAR(255),
    @FanEmail VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON; -- Suppress "xx rows affected" messages

    DECLARE @ProcedureName VARCHAR(255) = 'usp_AddFanToBand';
    DECLARE @InputParameters VARCHAR(MAX) = CONCAT('@BandName=', ISNULL(@BandName, 'NULL'), 
                                                 ', @FanName=', ISNULL(@FanName, 'NULL'), 
                                                 ', @FanEmail=', ISNULL(@FanEmail, 'NULL'));

    -- Log start of procedure
    INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
    VALUES (@ProcedureName, 'Start', @InputParameters, 'Procedure execution started.');

    -- 1. Parameter Validation
    IF @BandName IS NULL OR LTRIM(RTRIM(@BandName)) = ''
    BEGIN
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
        VALUES (@ProcedureName, 'Error', @InputParameters, 'Validation Error: BandName cannot be empty.');
        RAISERROR('BandName cannot be empty.', 16, 1);
        RETURN; 
    END

    IF @FanName IS NULL OR LTRIM(RTRIM(@FanName)) = ''
    BEGIN
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
        VALUES (@ProcedureName, 'Error', @InputParameters, 'Validation Error: FanName cannot be empty.');
        RAISERROR('FanName cannot be empty.', 16, 1);
        RETURN;
    END

    IF @FanEmail IS NULL OR LTRIM(RTRIM(@FanEmail)) = ''
    BEGIN
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
        VALUES (@ProcedureName, 'Error', @InputParameters, 'Validation Error: FanEmail cannot be empty.');
        RAISERROR('FanEmail cannot be empty.', 16, 1);
        RETURN;
    END
    -- You could add a UDF for email validation here if needed.

    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @CurrentBandId INT;
        DECLARE @CurrentFanId INT;
        DECLARE @LogMessage VARCHAR(MAX);

        -- 2. Find or Create Band
        -- Note: Your Bands table doesn't have a UNIQUE constraint on BandName.
        -- This logic will use the first band found if names are duplicated, or create a new one.
        -- For true "find or create unique", BandName should have a UNIQUE constraint.
        SELECT @CurrentBandId = BandId FROM dbo.Bands WHERE BandName = @BandName;

        IF @CurrentBandId IS NULL
        BEGIN
            INSERT INTO dbo.Bands (BandName) VALUES (@BandName);
            SET @CurrentBandId = SCOPE_IDENTITY();
            SET @LogMessage = CONCAT('New band created: ''', @BandName, ''' with BandId: ', @CurrentBandId);
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
            VALUES (@ProcedureName, 'Info', @InputParameters, @LogMessage);
        END
        ELSE
        BEGIN
            SET @LogMessage = CONCAT('Existing band found: ''', @BandName, ''' with BandId: ', @CurrentBandId);
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
            VALUES (@ProcedureName, 'Info', @InputParameters, @LogMessage);
        END

        -- 3. Find or Create Fan (using Email as it's unique)
        SELECT @CurrentFanId = FanId FROM dbo.Fans WHERE Email = @FanEmail;

        IF @CurrentFanId IS NULL
        BEGIN
            INSERT INTO dbo.Fans (FanName, Email) VALUES (@FanName, @FanEmail);
            SET @CurrentFanId = SCOPE_IDENTITY();
            SET @LogMessage = CONCAT('New fan created: ''', @FanName, ''' (', @FanEmail, ') with FanId: ', @CurrentFanId);
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
            VALUES (@ProcedureName, 'Info', @InputParameters, @LogMessage);
        END
        ELSE
        BEGIN
            -- Optional: Check if FanName matches for existing email; if not, you might log a warning or update it.
            -- For this exercise, we'll assume the existing fan is the correct one.
            SET @LogMessage = CONCAT('Existing fan found: ''', @FanName, ''' (', @FanEmail, ') with FanId: ', @CurrentFanId);
            INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
            VALUES (@ProcedureName, 'Info', @InputParameters, @LogMessage);
        END

        -- 4. Insert into BandFans (Junction Table)
        -- The PRIMARY KEY on BandFans (BandId, FanId) will prevent duplicate entries.
        -- If this INSERT fails due to a PK violation (meaning the link already exists), 
        -- or any other reason, it will jump to the CATCH block, and the transaction will be rolled back.
        INSERT INTO dbo.BandFans (BandId, FanId) VALUES (@CurrentBandId, @CurrentFanId);
        SET @LogMessage = CONCAT('Successfully linked FanId: ', @CurrentFanId, ' to BandId: ', @CurrentBandId);
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
        VALUES (@ProcedureName, 'Info', @InputParameters, @LogMessage);
        
        COMMIT TRANSACTION;
        SET @LogMessage = 'Transaction committed successfully.';
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage) 
        VALUES (@ProcedureName, 'Commit', @InputParameters, @LogMessage);
        
        PRINT @LogMessage;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 -- Check if a transaction is still active
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        DECLARE @ErrorNumber_ INT = ERROR_NUMBER(); -- Renamed to avoid conflict if you have a variable @ErrorNumber
        DECLARE @ErrorLine_ INT = ERROR_LINE();   -- Renamed to avoid conflict

        SET @LogMessage = CONCAT('Error: ', @ErrorMessage);
        INSERT INTO dbo.ProcedureLog (ProcedureName, ActionType, Parameters, LogMessage, ErrorNumber, ErrorSeverity, ErrorState, ErrorLine) 
        VALUES (@ProcedureName, 'Rollback', @InputParameters, @LogMessage, @ErrorNumber_, @ErrorSeverity, @ErrorState, @ErrorLine_);
        
        PRINT CONCAT('Error occurred: ', @ErrorMessage);
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState); -- Re-throw error to the client
    END CATCH
END
GO

-- Test cases
USE Metal;
GO

PRINT N'====================================================================';
PRINT N'CLEANING UP PREVIOUS TEST DATA AND LOGS';
PRINT N'====================================================================';
-- Clear the log table for a fresh start
TRUNCATE TABLE dbo.ProcedureLog; 
PRINT N'ProcedureLog table cleared.';

-- Delete specific fans created/used by the tests.
DELETE FROM dbo.Fans 
WHERE Email IN (
    'alice@example.com', 
    'bob@example.com', 
    'charlie@example.com'
);
PRINT N'Test fans deleted (if they existed).';

-- Delete specific bands created/used by the tests.
DELETE FROM dbo.Bands 
WHERE BandName IN (
    'Cosmic Void', 
    'Metallica',
    'Valid Band For Null Email' 
);
PRINT N'Test bands deleted (if they existed).';
PRINT N'Cleanup complete.';
GO

PRINT N'--------------------------------------------------------------------';
PRINT N'Starting Test Scenarios for usp_AddFanToBand';
PRINT N'--------------------------------------------------------------------';
GO

PRINT N'Scenario 1: Happy path (new band, new fan)';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand @BandName = 'Cosmic Void', @FanName = 'Alice Wonderland', @FanEmail = 'alice@example.com';
SELECT 'Bands after S1' AS Context, * FROM dbo.Bands WHERE BandName = 'Cosmic Void';
SELECT 'Fans after S1' AS Context, * FROM dbo.Fans WHERE Email = 'alice@example.com';
SELECT 'BandFans after S1' AS Context, B.BandName, F.FanName, F.Email 
FROM dbo.BandFans BF 
JOIN dbo.Bands B ON BF.BandId = B.BandId 
JOIN dbo.Fans F ON BF.FanId = F.FanId 
WHERE B.BandName = 'Cosmic Void' AND F.Email = 'alice@example.com';
PRINT N'ProcedureLog after S1:';
SELECT TOP 5 * FROM dbo.ProcedureLog ORDER BY LogTime DESC;
GO

PRINT N'Scenario 2: Existing band (Cosmic Void), new fan (Bob)';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand @BandName = 'Cosmic Void', @FanName = 'Bob The Builder', @FanEmail = 'bob@example.com';
SELECT 'Fans after S2' AS Context, * FROM dbo.Fans WHERE Email = 'bob@example.com';
SELECT 'BandFans after S2 for Bob' AS Context, B.BandName, F.FanName, F.Email 
FROM dbo.BandFans BF 
JOIN dbo.Bands B ON BF.BandId = B.BandId 
JOIN dbo.Fans F ON BF.FanId = F.FanId 
WHERE B.BandName = 'Cosmic Void' AND F.Email = 'bob@example.com';
PRINT N'ProcedureLog after S2:';
SELECT TOP 5 * FROM dbo.ProcedureLog ORDER BY LogTime DESC;
GO

PRINT N'Setting up data for Scenario 3...';
PRINT N'--------------------------------------------------';
IF NOT EXISTS (SELECT 1 FROM dbo.Bands WHERE BandName = 'Metallica')
BEGIN
    INSERT INTO dbo.Bands (BandName) VALUES ('Metallica');
    PRINT N'Inserted band: Metallica for Scenario 3 setup.';
END
ELSE
BEGIN
    PRINT N'Band: Metallica already exists for Scenario 3 setup.';
END

IF NOT EXISTS (SELECT 1 FROM dbo.Fans WHERE Email = 'charlie@example.com')
BEGIN
    INSERT INTO dbo.Fans (FanName, Email) VALUES ('Charlie Brown', 'charlie@example.com');
    PRINT N'Inserted fan: Charlie Brown (charlie@example.com) for Scenario 3 setup.';
END
ELSE
BEGIN
    PRINT N'Fan: Charlie Brown (charlie@example.com) already exists for Scenario 3 setup.';
END
PRINT N'Setup for Scenario 3 complete.';
GO

PRINT N'Scenario 3: Existing band (Metallica), existing fan (Charlie) (first time linking)';
PRINT N'--------------------------------------------------------------------------------';
EXEC dbo.usp_AddFanToBand @BandName = 'Metallica', @FanName = 'Charlie Brown', @FanEmail = 'charlie@example.com';
SELECT 'BandFans after S3 for Charlie & Metallica' AS Context, B.BandName, F.FanName, F.Email 
FROM dbo.BandFans BF 
JOIN dbo.Bands B ON BF.BandId = B.BandId 
JOIN dbo.Fans F ON F.FanId = BF.FanId 
WHERE B.BandName = 'Metallica' AND F.Email = 'charlie@example.com';
PRINT N'ProcedureLog after S3:';
SELECT TOP 5 * FROM dbo.ProcedureLog ORDER BY LogTime DESC;
GO

PRINT N'Scenario 4: Attempt to add an existing link (Cosmic Void, Alice) - should cause PK violation and rollback';
PRINT N'---------------------------------------------------------------------------------------------------';
EXEC dbo.usp_AddFanToBand @BandName = 'Cosmic Void', @FanName = 'Alice Wonderland', @FanEmail = 'alice@example.com';
PRINT N'ProcedureLog after S4:';
SELECT TOP 5 * FROM dbo.ProcedureLog ORDER BY LogTime DESC;
GO

PRINT N'Scenario 5: Invalid parameter (e.g., empty band name)';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand @BandName = '', @FanName = 'Test Fan Empty Band', @FanEmail = 'testemptyband@example.com';
PRINT N'ProcedureLog after S5:';
SELECT TOP 5 * FROM dbo.ProcedureLog ORDER BY LogTime DESC;
GO

PRINT N'Scenario 6: Invalid parameter (e.g., NULL fan email)';
PRINT N'--------------------------------------------------';
EXEC dbo.usp_AddFanToBand @BandName = 'Valid Band For Null Email', @FanName = 'Test Fan Null Email', @FanEmail = NULL;
PRINT N'ProcedureLog after S6:';
SELECT TOP 5 * FROM dbo.ProcedureLog ORDER BY LogTime DESC;
GO

PRINT N'--------------------------------------------------------------------';
PRINT N'Finished Test Scenarios for usp_AddFanToBand';
PRINT N'--------------------------------------------------------------------';
GO