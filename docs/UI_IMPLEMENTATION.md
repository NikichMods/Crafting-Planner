# Lightweight row card — implementation gate, 2026-10-15

## Current correction gate after the 0.1.5 runtime return

The earlier READY sections below describe the historical 0.1.5 implementation, whose final presentation is rejected. Do not treat those static assumptions as accepted runtime layout evidence.

Solution-space re-opened: compensating separate live HUD/CraftGUI parents would retain two transforms, sizes and hint lifecycles. Prefer one owned card/panel adjacent to the verified native screen-size frame under the shared UI Root. Native HUD hiding remains unchanged; explicit native window/HUD lifecycle decides this separate card's visibility. One transform/layout is reused in HUD, supported CraftGUI and open ChestGUI. Ordinary/special production does not become a planner goal.

**READY — geometry and typography:** exact screen_size_widget prefab chain is Screen size -> Screen size panel -> UI Root, all unit scale until the shared UIRoot. Anchor one owned shell to that widget's top-right with bounded insets; an inner shared scale changes text/icons/backing together. UIWidget.OnAnchor owns shell placement; UILabel.ProcessText/printedSize owns wrapped-name height. Reset background local position after changing pivot (native setter moves the transform). No per-name ShrinkContent. Native UIPanel owns geometry and active host panel depth; keep unrelated modal UI above/hidden by view classification. Verify actual HUD/build/chest transitions and projected rectangles on export. No raw screen arithmetic is used for placement.

**READY — actions and visibility:** reuse native focus and Y callbacks already exercised in the returned log. Display LT/RT hints with the card instead of relying on another window's tip parent. The storage action is an owned lightweight collider/label using verified UIEventListener.OnClick semantics, with the existing native Y path kept; the clicked callback requires the current top window to be ChestGUI. No extra gamepad focus tree. Reuse current-storage requirements/capacity/native moves; preserve goal/state/save/world behavior. Representative acceptance is mouse click and Y with visible current-card counts, plus no display through unrelated menus.

**READY — temporary calibration/evidence:** user-authorized research-only F1 panel varies bounded shared style values and logs one full settings snapshot plus actual projected card/backing rectangles. Static inspection and the existing candidate already prove the native frame, pivot and measurement owners; sliders tune residual visual parameters rather than substitute for them. While open, preserve/restore the native character control flag and temporarily suppress native input keys/mouse events so slider clicks cannot activate underlying game UI. Restore on close/unload. The tool changes no inventory, goals, saves or persistent world data; actual input guard and perceptual tuning require the game. Compilation symbols separate this calibration layer from a future stable build.

Private static evidence: `.local/pivot.txt`, `runtime-layout-il.txt`, `actions-api.txt`, `measure-label.txt`, `calibration-guard-il.txt` and `asset-layout-current.txt`, on the exact assembly identities recorded below.

Adopted design and deferred per-save persistence are owned by PRODUCT_SCOPE.md. All rows remain visible for the first candidate (explicit user choice); evaluate actual height in-game before choosing overflow navigation. Russian is the current review language; all 11 locale routes remain required.

## READY: shared row renderer and native anchoring

Observable change: one lightweight icon-led card with aligned Have / Required / Missing columns, identical across native builder/world-project CraftGUI and ordinary HUD. Owners are project-owned renderer children inside the already accepted HUD.zone_name parent and the existing CraftGUI UIPanel. Both transform chains have unit scale up to the common UI Root in exact level2 inspection. Keep the ordinary HUD lifecycle unchanged.

Final placement is owned by UIWidget.OnAnchor; UIRect.SetAnchor's relative+absolute overload and UIWidget.GetSides transform the target widget's edges into the current parent coordinates. Anchor both roots to HUD.zone_name bottom-right, with identical offsets/dimensions, through native anchor updates. This replaces the old craft-panel arbitrary offset; it does not resurrect screen-size-derived coordinates. Final mesh generation remains the native UIPanel path. Hidden HUD target geometry is a transform/width/height query, not an active-state visibility dependency. Runtime transitions/resizing remain a required representative acceptance check.

Native material images: ItemDefinition.GetIcon -> EasySpritesCollection.GetSprite(string,bool,string), displayed by a clone of craft.craft_item_prefab.item_gui.x1.icon. Preserve its native shader/tint, strip/reset inherited anchors and renderer placement only. Do not clone the interactive item-cell/controller/collider hierarchy. Labels use current native font/atlas, refresh from the live host template after language changes, and do not cache localized project/item names.

Blast radius: only the two owned presentation trees; goal storage, filters, arithmetic, player-carried counting, native storage moves, world execution, saves and unrelated UI remain invariant. No custom per-frame inventory scans or new focus owner. Verify both compilation against minimal stubs and actual installed assemblies, then real gamepad add/remove, menu/HUD continuity, native icon appearance, localization and normal menu visibility. Compilation is not visual acceptance.

