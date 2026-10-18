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

## 0.1.15 returned runtime — 2026-10-17

User explicitly accepts native round direction icons and the combined menu help appearance/functionality. Preserved private log SHA256198c6cb73b389a284cc6f49b28f70d2691cce1fc7a8e9e520e17184d09d2e4a5 loads Building Planner0.1.15 with no planner exception/disable found; no new format8 export, so style defaults are retained. Installed source identity is156ffd13c9d47bd317adadc9ea6043e257cd213e, expected DLL SHA256a771b795d1c2467c85e13e34fe29c16ca17e4ccf43f1b14aa184fe03a2e60267. One repair-table screenshot shows native energy overlap with quantity/arrows; this opens only the supported energy-row layout correction, not a redesign or stable acceptance.

## 0.1.16 installed candidate — 2026-10-17

- Exact sourcea8bda9eeb0f4829ff5eaf2fbec0dfbbaa213c25b, dev/0.1.16. Successful CI38977362668, artifact12079323553, ZIP SHA256b7c9c3883da0cb031a855bb3ef34d9f016e1bd5a1f0f8de966229ff88b155fb7.
- Installed/numbered DLL SHA25674f612880574d90a53d771a925f8cbc56bbb69f3e5b1acad8ffa7de189d2d37f, version0.1.16.0, calibration enabled. Installed with game closed after exact manifest/source/version/hash verification; prior0.1.15 DLL/log/receipt preserved privately.
- Both native-reference configurations compile with warnings as errors. Hosted build/state/localization/geometry/font/reset/rounding/transfer-coalescing/energy-bound checks and reference exclusion pass. Downloaded exact DLL tests pass. Native energy-field signature is verified against exact host assemblies. No visual/runtime acceptance is inferred.
- Check repair-table energy above quantity/arrows, mouse/gamepad, repeated quantity edits/reopen/scroll without drift, and unchanged ordinary construction/production geometry. Single-line native energy only; arbitrary multiline costs are not changed. Existing accepted tips/chest/Quick Stack presentation remains. No main/stable promotion.

## 0.1.16 visual milestone accepted — 2026-10-17

User reports the energy separation works and explicitly accepts the whole current interface from the visual standpoint. Exact installed sourcea8bda9eeb0f4829ff5eaf2fbec0dfbbaa213c25b / DLL74f612880574d90a53d771a925f8cbc56bbb69f3e5b1acad8ffa7de189d2d37f is unchanged and hash-verified. Current process remains running. Preserved live snapshot SHA25696ae57df373be5e2137e26f9e87678ddac37a51ecdccccde85c7a66570c5316d loads0.1.16 and has no planner exception/disable found; it is a partial live-session snapshot, not a completed shutdown log. No new build/install or main promotion.

Visual workstream is closed. Core exercised functionality remains bounded to earlier returns; next required milestone is exactly-once reduction after successful builder placement and separate world-project completion boundaries. Per-save persistence is still deferred. Full functional/release acceptance remains open.

## New multi-line spend defect and placement observer installation — 2026-10-17

User screenshot of a clearing row (`Расчистить путь`) shows a multi-line native cost block overlapping the planner quantity/arrows. This reopens only that layout case; the accepted ordinary/single-line-energy presentation is preserved. Current SeparateEnergy intentionally handles only single-line energy, so the earlier accepted repair case does not cover this one. No UI source change or new production artifact is made. A separate native-cost column before planner quantity is proposed, not adopted.

The user closed the game and explicitly authorized passive observer installation. Installed PlacementCommitProbe `0.1.0` / assembly `0.1.0.0`, frozen source `874383e27dc4ceeea7d00fc7a1bd2708046bc121`, SHA256 `d900518477ece2f807fa657d106383e13cacdba843020e6cb1fbb682ac45be4f`, alongside unchanged Building Planner0.1.16 SHA256 `74f612880574d90a53d771a925f8cbc56bbb69f3e5b1acad8ffa7de189d2d37f`. Game-closed, source/installed hash/version and baseline identity checks passed. Previous log and supplied screenshot preserved privately under `.local/evidence/placement-probe-install/`; baseline log SHA256 `3cc6b607f6a19c21bd85640953b332552796a19cfd53f9a39c045e40e3931851`, installation receipt stores machine paths/time. No game launch or Harmony/runtime activation acceptance is inferred. Follow [PLACEMENT_COMMIT_RESEARCH.md](PLACEMENT_COMMIT_RESEARCH.md) for the focused action matrix; plan reduction remains absent.

## Placement observer returned and multi-line column adopted — 2026-10-17

User reports the action matrix completed and adopts the separate native-cost column for multi-line rows only, preserving ordinary/single-energy rows. Preserved return log SHA256 `64e9a562de8baa072cc4d9c27c7aae2b817797b88c5a7c8a6a64cfeef0a0267a` loads exact probe0.1.0 and planner0.1.16; no observer error. Two successful same-project placements have distinct WGO IDs215333/215349, each exactly one successful spend/leave/built redraw and required4-item deltas for all3 materials. Busy attempt3 has zero commits, and3 standalone cancellations have no successful sequence. Insufficient materials are rejected upstream with can_craft=False, not an exercised DoPlace CanBuild failure. See PLACEMENT_COMMIT_RESEARCH.md for identities and limits. This establishes bounded native observations, not production reduction, other-goal isolation or world-project acceptance.

## 0.1.17 installed multi-line cost column candidate — 2026-10-17

