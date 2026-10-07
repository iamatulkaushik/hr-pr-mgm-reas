/* 0002_seed.sql | Target: HRMS_Master | Idempotent */
SET NOCOUNT ON;
GO
INSERT sec.Role(RoleName,UserType)
SELECT v.n, v.t FROM (VALUES (N'Associate','Associate'),(N'Company Admin','Company'),(N'Operator','Operator')) v(n,t)
WHERE NOT EXISTS (SELECT 1 FROM sec.Role r WHERE r.RoleName=v.n);

INSERT sec.Permission(Code,Module,Action)
SELECT v.m+'.'+v.a, v.m, v.a FROM (VALUES
 ('Company','View'),('Company','Add'),('Company','Edit'),('Company','Deactivate'),
 ('Employee','View'),('Employee','Add'),('Employee','Edit'),('Employee','Deactivate'),
 ('User','View'),('User','Add'),('User','Edit'),('User','Deactivate'),('User','AssignCompany'),
 ('Attendance','View'),('Attendance','Add'),('Attendance','Edit'),('Attendance','Lock'),('Attendance','Reopen'),
 ('Payroll','View'),('Payroll','Process'),('Payroll','Approve'),('Payroll','Lock'),
 ('Statutory','View'),('Statutory','Edit'),
 ('Career','View'),('Career','Add'),
 ('Reports','View'),('Reports','Export'),
 ('Audit','View'),
 ('Admin','Backup'),('Admin','Restore')) v(m,a)
WHERE NOT EXISTS (SELECT 1 FROM sec.Permission p WHERE p.Code=v.m+'.'+v.a);

-- Associate: everything
INSERT sec.RolePermission(RoleID,PermissionID)
SELECT r.RoleID,p.PermissionID FROM sec.Role r CROSS JOIN sec.Permission p
WHERE r.UserType='Associate' AND NOT EXISTS (SELECT 1 FROM sec.RolePermission x WHERE x.RoleID=r.RoleID AND x.PermissionID=p.PermissionID);

-- Company admin: everything except company creation, company access assignment, rate/statutory setup, restore
INSERT sec.RolePermission(RoleID,PermissionID)
SELECT r.RoleID,p.PermissionID FROM sec.Role r CROSS JOIN sec.Permission p
WHERE r.UserType='Company'
  AND p.Code NOT IN ('Company.Add','Company.Deactivate','User.AssignCompany','Statutory.Edit','Admin.Restore')
  AND NOT EXISTS (SELECT 1 FROM sec.RolePermission x WHERE x.RoleID=r.RoleID AND x.PermissionID=p.PermissionID);

-- Operator: employees, attendance (until month-close), view-only reports
INSERT sec.RolePermission(RoleID,PermissionID)
SELECT r.RoleID,p.PermissionID FROM sec.Role r CROSS JOIN sec.Permission p
WHERE r.UserType='Operator'
  AND p.Code IN ('Company.View','Employee.View','Employee.Add','Employee.Edit','Employee.Deactivate',
                 'Attendance.View','Attendance.Add','Attendance.Edit','Reports.View')
  AND NOT EXISTS (SELECT 1 FROM sec.RolePermission x WHERE x.RoleID=r.RoleID AND x.PermissionID=p.PermissionID);

INSERT cfg.Setting(SettingKey,SettingValue,Description)
SELECT v.k,v.v,v.d FROM (VALUES
 ('MaxFailedLogins','5',N'Failed logins before lock'),
 ('LockoutMinutes','15',N'Lock duration in minutes'),
 ('IdleTimeoutMinutes','10',N'UI idle lock')) v(k,v,d)
WHERE NOT EXISTS (SELECT 1 FROM cfg.Setting s WHERE s.SettingKey=v.k);

INSERT mst.States(StateCode,StateName)
SELECT v.c,v.n FROM (VALUES
 ('01',N'Jammu and Kashmir'),('02',N'Himachal Pradesh'),('03',N'Punjab'),('04',N'Chandigarh'),('05',N'Uttarakhand'),
 ('06',N'Haryana'),('07',N'Delhi'),('08',N'Rajasthan'),('09',N'Uttar Pradesh'),('10',N'Bihar'),('11',N'Sikkim'),
 ('12',N'Arunachal Pradesh'),('13',N'Nagaland'),('14',N'Manipur'),('15',N'Mizoram'),('16',N'Tripura'),
 ('17',N'Meghalaya'),('18',N'Assam'),('19',N'West Bengal'),('20',N'Jharkhand'),('21',N'Odisha'),
 ('22',N'Chhattisgarh'),('23',N'Madhya Pradesh'),('24',N'Gujarat'),
 ('26',N'Dadra and Nagar Haveli and Daman and Diu'),('27',N'Maharashtra'),('29',N'Karnataka'),('30',N'Goa'),
 ('31',N'Lakshadweep'),('32',N'Kerala'),('33',N'Tamil Nadu'),('34',N'Puducherry'),
 ('35',N'Andaman and Nicobar Islands'),('36',N'Telangana'),('37',N'Andhra Pradesh'),('38',N'Ladakh')) v(c,n)
WHERE NOT EXISTS (SELECT 1 FROM mst.States s WHERE s.StateCode=v.c);

-- Haryana districts: VERIFY against the current official list before release
INSERT mst.District(StateID,DistrictName)
SELECT s.StateID, v.n FROM mst.States s CROSS JOIN (VALUES
 (N'Ambala'),(N'Bhiwani'),(N'Charkhi Dadri'),(N'Faridabad'),(N'Fatehabad'),(N'Gurugram'),(N'Hisar'),(N'Jhajjar'),
 (N'Jind'),(N'Kaithal'),(N'Karnal'),(N'Kurukshetra'),(N'Mahendragarh'),(N'Nuh'),(N'Palwal'),(N'Panchkula'),
 (N'Panipat'),(N'Rewari'),(N'Rohtak'),(N'Sirsa'),(N'Sonipat'),(N'Yamunanagar')) v(n)
WHERE s.StateCode='06' AND NOT EXISTS (SELECT 1 FROM mst.District d WHERE d.StateID=s.StateID AND d.DistrictName=v.n);

INSERT mst.Banks(BankName)
SELECT v.n FROM (VALUES (N'State Bank of India'),(N'Punjab National Bank'),(N'Bank of Baroda'),(N'Canara Bank'),
 (N'Union Bank of India'),(N'Bank of India'),(N'Indian Bank'),(N'HDFC Bank'),(N'ICICI Bank'),(N'Axis Bank'),
 (N'Kotak Mahindra Bank'),(N'IDBI Bank'),(N'Yes Bank'),(N'IndusInd Bank'),(N'Post Office Savings')) v(n)
WHERE NOT EXISTS (SELECT 1 FROM mst.Banks b WHERE b.BankName=v.n);
GO
IF NOT EXISTS (SELECT 1 FROM cfg.SchemaVersion WHERE ScriptName=N'Master_0002_seed')
  INSERT cfg.SchemaVersion(ScriptName) VALUES (N'Master_0002_seed');
GO
