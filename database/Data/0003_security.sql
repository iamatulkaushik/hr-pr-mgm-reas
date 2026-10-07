/* 0003_security.sql | Target: HRMS_Data | Run after Master_0004 (login HrmsApp must exist) */
SET NOCOUNT ON;
GO
IF DATABASE_PRINCIPAL_ID(N'HrmsApp') IS NULL CREATE USER HrmsApp FOR LOGIN HrmsApp;
IF DATABASE_PRINCIPAL_ID(N'hrms_app_role') IS NULL CREATE ROLE hrms_app_role;
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members m
               WHERE m.role_principal_id = DATABASE_PRINCIPAL_ID(N'hrms_app_role') AND m.member_principal_id = DATABASE_PRINCIPAL_ID(N'HrmsApp'))
  ALTER ROLE hrms_app_role ADD MEMBER HrmsApp;
GO
GRANT EXECUTE ON SCHEMA::mst TO hrms_app_role;
GRANT EXECUTE ON OBJECT::sec.usp_Session_SetCompany TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::sec TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::mst TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::cfg TO hrms_app_role;
GO
IF NOT EXISTS (SELECT 1 FROM cfg.SchemaVersion WHERE ScriptName=N'Data_0003_security')
  INSERT cfg.SchemaVersion(ScriptName) VALUES (N'Data_0003_security');
GO
