# Development and recovery

Updated:2026-10-19. Current release preparation is **1.0.0**, preserving the accepted0.1.28 behavior. Historical candidate details and bounded native results are owned by TEST_LOG.md; dated pending instructions there describe their original stage.

## Prepared release build

The user requests a fixed ready1.0.0 build and current documentation. Publication preparation for Nexus is explicitly deferred to another conversation. This handoff freezes source, clean build, hashes and player package; no GitHub/Nexus release publication or stable-main promotion is performed by this preparation-only request.

Plugin.PluginVersion is the semantic owner; assembly/file/plugin/package identity is derived/checked. Only0.1.28->1.0.0 identity and SPDX comments change production source. PluginGuid remains nikich.gyk.craftingplanner, canonical installed filename CraftingPlanner.dll; persistence path/schema is unchanged.

Release source/CI/DLL/ZIP/install identity is recorded below after the build completes. Until that record exists, the installed game still uses exact accepted0.1.28, source569c6185ec4a670f43e83e2a96c328309aadabde, DLL257db7aa9ca9012952a1d87994bf2c20d8f29a12dc72a612c5e246fc1e3014f7, CI39171882746/artifact12152897159. No research helper remains installed: CarriedBagSetup0.1.0 was backed up/removed with host closed; CARRIED_BAG_CHECK.md owns return/cleanup identity.

## Accepted behavior and remaining boundary

- Completion: representative repeatable placement/cancel/failure/exactly-once decrement; repair/clearing; yard upgrade; one-time receiver; home cupboard Cancel/Apply. Authored57 builder and39 interior dispatch checks are static compiled coverage, not all definitions executed live. Dedicated lamp2->1 replay was declined; no all-world-script generalization.
- Persistence: per-save/version goals/quantities, save/load/restart/two-slot/deletion/empty plans/unsaved rollback accepted. Local mod snapshots are keyed by save path and native-byte digest; Steam Cloud/copied/renamed slots do not automatically carry/rebind plans. Native save bytes are unchanged.
- Inventory/input: current-storage stock/remainder/no-surplus, partial/full capacity/recovery, ordinary mouse/keyboard/gamepad/context control accepted. Representative builder-bag/flitch contents/ownership and actual5-item bag-only transfer at main capacity0 accepted; not all bags/toolbelt/nested inventories/foreign list mutation.
- UI: accepted0.1.19 independent construction row defaults, bare centred quantities, cap20/one-off1 arrows; no-F2; all11 languages/chest; longest-word growth in0.1.28 and accepted overflow below screen. Do not reopen visual tuning or add scrolling/caps.
- Compatibility: Quick Stack outbound refresh exercised; Max Buttons redraw failure also reproduced without Planner, repair deferred. No exhaustive mod-set guarantee.

RELEASE_1.0.0.md owns support-log/documentation/licensing/package review and proof of version-only parity. A native1.0.0 launch is not yet observed and must not be reported as such. Accepted gameplay evidence is inherited through the verified unchanged implementation; no full replay is required for a metadata-only build. Any future behavior change requires its own gate and a new numbered artifact.

## Source/build/package

Target net472/C#7.3; hosted SDK8 Windows CI in .github/workflows/build.yml. Compile-reference projects contain only verified authored signatures; proprietary game/runtime DLLs are local research inputs and never tracked/shipped. Native references are checked privately with .local/compile-native.ps1 (developer opt-in calibration by default; -Production excludes it). Local dependency-cache limitation is recorded privately in .local/REFERENCE_BUILD_NOTE.md; clean hosted CI is the canonical distributable build.

The player package contains only BepInEx/plugins/CraftingPlanner.dll, README, CHANGELOG, LICENSE, LICENSING and build-identity.json. CI verifies source/version/hash/calibration=false and permitted package files. Reuse the exact CI-produced release package; no differently rebuilt1.0.0 may replace a handed artifact. Numbered source/artifacts remain immutable.

## Recovery and evidence ownership

- AGENTS.md: project contract and current working limits.
- PRODUCT_SCOPE.md: active mechanics/UX; dated design evolution is historical.
- TEST_LOG.md: chronological exact artifacts and bounded native acceptance.
- VERIFIED_GAME_DATA.md: verified host seams, with historical probe gates distinguished.
- PLAN_PERSISTENCE.md: lifecycle/storage/failure semantics and native acceptance.
- PLACEMENT_COMMIT_RESEARCH.md, ONEOFF_COMPLETION_RESEARCH.md, INTERIOR_COMPLETION_RESEARCH.md: completion evidence/READY boundaries.
- FINAL_UI_CHECK.md, CARRIED_BAG_CHECK.md: accepted final presentation/bag check and helper cleanup.
- RELEASE_1.0.0.md: current release audit and artifact results.

Private artifacts/logs/installation backups/receipts remain ignored under artifacts/ and .local/. Accepted0.1.28 return .local/evidence/accept-0.1.28-visual/LogOutput.log hash8cbda4c4d2477c3bd5584ac4c47a26db4be3e1617df378e4f3a2d25c9d13ff06; bag return .local/evidence/return-carried-bag-0.1.0/LogOutput.log hashd31815479139f5ba47ed6cc1b2fb6dee0d768150f4fffce368225bce7ab389a3. Installed helper cleanup receipt is alongside the latter. Preserve readable raw logs locally; publish only derived facts/hashes.

Reusable native facts are promoted in GraveyardKeeperResearch research branches: ordinary inventory/UI, completion/persistence, Max Buttons necessity control, and carried-bag ownership/capacity at1b71926fb5925d59cb16e74ea08c115a0c65edef. Consult RESEARCH_INDEX.md before new host research; do not repeat closed discovery. Product acceptance/release status stays in this repository.
