# Crafting Planner — Working Contract

This repository follows the canonical global development rules in `NikichMods/DevRules`.

Before substantive technical work, read:
- `ENGINEERING_RULES.md`
- `CI_POLICY.md`
- `GIT_WORKFLOW.md`
- `PROJECT_BOOTSTRAP.md`
- `RUNTIME_TEST_HARNESS.md` when installed-runtime evidence is relevant

This file contains only project-specific additions, verified constraints, and explicit scope decisions.

## Project identity

- Project: **Crafting Planner**
- Repository: `NikichMods/Crafting-Planner`
- Game/runtime: **Graveyard Keeper 1.407**, PC, BepInEx/Harmony
- Purpose: replace the Recipe Pin concept with a vanilla-friendly multi-project construction/material planner without remote crafting or automatic logistics.

## Product boundary

Core player loop must remain manual:
- player chooses goals;
- player travels to the relevant storage/workstation;
- player gathers materials;
- player performs crafting/building.

The mod may reduce memory burden and repetitive item transfer. It must not become a magic cross-storage logistics network without a later explicit product/architecture decision.

### P0

P0 is:
- multiple pinned **projects**, not arbitrary workstation production recipes;
- repeatable construction/build goals such as benches or workstations;
- one-off world projects such as repairs, upgrades, or clearing blocked objects when the game exposes stable concrete material requirements;
- target quantity for repeatable placeables (for example, three benches); one-off world projects are quantity 1;
- aggregate exact ingredient requirements across all pinned projects;
- `Required`, `Have`, and `Missing = max(Required - Have, 0)`;
- `Have` means items physically carried by the player, including carried bags when native inventory semantics support them;
- `Take Needed` operates only on the currently open/explicitly interacted storage;
- take at most `Missing`, at most what storage contains, and at most what the player can actually accept;
- partial transfer is success;
- failure/unavailability of one ingredient must not block others;
- vanilla-style planner UI and UI refresh after relevant state changes.

Ordinary workstation item production (for example, pinning ten planks, polished stone, or a chisel) is not a P0 planner goal. Such items can appear as required materials for a project, but P0 does not recursively expand or pin their production recipes.

### Not P0

Do not pull these into the first production scope automatically:
- auto-decrement after completed craft/build;
- save persistence (explicitly deferred after product review; session-only goals are accepted for P0);
- Quick Stack reserve protection;
- dependency expansion/base-material planning;
- nearby/multi-storage collection;
- taking project materials from a storage without opening/interacting with its inventory UI;
- remote crafting;
- alternative recipe/workstation planning.

## Mandatory start-of-work checks

Before substantive implementation:
1. inspect current repository/source/history;
2. read this file;
3. read `docs/PRODUCT_SCOPE.md`, `docs/VERIFIED_GAME_DATA.md`, and `docs/RESEARCH_REPORT_2026-09-21.md`;
4. inspect `docs/TEST_LOG.md` for accepted runtime evidence;
5. verify any unfamiliar Graveyard Keeper/Recipe Pin/Quick Stack seam before production relies on it.

Repository evidence outranks chat memory.

## Evidence contract

Do not guess game APIs, IDs, recipe categories, inventory ownership, transfer semantics, UI lifecycle, input behavior, or third-party-mod internals.

Preferred evidence:
1. accepted project runtime evidence/current source;
2. exact 1.407 assembly or verified decompile/source evidence;
3. narrow research probe;
4. user runtime evidence when only the installed game can close the question.

Public decompile/source evidence is supporting evidence, not permission to copy game implementation into this repository.

## Architecture / runtime constraints

Host-native-first is mandatory.

Prefer:
- canonical `CraftDefinition` / `ObjectCraftDefinition` data;
- goal state that stores only the minimum identity/quantity needed to resolve current host definitions;
- derived aggregate requirements rather than a mirrored recipe database;
- player-only inventory counting for `Have`;
- the game's native inventory capacity and transfer operations for `Take Needed`;
- on-demand/event-driven redraw and recalculation;
- narrow Harmony seams.

Avoid:
- per-frame inventory/storage scans;
- recurring world-zone scans;
- copied recipe tables;
- custom stack/capacity logic;
- broad patches to world interaction/input when a narrower UI-context seam exists;
- reflection/private identifiers unless a verified public/narrow alternative is insufficient.

For P0, goals are session-local. The user explicitly accepted this simplification for the first implementation; per-save persistence remains a later upgrade.

For UI, prefer a vanilla-style HUD using the game's native GUI stack or a narrow hybrid that reuses native fonts, sprites, panels, icons, and gamepad conventions. A generic BepInEx/IMGUI debug overlay is acceptable for research only, not the desired production presentation.

## Verified P0 semantic boundary

