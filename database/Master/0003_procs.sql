/* 0003_procs.sql | Target: HRMS_Master | Re-runnable (CREATE OR ALTER) */
SET NOCOUNT ON;
GO
CREATE OR ALTER PROCEDURE sec.usp_User_GetByUsername @Username NVARCHAR(50) AS
BEGIN
  SET NOCOUNT ON;
  SELECT UserID, Username, FullName, PasswordHash, Salt, Iterations, RoleID, UserType,
         FailedCount, LockedUntil, MustChangePwd, IsActive
  FROM sec.[User] WHERE Username = @Username;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_User_LoginFailed @UserID INT, @MaxFailed INT, @LockMinutes INT AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  DECLARE @n INT;
  BEGIN TRAN;
    SELECT @n = CASE WHEN LockedUntil IS NOT NULL AND LockedUntil <= SYSUTCDATETIME() THEN 1 ELSE FailedCount + 1 END
    FROM sec.[User] WITH (UPDLOCK) WHERE UserID = @UserID;
    UPDATE sec.[User]
       SET FailedCount = @n,
           LockedUntil = CASE WHEN @n >= @MaxFailed THEN DATEADD(MINUTE, @LockMinutes, SYSUTCDATETIME()) ELSE NULL END
     WHERE UserID = @UserID;
  COMMIT;
  SELECT FailedCount, LockedUntil FROM sec.[User] WHERE UserID = @UserID;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_User_LoginSucceeded @UserID INT AS
BEGIN
  SET NOCOUNT ON;
  UPDATE sec.[User] SET FailedCount = 0, LockedUntil = NULL, LastLoginOn = SYSUTCDATETIME() WHERE UserID = @UserID;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_User_Create
  @Username NVARCHAR(50), @FullName NVARCHAR(100), @Email NVARCHAR(150) = NULL, @Mobile VARCHAR(15) = NULL,
  @PasswordHash VARBINARY(64), @Salt VARBINARY(32), @Iterations INT, @RoleID INT,
  @CreatedBy INT = NULL, @NewUserID INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  IF EXISTS (SELECT 1 FROM sec.[User] WHERE Username = @Username) THROW 50010, 'Username already exists.', 1;
  DECLARE @Type VARCHAR(10) = (SELECT UserType FROM sec.Role WHERE RoleID = @RoleID AND IsActive = 1);
  IF @Type IS NULL THROW 50011, 'Invalid role.', 1;
  INSERT sec.[User](Username, FullName, Email, Mobile, PasswordHash, Salt, Iterations, RoleID, UserType, MustChangePwd, CreatedBy)
  VALUES (@Username, @FullName, @Email, @Mobile, @PasswordHash, @Salt, @Iterations, @RoleID, @Type, 1, @CreatedBy);
  SET @NewUserID = SCOPE_IDENTITY();
END
GO
CREATE OR ALTER PROCEDURE sec.usp_User_SetPassword
  @UserID INT, @PasswordHash VARBINARY(64), @Salt VARBINARY(32), @Iterations INT, @MustChange BIT
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE sec.[User]
     SET PasswordHash = @PasswordHash, Salt = @Salt, Iterations = @Iterations, MustChangePwd = @MustChange,
         FailedCount = 0, LockedUntil = NULL, ModifiedBy = @UserID, ModifiedOn = SYSUTCDATETIME()
   WHERE UserID = @UserID;
  IF @@ROWCOUNT = 0 THROW 50012, 'User not found.', 1;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_User_SetActive @UserID INT, @IsActive BIT, @ModifiedBy INT AS
BEGIN
  SET NOCOUNT ON;
  UPDATE sec.[User] SET IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedOn = SYSUTCDATETIME() WHERE UserID = @UserID;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_User_Count AS
BEGIN
  SET NOCOUNT ON;
  SELECT COUNT(*) FROM sec.[User];
END
GO
CREATE OR ALTER PROCEDURE sec.usp_Role_GetByType @UserType VARCHAR(10) AS
BEGIN
  SET NOCOUNT ON;
  SELECT TOP (1) RoleID FROM sec.Role WHERE UserType = @UserType AND IsActive = 1 ORDER BY RoleID;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_Permission_ListByUser @UserID INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT p.Code
  FROM sec.[User] u
  JOIN sec.RolePermission rp ON rp.RoleID = u.RoleID
  JOIN sec.Permission p ON p.PermissionID = rp.PermissionID
  WHERE u.UserID = @UserID AND u.IsActive = 1;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_UserCompany_ListByUser @UserID INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT c.CompanyID, c.CompanyCode, c.CompanyName
  FROM sec.UserCompany uc JOIN mst.Company c ON c.CompanyID = uc.CompanyID
  WHERE uc.UserID = @UserID AND uc.IsActive = 1 AND c.IsActive = 1
  ORDER BY c.CompanyName;
