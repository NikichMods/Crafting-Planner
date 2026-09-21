# Crafting Planner — Product Scope

Updated: 2026-09-22

## Product model

Crafting Planner is a functional replacement/superset for the Recipe Pin idea, but its P0 focus is **projects**, not arbitrary production recipes.

Player loop:

`choose projects -> aggregate final required materials -> compare with carried inventory -> collect only Missing from the current storage -> travel/build/repair manually`

The mod reduces memory burden and repetitive inventory transfer without becoming a remote-crafting or logistics network.

## P0 goals

Supported product categories to implement after their host paths are verified:

- repeatable construction/build projects, such as several benches or workstations;
- one-off world projects such as repairs, upgrades, or clearing blocked objects, when the game exposes stable concrete requirements.

Ordinary workstation production is not a P0 goal. Examples intentionally excluded as standalone pins:
- make 10 planks;
- make 10 polished stone;
- make a chisel.

Those items can still be **materials** required by a project.

P0 does not expand dependencies. If a bench needs planks, the planner asks for planks; it does not turn that requirement into logs or another production plan.

## Quantity semantics

- Repeatable placeable/build projects may have quantity > 1, e.g. `Church Bench x3`.
- One-off repair/upgrade/clearing projects are quantity 1.
- Quantity belongs to the planner goal, not to a workstation craft queue or private amount widget.

## Materials

Across all pinned projects:
- `Required` = total exact material demand;
- `Have` = material physically carried by the player, including carried bags when native semantics support them;
- `Missing = max(Required - Have, 0)`.

## Take Needed

P0:
- only from the currently open/explicitly interacted storage;
- only exact missing material IDs;
- never surplus;
- partial transfer is success;
- unavailable/full items do not block other materials;
- use native inventory capacity/transfer behavior.

A world-level action that takes materials without opening the storage is not P0. It may be researched later because it overlaps Quick Stack's world-interaction surface.

## Persistence

P0 goals are session-local. Per-save persistence is a later upgrade if the implementation cost/risk is justified.

## UI

Desired production presentation is vanilla-style.

Preferred technical direction:
- use the game's native NGUI-style HUD hierarchy and visual components where practical;
- reuse/clone verified native fonts, panels, icons, sprites, and gamepad conventions;
- keep the planner layout custom and narrow rather than coupling to one unrelated vanilla screen.

A generic BepInEx/IMGUI text/debug overlay is suitable for research only, not the target release UI.

## Deferred

- per-save persistence;
- automatic project decrement after confirmed completion;
- Quick Stack reserve/surplus integration;
- dependency expansion/intermediate raw-material planning;
- multiple/nearby storage collection;
- world-level Take Needed without opening storage;
- remote crafting;
- alternative recipe/workstation planning.
