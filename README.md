# Crafting Planner

A vanilla-friendly Graveyard Keeper 1.407 mod project for pinning multiple construction/world projects and gathering exactly the materials still needed.

Core direction:

`pinned projects -> aggregate requirements -> player Have -> Missing -> Take Needed from the current storage`

The project is currently in the research/prototype stage. P0 is project-first: repeatable construction plus verified one-off repair/upgrade/clearing projects. Ordinary workstation production recipes such as "make 10 planks" are not planner goals. Production scope remains intentionally narrow: no remote crafting, no automatic cross-storage logistics, no dependency expansion, and no world-level chest collection without a later explicit decision.

Engineering contract: `NikichMods/DevRules`.

Current research:
- `docs/PRODUCT_SCOPE.md`
- `docs/RESEARCH_REPORT_2026-09-21.md`
- `docs/VERIFIED_GAME_DATA.md`
- `docs/TEST_LOG.md`
