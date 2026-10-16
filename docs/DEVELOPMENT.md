# Development and recovery

## Current working state

The user selected **Building Planner** as the display name and English HUD title on 2026-10-16. Stable technical identity remains CraftingPlanner (GUID, namespace, installed filename, packages and repository URL). The 0.1.11 return reports correct ordinary-HUD rendering across all 11 languages and working core actions, but requests finer menu/chest/footer calibration. No stable acceptance is inferred.

Current installed candidate: **0.1.13**, exact runtime source `3416079a59cdf73022d100ed63e6c07ca3352f50`, successful CI `38896495222`, artifact `12044480696`, DLL SHA256 `95d7f7b3a098cb895334297b3c448803f16dede5b488fc63ef5bbdcd5f609edc`. Exact CI bytes are under ignored `artifacts/CraftingPlanner-0.1.13/`; previous payload, log and installation receipt are preserved privately under `.local/`. Both native-reference configurations, clean hosted stub build, state/geometry/font/calibration/localization checks, exact-DLL policy tests and package boundary pass. The installed candidate is not runtime/visually accepted.

It replaces independent chest native/Y shifts with one measured right-aligned row A/X/B/Y (or keyboard F), preserving native omissions/disabled tips and input. Native text alpha is suppressed only while owned chest controls are shown, and restored on hide/modal/unload. F2 shows the last gamepad row while it owns input, and has common X/Y and equal-gap controls. Transfer result has its own anchor and X/Y/spacing controls, independent of row shifts.

Supported project menus use localized Add to planner above the measured ingredient area, at the native title Y, copying native font/color/effect. Caption X/Y and size16 baseline have individual reset controls. Other menu, quantity, ingredient and footer controls remain. F2 labels appear above all sliders; each reset uses the current candidate baseline. Export format6 includes the caption and common chest-row parameters.

Latest 0.1.12 return defaults are adopted: right9/icon50/backing39%/goalsOffset-7/materialsGap12; menuX17/Y7/minusX13/plusX-13/ingredientsX29/HUDfont16/native border retained; footerX-22/Y3/icon16; feedbackX264/Y-5/gap0/lineGap4. Unchanged values are in PlannerStyle/PlannerTuning. Settings remain temporary until exit; the returned log is preserved privately. Logical offsets/sizes remain whole units; scale uses .01 steps and opacity whole percentages.

Next game checkpoint: open a chest with gamepad; compare English/Russian row right edge and equal gaps; switch player/chest focus (Put/Take), one-item/empty slots (native omitted or disabled X), keyboard F, F2 preview, modal/reopen/restoration and transfer feedback. Open supported build/repair menus and verify Add to planner above ingredients, clear of close X, with native title styling; tune caption X/Y/size and test individual reset. Native build clicks, quantity input and scrolling must retain their existing behavior.

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
