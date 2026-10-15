# Lightweight row card — implementation gate, 2026-10-15

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
