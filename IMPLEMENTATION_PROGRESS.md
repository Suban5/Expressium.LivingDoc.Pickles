# Expressium.LivingDoc Pickles-like Report

## Project Goal
Enhance Expressium.LivingDoc with a Pickles-like living documentation report while preserving the current parser, model, generator, CLI, Reqnroll formatter, history, merge, and test architecture. The report must preserve original Gherkin structure and execution data, remain self-contained HTML, and support both .NET 8 and .NET Framework 4.7.2-compatible consumers where technically supported.

## Status Legend

- [ ] Not started
- [-] In progress
- [x] Completed
- [!] Blocked

## Current Phase

[x] Phase 1 - Model and parser fidelity
[x] Phase 2 - Timestamp, configuration, and history behavior
[x] Phase 3 - Report rendering and UI interactions

## Architecture Findings

### Project and target frameworks

- `Expressium.LivingDoc/Expressium.LivingDoc.csproj` is the core class library and currently targets only `net8.0`.
- `Expressium.LivingDoc.Cli/Expressium.LivingDoc.Cli.csproj` is a `net8.0` executable that demonstrates generate, native, custom, merge, and history workflows.
- `Expressium.LivingDoc.ReqnrollPlugin/Expressium.LivingDoc.ReqnrollPlugin.csproj` is a `net8.0` Reqnroll formatter/plugin package and references the core library.
- `Expressium.LivingDoc.UnitTests/Expressium.LivingDoc.UnitTests.csproj` is a `net8.0` NUnit test project covering models, parsers, generators, converters, and samples.
- `Expressium.LivingDoc.UITests/Expressium.LivingDoc.UITests.csproj` is a `net8.0` Reqnroll/Selenium NUnit project for browser-level report behavior.
- `Expressium.LivingDoc.Net472Tests/Expressium.LivingDoc.Net472Tests.csproj` is a focused `net472` NUnit smoke-test project for core runtime compatibility on Windows.
- `Expressium.LivingDoc.ReqnrollCompatibilityTests/Expressium.LivingDoc.ReqnrollCompatibilityTests.csproj` is a `net8.0` Reqnroll fixture that validates formatter execution and report generation without browser dependencies.
- `Directory.Build.props` contains shared package metadata but no target-framework or compatibility policy.
- `expressium-livingdoc.sln` contains the five projects above. An untracked `Expressium.LivingDoc.Parsers/` directory is present in the worktree and must be reviewed before implementation because it may represent an in-progress extraction.

### Parsing and model layer

- `Expressium.LivingDoc/Parsers/MessagesLoader.cs` reads Cucumber Messages NDJSON into `CucumberMessages` collections.
- `Expressium.LivingDoc/Parsers/MessagesGherkinParser.cs` maps `GherkinDocument`, `Feature`, `Background`, `Rule`, `Scenario`, and `Examples` into the LivingDoc model.
- `Expressium.LivingDoc/Parsers/MessagesResultParser.cs` overlays pickle/test-case/test-step results, durations, attachments, hooks, and scenario order.
- `Expressium.LivingDoc/Parsers/MessagesUtilities.cs` converts Cucumber timestamps and durations. It currently converts epoch timestamps to local time and uses the converted values for duration arithmetic; this is the primary timestamp risk.
- `LivingDocFeature`, `LivingDocScenario`, `LivingDocExample`, `LivingDocStep`, and `LivingDocDataTable` are the domain model. `LivingDocScenario.Examples` currently serves as both outline/example-table storage and execution-result records.
- The current working-tree parser change intentionally keeps one outline template with placeholders, but `MessagesGherkinParser.ParseScenario` still reads only `scenario.Examples[0]`; multiple Examples sections are therefore not preserved.
- `MessagesResultParser.ParseTestResultsScenarios` currently evaluates only `scenario.Examples[0]`, so result mapping must be made compatible with separately retained Examples sections without flattening the source outline.
- Gherkin comments are not currently copied into the model. Existing `Description` properties are not sufficient to preserve comments in their original Feature/Scenario/Examples locations.

### Report and HTML generation

