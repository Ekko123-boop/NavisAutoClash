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