END
GO
CREATE OR ALTER PROCEDURE sec.usp_UserCompany_Check @UserID INT, @CompanyID INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT CAST(CASE WHEN EXISTS (
      SELECT 1 FROM sec.UserCompany uc
      JOIN mst.Company c ON c.CompanyID = uc.CompanyID AND c.IsActive = 1
      JOIN sec.[User] u ON u.UserID = uc.UserID AND u.IsActive = 1
      WHERE uc.UserID = @UserID AND uc.CompanyID = @CompanyID AND uc.IsActive = 1)
    THEN 1 ELSE 0 END AS BIT);
END
GO
CREATE OR ALTER PROCEDURE sec.usp_UserCompany_Assign @UserID INT, @CompanyID INT, @ActorID INT AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  IF NOT EXISTS (SELECT 1 FROM sec.[User] WHERE UserID = @UserID) THROW 50013, 'User not found.', 1;
  IF NOT EXISTS (SELECT 1 FROM mst.Company WHERE CompanyID = @CompanyID) THROW 50014, 'Company not found.', 1;
  UPDATE sec.UserCompany SET IsActive = 1 WHERE UserID = @UserID AND CompanyID = @CompanyID;
  IF @@ROWCOUNT = 0
    INSERT sec.UserCompany(UserID, CompanyID, CreatedBy) VALUES (@UserID, @CompanyID, @ActorID);
END
GO
CREATE OR ALTER PROCEDURE sec.usp_UserCompany_Remove @UserID INT, @CompanyID INT AS
BEGIN
  SET NOCOUNT ON;
  UPDATE sec.UserCompany SET IsActive = 0 WHERE UserID = @UserID AND CompanyID = @CompanyID;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Company_Create
  @CompanyCode NVARCHAR(20), @CompanyName NVARCHAR(150),
  @AddressLine1 NVARCHAR(150) = NULL, @AddressLine2 NVARCHAR(150) = NULL, @City NVARCHAR(60) = NULL,
  @DistrictID INT = NULL, @StateID INT = NULL, @Pincode CHAR(6) = NULL,
  @Phone VARCHAR(15) = NULL, @Email NVARCHAR(150) = NULL, @ContactPerson NVARCHAR(100) = NULL,
  @CreatedBy INT, @NewCompanyID INT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET XACT_ABORT ON;
  IF EXISTS (SELECT 1 FROM mst.Company WHERE CompanyCode = @CompanyCode) THROW 50030, 'Company code already exists.', 1;
  BEGIN TRAN;
    INSERT mst.Company(CompanyCode, CompanyName, AddressLine1, AddressLine2, City, DistrictID, StateID, Pincode, Phone, Email, ContactPerson, CreatedBy)
    VALUES (@CompanyCode, @CompanyName, @AddressLine1, @AddressLine2, @City, @DistrictID, @StateID, @Pincode, @Phone, @Email, @ContactPerson, @CreatedBy);
    SET @NewCompanyID = SCOPE_IDENTITY();
    INSERT mst.CompanyStatuary(CompanyID) VALUES (@NewCompanyID);
    INSERT sec.UserCompany(UserID, CompanyID, CreatedBy) VALUES (@CreatedBy, @NewCompanyID, @CreatedBy);  -- creator gets access
  COMMIT;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Company_Update
  @CompanyID INT, @CompanyName NVARCHAR(150),
  @AddressLine1 NVARCHAR(150) = NULL, @AddressLine2 NVARCHAR(150) = NULL, @City NVARCHAR(60) = NULL,
  @DistrictID INT = NULL, @StateID INT = NULL, @Pincode CHAR(6) = NULL,
  @Phone VARCHAR(15) = NULL, @Email NVARCHAR(150) = NULL, @ContactPerson NVARCHAR(100) = NULL, @ModifiedBy INT
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE mst.Company
     SET CompanyName=@CompanyName, AddressLine1=@AddressLine1, AddressLine2=@AddressLine2, City=@City,
         DistrictID=@DistrictID, StateID=@StateID, Pincode=@Pincode, Phone=@Phone, Email=@Email,
         ContactPerson=@ContactPerson, ModifiedBy=@ModifiedBy, ModifiedOn=SYSUTCDATETIME()
   WHERE CompanyID=@CompanyID;
  IF @@ROWCOUNT = 0 THROW 50031, 'Company not found.', 1;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Company_SetActive @CompanyID INT, @IsActive BIT, @ModifiedBy INT AS
