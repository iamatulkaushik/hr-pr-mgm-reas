-- 0001_init.sql (HRMS_Data)
-- Schemas only. Tables arrive in later numbered scripts. Idempotent.
-- cfg.SchemaVersion is created by Hrms.Migrator before the first script runs.
SET XACT_ABORT ON;
GO
IF SCHEMA_ID('mst') IS NULL EXEC('CREATE SCHEMA mst');
GO
IF SCHEMA_ID('att') IS NULL EXEC('CREATE SCHEMA att');
GO
IF SCHEMA_ID('pay') IS NULL EXEC('CREATE SCHEMA pay');
GO
IF SCHEMA_ID('stat') IS NULL EXEC('CREATE SCHEMA stat');
GO
IF SCHEMA_ID('car') IS NULL EXEC('CREATE SCHEMA car');
GO
IF SCHEMA_ID('aud') IS NULL EXEC('CREATE SCHEMA aud');
GO
IF SCHEMA_ID('cfg') IS NULL EXEC('CREATE SCHEMA cfg');
GO
