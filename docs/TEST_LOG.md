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
