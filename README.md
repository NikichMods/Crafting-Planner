# Building Planner

Version **1.0.2** (development candidate) for **Graveyard Keeper 1.407 on Windows PC**, using BepInEx 5. Plan several construction, repair, upgrade or clearing projects and collect only their missing materials from the chest you open.

This candidate preserves the complete native location name/status block to the left of the HUD plan, including cemetery quality. Empty no-zone placeholders are hidden only without additional information. Both NPC cards retain native placement relative to the location, with extra overlap clearance when needed. Native verification is pending; the fixed1.0.0 build remains unchanged.

## Install

1. Install BepInEx 5 for the game. The tested environment uses BepInEx 5.4.23.5.
2. Close the game. Extract the package into the game directory so the plugin is at `BepInEx/plugins/CraftingPlanner.dll`.
3. Keep one copy of that DLL. Start the game and load a save.

BepInEx supplies the required runtime libraries; no other mod is required. Keep the game's own libraries in place. To update, close the game and replace the same DLL. To uninstall, close the game and remove it; local plan snapshots remain unless you separately delete `BepInEx/config/CraftingPlanner/plans`.

## Use

In a supported construction/repair menu, adjust the planner quantity using the arrows in the rightmost column. Activating the native construction row still starts the game's ordinary action. Repeatable goals allow 1–20; one-off repairs/upgrades/interior changes allow 1. Reducing a pinned goal to 0 removes it.

The plan shows exact combined **Required / Have / Missing**. Have counts physically carried materials, including supported carried bags; nearby chests and a bag stored in a chest do not count as carried. Missing is never below 0. Mouse arrows also adjust goals on the HUD. With a gamepad, press the right stick to edit the HUD plan, up/down to select a goal, left/right to change quantity, and B or the right stick to finish.

Open a chest and choose **Take Needed** by mouse, the native Work key (default F) or gamepad Y. It takes only missing materials from that chest, limited by stock and native inventory/bag capacity. Partial transfers are supported; one unavailable material does not block another. Crafting, construction and travel remain manual.

Supported successful placement, repair, clearing, upgrade and home-interior confirmation automatically reduce/remove the matching goal. Cancelled/failed actions preserve it. Names, fonts and labels follow all 11 built-in game languages; long words expand the plan left while quantity columns stay fixed. No F2 calibration window is included.

## Saved plans and limits

Plans and quantities are saved with the game's ordinary world save and restored for that save/version after loading or restarting. Unsaved plan edits roll back when loading. Native game saves are not modified by the mod.

Plan files are local, under `BepInEx/config/CraftingPlanner/plans`. Back up that directory together with your saves. Steam Cloud and copying/renaming a save slot do not automatically carry or rebind these plan snapshots; they are tied to the local save path and exact native save bytes.

Ordinary workstation production such as "make 10 planks" is not a goal. Requirements are not recursively expanded into raw materials. There is no remote crafting, nearby/multiple-chest collection or Quick Stack reserve protection. Unsupported scripted project shapes are excluded. A long plan can extend below the screen; there is no scrolling/paging or total-goal cap. The 20 limit applies per repeatable goal.

Quick Stack transfer refresh is exercised. Max Buttons Redux can produce a technology-menu redraw error after automatic crafting-menu hiding, also reproduced without Planner in the tested mod set; this release does not repair that separate issue. Compatibility evidence is representative, not a guarantee for every mod combination or bag type.

For support, include `BepInEx/LogOutput.log`, game/BepInEx/mod versions and the steps that produced the problem. Logs can include local identifiers; review them before posting publicly.

## Source and documentation

Original code is licensed under [MPL-2.0](LICENSE); see [LICENSING.md](LICENSING.md) for scope and dependency exclusions. Corresponding source: [NikichMods/Crafting-Planner](https://github.com/NikichMods/Crafting-Planner).

- [Changes](CHANGELOG.md)
- [Product scope](https://github.com/NikichMods/Crafting-Planner/blob/v1.0.0/docs/PRODUCT_SCOPE.md)
- [Build and recovery](https://github.com/NikichMods/Crafting-Planner/blob/v1.0.0/docs/DEVELOPMENT.md)
- [Verified game data](https://github.com/NikichMods/Crafting-Planner/blob/v1.0.0/docs/VERIFIED_GAME_DATA.md)
- [Bounded runtime evidence](https://github.com/NikichMods/Crafting-Planner/blob/v1.0.0/docs/TEST_LOG.md)

Technical identity remains `CraftingPlanner`; player-facing name is **Building Planner**.
