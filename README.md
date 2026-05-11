# NavisAutoClash

Automated Clash Test Generation for Navisworks.

> [!WARNING]
> **Current Status: NOT WORKING**
> This version is currently encountering a crash when loaded via the Navisworks Add-In Manager.
> Error: `System.ArgumentOutOfRangeException` in `SearchAssemblyFileInTempFolder`.
> We are currently investigating assembly resolution issues in .NET 6.

## Architecture
- **Core**: Business logic and interfaces.
- **Infrastructure**: Navisworks API implementations and repositories.
- **UI**: WPF/MVVM interface with search functionality.
- **Plugin**: Entry point for Navisworks 2024.
