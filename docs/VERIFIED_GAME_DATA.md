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

Support first:
- normal static crafting recipes with concrete `CraftDefinition.needs`;
- building recipes with concrete `ObjectCraftDefinition.needs`.

Initially exclude until separately verified:
- mixed alchemy/dynamic ingredient selection;
- multiquality ingredient-selection cases;
- survey/research;
- fixing/world repair special flows;
- body/autopsy insertion/extraction;
- resurrection;
- prayer;
- refugee/special scripted crafts;
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

1. Exact public/narrow pin capture seam for craft menu.
2. Exact public/narrow pin capture seam for build menu.
3. Concrete current-storage ownership lifecycle without private-field reflection.
4. Open-chest action/input placement that does not consume vanilla/Quick Stack/Recipe Pin behavior.
5. Player bag/toolbelt counting behavior in the exact target runtime.
6. Multiquality detection/exclusion behavior for the first supported subset.


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
