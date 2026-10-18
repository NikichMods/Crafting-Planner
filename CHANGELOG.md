# Changelog

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
