# Development and recovery

## Current working state

The user selected **Building Planner** as the display name and English HUD title on 2026-10-16. Stable technical identity remains CraftingPlanner (GUID, namespace, installed filename, packages and repository URL). The 0.1.11 return reports correct ordinary-HUD rendering across all 11 languages and working core actions, but requests finer menu/chest/footer calibration. No stable acceptance is inferred.

Current installed candidate: **0.1.12**, exact runtime source `aa49988daf9eaf28a26ada824ae16c8a5f8b5aea`, successful CI `38885542445`, artifact `12039238521`, DLL SHA256 `40e639d717f599e945518fe7ccc08cf61f24b2358b246fdc070fbac9b9bbac4d`. Exact CI bytes are under ignored `artifacts/CraftingPlanner-0.1.12/`; previous payload, log and installation receipt are preserved privately under `.local/`. Both native-reference configurations, clean hosted stub build, state/geometry/font/calibration/localization checks, exact-DLL policy tests and package boundary pass. The installed candidate is not runtime/visually accepted.

F2 now has 51 field-identified sliders with individual baseline resets and labels above controls, plus a global reset. Menu column/each arrow/quantity and ingredient-group offsets are independent; menu fonts and effects are separate. Native Advanced-sprite right border can be hidden/restored without resizing the row. The owned Plan caption joins the native title UIPanel (depth35) outside measuring tables. HUD RS/B has a calibration preview and independent icon/text sizing, position and effect controls. Title text has its own vertical offset. Chest Take Needed and feedback move independently; native A/X/B move together by reversible anchor offsets. A cached native tip-line preview assists mouse-operated F2 if keyboard switching hides gamepad hints. These previews do not invoke input actions.

Start values preserve the returned 0.1.11 settings (icon46, padding12, backing35%, alternate quantity font, footer craft font20, feedback X11/Y-5). Format=5 export includes every new parameter. Sizes/offsets remain integer logical UI units, scale uses 0.01 steps and opacity whole percent; no per-frame resolution compensation is added. Settings remain temporary until exit; the newest returned log is the next baseline source.

Next game checkpoint: tune construction before/after quantity edits and scrolling, restore individual controls, check native border/Plan-caption visibility and ordinary build click; open F2 from gamepad HUD and verify visible RS/B preview and separate icon size; tune Take Needed/native A-X-B line/result under the chest and check keyboard/gamepad actions plus reopen restoration. Verify the longer English Building Planner header fits and retain bounded all-language evidence separately from new contexts.

Per-save persistence and automatic successful-placement decrement remain absent; the latter is required follow-up under its own success-boundary gate. Preserve immutable prior artifacts and bounded returns in TEST_LOG.md. No stable/main promotion has occurred.

Earlier accepted HUD/font evidence is preserved in the exact 0.1.4 artifact recorded in `TEST_LOG.md`, built from `ad836eae5556858feb09f9cbf4d8eefead78931f`, SHA-256 `b8ac438d19399963d51b9d7c1fd8fe0349d229566c70f80a1e80764d72f5f2d9`. Preserve that historical evidence and immutable identity separately from 0.1.5.

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
