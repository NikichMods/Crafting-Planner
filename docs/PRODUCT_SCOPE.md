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

P0 goals are session-local. Per-save persistence is an explicit desired follow-up (reaffirmed 2026-10-15): the same save must recover its own pinned goals and quantities after exiting and restarting the game. It remains outside the current UI implementation; do not implement one global cross-save goal list or infer permission to change save data in this pass.

## UI

Desired production presentation is vanilla-style.

Preferred technical direction:
- use the game's native NGUI-style HUD hierarchy and visual components where practical;
- reuse/clone verified native fonts, panels, icons, sprites, and gamepad conventions;
- keep the planner layout custom and narrow rather than coupling to one unrelated vanilla screen.

A generic BepInEx/IMGUI text/debug overlay is suitable for research only, not the target release UI.

## Deferred

- per-save persistence;
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

Gamepad support is mandatory for P0 (explicitly reaffirmed by the user on 2026-10-15). All planner actions must be usable without a mouse: focused-project quantity changes and current-storage Take Needed. Provide context-appropriate button hints, preserve native menu focus/navigation, and keep ordinary HUD display from capturing input. The existing LT/RT and open-chest Y mappings are the prototype baseline, not blanket evidence of final input compatibility or support for every controller's glyphs. Final bindings, glyph presentation and compatibility need verification on the supported gamepad path.

The 0.1.6 baseline intentionally does **not** use a separate planner-management window. The user has since requested remote plan management; see the current follow-up scope below. Its entry gesture is not yet adopted.

Goal quantity is managed directly from the currently focused supported project card in the game's existing project/build craft UI:

- add/increment acts on the focused project;
- decrement acts on the focused project;
- decrement from quantity 1 removes the goal;
- one-off repairs, clearings, and one-time upgrades are capped at quantity 1;
- repeatable placeable construction projects may have quantity greater than 1.

In the 0.1.6 baseline, the goal-list presentation is display-only. The follow-up requests deliberate editing outside the builder; ordinary gameplay must still not capture focus unintentionally.

Planner state should remain visible while the player is managing goals in the native project/build UI. RT/LT (or later accepted equivalents) should update the displayed goal quantity/material totals immediately, without requiring the player to close the craft window first. After the craft/project UI closes, the same planner state remains available on the ordinary gameplay HUD.

Concrete key/button bindings remain an implementation/compatibility detail. Vanilla CraftGUI X/Y must not be repurposed if doing so overrides their existing behavior. The first 0.1.0 candidate uses LT/RT for direct-card decrement/increment and keeps Y for Take Needed in an open chest.


## Localization requirement

Crafting Planner must support every language exposed by Graveyard Keeper 1.407.

Current host language set:
- `en`
- `de`
- `fr`
- `pt-br`
- `es`
- `ru`
- `it`
- `pl`
- `ja`
- `zh_cn`
- `ko`

Rules:
- host-owned project and item names must be resolved from the game for the current language rather than cached as localized planner state;
- Crafting Planner-owned labels must have translations for every host language above, with English fallback only for unknown/unsupported external locale codes;
- changing the game language at runtime must refresh Planner text and the correct host font/atlas without restarting the game;
- localization must remain a small presentation layer and must not duplicate the game's item/project localization database.

## UI design checkpoint

The current text-only Planner output is a technical presentation scaffold, not the intended final visual design.

Accepted UX direction from the 0.1.4 runtime pass:

- the Planner must appear in the **same apparent screen position, size, hierarchy, and visual style** while the native project/build UI is open and after it closes;
- switching between the CraftGUI surface and the ordinary gameplay HUD should look like one persistent Planner card, not two different interfaces jumping between positions;
- both host-native surfaces should share one layout/rendering specification and the same planner state;
- the material presentation should be icon-led, using the game's own item icons rather than spelling out every item name as the primary visual;
- text should be minimal and information-dense; the title and section labels must not dominate the panel;
- Required / Have / Missing must remain unambiguous, but they may be encoded compactly with counts, state color/checks, and spacing rather than three long repeated labels per material;
- prefer a small readable backing/panel only when it materially improves contrast; avoid a large opaque management window;
- visual density should scale by content, with overflow handled deliberately rather than letting the panel grow across the playfield.

The 0.1.4 ordinary-HUD anchor mechanism is accepted as a technical placement substrate. The current CraftGUI-vs-HUD visual mismatch is not accepted as final UX.

After the following are runtime-accepted:
1. planner display inside the native project/build UI;
2. planner display in ordinary gameplay HUD;
3. live language switching;

stop before visual polish and explicitly run a UI/UX design pass with the user.

That design pass should decide the visual hierarchy, backing panel, icons/text balance, Required/Have/Missing presentation, compactness, and final placement. Only after the native anchor/coordinate model is verified should temporary F1 live-calibration controls be considered for fine tuning such as position, width, font scale, spacing, backing opacity, or similar presentation-only values.

### Adopted card direction — 2026-10-15

The user selected aligned material rows with one shared Have / Required / Missing header. Keep a small title, short goal rows with quantities and native item icons. The alternative tile layout is not selected.

The card must feel visually light: no thick border, heavy frame, or opaque window. Use a subtle translucent backing, strongest at the top-right and fading toward near-transparency at the bottom-left; exact opacity and spacing require in-game visual acceptance. Keep text and icons readable independently of backing opacity.

