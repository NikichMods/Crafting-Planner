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

## 4. Clean first recipe support

Recommended first production support set:

1. ordinary static crafting recipes represented by concrete `CraftDefinition.needs`;
2. building recipes represented by `ObjectCraftDefinition.needs`.

Do not claim universal recipe support in the first build.

Exclude initially until verified:
- mixed alchemy;
- multiquality ingredient-selection recipes;
- survey/research;
- fixing/world repairs;
- body/autopsy operations;
- resurrection;
- prayer;
- refugee/special scripted crafts.

The rule should be semantic, not a giant hardcoded ID list.

## 5. UI recommendation

Use one small planner HUD with two logical sections:

**Goals**
- recipe icon/name;
- target quantity;
- remove/edit affordance.

**Materials**
- aggregated exact item list;
- Required;
- Have;
- Missing.

Do not mirror the game's recipe database in UI state. Resolve current definitions and derive materials when the planner redraws.

Keep redraw event-driven/on-demand:
- pin added/removed/quantity changed;
- Take Needed completed;
- relevant inventory UI operation/lifecycle event.

No per-frame inventory scan.

## 6. Input/control options

Do not freeze a global key yet.

Static game bindings and current Recipe Pin/Quick Stack history show that F, X, Y, triggers, tabs and D-pad already have contextual ownership/conflict risk.

Best P0 direction:
- recipe pin action: contextual craft/build menu control using a verified focused/hovered recipe seam;
- Take Needed: contextual action while **ChestGUI is open**, not a new world interaction;
- expose visible native-style button help;
- make keyboard/gamepad choices configurable only after the runtime probe confirms which chest-context action is genuinely free.

This intentionally avoids competing with Quick Stack's world “Stack” action.

## 7. Persistent state

**Not required for P0.**

Keep goals session-local for the first implementation. This avoids save identity/migration/state-cleanup risk.

If later persistence is approved, make it per-save and verify stable recipe identity first. Do not use one global config list as if it were save data.

## 8. Compatibility risks

### Recipe Pin

Current Recipe Pin already owns:
- craft/build pin gestures;
- a HUD;
- one pinned recipe state.

Running both planners can produce duplicated UX even without a technical crash.

A product decision is required: coexistence vs “Crafting Planner supersedes Recipe Pin”.

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

## 10. Product decisions still needed

Material decisions:

1. **Relationship to Recipe Pin:** functional replacement/superset, or deliberate coexistence?
2. **First-release recipe promise:** approve “ordinary static craft + building” as the advertised supported subset, with special recipe systems excluded until verified?
3. **Session persistence:** accept session-only pins for P0, leaving per-save persistence to P1?
4. **Bags:** recommended P0 behavior is to count carried bag contents as Have and allow native transfer into carried bags when capacity permits. Approve this semantic?
5. **Goal quantity UX:** recommended P0 is a planner-owned quantity field rather than trying to mirror the craft menu's private amount widget. Exact +/-/gamepad presentation can be tuned after the UI probe.

No keybinding decision is requested yet; evidence should choose the safe candidates first.

## Next verification step

Build a research-only probe that proves four facts in one installed-game session:

1. normal craft and build definitions can be captured at a narrow public UI lifecycle seam;
2. planner player-only count differs correctly from world-zone craft availability;
3. `Take Needed` can transfer a bounded amount from the currently open storage through native `MoveItemTo`;
4. a chest-context action can be added without consuming the existing vanilla/world Quick Stack interaction.

The probe must not create persistent pin/save state and must emit concise diagnostics for one returned log.
