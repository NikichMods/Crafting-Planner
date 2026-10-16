# Development and recovery

## Current working state

The accepted browser mockup drives the native UI integration; the 0.1.9 return confirms menu quantity changes, storage transfer and RS character-input containment but rejects presentation and HUD mouse/focus behavior. Corrections are installed on `dev/0.1.10`; see PRODUCT_SCOPE.md for adopted design and UI_IMPLEMENTATION.md for native readiness gates. Stop speculative browser iterations; use the candidate's F2 calibration for native visual tuning.

Current installed candidate: **0.1.10**, exact runtime source `2af839b1a495007f46b7c8f889f98b6d3008a82c`, successful CI `38830954723`, artifact `12016149655`, DLL SHA256 `ad6e0c402775407fae1a9d3af882126905d0e473b69bd6f648e092771819767c`. Exact CI bytes are under ignored `artifacts/CraftingPlanner-0.1.10/`; previous payload, log and installation receipt are preserved privately under `.local/`. Native-reference candidate/production compilation, hosted clean stub build, geometry/localization/state tests, package checks and exact-DLL policy checks pass. Native visual/input acceptance remains open.

Changes: plain centred planner title; native craft/HUD font profiles and shadow/outline comparison; visible glyph-owned mouse controls independent of startup camera readiness; explicit controller selection band; shared quantity/Missing centre; Plan column without fills and measured ingredient alignment; native title-row Plan caption; plain chest Y/F action with actual-result feedback below its action row; visibility on the character Inventory tab only. Other character tabs/modal windows hide the planner. F2 exposes size/scale, grouped block/column spacing, density and feather; closing/export writes format=3 settings and geometry. Settings remain temporary until exit.

Per-save persistence and automatic successful-placement decrement remain absent; the latter is required follow-up with a separate success-boundary gate. Do not claim those mechanics were delivered with this UI candidate. Preserve immutable prior numbered artifacts and their bounded returned evidence in TEST_LOG.md. No stable/main promotion has occurred.

The latest prior candidate with accepted in-game HUD/font evidence is the exact 0.1.4 artifact recorded in `TEST_LOG.md`, built from `ad836eae5556858feb09f9cbf4d8eefead78931f`, SHA-256 `b8ac438d19399963d51b9d7c1fd8fe0349d229566c70f80a1e80764d72f5f2d9`. Preserve that historical evidence and immutable identity separately from 0.1.5.

Accepted evidence: supported builder/world-change requirement data; exercised goal quantities/aggregation; current-storage native transfer; CraftGUI visibility; ordinary HUD parent/lifecycle; live localization. Acceptance is bounded to the cases in `TEST_LOG.md`.

## Resume order

Read the canonical DevRules `WORKING_RULES.md` and `CODEX_WORKFLOW.md`, this repository's `AGENTS.md`, `PRODUCT_SCOPE.md`, `VERIFIED_GAME_DATA.md`, and the latest `TEST_LOG.md` entries. The dated initial research report is historical: later accepted evidence supersedes its open questions. Consult `NikichMods/GraveyardKeeperResearch` before new host research.

Inspect actual Git status, local commits and current remote state before editing. Keep one writer per checkout. Keep machine installation paths, logs and proprietary game inputs in ignored `.local/`, outside public Git.

## Local compilation

The project targets .NET Framework 4.7.2 and C# 7.3. The existing hosted toolchain uses .NET SDK 8; reference assemblies are restored from NuGet rather than supplied by the installed game.

From the repository root:

```powershell
dotnet restore CraftingPlanner.csproj --configfile nuget.config
dotnet build CraftingPlanner.csproj -c Release --no-restore --nologo
```

Verification output is `bin/Release/CraftingPlanner.dll`. Confirm it exists and that neither `Assembly-CSharp.dll` nor `Assembly-CSharp-firstpass.dll` appears in that output directory. These are signature-only compile references, never distributable game assemblies.

The development workflow now verifies exact source/version identity, compiles, tests project-owned gradient/localization policy, checks reference exclusion and packages a numbered candidate with a hash manifest. `Plugin.PluginVersion` owns the semantic version and derives both assembly/file versions. Candidate 0.1.5 must remain distinct from the earlier immutable 0.1.4 artifact. No runtime or visual acceptance follows from compilation alone. For the temporary 0.1.6 calibration candidate add `-p:PlannerCalibration=true`; without this property the F1 editor and its input guards compile out. Hosted candidate CI explicitly enables and records this flag.

## Current implementation / next acceptance checkpoint

The user selected aligned rows and authorized the UI implementation:

- one compact visual specification and apparent screen position across CraftGUI and ordinary HUD;
- native item icons and clear compact Required / Have / Missing counts;
- hierarchy, backing, placement and content overflow;
- retain display-only P0 with direct focused-card quantity management.

Use native renderer/anchor/font paths documented in `UI_IMPLEMENTATION.md`. All rows are visible for the first candidate (explicit user choice); actual long-plan height will inform a later overflow decision. Verify the candidate in-game for appearance, menu/HUD continuity and gamepad hints, starting with Russian. Preserve all 11 locales and defer final long-label/CJK visual/translation review until layout stabilizes. Per-save persistence is explicitly desired later, not implemented here.

Do not repeat the accepted HUD-anchor experiment or add Borderless-specific compensation. Final input/help presentation and any remaining carried-bag/capacity edge evidence should follow the chosen UI, rather than silently expand scope. Keyboard management is not accepted merely because an early candidate proposed it.

The user confirmed on 2026-10-15 that this checkout was added as the local app project and this chat moved into it. The active workspace is now the repository root. Web-only decisions or assets absent from Git remain unknown and must be supplied if they change this recovered baseline.

## Migration verification — earlier setup milestone, 2026-10-15

- Integration commit: `7334d4e1acf19ac2a6d85b805db600b237f4bf44`; both original branch histories are preserved.
- Runtime source, signature references, project file and NuGet configuration match `dev/0.1.4` exactly (`git diff --quiet` verified).
- Whitespace validation passed; generated outputs are ignored.
- Local SDK available: 6.0.406. Restore of the reference projects succeeded, but the main restore timed out downloading BepInEx.BaseLib/Core 5.4.21 from the configured BepInEx feed and was stopped. Local compilation is not verified; no installable artifact was handed out.
- Hosted SDK 8 verification was launched for the exact integration commit: https://github.com/NikichMods/Crafting-Planner/actions/runs/38782751823 . Completed successfully on 2026-10-15 at 18:06 UTC: dependency restore, Release compilation (0 compiler warnings / 0 errors), and compile-reference package-boundary checks passed. No numbered artifact was uploaded. The runner emitted action-runtime deprecation warnings, separate from compiler diagnostics.
- Local app-project setup is complete (user-confirmed). Hosted compilation verification is closed; local dependency restoration remains unverified. The next product step is the agreed UI design discussion, not a new runtime-acceptance claim.
