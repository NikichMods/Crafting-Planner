# Verified Graveyard Keeper 1.407 Data for Building Planner

Current status (2026-10-17): complete 0.1.16 visual presentation is user-accepted, with exercised mouse/gamepad management, current-storage transfers and immediate Quick Stack refresh. Exact artifacts and bounded cases are recorded in [TEST_LOG.md](TEST_LOG.md). Successful-operation reduction is required next and absent from 0.1.16; static placement evidence below does not yet prove the production success boundary. Dated prototype/calibration seams retain historical context; active layout/input policy is owned by [PRODUCT_SCOPE.md](PRODUCT_SCOPE.md).

Status legend:
- **Verified static** — established from Graveyard Keeper 1.407 decompile/API evidence and/or accepted NikichMods repository evidence.
- **Runtime gate** — plausible static seam exists, but installed-runtime behavior must still be proven before production relies on it.
- **Open** — not yet established.

Initial historical static reference: public `Kupie/GYK_DECOMP` at commit `6abf79199d92482af1c7573870dd9a20ec2270b9`, plus prior accepted NikichMods inventory/Quick Stack research. Later sections use inspected exact installed 1.407 assemblies/prefabs and accepted local/shared runtime evidence; consult their stated provenance rather than assuming every seam comes from the public decompile.

## Recipe model

**Verified static**

`CraftDefinition` owns the common recipe data required by the planner:
- `List<Item> needs`
- `List<Item> needs_from_wgo`
- `List<Item> output`
- `CraftType craft_type`
- `CraftSubType sub_type`
- multi-craft/special-behavior flags.

`ObjectCraftDefinition : CraftDefinition` is the building recipe subtype.

`GameBalance` exposes separate `craft_data` and `craft_obj_data` collections.

`CraftsInventory.GetCraftsList()` resolves normal craft IDs through `GameBalance`.
`CraftsInventory.GetObjectCraftsList()` resolves building definitions.

## Craft/build UI ownership

**Verified static**

`BaseCraftGUI.CommonOpen(...)` builds its visible recipe list from actual `CraftDefinition` instances.

`CraftItemGUI` receives a `CraftDefinition` through `Draw(CraftDefinition)`, exposes its current definition through `current_craft`, and draws ingredients from `current_craft.needs`.

Building UI uses `ObjectCraftDefinition`.

**Runtime gate**

The exact lowest-risk Harmony/UI seam for “pin the currently focused/hovered recipe” still needs one installed-runtime probe. Production must not read private UI fields by reflection merely because they are visible in decompiled source if a public callback/method boundary can be used.

## Ingredient semantics

**Verified static**

For ordinary crafts, `BaseCraftGUI.CanCraft(... amount)` evaluates `craft.needs` through native `MultiInventory.IsEnoughItems(... amount)` and separately evaluates `needs_from_wgo`.

Native craft start consumes the selected needs through `MultiInventory.RemoveItems(...)`; workbench-owned needs are removed from the workbench data separately.

Therefore planner requirements can derive from `CraftDefinition.needs` for the clean static-recipe subset, but `needs_from_wgo` and other special fields must not be silently treated as normal carried ingredients.

## Recipe categories

**Verified static**

`CraftDefinition.CraftType` includes:
- `None`
- `ResourcesBasedCraft`
- `Survey`
- `MixedCraft`
- `Fixing`
- `AlchemyDecompose`
- `PrayCraft`
- `RatBuff`
- `RefugeeCampCraft`

`CraftSubType` includes `Alchemy` and `SurveySciencePoints`.

`MixedCraftGUI`, alchemy-specific routing, survey, fixing, prayer, body/autopsy and other special GUIs demonstrate that not every “recipe-like” operation shares identical selection/ingredient semantics.

### First clean support candidate

P0 support boundary after Probe 0.2.0:
- builder projects supplied as concrete `ObjectCraftDefinition` entries with plain `needs`;
- representative one-time builder upgrades using the same object-craft model with concrete `needs`;
- representative repair/clearing world projects supplied as concrete `CraftDefinition` entries with plain `needs` and `change_wgo`.

Ordinary workstation production is intentionally not a P0 goal even though its static recipe model is understood.

Continue to exclude until separately approved/verified:
- mixed alchemy/dynamic ingredient selection;
- multiquality ingredient-selection cases;
- survey/research;
- body/autopsy insertion/extraction;
- resurrection;
- prayer;
- refugee/special scripted crafts;
- scripted world-project shapes that do not match either accepted P0 family;
- any recipe where the displayed/consumed ingredient set differs materially from plain `needs`.

## Player inventory vs crafting-access inventory

**Verified static — critical**

`MainGame.me.player.GetMultiInventoryForInteraction()` is **not** a valid definition of planner `Have`.

That method can include the nearest object's/world-zone inventories. It answers “what crafting can access here”, not “what the player physically carries”.

