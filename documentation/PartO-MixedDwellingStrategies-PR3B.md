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
| `PartOCooledDwelling` (new) | Zone, materialised unit guid, product, the unit's design duty (`DesignSupply_Lps` / `DesignExtract_Lps`), `CoolingOperatingAirFlow_Lps`, `Fingerprint_Guidance`. A run artefact on the record only. |
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

### Review round (28 Sep 2026)

Three P1 findings, each confirmed red first (`evidence/parto-mixed-pr3b/pr3b1-codex-findings-red.txt`) and fixed:
1. **v2 → v1 laundering.** A truncated v2 record (no `CooledDwellings`) re-saved as v1 and reopened valid. Now a record
   read as v2 is always written v2 with the route it stated, so it stays invalid.
2. **Descriptor capacity.** The cooling airflow was checked only against the template's capacity. Now also against the
   catalogue entry that selected the unit (the smaller governs); a cooled product not selected against the catalogue in
   this call is refused (`CoolingAirFlowOutsideGuidance`).
3. **Stored airflow unverified.** `IsCurrent` accepted any positive stored airflow. The cooled dwelling now records the
   design duty it was resolved from, and `IsCurrent` re-derives the airflow from that duty and the supplied guidance.

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

`SAM.Tests/PartODwellingStrategyCoolingTests.cs` (new: 17 facts + a 4-row theory) plus three updated pins
(`ActiveCooling_Persists_AndWithoutAProductIsRefused_NeverGated`; `PartOIterationSliceTests`
`AnUnknownStage_IsRefusedRatherThanGuessedAt` / `AnUncharacterisedStage_ProducesNoScenarios` now use an undefined
stage). **Red first:** with the PR1 gate and the `ActiveTrimCooling` refusal restored on top of this branch, 13 of the
materialisation/identity tests fail (`evidence/parto-mixed-pr3b/pr3b1-red-on-pr1-gate.txt`); the pure airflow-rule tests
are new API. Full `SAM.Tests` **2668/2668** (after the review round); `SAM.sln` Release 0 errors.

### SAM_UI compatibility

PR2's mixed run never passes templates, so SAM refuses any cooled dwelling there (`CoolingWithoutProductGuidance`) -
fail-closed until PR3C. One SAM_UI test pinned `CoolingGated`; a test-only SAM_UI PR accepts either reason name so SAM_UI
CI is green against SAM before and after this merge.

## 2. PR3B-2 — SAM_Systems ([SAM-BIM/SAM_Systems#31](https://github.com/SAM-BIM/SAM_Systems/pull/31), merged `005c4fe`)

Branch `feature/parto-mixed-cooling-pr3b2-2026-09-27` (head `56fcb8a6`). CI was red only until SAM#161 merged (it builds
against the SAM integration tip); re-run green after `85a13ec3`, no review findings, 268/268 locally against the merge.

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

## 3. PR3B-3 — SAM_Tas ([SAM-BIM/SAM_Tas#71](https://github.com/SAM-BIM/SAM_Tas/pull/71), merged `e7cc0ed`)

**No production thermal/TPD change** - PR3A's conclusion held: the grounding is per air system, by identity.

- `SAM.Analytical.Tas.TM59/Classes/PartODiagnosticLog.cs`: the run record's `partOIteration` took `scenarios[0]`. New
  public `RunPartOIteration(scenarios)` - the dwellings' one iteration (a common space's `DwellingIndependent` only when
  there is no dwelling), `Mixed` when they differ, null for none; new run field `partOIterations` (every distinct
  iteration). Space rows unchanged. The old line was also wrong for a single-iteration run whose corridor came first.
- `MixedGuidanceCoolingTests` (COM-free): the real SAM_Systems mixed materialisation through the production
  `SystemVentilationConversionContext` and `Modify.GroundGuidanceCooling` (by reflection - embedded interop parameter
  types): one `GuidanceCooling`, on the cooled unit's air system; the uncooled air system has none, no exchanger/coil,
  and the grounding leaves it untouched; ventilation intent identical to the uncooled document; 25 l/s design -> 80 l/s.
