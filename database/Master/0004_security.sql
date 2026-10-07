/* 0004_security.sql | Target: HRMS_Master | sqlcmd -v AppLoginPassword="<strong password>" */
SET NOCOUNT ON;
GO
IF SUSER_ID(N'HrmsApp') IS NULL
BEGIN
  DECLARE @sql NVARCHAR(MAX) = N'CREATE LOGIN HrmsApp WITH PASSWORD = N''' + REPLACE(N'$(AppLoginPassword)', N'''', N'''''')
                             + N''', CHECK_POLICY = ON, DEFAULT_DATABASE = HRMS_Master';
  EXEC(@sql);
END
GO
IF DATABASE_PRINCIPAL_ID(N'HrmsApp') IS NULL CREATE USER HrmsApp FOR LOGIN HrmsApp;
IF DATABASE_PRINCIPAL_ID(N'hrms_app_role') IS NULL CREATE ROLE hrms_app_role;
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members m
               WHERE m.role_principal_id = DATABASE_PRINCIPAL_ID(N'hrms_app_role') AND m.member_principal_id = DATABASE_PRINCIPAL_ID(N'HrmsApp'))
  ALTER ROLE hrms_app_role ADD MEMBER HrmsApp;
GO
-- EXECUTE only. Tables are reached through procedures (ownership chaining), never directly.
GRANT EXECUTE ON SCHEMA::sec TO hrms_app_role;
GRANT EXECUTE ON SCHEMA::mst TO hrms_app_role;
GRANT EXECUTE ON SCHEMA::aud TO hrms_app_role;
GRANT EXECUTE ON SCHEMA::cfg TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::sec TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::mst TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::aud TO hrms_app_role;
DENY SELECT, INSERT, UPDATE, DELETE ON SCHEMA::cfg TO hrms_app_role;
GO
IF NOT EXISTS (SELECT 1 FROM cfg.SchemaVersion WHERE ScriptName=N'Master_0004_security')
  INSERT cfg.SchemaVersion(ScriptName) VALUES (N'Master_0004_security');
GO