## READY: borderless gradient backing

Owner is a new owned UITexture using Unity's white texture and the verified native transparent-color shader path. UITexture.OnFill invokes UIWidget.onPostFill after producing its own quad; the owned callback attenuates its existing vertex alpha by normalized local position. Preserve native cumulative alpha. Strongest top-right, near-zero bottom-left; no outline, frame, custom shader or copied asset. Only this background's vertices are affected; label/icon alpha stays independent. Check attenuation bounds/direction offline; transparency/readability requires direct in-game visual review. No texture assets or persistent data are created.

## READY: contextual native gamepad hints

Keep existing verified BaseGUI semantic callbacks for project RT/LT and open-chest Option2/Y. Native GameKeyTip.Get supplies platform-dependent key symbols; LazyInput.gamepad_active and native input/window events bound refresh of separate display-only hint labels next to the active menu's button_tips.label. No overwrite of vanilla tips, no navigation component, collider, global key capture or second focus tree. Hide owned hints outside the correct topmost native window or unsupported focus. Source/card focus callbacks and exact native signatures are inspected; actual hints/navigation and Quick Stack coexistence must be checked in-game on the numbered candidate.

## Evidence identity

Direct static inspection: Graveyard Keeper 1.407 Assembly-CSharp SHA-256 e72e4270e4b88dd0a87ca23c9cf1750aec4c4a0fedb40b6d2dae7902fc9c7fd8; firstpass 9dc6def3b7715dd27eeb168ddc0af47e31c6f38d3fbee24bf592899392026498. Shared UI_INPUT_TIME_AND_ENVIRONMENT.md owns icon shader/tint, native anchor/font/lifecycle limits. Accepted 0.1.4 HUD-parent and localization evidence remains in TEST_LOG.md. Private bounded IL/prefab inspection stays in ignored .local/; no host implementation/assets enter Git.

Candidate 0.1.5 is built, policy-tested and installed from exact CI bytes; TEST_LOG.md owns its source/hash/run identity. These READY gates justified implementation; they are not runtime or subjective visual acceptance. The next evidence is the focused gamepad/menu/HUD pass in that log.

## Qualitative UX reference review — 2026-10-15

The research question was whether existing recipe trackers reveal usability problems relevant to the lightweight card. This is a targeted qualitative sample, not a representative survey:

- [Subnautica removal discussion](https://www.reddit.com/r/subnautica/comments/1rrozgz/one_thing_i_need_for_subnautica_2_to_improve_upon/): the poster objects to finding an original recipe/fabricator merely to unpin it. Supports explicit remote editing, not a universal preferred key/controller gesture.
- [Subnautica smart-pinning feedback](https://subnautica2.nolt.io/1046): the poster requests a combined material list to reduce multi-recipe clutter and repeated trips. Aggregation fits our adopted model; the request's recursive raw-material expansion remains outside our scope.
- [Pinup author feature/changelog documentation](https://www.nexusmods.com/subnautica/mods/2749): direct quantity controls and aggregate totals are existing mod patterns; repeated auto-unpin fixes, including multiple triggers for multi-output recipes, caution against treating arbitrary inventory changes as completion. This is precedent, not compatibility or implementation evidence for Graveyard Keeper. No code/assets were imported.

Own recommendations: visible inventory editing entry even when the plan is empty; stable rows rather than deficit-based reordering; concise storage-local transfer results. Full-project green tint stays deferred because shared stock requires allocation semantics. No expansion into recursive crafting, remote logistics, notes or automatic priorities follows from these examples.

The updated private comparison has two compact browser layouts (238px/218px), neutral Required/Have, green zero Missing, muted-red positive Missing and a light title on the native header. Browser typography/effects are approximate; these are not NGUI constants or game readability acceptance. Verified preview at 736/360/320px, day/night/chest views and sample action. Current installed game candidate remains 0.1.6.

## 0.1.6 implemented candidate checkpoint

One owned NGUI presentation now serves HUD/build/chest. Shared font size, wrapped label measurement, native numeric-width measurement and larger adjustable icons replace per-row shrinking. The backing pivot is reset and its fill invalidated for live gradient changes. Build hints remain visible before the first goal. The storage action is a mouse target and native Y hint with capacity/current-chest help text. The default top inset separates the card from the location banner; actual appearance remains open.

Temporary F1 calibration covers position, minimum width, shared scale, font/icon/row sizes, padding, columns/gaps and backing/text/gradient settings. Export and close log the entire shared style, current view, screen dimensions and projected body/backing bounds. Settings are session-local. Native character/input-camera state is preserved and restored; no native inventory scans occur per frame. Both compile profiles must pass against installed host references; CI must build/test the calibration candidate against signature stubs and exclude those references from the numbered package. Input protection, collider action, hints, placement and visual continuity still require runtime acceptance.
