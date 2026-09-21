# Test Log

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
