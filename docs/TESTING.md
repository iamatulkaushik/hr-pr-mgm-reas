# Testing Plan

## Levels
1. **Unit tests** (`Revolution.Hrms.Tests`): every calculation engine, permission checks, validation rules.
   Test framework: **xUnit** (chosen by Dev Sigma). Run with `dotnet test`. Tests that need Dev Brave's AuthService and PasswordHasher sit in `Pending\` and run with `dotnet test -p:BraveMerged=true` once those are merged.
2. **Manual module tests**: the checklists below with sample data.
3. **Integration and UAT**: a full cycle on the server PC with client PCs (13-10 to 14-10).

## Rules
- Unit test every calculation engine (attendance, salary, EPF, ESI, and later gratuity and bonus).
- Required cases: ceiling boundary, half day, zero overtime, mid-month join and exit.
- Both pay types: Daily wage (rate x days) and Monthly salary (structure x days paid / days in month).
- Validate test data against a manual calculation or a government calculator.
- No release with a failing test. Test a restore before every release.

## Sample data
At least: 2 companies, 3 departments, 20 employees (both pay types), one month of attendance, 1 payroll run. Users: one Associate, one Company user, two Operators (one with each company, one with both).

## Checklists
### Auth and access
- [ ] Login success and failure, lockout after 5 failed attempts
- [ ] Forced password change on first login
- [ ] Idle lock after 10 minutes, unlock with password
- [ ] Associate, Company and Operator each see only their allowed menus (`ACCESS_CONTROL.md`)
- [ ] Operator sees and edits only assigned companies. Removing access takes effect on the next action
- [ ] Operator cannot reach users, rates, backup, audit or payroll approval, even by direct navigation
- [ ] Associate creates Company and Operator users and assigns companies. Company admin creates Operators for own company only
- [ ] Row-level security: with the app login, a query without session context returns no tenant rows

### Masters and employees
- [ ] Add, edit, search, deactivate. Duplicate employee code blocked within a company
- [ ] Required fields validated. Pay type must be set

### Attendance
- [ ] Daily entry and monthly summary. Half day counts 0.5
- [ ] Holidays and weekly off paid. Absent unpaid unless approved leave
- [ ] Month-close lock. Reopen needs a reason and is audited. Reopen blocked once payroll is approved
- [ ] Operator adds and edits attendance before the lock. After the lock the edit is refused. Only an Associate or Company admin can reopen

### Payroll
- [ ] Correct gross, EPF, ESI and net for a known example in each pay type
- [ ] Overtime at 2x ordinary rate
- [ ] Negative net pay flagged and blocked from approval
- [ ] States Draft, Processed, Approved, Locked. Locked run cannot be edited
- [ ] Payroll cannot start before attendance is locked

### Backup
- [ ] Backup script creates a file. Restore returns the same data

## Extra checks before release
- [ ] Payroll parallel run against a manual calculation: 100% match
- [ ] Several PCs working at the same time on the LAN (target 10 users, minimum 3 PCs for 15-10)
- [ ] **Cross-company leak test: zero leaks** on every list screen
- [ ] Audit entries verified for payroll, rate changes and attendance unlocks
- [ ] Later: 1,000-employee payroll in under 60 seconds, security checklist and LAN penetration test, statutory reports match government forms

## Bug tracking
GitHub Issues with severity: Blocker, Major, Minor. From 13-10 only Blocker and Major bugs are fixed.

## Release checklist
- [ ] All Blocker and Major bugs closed
- [ ] Clean-PC install and run on server and clients
- [ ] Backup and restore verified
- [ ] `CHANGELOG.md` updated
- [ ] `v0.3.0` tag created
