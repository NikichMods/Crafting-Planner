# Per-save plan persistence — 0.1.25

Status: installed0.1.25 persistence accepted by the user on2026-10-19 after all requested scenarios; returned native evidence is recorded below. User adopts persistence on 2026-10-18. Exact artifact/build/install identity is owned by DEVELOPMENT.md and TEST_LOG.md.

## Outcome and mechanism

Save goal keys and quantities with a successful native world save. Reload/restart restores that plan; another slot and an older, different native world snapshot cannot inherit a newer plan. Empty plans are snapshots too. Unsaved edits follow native save/load semantics.

A global file fails slot isolation. A single latest file per slot can apply newer goals to an older world. Native inventory metadata would modify proprietary save payload and couple planner data to unrelated inventory consumers. Choose mod-owned versioned sidecars: ConfigPath/CraftingPlanner/plans/SHA256(full native slot path)/SHA256(native binary bytes).plan. Only keys and quantities are stored; definitions, names, repeatability and materials resolve from current host data. Byte-identical saves share the latest plan for those bytes. Sidecars are local and do not automatically accompany cloud saves or copied/renamed slots. Historical snapshots are retained without automatic pruning.

## Production gate: READY

1. Observable property: restore the saved ordered goal/quantity list, including deletion/empty plans, without cross-slot/version leakage.
2. Canonical owner: MainGame.save/save_slot and SaveSlotData.filename_no_extension; PlatformSpecific.GetSaveFolder supplies the native directory. GameSave owns binary serialization/deserialization; existing OnGameStartedPlaying resets planner state after world restore.
3. Final writer: private static PlatformSpecific.SaveGameDataToSlot(SaveSlotData, GameSave, OnCompleteDelegate) calls PrepareForSave, ToBinary, writes .new and rotates .dat/backups. It catches disk exceptions internally: normal return alone is insufficient. Capture ToBinary only within the exact save invocation; publish only after matching disk SHA256 and changed file timestamp/creation. FromBinary binds input SHA256 to its exact returned GameSave. The load closure assigns save_slot before OnGameLoaded assigns save and restores the scene. Restore at the existing session-reset boundary, after clearing completion trackers/context and before HUD refresh.
4. Blast radius: three persistence-only Harmony seams, existing session reset and plugin install/stop. Sidecar writes only under the mod directory. Native save files remain read-only to this mod. No crafting, inventory, input, layout or quantity policy changes.
5. Preserved invariants: native save failure cannot publish a new plan; stale save references cannot capture another session; unsupported/corrupt data fails closed and remains on disk; existing success/cancel receipts reset before restored goals are installed; requirements remain derived from host definitions. Hook failure disables persistence independently of gameplay/planner controls.
6. Evidence/acceptance: exact 1.407 signatures and IL inspected privately in planner-save-*.txt and persistence-load-order.txt. Pure codec/storage/isolation/failure/definition tests plus native-reference/clean CI checks precede installation. User exercises native save/reload/restart/two-slot isolation/removal after installation; static tests are not runtime acceptance.

No research sidecar is necessary: static inspection establishes the owners and final writer; the candidate directly exposes the remaining save/load property without synthetic world changes.

## Native return checklist

1. In save A pin two distinguishable projects, with a repeatable quantity3 (a one-off quantity1 can be the second). Use the game's ordinary save, then reload A. Verify both quantities/material totals.
2. Fully exit/restart the game, load A and verify the same plan.
3. Load save B: expect its independent plan (empty before its first planner save). Add a different goal/quantity, save B, then load A and B again: each must recover its own plan.
4. Remove a goal, save/reload: it must stay removed. Save/reload an entirely empty plan too. A completed goal must likewise stay removed after saving; any readily available previously accepted completion path suffices.
5. Optional direct control: after a successful save make an unsaved plan edit, reload that same native world state and expect the saved plan, not the unsaved edit.

Collect the native log before a restart replaces it. Ready marker is `plan_persistence state=ready`; save/restore records include short slot/snapshot hashes; `plan_restored_goal` records exact keys/quantities. No test helper or resources are needed for pinning alone. A user report of successful checks still requires matching candidate loader/installed identity and log review; no new acceptance is asserted here.

## Native acceptance —2026-10-19

User reports all requested scenarios pass, explicitly including unsaved plan changes and full restart. Loader/installed hash matches frozen0.1.25. Private return .local/evidence/return-0.1.25-persistence/LogOutput.log SHA256f42d10c98562044080bc5e2fad4176df71625ee8a0c2298a27f0bba382240cc7; own plan files are copied alongside it. This surviving post-restart session independently shows slot384f7c598c30 restoring3 goals with quantities3/1/1, slot0da2c5c38925 initially without a snapshot then saving/restoring0 and later2 goals, return to the first slot preserving its own plan, saved removals reducing3->1->0, and the second slot restoring1/1 after unsaved increases to2/3. All7 nonempty/empty snapshot restores report skipped=0;6 save receipts and1 no-snapshot load are present. No plan_persistence_error, disable or exception marker occurs in this snapshot. It contains no new goal_completed marker, so automatic-completion-then-save is covered by the user's all-scenarios report and prior completion evidence rather than a separate new traced completion. Full process restart is user-confirmed and supported by persisted files/initial restoration; the preceding process log has already been replaced. No claim of forced I/O-failure runtime testing, independent one-off1 test or exhaustive mod compatibility. Accept the requested persistence feature; no source/binary change, stable merge or release follows automatically.
