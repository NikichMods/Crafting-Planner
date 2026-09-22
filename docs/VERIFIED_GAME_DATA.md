# Verified Graveyard Keeper 1.407 Data for Crafting Planner

Status legend:
- **Verified static** — established from Graveyard Keeper 1.407 decompile/API evidence and/or accepted NikichMods repository evidence.
- **Runtime gate** — plausible static seam exists, but installed-runtime behavior must still be proven before production relies on it.
- **Open** — not yet established.

Primary static reference used in this research: public `Kupie/GYK_DECOMP` at commit `6abf79199d92482af1c7573870dd9a20ec2270b9`, plus prior accepted NikichMods inventory/Quick Stack research.

## Recipe model

**Verified static**

`CraftDefinition` owns the common recipe data required by the planner:
- `List<Item> needs`
- `List<Item> needs_from_wgo`
- `List<Item> output`
- `CraftType craft_type`
- `CraftSubType sub_type`
- multi-craft/special-behavior flags.

`ObjectCraftDefinition : CraftDefinition` is the building recipe subtype.

`GameBalance` exposes separate `craft_data` and `craft_obj_data` collections.

`CraftsInventory.GetCraftsList()` resolves normal craft IDs through `GameBalance`.
`CraftsInventory.GetObjectCraftsList()` resolves building definitions.

## Craft/build UI ownership

**Verified static**

`BaseCraftGUI.CommonOpen(...)` builds its visible recipe list from actual `CraftDefinition` instances.

`CraftItemGUI` receives a `CraftDefinition` through `Draw(CraftDefinition)`, exposes its current definition through `current_craft`, and draws ingredients from `current_craft.needs`.

Building UI uses `ObjectCraftDefinition`.

**Runtime gate**

The exact lowest-risk Harmony/UI seam for “pin the currently focused/hovered recipe” still needs one installed-runtime probe. Production must not read private UI fields by reflection merely because they are visible in decompiled source if a public callback/method boundary can be used.

## Ingredient semantics

**Verified static**

For ordinary crafts, `BaseCraftGUI.CanCraft(... amount)` evaluates `craft.needs` through native `MultiInventory.IsEnoughItems(... amount)` and separately evaluates `needs_from_wgo`.

Native craft start consumes the selected needs through `MultiInventory.RemoveItems(...)`; workbench-owned needs are removed from the workbench data separately.

Therefore planner requirements can derive from `CraftDefinition.needs` for the clean static-recipe subset, but `needs_from_wgo` and other special fields must not be silently treated as normal carried ingredients.

## Recipe categories

**Verified static**

`CraftDefinition.CraftType` includes:
- `None`
- `ResourcesBasedCraft`
- `Survey`
- `MixedCraft`
- `Fixing`
- `AlchemyDecompose`
- `PrayCraft`
- `RatBuff`
- `RefugeeCampCraft`

`CraftSubType` includes `Alchemy` and `SurveySciencePoints`.

`MixedCraftGUI`, alchemy-specific routing, survey, fixing, prayer, body/autopsy and other special GUIs demonstrate that not every “recipe-like” operation shares identical selection/ingredient semantics.

### First clean support candidate

P0 support boundary after Probe 0.2.0:
- builder projects supplied as concrete `ObjectCraftDefinition` entries with plain `needs`;
- representative one-time builder upgrades using the same object-craft model with concrete `needs`;
- representative repair/clearing world projects supplied as concrete `CraftDefinition` entries with plain `needs` and `change_wgo`.

Ordinary workstation production is intentionally not a P0 goal even though its static recipe model is understood.

Continue to exclude until separately approved/verified:
- mixed alchemy/dynamic ingredient selection;
- multiquality ingredient-selection cases;
- survey/research;
- body/autopsy insertion/extraction;
- resurrection;
- prayer;
- refugee/special scripted crafts;
- scripted world-project shapes that do not match either accepted P0 family;
- any recipe where the displayed/consumed ingredient set differs materially from plain `needs`.

