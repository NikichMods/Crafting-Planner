# Successful-placement boundary research

Updated: 2026-10-17. Research branch: `research/placement-commit`, based on `6379eb1`. No production behavior change or new production version; installed Building Planner 0.1.16 remains the baseline.

## Question and readiness

**Production gate: BLOCKED pending installed observation.** For a repeatable placeable, identify exactly one successful native placement/material commitment and the exact existing planner key. Cancelled/rejected actions must produce none. Native code alone spends materials; the planner later decrements one, removes at zero and derives new requirements. This does not concern completion of physical construction.

Owner: `BuildModeLogics.DoPlace`; native commit pipeline: resource check/removal, floating-object retention, optional `_place` replacement, scripts/one-time bookkeeping, `just_built=true`, redraw, optional preparation of another preview. The changed production surface would be session-local goal quantity and derived UI only. Existing manual transfer, UI/input, localization and host placement behavior are preserved invariants. Success/identity/exactly-once acceptance remains open; compilation is insufficient.

## Retrieved evidence and direct static findings

Prior project evidence: VERIFIED_GAME_DATA.md, TEST_LOG.md, private `.local/build-spend-stage.txt` and `.local/completion-discovery.txt`. Shared RESEARCH_INDEX.md / CRAFTING_INVENTORY_AND_TRADING.md establish the recipe model and separate common-craft execution; they contain no accepted placement-success boundary.

Read-only inspection uses installed 1.407 `Assembly-CSharp.dll`, SHA256 `e72e4270e4b88dd0a87ca23c9cf1750aec4c4a0fedb40b6d2dae7902fc9c7fd8`. Private raw IL stays ignored (`.local/commit-floating.txt`, `commit-redraw.txt`, `commit-craft.txt`, `commit-inventory-api.txt`). Distilled findings:

- `DoPlace` returns early when space/resources fail. It calls `RemoveItems` once with `_cd.needs`, destination `AllFromLast`, and discards its Boolean result.
- It captures the current floating WGO, calls `FloatingWorldGameObject.StopCurrentFloating(true)` once, optionally replaces the same WGO with the `_place` object, then writes `just_built=true` at IL `0239` and invokes `Redraw(false,false,false)` at `0242`. Scripts execute before that flag; exceptions cannot be conflated with an ordinary early return.
- `StopCurrentFloating(true)` retains the WGO, restores colliders/path/zone state, destroys the floating component and clears the global preview. The false branch destroys the preview GameObject. A missing preview returns early even with true; the Boolean argument alone is insufficient evidence.
- `DoPlace` may call `CraftBuilding(_cd)` again to prepare the next preview. Capture definition, builder and placed WGO on entry; do not infer the action from the current preview after return.
- Planner keys currently use context kind + builder `obj_id` + definition ID, not WGO unique ID. The probe records builder/object unique IDs as correlation evidence, without changing the adopted goal model. `last_build_desk` is a public static WGO; `_cd` / `_multi_inventory` are private instance fields. Private field injection is confined to research until production mechanism selection closes.
- Separate paths: `CraftBuilding` for non-Put object crafts invokes `CraftAsPlayer` and discards its Boolean. Common `CraftReally` checks material-removal failure; `ProcessFinishedCraft` applies `change_wgo` before `GameSave.OnFinishedCraft` and `End`. These facts locate later upgrade/repair/clearing research; they do not establish its exact identity/success gate or authorize a generic completion hook.

## Mechanism and research-method choice

Considered: unconditional `DoPlace` postfix (rejects cannot be separated); generic inventory observer (no project/success identity); scoped semantic observation of the captured placed WGO and actual native spend; transpiler callback at a verified commit instruction. Prefer investigating scoped observation first: no host instruction rewriting or duplicated placement logic. A transpiler remains an alternative only if the smaller observation cannot distinguish success reliably.

The remaining question is real call correlation, not visual appearance. Existing 0.1.16 logs do not contain these internal observations; ordinary gameplay alone cannot expose exact invocation counts/definition ownership. A passive sidecar adds only observation to the unchanged accepted baseline. No UI recreation, synthetic materials, forced placement, polling, input binding or plan mutation is needed.

`research/PlacementCommitProbe/Probe.cs` is a separate BepInEx plugin, GUID `nikich.gyk.placementcommitprobe`, research version `0.1.0`. All observation is nonpersistent; player actions retain their ordinary native/save consequences. The probe does not call placement or change inventory, saves or planner state. It scopes resource/preview/redraw observation to each actual DoPlace call, counts calls, logs before/after native material totals, captures initial identity and restores nested scope in a finalizer that preserves the original exception. Observer errors disable observation for the session. Initialization failure removes only this probe's patches. Research patches are never compiled by CraftingPlanner.csproj.

## Installed experiment

The user closed the game and explicitly authorized probe installation on 2026-10-17. The exact sidecar below is installed alongside unchanged 0.1.16; installed hash/version were verified, and the closed-session baseline log and newly reported visual-defect screenshot were preserved privately under `.local/evidence/placement-probe-install/`. No game process was running before the copy. Runtime activation/correlation remains unverified; tools will read/preserve the shared log after user gameplay. No user log copying or command entry is needed.