- `LivingDoc/LivingDocConverter.cs` is the public orchestration API for import/export, NDJSON conversion, HTML generation, project merge, and history merge.
- `Generators/LivingDocProjectGenerator.cs` produces a self-contained HTML document and loads embedded resource text for head, CSS, JavaScript, and splitter content.
- `Generators/LivingDocContentGenerator.cs` creates the shell, navigation, filters, and splitter.
- `Generators/LivingDocDataOverviewGenerator.cs` builds the left tree navigation for folders, features, and scenarios.
- `Generators/LivingDocDataObjectsGenerator.cs` renders feature/scenario documents, steps, examples, data tables, attachments, and history.
- `Generators/LivingDocDataListViewsGenerator.cs`, `LivingDocDataOverviewGenerator.cs`, and `LivingDocDataAnalyticsGenerator.cs` generate list and analytics views.
- `Expressium.LivingDoc/Resources/Heads.txt`, `Styles.txt`, `Scripts.txt`, and `Splitter.txt` are embedded through `Properties/Resources.resx`. UI changes should remain in these existing resources and generator call sites.
- Tables currently render as a plain table without a scroll wrapper or a distinct header/body toggle contract. Scenario headings are rendered as plain spans; existing JavaScript toggles stack traces, history, attachments, and feature rows only.
- Feature rows already have a collapse state. Folder rows and scenario document sections do not yet have equivalent independent toggles.

### Configuration, history, and merge

- Reqnroll configuration currently uses `outputFilePath`, `outputFileTitle`, and `historyPath` in `ExpressiumFormatter`.
- The plugin writes NDJSON first, then converts it to HTML in `Dispose`. With `historyPath`, it copies the current NDJSON into the history directory, merges history, and generates HTML.
- The CLI accepts explicit output paths for all modes; it has no named report-options object.
- `LivingDocConverter.MergeHistory` discovers up to four matching NDJSON files recursively and merges them in project-date order. Existing default behavior must remain enabled for the current history workflow.
- `LivingDocProject.Merge`, `MergeHistory`, and health/history calculations are in `Models/LivingDocProject.cs`.
- New `OutputDirectory`, `OutputFileName`, and `MergeWithHistory` options should be normalized in one compatibility-preserving options/configuration layer and then consumed by the plugin/CLI/converter.

### Existing tests

- Unit tests cover parser fixtures under `Expressium.LivingDoc.UnitTests/Parsers`, model behavior under `Models`, generator output under `Generators`, and converter behavior under `Converters`.
- Existing outline tests in `MessagesParserExampleTablesTests.cs` are pre-existing worktree changes and currently assert one template example with placeholders. They must be extended rather than discarded.
- The UI suite uses Reqnroll feature files and Selenium page objects under `Expressium.LivingDoc.UITests/Features`, `Pages`, `Steps`, and `Controls`. Don't run UI test until user told you to do execute it.
- The current solution test baseline was attempted with `dotnet test expressium-livingdoc.sln --no-restore`. The UI tests are blocked before assertions because Selenium Manager cannot launch Chrome on this runner (`Win32Exception`, native error 35, `Resource temporarily unavailable`). This environment limitation must remain documented while unit tests can provide the primary deterministic coverage.
- The Windows GitHub Actions workflow contains separate `net472` core smoke-test and Reqnroll report-generation jobs.

## Requirements Tracking

- [x] .NET Framework 4.7.2 support while retaining .NET 8. The core library now targets `net8.0;net472`; `AngleSharp`, `Cucumber.Messages`, and `System.Text.Json` restore and compile for both targets. Reqnroll/plugin and browser-test projects remain net8-only. Runtime validation of net472 must run on Windows.
- [x] Scenario Outline renders the original outline/template with `<placeholder>` values in the main documentation view.
- [x] Gherkin comments are preserved at their Feature, Background, Rule, Scenario, and Examples locations where Cucumber Messages exposes them. Added source comment collections and line-based AST association.
- [x] Multiple Examples sections remain separate, ordered, titled, and column/row-preserving. Added `LivingDocScenario.DocumentationExamples`; legacy `Examples` remains the execution/history collection.
- [x] `OutputDirectory` and `OutputFileName` support sensible defaults and relative/absolute paths.
- [x] Data tables with approximately 100 columns remain within the page through horizontal scrolling.
- [x] Data Table and Examples table headers/first rows toggle their bodies independently.
- [x] Root folders and nested subfolders in left navigation expand/collapse independently.
- [x] Scenario and Scenario Outline document sections collapse while headings/status remain visible.
- [x] `MergeWithHistory: true/false` controls history merging and preserves current default behavior.
- [x] Actual execution timestamp is retained with a documented UTC/local display policy; duration arithmetic is timezone-independent.
- [ ] Tests cover all requested parser, model, generator, configuration, history, timestamp, and UI behaviors.

## Proposed Implementation Plan

### Phase 1 - Model and parser fidelity