## Player inventory vs crafting-access inventory

**Verified static — critical**

`MainGame.me.player.GetMultiInventoryForInteraction()` is **not** a valid definition of planner `Have`.

That method can include the nearest object's/world-zone inventories. It answers “what crafting can access here”, not “what the player physically carries”.

For planner `Have`, count player-owned data only. Native `Item.GetTotalCount(item_id, count_in_bags:true)` recursively counts carried bag contents.

**Runtime gate**

Confirm exact P0 treatment of toolbelt/secondary inventory. Default planner assumption is that ordinary crafting materials are counted from carried inventory + bags, not the toolbelt.

## Storage-local inventory

**Verified static**

`WorldGameObject.GetMultiInventoryOfWGOWithoutWorldZone(true)` builds a `MultiInventory` from that WGO and its bags without adding its world-zone inventories.

This is the correct shape for “this specific storage” semantics.

## Native transfer/capacity path

**Verified static**

Vanilla `ChestGUI`:
- creates a player and current-chest `MultiInventory` when opened;
- computes move capacity using source total count and destination `CanAddCount(...)`;
- performs actual transfer through `MultiInventory.MoveItemTo(...)`;
- redraws both inventory panels after transfer.

Prior accepted Specialized Storage research also confirms that `MoveItemTo` is the native ownership path to preserve stack/capacity semantics.

### P0 Take Needed formula

For each aggregated exact item ID, immediately before transfer:

`Missing = max(Required - PlayerHave, 0)`

`Move = min(Missing, CurrentStorageAvailable, PlayerCapacity)`

If `Move > 0`, call native transfer for that bounded quantity.

Each ingredient is independent. Failure or absence of one item must not abort the loop for other ingredients.

## Craft completion

**Verified static**

Normal craft completion flows through `CraftComponent.FinishCurrentCraft()` -> `ProcessFinishedCraft()` and eventually `End()`, with special-case output/side-effect behavior.

This is evidence that P1 auto-decrement is feasible, but it is not yet a safe universal P0 seam because special recipes/build flows can differ.

## Input bindings

**Verified static**

Relevant default gamepad mappings include:
- Interaction = A
- Work = Y
- Action = X
- Select = A
- Back = B
- Option1 = X
- Option2 = Y
- LB/RB/LT/RT/D-pad have multiple context-dependent uses.

Relevant default keyboard mappings include:
- Interaction = E
- Work = F
- Action = X
- plus context/UI/navigation bindings.

Keyboard bindings are remappable by the game.

**Product/compatibility consequence:** do not hardcode a global F/Y/X gesture for Crafting Planner without contextual conflict verification.

## Third-party precedents

### Recipe Pin 0.3.0

Current Nexus documentation says Recipe Pin:
- supports crafting/building recipe pinning;
- shows available/required counts;
- counts player inventory and bags;
- supports mouse/keyboard and gamepad;
- currently supports only one pinned recipe.

Its 0.2.0 changelog documents input changes specifically to reduce Queue Everything/Quick Stack conflicts.

### Quick Stack 1.0.0

Current Nexus documentation describes Quick Stack as a world-interaction “Stack” action that transfers matching player items to the target chest.

Accepted binary/static research identifies `MultiInventory.MoveItemTo` use and patches around world interaction/bubble refresh.

**P0 compatibility direction:** keep Crafting Planner's `Take Needed` in open-storage context and avoid owning Quick Stack's world-interaction seam.

## Open runtime gates before production

1. Final production pin/manage gesture and help presentation, especially gamepad behavior and conflicts. Build-menu mouse focus is runtime-verified; gamepad build focus was not exercised in Probe 0.2.0.
2. Concrete current-storage ownership lifecycle without private-field reflection.
3. Final open-chest action/help placement. The native Option2/Y transfer seam already works with Quick Stack/Recipe Pin/Queue Everything installed.
4. Player bag/toolbelt counting behavior in the exact target runtime if production relies on those edge semantics.
5. Capacity-limited destination behavior if native-path static evidence is judged insufficient for release acceptance.
6. Production UI anchor/lifecycle in the native NGUI hierarchy.

