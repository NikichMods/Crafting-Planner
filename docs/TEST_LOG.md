# Test Log

## Crafting Planner Research Probe 0.1.0 — awaiting runtime evidence

- Purpose: verify the public craft/build focus seams, player-only versus interaction/world-zone inventory counts, bounded native chest-to-player transfer, and the open-chest Option2 gamepad context.
- Source branch: `research/p0-native-seams`
- Exact source commit: `f94be35330d17f3dccdfbe927d4f847fb5905498`
- CI run: `35652997475`
- GitHub Actions artifact: `CraftingPlannerResearchProbe-0.1.0` (artifact ID `10662547953`)
- DLL: `Crafting Planner Research Probe 0.1.0.dll`
- DLL SHA-256: `019eedc85979eb3a5f09d63d5b4a6a6feeb99251af6e837ce215c74918abc90f`
- Artifact ZIP SHA-256: `4c09f209162c8d042204b7f4323a7007207697037aded7e765a41e0562ca7f6f`
- Build status: restore, compile, package-boundary verification, and artifact upload all passed.
- Runtime status: pending user-installed Graveyard Keeper 1.407 evidence.
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