BEGIN
  SET NOCOUNT ON;
  UPDATE mst.Company SET IsActive=@IsActive, ModifiedBy=@ModifiedBy, ModifiedOn=SYSUTCDATETIME() WHERE CompanyID=@CompanyID;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Company_Get @CompanyID INT AS
BEGIN
  SET NOCOUNT ON;
  SELECT c.*, s.ShopActNo, s.ShopActDate, s.GSTNo, s.GSTDate, s.EPFRegnNo, s.EPFRegnDate, s.ESIRegnNo, s.ESIRegnDate,
         s.LabourRegnNo, s.LabourRegnDate, s.PSARARegnNo, s.PSARARegnDate
  FROM mst.Company c LEFT JOIN mst.CompanyStatuary s ON s.CompanyID = c.CompanyID
  WHERE c.CompanyID = @CompanyID;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_Company_SaveStatuary
  @CompanyID INT,
  @ShopActNo VARCHAR(30)=NULL, @ShopActDate DATE=NULL, @GSTNo VARCHAR(15)=NULL, @GSTDate DATE=NULL,
  @EPFRegnNo VARCHAR(30)=NULL, @EPFRegnDate DATE=NULL, @ESIRegnNo VARCHAR(30)=NULL, @ESIRegnDate DATE=NULL,
  @LabourRegnNo VARCHAR(30)=NULL, @LabourRegnDate DATE=NULL, @PSARARegnNo VARCHAR(30)=NULL, @PSARARegnDate DATE=NULL,
  @ModifiedBy INT
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE mst.CompanyStatuary
     SET ShopActNo=@ShopActNo, ShopActDate=@ShopActDate, GSTNo=@GSTNo, GSTDate=@GSTDate,
         EPFRegnNo=@EPFRegnNo, EPFRegnDate=@EPFRegnDate, ESIRegnNo=@ESIRegnNo, ESIRegnDate=@ESIRegnDate,
         LabourRegnNo=@LabourRegnNo, LabourRegnDate=@LabourRegnDate, PSARARegnNo=@PSARARegnNo, PSARARegnDate=@PSARARegnDate,
         ModifiedBy=@ModifiedBy, ModifiedOn=SYSUTCDATETIME()
   WHERE CompanyID=@CompanyID;
  IF @@ROWCOUNT = 0 THROW 50032, 'Company not found.', 1;
END
GO
CREATE OR ALTER PROCEDURE mst.usp_State_List AS
BEGIN SET NOCOUNT ON; SELECT StateID, StateCode, StateName FROM mst.States WHERE IsActive=1 ORDER BY StateName; END
GO
CREATE OR ALTER PROCEDURE mst.usp_District_List @StateID INT AS
BEGIN SET NOCOUNT ON; SELECT DistrictID, DistrictName FROM mst.District WHERE StateID=@StateID AND IsActive=1 ORDER BY DistrictName; END
GO
CREATE OR ALTER PROCEDURE mst.usp_Bank_List AS
BEGIN SET NOCOUNT ON; SELECT BankID, BankName FROM mst.Banks WHERE IsActive=1 ORDER BY BankName; END
GO
CREATE OR ALTER PROCEDURE cfg.usp_Setting_GetAll AS
BEGIN SET NOCOUNT ON; SELECT SettingKey, SettingValue FROM cfg.Setting; END
GO
CREATE OR ALTER PROCEDURE aud.usp_AuditLog_Write
  @UserID INT = NULL, @Username NVARCHAR(50) = NULL, @CompanyID INT = NULL, @MachineName NVARCHAR(100) = NULL,
  @Action VARCHAR(40), @TableName VARCHAR(80) = NULL, @RecordKey NVARCHAR(100) = NULL,
  @OldValue NVARCHAR(MAX) = NULL, @NewValue NVARCHAR(MAX) = NULL, @Detail NVARCHAR(500) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  INSERT aud.AuditLog(UserID, Username, CompanyID, MachineName, Action, TableName, RecordKey, OldValue, NewValue, Detail)
  VALUES (@UserID, @Username, @CompanyID, @MachineName, @Action, @TableName, @RecordKey, @OldValue, @NewValue, @Detail);
END
GO
IF NOT EXISTS (SELECT 1 FROM cfg.SchemaVersion WHERE ScriptName=N'Master_0003_procs')
  INSERT cfg.SchemaVersion(ScriptName) VALUES (N'Master_0003_procs');
GO
