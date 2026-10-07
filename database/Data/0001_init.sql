/* 0001_init.sql | Target: HRMS_Data | Idempotent | Dev Brave, Phase 1 */
SET NOCOUNT ON;
GO
IF SCHEMA_ID(N'sec')  IS NULL EXEC(N'CREATE SCHEMA sec AUTHORIZATION dbo');
IF SCHEMA_ID(N'mst')  IS NULL EXEC(N'CREATE SCHEMA mst AUTHORIZATION dbo');
IF SCHEMA_ID(N'att')  IS NULL EXEC(N'CREATE SCHEMA att AUTHORIZATION dbo');
IF SCHEMA_ID(N'pay')  IS NULL EXEC(N'CREATE SCHEMA pay AUTHORIZATION dbo');
IF SCHEMA_ID(N'stat') IS NULL EXEC(N'CREATE SCHEMA stat AUTHORIZATION dbo');
IF SCHEMA_ID(N'car')  IS NULL EXEC(N'CREATE SCHEMA car AUTHORIZATION dbo');
IF SCHEMA_ID(N'aud')  IS NULL EXEC(N'CREATE SCHEMA aud AUTHORIZATION dbo');
IF SCHEMA_ID(N'cfg')  IS NULL EXEC(N'CREATE SCHEMA cfg AUTHORIZATION dbo');
GO
IF OBJECT_ID(N'cfg.SchemaVersion') IS NULL
CREATE TABLE cfg.SchemaVersion(
  SchemaVersionID INT IDENTITY(1,1) CONSTRAINT PK_SchemaVersion PRIMARY KEY,
  ScriptName NVARCHAR(100) NOT NULL CONSTRAINT UQ_SchemaVersion_Script UNIQUE,
  AppliedOn  DATETIME2(0)  NOT NULL CONSTRAINT DF_SchemaVersion_On DEFAULT SYSUTCDATETIME(),
  AppliedBy  SYSNAME       NOT NULL CONSTRAINT DF_SchemaVersion_By DEFAULT SUSER_SNAME());
GO
IF OBJECT_ID(N'cfg.Counter') IS NULL
CREATE TABLE cfg.Counter(
  CompanyID INT NOT NULL, CounterName VARCHAR(30) NOT NULL, LastValue INT NOT NULL,
  CONSTRAINT PK_Counter PRIMARY KEY (CompanyID, CounterName));
GO
IF OBJECT_ID(N'mst.Department') IS NULL
CREATE TABLE mst.Department(
  DepartmentID INT IDENTITY(1,1) CONSTRAINT PK_Department PRIMARY KEY,
  CompanyID INT NOT NULL, DepartmentName NVARCHAR(80) NOT NULL,
  IsActive BIT NOT NULL CONSTRAINT DF_Dept_Active DEFAULT 1,
  CreatedBy INT NULL, CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_Dept_CreatedOn DEFAULT SYSUTCDATETIME(),
  ModifiedBy INT NULL, ModifiedOn DATETIME2(0) NULL,
  CONSTRAINT UQ_Department UNIQUE (CompanyID, DepartmentName));
GO
IF OBJECT_ID(N'mst.Designation') IS NULL
CREATE TABLE mst.Designation(
  DesignationID INT IDENTITY(1,1) CONSTRAINT PK_Designation PRIMARY KEY,
  CompanyID INT NOT NULL, DesignationName NVARCHAR(80) NOT NULL,
  IsActive BIT NOT NULL CONSTRAINT DF_Desig_Active DEFAULT 1,
  CreatedBy INT NULL, CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_Desig_CreatedOn DEFAULT SYSUTCDATETIME(),
  ModifiedBy INT NULL, ModifiedOn DATETIME2(0) NULL,
  CONSTRAINT UQ_Designation UNIQUE (CompanyID, DesignationName));
GO
IF OBJECT_ID(N'mst.Factory') IS NULL
CREATE TABLE mst.Factory(
  FactoryID INT IDENTITY(1,1) CONSTRAINT PK_Factory PRIMARY KEY,
  CompanyID INT NOT NULL, FactoryName NVARCHAR(120) NOT NULL, LicenseNo VARCHAR(40) NULL, Address NVARCHAR(250) NULL,
  IsActive BIT NOT NULL CONSTRAINT DF_Factory_Active DEFAULT 1,
  CreatedBy INT NULL, CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_Factory_CreatedOn DEFAULT SYSUTCDATETIME(),
  ModifiedBy INT NULL, ModifiedOn DATETIME2(0) NULL,
  CONSTRAINT UQ_Factory UNIQUE (CompanyID, FactoryName));
