# Crafting Planner — Product Scope

Updated: 2026-09-22

## Product model

Crafting Planner is a functional replacement/superset for the Recipe Pin idea, but its P0 focus is **projects**, not arbitrary production recipes.

Player loop:

`choose projects -> aggregate final required materials -> compare with carried inventory -> collect only Missing from the current storage -> travel/build/repair manually`

The mod reduces memory burden and repetitive inventory transfer without becoming a remote-crafting or logistics network.

## P0 goals

Runtime-verified P0 project categories:

- repeatable construction/build projects exposed by a native builder as `ObjectCraftDefinition` with concrete `needs`;
- one-time builder upgrades exposed in the same build UI with concrete `needs`;
- world repair/clearing projects exposed from the interacted object as a concrete `CraftDefinition` with `needs` and a world-state change (`change_wgo`).

Other scripted/special project shapes are not implicitly supported merely because they look like projects to the player; add them only after their real host data path is verified.

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


## Accepted project requirement model

Research Probe 0.2.0 established that the supported P0 families can use one planner-side requirement model without reproducing execution logic:

- resolve the exact host craft/object-craft definition;
- read its exact `needs`;
- store only minimal goal identity + planner quantity;
- derive aggregated `Required` on demand;
- never simulate the repair/build/clearing action itself.

Builder-internal pseudo actions such as remove/move controls are not planner projects. Production filtering should use semantic host data/context rather than a copied list of recipe IDs.


## P0 goal management UX

P0 intentionally does **not** use a separate planner-management window.

Goal quantity is managed directly from the currently focused supported project card in the game's existing project/build craft UI:

- add/increment acts on the focused project;
- decrement acts on the focused project;
- decrement from quantity 1 removes the goal;
- one-off repairs, clearings, and one-time upgrades are capped at quantity 1;
- repeatable placeable construction projects may have quantity greater than 1.

The planner HUD is display-only in P0. It does not own navigation, buttons, or a second focus system.

Concrete key/button bindings remain an implementation/compatibility detail. Vanilla CraftGUI X/Y must not be repurposed if doing so overrides their existing behavior. The first 0.1.0 candidate uses LT/RT for direct-card decrement/increment and keeps Y for Take Needed in an open chest.
