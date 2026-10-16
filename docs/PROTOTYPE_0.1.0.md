# Crafting Planner 0.1.0 — P0 interaction model

> Historical record. Describes the original candidate/research stage; later evidence supersedes its pending gates and proposed bindings. See [DEVELOPMENT.md](DEVELOPMENT.md) for current recovery, [PRODUCT_SCOPE.md](PRODUCT_SCOPE.md) for active requirements and [TEST_LOG.md](TEST_LOG.md) for acceptance.

Status: first production candidate; runtime acceptance pending.

## Direct-card management

P0 intentionally has no separate planner-management window.

While a supported project card is focused in the game's existing project/build craft UI:

- gamepad **RT**: add the project or increment a repeatable project's planner quantity;
- gamepad **LT**: decrement; quantity 1 -> 0 removes the project;
- keyboard **+ / =**: add/increment;
- keyboard **-**: decrement/remove.

One-off repairs, clearings, and one-time builder upgrades are capped at planner quantity 1.
Repeatable BuildType.Put construction projects may have quantity > 1.

Vanilla X/Y are not repurposed in CraftGUI because X/Option1 participates in queue behavior and Y/Option2 opens recipe information / queue behavior.

LT/RT use inherited sub-tab actions that CraftGUI itself does not consume. With the user's current Max Buttons Redux 1.4.0 stack, its RT/LT amount shortcuts are explicitly suppressed for the accepted project families (builddesk, fix, broken, blockage, one-time).

## Planner HUD

The HUD is display-only: a cloned native NGUI label inside the vanilla HUD hierarchy. It shows pinned projects and quantities plus aggregated Required / Have / Missing materials. There are no custom interactive buttons.

## Take Needed

Inside the currently open chest, gamepad Y / Option2 performs Take Needed when planner goals exist. Transfer is bounded by Missing, current storage availability, and native player capacity; each material transfers independently; no surplus is taken.

The 0.1.0 candidate intentionally does not add a separate keyboard Take Needed binding yet; this first runtime pass validates the already-proven gamepad chest seam together with multi-goal aggregation.

## Deferred

- automatic decrement after confirmed build/repair completion;
- separate management screen;
- reorder;
- persistence across sessions;
- Quick Stack reserve integration.


## 0.1.1 correction

Runtime evidence from 0.1.0 showed that the attempted keyboard + / - hook targeted a non-declared `CraftGUI.Update` method and caused Harmony `PatchAll` to throw during startup. The 0.1.1 candidate removes that unverified keyboard hook instead of broadening to `BaseGUI.Update`.

The gamepad RT/LT semantic path is retained because the returned runtime log directly showed it adding/removing both a world-repair project and a repeatable builder project.

The planner label is now cloned under the native reference label's existing parent rather than reparented directly to the HUD root. This follows NGUI's runtime warning that a widget must remain under the `UIPanel` hierarchy that manages it. The exact visual placement still requires runtime acceptance.

Keyboard goal management is therefore temporarily deferred until a narrow verified input seam is established.