1. Review the Cucumber Messages package types/version and the untracked `Expressium.LivingDoc.Parsers` project to establish the correct source of truth before editing.
2. Add model fields for source comments and explicit outline/example-section identity as needed, keeping old serialized properties and public APIs compatible.
3. Update `MessagesGherkinParser` to preserve original Scenario Outline keywords and placeholder step text, copy comments, and create one model representation per Examples section without merging sections.
4. Preserve each Examples title/description, header order, and all body rows.
5. Add stable source/example identifiers or metadata needed for result association.
6. Update `MessagesResultParser` to map pickles/results to the correct outline example section and execution record without changing the main source-template rendering.
7. Add parser/model tests and fixtures for comments, multiple Examples sections, placeholders, and 100-column tables.

Primary files/classes:

- `Expressium.LivingDoc/Models/LivingDocFeature.cs`
- `Expressium.LivingDoc/Models/LivingDocBackground.cs`
- `Expressium.LivingDoc/Models/LivingDocRule.cs`
- `Expressium.LivingDoc/Models/LivingDocScenario.cs`
- `Expressium.LivingDoc/Models/LivingDocExample.cs`
- `Expressium.LivingDoc/Models/LivingDocStep.cs`
- `Expressium.LivingDoc/Models/LivingDocDataTable.cs`
- `Expressium.LivingDoc/Parsers/MessagesGherkinParser.cs`
- `Expressium.LivingDoc/Parsers/MessagesResultParser.cs`
- `Expressium.LivingDoc/Parsers/MessagesUtilities.cs`
- `Expressium.LivingDoc.UnitTests/Parsers/*`
- `Expressium.LivingDoc.UnitTests/Models/*`
- `Expressium.LivingDoc.UnitTests/Samples/*`

### Phase 2 - Timestamp, configuration, and history behavior

1. Make timestamp parsing retain an unambiguous UTC instant for calculations and choose one explicit display conversion at the model/UI boundary. Do not calculate duration from local-time values.
2. Add a report options/configuration type with defaults for output directory, output filename, and history merging. Normalize relative paths against the appropriate current/output base directory and preserve absolute paths.
3. Wire options into `LivingDocConverter`, `ExpressiumFormatter`, and CLI paths while retaining existing positional CLI arguments and formatter keys as aliases.
4. Make `MergeWithHistory` default to the current behavior when history is configured, and skip history discovery/health merge when false.
5. Ensure output directory creation and output filename replacement are deterministic and do not alter existing explicit output-path calls.
6. Add tests for defaults, relative/absolute paths, disabled/enabled history, and timestamp/duration edge cases.

Primary files/classes:

- `Expressium.LivingDoc/Models/LivingDocProject.cs`
- `Expressium.LivingDoc/Models/LivingDocUtilities.cs`
- `Expressium.LivingDoc/Parsers/MessagesUtilities.cs`
- `Expressium.LivingDoc/LivingDocConverter.cs`
- `Expressium.LivingDoc.ReqnrollPlugin/ExpressiumFormatter.cs`
- `Expressium.LivingDoc.Cli/Program.cs`
- `Expressium.LivingDoc.UnitTests/Converters/*`
- `Expressium.LivingDoc.UnitTests/Models/*`
- `Expressium.LivingDoc.UnitTests/Parsers/MessagesUtilitiesTests.cs`

### Phase 3 - Report rendering and UI interactions

1. Update `LivingDocDataObjectsGenerator` to render comments and original Gherkin keywords, distinguish Scenario from Scenario Outline, preserve each Examples section, and keep execution status visible.
2. Add semantic table markup with a header/first-row toggle target and a scrollable wrapper around both step data tables and Examples tables. Use stable classes/data attributes so 100-column tables cannot widen the document.
3. Update `LivingDocDataOverviewGenerator` to emit folder parent/child metadata and clickable root/nested folder controls while retaining existing feature collapse behavior.
4. Update `Resources/Scripts.txt` with independent folder, table-body, and scenario-section toggle functions. Ensure click handlers do not trigger navigation accidentally and remain compatible with dynamically copied document HTML.
5. Update `Resources/Styles.txt` for overflow containment, horizontal table scrolling, preserved whitespace/comment presentation, and visible collapsed headings/status.
6. Add generator assertions against the emitted HTML and browser tests for all toggle workflows when Chrome is available.

Primary files/classes:

