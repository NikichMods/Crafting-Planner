# Crafting Planner Research Probe 0.2.0

Research-only. This DLL is not the production mod.

Purpose: close the P0 evidence gap for **construction/build projects** and **one-off world projects** such as repairs, upgrades, and clearing blocked objects.

The probe is observational. It does not add goals, transfer items, build objects, repair objects, or invoke project actions on its own.

## What it logs

Diagnostic prefix:

`CRAFTING_PLANNER_PROBE`

The probe records:

- opening a vanilla build menu and every `ObjectCraftDefinition` offered by that builder;
- each build card drawn by the game;
- mouse hover and gamepad focus over build cards separately;
- a selected build definition if the player selects one;
- the target object, interaction type, hint, script, and relevant object-definition flags when the player starts an interaction;
- the target object's native craft list when applicable;
- opening the normal craft GUI from a world object;
- `TryStartCraft` calls near a player interaction;
- the exact `CraftComponent.Craft` definition when a relevant project/craft actually starts.

Definitions include their concrete `needs`, `needs_from_wgo`, outputs, one-time/change-WGO/script flags, and for object crafts their build type, output object, builders, and script-building flag.

## Runtime procedure

1. Remove **Crafting Planner Research Probe 0.1.0** if it is still installed.
2. Install only **Crafting Planner Research Probe 0.2.0.dll** as the Crafting Planner research plugin.
3. Load the normal Graveyard Keeper 1.407 save with the usual mod set.
4. Open any ordinary construction/building desk that offers placeable projects. Move the mouse across several build cards. If using a gamepad, focus several cards as well. You do **not** need to build them.
5. Visit several still-available one-off world projects if convenient:
   - a repair or upgrade;
   - a blocked passage / rubble / clearing action;
   - another project-like world interaction.
   Start the normal interaction so the game shows/opens whatever it normally uses.
6. Do not deliberately finish an irreversible repair/build just for the probe. If a normal interaction would immediately change the world, stop after gathering whatever safe evidence is available. If you do complete something for testing, avoid saving afterward unless you want that gameplay change.
7. Return `BepInEx/LogOutput.log`.

One building desk plus two different one-off world-project interactions is enough for the first pass.

## Save safety

The probe itself performs no inventory or world mutation. Normal vanilla actions that you choose to complete can still modify the save exactly as they normally would.