GO
IF OBJECT_ID(N'mst.Employee') IS NULL
CREATE TABLE mst.Employee(
  EmployeeID INT IDENTITY(1,1) CONSTRAINT PK_Employee PRIMARY KEY,
  CompanyID INT NOT NULL,
  EmpCode NVARCHAR(20) NOT NULL,
  FullName NVARCHAR(100) NOT NULL,
  FatherHusbandName NVARCHAR(100) NULL,
  Gender CHAR(1) NOT NULL CONSTRAINT CK_Emp_Gender CHECK (Gender IN ('M','F','O')),
  DOB DATE NOT NULL, DOJ DATE NOT NULL, DOL DATE NULL,
  DepartmentID  INT NULL CONSTRAINT FK_Emp_Dept    REFERENCES mst.Department(DepartmentID),
  DesignationID INT NULL CONSTRAINT FK_Emp_Desig   REFERENCES mst.Designation(DesignationID),
  FactoryID     INT NULL CONSTRAINT FK_Emp_Factory REFERENCES mst.Factory(FactoryID),
  PayType VARCHAR(10) NOT NULL CONSTRAINT CK_Emp_PayType CHECK (PayType IN ('Daily','Monthly')),
  Mobile VARCHAR(15) NULL, Address NVARCHAR(250) NULL,
  IsActive BIT NOT NULL CONSTRAINT DF_Emp_Active DEFAULT 1,
  CreatedBy INT NULL, CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_Emp_CreatedOn DEFAULT SYSUTCDATETIME(),
  ModifiedBy INT NULL, ModifiedOn DATETIME2(0) NULL,
  CONSTRAINT UQ_Employee_Code UNIQUE (CompanyID, EmpCode),
  CONSTRAINT CK_Emp_Dates CHECK (DOJ > DOB AND (DOL IS NULL OR DOL >= DOJ)));
GO
IF OBJECT_ID(N'mst.EmployeeNominee') IS NULL
CREATE TABLE mst.EmployeeNominee(
  NomineeID INT IDENTITY(1,1) CONSTRAINT PK_Nominee PRIMARY KEY,
  CompanyID INT NOT NULL,
  EmployeeID INT NOT NULL CONSTRAINT FK_Nominee_Emp REFERENCES mst.Employee(EmployeeID),
  NomineeName NVARCHAR(100) NOT NULL, Relation NVARCHAR(30) NOT NULL, DOB DATE NULL,
  SharePct DECIMAL(5,2) NOT NULL CONSTRAINT CK_Nominee_Share CHECK (SharePct > 0 AND SharePct <= 100),
  IsActive BIT NOT NULL CONSTRAINT DF_Nominee_Active DEFAULT 1,
  CreatedBy INT NULL, CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_Nominee_CreatedOn DEFAULT SYSUTCDATETIME(),
  ModifiedBy INT NULL, ModifiedOn DATETIME2(0) NULL);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Employee_Name' AND object_id=OBJECT_ID(N'mst.Employee'))
  CREATE INDEX IX_Employee_Name ON mst.Employee(CompanyID, FullName);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Nominee_Emp' AND object_id=OBJECT_ID(N'mst.EmployeeNominee'))
  CREATE INDEX IX_Nominee_Emp ON mst.EmployeeNominee(EmployeeID);
GO
/* ---- Row-Level Security: tenant isolation ----
   App connections must set SESSION_CONTEXT CompanyID (sec.usp_Session_SetCompany).
   No context + db_owner member (support staff in SSMS) = bypass. App login is NOT db_owner. */
IF OBJECT_ID(N'sec.fn_CompanyPredicate') IS NULL
EXEC(N'CREATE FUNCTION sec.fn_CompanyPredicate(@CompanyID INT) RETURNS TABLE WITH SCHEMABINDING AS RETURN
  SELECT 1 AS Allowed
  WHERE @CompanyID = CAST(SESSION_CONTEXT(N''CompanyID'') AS INT)
     OR (SESSION_CONTEXT(N''CompanyID'') IS NULL AND IS_MEMBER(N''db_owner'') = 1)');
GO
IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE name = N'CompanyPolicy')
BEGIN
  DECLARE @t TABLE (n NVARCHAR(128));
  INSERT @t VALUES (N'mst.Department'),(N'mst.Designation'),(N'mst.Factory'),(N'mst.Employee'),(N'mst.EmployeeNominee'),(N'cfg.Counter');
  DECLARE @sql NVARCHAR(MAX) = N'CREATE SECURITY POLICY sec.CompanyPolicy ' +
    STUFF((SELECT N', ADD FILTER PREDICATE sec.fn_CompanyPredicate(CompanyID) ON ' + n +
                  N', ADD BLOCK PREDICATE sec.fn_CompanyPredicate(CompanyID) ON ' + n + N' AFTER INSERT' +
                  N', ADD BLOCK PREDICATE sec.fn_CompanyPredicate(CompanyID) ON ' + n + N' AFTER UPDATE'
           FROM @t FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, N'') + N' WITH (STATE = ON)';
  EXEC(@sql);
END
-- Later phases (att, pay, stat, car): ALTER SECURITY POLICY sec.CompanyPolicy ADD FILTER/BLOCK ... for each new table.
GO
IF NOT EXISTS (SELECT 1 FROM cfg.SchemaVersion WHERE ScriptName=N'Data_0001_init')
  INSERT cfg.SchemaVersion(ScriptName) VALUES (N'Data_0001_init');
GO