For planner `Have`, count player-owned data only. Native `Item.GetTotalCount(item_id, count_in_bags:true)` recursively counts carried bag contents.

**Runtime gate**

Confirm exact P0 treatment of toolbelt/secondary inventory. Default planner assumption is that ordinary crafting materials are counted from carried inventory + bags, not the toolbelt.

## Storage-local inventory

**Verified static**

`WorldGameObject.GetMultiInventoryOfWGOWithoutWorldZone(true)` builds a `MultiInventory` from that WGO and its bags without adding its world-zone inventories.

This is the correct shape for “this specific storage” semantics.

## Native transfer/capacity path

**Verified static**

Vanilla `ChestGUI`:
- creates a player and current-chest `MultiInventory` when opened;
- computes move capacity using source total count and destination `CanAddCount(...)`;
- performs actual transfer through `MultiInventory.MoveItemTo(...)`;
- redraws both inventory panels after transfer.

Prior accepted Specialized Storage research also confirms that `MoveItemTo` is the native ownership path to preserve stack/capacity semantics.

### P0 Take Needed formula

For each aggregated exact item ID, immediately before transfer:

`Missing = max(Required - PlayerHave, 0)`

`Move = min(Missing, CurrentStorageAvailable, PlayerCapacity)`

If `Move > 0`, call native transfer for that bounded quantity.

Each ingredient is independent. Failure or absence of one item must not abort the loop for other ingredients.

## Craft completion

**Verified static**

Normal craft completion flows through `CraftComponent.FinishCurrentCraft()` -> `ProcessFinishedCraft()` and eventually `End()`, with special-case output/side-effect behavior.

These callbacks are not a universal success seam for the required next automatic-reduction milestone: special recipes and builder/world flows differ. Ordinary workstation production remains outside planner scope.

**Placement-stage clarification, static 2026-10-15:** exact 1.407 `BuildModeLogics.DoPlace()` first checks `can_be_built` and `CanBuild(_cd)`, then calls its native `MultiInventory.RemoveItems(_cd.needs, ...)` before stopping the floating preview and optionally replacing the placed object with its `_place` definition. Later it marks the placed object `just_built=true`. Thus the repeatable placement material commitment precedes physical construction work; ordinary craft-completion callbacks are not a universal boundary for this planner requirement. The method has early returns and discards RemoveItems' return value: a generic postfix or an inventory-removal event alone is not sufficient success evidence. Actual successful placement identity, cancellation/no-material rejection and exactly-once decrement require narrow runtime verification. Private inspection is `.local/build-spend-stage.txt` on the pinned assembly identities; current 0.1.16 contains no decrement hook. The user accepted placement-time decrement in PRODUCT_SCOPE.md; repairs/upgrades/clearing remain separate success paths.

