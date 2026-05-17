# BUGS.md — NavisAutoClash Bug Log

All discovered bugs must be logged here **before** work begins on a fix.
Update the entry with root cause, fix summary, and test notes upon resolution.
Reference the bug ID in commit messages: `fix: BUG-001 — assembly crash on load`.

---

## Bug Entry Format

```
### BUG-NNN: Short title
- **Date:** YYYY-MM-DD
- **Area:** Core | Infrastructure | UI | Plugin
- **Status:** Open | In Progress | Resolved
- **Description:** What happened
- **Steps to Reproduce:**
  1. Step one
  2. Step two
- **Root Cause:** (filled when investigated)
- **Fix Summary:** (filled when resolved)
- **Tests Added:** (list test method names, or "Manual only")
```

---

## Bug Log

### BUG-001: Plugin crashes on load in Navisworks 2024 (ArgumentOutOfRangeException in SearchAssemblyFileInTempFolder)
- **Date:** 2026-05-10
- **Area:** Plugin
- **Status:** Resolved
- **Description:** Loading the plugin via the Navisworks Add-In Manager caused Navisworks to crash or show a silent failure. Stack trace pointed to `SearchAssemblyFileInTempFolder`.
- **Steps to Reproduce:**
  1. Build solution (targeting net6.0-windows)
  2. Copy plugin DLL to Navisworks plugin folder
  3. Launch Navisworks 2024 — crash occurs at startup
- **Root Cause:** All projects targeted `.NET 6`, which is incompatible with Navisworks 2024's `.NET Framework 4.8` CLR. The .NET 6 runtime's assembly binder cannot locate dependencies when loaded inside a net48 host process, causing `ArgumentOutOfRangeException` in the internal `SearchAssemblyFileInTempFolder` probing logic.
- **Fix Summary:** Retargeted all four projects (`Core`, `Infrastructure`, `UI`, `Plugin.2024`) from `net6.0`/`net6.0-windows` to `net48`. Added `CopyLocalLockFileAssemblies=true` and `AssemblyResolve` handler to ensure all dependency DLLs are copied next to the plugin and are discoverable at runtime.
- **Tests Added:** Manual — M1 (plugin loads without crash in clean Navisworks 2024 session)

### BUG-002: Navisworks hangs on startup due to system resource starvation
- **Date:** 2026-05-17
- **Area:** Infrastructure/Environment
- **Status:** Resolved
- **Description:** Navisworks was stuck/hanging on startup for over 20 minutes, giving the impression that the add-in was freezing the application.
- **Steps to Reproduce:**
  1. Trigger an internal MSBuild out-of-memory error ("The paging file is too small for this operation to complete").
  2. The `dotnet build` or `msbuild` process fails to terminate gracefully and continues consuming RAM.
  3. Attempt to launch Navisworks Manage 2024.
- **Root Cause:** A runaway `dotnet` build process from an earlier failed compilation attempt was left hanging in the background, consuming ~143 MB of working set memory and starving system resources, which blocked Navisworks from starting successfully. This was not related to the plugin code.
- **Fix Summary:** Manually killed the hanging `dotnet` process (`Stop-Process -Id <PID> -Force`).
- **Tests Added:** Manual only (verified process termination).

### BUG-003: Build errors after migration to .NET Framework 4.8 and Navisworks 2024 API
- **Date:** 2026-05-17
- **Area:** Core | Infrastructure | UI | Plugin
- **Status:** Resolved
- **Description:** Executing `dotnet build` revealed multiple compilation errors due to missing .NET Core API features in .NET 4.8, Navisworks 2024 API signature changes, and invalid WPF XAML properties.
- **Steps to Reproduce:**
  1. Retarget solution to `net48`.
  2. Run `dotnet build NavisAutoClash.sln`.
- **Root Cause:** 
  - `System.HashCode` is not natively available in .NET 4.8.
  - Navisworks API `SavedItem` requires casting to `GroupItem` to access `Children`.
  - Navisworks API `ClashTest` selection assignment requires `CreateSelectionSource(SelectionSet)` rather than direct assignment.
  - `ModelItemCollection` requires `AddRange` instead of a list constructor.
  - `System.Windows.Forms` assembly reference was missing in Plugin project (required for `IWin32Window` interop).
  - Invalid XAML properties (`StackPanel.Spacing`, `TextBlock.TextTransform`) were used.
- **Fix Summary:** 
  - Replaced `HashCode.Combine` with manual XOR hashing in `NwcModelInfo`.
  - Added explicit casts `((GroupItem)item).Children` and updated `SelectionSource` creation.
  - Added `System.Windows.Forms` reference to Plugin `.csproj`.
  - Removed unsupported WPF properties from XAML and replaced `Spacing` with `Margin`.
- **Tests Added:** Automated build verification (`dotnet build` succeeded with 0 errors).
