# AGENTS.md

Guidance for coding agents working in this repository.

## Communication

- Reply to the user in Japanese by default.
- Assume the user is a C# WinUI developer.
- C++ or C++/WinRT discussion is welcome when the user asks, but keep C# and WinUI as the default implementation path.

## Repository Shape

- `WinUIGallery/` is the main WinUI app.
- `WinUIGallery/Pages/` contains app-level pages such as home, search, sections, item details, all controls, and settings.
- `WinUIGallery/Samples/ControlPages/` contains individual sample pages shown inside item pages.
- `WinUIGallery/Samples/SamplePages/` contains auxiliary sample windows and pages used by samples.
- `WinUIGallery/Samples/SampleCode/` contains code snippets displayed by the gallery.
- `WinUIGallery/Samples/Data/ControlInfoData.json` is the catalog source of truth for sample groups and items.
- `WinUIGallery/Controls/` contains reusable app controls.
- `WinUIGallery/Helpers/` contains app services, platform helpers, navigation helpers, settings, and data loading helpers.
- `WinUIGallery/Styles/` contains shared resource dictionaries and control styles.
- `WinUIGallery.SourceGenerator/` generates navigation/page mapping code from catalog data.
- `tests/WinUIGallery.UnitTests/` contains unit and UI-thread tests.
- `tests/WinUIGallery.UITests/` contains WinAppDriver/Appium UI tests.

## Build and Test

- Open `WinUIGallery.slnx` in Visual Studio 2022 or newer and set `WinUIGallery` as the startup project.
- Prefer the repository project files as the source of truth for target frameworks, package versions, configurations, and platforms.
- Common commands:

```powershell
dotnet restore WinUIGallery.slnx
msbuild WinUIGallery.slnx /p:Configuration=Debug /p:Platform=x64
dotnet test tests\WinUIGallery.UnitTests\WinUIGallery.UnitTests.csproj
dotnet test tests\WinUIGallery.UnitTests\WinUIGallery.UnitTests.csproj --filter "FullyQualifiedName~TestMethodName"
dotnet test tests\WinUIGallery.UITests\WinUIGallery.UITests.csproj
```

- UI tests require the app plus Appium and WinAppDriver.
- Supported solution configurations include Debug, Release, Preview, Stable, Store, Sideload, Debug-Unpackaged, and Release-Unpackaged. Supported platforms include x86, x64, and ARM64; check solution/project files before changing ARM64EC behavior.

## Data Flow

1. `Samples/Data/ControlInfoData.json` defines groups and control entries.
2. `WinUIGallery.SourceGenerator` reads that JSON at compile time and generates page mappings.
3. `ControlInfoDataSource` loads the same JSON at runtime, normalizes item metadata, and uses generated mappings to decide which pages are included in the build.
4. The shell uses catalog data to populate NavigationView, search, sections, home page lists, protocol activation, and JumpList behavior.

## Code Structure Principles

- Preserve the current WinUI Gallery architecture unless the task explicitly asks for a broader reorganization.
- Prefer small, incremental refactors over large file moves.
- Keep sample behavior easy to inspect; this project is both an app and reference material.
- Do not introduce heavy MVVM or dependency injection ceremony unless it clearly reduces existing coupling.
- Prefer native WinUI controls, resources, and patterns before adding new abstractions or dependencies.
- Keep app shell responsibilities separate from sample page responsibilities.
- Keep catalog data, navigation routing, search logic, and UI test hooks as distinct concerns.

## Main Responsibility Boundaries

### App and Shell

- `App.xaml.cs` should focus on application startup, activation handling, unhandled exceptions, and top-level window creation.
- `MainWindow.xaml` and `MainWindow.xaml.cs` should focus on title bar, frame hosting, NavigationView wiring, and shell-level events.
- Avoid adding more catalog, search, settings, or sample-specific logic directly to `MainWindow`.
- If navigation logic grows, prefer adding a shell/navigation helper or service instead of expanding event handlers.

### Catalog and Search

- Treat `Samples/Data/ControlInfoData.json` as the source of truth for groups and sample metadata.
- Keep catalog normalization in one place: badge strings, default image paths, included-in-build checks, and ID lookups should not be duplicated across pages.
- Prefer read-only catalog surfaces for consumers.
- If adding search behavior, centralize matching/ranking in a search helper or service instead of duplicating query logic in multiple pages.

### Samples

- Keep sample pages focused on demonstrating the control or API named by their catalog item.
- Avoid putting global app state or shell navigation policy inside individual sample pages.
- When adding a sample, update all related pieces together:
  - catalog entry in `Samples/Data/ControlInfoData.json`
  - sample page under `Samples/ControlPages/`
  - displayed snippet files under `Samples/SampleCode/` when needed
  - preview image under `Assets/ControlImages/` when needed
  - tests when behavior or navigation coverage changes