The card's content, size and apparent position stay identical across build-menu and ordinary-HUD contexts. Contextual input hints appear below the shared card body; they must not resize or move that body between views. Goal management keeps the native focused project; the ordinary HUD has no separate focus/navigation. The interactive sketch used substitute symbols and illustrative counts, not extracted game sprites or evidence of real controller input. Production must reuse native item icons and the host presentation stack, including native icon shader/tint rather than copying only sprite images.

The user authorized implementing this direction with gamepad support. For the first candidate the user explicitly chose to show all rows; evaluate actual long-plan height in-game before deciding on clipping, paging or another overflow mechanism. No missing requirements may be silently hidden. Fine placement, visual density and controller hints remain subject to runtime verification.

Russian is the current visual-review language. All 11 built-in host languages remain required: preserve live host names/font switching and put new labels through the shared localization layer immediately. Final translation/long-label/CJK review may occur once layout stabilizes; a Russian visual pass is not acceptance for all languages.

### Runtime correction and calibration — 2026-10-15

0.1.5 is rejected as final UI: absent control hints, context-dependent scale, misplaced backing, unequal name font sizes and insufficient separation from the native location banner. The next candidate must keep one uniform name font (wrap long names rather than shrink individual rows), provide larger tunable icons, separate the Planner visually below the location banner, and retain the card while the current storage is open. Add an obvious storage-local Take Needed action with mouse click and native gamepad Y, plus visible LT/RT project hints.

The user explicitly requested a temporary live calibration panel with sliders and an export-to-log action. Tune only presentation; preserve native focused-project and storage-transfer semantics. Record all chosen parameters and context/geometry in one recoverable log event; this tool is research-only and must be removable before stable release.

Expansion to ordinary workstation production recipes was explicitly withdrawn in the same feedback. The project-first P0 boundary and deferred per-save persistence remain unchanged.

### Design return after 0.1.6 — 2026-10-15

The user confirmed the mouse storage action works but rejected its location/affordance in the information card. Move Take Needed into the native open-chest action row, before Take / Take One / Close, with native Y and a visible keyboard/mouse counterpart. The requested keyboard default is F in the open-chest context only; preserve Quick Stack's outside-chest action. A clickable mouse action must have recognizable button/hover feedback. Actual keyboard ownership and coexistence need verification before implementation.

Center the construction-plan title on a native decorative header, keep it visually separate from the location title, add canonical construction icons, and align project/material icons by the same sizing/spacing rules. Center each numeric column beneath its heading. Dense rectangular backing, excess night-time brightness and long wrapped names remain rejected presentation properties.

Text reduction is still a design choice: the user proposed an almost icon-only card, without adopting a final amount of text. Compare full names, project names only, and almost icon-only content before committing to that change. Preserve the explicit meaning of Have / Required / Missing unless an equally understandable replacement is agreed. Thin local text contrast and a softly fading body are candidate approaches, not accepted runtime results. Do not add time-of-day polling or a theme subsystem merely to compensate for an unaccepted palette.

### Adopted compact layout and reopened management/completion scope — 2026-10-15

The user selected the short design: project names with canonical construction icons; material icons and numeric columns; centered native title; restrained text with thin outline/soft shadow. Reduce column spacing and use **Required / Have / Missing**, corresponding to Missing=max(Required-Have,0). This supersedes the preceding open text-density choice; visual/runtime acceptance is still pending.

Automatic removal/decrement of a fulfilled pinned project is now required follow-up work, superseding its prior deferral. It is absent from 0.1.6. The user explicitly selected **successful placement/material commitment**, rather than waiting for final physical construction, for repeatable placeables: native materials are already spent at placement. Only decrement after a verified successful native operation for that exact project, never on menu selection, cancellation or inventory changes alone. Remove at zero and recompute derived aggregate requirements; native game code alone spends actual materials. Do not add an in-progress project tracker. One-time upgrade/repair/clearing success still needs its own verified native operation boundary.

The user also requests convenient quantity changes/removal away from a builder, for mouse and gamepad. The user accepted access through the inventory as preferable for simplicity/reliability; a global hold gesture is not required. Mouse minus/quantity/plus and an explicit native focus mode are design candidates. The exact inventory entry binding, adjustment and exit semantics still require compatibility verification. Prioritize native navigation and exclusive input ownership; do not intercept Quick Stack/world Work merely to emulate another mod. Immediate edits with B=close avoid a second draft/apply state; this remains the design recommendation, not a shipped feature.

### Compact mockup colour/readability review — 2026-10-15

Still a mockup-only pass, explicitly requested by the user. Keep zero Missing visibly green. Compare a restrained warm red for positive Missing instead of pale yellow; other numbers remain neutral and the value retains its meaning without colour. Compare a muted light title with thin dark contrast on the native header against the location's dark title. Reduce whitespace further while preserving icon/digit legibility; production width must still accommodate real localized names and large values.

Per-project green readiness is deferred: shared resources can make individual projects look simultaneously affordable without enough stock for their combined quantity. Do not introduce allocation/reservation semantics for a presentation effect. Aggregate Missing is the unambiguous resource signal. Existing all-rows and deferred-persistence decisions remain unchanged.

Reference-review recommendations, not new accepted mechanics: stable material-row order as counts change, and concise storage-local Take Needed feedback (moved/absent/full/partial). Keep editing entry visible in inventory, including for an empty plan.
