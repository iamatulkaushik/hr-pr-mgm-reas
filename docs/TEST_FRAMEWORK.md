# Test framework

Chosen: **xUnit** (2.9.x) with Microsoft.NET.Test.Sdk, project `tests/Hrms.Tests`, VB.NET, Option Strict On.

Run: `dotnet test tests/Hrms.Tests`

Rules: one test class per class under test, `Theory` + `InlineData` for boundary cases, no database in unit tests.