The P0 project requirement/capture model for the accepted builder and `change_wgo` world-project families is no longer an open runtime gate.


## Runtime evidence — Research Probe 0.1.0 (2026-09-22)

**Verified runtime** on Graveyard Keeper 1.407 with the user's normal 35-plugin setup, including Quick Stack 1.0.0, Recipe Pin 0.3.0, Queue Everything 2.2.1, Specialized Storage 1.2.0, and other installed mods.

Returned `LogOutput.log` SHA-256:
`2154067363657c21b4ce05bb1072420aff049732d33aa27463f7642ff81315e6`

### Craft focus seam

The probe captured five ordinary fixed-ingredient craft definitions through the candidate focus seam:
- `wood1_2 -> wood:1`
- `chisel_1_2 -> stick:4, detail_1:3`
- `armor_lamellar_1_2 -> skin:4, detail_1:6`
- `lense -> glass_0:2, polishing_paste:1, faith:2`
- `wooden_plank_3 -> flitch:1`

This proves the candidate craft-focus callback is live in the installed runtime. The probe did not tag whether each focus event originated from mouse versus gamepad, so that finer distinction is not claimed.

### Player Have versus interaction inventory

For `flitch`, the probe observed:
- player-only Have = 0;
- interaction/crafting-access count = 18;
- current chest count = 9.

This is direct runtime evidence that `GetMultiInventoryForInteraction()` is broader than player-carried inventory and must not be used for planner `Have`.

### Current-storage transfer

With `Required=1`, `Have=0`, `Missing=1`, chest count 9, and ample player capacity, native transfer moved exactly one `flitch`:
- player 0 -> 1;
- chest 9 -> 8.

Immediately afterward, the same action observed `Missing=0` and requested 0, proving no surplus transfer in that state.

The probe also exercised a chest with zero matching material: `Missing=1`, storage count 0, requested 0, no transfer.

### Open-chest input seam

Both research triggers worked:
- keyboard F8 research trigger;
- inherited chest-context `Option2 / Y`.

`Option2 / Y` successfully transferred the needed item while Quick Stack, Recipe Pin, and Queue Everything were installed. This makes open-chest Y a strong production candidate, but final production controls should still be tied to the planner's actual chest UI/help presentation rather than hardcoded globally.

### Not yet proven by this log

- build-menu `BuildItemGUI` focus capture (no `kind=build` diagnostic occurred);
- one-off world repair/upgrade/clearing project capture and exact ingredient semantics;
- bag placement/count behavior in a case where a needed item is actually in a carried bag;
- inventory-full/partially-full capacity edge in runtime (static native-path evidence remains strong).

## Production UI substrate

**Verified static**

The game HUD/UI uses native NGUI-style components including `UIPanel`, `UIWidget`, `UILabel`, `UIButton`, `UI2DSprite`, and `GamepadSelectableButton`. `GUIElements.me.hud` owns the main HUD hierarchy and native windows notify the HUD on open/close.

Therefore a vanilla-style planner HUD is technically feasible without using a generic BepInEx/IMGUI gameplay overlay. Preferred direction is a narrow custom planner object inside the game's UI hierarchy, reusing or cloning verified native visual components where practical. Exact anchor/prefab reuse remains a runtime/UI implementation gate.


## Construction UI ownership — static verification 2026-09-22

**Verified static**

Normal construction desks do not use a separate `BuildsGUI` list as the primary path observed by the player. `MainGame.OpenBuildObjectGUI(build_desk)` builds a `CraftsInventory` from visible `ObjectCraftDefinition` entries whose `builder_ids` contain the current builder object, then calls:

`CraftGUI.OpenAsBuild(build_desk, craftsInventory)`

`CraftGUI.OpenAsBuild` sets build-specific state, disables craft-amount buttons, and opens the ordinary `CraftGUI` with that object-craft inventory.

