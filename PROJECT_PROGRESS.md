# Project Progress

## Branch
`sow/2026-Q3` is at `be84d7b8`, the merge of [SAM#152](https://github.com/SAM-BIM/SAM/pull/152) (Part O mixed dwelling strategies, accept a dwelling's design). Below it: `8a22c82b`, the merge of [SAM#153](https://github.com/SAM-BIM/SAM/pull/153) (PR2A-1, `SpaceLoadPeak`); `0f866ec6`, the merge of [SAM#151](https://github.com/SAM-BIM/SAM/pull/151) (mixed-strategies PR1 closeout docs); `3de02102`, the merge of [SAM#150](https://github.com/SAM-BIM/SAM/pull/150) (mixed dwelling strategies
PR1, SAM authority + materialisation); `444d2db3`, the merge of [SAM#149](https://github.com/SAM-BIM/SAM/pull/149) (PR0, investigation); `872adb5f`, the merge of [SAM#148](https://github.com/SAM-BIM/SAM/pull/148) (Phase-2 B0 closeout docs);
`00db4b85`, the merge of [SAM#147](https://github.com/SAM-BIM/SAM/pull/147) (PR2A-0, B0 fix); `af0356a4`
(SAM#145, Phase-2 audit docs),
SAM#144 (Phase-1 closeout docs, `a947c5a3`) and `22f9c743`, the merge of [SAM#143](https://github.com/SAM-BIM/SAM/pull/143) (airflow symbol `L/s`).
Below it: [SAM#141](https://github.com/SAM-BIM/SAM/pull/141) PDF renderer (`ba343bfb`), [SAM#142](https://github.com/SAM-BIM/SAM/pull/142)
deep-clone fix (`78a57466`), [SAM#140](https://github.com/SAM-BIM/SAM/pull/140) visual polish (`3ec76eca`),
[SAM#139](https://github.com/SAM-BIM/SAM/pull/139) occupancy gain (`e1fbbb72`), [SAM#137](https://github.com/SAM-BIM/SAM/pull/137)
TM59 per-space status (`7dbeb2e4`), PR1 [SAM#136](https://github.com/SAM-BIM/SAM/pull/136) (`7daf0d32`) and PR0
[SAM#135](https://github.com/SAM-BIM/SAM/pull/135) (`4e027f55`).

## Current (Part O stream): Part O mixed dwelling strategies - accept a dwelling's design (`Modify.AcceptPartODwellingDesign`) (2026-09-27) - PR OPEN

Branch `feature/parto-accept-dwelling-design` from `sow/2026-Q3` `0f866ec6`, merged up with `sow/2026-Q3` `8a22c82b` (SAM#153, PR2A-1) - only this file conflicted. SAM only, additive. Needed by SAM_UI#126
(PR2) **Accept optimised airflow…**: a clean baseline has no design terminals, so a retained (2B) design was unreachable.
Record: `documentation/PartO-MixedDwellingStrategies-PR1.md` §11.

- **What:** `Modify.AcceptPartODwellingDesign(baseline, zone, source)` → `PartODwellingDesignAcceptance` (model, fingerprint,
  changes, refusals). Lineage by `PartFTerminalReference.Matches` (whole dwelling), then `RealizePartFVentilationTerminals`
  (dwelling scope) and `SetSpaceDesignFlowRate` per space/direction. No new airflow rule, nothing in the strategy, inputs
  unmodified, other dwellings untouched, non-clean baseline refused.
- **Files:** new `SAM.Analytical/Classes/PartODwellingDesignAcceptance.cs`, `SAM.Analytical/Modify/AcceptPartODwellingDesign.cs`,
  `SAM.Tests/PartODwellingStrategyMaterialisationTests.Acceptance.cs` (10 tests; local high review added the
  designer-added-terminal refusal); `PartODwellingStrategyMaterialisationTests`
  made `partial` (one keyword); PR1 doc §11.
- **Validation (this machine):** materialisation + acceptance tests 68/68; full `SAM.Tests` **2569/2569** on the tree merged with `sow/2026-Q3` `8a22c82b` (Codex P2 x6 fixed, each with a regression red on the previous head); `SAM.sln` Release
  0 errors. Real 2B data accepted (SAM_UI harness).
- **Next step:** review/merge into `sow/2026-Q3`; then SAM_UI#126 CI (builds against the SAM tip) goes green.

## Current: PR2A closeout - Phase 2 result authority COMPLETE (2026-09-27)

```text
Phase 2 result authority: COMPLETE
PR2A: COMPLETE
Ready for PR2B: YES
```

- [SAM#153](https://github.com/SAM-BIM/SAM/pull/153) (PR2A-1) merged into `sow/2026-Q3` as `8a22c82b`.
- SAM_Tas#69 (PR2A-2, `Convert.ToSAM_Results` fills the typed peaks) merged into SAM_Tas `sow/2026-Q3` as `46dacf22`.
- Reconciled 2026-09-27: both local `sow/2026-Q3` fast-forwarded to those tips; both clean; no other merges after them.
- Final validation: SAM Release build 0 errors; SAM.Tests 2544/2544; SAM_Tas build 0 errors; TM59 963/963;
  Benchmark 16/16; SAM#153 CI green; SAM_Tas#69 CI green.
- Follow-up filed, not fixed: [SAM#154](https://github.com/SAM-BIM/SAM/issues/154) (SAM_Tas `ToSAM_Results` skips a
  space's heating surface results when its cooling result has no `LoadIndex`). Not a PR2B blocker.
- **Next step:** PR2B - typed Space Design Load reporting data + collector in `SAM.Analytical.Reporting`, reading only
  `SpaceSimulationResultParameter.DesignDayPeak`/`AnnualPeak` (audit doc §7). No PDF (PR2C).

## Previous (Reporting Phase 2 stream): PR2A-1 - typed per-simulation load peaks (`SpaceLoadPeak`) (2026-09-27) - MERGED as SAM#153 (`8a22c82b`)

Branch `feature/pr2a1-space-load-peak-2026-09-27` from `sow/2026-Q3` `872adb5f`, merged up with `sow/2026-Q3` `0f866ec6` (SAM#149-#151, Part O mixed strategies) on 2026-09-27; only PROJECT_PROGRESS conflicted. It pairs with SAM_Tas
`fix/pr2a2-tas-peak-authority-2026-09-27` (PR2A-2), which builds against this `SAM\build`. **Merge this first.**
The full record is in `documentation/Reporting-Phase2-ResultAuthority.md` §3.2. Evidence is in
`documentation/evidence/reporting-phase2-gate/pr2a/`.

- **Why SAM owns it:**
  - the reporting collector (SAM.Analytical.Reporting) must read the peaks without referencing SAM_Tas;
  - OpenStudio results share the class.
- **Added (additive only):**
  - `SAM.Analytical.SpaceLoadPeak` (IJSAMObject; guid-less value record; JSON via `GetValue`/`TryGetValue`, safe
    for JsonElement-backed values);
  - the enums `LoadPeakBasis` and `LoadPeakComponent`;
  - `SpaceSimulationResultParameter.DesignDayPeak` / `AnnualPeak`, appended to the end of the enum.
  - No existing parameter changed meaning.
- **Contract, in one line each:**
  - the two peaks are never merged;
  - no peak = unavailable; `Load 0` with no hour = a real zero; a `null`/absent value = not reported;
  - `Load` is a magnitude; components keep the engine sign (+ gain to room air);
  - time is 0-based; `HourOfYear` is annual-only; a design day has an hour of the day only;
  - outdoor state comes from the results.
- **Tests:** `SAM/SAM.Tests/SpaceLoadPeakTests.cs` (9). They cover the whole-model JSON save/reopen through a string,
  sign/closure, zero vs missing, a genuine −1, non-finite refusal, and the Tas 1-based ↔ OpenStudio interval-end ↔
  SAM 0-based hour mapping.
- **Validation:**
  - `SAM.sln` Release: 0 errors;
  - full `SAM.Tests` 2494/2494 (Release, test project built explicitly; 2485 + 9);
  - SAM_Tas against this build: TM59 963/963, Benchmark 16/16.
- **Not done here:**
  - OpenStudio does not populate the peaks yet (its results read as unavailable in the new contract);
  - latent removal load is not persisted;
  - result freshness for non-Part-O runs stays Unknown (PR2D);
  - SAM_UI, SAM_Deploy and reporting are untouched.
- **Next step:** superseded by the closeout above (both PRs merged).

## Previous (Part O stream): Part O mixed dwelling strategies - PR1 SAM authority + NV/MVHR materialisation (2026-09-27) - MERGED as [SAM#150](https://github.com/SAM-BIM/SAM/pull/150) (`3de02102`)

Implementation following the approved PR0 (§D-§F + owner decisions). Branch `feature/parto-mixed-strategies-pr1` from
`sow/2026-Q3` `444d2db3`. SAM only; SAM_UI (`c96ac19a`), SAM_Tas (`aa00ff91`), SAM_Systems (`22133736`) unchanged.
**Full record:** `documentation/PartO-MixedDwellingStrategies-PR1.md` (types/APIs, rules, tests map, PR0 refinements,
risks); linked from `PartO-ARCHITECTURE.md` §9.

- **Authority:** `PartODwellingStrategy` (intent only: route, product-or-pool, `ActiveCooling`, `DesignAirFlowBasis` +
  fingerprint; no airflow) in `PartODwellingStrategySet` (`PartODwellingStrategies:v1`, canonical JSON) persisted as
  `AnalyticalModelParameter.PartODwellingStrategies`. Absent = legacy (refused `NoStrategies`, nothing inferred).
- **Baseline gate:** `Query.PartOBaselineFindings` / `IsPartOCleanBaseline` refuse Part O MVHR type, unresolvable air
  movements (not air movements as such - review pass, PR1 doc §2a), Part-F-applied ICs, record, isolation context, scenarios, provenance, any `IResult` (by stored type), cluster
  `DesignDay`s. Model-level Heating/Cooling Design Days are accepted (DesignDay gate pinned).
- **Materialiser:** `Modify.MaterialisePartODwellingStrategies(baseline, descriptors, scope)`: one call, returns a model only
  when nothing refused. Part F rates/terminals scoped to MVHR dwellings (new `ApplyPartFVentilationRates(…, spaces, …)`
  overload; old signature bit-identical). Per-dwelling design = Iteration 1a's own loop body, extracted as internal
  `RealizeBaseMVHRDwelling`. Dwelling-derived names (`MVHR <zone>`), canonical order. Refuses cooling (gated), NV
  contradictions, NV over authored duty, shared/unconnected authored plant, reused conditioned unit (P12), stale/unbalanced
  retained design, requirement-basis drift, unresolved/not-allowed/insufficient product, authored air movements that
  reach an MVHR dwelling or give an NV dwelling plant/outside air (space->space transfers into NV are carried through),
  mixed corridor zones and corridor-assigned spaces outside a whole-corridor common zone. Never isolates (owner-confirmed
  invariant).
- **Scenarios:** NV `BaseNaturalVentilation`/`NV`, MVHR `BasePassive`/`MVHR` (existing keys); assessed corridor (all
  spaces assigned the TM59 communal corridor IC) → new `PartOIteration.DwellingIndependent`, `CommonSpace`, `UV`, no
  assumptions.
- **Record:** `PartOMaterialisationRecord` (baseline = `SimulationResultProvenance.Fingerprint`, strategies, catalogue over
  identity + max supply + max extract + rank; zone → system guids; `IsCurrent`).
- **Files:** SAM.Analytical: new `Classes/PartODwellingStrategy.cs`, `PartODwellingStrategySet.cs`, `PartOMaterialisation.cs`,
  `PartOMaterialisationRecord.cs`, `PartOMaterialisationRefusal.cs`; `Enums/PartOActiveCooling.cs`,
  `PartODesignAirFlowBasis.cs`, `PartOMaterialisationRefusalReason.cs`; `Modify/MaterialisePartODwellingStrategies.cs`;
  `Query/PartOBaselineFindings.cs`, `PartOFingerprints.cs`; `Create/PartOCommonSpaceOverheatingScenario.cs`. Changed:
  `Enums/PartOIteration.cs`, `Enums/Parameter/AnalyticalModelParameter.cs`, `Create/OverheatingScenarios.cs`,
  `Modify/{PreparePartOIteration,ApplyPartFVentilationRates,AddPartOBaseMVHRSystem}.cs`,
  `Query/{PartOOperatingAssumptions,PartOIterationOperatingMode}.cs`. Tests: new
  `SAM.Tests/PartODwellingStrategyMaterialisationTests.cs` (50); removed disposable `PartOMixedStrategyProofTests.cs`
  (PR0 said PR1 deletes/inverts them); `OverheatingScenarioTests` enum-membership pin updated. Docs: PR1 doc, ARCHITECTURE §9.
- **Validation (after the review pass):** new class 50/50; `FullyQualifiedName~PartO|PartF` 1275/1275 (1238 - 13 PR0 proofs + 50); `PartOIterationPreparationTests` 86/86; `PartOBaseMVHRTests` 34/34; scenario/TM59 256/256; full `SAM.Tests` 2535/2535; `SAM.sln`
  Release 0 errors; `git diff --check` clean.
- **Owner review pass (2026-09-27), done:** undersized manual product refuses (confirmed); no isolation (confirmed,
  documented as invariant); blanket air-movement refusal replaced by the §2a engineering rule; mixed common zones refuse
  as ambiguous (scenarios are zone-scoped) and orphan corridor-assigned spaces refuse; `DwellingIndependent` kept as the
  smallest key-compatible identity, with search evidence in PR1 doc §8.1 (only SAM_Tas `PartODiagnosticLog` C9 mislabels,
  PR3 scope). Local code review then found a shared-unit gap (a unit shared with an unassessed dwelling or corridor was
  not refused); fixed with a red-then-green test.
- **Merged** 2026-09-27 on green CI (build, test, SPDX) at head `fcdcea93`. Codex review was unavailable (usage limit); a
  local high-effort code review stood in and its one finding (shared unit) was fixed before merge.
- **Next step:** PR2 SAM_UI (strategy grid, materialise on a copy,
  accept-2B onto baseline terminals, sidecar v3) in a fresh session. PR3 cooling stays gated on licensed TAS proof.

## Previous: Part O mixed dwelling strategies - PR0 architecture investigation (2026-09-26) - MERGED as SAM#149 (`444d2db3`), SAM_UI#125 (`c96ac19a`)

This is a new programme, separate from the closed Part O UX programme and from Phase 2 reporting. Goal: a
different final Part O strategy per dwelling in ONE analytical model, with ONE annual TAS run and TM59 as the
final authority. **PR0 is investigation only: no production code changes.**

- **Report (authoritative):** `documentation/PartO-MixedDwellingStrategies-PR0.md`, linked from
  `PartO-ARCHITECTURE.md` §9. It contains the mutation map, 21 verdicts, blockers C1-C11, the architecture and
  authority model, the PR sequence with gates, migration notes, and the **binding owner decisions** at the top.
- **Evidence:** `SAM/SAM.Tests/PartOMixedStrategyProofTests.cs` (at `444d2db3`), 13 DISPOSABLE tests
  (`Category=PR0Investigation`) that pinned the pre-PR1 behaviour; PR1 removed them and promoted the matrix.
- **Validation:**
  - PR0 filter: 13/13 pass.
  - `FullyQualifiedName~PartO|FullyQualifiedName~PartF`: 1238/1238 pass.
  - CI on #149: build, test and spdx all green.
- **Owner decisions, binding:**
  1. A clean pre-Part-O baseline is mandatory. There is no undo or adopt path, and
     `MaterialisePartODwellingStrategies` fails explicitly on a non-baseline model.
  2. Assessed common/corridor spaces are included automatically. They are not strategy rows, and they are
     classified from state, never from names.
  3. Project-wide constraints (all-MVHR, product pools) are project settings.
  4. The accepted 2B airflow lives only on `VentilationTerminal`; the strategy holds a reference or fingerprint.
  5. Cooling is recorded and refused in PR1 until the PR3 licensed TPD proof.
- **Order:**
  - PR1 SAM: per-dwelling authority + deterministic NV/MVHR materialisation.
  - PR2 SAM_UI: dwelling assignment + mixed run.
  - PR3 SAM + SAM_Tas: cooling + licensed proof.
  - PR4: acceptance + deploy.
- **Outcome:** merged; PR1 implemented it (section above). The PR0 proof tests were removed by PR1 as planned; they
  remain in history at `444d2db3`.

## Previous (Reporting Phase 2 stream): PR2A-0 - duplicate `SAM.Analytical` ParameterSets / stale TBD design-load read (2026-09-26) - MERGED as SAM#147 (`00db4b85`); B0 closeout docs SAM#148 (`872adb5f`)

```text
Phase 2 result authority: BLOCKED
B0: FIXED (SAM#147, 00db4b85)
Current blockers: B1–B6 / SAM_Tas result contract
PR2B reporting implementation: NOT STARTED
```

Merged 2026-09-26 on green CI (build, test, SPDX); no Codex review was posted (as for SAM#135). Audit doc §3.1 records
the B0 closeout (branch `docs/reporting-phase2-b0-fixed-2026-09-26`). Issue [SAM#146](https://github.com/SAM-BIM/SAM/issues/146) (audit B0). Branch `fix/parameterset-identity-pr2a0-2026-09-26` from
`sow/2026-Q3` `af0356a4`. **SAM.Core only.** SAM_Tas, SAM_UI and the reporting collector are unchanged.

- **Root cause (demonstrated):**
  - `SAM.Analytical` has had no `[assembly: Guid]` since `49069d9c` (2024-05-10); its stable GUID was `fbbd5ce9-…`, which
    old library `InternalCondition`s still carry. `Core.Query.Guid(Assembly)` therefore returns the per-build MVID.
  - `Core.Modify.Add(List<ParameterSet>, …)` matched **by GUID only**.
  - SAM_Tas `Query.UpdateT3D` (line ~91) does `space.Add(Create.ParameterSet(setting, TAS3D.Zone))`. That set is named
    `SAM.Analytical` and carries the current MVID, so a workflow run on a new build **appended** a set. The run's
    `UpdateFacingExternal`/`UpdateDesignLoads` then wrote into it by GUID match.
  - A reader on another build misses the GUID. It falls back to the first set by name, which lacks the key, and then to
    the **first** set containing the key, i.e. the oldest run.
  - Bathroom_2 (`C:\TasOut\final1b\open_out.sam`) has three sets: `990f1c57` authoring, `cc94e7a1` = 0 and
    `feae3a10` = 1139.87 W.
- **Fix:**
  - The name is the identity of a set; the GUID is a hint.
  - `Modify.Add` matches by GUID, else by non-empty name, and merges into that set (`Copy`, later values override).
  - `ParameterizedSAMObject.FromJsonObject` and the `IEnumerable<ParameterSet>` constructor go through `Modify.Add`.
    Legacy same-name sets therefore collapse on load, in stored order: the first set keeps its GUID and position, a
    later set overrides an earlier one for a key, and keys only in earlier sets survive.
  - Invariant: at most one set per non-empty name on an object. So read, write and re-save all hit the same set, and a
    Tas re-run updates rather than appends. That is Defect B, fixed at the SAM seam, so **no SAM_Tas change** is needed.
  - A file with no duplicates loads and re-saves unchanged.
  - `SAMCollection`'s own sets are untouched (round-trip only, with no read API).
- **Precedence evidence:** the rule "later stored set wins" was checked against the persisted
  `SpaceSimulationResult.DesignLoad` over 229 files (`C:\TasOut` 198, `SAM_daily` 22, Nextcloud 9). It gave
  **0 mismatches**; today's first-containing read gave 51. Conflicting values only ever occur in appended sets; the
  first set never conflicts.
- **Files:**
  - `SAM/SAM.Core/Modify/Add.cs`;
  - `SAM/SAM.Core/Classes/Base/ParameterizedSAMObject.cs`;
  - tests: `SAM/SAM.Tests/ParameterSetIdentityTests.cs` (new, 15), and `SpaceAssumptionsTests.cs` (+1 Phase-1 regression
    through the model JSON);
  - harness: `documentation/evidence/reporting-phase2-gate/harness/pr2a0_rescan.cs.txt`.
- **Validation:**
  - Focused tests pass. Red check: with the SAM.Core change stashed, 11 of them fail, including the Space Assumptions
    one (0 W).
  - Full `SAM.Tests`: **2485/2485** (Release, test project built explicitly).
  - `SAM.sln` Release: 0 errors.
  - SAM_Tas tests against the new SAM `build/`: TM59 947/947, Benchmark 16/16.
  - Real fixtures, read-only (`open_out.sam` md5 unchanged), with the production `Convert.ToSAM` and the Phase-1
    collector: **before**, Bathroom_2 read 0 W (PDF 0 W), and 51 stale in 17 of 198 files. **After**, Bathroom_2 reads
    1139.87451171875 W (collector likewise), with **0 stale** over 2056 heating and cooling comparisons. No space holds
    more than one `SAM.Analytical` set after load or after an in-memory re-save.
- **Residual risk:** suppose an old build without its own set wrote a key into the *first* set while a later set still
  held an older copy. The later copy would now win. This pattern was not seen in any of the 229 files.
- **Not in this PR:**
  - audit B1–B6;
  - the Phase-1 set-point sentinels;
  - SAM#138;
  - restoring a stable `[assembly: Guid]` (optional and unneeded);
  - the SAM_Deploy pointer bump, which is how users get the fix.
- **Next step (Reporting Phase 2 stream):**
  - the audit-doc B0 closeout is merged as SAM#148 (`872adb5f`);
  - merge the SAM_Deploy SAM-pointer bump to `00db4b85`, which carries the Phase-1 correction to users;
  - then PR2A in SAM_Tas (B1–B5).

## Previous: Reporting Phase 2 (Space Design Load Summary) - result-authority audit + design gate (2026-09-26) - MERGED as SAM#145 (`af0356a4`)

Branch `docs/reporting-phase2-result-authority-2026-09-26`, from `sow/2026-Q3` `a947c5a3` (SAM#144 merged).
Docs/evidence only; **no product code changed**. Full record: `documentation/Reporting-Phase2-ResultAuthority.md`;
gate PDFs, `Document` JSON and scratch harness in `documentation/evidence/reporting-phase2-gate/`.

```text
Phase 2 result authority: BLOCKED
Next PR: PR2A-0 (SAM) stale TBD design-load read, then PR2A (SAM_Tas) Convert.ToSAM_Results peak correctness
```

- **State:** the Phase-1 closeout docs PRs were open during the audit and merged afterwards: SAM#144 `a947c5a3`,
  SAM_UI#123 `6d2f2d6b` (after resolving a `PROJECT_PROGRESS.md` conflict with SAM_UI#122), SAM_Deploy#52
  `8fe17f64`. Inspected: SAM `sow/2026-Q3` `22f9c743`, SAM_Tas `aa00ff91`, SAM_UI `7e7de033`.
- **Method:**
  - production path traced: `RunWorkflow` → `WorkflowCalculator` → `AddResults`/`ToSAM_Results` → `UpdateDesignLoads`;
  - fixture scans of `C:\TasOut`, `SAM_daily`, `Nextcloud` and the SAM_Validation benchmark;
  - a **read-only** TSD reader on a copy of `C:\TasOut\final1b\open.tsd`;
  - a design gate rendered with the production `PdfRenderer` from the real `.sam`.
  - No simulation was run.
- **Blockers found:**
  - **B0 (SAM, affects shipped Phase 1):** duplicate `SAM.Analytical` parameter sets make `SpaceParameter.DesignHeatingLoad`
    read a stale value (Bathroom_2: 0 W read vs 1139.87 W current; 51/988 spaces in 17 fixture files). Root cause of
    the appending not yet determined.
  - **B1:** the heating `Simulation` branch writes the cooling variables.
  - **B2:** a zero peak is persisted as the Tas −1 sentinel with `LoadIndex` 0.
  - **B3:** `Load` = max(design day, annual); the annual peak is discarded (Bathroom_2 annual 104 W @ 8554 is lost).
  - **B4:** the `LoadIndex` base is Tas 1-based but OpenStudio 0-based.
  - **B5:** the heating results omit the internal gains and RH.
  - **B6:** no persisted cooling peak exists in any fixture.
- **Settled:**
  - Tas hour index is 1-based, 365-day, no DST.
  - A design-day peak has no calendar date.
  - `Load` is a magnitude ≥ 0. Components are + gain / − loss, and heating Σ terms = −load exactly (HDD and annual).
  - Outdoor state must come from the TSD: the model's weather differed from the run's.
- **Design gate:**
  - normal case 1 page, SI and IP;
  - stress case 2 pages (only the net row spills);
  - a single-column group header wraps, so PR2C uses 4-column tables.
- **Next step:** open a SAM issue for B0 and do PR2A-0. Then PR2A in SAM_Tas (B1–B5, plus one short licensed design-day
  run of a small cooled model for B6). PR2B–PR2E only after that (sequence in the doc, §10).

## Previous: SAM Documentation Framework Phase 1 - COMPLETE (closeout 2026-09-26)

```text
SAM Documentation Framework — Phase 1
Status: COMPLETE
```

Phase 1 = the **Space Assumptions PDF**. All of it is merged and deployed:
- SAM: SAM#135, #136, #139, #140, #141, #143 (last merge `22f9c743`). Full `SAM.Tests` 2469/2469.
- SAM_UI: SAM_UI#121 merged as `7e7de033` (Edit › Reports › Space Assumptions PDF; legacy Print RDS kept).
- SAM_Deploy: SAM_Deploy#51 merged as `8e6740af` (pins SAM `22f9c743`, SAM_UI `7e7de033`; installer payload
  gate; installed-product smoke test passed).

The completion record (production chain, delivered capabilities, what is outside Phase 1, follow-ups) is in
`documentation/Reporting-PDF.md` › *Phase 1 status*.

**Still open, deliberately outside Phase 1:** [SAM#138](https://github.com/SAM-BIM/SAM/issues/138) (RH query
naming/mapping, composite `Profile.MinValue`, gain queries returning 0 when nothing is authored) stays OPEN.

**Next step.** Phase 2 planning (started: see the Current section above).

## Previous: reporting airflow symbol `L/s` (2026-09-26) - MERGED as SAM#143 (`22f9c743`)

`sow/2026-Q3` is at `ba343bfb`, the merge of SAM#141 (PR2). Branch `fix/reporting-airflow-symbol-L-per-s` changes the
SI airflow display symbol from `l/s` to the approved Phase-1 convention `L/s`. The symbol is defined once, in
`QuantityFormatter.DisplayUnit(UnitCategory.AirFlow)` (SAM.Core.Reporting); only reporting consumes it, so SAM_UI and
the PDF renderer need no change. `SAM.Units`, Part F/Part O labels and `VentilationUnitPerformanceAxis.Unit_LitresPerSecond`
(source-data metadata) are deliberately untouched.
- Files: `QuantityFormatter.cs`, `DocumentOptions.cs` (doc comment), `QuantityFormatterTests.cs` (new
  `AirFlow_SIDefault_UsesCapitalLitreSymbol`; SI theory row), `PdfRendererTests.cs` (IP no-SI-symbol list now `L/s`).
- Also the `SpaceAssumptions_Full_SI.json` golden (4 `unit` entries; caught by CI, not by the first local filter).
- Tests: full `SAM.Tests` 2469/2469 (Release, built explicitly).
- Merged as `22f9c743` on green CI; SAM_UI#121 (`7e7de033`) and SAM_Deploy#51 (`8e6740af`) consume it.

## Previous: SAM Documentation Framework PR2 - MigraDoc/PDFsharp PDF renderer (2026-09-25) - MERGED as SAM#141 (`ba343bfb`)

**Status.** Merged as `ba343bfb`. Out of scope for PR2: SAM_UI integration (done since in SAM_UI#121), HTML/Excel
renderers, other documents, and any engineering change.

**Feasibility spike (passed, not committed).** PDFsharp-MigraDoc 6.2.0 was checked on netstandard2.0, consumed from
net8.0:
- Noto Sans is embedded as a subset through a custom font resolver;
- a PNG loads from a `base64:` source;
- a table with a repeated header row works;
- the footer prints "Page n / N";
- automatic flow reached 3 pages;
- A4 is 210 × 297 mm.

Two findings:
- MigraDoc resolves its predefined error font, Courier New, on every render. The resolver therefore falls back to
  Noto Sans.
- A table cannot be nested in a cell. Side-by-side sections use frames of measured height instead.

There was no blocker.

**What was added.**
- New project `SAM/SAM.Core.Reporting.Pdf` (netstandard2.0, output to `build/`, in `SAM.sln` under SAM).
  - Package: `PDFsharp-MigraDoc` 6.2.0 (MIT), matching SAM_Revit's PDFsharp 6.2.0.
  - Dependencies are one-way: `SAM.Core.Reporting` has no PDF reference.
  - `PdfRenderer : IDocumentRenderer`, `NotoSansFontResolver`, and the internal `MigraDocBuilder`, `PdfLayout` and
    `TextMeasure`.
  - `Fonts/NotoSans-{Regular,Bold}.ttf` are embedded, from the official Noto repository, with `OFL.txt`.
  - `InternalsVisibleTo("SAM.Tests")`.
- Layout is generic, with no engineering logic:
  - the `identity` section goes in the header band;
  - consecutive `Half` sections pair up; titled key/value blocks in a full section sit side by side (up to 3);
  - side-by-side content taller than half the body height is stacked, so it paginates and never clips;
  - tables are sized to their content, and a column unit goes in the header;
  - value and unit columns are aligned;
  - the footer carries the lines, the legend and "Page n / N"; pages 2 and later get a running header;
  - the SAM mark is drawn as vector shapes; the company logo is optional.
- Builder hints changed in `SAM.Analytical.Reporting`. No value changed; the goldens were checked semantically:
  - the section order is identity, geometry, design-criteria, internal-condition, ventilation, systems, fabric, sizing;
  - fabric and sizing are `SectionWidth.Half`;
  - the internal-condition blocks are ordered gains, occupancy, lighting, infiltration;
  - the fabric "Not present" note uses the new `NoticeLevel.Note`, which is additive in `SAM.Core.Reporting`.

**Files.**
- `SAM/SAM.Core.Reporting.Pdf/**` (new).
- `SAM/SAM.Core.Reporting/Enums/NoticeLevel.cs`.
- `SAM/SAM.Analytical.Reporting/Classes/{SpaceDocumentDefinitions.cs,Sections/SpaceSectionBuilders.cs}`.
- `SAM.sln`.
- `SAM/SAM.Tests/`:
  - `SAM.Tests.csproj`: the reference to the Pdf project;
  - `PdfRendererTests.cs` (new): 21 tests;
  - `Helpers/ReportingDesignGateFixture.cs` (new): the design-gate office, atrium and sparse models, ported from
    the throwaway generator, with fixed GUIDs;
  - `SpaceAssumptionsTests.cs`: order, width and note-level assertions;
  - `Golden/SpaceAssumptions_{Full_SI,Full_IP,Minimal_SI}.json`, regenerated.
- `documentation/Reporting-PDF.md` (new), `documentation/Reporting-LAYOUT.md`, `THIRD_PARTY.md` (new) and this file.

**Validation.**
- `SAM.sln` Release: 0 errors. There are no warnings in the reporting projects; the 12 warnings are pre-existing
  (Core, Geometry, Grasshopper).
- Full `SAM.Tests`: **2461/2461**, the test project built explicitly first. That is 2440 + 21.
- Representative PDFs are written by `DesignGate_RendersOneA4Page` to `SAM/SAM.Tests/bin/Release/net8.0/ReportingPdf/`.
  All four are **1 page A4**, which matches the mock-up:
  - `SpaceAssumptions_{office_SI,office_IP,atrium_SI,sparse_SI}.pdf`;
  - also `SpaceAssumptions_office_SI_logo.pdf` (1 page), `BlockTypes.pdf` (1 page) and `LongContent.pdf` (6 pages).
- They were inspected visually, rasterised with the Windows.Data.Pdf API through a scratch tool that is not
  committed:
  - Office SI, Office IP, Atrium and Sparse are all readable, with no overlap or clipping;
  - the units align, and the IP output has no SI symbol;
  - `n/a`, `not set`, `—` and `0` are distinct;
  - the humidity sub-labels are readable;
  - notices are compact;
  - the footer and legend are correct;
  - long content paginates, with the table headers repeated.
- Spare space on page 1 is about 15 mm for the office and the atrium, against about 35 mm in the HTML mock-up. MigraDoc
  uses the full Noto Sans line height.

**Final correction pass (2026-09-26, on SAM#141).**
- Long unbroken text can no longer overflow a column or frame.
  - MigraDoc breaks lines only at spaces. The renderer therefore measures each space-delimited token against the
    width it is printed in.
  - A token that does not fit is split into pieces, preferably after `_ - / \ . , ; : ) ] } | + &`. With no such
    character, it splits after the last character that fits. A forced line break goes between the pieces.
  - This is generic, with no engineering logic. No character is dropped or added, and the font size is unchanged.
  - A token that fits is never touched, and normal text is added exactly as before.
  - It applies to every text path: labels, values, units, table cells and headers, notices, notes, text, captions,
    headings, the header band and the footer.
- Tests (+6):
  - `LongTokens_WrapWithinTheirColumns_AndKeepTheFullText`: a long underscore name and a 120-character unbroken
    token placed everywhere text can go. Every word of every line is measured against its cell, frame or page width,
    and the full text is kept.
  - `LongTokens_BreakAfterSeparators_ElseByCharacter`.
  - `DesignGate_NoForcedBreaks_AndNothingEscapes` (4 cases): no forced break is inserted and no word escapes.
- Validation:
  - Focused (PdfRenderer, SpaceAssumptions, ReportValue): 65/65.
  - Full `SAM.Tests`: 2467/2467, with the test project built explicitly first.
  - The four design-gate pages are **pixel-identical** to the previous PR head (rasterised and diffed), and each is
    still 1 page.
  - `LongContent.pdf` still runs to 6 pages, with the headers repeated.
  - `LongTokens.pdf` is 1 page. It was inspected visually, and nothing escapes its column or frame.

**Decisions / limitations (documented in `documentation/Reporting-PDF.md`).**
- PDFsharp has one global font resolver per process. Ours is installed if none is set. A foreign resolver is kept if
  it resolves Noto Sans; otherwise `Render` throws a clear error.
- PDFs are not byte-deterministic (PDFsharp writes a creation time and a file id). The tests check structure: pages,
  A4 size, fonts, and the text of the MigraDoc model.
- The differences from the mock-up are listed in `Reporting-LAYOUT.md` §Implementation.
- **PR3 must deploy** `SAM.Core.Reporting.Pdf.dll`, `MigraDoc.*.dll`, `PdfSharp*.dll`, their Microsoft dependencies,
  and `OFL.txt` with SAM_UI. A netstandard library does not copy package DLLs to `build/`.

**Outcome.** SAM#141 merged; PR3 (SAM_UI#121) and the deployment (SAM_Deploy#51) followed. See *Current*.

## Previous: deep clone doubled Guid-less cluster objects (26 Sep 2026) - MERGED as SAM#142 (`78a57466`)

**Status.** Branch `fix/deepclone-guidless-objects-2026-09-26` from `sow/2026-Q3` `3ec76eca`; commit `685599dc`. First of
three coordinated PRs (SAM -> SAM_Tas `fix/parto-replace-run-records-2026-09-26` -> SAM_UI
`feature/parto-2b-sam-growth-2026-09-26`). Full record: SAM_UI `documentation/evidence/parto-2b-sam-growth/GROWTH.md`.

**Defect.** `SAMObjectRelationCluster(cluster, deepClone: true)` (from `bdcfe5df`) re-added each clone via `AddObject`.
An object with no Guid of its own - `Analytical.DesignDay` is a `WeatherDay` - is re-found only by `Equals` (reference
equality), so its clone got a NEW key beside the original, which stayed shared with the source. Every deep copy
doubled the design days; Part O takes one per TAS run, so the 2B live run's model went 12 -> 57,342 design days
(172 KB -> 3.7 MB). Engineering results unaffected (nothing sizes or simulates from those records).

**Fix.** `RelationCluster.ReplaceObjects(Func<X, X>)` (protected) swaps each object in its own slot (same bucket, same
key, relations intact); both deep-clone constructors use it; `DeepCloneObject` now returns the clone and keeps the
throw-rather-than-share guard. Files: `SAM.Core/Classes/Relation/RelationCluster.cs`, `SAMObjectRelationCluster.cs`,
`SAM.Tests/AdjacencyClusterDeepCloneTests.cs`.

**Validation.** SAM.Tests 2441/2441; new `DeepCopy_OfObjectsWithNoGuid_NeitherDuplicatesNorSharesThem` fails pre-fix
(2 -> 64 after five copies). `SAM.sln` Release exit 0. Live 2B re-run: see SAM_UI.

**Next step.** Merged as SAM#142 (`78a57466`). Then SAM_Tas, then SAM_UI; then SAM_Deploy pointers.

## Previous: reporting visual-polish PR - approved v2 builder changes (2026-09-25) - MERGED as SAM#140 (`3ec76eca`)

**Status.** The v2 layout was approved at the design gate as the Phase-1 direction. This small PR implements its
builder and spec changes (T1-T4 of the mock-up) from `sow/2026-Q3` `e1fbbb72`. Out of scope: MigraDoc/PDFsharp,
`SAM.Core.Reporting.Pdf`, SAM_UI changes and PR2.

**Changes.**
- `SAM.Core.Reporting`:
  - `KeyValueRow.SubLabel`: optional supporting text under a label.
  - JSON writes `subLabel` only when it is set.
- Occupancy: the duplicate `Profile` row is removed. The profile remains in the gains table.
- Fabric:
  - a display row whose external and internal areas are both *explicitly* zero is omitted;
  - the omitted rows are named in one `NoticeBlock` `fabric-not-present`, "Not present (zero area): ...". A wholly
    zero category is named once ("Windows"); otherwise the row is named ("Doors (frame)");
  - a missing area is never treated as zero;
  - the note has no unit, and the table keeps the selected SI/IP area unit.
- Sizing:
  - One `sizing-status` notice replaces the two notices. Its text is
    `DesignLoadsUnknownSizingMultiplierNotice` when loads and a multiplier are present, else
    `DesignLoadsUnknownNotice` / `DesignLoadsNoneNotice`. `SizingMultiplierNotice` is removed.
  - Sizing multiplier set nowhere: the collector returns NotApplicable (`Create.SizingFactorNotSet`), and the
    builder prints `not set`. An invalid stored value stays NotAvailable ("—").
- **Semantic, verified in SAM_Tas `Modify/UpdateSizingFactors.cs`:** a space factor of 0 or unset falls back to the
  model factor. With neither set, `zone.maxHeating/CoolingLoad` is left unscaled, so no multiplier applies.
- Room humidity labels are "Humidification set point" and "Dehumidification set point", with the sub-labels
  "lower RH limit" and "upper RH limit". Disabled control still prints `n/a`.
- Footer legend: an entry is added only for a marker actually printed (`—` or `n/a`). A `not set` cell (NotApplicable
  with its own text) adds none.
- New `documentation/Reporting-LAYOUT.md`: the approved v2 layout.
  - A4 portrait; Noto Sans at about 9 pt; paired sections; a fixed unit column; the three-column
    occupancy/lighting/infiltration block.
  - One page is the normal target, not an invariant: exceptional content paginates naturally.
  - Rev 3 §10 (outside git) now carries a pointer to this file.

**Files.** Under `SAM/`:
- `SAM.Core.Reporting/Classes/Blocks/KeyValueBlock.cs`, `SAM.Core.Reporting/Convert/ToJson.cs`;
- `SAM.Analytical.Reporting/Classes/Sections/{SpaceSectionBuilders,SectionFormat}.cs`,
  `SAM.Analytical.Reporting/Classes/Data/SpaceSizingData.cs`,
  `SAM.Analytical.Reporting/Create/{SpaceDocumentData,Document}.cs`;
- `SAM.Tests/SpaceAssumptionsTests.cs`, and the goldens `SpaceAssumptions_Full_{SI,IP}.json` (Minimal is unchanged).

Outside `SAM/`: `documentation/Reporting-LAYOUT.md` and this file.

**Validation.**
- Focused `SpaceAssumptionsTests` + `ReportValueTests`: 38/38.
- Full `SAM.Tests`: **2440/2440**, up from 2433. The test project was built explicitly first; it is not in `SAM.sln`.
- `SAM.sln` Release: 0 errors, and no warnings in the reporting projects.
- New or extended tests cover:
  - missing gain → "—" and authored zero → "0" in the document;
  - disabled humidity → "n/a";
  - `not set` versus invalid "—";
  - one sizing notice, with loads staying `Unknown`;
  - no occupancy Profile row;
  - fabric omission and the note in SI and IP;
  - whole-category naming, and missing ≠ zero;
  - the IP JSON contains no SI symbol.
- Mock-ups: regenerated from the builder JSON with no transforms (`render_mockup_v2.py` now only asserts T1-T4).
  All 4 v2 cases, office SI/IP, atrium SI and sparse SI, print to **1 page** A4. The pre-polish output is kept
  alongside.

**Next step (done).** Merged as SAM#140; PR2 followed (see *Current*).

## Also current: TM59 per-space overall status, structured (25 Sep 2026) - MERGED as SAM#137 (`7dbeb2e4`)

**Merge note.** The status paragraph below was written before the merge. SAM#137 is now merged into `sow/2026-Q3`;
SAM_UI#114 is next in the stated merge order.

**Status.** On `feature/parto-tm59-result-ux-2026-09-25`, from `sow/2026-Q3` `4e027f55` (#135, PR0 units, is merged).
A PR is open against `sow/2026-Q3` and stopped before merge. It pairs with SAM-BIM/SAM_UI#114 (the same branch name,
so SAM_UI's CI clones this branch). **Merge order: this SAM PR first, then SAM_UI#114.**

**Why.** The SAM_UI TM59 result window counts passing and failing spaces. It had restated the report's per-space
combining rule, which was private to `TM59AssessmentReportFormatter` (`CombineForDisplay`). The owner asked for one
authoritative rule, in SAM.

**What changed (`SAM.Analytical`, additive).**
- New `TM59AssessmentReportSpace`: Reference, SpaceName, the space's occupied-space `Checks`, and `ComplianceStatus`.
- New `TM59AssessmentReport.OccupiedSpaces`: NV then MV checks, grouped by `Reference` in report order. Each space's
  status is the report's existing private `Combine`, the same rule its section statuses use: any Fail fails; else any
  Pass passes; else NotApplicable. Corridor and supplementary rows are excluded.
- `TM59AssessmentReportFormatter`: the NV table and Assessment Hours rows iterate `OccupiedSpaces` (the NV entries),
  and the Overall column prints `space.ComplianceStatus`. `GroupBySpace` and `CombineForDisplay` are removed.
  The output is unchanged.
- No criterion, threshold, hour count or verdict changed.

**Validation.**
- `SAM.Tests`: 2368/2368, including +7 in `TM59AssessmentReportTests`:
  - per-space statuses, with no corridor among them;
  - the Overall column equals the structured status (4-case theory);
  - grouping by Reference, not name;
  - empty where no occupied space was assessed.
- `SAM.sln` Release: 0 errors.
- SAM_Tas `SAM.Analytical.Tas.TM59.Tests` 944/944 against this build (no SAM_Tas change).
- SAM_UI live: the saved 1a FAIL report regenerated byte-identically (same SHA-256).

**Next step.** Owner reviews, then merges this before SAM_UI#114. Then move SAM_Deploy's SAM pointer.


`sow/2026-Q3` at `54c43438` (merge of [SAM#133](https://github.com/SAM-BIM/SAM/pull/133), the Nuaire reply work; see the first *Previous*). The #123
manufacturer-guidance work (first *Previous* entry) is merged, and its feature branches are deleted.

`sow/2026-Q3` previously at `e2c0e2c0` - the merge commit of [SAM#120](https://github.com/SAM-BIM/SAM/pull/120), which
descends from [SAM#119](https://github.com/SAM-BIM/SAM/pull/119) and
[SAM#118](https://github.com/SAM-BIM/SAM/pull/118). `b4a1283f` (PR5A,
[SAM#117](https://github.com/SAM-BIM/SAM/pull/117)) is an ancestor. **`SAM#111` is now CLOSED** - see
*Current* below for the final closeout (B4 close/reopen Review acceptance, the manufacturer-ventilation
successor tracker, and the human closure decision).

`sow/2026-Q3` (at `86213eb3`, including the SAM#126/#127 .NET Framework cleanup) was merged into this branch on 2026-09-22 to resolve a `PROJECT_PROGRESS.md`-only conflict; both entries are kept below.

## Previous: Space Assumptions visual design gate (2026-09-25) - v2 APPROVED; T1-T4 implemented by the polish PR

**Status.** This is a design and review task only, before PR2. It adds no MigraDoc/PDFsharp, no
`SAM.Core.Reporting.Pdf` and no SAM_UI change. The owner has since approved v2. T1-T4 merged as SAM#140 (`3ec76eca`), and PR2 is
SAM#141 (see *Current*).

**Where.** The mock-up lives outside git, next to the plan, in
`Documents\SAM_daily\2026-09-25-Reporting\mockup\`. See its `README.md`. The folder contains:
- `generator/`: a throwaway .NET 8 exporter that builds representative spaces with the merged PR1 code and writes
  `Document` JSON;
- `render_mockup.py`: the Rev 3 layout as specified (v1);
- `render_mockup_v2.py`: the proposed layout (v2);
- `print_all.ps1`: headless Edge prints to `pdf/` and screenshots to `png/`.

**Finding.** Printed to A4 with the Rev 3 type sizes (Noto Sans, 9 pt body), the v1 layout overflows to **2 pages**
for realistic data: the office in SI and IP, and the atrium. The proposed v2 fits all four cases on **1 page** with
about 35 mm to spare, at the same type sizes.
- v2 layout: paired half-width rows, the internal condition in three columns, and a fixed-width unit column.
- The one-page fit is robust, because every Phase 1 section has a fixed maximum row count. The fabric table has at
  most 8 rows, and the gains table 5.
- v2 also simulates four builder changes (T1-T4): drop the duplicated occupancy profile row; omit all-zero fabric
  rows with a "Not present" note; merge the two sizing notices; shorten the humidity labels and add sub-labels.

**PR1 follow-up found while building the mock-up.** `Query.OccupancySensibleGain` / `OccupancyLatentGain` return
0 W when the per-person gain is not authored, the same out-parameter defect as SAM#138. PR1 guarded lighting and
equipment but not occupancy. The fix is SAM#139, merged as `e1fbbb72` (SAM.Tests 2433/2433), and SAM#138 is updated with a
comment.

**Next step.** The owner approves v2, or asks for changes. If approved:
1. Apply T1-T4 in the PR1 section builders as a small reporting PR, and regenerate the goldens.
2. Amend the Rev 3 §10 visual specification.
3. Start PR2: the MigraDoc spike, then `SAM.Core.Reporting.Pdf`.

(Done: steps 1 and 2 as SAM#140, step 3 as SAM#141.)

## Previous: SAM Documentation Framework PR1 - reporting domain + analytical collector (2026-09-25) - MERGED as SAM#136 (`7daf0d32`)

**Status.** Implemented and validated on `feature/reporting-pr1-domain`, and merged into `sow/2026-Q3` as SAM#136
(`7daf0d32`). The plan is the owner-approved *SAM Documentation
Framework* Revision 3, where the Rev 3 section overrides the Rev 2 body. The plan file is kept outside git at
`Documents\SAM_daily\2026-09-25-Reporting\SAM Documentation Framework_rev3.md`.

Out of scope of PR1: PR2 (the MigraDoc/PDF renderer, font, spike; now SAM#141), the design-gate mock-up, and PR3
(SAM_UI integration, deploy lists).

**New projects** (netstandard2.0, output to `build/`, both added to `SAM.sln` under the SAM folder, no third-party
dependencies):
- `SAM/SAM.Core.Reporting` depends on SAM.Units and System.Text.Json only.
  - `ReportValue<T>`: a private constructor and factories only.
    - `Available` rejects null, NaN, infinity and an invalid `Quantity` with `ArgumentException`.
    - `NotAvailable` / `NotApplicable` require a reason.
    - `Availability` is derived from the factory. `Source`, `Freshness` and `SourceTimestamp` are null unless the
      value is available.
    - `WithFreshness` returns a new instance and throws on a value that is not available.
    - `Value` throws when the value is not available; `TryGetValue` does not.
    - The value-independent part is exposed through `IReportValue`.
  - `FormattedValue`: text, unit, availability, freshness, source, timestamp, note, and `IsOutOfDate`.
  - `DisplayUnit`: unit type, symbol and decimals.
  - `IQuantityFormatter` / `QuantityFormatter`:
    - a display policy per `UnitCategory` for SI and Imperial;
    - `SelectDisplayUnit(category, quantities)` gives one unit per group. Power switches from W to kW at 10 kW (SI),
      and from Btu/h to kBtu/h at 100 kBtu/h (Imperial);
    - placeholders are "—" and "n/a";
    - numbers use the culture's separators, and negative zero is suppressed;
    - dates are formatted with invariant month names.
  - `DocumentOptions`: unit system, culture, SI air flow unit (L/s by default), metadata, style, and fixed
    `GeneratedAt` / `SoftwareVersion` for tests.
  - `DocumentMetadata`, `DocumentStyle`.
  - Blocks: `KeyValueBlock` (and its rows), `TableBlock` / `TableColumn` / `TableRow`, `NoticeBlock`, `TextBlock`,
    `ImageBlock`.
  - `DocumentSection`, `DocumentFooter` (lines plus the legend of markers actually used), `Document`,
    `IDocumentRenderer`.
  - `Convert.ToJson(Document)`: deterministic JSON with a fixed property order, "\n" line endings and readable
    non-ASCII text.
- `SAM/SAM.Analytical.Reporting` depends on SAM.Analytical, SAM.Core, SAM.Core.Reporting, SAM.Units and SAM.Weather.
  - `DocumentContext` and `DocumentProvenance`.
  - `Create.DocumentContext(model, options)`.
  - `SpaceDocumentData` and its typed records: Identity, Geometry, Occupancy, three gain records (lighting,
    equipment sensible, equipment latent), Infiltration, DesignCriteria, Ventilation, Systems, Fabric (with
    `FabricAreaRow` and `FabricCategory`), Sizing (with `DesignLoadStatus`), and Provenance.
  - The collector `Create.SpaceDocumentData(context, space)`.
  - Section builders: identity, geometry, internal-condition, design-criteria, ventilation, systems, fabric,
    sizing, plus `SpaceFooterBuilder`.
  - `ISectionBuilder<T>`, `IFooterBuilder<T>`, `DocumentDefinition<T>`,
    `SpaceDocumentDefinitions.SpaceAssumptions`, `Create.Document(...)` and `Create.SpaceAssumptions(context,
    space)`.

**Phase 1 rules as implemented:**
- No `SpaceSimulationResult` and no TSD data is read.
- Design loads come from `SpaceParameter.DesignHeating/CoolingLoad`. They are reported as Available, Source = TBD,
  Freshness = Unknown, and the footer reads "Design loads: from Tas sizing — date/currency not recorded".
- `SimulationResultProvenance` is never consulted. A test stamps a current TSD provenance plus a conflicting
  simulation result, and the loads stay 779 W / Unknown.
- Expected missing data becomes NotAvailable or NotApplicable with a reason, plus a `Log` warning. A section whose
  values are all missing renders one `NoticeBlock`.
- A value that is present but invalid (infinite, negative) becomes NotAvailable("invalid value in model"), plus a
  warning.
- Builder exceptions propagate, which aborts the document (tested with an injected fault).
- Comparable values share one display unit: heating/cooling pairs, the gains "Total" column, the fabric columns and
  the ventilation air flows.

**Decisions and discoveries (for review):**
- **Humidity set points** (confirmed in the final review from the SAM_Tas mappings `UpdateInternalCondition`,
  `UpdateInternalConditionTemplate`, `ProfileReuseIndex` and `ToSAM/InternalCondition`):
  - Humidification is the TBD thermostat `ticHLL`, the zone humidity LOWER limit. Tas "No Humidification" = 0 %.
  - Dehumidification is `ticHUL`, the UPPER limit. Tas "No Dehumidification" = 100 %.
  - Neither is tied to heating or cooling. The report lists them in their own "Room humidity" block, as
    "Humidification set point (lower RH limit)" and "Dehumidification set point (upper RH limit)", not in the
    heating/cooling table.
  - A 0 % lower limit or a 100 % upper limit is reported as NotApplicable ("No humidification" / "No
    dehumidification").
  - `Query.HeatingDesignRelativeHumidity` returns the dehumidification minimum, and `CoolingDesignRelativeHumidity`
    the humidification maximum. Their names do not match what they read. The Grasshopper ReportSpaces component
    uses them consistently with the profiles, but SAM_Mollier `AirHandlingUnitResult` uses them as heating/cooling
    design RH. The reporting code does not use them; they are left unchanged and recorded as a follow-up.
- **`Profile.MinValue` defect.** `Profile.GetMinValue()` takes each child profile's MAXIMUM, so MinValue is wrong
  for a profile built from other profiles. `Query.CoolingDesignTemperature` inherits the defect. The collector
  therefore takes min/max over `Profile.GetYearlyValues()`, which expands child profiles correctly. This is covered
  by a regression test; `Profile` itself is unchanged and the defect is recorded as a follow-up.
- **Sizing multiplier.** The stored factor is a multiplier: SAM_Tas `UpdateSizingFactors` sets
  `zone.maxLoad = maxLoad × factor`.
  - The space value wins; a value of 0 means "not set" and falls back to the model value.
  - It is displayed as "Sizing multiplier 1.20", not as 120 % or +20 %: those read as a margin, and a factor
    below 1 is possible.
  - SAM_Tas applies it to the TBD loads only on the `GenerateHDDCDDFile` sizing path, so the persisted
    `DesignHeating/CoolingLoad` may or may not include it. The sizing section says so in a notice.
- **Occupancy count** is displayed with 1 decimal ("3.0 persons"), not as an integer, so fractional occupancy is not
  hidden.
- **Imperial temperature difference** uses the symbol "Δ°F", to distinguish it from absolute °F.
- **Internal gain totals.** SAM CAN distinguish "not authored" from an authored 0, because
  `InternalCondition.TryGetValue` returns false for an absent parameter.
  - `CalculatedEquipmentLatentGain`, `CalculatedEquipmentSensibleGain` and `CalculatedLightingGain` lose that
    distinction: their `TryGetValue(..., out gain_2)` overwrites the NaN default with 0, so "nothing authored"
    returns 0 W.
  - The collector reads authorship from the internal condition parameters. No gain parameter at all is
    NotAvailable ("No ... gain authored"); an authored 0 shows "0".
  - The queries are unchanged; this is recorded as a follow-up.
- **Negative values.** SAM parameter validation rejects negative area and volume at `SetValue`, so the data-defect
  path is exercised with +∞.
- **Follow-up:** all three SAM defects above (RH query names, `Profile.MinValue`, `Calculated*Gain` returning 0 when nothing is authored) are recorded in [SAM#138](https://github.com/SAM-BIM/SAM/issues/138).
- **Formatter API.** The formatter has `Format` overloads for `Quantity`, string and `DateTime`, and a
  `DisplayUnit(category)` method, instead of a generic `Format<T>` and `UnitText`.
- **`FormattedValue`** carries Availability plus Freshness, following Rev 3 R3.2, rather than the Rev 2 `Status`.

**Tests:**
- New test files:
  - `ReportValueTests`: every factory has no contradictory state; rejects NaN, null and invalid values;
    immutability.
  - `QuantityFormatterTests`: SI and IP values for every category; absolute temperature vs difference; L/s,
    m³/s and cfm; the 10 kW and 100 kBtu/h shared-unit pairs; W/m² ↔ Btu/h·ft²; placeholders; culture.
  - `SpaceAssumptionsTests`: typed collector values; fabric; design-load freshness Unknown, including against a
    current TSD provenance; the minimal model; the invalid value; the injected fault; section order; IP
    conversions; shared units. The final review added:
    - a composite-profile set point regression;
    - humidity control off → n/a;
    - room-humidity labels;
    - latent gain authored-zero vs not authored;
    - the sizing multiplier "1.20" and its notice.
- Fixtures: `Helpers/ReportingFixture.cs` (Full and Minimal models, fixed GUID, time and version) and
  `Helpers/Golden.cs`.
- Goldens in `SAM.Tests/Golden/*.json` (Full SI, Full IP, Minimal SI) are copied to the test output. To regenerate,
  set `SAM_UPDATE_GOLDEN=1`, run the tests, rebuild, and review the diff.

**Validation (2026-09-25):**
- `dotnet build SAM.sln` (Debug): 0 errors, and no warnings from the new code.
- `SAM.Tests`, rebuilt explicitly (it is not in `SAM.sln`): 2425/2425 in Debug and 2425/2425 in Release, after the
  final review. CI on the first head (`0972b837`) was green: build, test and SPDX.
- After merging `sow/2026-Q3` at `7dbeb2e4` (SAM#137): `SAM.sln` Release has 0 errors, and `SAM.Tests` passes 2432/2432 in Debug and Release (2425 + 7 TM59).
- Final review:
  - no PDF, MigraDoc or SAM_UI dependency, and no `SpaceSimulationResult` or TSD read;
  - design-load freshness is always Unknown;
  - `ReportValue<T>` has no public setters or constructor;
  - the goldens are deterministic;
  - unused public API was removed (`QuantityFormatter.SelectDisplayUnits`; `FormatNumber` is now private).

**Next step.** The owner reviews the PR1 PR; merge only on the owner's go-ahead. After that comes the §10 design
gate (an HTML/PNG mock-up from the fixture data), then PR2 (the renderer spike plus `SAM.Core.Reporting.Pdf`).
(Done: merged as SAM#136; the gate approved v2; PR2 is SAM#141.)

## Previous: SAM Documentation Framework PR0 - `SAM.Units` extension (2026-09-25) - MERGED

**Status.** MERGED into `sow/2026-Q3` as [SAM#135](https://github.com/SAM-BIM/SAM/pull/135), merge commit
`4e027f55`, head `2005ba37`, on 2026-09-25. CI was green (build, test, SPDX). The focused final review found no
material issue:
- factor directions were verified against independent reference values;
- the linear fallback only runs after the explicit switch;
- every pair that changed from NaN to a value is intended;
- no consumer switches over `UnitType` or `UnitCategory`.

No automated Codex review was posted within about 25 minutes.

**What changed (`SAM/SAM.Units`, append-only enums):**
- `UnitCategory` gains these members (appended; existing ordinals unchanged): Area, Volume, Length,
  TemperatureDifference, AirChangeRate, SpecificPower, PowerPerPerson, AreaPerPerson, Illuminance,
  ThermalTransmittance, Time, Count, Ratio and Angle. `AirFlow` stays the volumetric-flow category.
- `UnitType` gains 24 appended members, each with a unique ASCII abbreviation:
  - area and volume: m2, ft2, m3, ft3;
  - flow and power: cfm, Btu/h, kBtu/h;
  - specific power: W/m2, Btu/h.ft2;
  - temperature difference: dK, dF;
  - per person: W/person, Btu/h.person, m2/person, ft2/person;
  - other: ac/h, lx, fc, W/m2K, Btu/h.ft2.F, person, h, deg, rad.
- `Factor` gains the new named constants. The derived ones (area, volume) are computed from `MetersToFeet`.
- `Convert.ByUnitType`:
  - **Fix:** Fahrenheit→Celsius now divides by 1.8 (it divided by 18). This also fixes F→K and `ToSI(F)`.
  - **Fix:** GramPerGram→GramPerKilogram multiplies by 1000, and the reverse divides by 1000 (both were
    inverted).
  - Pairs the explicit switch does not handle fall through to a private linear-factor table (`ByLinearFactor`).
    - Each unit has a factor to its family base.
    - The family is `Query.UnitCategory(unit)`.
    - Absolute temperatures stay in the switch only.
    - Cross-category or unsupported conversions still return NaN.
- `ToSI` / `ToImperial` cover every new type, plus the existing types that previously returned NaN (W, kW, l/s, N/m2,
  g/g, J/kg, kJ/kg, kg/m3; Imperial for m3/s, m3/h, l/s, W, kW).
- `Query.UnitType(style, category)`:
  - SI results for the existing categories are unchanged; the Mollier UI depends on them.
  - New Imperial results: Temperature→F, AirFlow→cfm, Power→Btu/h.
  - Every new category resolves in both styles.
- `Query.UnitType(string)`: **Fix** in the case-insensitive fallback. It indexed a two-texts-per-unit list against
  the enum array, so "METER" parsed to Celsius. Each text now keeps its own unit, and the fallback uses
  `ToUpperInvariant`.
- `Query.UnitTypes` gains lists for the new categories, cfm and Btu/h/kBtu/h Imperial entries, and l/s under SI.
- New `Query.UnitCategory(UnitType)` returns the primary category: Unitless/Percent→Ratio, Kelvin→Temperature.
  - Because the method shadows the enum inside `Query`, `Query/UnitType.cs` and `UnitTypes.cs` now write
    `Units.UnitCategory.X`. This follows the files' existing `Units.UnitType.X` convention.
- New `Classes/Quantity.cs`:
  - `readonly struct` with `Value`, `Unit`, `Category`, `IsValid` (finite and unit defined), and
    `ConvertTo(UnitType)` (NaN when unsupported).
  - `ToString` uses the invariant culture plus the abbreviation.
  - It holds no formatting policy.
- `SAM.Units.csproj`: removed a dead `<Compile Remove="Classes\**">` group. There was no `Classes` folder, and the group
  would have silently excluded `Quantity.cs`.

**Tests.** New `SAM/SAM.Tests/UnitsTests.cs` (109 cases), plus a `SAM.Units` ProjectReference in `SAM.Tests.csproj`.
Coverage:
- ordinal locks;
- reference values per family;
- every-pair round trip;
- same-category-convertible and cross-category-NaN closure;
- °C/°F/K, including −40 and 32/0, and the `/18` regression;
- temperature difference has no offset;
- the g/g regressions;
- every abbreviation is unique and parses back, both exactly and upper/lower-case;
- the parser regression;
- `UnitCategory` is defined for every unit;
- the SI compatibility lock;
- both styles resolve for every new category;
- `Quantity`.

**Validation (2026-09-25):**
- `dotnet build SAM.sln -c Debug`: 0 errors, 208 warnings (the same count as before the change, and none in
  SAM.Units or the new tests).
- **`SAM.Tests` is not in `SAM.sln`.** Build it explicitly: `dotnet build SAM/SAM.Tests/SAM.Tests.csproj`. Otherwise
  `dotnet test --no-build` runs a stale DLL.
- `dotnet test SAM/SAM.Tests`: 2361/2361 passed.
- Behaviour baseline: the pre-change DLL was diffed against the new one over every legacy pair, ToSI/ToImperial,
  both parser paths and `UnitType(style, category)`. The only differences are the three fixes, NaN→value additions,
  and the three new Imperial results.
- SAM_UI `SAM.Core.Mollier.UI.WPF` (at SAM_UI `d123f3da`) compiled against the new `SAM\build\SAM.Units.dll`
  (Release, output redirected to a scratch folder): 0 errors.

**Compatibility.** Consumers on disk use:
- Revit: `ToSI(Feet)` and `ToDegrees`;
- Tas: `ToDegrees`;
- LadybugTools: `ToRadians`;
- Mollier UI: `UnitType(SI, …)`, `Abbreviation` and `DefaultTolerance`.

Psychrometrics and Multitasker reference the DLL without using it. None of these consumers hits a changed path.

After the merge, rebuild SAM before SAM_UI, or SAM_UI picks up stale sibling `build/` DLLs.

## Previous: Nuaire reply (A. Nash, 2026-09-24) implemented - exchanger then DX drop, 13 C floor (Stage 3)

**Status.** Implemented, closed out, PR-reviewed and MERGED into `sow/2026-Q3` on 2026-09-24, in order: SAM-BIM/SAM#133 (`54c43438`) -> SAM-BIM/SAM_Systems#29 (`c88c9b37`) -> SAM-BIM/SAM_Tas#65 (`1b659756`) -> SAM-BIM/SAM_UI#107 (`8f58144c`). CI green and Codex review clean (all findings fixed and answered) on every PR. The `feature/parto-nuaire-reply-2026-09-24` branches are deleted. Accepted on the representative MG annual run (closeout). The base is the validated provisional baseline (SAM `875655fa` + docs
`9e0933af`, SAM_Systems `df5dd332`, SAM_Tas `f7d39351`, SAM_UI `4460dc3a`). This is manufacturer
modelling guidance; Nuaire has not called it certified or approved.

**Source.** The reply `.msg` is in `C:\TasOut\nuaire-guidance\REVIEW2\manufacturer data\`. Its text and
four content images (Part F Table 1.3, a process diagram, and two presentation slides) are extracted to
`...\manufacturer data\2026-09-24-reply-extract\`, with SHA256SUMS. The pack is private, so only figures
are transcribed.

**Evidence summary (verified against the source):**
- `to - X` was Nuaire's simplified IES work-around, valid at 32 C outside with the room at 23-26 C only.
  X is the component model evaluated at that one condition.
- Nuaire's switching formula `IF(to<=19,13,IF(ta<to,0.8466ta+0.1534to-7.84,to-7.84))` is:
  - exchanger efficiency at 90 l/s: 0.8466;
  - net drop at 90 l/s: DX 8.485 K less fan reheat 0.65 K = 7.835 K;
  - a 13 C floor.
- Presentation figures at 60/80/100/120 l/s:
  - HX 87.96/85.76/83.56/81.36 %;
  - DX drop 9.265/8.745/8.225/7.705 K (sensible 0.67/0.84/0.99/1.12 kW);
  - supply-motor reheat +0.3/0.5/0.8/1.1 K.
- 13 C: Nuaire's stated low limit (the tested supply bottomed out there).
- Bypass: To >= 12, To < Tra, Tra >= 19 C, decided by the MVHR independently of cooling.
- Airflow:
  - 60 l/s minimum;
  - about 90 l/s with 220x90 flat duct; about 125 l/s with larger ducting ("from memory");
  - 3x is a worked example, not a rule.
- 2.2 kW is the brochure's largest COMBINED (coolth recovery + sensible, no latent) figure. It is not a
  DX rating.

**Implemented representation.**
- The explicit TAS exchanger does bypass or recovery.
  - Recovery fraction: 0.8 at the design airflow, eta(Q) = 0.8576 at 80 l/s.
  - One bypass rule applies at both airflows, no longer suppressed by the cooling-stat.
- The DX coil's `MinimumOffcoil` is a table over its OWN entering dry bulb (`EDB`):
  `max(13, entering - (DX drop - fan rise)(Q))`. At 80 l/s the net drop is 8.245 K. Fan heat stays
  cleared; the rise is carried in the drop.
- The 13 C floor bounds what the coil produces. A coil cannot heat, so air already below 13 C passes
  through.
- The DX duty is a numerical 100 kW (`GuidanceCoolingDuty_W`), not a rating: TAS needs a finite duty, and
  the law, not the duty, then sets the leaving temperature whenever the stat calls. No product capacity is
  read (see close-out item 2).
- The exchanger table has a 0.1 K grid up to 45 C on both axes (close-out item 3).
- Elevated airflow: the dwelling's own figure, else the stated default of 80 l/s, else the midpoint.
  The range is 60-120 l/s (the data range); outside it, refused.

**Why not the literal formula.** The component form uses the TAS exchanger actually modelled, so bypass
and recovery follow the real TAS sensors. It reproduces the formula exactly at 90 l/s (0.8466 / 7.835 K)
and is continuous. It also removes the formula's 19 < to < 20.84 C hole.

**Files changed.**
- SAM:
  - `SupplyTemperatureRuleType.ExchangerThenCoil`;
  - `SupplyTemperatureRule.ExchangerThenCoil(...)`, `SupplyTemperature(..., bool exchangerBypassed)`,
    `ExchangerExtractFraction`, `CoilNetTemperatureDrop_K`; JSON `ExtractFractions`,
    `CoilTemperatureDrops_K`, `FanTemperatureRises_K`;
  - `VentilationUnitOperatingStrategy.ExchangerBypassed`, `DefaultElevatedAirFlow_Lps`;
  - `OperatingMode`: the room-stat bypass no longer requires extract <= activation;
  - tests (section G).
- SAM_Systems:
  - the Nuaire catalogue entry: bypass extract 19, range 60-120, default 80, ExchangerThenCoil figures,
    floor 13, new provenance;
  - `Query.MechanicalVentilationGuidanceSettings` (the default airflow);
  - catalogue tests.
- SAM_Tas:
  - `GroundGuidanceCooling`: recipe, exchanger table, EDB off-coil table with floor, read-back, note;
  - `GuidanceCoolingResults`: target law, exchanger-state, floor and part-flow counts;
  - close-out: `GuidanceCoolingDuty_W` (100 kW numerical) replaces the product-capacity read; exchanger
    grid 0.1 K to 45 C;
  - recipe tests.
- SAM_UI: the `PartOIteration3GuidanceResolution` note text only.

**Validation.**
- Tests (on the branches):
  - SAM.Tests 2250/2250 (+32);
  - SAM.Analytical.Systems.Tests 256/256 (+5);
  - TM59.Tests 941/941 (+3);
  - WPF.Tests 1031/1031.
- Framework MSBuild Debug: SAM, SAM_Systems, SAM_Tas and SAM_UI all 0 errors.
- Native probe (Stage 12, scratch, `C:\TasOut\nrev12\`, harness mode `s12`, analysis `analyse12.py`), on a
  copy of the Stage 11 prototype:
  - TAS accepts the EDB table on `MinimumOffcoil`;
  - full year: 182/182 full-flow cooling hours exact; exchanger state 100% in elevated hours;
    0 hours at 2200 W; 0 of 57 modulating hours below the law;
  - forced 18 C stat: 302 hours held at 13.0 C; every below-13 hour had the coil entering already
    below 13 C.
- Representative MG (`C:\TasOut\nuaire-reply-2026-09-24\02-Iteration3-MG\`; same model a7e09a25,
  DSY1 2050s, real UI via `scripts\stage.ps1`):
  - COMPLETE: 1a in 1.3 min, then Iteration 3 in 7.8 min; A Fail / B Fail.
  - Bias 0.53 K, RMSE 1.412 K (was 0.567 / 1.477). Reference A TM59 is identical to the accepted run.
  - Candidate B >26 C hours rise, because the supply is no longer over-cooled: Studio 320 -> 342,
    Bathroom_2 260 -> 355 (now FAIL), bedrooms 122/120 -> 156/153, ensuites now FAIL (273/286).
- MG read-back, per unit (`-It3BMG-OperatingAirFlow.csv`; independent check `analyse_mg.py`):
  - MVHR-01 30 -> 80 l/s, MVHR-02/03 63 -> 80 l/s; supply = extract within 0.014 l/s.
  - The supply law is exact in 1033/1033, 970/970 and 964/964 full-flow cooling hours (max error 0.0001 K).
  - 0 hours at the duty bound; 0 hours cooled below 13 C by the coil.
  - 5 h (MVHR-01) had the coil entering below 13 C (bypass at 12-13 C intake): minimum 12.3 C while the
    stat calls.
  - Exchanger state correct in 1028/1033, 964/970 and 958/964 elevated hours. The misses are all at 37-40 C
    intake, where the exchanger table's breakpoints jump 35 -> 38 -> 45 C (same grid as the accepted
    baseline); less coolth recovery there, by up to 0.8 K.
  - 3-7 h per unit of 4-14 W "DX without signal" are proportional-band slivers with the room at 21.95 C.
- Review-only reopen of the new MG pairing: reproduced, with only a TSD reader and no simulation.
- The first MG summary said "0 h modulating". That was a bug in the new summary code (the modulating count
  sat under the wrong `if`); it is fixed in the close-out.

**Close-out review (2026-09-24, after the first MG run).**
1. **Five cooling hours at 12.3-12.9 C (MVHR-01).**
   - Each hour: part flow (33-44 l/s), room 21.95-21.97 C at the stat band edge, exchanger in BYPASS by
     Nuaire's own rule (intake 12.3-12.9 C > 12 C), DX 0 W.
   - The unit delivered bypassed outdoor air, as it does in 1388 background hours (minimum 12.1 C).
   - Conclusion: Nuaire's 13 C is the DX-cooled supply floor ("supply temperature bottomed out at 13" in
     cooling tests; "where we set the new low limit"). It is not an absolute final-supply minimum. The
     formula's `to<=19 -> 13` branch is a simplification: it cannot hold 13 C at 12-13 C intake in bypass
     without heating, which neither the product nor TAS provides.
   - No model change.
2. **2200 W ceiling removed.**
   - It was not inert. A controlled TAS coil delivers signal x duty until the off-coil law stops it, so the
     2.2 kW value acted as a controller gain: in part-signal hours the DX delivered only part of its drop.
   - Evidence: Stage 13 July-August, 424 of 794 part-flow hours short of the law. On the first MG run the
     part-flow law held in only 377/577, 45/348 and 45/351 hours.
   - Nuaire's cooling is on/off at the room stat, and 2.2 kW is a combined figure, so the duty is now a
     numerical 100 kW and the recipe no longer reads the product's capacity.
3. **Exchanger grid refined.**
   - The axes are 0.1 K from the bypass thresholds up to 45 C on both axes (334x276x2 = 184k cells,
     about 17 s per unit to write).
   - Stage 13 on a copy of the MG TPD, July-August (harness mode `s13`, `C:\TasOut\nrev12\s13_*`): the base
     reproduces exactly the 17 misses (6/6/5); the refined grid has 0 misses (worst 0.021 K).
   - The refined grid alone changes 24 hours and no airflows; removing the cap alone changes 404 airflow
     hours.
- **Close-out MG** (`C:\TasOut\nuaire-reply-2026-09-24\03-Iteration3-MG-closeout\`):
  - COMPLETE, Iteration 3 in 10.6 min; A Fail / B Fail; bias 0.53 K, RMSE 1.413 K.
  - TM59 differs from the first run only in Bathroom_2 355 -> 353 and Ensuite_5 273 -> 274. DX energy is
    +0.5-1 %. The annual effect is immaterial.
  - Read-back:
    - law exact in 1033/1033, 970/970 and 964/964 full-flow hours;
    - exchanger state exact in 1033/1033, 970/970 and 964/964;
    - part-flow law 482/552 for MVHR-01 (was 377/577);
    - 0 hours cooled below 13 C by the coil; 6 bypass hours below 13 C (coil entering already below);
    - supply = extract within 0.03 l/s.
  - Known representation artefact, in BOTH runs: hours where the room hovers at the stat band edge show
    hour-average airflow between design and 80 l/s with the DX partly on (MVHR-01: 447 DX hours at an
    average below 60 l/s; MVHR-02/03: about 220 DX hours near design airflow). This is the hourly average
    of on/off cycling inside a 0.1 K proportional band, not DX running continuously at low airflow.
  - Tests after the close-out: TM59 942/942, SAM_Systems 256/256, WPF 1031/1031, SAM 2250/2250
    (unchanged). All builds 0 errors.
- **PR review fixes (Codex review on the four PRs, after the close-out MG).**
  - Bypass minimums are now INCLUSIVE, as Nuaire states them (to >= 12, extract >= 19, extract > to strict), in
    SAM `ExchangerBypassed`, the TAS recipe and the read-back. The TAS table carries each minimum on the grid
    with a 0.01 K step just below it.
  - The TAS extract axis continues coarsely beyond 45 C (50/60/80/100), so a hot extract keeps the exact state
    for any intake below 45 C. An intake above 45 C is held at the edge and documented; the DSY1 2050s peak is
    40.3 C.
  - The read-back summary formats a rule without a minimum as unfloored.
  - Each of SAM_Systems, SAM_Tas and SAM_UI now has its own `PROJECT_PROGRESS.md` entry.
  - Tests: SAM 2252, SAM_Systems 256, TM59 944, WPF 1031.
  - By owner decision there was no further annual MG. The inclusive threshold affects only hours at exactly
    12.0 C intake or 19.0 C extract (159 unit-hours at 12.0 C in the MG weather, background hours, so TM59
    >26 C counts are unaffected). No MG extract exceeded 45 C (max 37.8 C).
- B0 was NOT rerun. No code reachable from the B0/B4 routes changed: only the guidance grounding, the
  room-stat strategy and the MG note.

**Install state on this machine.** `Documents\SAM\resources\...\VentilationUnitCatalogue.JSON` is now the
feature-branch catalogue: a SAM_Systems `dotnet test` build copies resources there. The merged v3 copy is at
`C:\TasOut\nuaire-reply-2026-09-24\catalogue-backup\v3-sow-2026-Q3-df5dd332.json`. `SAM_UI\build` holds
feature-branch binaries.

**Residual uncertainties (manufacturer).**
- The exact 13 C semantics near 19-21 C.
- Recovery fraction at background airflows at or above 60 l/s (MVHR-02/03 run at 63 l/s but keep 0.8).
- The 125 l/s upper figure (from memory).
- No DX total-duty rating is published, so the DX has no manufacturer capacity limit in the model.
- The flat-duct 90 l/s limit is a project ducting fact, not enforced; the refusal is at 120.
- The DX and fan share one proportional stat (0.1 K band). Truly simultaneous on/off switching of cooling
  and speed 3 is approximated only in hourly averages.

**Exact next step.** Bump the SAM_Deploy pointers to the four merge commits above (and push the older unpushed
`chore/bump-parto-nuaire-merged-pointers` branch or supersede it). The installed `Documents\SAM\resources` catalogue is
already the merged one. Remaining manufacturer questions are listed under *Residual uncertainties*.

## Previous: SAM#123 manufacturer guidance - Iteration 3 mode "Selected product - manufacturer guidance" (2026-09-24)

**Merged-build acceptance (2026-09-24, later): PASSED. Technically ready to freeze, pending manufacturer
confirmation.** This is not manufacturer approval or certification. The owner has adopted this
acceptance as the validated PROVISIONAL Nuaire baseline. New manufacturer guidance has since been received.
It will be reconciled against this baseline in a separate, fresh session, so this is not final
manufacturer-approved behaviour. The acceptance was run on clean checkouts of merged `sow/2026-Q3`:
- SAM `875655fa`
- SAM_Systems `df5dd332`
- SAM_Tas `f7d39351`
- SAM_UI `4460dc3a`

No code was changed. Evidence is in `C:\TasOut\merged-acceptance-2026-09-24\`, outside git.
- **Build:** Framework MSBuild (VS 18), Debug, Restore+Rebuild, as in `BuildAlls_v4`. All four solutions
  have 0 errors.
- **Tests:** SAM.Tests 2218/2218, SAM.Analytical.Systems.Tests 251/251, SAM.Analytical.Tas.TM59.Tests
  938/938, SAM.Analytical.UI.WPF.Tests 1031/1031. All four equal the baselines.
- **Deployment:** the builds' PostBuild steps redeployed `%APPDATA%\SAM`. The key binaries there are
  byte-identical to the fresh `build/` output: SAM.Core, SAM.Analytical, SAM.Analytical.Systems,
  SAM.Analytical.Tas, TM59, SAM.Analytical.UI.WPF and `SAM Analytical.exe`/`.dll`. No feature-branch
  binary is left. The only older own-repo files are `SAM.Core.Rhino.dll` (not a GH project, so never
  copied) and `SAM.Analytical.Tas.GenOpt.dll` (deployed by SAM_Tas_Grasshopper). Both are 2026-09-23 `sow`
  builds. `Documents\SAM\resources\...\VentilationUnitCatalogue.JSON` is byte-equal to the merged
  SAM_Systems v3 catalogue. The v1 catalogue was not restored.
- **MG review-only reopen** (`02-Iteration3-MG`):
  - Review COMPLETE in 14.7 s: A Fail / B Fail, bias 0.567 K, RMSE 1.477 K, max 4.121 K.
  - No behaviour picker appeared, and no simulation ran. Only a TSD reader opened.
  - The result window text is identical to the pre-merge reopen.
  - The Review rewrites its TM59 and Review reports. They equal `03-Resume`'s apart from the path and
    "review/run" wording.
- **1a reopen / Iteration 3 readiness** (`03-Resume`):
  - The hub restores the run ("Reopened from this model's own saved run"). Iteration 3 is enabled, as
    "Review It. 3" because that folder holds the MG pairing.
  - The Run path was checked by temporarily renaming only `-Iteration3.json`. Iteration 3 then read
    "Iteration 3 (A/B)", and pressing it opened the behaviour picker (4 modes) directly, with no Prepare &
    Run. The picker was cancelled, so nothing ran.
  - The record was restored hash-equal, and the folder snapshot (names, sizes, write times) is unchanged.
  - Note: a copied 1a cannot be used for this check. The saved `.sam` embeds its absolute output path, so
    a copy restores the original folder's run. This is by design.
- **B0 fresh rerun** (`B0-rerun`, source model a7e09a25):
  - COMPLETE: 1a in 1.3 min, then Iteration 3 in 3.0 min; A Fail / B Fail, bias 0.05 K, RMSE 0.736 K.
  - Both TM59 reports equal `01-Iteration3-B0` apart from the `Source:` line.
  - The Review differs only in per-run IDs, timestamps and the Reference A design fingerprint. That
    fingerprint also differs between each earlier fresh 1a run.
  - Two earlier attempts were aborted by the UI driver's own guard before any simulation. The
    output-directory field read back with stray leading characters, consistent with live keyboard input
    reaching the focused window. The driver copy in `merged-acceptance-2026-09-24\scripts\stage.ps1` now
    re-applies and verifies that field.
- **SAM_Deploy:** the pointers were behind (SAM `86213eb3`, SAM_Systems `0e891145`, SAM_Tas `c267f52f`,
  SAM_UI `5606a82d`). A minimal fast-forward bump of just these four is committed locally on SAM_Deploy
  branch `chore/bump-parto-nuaire-merged-pointers` (`3dd413c`). It is not pushed yet. No other submodule
  had drifted.
- **Not touched:** DisplacementVent ([SAM#129](https://github.com/SAM-BIM/SAM/issues/129)) and the
  hardening ([SAM#130](https://github.com/SAM-BIM/SAM/issues/130)). Neither blocked the merged workflow.

**Final integration review (2026-09-24, before merge; the merges followed).** One consolidated review of all four branches
against `sow/2026-Q3`; no blockers, no code changed at review.
- Each branch merges into current `sow/2026-Q3` without conflicts. The diffs are limited to the #123 guidance
  scope. The new public surface is additive, and a template, settings or route without guidance serialises
  and materialises as before.
- Local tests on the branch heads: SAM.Tests 2218/2218, SAM.Analytical.Systems.Tests 251/251,
  SAM.Analytical.Tas.TM59.Tests 938/938 (7 guidance-cooling), SAM.Analytical.UI.WPF.Tests 1031/1031.
  SAM#125 and SAM_Systems#25 CI green.
- Evidence re-checked on disk:
  - B0 TM59 reports equal the 2026-09-23 ones except the `Source:` path line.
  - The Resume MG TM59 report equals the in-session MG one except the path line. The hourly comparison agrees
    to 3 d.p. (bias 0.5672 vs 0.5674 K). It is not bit-identical, because Resume ran a fresh 1a.
- Non-blocking findings (JSON edge cases, read-back strictness, stale comments, test gaps) are in
  [SAM#130](https://github.com/SAM-BIM/SAM/issues/130). They were deliberately not fixed, so that the merged code is
  the accepted code.
- The DisplacementVent wet-room issue is [SAM#129](https://github.com/SAM-BIM/SAM/issues/129). It is not changed here.
- Wording: "certified" appears only in negations; values stay PROVISIONAL pending Nuaire.

**Status.** MERGED into `sow/2026-Q3` on 2026-09-24, in dependency order:
- [SAM#125](https://github.com/SAM-BIM/SAM/pull/125) -> `1f9a5b95`
- [SAM_Systems#25](https://github.com/SAM-BIM/SAM_Systems/pull/25) -> `6c042609`
- [SAM_Tas#63](https://github.com/SAM-BIM/SAM_Tas/pull/63) -> `83653aaa`
- [SAM_UI#105](https://github.com/SAM-BIM/SAM_UI/pull/105) -> `620aa701`

All four PRs had green CI. The `feature/parto-nuaire-manufacturer-guidance` branches are deleted. All product
values are still PROVISIONAL Nuaire guidance: Nuaire was emailed and has not yet confirmed.
[SAM#123](https://github.com/SAM-BIM/SAM/issues/123) stays open.

**Where it came from.**
- The Stage 11 TAS prototype passed. Its evidence is under
  `C:\TasOut\nuaire-guidance\REVIEW2\resume-2026-09-23\evidence\stage11\`.
- Stage 11b, the production correction, was found on the real model:
  - Controlled dampers do not converge when a supplied room's only outlet is a transfer damper
    (MVHR-02/03: more than 20 min for one day, against 3 s with the controllers pinned).
  - The elevated airflow is now carried on the fans, with the dampers uncontrolled at their elevated share.
    Room-stat controllers drive the supply and extract fans with Min = (design/elevated)^2, because a
    controlled fan's airflow goes as the square root of its signal.

**Per repo.**
- **SAM:**
  - `SupplyTemperatureRule.IntakeOffset`: `to - X(Q)`, Refuse outside the stated airflows, with reported
    domain use.
  - `CoolingActivationSignal` (room / extract).
  - A room-stat bypass keeps the rule's extract <= activation condition.
- **SAM_Systems:**
  - The Nuaire entry uses IntakeOffset 70/80/90/100/110 -> 15/14.5/14/13.5/13 K. There is no 16 C floor
    (Nuaire, 13 Aug 2025), activation is on the room stat, and the entry is marked PROVISIONAL.
  - `GuidanceSettings` materialisation: MVRE plus a supply DX coil after the exchanger.
  - `MechanicalVentilationGuidanceCooling` is the record the TAS grounding reads.
  - `Query.MechanicalVentilationGuidanceSettings`: the elevated airflow is the midpoint of the stated range
    (80 l/s), refused above unit capacity.
- **SAM_Tas:**
  - `Modify.GroundGuidanceCooling` writes and reads back:
    - DX room-stat controller: Studio/Bedroom zone, `SensorArc1`, 22.05 / 0.1 K;
    - fan controllers;
    - uncontrolled exchanger with an (ODB, EDB2, EFlow) state table;
    - DX: finite 2200 W (the table's max combined capacity), no gates, `MinimumOffcoil = to - 14.5`.
  - A disagreement refuses the grounding.
  - `Modify.GuidanceCoolingResults` gives the hourly read-back.
- **SAM_UI:**
  - New mode `SelectedProductManufacturerGuidance`, with `-It3BMG` files, a resolution step with no product
    constants, and the operation CSV and summaries persisted in the record.
  - Review-consistency branch for the mode.
  - Separately: Iteration 3 can start from a completed 1a reopened in a later session.
    `<project>.prepared.sam` and `<project>.partorun.json` are bound to the TSD, so there is no need to
    re-run Prepare & Run.

**Acceptance (2026-09-24, real `SAM Analytical.exe`, `SAM_zoningAM-CIBSEfutureZ1.sam` a7e09a25, DSY1 2050s).**
Evidence is in `C:\TasOut\parto-guidance-2026-09-24\`, outside git.
- **B0 on the new binaries:** COMPLETE. Both TM59 reports are byte-identical to 2026-09-23; bias 0.05 K,
  RMSE 0.736 K. So B0 is unchanged.
- **MG:** COMPLETE in 11.7 min, A Fail / B Fail. TM59 >26 C hours against A:
  - Studio -68 and bedrooms -101 / -96, where B0 is -6 / +8 / +12;
  - kitchens -38;
  - wet rooms -284 to -471 (Bathroom_2 becomes a Pass).
  - Annual mean bias +0.57 K, RMSE 1.48 K, from heat recovery outside cooling. The DisplacementVent-inflated
    extract (3.5-6 kh extract > 22 C while room <= 22 C) suppresses bypass. It is a model-hygiene decision
    and not changed here.
- **MG TAS read-back, per unit (design -> elevated):**
  - MVHR-01: 30 -> 80 l/s, supply = extract (< 0.02 l/s). `to - 14.5` is exact in 849/849 full-flow cooling
    hours that are not capacity-limited. 122 h are at the 2.2 kW total-duty bound.
  - MVHR-02/03: 63 -> 80 l/s. The law holds in 699/700 and 692/693 such hours; 85 h are capacity-limited.
  - All units: minimum supply 7.3 C; the stat room peaks at 36.9-39.3 C in a 40 C intake heatwave, with the
    supply at exactly `to - 14.5`.
- **Reopen:** a new session opened the saved 1a run and reviewed the MG pairing in 10.8 s with no simulation.
- **Resume:** a fresh 1a wrote its sidecar. A new session then opened that saved 1a run and ran Iteration 3
  (MG) with no Prepare & Run, COMPLETE in 14.0 min. The comparison matches the in-session MG run to 3 d.p.
  (bias 0.567 K, RMSE 1.477 K, max 4.121 K). Evidence: `C:\TasOut\parto-guidance-2026-09-24\03-Resume\`.
- **Installed state on this machine** (updated at the merged-build acceptance):
  - `Documents\SAM\resources\...\VentilationUnitCatalogue.JSON` is the v3 catalogue, which is also merged
    `sow`'s. The v1 backup is at `C:\TasOut\parto-guidance-2026-09-24\catalogue-backup\documents-SAM-before.json`.
    Restore it only to test pre-merge binaries.
  - `%APPDATA%\SAM` was redeployed from the merged `sow/2026-Q3` builds on 2026-09-24 15:21-15:23.

**Exact next step.** The merged-build acceptance (step 1 of the previous plan) is done; see the top of this
entry.
1. Push SAM_Deploy `chore/bump-parto-nuaire-merged-pointers` and open its PR into `sow/2026-Q3`. After it
   merges, delete the branch.
2. In a fresh session, reconcile the newly received manufacturer guidance against this PROVISIONAL baseline.
   Hold any "certified" or "approved" wording until then (stat location, X vs airflow, low-ambient
   behaviour, 30 l/s).
3. Decide the DisplacementVent wet-room issue ([SAM#129](https://github.com/SAM-BIM/SAM/issues/129)) and the
   non-blocking hardening ([SAM#130](https://github.com/SAM-BIM/SAM/issues/130)) separately.

## Previous: SAM#125 revision - intake-offset cooling rule and room cooling-stat signal (2026-09-24)

**Why.** Stage 9-11 of the TAS-native Nuaire investigation changed two assumptions:
- Nuaire's current cooling rule is `to - X(Q)`, with no floor, per the 13 Aug 2025 email. The old vocabulary
  could not express it.
- Cooling is switched by a wall-mounted **room** cooling-stat, while bypass/recovery use the unit's extract
  sensor.

The Stage 11 TAS prototype passed and proves the whole strategy natively. Its report is at
`C:\TasOut\nuaire-guidance\REVIEW2\resume-2026-09-23\evidence\stage11\STAGE11-REPORT.md`, and the summary is on
`docs/parto-regression-run-2026-09-23`. All values are provisional manufacturer guidance until Nuaire confirms.

**What changed (on this branch).**
- `SupplyTemperatureRuleType.IntakeOffset` and `SupplyTemperatureRule.IntakeOffset(airflows, offsets, min = NaN,
  policy = Refuse)`:
  - `SupplyTemperature = intake - X(Q)`, with X linear between the stated airflows.
  - Outside them the policy decides: `Refuse` (the default) gives NaN, `ClampToDomain` holds the edge value.
  - `AirFlowDomainCondition(Q)` reports out-of-domain use whatever the policy.
  - `IntakeOffset_K(Q)` exposes X.
  - JSON keys `AirFlowRates_Lps` / `IntakeOffsets_K`.
- `Enums.CoolingActivationSignal { Undefined, ExtractTemperature, RoomTemperature }` and
  `VentilationUnitOperatingStrategy.CoolingActivationSignal`:
  - The default is `ExtractTemperature`, so existing behaviour is unchanged.
  - New `OperatingMode(intake, extract, room)` and a `SupplyTemperature(..., room, ...)` overload.
  - A room-stat strategy called without a room temperature selects `Undefined`; it is never quietly evaluated
    on the extract.
  - JSON: an absent signal reads as extract; an unknown name refuses.
- Tests: 12 new cases in `SAM.Tests/PartOVentilationUnitOperatingStrategyTests.cs` (section F).

**Validation.** `dotnet test SAM/SAM.Tests/SAM.Tests.csproj -c Release`: 2218 passed, 0 failed.
`dotnet build SAM.sln -c Release`: 0 errors.

**Not done here (intentionally).** The `PerformanceTable` default stays `ClampToDomain`, because the B4 path
uses it. The `EnteringDryBulbTemperature` axis name is unchanged.

**Next step.** SAM_Systems#25: set the Nuaire entry's cooling rule to `IntakeOffset` 70/80/90/100/110 ->
15/14.5/14/13.5/13 K, with no floor, room-stat activation, and the source citing 13 Aug 2025. Then SAM_Tas
grounding, then the SAM_UI mode.

## Previous: SAM#123 - manufacturer operating-strategy vocabulary (Nuaire guidance track, 2026-09-18)

**What this entry is.** Direct manufacturer modelling guidance for the Nuaire `MRXBOXAB-ECO5-AECV` /
`MR-ECO-COOL-V` hybrid cooling unit was received on 2026-09-18 and supersedes
[#123](https://github.com/SAM-BIM/SAM/issues/123)'s recorded position that manufacturer bypass thresholds
are unavailable **for this product**. This entry adds the generic, product-neutral vocabulary that
manufacturer control guidance is expressed in. It is additive: nothing existing changed behaviour.

**The source pack is private.** The manufacturer's documents carry redistribution restrictions. They are
held locally with a hashed manifest outside every repository, and are not committed, attached or quoted.
What is committed is the generic vocabulary and, separately in SAM_Systems, one catalogue entry's
transcribed figures with a provenance string.

**New in `SAM.Analytical`:**

- `VentilationUnitOperatingMode` (enum) - `HeatCoolthRecovery` / `SummerBypass` / `Cooling`, one operating
  state per hour. Not `PartOVentilationMode`, which says how a dwelling is assessed.
- `SupplyTemperatureRuleType` + `SupplyTemperatureRule` - how a manufacturer states the **package** supply
  temperature in one mode: intake air, a linear blend of extract and intake, or the product's own published
  table, with an optional stated lower limit and an explicit domain policy.
- `VentilationUnitOperatingStrategy` - the mode thresholds, the three rules, the cooling activation
  temperature and its permitted range, and the elevated cooling airflow and its stated range.
- `VentilationUnitTemplate.OperatingStrategy` - optional; a template without one reads exactly as before and
  serialises exactly as before.

**Decisions.**

1. **A blend fraction is manufacturer modelling guidance, never certified E1.** `SupplyTemperatureRule`
   carries it; `HeatRecoveryPerformance` is untouched and still required by
   `Query.VentilationUnitOperatingParameters`. A test pins that a template carrying a strategy carries no
   certified heat-recovery or fan performance because of it. E1/E2 remain open at #123.
2. **The control law is one ordered decision, not three independent conditions.** Manufacturers publish
   these as separate formulae that can leave a temperature exactly on a threshold in no mode at all
   (a source writing `to > 12` and `to < 12` leaves 12 itself unassigned). Cooling is tested first, on the
   extract temperature alone; then bypass, requiring all of its conditions; then recovery as the default.
   Every comparison is strict, and a sweep test proves no temperature pair is left without a mode.
3. **The setpoint is data, and so is the elevated airflow - but they are resolved in different places.** A
   catalogue states a manufacturer's thresholds, rules and ranges, including the default activation
   temperature; how far up the elevated range one dwelling goes is that dwelling's decision.
   `TemplateRefusal()` is the catalogue's question and does not require it; `Refusal()` is the operating
   question and does. Both refuse rather than clamp.
4. **The package supply temperature is the quantity.** Every rule produces the air leaving the whole unit -
   not an exchanger effectiveness, not an off-coil condition, and not a figure to be corrected for fan heat
   afterwards. It is the quantity that can be observed downstream of a unit in a simulation, which is what
   an implementation is held to.

**Files changed.** `SAM/SAM.Analytical/Enums/VentilationUnitOperatingMode.cs` (new),
`SAM/SAM.Analytical/Enums/SupplyTemperatureRuleType.cs` (new),
`SAM/SAM.Analytical/Classes/System/SupplyTemperatureRule.cs` (new),
`SAM/SAM.Analytical/Classes/System/VentilationUnitOperatingStrategy.cs` (new),
`SAM/SAM.Analytical/Classes/System/VentilationUnitTemplate.cs` (one optional property),
`SAM/SAM.Tests/PartOVentilationUnitOperatingStrategyTests.cs` (new, 49 tests).

**Validation.** `SAM.Tests` Release: **2203 passed, 0 failed, 0 skipped** - 2154 before this work plus the
49 new ones, which pass on their own under a name filter as well. Release build of `SAM.Analytical` clean.
`git diff --check` clean.

**Unresolved / next.** The vocabulary is dormant here exactly as `PerformanceTable` and
`FlowFractionByControlTemperature` were: nothing in SAM reads it. The catalogue entry, the materialised
strategy and the native grounding continue in SAM_Systems, SAM_Tas and SAM_UI under #123. Certified E1/E2
and the EDSL displacement + active-HR question are unchanged and still external.

## Also current (maintenance, sow/2026-Q3): .NET Framework leftover cleanup across the repo family - MERGED, closeout in progress (2026-09-22)

**Status.** SAM#126 merged first (`6e645cab`), then all 17 sibling PRs below (merge commits), their branches
deleted remote + local. Closeout branch `build/netfx-cleanup-closeout` in 20 repos (SAM, the 17 sibling
repos, SAM_OpenStudio and SAM_UI) carries the remaining fixes and a `PROJECT_PROGRESS.md` update per repo:
- SAM_OpenStudio: deleted the missed `Grasshopper/SAM.Analytical.Grasshopper.OpenStudio/app.config`.
- SAM_UI (13 projects) and SAM_Solver (`Rhino/SAM.Analytical.Rhino.Solver`): dropped bare `<Reference>`s to
  `System.Data.DataSetExtensions`, `Microsoft.CSharp`, `System.IO.Compression` and `PresentationFramework.Aero2`.
  All come from the .NET 8 shared framework and were the only source of MSB3243 warnings in the full BuildAlls
  (30 -> 15 after the first two, the remaining 15 were the last two).

**What.** Branch `build/remove-netfx-appconfig-leftovers` in 18 repos deletes net472-era `app.config` files
(binding redirects, `<supportedRuntime>`, `loadFromRemoteSources`) from projects that all target
`netstandard2.0`/`net8.0(-windows)`, and removes bare `System.IO.Compression` references (SAM, SAM_Revit).
Base repo: [SAM#126](https://github.com/SAM-BIM/SAM/pull/126) (`73ff9d49`, pushed earlier without a PR; opened
2026-09-22). Siblings: SAM_Acoustic#7, SAM_AssemblyResolver#7, SAM_BHoM#7, SAM_Excel#5, SAM_GEM#7, SAM_IFC#6,
SAM_IoT#7, SAM_LadybugTools#9, SAM_Multitasker#6, SAM_Origin#6, SAM_Revit#18, SAM_SolarCalculator#26,
SAM_Solver#12, SAM_Systems#26, SAM_Tas_Grasshopper#5, SAM_Template#7, SAM_gbXML#7 - all into `sow/2026-Q3`.

**Validation.** All PRs MERGEABLE/CLEAN, CI green where it exists (SAM_Origin has no workflows; SAM_Acoustic
spdx only; SAM_Solver build only). Deleted lines scanned: only binding/runtime config, no appSettings; no
`ConfigurationManager` use. Full `BuildAlls_v4.bat` (Debug RestoreCleanRebuild, all repos on the PR branches,
emptied `%APPDATA%\SAM` first): exit 0, 0 errors, 7m22s. `%APPDATA%\SAM` repopulated (50 .gha) with no
`SAM.*.dll.config`; the only `.dll.config` left are SAM_UI's 4 WinExe apps (intentional).

**Local incident (resolved).** In all 18 local clones the checked-out branch ref had been deleted, so Git
showed an unborn branch with every file staged ("2.9k changed files" in GitHub Desktop). Index matched the
pushed PR head exactly; refs were recreated with `git update-ref` - no content changed.

**Decision.** Merge SAM#126 first (SAM builds first; its stale `SAM.*.dll.config` otherwise ride along into every
repo's `build/` and `%APPDATA%\SAM`), then the 17 siblings in any order.

**SAM#125 conflict.** The progress entry added by SAM#126 made open SAM#125
(`feature/parto-nuaire-manufacturer-guidance`) conflict in `PROJECT_PROGRESS.md` only; it is resolved by merging
`sow/2026-Q3` into that branch and keeping both entries.

**Stale branches.** Deleted on origin: the 15 old merged SAM branches (tips verified equal to their merged PR heads,
no open PR). **Kept by decision:** everything in SAM_Deploy (release repo, incl. `release/2026-Q3-freeze`). Still to
delete (user-approved, blocked for the agent by a permission check): 18 in SAM_UI, 9 in SAM_Tas, 5 in SAM_Systems,
`chore/drop-rhino-pre-8` in SAM_AssemblyResolver/SAM_IoT/SAM_SQL/SAM_Template, and
`feature/solar-control-context-visibility` in SAM_SolarCalculator.

**Follow-ups (2026-09-23).** SAM_gbXML's two stray legacy `v4.6.1` project folders were removed (SAM_gbXML#9,
merged). SAM_Deploy#44 then bumped every submodule to its `sow/2026-Q3` tip, and test installer run 215 passed
(SAMVersion `2026.3.215.0`, H12 audit clean; not published, not an accepted candidate - run 214 still is).

**SAM package cleanup (2026-09-23, branch `build/drop-redundant-packages`).** Removed redundant
`<PackageReference>`s:
- `System.ValueTuple` 4.6.2 from 9 projects. It is in-box on netstandard2.0 and net8.0.
- `System.Data.DataSetExtensions` 4.5.0 from 9 netstandard2.0 projects. None of its APIs (`AsEnumerable()`,
  `Field<>`, `CopyToDataTable`) are used.

No other repo references either DLL from `SAM\build`. A fresh SAM build no longer emits them; a stale
2018-dated `System.Data.DataSetExtensions.dll` left in `build/` by older builds was deleted locally. Full
`BuildAlls_v4.bat` without them: exit 0, 0 errors, 0 MSB3243, and warnings unchanged against the previous
baseline apart from 2 transient MSB3061. **Kept on purpose:**
- `Microsoft.CSharp` and `System.Runtime.CompilerServices.Unsafe`: 31 downstream repos HintPath 274 / 17
  references to their copies in `SAM\build`.
- `System.Drawing.Common` 7.0.0: a version pin, not a cleanup.

**Tried and rejected:** `<Nullable>annotations</Nullable>` on SAM.Core / SAM.Analytical /
SAM.Analytical.Grasshopper. It removes 101 CS8632 in SAM, but it makes SAM's public API advertise
non-nullable parameters. `SAM_UI/WPF/SAM.Analytical.UI.WPF` (nullable-enabled) then gains 131 CS8604/CS8625
warnings. That is an API-contract change, so it needs a deliberate decision rather than a cleanup.

**Next step.** Merge the `build/netfx-cleanup-closeout` PRs (full BuildAlls on those branches: exit 0, 0 errors, 0 MSB3243), delete their
branches, then delete the remaining approved stale branches listed above.

## Previous: SAM#111 final closeout - B4 close/reopen Review 7/7, manufacturer ventilation moved to a
successor tracker, #111 CLOSED (2026-09-16)

**What this entry is.** The last remaining SAM#111 acceptance gate - reopening the persisted B4
(selected-product cooling) pairing in a fresh process - has now passed. Combined with a human programme
decision, this closes out SAM#111.

**B4 close/reopen Review acceptance - PASS 7/7.** A fresh SAM process reopened the preserved real-project
B4 pairing (Reference A vs Candidate B `-It3B4`, `SelectedProductCooling`, 3 cooling modules, 3 air systems,
8 assessed rooms, 14 ventilation legs) on the current merged binaries:

1. the pairing was recovered (not rerun) - `IsRestored = true`, Review reported the reopened persisted
   pairing, no behaviour-mode picker appeared;
2. engineering results reproduced exactly - Reference A FAIL, Candidate B FAIL, same comparison statistics,
   same TM59 outcomes, deep comparison of `Record`/`Comparison` produced zero engineering violations;
3. the ledger survived - 15/15 stages present and `COMPLETED`;
4. no refusal;
5. no TAS simulation during Review - only TSD result-reader processes; no new TBD/TPD/TAS3D/TasConv
   process; the pre-existing TAS-process baseline was empty;
6. catalogue provenance revalidated - schema `VentilationUnitCatalogue:v1`, expected SHA matched disk.

Review completed in seconds rather than rerunning the ~20-minute annual simulation. No production code was
changed to obtain this result.

**A separate, non-blocking evidence-archive finding from the same reopen: [#122](https://github.com/SAM-BIM/SAM/issues/122).**
The in-session Review that produced the B4 pairing recorded 52 run-time `Notes`; the persisted record does
not store them, so a reopen reconstructs only 1 and overwrites the standard `Iteration3-Review.txt/.json`
archive with the thinner reconstruction (original review text ~37,811 chars, restored ~26,377 chars) -
calculations, pairing identity, ledger, TM59 and comparison results are unaffected. This did **not** fail
the reopen acceptance gate; it is tracked separately as an evidence/history-preservation defect only.

**Human programme decision.** The proven Iteration 3 explicit Systems/TPD route, including the accepted
selected-product cooling/B4 route, is complete. The remaining manufacturer **ventilation** behaviour -
certified heat recovery, fan SFP/power/heat, B1/B2, and manufacturer bypass/B3 - is moved to
[#123](https://github.com/SAM-BIM/SAM/issues/123), a separate tracker, because its acceptance is blocked on
external manufacturer/EDSL evidence (PCDB product-variant equivalence, certified SFP, the EDSL displacement
+ active-HR question), not on anything left to implement in this repository set. This satisfies SAM#111's
own closure rule ("close only when PR5/full Iteration 3 is accepted - unless a later human decision
deliberately moves PR5 to a separate tracker").

**SAM#111 is CLOSED.** PR1-PR4 foundation frozen; PR5A infrastructure merged; PR5B selected-product cooling
accepted (canonical + real-project + close/reopen Review); #113 resolved and closed; #115 remains separate,
unrelated native debt; the TM59 Criterion 1 seasonal-basis fix and the SAM_Tas diagnostic alignment are both
merged; the four-airflow invariant is unchanged. Manufacturer ventilation behaviour continues at #123;
restored-Review evidence narration continues at #122. Neither is a reopened blocker for the work #111
tracked.

## Previous: Part O closeout reconciliation - four PRs merged, SAM#111 stays open (2026-09-16, later)

**What this entry is.** After SAM#119, SAM_Tas#61, SAM#120 (below) and
[SAM_Tas#62](https://github.com/SAM-BIM/SAM_Tas/pull/62) all merged into `sow/2026-Q3` the same day, this is
the final reconciliation pass across `PROJECT_PROGRESS.md` (both repos), `PartF-HANDOVER.md` and
`PartO-TAS-VALIDATION.md`, and the explicit SAM#111 closure decision.

**Final engineering position, distinguished as instructed:**
1. **B4 real-project acceptance: verified.** Iteration 3 runs end to end on the real project with the
   recirculation clamp in place (§ *3-B4 rerun on the merged clamp* below).
2. **Recirculation-flow clamp: verified.** SAM_Tas#60, merged and re-exercised on the real project - the
   refusal is gone.
3. **TM59 NV Criterion 1 seasonal basis: fixed and regression-covered.** SAM#120 - see § *TM59 Criterion 1
   ... found and fixed* below.
4. **SAM_Tas diagnostic representation: aligned to the same summer basis.** SAM_Tas#62 - diagnostic-log-only,
   no compliance-decision change.
5. **Iteration 1b (authored mechanical air, investigation-only) and Iteration 2 (acoustic
   restriction/bypass/boost placeholder), and the reopen-path investigation, remain separate, un-started
   follow-ups** - not touched by this reconciliation, per instruction.

**SAM#111 is NOT closed by this reconciliation.** Its own closure rule, stated in the issue body, is explicit:
*"Close this issue only when PR5 / full Iteration 3 is accepted - unless a later human decision deliberately
moves PR5 to a separate tracker."* PR5 ("manufacturer-aware behaviour") is the issue's own next milestone and
is listed there as **unchecked/open**, with a "Potential later work" list (selected-product parameter
overrides, justified heat recovery, fan behaviour/power, bypass/control behaviour, cooling modules, topology
variants) that is not exhausted. Items 1-4 above are real, verified evidence *within* the PR5 line of work
(PR5A/PR5B and this Criterion 1 slice), but they are sub-slices of PR5, not the whole of it. Closing #111 now
would contradict the issue's own recorded closure rule without the human decision it explicitly requires, so
it stays open. This is a genuine remaining item, not an oversight.

**Status.** Branch `fix/tm59-criterion1-summer-basis` off `sow/2026-Q3` (`192d069a`), PR TBD. SAM only - the
defect and its fix are entirely within `SAM.Analytical`; SAM_Tas, SAM_Systems and SAM_UI consume the result
types and report verbatim, with one related-but-unfixed instance noted below.

**What this closes.** The Criterion 1 inconsistency flagged in the (open, unmerged) SAM#119 doc PR and
independently pinned much earlier as **Codex 3821633849** (`## Deferred` below, previously left as "a
regulatory decision, not a review fix" because it would change Pass/Fail on every naturally-ventilated real
project). This session treats that decision as made, per explicit instruction, and implements it.

**The defect, confirmed by direct source inspection.** `TMExtendedResult.Criterion1`
(`SAM.Analytical/Classes/Result/TM/TMExtendedResult.cs:240-263`) decides Pass/Fail from the FULL-YEAR
`OccupiedHours`/`GetOccupiedHoursExceedingComfortRange()`, with no date filter.
`TM59NaturalVentilationExtendedResult` never overrode it, so it inherited the annual verdict - even though
that same type already carried the correct summer-restricted denominator (`GetSummerOccupiedHours()` /
`GetSummerMaxExceedableHours()`, `HourOfYear.SummerStartIndex..SummerEndIndex` = May-September, matching
TAS). Worse than the report-only framing in SAM#119: `TM59AssessmentReport.NaturalVentilationChecksFor`
already trusted the summer denominator for its displayed `Limit`, but read the **annual** numerator for its
displayed `Actual` - so a report row could show three different bases (annual Actual, summer Limit, annual
verdict) that were never required to agree.

**The fix.**
- `TM59NaturalVentilationExtendedResult` gained `GetSummerOccupiedHourIndicesExceedingComfortRange()` /
  `GetSummerOccupiedHoursExceedingComfortRange()` and now **overrides** `Criterion1` to decide Pass/Fail from
  these summer-restricted figures, mirroring the base implementation's own comparison (0 exceeding passes
  trivially; otherwise strictly `<`, never `<=`) on the summer basis instead of the annual one.
  `TM59NaturalVentilationBedroomExtendedResult` needed no change - it inherits the fix, since it never
  overrode `Criterion1` either.
- `TM59AssessmentReport.NaturalVentilationChecksFor`'s `actual_Criterion1` now reads the same new summer
  getter, so Actual, Limit and the verdict share one authority.
- `Query.Simplify`'s natural-ventilation and bedroom branches now populate the flattened plain result's
  `HoursExceedingComfortRange` from the same summer getter, so `TM59AssessmentCalculator.Calculate(extended:
  false)` - the default - carries the identical, internally-consistent figures through flattening instead of
  a stale annual copy.

**Regression** (`SAM.Tests/TM59NaturalVentilationCriterion1SeasonalBasisTests.cs`, new file, 3 cases):
600 non-summer + 100 summer occupied hours (annual limit 21, summer limit 3 - can never coincide), moving
exceedance hours between the two periods. 4 summer-only exceedances: old code passes (4 < 21), fixed code
correctly fails (4 < 3) - a real May-September failure the annual basis hid. 2 summer + 20 non-summer
exceedances: old code fails (22 < 21 is false), fixed code correctly passes (2 < 3) - a room wrongly failed
by hours TM59 was never meant to count. Exactly 3 summer exceedances (== the summer limit): old code passes
(3 < 21), fixed code correctly fails (3 < 3 is false), preserving the strict-`<` boundary intentionally. All
three fail pre-fix, pass post-fix.

**Verified NOT to affect the 3-B4 real-project result recorded above / in SAM#119.**
`TMOverheatingCalculator.Calculate_TM59` branches per space onto either the natural-ventilation types or
`TM59MechanicalVentilationExtendedResult` - the two never share a `Criterion1`.
`TM59MechanicalVentilationExtendedResult.Criterion1` is its own separate override
(`GetHoursNumberExceeding26() < MaxExceedableHours`, both annual by TM59:2017's own design for the mechanical
`>26°C` check), untouched by this fix. Every one of the 3-B4 real project's 8 assessed rooms is on that
mechanical route, so this fix changes none of its reported numbers. **No licensed rerun was performed or
required.**

**Test results.** `SAM.Tests` rebuilt explicitly (`dotnet build SAM.Tests/SAM.Tests.csproj -c Release`, not
`--no-build` against a stale binary) then run in full: **2154 passed, 0 failed, 0 skipped** - no regression,
including `TM59AssessmentReportTests`, `TMOverheatingCalculatorTests`, `TM59AssessmentCalculatorTests`, and
`TM59SimplifyTests` (one assertion updated to read the new summer getter, matching the three figures already
beside it - not reopening the rotation-order defect that test otherwise pins).

**A related instance found but explicitly NOT fixed here, out of this fix's scope - since fixed
(`SAM_Tas`#62).** `SAM_Tas/SAM.Analytical.Tas.TM59/Classes/PartODiagnosticLog.cs:646` and `:655`
(`SetCriterionSpecificFields`) logged `hoursExceedingComfortRange` for the extended natural/bedroom branches
from the same annual `GetOccupiedHoursExceedingComfortRange()` this fix moved away from, beside the
already-summer `summerOccupiedHours`/`maxExceedableSummerHours` fields on the same record - the identical
defect pattern, in a diagnostic evidence log rather than the assessment or report. It fed no Pass/Fail
decision. Left unfixed here: separate repo, its own prebuilt-DLL/TPD build chain, outside this fix's stated
scope. Fixed the same day in [SAM_Tas#62](https://github.com/SAM-BIM/SAM_Tas/pull/62), once this fix's
`GetSummerOccupiedHoursExceedingComfortRange()` reached a merged `SAM.Analytical.dll`.

**Files changed:**
- `SAM.Analytical/Classes/Result/TM/TM59NaturalVentilationExtendedResult.cs`
- `SAM.Analytical/Classes/TM59AssessmentReport.cs`
- `SAM.Analytical/Query/Simplify.cs`
- `SAM.Tests/TM59SimplifyTests.cs` (1 assertion updated)
- `SAM.Tests/TM59NaturalVentilationCriterion1SeasonalBasisTests.cs` (new, 3 cases)
- `PROJECT_PROGRESS.md` (this entry)

**Merge sequencing (updated after the fact).** SAM#119 and SAM_Tas#61 (documentation, recording the 3-B4
rerun and this same defect as "found, not yet fixed") were merged into `sow/2026-Q3` **before** this fix, per
explicit instruction to preserve that chronology in the integration history - see the demoted entry
immediately below. This fix's own branch was then reconciled against the post-#119 `sow/2026-Q3` tip
(conflict confined to this file, in the "Current" section header stack; no production code touched by the
reconciliation) and re-validated (`SAM.Tests` **2154/2154**, unchanged) before merging. No other blocker is
known.

## Previous (2026-09-16): 3-B4 rerun on the merged clamp - refusal gone, Candidate B FAILS TM59 (2026-09-16)

**Historical entry.** At the time this entry was written, item 0 below (natural-ventilation Criterion 1's
annual-vs-summer hour basis) was a confirmed, unfixed defect. It was fixed and regression-covered the same
day - see the *TM59 Criterion 1 ... - fixed (2026-09-16)* entry above. The text below is preserved unchanged
as the historical record; its "CONFIRMED, NOT FIXED" / "must not be closed" language describes that point in
time, not the current state.

**Status.** SAM#118 and SAM_Tas#60 (the clamp that fixes the 3-B4 refusal below) are both **merged**
(`SAM 192d069a`, `SAM_Tas 96f8ba79`). This entry is the follow-up session: rebuild all four repos at the
merged tips, rerun Iteration 3-B4 on the same preserved real project, and confirm the fix actually closes the
gap it was merged to close.

**Work completed.**
1. Verified starting state: all four repos on `sow/2026-Q3`, clean, level with remote (`git ls-remote`), at
   exactly the expected merged tips; SAM#118 and SAM_Tas#60 both confirmed `MERGED` via `gh pr view`.
2. Rebuilt Release in dependency order SAM -> SAM_Systems -> SAM_Tas -> SAM_UI (VS Framework MSBuild,
   `-p:RunPostBuildEvent=OnOutputUpdated` on SAM_UI). Confirmed `SAM_UI\build\SAM.Analytical.Tas.TPD.dll`
   refreshed and containing `RecirculationCoolingClamp`.
3. Reran Iteration 3-B4 on `SAM_zoningAM-CIBSEfutureZ1.sam` (SHA-256 `a7e09a25...`) through the external
   PowerShell UI-Automation harness (`stage.ps1 -Stage B4`, outside every repository - it drives the native
   `SAM Analytical.exe`, not a UI-automation framework inside the repositories) - same weather, full year,
   same 3 dwellings / 8 rooms as the 2026-09-15 acceptance. **The refusal is gone.** All 15 ledger stages
   COMPLETED (previously 9 completed, 1 refused, 5 never run) - `PartOIteration3Stage` carries 15 stages, not
   14, because `EquipmentResolution` was added; verified by name against the ledger, not assumed. **Candidate
   B TM59 now exists: FAIL**, 3 of 8 rooms (`Studio 1_0`, `Kitchen_4`, `Kitchen_7`).
4. Extracted and compared all stages (`extract-all.sh` + `compare.py --delta 3-B0:3-B4 --delta 1a:3-B4`) -
   `3-B4` no longer prints `SKIP` or `*refused*`; real numbers throughout.
5. Reran the coil replay (`prod B4 resolve=model stop=gen coolev=1`) on MVHR-02 (the unit that refused):
   `Count_Clamped = 2`, `MaximumClampedExcursion_Lps = 0.050278` - matches the SAM_Tas#60 investigation's
   `~0.0503` measurement.
6. Updated `PartF-HANDOVER.md` §0 (both PRs merged, verification pin repinned to `96f8ba79`),
   `PartO-TAS-VALIDATION.md` (rerun outcome + deltas), and the preserved
   `C:\TasOut\parto-final-real-project\evidence-draft.md` §9/§14 (rerun appended, history preserved) in one
   documentation-only PR.

**Important decisions and assumptions.**
- **The FAIL is accepted, not treated as a defect.** 3-B0 already failed 4 of 8 rooms on the same weather;
  the clamp fixes a reporting refusal at the control-law boundary, not the building's overheating. No
  production code was changed to obtain this result, and none should be.
- **A genuine but unrelated anomaly was found and NOT acted on.** The same coil replay showed a different air
  system (MVHR-03) with `Count_Clamped = 6015` at a ~170x smaller magnitude (`0.000587` l/s) than MVHR-02's -
  consistent with floating-point-scale noise at the `36` l/s floor, not the ramp-overshoot mechanism the
  clamp addresses. Per the session's own stop-condition ("investigate and report before making code
  changes"), this is recorded (`PartO-TAS-VALIDATION.md` § *3-B4 rerun on the merged clamp*,
  `evidence-draft.md` §9b) and left for a future session - it changes no TM59 result and no code was touched.
- **The preserved extraction harness needed machine-specific repair, not a design change.** `h\p0.csproj`,
  `tool\tool.csproj` and their `Prod.cs`/`Program.cs` AssemblyResolve hooks had `C:\Users\Virtual Machine\...`
  hardcoded from the machine the 2026-09-15 acceptance ran on; repointed to this machine and `p0.exe`
  rebuilt (its prebuilt `deps.json` predated the `SAM.Analytical.UI.WPF` reference its own source needs).
  This is explicitly non-production harness plumbing.

**Files changed** (documentation only, this repo): `documentation/PartF-HANDOVER.md` (§0),
`documentation/PartO-TAS-VALIDATION.md` (new § *3-B4 rerun on the merged clamp*), `PROJECT_PROGRESS.md` (this
entry). Outside this repo: `C:\TasOut\parto-final-real-project\evidence-draft.md` (§9b appended, §14
appended), `06-Iteration3-B4\` (rerun evidence; the refused run preserved under `refused-2026-09-15\`),
`replay\B4-coolev-2026-09-16\` (new coil replay), `extract\comparison.md` (regenerated), and the harness
source path fixes above (not production code).

**Validation.** All four suites rebuilt Release at the merged tips (unchanged from the closeout entry below):
`SAM.Tests` 2151/2151, `SAM.Analytical.Systems.Tests` 203/203, `SAM.Analytical.Tas.TM59.Tests` 930/930,
`SAM.Analytical.UI.WPF.Tests` 1027/1027. The rerun itself is licensed-TAS evidence, not a test suite result -
see `PartO-TAS-VALIDATION.md` for the full comparison tables.

**Unresolved issues, risks, blockers.**
0. **CONFIRMED, NOT FIXED - natural-ventilation Criterion 1 Pass/Fail uses the wrong hour basis.** Found by
   independent review during this session, confirmed by direct source inspection:
   `TMExtendedResult.Criterion1` (`SAM.Analytical\Classes\Result\TM\TMExtendedResult.cs:240-263`) decides
   Pass/Fail from the full-year occupied-hours basis, while `TM59AssessmentReport.NaturalVentilationChecksFor`
   (`TM59AssessmentReport.cs:266-285`) already displays the correct May-September basis for `Actual`/`Limit`
   - the two are never required to agree, and no test exercises a fixture where they would disagree. **Does
   not affect this session's own B4/B0/1a results** (100% mechanical route, no analogous split) - see
   `documentation/PartO-TAS-VALIDATION.md` § *Known open defect* for the full trace, consequence
   classification and the smallest correct remediation (not applied). **SAM #111 must not be closed as fully
   verified while this is open** - it bears on the natural-ventilation route's absolute Pass/Fail, not on the
   evidence gate this session's rerun satisfies.
1. **The MVHR-03 `Count_Clamped` anomaly above is uninvestigated beyond the observation.** Negligible
   magnitude, no result impact, no code changed - but not yet explained down to its source line.
2. **Iteration 1b simulates authored mechanical air** - unchanged from the closeout entry below;
   investigation-only, no code change made or intended by this session.
3. Everything else carried over unchanged from the closeout entry below (2B placeholder stage, clamp bound
   is measured not derived, `SAM.Tests` not in `SAM.sln`).

**Exact recommended next step (as originally written; item 0 has since been fixed - see above).** Open and
merge the documentation-only PR from this session (branch off `sow/2026-Q3`, never commit to it directly).
After merge, SAM#111's evidence gate for the recirculation-flow refusal is satisfied - **but #111 should not
be closed as fully verified** until item 0 above (the Criterion 1 hour-basis defect) is either fixed
(smallest remediation is stated in `PartO-TAS-VALIDATION.md`) or conclusively disproved by someone
re-deriving it independently. Separately, decide whether to investigate the MVHR-03 `Count_Clamped` anomaly,
and whether to act on Iteration 1b's standing recommendation (refuse rather than clear authored mechanical
air). Do not start Iteration 1b, Iteration 2, the reopen-path investigation, or a clamp redesign without an
explicit decision to do so.

## Previous (2026-09-16, earliest): Part O real-project acceptance closed out

**Status.** Documentation checkpoint on branch `docs/parto-real-project-acceptance-closeout`, PR
[SAM#118](https://github.com/SAM-BIM/SAM/pull/118) against `sow/2026-Q3`, **not merged**. SAM carries no
production change from it. The one production change the acceptance produced is in SAM_Tas
([#60](https://github.com/SAM-BIM/SAM_Tas/pull/60)), also not merged.

**Work completed.** The 2026-09-15 licensed acceptance ran Iterations 1a / 1b / 2 / 2B / 3-B0 / 3-B4 end to
end on a real project (`SAM_zoningAM-CIBSEfutureZ1.sam`, SHA-256 `a7e09a25...`, 3 dwellings, 9 spaces of
which 8 assessed, embedded DSY 2050s weather). **Nothing passes; 3-B4 refused.** This checkpoint recovered
the evidence onto a second machine, completed the evidence record, and closed both investigations.

**Decisions and assumptions.**
- **3-B4's refusal**: clamp at the law's ceiling (SAM_Tas#60). A flow just outside the range is reported AT
  the range and counted, not refused. Assumption stated in the code: `RecirculationCoolingClamp_Lps = 0.1` is
  a **measured** bound (~2x the largest of 5 excursions in 26 280 hours), not a derived one.
- **Iteration 1b's finding is investigation-only.** Frozen 1b behaviour is deliberately unchanged; the
  recommendation is a refusal, not a clear, because 1b's contract is "leaves the model as authored".
- Provenance travels **by commit**, not by binary hash: the build path is embedded in the assembly, so
  rebuilding identical commits on another machine gives different bytes.

**Files changed** (documentation only, this repo):
- `documentation/PartO-TAS-VALIDATION.md` - the acceptance record and both investigations.
- `documentation/PartO-ARCHITECTURE.md` - Iteration 3 is implemented; `ActiveTrimCooling` does **not** map to
  it; section 5's "Iterations 2 and 3 ... not implemented" split, since only Iteration 2's half is still true.
- `documentation/PartF-HANDOVER.md` - section 0 rewritten (four repos merged, two PRs in flight), the
  verification recipe repinned to the four tips, and the "level with its remote" invariant now says to check
  it with `git ls-remote`.
- `PROJECT_PROGRESS.md` - this entry, and PR5A corrected from "not merged" (`b4a1283f` **is** its merge).

**Validation.** All four suites Release at the pinned commits, 2026-09-16: `SAM.Tests` **2151 / 2151**,
`SAM.Analytical.Systems.Tests` 203 / 203, `SAM.Analytical.Tas.TM59.Tests` 922 / 922 (929 with SAM_Tas#60),
`SAM.Analytical.UI.WPF.Tests` 1027 / 1027. All four solutions rebuilt Release, 0 errors. The handover
verification recipe was re-run and prints exactly four `descends from` lines.

**Unresolved issues, risks, blockers.**
1. **Iteration 1b simulates authored mechanical air.** `PreparePartOIteration`'s `SkipNaturalVentilation`
   branch is a pure no-op copy: it clears nothing and refuses nothing, so an authored model's 17
   `SpaceAirMovement`s survive and are simulated. Measured: 1b is bit-identical to 1a across all 78 840
   readings. **No test pins removal.** Open; no change made.
2. **The clamp bound is measured, not derived.** Nothing structural bounds a controller overshoot, so a
   steeper model can consume more of the 0.1 l/s. Clamped hours are recorded (`Count_Clamped`,
   `MaximumClampedExcursion_Lps`) so this stays visible; revisit with a fresh measurement, do not raise.
3. **Iteration 2 is a placeholder stage.** Acoustic restriction / bypass / boost are not implemented, which
   is why stage 2 is bit-identical to 1a. `AcousticRestricted` still refuses for want of an operating
   condition.
4. **`SAM.Tests` is not in `SAM.sln`**, so `dotnet test --no-build` silently runs a stale binary - it
   reported 2114 here until caught. Build it by path.
5. The licensed inputs (model, harness, every `.tsd`, evidence) exist only under
   `C:\TasOut\parto-final-real-project\` on one machine. The repositories recover the code revision, not the
   run.

**Exact recommended next step.** Merge [SAM#118](https://github.com/SAM-BIM/SAM/pull/118) (approved), then
review and merge [SAM_Tas#60](https://github.com/SAM-BIM/SAM_Tas/pull/60). After #60 merges, re-run the 3-B4
stage on the real project to confirm the refusal is gone and Iteration 3 completes end to end - that run is
what would let SAM#111 close. Then decide Iteration 1b (item 1): the recommendation on record is that 1b
should **refuse** a model carrying authored mechanical air movement, naming the objects.

`fix/simulation-provenance-roundtrip` is merged (PR #116 -> `553957dc`). The Iteration 1a / 1b / 2 / 2B
programme is **FROZEN** - see the two entries after the PR5A one.

Everything below those entries is superseded history retained for context, and its forward-looking
claims (branch names, "next step" lists) are historical rather than current.

## PART O ITERATION 3 PR5A - SAM SLICE: MANUFACTURER VOCABULARY + COM-FREE RESOLVER (2026-09-11)

First production PR of PR5A (`SAM#111`, frozen PR5 plan §B / §K.1). PR5A merge order is SAM -> SAM_Systems ->
SAM_Tas -> SAM_UI. **SAM only**: SAM_Systems, SAM_Tas and SAM_UI have no production change from this slice.

**What was added** (all in `SAM.Analytical`, all optional, none populated):
- `HeatRecoveryPerformance` and `FanPerformance` on `VentilationUnitTemplate`. Both are typed views of the
  existing `VentilationUnitPerformanceTable` grammar, sharing the abstract base `VentilationUnitAirFlowPerformance`:
  one `AirFlowRate` axis in l/s and one output, `SensibleHeatRecoveryEfficiency` [-] or `SpecificFanPower`
  [W/(l/s)]. Each also carries a stated basis (`HeatRecoveryEfficiencyBasis.SupplyTemperatureEfficiency`,
  `SpecificFanPowerBasis.TotalBothFans`; `Undefined` refuses), a required `Source`, and a stored domain policy.
  The policy defaults to Refuse; only Refuse and ClampToDomain are accepted, and extrapolation makes the data
  unusable.
- `Query.VentilationUnitOperatingParameters(template | templates + reference, supply_Lps, extract_Lps,
  tolerance)` returns an immutable `VentilationUnitOperatingParameters`: ε and SFP, each resolved or refused
  independently with a sentence. It never returns null.
- `Query.SelectedVentilationUnitTemplate(AirHandlingUnit, templates)`: identity only.

**Decisions:**
- **Absent is not zero.** A missing field refuses. It never becomes 0, never becomes MVRE's 0.7, and never
  falls back to parity.
- The design airflow is used only as a lookup coordinate. Capacity is never read.
- **An unbalanced duty refuses both quantities** (tolerance default 0.001 l/s). This is the generic reason
  certified figures are stated at balanced flow, and it matches Phase 0 X1-C, where TAS unbalanced MVHR is
  UNKNOWN.
- **Not added to SAM:**
  - the plan's `SummerBypass` descriptor: nothing in PR5A consumes it and its thresholds are unpublished;
  - the B3 supply-limit setpoint: declared scenario/route policy, downstream per plan §C;
  - any `ExchCalcType` or other TAS concept;
  - a catalogue schema tag, since `VentilationUnitCatalogue:v2` is the SAM_Systems reader's.
- **E1/E2 (certified MRXBOX ε and SFP vs airflow) are still missing.** No value was transcribed; the
  real product therefore refuses B1/B2.

**Files:**
- `SAM.Analytical/Classes/System/{VentilationUnitAirFlowPerformance,HeatRecoveryPerformance,FanPerformance,VentilationUnitOperatingParameters}.cs`
  (new)
- `VentilationUnitTemplate.cs` and `VentilationUnitPerformanceOutput.cs` (new constants)
- `Enums/{HeatRecoveryEfficiencyBasis,SpecificFanPowerBasis}.cs`
- `Query/VentilationUnitOperatingParameters.cs`
- `SAM.Tests/PartOVentilationUnitOperatingParametersTests.cs` (+31 tests)

**Validation:**
- Focused Part O ventilation-unit, catalogue-drift, project-test, equipment and capacity-envelope suites: 348 / 348.
- `SAM.Tests`: **2151 / 2151** (was 2120).
- `SAM.sln` Release (VS MSBuild 18): 0 errors.
- Note: `SAM\build` now holds this branch's binaries.

**Next step:** independent review and merge of this PR. Then the **SAM_Systems PR5A slice**:
- catalogue reader v1 + v2;
- the E1/E2 evidence gate (transcription only from a certified source);
- `MechanicalVentilationUnitSettings` keyed by AHU GUID;
- DisplacementVentilation normalisation on MV -> MVRE (Phase 0 test A).

## SIMULATION-RESULT PROVENANCE ROUND TRIP - MERGED (PR #116 -> `553957dc`, 2026-09-11)

Branch `fix/simulation-provenance-roundtrip` from `sow/2026-Q3` `413215cc`; merged into `sow/2026-Q3` as
`553957dc`, and `SAM_UI#100` merged after it (`736e1771`). `SAM#111` stays open for PR5.

**The defect.** `SimulationResultProvenance.Fingerprint(AnalyticalModel)` did not survive a TAS run's
`.sam` save and reopen, so `PartORun.Restore` and the Iteration 3 Review refused the model their own record
was taken from ("The model has changed since the simulation results ... were produced from it"). Measured
on the licensed PR4 files, not assumed - there were **two independent contributors**, and TAS result series
were neither:

1. **`GroundTemperature` turned "not stated" into 0.** `ToJsonObject` omits a NaN `Depth` / `Conductivity`
   / `Density` / `SpecificHeat`; `FromJsonObject` assigned only present keys, so an omitted one came back
   as the C# default 0. `SAM.Weather.Tas` writes NaN for all four (TAS weather carries no soil
   properties), so every TAS-weather run reopened as different weather. Proof: hashing the saved `.sam`
   text reproduced the recorded fingerprint exactly (`3bcc2e42...`, `12689ed1...`); the reload's only
   delta was those four fields. Fix: an absent field reads back NaN (the inverse of the write rule).
2. **SAM_UI rewrites its view state on every open.** `AnalyticalWindow` (`Reload` ->
   `UpdateUIGeometrySettings`) regenerates the legends inside the `UI Geometry Settings` model parameter
   before `PartORun.Restore` fingerprints the model; 29 legend colours/labels were the ONLY difference
   (captured by Save As from the running app and diffed section by section). Fix: excluded by name as
   presentation-only, beside `CaseDescription` - it is SAM_UI's own `AnalyticalModelParameter`.
   **Redefines the digest:** records stamped before it fail closed and their runs are re-simulated
   (documented rule). The existing `C:\TasOut\pr4h` pairing is one of those.

Files: `SAM.Weather/Classes/GroundTemperature.cs`, `SAM.Analytical/Classes/SimulationResultProvenance.cs`,
`SAM.Tests/SimulationResultProvenanceTests.cs` (+5 tests), `SAM.Tests/WeatherTests.cs` (+1). Each new test
failed before its fix.

**Validation.** `SAM.Tests` 2120 / 2120. `SAM.sln`, `SAM_Systems.sln`, `SAM_Tas.sln` Release (VS MSBuild)
and `SAM_UI.sln` Release 0 errors; `SAM Analytical.csproj` rebuilt `--no-incremental`, `deps.json` lists
`SAM.Analytical.Tas.TPD`, app `SAM.*.dll` byte-identical to `SAM\build`.

**Licensed acceptance in `SAM Analytical.exe`** (UI Automation, `C:\TasOut\pr5a`, canonical fixture
SHA-256 verified, `Leeds_TRY` from `CIBSE Weather 2021.twd`): fresh Iteration 1a -> Reference A TM59 PASS,
run model fingerprint `b523598e04ea1e4d` recorded = reopened; Iteration 3 run COMPLETE, 14 / 14 stages,
3 systems, 8 rooms, 14 legs (3/6/5), bias +0.373 K, RMSE 0.834 K, max 2.909 K, 0 of 8 outcomes differ;
Candidate B model `455a9cb66897497e` recorded = reopened. Review #1, Review #2 and Review after a full
application close/reopen all COMPLETE with the same numbers; no TBD / TPD / TAS3D process (the only TAS
processes are TSD document servers reading the two results files for the TM59 re-assessment); every
comparison-defining file byte-identical - only the two `-TM59.txt` reports are rewritten, by design, with
identical content across reviews. Negatives: bridge TSD write time +1 h -> REFUSED at Input naming that
file, no TAS process, time restored exactly; north-angle-mutated Reference A -> restore refused "The model
has changed", Review and Iteration 3 disabled.

**Next step:** independent review of the SAM PR; then merge it **before** `SAM_UI#100`, and rebuild
`SAM_UI` (app project non-incrementally) against the merged `SAM\build`.

## PART O ITERATIONS 1a / 1b / 2 / 2B - FROZEN (2026-09-08)

Native acceptance PASSED in SAM_UI, and the equipment-selection closeout is merged into `sow/2026-Q3` in
dependency order.

| Repository | PR | Reviewed head | Merge commit | Frozen integration SHA |
| --- | --- | --- | --- | --- |
| `SAM` | #110 | `0f6dbaf7f9d5c490203e62ebc507ba6a8370a900` | `85d7b70d9d33cb0349be77f27a1c53b8679d09c5` | `85d7b70d9d33cb0349be77f27a1c53b8679d09c5` |
| `SAM_UI` | #98 | `f5c6ef49631a56decf3cc08ff02b5ecdeb84564f` | `68e314a5f62dd3cd78fd832b92b1911fb4fbeaf2` | `68e314a5f62dd3cd78fd832b92b1911fb4fbeaf2` |
| `SAM_Systems` | - | untouched | - | `9e1cd06f6031852a3c9395be476b01d8c44a1a4c` |
| `SAM_Tas` | - | untouched | - | `ec7f50543e123f8a734b6e27b2b16c0cf1f1edde` |

This documentation commit is a descendant of the frozen `SAM` SHA and changes no code. `SAM_Systems` and
`SAM_Tas` carry **no production change** in this closeout: both are at the same SHA they were at when the
work started, with clean trees - `VentilationUnitCatalogue.JSON` is byte-identical, which is deliberate,
because a project test product is composed with the shipped catalogue at the point of use and never
written into it.

Post-merge verification on those exact heads, `SAM` built first so `SAM_UI` consumed the merged
`SAM\build\SAM.Analytical.dll` rather than the old feature branch:

```text
SAM.sln Release                                  0 errors
SAM_UI.sln Release                               0 errors
SAM.Tests                                        2114 / 2114
SAM.Analytical.UI.WPF.Tests                       771 /  771
SAM.Analytical.Systems.Tests                       90 /   90
git diff --check (all four repositories)         clean
working trees (all four repositories)            clean
```

### The authority split this freeze protects

Four quantities that share a unit and mean different things, still separate in the merged tree:

```text
PartFRequiredAirFlow  !=  DesignAirFlow  !=  SelectedEquipmentCapacity  !=  OperatingAirFlow
```

- `AirHandlingUnitParameter` has **exactly one** member, `VentilationUnitReference` - identity, never
  capability and never duty - and no other parameter enum is associated with `AirHandlingUnit`.
- `PartOEquipmentSelection`'s only field is `List<VentilationUnitReference>`; it serializes `Mode` and
  `AllowedVentilationUnitReferences` and nothing numeric. `AllowedVentilationUnitReferences !=
  SelectedVentilationUnitReference`.
- Equipment capacity stays in a descriptor, resolved by identity, never copied onto a model.

Confirmed on the merged trees, by reading them rather than by adding anything:

- **Iteration 1a** remains the base mechanical MVHR design; no manufacturer selection runs where none was
  asked for (a null catalogue means "run no rule", not "a catalogue offering nothing").
- **Iteration 1b** remains natural ventilation and invents no mechanical topology and no continuous
  mechanical airflow - `PartOBaseMVHRTests.TheNaturalVentilationRoute_BuildsNoMechanicalTopology`,
  `PartOIterationPreparationTests.NVDwelling_InventsNoContinuousMechanicalSupplyOrExtract` and
  `NVDwelling_SaysWhyNoMechanicalAirflowWasApplied_WithoutClaimingItSizedAnything`.
- **Iteration 2** selects and assigns equipment without moving any airflow.
- **Automatic - all catalogue products** works and means the shipped manufacturer catalogue.
- **Automatic - selected pool** has **no fallback**: an empty pool refuses by name and is never widened.
- **Manual per dwelling** remains authoritative - no rule runs, an insufficient assignment is held and
  reported, and a deliberately larger product is never reduced.
- **The project test product** is project-scoped (`AnalyticalModelParameter.PartOProjectTestVentilationUnit`,
  no `ActiveSetting` anywhere near it) and **never joins Automatic - all**; the rule is written once, in
  `PartOEquipmentSelection.CandidateDescriptors`.
- **Iteration 2B** locks the selected identity and treats the selected product's capacity as a **ceiling
  only** - `PartOCapacityEnvelopeTests.TheEnvelope_NeverReselectsTheVentilationUnit`,
  `TheEnvelope_GrowsWithinAnAssignedProjectTestUnit_AndReselectsNoManufacturerProduct`,
  `PartODesignAirFlowRoundTests.ACombinedRoundBeyondSelectedCapacity_IsNotAdopted`,
  `PartOVentilationUnitSelectionTests.Candidate_BeyondSelectedCapacity_NeverReselects_EvenWhereABiggerProductIsOffered`.
- **No silent MRXBOX <-> XBC15 <-> test-product reselection** anywhere: every dwelling round reports
  `Kept`, never `Reselected`.
- **Saved and reopened** equipment configuration stays project-scoped, and restored result workflows
  trigger no reselection.

**Iteration 1a FROZEN. Iteration 1b FROZEN. Iteration 2 FROZEN. Iteration 2B FROZEN.**

A presentation-only UI consistency review follows separately - wording, tooltips, spacing, hierarchy,
disabled-control visibility, table sizing. It must not reopen the engineering authority closed here.
**Iteration 3 has not been started.**

## Latest (2026-09-08): a project may state one test ventilation unit, and a picker stops printing rank

**Status: implemented, tested, natively accepted, merged, frozen.**
PR #110, branch `feature/parto-equipment-selection-ux`, off `sow/2026-Q3` at **`847a505`**.

Two additions to the Part O equipment domain, both deliberately narrow.

### `PartOProjectTestVentilationUnit` - one optional what-if, and where it has to live

A name and two maximum airflows, so "what would a 165 l/s box do here" can be answered without editing
shipped manufacturer data and without inventing a fictional Nuaire entry that no later report could tell
from real data. Its identity is synthetic and says so: manufacturer is the literal words `Project test`,
model is the name.

It persists on the `AnalyticalModel`, as
`AnalyticalModelParameter.PartOProjectTestVentilationUnit`, beside `PartOEquipmentSelection` and
`PartOIsolationContext`. **It has to.** A dwelling assigned to it stores only an identity, so the capacity
behind that identity must be readable again after the project is reopened - otherwise a sound saved
dwelling returns as "capacity unknown" and Iteration 2B loses its ceiling. And it must persist **no
wider** than the project: an application setting is process-global and would carry one project's what-if
into the next, which is the same argument `PartOEquipmentSelection` was written for.

Its capability crosses into selection through the **one existing seam** -
`Query.VentilationUnitTemplate(PartOProjectTestVentilationUnit)` then the existing
`Query.CapacityDescriptor` - so it is judged by the same rule a manufacturer product is, and there is no
second mapping to drift. `Source` states in words that the figures are not manufacturer data. `Rank` is
far beyond any shipped rank, so a test product the same size as a real one **loses** the tie rather than
making the catalogue ambiguous: a what-if must never turn a working selection into a refusal.

Two new `PartOEquipmentSelection` overloads carry the whole of its selection semantics, and the one that
matters is stated once:

- **`AutomaticAllProducts` means the MANUFACTURER catalogue.** The test product is not offered to it at
  all, so a capacity left typed in a box cannot silently size a project and no historic answer moves.
- **`AutomaticSelectedPool`** offers it, and then the pool decides - it has to have been ticked.
- **`ManualPerDwelling`** may pick it.

The existing single-argument methods are untouched, so every prior test still exercises the old path.
Nothing about the type is stored in `PartOEquipmentSelection`, which still holds identities and never a
capacity, and nothing is stored on the air handling unit, which still holds an identity and never a
capability.

### `VentilationUnitCapacityDescriptor.Label` - rank is internal

`Rank` is a catalogue tie-breaker. It is not an airflow, an efficiency, a performance score or an
engineering output, and a product picker that prints `rank 10` invites an engineer to read a preference
number as a rating or to compare two products by it. `Label` is the engineer-facing form - identity and
both maximums, no rank. `ToString()` is unchanged and remains the diagnostic form, because rank is the
subject of the two refusals that embed it (a catalogue conflict, and two products of the same size and
rank). Every ordering role of rank is untouched: `Compare`, `CapableVentilationUnits`,
`AirHandlingUnitDesignDuty`, and the assignment set's own duplicate-identity index.

### Files changed
- `SAM/SAM/SAM.Analytical/Classes/PartOProjectTestVentilationUnit.cs` (new)
- `SAM/SAM/SAM.Analytical/Query/ProjectTestVentilationUnit.cs` (new)
- `SAM/SAM/SAM.Analytical/Classes/PartOEquipmentSelection.cs` (two overloads)
- `SAM/SAM/SAM.Analytical/Classes/System/VentilationUnitCapacityDescriptor.cs` (`Label`)
- `SAM/SAM/SAM.Analytical/Enums/Parameter/AnalyticalModelParameter.cs` (one entry)
- `SAM/SAM/SAM.Tests/PartOProjectTestVentilationUnitTests.cs` (new, 30 tests)
- `SAM/SAM/SAM.Tests/PartOEquipmentPresentationTests.cs` (new, 13 tests)
- `SAM/SAM/SAM.Tests/PartOCapacityEnvelopeTests.cs` (+1: the 2B ceiling on an assigned test product)
- `SAM/PROJECT_PROGRESS.md` (this file)

### Validation
- Focused Part O runs: 417/417 passed on the merged tree.
- Full `SAM.Tests` Release: **2114 passed, 0 failed** (was 2070; +44 new).
- `SAM.sln` Release: 0 errors. `git diff --check`: clean.

## XBC15 CLOSEOUT COMPLETE - READY FOR FINAL EQUIPMENT-SELECTION UX

Merged into `sow/2026-Q3` on 2026-09-07, in dependency order, after native Iteration 2 acceptance PASSED:

| Repository | PR | Merge commit | `sow/2026-Q3` head |
| --- | --- | --- | --- |
| `SAM_Systems` | #19 | `61cedcf52f78ef4b029a5f6ef9980759ab329b9e` | `61cedcf52f78ef4b029a5f6ef9980759ab329b9e` |
| `SAM` | #109 | `a08c9df7c1a1513a61e17618539e037c8e6bf75c` | `a08c9df7c1a1513a61e17618539e037c8e6bf75c` |
| `SAM_UI` | #97 | `ec0d0f624c066719e219bbf2e5e30e392aaf07f0` | `ec0d0f624c066719e219bbf2e5e30e392aaf07f0` |
| `SAM_Tas` | - | untouched | `ec7f50543e123f8a734b6e27b2b16c0cf1f1edde` |

Post-merge verification on those exact heads, rebuilt in dependency order `SAM` -> `SAM_Systems` -> `SAM_UI`:

```text
SAM.sln / SAM_Systems.sln / SAM_UI.sln Release   0 errors each
SAM.Tests                                        2024 / 2024
SAM.Analytical.Systems.Tests                       90 /   90
SAM.Analytical.UI.WPF.Tests                       645 /  645
git diff --check (all four repositories)         clean
```

The catalogue deploys with both products: the installed copy at
`%APPDATA%\SAM\resources\Analytical\Systems\VentilationUnit\VentilationUnitCatalogue.JSON` is
byte-identical to the merged repository copy and carries `MRXBOXAB-ECO5-AECV` (150/150 l/s) and `XBC15`
(190/190 l/s). That is also what proves the "skip where the catalogue is not installed" guard in
`PartOPresentationTests` does not trip on this machine, so the two-product assertions really ran.

**Nothing is FROZEN.** Iterations 1a / 1b / 2 / 2B are deliberately not marked frozen: the freeze gate is
the equipment-selection UX task that follows - allowed product pool, convert-to-manual, per-dwelling manual
assignment - which adds the very product-list surface the native Iteration 2 check could not exercise.

## Latest (2026-09-07): the real 150 -> 190 l/s selection ladder, and the Iteration 2B closeout

**Status: implemented, tested, and natively accepted.**
Branch `test/parto-real-selection-ladder`, off `sow/2026-Q3` at **`c980d4c`** (the merge of PR #103).

### Native acceptance (recorded 2026-09-07)

**Iteration 2 PASSED**, observed in SAM_UI on the combined PR heads, with no TAS rerun. All three dwellings
still automatically select `Nuaire MRXBOXAB-ECO5-AECV`; design duties remain 30/30, 63/63 and 63/63 l/s;
selected equipment capacity remains 150/150 l/s; headroom remains 120/120, 87/87 and 87/87 l/s; Approved
Document F and Design airflow are still shown as separate quantities and remain consistent with the
previously accepted model; and no equipment-selection operation changed a design duty.

The "catalogue reports two selectable products" assertion is **NOT APPLICABLE to the current interface**:
SAM_UI does not expose the catalogue product list at all - `PartOIterationWindow` offers only the
`SelectVentilationUnit` on/off checkbox - so there is no surface on which a product count could be read.
The two-product fact is pinned in CI instead. The missing surface is an explicit usability gap carried into
the equipment-selection UX task that follows, not a defect of this closeout.

**Nothing is FROZEN.** This closeout is recorded as
**XBC15 CLOSEOUT COMPLETE - READY FOR FINAL EQUIPMENT-SELECTION UX**.

### Tests only. No production code changed.

`SAM_Systems` now ships a second real product - **Nuaire XBOXER XBC15, 190/190 l/s** beside the MRXBOX's
150/150 - which turns the Approved Document O selection ladder into a manufacturer boundary rather than a
fixture, and gives Iteration 2B a real ~40 l/s capacity-headroom case. Nothing in `SAM.Analytical` needed to
change for it: the generic architecture already carried it. That is the point of the seam, and these tests
are the evidence.

`SAM.Analytical` does not reference `SAM.Analytical.Systems` and must not start. The descriptors below
therefore **state** the real identities and capacities (`Nuaire`/`MRXBOXAB-ECO5-AECV`/`MR-ECO-COOL-V` at
150/150, `Nuaire`/`XBC15` at 190/190) rather than reading a file. That those figures are what the shipped
catalogue actually says is `SAM_Systems`' own assertion, in `VentilationUnitCatalogueTests`. Both new
fixtures also offer a 500 l/s product nothing may reach for, so a test proving nothing escalated is proving
a choice rather than the absence of an alternative.

### `PartOVentilationUnitSelectionTests` - three tests

- `TheRealLadder_EscalatesFromMRXBOXToXBC15_AndThenRefuses` - one physical dwelling grown through the whole
  ladder on the model: at 150 l/s the MRXBOX is exactly on its rating and re-selecting stays put; at 160 the
  MRXBOX is **exhausted** and re-selecting from the *current* design escalates to the XBC15 with 30 l/s of
  headroom; at 190 exactly on the XBC15's rating, still not escalated; at 191 refused, naming 190, with
  **nothing written** - the unit keeps the selection it already had. Approved Document F never moved
  through any of it, asserted over the whole model at the end.
- `ExplicitlySelectingXBC15_ChangesOnlyTheEquipmentFacts` - the closeout's own question. The automatic rule
  chooses the MRXBOX at this dwelling's duty because it is the smaller capable product; writing the XBC15's
  identity onto the unit instead moves the identity and the capacity that identity resolves to, and moves
  **nothing else**. Every Approved Document F requirement, every design airflow, every air movement, every
  runtime/profile airflow and the air handling unit's own design duty are bit-identical before and after.
- `TheXBC15Selection_StoresIdentityAndLooksTheCapacityUp` - the model stores an identity and no capacity.
  The three identity fields survive serialization; offered the real ladder the ceiling resolves to 190;
  offered a catalogue that does not hold the XBC15 the ceiling is **unknown** - not the MRXBOX's 150, and
  not unlimited - and is reported as a refusal with a reason rather than as a pass.

### `PartOCapacityEnvelopeTests` - four tests, the Iteration 2B closeout

A new `RealLadderFixture` builds the Flat 1 shape at the design figures native acceptance recorded once the
MRXBOX's own envelope had been taken up - **150 supply / 82.5 + 67.5 extract, balanced at 150/150** - with
the XBC15 **deliberately selected** rather than the smaller MRXBOX, and the MRXBOX offered *first* in the
list so an envelope that re-ran the selection rule or took the head of the list would visibly swap the plant.

- `XBC15sRating_IsTheCeilingTheOneFiftyDesignGrowsWithin` - scale `190/150`, so 190 supply / 104.5 + 85.5
  extract; `82.5/67.5 == 104.5/85.5`, the proportions survive; duty 150 -> 190 with **zero headroom left and
  not a thousandth past the rating**; the 40 l/s spent as design airflow on both sides.
- `TheEnvelope_KeepsXBC15Selected_EvenThoughTheSmallerProductIsStillOffered` - the premise is asserted first
  (the automatic rule *would* pick the MRXBOX at 150/150), then the identity is shown stable on both the
  source and the envelope cluster, the group's product is the XBC15, and every dwelling round reports
  `Kept` rather than `Reselected`.
- `TheEnvelopeOverXBC15_MovesDesignAirflowAndNothingElse` - the authority separation, on the real case:
  requirements unchanged (13 l/s stays 13 l/s beside a 190 l/s design), the capacity present only on the
  descriptor the envelope was handed, no air movement created/removed/re-rated, and the source design
  untouched by value **and** by reference. The design did move, and the deliberate adjustments report it
  against the design it grew from.
- `ADesignAlreadyAtXBC15sRating_ReportsNoHeadroomRatherThanExceedingIt` - at 190/190 the existing
  `NoHeadroom` outcome fires, no model is produced, the XBC15 is still selected, the 500 l/s product was
  never reached for, and the design is left where it was rather than shrunk to create headroom to report.
  **No capacity rule was relaxed to make the diagnostic produce an answer.**

### Validation

| Suite / build | Result |
| --- | --- |
| `PartOVentilationUnitSelectionTests` (focused) | **133 / 133** (was 130) |
| `PartOCapacityEnvelopeTests` (focused) | **38 / 38** (was 34) |
| `SAM.Tests` (full) | **2024 / 2024** (was 2017) |
| `dotnet build SAM.sln -c Release` | **0 errors** |
| `git diff --check` | clean |

Diff is **489 insertions, 0 deletions**, across those two test files only. Sibling repositories:
`SAM_Systems` `feature/parto-catalogue-xbc15` (the catalogue entry - the substantive change),
`SAM_UI` `test/parto-xbc15-capacity-ceiling` (the same closeout through the production orchestration),
`SAM_Tas` `ec7f505` unchanged. None is a build dependency of this branch.

### Iteration 3 boundary

Nothing here needed MRX cooling or performance data. Iteration 2B read **equipment capacity and nothing
else** - `SelectedVentilationUnitCapacityDescriptor`, a ceiling - exactly as designed. The XBC15 entry
deliberately carries no cooling capacity, supply-air temperature, heat-exchanger performance or controller
ramp, because Nuaire publishes none for that range, and no architectural dependency on such data surfaced.

### Next step

- Human review, then merge. **Do not merge automatically.**

## Superseded (2026-09-07): SAM #103 - Rhino-hosted GH test harness, regression run, two production fixes

**Status: ready for review, HOLD lifted. Not merged.** Branch `fix/gh-variable-output-updater`,
pushed. This closes out the "HOLD - SAM #103" item recorded below on 2026-09-05: the Grasshopper
tests could not run outside a Rhino-enabled environment before this; they now do, for real.

### The harness: `SAM.Core.Grasshopper.Tests` converted from xUnit to NUnit + Rhino.Testing

The previous xUnit + hand-rolled NuGet-cache assembly resolver never initialized Rhino's native
runtime, so any test touching `GH_Document` hung `dotnet test` instead of skipping. Converted to
NUnit + McNeel's `Rhino.Testing` package, which hosts a real headless Rhino 8 + Grasshopper
runtime in-process (`Rhino.Runtime.InProcess.RhinoCore`, configured by
`Grasshopper/SAM.Core.Grasshopper.Tests/Rhino.Testing.Configs.xml`, pointing at the installed
`C:\Program Files\Rhino 8\System`). Requires Rhino 8 actually installed on the machine running the
tests; CI (`.github/workflows/test.yml`) still runs only `SAM/SAM.Tests` and does not cover this
project.

Every test method body is unchanged: `XunitCompatAssert.cs` is an xUnit-shaped `Assert`/`Skip`
surface backed by NUnit, in the same namespace as the tests, so unqualified `Assert`/`Skip` calls
resolve there rather than to `NUnit.Framework.Assert` - only `using`s and
`[Fact]`/`[SkippableFact]` → `[Test]` attributes changed. Verified 1:1: same 44 test method names,
same per-file counts, before and after (the 45th, `CopyPersistentDataReplacementTests`, is new -
see below).

Test-fixture helper classes that derive from Grasshopper base types (`TestUpdatableComponent`,
`TestVariableOutputComponent`, `TestDocument`, ...) moved to a new sibling project,
`Grasshopper/SAM.Core.Grasshopper.Tests.Fixtures`. NUnit's test discovery enumerates every type in
the discovered assembly before any `[SetUpFixture]` runs, so those classes needed Grasshopper.dll
resolvable just to be discovered - before Rhino.Testing ever got a chance to host it. Kept out of
the discovered assembly, they're only touched from inside a running `[Test]`, after Rhino is up. A
`[ModuleInitializer]` bootstrap does **not** fix this - it does not run early enough to precede the
NUnit engine's own type enumeration of the assembly; this was tried and empirically failed before
the project-split was found.

**Result: all 45 tests pass for real (0 skipped)** - required: `VariableOutputUpdateTests` 9/9,
`ManualReconnectionTests` 11/11, `MissingConnectionsTests` 11/11 (31/31); full suite adds
`SAMCoreUpdateContractTests` 5/5, `TryGetSAMGeometriesTests` 8/8, and (new, see below)
`CopyPersistentDataReplacementTests` 1/1 (45/45 total). Re-run and confirmed after the regression
test was added, not carried over from an earlier checkpoint.

### Two production defects the real run surfaced (both invisible while tests only skipped)

1. **`Modify.CopyPersistentData` reflection was ambiguous, so it copied nothing.**
   `GH_PersistentParam<T>` declares both `AddPersistentData(T)` and `AddPersistentData(object)`, so
   the unqualified `type.GetMethod("AddPersistentData")` always threw `AmbiguousMatchException`,
   silently swallowed by the surrounding best-effort `catch`. Persistent data was never actually
   copied onto a replacement parameter during a component update.
2. **Codex P1, found on this branch's own fix.** The first fix for #1 resolved the ambiguity by
   naming the parameter type and looping `AddPersistentData(item)` per item - but a component whose
   *default* input already ships with baked-in persistent data (e.g.
   `SAMAnalyticalCreateCaseByApertureByAzimuths`'s `_ratios`, `SetPersistentData(0.15, 0.2, 0.25,
   0.2)` at registration) gets a **freshly-constructed replacement parameter that already carries
   those same defaults**. Appending the old parameter's values onto that instead of replacing it
   doubles up: 4 baked-in defaults + N authored values, not N. Fixed properly by reflecting
   `GH_PersistentParam<T>.SetPersistentData(GH_Structure<T>)` and handing over the old parameter's
   whole `PersistentData` tree in one call - replaces wholesale (defaults and all) and preserves
   branch/tree structure, rather than flattening it through repeated single-item `Add` calls.

Both fixes are in `Grasshopper/SAM.Core.Grasshopper/Modify/CopyPersistentData.cs`, each a handful
of lines. A new regression test exercises exactly the Codex-flagged scenario (default input with
baked-in persistent data, updated to a different authored value set, asserting the replacement
carries exactly the authored values - not defaults-plus-authored).

### Files changed

**This session** (the Rhino harness and the two `CopyPersistentData` fixes):
`Grasshopper/SAM.Core.Grasshopper.Tests/*` (converted to NUnit, `GrasshopperTestSetup.cs`,
`XunitCompatAssert.cs`, `Rhino.Testing.Configs.xml`, `CopyPersistentDataReplacementTests.cs` new;
`GrasshopperTestAssembly.cs` removed); new project
`Grasshopper/SAM.Core.Grasshopper.Tests.Fixtures/*` (including the new
`TestDefaultPersistentDataComponent` fixture, in `TestComponents.cs`);
`Grasshopper/SAM.Core.Grasshopper/Modify/CopyPersistentData.cs`.

**The updater fix itself** (PR #103's original commit, predates this session - recorded here
because it was missing from this file, itself a Codex P1 finding):
`Grasshopper/SAM.Core.Grasshopper/Classes/GH_SAMVariableOutputParameterComponent.cs` (the
`TemplateParams(side)` internal reader), `Grasshopper/SAM.Core.Grasshopper/Modify/ExpandVariableParameters.cs`
(rewritten around the declared template, see "What was actually wrong" / "The fix" in the PR
description), `Grasshopper/SAM.Core.Grasshopper/Modify/UpdateComponent.cs` (wires
`ExpandVariableParameters` into the update lifecycle, between `AddObject` and
`RestoreConnections`); tests `Grasshopper/SAM.Core.Grasshopper.Tests.Fixtures/TestVariableOutputComponent.cs`,
`Grasshopper/SAM.Core.Grasshopper.Tests/VariableOutputUpdateTests.cs`.

### Validation

```
SAM.Core.Grasshopper.Tests (Release)     45 passed / 0 failed / 0 skipped
SAM.sln (Release)                        0 errors, all 21 projects
SAM/SAM.Tests (Release)                  2017 passed / 0 failed
git diff --check                        clean
```

### Exact next step

Human final review of PR #103 (already updated: HOLD warning removed, harness and both defects
recorded). Merge only after that review; do not merge automatically.

## Previous (2026-09-05, latest): Part O closeout merged, review findings addressed

**Status: merged, plus one open branch for the review findings. One item HOLD.**

### What is on `sow/2026-Q3`

| PR | subject | merge commit |
|---|---|---|
| SAM_UI #89 | Part O Simulate defaults / minimum-click | `6fee4ecf2da709aa4bab132b62375e4e34104979` |
| SAM #102 | re-isolation / re-cutting correctness | `f982880ac5649790a625603fab53483c8d9e639e` |
| SAM #104 | catalogue identity - closed as intentionally unsupported | `d5a2b07a537708cfe73e11c56cfc33c1f4dbc66c` |
| SAM_UI #90 | the 2B resume decision record | `149cb951ffd3f62bb309913146ebeee95fec775d` |

Verified on the merged state, with `SAM.sln` rebuilt in Release first so the deployed SAM `build` folder
that `SAM_UI` and the TM59 suite compile against carried the merged assemblies:

```
SAM.sln / SAM_UI.sln (Release)          0 errors
SAM.Tests                               1993 passed / 0 failed / 0 skipped
SAM.Analytical.UI.WPF.Tests              554 passed / 0 failed / 0 skipped
SAM.Analytical.Tas.TM59.Tests             690 passed / 0 failed / 0 skipped
Grasshopper projects (6, Release)       0 errors; 0 duplicate component GUIDs
git diff --check                        clean in all four repositories
```

### HOLD - SAM #103, the Grasshopper variable-output updater

**Resolved 2026-09-07 - see the entry at the top of this file.** The Rhino-enabled run this section
called for is done (45/45 passing, 0 skipped), and it surfaced two real defects in
`CopyPersistentData` beyond the original nine regressions. HOLD lifted; ready for human review.

### This branch - the review findings

An automated review raised six findings across #102, #103 and #104. Three were acted on; the rest are
recorded here with the evidence, so they are not re-raised.

1. **P1, acted on.** `AdjacencyCluster.Filter` is public and `SAMAnalytical.FilterBySpaces` calls it
   directly: it marks the cut but strips nothing, because taking a person's openings off is not what a
   geometric filter is for. A model arriving from that component therefore carries cuts that still have
   their doors, and `IsolateSpaces` counted such a panel as a cut and then skipped the aperture cleanup -
   the disclosure would have said the interface was treated as adiabatic while the derived model still
   opened onto the omitted space, and the conversion would have exported that door as an opening to
   outside. Reproduced before fixing. A carried cut now gets exactly the same treatment as a fresh one.
   `CutFromFilterBySpaces_StillLosesItsApertures` and `AuthoredAdiabaticWall_KeepsItsAperturesThroughAReIsolation`.
2. **P2, acted on.** `TheModelStoresTheProductIdentityAndNoCapacity` asserted a null capacity by handing
   the lookup a null catalogue - which that lookup answers null to whatever the model stores, so the test
   would have kept passing if capacity ever began to be persisted on the unit. It now reads the unit's own
   serialized state.
3. **P1 x3, acted on.** This file was not updated by #102, #103 or #104. It is now.
4. **P1, already fixed before merge.** Re-typing a constructionless air panel: commit `e2bfcfb7`, in #102's
   merged head, only replaces a construction where `Query.DefaultConstruction` returns a real one, so an
   unconfigured default library no longer strips a panel's own fabric. The review read an earlier commit.

Validation on this branch: `SAM.Tests` **1995 passed / 0 failed / 0 skipped**, `SAM.sln` 0 errors,
`git diff --check` clean.

### Exact next step

Manual Part O UI acceptance over Iterations 1a / 1b / 2 / 2B, against a Rhino/TAS-enabled machine. Do not
declare the iterations frozen until it has passed. Do not start Iteration 3.

## Latest (2026-09-05, later): large-model lookup closeout - PF3-PF7 and `SetSpaceDesignFlowRate`

**Status: root-caused, implemented, tested and measured. Not merged.**

The pre-Iteration-3 scaling list, closed. Nothing here changes an engineering result, Part O semantics, a
TM59 criterion, TAS simulation behaviour or the Iteration 3 architecture: every change replaces *how* an
object is found with a cheaper way of finding **the same object**, and every one of them is pinned against
the search it replaced.

### The one defect, in six places

`AdjacencyCluster.GetSpaces()` and `GetZones()` **rebuild** their whole list from the relation cluster on
every call, and `AnalyticalModel.AdjacencyCluster` hands out a **new shallow copy of the entire cluster** on
every read. Asked once, all three are nothing. Asked inside a loop over the model's rooms, its zones or its
dwellings - which is what every one of the sites below did - they are quadratic in the model.

| item | site | genuine? |
|---|---|---|
| PF3 | `SAM_UI` `Query.PartOOptimisationTargets`, `Modify.PartialAssessment` | **yes** |
| PF4 | `TM59AssessmentCalculator.Spaces`, `OverheatingScenarioMap.Add`, `SAM_UI` `Query.PartODwellingSpaceGuids` | **yes** |
| PF5 | `Modify.RealizePartFVentilationTerminals` | **already solved** by PR #99; one redundant relation read remained and is now gone |
| PF6 | `Query.PartFTransferAirSpaces` | **already solved** by PR #99; nothing left to do |
| PF7 | `TMOverheatingCalculator.Calculate_TM59` / `Calculate_TM52` | **yes, and the largest** |
| - | `Modify.SetSpaceDesignFlowRate` | **yes** - deliberately excluded from PR #99 as a write path |

### PF7 - the largest, and the one nobody had looked at

Both `TMOverheatingCalculator.Calculate_TM59` and `Calculate_TM52` resolve every room they are handed
against the model before reading it. That resolution is correct and is not removed: the caller holds the
simulation space instances `SimulationSpaceMap` retained, and those predate
`TM59AssessmentCalculator.RestoreDesignInternalConditions`, so the instance the model now holds is the one
carrying the restored design internal condition. Classifying the caller's instance picks the wrong TM59
result type.

It was `GetSpaces().Find(...)`, **inside** the loop over the rooms. The TM52 path went through
`AnalyticalModel.GetSpaces()`, which additionally **copies every space in the model** - so assessing 5,000
rooms built 5,000 space lists and made 25,000,000 `Space` copies before a single hourly value was read.

Replaced by one `Dictionary<Guid, Space>` per call, built from the single `GetSpaces()` the loop used to
make per room and dropped when the call returns. Identity only; first occurrence wins, because `List.Find`
did.

### PF4 - the zone lookups, and a whole-cluster copy per dwelling

`OverheatingScenarioMap.Add` resolved its scenario's design zone with `GetZones().Find(...)` and read that
zone's spaces off `analyticalModel_Design.AdjacencyCluster` - **once per scenario**, and there is one
scenario per dwelling. `TM59AssessmentCalculator.Spaces` did the same per requested zone, and de-duplicated
the rooms it gathered with a linear `Find` over everything gathered so far.

Both now build one zone index and hold one cluster reference for the traversal, and the de-duplication is a
`HashSet<Guid>` beside the list. The cluster copy is shallow, so a hoisted one shares every `Zone` and
`Space` instance with the model - there is nothing for it to disagree with. `OverheatingScenarioMap` no
longer holds the design model at all, because everything it read off it is resolved once in the constructor.

### PF3 - the failing-space and dwelling-zone lookups

`PartOOptimisationTargets` rebuilt the model's space list once per **failing** room; `PartialAssessment`
once per unassessed room; `PartODwellingSpaceGuids` walked the whole zone list once per **requested
dwelling**. All three now ask `AdjacencyCluster.GetObject<Space>(guid)` / `GetObject<Zone>(guid)`, which is
a lookup in the cluster's live object dictionary.

### PF5 and PF6 - already solved, and said so

PR #99 removed the quadratic space resolution from both. What remained in
`RealizePartFVentilationTerminals` was a third relation read per room: pass 1 re-linked terminals, then the
space's terminals were **re-read** so pass 2 would see the new references. `AddObject` replaces the terminal
stored under that guid, so the re-read returned the same list with the same element swapped - it is now
substituted in place, and the read is gone. `PartFTransferAirSpaces` needed nothing.

### `Modify.SetSpaceDesignFlowRate` - a write path, and why the O(1) authority is the *only* safe answer

PR #99 excluded it deliberately: it replaces the room's terminals, so a `PartFIndex` snapshot shared across
a loop of writes would compute a later write's proportional split from duties an earlier one had already
replaced. That reasoning is unchanged and no snapshot was introduced.

`AdjacencyCluster.GetObject<Space>(guid)` is not a snapshot. It reads the cluster's live object dictionary
on every call and therefore sees every write made before it, exactly as re-running `GetSpaces().Find` did.
The requirement is then read with the internal `Query.PartFRequiredFlowRate_Lps(Space, FlowClassification)`
- the shared reader the public one-space query and `PartFIndex` both end at - over the instance just
resolved, instead of resolving that same instance a second time through the whole model. **Two `O(model)`
walks per write became one `O(1)` lookup.** The rate rule is untouched; the regulatory floor is still read
live, which `TheRegulatoryFloor_IsReadFromTheModelAsItStandsAtEachWrite` pins by recalculating Part F
between two writes.

### Equivalence, proved before anything was changed

`GetObject<T>(guid)` answers what `GetXs().Find(x => x.Guid == guid)` answers, because `GetXs()` is that
same object dictionary flattened into a list and the guid an object is stored under is the guid the `Find`
compared. Both go through the same `AdjacencyCluster.IsValid(Type)` gate and the same
`type.IsAssignableFrom` filter. Asserted rather than argued, **by reference**, over every space and every
zone of models of 100 / 500 / 1,000 / 5,000 spaces, plus an unknown guid, `Guid.Empty` and a guid belonging
to an object of the other type - `TheO1Authority_ReturnsTheSameInstanceTheLinearSearchReturns` and
`TheO1Authority_AnswersNullForExactlyWhatTheLinearSearchAnswersNullFor`.

### Operation counts, before and after (n rooms, z zones, d dwellings, k rooms gathered)

| site | before | after |
|---|---|---|
| `Calculate_TM59` | n space-list rebuilds, n linear scans | 1 rebuild, n hash lookups |
| `Calculate_TM52` | n rebuilds **and n x n `Space` copies** | 1 rebuild, n copies, n hash lookups |
| `TM59AssessmentCalculator.Spaces` | z zone-list rebuilds, z cluster copies, ~k x k / 2 de-dup comparisons | 1 rebuild, 1 cluster copy, k hash inserts |
| `OverheatingScenarioMap` | d zone-list rebuilds, d cluster copies | 1 rebuild, 1 cluster copy |
| `SetSpaceDesignFlowRate` (per call) | 2 space-list rebuilds, 2 linear scans | 1 hash lookup |
| `RealizePartFVentilationTerminals` (per room) | 3 relation reads | 2 relation reads |

### Scaling evidence (structural; allocated bytes, no timing thresholds)

Doubling ratios from `GC.GetAllocatedBytesForCurrentThread`. Linear work sits at 2, quadratic at 4; every
test asserts only `< 2.6`.

| measurement | after | the code it replaced |
|---|---|---|
| whole-model TM59 assessment | x1.79, x1.88 | x3.94, x3.97 |
| whole-model TM52 assessment | x1.75, x1.86 | (same shape) |
| `SetSpaceDesignFlowRate` write sweep | x2.00, x2.00 | x3.34, x3.61 |
| `OverheatingScenarioMap` construction | x2.05, x2.00 | x4.12, x3.90 |
| TM59 assessment scope resolution | x2.06, x1.95 | x4.12, x3.90 |

Local wall clock, reported by `[Trait("Category", "Benchmark")]` tests that assert nothing about time:

| operation | size | after | before |
|---|---|---|---|
| whole TM59 assessment | 5,000 rooms | **160.9 ms** | 1,749.4 ms *for the resolution alone* |
| `SetSpaceDesignFlowRate` sweep | 5,000 rooms | **36.5 ms** | 2,935.7 ms *for the two resolutions alone* |
| `OverheatingScenarioMap` | 1,000 dwellings / 5,000 spaces | **16.7 ms** | 2,952.9 ms |
| TM59 assessment scope | 1,000 dwellings / 5,000 spaces | **3.9 ms** | 3,264.9 ms |

### Files changed

`SAM.Analytical`: `Classes/TMOverheatingCalculator.cs` (one resolution index per call),
`Classes/TM59AssessmentCalculator.cs` (one zone index, one cluster, hashed de-duplication),
`Classes/OverheatingScenarioMap.cs` (one zone index and one cluster for the whole map; the design model is
no longer held), `Modify/SetSpaceDesignFlowRate.cs` (the O(1) authority and the already-resolved requirement
reader), `Modify/RealizePartFVentilationTerminals.cs` (the redundant relation read).

New tests: `SAM.Tests/TM59SpaceResolutionScalingTests.cs`, `SAM.Tests/SetSpaceDesignFlowRateLookupTests.cs`,
`SAM.Tests/PartOZoneLookupScalingTests.cs`.

### Validation

| suite | result |
| --- | --- |
| `SAM.Tests` | **1974 passed, 0 failed** (1937 pre-existing, all passing, + 37 new) |
| `SAM.Analytical.Tas.TM59.Tests` | **690 passed, 0 failed**, against the rebuilt `SAM.Analytical.dll` |
| `SAM.Analytical.UI.WPF.Tests` | **528 passed, 0 failed** (510 + 18 new) |

`SAM.sln` and `SAM_UI.sln` both build with 0 errors. `git diff --check` clean in both.

No licensed TAS run was made, and none is owed: no engineering result changes, and the TAS-side TM59 suite -
which builds against this assembly - is unchanged at 690 passing.

### Remaining risks

- The equivalence of `GetObject<T>(guid)` and `GetXs().Find(...)` would only diverge on a cluster holding
  two objects of different stored types under one guid. That is unreachable today (objects are stored per
  type name in a `Dictionary<Guid, X>`, and `Space` and `Zone` have no subtype in SAM) and is asserted by
  reference at every size rather than assumed.
- `Query.PartFRequiredFlowRate_Lps(AdjacencyCluster, Space, FlowClassification)` still resolves with
  `GetSpaces().Find`. Left alone on purpose: it is the compatibility oracle `PartFIndexTests` measures
  against, and every bulk caller now reaches it through `PartFIndex` or through an already-resolved space.

### Remaining pre-Iteration-3 list

Superseded - see the 2026-09-05 (latest) entry at the top of this file. Of the list that stood here, the
Part O defaults / minimum-click workflow, re-isolation and re-cutting, and catalogue identity drift are all
merged and closed; the Grasshopper variable-output updater is SAM #103 and is HOLD pending a Rhino-enabled
test run; final UI acceptance is the remaining step before Iterations 1-2 can be frozen.

## Previous (2026-09-05): Iteration 2B correctness closeout - the SAM half

**Status: implemented, tested and measured. Not merged.**

Four findings from an independent DeepSeek V4 Pro Max review of the Iteration 2B production path, plus two
defects found by Codex review of the first fix. F1/F3 and F4 are SAM's; F2 is SAM_UI's.

### F1 / F3 - the working-copy ownership rule

`new AnalyticalModel(analyticalModel)` and the `AdjacencyCluster` getter rebuild the cluster's dictionaries
but store **the same** `Space`, `Panel` and `Aperture` instances - `RelationCluster(RelationCluster<X>)`
copies `dictionary[key] = keyValuePair.Value`.

That is the right default. Every operation in this assembly writes by **same-guid replacement**, and
`Modify.EvaluateTargetedDesignAirFlows` states exactly that rule at its own boundary and relies on it.

The TAS conversion does not obey it: `SAM.Analytical.Tas.Modify.UpdateIds` reads the live objects out of the
cluster and stamps zone and building-element identity onto their parameter sets **in place**. So a caller
that took a copy in order to be free to mutate it - the TAS workflow does, and says so in its own comment -
was not isolated at all. On the optimisation path that caller is the retained last-valid design, so a round
that stamped it and then failed left a model disagreeing with its own persisted
`SimulationResultProvenance.Fingerprint_Model`.

`AnalyticalModel(AnalyticalModel, bool deepClone)` is the authority that establishes:

> A simulation or optimisation working model may be mutated freely, but no caller, retained last-valid
> model or previously completed run sharing ancestry with it can observe those mutations unless that
> working model is explicitly adopted.

It is built on the `AdjacencyCluster(AdjacencyCluster, bool deepClone)` constructor that was already here
rather than a second clone implementation. The shallow copy stays the default so no getter pays for it.

### F1 follow-up (Codex P2) - the deep clone was not deep

`Core.Query.Clone` resolves by reflection - an instance `Clone()`, else a single-argument constructor
accepting the type, else a parameterless one - and returns null when it finds none. **Constructors are not
inherited**, so a subclass of a type that has a copy constructor does not have one.

Seven types accepted by `AdjacencyCluster.IsValid` were in exactly that position. The null clone was handed
to `AddObject`, rejected, and the **original instance left in place** in the dictionary the shallow base
constructor had already filled - so the "deep" copy silently shared those objects with its source:

`ZoneSimulationResult`, `TM52Result`, `TM59Result`, `TM59CorridorResult`,
`TM59MechanicalVentilationResult`, `TM59NaturalVentilationResult`, `TM59NaturalVentilationBedroomResult`.

Each now has the same-type copy constructor its base already had. An exhaustive reflection audit over
**every** concrete type the cluster accepts (60 in this build) asserts a clone path exists for each, so a
type added later fails in a test rather than in a Part O run. And
`SAMObjectRelationCluster(..., deepClone: true)` now **throws**, naming the type, rather than falling back
to sharing: a declared deep clone may not quietly leave the source instance in place.

### F4 - a partial year is refused, not clipped

`TMOverheatingCalculator.Collect` bounded its walk by the **shorter** of the two hourly series. That stopped
a truncated result throwing out of the whole run - kept - but it also assessed the room over the hours the
two happened to share and reported the answer as the room's verdict.

A space whose series are absent, empty or of unequal length is now refused with a reason, on
`HourlySeriesRefusals` / `SpaceGuids_HourlySeriesRefused`, carried through `TM59AssessmentResult`.

Whether an equal-length pair is long enough to be a **year** is a question about the run rather than the
calculation, so the calculator asks it only where a caller states the answer - `HourCount_Expected`, default
`0`. Every existing caller (the Grasshopper components, a summer-only TM52 window) is unchanged.

### F4 follow-up (Codex P1) - length is not evidence

Where a full year **is** requested, every hour of both series must be a finite number. `Collect` skips a
temperature it cannot read and treats an unreadable occupancy value as an unoccupied hour; for Part O that
is silently wrong, because the criteria are counts - a year with unreadable occupancy is assessed over fewer
occupied hours than the building has, against a proportionally smaller allowance, and reads as a normal
pass. **Zero occupancy is a value; an hour that states nothing is not.**

Found while testing it: a `JsonArray` *will* hold a NaN or an infinity, and reading one back **throws**
`ArgumentException` out of `System.Text.Json`. So a single unrepresentable hour anywhere in a building threw
out of `Calculate_TM59` and lost every space's assessment. The read is now guarded; one bad hour costs at
most its own room.

`WeatherData.WeatherYears` returns null rather than throwing on a `WeatherData` built without years - the
guard its neighbour `WeatherYear.WeatherDays` already has, and what every caller already reads it as.

### Behaviour changes to pinned tests

`AResultantSeriesShorterThanTheOccupancySeries_IsAssessedOverTheSharedRange` asserted the clipping and is
replaced by `SeriesOfDifferentLengths_AreRefusedRatherThanAssessedOverTheSharedRange`, which keeps the half
that still holds - nothing throws, the other rooms survive - and reverses the half that was the defect.
`MissingRequiredSeries_ProducesNoAssessment` explicitly pinned the absence of a diagnostic as "separate
work"; this is that work.

### Changed files

- `SAM/SAM.Analytical/Classes/AnalyticalModel.cs` - the ownership constructor
- `SAM/SAM.Core/Classes/Relation/SAMObjectRelationCluster.cs` - deep clone fails rather than shares
- `SAM/SAM.Analytical/Classes/Result/ZoneSimulationResult.cs` and the six `Result/TM/TM5*.cs` - copy
  constructors
- `SAM/SAM.Analytical/Classes/TMOverheatingCalculator.cs` - series refusals, `HourCount_Expected`, the
  guarded value read
- `SAM/SAM.Analytical/Classes/TM59AssessmentResult.cs`, `TM59AssessmentCalculator.cs` - carrying the
  refusals through
- `SAM/SAM.Weather/Classes/WeatherData.cs` - the null guard
- `SAM/SAM.Tests/AnalyticalModelWorkingCopyTests.cs`, `AdjacencyClusterDeepCloneTests.cs`,
  `TMOverheatingCalculatorTests.cs` - the regressions

### Merge order

1. **SAM-BIM/SAM#100** - the ownership constructor, the deep-clone completeness fix, and the TM59 series
   rules. Nothing else compiles without it.
2. **SAM-BIM/SAM_Tas#48** - the workflow's owned-model overload. Needs #100.
3. **SAM-BIM/SAM_UI#87** - the Part O boundaries, the capacity-envelope name, and the full-year authority.
   Needs #100. Independent of #48 to compile; both are needed for the F1/F3 invariant to hold end to end.

`SAM_Systems` is untouched.

### Validation

| Suite | Result |
| --- | --- |
| `SAM.Tests` | 1934 passed, 0 failed |
| `SAM.Analytical.Tas.TM59.Tests` | 690 passed, 0 failed |
| `SAM.Analytical.UI.WPF.Tests` | 510 passed, 0 failed |

`SAM.sln`, `SAM_Tas.sln` (MSBuild - COM references) and `SAM_UI.sln` all build with 0 errors.
`git diff --check` is clean in all three repositories.

Deep-clone cost, measured Release on 5,000 spaces / 30,000 panels: **250.7 ms, 136.8 MB**, against
36.5 ms / 13.8 MB for the shallow copy. Paid **once** per Part O TAS run.

### Remaining risks and next task

- No licensed TAS run was made. Every invariant here is established by deterministic tests over production
  code; what is not covered is TAS's own behaviour, which none of these changes touch.
- `SAM.Weather.Query.RunningMeanDryBulbTemperatures` still throws on a weather year shorter than the one the
  running mean needs, so a TSD with a damaged weather record fails loudly rather than being refused with a
  diagnostic. Pre-existing, characterised by test, and a fix reaches wider than Part O.
- Next: the pre-Iteration-3 list is unchanged - PF3-PF7 and `SetSpaceDesignFlowRate` indexing, the Part O
  defaults / minimum-click audit, re-isolation, the Grasshopper variable-output updater, catalogue identity
  drift, final UI acceptance, then freeze Iterations 1-2.

## Previous (2026-09-04): Part F large-model scaling - `PartFIndex`

**Status: root-caused, implemented, tested and measured. Not merged.**

### Root cause

`Query.PartFRequiredFlowRate_Lps(AdjacencyCluster, Space, FlowClassification)` re-resolves the space it is
handed against the cluster before reading it - correctly, because the Part F application replaces spaces
wholesale. The resolution was:

```csharp
(adjacencyCluster.GetSpaces() ?? []).Find(x => x is not null && x.Guid == space.Guid) ?? space
```

`AdjacencyCluster.GetSpaces()` is `RelationCluster.GetObjects<Space>()`, which **rebuilds** the whole space
list on every call - four intermediate `List` allocations of size n, plus a reflective `Query.Type` resolve
per stored type name in `GetTypeNames` - and `Find` is then a linear search through it. Asked once that is
nothing; asked twice per room inside a loop over the model's rooms it is `O(n²)`.

Seven production sites did exactly that. Six were read-only loops and are fixed; one is a write path and was
deliberately left alone (below).

### Architecture: `SAM/SAM.Analytical/Classes/PartF/PartFIndex.cs`

An immutable, **request-scoped** snapshot built once per traversal from one `AdjacencyCluster`. It indexes
**identity only** - `Guid -> Space`, first occurrence winning exactly as `List.Find` does - and stores **no
rate, no `PartFSpaceData` and no derived engineering value at all**. Every rate is read live from the
resolved space on every call, so it is an index and not a cache; `PartFIndex_HoldsNoRate_...` pins that.

It is **not a second engineering authority**. The rate rule was extracted into one internal reader,
`Query.PartFRequiredFlowRate_Lps(Space space_Cluster, FlowClassification)`, which the public one-space query
and `PartFIndex.PartFRequiredFlowRate_Lps` both call once they have resolved the space. The public one-space
query is otherwise unchanged and remains the compatibility oracle.

Modelled on `PartFAirflowNetwork`: a plain class built from a cluster, deliberately not a serialised
`*Context` (`PartOIsolationContext`, `PartOPreparationContext` are records of what happened; this is a
lookup), hence `Index` rather than `Context`.

### Deliberately NOT migrated

`Modify.SetSpaceDesignFlowRate` still does two `GetSpaces().Find(...)` per call. It is a **write** path -
it replaces the space - so an index shared across a write loop would go stale, which is worse than the cost.
A per-call index would only halve a constant and leave it `O(n)` per call. The same reasoning kept
`EvaluateDwellingRound`'s Part F reads on the one-space query: `EvaluateTargetedDesignAirFlows` writes to
the candidate cluster per dwelling, so its snapshot covers the resolution pass only.

`AdjacencyCluster.GetObject<Space>(guid)` is already an `O(1)` dictionary lookup and would make the one-space
query cheap on its own. It was **not** used, because changing the oracle's own resolution mechanism is what
the equivalence tests are measuring against. Recorded as a possible follow-up simplification, not done here.

### Compatibility

Every answer is compared against the one-space query, exactly (`Assert.Equal` on `double?`, no tolerance),
over 50 / 200 / 500 / 1000 / 5000 space models. Pinned edge cases: null model (answers nothing, never falls
back to the caller's space), null space, empty model, a space not in the model (answered from the instance
handed in), a stale copy of a model space (answered from the model's instance), an unsized space (null, not
zero), zero, negative, NaN (null), a direction that is neither supply nor extract (null), a subdivided room,
equivalent dwelling zones sharing rooms, a zone naming a removed space, and an empty scope.

**Approved Document F has no space multiplier anywhere in SAM** - `grep -i multiplier` over `SAM/` returns
only comments and unrelated test prose. There is nothing to preserve and the index introduces none.

### Scaling evidence (structural, no timing thresholds)

Allocated bytes per bulk aggregation, `GC.GetAllocatedBytesForCurrentThread`:

| spaces | via `PartFIndex` | via one-space query |
|---|---|---|
| 400 | 1,716,392 | 25,204,888 |
| 800 | 3,434,856 | 94,988,928 |
| 1600 | 6,877,672 | 368,045,120 |

Doubling ratio: index **x2.00, x2.00** (linear); one-space query **x3.77, x3.87** (quadratic). The test
asserts only that the index ratio is `< 2.6`.

Operation counts, asserted exactly: one snapshot built, `2n` requirement calls and `3n` identity resolutions
for `n` rooms in scope - in SAM and again in SAM_UI against one `PartOWorkflowInspection.Inspect`.

Local timings, reported by a `[Trait("Category", "Benchmark")]` test that asserts nothing about time:

| spaces | one-space query | `PartFIndex` |
|---|---|---|
| 50 | 0.8 ms | 0.4 ms |
| 200 | 8.0 ms | 1.7 ms |
| 500 | 27.5 ms | 3.9 ms |
| 1000 | 98.4 ms | 8.6 ms |
| 5000 | 2221.5 ms | 46.3 ms |

### Files changed

`SAM`: new `SAM.Analytical/Classes/PartF/PartFIndex.cs`; `Query/PartFRequiredFlowRate.cs` (shared reader
extracted), `Query/PartFTransferAirSpaces.cs`, `Modify/ApplyTargetedDesignAirFlow.cs`,
`Modify/EvaluateTargetedDesignAirFlows.cs`, `Modify/EvaluateDesignAirFlowCapacityEnvelope.cs`,
`Modify/AddPartOBaseMVHRSystem.cs`, `Modify/RealizePartFVentilationTerminals.cs`; new tests
`SAM.Tests/PartFIndexTests.cs`, `SAM.Tests/PartFIndexScalingTests.cs`.

`SAM_UI`: `SAM_UI/SAM.Analytical.UI/Classes/PartO/PartOWorkflowInspection.cs` (one snapshot per inspection,
scope resolution delegated to SAM), `WPF/SAM.Analytical.UI.WPF/Modify/OptimisePartOTM59.cs`; new test
`WPF/SAM.Analytical.UI.WPF.Tests/PartOWorkflowInspectionScalingTests.cs`. **No cache was added to
`PartOWorkflowWindow`, `PartOWorkflowInspection` or anywhere in WPF**, and the workflow state semantics
from PR #85 are untouched.

### Validation

`SAM.sln` builds, 0 errors. `SAM.Tests` **1902 passed / 0 failed** (1867 pre-existing, all passing, + 35
new). `SAM_UI.sln` builds against the rebuilt `SAM.Analytical.dll`, 0 errors.
`SAM.Analytical.UI.WPF.Tests` **498 passed / 0 failed** (491 + 7 new). `git diff --check` clean in both.

Licensed TAS was not run: results are bit-identical to the oracle, so there is no engineering change for a
simulation to validate.

### Unresolved / follow-ups

Unchanged from before: catalogue identity/content drift (investigate only when a wrong-result path is
demonstrated), the Part O re-isolation/re-cutting limitation, and the Grasshopper variable-output updater.
New: `SetSpaceDesignFlowRate` remains `O(model)` per write call (see "Deliberately NOT migrated").

### Recommended next step

Review SAM-BIM/SAM#99, then SAM-BIM/SAM_UI#86. Merge SAM first. Do not merge automatically.

## Previous (2026-09-03, later): a genuine roof must not come out of isolation adiabatic

**Status: reproduced, root-caused, fixed and tested.** Reported from manual testing of Flat 1 in isolated
mode: a genuine roof reached TAS as an adiabatic surface.

### The truth table

An isolation cut is one thing: **the source panel separated two thermal spaces and exactly one side is
retained**. `AdjacencyCluster.External(Panel)` and `.Internal(Panel)` count adjacent SPACES - `== 1` and
`> 1` respectively - and classify no envelope at all:

| source | derived | meaning | adiabatic |
|---|---|---|---|
| > 1 space | 1 space | the isolation cut | **yes** |
| 1 space | 1 space | already a boundary to the outside | no |
| > 1 space | > 1 space | both sides retained | no |

The derived cluster alone cannot tell row 1 from row 2 - both leave one adjacent space - so the decision
has to consult the source adjacency, which is what makes the first row's second condition load-bearing.

### The predicate in `Modify.IsolateSpaces` is CORRECT

`!result.External(panel) || !adjacencyCluster.Internal(panel)` is exactly row 1 of that table: continue
(not a cut) unless the derived cluster has one adjacent space and the source has more than one. It was
put under suspicion and it is not the defect. It is now pinned by
`EveryPanelTurnedAdiabatic_SeparatedASelectedSpaceFromAnExcludedOneInTheSource`.

### The defect is one level down, in `AdjacencyCluster.Filter`

`Filter(spaces, setAdiabatic: true)` - which `IsolateSpaces` calls, and which is the production authority
for the flag - had two branches sharing one `SetValue(PanelParameter.Adiabatic, true)` at the bottom:

- the **non-`Air`** branch required `source > 1` - correct, row 1;
- the **`Air`** branch required `source == 1` - the OPPOSITE condition, row 2 - re-typed the panel from its
  normal (an upward-facing one becomes a `Roof`) and gave it that type's real construction, **and then fell
  through to the same adiabatic assignment.**

So a genuine external boundary modelled as an `Air` panel - the shape a roof arrives in from a model whose
top surface was left unzoned, which is Flat 1's roof - was correctly re-typed to `Roof`, correctly given a
roof construction, and then wrongly marked adiabatic. No solar gain, no heat loss, and a construction
statement contradicting its own flag.

Nothing was wrong with the classification of `Roof`, `FloorExposed`, `SlabOnGrade`, `WallExternal`,
`CurtainWall` or `UndergroundWall` panels that arrive already typed - those all pass unchanged, before and
after. The defect reached only panels typed `Air` in the source.

### The fix

Three lines: the `Air` branch adds its re-typed panel to the result and `continue`s, instead of falling
through to the adiabatic assignment. The `else` is de-nested since the branch now returns. Nothing else
changes - the re-typing, the construction, the geometry, the apertures and the source model are all as
they were.

### Files changed

- `SAM/SAM.Analytical/Classes/AdjacencyCluster.cs` - the `Air` branch no longer falls through, plus the
  truth table as a comment where the decision is made.
- `SAM/SAM.Tests/PartOIsolationEnvelopeTests.cs` - **new**, 19 tests.

### Tests

`PartOIsolationTests`' fixture carries external walls and internal partitions only, which is why this was
never caught: it has no roof, no exposed floor, no ground floor and no `Air` panel. The new fixture adds
them. Coverage: selected-to-excluded → adiabatic; selected-to-selected → stays internal; external wall,
roof, exposed floor, ground floor, curtain wall and underground wall → not adiabatic, and keeping type,
construction and area; an `Air` panel re-typed but not made adiabatic; external apertures kept; cut
apertures removed; an authored adiabatic panel keeping its state and not counted as a cut; selecting every
space turning nothing adiabatic; the source model unchanged; the before/after envelope classification; and
the universal invariant that every panel turned adiabatic separated a selected from an excluded space in
the source.

**Load-bearing:** against the unmodified pre-fix code the `Air` test fails with "It was external in the
source model, so isolation cut nothing here and it must keep its external boundary", and the envelope
classification test fails with it.

### Validation

- `SAM.Tests` - **1850 passed** (1831 baseline + 19).
- `SAM_Tas` **684 passed**, `SAM_UI` **411 passed**, both against the rebuilt `SAM.Analytical.dll`.

### Not performed

**No run on the real multi-flat model, and no licensed TAS run.** The before/after envelope classification
is asserted on the fixture, not on Flat 1 itself. Re-running Flat 1 isolation and inspecting its envelope
classification is still outstanding and is the acceptance step that closes this out.

## Previous (2026-09-03, latest): the isolation cut is an adjacency change, not a flag

**Status: implemented and tested. One production line, plus the coverage two review comments asked
for.** Raised by Copilot on PR #92; real, and reachable from an ordinary project model.

### The defect

`Modify.IsolateSpaces` decided which derived panels were this run's isolation cut by reading
`PanelParameter.Adiabatic` back off them - the flag `AdjacencyCluster.Filter` had just written. Every
panel so identified had its apertures removed and was counted in the disclosure note.

The flag is not this run's evidence. Three other places write it, and all three write it on ordinary
surfaces of an ordinary model:

- `SAM.Analytical.Tas.Convert.ToSAM` sets it on every surface a TBD calls adiabatic;
- `SAM.Analytical.gbXML.Convert.ToSAM` sets it wherever a gbXML surface names an adjacent space the file
  does not contain;
- the `SAMAnalyticalSetAdiabatic` Grasshopper component sets it because a person asked for it.

`Filter` carries a panel's parameters into the derived cluster with it, so every one of those arrived at
the cut loop already flagged. A wall that was adiabatic in the whole building was therefore read as a
cut: **its apertures were removed from the isolated model**, and it was counted in the report's
disclosure note as an interface to an excluded space it does not touch. The code's own comment said the
opposite was intended - that such a surface "keeps whatever apertures it had, exactly as it would in a
whole building run" - so this is the code not doing what it says. A TBD or gbXML import is the normal way
a real project model arrives, which is what makes it reachable rather than theoretical.

### The correction

The cut is the **adjacency change this run made**, which is what the class documentation already called
it: two spaces in the source cluster and one in the derived one. Both clusters are in hand, so it is
asked of them directly and of no flag:

```
if (panel is null || !result.External(panel) || !adjacencyCluster.Internal(panel))
```

This is the same predicate `Filter` uses to write the flag, so nothing about which panels become
adiabatic changes - only which ones this method then treats as cuts. It stays right where the two
coincide: a surface that was already adiabatic **and** divides a selected space from an excluded one is
a cut, and does lose its apertures, because in the derived model it has nothing left to open onto.

### Files

- `SAM/SAM.Analytical/Modify/IsolateSpaces.cs` - the cut predicate.
- `SAM/SAM.Tests/PartOIsolationTests.cs` - 3 new tests: an already-adiabatic partition between two
  selected spaces keeps its door and is not counted; one that is also the cut still loses its door; an
  already-adiabatic external wall is not a cut and keeps its window.
- `SAM/SAM.Tests/TM59AssessmentReportTests.cs` - 2 new tests (4 cases) for the report scope line, which
  Copilot noted was uncovered: an isolated scope is printed above the verdicts, and a report stating no
  scope falls back to `WHOLE BUILDING` rather than to silence.

### Validation

Full `SAM.Tests` **1831 passed**, 0 failed (1824 before, +7). `SAM.sln` builds clean. Rebuilt
`SAM.Analytical.dll` re-validated downstream: `SAM_UI` **410 passed** and `SAM_UI.sln` clean,
`SAM_Tas` **669 passed** and `SAM_Tas.sln` clean.

### Not addressed, deliberately

- **The optional `isolate` parameter on `PreparePartOIteration` (Copilot, PR #92).** Adding it changes
  the CLR signature, so an assembly compiled against the five-parameter version would need recompiling.
  Not preserved as a separate overload: this repository has twice added an optional parameter to this
  same method in place (`0c8a04eb`, `41a02d4e`), the method is Part O internal with no consumer outside
  this family, and every SAM assembly is built and released together.
- **The second `-ISO-` suffix (Copilot, SAM_UI PR #82).** Re-preparing an isolated scope from a model
  adopted from an optimisation round - named `<project>-ISO-<token>-OptNN` - appends the suffix again,
  because the idempotence check only recognises it as the final segment. Naming only, never provenance,
  and it can grow at most once; the Iteration 2B rounds named in that comment call
  `Analytical.Modify.PreparePartOIteration` directly and never reach `Query.ProjectName_Isolated`.
  Recorded for a decision rather than fixed under a review round.
- **The six MVHR plant zones** in the full-building Opt TBDs, unchanged from the entry below.

## Latest (2026-09-03, later): two model-generation defects TAS refuses a model for

**Status: implemented, tested, and validated on the licensed acceptance model. Flat 1 in isolation now
completes a full-year TAS simulation.** Found by running the real-project acceptance case for the
isolation work above; the isolated run wrote a 22 KB stub of a results file and an error log saying
only `Simulation Failed`.

### 1. Transposed humidistat limits - predates Part O and predates isolation

`Modify.AddAirMovementObjects` gave every `AirHandlingUnitAirMovement` **Humidification 100%** and
**Dehumidification 0%**. Those two profiles become the humidistat on the unit's generated TAS plant zone:
`SAM_Tas Modify.UpdateIZAMs` writes Humidification to the humidity **lower** limit (`Profiles.ticHLL`) and
Dehumidification to the **upper** (`ticHUL`). So the zone was asked to hold its air above 100% and below 0%
relative humidity at once, and TAS's pre-simulation check refused it:

> `Internal Condition 'MVHR-01' humidistat has overlapping limits.`

Read out of the failing TBD: `ticHLL value=100 setback=0`, `ticHUL value=0 setback=100` - TAS's own
correct **setbacks** still beside them, which is what a transposition of the *values* alone looks like.

Corrected to lower 0% / upper 100%, which is the SAM convention for "no humidity control" - verbatim what
the shipped profile library states as *No Humidification* (0) and *No Dehumidification* (100), and the pair
TAS's own new-internal-condition defaults carry. **No heating, cooling, humidification or dehumidification
control was invented to satisfy TAS**; the limits are inert.

**It predates both.** The pair has been in `AddAirMovementObjects` since 2024-01-25 (`3f724001`), on the
general path every model with an air handling unit goes through. The acceptance project's own `.sam`,
saved by ordinary **non-isolated** runs, carries lower 100 / upper 0 on MVHR-01, **-02 and -03**, and so
does a full-building `Opt` round's TBD.

### 2. …but that was not the isolated run's blocker

The same full-building TBD, with the same transposed humidistat, **simulated a full year** (18.5 MB TSD).
The overlap is a genuine invalid state TAS reports and was never fatal to the solver. The real blocker:

`UpdateIZAMs` writes the plant zone one air movement per room the unit **supplies**, plus one
`IZAM <unit> FROM OUTSIDE` bringing in what it therefore has to draw - sized by `Query.AirFlow`, which
reads the deliveries **related** to the unit. Where that resolves nothing the intake is not written, and
the zone delivers the dwelling's whole supply while gaining nothing. TAS refuses to simulate a zone whose
air movements do not balance. Measured on the isolated Flat 1: **MVHR-01 out 36.3 l/s, in 0**.

`Modify.IsolateSpaces` depended on two lookups that **cannot work** on an `AdjacencyCluster`.
`AdjacencyCluster.IsValid(Type)` asks whether a type is assignable **to** one of the analytical families it
admits, which `IJSAMObject` - broader than all of them - is not; and *both*
`RelationCluster.GetObjects(Type)` and `RelationCluster.GetObject(Type, Guid)` gate on it. Verified on this
model: `GetObjects<IJSAMObject>()` → **null**, `GetObjects()` → **641**.

- **`RestoreRelations` was a complete no-op.** It opened with `result.GetObjects<IJSAMObject>()` and
  returned at its first line, so it restored **nothing** - not an air movement to its unit, not a terminal
  to its system. Now asks the ungated `GetObjects()`. That gate is right for *admitting* an object and
  wrong for asking what is in there.
- **`CarryAirHandlingUnits`' existence guard was unsatisfiable** (`GetObject<IJSAMObject>(guid)` is null
  for every guid the cluster holds), so no unit relation was restored either. The two are now split by
  responsibility and **ordered**: the carry adds objects, `RestoreRelations` runs after it and is the
  single authority on relations.
- **The unit's own exhaust was never carried.** It is a `SpaceAirMovement` related to *no space at all*.
  Plant-side movements of a carried unit now come across.

### 3. Cross-cut air movement detection (raised by Copilot on PR #92)

`Refusals_AirMovementScope` inspected only the relation graph, but `AddPartFTransferAirMovements` relates
a transfer to the **downstream** space only - relating it to both would have the TBD writer write the
dwelling two identical inter-zone air movements - so the upstream space exists on the object as a `From`
reference and nowhere else. Two cases passed silently: *excluded → selected* was carried with a dangling
`From`; *selected → excluded* was neither refused nor carried, so the selected room passed air to a room
that is not in the model. New `Spaces_AirMovement` reads both ends, references included, and both refuse.

### 4. Both defects are now SAM Check rules

`Create.Log` - the authority behind `SAMAnalytical.Check` and the Check command - gains:

- an **`InternalCondition`** whose humidification (lower) limit is above its dehumidification (upper) limit
  at any index the two profiles share → **Error**. Read index by index, not by comparing one profile's
  maximum against the other's minimum, because two schedules can each be higher than the other at
  different hours without ever overlapping;
- an **`AirHandlingUnitAirMovement`** carrying that same invalid pair → **Error**;
- an `AirHandlingUnitAirMovement` whose unit **supplies a movement the unit is not related to** → **Error**:
  the model says it delivers, the relation graph does not, so no intake can be sized. Asked that way round
  deliberately, so a legitimate **extract-only** unit - which delivers to no room and balances its
  extract against its own exhaust with no outside intake - is not reported (Codex P2 on PR #92). New
  `Query.SpaceAirMovement_Delivered` answers "does the model say this unit delivers" over the whole
  cluster;
- an `AirHandlingUnitAirMovement` related to **no unit** → **Warning**, not an Error: nothing generates a
  plant zone from it, so it is inert.

Reached from `Log(AdjacencyCluster)` **before** the space and panel checks, because a unit's air movement
is related to the unit and to no space - which is why nothing that walks the spaces could ever find it.

Not Part O rules: an invalid model is invalid whoever asks.

### Important decisions and assumptions

- `AdjacencyCluster.IsValid(Type)` is **not** changed. It correctly restricts what may be *added*; the
  defect was using a type-gated single-object lookup as an existence test. The type-agnostic
  `GetTypeName(Guid)` is what `CarryAirHandlingUnits` now asks.
- The humidistat rule reports **only** a lower limit above an upper limit. NaN is treated as an unstated
  hour, not an overlap.
- The intake rule does not attempt to reproduce `UpdateIZAMs`' whole plant-zone balance in
  `SAM.Analytical` - that would duplicate SAM_Tas logic in the wrong layer. It reports one deterministic
  disagreement between the model and its own relation graph.

### Files changed

- `SAM/SAM.Analytical/Modify/AddAirMovementObjects.cs` - the transposed humidity pair.
- `SAM/SAM.Analytical/Modify/IsolateSpaces.cs` - `RestoreRelations` enumeration, `CarryAirHandlingUnits`
  split/ordering and plant-side carry, `Spaces_AirMovement` for cross-cut detection.
- `SAM/SAM.Analytical/Create/Log.cs` - the humidistat rule, the intake rule, the
  `Log(AirHandlingUnitAirMovement, AdjacencyCluster)` overload and its wiring into `Log(AdjacencyCluster)`.
- `SAM/SAM.Analytical/Query/SpaceAirMovement_Delivered.cs` - new.
- `SAM/SAM.Tests/PartOHumidistatTests.cs` - new, 16 tests.
- `SAM/SAM.Tests/PartOPlantZoneIntakeTests.cs` - new, 18 tests.
- `SAM/SAM.Tests/PartOIterationPreparationTests.cs` - 4 prepared-model regressions.

### Tests, builds and validation

- Full `SAM.Tests` **1824 passed**, 0 failed. `SAM.sln` builds clean (VS 18 MSBuild).
- **Licensed TAS acceptance run**, Flat 1 isolated out of
  `000000_SAM_AnalyticalModel-It1a-futureZ1.sam`, full year 1-365 with sizing:

| | before | after |
| --- | --- | --- |
| TSD | 22,414 bytes (sizing stub) | **4,691,076 bytes** |
| `_error_log.txt` | `Simulation Failed` | *none written* |
| TBD humidistat | `ticHLL 100` / `ticHUL 0` | `ticHLL 0` / `ticHUL 100` |
| MVHR-01 zone balance | in 0, out 0.0363 m³/s | in 0.0363, out 0.0363, **net 0** |
| workflow | stopped after Sizing | Simulating Model → Adding Results → Updating Design Loads → Saving Model |
| `SAMAnalytical.Check` on the derived model | 3 errors on the source | **0 errors, 0 warnings** |

  Only Flat 1's two spaces are simulated; the source `.sam` is byte-identical afterwards, timestamp
  included.

### Unresolved issues, risks and blockers

- The intentional `Zone 'MVHR-01' is missing internal conditions on some daytypes` warning remains, by
  design. Pinned in **SAM-BIM/SAM_Tas#46**; nothing on the Part O path promotes it to an error.
- Passing `SAMAnalytical.Check` is **not** a guarantee that TAS can run - licensing, file I/O, the solver
  and weather data are all still ahead, and TAS's own check knows rules this one does not.
- The full-building `Opt` TBDs carry **six** MVHR plant zones (three with no internal conditions at all),
  i.e. `UpdateIZAMs` appears to accumulate duplicate plant zones across optimisation rounds. **Observed,
  not investigated, and not addressed here.** It does not stop TAS.
- Copilot's other PR #92 comments (optional-parameter binary compatibility on
  `PreparePartOIteration`; `TM59AssessmentReportFormatter` header coverage; the adiabatic-flag condition
  also matching a source-adiabatic envelope; `PartOIsolationTests` modelling a transfer with both
  relations) are **open and unaddressed**.

### Exact recommended next step

Wait for the re-requested Codex review on **SAM-BIM/SAM#92** (and SAM_UI#82, SAM_Tas#46), then decide on
the four open Copilot comments listed above. **Do not merge** - merge order is SAM #92, then SAM_UI #82,
then SAM_Tas #46 (order free).

---

## Latest (2026-09-03): running selected dwellings in isolation

**Status: implemented and tested. No Part O or Part F engineering semantics changed.**

A large residential model can be ~5,000 spaces. Assessing one flat should not require simulating all of
them. Isolation builds a **derived** model containing only the selected dwellings as thermal zones,
keeping the thermal boundaries and the solar context they need.

### 0. What the investigation found, before anything was written

`FilterBySpaces` exists only as the Grasshopper component
`SAMAnalyticalFilterBySpaces`; the production authority behind it is
**`AdjacencyCluster.Filter(IEnumerable<Space>, bool setAdiabatic = true)`**. It already does more than
expected, and none of it was rebuilt:

* returns a **new** cluster - the source is never mutated;
* identifies spaces by **guid** (`SAMObject`'s copy constructor carries the guid, so identity survives -
  which TAS zone mapping, TM59 result mapping and provenance all depend on);
* clones each selected space's related objects and relates them to that space;
* and classifies **every** panel by comparing its adjacency in the DERIVED cluster against its adjacency
  in the SOURCE - which is exactly the three-case matrix this feature needed:
  * *selected/selected* - two spaces in the derived cluster, left as the internal partition it is;
  * *selected/excluded* - one space derived, two in the source, marked `PanelParameter.Adiabatic`;
  * *selected/external* - one space in both, left completely alone (type, construction, apertures,
    orientation, exposure).

`Filter` had **no tests**. It now has them.

What `Filter` could not know, because it is a general geometric filter and these are questions about a
*simulation*, is what `Modify.IsolateSpaces` adds around it - see below.

**The adiabatic representation already exists end to end.** `PanelParameter.Adiabatic` is read by
`SAM_Tas Modify.UpdateAdiabatic`, which sets the matching TBD zone surface to
`SurfaceType.tbdNullLink`, and `WorkflowCalculator` already calls it ("Setting Adiabatic"). Nothing was
invented and `SAM_Tas` needed no change.

**The shading representation already exists too.** `PanelType.Shade` maps to gbXML
`surfaceTypeEnum.Shade`, the Part O route converts through gbXML into T3D and then TBD, and the gbXML
export writes a panel with no adjacent space perfectly well. So excluded external geometry reaches TAS as
shading through the path that already existed.

### 1. `Modify.IsolateSpaces` - the extraction

Built **on** `Filter`, not beside it. It adds only the four things a simulation needs and a geometric
filter cannot decide:

**Relations `Filter` does not rebuild.** `Filter` relates each carried object to the space it was carried
for. A simulation needs the rest of the graph - a terminal to its system, an air movement to its unit -
so every source relation whose *both* ends were carried is restored. Only carried objects are walked.

**The plant.** An `AirHandlingUnit` is related to no space at all: a `VentilationSystem` names it in
`VentilationSystemParameter.SupplyUnitName`. Nothing reachable from a space finds it, so `Filter` cannot
carry it, and without this step an isolated model would have systems with no unit and every duty and
equipment selection downstream would read as absent.

**The apertures on the cut.** Removed from the derived model. In the derived cluster a cut panel has one
adjacent space, so an aperture left on it would be exported as an **external** window - giving the flat a
door to outside, with solar gain and outside air behind it, where a corridor used to be. That is a
fabricated boundary. Keyed on the `Adiabatic` flag `Filter` sets, deliberately **not** on
`Query.Adiabatic`, which also reports any zero-thickness construction as adiabatic: a surface that was
already adiabatic in the source building is not this run's cut and keeps its apertures.

**The shading context**, using the definitions that already exist rather than a new rule: an uncarried
source panel becomes shade when it is `Query.ExposedToSun` **and** has at most one adjacent space. An
excluded façade, roof or exposed floor comes across; an internal partition (two adjacent spaces) never
does; and an existing source shade (no adjacent space) keeps coming across as before. No proximity
culling - the reduction comes from not simulating thousands of thermal zones, not from pruning shade.

### 2. Fail closed on shared plant and on airflow crossing the cut

Checked **before** anything is built, so a person is never told the scope was invalid after a long
conversion. Refused, never silently narrowed:

* a **ventilation system** serving both a selected and an excluded space;
* an **air handling unit** that straddles the cut through two systems - the per-system check cannot see
  that, so the unit is asked in its own right;
* a **`SpaceAirMovement`** connecting a selected space to an excluded one.

Splitting shared central plant proportionally is a different engineering problem and is deliberately not
attempted. The dedicated per-dwelling MVHR the Part O workflow builds serves exactly one dwelling, so
none of these fire on it.

A refusal returns **no model**, by contract - the same rule `PartOIterationPreparation` already follows.

### 3. `PreparePartOIteration(..., isolate)` - and why isolation comes last

Isolation happens **after** the whole preparation, never before. Part F requirements, design terminals,
system duties and the dwelling's internal transfer air are settled on the whole building first, and
isolation then extracts the assessed dwellings carrying that state unchanged. So isolation **cannot move
a Part F number** - it is a thermal model scope, which is the one thing it claims to be. (The dwelling's
transfer air is solved over the *dwelling*, and a communal corridor belongs to no dwelling, so removing
the entrance door to an excluded corridor cannot change it either - which is what makes re-preparing an
isolated model in a later optimisation round give the same answer as the first.)

It is placed before the openings report and the overheating scenarios so both describe the model that
will actually be simulated. Only the selected dwellings therefore appear in the run's assessment scope.

### 4. `PartOIsolationContext` - stamped, never inferred

A new `AnalyticalModelParameter`, so it travels into the run's `.sam` with everything else. It carries
the isolated space guids, the dwelling zone guids, the dwelling names for reporting, and a **scope
token** (an FNV-1a digest over the sorted space guids) that `SAM_UI` uses to keep one isolated run's
artifacts off another's.

It exists because an isolated model cannot state this by itself: reopened later it is indistinguishable
from a building that only ever had those spaces, and the two are different thermal models. Nothing reads
isolation state back out of a filename.

`TM59AssessmentReport.ThermalModelScope` (new) prints the scope in the report header, defaulting to
`WHOLE BUILDING`.

### 5. The engineering assumption, stated

Selected-to-excluded conditioned-space interfaces are treated as **adiabatic** - approximately zero net
conductive heat transfer across the omitted neighbours. Isolated results may therefore differ from a
whole-building simulation of the same dwellings, and **must not be presented as equivalent**. The dialog,
the preparation summary and the TM59 report all say so.

### Tests

`SAM.Tests.PartOIsolationTests` - 25 tests over the acceptance fixture (Flat 1: Studio + Bathroom;
Flat 2: Bedroom + Kitchen; a Corridor between them; one dedicated MVHR each), covering extraction,
identity under duplicate names, the source model being untouched, the three panel cases, shade
qualification and non-duplication, the cut aperture rule both ways, dedicated and shared plant,
cross-cut air movement, Part F data survival, context round-trip, token stability, and a 2,000-space
synthetic model asserted on shape rather than on a clock.

`SAM.Tests.PartOIterationPreparationTests` - 5 further tests for isolation through the preparation:
the isolated model, the un-isolated control, Part F and design duty unmoved, scenario scope, and the
reported scope note.

Full `SAM.Tests`: **1786 passed, 0 failed**. `SAM.sln` MSBuild: **0 errors**.

## Previous: result review, provenance, and two report defects

**Status: implemented and tested. No Iteration 2B engineering semantics changed.**

Three seams in this repository, all found by tracing defects reported from a real SAM_UI Part O run.

### 1. `Modify.ApplyPartFVentilationRates` - the internal-condition name multiplied

The production TM59 report was showing names like
`Studio - Studio 1_0 - Studio 1_0 - Studio 1_0 - Studio 1_0`. The generated name is
`<condition> - <space>`, and the method was applied over its own previous output on every pass - baseline
conversion, restoration, and each Iteration 2B round re-prepare - so each pass appended the space name
again.

- `UniqueName` is now **idempotent**: before applying the suffix it strips every trailing copy of *this
  space's own* suffix, and the `" (n)"` disambiguation it adds, but only where removing it leaves a name
  carrying this space's suffix. A condition authored for another room is never rewritten toward this one.
- The **name reservation set changed with it**, and had to. It is now seeded from the conditions that
  *survive* the call - unsized spaces and the refusals - not from every condition present. Reserving the
  names this call is about to replace would push every re-prepared space to the next index
  (`" (2)"`, `" (3)"`, ...) on every round, which is the same defect wearing a different suffix. This is
  why the method now decides its sized set once, up front, before reserving anything.
- Pinned by four tests in `PartFAirflowApplicationTests`: `f(x) == f(f(x)) == f(f(f(x)))`; an
  already-multiplied name heals back to one suffix; a disambiguated pair stays stable across
  re-application; and an untouched condition that happens to hold a generated-looking name still reserves
  it, so the sized space is pushed off it rather than colliding.

### 2. `Query.Simplify` - the report's "TM59 Application" column was `-` for every space

Not a classification defect. The extended results are classified correctly at calculation time from the
internal condition; `Simplify`, which produces the results the report renders, was dropping
`TM59SpaceApplications` on both the natural- and mechanical-ventilation branches. Carried through now.
The engineering is untouched - the mechanical criterion does not vary by application - and nothing is
inferred from a room name. Two tests in `TM59AssessmentReportTests`.

### 3. `SimulationResultProvenance` (new) - which results a model was produced from

The record that lets a saved model be reopened in a later session and its TM59 assessment **reviewed**
from the existing results, with no new annual TAS simulation. Stamped onto the model by SAM_UI's
`Modify.RunPartOSimulation`; read back by `PartORun.Restore`.

- **Three things are bound together, none of them guessed.** (1) The results file: the recorded path,
  length and write time - a name alone is never enough, because two runs of one project write results at
  the same derived path. (2) The model's design state: an FNV-1a digest of the `AdjacencyCluster`'s JSON,
  so a model edited since its run is refused rather than paired with results a different design produced.
  (3) The **overheating scenarios**: `Fingerprint_OverheatingScenarios`, a digest of the persisted
  scenarios' derived `Key`s.
- **Why the scenario fingerprint had to exist** (final-review blocker). The scenarios are *model-level*
  state, not part of the `AdjacencyCluster`, so the design fingerprint cannot see them at all - yet
  `PartORun.Restore` reads them straight back as the authoritative TM59 assessment context. Without this
  half: run A produces results and is saved with Scenario A; the same model file later has its
  `OverheatingScenarios` swapped to Scenario B; the cluster is unchanged, the TSD length and write time
  are unchanged, and the record still validated - so run A's results would be reassessed under an
  assessment authority that did not produce them. Now refused, with a refusal that names the scenarios
  rather than blaming the model or the file.
- **Kept as a second fingerprint rather than folded into the first** (approach B). It makes the contract
  observable - a refusal says *which* half moved - and keeps the large-project cluster hashing seam
  separate from the much smaller assessment-context one.
- **The scenario digest is over `OverheatingScenario.Key`, deliberately.** `Key` *is* the scenario's
  identity: derived canonically from the scope, the design zone guid, the mitigation iteration, the
  system template field by field, and the operating assumptions - never persisted, never round-tripped,
  so it cannot go stale. Digesting it binds the *assessment authority*. Digesting the scenarios' JSON
  instead would additionally bind `Name` and `Source`, which the type documents as outside its identity;
  a renamed scenario is the same assessment, and refusing valid results over it would be a false refusal.
  Keys are sorted before hashing (a reordering of a set is not a difference) and the count is hashed with
  them (a set is never confused with a subset). Sixteen bytes per scenario through the same streaming
  digest - no text, no intermediate copy.
- **Fail closed: a partial record is not a record.** Every field is required - TSD path, TSD length, TSD
  write time, design fingerprint, scenario fingerprint. A record missing any of them is refused wholesale
  with its reason, never validated on the halves it happens to carry. The previous rule, which let an
  absent model fingerprint return `true`, is gone along with the test that pinned it. This costs no
  compatibility: real files from before this work carry *no* record at all and are handled as models that
  were never simulated, with the ordinary "prepare and simulate" guidance. There is no third, partial
  state to be lenient towards. Completeness is checked before the filesystem is touched, so an unusable
  record is refused without hashing a large project on the way.
- **The name only ever locates a candidate.** The recorded absolute path first; failing that, the file of
  the same name beside the model, for the case where the whole output folder moved. The length/write-time
  check is what accepts or refuses either.
- **Cheap half first.** File stats are performed before the model digest, so a record whose results are
  simply absent costs nothing to refuse.
- **Scalability.** The digest is taken over the serialized bytes **as they are written**, never over a
  materialized copy. Measured on a real Part O run (`...-OptMax.json`, 9 spaces - that run's model artifact
  is now written as `.sam`, which does not change the digest, taken over the cluster's JSON either way):
  the cluster JSON is
  1.28 MB, i.e. ~140 kB per space, digested in ~30 ms. At the ~5,000 spaces a real SAM project reaches,
  `Encoding.UTF8.GetBytes(node.ToJsonString())` would have allocated the JSON as a string *and* as a byte
  array - a multi-gigabyte spike to produce sixteen characters. The bytes hashed are identical either way
  and the digest value is unchanged (verified against the real model: `b582a931a6a09a54` before and
  after), so this is a memory fix, not a semantic one. Pinned by
  `Fingerprint_IsTheDigestOfTheClusterJson`, which would also catch a future drift in writer options
  silently invalidating every recorded digest.
- `AnalyticalModelParameter` gains `OverheatingScenarios` and `SimulationResultProvenance`. The scenarios
  travel with the model because they are the assessment's authority over which TM59 criterion applies to
  which space; without them a reopened model cannot be assessed, and inventing them would be a guess.

### Validation

- `SAM.Tests`: **1750 passed, 0 failed** (`SimulationResultProvenanceTests`: 10 of them).
- `SAM.sln` Debug: 0 errors.
- `SAM_Systems` and `SAM_Tas` untouched; both working trees clean.

New/changed provenance tests: `IsCurrent_FailsClosedOnAMissingFingerprint` (replaces the assertion that
an absent model fingerprint validates), `Fingerprint_Scenarios_BindsTheAssessmentContext` (save/open
determinism, order independence, changed iteration, dropped scenario, none-vs-some),
`TryResolvePath_TSD_RefusesScenariosChangedUnderUnchangedResults` (the blocker itself, with the design
fingerprint and the file assertion-checked as *unchanged* so the refusal is attributable to the scenario
half alone), and `TryResolvePath_TSD_RefusesAnIncompleteRecord` (each required field removed in turn).

### Known limitation, stated deliberately

A model saved **before** this round carries neither the provenance nor the scenarios - verified on the
user's own `2026-08-05-PartO` output, where all four runs report `NONE (legacy)` for both. Such a model
cannot have its TM59 assessment reviewed, and there is no safe fallback: without the recorded scenarios
there is no authoritative ventilation strategy per space, and supplying one would be exactly the guess
this record exists to prevent. Legacy models keep the ordinary "prepare and simulate" guidance. Runs
produced from this build onward carry both.

## Previous: the selected-equipment capacity envelope

**Status: implemented and tested; PR open against `sow/2026-Q3`.**

### Why it was needed

The ordinary Iteration 2B optimisation is all-or-nothing at a fixed +5 l/s step, and that is right: an
automatic optimiser running fixed steps must not adopt three fifths of one and simulate it as though it
were the step. So the run stops - on `CapacityReached`, or on the iteration guard - with eligible rooms
still failing TM59, and the engineer's next question is a different one: **not** "can this design take
another whole step?" but "what is the best this dwelling and the unit I have already bought could do?".

That question cannot be answered by weakening the round. It is answered by a separate operation producing
a separate, clearly diagnostic model.

### What was added

`Modify.EvaluateDesignAirFlowCapacityEnvelope` - one coherent scaling of the deliberate target vector the
ordinary policy would currently have asked for, per **serving equipment group**, taken to the point where
the first selected-equipment capacity constraint binds.

- **The increments are scaled, not the airflows.** Each target becomes
  `before + scale * (planned - before)`, so a kitchen and an ensuite each asked for +5 l/s with 7 l/s of
  unit headroom left come out at +3.5 and +3.5 - and the balancing supply moves the matching +7, derived
  **once**, by `EvaluateTargetedDesignAirFlows`. Nothing is allocated by arrival order.
- **The group is the air handling unit, not the dwelling.** The scale is solved against
  `Query.AirHandlingUnitDesignDuty`, which sums every system a unit supplies, so two flats sharing one unit
  share one ceiling and one factor. No dwelling-to-unit ownership is assumed and nothing is inferred from a
  name.
- **Analytical, then confirmed.** A round moves a balanced dwelling by a single amount `m` that is
  positively homogeneous in the scale, so the ceiling is at exactly `headroom / M` on whichever side of the
  rating is tighter - a division, not a search. The round is then evaluated once at it. Only where that is
  refused for a reason which is *not* capacity does a bounded, monotonic, deterministic bisection retreat
  within the same interval, and the retreat is recorded on the group.
- **Not capped at one step.** Where the iteration guard stopped the run with capacity to spare the scale
  goes past 1. The bound is selected-equipment feasibility, never `scale <= 1`.
- **The headroom is not stretched by the tolerance.** Comparisons accept a duty a tolerance past the
  rating; an envelope deliberately does not spend that.
- **Every "no" is its own stated outcome.** `DesignAirFlowCapacityEnvelopeOutcome` -
  `Scaled` / `NoHeadroom` / `NoTargets` / `CapacityUnresolved` / `Refused` - with a sentence per group and
  overall. A null catalogue is `CapacityUnresolved` here rather than the backward-compatible "equipment is
  no constraint" it means elsewhere: an unknown ceiling is never an unlimited one.

### What it deliberately does not do

- It **never** weakens the ordinary round. `EvaluateTargetedDesignAirFlows` is unchanged, still refuses a
  partial step, and is the authority the envelope delegates every balancing, Approved Document F and
  capacity decision to.
- It **never** reselects equipment, writes a Part F requirement, or touches an operating, profile or
  runtime airflow - so nothing here is Iteration 3 behaviour.
- It **never** modifies the model it was handed. The analytical value plus a possible bisection means the
  source design is read many times and written to never, so no search state accumulates and the same input
  always gives the same answer.
- `DesignAirFlowCapacityEnvelope.AdjacencyCluster` is a model to **look at**. It is not the accepted design
  and must never become a later round's baseline.

### Files

| File | Change |
| --- | --- |
| `SAM/SAM.Analytical/Modify/EvaluateDesignAirFlowCapacityEnvelope.cs` | New - the envelope authority. |
| `SAM/SAM.Analytical/Classes/System/DesignAirFlowCapacityEnvelope.cs` | New - the whole diagnostic result. |
| `SAM/SAM.Analytical/Classes/System/DesignAirFlowCapacityEnvelopeGroup.cs` | New - one equipment group's share. |
| `SAM/SAM.Analytical/Enums/DesignAirFlowCapacityEnvelopeOutcome.cs` | New - the stated outcomes. |
| `SAM/SAM.Tests/PartOCapacityEnvelopeTests.cs` | New - 24 focused tests. |

### Validation

- `SAM.sln` builds clean; `SAM.Analytical` builds clean in Release too.
- `SAM.Tests`: **1734 passed, 0 failed** (1703 post-#89 baseline + 31 new).
- The 24 new tests cover: headroom below one whole round; proportional scaling with equal and with unequal
  increments; headroom above one step; a vector far past the rating never producing a design past it;
  shared equipment judged on its whole duty; separate units each at their own factor; order independence of
  design, adjustments and report; targeted and derived kept apart; a balancing-only room never promoted to
  a target; Part F requirements unchanged; no runtime airflow written; the product never reselected; the
  source design untouched and repeat runs identical; no targets at all; a dropped target with no lever;
  zero headroom, negative headroom, and a vector that moves no duty; no catalogue, nothing selected, and a
  selection not on offer; a malformed vector refused rather than partly scaled; an unbalanced dwelling
  refused rather than repaired.

### Review findings addressed (Codex, PR #90)

1. **P2** - a catalogue entry that *exists* but states a negative or non-finite maximum passed the
   null-only descriptor check, and the arithmetic then turned it into the **wrong** answer rather than no
   answer: a NaN maximum gives a NaN headroom and a negative one gives a negative headroom, and both fell
   into the no-headroom branch - reporting a malformed catalogue as a perfectly good unit with nothing left
   to give. Now asked through `VentilationUnitCapacityDescriptor.IsValid`, so what "a usable capacity"
   means cannot drift from what selection means by it, and answered `CapacityUnresolved`: an unknown
   capacity is neither an unlimited one nor an exhausted one.
   <br>A rated maximum of **zero** is deliberately NOT caught by this - `IsUsable` says zero is valid and
   simply never sufficient - so it stays `NoHeadroom`, which is the honest answer for a unit that genuinely
   cannot carry anything. Both are pinned.
2. **P2** - the deterministic retreat started from zero, discarding the fact that the measuring round had
   already accepted this very vector at scale 1. A ceiling above 1 means the rating permits that step too,
   so the retreat is now **seeded** from it: it can no longer answer with less than one ordinary step, nor
   refuse a group whose full step is demonstrably fine.
3. **P2** - a retreat left `BindingFlowClassification` set to the capacity side chosen by the analytical
   calculation, so a group stopped by an Approved Document F floor reported supply or extract as binding
   while still holding substantial headroom - telling an engineer to buy a bigger unit that would not help.
   Cleared on retreat, and the reason and note now say the product is **not** what limits that group.
   `Scale_Capacity` standing above `Scale` is the evidence. A group that really does reach its rating still
   names the side, and that is pinned too.

### Issues / blockers

- None known.

### Next step

- SAM_UI's Iteration 2B orchestration calls this after its own terminal condition, gives the result its own
  `-OptMax` TSD identity, and keeps it apart from the last ordinary accepted design.
- Task 2, the canonical-TBD TAS warm-start, is a separate deliverable and a separate PR.

## Superseded (2026-09-02): a reselection stays inside the cluster it was handed - merged as PR #89

**Status: implemented and tested; PR open against `sow/2026-Q3`.**

The manual-seam defect parked under PR #88's blockers, now fixed. `Modify.SelectVentilationUnit` set the
selection on the air handling unit object **in place**, and `AdjacencyCluster`'s copy constructor copies the
object dictionary but not the objects - so `AnalyticalModel.AdjacencyCluster` hands out a copy that still
shares the unit with the model behind it, and a reselection written into the copy was visible on the model
the caller was promised would not move. The airflow half of `ApplyTargetedDesignAirFlow` always wrote
replacements for exactly this reason; the equipment half did not.

The selection is now written onto a guid-preserving `new AirHandlingUnit(unit)` - `SAMObject` carries the
identity, `ParameterizedSAMObject` clones the parameter sets, `ComplexEquipment` clones the internal
equipment model - and added over the model's instance, so every name- and guid-based lookup resolves the
replacement and every relation survives. `PreparePartOIteration` re-resolves the unit by guid after
selecting, since the unit it was holding is not the object that now carries the selection.

### Validation

- `SAM.sln` builds clean.
- `SAM.Tests`: **1703 passed, 0 failed** (1702 post-#88 baseline + 1 new).
- New test: `EquipmentValidation_AReselection_IsNotWrittenBackOntoTheModelTheClusterCameFrom` - an edit on
  the copy carries both halves of the change, the model behind it carries neither, and the replacement unit
  keeps identity, supply temperatures, section arrangement and relations. The existing equipment-validation
  assertions now read the selection from the cluster throughout, never from a stale unit handle.

### Issues / blockers

- None known.

### Next step

- Merge the PR. The declined PR #88 finding (validating terminal quantities across every room of a touched
  system, at the level both seams share) remains the one open follow-up.

## Superseded (2026-09-02): the Iteration 2B design airflow optimisation round - merged as PR #88

**Status: implemented, tested, and exercised end to end on the licensed future-weather acceptance through
SAM_UI #77.**

### Why it was needed

Iteration 2B raises several failing rooms at once. Every existing seam here is strictly one room and one
direction - `ApplyTargetedDesignAirFlow`, `EvaluateTargetedDesignAirFlow`, `ResolveTargetedDesignAirFlow` -
and sequencing them is order dependent twice over:

1. each rebalance moves the rooms the next allocation is computed across, so a different design comes out
   of each ordering, and the ordering is whatever order the assessment results came back in;
2. under `MinimumFirstCookingPriority` the derived extract lands on the local kitchen extract - the very
   room the next deliberate target is about to set.

### What was added

`Modify.EvaluateTargetedDesignAirFlows`, with `DesignAirFlowTarget`, `DwellingDesignAirFlowRound`,
`DesignAirFlowRoundCandidate` and `DesignAirFlowTargetRefusal`.

Per dwelling, with `cS` / `cE` the total deliberate change on each side:

```
only supply targeted    m = cS
only extract targeted   m = cE
both sides targeted     m = max(cS, cE)
derived on each side    = m - c(side), allocated ONLY over rooms nobody targeted on that side
```

`max` is the only choice that never writes a deliberately requested room back down, and it makes every
derived change in the both-sides case an increase, so no Part F floor can be approached by it. Targets sort
by `(SpaceGuid, FlowClassification)` and systems by Guid, so the answer is a function of the **set** of
targets.

- **Never clamps.** Every target gets exactly the figure asked for, or the whole round is refused with no
  model. `ResolveTargetedDesignAirFlow` still clamps and is **unchanged**.
- **Equipment is a constraint, never a variable.** Nothing is ever reselected.
- **Design airflow only** - no requirement, transfer path, AHU duty or runtime airflow is written.
- **No dwelling-to-unit ownership assumed.**

### The engineering is borrowed, not reimplemented

The operation is a **sibling of `ApplyTargetedDesignAirFlow` in the same partial class** and calls the same
private helpers - `VentilationSystem`, `TerminalsOfSystem`, `Allocate`, `IsRedistributable`,
`SetSpaceDesignFlowRate` - plus `ReconcileVentilationSystemDesignDuty` and the capacity verdict extracted
from `EvaluateTargetedDesignAirFlow`'s own check into a shared core. **Not one Part F, balancing or
capacity equation is restated.** The only change to existing code is that extraction (+97/-23, behaviour
preserving).

### Review findings addressed (Codex, PR #88)

1. **P1** (`0a6cb6fa`) - the capacity check ran inside the per-system loop, so a unit shared by two
   targeted systems was judged on a state that never existed: one system rising 10 l/s and another falling
   10 l/s had the first checked 10 l/s above where the round leaves it, refusing a round that fits the
   rating exactly, and accepted cases reported stale headroom. Moved to a second pass,
   `ValidateVentilationUnits`, run after every dwelling is written and **once per unit**; dwellings sharing
   a unit share its verdict and its refusal. Two tests.
2. **P2** (`5740a590`) - `TargetRefusals` were appended in the caller's enumeration order and never sorted,
   so the same unoptimisable set read differently depending on enumeration - contradicting this operation's
   own stated order independence. Now sorted on the same key, with the reason breaking a tie. One test.
3. **P2 - DECLINED with reasons.** Validating terminal quantities across every room of a touched system,
   not only the rooms the round writes. The observation is accurate, but `ApplyTargetedDesignAirFlow`
   validates the same narrow set, and widening only the round would make it refuse dwellings a manual edit
   accepts - the exact drift the shared-helper design exists to prevent. The correct fix is at the level
   both seams share; raised as a separate follow-up. A NaN design flow can only arrive from an externally
   authored or deserialized model, since `SetSpaceDesignFlowRate` refuses one.
4. **P2** (`5bff1327`) - the duplicate check returned from inside the resolution loop, so a target sitting
   after the duplicate was never examined: its reason went unreported and the refusal sort was bypassed
   entirely, making even a refused round's report order dependent. The loop now examines every target and
   collects the duplicate refusals, once per room and direction, before returning.
5. **P2** (`3b77b83b`) - a malformed target (NaN/negative rate, invalid direction, null or foreign space)
   was dropped as merely "not optimisable" while the rest of the batch was applied - silently executing
   part of a transaction. Malformed now refuses the round; only a coherent request the building cannot
   answer is dropped, which is what `DesignAirFlowTargetRefusal` documents.
6. **P2** - two dropped targets sharing a room, a direction and a reason (one bathroom asked for two
   different supply figures, with no supply terminal to move for either) compared equal on the refusal
   sort key, while the report prints the rate - so the same set could still read two ways. The requested
   rate now breaks the last tie in `CompareTargetRefusals`. One test.

### Validation

- `SAM.sln` builds clean; CI green on all three checks.
- `SAM.Tests`: **1702 passed, 0 failed** (1671 baseline + 31 new in `PartODesignAirFlowRoundTests`).
- New tests cover A-K from the programme brief: order independence (model **and** report), a deliberate
  target never overwritten by the balancing allocation, targeted vs derived derived once, Part F floors,
  dwelling isolation, balance, combined capacity refusal, **a resolver clamp not adopted as a full round**,
  the last valid model surviving a capacity refusal exactly, the explicit not-optimisable answer, mixed
  targets that balance directly deriving nothing, no room becoming a target by being moved, shared-unit
  judgement, and refusal ordering.
- Licensed acceptance through SAM_UI #77 on `SAM_zoningAM-CIBSEfutureZ1.sam`: round 1 produced exactly the
  intended targets and derived consequences; at round 9 Flats 2 and 3 were refused at 153/153 against
  150/150 with the product unchanged.

### Issues / blockers

- None blocking. One declined finding above, raised as a follow-up.
- ~~The parked manual-seam defect~~ **Resolved** - see "Latest (2026-09-02): a reselection stays inside the
  cluster it was handed" above.

### Next step

- Merge this PR, then `SAM-BIM/SAM_UI` #77.

## Latest (2026-09-01): Iteration 1a / 1b / 2 licensed acceptance - ACCEPTED

**Status: accepted. Documentation-only change in this repository.**

Each of the three routes was carried end to end - Part F, `PreparePartOIteration`, gbXML, a licensed
full-year TAS simulation (days 1-365, CIBSE Weather 2021), result import, and the TM59 query - on the
corrected `SAM_zoningAM.sam` fixture (9 spaces, 4 zones, Flat 1/2/3 `IsDwelling = true`, Corridor `false`).
The result was read through the **same production path the `Tas.TSDQueryTM59Results` component runs**:
`Convert.ToSAM(TSD)` with the component's own conversion settings, `Create.TM59AssessmentCalculator`,
`OverheatingScenarioMap`, `RestoreDesignInternalConditions`, `Spaces`, `Calculate`, `TM59AssessmentReport` -
not a private summary.

| Route | Prepared | TAS | TM59 |
|---|---|---|---|
| 1b, natural ventilation | 0 systems, 0 AHUs, 0 terminals, 0 air movements, duty `NaN` | 9 zones | 9 natural results, PASS |
| 1a, Base MVHR, generic plant | 3 systems, 3 AHUs, 30+63+63 = 156/156 l/s, 11 nodes at residual 0 | 12 zones | 9 mechanical results, PASS |
| 2, Base MVHR + catalogue | identical to 1a, plus one selected product per AHU | 12 zones | 9 mechanical results, PASS |

**The Iteration 2 invariant held, measured three ways.** The prepared-model dumps for 1a and 2 diff clean
over every air movement, terminal, system and design-duty line; the AHU duties read 30/63/63 l/s before and
after selection; and comparing the two TSDs hour by hour gives **105,120 of 105,120 hourly resultant
temperatures identical, max absolute difference 0**. Selection reads the duty and never writes it.

`MVHR-01/02/03` Air Movement Gain is **0 W for all 8,760 hours** on the 1a and 2 runs, and the 17 TBD
inter-zone air movements carry exactly one room-to-outside extract per extracting room - no duplicate
extract, and no stale unit-to-outside exhaust.

**The 8,760 occupied-hours figure is genuine fixture profile data**, not a query interpretation or an import
problem. `OccupiedHours` counts hours whose occupant sensible gain is above zero, and the residential
internal conditions on this fixture never reach zero (min 105 W / 75 W). The kitchens are the control that
settles it: their profile does drop to exactly zero, for 4,015 hours, and they report 4,745. A broken count
would have returned 8,760 for them too. Note also that the base `MaxExceedableHours` of 262 is 3% of the
*annual* occupied hours - the correct limit for the mechanically ventilated criterion, and **not** the
natural-ventilation limit, where Criterion 1 uses the summer subset (3,672 hours, limit 110) and Criterion 2
the night window (3,285 hours, limit 32).

### Non-blocking findings recorded for follow-up

None of these blocked acceptance, and none is an equipment-selection architecture question.

1. **Ventilation-strategy vocabulary cleanup.** `PreparePartOIteration` accepts `"NaturalVentilation"` as a
   synonym for `NV` and reports success, while the TM59 assessment path compares the raw string against
   `NV / MV / MVRE / MVHR / UV / EOL / EOC / CAV / VAV / DISP` and therefore refuses every space. Measured
   both ways on the same model: with `"NaturalVentilation"`, nine `VENT_STRATEGY_REFUSAL` lines and zero
   results after a *successful* preparation; with `NV`, nine results. The Grasshopper components document
   the two vocabularies differently - `SAMAnalyticalCreateOverheatingScenarios` says "NV, MV, MVRE or UV",
   `SAMAnalyticalPreparePartOIteration` says "NV / NaturalVentilation, or MVHR / MVRE". Deciding which
   vocabulary wins touches the scenario factory or the criterion selection, so nothing was changed here.
   **Vocabulary cleanup, not an Iteration 2 blocker.**

2. **`Tas.TSDQueryTM59Results` must receive the model from the completed workflow / result-import path**, not
   the earlier preparation model. The query resolves a simulated space to a design space through
   `SpaceParameter.ZoneGuid`, the identity TAS preserves across the round trip, and only the model the
   workflow *returns* carries the current TAS zone identities - a preparation output can still hold stale
   guids from an earlier round trip on the source fixture. Verified both ways: preparation output gives
   `SimulationSpaceMap complete = False` and all nine spaces refused; workflow output gives `complete = True`
   and nine results. The Grasshopper Part O canvas already wires it correctly. **This is an important
   SAM_UI wiring requirement.**

3. **`Corridor_1` currently follows a dwelling TM59 criterion on this fixture**, because one ventilation
   strategy was applied to every zone - so it was assessed as a bedroom on the NV route (its fixture
   internal condition carries the Sleeping application) and as a mechanically ventilated room on the MVHR
   routes. Common-space assessment should use the appropriate `UV` scenario where required, which selects
   the TM59 corridor >28 degC risk check rather than a compliance criterion.
   `Create.OverheatingScenarios` already classifies the corridor as `CommonSpace` by scope, so this is a
   **TM59 scenario/fixture follow-up, not MVHR equipment-selection architecture.** It passes on every
   criterion either way, but the corridor's result should come from the corridor criterion before it is
   quoted.

4. **The Nuaire 150 l/s is the highest fan-curve free-air capability point of the evidenced product.** It is
   a legitimate maximum and a legitimate selection ceiling, correctly named "Maximum". **It must not be
   reinterpreted as a selected operating flow or an installed design duty** - a project needing the
   installed duty wants Nuaire's project-specific selection. See `SAM_Systems/PROJECT_PROGRESS.md`,
   *THE CAPACITY COMES FROM THE FAN CURVE*, and `SAM-BIM/SAM_Systems` PR #18.

Companion change: `SAM-BIM/SAM_Systems` PR #18, which maps the Nuaire maximum airflow. **Iteration 3, the
physical/operating-state work, and SAM_UI are out of scope for this acceptance.**

## Latest (2026-08-31): Grasshopper Seam 2 - targeted design airflow, rebalance, and equipment validation

**Status: implemented, tested, PR pending review. Not merged.**

### Goal

Expose the already-designed targeted room design-airflow workflow through Grasshopper: target one room's
design airflow, rebalance the network, recalculate duty, validate the currently selected ventilation unit,
and keep/reselect/refuse accordingly - without redesigning the analytical architecture and without
starting Iteration 3 (`SAM_Systems` materialisation).

### Inspection findings (before any code was written)

Re-verified against the current merged code (post Seam 1, tip `bb82865a`), not assumed from an earlier
session:

- `Modify.ApplyTargetedDesignAirFlow` already did everything through recalculating the dwelling's design
  duty after a targeted change: validated the existing network first (balance, then Approved Document F
  compliance via `Query.ReconcileVentilationSystemDesignDuty`), refused a request below the room's Part F
  floor, applied the targeted change and derived every balancing consequence through a private `Allocate`
  helper (the design-side application of `PartFCalculator.AllocateContinuousExtract`'s cooking-priority
  rule), validated the resulting network, and wrote nothing on any refusal - all before a single terminal
  was touched. Its return type, `DwellingDesignAirFlowChange`, already separated `TargetedAdjustment` from
  `DerivedAdjustments` (each `DesignAirFlowAdjustment` carrying its own `IsDerived` flag).
- It did **not** touch equipment at all - no call to `Query.IsVentilationUnitSufficient` or
  `Modify.SelectVentilationUnit` anywhere, and no catalogue parameter. Both of those methods already
  existed, already worked exactly as designed (confirmed by reading their full bodies), and were already
  exercised in `SAM.Tests` - but only as three separately-sequenced calls a test helper (`Retarget`, then
  `IsVentilationUnitSufficient`, then `SelectVentilationUnit`) made by hand. No single call gave the
  keep/reselect/refuse guarantee Grasshopper needs.
- The existing contract, proven directly by `ATargetedChangeBeyondCapacity_ExposesExhaustionAndEscalates`:
  a design airflow change **commits regardless of equipment adequacy**. The airflow write and the
  equipment question are separate, and nothing in the existing code rolls the airflow change back because
  no product in a catalogue is capable. This is the authority preserved below - not a policy invented for
  Grasshopper.
- No "zone owns AHU" shortcut exists anywhere in `ApplyTargetedDesignAirFlow` or its helpers. The
  air handling unit is resolved from a `VentilationSystem` by a **pre-existing, documented, name-based**
  lookup (`Query.AirHandlingUnit`, technical debt recorded on the method itself, shared by
  `Modify.AddAirMovementObjects`, `Modify.AddVentilationSystem` and the TAS export) - not something this
  stage introduces, and `AirHandlingUnitDesignDuty` is explicitly summed over every system a unit supplies,
  "not one... the general MEP arrangement of one unit serving several zones is precisely what this
  architecture must not foreclose."

**Conclusion: this is an analytical orchestration change, not a pure GH-exposure seam.** The library
intentionally stopped before equipment revalidation, so a tiny, reusable addition was made to
`SAM.Analytical` first - composing the two existing primitives, never reimplementing either - and
Grasshopper wraps that, exactly as Seam 1 wrapped `Modify.PreparePartOIteration`'s existing overload.

### What was added

| File | What |
|---|---|
| `SAM.Analytical/Enums/VentilationUnitSelectionOutcome.cs` (new) | `NotApplicable`/`Kept`/`Reselected`/`Refused` - what happened to the serving unit's selection. |
| `SAM.Analytical/Classes/System/DwellingDesignAirFlowChange.cs` | +4 properties: `VentilationUnitSelectionOutcome`, `AirHandlingUnit`, `VentilationUnitReference`, `VentilationUnitSelectionReason`. None of the four participate in `Successful` - equipment adequacy is reported beside a successful airflow change, never instead of it. |
| `SAM.Analytical/Modify/ApplyTargetedDesignAirFlow.cs` | One new optional parameter, `ventilationUnitCapacityDescriptors_` (`IEnumerable<VentilationUnitCapacityDescriptor>`, appended last, default `null` - every existing call site is unaffected). Null skips equipment validation entirely, exactly as before this parameter existed. Where supplied, a new private `ValidateVentilationUnit` helper runs strictly AFTER the airflow change has already committed: `Query.IsVentilationUnitSufficient` first (Kept if it still suffices - never reselects just because a smaller capable product exists elsewhere in the catalogue), then `Modify.SelectVentilationUnit` only if it does not (Reselected on success, Refused - and left exactly as it was - if no product is capable). Composes the two existing primitives; adds no selection or sufficiency logic of its own. |
| `Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalApplyTargetedDesignAirFlow.cs` (new) | Thin GH wrapper. Inputs: `_analyticalModel`, `_space`, `_flowClassification`, `_designAirFlow`, `partFExtractAllocationStrategy_`, `tolerance_`, `ventilationUnitCapacityDescriptors_` (optional list, same `GH_ObjectWrapper`/`IGH_Goo` unwrap idiom Seam 1's Codex fix established - not repeated as a marshalling defect this time). Outputs: `analyticalModel`, `space`, `designAirFlowBefore/After l/s`, `derivedAdjustments`, `supply/extract duty l/s`, `airHandlingUnit`, `ventilationUnitReference`, `equipmentOutcome`, `equipmentReason`, `notes`, `refusals`, `successful`. |
| `SAM.Tests/PartOVentilationUnitSelectionTests.cs` (+7 tests, section K) | Focused on the NEW orchestration only - not a retest of the rebalancing rules sections D/E/H already cover. See "Tests" below. |

### Transactional semantics, exactly as inspected and preserved

- **Targeted vs derived**: unchanged, already correct, already tested (sections D/E). Not retested here
  beyond what the new equipment tests incidentally exercise.
- **Part F floor / invalid rebalance**: unchanged - refuses with nothing written, already tested (sections
  D/H). The new optional parameter defaults to `null` for every pre-existing call, so every one of these
  tests exercises the exact same code path as before this stage.
- **Keep**: `IsVentilationUnitSufficient` true - `equipmentOutcome` = Kept, reference unchanged, **no call
  to `SelectVentilationUnit` at all** - so a smaller-but-still-capable product elsewhere in the catalogue
  can never displace a unit that is still adequate.
- **Reselect**: `IsVentilationUnitSufficient` false, `SelectVentilationUnit` finds a capable product -
  `equipmentOutcome` = Reselected, the SMALLEST capable product (proven with the catalogue's own
  100/150/180/220 fixture: a duty of 160 l/s selects 180, skipping 150, which the smallest-capable rule
  already guaranteed and this stage only exposes).
- **Refuse (equipment)**: `SelectVentilationUnit` finds nothing capable - `equipmentOutcome` = Refused, the
  unit keeps whatever it had before this call, and **the airflow change's `Successful` stays true** - the
  existing, unmodified contract, not a new rollback policy.
- **No descriptors connected**: `equipmentOutcome` = NotApplicable, the unit's existing selection is not
  even read, let alone touched - true backward compatibility, matching Seam 1's own convention for the
  same distinction.
- **Multi-dwelling / no zone-owns-AHU shortcut**: one dwelling's equipment escalation cannot read, report
  on or touch another dwelling's unit - proven directly (not just structurally) with a new test against
  `TwoDwellingModel()`.

### Deferred guards, re-evaluated for this seam specifically

Seam 1 could not make either guard reachable (it never touched `VentilationTerminal.DesignFlowRate_Lps` or
any classification at all). Seam 2 is different - it is the first public entry point onto
`Modify.ApplyTargetedDesignAirFlow`, which **does** write `DesignFlowRate_Lps` - so both were re-traced
against the actual new reachability surface, not re-asserted from the earlier note:

- **`FlowClassification.Undefined` as the `_flowClassification_` input**: already refused, by the
  existing top-of-method check (`flowClassification != Supply && != Extract`) - confirmed by reading the
  method, not assumed. Pinned with `UndefinedFlowClassification_IsRefused`. The low-level setter
  (`Modify.SetSpaceDesignFlowRate`) was **not** modified - it never sees an `Undefined` terminal in the
  first place, because every terminal list it works from is already filtered to Supply/Extract upstream.
- **`VentilationTerminal.DesignFlowRate_Lps == null` on a pre-existing, externally-authored terminal**:
  traced in full rather than assumed safe. Two distinct scenarios exist:
  1. **A null-flow terminal sharing a room with an already-established one.** `IsRedistributable`
     deliberately allows null through as a zero-weighted quantity (refusing only NaN/Infinity/negative) -
     the null terminal's calculated share is then exactly zero, never `NaN`, and the healthy terminal
     absorbs the whole change. This is not a bug: refusing outright would block the ordinary case of
     designing a room for the first time through this operation. Pinned with
     `ASiblingNullTerminal_GetsAnExplicitZeroShare_NeverCorruptingTheRoomTotal`. **No guard was added** -
     the existing behaviour is correct and is not the risk the deferred note names.
  2. **A whole system with no established terminal at all**, which could make
     `Query.VentilationSystemDesignDuty` report a coerced, falsely-"established" 0/0 duty that
     `IsVentilationUnitSufficient` would then trivially pass - the exact risk the deferred note names.
     Traced and found **still not reachable through this seam**: `ApplyTargetedDesignAirFlow`'s own
     pre-existing balance and Approved Document F compliance preconditions (unchanged by this work) refuse
     before the transaction ever reaches equipment validation, for any system a real Part F preparation
     could produce. Reaching this state would require a hand-built model bypassing those preconditions
     entirely - a genuine authoring path this seam does not add. **No guard was added.** This risk's own
     reopening condition (a production path that *creates* `VentilationTerminal` objects) still does not
     apply: this operation only ever rewrites terminals the model already had.

### Tests

Section K, `SAM.Tests/PartOVentilationUnitSelectionTests.cs` (9 new - 7 from the initial pass, +1 added
after the internal review pass, +1 added after Codex's PR review):

1. `EquipmentValidation_KeptWhenSelectedProductRemainsSufficient`
2. `EquipmentValidation_ReselectsTheSmallestCapableProductWhenExhausted`
3. `EquipmentValidation_RefusesEquipmentButKeepsTheAirflowChangeSuccessful`
4. `EquipmentValidation_UnconnectedDescriptors_LeavesTheSelectionUntouched`
5. `EquipmentValidation_NeverSelected_StaysNotApplicable_EvenWithACatalogueOffered`
6. `EquipmentValidation_SelectedProductNotInThisCatalogue_IsRefusedAsUnknown_NeverDowngraded`
7. `EquipmentValidation_TwoDwellings_StayIndependent`
8. `UndefinedFlowClassification_IsRefused`
9. `ASiblingNullTerminal_GetsAnExplicitZeroShare_NeverCorruptingTheRoomTotal`

### Internal review pass (before pushing) - 4 real findings, all fixed

A dedicated multi-angle review of the diff (line-by-line, removed-behaviour, cross-file, reuse,
simplification/efficiency, altitude/conventions) surfaced four findings worth acting on immediately,
beyond what the focused tests above already covered:

1. **`VentilationUnitSelectionOutcome.NotApplicable`'s own doc comment was wrong.** It claimed a unit
   with no prior selection reads `NotApplicable`; the code actually let `SelectVentilationUnit` make a
   FIRST selection for it, landing on `Reselected`/`Refused` instead - an unrequested side effect of a
   targeted airflow change. Fixed in the code (not the doc): `ValidateVentilationUnit` now checks
   `SelectedVentilationUnitReference() is null` up front and stays `NotApplicable` - this call validates
   an EXISTING selection, it does not make a first one. Pinned with a new test,
   `EquipmentValidation_NeverSelected_StaysNotApplicable_EvenWithACatalogueOffered`.
2. **`VentilationUnitSelectionReason` never reached the Grasshopper component.** A real gap - a `Refused`
   equipment outcome gave a Grasshopper user zero explanation, even though the reason already existed on
   the object returned. Fixed: new `equipmentReason` output added.
3. **`VentilationUnitReference` was three hand-maintained copies of `AirHandlingUnit`'s own selection.**
   Simplified to a computed property reading `AirHandlingUnit?.SelectedVentilationUnitReference()` on
   demand, so the two can never disagree.
4. **The post-`SelectVentilationUnit` guid re-resolution reinvented `RelationCluster.GetObject<T>(Guid)`.**
   Replaced the manual `GetObjects<T>().Find(x => x.Guid == ...)` scan with the existing single-item
   lookup - simpler and cheaper.

Three further observations were considered and deliberately left as-is: a connected-but-wrong-typed
`ventilationUnitCapacityDescriptors_` wire collapsing to an empty (not null) list - an established
convention already shipped in Seam 1's own component, not something newly introduced here; the
`IEnumerable<VentilationUnitCapacityDescriptor>` parameter being enumerated twice inside
`ValidateVentilationUnit` - a pre-existing characteristic of calling `IsVentilationUnitSufficient` then
`SelectVentilationUnit` in sequence (the same sequence the test helpers already used by hand), and both
real callers (this GH component, the SAM_Systems catalogue reader) hand over materialized lists in
practice; and keeping equipment validation as a private, inline step of `ApplyTargetedDesignAirFlow`
rather than a separately composable public operation - a deliberate choice matching Seam 1's own
precedent of extending an existing method rather than adding a parallel entry point, revisit only if a
second caller genuinely needs the same guarantee.

### Codex review (PR #84) - two real P2 findings, both fixed

1. **Unknown capacity was being treated as exhausted.** Where the CURRENTLY selected product is not
   among the descriptors this call was given (a filtered or narrower catalogue than the one it was
   originally selected from), `IsVentilationUnitSufficient` returns `false` because the capacity is
   *unknown*, not because the unit is insufficient - and the code was falling through to reselection
   regardless, which could silently downgrade a unit (e.g. from MVHR-220 to MVHR-100) that was never
   actually exhausted. Fixed: `ValidateVentilationUnit` now checks
   `Query.SelectedVentilationUnitCapacityDescriptor` directly before consulting sufficiency at all: unknown
   capacity now refuses immediately (unit untouched, `VentilationUnitSelectionReason` explains why) and
   never reaches `SelectVentilationUnit`. Pinned with a new test,
   `EquipmentValidation_SelectedProductNotInThisCatalogue_IsRefusedAsUnknown_NeverDowngraded`.
2. **`result.AirHandlingUnit` stayed null when a resolved unit had no prior selection.** Contradicted the
   documented contract ("null where none resolved") and hid the resolved unit from Grasshopper's
   `airHandlingUnit` output even though it *had* resolved - only its selection was missing. Fixed:
   `result.AirHandlingUnit` is now assigned as soon as the unit resolves, before the no-selection check
   that keeps the outcome `NotApplicable`. Pinned by extending
   `EquipmentValidation_NeverSelected_StaysNotApplicable_EvenWithACatalogueOffered` to assert
   `change.AirHandlingUnit` is non-null.

### Validation

- Focused: `PartOVentilationUnitSelectionTests` 80/80 (71 existing + 9 new, including both post-review pins).
- Full `SAM.Tests` Release: **1622 passed, 0 failed** (was 1613 before this stage; +9 new, zero
  regressions - every pre-existing call to `ApplyTargetedDesignAirFlow` exercises the exact same code path,
  since the new parameter defaults to `null`).
- `SAM.Analytical`, `SAM.Analytical.Grasshopper` compile with 0 CS errors in Release.
- `SAM-BIM/SAM_Systems`'s existing `SAMAnalyticalSystemVentilationUnitCatalogue` (Seam 1) rebuilt clean
  against these fresh SAM binaries (after the review-pass fixes too), confirming no companion change is
  needed there.
- The project-level `dotnet build` of either `.csproj` alone still fails its post-build deploy step with
  the pre-existing `*Undefined*\files\resources` xcopy quirk when `$(SolutionDir)` is not supplied -
  unrelated to this change, same as Seam 1; `-p:SolutionDir=...` builds clean.

### GH acceptance

No live-Grasshopper/Rhino test harness exists in this repository (unchanged since Seam 1). The manual
canvas for final licensed acceptance:

```
SAMAnalytical.SystemVentilationUnitCatalogue (SAM_Systems)
  -> ventilationUnitCapacityDescriptors
SAMAnalytical.PreparePartOIteration
  -> analyticalModel
SAMAnalytical.ApplyTargetedDesignAirFlow
  _analyticalModel          <- analyticalModel
  _space                    <- the room to target
  _flowClassification       <- "Supply" or "Extract"
  _designAirFlow            <- the new l/s
  ventilationUnitCapacityDescriptors_ <- ventilationUnitCapacityDescriptors
```

- **Keep**: target a room by a small amount that keeps the dwelling's duty within the selected product's
  rating. Expect `equipmentOutcome` = Kept, `ventilationUnitReference` unchanged.
- **Reselect**: target a room by enough that the recalculated duty passes the selected product's rating,
  against a controlled test catalogue with more than one capable size above it (not the real Nuaire
  product - its authoritative maximum capacities remain unresolved, so it can never prove a reselection).
  Expect `equipmentOutcome` = Reselected, `ventilationUnitReference` the next SMALLEST capable product.
- **Refuse**: target a room by enough that no product in the catalogue offered is capable. Expect
  `equipmentOutcome` = Refused, `successful` still `true`, and `ventilationUnitReference` unchanged from
  before the call.
- **Part F floor**: request a design airflow below the targeted room's Approved Document F requirement.
  Expect `successful` = `false`, `analyticalModel` unchanged, and `refusals` naming the shortfall.

### Next step

Merge this PR once reviewed. Seam 2 and Iteration 3 remain out of scope beyond what this stage adds - no
`AirSystem` materialisation, no generic `AirHandlingUnitTemplate`, no runtime/profile airflow, no TAS
export change.

## Latest (2026-08-31): Grasshopper Seam 1 - exposing existing Iteration 2 selection through Grasshopper

**Status: implemented, tested, PR pending review. Not merged.**

### Goal

Make the already-implemented Iteration 2 ventilation-unit selection (`Modify.PreparePartOIteration`'s
5-argument overload, `Query.SelectSmallestCapableVentilationUnit`) usable from a normal Grasshopper canvas -
expose it, do not reimplement it, do not redesign the analytical architecture.

### Inspection findings (before any code was written)

Confirmed against the current merged code, not assumed from an earlier session:

- `Modify.PreparePartOIteration(..., IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)`
  already selects **independently per dwelling** (inside the per-dwelling loop in `PrepareBaseMVHR`), already
  derives duty from the **realised terminal network** (`VentilationSystemDesignDuty`/`AirHandlingUnitDesignDuty`,
  never from Part F requirement figures), already calls the existing smallest-capable kernel
  (`Query.SelectSmallestCapableVentilationUnit`), and writes **only**
  `AirHandlingUnitParameter.VentilationUnitReference` (`Modify.SelectVentilationUnit.cs:129` - nothing else on
  the model moves). `PartOIterationPreparation.VentilationUnitSelections` already existed and was already
  populated. When `ventilationUnitCapacityDescriptors` is `null`, the whole selection block is skipped by a
  guard clause (`if (ventilationUnitCapacityDescriptors is not null)`) - behaviour is unchanged from Iteration
  1a's four-argument call.
- `SAMAnalyticalPreparePartOIteration` (the GH component) called only the **four**-argument overload -
  descriptors were never passed, so selection never ran from Grasshopper. `PartOIterationPreparation` already
  carried `VentilationSystems`/`AirHandlingUnits` (plural) and `VentilationUnitSelections`, all three unwired
  to any GH output.
- `VentilationUnitCapacityDescriptor` and `VentilationUnitSelection` are deliberately **not** `IJSAMObject` (own
  doc comments: "the catalogue is the wire format" / same reasoning as `SystemCapabilityDescriptor`), so
  neither can ride the existing `GooJSAMObject<T>`/`GooSAMObject` family - they ride the untyped
  `GooObject`/`GooObjectParam` instead. `VentilationUnitTemplate` **is** a `SAMObject`, so it rides
  `GooSAMObject`/`GooSAMObjectParam` directly. No new Goo/Param type was created for any of the three.

### What was added

| File | What |
|---|---|
| `Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalPreparePartOIteration.cs` | One new optional input, `ventilationUnitCapacityDescriptors_` (`Param_GenericObject`, list, unwrapped to `List<VentilationUnitCapacityDescriptor>` or left `null`), passed straight into the existing 5-argument `PreparePartOIteration` overload. Three new outputs forwarding previously-hidden `PartOIterationPreparation` properties verbatim: `ventilationSystems`/`airHandlingUnits` (plural, `GooAnalyticalObjectParam`) and `ventilationUnitSelections` (`GooObjectParam`). |
| `SAM/SAM.Tests/PartOVentilationUnitSelectionTests.cs` (+4 tests, section G) | Focused on the new wiring, not a retest of the selection algorithm sections A-F already cover: null descriptors leave the plural network outputs populated but `VentilationUnitSelections` empty (backward compatibility); a connected catalogue's selection is the same identity written to the unit; two dwellings' plural outputs stay independent with matching per-dwelling duties; a selection's reported duty is the design duty, never the selected product's capacity. |

### What was deliberately not touched

The selection kernel, the catalogue vocabulary, and every existing Part F/Part O behaviour. No capability was
copied onto `VentilationTerminal.DesignFlowRate_Lps`, no maximum airflow was inferred from any performance
table, no manufacturer performance data was written onto the analytical AHU, and `SAM.Analytical` gained no
reference to `SAM.Analytical.Systems`.

### Deferred guards, re-checked

Both re-confirmed **not reachable** through this seam, matching the prior investigation:

- `VentilationTerminal.DesignFlowRate_Lps == null` - terminals on this path are only ever constructed by
  `Modify.RealizePartFVentilationTerminals` after `continuous_Lps.HasValue` is verified
  (`RealizePartFVentilationTerminals.cs:189-208`); the new GH input/outputs create no new terminal-construction
  path.
- Undefined flow classification - the same constructor call always passes the ternary of
  `FlowClassification.Extract`/`Supply`, never `Undefined`.

### Validation

- Focused: `PartOVentilationUnitSelectionTests` 71/71 (67 existing + 4 new).
- Full `SAM.Tests` Release: **1613 passed, 0 failed**.
- `SAM.Analytical.Grasshopper` compiles with 0 CS errors in Release. The project-level `dotnet build` of the
  `.csproj` alone fails its post-build deploy step with the pre-existing `*Undefined*\files\resources` xcopy
  quirk when `$(SolutionDir)` is not supplied - the same environmental quirk recorded lower in this file
  (search "xcopy quirk"); confirmed pre-existing by rebuilding the unmodified baseline commit, which fails
  identically. Passing `-p:SolutionDir=...` (or building via `SAM.sln`) builds clean.

### GH acceptance

No live-Grasshopper/Rhino test harness exists in this repository - the one Grasshopper-driving test project,
`SAM.Core.Grasshopper.Tests`, never calls `SolveInstance`, and `SAM.Tests` does not reference
`Grasshopper.Kernel`/`GH_IO` at all. Per the brief, no new testing framework was built for this stage;
automated coverage instead targets `Modify.PreparePartOIteration` and `PartOIterationPreparation` directly -
exactly what the GH component thinly wraps. The manual canvas for final licensed acceptance, and both
required cases, are documented in `SAM_Systems/PROJECT_PROGRESS.md` under "Grasshopper Seam 1", alongside the
companion `SAMAnalyticalSystemVentilationUnitCatalogue` component this input is meant to be wired from.

### Codex review (2026-08-31) - one P1 finding, fixed

Codex found a real defect on the first pass: reading `ventilationUnitCapacityDescriptors_` as
`List<object>` never unwraps Grasshopper's `IGH_Goo` wrapper (a descriptor arrives as a `GooObject`, since
it is not an `IJSAMObject`), so `@object is VentilationUnitCapacityDescriptor` was false for every item -
a connected catalogue silently became a non-null *empty* list, which the library reads as "a catalogue was
offered and nothing in it is sufficient", refusing every dwelling instead of selecting.

Fixed by following the codebase's own established unwrap idiom for a generic input
(`SAMAnalyticalAddAirPanels.cs`/`SAMAnalyticalAddAirPartitions.cs`: read as `List<GH_ObjectWrapper>`, then
`objectWrapper.Value`, then `if (@object is IGH_Goo) { @object = (@object as dynamic).Value; }` before the
type check). No automated regression test could be added for this specific defect - it only manifests
through Grasshopper's own data-marshalling layer, which (see "GH acceptance" below) neither this repository
nor `SAM.Tests` has a harness to exercise outside a live Rhino/Grasshopper session; the fix is proven by
precedent (copied verbatim from two existing components) and by rebuild + full `SAM.Tests` re-run
(1613/1613, unaffected - the defect was in GH-layer unwrapping the tests cannot reach).

### Next step

Merge this PR alongside `SAM-BIM/SAM_Systems`'s `feature/parto-iteration2-gh-ventilation-catalogue`, in either
order - neither depends on the other at the code level. Seam 2 and Iteration 3 are deliberately not started
here.

## Latest (2026-08-29, second pass): parsed-JSON numeric round-trip fix - PR #81

**Status: COMPLETE, merged into `sow/2026-Q3` via PR #81.**

### The defect

A `JsonValue` built in-process wraps a boxed CLR number; a `JsonValue` that came out of
`JsonNode.Parse` - **every path that reads a saved SAM file** - wraps a `System.Text.Json.JsonElement`.
Readers that tested the CLR type of `GetValue<object>()` (`Core.Query.IsNumeric`) passed in-process and
silently deserialised empty/zeroed from disk. No exception, so in-memory-only tests never saw it. Found
while adding `MultilinearInterpolation` on the catalogue branch (recorded in "A defect found on the way"
below, which this PR resolves).

### The fix

- **`SAM.Core/Query/TryGetDouble.cs` (new)** - the one place that knows this: asks a `JsonValue` for
  `double`, then `long`, then `decimal`, with the old `IsNumeric` test as a last fallback for exotic boxed
  types. JSON strings are deliberately rejected (the CLR test it replaces rejected them too).
- **`LinearInterpolation.FromJsonObject`** and **`PolynomialEquation.FromJsonObject`** use it.
- **`Core.Query.Array<T>`** hands the `JsonNode` to `TryConvert` whole instead of unwrapping with
  `GetValue<object>()` first.
- **`Core.Query/TryConvert.cs`** - `double` read straight off the node (fast path, never becomes text);
  `TryConvertJsonNumber` now parses JSON number tokens **invariantly** (`NumberStyles.Float`/`Integer`,
  `InvariantCulture`). A JSON number is invariant by definition; the culture-guessing `TryParseDouble`
  read `"-1.000"` as *minus one thousand* under `en-US` (the `-1,000` group-separator reading wins the
  fractional-part tie via `Math.Min`). Codex P2 finding on the first commit, fixed in the second.
- **`PerformanceJson`**'s private `TryGetDouble` delegates to the shared helper, keeping only its own
  NaN/infinity rule.

Every `GetValue<object>()` in the repository is gone bar the one inside the helper. Remaining `IsNumeric`
call sites are non-JSON contexts (parameter values, Grasshopper `params object[]` inputs).

### Files changed (PR #81)

- `SAM/SAM.Core/Query/TryGetDouble.cs` (**added**)
- `SAM/SAM.Core/Query/TryConvert.cs`
- `SAM/SAM.Core/Query/Array.cs`
- `SAM/SAM.Math/Classes/Interpolation/LinearInterpolation.cs`
- `SAM/SAM.Math/Classes/Equation/PolynomialEquation.cs`
- `SAM/SAM.Analytical/Classes/System/PerformanceJson.cs`
- `SAM/SAM.Tests/InterpolationRoundTripTests.cs` (**added**, 7 tests - all round-trip through a JSON
  **string**, which is what forces the parsed-`JsonElement` path)
- `PROJECT_PROGRESS.md` (this file)

### Validation

- Full `SAM.Tests` Release locally: **1600 passed, 0 failed**.
- CI on the branch: build (Release), test (Release), spdx - all green.

### Unresolved

- One P3 review note, parked: `Array<T>` now routes elements through `TryConvert`'s string branch too, so
  a JSON *string* element in a numeric array (e.g. `["1.5"]`) converts, and still via the
  culture-guessing `TryParseDouble`. Sole in-repo caller is `Array<double>` over serialised matrices, so
  real-world exposure is negligible. If it ever matters, gate on `GetValueKind() == JsonValueKind.Number`.

### Next step

Nothing open on this fix. The standing next seam is unchanged - see "Next step" below: the `SAM_Systems`
MVHR catalogue reader (its companion branch needs PR #80 + this PR merged first; both now are).

---

## Current status

**Part F / Part O Iteration 2 is COMPLETE and MERGED into `sow/2026-Q3`.**

```
SAM-BIM/SAM #79
merge commit fafed15f
```

Iteration 1a shipped a design that realized the Approved Document F requirement *exactly*, on generic
plant. Iteration 2 puts a real product behind the plant and lets the design move above the requirement -
which means **four quantities now exist where one used to serve**, and the whole iteration is about not
letting them collapse into each other.

The merged implementation provides:

- the authority separation `PartFRequiredAirFlow != DesignAirFlow != SelectedMVHRCapacity !=
  OperatingAirFlow`;
- **room-level** Approved Document F floors (not a dwelling average);
- design airflow held on `VentilationTerminal.DesignFlowRate_Lps`;
- a per-dwelling `VentilationSystem` + `AirHandlingUnit`;
- system and air handling unit duties **derived, never stored**;
- a reusable `VentilationUnitCapacityDescriptor`;
- **smallest-capable** MVHR selection (never nearest-by-distance);
- the selected product **identity** persisted on the analytical air handling unit, capacity left in the
  catalogue;
- transactional targeted-vs-derived design airflow adjustment (all-or-nothing);
- dwelling balance preservation across every successful transaction;
- **no runtime / profile / TAS airflow coupling** - Iteration 2 writes no operating airflow.

### The authority separation - requirement != design != equipment capability != operating airflow

| Authority | Lives in | Written by |
|---|---|---|
| Part F requirement | `PartFSpaceData.Terminals[].ContinuousDesignFlowRate_Lps` | `PartFCalculator` **only** - immutable to everything in Iteration 2 |
| Equipment capability | `VentilationUnitCapacityDescriptor` (the catalogue, outside the model) | never written into the model at all |
| Design airflow | `VentilationTerminal.DesignFlowRate_Lps` | the design / Part O optimisation path |
| Operating airflow | `InternalCondition`, profiles, TAS `ticV`, IZAM state | **untouched** - a later iteration |

The invariant:

```
PartFRequiredAirFlow  <=  DesignAirFlow  <=  SelectedMVHRCapacity
```

**The two constraints are enforced at different levels, deliberately.** The Part F floor is enforced at the
applicable **terminal/space** level - a room's design may never fall below what the Approved Document
requires *of that room*. The capacity ceiling is evaluated at the **AHU/system duty** - equipment serves a
dwelling, not a room, so "does it fit?" is only meaningful about the summed duty. Neither check substitutes
for the other, and a system total substitutes for neither (see fix 3 below, which is exactly that mistake).

### Targeted vs derived

`Modify.ApplyTargetedDesignAirFlow` sets **one** room's design airflow and rebalances the dwelling around
it, as a single all-or-nothing transaction.

> **Targeted adjustment = explicit design decision.**
> **Derived adjustment = consequence required to restore a valid balanced dwelling network.**

```
targeted:   Bedroom 1 supply 20 -> 24        <- the only room anyone selected
derived:    extract +4, allocated across the dwelling's extract terminals
            transfer paths recalculated on the next preparation
            AHU design duty follows
unchanged:  Bedroom 2, Living Room, and every Part F requirement
```

A Part O iteration targets the room that *failed*. The wet room whose extract rises by the matching 4 l/s
was never selected for optimisation, and reporting the two the same way would make it impossible to say
afterwards which rooms were engineering decisions. `DesignAirFlowAdjustment.IsDerived` carries the
distinction **on the report only** - it is not stored on the model and is not a fifth authority.

The allocation rule is **borrowed, not invented**: `PartFExtractAllocationStrategy.MinimumFirstCookingPriority`,
the same strategy `PartFCalculator.AllocateContinuousExtract` used to size the dwelling. It is applied to
the *change*, never recomputed from scratch, so a deliberate imbalance a designer authored survives.

### The MVHR descriptor / reference selection seam

`SAM.Analytical` **cannot** depend on `SAM_Systems`, and Iteration 2 adds no such dependency. It reuses the
seam Iteration 1a established for system templates rather than inventing a second one:

| Iteration 1a (settled) | Iteration 2 (mirror) |
|---|---|
| `SystemCapabilityDescriptor` - identity + capability bits + `Rank`, plain class, **handed in** | `VentilationUnitCapacityDescriptor` - identity + max supply/extract + `Rank` |
| `SystemCapabilitySelection` - Selected or Refused, no third answer | `VentilationUnitSelection` |
| `Query.CapableSystems` / `SelectPreferredCapableSystem` - pure, open no file | `Query.CapableVentilationUnits` / `SelectSmallestCapableVentilationUnit` |
| `SAM_Systems.Query.SystemCapabilityDescriptors(dir, app)` supplies values | catalogue stays an **argument** - see open items |
| `SpaceParameter.PartFSpaceData`, `VentilationTerminalParameter.PartFTerminalReference` | `AirHandlingUnitParameter.VentilationUnitReference` |

Only the product's **identity** is stored on the model; capacities stay in the catalogue. Duties stay
**derived, never stored** - `Query.AirHandlingUnitDesignDuty` sums over every system a unit supplies, so
the general `AHU-01 -> Zone A/B/C` arrangement stays open even though the Part O workflow is one dwelling
per unit.

Selection rule: **smallest compliant, never nearest**; both sides checked independently; nothing compliant
is an explained refusal, never an undersized fallback; two products tied on size *and* rank refuse as
ambiguous.

## Current stage: the manufacturer catalogue seam

**The manufacturer template seam.** PR #79 left one thing deliberately undone: "no shipped product
catalogue - selection is a pure function over supplied descriptors, and the `SAM_Systems` reader mirroring
`Query.SystemCapabilityDescriptors` / `CapabilityIndex.JSON` is the one remaining seam needed to drive this
from Grasshopper." That seam is now built, and the data model behind it is deliberately shaped for
Iteration 3. **No Iteration 3 behaviour is implemented, and PR #79's selection kernel is unchanged.**

### The five quantities, and the one that is new

Iteration 2 keeps four quantities apart. The template work adds a fifth that is easily mistaken for
equipment capability, and the whole design turns on the difference:

| Quantity | Lives in | Example |
|---|---|---|
| Part F requirement | `PartFSpaceData.Terminals[].ContinuousDesignFlowRate_Lps` | the Approved Document's demand of a room |
| Equipment capability | `VentilationUnitTemplate.MaximumSupplyFlowRate_Lps` / `...Extract...` | what the fan can move |
| **Published duty point** | `VentilationUnitPerformanceTable` axis values | *conditions the manufacturer measured at* |
| Design airflow | `VentilationTerminal.DesignFlowRate_Lps` | what this dwelling is designed to move |
| Operating airflow / leaving-air temperature | Iteration 3, hourly | what it moves at 3pm in August |

**A published duty point is never read as a capacity.** The Nuaire selection tables are published at
50-120 l/s; 120 l/s is the last column of a table, not a statement about the fan. The unit's capacity comes
from a different figure on the same brochure pages - the fan static-pressure chart's Curve 1 free-air (0 Pa)
endpoint, 150 l/s - so the catalogue ships it at 150/150 l/s, and the two numbers are asserted **not** to be
equal so they cannot be confused again. Curves 2 and 3 (120 and 90 l/s) are the same fan at lower speed
settings: operating states of one physical product, never catalogue entries. See
`SAM_Systems/PROJECT_PROGRESS.md`, *THE CAPACITY COMES FROM THE FAN CURVE*.

### What was added

**`SAM.Math`** - one class, because there was a one-dimensional and a two-dimensional interpolator and
manufacturer data is routinely three-dimensional:

- `MultilinearInterpolation` - N-dimensional regular-grid interpolation, and **arithmetic only**: it is
  deliberately not an `IJSAMObject`, because what gets written to a file is the manufacturer table, not an
  interpolator. Exact at the nodes (the bracketing fraction of a coordinate sitting on an axis value is
  exactly 0 or exactly 1, so a lookup gives the published number back bit for bit). `Calculate` **never
  extrapolates** and answers `NaN` outside the grid; `CalculateClamped` and `CalculateExtrapolated` are
  separate, named methods. N-D rather than 3-D-specific because this feature already uses it at **1-D**
  (the control curve) and **3-D** (the performance table) through one code path.

**`SAM.Analytical`** - the vocabulary, no data:

- `VentilationUnitPerformanceAxis` / `VentilationUnitPerformanceOutput` / `VentilationUnitPerformanceTable` -
  a manufacturer's raw table: the conditions it was measured at, in the units it was published in, and every
  measured value, flattened row-major. Shape is data - a two-condition table and a four-condition table are
  the same type, and a product that also publishes an input power needs no schema change.
- `FlowFractionControlCurve` - the controller ramp as data (22 degC -> 0.30, 26 degC -> 1.00), carrying its
  **own** domain policy. Nothing generic holds a hard-coded 22 or 26.
- `VentilationUnitTemplate` (`SAMObject`) - identity, cooling-module model, **source**, both maximum
  airflows (`NaN` = unresolved), rank, performance table, control curve.
- `Enums.PerformanceDomainPolicy` - `Refuse` (default) / `ClampToDomain` / `OuterCellLinearExtrapolation`.
- `Query.CapacityDescriptor` / `CapacityDescriptors` / `UnselectableVentilationUnitTemplates` /
  `MatchingVentilationUnitTemplate` - the mapping into PR #79's `VentilationUnitCapacityDescriptor`, and the
  identity lookup Iteration 3 will cross to reach performance data from what the model stores.
- `Query.PerformanceValue` / `SupplyAirTemperature_C` / `CombinedCoolingCapacity_kW` - lookups by **named**
  condition, so the axis order in a hand-edited file is never load-bearing.

**`SAM_Systems`** - the data and the reader (see that repository's `PROJECT_PROGRESS.md`).

### Review pass (same session, after PR #79 merged)

A focused architecture/diff review challenged the size of the new code and removed what it could not
justify. **Removed, all of it dead and untested:**

| Removed | Why |
|---|---|
| `MultilinearInterpolation`'s whole `IJSAMObject` surface - the interface, the `JsonObject` constructor, `FromJsonObject`, `ToJsonObject` and its private JSON number reader | Nothing serialises an interpolator. Verified by search: the type is never serialised in code and never named in any resource file. It was ~135 lines of untested serialiser, **and a third copy of the `JsonElement` workaround** - removing it leaves the workaround in the two places that genuinely parse a wire format |
| `MultilinearInterpolation.Load` made private | The grid is now immutable after construction, which is what makes a cached interpolator safe to share |
| `VentilationUnitPerformanceTable.Axes` and `.Outputs` | Zero callers, and each deep-copied the entire 96-value table on every property read |
| `VentilationUnitPerformanceTable.Interpolation(...)` made private | Zero external callers, and it leaked a `SAM.Math` type through this type's public surface. Every read now goes through `Value(...)`, which is also where the domain policy is applied - so there is no way to reach the arithmetic while bypassing the decision about what happens outside the published range |

**Added:** a cap of 24 axes in the grid validator. Corners are enumerated as one bit per dimension, and a
shift of 32 or more wraps silently in C# - it would enumerate the wrong corners rather than fail. Far
beyond any real performance table, but a wrong answer is worse than a refusal.

`MultilinearInterpolation.cs` 630 -> 495 lines; `VentilationUnitPerformanceTable.cs` 586 -> 547. No
behaviour change: the same 32 focused tests pass unmodified.

**Kept, and why:** the two near-identical `...PerformanceAxis` / `...PerformanceOutput` types (an axis must
be strictly increasing and an output need not be - merging them would need a flag that decides which
validity rule applies); `FlowFractionControlCurve` delegating to a one-axis `VentilationUnitPerformanceTable`
(it inherits exactness, validation, both domain policies and serialisation rather than duplicating them);
and the Iteration-3 lookup seams (`PerformanceValue`, `SupplyAirTemperature_C`, `CombinedCoolingCapacity_kW`,
`MatchingVentilationUnitTemplate`) which have no production caller yet **by design** and are covered by tests.

### Decisions worth not re-litigating

1. **Litres per second, not SI cubic metres per second.** Every airflow in the Part F and Part O work is
   `_Lps`, and a template in m3/s would need converting at exactly the seam where a units mistake is least
   visible. Temperatures are degC and cooling is kW because that is what the brochure publishes - raw
   manufacturer data is never converted on the way in.
2. **The template lives in `SAM.Analytical`, the catalogue in `SAM_Systems`.** Same seam as Iteration 1a:
   the core library owns the vocabulary and the selection rule and carries no manufacturer list; which
   products exist is a fact about whoever is asking.
3. **Refuse, never default, on a hand-edited file.** A missing capacity is a legal named state; a *badly
   written* one refuses. A missing `Rank` refuses the whole catalogue (a missing rank is a unique 0, 0 sorts
   first, and the unranked entry becomes the preferred answer) - the trap `SystemCapabilityDescriptors`
   was hardened for.
4. **Extrapolation is a named, non-default policy that stores nothing** - see *The three authorities*
   above. The supplied engineering spreadsheet is a transcription aid, not an architectural specification,
   and nothing derived from it - no formula, no fitted curve, no value outside the manufacturer's published
   domain - was imported. The authoritative source is the Nuaire brochure.

### The three authorities, and which is which

```
Nuaire published table       MANUFACTURER AUTHORITY          the catalogue holds this, and only this
SAM interpolation/extrap.    explicit generic policy         Refuse | ClampToDomain | OuterCellLinearExtrapolation
legacy IES spreadsheet       HISTORICAL, NON-AUTHORITATIVE   never stored, never asserted, not reconstructed
```

The policy formerly called `LegacyLinearExtrapolation` is now
**`OuterCellLinearExtrapolation`**. The old name claimed something that is not true: comparison against the
legacy spreadsheet's own derived figures shows they **disagree** - it gives roughly 14.9 degC at
26 / 23 / 80 where SAM gives 15.1, and roughly 18.9 degC at 26 / 26 / 120 where SAM gives 19.4. The name
was chosen over the shorter `LinearExtrapolation` because it says which linear extrapolation it is -
continuation of the outermost cell, not a fit through all the points - and this codebase prefers names that
cannot be read the wrong way.

That spreadsheet is historical reference material: its derivation is unavailable, the engineer who produced
it is unavailable, and it is not being reconstructed. No polynomial was fitted and no ramp expression was
reverse-engineered. Exact compatibility with that tool, should a project ever need it, is a **separate task
requiring an authoritative specification or validated acceptance data** - not a reason to bend SAM's policy.

SAM's own extrapolation arithmetic **is** pinned, in
`VentilationUnitCatalogueTests.SAMsLinearExtrapolation_IsPinnedOutsideThePublishedDomain`, at nine points -
five below the published 29 degC external floor, four above the published 26 degC entering ceiling. Every
value was computed independently in Python before being asserted in C#; the two agree to 1e-6. The test
states in its own documentation that it pins SAM arithmetic and **not** IES compatibility.

### Deployment - closed, with evidence

`Query.DefaultVentilationUnitDirectory()` resolves `<resources>/Analytical/Systems/VentilationUnit`, where
`Analytical/Systems` is derived by `Core.Query.ResourcesDirectory(setting, assembly)` from the assembly
name `SAM.Analytical.Systems` (leading `SAM.` stripped, dots to separators) and the leaf comes from
`AnalyticalSystemSettingParameter.DefaultVentilationUnitDirectoryName`.

**The existing mechanism already carries it - no new mechanism was added.** The chain, traced end to end:

1. `SAM_Systems/Grasshopper/SAM.Analytical.Grasshopper.Systems.csproj` has an **unconditional**
   `Target Name="PostBuild" AfterTargets="PostBuildEvent"` that runs
   `xcopy "$(SolutionDir)\files\resources" "$(APPDATA)\SAM\resources" /Y/I/E/S` and the same to
   `%USERPROFILE%\Documents\SAM\resources`. `/E/S` is recursive, and it copies the **whole**
   `files/resources` tree - which is why `SystemEnergyCentre/CapabilityIndex.JSON` reaches installs today.
2. CI `installer.yml:702` copies `%USERPROFILE%\Documents\SAM` into `stage\user\Documents\SAM` after the
   full release build (recorded in `PLAN_SAM_TAS_SPLIT.md:228`).
3. `SAM_Installer/Build_Installer.iss:62` stages `build\user\Documents\SAM\resources\*` into
   `{userappdata}\SAM\resources`, recursively.
4. At runtime `Core.Query.ResourcesDirectory` finds it under `Documents\SAM\resources` or, failing that,
   beside the executing assembly - which is where step 1 and step 3 both put it.

**Empirical proof on this machine**, after building `SAM_Systems.sln`: the catalogue is present and
**byte-identical to the repository source** (11042 bytes, md5 `f56eabebe6529afed9dc64512c4fb222`) at both

```
%APPDATA%\SAM\resources\Analytical\Systems\VentilationUnit\VentilationUnitCatalogue.JSON
%USERPROFILE%\Documents\SAM\resources\Analytical\Systems\VentilationUnit\VentilationUnitCatalogue.JSON
```

with `SystemEnergyCentre/CapabilityIndex.JSON` sitting beside it in the same tree.

Two focused tests lock the invariant **machine-independently**, so a clean CI runner checks the same thing:
`TheCatalogue_SitsWhereTheRuntimeResolverWillLookForIt` re-derives the assembly-name segment and the setting
leaf and asserts the shipped file is at exactly that relative path (it fails if the folder moves, the
setting is renamed, or the assembly is renamed), and `TheReaderAndTheShippedFile_AgreeOnTheFileName` stops
either side of the file name being renamed alone.

### A defect found on the way

`SAM.Math.LinearInterpolation` and `BilinearInterpolation` read numbers with
`GetValue<object>()` + `Core.Query.IsNumeric`. A **parsed** JSON number is backed by a `JsonElement`, which
is not a numeric CLR type, so those classes silently deserialise empty from any saved file. The new code
uses `JsonValue.TryGetValue` instead; the two pre-existing classes were left untouched here and fixed the
same day as separate work - **`fix/json-numeric-roundtrip`, PR #81, merged** (see "Latest" at the top).

### Files added (`SAM`)

```
SAM/SAM.Math/Classes/Interpolation/MultilinearInterpolation.cs
SAM/SAM.Analytical/Enums/PerformanceDomainPolicy.cs
SAM/SAM.Analytical/Classes/System/PerformanceJson.cs                    (internal)
SAM/SAM.Analytical/Classes/System/VentilationUnitPerformanceAxis.cs
SAM/SAM.Analytical/Classes/System/VentilationUnitPerformanceOutput.cs
SAM/SAM.Analytical/Classes/System/VentilationUnitPerformanceTable.cs
SAM/SAM.Analytical/Classes/System/FlowFractionControlCurve.cs
SAM/SAM.Analytical/Classes/System/VentilationUnitTemplate.cs
SAM/SAM.Analytical/Query/CapacityDescriptor.cs
SAM/SAM.Analytical/Query/PerformanceValue.cs
SAM/SAM.Tests/PartOVentilationUnitTemplateTests.cs
```

**Nothing existing in `SAM` was modified.** Every file is an addition.

### Tests (this session)

| Suite | Result |
|---|---|
| `PartOVentilationUnitTemplateTests` (new, `SAM.Tests`) | **32 / 32** |
| `SAM.Tests` (full) | **1583 / 1583** (was 1551 + 32) |
| `SAM.Analytical.Systems.Tests` (full, incl. 28 new) | **68 / 68** |
| `SAM.Analytical.Systems.Mollier.Tests` | **123 / 123** |
| `SAM.Analytical.Tas.TM59.Tests` (against the rebuilt DLL) | **649 / 649** |

### The legacy IES/TAS workbook is not what was supplied

Recorded because a later session will otherwise re-derive it. The file supplied as
`02_Nuaire_Performance_Interpolation.xlsm` is Duncan MacArthur's **"Nuaire IZAM Vent Rates.xlsm"** - the
attachment named in the email thread - created 2017 by Richard Summers, last saved by Michal Dengusiak on
2026-08-28. Full forensic read of the package: four worksheets, none hidden (`Sheet1`, `AHU1`, `Nuaire`,
`Sheet2`); no defined names; no chart parts; no external workbook links; one VBA code module (`Module1`,
matching the supplied `.bas`). **478 formula cells, all of two kinds**: 476 instances of
`(E{n}/1000)*$B$1` / `(G{n}/1000)*$B$1` on `Sheet1` - litres per second to kilograms per second at
1.21 kg/m3 - and two scratch cells on `Nuaire` (`=80*0.3`, `=0.3^2`). The strings `ramp` and `interpolat`
appear nowhere in any part, text or binary; `IES` appears nowhere (earlier apparent hits were the substring
in "properties").

So this workbook contains **no interpolated table, no external axis below 29 degC and no ramp expression**.
Its `Nuaire` sheet is an 80 l/s slice of the manufacturer table at external 29/32/34; its `Sheet2` is the
full 3 x 4 x 8 table as static values with hand-written entry instructions. The IES workbook Michal
describes in his 10 October 2025 email is a **different, third-party file that was not supplied**.

An earlier note in this file said the workbook had "five formulas". That count was wrong - it missed
Excel's shared-formula stubs, which carry no formula text of their own. The conclusion it supported
(no interpolation logic in this workbook) is unchanged and is now established from the whole package.

### Open items

- **The Nuaire capacity is unresolved and must stay that way until sourced.** The July 2022 brochure states
  no maximum supply or extract airflow. The related email thread mentions "80-90 L/s" as the project's own
  assumption, which is not a manufacturer statement and was not written into the catalogue.
- ~~Deployment~~ **closed** - traced, proved byte-identical in both deployed trees, and locked by two
  machine-independent tests. See *Deployment - closed, with evidence* above.
- **No `Application` eligibility on the ventilation unit catalogue.** `CapabilityIndex.JSON` filters
  domestic vs commercial templates; the unit catalogue has no equivalent because PR #79's selection kernel
  has no application concept to consume one. Worth revisiting before a commercial AHU is catalogued.

## Codex review round (2026-08-29) - PR #80, three P2 findings, all fixed

Codex reviewed PR #80 (`8813bed0`) and posted three P2 findings, all against the manufacturer catalogue
seam described above. All three were accepted and fixed on top of `8813bed0`; the architecture, scope and
every "decision worth not re-litigating" above are unchanged.

### 1. Typed C/l-s/kW lookups did not check the table's declared units

`SupplyAirTemperature_C` / `CombinedCoolingCapacity_kW` resolved axes/output by name only and never checked
that the table's `Unit` fields actually said degC/l-s/kW - a table published in Fahrenheit, m3/s or any
other unit, or one that simply omitted the optional `Unit` field, would have its raw numbers handed back as
if they were Celsius/litres-per-second/kilowatts.

**Fixed** by hardening the typed convenience seam only, per the review's explicit scope: `VentilationUnitPerformanceAxis`/
`VentilationUnitPerformanceOutput` gained `Unit_DegreesCelsius` / `Unit_LitresPerSecond` / `Unit_Kilowatts`
constants, and `PerformanceValue.cs`'s two typed methods now check every required axis/output unit before
doing the lookup, refusing with `NaN` on any mismatch or missing unit. The generic raw
`Query.PerformanceValue(table, outputName, conditions, policy)` is completely unaffected and still answers
from a table in any units the manufacturer published - no general unit-conversion framework was added.
`VentilationUnitPerformanceTable.IsValid` is unchanged (`Unit` was never part of validity and still isn't).

Test: `PartOVentilationUnitTemplateTests` section H (`TypedLookup_AcceptsATableDeclaringDegCLpsAndKw`,
`TypedLookup_RefusesWhenARequiredAxisUnitIsMissing`, `TypedLookup_RefusesAWrongTemperatureUnit`,
`TypedLookup_RefusesAWrongAirflowUnit`, `TypedLookup_RefusesAWrongOutputUnit`,
`GenericPerformanceValue_StillReadsArbitraryManufacturerUnits`). Confirmed against the real shipped Nuaire
catalogue in `SAM_Systems`: every axis/output there already declares exactly degC/l-s/kW, so this fix
changes nothing about the one product currently catalogued.

### 2. A table reload racing an in-flight interpolator build could leave a stale value cached

`VentilationUnitPerformanceTable.Interpolation` read the (unlocked) `axes`/output fields, built a
`MultilinearInterpolation` from them, then wrote the result into the cache under a second, separate lock. A
`FromJsonObject` reload landing between those two steps clears the cache and installs new axes/outputs, and
the in-flight build - built from the now-superseded data - would then be written into the *new* generation's
cache, so a later lookup for the same output could silently read a value from the table the reload just
replaced.

**Fixed** with a generation counter: `FromJsonObject` increments `generation` in the same lock that clears
the cache, and `Interpolation` captures `generation` alongside its axes/output snapshot before building,
then only publishes the built interpolator into the cache if `generation` is unchanged - an interpolator
built against generation N can never be published as generation N+1's value. No new locking/concurrency
infrastructure beyond the counter and the existing lock.

Test: `AReloadDuringAnInFlightLookup_NeverLeavesTheOldValueCached`, made deterministic (no threads, no
sleeps) via a test-only seam - `VentilationUnitPerformanceTable.OnInterpolationSnapshotCaptured`, an
`internal` hook (exposed to `SAM.Tests` via a new `InternalsVisibleTo` in
`SAM.Analytical/Properties/AssemblyInfo.cs`) that fires exactly in the window the race used to occupy, so
the test can trigger the reload synchronously from inside it and reproduce the race on one thread every
time.

### 3. Corner enumeration counted a corner for every axis, including singleton ones

`MultilinearInterpolation.Interpolate` enumerated `2^dimensions` corners regardless of which axes actually
varied. A singleton (one-value) axis has no upper corner and every corner touching it was already
zero-weighted and skipped, but the corner was still allocated and iterated first - for a table with (say)
24 singleton axes and one real value, that is 16,777,216 iterations and `int[24]` allocations to answer a
lookup with exactly one possible answer.

**Fixed** by enumerating corners only over the axes with more than one value (`dimensions_Varying`);
`corners = 1 << dimensions_Varying.Count`. Singleton axes stay fixed at index 0 in every corner and
contribute no bit, matching the pre-existing "no corner" comment that was already there but not acted on
structurally. N-D behaviour, exact-at-node behaviour, clamping, outer-cell extrapolation, row-major
indexing and "singleton axis = constant" are all unchanged - confirmed by the identity test below, not just
inspection. The 24-axis validator cap was left as-is (it already bounds the varying-axis count too, since
varying <= total).

Test: `ManySingletonAxes_AnswerWithTheSingleRealValue`, `AMixOfSingletonAndVaryingAxes_InterpolatesOnlyOverTheVaryingOnes`,
`SingletonAxesInflated_MatchTheEquivalentLowerDimensionalTable` (the last one proves the fix numerically:
the full table with its singleton axes still present and the same table with them stripped out entirely
share one flattened values array by construction, and both must - and do - interpolate to the same number).

### Validation

| Suite | Result |
|---|---|
| `PartOVentilationUnitTemplateTests` (focused, incl. 10 new) | **42 / 42** |
| `SAM.Tests` (full) | **1593 / 1593** |
| `SAM.Analytical` / `SAM.Tests` Release build | 0 errors |
| `SAM_Tas` regression | **not run** - grepped `SAM_Tas` for every changed symbol (`VentilationUnitPerformanceTable`, `PerformanceValue`, `SupplyAirTemperature_C`, `CombinedCoolingCapacity_kW`, `MultilinearInterpolation`); zero references. Nothing in this round touches SAM.Analytical surface SAM_Tas consumes |

New commit on top of `8813bed0` on `feature/parto-iteration2-manufacturer-catalogue`. Not merged.

## Review round 1 - the four correctness fixes at `c67cf5d4`

Applied on top of the first Iteration 2 commit `41a02d4e`. Two were found by Codex, two in review of the
PR itself.

**1. Reductions consume available design headroom, not proportional total duty.** *(Codex P1)*
`Allocate` shared a negative change in proportion to each room's *total duty*, which handed a share to a
room sitting exactly on its Part F floor, saw that share breach it, and refused the whole change as
impossible - while another room held all the headroom needed. Reversing a previous targeted change, the
most ordinary thing an optimisation does, therefore failed. A reduction is now shared in proportion to
`max(0, duty - requirement)`, in tiers (cooking-priority rooms first on the extract side, then the rest),
capped at each room's own headroom, and only refuses when the total removable headroom genuinely falls
short. **Note for the next agent:** on a dwelling the real `PartFCalculator` sized, both sides hold equal
removable headroom, so a reduction can *always* be balanced - the shortfall refusal is only reachable on a
model with asymmetric requirement totals, which is why its test builds one by hand.

**2. A targeted change never touches another ventilation system's terminals.** *(Codex P1)*
A duty is summed per room and per direction, and `Modify.SetSpaceDesignFlowRate` writes *every* terminal of
that room and direction. A room holding terminals from this Part O system and from another one would have
had both rewritten - silently moving the other system's design duty while the result claimed the change
belonged to this one. New `TerminalsOfSystem` validates attribution for the targeted room **and every
candidate derived room** before anything is written, and **refuses** where a room is shared or where a
terminal belongs to no system at all. Refused rather than filtered: writing only the subset that belongs
here needs a system-scoped setter that does not exist, and inventing one would be a multi-system allocation
architecture Iteration 2 has no business introducing.

**3. The Part F floor is enforced at ROOM level, not only at the system total.** *(found in PR review)*
`ReconcileVentilationSystemDesignDuty` warned about a room below its requirement but only *refused* on the
system total. A bedroom 2.5 l/s under its requirement and a living room 2.5 l/s over it summed to a total
that agreed exactly, and the preparation passed - simulating a bedroom ventilated below its Approved
Document rate while reporting compliance. Surplus in one room is not tradeable against a shortfall in
another. New `RefuseSpace` refuses per room; above-requirement stays valid headroom and is still only
reported. This did **not** restore `Design == Required`.

**4. `ApplyTargetedDesignAirFlow` refuses an already-unbalanced dwelling before writing.** *(found in PR review)*
It previously checked balance *after* mutating and only added a warning, leaving `Successful` true - a
result claiming a valid balanced design for a dwelling that gains air it never loses. A targeted change and
its derived consequence move both sides equally and cannot close a pre-existing residual anyway. The check
is now a **pre-write refusal**, and the post-write balance assertion is a refusal rather than a warning.

## Review round 2 - the three further fixes on top of `c67cf5d4`

Codex did not re-raise fixes 1 or 2 above; it found three more. All three were verified independently
before being accepted.

**5. Every served room's Part F floor is validated before any mutation.** *(Codex P1)*
Fix 4 made a *balanced* dwelling the precondition, but balance is a property of the totals and the
Approved Document F floor is a property of each room, and one is not evidence of the other. A bathroom at
5 l/s against a 10 l/s requirement, offset by a kitchen at 15 against 10, totals 20 either way and
balances perfectly against 20 l/s of supply - so a +1 l/s bedroom target derived 1 l/s of kitchen extract,
never touched the bathroom, and reported success on a dwelling that was never compliant. The reduction
path already checked every candidate room; the **increase** path returned without any floor check, which
is why only increases were exposed.

The precondition now runs `Query.ReconcileVentilationSystemDesignDuty` over the whole served system and
refuses on its refusals - **reusing the one definition of compliant** rather than adding a second, so this
can never drift from what `Modify.PreparePartOIteration` refuses to simulate. Only its refusals are read;
its notes and warnings are about design headroom, which is legal. An already-invalid dwelling is refused,
**never repaired** - quietly fixing a room nobody targeted would be an unrequested design decision.

**6. A product identity that means two things is refused.** *(Codex P2)*
The model stores only `VentilationUnitReference` and looks capability up again by that identity, so a
catalogue with two entries sharing manufacturer/model/reference but rated 100/100 and 200/200 leaves no
single answer to "what did we select" - `SelectedVentilationUnitCapacityDescriptor` returned whichever came
first, making a unit's adequacy depend on catalogue order. `SelectSmallestCapableVentilationUnit` now scans
the whole valid catalogue for identity collisions before choosing anything and refuses a conflicting one,
so an ambiguous identity is never written onto an air handling unit. The refusal names the pair in a fixed
ordinal order, so the same broken catalogue produces the same sentence however it was read. The lookup is
independently defensive and returns null on a conflict, for a unit selected from one catalogue and later
checked against another.

*Classification, stated deliberately:* an exact repeat (same identity, same capacities, same rank) is a
duplicated line in a hand-edited file and stays **harmless**, matching how `SelectPreferredCapableSystem`
already treats a duplicated template entry. Conflicting **rank** on one identity is treated as
**conflicting**, because rank decides selections and two answers for it is the same defect as two answers
for a capacity. The invariant: *stored product identity -> exactly one capability meaning.*

**7. A tolerance that cannot be compared against is refused.** *(Codex P2)*
Every Iteration 2 safety rule is a comparison against `tolerance_Lps`, so `double.NaN` made the derived
allocation, the imbalance refusal and the capacity check all evaluate false at once and the transaction
reported success on an unbalanced dwelling; an infinity is the same failure wearing the opposite mask.
New `Query.IsValidFlowRateTolerance` / `Query.FlowRateToleranceRefusal` define one rule (finite, `>= 0`,
zero meaning exact) and one sentence, applied at every Iteration 2 public entry point that takes a
tolerance:

| Entry point | Behaviour on an invalid tolerance |
|---|---|
| `ApplyTargetedDesignAirFlow` | refusal, zero writes |
| `SetSpaceDesignFlowRate` | refusal, zero writes |
| `SelectVentilationUnit` | refusal, nothing written to the unit |
| `SelectSmallestCapableVentilationUnit` | `VentilationUnitSelection.Refused` |
| `ReconcileVentilationSystemDesignDuty` | refusal |
| `IsVentilationUnitSufficient` | false, with the reason |
| `CapableVentilationUnits` | empty list - the return type carries no reason, and empty can never approve an undersized unit |
| `VentilationUnitCapacityDescriptor.IsSufficientFor` | false - a predicate that cannot show sufficiency does not |

Refused, never clamped: substituting a default would hide the caller's mistake behind an answer that looks
right, and the answer is a compliance statement.

## Review round 3 - two further guards on top of the round 2 head

Codex's third pass raised no P1 and did not re-raise anything. Two P2s, both verified and both real.

**8. A negative or infinite design duty is met by nothing, not by everything.** *(Codex P2)*
A capacity check is `maximum >= duty`, so a negative duty was satisfied by every non-negative capacity and
`SelectSmallestCapableVentilationUnit` returned the smallest product on the shelf as a successful answer to
a physically impossible design. Reachable through the public selector, or from a terminal deserialized with
a negative `DesignFlowRate_Lps`. A duty must now be finite and `>= 0` - zero stays valid, because a system
with no terminal of that direction really does move nothing - checked in the selector, in
`CapableVentilationUnits`, and in `IsSufficientFor` underneath both.

**9. An air handling unit the model does not hold is refused, not inserted.** *(Codex P2)*
`SelectVentilationUnit` resolved the unit by GUID and **fell back to the caller's object** when that failed.
The duty above it is resolved by the unit's *name*, which is how a ventilation system names its plant - so
a detached unit merely sharing a name with one in the model got a duty, looked selectable, and was written
to and added. The cluster would then hold two units of that name with the product reference on the wrong
one, and every name-based lookup afterwards - `Query.VentilationSystems` and the TAS export among them -
could resolve the original, unselected unit. It now refuses: a selection nothing can find again is worse
than no selection.

## Review round 4 - scoping the compliance read, and two more guards

**10. The room-level Part F check is scoped to THIS ventilation system.** *(Codex P1)*
Fixes 3 and 5 put the compliance decision on `ReconcileVentilationSystemDesignDuty`'s per-room duty, which
was summed from **every** terminal in the room regardless of system - pre-existing Iteration 1a code, but
newly load-bearing. A room holding a foreign system's terminal therefore had that air counted towards this
system's Approved Document F floor: the room check passed while this system's own terminal stayed short,
and a little headroom in a neighbouring room carried the system total too, so both halves of the check
rested on air this system does not move. The per-room duty is now filtered to the system being reconciled.

*Filtered rather than refused, deliberately:* this is a **read**, and the honest answer to "what does this
system put into this room" is available exactly. **Writes** stay conservative -
`ApplyTargetedDesignAirFlow` still refuses outright to touch a room it cannot attribute unambiguously,
because a write cannot be filtered the same way. For the single-system dwelling the Part O workflow builds
the filtered set is identical, so Iteration 1a behaviour is unchanged.

**11. Adequacy is resolved from the cluster's own unit.** *(Codex P2)*
`IsVentilationUnitSufficient` read the product reference off the **caller's** object while
`AirHandlingUnitDesignDuty` derived the duty from the model by name, so a detached same-named unit - or a
stale copy kept from before a re-selection - could report adequate on a selection the model does not hold,
suppressing the escalation an outgrown unit needs. Both halves of the comparison now come from the same
object, resolved by GUID, and a unit the model does not hold refuses.

**12. A terminal carrying an impossible duty refuses before redistribution.** *(Codex P2)*
`DesignFlowRate_Lps` is publicly settable and deserialized without a range check, so an infinite one is
reachable. `SetSpaceDesignFlowRate` shares a room total in proportion to what each terminal already
carries, and `finite * Infinity / Infinity` is `NaN` - which would have been written and reported as the
requested total while `VentilationTerminalDesignDuty_Lps` afterwards skipped it and read a silently wrong
duty. Every existing terminal duty is validated finite and non-negative before anything is written.

## Review round 5 - closing the all-or-nothing gaps fix 12 opened

No P1. Three P2s, two of which regressed the transaction's headline contract - worth noting that a
guard added in one round can break a promise made in another.

**13. Every room the transaction will write is preflighted, not just the target.** *(Codex P2)*
Fix 12 put the terminal-validity check inside the setter, and `ApplyTargetedDesignAirFlow` writes the
target first. A derived room holding a NaN terminal beside healthy ones sums to a room total that meets
its requirement - `VentilationTerminalDesignDuty_Lps` skips NaN - so every plan check passed, the target
was written, and only then did the derived write refuse. All-or-nothing, broken by the guard meant to
protect it. The check is now shared (`Modify.IsRedistributable`) and asked of the target **and every
planned derived room before the first write**.

**14. A share smaller than the tolerance is still applied.** *(Codex P2)*
The apply loop skipped a derived room whose share was within tolerance. A change well above tolerance can
divide into shares that are each below it - 1.5 l/s across two rooms against a 1 l/s tolerance gives two
0.75 l/s shares - so all of them were skipped, the target was written, nothing balanced, and the post-write
check refused a change already made. The tolerance decides whether a *change* is worth making, which is
settled before planning; it does not get to veto the pieces that change is made of. A room is now skipped
only where it genuinely does not move.

**15. A unit with no design duty is unknown, not adequate.** *(Codex P2)*
`IsVentilationUnitSufficient` ignored `AirHandlingUnitDesignDuty`'s return value, so a unit whose systems
or terminals had been removed derived 0/0 - which every non-negative capacity satisfies - and was reported
adequate. `Modify.SelectVentilationUnit` already refuses that case; adequacy now agrees with it.

## Deliberate behaviour change carried from the first commit

`Query.ReconcileVentilationSystemDesignDuty` compared design duty and requirement with an **absolute**
difference, hard-coding `Design == Required` and making the Iteration 2 invariant impossible to express.
It is now **one-sided**: below the requirement refuses (at room level *and* system level, per fix 3), above
it is design headroom and is reported. `PartOBaseMVHRTests.ADutyThatDisagreesWithTheRequirement_Refuses`
was split into `ADutyBelowTheRequirement_Refuses` and
`ADutyAboveTheRequirement_IsReportedAsHeadroomAndNotRefused`. This is the only Iteration 1a behaviour change
in the PR.

### Files changed (this session)

`SAM` - **all Iteration 2 production changes are in `SAM.Analytical` alone.**
- `SAM/SAM/SAM.Analytical/Classes/System/VentilationUnitReference.cs` (new) - the product identity stored on the unit.
- `SAM/SAM/SAM.Analytical/Classes/System/VentilationUnitCapacityDescriptor.cs` (new) - the catalogue entry; capability only.
- `SAM/SAM/SAM.Analytical/Classes/System/VentilationUnitSelection.cs` (new) - Selected or Refused, plus headroom.
- `SAM/SAM/SAM.Analytical/Classes/System/DesignAirFlowAdjustment.cs` (new) - one room's move, carrying `IsDerived`.
- `SAM/SAM/SAM.Analytical/Classes/System/DwellingDesignAirFlowChange.cs` (new) - targeted + derived + duties, all-or-nothing.
- `SAM/SAM/SAM.Analytical/Query/CapableVentilationUnits.cs` (new) - the pure selection rule.
- `SAM/SAM/SAM.Analytical/Query/PartFRequiredFlowRate.cs` (new) - the immutable floor, per terminal / space / system.
- `SAM/SAM/SAM.Analytical/Query/AirHandlingUnitDesignDuty.cs` (new) - AHU<->system resolution, derived duty, capacity check;
  the identity lookup is conflict-defensive (fix 6).
- `SAM/SAM/SAM.Analytical/Query/IsValidFlowRateTolerance.cs` (new) - the one tolerance rule and the one refusal sentence (fix 7).
- `SAM/SAM/SAM.Analytical/Modify/SelectVentilationUnit.cs` (new) - binds a selection to one unit.
- `SAM/SAM/SAM.Analytical/Modify/SetSpaceDesignFlowRate.cs` (new) - the primitive; writes what it is told, does **not** rebalance.
- `SAM/SAM/SAM.Analytical/Modify/ApplyTargetedDesignAirFlow.cs` (new) - the transaction; fixes 1, 2 and 4 live here.
- `SAM/SAM/SAM.Analytical/Query/VentilationSystemDesignDuty.cs` - one-sided reconciliation; fix 3 (`RefuseSpace`) here.
- `SAM/SAM/SAM.Analytical/Enums/Parameter/AirHandlingUnitParameter.cs` - `VentilationUnitReference` member (enum was empty).
- `SAM/SAM/SAM.Analytical/Classes/PartOIterationPreparation.cs` - `VentilationUnitSelections`.
- `SAM/SAM/SAM.Analytical/Modify/PreparePartOIteration.cs` - optional catalogue threaded into the per-dwelling loop.
- `SAM/SAM/SAM.Tests/PartOVentilationUnitSelectionTests.cs` (new) - the Iteration 2 suite.
- `SAM/SAM/SAM.Tests/PartOBaseMVHRTests.cs` - the split reconciliation tests plus the room-level floor regression.
- `SAM/PROJECT_PROGRESS.md` (this file).

**No `SAM_Systems` and no `SAM_Tas` production changes in Iteration 2.**

*Not part of this PR:* diagnosing why `SAM_Tas` would not compile produced
`SAM_SolarCalculator/build/SAM.{Core,Geometry}.SolarCalculator.dll` as local build artefacts. They are
untracked deployment output, they belong to no repository's source, and BuildAlls regenerates them.

## Tests

`PartOVentilationUnitSelectionTests` - **67 facts** (37 at `41a02d4e`, and one replaced and thirty added
across five review rounds - eight of them `[Theory]` cases):
- Selection: smallest compliant, exact match, undersized rejected, supply/extract independent, refusal
  determinism, rank ties, catalogue-order independence.
- Authority separation: capacity never written into a requirement, capacity never taken up as design,
  design changes write no runtime airflow.
- Dwelling isolation: two dwellings select independently and pick different products; changing one does
  not touch the other.
- Targeted vs derived: exactly one targeted room; derived movements flagged and totalling the balancing
  delta; derived extract follows the existing allocation strategy.
- Network recalculation: re-preparation rebuilds transfer paths and air movements; supply == extract
  afterwards; AHU duty follows.
- Capacity: below rating keeps the unit; exactly at rating valid; above rating exhausts and escalates.
- Identity: product survives serialization on the unit and through the cluster; two units share a product
  with independent duties.

New with the review fixes:
- `AReductionConsumesAvailableHeadroom_AndReversesATargetedChangeExactly` (replaces the test that pinned
  the wrong behaviour, `ATargetedChangeThatCannotBeBalanced_RefusesAndWritesNothing`)
- `AReductionBeyondAllAvailableHeadroom_RefusesAndWritesNothing`
- `AReductionExactlyAtAvailableHeadroom_Succeeds`
- `ASpaceSharedWithAnotherVentilationSystem_RefusesAndTouchesNeitherSystem`
- `ASpaceHoldingAnOrphanTerminal_Refuses`
- `AnAlreadyUnbalancedDwelling_RefusesBeforeWritingAnything`
- `EverySuccessfulTransaction_LeavesTheDwellingBalanced`

`PartOBaseMVHRTests` - **34 facts**, including the new
`ARoomBelowItsRequirement_RefusesEvenWhereTheSystemTotalAgrees` (fix 3).

## Validation

**Final validation at merge:**

```
PartOVentilationUnitSelectionTests   67 / 67
PartOBaseMVHRTests                   34 / 34
SAM.Tests                          1551 / 1551
SAM.Analytical.Systems.Tests         40 / 40
SAM.Analytical.Tas.TM59.Tests       649 / 649
Release build                         PASS
PR CI                                 PASS
```

- **`SAM.Tests`: 1551 passed, 0 failed** (1548 / 1545 / 1541 / 1527 after rounds 4/3/2/1; 1513 at Iteration 1a).
- **`SAM.Analytical.Systems.Tests`: 40 passed, 0 failed.**
- **`SAM.Analytical.Tas.TM59.Tests`: 649 passed, 0 failed** - **run against the deployed Iteration 2
  `SAM.Analytical.dll`** after a BuildAll, with **symbol/type presence verified in that deployed DLL**, so
  the result is genuinely against this work and not against a stale deployment. No `SAM_Tas` production
  code calls any changed API; its only touchpoint is `PreparePartOIteration`'s four-argument form, which is
  source-compatible and defaults to Iteration 1a behaviour.
- Release build clean. PR #79 CI green (`build (Release)`, `test (Release)`, `spdx`) at every head,
  including the final code head **`ff967766`** and the documentation head **`d15a55cd`**.

**Trap worth knowing.** `SAM_Tas` references **deployed** DLLs by `HintPath`
(`..\..\..\SAM\build\SAM.Analytical.dll`), not project references, so running its suite without a BuildAll
silently tests whatever was last deployed rather than your working tree - an earlier "649 passed" in this
session was exactly that and meant nothing. Rebuilding it also needs
`SAM_SolarCalculator\build\SAM.{Core,Geometry}.SolarCalculator.dll` present, and the COM-interop projects
need **.NET Framework MSBuild** (`dotnet build` fails with MSB4803 on `ResolveComReference`). Deploy first,
confirm `SAM\build\SAM.Analytical.dll` is newer than your changes, then run the suite.

## Codex reviews of PR #79

| Finding | Severity | Resolution |
|---|---|---|
| Allocate reductions from available headroom before refusing | P1 | Fixed - fix 1 above; the test that pinned the wrong behaviour was replaced with a reversibility test |
| Scope balancing terminals to the selected ventilation system | P1 | Fixed - fix 2 above; refuses on a shared or unattributed room, with a regression asserting zero writes in both systems |
| Record Iteration 2 in `PROJECT_PROGRESS.md` | - | Fixed - this update |

Fixes 3 and 4 were **not** found by Codex; they came out of reviewing the PR against the agreed invariant.

**Round 2, at `c67cf5d4`.** Codex did not re-raise round 1's findings (GitHub re-anchors old comment
bodies to the new head - check `original_commit_id`, not `commit_id`, to tell an old comment from a new
one). Three new findings, all verified independently and all real:

| Finding | Severity | Resolution |
|---|---|---|
| Refuse balanced dwellings with room-level shortfalls | P1 | Fixed - fix 5 |
| Reject duplicate identities with conflicting capacities | P2 | Fixed - fix 6 |
| Reject NaN tolerances before balancing | P2 | Fixed - fix 7 |

**Round 3.** No P1, nothing re-raised. Two new findings, both verified and both real:

| Finding | Severity | Resolution |
|---|---|---|
| Reject negative design duties before selecting | P2 | Fixed - fix 8 |
| Refuse air handling units outside the cluster | P2 | Fixed - fix 9 |

**Round 4.** One P1 and two P2s, all verified and all real:

| Finding | Severity | Resolution |
|---|---|---|
| Scope room-floor checks to this ventilation system | P1 | Fixed - fix 10 |
| Resolve adequacy from the cluster-owned unit | P2 | Fixed - fix 11 |
| Reject non-finite existing terminal duties | P2 | Fixed - fix 12 |

**Round 5.** No P1. Three P2s, all verified and all real:

| Finding | Severity | Resolution |
|---|---|---|
| Preflight derived-room duties before mutating the target | P2 | Fixed - fix 13 |
| Apply shares whose aggregate exceeds the tolerance | P2 | Fixed - fix 14 |
| Refuse adequacy when no design duty exists | P2 | Fixed - fix 15 |

**Trend worth reading before commissioning a sixth round.** Rounds 1-4 each found a P1; round 5 found
none, and its three P2s were all consequences of round 4's own guards rather than defects in the Iteration
2 design. The findings are converging on input-hardening against states the Part O workflow does not
produce (detached objects, infinite property values, disconnected systems). That is worth having, and it
is no longer telling us anything about the architecture.

**Round 6, reviewed at `ff967766`** (submitted against `d15a55cd`, which is documentation-only over
`ff967766`, so the production code reviewed is `ff967766`). **No P1.** Two P2s, both verified
independently and both **deliberately accepted as non-blocking for this merge**:

| Finding | Severity | Disposition |
|---|---|---|
| Refuse terminals whose design duty is not established (`Query.VentilationSystemDesignDuty`) | P2 | **Accepted, not fixed** - see below |
| Reject `Undefined` flow classifications in `Modify.SetSpaceDesignFlowRate` | P2 | **Accepted, not fixed** - see below |

Both mechanisms are real and were traced through the code, not dismissed:

1. A `VentilationTerminal` whose `DesignFlowRate_Lps` is null makes
   `Query.VentilationTerminalDesignDuty_Lps` return null, which `VentilationSystemDesignDuty` coerces to
   `0` while still returning `true`. `AirHandlingUnitDesignDuty` then reports an established duty of 0/0,
   `IsValidDesignDuty(0)` passes, and `SelectVentilationUnit` would select and persist the smallest
   catalogue product for an unsized system. **Would only bite if such terminals were introduced through a
   future external or manual terminal-authoring path.**
2. `Modify.SetSpaceDesignFlowRate` does not independently reject `FlowClassification.Undefined`; given an
   `Undefined` terminal it would write shares that no supply/extract duty sum ever reads.

**These are NOT current Iteration 2 blockers.** The only production path that creates
`VentilationTerminal` objects is `Modify.RealizePartFVentilationTerminals`, which `continue`s on any
requirement without a finite `ContinuousDesignFlowRate_Lps` before passing `continuous_Lps.Value`, and
which sets the classification from a strict `IsExtract ? Extract : Supply` ternary. It therefore cannot
produce either state. For (2), the guard Codex asks for **already exists at the Iteration 2 entry point** -
`Modify.ApplyTargetedDesignAirFlow` refuses anything that is not Supply or Extract before doing anything
else - and `SetSpaceDesignFlowRate` is an internal primitive whose only production callers are inside that
guarded transaction. No Iteration 2 entry point is exposed to Grasshopper.

> **Revisit these guards if/when another production path begins creating `VentilationTerminal` objects,
> such as an importer, a Grasshopper authoring component, or a future `SAM_Systems` materialisation path.**

Do not reopen them before that happens.

## Remaining ambiguity / open items

- **The deliberate deferred seam: no shipped product catalogue.** Selection is a pure function over supplied
  descriptors, and the `SAM_Systems` reader mirroring `Query.SystemCapabilityDescriptors` /
  `CapabilityIndex.JSON` is what would make it reachable from Grasshopper. The Grasshopper component still
  calls the four-argument `PreparePartOIteration`; no input is exposed until there is a catalogue to feed
  it. **This is deferred on purpose, not missing by accident.**
- **Two accepted round-6 P2 guards**, merged unfixed on purpose: a null `DesignFlowRate_Lps` producing a
  0/0 design duty, and `SetSpaceDesignFlowRate` not independently rejecting `Undefined`. Neither is
  reachable through `RealizePartFVentilationTerminals`. **Revisit both when another production path starts
  creating `VentilationTerminal` objects** - an importer, a Grasshopper authoring component, or a future
  `SAM_Systems` materialisation path. Full reasoning under "Round 6" above.
- `SAM_Tas` validation complete - 649/649 against the deployed Iteration 2 DLL with symbol presence
  verified; see Validation above.
- Out of scope and untouched: heat-recovery efficiency, `HeatRecoveryUnit` -> `SystemExchanger`,
  `AirSystem` materialisation, runtime/`ticV` mapping, fan curves, pressure/duct/SFP/acoustics, the direct
  Part O/TBD route, and any broad Part O optimisation search. Iteration 2 adds the architectural operation
  that turns one targeted room change into a valid rebalanced design - **not** an algorithm that decides
  which room to target.

## Next step

Iteration 2 is merged; nothing on PR #79 remains open. **The immediate next seam is the `SAM_Systems` MVHR
catalogue reader:**

```
SAM_Systems MVHR catalogue reader
        ↓
VentilationUnitCapacityDescriptor list
        ↓
SAM.Analytical selection seam
        ↓
Grasshopper exposure
```

**Goal: make the already-merged Iteration 2 equipment-selection logic reachable from normal Grasshopper
without introducing a `SAM` -> `SAM_Systems` dependency.** Selection is already a pure function over
supplied descriptors, so the seam is a reader that produces a `VentilationUnitCapacityDescriptor` list -
mirroring `Query.SystemCapabilityDescriptors` / `CapabilityIndex.JSON` - and a Grasshopper input that feeds
it. The direction of the dependency is the whole point: `SAM.Analytical` must not learn about
`SAM_Systems`.

**Do NOT start Iteration 3 materialisation yet.**

**Longer-term direction (later work, not now):**

```
SAM analytical model
    -> explicit materialisation
    -> SAM_Systems
    -> SAM_Tas adapter
    -> TAS TPD
```

This remains the intended eventual route and is recorded here so it is not rediscovered, but it is
explicitly **later work**: heat-recovery efficiency, `HeatRecoveryUnit` -> `SystemExchanger`, `AirSystem`
materialisation, runtime/`ticV` mapping, fan curves, pressure/duct/SFP/acoustics, and any broad Part O
optimisation search all stay out until the catalogue seam above is in.

**Validation shortcut.** `SAM_Tas` compiles against the *deployed* `SAM\build\SAM.Analytical.dll`, so its
suite means nothing until a BuildAlls has run after your last edit. Check that DLL's timestamp against your
newest source edit, confirm a symbol you just added is actually in it, then build the test project with
**.NET Framework MSBuild** (`dotnet build` fails MSB4803 on the COM projects) and run
`dotnet test --no-build`. Do not build sibling repositories yourself - Michal runs BuildAlls.

---

## Previous session (2026-08-27, Iteration 1a accepted)
**The block was conservation, not the inter-zone air movement record.** TAS refuses to simulate a TBD in
which any one zone's air movements do not balance - building-wide balance is not enough - and every room
of a balanced heat recovery dwelling is individually out of balance by design. Two objects close it, and
neither adjusts a design duty:

- `Modify.AddPartFTransferAirMovements` routes each space's net through `PartFAirflowNetwork`, the same
  network Approved Document F paragraph 1.25 is assessed over. Where a net cannot be routed it **refuses
  and names the room** rather than inventing a route or connecting the room to outside.
- The unit's exhaust, added by `Modify.AddAirMovementObjects` as a movement to a destination of `null`.

`Query.AirMovementResidual` then sums every movement at each node - never matching route against route,
because these flows split and recombine - and `Modify.PreparePartOIteration` refuses on any node that does
not come out at zero.

Licensed acceptance, same dwelling / weather / period as Iteration 1b: **`differing=78835` of 78 840**
hourly temperatures, against `differing=0` before this work. TM59 takes the mechanical route with zero
strategy refusals, and every sized space reads `freshAirRate=0`, so the mechanical ventilation is the air
movements and nothing else. Evidence in
[`documentation/PartO-TAS-VALIDATION.md`](documentation/PartO-TAS-VALIDATION.md) §"Iteration 1a / Base
MVHR - the block resolved (2026-08-27)".

Full `SAM.Tests`: **1464 passed, 0 failed** (was 1455, +9). `SAM.Analytical.Tas.TM59.Tests`: **633
passed, 0 failed**, unchanged.

Two pre-existing defects were found and deliberately **not** fixed here: TAS reads an air movement's
stored flow as a mass flow in kg/s while SAM writes m3/s, and `SAM.Analytical.Tas.Modify.Simulate` reports
a refused simulation as a success.

---

## Previous session (2026-08-26, Iteration 1b)
**Milestone: Iteration 1b / Base Natural Ventilation is proven end to end** from an explicitly prepared SAM
dwelling, through authored opening behaviour, TAS simulation and comparable Part O / TM59 results, without
inventing an MVHR system.

The target architecture is now recorded durably in
[`documentation/PartO-ARCHITECTURE.md`](documentation/PartO-ARCHITECTURE.md) - the five-box separation
(requirement -> route -> equipment -> operating scenario -> simulation -> result), the full iteration
algorithm including the unimplemented 1a/2/3, and the wet-room investigation. It is indexed from
`PartF-HANDOVER.md`.

Full `SAM.Tests`: **1425 passed, 0 failed** (was 1397, +28). `SAM.Analytical.Tas.TM59.Tests`:
**624 passed, 0 failed** (was 620, +4).

**Licensed TAS A/B acceptance: PASSED.** See
[`documentation/PartO-TAS-VALIDATION.md`](documentation/PartO-TAS-VALIDATION.md) §"Iteration 1b / Base
Natural Ventilation - licensed A/B acceptance (2026-08-26)".

### What was wrong with the gate this replaces
The NV gate asked one question of a string - is it `"NV"`? - and treated **every other answer as a
mechanical dwelling**. `UV`, an empty Grasshopper panel, a typo, a stale word and a model with no zones all
reached `ApplyPartFVentilationRates` and wrote Approved Document F System 4 supply and extract onto every
sized space, successfully, with nothing downstream saying an MVHR system had been invented. It closed the
NV hole and left the rule that caused it in place.

Separately, `PartOIteration.BasePassive` asserts `Mechanical Ventilation At Design Rate = True`, and that
assumption is **inside the derived `OverheatingScenario.Key`**. Preparing an NV dwelling there produced a
true simulation filed permanently under a false claim.

## Completed (this session)
- **`Enums.PartOVentilationMode`** (new): `Undefined` / `NaturalVentilation` / `MVHR`. The stated Part O
  route. No fallback member, deliberately.
- **`Query.PartOVentilationMode(string, out refusal)`** (new): a total, explicit mapping.
  `NV`/`NaturalVentilation`/`Natural Ventilation`/`BaseNaturalVentilation` and
  `MVHR`/`MVRE`/`BaseMVHR` resolve; everything else refuses **with the reason**. `MV` refuses because
  "mechanical" is not a route - Part F System 3 (continuous extract) and System 4 (supply and extract with
  heat recovery) are different buildings and only System 4 is what `PartFCalculator` sizes. `UV` refuses
  because it selects the TM59 corridor criterion for a common space and says nothing about a dwelling.
- **`Query.PartOVentilationMode(zones, dictionary, out refusal)`** (new): the one route the assessed zones
  state, naming every zone that does not settle one. Mixed routes refuse; **no assessed zones refuses**,
  where the old gate kept applying.
- **`PartOIteration.BaseNaturalVentilation`** (new member, appended so nothing is renumbered) with its
  operating assumptions: `Openings Restricted = False`, **`Mechanical Ventilation At Design Rate = False`**,
  no boost, no summer bypass. `BasePassive` is documented as the historical name for `BaseMVHR` and
  deliberately **not** renamed - the member name is inside the key, so renaming is a migration.
- **`Query.PartOIterationVentilationMode`** (new): the route each iteration is defined over. `BasePassive`
  -> MVHR, `BaseNaturalVentilation` -> NaturalVentilation, everything else refuses (delegating to
  `PartOIterationOperatingMode` so the two cannot drift).
- **`Query.PartOPartFAirflowApplication`** rewritten as a total function of the route alone - no string, no
  model, no `SystemTemplate`. `RefuseMixed` becomes `RefuseUnstatedRoute`, which covers every way a route
  fails to settle.
- **`Modify.PreparePartOIteration`** restructured into four ordered gates: the stated route, the
  iteration's route (a mismatch refuses in **both** directions), the airflow (the Approved Document F
  operating condition is now asked for **only** on the MVHR route), the authored openings (reported, never
  acted on). `PartOIterationPreparation.VentilationMode` is on the result.
- **`SAMAnalyticalPreparePartOIteration`** `1.0.2` -> `1.0.3`: new `ventilationMode` output, rewritten
  documentation, route-aware messages. No decision moved back into the component.

### The route is stated, never inferred, and never written
No `SAM_System`, `SystemTemplate` or `InternalCondition.VentilationSystemTypeName` is read to decide what is
simulated - and none is **mutated to force it**, which would put the decision back into the metadata it was
taken out of. `AStaleMVREOnTheModel_DoesNotOverrideAnExplicitNVRoute_AndIsNotRewritten` prepares a dwelling
carrying `VentilationSystemTypeName = "MVRE"` on an explicit NaturalVentilation route and asserts both
halves.

## Deliberate behaviour changes (each reported, none accidental)
| Input | Before | Now |
|---|---|---|
| `UV` | Applied System 4 airflow | Refuses, naming the corridor criterion |
| `MV` | Applied System 4 airflow | Refuses, naming Systems 3 and 4 |
| Unrecognised word / empty panel | Applied System 4 airflow | Refuses, quoting the word |
| No assessed zones | Applied System 4 airflow | Refuses |
| `NV` at `BasePassive` | Skipped, prepared | Refuses; use `BaseNaturalVentilation` |
| `MVRE` at `BaseNaturalVentilation` | n/a (member is new) | Refuses; use `BasePassive` |
| `MVRE` / `MVHR` at `BasePassive` | Applied | Unchanged |
| `NV` at `BaseNaturalVentilation` | n/a (member is new) | Skips, prepares |

## Files changed
- `SAM/SAM/SAM.Analytical/Enums/PartOVentilationMode.cs` (new)
- `SAM/SAM/SAM.Analytical/Query/PartOVentilationMode.cs` (new)
- `SAM/SAM/SAM.Analytical/Query/PartOIterationVentilationMode.cs` (new)
- `SAM/SAM/SAM.Analytical/Enums/PartOIteration.cs`
- `SAM/SAM/SAM.Analytical/Enums/PartOPartFAirflowApplication.cs`
- `SAM/SAM/SAM.Analytical/Query/PartOPartFAirflowApplication.cs`
- `SAM/SAM/SAM.Analytical/Query/PartOOperatingAssumptions.cs`
- `SAM/SAM/SAM.Analytical/Query/PartOIterationOperatingMode.cs`
- `SAM/SAM/SAM.Analytical/Modify/PreparePartOIteration.cs`
- `SAM/SAM/SAM.Analytical/Classes/PartOIterationPreparation.cs`
- `SAM/Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalPreparePartOIteration.cs`
- `SAM/SAM/SAM.Tests/PartOIterationPreparationTests.cs`
- `SAM/SAM/SAM.Tests/OverheatingScenarioTests.cs`
- `SAM/documentation/PartO-ARCHITECTURE.md` (new)
- `SAM/documentation/PartO-TAS-VALIDATION.md`
- `SAM/documentation/PartF-HANDOVER.md`
- `SAM/PROJECT_PROGRESS.md` (this file)
- `SAM_Tas/SAM_Tas/SAM.Analytical.Tas.TM59.Tests/PartONaturalVentilationWorkflowTests.cs` (separate repo)

## Tests
Section I of `PartOIterationPreparationTests` was rewritten - the cases it has to cover are different cases
now - and sections J and K added:

- `AStatedRoute_ResolvesToItsMode` [Theory x8], `AnythingElse_IsNoRouteAtAll` [Theory x9, including `MV`,
  `UV`, `EOL`, `CAV`, `MVHRR`, empty and null], `MVAndUV_AreRefusedWithTheReasonTheyAreNotRoutes`.
- `AnUnstatedRoute_RefusesAndAppliesNothing` [Theory x4] - on the production path, asserting no model, no
  scenarios and the supplied model unmutated.
- `AZoneStatingNothing_IsNamedInTheRefusal`, `NoAssessedZones_Refuse`, `MixedRoutes_*`,
  `NVBesideAZoneStatingNothing_Refuses`.
- `TheMVHRRoute_KeepsTheExistingApplication` [Theory: MVRE, MVHR] - the mechanical-path guard, proving both
  spellings are one route.
- `EachBaseIteration_StatesItsRoute`, `AnNVDwellingAtTheMVHRIteration_Refuses`,
  `AnMVHRDwellingAtTheNVIteration_Refuses`,
  `TheTwoBaseIterations_AssertOppositeMechanicalVentilationAssumptions`,
  `TheTwoBaseIterations_KeyDifferentlyOverTheSameZone`.
- `AStaleMVREOnTheModel_DoesNotOverrideAnExplicitNVRoute_AndIsNotRewritten`.
- `Iteration1b_Open_LeavesTheOpeningUnrestrictedAndCompatible`,
  `Iteration1b_Night_KeepsTheAuthoredRestrictionAndItsSchedule`,
  `Iteration1b_OpenAndNight_DifferOnlyInTheOpeningAvailability`.

Every pre-existing opening / idempotency / airflow test still passes; the NV ones now state
`BaseNaturalVentilation`.

`SAM_Tas` - `PartONaturalVentilationWorkflowTests` (11, COM-free): the fixture is parameterised by
`OpeningRestriction`, so it builds both acceptance cases. Added
`TheUnrestrictedAperture_ResolvesTheApertureControlWithNoAvailabilityRestriction`,
`BothCases_ExportAsNaturalVentilation`, `BothCases_SelectTheSameNaturalVentilationTM59Route`,
`TheTwoCases_DifferOnlyInTheOpeningAvailability`.

## Validation
- `SAM.Analytical` Debug build: 0 errors, no new warnings from the new files.
- `SAM.Analytical.Grasshopper`: 0 CS errors (the post-build deploy step fails locally with the pre-existing
  environmental `*Undefined*` path quirk; CI is green).
- Full `SAM.Tests` Debug: **1425 passed, 0 failed**.
- `SAM.Analytical.Tas.TM59.Tests` Debug: **624 passed, 0 failed**.
- Licensed headless TAS, both cases (`CIBSE Weather 2021.twd`, `Sizing = true`, `Simulate = true`,
  days 1..365), base model `C:\TasOut\v40\A0.sam`, outputs in `C:\TasOut\p1b`:
  - The two produced **TBDs differ by exactly one line** - the extra `ApertureType` carrying
    `schedule=PartO_DayOpen_08_23`, `values=000000001111111111111110`.
  - `"flow"` keys in the TBD zone descriptions: **NV-OPEN 0, NV-NIGHT 0, MVRE control 8.** No continuous
    mechanical supply was invented in either case; the authored `freshAirRate` 8 l/s/p survives, where the
    mechanical control zeroes it and writes `flow = 0.0455`.
  - TM59 from each produced TSD: **5 natural-ventilation, 0 mechanical, 4 corridor** in both, no refusals
    of any kind, every space passing.
  - **The two simulations genuinely differ**: 16 690 of 78 840 hourly resultant temperatures, with the
    largest delta in the whole model - 0.674 K - on `Bedroom 2_3`, the one space whose window was
    restricted, and in the expected direction.

## Acceptance-model decision
The **fallback** in the brief was taken deliberately: the licensed A/B adapts `C:\TasOut\v40\A0.sam` (the
9-space TM59 residential model already proven by the 2026-08-25 acceptance) rather than constructing TAS
geometry from scratch. Building a simulatable dwelling through SAM APIs means closed shells, constructions
and adjacency the TAS importer accepts - unrelated complexity that would put the acceptance's own
correctness in question. The **preferred** option is used where it costs nothing: both COM-free suites build
their dwellings from scratch through normal SAM APIs. `build_tests/Fixtures/original_v1.sam` remains
rejected (office massing, 0 zones, 0 internal conditions, 0 opening properties, in a build output
directory).

## Remaining ambiguity / open items
- **Iteration 1a (`BaseMVHR`) is not implemented.** The Part F requirement is applied on the MVHR route,
  but no physical unit is selected against it. The algorithm is recorded in `PartO-ARCHITECTURE.md` §5 and
  is the next piece.
- **Iterations 2 and 3 are not implemented** - recorded in `PartO-ARCHITECTURE.md` §2.
- **Mixed-route models refuse.** `ApplyPartFVentilationRates` is whole-model; a per-zone application is a
  separate change with its own transfer-air and balance consequences.
- **System 1 sizing is not implemented anywhere.** Zero continuous mechanical airflow means no MVHR/MVRE
  system was invented; it does NOT mean the dwelling's natural-ventilation Part F design has been sized. Do
  not report the NV result as "Part F NV sizing".
- **Wet-room intermittent extract has no runtime behaviour, deliberately.** SAM parses
  `PartFCategory.IntermittentExtractRate_Lps` from the rule set and reads it nowhere; a wet room actually
  receives a *continuous* balanced extract because `PartFCalculator` is System 4 shaped for every route;
  nothing carries an operating schedule or control for an intermittent extract; and `SAM.Analytical.Tas`
  has **no TBD write path for exhaust at all** (`ExhaustAirFlow` is read only by `PartODiagnosticLog`).
  The rate is preserved as data. Full write-up in `PartO-ARCHITECTURE.md` §6.
- **`OverheatingScenario:v2` remains deferred.** The stage still asserts `Openings Restricted`, so NV-NIGHT
  is still reported `Incompatible` and still only warned about, and the two acceptance cases still share a
  scenario key - correctly, since opening behaviour is a property of the model rather than of the stage.
- **Neither acceptance case fails.** The result pipeline has been shown to report a pass truthfully; it has
  not been shown on this model and weather to report a fail.
- The TM59 assessment must be given the **workflow output** model, not the pre-workflow one -
  `SimulationSpaceMap` resolves on the `ZoneGuid` that `Modify.UpdateIds` stamps during the workflow.
  Pre-existing behaviour, recorded because it silently produces zero results.

## Next step
1. Push, update PR [SAM#76](https://github.com/SAM-BIM/SAM/pull/76) and
   [SAM_Tas#43](https://github.com/SAM-BIM/SAM_Tas/pull/43), run CI. **Do not merge.**
2. **Iteration 1a / `BaseMVHR`**: apply the Part F continuous requirement on the explicit MVHR route and
   select the minimum compliant unit from `MVHR_Template`. The unit's capacity is equipment capability and
   must never become the source of the requirement.
3. Per-zone Part F airflow application, which is what turns today's mixed-route refusal into a real answer.
   It needs a decision on transfer air and dwelling balance across a route boundary before any code.
4. `OverheatingScenario:v2` - moving `Openings Restricted` from the stage to the model.

---

## Historical session note - the first NV gate (same branch, commit `0c8a04eb`)

The commit that closed the NV hole. `PreparePartOIteration` stopped carrying Approved Document F continuous
mechanical supply and extract onto a naturally ventilated dwelling, and the whole preparation moved into the
library so the component and the tests run the same code. `SAM.Tests` 1397, `SAM.Analytical.Tas.TM59.Tests`
620; licensed acceptance recorded in `PartO-TAS-VALIDATION.md` §"Natural-ventilation Part O workflow".

Its mechanics are superseded by the work above - the gate was `Query.PartOPartFAirflowApplication(zones,
dictionary, out diagnostic)` reading the string `"NV"`, at `PartOIteration.BasePassive`, and both of those
now refuse - but the defect it identified and the wording it pinned are unchanged:

- `SAMAnalyticalPreparePartOIteration.SolveInstance` called `Modify.ApplyPartFVentilationRates`
  **unconditionally, and before `_ventilationStrategies` was read at all**.
- `PartFCalculator` is unconditionally System 4 shaped - paragraph 1.67 gives every habitable room a
  mechanical supply terminal, and nothing in `SAM.Analytical/Classes/PartF/` takes a ventilation strategy.
- So an NV dwelling was simulated with mechanical supply/extract it does not have, **successfully**, with
  nothing in the result saying the system was invented. Mirror failure: an NV dwelling with no
  `PartFSpaceData` was refused outright with "run a Part F component first".

---

## Historical session notes (previous branch `feature/partf-transfer-door-panel-selection`, merged as PR #74)

### Branch
`feature/partf-transfer-door-panel-selection` (off `sow/2026-Q3`; PR #73 already merged). UNCOMMITTED —
implementation and tests complete and reviewed-pending; do not push until reviewed.

### Last updated
2026-08-25 - transfer-door panel selection for split shared walls implemented, tested, ready for review.

### Status at the time
`Modify.AddTransferAirDoorsByPartF` no longer refuses a route just because two shared wall panels can each
take the generated door. The host panel is resolved by a fixed selection hierarchy: host validity
(`TryTransferAirDoorGeometry`, unchanged, always first) -> geometric relevance (the panel the direct line
between the two space locations passes through scores 0, the others score the distance from that line) ->
shorter valid shared wall (geometric ties within `Core.Tolerance.Distance`) -> the stable first candidate
from the guid-sorted list (equal lengths within `Core.Tolerance.Distance`). A route is never refused merely
because two candidates are equal; a space with no valid location (`Space.IsPlaced()` false - missing or
NaN) still refuses cleanly because there is no valid primary geometric ranking. Selection is independent of
candidate creation/enumeration order; guid order is the absolute final deterministic fallback only.
Full `SAM.Tests`: **1354 passed, 0 failed** (Debug and Release; was 1344, +10 net).

### Real-model acceptance run (SAM_zoningAM_v1.sam, 2026-08-05-PartO)
Ran `AddTransferAirDoorsByPartF("Flats", ...)` against the ACTUAL model (9 spaces, 50 panels): **5 doors
created, 0 refusals**, exactly one door per route, no duplicates.

- **Studio 1_0 -> Bathroom_2**: candidates `7e09a798` (vertical, x=5, y in [5,10], 5 m) and `3e01ed80`
  (horizontal, y=5, x in [5,10], 5 m) - the two legs of the L-shaped partition meeting at (5,5). The
  centroid diagonal passes exactly through the corner: GEOMETRIC TIE, and both legs are the SAME length
  (5 m), so the stable first candidate won: `3e01ed80` (horizontal partition). Door `b9517704` created,
  centred at x=7.5. (The two legs are genuinely identical - only the documented final fallback separates
  them.)
- **Kitchen_4 -> Ensuite_5**: candidates `69de3fb5` (vertical, 5 m) and `fe27dac4` (horizontal, y=5,
  x in [25,31], 6 m). Geometric winner `fe27dac4` (score 0 - crossed at x=25.75; the other stands 0.833 m
  off). Door `80cd38c7` centred at x=28 - the horizontal partition from the screenshot.
- **Kitchen_7 -> Ensuite_8**: same shape: geometric winner `ab1b0798` (horizontal, y=5, x in [46,52], 6 m;
  other `b154e0b7` 0.833 m off). Door `d75ec2e5` centred at x=49.
- Bedroom 2_3 -> Kitchen_4 and Bedroom 2_6 -> Kitchen_7 (single candidates) created as before.

### Completed then
- `Modify.AddTransferAirDoorsByPartF`: the `candidates.Count > 1` refusal was replaced by the selection
  hierarchy: host validity (`TryTransferAirDoorGeometry` / `Query.ApertureHost`, always first, unchanged) ->
  geometric relevance (`TransferAirDoorPanelScore`: 0 where the centre segment passes through the panel,
  otherwise the distance from the segment to the panel, with exact handling for degenerate segments and
  segments parallel to the panel plane) -> shorter valid shared wall (`WallLength`, the bottom-edge length,
  for geometric ties within `Core.Tolerance.Distance`) -> the stable first candidate from the guid-sorted
  list (equal lengths within `Core.Tolerance.Distance`). A route is never refused merely because candidates
  are geometrically and dimensionally equal. A concise `notes` entry names the chosen panel, the reason and
  the rejected candidates' distances. Remaining refusals: unscoreable panel geometry (defensive), and a
  space that is not `Space.IsPlaced()` (missing or NaN location) - no valid primary geometric ranking, so
  no winner is ever manufactured from invalid geometry.
- `SAMAnalyticalAddTransferAirDoorsByPartF` (GH): component description documents the selection hierarchy.
- Codex review fixes (PR #74, both accepted):
  - **P1 - no NaN refusal for walls beyond the segment.** A candidate whose plane the direct line crosses
    only BEYOND the bounded segment used to score NaN and refuse the whole route. It now scores the finite
    distance from its nearer endpoint to the panel region (`DistanceToPanel`) and simply loses.
  - **P2 - coincident locations scored against the panel region.** A point facing the middle of a large
    wall was scored by its distance to the wall's EDGES, overstating the offset; the score is now the
    perpendicular offset where the projection falls inside the panel, edge distance only otherwise.
    `DistanceToPanel(Face3D, Point3D)` is the single helper for both; the NaN guard in the selection block
    is now truly defensive-only.
- Tests (`PartFTransferAirDoorTests`, 25 total):
  - `SplitWall_DirectLineCrossesOnePanel_DoorCreatedThere` [Theory, both creation orders]: door in the
    crossed panel, other panel untouched, selection note present.
  - `TwoParallelWalls_DifferentLengths_ShorterWallSelected` [Theory, both creation orders]: geometric tie,
    10 m vs 4 m -> the 4 m wall wins in both orders.
  - `TwoParallelWalls_EqualLengths_StableFirstCandidateSelected` [Theory, both creation orders]: geometric
    tie, both 10 m -> the guid-first panel wins in both orders, "stable first candidate" reported.
  - `SplitWall_DirectLineHitsTheJoint_StableFirstCandidateSelected`: joint crossing is a geometric tie,
    equal 5 m lengths -> guid-first panel selected.
  - `TwoSharedWalls_SpaceLocationInvalid_RefusedCleanly` [Theory, missing and NaN location]: still refused
    with "no valid location", candidates untouched, no winner manufactured.
  - `SplitWall_SecondCandidateBeyondTheLocations_DoorStillCreated` [Codex P1]: crossed wall wins, the
    beyond-the-segment wall scores 2 m and loses, no refusal.
  - `CoincidentLocations_ProjectionInsidePanel_ShorterPerpendicularWallWins` [Codex P2]: the wall whose
    interior faces the point wins over a narrower wall whose edges are nearer.
  - `ExampleModelFlatPairs_SplitSharedWalls_DoorLandsOnTheCrossedPanel`: reproduces the three reported pairs
    (Flat 1 Studio 1_0->Bathroom_2, Flat 2 Kitchen_4->Ensuite_5, Flat 3 Kitchen_7->Ensuite_8) as split
    walls; all 5 routes' doors land on the crossed panel.
  - The previous "two walls both fit -> refuse" behaviour is gone by design; single-candidate behaviour is
    unchanged.

### Files changed then
- `SAM/SAM/SAM.Analytical/Modify/AddTransferAirDoorsByPartF.cs`
- `SAM/SAM/SAM.Tests/PartFTransferAirDoorTests.cs`
- `SAM/Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalAddTransferAirDoorsByPartF.cs`
- `SAM/documentation/PartF-HANDOVER.md`
- `SAM/PROJECT_PROGRESS.md` (this file)

### Validation then
- `SAM.Analytical` Debug build: 0 errors.
- Focused `PartFTransferAirDoorTests`: 25/25 passed.
- Full `SAM.Tests` Debug: **1354 passed, 0 failed**. Full `SAM.Tests` Release: **1354 passed, 0 failed**.
- `SAM.Analytical.Grasshopper` compiles with 0 CS errors; its post-build deploy step fails locally with the
  pre-existing `::erase` quirk (environmental - CI uses `RunPostBuildEvent=OnOutputUpdated` and is green).
- Real-model acceptance run re-checked after the Codex fixes: 5 doors, 0 refusals, same panel selections
  as before.

### Open items then
- Flat 1's Studio->Bathroom pair is a true geometric AND dimensional tie (two identical 5 m legs of the
  L-shaped partition, the centroid diagonal passing exactly through their corner): it is resolved only by
  the documented final fallback (stable first candidate `3e01ed80`). Recorded here so it is not mistaken
  for a geometric choice.
- A space with no valid location (missing/NaN) still refuses cleanly in the multi-candidate branch; covered
  by `TwoSharedWalls_SpaceLocationInvalid_RefusedCleanly`.
- No commit/push yet - awaiting review.

---

## Historical session notes (previous branch, merged as PR #73)
`feature/partf-terminal-transfer-compliance` (PR: SAM#73 against `sow/2026-Q3`)

## Current status (previous session - merged as PR #73)
Eight Codex findings implemented with regression tests. The TAS availability schedule export has PASSED
licensed acceptance (see below), so the Part O schedule foundation is proven end to end and the earlier
acceptance-failure hypothesis is withdrawn. Three behaviour/policy findings are explicitly DEFERRED for a
dedicated decision and are deliberately not part of this work.
Full `SAM.Tests`: **1344 passed, 0 failed** (Debug and Release; was 1341, +3 new in the final pass).

## Completed (this session)
- `Core.Query.TryGetEnum`: case-insensitive enum fallback made culture-invariant (`ToUpperInvariant`) -
  under a Turkish culture `"nightclosed"` previously uppercased with a dotted capital I and was silently
  rejected, so `SAMAnalytical.AddOpeningPropertiesByPartO` substituted `Unrestricted`. (Codex 3819617732)
- `VentilationStrategyMap`: the same defect class - strategy normalisation is now `ToUpperInvariant`, so
  a lowercase "disp" no longer reads as an unrecognised strategy under a Turkish culture.
  (Codex 3802695368)
- `DailyAvailabilitySchedule.FromJsonObject`: each `Values` element must be a genuine JSON boolean
  (`JsonValue` + `TryGetValue<bool>`); a string/number/null element now fails deserialization cleanly
  instead of `GetValue<bool>()` throwing out of the load. (Codex 3821025633)
- `PartOOpeningProperties.FromJsonObject`: a PRESENT but unrecognised `OpeningRestriction` name fails
  deserialization (via `TryGetEnum`) instead of silently reading as `Unrestricted`; the ABSENT key keeps
  meaning `Unrestricted`, which is the correct legacy behaviour. (Codex 3815926714)
- `PartFPurgeAssessor`: a non-finite provided opening area (NaN/infinity) is `CannotBeDetermined` instead
  of falling through the NaN comparisons to PASS. (Codex 3821633858)
- `Modify.AddTransferAirDoorsByPartF`: `doors_Created` now returns the PERSISTED aperture (re-read from
  the cluster after `SetPartFDoorTransferData`'s replacement), so the returned door carries the
  `PartFDoorTransferData` record like the door in the returned model. (Codex 3820935573)
- `Modify.ApplyPartFVentilationRates`: the generated-name set is seeded from every EXISTING
  internal-condition name, so a clone can no longer collide with an untouched condition whose name already
  matches the `<condition> - <space>` pattern (TAS identifies conditions by name). (Codex 3821025626)
- `TMOverheatingCalculator.Collect`: an hour is assessed only where BOTH comfort bounds exist, never
  against the `IndexedDoubles` default of 0. The comfort series is bounded by the weather year
  (365 days = 8760 hours), so an 8784-hour leap-year simulation previously assessed its last 24 hours
  against a 0 degC comfort limit and manufactured exceedances. (Codex 3821025624)
- `SAMAnalyticalCheckPartFCompliance`: `extractAllocationStrategy_` now guards with `Enum.IsDefined` like
  the operating-mode branch below it, so numeric text such as "9" is refused instead of selecting an
  undocumented hybrid allocation. (Codex 3796859272)
- `SAMAnalyticalPreparePartOIteration`: the BasePassive restriction-reset lines are ALSO raised as GH
  runtime warnings, not only on the `notes` output, so the reset is visible on the canvas rather than
  buried in a list output.

## Final pre-merge pass (this session) - backlog triage and fixes

The rounds 1-2 Codex backlog (17-19 Aug) was re-triaged against current source. Every thread now has a
classification. Implemented in this pass:

- `TMOverheatingCalculator.Collect`: the loop is bounded by the SHORTER of the two hourly series. A
  truncated resultant-temperature series previously threw `ArgumentOutOfRangeException` out of the whole
  TM52/TM59 run. (Codex 3796859276)
- `TM59AssessmentCalculator.RestoreDesignInternalConditions`: the design condition is restored onto a COPY
  of each simulated space, never onto the caller's instance - the cluster getter's shallow copy shares
  Space objects, so the previous in-place assignment contaminated the caller's raw simulation model.
  (Codex 3811381358)
- `TMOverheatingCalculator.Evaluate`: TM59 space applications are classified from the model's own restored
  space (`space_Temp`), not the stale listed instance - explicitly scoped assessments receive the map's
  retained pre-restore spaces, which selected the wrong TM59 result type or corridor fallback.
  (Codex 3804690049)
- `SAMAnalyticalSetPartFCommissioningData`: a `_zoneName` matching more than one zone is refused rather
  than resolved to the first, so commissioning evidence can no longer be written to the wrong dwelling.
  (Codex 3796737963)
- `SAM_PartFSpaceRulesUKDwellingsMVHR.json`: removed the stale claim that a kitchen's 13 l/s Table 1.2
  minimum can raise the continuous design rate (the code sets it to max(bedroom-or-habitable, area) only),
  in the migration note and the two formula strings. (Codex 3803896492)

**Classified DEFERRED POLICY (next branch - behaviour/policy changes to reported outcomes):**
imbalanced disconnected airflow components (3795669833/3814057499), reversed high-rate schematic paths
(3795832901), zone-marking basis for scenario classification (3796737956), creation-time strategy
vocabulary validation (3796737970), preservation of site evidence across excluded spaces (3804873037),
clearing stale door-transfer records (3804873038), headline status when requested spaces produce no
result (3805522563), overall compliance with unclassified rooms (3805522570), floor-finish requirement
for undercuts (3806862925), free area proven from an upper-bound width (3806862928), opening angle vs
purge row validation (3806914162), unreachable transfer endpoints blocking compliance (3814057488).

**Confirmed already fixed by earlier commits:** 3795669854, 3795768191, 3795768196, 3795832892,
3795832897, 3795895972, 3795895977, 3795895959/3796859273 (Solver2D budget), 3796859271, 3814305690.

## TAS availability schedule export - licensed acceptance PASSED
The schedule foundation is proven on the licensed TAS PC against SAM_Tas `7ef2aff3`:

- 20 schedules requested, **20 assigned**, **0 assignment or persistence warnings**.
- `PartO_DayOpen_08_23` visually correct in TAS: 00:00-08:00 OFF, 08:00-23:00 ON, 23:00-24:00 OFF.
- Slot 24 verified separately with `openingHour = 23`, `closingHour = 24`.

The root cause was the TAS adapter's schedule mapping, fixed in SAM_Tas: TBD's hourly indexed properties
are 1-based hour-ending (SAM hour `h` -> TBD slot `h + 1`), and COM hands VARIANT_BOOL TRUE back as -1.

The earlier "BasePassive reset" acceptance-failure hypothesis recorded here is **withdrawn** - it was not
the defect. `Modify.ResetPartOOpeningRestrictions` behaves as designed; the GH warnings added above remain
useful for reading a BasePassive run, but they are diagnostics, not a fix.

## Deferred - valid findings NOT implemented here
These are behaviour/policy changes that alter reported outcomes. They need a dedicated decision and must
not be folded into a review-fix commit.

- **Codex 3815926703 - zero occupied-hour TM59 spaces.** Technically valid concern, but changing the
  behaviour from an assessment result to a refusal changes report outcomes and requires a dedicated TM59
  policy decision. (Today: a naturally ventilated space with no occupied hour reads zero exceedances as a
  PASS, and a bedroom's Criterion 2 passes 0 against a limit of 0; a mechanically ventilated one reads
  `0 < 0` as a FAIL.)
- **Codex 3821633849 - Criterion 1 annual vs summer basis. FIXED 2026-09-16** (see `## Current` at the top
  of this file) - the regulatory decision this was deferred pending was made explicitly, and
  `TM59NaturalVentilationExtendedResult.Criterion1` now decides Pass/Fail, and `TM59AssessmentReport`'s
  `Actual`, from the summer basis, matching the `Limit` both already displayed. Originally: `TM59AssessmentReport`
  paired the ANNUAL `GetOccupiedHoursExceedingComfortRange()` with the SUMMER limit and basis, while `Pass`
  derived from the annual count against the annual `MaxExceedableHours`, so a row could show an actual above
  its displayed limit and still read Pass.
- **Codex 3802695375 - `successful` is true when no scenarios are produced.** `successful` is
  `refusals.Count == 0`, and a model with no zones adds only a warning. That case is deliberately
  supported - a single-house model carries no zones - so requiring at least one scenario would flip a
  supported configuration to failure. Needs a decision on what `successful` should mean.

D2 (aperture matching) and D3 (schedule-removal transition, Codex 3821601792) remain parked. **No D2 or
D3 code exists in this repository or in SAM_Tas.**

## Files changed
- `SAM/SAM/SAM.Core/Query/TryGetEnum.cs`
- `SAM/SAM/SAM.Analytical/Classes/DailyAvailabilitySchedule.cs`
- `SAM/SAM/SAM.Analytical/Classes/OpeningProperties/PartOOpeningProperties.cs`
- `SAM/SAM/SAM.Analytical/Classes/PartF/PartFPurgeAssessor.cs`
- `SAM/SAM/SAM.Analytical/Classes/TMOverheatingCalculator.cs`
- `SAM/SAM/SAM.Analytical/Classes/TM59AssessmentCalculator.cs`
- `SAM/SAM/SAM.Analytical/Classes/VentilationStrategyMap.cs`
- `SAM/SAM/SAM.Analytical/Modify/AddTransferAirDoorsByPartF.cs`
- `SAM/SAM/SAM.Analytical/Modify/ApplyPartFVentilationRates.cs`
- `SAM/Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalCheckPartFCompliance.cs`
- `SAM/Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalPreparePartOIteration.cs`
- `SAM/Grasshopper/SAM.Analytical.Grasshopper/Component/SAMAnalyticalSetPartFCommissioningData.cs`
- `files/resources/Analytical/SAM_PartFSpaceRulesUKDwellingsMVHR.json`
- `SAM/SAM/SAM.Tests/DailyAvailabilityScheduleTests.cs` (+1 theory, 5 cases)
- `SAM/SAM/SAM.Tests/PartOOpeningPropertiesTests.cs` (+2)
- `SAM/SAM/SAM.Tests/PartFPurgeAndComplianceTests.cs` (+1)
- `SAM/SAM/SAM.Tests/PartFTransferAirDoorTests.cs` (+1)
- `SAM/SAM/SAM.Tests/PartFAirflowApplicationTests.cs` (+1)
- `SAM/SAM/SAM.Tests/TMOverheatingCalculatorTests.cs` (+2)
- `SAM/SAM/SAM.Tests/TM59AssessmentCalculatorTests.cs` (+2)
- `SAM/SAM/SAM.Tests/VentilationStrategyTests.cs` (+1)
- `SAM/PROJECT_PROGRESS.md` (this file)

## Validation
- Focused runs for the retained fixes: 12/12 passed.
- Full `SAM.Tests` Debug: **1341 passed, 0 failed** (was 1329; +12 new).
- Full `SAM.Tests` Release: **1341 passed, 0 failed**.
- Final pass: focused TM59 runs 19/19, then full `SAM.Tests` Debug **1344 passed, 0 failed** and
  Release **1344 passed, 0 failed**.
- `SAM.Analytical.Grasshopper` and `SAM.Analytical` compile with 0 CS errors; `SAM.Analytical.Grasshopper`
  post-build deploy step fails locally with the pre-existing `*Undefined*` xcopy quirk (environmental -
  CI uses `RunPostBuildEvent=OnOutputUpdated` and is green).

## Issues / blockers
- None blocking. The deferred findings above are the outstanding decisions.
- Fresh Codex review on the current head is unavailable: `@codex review` is refused by
  `chatgpt-codex-connector[bot]` ("create a Codex account and connect to github"). The delta after the
  latest Codex-reviewed SHA (`9b4e46fe`) was reviewed manually. GitHub reports every Codex thread as
  unresolved because the bot never resolves them - every thread is now triaged here.

## Next step
- Merge PR #73, then SAM_Systems #14, SAM_Tas #29, SAM_Tas_Grasshopper #4 (dependency chain).
- Decide the deferred findings (3815926703, 3821633849, 3802695375 and the DEFERRED POLICY set above)
  as a TM59/Part O policy pass on the NEXT branch.
