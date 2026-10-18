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

### Required follow-up

Automatic reduction after verified successful placement/material commitment is required next and absent from 0.1.16. Separate upgrade/repair/clearing success boundaries must be verified under `docs/PRODUCT_SCOPE.md`. This supersedes its initial deferral without expanding ordinary production scope.

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

Initial P0 used session-local goals. The user now requires per-save persistence before release; 0.1.25 implements a candidate under docs/PLAN_PERSISTENCE.md, with native acceptance pending.

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
- verify exact successful-operation identity/boundary and exactly-once reduction for builder placement, then separate upgrade/repair/clearing paths;
- retain the user-accepted 0.1.16 visual presentation and exercised mouse/gamepad gestures; acceptance is bounded by `docs/TEST_LOG.md`, not a claim of exhaustive host-flow or language/context coverage;
- isolate carried-bag/toolbelt and capacity-limited transfer edges only where production correctness still needs evidence;
- review temporary calibration and release readiness at the functional milestone; do not infer stable promotion from visual acceptance.

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

Read docs/DEVELOPMENT.md for exact installed identity/build/evidence and docs/UI_IMPLEMENTATION.md for gates. Installed dev/0.1.24 preserves the user-accepted0.1.19 visuals: independent plain/cost-row materials, shared cost/counter settings, bare centred quantities, measured arrow spacing and cap20/one-off1 arrow visibility. F2 format10 has60 individually resettable sliders. Defaults/acceptance are owned by TEST_LOG.md and PRODUCT_SCOPE.md; this is not full release acceptance.

Repeatable-placement reduction has bounded0.1.20 native acceptance (9 once-only decrements, zero removal, unrelated goal preservation, cancellation and4 busy failures).0.1.21 repair/clearing removal and unrelated repair preservation pass.0.1.22 adds equivalent one-time builder None/auto/no-wait/end_script completion and one-time Put removal using existing verified hooks/receipts. The authored one-time audit57 has compiled dispatch coverage39 upgrades/17 placements/1 no-needs exclusion, not57 runtime acceptances. The0.1.22 yard return verifies one native completion/removal1->0 with no duplicate or tracker error; representative one-time Put cancellation/success/control preservation now pass in0.1.23. Read PLACEMENT_COMMIT_RESEARCH.md and ONEOFF_COMPLETION_RESEARCH.md before repeating research.

YardUpgradeSetup0.1.0 F8 was backed up and removed after the0.1.22 yard return with host closed; passive OneOffCompletionProbe is also absent. Exact helper/core/log identities and bounded acceptance are in TEST_LOG.md. Earlier probe/setup returns remain historical evidence.

Home furniture/decor callback placement has a separate unverified apply boundary and is not included as an instant one-time upgrade. Max Buttons remains explicitly deferred until compatibility/release review with unresolved cause/ownership; the old explicit planner-prefix exception is absent but9 dynamic Redraw exceptions remained in0.1.21 technology tabs. Do not attribute them solely to another mod or suppress native Redraw without evidence. Ordinary HUD across all11 languages is user-reported from0.1.11; gamepad/all-language acceptance remains bounded/open. Per-save persistence remains deferred.


Current0.1.23 adds quantity20 for the three empty lantern_place choices only; normal world completion decrements1, other world repair/upgrade goals remain1. Exact candidate/CI/hash/install and pending native tests are in DEVELOPMENT.md/TEST_LOG.md. OneTimePlacementSetup0.1.0 was backed up/removed after the return with host closed; all temporary completion/resource sidecars are absent.0.1.23 native return verifies receiver cancellation without decrement, two once-only placements and control preservation; lamp_2/3 counters20/19 and lamp_3 final1->0. Nonzero auto-decrement and lantern_1 live cap/cross-point visual readback were not independently logged. TEST_LOG.md owns bounds; no stable promotion. Shared static lamp note is on research/lighting-points, not shared main.

Current research handoff: active research/interior-completion keeps installed production0.1.23 unchanged. INTERIOR_COMPLETION_RESEARCH.md owns39 home callback definitions, early native finish versus later apply/cancel and the BLOCKED production gate. InteriorCompletionProbe0.1.0 + F8 InteriorResourceSetup0.1.0 are temporarily installed (exact source/hashes/receipt in TEST_LOG.md/DEVELOPMENT.md), pending one native cancel/apply pass. They observe/setup only; production intentionally retains interior goals. Collect return and remove both sidecars; preserve declined lamp replay and deferred Max Buttons decisions.


Interior research return is now preserved:7 table/cupboard attempts verify4 cancellations with refund/restoration and3 confirmed applies retaining spent materials; both sidecars were removed with host closed. INTERIOR_COMPLETION_RESEARCH.md owns the READY native boundary, exact snapshot and pending production implementation/new-candidate gate. Production remains0.1.23 with no callback-interior auto-removal; do not restore probes or repeat discovery. Shared facts are on research/interior-completion a3079a2, not shared main. Prior installation/pending statements are historical.


Current0.1.24 supersedes the research-only handoff: installed production now removes supported home callback goals after one correlated Apply, normal exact-script termination and normal native decision return, preserving preview/Cancel goals. Exact source09f4febf/CI/hash/install and pending native acceptance are in DEVELOPMENT.md/TEST_LOG.md; INTERIOR_COMPLETION_RESEARCH.md owns gates. Separate interior hook owner preserves prior world/placement handlers, visuals, quantity1/native resources. Existing F8 InteriorResourceSetup0.1.0 is temporarily restored; passive probe remains absent. Native check is preview/cancel retention then Apply removal with control preserved. Remove helper after return, do not repeat discovery. This is not stable acceptance; Max Buttons deferred, lamp replay declined.


0.1.24 cupboard return is now verified: Cancel retains the original1 goal, subsequent Apply removes it once after the script, both control carpet goals remain. TEST_LOG.md/INTERIOR_COMPLETION_RESEARCH.md own bounded acceptance (not39 production native passes/full release). Exact F8 resource helper was backed up/removed with host closed; core unchanged. Earlier pending/temporary-helper statements are historical. Do not repeat the closed callback gate; release/localization/gamepad/capacity/compatibility acceptance remains separate, Max Buttons deferred to that pass and dedicated lamp replay declined.


Per-save persistence is required before release (user2026-10-18). Candidate0.1.25 stores only goal keys/quantities in mod-owned snapshots isolated by native slot path and binary SHA256, at verified disk commitment; existing session reset restores after clearing completion trackers. PLAN_PERSISTENCE.md owns the READY gate, format, failure semantics and limits. Native save/reload/restart/two-slot/empty-plan acceptance remains pending. PRODUCT_SCOPE.md owns product semantics; DEVELOPMENT.md owns exact installed identity.