- Red first (SAM_Tas `Documentation/evidence/parto-mixed-pr3b/pr3b3-red-first.txt`): 2 diagnostic tests fail on the old
  line; the TPD tests pass on unchanged production and 2 of 4 fail under a building-wide mutation of `GuidanceCooling`.
- TM59 970/970, benchmark 16/16; CI green after SAM_Systems#31; no review findings.
- Grasshopper twin: [SAM-BIM/SAM_Tas_Grasshopper#7](https://github.com/SAM-BIM/SAM_Tas_Grasshopper/pull/7) - the log file
  name uses `RunPartOIteration` instead of `overheatingScenarios[0]`.

## 4. Gate - licensed TAS proof

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

### Gate result (28 Sep 2026) - mixed cooling PASSED; legacy B0/MG RE-ACCEPTED with DV = false

Harness SAM_UI `WPF/SAM.Analytical.UI.WPF.Tests/PartOMixedCoolingGateTests.cs` (env-gated) against the merged tips
(SAM `85a13ec3`, SAM_Systems `005c4fe`, SAM_Tas `e7cc0ed`); log in SAM_UI
`documentation/evidence/parto-mixed-pr3b/gate.txt`, TAS files in `C:\TasOut\parto-pr3b-gate-2026-09-28` (local only).
**38/38 checks pass** (51/51 after the SAM_UI#132 review rounds, rerun on licensed TAS: TM59 completeness - nothing
unassessed, every occupied room of every flat has its rows; the accepted model object unchanged; the Optimised + cooled
range read from the selected product; and a relationship-preserving topology signature beside the guid-masked content
comparison - SAM_UI `documentation/evidence/parto-mixed-pr3b/topology.txt`):

- One analytical model; route `Systems`; exactly one cooled dwelling (Flat 3, Nuaire MRXBOXAB-ECO5-AECV + MR-ECO-COOL-V,
  design 63/63 l/s -> cooling operating airflow **80 l/s**). Scenarios: Flat 1 `BaseNaturalVentilation`, Flat 2
  `BasePassive`, Flat 3 `ActiveTrimCooling`, corridor `DwellingIndependent`.
- One SAM_Systems graph, one plant room: one guidance unit (Flat 3's), DX coil only on Flat 3's air system (Flat 2: 0),
  `DisplacementVentilation = false` on all 6 system zones. Flat 1 and the corridor are outside the Systems scope
  (free-running, no system, no cooling).
- One TPD document, complete; guidance read-back for 1 unit (Flat 3's air system): **1505 h** with DX load, peak sensible
  802 W at 80 l/s. Bridge complete (6 rooms).
- TM59 with the materialiser's scenarios: Flat 2 and Flat 3 rooms on the mechanical criterion, Flat 1 on the natural
  criteria. Results: Flat 3 bedroom 199/262 h PASS (cooled) vs Flat 2 bedroom 316/262 FAIL (uncooled); both kitchens
  fail (Flat 3 296/142, Flat 2 320/142); Flat 1 studio PASS (95/110, 20/32); corridor significant risk (899/262).
- Cooling removed and rebuilt from the clean baseline: no cooled dwelling, route `Izam`, Flat 3 `BasePassive`, SAM_Systems
  graph with no guidance record and no DX coil; engineering state equal to the cooled model's (guid- and order-free -
  every materialisation mints fresh guids; the same comparison of two uncooled rebuilds is the control).
- Optimised + cooled (fixture `...-Flat3Accepted2B.sam`, retained 143 l/s): refused `CoolingAirFlowOutsideGuidance`
  (beyond the published 60-120 l/s); the accepted design and the fixture unchanged. No accepted Optimised design inside
  the range exists in the fixtures, so a valid Optimised + cooled case was not exercised on TAS.
- Baseline file SHA-256 unchanged (`89A8AC7B...9446`); baseline object unchanged by every materialisation.

### Legacy Iteration 3 B0 / MG re-acceptance with DV = false (28 Sep 2026) - RE-ACCEPTED

The saved 24 Sep run (`03-Resume`) no longer restores under the current SAM ("the model has changed since the simulation
results ... were produced") and was not bypassed. Fresh evidence was produced through the **real product UI**
(`SAM Analytical.exe` driven by UI Automation - driver `closeout-2026-09-28/driver-closeout.ps1.txt`, adapted from the
24 Sep `stage.ps1`): the same source model `SAM_zoningAM-CIBSEfutureZ1.sam` (SHA-256 `A7E09A25...`), Part O Hub ->
Iteration 1a -> Prepare & Run -> Accept -> TAS (Z1 DSY1 2050s, TAS solar, full year) -> TM59 -> Iteration 3 method ->
Run system case. App DLLs byte-identical to the merged-tip builds (SAM `85a13ec3`, SAM_Systems `005c4fe`, SAM_Tas
`e7cc0ed`, SAM_UI `11d9078`); installed catalogue = SAM_Systems resource (SHA-256 `D3878908...`). Evidence:
`documentation/evidence/parto-mixed-pr3b/closeout-2026-09-28/`; TAS files in `C:\TasOut\parto-pr3b-closeout-2026-09-28\`
(local only; `01-Iteration3-B0`, `02-Iteration3-MG`, one fresh Iteration 1a run each).

- **Fresh Iteration 1a provenance.** Both runs: TAS 1.2 min, TM59 FAIL, 8 spaces assessed (2 pass, 6 fail), 1 not
  assessed - as on 24 Sep. Both `PartORun`s restore under the merged stack, their results are their own folder's, both
  are eligible for Iteration 3, folders unchanged by the check (`fresh-run-restore.txt`, SAM_UI
  `Gate_LegacyIteration3_FreshRunRestores`, read-only - a run's provenance records absolute paths, so no Iteration 3 is
  ever run from a copied folder).
- **DV state, read back from the native TPD.** Displacement ventilation on **0 of 8** native zones (24 Sep: 8 of 8), both
  methods. Topology, bound rooms and every design supply/extract airflow identical to 24 Sep (3 air systems, 8 rooms,
  3 supply / 6 extract / 5 transfer legs).
- **B0 (Route check / Parity)**: COMPLETE, A Fail / B Fail, Iteration 3 in 2.1 min. Bias B-A +0.55 K (DV true +0.05),
  RMSE 0.86 K (0.736), max 3.76 K (3.85); **0 of 8** TM59 outcomes differ from the reference (DV true: 2). The two
  DV-true differences were Ensuite_5 536 -> 247 h and Ensuite_8 677 -> 246 h flipped to Pass by stratification (the
  non-physical wet-room signature of PR3A §6/SAM#129); with mixing ventilation they are 588 / 723 h, Fail like the
  reference. Habitable rooms move +0.07-0.2 K (bedrooms 231/228 -> 239/233 h, Pass).
- **MG (Selected product - manufacturer operating guidance)**: COMPLETE, A Fail / B Fail, 6.6 min, Nuaire
  MRXBOXAB-ECO5-AECV + MR-ECO-COOL-V on all three units at 80 l/s elevated. Compared with the like-for-like DV = true run
  (same Nuaire-reply recipe, SAM_Tas#65 close-out `C:\TasOut\nuaire-reply-2026-09-24\03-Iteration3-MG-closeout`; the
  24 Sep `02-Iteration3-MG` used the superseded intake-offset recipe and is not comparable): fully elevated
  1033/970/964 -> 1034/970/965 h; modulating 485/100/97 -> 494/102/97 h; DX 1610/1293/1282 -> 1636/1306/1304 h (+0.5-1 %
  energy); exchanger state and supply law max(13, entering - 8.245) exact in every full-flow hour (both); heat-recovery
  **bypass 4674/4568/4563 -> 4468/4223/4258 h** following the rule (intake >= 12, < extract, extract >= 19) - 5-9 % fewer
  eligible hours because a mixed room's extract is cooler than a stratified one's; peak extract ~37 C (no hot-extract
  artefact); airflow 80 l/s elevated, design 30/63/63 l/s. TM59: bias +0.92 K (0.53), RMSE 1.40 K (1.413), **0 of 8**
  outcomes differ (0 before); bedrooms 156/153 -> 157/156 h, kitchens 240/238 -> 251/255 h; the bias rise is the wet
  rooms (Bathroom/Ensuites 353/274/286 -> 648/550/683 h) losing the stratification cooling, as in B0.
- **Verdict.** Every change is the intended move from displacement to mixing ventilation: no topology, airflow,
  control, bypass, cooling-law or TM59-generation regression. **B0 and MG are re-accepted with DV = false.** The
  24 Sep DV = true evidence (`C:\TasOut\parto-guidance-2026-09-24\`) and the SAM_Tas#65 MG close-out are **superseded**,
  kept, not deleted.
- Minor, cosmetic (not fixed here): the SAM_Tas conversion note still says displacement ventilation is "inherited from the
  template prototype, not decided by this route"; since PR3B-2 SAM_Systems states it explicitly (`false`).

## 5. Closeout (28 Sep 2026) - PR3B CLOSED

| PR | State |
|---|---|
| [SAM#161](https://github.com/SAM-BIM/SAM/pull/161) PR3B-1 | merged `85a13ec3` (incl. the review round `2be58f1e`) |
| [SAM_Systems#31](https://github.com/SAM-BIM/SAM_Systems/pull/31) PR3B-2 | merged `005c4fe` |
| [SAM_Tas#71](https://github.com/SAM-BIM/SAM_Tas/pull/71) PR3B-3 | merged `e7cc0ed` |
| [SAM_UI#131](https://github.com/SAM-BIM/SAM_UI/pull/131) test-only | merged `11d9078` |
| [SAM_UI#132](https://github.com/SAM-BIM/SAM_UI/pull/132) gate harness + evidence | merged `07077b7` (review findings fixed: legacy check read-only, TM59 completeness per room, path containment, product range, accepted-model snapshot, topology signature) |
| [SAM_Tas_Grasshopper#7](https://github.com/SAM-BIM/SAM_Tas_Grasshopper/pull/7) log file name | merged `9ddf8ff` |
| [SAM#162](https://github.com/SAM-BIM/SAM/pull/162) closeout docs + legacy re-acceptance evidence | merged `3a998007`; gate-count corrections SAM#164 (`3e8670da`) and this final record |

- Licensed mixed-cooling gate 38/38 (51/51 after review); legacy Iteration 3 B0 and MG re-accepted with DV = false from
  fresh Iteration 1a runs through the real UI; no SAM_Tas production physics change.
- **Remaining limitation.** No fixture carries an accepted Optimised design inside the published 60-120 l/s cooling
  range, so a valid Optimised + cooled case has not been run on TAS (the 143 l/s case is correctly refused).
- **Before / in PR3C** (do not start without the owner): the production `PartOIteration3Pipeline.Materialise` has no
  `GuidanceTemplate` - PR3C must make the mixed SAM_Systems call (as the gate harness does) and pass the catalogue
  descriptors **and** templates to `MaterialisePartODwellingStrategies` (a cooled product must be selected against the
  catalogue in that call). Optional: legacy SAM_UI `PartOIteration3GuidanceResolution` still uses the product default
  airflow; SAM_Tas displacement-ventilation note wording.
- Other stream: reporting PR2F-1 merged as SAM#163 (`afe90e94`) by the other session - not part of PR3B.
- **Next step:** PR3C (per-dwelling cooling On/Off in SAM_UI) only on the owner's go-ahead.
