<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O — mixed dwelling strategies: PR3B (active cooling, domain)

Builds on PR1 (SAM#150) and the PR3A investigation and decisions (SAM_UI `documentation/PartO-MixedDwellingStrategies-PR3A.md`
§14, merged as SAM_UI#129 `b3b061b8`). Three focused PRs: **PR3B-1 SAM** (this file §1), PR3B-2 SAM_Systems (§2), PR3B-3
SAM_Tas (§3). No SAM_UI cooling toggle (PR3C).

## Binding decisions (owner, 27 Sep 2026)

- **Route.** Any cooled dwelling → the complete mixed model on the TAS Systems (TPD) route; never a hybrid. Natural
  dwellings free-run, uncooled MVHR = ordinary MV, cooled MVHR = manufacturer-guidance cooling, corridors free-run.
  The route is recorded.
- **Cooling operating airflow** = max(accepted design total, manufacturer guidance airflow), valid only within the
  manufacturer's published cooling airflow range and the selected unit's capacity. Nuaire: default 80, minimum 60,
  published to 120 l/s → a 63 l/s design cools at 80, 100 at 100, the PR2 143 l/s Optimised design is refused. Never
  stored as a design value. No project-level commissioned airflow in PR3.
- **Identity.** A cooled dwelling is `PartOIteration.ActiveTrimCooling`, never `BasePassive`. TM59 = the existing
  mechanical criterion (operative > 26 °C for < 3 % of occupied hours). Uncooled dwellings keep their identities; the
  corridor stays `DwellingIndependent`.
- **Displacement.** Dwelling system zones `DisplacementVentilation = false`, explicitly (PR3B-2).

## 1. PR3B-1 — SAM (`SAM.Analytical`)

### What changed

| Piece | Change |
|---|---|
| `Modify.MaterialisePartODwellingStrategies(baseline, descriptors, scope, ventilationUnitTemplates = null)` | The PR1 `CoolingGated` refusal is gone. A cooled MVHR dwelling is built exactly as the same dwelling uncooled (test `CooledDwelling_HasExactlyTheEngineeringStateOfTheSameDwellingUncooled`); then its selected product's template (new optional argument) is resolved and `Query.PartOCoolingOperatingAirFlow` gives its cooling airflow from the unit's design duty. Scenario: `ActiveTrimCooling`, criterion code `MVHR`. Post-materialisation invariant: the cooled dwelling's unit, movements and systems stay inside it. |
| `Query.PartOCoolingOperatingAirFlow(template, designSupply, designExtract, out refusal)` | The binding rule. Guidance airflow = the strategy's resolved `ElevatedAirFlow_Lps`, else `DefaultElevatedAirFlow_Lps`; refused without guidance, without a stated default, without a published range, beyond the range ("no cooling data"), beyond capacity. The minimum cannot be undershot: `TemplateRefusal` holds the guidance figure inside the range. |
| `Query.PartOCoolingGuidanceFingerprint(template)` | Product identity + capacity + the whole `OperatingStrategy` JSON (source included). |
| `Query.PartOCoolingTemplate(templates, reference)` | Exactly one matching template, else null (ambiguous never guessed). |
| `PartOCooledDwelling` (new) | Zone, materialised unit guid, product, `CoolingOperatingAirFlow_Lps`, `Fingerprint_Guidance`. A run artefact on the record only. |
| `PartOMaterialisationRecord` | `CooledDwellings`, derived `Route` (`Izam` / `Systems`). **Uncooled records are written exactly as PR1 wrote them** (`PartOMaterialisation:v1`, no new keys, so PR2 sidecars never go stale); a record with a cooled dwelling is `PartOMaterialisation:v2` with `Route` and `CooledDwellings`. A v2 record with no cooled dwelling, a non-Systems route or an unreadable entry is invalid. New `IsCurrent(baseline, descriptors, templates, out reason)`: a cooled record is current only with each cooled product's guidance unchanged; the old overload (no templates) reports a cooled record not current. |
| `PartOMaterialisation.Route`, `Enums.PartOSimulationRoute` (new) | `Undefined` / `Izam` / `Systems`. |
| `PartOMaterialisationRefusalReason` | Appended `CoolingWithoutProductGuidance` (generic unit, project test unit, no or ambiguous template, template without strategy), `CoolingAirFlowOutsideGuidance`. `CoolingGated` kept (persisted values keep their meaning), no longer produced. |
| `Query.PartOOperatingAssumptions(ActiveTrimCooling)` | Characterised (below). New constants `ActiveCooling`, `ActiveCooling_SupplyAirManufacturerGuidance`. `PartOIterationOperatingMode` / `PartOIterationVentilationMode` still refuse it, so the legacy `PreparePartOIteration` path is unchanged. |
| `ConditionedReusedUnit` | Unchanged rule, reworded: an authored supply setpoint is refused for every strategy, cooled or not - a dwelling's cooling is its product's guidance, never an authored setpoint. |

### `ActiveTrimCooling` assumptions — each verified against the production route

Verified against SAM_Tas `Modify/GroundGuidanceCooling.cs` (the Iteration 3 manufacturer-guidance grounding) and the
no-IZAM source, not chosen to fill the key:

| Assumption | Value | Evidence |
|---|---|---|
| Openings Restricted | false | the route changes no aperture (`NoIzamThermalSource` touches IZAMs and ventilation gains only; PR1 NV/MVHR leave apertures as authored) |
| Mechanical Ventilation At Design Rate | true | "the unit moves its design airflow with the stat satisfied and the elevated airflow calling" (`GroundGuidanceCooling.cs` §Elevated airflow, lines 28-31) - design rate is the background state |
| Boost Available | **false** | there is no boost state separate from the cooling call: the elevated airflow is driven only by the cooling-stat controllers (lines 247-249). **Differs from the PR3A §14.3 proposal (true)**; the elevated airflow is part of the cooling provision |
| Summer Bypass Available | true | "the unit's own bypass decision - the same at every airflow, independent of the cooling-stat" (lines 376-388) |
| Active Cooling | `Supply Air - Manufacturer Guidance` | the DX supply coil at the product's guidance law (lines 206-224) |

New keys only: `ActiveTrimCooling` had never been persisted (it refused everywhere), so nothing is re-keyed.

### Leakage

- Authored air movement reaching a cooled (MVHR) dwelling: already refused by PR1 (`AuthoredAirMovementConflict`),
  pinned for a cooled dwelling → corridor transfer.
- Shared authored systems/units: already refused (`SharedSystem`).
- New invariant after materialisation: every movement of a cooled dwelling's rooms and unit stays between those rooms,
  that unit and outside; every system naming the unit serves only those rooms.
- Nothing cooling-specific is written into the model (unit supply temperatures NaN, no cooling profile on the unit
  movement): on the IZAM route a cooled strategy cannot condition anything, and the cooling exists only on the
  Systems route, configured from the record.

### Tests

`SAM.Tests/PartODwellingStrategyCoolingTests.cs` (new: 14 facts + a 4-row theory) plus three updated pins
(`ActiveCooling_Persists_AndWithoutAProductIsRefused_NeverGated`; `PartOIterationSliceTests`
`AnUnknownStage_IsRefusedRatherThanGuessedAt` / `AnUncharacterisedStage_ProducesNoScenarios` now use an undefined
stage). **Red first:** with the PR1 gate and the `ActiveTrimCooling` refusal restored on top of this branch, 13 of the
materialisation/identity tests fail (`evidence/parto-mixed-pr3b/pr3b1-red-on-pr1-gate.txt`); the pure airflow-rule tests
are new API. Full `SAM.Tests` **2665/2665**; `SAM.sln` Release 0 errors.

### SAM_UI compatibility

PR2's mixed run never passes templates, so SAM refuses any cooled dwelling there (`CoolingWithoutProductGuidance`) -
fail-closed until PR3C. One SAM_UI test pinned `CoolingGated`; a test-only SAM_UI PR accepts either reason name so SAM_UI
CI is green against SAM before and after this merge.

## 2. PR3B-2 — SAM_Systems ([SAM-BIM/SAM_Systems#31](https://github.com/SAM-BIM/SAM_Systems/pull/31), open)

Branch `feature/parto-mixed-cooling-pr3b2-2026-09-27` (head `56fcb8a6`). **CI build fails until SAM#161 merges** -
`'Query' does not contain a definition for 'PartOCoolingOperatingAirFlow'` (CI builds against the SAM integration tip);
re-run CI after #161 merges.

- `MechanicalVentilationSettings.GuidanceTemplate` (the MVRE topology). Stated → `GuidanceSettings` may be partial: a
  named unit = the product's arrangement (MVRE exchanger + supply DX coil) cloned from the MVRE prototype, which is
  **imported into the call's (MV) plant room** (`TryImportGuidancePrototype`: same subgraph walk as a unit copy, guids
  kept, shared plant collections mapped by kind + name - both shipped templates reference collections by name - anything
  else shared refuses); every other unit = ordinary MV. One plant room, one set of collections (test pins the count).
  Null = legacy call unchanged (partial guidance still refuses). Identity key adds the topology + `-` per uncooled unit
  only when stated. The imported prototype's subgraph is removed with the template's (D9).
