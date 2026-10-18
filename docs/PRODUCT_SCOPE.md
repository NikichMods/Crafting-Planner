# Building Planner — Product Scope

Updated: 2026-10-18

## Current milestone

The complete 0.1.16 visual presentation is user-accepted as of 2026-10-17. Preserve it unless a new defect/request reopens visual work. Exercised core functionality is recorded in [TEST_LOG.md](TEST_LOG.md); this is not full functional or stable-release acceptance. Exact installed artifact and recovery are in [DEVELOPMENT.md](DEVELOPMENT.md).

The user reports the 0.1.17 multi-line native-cost separation technically works and adopts one shared cost-block format for both single-energy and multi-line costs. The rightmost menu column belongs exclusively to planner quantity, centred horizontally/vertically by default. Native cost text/width/height remain host-owned; F2 independently moves its whole block in X/Y and adjusts materials relative to it. The earlier stacked single-energy arrangement is superseded. Recovered tuning below owns the defaults; the user accepts0.1.19 cross-context visual behavior, independent settings and20/19 arrow behavior on2026-10-17.

The 0.1.18 return supplies separate tuned layouts: ordinary rows need material X/Y 29/0; rows with native costs need 46/0, with cost-block 24/-1. Shared counter settings are X/Y 17/1 and arrow fine-X 8/-9. Preserve both sets independently based on visible native cost-block presence in each row, including mixed menus; changing repair/cost-row materials must not move ordinary builder rows. These recovered settings are the accepted visual defaults. The right arrow disappears at the repeatable cap20 in both menu and HUD, reappears below20, and retains the one-off0/1 behavior.

## Product model

Building Planner is a functional replacement/superset for the Recipe Pin idea, but its P0 focus is **projects**, not arbitrary production recipes.

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

- Repeatable placeable/build projects and the three accepted empty-point lighting choices have quantity 1..20; the limit is per goal counter and applies to every menu/HUD/mouse/gamepad entry path.
- One-off repair/upgrade/clearing projects are quantity 1.
- Quantity belongs to the planner goal, not to a workstation craft queue or private amount widget.
- Display bare quantities without an `×` prefix in both menu and shared HUD/card counters. Keep the numeral centred; arrow glyphs default to a13-unit gap from the measured visible numeral edges, with common gap tuning and existing menu fine offsets. Zero is shown for unpinned menu projects; decrementing an existing goal to zero removes it.

## Required next milestone: successful-operation reduction

Automatic reduction is required follow-up, absent through0.1.19, and implemented in0.1.20 with bounded native success/failure/identity/zero-removal verification recorded in TEST_LOG.md. For repeatable placeables, decrement exactly one at verified successful placement/material commitment, not at later physical construction completion. Failed/cancelled/rejected placement removes none. Remove a goal at zero and recompute derived aggregate requirements; native code alone spends materials. Do not introduce an in-progress tracker.

Repair/clearing world-change success correlation is now verified by the returned probe;0.1.21 removal is now verified for the exercised pinned repair/clearing and unrelated-goal preservation cases in TEST_LOG.md. The yard builder boundary is now runtime observed;0.1.22 implements completion removal for equivalent one-time None/auto/no-wait scripted builder goals and one-time Put placement, with0.1.22 yard removal now verified once at the native completion boundary. Other definitions retain static/compiled coverage; representative one-time Put cancellation/success/control preservation are now verified in0.1.23. The authored audit covers39/17 definitions respectively. Home furniture/decor callback placement now has verified native research; installed0.1.24 adds correlated Apply removal, now verified for cupboard Cancel/Apply and control-goal preservation. Menu selection and inventory changes alone are not success evidence. Returned probe evidence and the0.1.20 production gate are owned by PLACEMENT_COMMIT_RESEARCH.md; compilation/state tests do not close native decrement acceptance.

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

Through0.1.24 goals are session-local. On2026-10-18 the user makes per-save persistence an active required follow-up before release, superseding its earlier deferral: saving/loading the same game must recover its own pinned goals and quantities, including a full process restart. Keep different saves isolated and align the restored plan with the loaded saved world rather than unsaved later goal changes. Persist only stable goal identity/quantity; resolve current native definitions and derive materials. Candidate0.1.25 implements this requirement with local mod-owned snapshots; native persistence scenarios are accepted on2026-10-19, bounded by TEST_LOG.md. See PLAN_PERSISTENCE.md for lifecycle and storage limits.