| Scenario | Native action and hypothesis | Required evidence |
| --- | --- | --- |
| Repeated success | Pin a repeatable project x3; place two copies through the ordinary menu/preview | Two distinct attempt sequences, same expected builder/craft key, separate placed object IDs; each has one successful matching spend, one retained preview and built redraw/state; expected material deltas |
| Cancel | Enter another preview and cancel before placement | Cancel outside an active attempt, no additional committed sequence or material spend |
| Busy position | Attempt placement on blocked space if native input reaches DoPlace | Attempt with no spend/retention/built commit; if host suppresses DoPlace, record that as upstream rejection, not exercised DoPlace failure |
| Insufficient resources | Exhaust/move away required resources and use the normal build route | Native rejection with no committed attempt; distinguish upstream UI suppression from DoPlace early return |
| Other goal | Place a different supported definition/builder | Captured identity differs; future implementation must leave unrelated goals untouched |

`event=ready` proves observer initialization only. Each `attempt` must pair with one `attempt_end`; raw counters are observations, not an automatic success assertion. `built_before=true`, missing spend/retention/redraw, unexpected counts, observer/native exception or identity disagreement blocks choosing that predicate. A `cancel` inside an active successful attempt can be the native cleanup after the last affordable copy, so it does not mean placement failed. No HUD reduction is expected with this sidecar. Remove the sidecar after evidence collection; next production candidate will need its own exact-source build and runtime checks of decrement/removal/recalculation.

Upgrade/repair/clearing require separate focused evidence after repeatable placement. Do not expand this probe into a general completion framework or repeat closed visual acceptance.

## Prepared artifact and verification

Frozen observer source: `874383e27dc4ceeea7d00fc7a1bd2708046bc121`; research version/assembly `0.1.0` / `0.1.0.0`. Local ignored artifact: `artifacts/PlacementCommitProbe-0.1.0/PlacementCommitProbe-0.1.0.dll`, SHA256 `d900518477ece2f807fa657d106383e13cacdba843020e6cb1fbb682ac45be4f`; companion `build-identity.json` records source/toolchain. Do not overwrite this artifact. Canonical installed sidecar filename, when authorized, is `PlacementCommitProbe.dll`.

Checks passed: native-reference Roslyn SDK 6.0.406 compilation, C# 7.3, warnings as errors; binary version; metadata-only VerifySeams against exact host private fields/static ownership and public method overloads, MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`; source diff excludes all production/stub/test/workflow inputs. Package directory contains only the original probe DLL and its identity receipt, no host/reference DLLs. This is a local native-reference build, not hosted CI or proof of Harmony activation/Unity behavior. The actual installed baseline hash remains `74f612880574d90a53d771a925f8cbc56bbb69f3e5b1acad8ffa7de189d2d37f`.

Installed at `2026-10-16T23:56:54.8265939Z` (2026-10-17 Moscow): sidecar version `0.1.0.0`, installed SHA256 `d900518477ece2f807fa657d106383e13cacdba843020e6cb1fbb682ac45be4f`. Preserved baseline log SHA256 `3cc6b607f6a19c21bd85640953b332552796a19cfd53f9a39c045e40e3931851`; private `installation.json` owns machine paths/receipt. No game launch or runtime attempt was performed by the agent.

## Returned native observation — 2026-10-17

Preserved log `.local/evidence/placement-probe-return/LogOutput.log`, SHA256 `64e9a562de8baa072cc4d9c27c7aae2b817797b88c5a7c8a6a64cfeef0a0267a`, loads exact probe0.1.0 with the verified host MVID and unchanged planner0.1.16. Installed payload hashes match the identities above. No observer_error is present. No completed-shutdown marker is asserted from this snapshot.

| Observation | Exact evidence |
| --- | --- |
| Two successful repeatable placements | Attempts1/2: `mf_wood_builddesk:p:mf_box_stuff_place`, builder `mf_wood_builddesk` UID182, placed WGO UIDs215333/215349; both before=false/after=true, one matching successful spend, one leave, one built redraw, no exception |
| Material commitment | Each placement removes4 flitch,4 nails,4 detail_1: totals18/51/31 ->14/47/27 ->10/43/23 |
| Busy-space rejection | Attempt3, WGO UID215381: place_allowed=false, native busy log, zero spend/leave/built redraw, built_after=false, no exception |
| Cancellation | Three independent cancel events seq0, without another committed attempt |
| Insufficient resources | Garden build UI logs `OnCraftPressed ... can_craft=False`; no DoPlace attempt follows. This proves upstream menu rejection, not the CanBuild early-return branch inside DoPlace |

The observed repeatable-placement success/cancel/busy paths are now separated and native identity is captured before another preview. This is not evidence of actual planner decrement (still absent), another-project isolation, arbitrary modded/scripted actions or upgrade/repair/clearing success. Before production choose the smallest predicate from these facts and verify goal matching, exactly-once decrement/zero removal and derived refresh on its meaningful candidate. The separate multi-line cost correction adopted in the same user return does not modify this execution path.
