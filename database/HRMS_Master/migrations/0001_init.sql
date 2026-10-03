-- 0001_init.sql (HRMS_Master)
-- Schemas only. Tables arrive in later numbered scripts. Idempotent.
-- cfg.SchemaVersion is created by Hrms.Migrator before the first script runs.
SET XACT_ABORT ON;
GO
IF SCHEMA_ID('sec') IS NULL EXEC('CREATE SCHEMA sec');
GO
IF SCHEMA_ID('mst') IS NULL EXEC('CREATE SCHEMA mst');
GO
IF SCHEMA_ID('aud') IS NULL EXEC('CREATE SCHEMA aud');
GO
IF SCHEMA_ID('cfg') IS NULL EXEC('CREATE SCHEMA cfg');
GO