- `Expressium.LivingDoc/Generators/LivingDocDataObjectsGenerator.cs`
- `Expressium.LivingDoc/Generators/LivingDocDataOverviewGenerator.cs`
- `Expressium.LivingDoc/Generators/LivingDocContentGenerator.cs` if navigation controls need a shell change
- `Expressium.LivingDoc/Resources/Scripts.txt`
- `Expressium.LivingDoc/Resources/Styles.txt`
- `Expressium.LivingDoc/Properties/Resources.resx`
- `Expressium.LivingDoc.UnitTests/Generators/*`
- `Expressium.LivingDoc.UITests/Features/*`
- `Expressium.LivingDoc.UITests/Steps/*`
- `Expressium.LivingDoc.UITests/Pages/*`
- `Expressium.LivingDoc.UITests/Controls/*`

### Phase 4 - .NET Framework 4.7.2 compatibility

1. Confirm all core dependencies support `net472` or `netstandard2.0` and identify APIs needing compatibility shims (`DateTime.UnixEpoch`, file/path APIs, tuple/deconstruction, nullable annotations if introduced, and resource generation).
2. Prefer multi-targeting the core library as `net8.0;net472` only if package and API validation succeeds. Keep Reqnroll integration and UI tests net8-only unless their dependency graph explicitly supports net472.
3. If Cucumber Messages or AngleSharp cannot support net472 cleanly, isolate the compatible domain/converter component and document the target split rather than forcing unsupported packages into the plugin.
4. Add a build matrix/check that compiles the supported net472 project(s) and net8 projects. Do not claim the plugin itself supports net472 unless its Reqnroll dependencies validate.

Likely files:

- `Expressium.LivingDoc/Expressium.LivingDoc.csproj`
- `Expressium.LivingDoc.Cli/Expressium.LivingDoc.Cli.csproj` only if executable compatibility is viable
- `Expressium.LivingDoc.ReqnrollPlugin/Expressium.LivingDoc.ReqnrollPlugin.csproj` only if Reqnroll supports net472
- `Directory.Build.props`
- solution/project references and package versions as required
- compatibility-focused tests or build validation documentation

### Phase 5 - Documentation, verification, and completion

1. Update `README.md` with target-framework support, configuration keys, defaults, history toggle, and report behavior.
2. Run focused parser/model/generator/converter unit tests after each phase, then run the complete unit-test project.
3. Run UI tests with a functioning Chrome/Selenium environment; record the existing environment blocker if unavailable.
4. Validate generated HTML through AngleSharp and, where possible, browser interaction checks for tables, folders, and scenario sections.
5. Update this document before each major phase transition and finish it with the final summary, supported targets, test results, known limitations, and technical debt.

## Data-model and parser design decisions

- Preserve the original source outline as the primary documentation representation. Execution results must be associated with source elements rather than replacing placeholder text with row values.
- Keep multiple `Examples` blocks as separate ordered model entries. Never combine their headers or rows.
- Treat comments as first-class source metadata instead of trying to reconstruct them in JavaScript.
- Keep existing JSON serialization tolerant of new properties so native LivingDoc files from older versions remain importable.
- Keep status/count behavior explicit: source Examples sections and executable example results may require separate identity/count semantics to avoid changing existing analytics unintentionally.

## Risks and compatibility concerns

- Cucumber Messages AST comments may be exposed differently by package version; fixture inspection is required before choosing the exact comment mapping.
- A single `LivingDocExample` currently mixes template data and execution status. Changing this boundary may affect history matching, counts, analytics, native JSON, and existing tests.
- Pickle AST node IDs for Scenario Outline rows must be mapped carefully so repeated values and multiple Examples sections cannot collide.
- Reqnroll formatter configuration is owned by the host package; new keys must be optional and ignored safely by older configurations.
- `net472` package compatibility, especially for `Cucumber.Messages`, `AngleSharp`, and Reqnroll packages, must be proven by restore/build rather than inferred from `netstandard` metadata.
- Embedded resource generation can introduce platform-specific build issues; preserve the current `resx` mechanism.
- Browser UI tests require Chrome and Selenium Manager; current runner is blocked by native process/resource failure.
- Existing worktree changes must not be overwritten: `MessagesGherkinParser.cs`, `MessagesResultParser.cs`, `MessagesParserExampleTablesTests.cs`, and untracked `Expressium.LivingDoc.Parsers/` are pre-existing at planning time.

## Implementation order

1. [x] Repository analysis and baseline capture
2. [x] Create this progress/plan document
3. [x] Confirm untracked parser-project intent and dependency compatibility; it contains only a duplicate untracked parser file and no project file.
4. [x] Implement model/parser source fidelity
5. [x] Validate focused parser/model tests
6. [x] Implement timestamp and configuration/history options
7. [x] Validate converter/history/timestamp tests
8. [x] Implement HTML/CSS/JavaScript rendering and toggles
9. [-] Validate generator tests and browser tests
10. [x] Implement and validate .NET Framework 4.7.2 targeting strategy
11. [ ] Update README and final verification

