# Changelog

## 1.0.4 — candidate

- Follow the native HUD alpha and active state during cutscenes; automatically restore the plan when native gameplay HUD returns, preserving goals and quantities.
- Keep construction, chest and inventory Planner visibility independent of the hidden gameplay HUD. Native cutscene verification is pending.

## 1.0.3 — candidate

- Correct location preservation for a scaled HUD, including the configured Show Me Moar HUD scale. Convert location and additional NPC anchor displacement between their actual coordinate spaces.
- Keep the independent Planner banner at the original native banner geometry when HUD scale changes. Native verification remains pending;1.0.2 was rejected because its unit-scale guard disabled the feature.

## 1.0.2 — candidate

- Preserve the original location heading and complete status/quality display to the left of Planner; keep Planner title and counters fixed.
- Hide only an empty no-zone placeholder without useful description. Retain native information for all locations, without a location whitelist.
- Coordinate the relocated location with both NPC cards, applying only remaining overlap clearance and restoring native placement without a visible HUD plan. Native rendering remains under verification.

## 1.0.1 — candidate

- Move the complete native NPC information group left when it intersects the HUD plan, including both character cards and their quest rows.
- Preserve native card spacing/anchors, restore placement when the plan is hidden and keep the group within the available screen width. Actual rendering/transitions remain under native verification.

## 1.0.0 — 2026-10-19

First release build, preserving the gameplay and presentation accepted in0.1.28.

- Plan multiple supported construction, repair, clearing, upgrade and home-interior projects; repeatable quantities1–20, one-off goals1.
- Combine exact material requirements and show Required/Have/Missing from carried inventory and supported bags.
- Take only missing materials from the currently open chest using native stock/capacity limits, including partial and bag-capacity transfers.
- Reduce/remove matching goals after verified native success; cancellation and failure preserve them.
- Restore plans and quantities separately for each local save snapshot, including restart and unsaved-edit rollback.
- Native-style mouse/gamepad UI and all11 built-in languages, fixed quantity columns and width sufficient for long words.
- Development calibration and research helpers are excluded from the player package.

Local plan-file/cloud/copy limits, accepted long-plan overflow and compatibility boundaries are described in README. Detailed prerelease evidence remains in docs/TEST_LOG.md.
