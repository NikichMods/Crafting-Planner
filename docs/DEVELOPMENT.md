# Development and recovery

## Current working state

The user selected **Building Planner** as the display name and English HUD title on 2026-10-16. Stable technical identity remains CraftingPlanner (GUID, namespace, installed filename, packages and repository URL). The 0.1.11 return reports correct ordinary-HUD rendering across all 11 languages and working core actions, but requests finer menu/chest/footer calibration. No stable acceptance is inferred.

Current installed candidate: **0.1.16**, exact runtime source `a8bda9eeb0f4829ff5eaf2fbec0dfbbaa213c25b`, successful CI `38977362668`, artifact `12079323553`, DLL SHA256 `74f612880574d90a53d771a925f8cbc56bbb69f3e5b1acad8ffa7de189d2d37f`. Exact CI bytes are under ignored `artifacts/CraftingPlanner-0.1.16/`; previous payload, log and installation receipt are preserved privately under `.local/`. Both native-reference configurations, clean hosted build, state/geometry/font/calibration/localization/invalidation/energy-layout checks, exact-DLL tests and package boundary pass. User explicitly accepts the entire current visual presentation on2026-10-17, including the0.1.16 energy correction. This closes visual iteration; it is not full functional/release acceptance or permission to promote runtime to main.

The accepted A/X/B/Y row is retained. Transfer results now use the same right edge, measured number/icon groups and independent right-edge-relative X/Y; each wrapped line is right-aligned. Fixed48-unit cells are removed; gap supports -8..40, arbitrary digit counts remain measured. The old feedbackX234 origin is superseded by right-edge-relative default0, with Y-5 retained.

Candidate0.1.15: supported-menu planner help joins native action hints in ONE right-aligned row. Planner help is leftmost; original native blocks follow in their current order, with final Close at the frame right edge. Gaps are measured after localized rendering; keyboard shows only planner help. A separate owner observes only CraftGUI's ButtonTipsStr, suppresses original label alpha reversibly and restores it for unsupported menus/modal/close/unload. The former per-row second help line is removed. F2 MenuCaption X/Y now move the WHOLE row, MenuCaptionFont sizes the planner caption, and MenuActionGap controls common gaps, all with individual reset. Format8 reflects these semantics; other tuned values remain unchanged.

The native tip's base color is white but its UILabel gradient is cream-to-brown. The new build-only owner copies applyGradient/gradientTop/gradientBottom in addition to the native font/effect. It reuses atlas-backed dark round gp_SW_arrow_L/R sprites at18x18 (alpha90%), with verified ASCII fallback if a language font lacks them. No custom artwork or gradient changes to accepted chest/HUD presentation.

Quick Stack1.0.0 uses native MultiInventory.MoveItemTo, whose removal bypasses the observed Item.RemoveItem overload. A scoped successful-transfer observer checks native inventory owners for player.data, marks dirty and refreshes the visible planner once in LateUpdate after the batch. No Quick Stack private hooks/dependency or recurring inventory scans; native world-Y and open-chest-Y handlers retain ownership. Existing Item Add/Remove observers remain. The diagnostic event inventory_refresh trigger=native_transfer identifies that this final-commit path ran.

0.1.15 user return accepts the combined construction help row and native round arrows. New repair screenshot exposes an energy/plan-quantity overlap. Candidate0.1.16 keeps those accepted tips and separates only visible single-line energy rows: native txt_spend bottom anchor moves up by twice a bounded shift (centre up to8), owned quantity/minus/plus move down by the matching shift. Native text/font/X/top edge and row/table height stay owned by the host. Measured printed height plus2 bounds the shift. Captured native anchor is restored before redraw/re-measurement and on hide/ineligible/unload, preventing cumulative movement. Empty, non-energy and multiline spend labels keep native geometry. No new calibration fields or baseline changes.

## Next milestone after visual acceptance

The user accepts the visual/UI design as a whole on2026-10-17. Do not continue decorative iteration without a new defect or request. The active installed0.1.16 remains in use; the game is running and no new binary/install is requested. Its live log snapshot is preserved privately; loading/version/hash match and no planner exception/disable appears in that snapshot.

The core manual loop is implemented and exercised: multiple project quantities, aggregate carried-player Required/Have/Missing, current-chest Take Needed/results, keyboard/mouse/gamepad management and Quick Stack refresh. Visual acceptance does not imply exhaustive capacity, uncommon host-flow or all-language/context coverage.

The next required functional milestone is automatic plan reduction after actual success. User-selected builder semantics: successful placement commits materials and removes exactly one planned instance; failed/cancelled placement removes none. Repairs/upgrades/clearing need their own verified success boundary and identity. Static DoPlace evidence alone is insufficient because of early returns and ignored removal results. Start by closing those success/identity gates with existing shared/native evidence, then implement narrowly and verify success/cancel/repeat/exactly-once behavior. No implementation or new artifact has been started by the milestone discussion.

After that, perform a focused functional/release pass and prepare the first normal distributable build. Existing calibration tooling/final packaging should be reviewed at that boundary, without treating removal of F2 or a new settings system as an adopted product choice. Per-save goal persistence remains explicitly deferred; current goals are session-local. Main/stable promotion and release require their own explicit acceptance.

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