Therefore the production/research capture boundary for normal construction should distinguish `CraftGUI` **build context** rather than assuming that `CraftItemGUI` implies ordinary workstation production.

**Verified static input detail**

`CraftItemGUI.OnMouseOvered()` is the mouse-hover callback. `CraftItemGUI.OnOver()` is the gamepad-focus callback and returns immediately when the GUI is not in gamepad mode. Probe 0.1.0 only instrumented `OnOver`, so its successful focus evidence must not be treated as proof of mouse capture. Probe 0.2.0 instruments both callbacks separately.

## One-off world-project discovery — static basis for Probe 0.2.0

**Verified static, semantic boundary not yet accepted**

World projects may enter several host-owned paths:

- `WorldGameObject.Interact(...)` dispatches by `ObjectDefinition.InteractionType`;
- `Builder` opens `MainGame.OpenBuildObjectGUI`;
- `Craft` can run a valid interaction script first and otherwise delegate to `CraftComponent.Interact`;
- `RunScript` executes the object's valid interaction script;
- `CraftComponent.Interact` opens the native craft GUI when the object has interactable crafts;
- `CraftComponent.FillCraftsList()` resolves object crafts from `GameBalance.me.GetCraftsForObject(wgo.obj_id)`;
- `WorldGameObject.TryStartCraft(craftName)` resolves a concrete `CraftDefinition` and invokes native craft start;
- project-like definitions can additionally carry `one_time_craft`, `change_wgo`, `end_script`, `end_event`, or `craft_after_finish`.

This proves that repair/upgrade/clearing operations cannot safely be assumed to be one uniform recipe category from static inspection alone. Probe 0.2.0 observes representative live interactions before a production inclusion rule is chosen.


## Runtime evidence — Research Probe 0.2.0 (2026-09-22)

**Verified runtime** on Graveyard Keeper 1.407 with the user's normal 35-plugin setup.

Returned `LogOutput(1).log` SHA-256:
`3bf9cb3782b623e40c38ef70189f21de751585d2ea568c0b09ec3c8db9b29fce`

No Crafting Planner Research Probe warning/error/exception was logged.

### Builder projects

The native graveyard builder opened a build-context `CraftGUI` with exact `ObjectCraftDefinition` entries. A flower bed was observed with:

- `needs = stone_plate_1:2, peat:1, flw_poppy:2`;
- `BuildType.Put`;
- `out_obj = flowerbed_2x2`;
- `builder_ids = graveyard_builddesk`.

Mouse focus produced the same exact definition, proving the build-card mouse capture seam in runtime.

The user also selected `graveyard_builddesk:p:grave_empty_place`; the probe recorded `build_selected`, then vanilla entered `CraftBuilding(... type=Put)` / build mode. This confirms that the observed card definition is the same host object used by the native selection path.

### One-time builder upgrade

The home wood builder exposed:

`mf_wood_builddesk::upgrade_player_buildzone`

with:

- `needs = detail_2:10, wooden_plank:8, nails:4`;
- `one_time_craft = true`;
- `BuildType.None`;
- `out_obj = upgrade_player_buildzone`;
- `end_script = player_buildzone_upgrade`.

The same definition was captured through mouse focus. This is accepted evidence that at least this class of one-time builder upgrade has stable concrete material requirements compatible with the planner model.

The same builder also exposed a normal repeatable placement:

`mf_wood_builddesk:p:mf_box_stuff_place -> flitch:4, nails:4, detail_1:4`

using `BuildType.Put`.

### Repair projects

Representative broken mortuary objects exposed a single ordinary `CraftDefinition` through the interacted object's normal craft GUI:

- `fix_morgue_builddesk -> flitch:2, detail_1:2; change_wgo=morgue_builddesk`;
- `fix_morgue_throw_in -> stone_plate_1:2, detail_1:4; change_wgo=morgue_throw_in`;
- `fix_morgue_throw_out -> stone_plate_1:2, detail_1:4; change_wgo=morgue_throw_out`.