## UI

The accepted presentation is a compact native-style card: project icons/names with fixed quantity positions, stable canonical-ID material order and centered numeric columns. Missing zero is green, positive Missing is restrained red; other counts remain neutral. Native title and restrained text sit over a soft rounded backing fading at the edges. Extra corner ornaments and hard/heavy rectangular framing are not adopted.

Preferred technical direction:
- use the game's native NGUI-style HUD hierarchy and visual components where practical;
- reuse/clone verified native fonts, panels, icons, sprites, and gamepad conventions;
- keep the planner layout custom and narrow rather than coupling to one unrelated vanilla screen.

A generic BepInEx/IMGUI text/debug overlay is suitable for research only, not the target release UI.

## Deferred

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

Mouse/keyboard and gamepad management are required and exercised on supported paths. Native build-row activation still builds; separate left/right quantity controls adjust only the plan. Decrement from one removes a goal; one-off projects remain capped at one. Native ordinary production/queue controls retain ownership.

The ordinary HUD shows project names/icons and fixed quantity slots above aggregate material icon/count rows. Mouse arrows remain visible with hover emphasis, and hovered rows highlight. Gamepad RS enters deliberate HUD editing; up/down selects a visibly highlighted row, left/right changes quantity, B finishes. Outside deliberate editing, the HUD does not capture gameplay. The RS/Edit Plan or B/Done hint is right-aligned and remains previewable while F2 is open. There is no separate management window or draft/apply state.

The shared card stays in the same apparent position across ordinary HUD, supported build/world-project menus, character Inventory tab and chest. The native location banner is replaced by the planner title while shown and restored when hidden. English title/display name is **Building Planner**; host project/item names and owned labels follow the live language.

Supported-menu help is ONE right-aligned row: round native left/right arrow symbols plus localized Add to planner first, followed by original native action blocks in native order, with final Close at the frame edge. Measured equal gaps and leftward growth accommodate translations. Planner help copies the native cream-to-brown label gradient/font/effect and round arrow sprites. Keyboard/mouse shows only planner help. Do not restore the rejected Add to planner header: it misleadingly made planning look like the window's primary build action. Single-energy and multi-line native costs share one separately movable block to the left of planner quantity; row height and native cost formatting remain unchanged.

Chest gamepad hints are one right-aligned **A / X / B / Y** row, preserving native omitted/disabled actions. Y Take Needed ends at the chest's right edge, equal measured gaps expand leftward; keyboard F uses that edge. Results/notices right-align to the same edge and report actual transferred item icons/counts, with compact measured spacing. A guaranteed pre-action transfer manifest is not adopted. Quick Stack's world transfer now immediately refreshes Have/Missing through a scoped native transfer observer; reserve protection and gesture takeover remain outside scope.

F2 is temporary live calibration (format10): individually resettable sliders, labels above controls, separate text/numeric/quantity styles, title and footer controls, independent icon visual size/offsets, grouped row/column placement, cost-block X/Y, relative material X/Y, shared measured arrow gap and soft backing controls. Supported Build calibration is available with an empty plan. Logical dimensions use integer steps with deliberate scale/opacity steps. Current accepted defaults are owned by source; calibration logs are recovery evidence, not a second settings database. The final public-build fate of F2 remains unresolved.

## Localization requirement

Building Planner must support every language exposed by Graveyard Keeper 1.407.

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
- Building Planner-owned labels must have translations for every host language above, with English fallback only for unknown/unsupported external locale codes;
- changing the game language at runtime must refresh Planner text and the correct host font/atlas without restarting the game;
- localization must remain a small presentation layer and must not duplicate the game's item/project localization database.

## Historical design decisions and candidate evolution

The following dated record preserves adopted, rejected and superseded design steps. Present-tense wording inside an older entry describes that stage only. Active requirements above and current acceptance in TEST_LOG.md take precedence; do not reopen old pending gates or restore old bindings/layouts from this history.

### Initial UI design checkpoint

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

### Dense design / hints / native world icons — 2026-10-15