- **`DisplacementVentilation = false`** on every materialised zone (was the parity-driven `true`) - moves legacy
  Iteration 3 B0/MG results; re-acceptance is in the gate below.
- `Query.MechanicalVentilationGuidanceSettings(template, designSupply, designExtract, out refusal)` - operates at SAM's
  `PartOCoolingOperatingAirFlow` (called, not restated). Legacy overload unchanged.
- Tests: `MechanicalVentilationMixedCoolingTests` (10 facts + 2-row theory: only the named unit cooled; uncooled unit =
  ordinary MV unit; cooled unit = legacy MG unit; collections not duplicated; DV false; refusals; determinism; JSON;
  SAM rule 63→80, 100→100, 143 refused); 2 DV pins inverted. 8 red on the old behaviour
  (`SAM_Systems/docs/evidence/parto-mixed-pr3b/`). 268/268 locally against #161.

## 3. PR3B-3 — SAM_Tas (not started)

Branch `fix/parto-mixed-cooling-pr3b3-2026-09-27` created from `sow/2026-Q3` `fedf34cd`, **no changes yet**. Planned:
1. `SAM.Analytical.Tas.TM59/Classes/PartODiagnosticLog.cs:167` - the run record's `partOIteration` takes
   `scenarios[0]`. Keep the value for a single-iteration run, write `Mixed` when the scenarios state more than one, and
   add a sorted `partOIterations` array. Per-space rows already carry their governing scenario's iteration (line 297).
   Test in `SAM.Analytical.Tas.TM59.Tests/PartODiagnosticLogTests.cs` (NUnit; fixture helpers `Model_Design`,
   `Spaces_Simulated`, `Scenarios`, `Input`, `RecordsOf`) - red first on the current code. The Grasshopper twin
   (`SAM_Tas_Grasshopper .../TasLogPartODiagnostics.cs:236`) is a separate repo: small follow-up PR.