These are not `ObjectCraftDefinition` builds. They are ordinary craft definitions whose semantics are world-state replacement/repair, but their planner requirement source is still the canonical `needs` list.

### Blocked-passage clearing

`blockage_V_low` exposed exactly one ordinary craft:

`blockage_V_low_destruction -> spike_1:10, wood_balk_1:1, detail_1:4`

with `change_wgo=0`.

It opened the normal `CraftGUI`, just like the repair cases. This verifies the same P0 requirement shape for at least this blocked-passage/clearing class.

### Accepted semantic inclusion rule

For P0, the evidence now supports two host-native project families without hardcoded recipe-ID tables:

1. **Builder context**: concrete `ObjectCraftDefinition` supplied by the native builder UI, with plain exact `needs`. Repeatable placement and the observed one-time builder upgrade are both valid planner shapes. Internal move/remove pseudo cards are not project goals.
2. **World change craft context**: a concrete ordinary `CraftDefinition` owned by the interacted world object, with plain exact `needs` and a non-empty `change_wgo`. The observed repair and clearing cases fit this shape.

Do not extend this rule to unrelated special/scripted craft systems merely because they also open a craft-like UI.

### Not proven / not required to close this P0 data-model question

- gamepad build-card focus was not exercised; only mouse focus is runtime-confirmed in Probe 0.2.0;
- the repair/clearing crafts were opened but not completed, so completion hooks remain a later concern (P1 auto-decrement, not P0);
- carried-bag and capacity-limited transfer edges remain separate inventory-runtime questions.


## Native resolution / NGUI scaling lifecycle — static verification 2026-09-22

**Verified static**

Graveyard Keeper owns GUI resolution changes through `MainGame.OnScreenSizeChanged(int w, int h)`.

That path:

- resolves the current screen dimensions;
- applies the game's configured pixel size to the world camera;
- updates `MainGame.ui_root.manualHeight = screenHeight / gui_pixel_zoom`;
- calls `GUIElements.RecalcScreenResolution(w, h)`;
- then lets the existing NGUI hierarchy recalculate its layout.

The target runtime currently uses `gui_pixel_zoom = 2`.

NGUI's `UIRoot` also exposes its own scaling model (`scalingStyle`, `activeHeight`, `pixelSizeAdjustment`) and anchored rectangles/widgets.

**Architecture consequence**

Crafting Planner must not treat the user's current 2560x1440 resolution or a manually derived `Screen.width/Screen.height` offset as the canonical HUD coordinate system.

The production HUD should attach to a verified native NGUI ownership/anchor context and allow the game's own resolution/layout lifecycle to reposition/scale it across resolutions and aspect ratios. Small user-facing offsets may later be configurable for visual preference, but those offsets must be relative to the accepted native anchor/layout model rather than compensating for an unknown coordinate system.

HUD Rendering Probe 0.3.0 completed the runtime verification.

**Verified runtime**

- A Planner label placed directly in the HUD panel with screen/root-derived coordinates could be fully active and drawable while its projected rectangle remained entirely outside the viewport.
- A control label cloned from the live `HUD.zone_name` label and kept under that label's existing native parent rendered on-screen.
- A second control moved directly under the HUD panel did not.
- The same native-parent control remained correctly attached to the visible HUD after a 1600x1200 resolution change followed by an external borderless/window resize back toward the desktop-sized window.

**Accepted HUD layout rule**

For the ordinary gameplay Planner HUD, use the verified `HUD.zone_name` parent coordinate context. Position Planner content only by a small local offset from that native label/context.

Do not use `Screen.width`, `Screen.height`, `UIRoot.activeHeight`, or one-time root-panel coordinate conversion as the canonical placement mechanism.

Do not add Borderless Gaming-specific polling or resolution hooks merely to compensate for external window resizing; inheriting the native HUD parent transform is the narrower compatibility mechanism.