The user selected the denser compact variant and requests slightly stronger green/red status colours. Stable row order is accepted. Proposed minimal ordering is ordinal canonical item ID, independent of localization and counts; a complete copied item/index database is not required. Exact semantic priorities such as nails-before-boards remain optional and unadopted. This deliberately replaces localized-name ordering only once implemented and verified.

User requests an immediate discoverable editing route from HUD without implying builder/world-project management was removed. Proposed short HUD hint: “Изменить план — в инвентаре”; active supported build/repair/clearing menus show their actual focus-adjustment hints instead. Visibility/wording remains part of the mockup review. World-project icons must be host-authored, preserve names when icons are shared, and fall back to text rather than omit a project if artwork is absent.

Compare a centre-weighted backing fading smoothly to transparency at all edges against the same backing with small native corner ornaments. Hard rectangular edges are rejected; neither a heavy frame nor entirely absent backing is required. Four-corner treatment is exploratory, not the final accepted design. Keep the native header and chosen density unchanged between comparisons.

Chest result meaning: the proposed feedback is after executing Take Needed in the currently open chest, adjacent to its stable Y/F action label, with actual moved item icons/counts or a concise no-op reason. A pre-action guaranteed transfer manifest is not adopted; native shared capacity/sequential transfer semantics would need separate verified forecasting. The private mockup demonstrates partial/no-stock/no-space results, not real runtime transfers.

### Adopted mockup baseline and optional copper ornaments — 2026-10-16

