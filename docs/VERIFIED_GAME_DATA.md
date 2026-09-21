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
