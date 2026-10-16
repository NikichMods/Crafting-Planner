# Test Log

## 0.1.9 installed UI candidate — 2026-10-16

- Current immutable test candidate: source `932a7ae0897d48e20a54ed977c0cfae91d4d5d6d`, branch `dev/0.1.9`, successful CI [38826114338](https://github.com/NikichMods/Crafting-Planner/actions/runs/38826114338), artifact `12015505450`. DLL `CraftingPlanner-0.1.9.dll`, version `0.1.9.0`, SHA-256 `5ca022e46edae854e6841594a189a407fbcbf6aab2001275a573958de2a4f590`; downloaded ZIP independently matches `b5b0687c512d14825a259203d0a4031138b7f1766e17ae2c17996f57b6079576`.
- Contains the full 0.1.8 native UI integration described below, plus normalization of the stored HUD selection index before navigation after a row is removed. The regression test covers removing the selected last row and then wrapping Down to the first surviving row. Build, identity, shape/selection/state/localization tests and reference-exclusion checks pass in clean CI. Current source compiles against installed native references; the exact CI artifact passes pure policy/localization checks with installed dependencies.
- Installed exact CI bytes with the game closed; digest matches the manifest. Previous 0.1.8 and its log are preserved outside plugins; receipt `.local/evidence/pre-0.1.9/installation.json`. 0.1.7 and 0.1.8 were intermediate unaccepted installations, not overwritten numbered artifacts. No new game session, loader result or user visual/input acceptance is claimed.
- Next is the same focused runtime pass listed under 0.1.8: native menu Plan controls vs main-row build action, HUD mouse/device switching/RS/directions/B, post-exit ordinary controls, chest Y/F/mouse/actual feedback, shared geometry and unrelated-window hiding. F1 format=2 export remains available. Automatic placement decrement, per-save persistence and a separate inventory management view remain absent; follow the scope distinctions below.

## 0.1.8 native UI integration candidate — 2026-10-16

- Exact source `9a394f917011ebcac99463b349da35bbb62291dc`, branch `dev/0.1.8`; CI [38825543383](https://github.com/NikichMods/Crafting-Planner/actions/runs/38825543383), artifact `12014159309`. All build, identity, policy/state/localization and reference-exclusion checks passed. Numbered DLL `CraftingPlanner-0.1.8.dll`, assembly/file version `0.1.8.0`, SHA-256 `3fd0b6a8e2867e764891e599cee98958e0683e64c3aeecca965695e30c2d4f2f`. CI ZIP SHA-256 `0cdc0e6d58a3edddbb2025708751da2f8e55b369f9bbf13992aa5bb196b1d8d1` independently matched before extraction.
- Installed exact CI bytes at the canonical CraftingPlanner.dll with game closed; installed digest independently matches the manifest. Prior payload/log are preserved outside the plugin directory in ignored evidence/backups; installation receipt is `.local/evidence/pre-0.1.8/installation.json`. No loader/runtime/visual acceptance is yet recorded. The candidate has the temporary F1 calibration build flag; exports are format=2 with shadow radius/feather.
- Integration: one compact shared card, native title/project/material art, ordinal item-ID rows, Required/Have/Missing centering and adopted red/green; feathered rounded-rectangle backing defaults to 40%. Fixed quantity slots and direct mouse HUD arrows; separate Plan column in supported native project rows; supported-focus left/right handlers preserve ordinary crafting and active native counters. Native Rewired gamepad-template RS enters HUD editing, up/down chooses, left/right adjusts, B exits; native gameplay eligibility and the exit frame prevent world input ownership from leaking. No hardware button index or global Y hook was added.
- Storage action moved out of the informational card beside the native chest action row. Native Y, mouse and keyboard Work (default F) share the existing current-storage transfer. Actual before/after carried counts drive moved-icon feedback; capacity/no-stock/other failure reasons are separate. No transfer forecast or remote storage logic.
- Verification: both calibration and production source compile against installed 1.407 references. Headless stub-reference tests exercise the same goal-state helper for editing outside the original context, exact aggregation, zero removal, stale callback rejection and one-off bounds; rounded backing symmetry/bounds/top-row coverage, fixed quantity offset and all 11 language schemas pass. The exact CI artifact's pure policy/localization tests also pass against installed dependencies. None of these tests emulate native layout, physical input or capacity edges.
- Required game pass: compare card position/scale in HUD/build/world-project/chest; use row Plan arrows without triggering build, then use main row with materials to retain native placement; mouse HUD +/- and mouse/gamepad switch keep quantity positions; RS/up/down/left/right/B and ordinary movement/work after exit; Y/F/mouse chest action, actual feedback and partial/no-space/no-stock cases; Quick Stack outside the chest, unrelated-menu hiding, scrolling and live language change. F1 export is available if geometry differs. Read the new log directly after user testing.
- Automatic decrement on successful placement remains required follow-up and is absent here; per-save persistence remains deferred. A separate inventory management view is not implemented: this candidate uses the adopted direct HUD route. Do not claim stable or whole-product completion.

### Preserved 0.1.7 intermediate installation

0.1.7 source `1e4ffd0412dc7f6e2ba996e67aba0392ca7e1291`, successful CI `38824827828`, artifact `12014198199`, DLL SHA-256 `e8bee2915215eb14367998e98f8fa85f14c083d952dada70c2a0b8eaf8a38319` was installed with the game closed, without a returned runtime session. A final static bitmap-font check found its Unicode chevrons/directional characters absent from the verified native font. Rather than replace installed numbered bytes, 0.1.8 changes counters to verified ASCII `<`/`>` and uses native GameKeyTip direction symbols. The 0.1.7 artifact/receipt remain preserved and unaccepted.

## Crafting Planner Research Probe 0.1.0 — runtime evidence received

- Purpose: verify the public craft/build focus seams, player-only versus interaction/world-zone inventory counts, bounded native chest-to-player transfer, and the open-chest Option2 gamepad context.
- Source branch: `research/p0-native-seams`
- Exact source commit: `f94be35330d17f3dccdfbe927d4f847fb5905498`
- CI run: `35652997475`
- GitHub Actions artifact: `CraftingPlannerResearchProbe-0.1.0` (artifact ID `10662547953`)
- DLL: `Crafting Planner Research Probe 0.1.0.dll`
- DLL SHA-256: `019eedc85979eb3a5f09d63d5b4a6a6feeb99251af6e837ce215c74918abc90f`
- Artifact ZIP SHA-256: `4c09f209162c8d042204b7f4323a7007207697037aded7e765a41e0562ca7f6f`
- Build status: restore, compile, package-boundary verification, and artifact upload all passed.
- Runtime status: executed successfully by the user on Graveyard Keeper 1.407 with the normal 35-plugin setup.
- Returned `LogOutput.log` SHA-256: `2154067363657c21b4ce05bb1072420aff049732d33aa27463f7642ff81315e6`.
- No Crafting Planner probe exception/error was logged.
- Save safety: the probe creates no custom persistent planner state. Its transfer action moves real existing items from the currently open chest to the player through the native inventory path; ordinary inventory changes can therefore be saved if the game is saved afterward.

### Deterministic runtime check

1. Install the exact probe DLL as an additional BepInEx plugin.
2. Open an ordinary crafting recipe with simple fixed ingredients and hover/focus it once.
3. Close the crafting UI and open a chest that contains at least one needed ingredient.
4. Keyboard: press `F8` once. Gamepad: press native `Option2 / Y` once.
5. If convenient, repeat the focus step for an ordinary building recipe and open a relevant chest.
6. Return `BepInEx/LogOutput.log`.

Expected diagnostic prefix: `CRAFTING_PLANNER_PROBE`.

Research harnesses are not production releases.


### Runtime result — 2026-09-22

Accepted findings from the returned log:

- The craft-focus seam fired for five ordinary fixed-ingredient recipes, proving that the candidate public focus callback is live.
- Player-only Have and crafting-access inventory are materially different in runtime. Example: `flitch` player Have 0 vs interaction count 18.
- Current chest identity was captured correctly through open/close lifecycle.
- F8 research trigger: `Required=1, Have=0, Missing=1, storage=9` moved exactly one `flitch`; player 0 -> 1 and storage 9 -> 8.
- Immediately after that move, Y/Option2 saw `Missing=0` and requested 0, so there was no surplus transfer.
- Y/Option2 separately performed successful native transfers when `Missing=1`.
- With a chest containing 0 matching items, both F8 and Y/Option2 requested 0 and left player/storage counts unchanged.
- Open-chest Y/Option2 worked while Quick Stack 1.0.0, Recipe Pin 0.3.0, Queue Everything 2.2.1, and the rest of the user's normal mod stack were loaded.

Still open:
- no `kind=build` event was present, so build-menu capture is not runtime-accepted yet;
- world repair/upgrade/clearing project semantics were not exercised;
- carried-bag behavior was not isolated;
- a capacity-limited destination was not exercised.

Product consequence: ordinary workstation recipe capture is technically viable but no longer a P0 product goal. The next runtime evidence should focus on build/world-project seams rather than more normal crafting recipes.


## Crafting Planner Research Probe 0.2.0 — awaiting runtime evidence

- Purpose: verify the actual construction/build-menu context and identify the host-owned paths used by representative one-off world projects such as repairs, upgrades, and clearing actions.
- Research branch: `research/build-world-projects`.
- Exact source commit: `f11c6b3c9e20443431aaf3eb25339c78e5e711e6`.
- CI run: `35658461574`.
- GitHub Actions artifact: `CraftingPlannerResearchProbe-0.2.0` (artifact ID `10664933815`).
- DLL: `Crafting Planner Research Probe 0.2.0.dll`.
- DLL SHA-256: `4225666d7b67c8af0e49dab037a18505459c4ca59cb9523815c790274ca405e4`.
- Artifact ZIP SHA-256: `2e70c333896da19a0f70add0caa624865371c48fe31374bb5e95f82beda3dde5`.
- Build status: restore, compile, package-boundary verification, and artifact upload passed with 0 warnings / 0 errors.
- Runtime status: executed successfully by the user on Graveyard Keeper 1.407 with the normal 35-plugin setup.
- Returned `LogOutput(1).log` SHA-256: `3bf9cb3782b623e40c38ef70189f21de751585d2ea568c0b09ec3c8db9b29fce`.
- No Crafting Planner Research Probe warning/error/exception was logged.
- Save safety: the probe itself is observational. It does not transfer inventory, add planner state, start projects on its own, or mutate world state. Ordinary vanilla actions the user chooses to complete can still change the save normally.

### Probe 0.2.0 runtime check

1. Remove Probe 0.1.0 if it is still installed; install Probe 0.2.0.
2. Open an ordinary construction/building desk and move the mouse across several build cards. If convenient, focus several cards with gamepad too. No construction needs to be completed.
3. Interact normally with at least two still-available one-off world projects if convenient: ideally one repair/upgrade and one blocked-passage/rubble/clearing action.
4. Do not finish an irreversible project solely for the probe. If a test action does change the world, avoid saving unless that gameplay change is wanted.
5. Return one `BepInEx/LogOutput.log`.

Expected diagnostic prefix: `CRAFTING_PLANNER_PROBE`.

The probe records build-menu definitions/focus plus world-object interaction, script/craft ownership, target craft lists, `TryStartCraft`, and relevant `CraftComponent.Craft` starts. It does not manufacture the project result being investigated.


### Runtime result — 2026-09-22

Accepted findings:

- Build context is runtime-distinguishable and uses exact `ObjectCraftDefinition` entries in `CraftGUI`.
- Mouse focus is live for build cards. The probe captured real build definitions, including their exact `needs`.
- The graveyard builder exposed `flowerbed_2x2` with `stone_plate_1:2, peat:1, flw_poppy:2`.
- The user selected a grave placement; the probe captured `build_selected`, followed by vanilla `CraftBuilding(... type=Put)` and build-mode entry.
- The home wood builder exposed both:
  - repeatable `mf_box_stuff_place -> flitch:4, nails:4, detail_1:4`;
  - one-time `upgrade_player_buildzone -> detail_2:10, wooden_plank:8, nails:4`, `one_time=true`, `BuildType.None`, `end_script=player_buildzone_upgrade`.
- Repairs of the mortuary build desk and corpse chute endpoints used ordinary `CraftDefinition` entries with concrete `needs` and `change_wgo`.
- `blockage_V_low` used an ordinary `CraftDefinition` with `spike_1:10, wood_balk_1:1, detail_1:4` and `change_wgo=0`.
- Repair/clearing screens used the normal `CraftGUI`, but their definitions are world-change projects rather than ordinary production goals.

Acceptance consequence:

The P0 project-material model is now runtime-accepted for:
1. native builder `ObjectCraftDefinition` projects with plain `needs` (repeatable placement plus the observed one-time builder upgrade);
2. interacted-world-object `CraftDefinition` projects with plain `needs` + `change_wgo` (observed repairs and blocked-passage clearing).

No additional repair/clearing probe pass is required before production work on the planner data model.

Still open:
- gamepad build-card focus/input was not exercised in this log;
- final production pin/manage gesture and UI lifecycle;
- carried-bag/toolbelt and capacity-limited transfer edges if needed for release acceptance;
- project completion hooks, which remain P1 rather than P0.


## Crafting Planner 0.1.0 — first production candidate

- Status: runtime acceptance pending.
- Development branch: `dev/0.1.0`.
- Exact source commit: `619d17261b2f899736cedd31d47b93393209ed3f`.
- CI run: `35662116041`.
- GitHub Actions artifact: `CraftingPlanner-0.1.0` (artifact ID `10668240014`).
- Handed DLL: `Crafting Planner 0.1.0.dll`.
- DLL SHA-256: `1fbebbe61d5848a215a38ce1b51dcc0e39d9358eb6f5633bac48a7430c2697d1`.
- Artifact ZIP SHA-256: `2d64f9d314c8025b9b18675a8a73fda1ec97f9282f06c63ff9ab1a5c3a3c0c39`.
- CI: restore/build/package-boundary/artifact upload passed, 0 warnings / 0 errors.
- Compile-time `Assembly-CSharp` and `Assembly-CSharp-firstpass` stubs are excluded from the distributed output.

### Candidate behavior

- Session-local multiple project goals.
- Supported builder/world-change project filtering from already accepted host definitions.
- Direct-card quantity management: RT add/increment, LT decrement/remove; keyboard + / - equivalent.
- One-off projects capped at x1; repeatable `BuildType.Put` projects allow quantity >1.
- Display-only native-NGUI HUD showing goals plus aggregated Required / Have / Missing.
- Player Have uses player-carried count with bags.
- Open-chest Y / Option2 executes native bounded Take Needed across all missing materials.
- No automatic completion decrement, persistence, dependency expansion, or custom management window.

### Runtime acceptance pass

1. Remove the research probe DLL before installing this candidate.
2. Open a builder with a repeatable supported project. Focus one card with gamepad, press RT three times and LT once; close the UI. Expected planner quantity: x2.
3. Reopen the builder and focus a one-time upgrade. Press RT twice. Expected quantity remains x1; press LT once to remove it.
4. Add at least two different goals and close the UI. Verify that both appear and shared materials aggregate.
5. Open a chest containing at least one currently Missing material and press Y once. Partial transfer is acceptable. Verify that no surplus moves and Have/Missing updates.
6. Optionally verify mouse + keyboard + / - on a supported focused card.
7. Return one `BepInEx/LogOutput.log`. A screenshot is needed only if HUD position/readability is wrong.

This pass does not require completing any irreversible build/repair.


## Crafting Planner 0.1.0 runtime correction

- Returned log: `LogOutput(2).log`, SHA-256 `5aad3ec6875f08a0c4a923e3ebc5b2ccacb9dbf4200a0da3271f6d64ed79cb5b`.
- 0.1.0 startup attempted to patch a non-declared `CraftGUI.Update` method and Harmony `PatchAll` threw. The intended keyboard + / - hook was therefore invalid.
- The returned log nevertheless directly verifies the already-applied gamepad goal-management patches:
  - world repair `fix_morgue_builddesk`: RT add x1, LT remove;
  - world repair `fix_morgue_throw_out`: RT add x1, LT remove;
  - builder project `graveyard_builddesk:p:flowerbed_2x2`: RT add x1, LT remove.
- The user reported not seeing the planner HUD, but the logged goal sequences returned quantity to zero before CraftGUI closed, so those sequences do not test visible non-empty HUD state.
- Later runtime emitted an NGUI warning that widgets should not be moved outside the `UIPanel` hierarchy that manages them. 0.1.0 cloned the reference label directly under the HUD root; 0.1.1 corrects that parenting.
- Status: 0.1.0 rejected; do not promote.

## Crafting Planner 0.1.1 — corrected runtime candidate

- Runtime acceptance pending.
- Development branch: `dev/0.1.1`.
- Exact built source commit: `3236ef83af04a67ad620f4d1f9587450f83ed93a`.
- CI run: `35664503282`.
- Artifact ID: `10668039306`.
- Artifact: `CraftingPlanner-0.1.1`.
- DLL: `Crafting Planner 0.1.1.dll`.
- DLL SHA-256: `1c66e15d50ebec526221259ee361f23b192dd93eee47d843f2b2140c033bcc7e`.
- Artifact ZIP SHA-256: `3d7454ee0da62d5dba036a6f5a9977f2e05d58af0af82111d24b08313bffbbb9`.
- CI: Release build succeeded with 0 warnings / 0 errors; package-boundary check and artifact upload passed.

### 0.1.1 changes

- Removed the unverified `CraftGUI.Update` keyboard patch entirely rather than broadening to a high-frequency `BaseGUI.Update` hook.
- Retained runtime-verified gamepad RT/LT direct-card management.
- Cloned the HUD label under the native reference label's existing parent instead of reparenting directly to the HUD root.
- Added one-time HUD creation diagnostics for native reference/parent identity.
- Keyboard direct-card management is temporarily deferred until a narrow verified input seam exists.

### Runtime check

1. Replace 0.1.0 with 0.1.1.
2. Open a supported project card and press RT once. **Do not press LT.**
3. Close the craft/build UI. Verify whether the planner HUD is visible and readable.
4. Reopen the same project and use RT/LT to verify quantity changes.
5. Return one log. Add a screenshot only if HUD position/appearance is wrong.

The separate gameplay-stutter investigation belongs to GraveyardKeeperResearch and is not an acceptance condition for Planner unless a controlled test later implicates Planner.


## Crafting Planner 0.1.1 runtime result — goal logic accepted, display rejected

- Runtime: Graveyard Keeper 1.407, normal 35-plugin setup.
- Plugin loaded as 0.1.1 without the 0.1.0 Harmony startup failure.
- Native HUD clone creation ran successfully: reference `zone description text`, parent `bottom field`.
- Repeatable builder goal management is confirmed live:
  - first graveyard flowerbed RT -> quantity 1;
  - close/reopen builder;
  - second RT on the same exact project -> quantity 2.
- Therefore session-local goal identity, quantity retention across GUI reopen, build-context focus, and RT increment are accepted for this case.
- User still saw no Planner text after closing the craft UI.
- Interpretation: goal/model behavior is working; 0.1.1 is rejected specifically at the presentation layer.
- Root cause hypothesis supported by implementation/evidence: the 0.1.1 label stayed under the native `bottom field` parent while retaining screen-root-style positioning. The label object existed, but its local coordinate model did not match the coordinates being assigned.
- No automatic decrement/completion behavior was involved.

## Crafting Planner 0.1.2 — dual-surface UI candidate

Purpose:
- make Planner state visible immediately while the player is still in the native project/build CraftGUI;
- keep the same state visible in the ordinary HUD after the CraftGUI closes;
- avoid a separate management window or second focus/navigation system.

Implementation direction:
- display-only CraftGUI overlay created inside the CraftGUI's existing NGUI panel;
- display-only HUD label created inside the HUD's existing NGUI panel;
- both surfaces render the same planner text and update from the same goal state;
- RT/LT goal adjustments refresh both immediately;
- no polling/per-frame UI scan;
- no new interactive widgets.

This directly answers the user request to see quantity/material changes at the moment RT/LT is pressed.


### 0.1.2 handoff

- Status: runtime acceptance pending.
- Development branch: `dev/0.1.2`.
- Exact built source commit: `5b27ccdf1642cd2d20b56a54752ad7a12e808f2d`.
- CI run: `35666120901`.
- Artifact: `CraftingPlanner-0.1.2` (artifact ID `10669417044`).
- DLL: `Crafting Planner 0.1.2.dll`.
- DLL SHA-256: `dee32a5ced4279e46f5f764377798da7cb048976db5533124350747d485c9b7a`.
- Artifact ZIP SHA-256: `f54ac748c189e0c3d9173331e8b8b24fccc2195cc937e4b663a8baf23ccc1a35`.
- CI: Release build succeeded with 0 warnings / 0 errors; package-boundary check and upload passed.

Runtime acceptance focus:
1. Open a supported builder/project UI with no goals. The craft-window Planner surface may stay hidden until the first goal is added.
2. Focus a repeatable project and press RT once. Planner text should appear **inside the still-open CraftGUI** immediately with quantity x1 and material totals.
3. Press RT again. The same visible text should update immediately to x2.
4. Press LT once. It should update immediately back to x1.
5. Close CraftGUI without removing the goal. The gameplay-HUD Planner surface should then be visible with the same x1 state.
6. Return one log; add a screenshot if either surface is missing, misplaced, clipped, or visually unsuitable.

The test need not complete any construction and does not need to re-prove goal identity/retention, which 0.1.1 already established.


## Crafting Planner 0.1.2 runtime result — CraftGUI accepted, gameplay HUD rejected

- Runtime: Graveyard Keeper 1.407, normal 35-plugin setup, tested on both an early save and a day-137 save.
- CraftGUI Planner surface is runtime-visible and updates immediately on RT/LT.
- Multiple project goals, quantities, and aggregated material totals behaved correctly in the exercised cases.
- The gameplay HUD surface remained visually absent after CraftGUI closed, despite diagnostics reporting the Planner label as active in hierarchy.
- 0.1.2 positioned the gameplay HUD label at approximately `-1256,615` by deriving a local NGUI position from screen/UIRoot dimensions. The same run used 2560x1440 with game pixel size 2. This implementation mixed screen-derived values with the HUD panel's local coordinate model and is rejected.
- Runtime language switching exposed a second presentation defect:
  - Planner-owned labels remained Russian after switching the game language;
  - pinned project names were cached at goal-add time, so they could remain in the previous language;
  - host item names did follow the game's current localization when re-resolved.
- Status: 0.1.2 goal/math behavior remains useful evidence; 0.1.2 gameplay-HUD placement and localization are rejected.

## Crafting Planner 0.1.3 — native-anchor HUD + live localization candidate

- Status: runtime acceptance pending.
- Development branch: `dev/0.1.3`.
- Exact built source commit: `be76b89fa7f68b00770b7a409eaddb0bde9654d2`.
- CI run: `35668673883`.
- GitHub Actions artifact: `CraftingPlanner-0.1.3` (artifact ID `10670926043`).
- DLL: `Crafting Planner 0.1.3.dll`.
- DLL SHA-256: `1ab761fb8adae47dccd9eb206dbb543db7fa7e130197be4a35d3f90aaec36c25`.
- Artifact ZIP SHA-256: `9f73bcd8b0a0cbf85045d7dd91fa2fd05cf40f0f13982f214059ba8fba14e23a`.
- CI: Release build succeeded with 0 warnings / 0 errors; package-boundary check and artifact upload passed.

### 0.1.3 changes

- Removed gameplay-HUD placement derived from `Screen.width`, `Screen.height`, and `UIRoot.activeHeight`.
- The gameplay Planner label remains a child of the native HUD `UIPanel`, but its position is now derived from the already-positioned vanilla `HUD.time_circle_rotating` transform through `panel.transform.InverseTransformPoint(anchor.position)`, followed by only a small provisional local offset.
- Added a one-time `CRAFTING_PLANNER event=hud_geometry` diagnostic on the first visible gameplay HUD state. It records panel size/alpha/clipping/scale, anchor and label panel-local positions, widget depth/alpha/visibility, and label corners. No polling or per-frame scan was added.
- Removed localized project-name caching from goal state. Project names are resolved from the current host localization when the Planner text is built.
- Item names continue to resolve from current host item definitions.
- Added Planner-owned strings for every language exposed by Graveyard Keeper 1.407:
  `en, de, fr, pt-br, es, ru, it, pl, ja, zh_cn, ko`.
- Added a narrow postfix on `GameSettings.ApplyLanguageChange()` to refont and redraw the CraftGUI and gameplay-HUD Planner surfaces after the host completes a language change.
- Planner labels call the host font-selection path so Japanese, Chinese, and Korean can use the current game's appropriate font/atlas.
- Current visual placement/text layout is still a technical scaffold, not the accepted final UI design.

### Runtime acceptance pass

1. Replace 0.1.2 with the exact 0.1.3 DLL; do not keep both versions installed.
2. Add at least one supported project with RT while CraftGUI is open. Confirm the already-working in-window Planner text still appears and updates.
3. Close CraftGUI while at least one goal remains. Confirm the Planner is now visible in ordinary gameplay near the top-left HUD/time-wheel area. Exact styling/placement is provisional; visibility is the acceptance target.
4. Open Options and switch from Russian to English while goals remain. Confirm Planner-owned headings, pinned project names, and item names all switch without restarting.
5. If convenient, switch once to Japanese, Chinese, or Korean and confirm Planner glyphs render rather than showing missing-character boxes.
6. Return one `BepInEx/LogOutput.log` and one screenshot of the ordinary gameplay HUD. If language switching or CJK rendering is wrong, include a screenshot of that state too.

If ordinary HUD placement and live localization are accepted, the next product checkpoint is the explicit UI/UX design pass before visual polish/F1 calibration.


## HUD Rendering Probe 0.3.0 — runtime result

- Returned runtime log: `LogOutput(4).log`.
- Log SHA-256: `8bcff4bbbda0623186eb67c52e85031f5d5f2de7f65e51610049f8436f1c8aad`.
- User screenshot showed only **HUD PROBE A - sibling** in ordinary gameplay; Probe B and the 0.1.2 Planner HUD were not visible.
- The log proves the 0.1.2 Planner HUD was not blocked by alpha/depth/NGUI geometry generation. After CraftGUI closed it was active, visible, and had vertices, but its projected screen rectangle was entirely outside the 2560x1440 viewport.
- Probe A, cloned under the same native parent as the visible `HUD.zone_name` label, remained on-screen.
- Probe B, moved directly under the HUD panel using converted coordinates, remained off-screen.
- The game's 1600x1200 transition was exercised. An external borderless/window manager then resized the actual window through approximately 1616x1239 back to 2560x1440 while the game's internal NGUI manual height still reflected the 1600x1200 transition. Probe A still followed the visible native HUD context.
- Accepted conclusion: production HUD placement must live in the verified `HUD.zone_name` parent coordinate context and must not derive canonical placement from `Screen.width`, `Screen.height`, `UIRoot.activeHeight`, or a one-time root-panel coordinate conversion.
- No special Borderless Gaming polling/integration is justified: following the native HUD parent is the narrower and more robust mechanism.

## Crafting Planner 0.1.3 — superseded before runtime acceptance

- 0.1.3 remains immutable at its recorded source/artifact identity.
- Its direct-HUD-panel + `time_circle_rotating` anchor approach is no longer the preferred candidate after HUD Rendering Probe 0.3.0 directly demonstrated a stronger host-native model.
- 0.1.3 is therefore superseded without promotion; do not rebuild or overwrite it.

## Crafting Planner 0.1.4 — verified-native-parent HUD candidate

- Status: runtime acceptance pending.
- Development branch: `dev/0.1.4`.
- Exact built source commit: `ad836eae5556858feb09f9cbf4d8eefead78931f`.
- CI run: `35754275461`.
- GitHub Actions artifact: `CraftingPlanner-0.1.4` (artifact ID `10707298538`).
- DLL: `Crafting Planner 0.1.4.dll`.
- DLL SHA-256: `b8ac438d19399963d51b9d7c1fd8fe0349d229566c70f80a1e80764d72f5f2d9`.
- Artifact ZIP SHA-256: `a270a47b9ba6d9d914dd55d29770f1a30cdc66edc80f558d64bad661e0921d62`.
- CI: Release build succeeded with 0 warnings / 0 errors; package-boundary verification and artifact upload passed.
- The immediately preceding CI run `35754054842` failed before artifact creation because the minimal compile-time stubs lacked already-verified public members `HUD.zone_name` and `UIWidget.localCorners`; the stubs were corrected in the exact built source above.
- Inherits the 0.1.3 live-localization work.
- Gameplay Planner HUD is cloned from `HUD.zone_name` and kept under the exact same native parent that rendered Probe A.
- Planner position is relative to the native zone-name label's bottom-right corner with a small fixed NGUI-local gap. The label uses a TopRight pivot so its wider content grows leftward into the viewport.
- Removed the production dependency on root/screen-derived HUD coordinates and on the 0.1.3 `time_circle_rotating` panel-coordinate conversion.
- No per-frame update, resolution polling, or Borderless-specific compatibility hook.


### 0.1.4 focused runtime check

1. Remove the HUD Rendering Probe DLL and replace any older Crafting Planner DLL with the exact 0.1.4 candidate.
2. Load a save, add one supported project with RT, and close CraftGUI while the goal remains.
3. Confirm the ordinary gameplay Planner HUD is visible and remains attached to the native upper-right zone-info context.
4. Switch the game once from Russian to English while the goal remains; confirm Planner headings/project/item names redraw in the new language.
5. Change the game resolution to 1600x1200 once with the user's normal Borderless Gaming setup still active, return to gameplay, and confirm the Planner remains visible/attached. Restore the preferred resolution afterward.
6. Return one `BepInEx/LogOutput.log` and one ordinary-gameplay screenshot.

This pass does not need to re-prove goal math, aggregation, or CraftGUI RT/LT behavior already accepted from earlier candidates.


## Crafting Planner 0.1.4 runtime result — HUD substrate accepted, final UI deferred

- Returned log: `LogOutput(5).log`.
- Log SHA-256: `3a19a6ca530b2de670509db5cfea4c250d0256795cad00e29a5000374229ae1d`.
- Runtime: Graveyard Keeper 1.407 with the user's normal 35-plugin setup.
- Ordinary gameplay Planner HUD is visible at 2560x1440.
- Native project/build CraftGUI Planner surface remains visible and goal/material state remains consistent.
- User exercised live language switching and reported correct Planner localization; the log records redraws across multiple runtime languages including Korean, Simplified Chinese, Japanese, Polish, Italian, and Russian.
- The verified ordinary-HUD surface remained in the native `zone_name` parent context.
- The 1600x1200 test is not treated as a Planner-specific layout failure:
  - the game applied 1600x1200 and ran `OnScreenSizeChanged(1600,1200)`;
  - an external borderless/window resize then changed the actual window through ~1616x1239 back to 2560x1440;
  - the screenshot shows the vanilla HUD itself globally displaced/scaled in that mixed state;
  - Planner continued to follow the same native HUD context.
- Accepted technical conclusion: 0.1.4 closes the ordinary gameplay HUD anchor/lifecycle and live-localization gate. No Borderless-specific compensation is warranted.

### UX acceptance status

0.1.4 is **not** the final presentation candidate.

User feedback accepted for the next UI pass:
- project/build and ordinary gameplay Planner surfaces must look identical;
- both must occupy the same apparent screen position so closing/opening CraftGUI feels like the same Planner card remains in place;
- materials should use native item icons as the primary representation;
- presentation should be as concise, clean, and immediately readable as possible;
- current large text-only headings/material-name rows are a technical scaffold only.

The next candidate should therefore be a UI-design implementation, not another coordinate-debug build.

## Crafting Planner 0.1.5 — lightweight row-card candidate

- Status: installed, awaiting real-runtime and visual acceptance. No stable promotion/release.
- Branch: `dev/0.1.5`; exact candidate source `c300b01f28c6c852178293eb10e21ce4002ae2b8`.
- Successful CI run `38787593910`; artifact `CraftingPlanner-0.1.5`, ID `11998627674`. Release compilation, source/version checks, policy tests, package-boundary verification and upload all passed.
- Immutable DLL `CraftingPlanner-0.1.5.dll`; plugin 0.1.5, assembly/file version 0.1.5.0. SHA-256 `4f01f304a6a03cb122a9ad0bac1515d6fce4fbc1ef5f5ac1f39b92a057c56831`. ZIP SHA-256 `81bc1146908528bb7f447a8b9b7154e6dd194b5dd87d92362cc4698adca38d87`.
- Exact downloaded CI bytes also passed gradient/localization policy checks against the installed game's actual managed references. Local artifact and manifest are preserved under ignored `artifacts/CraftingPlanner-0.1.5/`.
- Installed as canonical `CraftingPlanner.dll` with the game closed on 2026-10-15; installed hash/version matched the exact CI artifact. No prior Planner payload existed; unrelated mods remain in place. Private preinstall log snapshot/receipt: `.local/evidence/pre-0.1.5/`. No new loader or visual observation exists yet.
- Replaces the two text-only implementations with one owned renderer/layout. Native item sprites retain the native renderer's shader/tint. The backing is a borderless alpha gradient, strongest top-right and transparent bottom-left; icons/text retain independent opacity.
- Both native surfaces anchor to the same HUD.zone_name geometry using NGUI anchors, preserving vanilla HUD hiding and existing project focus.
- Existing RT/LT goal management and open-chest Y collection are retained. Separate hints use native platform key symbols and current top-window context; they do not replace vanilla tips or capture input.
- All 11 locale dictionaries include the new compact labels. Actual live label fonts are refreshed from host templates rather than retaining a previous locale's applied font. Translation quality and all-language visual acceptance remain open.
- Per-save persistence remains deferred. No save or world-completion behavior changes. All goals/material rows are shown for this candidate by explicit user choice.
- Local compile against exact installed 1.407 game/BepInEx references passed; compiled-candidate gradient direction/bounds and 11-language schema/locale-normalization checks passed. This does not establish NGUI rendering, glyph quality, real gamepad operation or final appearance.

### Focused runtime pass

1. Open a supported builder using the gamepad. Focus a project, add it with RT, add again and decrement with LT. Confirm the native focus stays usable and the added button hints are readable without replacing existing menu hints.
2. Leave at least one goal pinned. Compare the card before/after closing the menu: same position, width, row order/counts, native icons and light frameless backing. Assess opacity and readability; a pair of screenshots is useful for this visual property.
3. Open a storage with a missing material. Confirm the contextual collection hint and Y action; no surplus should move. No need to complete an irreversible construction/repair.
4. Open/close an unrelated menu once; the card should obey its native surface visibility and should not intercept gameplay/menu input.
5. If convenient, switch Russian -> English -> Russian. A later final localization pass must cover long labels and CJK as well; this focused pass does not replace it.

After the pass, collect the accessible local BepInEx log directly and match the loaded/installed version and hash. Runtime/visual acceptance is pending until user observations arrive.

### 0.1.5 runtime return — 2026-10-15

- Preserved local log `.local/evidence/0.1.5-return/LogOutput.log`, SHA-256 `dc100d6bf14c6bc0b9723d18dc3bc3166ec8be86db3c2a71dfb3572a9ff74712`; installed payload still matches the frozen 0.1.5 digest.
- Loader identifies Crafting Planner 0.1.5 and both native card surfaces; no Planner failure/exception markers occur. RT increment and LT decrement/removal are exercised. Current-storage Y requested/moved 8 flitch against Missing=8 and storage=9; other absent ingredients requested0. This is bounded logic/load evidence, not presentation acceptance.
- Shared log also contains Unity idle-point and missing-world-object errors. Their relationship to Planner is not established; do not attribute them to this plugin from the error alone.
- User screenshots reject the current UI: differing scale between gameplay and construction, offset backing, inconsistent per-name text size, small icons, absent hints, confusion with the vanilla location banner, and missing card/action presentation in storage.
- Confirmed static owners: UILabel.ShrinkContent caused per-name font reduction; UIWidget.pivot preserves world corners by moving the transform, and the backing's transform was not reset after the pivot assignment. Runtime parent-scale identity was not measured; the previous claim based only on prefab unit scale does not prove identical live presentation.
- The next evidence must come from a corrected shared presentation and bounded live calibration, rather than guessed constants or acceptance of 0.1.5.


## 0.1.6 shared-card/calibration candidate — built and installed, 2026-10-15

- Exact source: `338038532cb929d139754b5e6e685d0607c2c3c1`, branch `dev/0.1.6`. CI: https://github.com/NikichMods/Crafting-Planner/actions/runs/38791576457 ; job `116617196904`, artifact `12000307085`.
- CI Release build succeeded with 0 compiler warnings/errors; source/version identity, gradient policy, all 11 locale schemas and compile-reference exclusion passed. Calibration is explicitly enabled and recorded in the manifest. Production-without-calibration and calibration profiles also compile directly against the exact installed host references.
- ZIP SHA-256 `b88f7c643ccd2764b17be4e9d6e88a8bba0b195e37ac206eadc2647faa411f06`. Package contains exactly `CraftingPlanner-0.1.6.dll` and `build-identity.json`. DLL SHA-256 `359a808eb86b0069b87b49757d192d61ebbb9b1486ba829fec07eeaf3d6b89df`; assembly/file version `0.1.6.0`. Downloaded CI bytes passed the policy checks using actual installed runtime references.
- Game was closed for installation. Canonical installed DLL matches the exact CI digest; previous 0.1.5 DLL was copied outside the plugin tree and its digest verified. Prior log was preserved; no additional Planner DLLs were found. Installation does not establish successful loading or runtime behavior.
- Changes awaiting acceptance: one shared native-frame card in HUD/build/chest; uniform wrapped names and adjustable larger icons; backing/text alignment; separation below location heading; LT/RT hints before/after the first goal; visible chest-local mouse action/native Y and capacity help.
- Temporary F1 editor: shared position/scale/spacing/type/icon/opacity/gradient sliders. Export button and close log `event=calibration_export format=1` with all 15 settings, context, actual dimensions and projected card/backing rectangles. Settings persist only within the running session. Editor suspends native input and restores prior character/camera flags on close; actual suppression/restoration needs runtime verification.
- Focused next run: compare unchanged settings across HUD/build/chest; check long item names stay the same font size; open F1 in each view, adjust/export/close and verify no unintended actions plus normal controls afterward; click Take Needed and press Y with current storage open; check card remains visible and counts refresh. Check hiding through unrelated menus. All rows remain visible; workstation-production expansion remains cancelled.

## 0.1.6 runtime/design return — 2026-10-15

- Preserved returned log SHA-256 `b0a8155a9cbc1526adfc12554f6d7b471c08a19172af1b2a0b2a9703dde23bac`. Loader identifies 0.1.6; no Planner failure/error markers found. Mouse transfer requested/moved four flitch in one chest and four nails/eight detail_1 in another, with repeated clicks requesting zero after needs were met. User explicitly confirms the mouse action works; its presentation/location remains rejected.
- Calibration export works. Latest HUD values: right=3.854, top=72.794, width=240, scale=0.8, font=16.777, icon=30.138, row=18, padding=4, column=32, column_gap=4, title_gap=5.449, footer_gap=8.988, opacity=0, text_opacity=1, fade_power=0.5. Actual body/backing bounds both `2139.8,892.6,2554,1294` at 2560x1440, width259/height251 logical units. Identical HUD body/backing rectangles prove this placement case only; no build/chest export comparison or exhaustive input-guard acceptance follows.
- User supplied day/night and calibration screenshots: zero backing loses contrast in day; high opacity reads as a heavy rectangle; bright extensive text is noisy at night. Narrow names wrap into tall rows. The current visual direction is not accepted as final UI.
- Follow-up private static inspection on the same pinned 1.407 assemblies/assets: native CraftItemGUI.Redraw uses CraftDefinition.icon via BaseItemCellGUI.DrawIcon; canonical `i_b_chest` / `i_b_furnace` are 32px blue construction pictograms. Material artwork includes i_nails/i_flitch/i_detail_1/i_stone_plate_1. Native HUD zone-name graphic is `hud_right_01`, resources Sprite25103, referenced by level2 UI2DSprite33399. Native ButtonTipsStr.Print(List<GameKeyTip>,string) owns final tip-label composition. KeyBindings default maps F to Work (8), while Option2 (23) has no default keyboard binding; a Y hint alone cannot expose a working F shortcut. Runtime compatibility still needs a narrowly scoped chest test.
- A private interactive design comparison uses native extracted artwork and user-supplied day/night background crops, with approximate browser typography/effects; it is not a runtime candidate or visual acceptance. It compares full labels, project labels only, and almost icon-only rows. No new DLL was built/installed for this design review; 0.1.6 remains the installed immutable candidate.

## 0.1.9 returned runtime — 2026-10-16

Exact source 932a7ae0897d48e20a54ed977c0cfae91d4d5d6d; installed DLL SHA256 5ca022e46edae854e6841594a189a407fbcbf6aab2001275a573958de2a4f590 matches the immutable CI artifact. Preserved private LogOutput.log SHA256 d2ef5b6ce331cff731ba36e30ae58b18fa542a1a9e3774d9f7dc1187fa3f71d8 records loading 0.1.9; no planner exception/circuit-disable event found. User reports and seven screenshots confirm supported menu quantities/aggregation and actual needed-item transfer. RS enters/exits and directional changes work with character movement contained. This does not establish universal compatibility/transfer edges.

Rejected: native presentation differs from mockup, missing HUD arrows/mouse clicks, insufficient controller focus contrast, duplicated/off-centre title, quantity/Missing misalignment, menu rectangular stripe and ingredient/name spacing, framed chest action, feedback hidden behind inventory windows. Character Inventory-tab visibility and F2 grouped calibration are newly requested. Runtime source correction continues on dev/0.1.10; 0.1.9 is not accepted for stable promotion.

## 0.1.10 installed correction candidate — 2026-10-16

- Exact source: 2af839b1a495007f46b7c8f889f98b6d3008a82c, dev/0.1.10.
- Successful clean hosted CI: 38830954723; artifact 12016149655. ZIP SHA256 262b4ebf67b296cca0493d741a0f3f419fa9618e6ea68083956b7751deea2cea.
- Numbered DLL and installed canonical DLL SHA256: ad6e0c402775407fae1a9d3af882126905d0e473b69bd6f648e092771819767c. Binary version 0.1.10.0, calibration enabled.
- Both native-reference configurations compile; CI clean stub build, source/version identity, state/geometry/11-language tests and reference-exclusion checks pass. Exact downloaded DLL policy tests pass; archive digest and manifest verified. Unhanded intermediate CI 38830747876 is not the installed artifact.
- Installed with host process absent; previous 0.1.9 payload/log and installation receipt preserved privately. No stable acceptance inferred.
- Requested native checks: mouse HUD arrows/hover/click without world actions; RS focus/quantity/B exit; quantity/Missing alignment and F2 tuning/export; Inventory-tab visibility and other-tab/modal hiding; supported menu title/ingredients/names/scroll and native row-build click; plain chest Y/F action with actual/no-op/partial feedback below tips. All-language dictionary coverage is automated; native glyph/spacing review remains bounded to actual returns.

## 0.1.10 returned runtime — 2026-10-16

Exact source 2af839b1a495007f46b7c8f889f98b6d3008a82c, installed SHA256 ad6e0c402775407fae1a9d3af882126905d0e473b69bd6f648e092771819767c. Preserved private log SHA256 69dc51e43353b8779a3b487bc9da366c9f4090242c6e1b1fe44957f135b99ff8 records loading 0.1.10 with no planner exception. User confirms HUD mouse arrows, visible hover, RS editing and chest mouse/keyboard/gamepad transfer for exercised cases. This is bounded acceptance of those actions, not stable acceptance.

Construction first-open geometry is broken; a quantity refresh partially repairs it. Six screenshots are preserved privately with the log. Chest result is visible but its position/spacing needs calibration. Recovered F2 settings from exports at log lines 2272/4004/4005/4049 are recorded in PRODUCT_SCOPE.md; preserve the original fractional measurements separately from rounded 0.1.11 defaults. New title/banner, font groups, icon independence and calibration controls follow the explicit user return. Inventory-tab and full multilingual visual acceptance are not inferred from this return.

## 0.1.11 installed correction candidate — 2026-10-16

- Exact source: 7749d701a84fa08f75ccc297ab994d6403af28f8, dev/0.1.11.
- Successful CI: 38837166111; artifact 12019039672. ZIP SHA256 dcc201635dfcea19611b3a697a27aa80886bdc89af09e6a52edab5c489427a6d.
- Numbered/installed canonical DLL SHA256: 19401a0e4111e13dc570ed6399f0863e348660fa8e8a09ef32a1003a7472e5f3, binary version 0.1.11.0, calibration enabled.
- Both native-reference configurations compile. Clean hosted stub build, identity, state/geometry/font-policy/integer-calibration/localization coverage and reference-exclusion gates pass. Exact downloaded DLL policy tests pass. Dimensions/scale normalization is idempotent; quantity-only font selection leaves material numbers unchanged; icon visual size does not determine text row/card-width policy.
- Installed while the game was closed, with previous 0.1.10 DLL/log preserved privately. Runtime and visual acceptance remain pending.
- Requested check: first-open construction before edits, then scroll/reopen; HUD/gamepad and native row click ownership; independent icon tuning with stable text; font groups and title/footer placement; chest feedback calibration/export; location restoration and resolution-change stability. No all-language visual or automatic placement-decrement acceptance is inferred.

## 0.1.11 returned runtime — 2026-10-16

Exact source 7749d701a84fa08f75ccc297ab994d6403af28f8, installed DLL SHA256 19401a0e4111e13dc570ed6399f0863e348660fa8e8a09ef32a1003a7472e5f3. Private log SHA256 bb89b1611047fef6adaa16791c84541cab69078ee398362d43738fcb77120246 loads 0.1.11 with no planner exception found. User reports ordinary-HUD rendering appears correct across all 11 languages, with English screenshot; this is a bounded visual report, not exhaustive language/context acceptance. Menu geometry is improved but not accepted: quantity column is low and intersects a native vertical boundary. Three screenshots are preserved privately. Per-widget F2 tuning/reset and persistent calibration-time RS/B preview are requested.

Recovered format=4 baseline: icon46/padding12/column67/backing35%/goalsOffset0/columnInset20/alternateQuantity=true/footerCraftFont=true/footerFont20/iconX=-3/feedbackX11/feedbackY-5/feedbackGap0; other fields match the returned log. Native A/X/B movement was explicitly simplified by the user to one horizontal whole-line control. User explicitly selected Building Planner for display name/English HUD title; stable technical identifiers are preserved.

## 0.1.12 installed Building Planner candidate — 2026-10-16

- Exact source aa49988daf9eaf28a26ada824ae16c8a5f8b5aea, dev/0.1.12. Plugin display name/English HUD header Building Planner; technical identity preserved.
- Successful CI 38885542445, artifact 12039238521. ZIP SHA256 80e2206f5d4e5eea6c42c35723453edb37a5f51562d717aae90c576bfd20f187.
- Numbered and installed DLL SHA256 40e639d717f599e945518fe7ccc08cf61f24b2358b246fdc070fbac9b9bbac4d, binary version 0.1.12.0, calibration enabled. Intermediate CI 38885204969 is not the installed payload.
- Both native-reference configurations compile; clean hosted stub build, identity/state/geometry/localization/font/reset/rounding tests and reference-exclusion pass. All 51 slider reset field IDs match their assignments; exact downloaded DLL policy tests pass. This does not prove native layout/input acceptance.
- Installed with the host closed, previous 0.1.11 DLL and returned log preserved privately. Next native checks are listed in DEVELOPMENT.md; format=5 carries menu, footer, chest and title tuning.

## 0.1.12 returned UI tuning — 2026-10-16

Installed hash 40e639d717f599e945518fe7ccc08cf61f24b2358b246fdc070fbac9b9bbac4d matches the recorded source/artifact. Preserved private log SHA256 7752e1477aaffca6fc9b09890d2b87d2612e78760132f0ee490174d0c1ae56f2 loads Building Planner0.1.12; no planner exception/disable found. Two new screenshots show configured chest/menu. User accepts current HUD English header and completed RS/B tuning, but requests a unified right-aligned chest row to remove language-dependent gaps and a longer ingredient-centred menu caption to avoid the native close X. No stable acceptance.

Final format=5 export supplies next defaults: right9/icon50/backing39%/goalsOffset-7/materialsGap12; menuX17/Y7/minusX13/plusX-13/ingredientsX29/HUDfont16/border retained; footerX-22/Y3/icon16; feedbackX264/Y-5/gap0/lineGap4. Other existing values are unchanged. Independent chest TakeX264/nativeTipsX-145 are superseded by the explicitly requested common right-aligned row; result coordinates remain based on their previous independent origin.

## 0.1.13 installed candidate — 2026-10-16

- Exact source 3416079a59cdf73022d100ed63e6c07ca3352f50, branch dev/0.1.13. Successful CI38896495222, artifact12044480696, ZIP SHA256 2b01b9f12cd490032a22efa32c0604ccbe4269f9889719bd6a03de9c44431452.
- Numbered/installed DLL SHA256 95d7f7b3a098cb895334297b3c448803f16dede5b488fc63ef5bbdcd5f609edc, version0.1.13.0, calibration enabled. Installed with the host closed after verifying exact manifest/source/version/hash; previous payload/log/receipt preserved privately.
- Both native-reference configurations compile with warnings treated as errors; clean hosted build, identity/state/geometry/localization/font/reset/rounding tests and package-boundary pass. Variable-width/omitted-action geometry preserves the common right edge/equal gaps. All54 slider field/reset IDs were checked; downloaded exact DLL policy tests pass. Native rendering/input are pending game evidence.
- Check unified A/X/B/Y row across English/Russian, Put/Take, omitted/disabled native actions, keyboard F, F2 preview, modal/reopen/restoration and feedback. Check Add to planner above ingredients with native title styling, caption X/Y/size/reset, first-open/scroll and native construction click. Existing English HUD title and tuned RS/B are retained. No stable acceptance or main promotion.

## 0.1.13 returned runtime — 2026-10-16

Installed exact DLL SHA25695d7f7b3a098cb895334297b3c448803f16dede5b488fc63ef5bbdcd5f609edc matches source3416079a59cdf73022d100ed63e6c07ca3352f50. Private log SHA2564af827aebdfb6557694edd61c4e325ca7fa98ba7e890f681df94cc932888445b loads Building Planner0.1.13 and Quick Stack1.0.0; no planner exception/disable found. Three screenshots are preserved. User confirms unified chest actions work well. The header caption is legible but changes the perceived purpose of construction; requested move to lower help line. Results require right alignment and tighter spacing. Quick Stack world stacking leaves planner counts stale until another refresh; this is a newly reported invalidation defect, not a transfer or counting failure. Final format6 keeps all baseline values except feedbackX234; the new required right-edge origin supersedes that X offset. No stable acceptance.

## 0.1.14 installed candidate — 2026-10-16

- Exact source91ea1558f83b5ede2d18363d3b34522d50c22260, dev/0.1.14. Successful CI38902057056, artifact12046073039; ZIP SHA2560df3c1343e7b234512fd88d5e4a9a912dca0a0476ea87c8d74f88fa065af8f5e.
- Installed/numbered DLL SHA2565b2525936fa6ec32123cf19f54a3882eaa844f0db6f10b08d7616c9fb1f72ff1, version0.1.14.0, calibration enabled. Installed while game closed, exact manifest/version/hash verified; previous DLL/log and receipt preserved privately.
- Both native-reference configurations compile with warnings as errors. Clean hosted build, state/localization/geometry/font/reset/rounding/transfer-invalidation-coalescing tests and package-boundary pass. Downloaded exact DLL policy tests pass. All54 reset field identifiers checked. These establish build/policy evidence, not native visual/input/Quick Stack runtime acceptance.
- Next check lower help line and untouched build click; right-aligned compact Taken/all notices and F2 offsets/negative gap; Quick Stack world transfer refresh without chest/RS, native_transfer diagnostic, no-op/failed/unrelated transfer preservation. Existing accepted A/X/B/Y row and tuned HUD/RS/B retained. No stable/main promotion.

## 0.1.14 returned runtime — 2026-10-16

Installed DLL5b2525936fa6ec32123cf19f54a3882eaa844f0db6f10b08d7616c9fb1f72ff1 matches exact runtime source91ea1558f83b5ede2d18363d3b34522d50c22260. Private log SHA256efec78fbeb321022009892eab05dab2f9099a2cd74029bf9ac2eb90003d8e6de loads0.1.14 with no planner exception/disable found and multiple event=inventory_refresh trigger=native_transfer records. Two screenshots preserved. User explicitly confirms Quick Stack updates Have/Missing immediately and accepts current chest action/result appearance. This is bounded acceptance of exercised UI/transfer/compatibility cases, not exhaustive capacity or stable acceptance. Latest format7 defaults unchanged.

Remaining UI request: combined bottom construction actions, native subdued text gradient and visible dark circular arrow glyphs; separate second help line is superseded. English Building Planner header, HUD/RS/B and accepted chest behavior remain unchanged.

## 0.1.15 installed candidate — 2026-10-16

- Exact source156ffd13c9d47bd317adadc9ea6043e257cd213e, dev/0.1.15. Successful CI38941628701, artifact12063467100, ZIP SHA256a3462efb4d5b34a68f831a3a5b6924b944ed6276b97a3b3ee3a291b52b172b15.
- Installed/numbered DLL SHA256a771b795d1c2467c85e13e34fe29c16ca17e4ccf43f1b14aa184fe03a2e60267, version0.1.15.0, calibration enabled. Installed with game closed after exact manifest/source/version/hash validation; prior0.1.14 DLL/log/receipt preserved privately.
- Both native-reference configurations compile with warnings as errors. Clean hosted build, state/localization/geometry/font/reset/rounding/transfer-coalescing and package-boundary pass. Downloaded exact DLL tests pass; all55 slider reset IDs checked. Native gradient/atlas signatures compile against exact host assemblies. These do not prove the new combined row's actual rendering/lifecycle.
- Check one lower supported-menu row: planner action first, native actions in native order, Close at right frame edge; RU/EN/longer locale, native cream/brown styling, visible round arrows, mouse-only planner hint and gamepad actions. Tune common X/Y/gap and planner-caption size in F2; test open/close/modal and ordinary production restoration. Accepted0.1.14 chest/results/Quick Stack behavior retained, no main/stable promotion.
- Reusable native transfer and tip-style findings promoted to shared GraveyardKeeperResearch commit70320d9, with bounded consumer evidence links; mod product/acceptance ownership remains here.
