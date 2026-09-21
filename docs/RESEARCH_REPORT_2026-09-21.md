# Initial Research Report — 2026-09-21

Project: **Crafting Planner**
Target: **Graveyard Keeper 1.407**

This report is the initial discover/verify result. It deliberately stops short of production behavior where runtime evidence or a product decision is still required.

## 1. Native seams found

Strong native seams exist for all core data operations:

- recipe identity and ingredients: `CraftDefinition` / `ObjectCraftDefinition`;
- canonical recipe resolution: `GameBalance`;
- craft-menu recipe presentation: `CraftItemGUI.Draw(CraftDefinition)`;
- native availability/consumption: `MultiInventory`;
- exact-storage inventory: `WorldGameObject.GetMultiInventoryOfWGOWithoutWorldZone(true)`;
- capacity: `MultiInventory.CanAddCount(...)`;
- transfer: `MultiInventory.MoveItemTo(...)`;
- chest lifecycle/UI: `ChestGUI.Open(WorldGameObject)`, inventory panels, `Hide`;
- craft completion exists at a semantic `CraftComponent` boundary, but is P1 research rather than P0.

## 2. P0 feasibility

**P0 is realistic.**

Low-risk core:
- multiple in-memory goals;
- quantity field per goal;
- aggregate exact item requirements;
- player-only Have;
- Missing calculation;
- current-storage availability;
- bounded native transfer;
- partial success;
- independent per-ingredient transfer.

Research-sensitive parts:
- exact pin input/callback seam;
- final HUD construction/anchors;
- conflict-safe chest action;
- clean inclusion/exclusion rules for special recipe categories.

Potentially fragile if done badly:
- reading private UI fields by reflection;
- global input hooks;
- treating world-zone craft inventory as player Have;
- manually reproducing capacity/stacking;
- assuming all recipe categories are equivalent.

## 3. P0 difficulty/risk split

### Easy / structurally clean

- goal list model;
- target quantity model;
- aggregation;
- Required/Have/Missing arithmetic;
- player + carried-bag counting;
- “take no surplus” calculation;
- native chest -> player transfer;
- partial transfer semantics.

### Requires runtime verification

- pin/unpin trigger in craft/build GUI;
- gamepad/keyboard action placement;
- HUD anchor/lifecycle around menus/cutscenes;
- exact chest refresh/focus after batch transfer.

### Defer as higher-risk

- universal special-recipe support;
- automatic goal decrement on confirmed completion;
- save persistence;
- arbitrary Recipe Pin private-state integration;
- Quick Stack reserve interception.

## 4. Product support boundary — updated 2026-09-22

The original research showed that ordinary static workstation recipes are technically clean to read, but the product decision is narrower and project-focused.

P0 planner goals are:
1. repeatable construction/build projects represented by verified `ObjectCraftDefinition` data;
2. one-off world repairs/upgrades/clearing projects when their real host path exposes a stable concrete material list.

Ordinary workstation item production is **not** a P0 goal. The player does not pin "10 planks", "10 polished stone", or "1 chisel" as production tasks. Those items may appear as materials required by a pinned project.

There is no dependency expansion in P0: if a project requires planks, the planner asks for planks; it does not automatically turn that into logs or pin the plank recipe.

World repair/clearing support is now a P0 product requirement, but it remains an evidence gate because different one-off interactions may not all share `ObjectCraftDefinition` semantics.

Dynamic/special systems such as mixed alchemy, multiquality ingredient-selection recipes, survey/research, body/autopsy operations, resurrection, prayer, and refugee scripted production remain outside P0 unless separately approved and verified.

## 5. UI recommendation

Use one small **vanilla-style** planner HUD with two logical sections:

**Goals**
- recipe icon/name;
- target quantity;
- remove/edit affordance.

**Materials**
- aggregated exact item list;
- Required;
- Have;
- Missing.

Prefer the game's native NGUI-style visual language (fonts, panels, icons, button/gamepad conventions) or a narrow hybrid built inside the native HUD hierarchy. Do not ship a generic BepInEx/IMGUI debug-looking panel as the production UI.

Do not mirror the game's recipe database in UI state. Resolve current definitions and derive materials when the planner redraws.

