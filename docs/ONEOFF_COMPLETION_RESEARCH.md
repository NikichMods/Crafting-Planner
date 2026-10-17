# One-off repair, upgrade and clearing completion

Updated:2026-10-17. Research branch: research/oneoff-completion. Production baseline stays exact Building Planner0.1.20 (DEVELOPMENT.md); no production behavior/version change.

## Question, retrieved evidence and method

User requests automatic removal after successful repair, upgrade and clearing. Repeatable placement is already verified and must remain unchanged. Existing requirement/capture evidence (VERIFIED_GAME_DATA.md), accepted TEST_LOG.md, PLACEMENT_COMMIT_RESEARCH.md and shared GraveyardKeeperResearch CRAFTING_INVENTORY_AND_TRADING.md establish representative native definitions and common-craft lifecycle, but not exact successful one-off goal identity/count correlation. The prior probe observed only repeatable placement. The0.1.20 return opens a world craft, without scoped one-off completion evidence.

Observable outcome: remove only the already pinned exact one-off goal after its real successful native operation, never on mere menu selection/start, cancellation, resource refusal or unrelated craft. Native game alone spends/refunds resources and changes the world. **Production gate remains BLOCKED pending bounded native correlation; research is ready.** The candidate success seam is not yet a production contract.

Considered: GameSave.OnFinishedCraft alone (definition but no original object identity); inventory spend/start success (materials/work can precede cancellation/completion); scoped ProcessFinishedCraft observation with captured original owner, exact definition and matching successful completion marker. Investigate the scoped semantic path first; no reason to rewrite host IL or add polling. A direct ordinary playthrough on0.1.20 does not expose these inner correlations; a passive sidecar is the smallest way to distinguish the real paths without manufacturing results.

## Static follow-up on pinned1.407

Assembly-CSharp SHA256 e72e4270e4b88dd0a87ca23c9cf1750aec4c4a0fedb40b6d2dae7902fc9c7fd8, MVID6f50b8e7-156b-49ac-bbe8-7505894b2364. Private IL remains ignored in .local/commit-craft.txt and .local/oneoff-boundaries.txt.

- Builder non-Put CraftBuilding calls last_build_desk.components.craft.CraftAsPlayer and discards its Boolean. The representative upgrade is one_time_craft=true, BuildType.None, end_script=player_buildzone_upgrade. Capture actual component owner and definition; compare builder-dispatch identity. CraftAsPlayer can return true for already active/queued work, so that Boolean alone is insufficient.
- CraftReally removes native needs and starts current_craft/is_crafting. It checks RemoveItems failure. Depending on automatic/zero-time flags it can invoke FinishCurrentCraft synchronously before returning; parent scopes must survive this nesting. Inventory needs may be a derived list, so probe records actual items and reference equality without assuming identity to definition.needs.
- ProcessFinishedCraft processes scripts/events, applies change_wgo through ReplaceWithObject, reaches GameSave.OnFinishedCraft(current_craft), then End and redraw. Capture owner ID/UID and definition before scripts/replacement/End. End clears current_craft on ordinary flow but has a destroyed-component early return; cancellation also calls End, so End alone is not success.
- ReplaceWithObject has early exits for removed/missing objects. On its normal path it delegates to SetObject after resolving string parameters, then refreshes object machinery. A replacement call alone is not success proof. The clearing recipe's change_wgo=0 is not an ordinary new object ID; record actual resulting state rather than assume destruction timing.
- GameSave.OnFinishedCraft updates native quest/achievement/completed craft state. Its signature lacks the source WGO; only a matching scoped call is relevant. The probe never invokes it.

These are static facts, not proof that every repair/upgrade/clearing action reaches the same runtime path.

## Passive sidecar

research/OneOffCompletionProbe/Probe.cs, GUID nikich.gyk.oneoffcompletionprobe, research version0.1.0. Separate compilation excludes it from CraftingPlanner.csproj. Eight exact observers: CraftReally, ProcessFinishedCraft, Cancel, End, MultiInventory.RemoveItems, WorldGameObject.ReplaceWithObject, GameSave.OnFinishedCraft and BuildModeLogics.CraftBuilding. Bindings verify exact overload/static/return contracts before activation; installation failure removes only probe-owned patches.

Only ordinary definitions with nonempty needs/change_wgo and one-time BuildType.None object definitions produce detailed records. This is an observational filter, not production inclusion acceptance. Other nested calls use neutral scopes so their spend cannot be attributed to a parent. Thread-local scopes exist only during calls; no saved registry, background scan, key binding or UI. seq/parent/phase, original owner/UID and definition correlate asynchronous work and synchronous nesting. Start records actual spend calls/results/items; finish records replacement before/after, matching normal save completion, End, exceptions and after-state. Cancel and builder dispatch provide independent context. Observer errors disable observation; finalizers preserve original native exceptions and restore parent scopes.

The probe never adds resources, starts/finishes/cancels work, edits definitions, changes saves/world/inventory/planner, or automatically removes one-off goals. Its own observation is nonpersistent; the player's actual actions retain ordinary native save consequences. No synthetic setup or forced one-off replay.

## Minimal runtime pass

Use available still-unfinished projects in normal gameplay; no need to repeat the repeatable-placement probe or visual calibration.

| Case | Player action | Evidence sought |
| --- | --- | --- |
| Repair | Pin an available repair such as the broken morgue table; perform and finish it normally | Original world owner/definition, successful start/spend, actual replacement and one normal matching finish |
| Clearing | Pin an available blocked passage; finish clearing it normally | Exact original blockage identity retained through change_wgo=0 and completion, no exception/duplicate |
| Builder upgrade | Pin an available one-time builder upgrade (for example yard expansion if still available); finish its native action | Builder dispatch/component owner agree; script-only completion identified even without change_wgo |
| Refusal/cancellation | Before one success, attempt unavailable materials or leave the menu without starting; interrupt work if naturally available | No finish for rejected/not-started/cancelled work; distinguish pausing manual work from true cancellation |
| Isolation | Keep another pinned goal and perform one unrelated ordinary craft if convenient | No candidate one-off completion attributed to it; production planner remains unchanged for one-off actions during this probe |

One successful example per family suffices for this correlation question. Do not request many irreversible completions. If one family has already been completed on the current save, report that limit; do not fabricate a completed operation or broaden acceptance. Tools collect the accessible local log. event=ready proves only activation, not success. Probe goals are expected to remain in the plan until separately implemented production removal.

## Verification and handoff

Native-reference Roslyn SDK6.0.406 compilation, C#7.3, optimization/deterministic/warnings-as-errors passes. VerifySeams checks all eight exact targets plus public component owner/state against the pinned installed assembly without executing native methods. These checks do not prove live Harmony activation or completion behavior. Exact frozen source/artifact/install identities are recorded in TEST_LOG.md once prepared. No production reference stub changes or UI edits are included.

Installed research0.1.0 at2026-10-17T18:30:03Z beside unchanged0.1.20, with the host closed. Frozen source8c82c7dbe93b0018ab97cb1c1d594402fa759205; DLL SHA25663eb7439581d5e4824765aaf8c1e7cc1bd3ae0b1ba3ecd79d0e78d212cdfe8e3. TEST_LOG.md records exact baseline/build/evidence and open acceptance. No live activation or result is claimed yet.
