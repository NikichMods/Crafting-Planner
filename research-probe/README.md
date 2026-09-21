# Crafting Planner Research Probe 0.1.0

Research-only. Do not treat this DLL as the mod.

Questions proved by this probe:
1. Can normal craft/build focus be captured through narrow public `OnOver` seams?
2. Does player-only inventory differ from interaction/world-zone availability as expected?
3. Can a bounded storage -> player move use native `MultiInventory.MoveItemTo` safely?
4. Does the inherited chest `Option2` path provide a usable contextual gamepad action?

## Runtime procedure

1. Install only the built `CraftingPlannerResearchProbe.dll` as an additional BepInEx plugin.
2. Open an ordinary crafting recipe with simple fixed ingredients and hover/focus it once.
3. Close the craft UI and open a chest that contains at least one needed ingredient.
4. Keyboard: press **F8** once. Gamepad: press the native **Option2 / Y** once.
5. Repeat once for an ordinary building recipe if convenient.
6. Return `BepInEx/LogOutput.log`.

Expected log prefix:

`CRAFTING_PLANNER_PROBE`

The probe moves real items from the open chest into the player's inventory. This is ordinary reversible inventory state, but it can be saved if the game is saved afterward.

The probe deliberately rejects non-`CraftType.None`, `needs_from_wgo`, and detected multiquality recipes. That is a fail-safe research boundary, not a final support list.
