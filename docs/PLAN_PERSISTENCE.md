# Per-save plan persistence — 0.1.25

Status: READY for implementation; native candidate acceptance pending. User adopts persistence on 2026-10-18.

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