- If reorganizing samples physically, support old and new layouts during migration.

### Adding a Control Page

- Add the item to `Samples/Data/ControlInfoData.json`.
- Keep `UniqueId` unique and match it to the page class name without the `Page` suffix.
- Create `Samples/ControlPages/[ControlName]Page.xaml` and `.xaml.cs`.
- Use `ControlExample` for interactive examples and `SampleCodePresenter` through `Xaml`, `XamlSource`, `CSharp`, or `CSharpSource` for displayed code.
- Place raw snippets under `Samples/SampleCode/` as `.txt` files. Existing naming commonly follows `{ControlName}Sample{N}_{xaml|cs|csharp}.txt`.
- Add or update `Assets/ControlImages/[ControlName].png` when the catalog entry needs a preview image.
- The source generator should handle page mapping; do not manually register the page unless the existing code path requires an exception.

### Controls

- Put reusable UI in `Controls/`.
- Keep demo-only UI inside the relevant sample page unless it is reused by multiple samples.
- Reuse `ControlExample` and `SampleCodePresenter` for sample presentation instead of creating parallel sample wrappers.
- Keep theme support intact for light, dark, and high contrast modes.

### Helpers and Services

- `Helpers/` currently contains mixed concerns. New code should use clearer names and smaller files.
- Prefer names that describe the domain responsibility, for example `Catalog`, `Navigation`, `Settings`, `Windowing`, or `SampleCode`.
- Do not add new global singletons unless they follow an existing pattern and the lifetime is genuinely app-wide.
- When a static helper becomes stateful or hard to test, consider a thin service interface.

### Source Generator

- Keep generated navigation/page mapping behavior compatible with `ControlInfoData.json`.
- Prefer build-time diagnostics for catalog problems such as duplicate IDs, invalid IDs, missing pages, or orphan pages.
- Do not hand-edit generated files.
- Keep source generator model types minimal and independent from WinUI runtime types.

### Build and Content

- Be careful with `WinUIGallery.csproj` and `ContentIncludes.props`; packaging behavior depends on explicit content entries.
- Avoid duplicating content includes.
- Prefer centralizing content rules before adding more long explicit item lists.
- Consider packaged and unpackaged configurations when changing file loading, content paths, or publish behavior.

### Tests

- Add catalog invariant tests for metadata-driven changes.
- Add UI tests only when the change affects navigation, search, accessibility, or visible workflows.
- Keep hidden UI automation hooks isolated from normal shell behavior.
- Do not make tests depend on arbitrary delays when a deterministic wait or existing idle helper is available.
- New control pages are expected to pass the Axe.Windows accessibility scans run by the UI test suite.

## Accessibility

- Set `AutomationProperties.Name` on interactive controls when the visible text or purpose is not already clear to UI Automation.
- Use `AutomationProperties.HeadingLevel` for meaningful section headers.
- Hide decorative-only elements from the accessibility tree with `AutomationProperties.AccessibilityView="Raw"`.
- Ensure keyboard navigation reaches and operates all interactive UI.
- Preserve light, dark, and high contrast support.
- Meet color contrast requirements for new visible UI.

## Coding Conventions

- Use file-scoped namespaces.
- Include the standard Microsoft copyright and MIT license header in C# files.
- Prefer explicit types for built-in types instead of `var`.
- Avoid `this.` qualifiers unless needed for disambiguation.
- Use Allman braces.
- Use 4-space indentation and CRLF line endings.
- Prefer pattern matching over `as` plus null checks.
- Use explicit accessibility modifiers on non-interface members.
- Use PascalCase for types, methods, and properties; prefix interfaces with `I`.
- Use `System.Text.Json` source generation for JSON serialization; do not introduce Newtonsoft.Json.
- Use `xmlns:controls="using:WinUIGallery.Controls"` for custom controls in XAML.
- Pages that bind to control info item collections should generally inherit from `ItemsPageBase`.

## Documentation and References

- Prefer Microsoft Learn for WinUI 3, Windows App SDK, and Windows developer API guidance.
- Prefer WinUI 3 / Windows App SDK documentation over WPF, MAUI, or UWP equivalents.
- Useful reference repositories:
  - `microsoft/microsoft-ui-xaml`
  - `microsoft/WindowsAppSDK`
  - `microsoft/WindowsAppSDK-Samples`
  - `microsoft/ai-dev-gallery`

## Things To Avoid

- Do not perform broad folder moves without tests and compatibility strategy.
- Do not replace WinUI shell patterns with custom UI frameworks.
- Do not add unrelated style, formatting, package, or solution changes while making a focused code change.
- Do not change generated output by hand.
- Do not remove existing user or local changes unless explicitly asked.
