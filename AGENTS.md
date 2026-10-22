# Building Planner — Working Contract

This repository follows the canonical global development rules in `NikichMods/DevRules`. Start with `WORKING_RULES.md`; local checkout work also follows `CODEX_WORKFLOW.md`.

Before substantive technical work, read:
- `ENGINEERING_RULES.md`
- `CI_POLICY.md`
- `GIT_WORKFLOW.md`
- `PROJECT_BOOTSTRAP.md`
- `RUNTIME_TEST_HARNESS.md` when installed-runtime evidence is relevant

This file contains only project-specific additions, verified constraints, and explicit scope decisions.

## Project identity

- Project: **Building Planner** (stable technical identity: CraftingPlanner)
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

### Automatic completion

Automatic reduction/removal after verified native commitment is implemented and accepted for representative placement, repair/clearing, builder upgrade and home-interior paths. PRODUCT_SCOPE.md and TEST_LOG.md own supported-family bounds. Cancellation/failure preserves goals; ordinary production remains outside scope.

### Not P0

Do not pull these into the first production scope automatically:
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

Per-save/version goal persistence is implemented and user-accepted under docs/PLAN_PERSISTENCE.md/TEST_LOG.md, including restart, two slots, deletion/empty plans and unsaved-edit rollback. Local path/binary-hash snapshots do not automatically travel with cloud/copy/rename.

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

Current release baseline: accepted0.1.28 behavior is frozen as1.0.0 with metadata-only runtime changes, no F2 editor and no installed research helper. User2026-10-19 requests document reconciliation and a frozen ready build; Nexus material/publication is separate. Do not reopen accepted mechanics/language/long-plan/bag scenarios without a new defect. docs/RELEASE_1.0.0.md owns readiness/audits and DEVELOPMENT.md owns exact artifact/install state. Do not claim a native1.0.0 launch before observation or infer publication from preparation.

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

## Shared Graveyard Keeper research

Cross-project Graveyard Keeper 1.407 host/runtime research is centralized in `NikichMods/GraveyardKeeperResearch`.

Before starting a fresh investigation into vanilla/game-engine/UI/NGUI/data/lifecycle behavior:

1. read this repository's own canonical verified-data / architecture docs first;
2. consult `NikichMods/GraveyardKeeperResearch/docs/RESEARCH_INDEX.md` and the linked shared knowledge documents;
3. search accepted local/shared test evidence and relevant history if the result has not yet been promoted;
4. perform new static/runtime research or a probe only if the question remains open.

Project-specific mechanics, product/UX decisions, release state, and build acceptance remain canonical in this repository. Reusable host/runtime facts that can serve multiple Graveyard Keeper mods should be promoted back into the shared research repository after acceptance rather than left only in chat, commit history, or a test log.


## Local development recovery

Read docs/DEVELOPMENT.md for the exact prepared1.0.0 source/CI/artifact/install state and docs/RELEASE_1.0.0.md for release audit. TEST_LOG.md owns bounded native acceptance.1.0.0 must preserve accepted0.1.28 mechanics and visuals; normalized source/compiled parity is checked with version literals/metadata excluded. Keep PluginGuid, canonical CraftingPlanner.dll, persistence schema/path and all accepted defaults.

Accepted representative behavior includes placement/cancellation/exactly-once decrement; repair/clearing; yard upgrade; one-time receiver placement/cancellation; home cupboard Cancel/Apply; save/reload/restart/two-slot/deletion/empty/unsaved rollback; mouse/keyboard/gamepad/current-chest stock/partial/full/recovery; no-F2/all11-language/chest/long-word width; carried builder-bag ownership and5-item native bag-only transfer at ordinary capacity0. Authored57 builder cases (39 upgrades/17 placements/1 no-needs exclusion) and39 interior cases are compiled policy coverage, not57/39 native executions. Lamp1/nonzero cases are not independently replayed and dedicated lamp2->1 replay was declined. CARRIED_BAG_CHECK.md owns bag-family bounds; do not generalize to toolbelt/nested/all bags/foreign direct-list changes.

All temporary resource/observer sidecars are backed up and absent. CarriedBagSetup0.1.0 was removed after accepted return; do not restore it without new need. Max Buttons redraw failure reproduced with Planner absent; necessity control is complete, exact third-party/native fault ownership is not fully isolated and repair remains deferred by user. No automatic re-investigation/suppression. Shared host facts are promoted on research branches in GraveyardKeeperResearch; product/release acceptance stays here.

Player-side plan files are mod-owned local snapshots isolated by native slot path/binary SHA256 after verified disk commitment; native save payload remains unchanged. Long plans may extend below the screen by explicit user choice, with no scrolling/clipping/cap redesign. Longest measured words expand the card left while right numeric columns stay fixed; CJK native character wrapping remains. No further visual tuning or full test-matrix replay is requested. Stable publication and Nexus work are separate from the current release preparation.

User subsequently reports NPC-card overlap and adopts conditional left placement of both cards together. Active dev/1.0.1 uses the verified native relation UIPanel common parent (including anchors/tasks), adjusted before its native matrix/widget draw consumer. NPC_CARD_AVOIDANCE.md owns READY evidence and limited-screen policy. Single-card overlap resolution is user-accepted with13 logged shift/restore cycles; simultaneous two-card native QA remains pending.1.0.0 stays immutable; no gameplay/plan layout/persistence change or new probe is requested.

User adopts native location preservation; active dev/1.0.3 corrects rejected1.0.2 for configured non-unit HUD scale. LOCATION_CARD_PRESERVATION.md owns the READY gate: original name/status/quality block left of fixed Planner, exact empty no-zone placeholder hiding only without description, equal horizontal shift for both normal and additional NPC anchor families, and residual overlap clearance. 1.0.3 cemetery name/quality and fixed Planner are user-accepted, with scaled shifts/restores/placeholder hiding corroborated by log; additional-only/two-card visual coverage remains pending. All handed bytes stay immutable.

User requires Planner to follow native cutscene HUD hiding/fade and automatic return. Active dev/1.0.4 uses actual HUD panel alpha plus active state, synchronizes owned panel before mesh and compares view transitions in existing LateUpdate without repeated inventory/row work. Menu contexts remain independent. CUTSCENE_HUD_VISIBILITY.md owns READY evidence and accepted representative prayer/bishop hide/automatic return, original plan and chest/build continuity; do not infer cutscenes from dialogue or blocked controls.

Active dev/1.0.5 removes only proven menu work/allocation under MENU_OPEN_PERFORMANCE.md: unchanged-visibility focus/blur updates hints without full card rebuild, quantity key built once, fixed label list reused. Preserve native layout callbacks and both GJL font passes; native cache inspection rejected naive deletion of the initial pass. User declines mandatory profiling-first for this bounded cleanup; no speedup estimate or native acceptance is implied. No profiling/helper installed.