**Placement follow-up, 2026-10-17:** inspected exact installed assembly SHA256 `e72e4270e4b88dd0a87ca23c9cf1750aec4c4a0fedb40b6d2dae7902fc9c7fd8`, MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`. `StopCurrentFloating(true)` retains the captured WGO and clears the preview; `DoPlace` subsequently writes that WGO's `just_built` flag and redraws it before optionally preparing the next preview. Capture initial definition/builder/WGO identity: the post-return preview may already be different. Passive probe 0.1.0 now observes two successes with exactly one spend/leave/built redraw each, a busy failure with none, standalone cancellations and upstream resource rejection. [PLACEMENT_COMMIT_RESEARCH.md](PLACEMENT_COMMIT_RESEARCH.md) owns exact source/log identities and limits; production decrement remains absent. Reusable native findings were promoted to shared research commit `8a8aef1`; the observer was removed after evidence capture.

**Native cost layout, 2026-10-17:** native GetSpendTxt can append tool symbol/efficiency lines after energy. Verified row prefabs24662/27338 use spend width38 with separate horizontal anchors and centre pivot. The 0.1.17 multi-line column is technically confirmed by user screenshot/log. The user's next policy unifies all nonempty costs, including single energy, in installed 0.1.18: horizontal anchors retain the cost column, X adjusts both sides, Y adjusts top/bottom together, and original horizontal target/relative/absolute plus top/bottom are restored before redraw/hide/unload. Native text/size/row height remain unchanged. Signed material calibration uses a fixed29-unit origin rather than cancelling positive offsets. Number/arrow positions are independent of cost height and use native printedSize; no font/glyph advance is guessed. Source gates are in UI_IMPLEMENTATION.md; exact installed identities and remaining rendering/tuning acceptance are in TEST_LOG.md.

## Input bindings

**Verified static**

Relevant default gamepad mappings include:
- Interaction = A
- Work = Y
- Action = X
- Select = A
- Back = B
- Option1 = X
- Option2 = Y
- LB/RB/LT/RT/D-pad have multiple context-dependent uses.

Relevant default keyboard mappings include:
- Interaction = E
- Work = F
- Action = X
- plus context/UI/navigation bindings.

Keyboard bindings are remappable by the game.

**Product/compatibility consequence:** do not hardcode a global F/Y/X gesture for Crafting Planner without contextual conflict verification.

## Third-party precedents

### Recipe Pin 0.3.0

Current Nexus documentation says Recipe Pin:
- supports crafting/building recipe pinning;
- shows available/required counts;
- counts player inventory and bags;
- supports mouse/keyboard and gamepad;
- currently supports only one pinned recipe.

Its 0.2.0 changelog documents input changes specifically to reduce Queue Everything/Quick Stack conflicts.

### Quick Stack 1.0.0

Current Nexus documentation describes Quick Stack as a world-interaction “Stack” action that transfers matching player items to the target chest.

Accepted binary/static research identifies `MultiInventory.MoveItemTo` use and patches around world interaction/bubble refresh.

**P0 compatibility direction:** keep Crafting Planner's `Take Needed` in open-storage context and avoid owning Quick Stack's world-interaction seam.

## Open runtime gates before production

1. Final production pin/manage gesture and help presentation, especially gamepad behavior and conflicts. Build-menu mouse focus is runtime-verified; gamepad build focus was not exercised in Probe 0.2.0.
2. Concrete current-storage ownership lifecycle without private-field reflection.
3. Final open-chest action/help placement. The native Option2/Y transfer seam already works with Quick Stack/Recipe Pin/Queue Everything installed.
4. Player bag/toolbelt counting behavior in the exact target runtime if production relies on those edge semantics.
5. Capacity-limited destination behavior if native-path static evidence is judged insufficient for release acceptance.
6. Production UI anchor/lifecycle in the native NGUI hierarchy.

The P0 project requirement/capture model for the accepted builder and `change_wgo` world-project families is no longer an open runtime gate.


## Runtime evidence — Research Probe 0.1.0 (2026-09-22)

**Verified runtime** on Graveyard Keeper 1.407 with the user's normal 35-plugin setup, including Quick Stack 1.0.0, Recipe Pin 0.3.0, Queue Everything 2.2.1, Specialized Storage 1.2.0, and other installed mods.

Returned `LogOutput.log` SHA-256:
`2154067363657c21b4ce05bb1072420aff049732d33aa27463f7642ff81315e6`

### Craft focus seam

The probe captured five ordinary fixed-ingredient craft definitions through the candidate focus seam:
- `wood1_2 -> wood:1`
- `chisel_1_2 -> stick:4, detail_1:3`
- `armor_lamellar_1_2 -> skin:4, detail_1:6`
- `lense -> glass_0:2, polishing_paste:1, faith:2`
- `wooden_plank_3 -> flitch:1`

This proves the candidate craft-focus callback is live in the installed runtime. The probe did not tag whether each focus event originated from mouse versus gamepad, so that finer distinction is not claimed.

### Player Have versus interaction inventory

For `flitch`, the probe observed:
- player-only Have = 0;
- interaction/crafting-access count = 18;
- current chest count = 9.

This is direct runtime evidence that `GetMultiInventoryForInteraction()` is broader than player-carried inventory and must not be used for planner `Have`.

### Current-storage transfer

With `Required=1`, `Have=0`, `Missing=1`, chest count 9, and ample player capacity, native transfer moved exactly one `flitch`:
- player 0 -> 1;
- chest 9 -> 8.

Immediately afterward, the same action observed `Missing=0` and requested 0, proving no surplus transfer in that state.

The probe also exercised a chest with zero matching material: `Missing=1`, storage count 0, requested 0, no transfer.

### Open-chest input seam

Both research triggers worked:
- keyboard F8 research trigger;
- inherited chest-context `Option2 / Y`.

`Option2 / Y` successfully transferred the needed item while Quick Stack, Recipe Pin, and Queue Everything were installed. This makes open-chest Y a strong production candidate, but final production controls should still be tied to the planner's actual chest UI/help presentation rather than hardcoded globally.

### Not yet proven by this log

- build-menu `BuildItemGUI` focus capture (no `kind=build` diagnostic occurred);
- one-off world repair/upgrade/clearing project capture and exact ingredient semantics;
- bag placement/count behavior in a case where a needed item is actually in a carried bag;
- inventory-full/partially-full capacity edge in runtime (static native-path evidence remains strong).

## Production UI substrate

**Verified static**

The game HUD/UI uses native NGUI-style components including `UIPanel`, `UIWidget`, `UILabel`, `UIButton`, `UI2DSprite`, and `GamepadSelectableButton`. `GUIElements.me.hud` owns the main HUD hierarchy and native windows notify the HUD on open/close.

Therefore a vanilla-style planner HUD is technically feasible without using a generic BepInEx/IMGUI gameplay overlay. Preferred direction is a narrow custom planner object inside the game's UI hierarchy, reusing or cloning verified native visual components where practical. Exact anchor/prefab reuse remains a runtime/UI implementation gate.


## Construction UI ownership — static verification 2026-09-22

**Verified static**

Normal construction desks do not use a separate `BuildsGUI` list as the primary path observed by the player. `MainGame.OpenBuildObjectGUI(build_desk)` builds a `CraftsInventory` from visible `ObjectCraftDefinition` entries whose `builder_ids` contain the current builder object, then calls:

`CraftGUI.OpenAsBuild(build_desk, craftsInventory)`

`CraftGUI.OpenAsBuild` sets build-specific state, disables craft-amount buttons, and opens the ordinary `CraftGUI` with that object-craft inventory.

Therefore the production/research capture boundary for normal construction should distinguish `CraftGUI` **build context** rather than assuming that `CraftItemGUI` implies ordinary workstation production.

**Verified static input detail**

`CraftItemGUI.OnMouseOvered()` is the mouse-hover callback. `CraftItemGUI.OnOver()` is the gamepad-focus callback and returns immediately when the GUI is not in gamepad mode. Probe 0.1.0 only instrumented `OnOver`, so its successful focus evidence must not be treated as proof of mouse capture. Probe 0.2.0 instruments both callbacks separately.

## One-off world-project discovery — static basis for Probe 0.2.0

**Verified static, semantic boundary not yet accepted**

World projects may enter several host-owned paths:

- `WorldGameObject.Interact(...)` dispatches by `ObjectDefinition.InteractionType`;
- `Builder` opens `MainGame.OpenBuildObjectGUI`;
- `Craft` can run a valid interaction script first and otherwise delegate to `CraftComponent.Interact`;
- `RunScript` executes the object's valid interaction script;
- `CraftComponent.Interact` opens the native craft GUI when the object has interactable crafts;
- `CraftComponent.FillCraftsList()` resolves object crafts from `GameBalance.me.GetCraftsForObject(wgo.obj_id)`;
- `WorldGameObject.TryStartCraft(craftName)` resolves a concrete `CraftDefinition` and invokes native craft start;
- project-like definitions can additionally carry `one_time_craft`, `change_wgo`, `end_script`, `end_event`, or `craft_after_finish`.

This proves that repair/upgrade/clearing operations cannot safely be assumed to be one uniform recipe category from static inspection alone. Probe 0.2.0 observes representative live interactions before a production inclusion rule is chosen.


## Runtime evidence — Research Probe 0.2.0 (2026-09-22)

**Verified runtime** on Graveyard Keeper 1.407 with the user's normal 35-plugin setup.

Returned `LogOutput(1).log` SHA-256:
`3bf9cb3782b623e40c38ef70189f21de751585d2ea568c0b09ec3c8db9b29fce`

No Crafting Planner Research Probe warning/error/exception was logged.

### Builder projects

The native graveyard builder opened a build-context `CraftGUI` with exact `ObjectCraftDefinition` entries. A flower bed was observed with:

- `needs = stone_plate_1:2, peat:1, flw_poppy:2`;
- `BuildType.Put`;
- `out_obj = flowerbed_2x2`;
- `builder_ids = graveyard_builddesk`.

Mouse focus produced the same exact definition, proving the build-card mouse capture seam in runtime.

The user also selected `graveyard_builddesk:p:grave_empty_place`; the probe recorded `build_selected`, then vanilla entered `CraftBuilding(... type=Put)` / build mode. This confirms that the observed card definition is the same host object used by the native selection path.

### One-time builder upgrade

The home wood builder exposed:

`mf_wood_builddesk::upgrade_player_buildzone`

with:

- `needs = detail_2:10, wooden_plank:8, nails:4`;
- `one_time_craft = true`;
- `BuildType.None`;
- `out_obj = upgrade_player_buildzone`;
- `end_script = player_buildzone_upgrade`.

The same definition was captured through mouse focus. This is accepted evidence that at least this class of one-time builder upgrade has stable concrete material requirements compatible with the planner model.

The same builder also exposed a normal repeatable placement:

`mf_wood_builddesk:p:mf_box_stuff_place -> flitch:4, nails:4, detail_1:4`

using `BuildType.Put`.

### Repair projects

Representative broken mortuary objects exposed a single ordinary `CraftDefinition` through the interacted object's normal craft GUI:

- `fix_morgue_builddesk -> flitch:2, detail_1:2; change_wgo=morgue_builddesk`;
- `fix_morgue_throw_in -> stone_plate_1:2, detail_1:4; change_wgo=morgue_throw_in`;
- `fix_morgue_throw_out -> stone_plate_1:2, detail_1:4; change_wgo=morgue_throw_out`.

These are not `ObjectCraftDefinition` builds. They are ordinary craft definitions whose semantics are world-state replacement/repair, but their planner requirement source is still the canonical `needs` list.

### Blocked-passage clearing

`blockage_V_low` exposed exactly one ordinary craft:

`blockage_V_low_destruction -> spike_1:10, wood_balk_1:1, detail_1:4`

with `change_wgo=0`.

It opened the normal `CraftGUI`, just like the repair cases. This verifies the same P0 requirement shape for at least this blocked-passage/clearing class.

### Accepted semantic inclusion rule

For P0, the evidence now supports two host-native project families without hardcoded recipe-ID tables:

1. **Builder context**: concrete `ObjectCraftDefinition` supplied by the native builder UI, with plain exact `needs`. Repeatable placement and the observed one-time builder upgrade are both valid planner shapes. Internal move/remove pseudo cards are not project goals.
2. **World change craft context**: a concrete ordinary `CraftDefinition` owned by the interacted world object, with plain exact `needs` and a non-empty `change_wgo`. The observed repair and clearing cases fit this shape.

Do not extend this rule to unrelated special/scripted craft systems merely because they also open a craft-like UI.

### Not proven / not required to close this P0 data-model question

- gamepad build-card focus was not exercised; only mouse focus is runtime-confirmed in Probe 0.2.0;
- the repair/clearing crafts were opened but not completed, so completion hooks remain a later concern (P1 auto-decrement, not P0);
- carried-bag and capacity-limited transfer edges remain separate inventory-runtime questions.


## Native resolution / NGUI scaling lifecycle — static verification 2026-09-22

**Verified static**

Graveyard Keeper owns GUI resolution changes through `MainGame.OnScreenSizeChanged(int w, int h)`.

That path:

- resolves the current screen dimensions;
- applies the game's configured pixel size to the world camera;
- updates `MainGame.ui_root.manualHeight = screenHeight / gui_pixel_zoom`;
- calls `GUIElements.RecalcScreenResolution(w, h)`;
- then lets the existing NGUI hierarchy recalculate its layout.

The target runtime currently uses `gui_pixel_zoom = 2`.

NGUI's `UIRoot` also exposes its own scaling model (`scalingStyle`, `activeHeight`, `pixelSizeAdjustment`) and anchored rectangles/widgets.

**Architecture consequence**

Crafting Planner must not treat the user's current 2560x1440 resolution or a manually derived `Screen.width/Screen.height` offset as the canonical HUD coordinate system.

The production HUD should attach to a verified native NGUI ownership/anchor context and allow the game's own resolution/layout lifecycle to reposition/scale it across resolutions and aspect ratios. Small user-facing offsets may later be configurable for visual preference, but those offsets must be relative to the accepted native anchor/layout model rather than compensating for an unknown coordinate system.

HUD Rendering Probe 0.3.0 completed the runtime verification.

**Verified runtime**

- A Planner label placed directly in the HUD panel with screen/root-derived coordinates could be fully active and drawable while its projected rectangle remained entirely outside the viewport.
- A control label cloned from the live `HUD.zone_name` label and kept under that label's existing native parent rendered on-screen.
- A second control moved directly under the HUD panel did not.
- The same native-parent control remained correctly attached to the visible HUD after a 1600x1200 resolution change followed by an external borderless/window resize back toward the desktop-sized window.

**Accepted HUD layout rule**

For the ordinary gameplay Planner HUD, use the verified `HUD.zone_name` parent coordinate context. Position Planner content only by a small local offset from that native label/context.

Do not use `Screen.width`, `Screen.height`, `UIRoot.activeHeight`, or one-time root-panel coordinate conversion as the canonical placement mechanism.

Do not add Borderless Gaming-specific polling or resolution hooks merely to compensate for external window resizing; inheriting the native HUD parent transform is the narrower compatibility mechanism.


## Planner UI continuity and native item icons — verification 2026-09-22

### World-project icon spot-check — static 2026-10-15

Using the same pinned 1.407 `resources.assets` game_data (MonoBehaviour150254), reconstructed with the already documented UnityPy/TypeTreeGenerator toolchain, verified canonical `CraftDefinition.icon` values: `fix_morgue_builddesk`, `fix_morgue_throw_in`, `fix_morgue_throw_out` all use `i_b_hammer`; `blockage_V_low_destruction` uses `i_b_mining`. Matching native Sprites exist and are 32x32 blue construction pictograms. Thus these accepted representative world-project families have native artwork, but different repairs share a pictogram: project names remain meaningful. This is a four-definition static spot-check, not artwork/runtime acceptance for every scripted quest. Private data dump/art stays under ignored `.local/`; no host payload entered Git.

### Direct-edit input review — static 2026-10-16

Current 0.1.6 project source has mouse hover focus (`CraftItemGUI.OnMouseOvered/OnMouseOuted`) and native focused-project LT/RT adjustment (`BaseGUI.OnPressedPrevSubTab/OnPressedNextSubTab`). Mouse hovering alone does not add a goal. The only current owned mouse click action is chest Take Needed; HUD/project quantity arrows, a mouse add-to-plan action and inventory editing are not implemented.

Exact 1.407 `GamePadButton` enum includes face buttons, bumpers, triggers, directional inputs, Back and Start, but no stick-click LS/RS members. `GamePadController` owns separate movement sticks and reads semantic buttons through `_rewired_bindings` / `Rewired.Player`; no ready stick-click semantic route is established by this inspection. This does not prove hardware clicks unavailable or globally conflict-free. Rewired maps/raw supported controller paths and native glyph availability still need verification before choosing a stick-click binding. Private inspection output: `.local/stick-binding-review.txt`, `.local/stick-controller-il.txt`.

External compatibility reference: [Recipe Pin author description/changelog](https://www.nexusmods.com/graveyardkeeper/mods/154), inspected 2026-10-16, documents hold-Y HUD management only while no normal work interaction is available. Its 0.2.0 changelog records hold-X pinning to preserve short-X/Queue Everything and a fix for QuickStack broken by Recipe Manage integration. The description still says press-X; retain this internal documentation inconsistency rather than claiming exact current runtime behavior from prose. These are author claims, not acceptance of our input path or permission to copy implementation/assets.

Exact 1.407 `CraftGUI.OnPressedLeft/Right` already owns horizontal input: it decreases/increases a focused queue item while `_gamepad_in_queue_area`; otherwise it calls `CraftItemGUI.OnAmountMinus/Plus` when the focused item's `amount_buttons` exists and is active; otherwise it delegates to `BaseGUI.OnPressedLeft/Right`, which calls native navigation. Up/down also checks `ProcessIngredientStep` before base navigation. Therefore generic horizontal replacement is unsafe; a plan handler must prove its supported focus/context and preserve native active amount/queue/navigation consumers. Private inspected IL: `.local/directional-plan-input.txt`. This is static evidence, not installed directional-input acceptance.

Further exact 1.407 static inspection identifies a physical stick-click route: Assembly-CSharp-firstpass defines Rewired.IGamepadTemplate; the installed Rewired_Core Controller.GetTemplate<T>() resolves it, with rightStick.press.justPressed available through core template interfaces. GamePadController uses ReInput.players.GetPlayer(0), so the same player's last active connected joystick is the bounded device owner. No hardcoded raw button index is needed. Native GameKeyTip.RightStick(true,true,false,text) formats the host `(RS)` symbol; GetIcon special-cases MapCursor (41). This establishes host query/signature/artwork paths, not successful input delivery on the user's controller. Private evidence: .local/rewired-template-api.txt, rewired-template-input.txt, native-stick-tips.txt and native-tip-icons.txt.

Native UIFont151314 (the zone-name label's verified bitmap font) has 305 BMFont saved glyphs and 156 symbol entries. The actual glyph table contains ASCII `<` (60) and `>` (62), but not `‹`/`›` (8249/8250) or Unicode directional arrows (8592–8595). Browser appearance cannot establish their native availability. Use verified ASCII counter arrows and native GameKeyTip direction/RS symbols rather than those Unicode glyphs; runtime language/font acceptance remains required. Private read-only inspection: .local/check-ui-glyphs.py.

**Verified runtime / static**

Crafting Planner 0.1.4 is visible in the ordinary gameplay HUD when attached to the verified `HUD.zone_name` parent context. Live language changes redraw the Planner correctly in the exercised runtime languages.

The native project/build `CraftGUI` hides the ordinary HUD when opened and shows it again when closed. Therefore production should not keep a Planner child under the hidden HUD alive by overriding vanilla HUD lifecycle merely to preserve continuity.

Accepted architecture direction:

- keep the ordinary Planner surface in the verified HUD context;
- keep the project/build Planner surface in the native CraftGUI context;
- make both surfaces use the same visual/layout specification and the same planner state;
- place them at the same apparent screen location so the transition is visually seamless;
- do not create a second navigation/focus system merely for the Planner display.

For material icons, the game already owns the canonical icon path. Native item UI resolves an `ItemDefinition` icon name with `ItemDefinition.GetIcon()` and loads the sprite through `EasySpritesCollection.GetSprite(...)` into a `UI2DSprite` (for example, `BaseItemCellGUI`). Crafting Planner should reuse that path rather than maintain an icon database.

**1600x1200 + external borderless behavior**

The 0.1.4 runtime pass applied the game's native 1600x1200 windowed resolution and `OnScreenSizeChanged(1600,1200)`. Immediately afterward the runtime detected external window-size changes through approximately 1616x1239 back to 2560x1440. The user screenshot showed the vanilla HUD itself globally displaced/scaled in that state, not a Planner-only failure.

The Planner remained attached to its native HUD context throughout. Do not add Planner-specific Borderless Gaming polling, screen-size compensation, or alternate coordinate math to repair a host/global UI state. Such compensation would reintroduce the coordinate-system duplication that Probe 0.3.0 rejected.

### 0.1.10 native correction evidence — 2026-10-16

Same pinned 1.407 assemblies as above. UICamera.FindCameraForLayer searches registered cameras by culling-mask layer; cachedCamera is public. Arrow visibility must not depend on a one-time camera lookup before native GUI initialization. UILabel glyph/collider ownership uses the same verified UIEventListener dispatch as other controls.

GUIElements.game_gui is public GameGUI. GameGUI.SelectTab(TabType) writes private _current_tab_type; enum Inventory=0, Techs=1, NPCs=2, Bodies=3, Map=4. No public current-tab getter was found. The planner reads that field only to restrict inventory visibility and refreshes after SelectTab. BaseCraftGUI.object_info and UniversalObjectInfoGUI.text_header are public title anchors. Native ingredient child worldCorners, not the table's own two-unit anchor widget, describe the visible right edge. Static evidence supports the correction; installed geometry acceptance remains open.

Native current-language font correction is owned by GJL.EnsureLabelHasCorrectFont. Shared research UI_INPUT_TIME_AND_ENVIRONMENT.md documents atlas font/language distinctions and the live-language consumer requirement. Candidate raw labels rejoin that method without caching destroyed widgets; HUD/craft-template profile comparison imports no new font asset. This is not a promise that a browser Georgia mockup renders identically.

### Native UI correction ownership — 0.1.11 static inspection

On the same pinned 1.407 assemblies, CraftItemGUI.Redraw IL0506 sets ingredients_table.repositionNow. UITable.set_repositionNow merely marks mReposition/enabled; UITable.RepositionVariableSize later measures via NGUIMath.CalculateRelativeWidgetBounds with considerChildren. UITable.Reposition finishes with its public parameterless onReposition callback. 0.1.11 subscribes only to actual supported rows' ingredient tables and the native craft list, preserving other callbacks and unsubscribing on teardown; no global UITable Harmony patch is used. Own controls are outside the table's measured descendants, with explicit lifecycle hiding because they no longer inherit row activation.

UIRect.AnchorPoint has public target/relative/absolute. ResetAndUpdateAnchors calls ResetAnchors then UpdateAnchors; ResetAnchors refreshes each cached rect from its current target. This permits an owned caption outside the native title table to follow quantity horizontally and native title vertically without contributing to either table's measurement.

HUD.UpdateZoneInfo only writes zone strings and toggles zone_descr_object. Native level2 hierarchy places zone-name UI2DSprite and bottom-field description as siblings under the same bare location widget. Suppress that common widget's alpha while the owned planner banner is visible, then restore its captured alpha. Keep native text and active-state updates intact. Both resources UIFont151301 (small_font) and UIFont151314 contain 156 symbols including (RS) and (B); native-profile selection does not require importing controller art. These are static facts, not acceptance of all languages or the new layout in-game.

### 0.1.12 native calibration seams

Exact 1.407 level2 UI2DSprite32958, targeted by the row selection-frame's left anchor, has native UIBasicSprite.type Advanced (4), border10 on all edges. UIBasicSprite publicly exposes rightType, with AdvancedType.Invisible=0, Sliced=1, Tiled=2. This is a native visual-only edge setting; preserve/restore its prior value and mark geometry dirty rather than alter row width or recreate the renderer.

Chest ButtonTipsStr25040 label left/right anchors target Transform10809, absolute -15/-7, pivot Center, width252/height16. Native ButtonTipsStr.Print only writes label.text. Apply a reversible X delta to both anchor absolutes for the whole native line; keep its original localized text/active state and input handlers. Take Needed/feedback offset formulas compensate this native-line movement so each calibration block has an independent origin. No native-tip splitting or private GameKeyTip fields are needed.

Native caption panel chain in exact level2: Craft GUI / Craft GUI (1) owns UIPanel depth3/2; Universal Object Info contains the native header under its own UIPanel depth35. An owned header caption must join that native title panel instead of relying on a higher widget depth in the lower CraftGUI panel. This statically explains the owned caption's occlusion, not universal menu acceptance.

Static 1.407 chest tip composition: ChestGUI.OnItemOver passes Select/Option1/Close to ButtonTipsStr.Print in A/X/B order. When native max transfer is <=1 it supplies only Select/Close; custom/non-main unsupported cells use PrintClose. Array Print overloads delegate to Print(List<GameKeyTip>,string); single Print(GameKeyTip) directly writes label.text. Public GameKeyTip.ToString supplies localized icon/text plus disabled alpha tags. This permits a scoped display-only observer without private key-field access or parsing localized phrases. The original tip label's rightAnchor targets the chest window with absolute -7; use that target directly for a stable right edge. Native input remains owned by ChestGUI/BaseGUI. Exact native inspections are preserved privately in revision13-chest.txt/revision12-tips.txt; no proprietary code/assets are committed.

### Quick Stack outbound invalidation path — static 1.407, 2026-10-16

The installed Quick Stack1.0.0 DLL SHA2569ade7dcf7f23af0bfd5cf06dbfa55c836f3b72e31b657013cf57dcc33d0d34fa calls MultiInventory.MoveItemTo(MultiInventory,Item,int,bool,bool). Native MoveItemTo removes through private MultiInventory.RemoveItemNoCheck -> public Item.RemoveItemOrReturnLeftCount/RemoveItemNoCheck, or public TryRemoveSpecificItemNoCheck directly removes a unique item from a list. It then calls destination AddItemNoCheck -> Item.AddItem, and returns true after the move. Thus an Item.RemoveItem(Item,int,Item) observer misses this outbound path; the destination AddItem observer also cannot refresh a player-only filter when destination is the chest. This explains the reported stale HUD without changing the accepted counting semantics.

Public MultiInventory.all is List<Inventory>; public Inventory.data identifies each native inventory owner. Quick Stack constructs source Inventory(MainGame.player) and destination Inventory(chest). Observe that exact native successful MoveItemTo overload, filter either side by player.data identity, coalesce until LateUpdate and refresh only visible goals. This avoids a dependency on Quick Stack private methods or input. Implemented in 0.1.14 and user-accepted for the exercised Quick Stack1.0.0 outbound path: Have/Missing refresh immediately (TEST_LOG.md). Foreign direct-list mutation and arbitrary bag-only owners are not generalized. Private static evidence: revision14-quickstack.txt, revision14-inventory.txt, revision14-nocheck.txt, revision14-item.txt. Only minimal verified signatures are added to public stubs.

### Native action-tip gradient/frame/artwork — static 1.407, 2026-10-16

ButtonTipsStr22661 and23510 labels use font151314, base mColor white, applyGradient=true, gradientTop(1,.9905462265,.9705882072,1), gradientBottom(.7215686440,.5921568871,.4392157197,1), native Outline8. Copying only color/effect therefore loses native tan shading. Public UILabel.applyGradient/gradientTop/gradientBottom allow a scoped style copy; do not change the shared font helper for accepted consumers without a separate gate.

Their rightAnchor targets Transform11664/10878, native background widgets383x317/344x265 respectively. For a frame-pinned right-aligned tip row use that verified target's right edge, not BaseGUI.ui_rect (which is simply GetComponent<UIRect> on its root) or the original centred tip label's text bounds. UIFont151314.atlas resolves UIAtlas151315, containing gp_SW_arrow_L/R (17x17) visually verified as dark round directional buttons. Public UIFont.atlas, UIAtlas.GetSprite and UISprite.atlas/spriteName can reuse them without copying/distributing assets. Native-reference compilation verifies signatures; the combined action row and round arrows were user-accepted in 0.1.15 and the complete visual presentation in 0.1.16. This does not establish exhaustive sprite availability across every language/context combination. Private inputs: revision15-tip-assets.py, revision15-native-api.txt, revision15-buttons.png, revision15-menu-anchor.py; no native payloads are tracked.

### Native repair spend label — static1.407, 2026-10-17

CraftItemGUI.txt_spend is public UILabel; Redraw writes current_craft.GetSpendTxt(craft_gui_interface.GetCrafteryWGO(),_amount). GetSpendTxt formats native energy as `[c](en)[/c]` plus the computed value; other auto/time/special costs may append additional lines. Do not recompute expressions or identify energy by a copied recipe list. Native row prefabs24662/27338 use centred spend labels27624/26309, height36/font16, bottomAnchor.absolute=-12 and topAnchor.absolute=22 against narrow right-side anchor widgets. Keeping top fixed and raising bottom by2*shift moves the centre up byshift while reducing empty vertical bounds, so the native36-unit widget need not expand beyond the row. Limit this to visible single-line energy and bound by printed height+2; restore the captured bottom anchor before native redraw and on hide/unload. Existing planner post-Redraw/layout callbacks own final overlay positioning. Static inputs preserved privately in revision16-row.txt/revision16-spend.txt/revision16-spend-assets.py; the energy separation and complete visual presentation were user-accepted on 0.1.16 (2026-10-17), bounded to the exercised single-line repair case.

**Production use,0.1.20 candidate (2026-10-17):** the repeatable placement tracker now combines the previously verified scoped successful RemoveItems call with normal DoPlace completion and the captured original WGO false->true just_built transition. Exact goal identity and once-only state checks are tested; real0.1.20 Harmony activation/decrement remains pending. See PLACEMENT_COMMIT_RESEARCH.md. No upgrade/repair/clearing completion is inferred.

**One-off completion investigation (2026-10-17):** ONEOFF_COMPLETION_RESEARCH.md records narrow pinned-assembly follow-up on CraftReally/ProcessFinishedCraft, builder dispatch, replacement, cancellation and GameSave completion. A separate passive probe will verify original-owner identity and actual success correlation for representative repair/clearing/one-time builder upgrade. No production one-off removal or universal completion seam is accepted yet.