2. Mixed TPD regression: from a SAM_Systems mixed materialisation (two dwellings, one guidance unit), the SAM_Tas
   conversion context (`TPD/Create/SystemVentilationConversionContext.cs`) holds exactly one `GuidanceCooling`, bound to
   the cooled unit's air system; the uncooled air system carries no exchanger/coil. COM-free if possible; grounding is
   per record (`SystemVentilationRoute.cs:217-240`), so no route change is expected.

## 4. Gate - licensed TAS proof (after SAM#161, SAM_Systems#31, SAM_Tas PR3B-3 merge)

PR2 clean fixture `C:\TasOut\parto-mixed-pr2-2026-09-27\fixtures\SAM_zoningAM-CIBSEfutureZ1-MixedBaseline.sam` (local,
3 flats + corridor; product Nuaire MRXBOXAB-ECO5-AECV with MR-ECO-COOL-V, capacity 150 l/s). Extend the env-gated harness
SAM_UI `WPF/SAM.Analytical.UI.WPF.Tests/PartOMixedCoolingRouteProofTests.cs` (it already runs IZAM + Systems stages over a
PR1-materialised model in ~4 min): Flat 1 Natural / Flat 2 MVHR uncooled / Flat 3 MVHR cooled (Part F → 80 l/s); pass the
catalogue descriptors **and templates** to `MaterialisePartODwellingStrategies`, build SAM_Systems settings with
`GuidanceTemplate = MVRE` and guidance for Flat 3's AHU only (`MechanicalVentilationGuidanceSettings(template, duty…)`),
`Pipeline.Route` / bridge / `PartOTM59Assessment.Assess` with the materialiser's scenarios. Verify: one model, one TPD,
DX only in Flat 3's system, guidance read-back for 1 unit, Flat 2 uncooled, Flat 1 and corridor free-running, scenario
keys (Flat 3 `ActiveTrimCooling`), cooling removed → rematerialised model has no cooling, baseline JSON unchanged.
Optional: Optimised + cooled with a retained design within 60-120 l/s (never alter the 143 l/s design). Then re-accept
legacy Iteration 3 B0 and MG with DV = false (env-gated `PartOWorkflowEvidenceHarness` reopens a saved run from
`C:\TasOut\parto-guidance-2026-09-24\` and calls `ReviewPartOIteration3`).

## 5. Handover (end of session, 28 Sep 2026)

| PR | State | Merge order |
|---|---|---|
| [SAM#161](https://github.com/SAM-BIM/SAM/pull/161) PR3B-1 | open, CI green (build/test/spdx) | 1st |
| [SAM_UI#131](https://github.com/SAM-BIM/SAM_UI/pull/131) test-only | open, CI green | any time (passes before and after #161) |
| [SAM_Systems#31](https://github.com/SAM-BIM/SAM_Systems/pull/31) PR3B-2 | open, build red until #161 merges | 2nd - re-run CI after #161 |
| SAM_Tas PR3B-3 | branch only, no changes | 3rd |

- Merged this session: SAM_UI#129 (PR3A) `b3b061b8`.
- Local builds: SAM (`SAM.sln`, dotnet), SAM_Systems (dotnet) OK; **SAM_Tas and SAM_UI need VS 18 Framework MSBuild**
  (`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, COM references). SAM_UI CI
  only builds (no tests) - run WPF tests locally (`EveryClassWithStaTests_IsInTheWpfCollection` caught a PR3A harness).
- Not in PR3B: SAM_UI cooling toggle (PR3C, do not start without the owner); legacy SAM_UI
  `PartOIteration3GuidanceResolution` still uses the product default airflow (could move to the new overload later);
  PR2F reporting review was paused mid-investigation (nothing written).
- **Next step:** implement PR3B-3 (§3), merge in order, then the gate (§4).