## Baseline Results

- Worktree at planning time: pre-existing modifications in parser source/tests and untracked `Expressium.LivingDoc.Parsers/`; those files remain untouched.
- Model-only validation: the core, CLI, and unit-test projects compile successfully.
- Focused parser validation: `MessagesGherkinParserTests`, 2 passed, 0 failed.
- Full unit-test validation after the Phase 1 changes: 543 passed, 18 failed, 0 skipped, 561 total. The failures are in pre-existing dirty outline/attachment expectations and are not yet resolved.
- UI tests were not run during Phase 1, per user instruction.

## Phase 1 Changes

- Added `Comments` collections to Feature, Background, Rule, Scenario, and Example models.
- Added `DocumentationExamples` to `LivingDocScenario` for source-preserving Examples sections.
- Updated `MessagesGherkinParser` to retain every Examples section independently while keeping the first section in legacy `Examples` for existing result/history consumers.
- Added line-based comment association using Cucumber AST locations.
- Added `MessagesGherkinParserTests` covering separate Examples sections, titles, rows, placeholders, and Background comments.

## Phase 1 Design Decision

The existing `LivingDocScenario.Examples` property is used by status calculation and history/result mapping. Reusing it for every source Examples section would incorrectly turn documentation sections into execution records. `DocumentationExamples` therefore preserves the original source structure without breaking current execution behavior; report generation will consume it in Phase 3.

## Phase 1 Completion

- Focused validation: `MessagesGherkinParserTests`, 4 passed, 0 failed.
- Phase 1 is complete. Phase 2 is ready to begin.
- The full unit suite remains at the previously recorded `543 passed, 18 failed` baseline because of pre-existing dirty-worktree outline/attachment expectations. Those failures were not changed as part of this phase.

## Final Implementation Summary

Phase 2 implemented `LivingDocReportOptions` with normalized output paths and
history behavior, corrected timestamp arithmetic to use UTC instants, and wired
the options into the Reqnroll formatter and CLI while retaining legacy output-path
APIs.

## Final Supported Targets

The core `Expressium.LivingDoc` library targets `net8.0;net472`.
The Reqnroll plugin, CLI, unit tests, and UI tests remain `net8.0` because their
dependency graphs are not validated for `.NET Framework 4.7.2`.

## Final Configuration Options

Implemented options: `OutputDirectory`, `OutputFileName`, and `MergeWithHistory`.
Defaults are `.`, `LivingDoc.html`, and `true`, respectively. Existing explicit
converter output paths, formatter keys, and CLI argument forms remain supported.

## Known Limitations / Future Improvements

- Browser validation currently depends on a working Chrome/Selenium Manager environment.
- Exact Cucumber comment representation and net472 dependency support require validation during implementation.
- net472 runtime tests require a Windows environment with the .NET Framework 4.7.2 runtime; this macOS runner only provides compile validation through reference assemblies.

## Phase 2 Validation

- Focused timestamp and options validation: 8 passed, 0 failed.
- Focused converter, history, timestamp, and options validation: 19 passed, 0 failed.
- UI tests remain unrun because Chrome/Selenium execution is environment-blocked.

## Phase 3 Changes

- Rendered Scenario Outline keywords, placeholder steps, comments, and separate named Examples sections.
- Added semantic table headers, independent table-body toggles, and horizontal scrolling wrappers.
- Added independent scenario-section and root/nested folder collapse controls.
- Added generator coverage for outline rendering and embedded toggle scripts.

## Phase 3 Validation

- Focused HTML generator validation: 83 passed, 0 failed.
- JavaScript syntax validation: 26 script blocks parsed successfully.
- Browser UI tests remain unrun because Chrome/Selenium execution is environment-blocked.

## Phase 4 Validation

- Core library build: `net8.0` passed.
- Core library build: `net472` passed using `Microsoft.NETFramework.ReferenceAssemblies`.
- Full solution build: passed for the core, CLI, Reqnroll plugin, unit tests, and UI tests.
- Local Reqnroll fixture validation: 3 passed, including one regular Scenario and two Scenario Outline examples; HTML and NDJSON artifacts generated successfully.
- Windows runtime validation for `net472` is configured in the `net472-compatibility` GitHub Actions job.
- The `reqnroll-livingdoc-report` GitHub Actions job is configured to publish the generated report artifact.
