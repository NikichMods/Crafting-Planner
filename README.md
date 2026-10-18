# Building Planner

A vanilla-friendly Graveyard Keeper 1.407 mod for planning multiple construction, repair, upgrade and clearing projects, and collecting the materials still needed.

Choose projects and quantities in supported native menus, then use the shared HUD to compare **Required / Have / Missing** against what your character carries. Adjust the plan with mouse or gamepad. **Take Needed** collects only missing materials from the currently open chest, bounded by its contents and native inventory capacity; partial transfers are supported.

The current development candidate is **0.1.26**. Supported placement, repair, upgrade, clearing and home-interior completion paths automatically reduce/remove goals after verified commitment; cancellation preserves them. Plans and quantities persist separately for each saved game. Representative runtime cases are accepted; final release checks continue.

Plan snapshots are local mod-owned files under `BepInEx/config/CraftingPlanner/plans`. They are saved with the native world save; unsaved plan edits roll back when loading. These files do not automatically travel with Steam Cloud or copied/renamed save slots.

Ordinary workstation production recipes such as "make 10 planks" are not planner goals. Materials are not recursively expanded; crafting/building and travel remain manual. There is no remote crafting or automatic cross-storage collection. Gamepad and all 11 host languages are supported targets.

Project documentation:

- [Product scope and current UX](docs/PRODUCT_SCOPE.md)
- [Development status and recovery](docs/DEVELOPMENT.md)
- [Verified game data](docs/VERIFIED_GAME_DATA.md)
- [Runtime acceptance and candidate history](docs/TEST_LOG.md)

Engineering contract: `NikichMods/DevRules`. Technical identity remains `CraftingPlanner`.