- Frozen source `50652ee3e8ecc3afbabc26ae06f407228e28b6ce`, branch `dev/0.1.17`. Successful CI [39022244129](https://github.com/NikichMods/Crafting-Planner/actions/runs/39022244129), artifact `12096483899`; ZIP SHA256 `0c0f96ba23bb12c9af3f9d8a6221befe62706dae150c0f47689c6baf8577113a`.
- Exact downloaded/installed DLL SHA256 `5db81e4f0ecdf7527399564135790be8aa64887890dd6986fa0893f229b81343`, version `0.1.17.0`, calibration enabled/F2 format8 unchanged. Multi-line native spend gets a separate column before planner controls; native vertical bounds/text/width and row height are preserved. Ordinary/single-energy branches retain their adopted geometry.
- Both native-reference modes compile with warnings as errors. Clean hosted build, source/version identity, policy/state/localization/geometry/font/reset/transfer/energy checks and package exclusion pass. The new discrimination covers native multiline/CRLF/CR, empty/ordinary/single-energy and trailing-newline inputs. Downloaded exact-DLL policy checks pass; no native rendering/input acceptance is inferred.
- Installed while host closed after exact source/manifest/hash/version checks. Prior0.1.16 payload/log and installation receipt preserved privately under `.local/evidence/pre-0.1.17/` and `.local/installation-backups/pre-0.1.17/`. Completed passive probe was hash-verified, backed up outside the load directory and removed.
- Required native pass: reported clearing row's multi-line costs/ingredients/quantity separation, mouse/gamepad quantity changes and reopen without drift; unchanged ordinary rows and single-energy repair row. No new placement test matrix is required for this UI-only candidate. Automatic reduction remains absent; no main/stable promotion.
- Reusable probe findings promoted to shared GraveyardKeeperResearch commit `8a8aef1`; project goal semantics and production acceptance remain owned here.

## 0.1.17 returned technical separation and adopted calibration/quantity changes — 2026-10-17

User screenshot confirms the native multi-line energy/tool/efficiency block is separate from planner controls; user explicitly reports technical correctness and requests independent position calibration. Preserved log `.local/evidence/return-0.1.17/LogOutput.log`, SHA256 `7dbfc0620612a51bb907b046570a499f18915a058a6d805ae317832301bcf81b`, loads Building Planner0.1.17. No planner exception/disabled-row result is found. Format8 export retains source defaults, including ingredients29/0 and old fixed-arrow13/-13 offsets; screenshot and log are private. This is not final visual positioning or full release acceptance.

Adopted next: same movable cost block for single/multi-line spend; rightmost column exclusively bare centred quantity; measured glyph-edge gap13 with tuning; real signed material offsets relative to costs; per-repeatable-goal maximum20 across all edit paths, one-off maximum1. User will tune cost/material positions before adopting those tuned values as defaults. These decisions supersede preservation of the old stacked single-energy geometry.

## 0.1.18 installed common-cost calibration and quantity candidate — 2026-10-17

- Frozen source `23910f94bc1118dbb64a4b55fa466584981f1419`, branch `dev/0.1.18`; successful CI [39024756811](https://github.com/NikichMods/Crafting-Planner/actions/runs/39024756811), artifact `12098465914`. ZIP SHA256 `6da6855bfca73f7606809723d1e19429e0e7aedff2a94ed6d90b9ea388f3f45e`.
- Exact downloaded/installed DLL SHA256 `faeba2d7538196b8975d61344268d8dc03293d01874231c01154832a4721a7ef`, version 0.1.18.0, calibration enabled. Format9/58 resettable sliders; new MenuSpendX/Y and QuantityArrowGap(default13), old menu arrow fine offsets reset to0. Quantity has no multiplication prefix in menu or shared card and centres independently of native cost height. All native costs use the same movable column; text/width/height/row height remain native. Materials now genuinely move in both X directions relative to costs.
- Both native-reference modes compile with warnings as errors. Clean hosted source/version/state/geometry/localization/font/reset/transfer/package-boundary checks pass. Exact downloaded-DLL policy and project-state tests also pass against real native metadata. A project arithmetic fixture constructs only id/value data without the native Item constructor (which loads Unity resources); it proves planner state/aggregation, not native inventory behavior. Tests cover19->20, repeated HUD/menu overflow attempts including a large input, unchanged aggregate at cap, decrement/re-add, zero removal and one-off cap1; measured arrow gaps over one/two-digit widths and reserved hitbox span; new baseline/reset/Record fields. All58 slider assignment/reset field IDs match.
- Installed while the game was closed; prior0.1.17 hash matched and was backed up with its log outside the load directory. Source/manifest/version/archive/DLL hashes and installed digest verified. Private receipt: `.local/evidence/pre-0.1.18/installation.json`. Research sidecar remains absent. No main/stable promotion or installed 0.1.18 load/render acceptance is asserted.
- Next user pass: F2 cost X/Y, material X/Y, independent reset/export, energy-only and multi-line cases/reopen; bare centred counts and arrow spacing at0/1/9/10/20, menu and HUD mouse/gamepad maximum20 and one-off1. Supported Build calibration can open with no pinned goals. User will export format 9 and then choose tuned cost/material positions as defaults. Do not re-run the placement probe for this UI/state candidate; automatic successful-operation decrement remains absent.

## 0.1.18 returned calibration and confirmed cap — 2026-10-17

User confirms removal of the multiplication prefix and maximum20, supplies four screenshots, and reports repair tuning moves previously tuned ordinary-builder ingredients into the counter. Preserved private log .local/evidence/return-0.1.18/LogOutput.log, SHA256 88e97f1df8e6072efe0c9b21bd54de975d12512128619cf02860a080decaec9d, loads0.1.18 with no planner disabled-row/calibration error found. Two format9 exports retain both calibration stages: shared menu17/1, arrow fine-X8/-9 and quantity fine-offsets0/0; first ordinary materials29/0 and native cost0/0, second cost-row materials46/0 and native cost24/-1. Other existing defaults are unchanged. Both snapshots match the relevant screenshots; no further manual value recall is required.

Adopted correction: independent material profiles per actual visible cost-block presence, recovered ordinary29/0 and cost46/0 defaults, shared native cost24/-1 and counter17/1/8/-9. Right arrow must hide at20 in both menu/HUD and return after decrement, matching the existing one-off maximum affordance. This is not whole-build stable acceptance or proof of automatic goal reduction.

## 0.1.19 installed independent-material/limit-arrow candidate — 2026-10-17

- Frozen source 0a1fd467e3684905682eb95a1cd71061227610f7, branch dev/0.1.19; successful CI39027094562, artifact12099300030. ZIP SHA256 8b03b83f052cc8f72a36c4d5c5434ea5d987d795e7fba01acf4de60bd4cabcdd.
- Exact downloaded/installed DLL SHA256 6b98f886ffa64791306cc9334419a5f4b358f51dd32a429779cbb1511ab56382, version 0.1.19.0, calibration enabled. Independent plain-material29/0 and cost-material46/0 profiles, common native-cost24/-1, counter17/1 and arrow fine-X8/-9 reproduce both returned snapshots. Format10/60 resettable sliders export both profiles.
- Both native-reference modes compile with warnings as errors. Clean hosted identity/build/state/policy/localization/font/geometry/package checks and exact downloaded-DLL tests pass. Regression checks mutate each material profile without changing the other; shared CanIncrease covers0/19/20/20->19 and one-off0/1, used by increments and both arrow consumers. All60 reset field IDs match. These are compiled-policy/state proofs, not native rendering acceptance.
- Installed with the host closed; previous0.1.18 digest matched and was backed up with its log. Source/artifact/version/hash and installed digest verified. Private receipt: .local/evidence/pre-0.1.19/installation.json. No main/stable promotion.
- Next native pass: builder -> repair -> builder/reopen, separate F2 material pairs/reset/export, right arrow hidden at20/restored at19 in menu/HUD, without moving the number. No placement probe repetition is required; automatic goal reduction remains absent.

## 0.1.19 visual acceptance — 2026-10-17

User confirms independent settings no longer affect one another, the right arrow disappears at20 and returns at19, and explicitly accepts the current visuals. Frozen0.1.19 source/hash remain unchanged. Preserved private snapshot `.local/evidence/accept-0.1.19-visual/LogOutput.log`, SHA256 `023e04bcdf165d7f9c69f78b2f336ae1ac9a9bd8460660766a456b772f5cb51a`, loads0.1.19 with no planner exception/disable found. This is bounded visual acceptance, not exhaustive gamepad/language/functional or stable-release acceptance. Preserve the tuned defaults and close visual iteration unless a new defect/request appears. The user explicitly resumes automatic successful-placement reduction.

## 0.1.20 installed repeatable-placement reduction candidate — 2026-10-17

- Frozen source `db04213bfed06560fc4cc188f1ca2d73e2b884df`, branch dev/0.1.20; successful CI [39053843624](https://github.com/NikichMods/Crafting-Planner/actions/runs/39053843624), artifact12108657880. ZIP SHA256 `466f5862e50463d2f88b579a87f3f6b0969080d643edb7b7360ec32e05b26961`.
- Exact downloaded/installed DLL SHA256 `e45dd14fa6a301e7d887811a7008aa107edca1e5bc7bafdd0cb71839d9396058`, version0.1.20.0, calibration enabled/F2 format10 unchanged. No accepted visual source/default changes. Scoped native successful spend + normal DoPlace completion + captured original WGO just_built transition reduce the matching existing repeatable goal by1;0 removes it. Native action/inventory remain host-owned. Separate upgrade/repair/clearing reductions remain absent.
- Both native-reference modes compile with warnings as errors; exact native method/field metadata checks pass on MVID6f50b8e7-156b-49ac-bbe8-7505894b2364. Clean hosted build/source/version/state/policy/localization/geometry/package exclusion pass. Exact downloaded-DLL tests pass against native references. New state tests cover exact builder/definition/context identity, unrelated goal preservation, wrong inventory/needs, absent/failed/duplicate spend, native exception/rejection, once-only receipts, stale removed/re-added goal membership, repeated success, material aggregation and zero removal. No native activation/decrement is inferred from these checks.
- Installed at2026-10-17T17:29:17Z with host closed, previous0.1.19 digest verified and backed up outside loading, along with its log and private installation receipt under .local/evidence/pre-0.1.20/. Numbered artifact/source/version/archive/DLL and installed hashes verified. Passive probe remains absent. No main/stable promotion or game launch by the agent.
- Required runtime pass: event=placement_tracking state=ready; pin a repeatable goal3 and place copies through the normal game route (3->2->1->removed), with exactly one event=goal_placed per successful placement and updated material totals. Leave another pinned goal unchanged. Cancel a preview, attempt busy space/resource rejection and place an unpinned/different project: no unrelated decrement. Reopen menu to check refreshed quantity. These checks concern the new production behavior, not a repeat of passive research or closed visual tuning. Tools will collect the local log after user gameplay.

## 0.1.20 returned production placement check — 2026-10-17

User reports completing the proposed placement test list and asks for log verification. Exact installed DLL hash remains e45dd14fa6a301e7d887811a7008aa107edca1e5bc7bafdd0cb71839d9396058, source db04213bfed06560fc4cc188f1ca2d73e2b884df. Preserved private snapshot .local/evidence/return-0.1.20-placement/LogOutput.log has SHA256 c4adec9b00a3f276046d462cd0560137f32d24f65ba8d3171b3d27919aca9b7c. Loader confirms0.1.20 and event=placement_tracking state=ready. No planner exception/disable/warning is found. The host is closed at collection; the captured log ends at main-menu/loading-hide, not an asserted complete shutdown marker. Other Unity resource/world-object errors exist without evidence linking them to planner placement; do not call the whole log error-free.

-13 native DoPlace calls correlate to9 successful retained-object placements, each exactly one goal_placed, plus4 busy-space failures with none. All9 object UIDs are distinct. Reconstructed quantities including manual edits decrease by exactly1 per success.
-Box goal mf_box_stuff_place decreases3->2 twice, with an explicit manual re-increase between them. Anvil goal mf_anvil_2 exercises3->2->1->0 (UIDs215700/215714/215728), then2->1->0 twice (215756/215784 and215826/215854). Empty-plan HUD becomes invisible after the first two zero removals; with another goal present it remains visible.
-In the final paired-goal case, box quantity2 is unchanged while anvil2->1->0; there is no box adjustment or box placement event in that interval. A subsequent three-busy-attempt block likewise has no goal_placed.
-Standalone preview cancellations include the pinned ore-goal and paired-goal runs and produce no automatic decrement. can_craft=False at line1528 blocks the menu action without another DoPlace/decrement; this is upstream resource refusal, not direct coverage of the internal CanBuild early return. Native CancelPlacing inside a successful call's cleanup is not classified as failure.

This closes the exercised0.1.20 production activation, once-per-placement reduction, zero removal, unrelated-goal preservation, cancellation and busy/upstream-resource-rejection checks. Actual material totals and pixel rendering are not recorded by this log; derived requirement arithmetic is separately automated-test verified. Successful unpinned placement, arbitrary scripted/modded definitions and all gamepad/language permutations are not newly established. This bounded technical pass is not full release acceptance or main/stable promotion. Upgrade/repair/clearing success boundaries remain separate required follow-up; reuse accepted repeatable evidence rather than repeating the probe.

## One-off completion probe0.1.0 installed — 2026-10-17

User requests investigating automatic removal for repair/upgrade/clearing and preparing a probe if needed. ONEOFF_COMPLETION_RESEARCH.md records the prior-evidence lookup, static findings, mechanism/method choice and bounded native matrix. Production one-off removal remains absent and blocked on returned correlation; no production source/version/default changes.

- Frozen research source8c82c7dbe93b0018ab97cb1c1d594402fa759205 on research/oneoff-completion; version/assembly0.1.0/0.1.0.0, GUID nikich.gyk.oneoffcompletionprobe. Immutable ignored artifact artifacts/OneOffCompletionProbe-0.1.0/OneOffCompletionProbe-0.1.0.dll, SHA25663eb7439581d5e4824765aaf8c1e7cc1bd3ae0b1ba3ecd79d0e78d212cdfe8e3, accompanied by build-identity.json.
- Native-reference Roslyn SDK6.0.406/C#7.3 deterministic optimized warnings-as-errors compilation passes. Metadata-only VerifySeams verifies all eight exact target signatures and public owner/state on pinned host MVID6f50b8e7-156b-49ac-bbe8-7505894b2364, assembly SHA256e72e4270e4b88dd0a87ca23c9cf1750aec4c4a0fedb40b6d2dae7902fc9c7fd8. This is a local research build, not hosted CI or live Harmony activation proof. Package contains only probe DLL and identity receipt; no proprietary/reference DLLs.
- Installed at2026-10-17T18:30:03Z with game closed; installed digest/version/source verified. Unchanged production0.1.20 DLL hash e45dd14fa6a301e7d887811a7008aa107edca1e5bc7bafdd0cb71839d9396058 verified before/after. Old placement probe remains absent. Private .local/evidence/oneoff-probe-install/installation.json owns machine paths/time; prior log snapshot SHA256c4adec9b00a3f276046d462cd0560137f32d24f65ba8d3171b3d27919aca9b7c was preserved. No game launch, operation or activation claim by the agent.
- Next native pass: one naturally available repair, clearing and one-time builder upgrade, with exact original-owner/definition, start/spend, matching completion, replacement/script and exception/end correlation; one rejection/menu exit or naturally available true cancellation and an unrelated goal/craft. Pausing manual work is not cancellation. Probe never mutates planner/game state; actual player actions have ordinary native save consequences. One-off goals intentionally remain pinned during observation. Tools collect the local log; remove the sidecar after evidence capture before production candidate testing.

## One-off probe returned repair/clearing evidence — 2026-10-17

User reports checking repair and clearing and asks whether production removal is now justified. Preserved .local/evidence/oneoff-probe-return/LogOutput.log SHA25646a009f123d961e242980753ce1261c14daab6381cb8ef2a88f408da2fe8852d; exact installed probe SHA25663eb7439581d5e4824765aaf8c1e7cc1bd3ae0b1ba3ecd79d0e78d212cdfe8e3 and0.1.20 SHA256e45dd14fa6a301e7d887811a7008aa107edca1e5bc7bafdd0cb71839d9396058 match. Ready marker/MVID match; no observer_error. Host is closed; snapshot ends at main-menu/loading-hide, without an asserted shutdown marker.

Repair fix_morgue_builddesk, original morgue_builddesk_broken UID183: seq1 successful exact spend/start; later seq2 normal completion, one successful replacement, one successful matching OnFinishedCraft, one End, cleared craft. Clearing steep_yellow_blockage_С_destruction, original steep_yellow_blockage UID2609: seq3 successful exact spend/start; later seq4 target0 replacement plus native DestroyMe, one successful matching completion, one End, cleared craft. Unity owner remains alive with original ID at return, so immediate destruction/null or obj_id inequality must not be required. Native success correlation is READY for these two world-change cases; actual production removal remains absent/unaccepted. Upgrade, cancellation and rejection are not exercised here. Repair pin was removed manually after work; performed clearing was unpinned (different from initial pinned blockage), so no actual clearing-goal removal proof is asserted. See ONEOFF_COMPLETION_RESEARCH.md for exact sequence, limits and smallest proposed production predicate.

Four earlier NullReferenceException stacks go through PlannerRowRedrawPatch.Prefix/GetComponent when Max Buttons Redux calls CraftItemGUI.Redraw for maximum amount via gamepad. This is a separate open row-lifecycle/compatibility defect; underlying invalid component cause is not proved. No completion-observer exception is present. Probe remains installed for the still-open upgrade observation; no DLL/build/production code changed in this log review.

## 0.1.21 installed world-completion/redraw candidate —2026-10-17

User explicitly requests automatic repair/clearing removal and Max Buttons Redux error investigation; one-time builder upgrades stay separate. Two READY checkpoints preceded production mutation; ONEOFF_COMPLETION_RESEARCH.md owns evidence, gates and invariants.

- Frozen source25fd3181091492b20735d52e1b2ccc16c09a7405, dev/0.1.21; successful CI [39061707706](https://github.com/NikichMods/Crafting-Planner/actions/runs/39061707706), artifact12111662358. ZIP SHA2564b5d445e78b4fb92197d221793ce5fc5d8318595fd00b37b8c708ba2234cb166.
- Exact downloaded/installed DLL SHA25690177d2662daf51eb87858884ea8f93878b3cd9d90d993c1b52006fd7ed102da, version0.1.21.0, calibration/F2 format10 unchanged. A separate scoped completion tracker removes the matching existing nonrepeatable WorldChange goal after normal native completion with one successful matching OnFinishedCraft. It captures original key before replacement/clearing and refreshes existing consumers after return. Builder ObjectCraftDefinition upgrades, ordinary production and unsupported needs are excluded. Repeatable tracker and visual defaults unchanged.
- Redraw prefix now checks Unity existence before GetComponent, matching existing Ensure behavior. It does not skip native Redraw or alter Max Buttons. Exact installed Max Buttons DLL/control path is documented; the original invalid-reference source remains unproved.
- Both native-reference modes compile with warnings as errors. Native signature verification and clean hosted source/version/build/state/geometry/localization/package boundaries pass. New state tests cover world key/context/owner/definition, production/upgrade exclusion, absent/failed/duplicate/wrong markers, native exception, once-only removal, stale re-added goal and exact aggregate reduction. Existing placement/cap/visual policies pass. Exact downloaded candidate passes these tests against installed host metadata. Native-only tests execute the actual redraw prefix for CLR-null and detached Unity row without native GameObject creation; both pass. Full native/postfix input behavior is not proved outside Unity.
- Installed at2026-10-17T19:17:40Z with host closed, previous0.1.20 digest matched/backed up along with log; exact version/source/archive/DLL and installed digest checked. OneOffCompletionProbe0.1.0 hash matched, was backed up outside plugin loading and removed after new payload verification. Private backups/receipt are under .local/installation-backups/pre-0.1.21/ and .local/evidence/pre-0.1.21/. No main/stable promotion or game launch by the agent.
- Required runtime: pin an available unfinished repair and clearing before native work; each disappears only on successful completion (event=goal_completed), with materials refreshed and another pinned goal unchanged. Menu exit/no-start/resource refusal preserve the goal. Repeat maximum/minimum amounts via Max Buttons gamepad in ordinary crafting, close/reopen and switch technology tabs/return as in the old stacks; verify no new redraw-prefix exception. Upgrades intentionally remain pending. Tools collect the local log; no passive probe replay needed for these closed native facts.

## 0.1.21 returned removal pass and remaining Max Buttons errors —2026-10-17

User reports exercising repair/clearing and asks whether goals remove and redraw errors disappear. Exact installed0.1.21 SHA25690177d2662daf51eb87858884ea8f93878b3cd9d90d993c1b52006fd7ed102da matches frozen source25fd3181091492b20735d52e1b2ccc16c09a7405. Snapshot .local/evidence/return-0.1.21/LogOutput.log SHA2563972a147f116c7fc1d3a735a71c84894084ab2e2298b94ad762d970659b7fc26 loads0.1.21 with placement_tracking/world_completion_tracking ready. No completion disable is present. Host is closed at collection; log ends at main-menu/loading-hide without a claimed shutdown marker. No new DLL/install/source behavior change in this review.

- Clearing was pinned at line1186. Native target0 replacement/DestroyMe and Craft end precede exactly one goal_completed at2382: key2|steep_yellow_blockage|steep_yellow_blockage_С_destruction, quantity0, UID2611. No manual removal intervenes. With the plan empty the HUD becomes invisible after returning to gameplay. This now proves actual pinned clearing removal, unlike the earlier passive unpinned observation.
- After session reset3332, fix_morgue_throw_in is pinned1 at3434, then fix_morgue_builddesk1 at3514. Native table replacement/end precedes exactly one goal_completed at3970 for2|morgue_builddesk_broken|fix_morgue_builddesk quantity0 UID183. There is no goal adjustment/completion for the other repair, and HUD remains visible. Exact goal removal and unrelated-goal preservation pass in this case.
- Exactly2 automatic completion events, each for its correct original owner/craft and0 quantity; no duplicates or unrelated automatic completion found. Goals persist through earlier menu visits/material collection before actual completion. True cancellation/failed native completion and all material display values are not newly instrumented/proved by this log; compiled state checks remain separate. Builder upgrades stay excluded. This is a bounded production-removal pass, not full release acceptance.
- Max Buttons compatibility does NOT pass.9 NullReferenceException stacks at1642/1722/1757/1796/1851/1897/1959/1998/2026 enter the dynamic CraftItemGUI.Redraw wrapper via MaxButtonsRedux.SetAmount -> SetMaximumAmount -> HandleCraftGuiInput -> HandleGamepadInput. No PlannerRowRedrawPatch.Prefix/GetComponent frame remains. Thus the old explicit planner-prefix symptom is absent, but the overall failure persists. A dynamic wrapper stack does not identify the exact original/patch instruction; do not claim every remaining error is solely vanilla or that compatibility is fixed.
- All9 errors occur with TechTreeGUI open (1610..2040), after CraftGUI hidden1424, and correlate to technology tab changes. Direct pinned Max Buttons DLL inspection (.local/maxbuttons-menu-lifecycle.txt) verifies HandleGamepadInput gates craft handling on its cached _craftGuiOpen plus _unsafeInteraction; CraftGUI_Open sets the flag true and CraftGUI_OnClosePressed sets false. The inspected hooks do not track every BaseGUI.Hide path or actual top window. Combined with the observed call stack, this establishes max/min craft input running outside the craft surface; stale cached open/row state is the supported explanation to investigate next, not proof of the exact null member. No private Max Buttons state patch, native redraw suppression or third-party binary change is made.

## Yard upgrade setup0.1.0 plus existing observer installed —2026-10-17

User has an unupgraded save but gathering the yard-expansion resources is impractical; explicitly authorizes preparation such as a resource button. Static-first lookup finds authored craft_obj_data mf_wood_builddesk::upgrade_player_buildzone: one_time_craft=true, None, wait_script_callback=false, is_auto=true, zero craft time, end_script=player_buildzone_upgrade, empty change_wgo; needs detail_2:10/wooden_plank:8/nails:4. Exact data/IL remain private; ONEOFF_COMPLETION_RESEARCH.md owns the method choice and save-safety/cleanup contract. Static routing is not claimed as live upgrade acceptance.

- Frozen setup source5e14e3dab2ac112d83fdd39aefa7fb22d65ba5b4, branch research/yard-upgrade-setup. YardUpgradeSetup0.1.0/assembly0.1.0.0, GUID nikich.gyk.yardupgradesetup. Ignored immutable artifacts/YardUpgradeSetup-0.1.0/YardUpgradeSetup-0.1.0.dll SHA256051fcb8a4202a32b692f3b7a92fc289d11939670bc42b74c02f82b227fd41e30 with build-identity.json. Roslyn SDK6.0.406/native1.407 references/C#7.3/deterministic optimized/warnings-as-errors compilation passes. No hosted CI or live F8/grant/cleanup acceptance asserted; research package contains only helper DLL and identity, not host/reference payloads.
- Unchanged exact OneOffCompletionProbe0.1.0 is reused: source8c82c7dbe93b0018ab97cb1c1d594402fa759205, SHA25663eb7439581d5e4824765aaf8c1e7cc1bd3ae0b1ba3ecd79d0e78d212cdfe8e3. Its eight metadata seams pass again on host MVID6f50b8e7-156b-49ac-bbe8-7505894b2364. The new setup helper has no Harmony patch or completion invocation; native capacity-checked resource addition/removal signatures and GameBalance lookup/save completion state are statically verified. No copied reference stubs or production source/version/defaults change.
- Both research sidecars installed at2026-10-17T19:45:49Z with host closed, exact source/version/hash checks and installed digests verified. Production0.1.21 remains SHA25690177d2662daf51eb87858884ea8f93878b3cd9d90d993c1b52006fd7ed102da. Prior log SHA2563972a147f116c7fc1d3a735a71c84894084ab2e2298b94ad762d970659b7fc26 preserved under .local/evidence/yard-setup-install/; installation.json owns machine paths/time. No tools launched the game or changed inventory/save data.
- Next user action: load the available unupgraded test save; F8 -> explicit missing-resource button -> close F8 -> normal home builder menu -> pin yard expansion -> ordinary upgrade action. Setup grants only missing carried quantities using native capacity and logs before/after; full inventory refuses without unchecked injection. Undo button before upgrade verifies same save/player and unchanged relevant post-grant counts before removing only its recorded added amounts. Otherwise use the pre-test save for rollback. Actual resources/world upgrade can be saved; panel explicitly explains this. Existing0.1.21 deliberately leaves the builder-upgrade goal pinned. No manual resource gathering, repeatable/repair/clearing test or Max Buttons investigation is requested. Tools will collect the local log and remove both sidecars after observation/cleanup.

## Yard upgrade returned success correlation and research cleanup —2026-10-18

User completed the normal yard upgrade with helper resources. Snapshot .local/evidence/yard-upgrade-return/LogOutput.log SHA2561d696f78b8b921e694dd353e392d078a8d587cb75786fc71e2ee0c1fafe94ad1 matches exact loaded probe0.1.0, setup0.1.0 and unchanged0.1.21 identities recorded above. No observer/setup error, native exception in this chain or tracker disable. Host closed at collection; tail is ordinary NPC activity, not a shutdown marker.

Pinned yard upgrade1; helper grants exactly missing10 detail_2/8 wooden_plank/4 nails from carried0, with verified after counts. Native builder dispatch and component owner both mf_wood_builddesk UID182, same exact recipe. Start seq1 has one successful native spend, then nested finish seq2(parent1) starts the yard script, records one successful matching OnFinishedCraft and End, returns normally with current craft cleared/is_crafting=false. Outer start returns true afterwards; no replacement, same owner. Script termination follows the native craft return. See ONEOFF_COMPLETION_RESEARCH.md for exact line references, bounded readiness and applicability. Production upgrade removal is still absent in0.1.21; this is not acceptance of automatic deletion, arbitrary upgrades or missing negative scenarios.

Observed builder completion boundary is READY for production implementation; no new research probe is needed just to recover it. Existing scope/receipt/goal-key logic can be extended to the verified builder family with separate compiled/native acceptance. Setup undo/full-inventory/refusal paths were not exercised. Shared native facts promoted to GraveyardKeeperResearch7b8fbdfc318b5681ca36f28a7cead7c4cffa0e84.

Both temporary DLLs were exact-hash checked, backed up outside loading and removed with game closed at2026-10-18T00:26:53Z. Private .local/evidence/yard-upgrade-return/cleanup.json and .local/installation-backups/post-yard-upgrade/ own receipt/payloads. Production remains0.1.21 SHA25690177d2662daf51eb87858884ea8f93878b3cd9d90d993c1b52006fd7ed102da. No game launch, inventory/save edit, new production build or stable promotion by the agent. Max Buttons remains deferred.

## 0.1.22 installed one-time builder completion candidate —2026-10-18

User requests automatic yard-upgrade removal and checking/adding similar improvements. The full static population and two independent READY gates are in ONEOFF_COMPLETION_RESEARCH.md. No copied recipe table, native definition mutation, new Harmony seam, visual tuning or Max Buttons change.

- Frozen sourcec5a48b283d5bf3a572ac49feb8abd8b968d65093, dev/0.1.22; successful CI [39083281426](https://github.com/NikichMods/Crafting-Planner/actions/runs/39083281426), artifact12119389208. ZIP SHA2564098c91a8af03c7539421b54e036a8c040cfcc0941469838b30b718956638548.
- Exact downloaded/installed DLL SHA256e26b74274b9a7157aab9e111442506abcfc117dcbfbacfe257a95e3c6ef2bdec, version0.1.22.0, calibration/F2 format10 unchanged. Existing scoped completion includes matching nonrepeatable Builder None/one-time/auto/no-wait/end_script/plain-needs goals; their1 removes to0 through the existing receipt. One-time Put goals now match placement instead of being excluded and use the same successful spend/built transition as repeatables. Their later craft completion cannot double-consume a builder Put goal. World repair/clearing and repeatable semantics are preserved.
- Both native-reference modes compile with warnings as errors; clean hosted build/source/version/state/geometry/localization/package-boundary checks pass. New builder tests exercise actual hook router, owner/context isolation, unsupported/manual/scriptless/wait-callback/Put/repeatable exclusion, failed/missing/duplicate/native-error marker, exact goal removal and unrelated requirements. Placement tests add one-time rejection/success/once-only removal. Exact downloaded DLL also passes all existing/new tests against actual host metadata, plus actual null/detached redraw-prefix checks.
- Private authored projection .local/builder-upgrade-cases.tsv SHA256b01da97a624c2e79b4bd5e3ce337825f62a0557a302d6b6043372daf59e696c7 exercises compiled dispatch of all57 one-time definitions:39 upgrades,17 placements,1 no-needs exclusion. It is not shipped/committed or57 live acceptances. Separate audit of85 None records finds45 non-one-time actions, including callback-driven home furniture/decor; these are not equated with the instant-upgrade family. Native facts promoted to shared researche8eabd8f1eb812eb49de0a6296b1ecd5ed0c32bd.
- Installed with host closed at2026-10-18T00:36:54Z after exact source/version/archive/DLL identity checks; previous0.1.21 hash matched and was backed up outside loading with its log/receipt under .local/installation-backups/pre-0.1.22/ and .local/evidence/pre-0.1.22/. Exact F8 YardUpgradeSetup0.1.0 SHA256051fcb8a4202a32b692f3b7a92fc289d11939670bc42b74c02f82b227fd41e30 is restored for convenient acceptance on a not-yet-upgraded save; normal save consequences/guarded undo contract remain. Passive probe stays absent. No game launch or stable/main promotion by the agent.
- Required native acceptance: pin yard or another equivalent action and a control goal, perform normally, verify one goal_completed and disappearance/material refresh with control preserved. If available one-time Put/totem verifies one goal_placed/removal and unchanged goal on rejection/cancel. No57-action manual matrix or closed repair/visual replay is required. Callback furniture placement remains a separate unclosed boundary and Max Buttons remains deferred. Tools collect log/clean up temporary resource helper afterwards.


## 0.1.22 returned yard completion —2026-10-18

User reports testing the yard upgrade on the new candidate. Installed DLL still matches frozen source c5a48b283d5bf3a572ac49feb8abd8b968d65093 / SHA256 e26b74274b9a7157aab9e111442506abcfc117dcbfbacfe257a95e3c6ef2bdec. Private snapshot `.local/evidence/return-0.1.22-yard/LogOutput.log`, SHA256 `ade1306b0fdad9b09dedd2f5a3829815ea6e47f01a312f757e766f684755cd96`, loads0.1.22 with world_completion_tracking ready. No exception or tracker-disable marker is present in this snapshot.

- Line886 pins the one-off yard goal quantity1. Lines900–907 record explicit synthetic preparation: native helper grants exactly missing detail_2:10, wooden_plank:8 and nails:4, with verified after counts. This prepares resources only; it does not complete or remove the goal.
- Native yard action starts at924–925; script player_buildzone_upgrade starts937 and native Craft end938 precedes exactly one planner goal_completed at939: key1|mf_wood_builddesk|mf_wood_builddesk::upgrade_player_buildzone, quantity0, original object_uid182. No manual remove or second completion is logged. Script termination944 follows this committed craft boundary, as in the research observation.
- Returning from pause shows view=Hud visible=False at998, consistent with the sole goal having been removed. This does not prove immediate frame timing, visual material totals or unrelated-goal preservation: no control goal was pinned in this pass. Representative yard automatic removal is verified; other39-family members retain static/compiled coverage, and one-time Put native acceptance remains pending. Callback furniture and deferred Max Buttons ownership remain open.

With host closed, exact F8 helper was backed up outside loading and removed at2026-10-18T08:29:07Z. `.local/evidence/return-0.1.22-yard/cleanup.json` records helper absence, passive-probe absence and unchanged production digest; backup is under `.local/installation-backups/post-0.1.22-yard/`. No save/inventory edit or game launch by tools. This is bounded functional evidence, not main/stable release acceptance.

## 0.1.23 installed lamp quantities and one-time placement setup —2026-10-18

- Frozen production/helper source a98c3ac47efe61b3de3c7287a3fba26d59959fdf, dev/0.1.23. Clean CI [39112334329](https://github.com/NikichMods/Crafting-Planner/actions/runs/39112334329) success, artifact12130499837. ZIP SHA256c84e07195eb1f5f8b158f15ce54d258ec6ff7162974616cbd2b2393b5bd9a6cf. Downloaded/installed0.1.23.0 DLL SHA256b09065278ed8b2f8c4c048d57ae9503d6442ad7bb04ab61cdab10886bf4649f8, F2 calibration unchanged.
- Both native-reference modes and all hosted checks pass. Exact downloaded DLL passes existing/new state, native absent-row and57-row catalogue dispatch checks against actual installed metadata. New cases cover three verified lamp recipes, narrow owner/replacement/upgrade exclusions, cap20, completion20->19, requirements57 after19 at3 each, once-only and last-goal removal. Other repair1 semantics and failures remain covered. New lamp runtime behavior is not yet accepted.
- OneTimePlacementSetup0.1.0, GUIDnikich.gyk.onetimeplacementsetup, exact frozen DLL SHA256503b6d633274e62bc45c10c9c8733af86d7c15b4ec1a18a2b0236d2969b504bc is native-reference compiled separately from production and retained with build identity under ignored artifacts/OneTimePlacementSetup-0.1.0/. It selects actual highlighted one-time Put rows through verified mouse/gamepad focus callbacks; reads registered native needs, issues missing carried resources only after explicit F8 click, verifies counts and records save/player/ledger. Same-save/unchanged-count guarded undo precedes placement; saved grants/real placement may persist. No starts/completions/technology/definition/world/planner mutation. This selection/setup variant still needs its first live pass; reused native inventory primitive already has accepted yard evidence. Research DLL is excluded from production compilation/package.
- Installed core and helper with host closed at2026-10-18T08:42:44Z, verified hashes/versions; prior0.1.22 exact digest was backed up outside loading. Private .local/evidence/pre-0.1.23/installation.json and setup-installation.json own receipts, prior log retained there, backup under .local/installation-backups/pre-0.1.23/. Yard helper/passive observer remain absent. No game launch or stable/main promotion. Static shared note saved in research/lighting-points eebaafc; default-branch publication was rejected by automatic review, so shared main is unchanged.
- User reports receivers accessible but already built only in some locations. Native pass: retain control goal; at an accessible unbuilt receiver pin1/highlight/F8, confirm selection and issue missing resources, close helper, cancel first placement (no decrement), then place normally (one goal_placed quantity0 and control preserved). Capture actual resource grant and once-only native placement; do not equate placement commitment with later manual work completion. Lamp pass: each of three empty-point choices reaches20, arrow hides/restores at20/19, same type shares quantity at another point, one ordinary completed lamp decrements1. No twenty-build or57-action matrix. Remove temporary setup after return preservation; callback furniture and Max Buttons remain separately deferred/open.


## 0.1.23 returned receiver placement and light counters —2026-10-18

User reports completing the receiver/light checklist. Installed0.1.23 DLL matches frozen sourcea98c3ac47efe61b3de3c7287a3fba26d59959fdf / SHA256b09065278ed8b2f8c4c048d57ae9503d6442ad7bb04ab61cdab10886bf4649f8; exact helper digest also matches. Preserved private snapshot .local/evidence/return-0.1.23-placement-lights/LogOutput.log SHA2562d0dfa0df17ee3409bea953747f957ee1aaec979bfa864708d951a28c60cd053 loads0.1.23 and both trackers ready. Three session_reset entries840/1552/2098 mark separate loads; evidence must not bridge those goal sessions.

- Helper selection957 identifies morgue receiver. Native capacity refuses twice1011/1095; both later previews still show carried0 for all three needs. One explicit grant1166–1170 verifies detail_1:3/glass_1:1/wooden_plank:1. No setup_error/duplicate grant. Guarded undo was not exercised. These are real capacity-refusal/success observations for the adapted selector, with explicitly synthetic resources.
- Morgue native DoPlace1233/leave_on_scene=True1234 leads to exactly one goal_placed1255 quantity0/original UID214983. Its CancelPlacing1237 follows successful placement/native next-building resource refusal, so it is not the standalone negative cancellation test. Control alchemy stirrer903 and organ rack988 have no mutation before the next session reset1552. Later morgue pins/manual edits occur in another load and do not retroactively invalidate that receipt.
- Garden receiver pinned2268 quantity1 alongside two control goals2269/2270. Selection2494 enters placement; cancellation2542 with StopCurrentFloating leave_on_scene=False2543 emits no goal_placed/completed or goal adjustment. Re-entry2585 leads to DoPlace2621/leave_on_scene=True2622 and exactly one goal_placed2643 quantity0/original UID204297. No reset/re-pin/manual decrement between cancellation and success. Both control goals remain until explicitly manually removed3467/3468. Another later cancelled placement2704/2705 adds no completion. Representative one-time Put native cancellation, success, once-only removal and control preservation are verified across these cases; not17 individual native acceptances or later manual building-work completion.
- Lantern_2 reaches20 at3357, returns19 on menu decrement3358, is later manually removed3382. Lantern_3 reaches20 at3408 and returns19 through HUD decrement3448; user then manually reduces it to1 at3466. No quantity exceeds20. Several native lantern_place menus are opened before the successful native lantern_3 action3864/3865; replacement3898 and Craft end3899 precede exactly one goal_completed3900 quantity0/original UID204605. No duplicate or manual removal of this goal after3466; later HUD visibleFalse3954/3969 agrees with empty plan. This verifies real lamp final1->0, not automatic20->19 or2->1 in this session. No lantern_1 goal adjustment is logged. Arrow visibility/readback at another point are not directly instrumented; user reports following the checklist, while logs independently establish the counter transitions/completion above. Compiled tests cover all three types/nonzero decrement/cap/requirements; extra native nonzero remaining quantity would be a bounded optional2->1 check, requiring no probe/new build.

No Exception, planner disabled_for_process or setup_error marker is found. Other Unity Error entries (missing assets/animators/sprites/idle points/lights) exist; this pass does not diagnose their ownership or claim a globally error-free game. Deferred Max Buttons investigation is not reopened.

With host closed, exact helper was backed up outside loading and removed at2026-10-18T09:06:35Z. Private cleanup.json and .local/installation-backups/post-0.1.23-placement-lights/ preserve receipt/payload; production digest unchanged. No save/inventory mutation or game launch by tools. Native acceptance is bounded as above; no main/stable promotion or full language/gamepad release acceptance.

User decision2026-10-18: skip the optional dedicated lamp2->1 native replay; user accepts proceeding and will report any gameplay issue. This is a deliberate test waiver, not newly observed nonzero native decrement. Do not treat that replay as a blocker or request it again without new evidence. Callback-driven home furniture remains a separate open mechanism; no furniture implementation/product decision is adopted by this planning question.

## Home interior callback research installed —2026-10-18

User authorizes static home furniture/interior completion/cancellation investigation and a native check if required. INTERIOR_COMPLETION_RESEARCH.md owns the39-definition/five-graph static evidence, BLOCKED production gate and method choice. No production source or version change; installed0.1.23 frozen sourcea98c3ac47efe61b3de3c7287a3fba26d59959fdf / DLLb09065278ed8b2f8c4c048d57ae9503d6442ad7bb04ab61cdab10886bf4649f8 remains exact.

- Frozen research source80db85c7e283959a227d4e2f13c49fe320a588fa on research/interior-completion. InteriorCompletionProbe0.1.0 GUIDnikich.gyk.interiorcompletionprobe, DLL SHA25634f930a31ddbe07cc5750e267e9b0edfc7f658032095ffad4eb068627bb63ccb. InteriorResourceSetup0.1.0 GUIDnikich.gyk.interiorresourcesetup, DLL SHA2562c012e963a4d30fe101cd4d5345e272cf8559a79e82737cc92646f50866d2246. Both artifacts/build-identity.json retained privately under artifacts/<name>-0.1.0/. Native-reference C#7.3 deterministic release/warnings-as-errors build passes with SDK6.0.406 csc; separate research payloads, not production CI/distribution.
- Exact artifacts pass metadata verification for11 native methods/2 public static Action events; actual compiled observer/setup selectors accept all39 native callback rows and reject no-callback/one-time flag variants. Private .local/interior-cases.tsv/.local/verify-interior.* retain original verifier/local projection. This is structural/filter evidence, not successful Harmony activation, event correlation, refund or UI acceptance.
- Installed both with host closed at2026-10-18T09:19:33Z after hashes/versions/destination checks, no previous same-plugin DLLs present. Core digest unchanged before/after. Private .local/evidence/interior-research-install/installation.json and LogOutput-before.log preserve receipt/log. Prior placement/yard sidecars remain absent. No game launch/save edit/craft execution by tools; no main/stable promotion.
- Native pass: retain control goal, pick accessible home kitchen-table variant different from current (fallback cupboard/bed/finish), pin1/highlight, F8 explicit missing-resource grant. Begin preview and Cancel; wait for camera/control and former furniture restoration. Repeat same variant and Apply; wait for camera/control and chosen furniture retention. Automatic goal removal for this callback family is intentionally absent0.1.23; goal/control should remain throughout this research. Probe records early native spend/finish, separate decision/normal return, script identity/termination, relevant room mutations and carried counts. One grant per helper session; normal cancellation refund should make the same materials available for the second attempt, subject to actual native behavior. No per-frame inventory/world scans or synthetic completion. Extra family/rotation is optional. Capture returned log and remove both sidecars afterwards. Resource grants/real interior changes can be saved; use pre-test-save reload for full rollback, or guarded ledger undo only with unchanged relevant counts/same save/player before successful spend.


## Returned home interior observer —2026-10-18

User reports completing the native test. Installed production0.1.23 SHA256b09065278ed8b2f8c4c048d57ae9503d6442ad7bb04ab61cdab10886bf4649f8 and both frozen80db85c7 research sidecars match installed hashes. Snapshot .local/evidence/interior-research-return/LogOutput.log SHA256752d0cf821bdac65465fc797ca81db013e080929bd71d005725393a4241e435d; INTERIOR_COMPLETION_RESEARCH.md owns the7-attempt/line-indexed observation and READY boundary gate.

7 attempts, original keeper builder UID3444: cooking_table_2 cancel/apply, cooking_table cancel, cooking_table_2 cancel, cupboard cancel/apply, cupboard_2 apply. Each has1 successful native spend,1 normal early finish/save marker,1 decision,1 normal associated-script termination and1 normal decision-handler return. Four cancels restore exact prior carried counts; three applies retain spent counts. Compound table/stove-stand IDs restore on real change/cancel; cupboard preview old/new UID destruction and restoration observed. Same-ID native applies also occur and must not be filtered by object-ID inequality. In these cases script termination precedes normal handler return; broader asynchronous coverage is not asserted. No observer/setup exception/disable; helper has no grant events, so no synthetic issuance is claimed. Production intentionally retains callback goals; later manual removals are not automatic-removal evidence.39 authored variants have static/filter coverage;2 script families exercised live, not all39 variants.

Both sidecars verified/backed up and removed with host closed2026-10-18T09:28:19Z; .local/evidence/interior-research-return/cleanup.json and .local/installation-backups/post-interior-research/ retain receipt/payloads. Core unchanged, no agent-triggered game launch or save/inventory edit. Shared facts saved in research/interior-completion a3079a28ef7d24379479165d6038929917cc5f34, no shared main change. Next is production implementation/testing of the verified callback boundary and exact candidate goal-removal acceptance, not another discovery probe. No new binary/main/stable promotion here; Max Buttons deferred, optional lamp replay declined.

## 0.1.24 installed confirmed home interior removal candidate —2026-10-18

User explicitly authorizes Apply removal/Cancel retention, build and installation. READY selection/implementation gate is in INTERIOR_COMPLETION_RESEARCH.md; existing native7-attempt evidence is reused. No new discovery probe, room mutation, copied recipe data, native inventory/refund rules, visual/default or quantity-policy change.

- Frozen source09f4febf8588fb18a7c9aada3fff5f836ab8787a, dev/0.1.24. Clean successful CI [39116068017](https://github.com/NikichMods/Crafting-Planner/actions/runs/39116068017), artifact12132217648. ZIP SHA256fa723e3583f28a3a02df866a8e29a9571735c7bcd1c4e8a779567c3a08d86e27; downloaded/installed0.1.24.0 DLL SHA256d38cafe37f7ab810e655b1c71cd00042b533ea6a74243e203766ed9ac2094330. F2 calibration/defaults unchanged.
- New dedicated interior integration uses6 verified seams/public decision events, original goal/definition/save/builder UID/script reference, neutral nested early-finish scope and once-only normal latches. Apply/termination/normal decision return may arrive in either order. Early finish alone/Cancel/rotation/errors/missing/duplicate/wrong script/stale membership cannot remove goal; successful receipt adjusts1->0 and defers existing redraw. New dispatch/reset/stop invalidates pending state; failures disable only interior observation and preserve native exceptions. No per-frame inventory/world scan or unchanged-frame logging allocation. Existing placement/world/yard/light handlers remain separate.
- Both native-reference configurations pass. Clean CI identity/build/state/geometry/localization/package boundaries pass; exact downloaded DLL also passes against actual host metadata, including real absent-row prefix, existing57 authored dispatch checks and new39 callback identity/script dispatch checks. New state tests exercise actual router/five verified script families and payload/flag exclusions, preview retention, cancellation/native errors/missing and duplicate events, script identity, handler return, delayed termination, once-only/control/derived requirements, stale re-add and reset. Separate fresh actual balance assertion confirms all39 positive plain-needs/no multiquality/no needs_from_wgo and exact owner/flags. These checks do not establish new candidate native goal removal.
- Installed with host closed2026-10-18T09:40:37Z after source/version/archive/hash verification; previous0.1.23 digestb09065278ed8b2f8c4c048d57ae9503d6442ad7bb04ab61cdab10886bf4649f8 verified/backed up outside loading with its log. Private .local/evidence/pre-0.1.24/installation.json and .local/installation-backups/pre-0.1.24/ own receipts/backup. Existing immutable InteriorResourceSetup0.1.0 (source80db85c7e283959a227d4e2f13c49fe320a588fa, DLL2c012e963a4d30fe101cd4d5345e272cf8559a79e82737cc92646f50866d2246) restored as separate temporary F8 resource helper; setup-installation.json records exact digest. Passive InteriorCompletionProbe stays absent. No game launch/save edit/main/stable promotion.
- Required native pass: pin1 of an accessible home variant plus control goal, highlight and F8 grant only if needed; preview must retain goal, Cancel/wait must retain it/control, repeat Apply/wait must emit exactly one goal_completed family=interior quantity0 and preserve control/refresh requirements. Same-ID valid applies are supported; no39-variant replay. Resource/native changes may be saved; prior guarded undo/pre-test-save contract remains. Capture return and remove helper. Max Buttons stays deferred; dedicated lamp replay remains declined. New0.1.24 native acceptance is pending.

## 0.1.24 first return: ordinary home plant placement only —2026-10-18

User reports completion. Exact installed core0.1.24 d38cafe37f7ab810e655b1c71cd00042b533ea6a74243e203766ed9ac2094330 and restored helper2c012e963a4d30fe101cd4d5345e272cf8559a79e82737cc92646f50866d2246 match. Preserved snapshot .local/evidence/return-0.1.24-first/LogOutput.log SHA25663c6611463b2395e8b8223c9d7b39263edeacab4bbd69305d34a55a49ed909ea. Loader0.1.24/interior tracker ready105–106, no Exception/disabled/setup error found. Host closed at collection; log reaches title screen.

Plant keeper_room_builddesk:p:keeper_room_plant_red_1 is pinned974 quantity1 repeatableTrue. Native Put selection1377/1378, genuine cancellation1408/leave_on_sceneFalse1411 emits no completion/goal adjustment. Second selection1452/1453, DoPlace1480/leaveTrue1481 yields exactly one goal_placed1502 quantity0/original UID204105. Pending cupboard_2 goal995 is not removed; no later session reset or manual mutation before return. This is successful native regression evidence for ordinary home Put cancellation/placement/control isolation, not acceptance of the new callback interior removal.

Only plant native OnCraft/CraftBuilding operations appear in this snapshot. Cupboard base is pinned992/manually removed993; cupboard_2 pinned995, but no native None/keeper_table_place/keeper_cupboard_place dispatch or goal_completed family=interior appears. New interior path therefore remains unexercised, with no observed defect established. Helper has selections only, no grant events. Clarified next action: use an actual cupboard/kitchen-table variant (not a freely placeable potted plant), pin/control/highlight/F8 as needed, preview Cancel retention then Apply removal. Existing0.1.24 and exact F8 helper remain installed for this pending check; no cleanup/new build/probe/game launch is needed yet. Keep prior accepted placement/visual/light evidence and deferred Max Buttons/lamp replay decisions unchanged.


## 0.1.24 confirmed cupboard callback removal —2026-10-18

User explicitly reports testing cupboard Cancel then Apply. Exact installed core0.1.24 SHA256d38cafe37f7ab810e655b1c71cd00042b533ea6a74243e203766ed9ac2094330/frozen09f4febf8588fb18a7c9aada3fff5f836ab8787a and helper2c012e963a4d30fe101cd4d5345e272cf8559a79e82737cc92646f50866d2246 match. Snapshot .local/evidence/return-0.1.24-interior/LogOutput.log SHA2569dabdc36f7b7f089f2ac1b7f8781c2fd4b4bf24f0146a28c99a09cb238884b5e loads0.1.24/interior tracker ready105–106; no Exception/disabled_for_process/setup_error marker found.

- Control carpet goals pinned940/941, cupboard_2 quantity1/nonrepeatable pinned946. One explicit native helper grant950–954 verifies missing wooden_plank8/nails6/detail_2:4 from carried0. Synthetic resource setup is explicit and separate from native player action.
- First native None dispatch985/986 fires keeper_cupboard_place:place:cupboard_2 at1005/1006. Preview spawns cupboard_2/destroys cupboard_home; cancellation restores cupboard_home/destroys preview and terminates script1036. No goal_completed, manual goal decrement/re-add or session reset occurs before the second dispatch1087/1088. Thus original captured goal remains1 across Cancel and the next attempt.
- Second dispatch fires the same family/payload1107/1108; Apply keeps cupboard_2, terminates script1135 and returns from build mode. Exactly one goal_completed family=interior1146 removes the original key1|keeper_room_builddesk|keeper_room_builddesk::cupboard_2 to0, object_uid3444. Neither early Craft end nor first script termination removes the goal. Both carpet controls have no subsequent mutation and HUD remains visible. No duplicate removal. Actual rendered material totals/full localization/gamepad matrix are not asserted from this log.

This closes representative0.1.24 native callback Cancel retention/Apply removal/once-only/control isolation. Other39-family variants retain static/compiled coverage and prior table/cupboard native boundary research, not39 individual production native passes. General native multiple-object/NodeCanvas/physics warnings exist; their ownership is not diagnosed and a globally warning-free runtime is not claimed. Max Buttons remains deferred, lamp replay declined, no main/stable acceptance/promotion.

At2026-10-18T09:49:05Z with host closed, exact F8 helper was verified/backed up outside loading and removed. Private cleanup.json and .local/installation-backups/post-0.1.24-interior/ preserve receipt/payload; core remains unchanged0.1.24. No game launch/save/inventory edits by tools. Synthetic grants/actual native interior changes retain the previously disclosed save consequences; cleanup removes only the helper DLL.

## 2026-10-18 — 0.1.25 per-save plan persistence candidate

- User authorizes saving goal identities/quantities per save, recovery after load/restart, new build and installation. PLAN_PERSISTENCE.md records the READY gate before production mutation and the mechanism/limits. No probe/resource helper is installed.
- Native1.407 static evidence: SaveGameDataToSlot is the private static native disk writer; ToBinary captures the serialized snapshot within its exact invocation. Disk exceptions are caught internally; publication requires matching serialized/disk SHA256 and native-file creation/timestamp change. FromBinary binds input hash to the exact returned GameSave; native load assigns save_slot before invoking OnGameLoaded, and existing OnGameStartedPlaying restores after old completion/goal state is cleared. No native file or inventory payload is modified.
- Local mod-owned format1 snapshots retain only ordered keys/quantities, isolated by full native slot path hash and binary version hash. Resolve current native definitions/materials and repeatability; invalid data fails closed, unsupported/corrupt snapshots remain on disk. Empty snapshots preserve removals/completion. Unsaved plan changes follow native save semantics. Byte-identical native files share their latest plan; copied/renamed slots and cloud transfer do not automatically carry sidecars.
- Both native-reference compiler modes pass. State tests add ordered/Unicode key and quantity roundtrip, empty/deleted plan, malformed/schema/duplicate/quantity rejection, current definition and supported-shape resolution, one-off bound, successful/failed disk-commit predicates and real atomic per-slot/version storage/overwrite. An initial restricted-environment File.Replace denial is resolved by running the same unchanged tests with filesystem permissions; no code workaround was introduced.
- Frozen source90c5bae9fc5474a0f9d867fc152dd5a98a61861f on dev/0.1.25; clean CI39118320369 succeeds; artifact12133380936. Exact downloaded DLL SHA2560922e8d7c27699cbe8483c2ee6c158034c105916df256070011609c49f31b1e2, ZIP SHA256cc1e505bc5c02f168af4f0432af3439b25143f6fcbb3bcc42912fbafc8125f81. CI version/source/package-boundary checks pass; temporary calibration remains enabled.
- Exact downloaded DLL passes state/persistence/native absent-row checks and all57 authored one-time builder plus39 callback dispatch cases against actual native references. These are compiled-policy checks, not repeated native operation acceptances.
- Installed exact CI bytes at2026-10-18T10:14:23Z with host closed; installed assembly/file0.1.25.0 and digest verified, prior0.1.24 backed up outside plugins. Private receipt .local/evidence/pre-0.1.25/installation.json; backup .local/installation-backups/pre-0.1.25/. No additional planner payload/helper is present.
- Native acceptance pending: ordinary save/reload with quantities, full restart, two-slot isolation, manual/automatic removal then save/reload, empty-plan persistence. Optional unsaved edit rollback is a direct control. plan_persistence ready, plan_save/restore slot/snapshot and plan_restored_goal key/quantity markers support the return. Prior accepted visuals and completion policies remain unchanged; main/stable promotion not performed.

## 2026-10-19 — 0.1.25 persistence return accepted

User reports every requested save/load/restart/two-slot/removal/empty scenario passed, explicitly including unsaved plan changes. Exact installed DLL SHA2560922e8d7c27699cbe8483c2ee6c158034c105916df256070011609c49f31b1e2 matches CI39118320369/artifact12133380936/source90c5bae9fc5474a0f9d867fc152dd5a98a61861f. Loader0.1.25 and plan_persistence ready schema1 are present.

Preserved current log .local/evidence/return-0.1.25-persistence/LogOutput.log SHA256f42d10c98562044080bc5e2fad4176df71625ee8a0c2298a27f0bba382240cc7;8 own snapshot files across2 slot directories are copied in the same evidence directory. It is the surviving post-restart session, not all earlier process logs.6 native-verified save receipts,7 restored snapshots (all skipped0) and1 no-snapshot load occur. Slot384f7c598c30 initially restores three goals3/1/1, then saves/restores one goal1 and then0. Slot0da2c5c38925 starts without snapshot, saves/restores0, then saves two alchemy build projects1/1; switching back preserves each slot's own state. Final unsaved increments to2/3 have no intervening plan_save, and reloading identical snapshot04bd58ff0285 restores1/1. Empty files contain only format header; own saved files match logged versions. No planner persistence error/disable/exception is found; unrelated Unity World Object not found warnings are not attributed to this feature without evidence.

Full process restart and all scenarios are user-confirmed; this snapshot independently corroborates loaded persisted data and the above paths. No goal_completed marker occurs here, so do not call automatic-completion-then-save a separately traced new native completion; no forced I/O-failure/one-off-specific/universal compatibility acceptance follows. PLAN_PERSISTENCE.md owns accepted semantics and limitations. No new DLL or stable promotion. Next milestone is focused release readiness: remaining carried-bag/capacity/gamepad/language cases, deferred Max Buttons cause/compatibility, and public F2 calibration decision. Reusable native save lifecycle facts are promoted to shared research, while planner product acceptance remains here.

## 2026-10-19 — passive Max Buttons attribution probe installed

User reopens deferred compatibility question. Production core remains exact accepted0.1.25; third-party Max Buttons1.4.0 remains exact hash e811fc228ece102e389759d278bb66569884898b827369f99e11c984ffc2af30. Shared GraveyardKeeperResearch research/max-buttons-redraw owns investigation and frozen observer sourcec3777c192e7bbe07becf71d6c95eeedec2f400ba. New diagnostic sidecar0.1.0 SHA256d82e8071958493ef31ec5877333dc5b4b9b4ead2a824698e8a290a88cd681f38 installed at2026-10-18T21:15:17Z, host closed; baseline DLLs unchanged and verified, native references not distributed. Private artifact artifacts/MaxButtonsRedrawProbe-0.1.0/, pre-install log/receipt .local/evidence/max-redraw-observer-install/. Native-reference warning-free build and executable original-exception/normal-return/input-scope/budget checks pass. No installed runtime readiness/reproduction/attribution/fix is asserted. Probe reads actual row/window/cache/patch state and propagates exceptions; no persistent/quantity/visual behavior patch. User gamepad sequence and cleanup are in shared MAX_BUTTONS_REDRAW_RESEARCH.md and DEVELOPMENT.md. No production version increment or release promotion.

## 2026-10-19 — Max Buttons first diagnostic return, no reproduced exceptions

Exact preserved log SHA256366aee5696c89f2e034ee09671a54b83c3a7cbee6c55ea8004d4b973670fe991 loads observer0.1.0/planner0.1.25/Max1.4.0.11 normal amount/redraw pairs and4 technology openings/branch navigation, zero exceptions or snapshot/observer errors. Real window diagnostics were lost to the observer's startup-budget defect; result is bounded non-reproduction, not cause or compatibility acceptance. Shared research owns further findings/old crafting-auto-hide difference.

Updated only diagnostic observer to0.1.1, frozen shared source0d2819401b08cc41569cb828014a167c16e2070f, DLL SHA2563b99d772ac8be9896290382da277344db52d7d1bbf721d9ecf191ab14888ab4c; local compile and actual exception/scope/startup-budget tests pass. Installed2026-10-18T21:21:22Z with host closed, prior helper backed up, working mod hashes unchanged. Evidence/install receipt .local/evidence/max-redraw-observer-return/; DEVELOPMENT.md/shared MAX_BUTTONS_REDRAW_RESEARCH.md own one-recipe auto-hide follow-up. New runtime evidence pending; no core/source version change or stable promotion.