Keep redraw event-driven/on-demand:
- pin added/removed/quantity changed;
- Take Needed completed;
- relevant inventory UI operation/lifecycle event.

No per-frame inventory scan.

## 6. Input/control options

Do not freeze a global key yet.

Static game bindings and current Recipe Pin/Quick Stack history show that F, X, Y, triggers, tabs and D-pad already have contextual ownership/conflict risk.

Best P0 direction after runtime probe:
- project pin action: contextual build/project UI control using a verified focused/hovered project seam;
- Take Needed: contextual action while **ChestGUI is open**, not a new world interaction;
- expose visible native-style button help;
- open-chest `Option2 / Y` is now a strong gamepad candidate because it worked in the user's real mod stack, but do not own Y globally.

A world-level "Take Needed" without opening the chest is a later research option, not P0, because it overlaps the same interaction surface where Quick Stack already operates.

## 7. Persistent state

**Decision: session-local for P0.**

The user accepted disappearing goals after restart as an intentional first-version simplification. This avoids save identity/migration/state-cleanup risk.

Per-save persistence remains a later upgrade. If implemented, verify stable project identity first; do not use one global config list as if it were save data.

## 8. Compatibility risks

### Recipe Pin

Current Recipe Pin already owns:
- craft/build pin gestures;
- a HUD;
- one pinned recipe state.

Crafting Planner is intended to supersede Recipe Pin functionally. Recipe Pin remains useful as a compatibility test during development, but is not a dependency and should not be required for production.

Do not depend on Recipe Pin private internals for P0.

### Quick Stack

Quick Stack owns a world chest “Stack” interaction and historically had an Xbox Y conflict with Recipe Pin.

Crafting Planner should avoid its world-interaction patch area for P0. An open-chest `Take Needed` action has a much smaller collision surface.

P1 reserve protection/surplus-only Quick Stack behavior will require an explicit integration boundary.

### Other UI/input mods

Queue Everything is a known reason Recipe Pin moved to hold-X behavior.
HUD scale/position mods can affect layout.
Therefore planner input and HUD anchoring need runtime verification, but no generalized compatibility framework is justified yet.

## 9. Recommended minimum architecture

Keep four small responsibilities:

1. **Goal store**
   - in-memory list of `recipeId + quantity` and only additional identity needed to resolve normal vs object craft;
   - no copied ingredients.

2. **Requirement calculator**
   - resolve host recipe definition;
   - reject unsupported semantics safely;
   - aggregate exact `needs`;
   - read player-only Have;
   - compute Missing.

3. **Planner HUD**
   - render current goals + aggregated material status;
   - redraw on explicit state changes.

4. **Current-storage collector**
   - active only for the currently open storage;
   - compute bounded move per item;
   - use native `MultiInventory` capacity/transfer;
   - continue after individual ingredient failure;
   - redraw chest/player/planner UI afterward.

This needs only a small number of narrow Harmony seams if runtime verification confirms the candidate lifecycle methods.

## 10. Product decisions resolved — 2026-09-22

1. **Recipe Pin:** Crafting Planner is its functional replacement/superset, not an add-on that depends on it.
2. **Goal type:** P0 goals are construction and concrete world projects (repair/upgrade/clearing), not arbitrary workstation production recipes.
3. **Persistence:** session-only goals are accepted for P0; per-save persistence is a later upgrade.
4. **Bags:** carried bag contents count as Have, and native transfer may use carried bag capacity when the host allows it.
5. **Quantity:** planner-owned quantity means number of repeatable project instances (for example, three benches). One-off projects remain quantity 1.

The returned Probe 0.1.0 log also closed the player-Have, current-storage transfer, and open-chest Y questions.

## Next verification step

The remaining P0 evidence target has changed with the product scope:

1. capture an ordinary repeatable construction project through the build UI;
2. inspect representative one-off world repair/upgrade/clearing interactions and determine whether they use `ObjectCraftDefinition`, ordinary `CraftDefinition`, or a separate scripted path;
3. verify their exact displayed/consumed material list can be mapped without a hardcoded per-ID database;
4. then implement the smallest production goal model and vanilla-style HUD around the verified project seams.

No new research is needed for ordinary workstation production recipes unless they later become part of the product scope.
