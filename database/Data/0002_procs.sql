/* 0002_procs.sql | Target: HRMS_Data | Re-runnable. CompanyID always comes from SESSION_CONTEXT, never from a parameter. */
SET NOCOUNT ON;
GO
CREATE OR ALTER PROCEDURE sec.usp_Session_SetCompany @CompanyID INT, @UserID INT = NULL AS
BEGIN
  SET NOCOUNT ON;
  EXEC sp_set_session_context @key = N'CompanyID', @value = @CompanyID, @read_only = 1;
  EXEC sp_set_session_context @key = N'UserID',    @value = @UserID,    @read_only = 1;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Department_Create @DepartmentName NVARCHAR(80), @CreatedBy INT, @NewID INT OUTPUT AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  DECLARE @CompanyID INT = CAST(SESSION_CONTEXT(N'CompanyID') AS INT);
  IF @CompanyID IS NULL THROW 50001, 'No company selected.', 1;
  INSERT mst.Department(CompanyID, DepartmentName, CreatedBy) VALUES (@CompanyID, @DepartmentName, @CreatedBy);
  SET @NewID = SCOPE_IDENTITY();
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Department_List @ActiveOnly BIT = 1 AS
BEGIN
  SET NOCOUNT ON;
  SELECT DepartmentID, DepartmentName, IsActive FROM mst.Department WHERE (@ActiveOnly = 0 OR IsActive = 1) ORDER BY DepartmentName;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Designation_Create @DesignationName NVARCHAR(80), @CreatedBy INT, @NewID INT OUTPUT AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  DECLARE @CompanyID INT = CAST(SESSION_CONTEXT(N'CompanyID') AS INT);
  IF @CompanyID IS NULL THROW 50001, 'No company selected.', 1;
  INSERT mst.Designation(CompanyID, DesignationName, CreatedBy) VALUES (@CompanyID, @DesignationName, @CreatedBy);
  SET @NewID = SCOPE_IDENTITY();
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Designation_List @ActiveOnly BIT = 1 AS
BEGIN
  SET NOCOUNT ON;
  SELECT DesignationID, DesignationName, IsActive FROM mst.Designation WHERE (@ActiveOnly = 0 OR IsActive = 1) ORDER BY DesignationName;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Employee_Create
  @FullName NVARCHAR(100), @FatherHusbandName NVARCHAR(100) = NULL, @Gender CHAR(1), @DOB DATE, @DOJ DATE,
  @DepartmentID INT = NULL, @DesignationID INT = NULL, @FactoryID INT = NULL, @PayType VARCHAR(10),
  @Mobile VARCHAR(15) = NULL, @Address NVARCHAR(250) = NULL, @CreatedBy INT,
  @NewEmployeeID INT OUTPUT, @EmpCode NVARCHAR(20) OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  DECLARE @CompanyID INT = CAST(SESSION_CONTEXT(N'CompanyID') AS INT);
  IF @CompanyID IS NULL THROW 50001, 'No company selected.', 1;
  IF DATEDIFF(YEAR, @DOB, @DOJ) - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, @DOB, @DOJ), @DOB) > @DOJ THEN 1 ELSE 0 END < 14
    THROW 50020, 'Employee must be at least 14 years old on joining.', 1;
  -- RLS makes these lookups company-scoped, so another company's IDs are rejected
  IF @DepartmentID  IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mst.Department  WHERE DepartmentID  = @DepartmentID  AND IsActive = 1) THROW 50021, 'Invalid department.', 1;
  IF @DesignationID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mst.Designation WHERE DesignationID = @DesignationID AND IsActive = 1) THROW 50022, 'Invalid designation.', 1;
  IF @FactoryID     IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mst.Factory     WHERE FactoryID     = @FactoryID     AND IsActive = 1) THROW 50023, 'Invalid factory.', 1;
  BEGIN TRAN;
    DECLARE @n INT;
    UPDATE cfg.Counter WITH (UPDLOCK, HOLDLOCK) SET LastValue = LastValue + 1, @n = LastValue + 1
     WHERE CompanyID = @CompanyID AND CounterName = 'EmpCode';
    IF @@ROWCOUNT = 0
    BEGIN
      INSERT cfg.Counter(CompanyID, CounterName, LastValue) VALUES (@CompanyID, 'EmpCode', 1);
      SET @n = 1;
    END
    SET @EmpCode = N'E' + CASE WHEN @n < 100000 THEN RIGHT('00000' + CAST(@n AS VARCHAR(10)), 5) ELSE CAST(@n AS VARCHAR(10)) END;
    INSERT mst.Employee(CompanyID, EmpCode, FullName, FatherHusbandName, Gender, DOB, DOJ, DepartmentID, DesignationID, FactoryID,
                        PayType, Mobile, Address, CreatedBy)
    VALUES (@CompanyID, @EmpCode, @FullName, @FatherHusbandName, @Gender, @DOB, @DOJ, @DepartmentID, @DesignationID, @FactoryID,
            @PayType, @Mobile, @Address, @CreatedBy);
    SET @NewEmployeeID = SCOPE_IDENTITY();
  COMMIT;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Employee_Update
  @EmployeeID INT, @FullName NVARCHAR(100), @FatherHusbandName NVARCHAR(100) = NULL, @Gender CHAR(1), @DOB DATE, @DOJ DATE,
  @DepartmentID INT = NULL, @DesignationID INT = NULL, @FactoryID INT = NULL, @PayType VARCHAR(10),
  @Mobile VARCHAR(15) = NULL, @Address NVARCHAR(250) = NULL, @ModifiedBy INT
AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  IF CAST(SESSION_CONTEXT(N'CompanyID') AS INT) IS NULL THROW 50001, 'No company selected.', 1;
  IF DATEDIFF(YEAR, @DOB, @DOJ) - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, @DOB, @DOJ), @DOB) > @DOJ THEN 1 ELSE 0 END < 14
    THROW 50020, 'Employee must be at least 14 years old on joining.', 1;
  IF @DepartmentID  IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mst.Department  WHERE DepartmentID  = @DepartmentID  AND IsActive = 1) THROW 50021, 'Invalid department.', 1;
  IF @DesignationID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mst.Designation WHERE DesignationID = @DesignationID AND IsActive = 1) THROW 50022, 'Invalid designation.', 1;
  IF @FactoryID     IS NOT NULL AND NOT EXISTS (SELECT 1 FROM mst.Factory     WHERE FactoryID     = @FactoryID     AND IsActive = 1) THROW 50023, 'Invalid factory.', 1;
  UPDATE mst.Employee
     SET FullName=@FullName, FatherHusbandName=@FatherHusbandName, Gender=@Gender, DOB=@DOB, DOJ=@DOJ,
         DepartmentID=@DepartmentID, DesignationID=@DesignationID, FactoryID=@FactoryID, PayType=@PayType,
         Mobile=@Mobile, Address=@Address, ModifiedBy=@ModifiedBy, ModifiedOn=SYSUTCDATETIME()
   WHERE EmployeeID = @EmployeeID;
  IF @@ROWCOUNT = 0 THROW 50024, 'Employee not found.', 1;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Employee_Get @EmployeeID INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT e.EmployeeID, e.EmpCode, e.FullName, e.FatherHusbandName, e.Gender, e.DOB, e.DOJ, e.DOL,
         e.DepartmentID, d.DepartmentName, e.DesignationID, g.DesignationName, e.FactoryID, f.FactoryName,
         e.PayType, e.Mobile, e.Address, e.IsActive
  FROM mst.Employee e
  LEFT JOIN mst.Department  d ON d.DepartmentID  = e.DepartmentID
  LEFT JOIN mst.Designation g ON g.DesignationID = e.DesignationID
  LEFT JOIN mst.Factory     f ON f.FactoryID     = e.FactoryID
  WHERE e.EmployeeID = @EmployeeID;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Employee_Search
  @Text NVARCHAR(100) = NULL, @ActiveOnly BIT = 1, @PageNo INT = 1, @PageSize INT = 50, @TotalRows INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  IF @PageNo < 1 SET @PageNo = 1;
  IF @PageSize < 1 OR @PageSize > 500 SET @PageSize = 50;
  SELECT @TotalRows = COUNT(*) FROM mst.Employee e
   WHERE (@ActiveOnly = 0 OR e.IsActive = 1)
     AND (@Text IS NULL OR e.EmpCode LIKE @Text + N'%' OR e.FullName LIKE N'%' + @Text + N'%' OR e.Mobile LIKE @Text + N'%');
  SELECT e.EmployeeID, e.EmpCode, e.FullName, e.Gender, e.DOJ, e.DOL, d.DepartmentName, g.DesignationName, e.PayType, e.Mobile, e.IsActive
  FROM mst.Employee e
  LEFT JOIN mst.Department  d ON d.DepartmentID  = e.DepartmentID
  LEFT JOIN mst.Designation g ON g.DesignationID = e.DesignationID
  WHERE (@ActiveOnly = 0 OR e.IsActive = 1)
    AND (@Text IS NULL OR e.EmpCode LIKE @Text + N'%' OR e.FullName LIKE N'%' + @Text + N'%' OR e.Mobile LIKE @Text + N'%')
  ORDER BY e.FullName, e.EmployeeID
  OFFSET (@PageNo - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Employee_Deactivate @EmployeeID INT, @DOL DATE, @ModifiedBy INT AS
BEGIN
  SET NOCOUNT ON;
  IF CAST(SESSION_CONTEXT(N'CompanyID') AS INT) IS NULL THROW 50001, 'No company selected.', 1;
  IF EXISTS (SELECT 1 FROM mst.Employee WHERE EmployeeID = @EmployeeID AND @DOL < DOJ) THROW 50025, 'Leaving date is before joining date.', 1;
  UPDATE mst.Employee SET IsActive = 0, DOL = @DOL, ModifiedBy = @ModifiedBy, ModifiedOn = SYSUTCDATETIME() WHERE EmployeeID = @EmployeeID;
  IF @@ROWCOUNT = 0 THROW 50024, 'Employee not found.', 1;
END
GO
IF NOT EXISTS (SELECT 1 FROM cfg.SchemaVersion WHERE ScriptName=N'Data_0002_procs')
  INSERT cfg.SchemaVersion(ScriptName) VALUES (N'Data_0002_procs');
GO
