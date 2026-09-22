# Crafting Planner HUD Rendering Probe 0.3.0

Research-only. This DLL is not the production mod.

It must be installed **alongside the exact Crafting Planner 0.1.2 candidate**.

## Question

Why is `CraftingPlannerHUD` reported active by NGUI but not visible in ordinary gameplay, while `CraftingPlannerCraftOverlay` is visible inside `CraftGUI`?

The probe also establishes a resolution/aspect-ratio-safe positioning model rather than calibrating offsets only for 2560x1440.

## What it observes

The probe is event-driven. It does not perform per-frame scans.

It records snapshots after:

- the first gameplay HUD open;
- opening the native build CraftGUI;
- RT/LT project quantity changes;
- closing CraftGUI back to gameplay;
- `MainGame.OnScreenSizeChanged`;
- the first HUD reopen after a resolution change.

Each snapshot includes:

- actual screen dimensions;
- `gui_pixel_zoom`;
- `UIRoot` scaling mode, `activeHeight`, `manualWidth/manualHeight`, pixel-size adjustment and transform scale;
- HUD/Craft panel depth, alpha, clipping region and transform;
- planner HUD label;
- working planner CraftGUI overlay;
- representative vanilla HUD labels;
- NGUI active/visible/geometry state;
- widget/panel depth;
- alpha/final alpha/color;
- anchors;
- local/world position and scale;
- widget dimensions;
- projected screen rectangle and whether it intersects the current screen.

Diagnostic prefix:

`CRAFTING_PLANNER_HUD_PROBE`

## Native control labels

For comparison, the probe creates two temporary labels only while the gameplay HUD exists:

- **HUD PROBE A - sibling**: clone of a currently visible vanilla HUD label kept under that label's native parent/anchor context;
- **HUD PROBE B - panel**: clone of the same visible label moved directly under the HUD panel with anchors removed and its position converted into panel coordinates.

They are research-only controls. They do not alter Crafting Planner state and are destroyed when the probe unloads.

The two controls let one runtime answer both questions:
- which ownership/anchor model actually renders;
- which model survives a real resolution/aspect-ratio change.

## Runtime procedure

1. Keep **Crafting Planner 0.1.2** installed.
2. Install **Crafting Planner HUD Rendering Probe 0.3.0**.
3. Load either test save.
4. In normal gameplay, note whether you can see either temporary text:
   - `HUD PROBE A - sibling`
   - `HUD PROBE B - panel`
5. Open a build desk, focus a supported project and press RT once. The existing Crafting Planner build-window text should still work.
6. Close the build window and wait about one second.
7. In Options, change resolution to **1600x1200** (4:3), apply it, return to gameplay, and wait about one second.
8. Open the same build desk once at 1600x1200, then close it.
9. Restore the original **2560x1440** resolution before finishing.
10. Return one `BepInEx/LogOutput.log`.

A screenshot is useful only to state which of Probe A / Probe B is actually visible or if either overlaps/clips unexpectedly. The log is the primary evidence.

## Safety

The probe changes no save/gameplay data. It only creates temporary UI labels and diagnostics.

Changing the game resolution uses the game's own Options path and can persist in settings, so restore the preferred resolution before finishing the test.