User accepted the denser layout, stronger status colours (preview green #88d77c / red #e47766), soft all-edge fading shadow with 40% centre opacity, and ordinal native item-ID ordering. Move the smaller grey readable note immediately below the title: “Изменить план можно в инвентаре”. The same note slot carries the relevant LT/RT hints in supported construction/repair/clearing contexts; avoid moving the common body between contexts.

User explicitly accepted post-action chest feedback rather than a guaranteed pre-action transfer preview: keep the Y/F action label stable, show actual moved icons/counts in green, and failure/unavailability reasons in red. Successful partial transfers remain success; a reason may accompany them when verified. Do not mark actual collected materials as an error because another ingredient could not move.

The plain native corner crops were rejected as too primitive. The user requests an exploratory original decorative sprite inspired by warm copper/brass book-corner fittings with small scrolls. This is a limited exception for optional decoration; project/material icons remain host-native. One generated transparent raster corner is mirrored in the private comparison with the accepted plain shadow. It is not accepted production art, does not ship in 0.1.6 and must not delay the functional candidate if the plain baseline remains preferred. No change to input ownership, saves or inventory follows from decoration.

### Adjustable mockup / editing discoverability — 2026-10-16

The next private mockup compares the plain shadow against shorter, thicker copper corner fittings with restrained scrolls. Expose fitting size/opacity and independent paired offsets: left/right pairs horizontally, top/bottom pairs vertically. Decoration remains optional and unaccepted for production.

Expose independent vertical placement for the hint and for one combined block containing all goals and aggregate materials. Keep a hint visibility toggle and compare the user's requested full wording, “Изменить план можно в инвентаре и на рабочих станциях”, with scope-accurate construction/repair wording and a short inventory route. “Working stations” must not silently extend P0 to ordinary production recipes. Whether a persistent HUD sentence is needed remains open; hiding it in the mockup is not acceptance to remove discoverability.

The user reopened direct HUD editing as a desirable route for mouse and gamepad, while continuing to value the inventory route for simplicity/reliability. No global hold gesture or HUD input interception is adopted. Consider a discoverable editing affordance and contextual input hints; verify native cursor/focus and conflicting game/mod bindings before committing to a global route. A mod description alone does not answer a returning player's immediate in-game editing question.

### Plain compact baseline / direct mouse controls — 2026-10-16

User selected the plain soft-shadow variant without ornamental fittings and without the explanatory HUD sentence. Stop ornament work. Close the space between title and goals; the current private comparison exposes a direct gap control rather than retaining space for a hidden note.

User prefers direct quantity arrows beside each goal for mouse, without a separate editing-entry button or modal mode. Compare always-visible arrows against hover emphasis, retaining discoverability. Zero removes a goal; adding an absent goal remains available in its supported build/world-project menu. One-off goals are quantity 1 and offer removal rather than arbitrary multiplication. Exact click ownership must be verified so planner clicks cannot reach world attacks/interactions underneath. This is mockup direction, not implemented input acceptance.

Provide full-context build and repair/clearing mockups with a visible focused-project add/remove action, retaining ordinary native actions. A private three-context comparison demonstrates shared goals, direct arrow editing and a candidate controller focus/exit flow. Its menu chrome/font/layout and physical button artwork are illustrative, not verified native rendering. The current 0.1.6 source still has LT/RT focused-project hooks and mouse hover focus, but no mouse quantity/add controls or complete keyboard management shortcut.

For gamepad, the user proposes a stick click (RS/LS) as a possible direct HUD entry, with a short visible hint, avoiding a global Work/Y conflict. Investigate this before choosing a default. The mockup allows RS, LS and hold-Y labels for comparison only; no physical binding is accepted or claimed to work. Inventory management remains the accepted simpler fallback, not a requirement to reject a viable direct route.

### Per-row management and backing refinement — 2026-10-16

User adopted always-visible, muted mouse quantity arrows with hover emphasis, zero title-to-body gap, and the short gamepad editing-entry hint below materials. The illustrative RS glyph is visually accepted; the actual physical binding remains unverified. No permanent explanatory sentence or decorative corners.

Replace the oval shadow experiment with a softly feathered rounded rectangle supporting every goal/material row, including the topmost goal. User accepted the preview defaults: 40% backing density, 24px roundness, 13px feather, and 0px title/body gap. These are browser mockup values; native rendering still requires in-game visual acceptance.

Correct the earlier mockup's click-to-select assumption: the native main project row retains its build/execute action. User adopted mouse management through a distinct grey Plan column with per-row quantity arrows, separate from native production/amount buttons; no click-selection prerequisite or keyboard/gamepad mixing. Native insertion/layout must still prove simple and reliable before production implementation.

User prefers gamepad up/down to choose a row and left/right to adjust plan quantity, with B to leave HUD editing, replacing the LT/RT mockup scheme. In supported project menus retain native focused-project navigation and show a short horizontal-direction hint. Only adopt interception where native quantity/queue/navigation ownership is proven safe; ordinary production, active native amount controls, queue areas and unrelated UI keep vanilla input. Current 0.1.6 LT/RT implementation remains historical until a verified later candidate replaces it.

Quantity positions are invariant within each surface: reserve left-arrow / centered quantity / right-arrow slots even when either arrow is absent. Mouse/gamepad switches, HUD focus-mode entry/exit and one-off zero/one states must not move the quantity label. Use the same specification in builder and repair/clearing Plan columns; native per-surface coordinates differ, but quantity alignment within each column stays fixed. User confirms horizontal plan input is intended only for supported construction/repair/clearing, never ordinary workstation crafting; shared CraftGUI handlers still require narrow context/focus ownership checks.

### Mockup design accepted for implementation — 2026-10-16

User considers the mockup complete and requests no further design iteration. The adopted compact plain card, backing defaults, fixed quantity slots, direct mouse arrows, separate per-row Plan column and proposed directional gamepad flow above are the implementation target. Reopen visual design only for a concrete native-rendering/usability problem or new user feedback. Browser acceptance is not native runtime acceptance or permission to mark the installed 0.1.6 stable.

Next work is native implementation readiness and a new immutable candidate: verify per-row insertion/click ownership and supported-context horizontal input; resolve the physical gamepad HUD-entry binding; then implement the shared renderer/actions and exercise HUD/build/world-project/chest continuity in-game. Preserve the accepted storage-local action/feedback, automatic successful-placement decrement follow-up, localization requirements and deferred per-save persistence; do not turn this milestone into ordinary-recipe expansion or more ornamental exploration.

### Runtime presentation correction — 2026-10-16

The 0.1.9 return reopens the native presentation: replace the duplicated location-style planner header with distinct plain centred text, remove all Plan-column/action-button rectangular fills, keep only B=Done while editing, and remove redundant directional help in supported menus. Missing and project quantity share one fixed horizontal centre. Keep the planner visible on the character Inventory tab. Mouse arrows remain always visible, with hover emphasis; controller editing needs a clear row highlight. F2 exposes temporary live typography/scale/shadow and grouped layout controls with log export. Existing 40% backing and all 11 host languages remain baseline; no browser/custom font is accepted for distribution.

### 0.1.10 return / adopted next visual pass — 2026-10-16

HUD mouse/gamepad quantity editing and current-storage Take Needed are accepted for the exercised cases. First-open construction layout is broken and requires an ownership/timing fix. Adopt independent native font switches for all numeric counts and, separately, quantity/arrows; independent title font/size; right-aligned RS/Edit Plan and B/Done with independent font/size. Keep the chest feedback font, expose its whole-block position and spacing. Icon visual size/offset/vertical spacing must be independent of text rows and columns, allowing intentional overlap during tuning.

User explicitly chooses to replace the location banner with the planner title while the planner is displayed; no relocated location label is required. Restore vanilla display when the planner is hidden/unloaded. Recovered F2 body baseline: right13, top40, width286, scale0.647, font24, icon29, effective row28, padding4, column70, gap5, title gap0, footer gap9, backing40%, text100%, radius34, feather28, goals offset13, materials gap13, column inset19, craft-name font, outline on. Next calibration uses integer logical dimensions and explicit scale/opacity steps; preserve source log before rounding.

### Adopted 0.1.12 calibration and name — 2026-10-16

User selects **Building Planner** as the mod display name and English HUD header. Technical identity/repository remains Crafting-Planner for upgrade continuity; no ordinary production recipes are added. Add supported-menu column/individual arrow/quantity/ingredient offsets, native font/effect controls and an optional native right-edge removal. Every F2 slider needs a local baseline reset and its label above it. F2 must keep RS/B preview visible despite keyboard input, with independent footer position, text/icon sizes and shadow. Add title vertical offset. Chest Take Needed and feedback move independently; native A/X/B move together horizontally per the user's simplification.

### Unified chest actions and menu caption — 2026-10-16

User requires native A / X / B followed by Y Take Needed in one row, right-aligned under the chest, with equal gaps between measured localized blocks and expansion leftward. Preserve native omitted/disabled actions and input. Keyboard F uses the same right edge. Common row X/Y and gap replace independent native/Y offsets; result placement remains independently tunable. Replace the supported menu Plan caption with localized **Add to planner**, centred over ingredients at the native title level, matching native title styling. Expose caption X/Y/font size with individual resets. Retain accepted Building Planner English HUD title, RS/B tuning and all 11 languages. English is the priority visual checkpoint, not a reduction in supported locales.

### 0.1.13 return — secondary help and storage refresh

User accepts the unified right-aligned A/X/B/Y row. Add to planner in the construction header overstates the mod action and obscures the native window's primary build purpose; replace it with a separate lower help line using the same < / > glyphs/font as the quantity controls. Native row click still builds; horizontal input only adjusts the plan. Keep the existing independent help position/size controls, relocated with the text.

Every storage result, including Taken/icon/count rows and all notices, must right-align to the action row edge and grow left. Measure number widths instead of reserving fixed cells; retain actual transferred quantities and optional negative spacing for visual tuning. Feedback X becomes relative to that right edge.

User reports stale Have/Missing after Quick Stack's world stack action, corrected only by a later UI refresh. Fix invalidation at the verified native successful transfer boundary; do not change Quick Stack's world gesture, transfer rules, player inventory counting or current-storage Take Needed semantics. Direct Quick Stack internals and remote storage behavior remain outside scope.

### 0.1.14 return — combined construction controls

User accepts current chest actions/results, compact right alignment and immediate Quick Stack refresh. Preserve them. Replace the separate second Add to planner line with ONE common row: planner action at the left, existing native actions in native order to the right, final Close aligned to menu frame right edge. Equal measured gaps and leftward growth accommodate translations. Keyboard/mouse shows only the planner help at the same edge. Native build click and gamepad bindings remain unchanged. Match the native action text's cream/brown gradient, not merely its white base color. Use native dark round arrow artwork comparable in size to native button glyphs; no custom texture is required. F2 may tune common row X/Y/gap and planner-caption size.

### Energy-cost row separation — 2026-10-17

User accepts the combined lower action row and native round arrow icons in0.1.15. A supported repair action shows energy behind the planner counter. Separate native energy above and quantity/arrows below only when a single-line energy cost is visible. Preserve native cost semantics, ordinary builder layout, row height, accepted footer/chest and all input behavior. Prefer a bounded integer shift within existing whitespace over native row resizing or a new cost renderer. Multiline/special costs are not generalized by this repair-specific pass.

### Visual milestone accepted — 2026-10-17

User explicitly accepts the complete current visual presentation, including the0.1.16 energy-row correction. Close visual iteration unless a new defect/request reopens it. This is visual acceptance, not an assertion that all functional edge cases or the release are accepted. No new binary/install is requested while the game remains running. Next required function is automatic reduction after verified successful placement, with separate repair/upgrade/clearing success paths; session persistence stays deferred.

### Repeated roadside lighting exception —2026-10-18

The user adopts quantity1–20 for each of the three lighting choices on empty roadside/quarry lantern points. These native WorldChange recipes (lantern_1/2/3 on lantern_place) aggregate by existing owner-type/recipe key across points; each successful completed light reduces its own quantity by1. Their requirements multiply the chosen quantity as for repeated builder goals. Other repairs/clearing/one-time upgrades retain quantity1, including lamp-to-lamp upgrades and broken-lamp repair; no host build/craft rules change. Installed0.1.23 implements this narrow exception; TEST_LOG.md records its exact identity. Returned native evidence confirms lantern_2/3 counters20/19 and lantern_3 completion1->0; nonzero automatic remaining quantity and lantern_1 were not independently exercised in this log.

### Home interior completion adoption —2026-10-18

The user requests automatic removal of pinned domestic furniture/interior goals after native Apply; preview/Cancel/rotation must preserve them. Keep quantity1, same-ID valid applies and the native material/refund/world mechanisms. Returned7-attempt research supports the39 authored five-script callback family.0.1.24 implements a correlated original-goal/definition/save/script receipt with normal decision return and script termination, independently from early craft finish. Exact candidate native removal/cancellation acceptance remains required. INTERIOR_COMPLETION_RESEARCH.md owns the READY gate; no generalization to arbitrary scripts or repeatable production. Installed0.1.24 now implements this adopted policy; its cupboard return verifies Cancel retention and once-only Apply removal with control goals retained. TEST_LOG.md owns exact identity and bounded acceptance.


### Per-save persistence priority —2026-10-18

After cupboard acceptance the user explicitly requests preserving added goals across saving/loading and asks whether it exists. Source inspection confirms0.1.24 ResetSession clears Goals on OnGameStartedPlaying, with no restore/store path. Per-save persistence becomes the next functional work item ahead of the remaining inventory/compatibility/release pass; earlier dated deferrals are historical. Required outcome is per-save goal/quantity recovery, empty-plan/deletion/completion recovery and isolation on switching saves. Saving native game state should capture the corresponding plan; loading an older native save must not apply a later plan indiscriminately. No new logistics, recursive crafting, visuals or quantity scope.

Initial static inspection finds public MainGame.save/save_slot, SaveSlotData.filename_no_extension/linked_save, native PlatformSpecific.SaveGameDataToSlot -> PrepareForSave -> ToBinary -> .new/backup rotations, and FromBinary/OnGameStartedPlaying restoration. Native disk-write exceptions are caught internally, so a normal save-method return alone is insufficient proof of successful disk commitment. A separate mod-owned snapshot tied to verified native save identity/version is the current least-invasive storage candidate; native save payload mutation is not adopted. Research inputs remain private .local/planner-save-*.txt. No persistence source/build/probe/install or runtime acceptance is performed in this status/priority review; exact storage/identity/failure/readiness gate must precede implementation.

### Per-save persistence candidate —2026-10-18

User explicitly authorizes implementation/build/installation.0.1.25 records an ordered key/quantity snapshot only when native serialization matches the committed disk bytes and the native file was created/rewritten. Restore uses the exact loaded binary digest and native slot path, after clearing old goal/completion state; resolve current host definitions and repeatability rather than a stored material database. Empty snapshots preserve manual deletion and automatic completion. Plan edits since the last native world save are unsaved edits; no extra autosave or separate user gesture is introduced.

Storage is local BepInEx config/CraftingPlanner/plans, isolated by SHA256(full native slot path), then SHA256(native binary bytes). Byte-identical native snapshots share their latest plan; historical distinct snapshots are retained. Plans do not automatically travel with cloud saves or copied/renamed native slots. Unsupported/corrupt snapshots are preserved and fail closed; no native payload is modified. PLAN_PERSISTENCE.md owns the READY gate and native acceptance envelope.