Current product scope is project-first rather than workstation-recipe-first.

Runtime evidence from Research Probe 0.2.0 accepts two concrete P0 project families:

- **Builder projects**: the native build menu supplies exact `ObjectCraftDefinition` instances through the ordinary `CraftGUI` build context. Runtime evidence covers repeatable placement (`BuildType.Put`) and a one-time builder upgrade (`one_time_craft=true`, `BuildType.None`) with concrete `needs`. Mouse focus and the native build-selection path are live.
- **World change projects**: representative repairs and a blocked-passage clearing action use ordinary `CraftDefinition` entries with concrete `needs` and a non-empty `change_wgo`, opened through the normal craft GUI from the interacted world object.

These families share the host-owned `CraftDefinition.needs` requirement model, so P0 may aggregate their exact materials without inventing a second recipe database. Do not generalize this acceptance to every scripted/special world interaction; unsupported shapes must fail closed until separately verified.

Ordinary workstation production recipes remain useful research evidence for recipe capture and material semantics but are intentionally not P0 planner goals.

Do not silently include dynamic/special systems such as mixed alchemy, multiquality ingredient selection, survey/research, body/autopsy operations, resurrection, prayer, refugee scripted production, or other flows unless separately approved and verified.

## Inventory semantics

For planner `Have`, do **not** use the normal crafting `GetMultiInventoryForInteraction()` result: it may include nearby/world-zone storage and therefore does not mean “carried by the player”.

Count only player-owned carried inventory (and carried bags when enabled by verified native semantics).

For `Take Needed`, operate only between:
- the explicitly current storage; and
- player-owned carried inventory.

Use native `MultiInventory` capacity/transfer behavior rather than reproducing stack rules.

## Compatibility boundary

### Recipe Pin

Recipe Pin is a precedent and compatibility target, not a dependency. Do not copy its code/assets or depend on private internals.

Crafting Planner is the functional replacement/superset for Recipe Pin. Recipe Pin is not a dependency and production UX must not require it.

Compatibility during development remains useful, but do not design P0 around Recipe Pin's private state or gestures.

### Quick Stack

P0 must avoid patching Quick Stack's world-interaction ownership path if possible.

`Take Needed` should live in the open-storage context and move storage -> player, while Quick Stack's primary action is world interaction moving matching player items -> storage.

Reserve/surplus-aware Quick Stack integration is P1 and requires separate evidence.

A world-level "Take Needed" action without opening the chest is also deferred from P0. It may be researched later, but it must not take over Quick Stack's world-interaction surface without an explicit compatibility decision.

## Git / version / acceptance workflow

- `main` is the stable/documentation baseline.
- Research-only code uses `research/<topic>`.
- Build-bearing production work uses `dev/<version>` or a clear feature branch.
- Research-only work does not consume production version numbers.
- Runtime behavior reaches `main` only after exact build/runtime evidence and explicit user acceptance.
- Handed numbered artifacts are immutable and tied to exact source identity.
- Stable binaries are published through GitHub Releases.

## Runtime harness specifics

Prefer one narrow research harness over repeated manual tests when runtime evidence is required.

Current remaining runtime/product priorities:
- complete the explicit UI/UX design pass: CraftGUI and ordinary HUD surfaces must share one visual/layout specification, occupy the same apparent screen position, use native item icons, and remain compact/readable;
- choose and verify the final production pin/manage input gesture, including gamepad focus/compatibility; Probe 0.2.0 explicitly verified mouse focus but did not record a gamepad build-focus event;
- isolate carried-bag/toolbelt and capacity-limited transfer edges only if production correctness still depends on runtime evidence beyond the already verified native inventory path.

The ordinary gameplay HUD anchor/lifecycle and live-localization gate are accepted from the 0.1.4 runtime pass.

The build/world-project requirement model itself is runtime-accepted for the two P0 families documented above.

Research probes must be nonpersistent where possible and must not ship as production.

## CI / build specifics

This is a public repository. Standard GitHub-hosted runner minutes are not treated as scarce.

A Windows standard runner is an acceptable default for BepInEx/managed-code builds when it best matches the established NikichMods toolchain.

Use minimal compile-time reference stubs containing only verified type/member signatures. Never commit proprietary game assemblies. Verify that reference-stub DLLs cannot leak into distributed artifacts.

## Long-lived sources of truth

- `AGENTS.md`
- `docs/PRODUCT_SCOPE.md`
- `docs/VERIFIED_GAME_DATA.md`
- `docs/RESEARCH_REPORT_2026-09-21.md`
- `docs/TEST_LOG.md`
- later architecture/release docs as actually needed

When chat memory conflicts with accepted repository evidence, investigate and update the canonical repository record.
