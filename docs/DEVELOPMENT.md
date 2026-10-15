# Development and recovery

## Current working state

The user accepted the completed interactive UI mockup for implementation on 2026-10-16. PRODUCT_SCOPE.md owns the final compact plain-card appearance, backing defaults, fixed quantity slots, per-row Plan controls and directional gamepad flow. Stop further speculative mockup iteration. Implementation is on `dev/0.1.9`: shared compact renderer, generated rounded backing, native title/project icons, HUD mouse arrows, Rewired-template RS entry with native player-control containment, guarded project-menu left/right, separate per-row Plan column, and chest-row mouse/Y/keyboard-Work action with actual transfer feedback. Gates are in UI_IMPLEMENTATION.md. Native-reference and stub-reference compilation plus policy/state tests and hosted CI pass. Exact CI bytes are installed with the game closed; installed-game acceptance is pending.

Per-save persistence and automatic successful-placement decrement remain absent; the latter is required follow-up with a separate success-boundary gate. Do not claim those mechanics were delivered with this UI candidate. Local calibration exports use format=2 with shadow radius/feather replacing the old directional fade field.

Continue on `dev/0.1.9`, based on the synchronized 0.1.7 UI integration. The migration preserved both `dev/0.1.4` (`877cb0b`) and the newer documentation/licensing baseline `main` (`f2921a8`). The 0.1.5/0.1.6 runtime returns rejected their presentation; the accepted browser mockup now drives the native UI integration. Visual/runtime acceptance remains pending. See `UI_IMPLEMENTATION.md` and the latest `TEST_LOG.md` entry.

Current installed candidate: **0.1.9**, exact source `932a7ae0897d48e20a54ed977c0cfae91d4d5d6d`, successful CI `38826114338`, artifact `12015505450`, DLL SHA-256 `5ca022e46edae854e6841594a189a407fbcbf6aab2001275a573958de2a4f590`. Temporary F1 calibration remains enabled. Exact CI bytes/manifest are under ignored `artifacts/CraftingPlanner-0.1.9/`; private receipts, logs and previous payloads are under `.local/`. Loader, native geometry, all new input paths and chest action-row placement await runtime acceptance. Preserve immutable 0.1.6 and the unaccepted 0.1.7/0.1.8 intermediate installations separately; TEST_LOG.md records their identities and the bitmap-glyph correction that required 0.1.8.

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
