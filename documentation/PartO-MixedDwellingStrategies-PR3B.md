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

### Codex review round (28 Sep 2026)

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
are new API. Full `SAM.Tests` **2668/2668** (after the Codex round); `SAM.sln` Release 0 errors.

### SAM_UI compatibility

PR2's mixed run never passes templates, so SAM refuses any cooled dwelling there (`CoolingWithoutProductGuidance`) -
fail-closed until PR3C. One SAM_UI test pinned `CoolingGated`; a test-only SAM_UI PR accepts either reason name so SAM_UI
CI is green against SAM before and after this merge.

## 2. PR3B-2 — SAM_Systems ([SAM-BIM/SAM_Systems#31](https://github.com/SAM-BIM/SAM_Systems/pull/31), merged `005c4fe`)

Branch `feature/parto-mixed-cooling-pr3b2-2026-09-27` (head `56fcb8a6`). CI was red only until SAM#161 merged (it builds
against the SAM integration tip); re-run green after `85a13ec3`, Codex no findings, 268/268 locally against the merge.

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
- TM59 970/970, benchmark 16/16; CI green after SAM_Systems#31; Codex no findings.
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

### Gate result (28 Sep 2026) - mixed cooling PASSED; legacy B0/MG re-acceptance NOT RUN

Harness SAM_UI `WPF/SAM.Analytical.UI.WPF.Tests/PartOMixedCoolingGateTests.cs` (env-gated) against the merged tips
(SAM `85a13ec3`, SAM_Systems `005c4fe`, SAM_Tas `e7cc0ed`); log in SAM_UI
`documentation/evidence/parto-mixed-pr3b/gate.txt`, TAS files in `C:\TasOut\parto-pr3b-gate-2026-09-28` (local only).
**38/38 checks pass:**

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

**Legacy Iteration 3 B0 / MG re-acceptance (DV = false): not run.** The saved 24 Sep run (`03-Resume`) no longer
restores (`PartORun.Restore`: "the model has changed since the simulation results ... were produced"), so Iteration 3
cannot be rerun from it. It needs a fresh Iteration 1a run of `SAM_zoningAM-CIBSEfutureZ1.sam` through the product
workflow, then Iteration 3 B0 and MG (the harness's `Gate_LegacyIteration3_B0AndMG_DisplacementOff` reruns both from a
copy of any restorable run folder, `SAM_PARTO_LEGACY_RUN`). Until then the legacy B0/MG results on record are DV = true.

## 5. Handover (28 Sep 2026, after the gate)

| PR | State |
|---|---|
| [SAM#161](https://github.com/SAM-BIM/SAM/pull/161) PR3B-1 | merged `85a13ec3` (incl. the Codex round `2be58f1e`) |
| [SAM_Systems#31](https://github.com/SAM-BIM/SAM_Systems/pull/31) PR3B-2 | merged `005c4fe` |
| [SAM_Tas#71](https://github.com/SAM-BIM/SAM_Tas/pull/71) PR3B-3 | merged `e7cc0ed` |
| [SAM_UI#131](https://github.com/SAM-BIM/SAM_UI/pull/131) test-only | merged `11d9078` |
| [SAM_Tas_Grasshopper#7](https://github.com/SAM-BIM/SAM_Tas_Grasshopper/pull/7) | open - log file name |
| SAM_UI gate harness + evidence | open, test-only |

- Before PR3C: (1) re-accept legacy Iteration 3 B0 / MG with DV = false from a fresh Iteration 1a run; (2) the production
  `PartOIteration3Pipeline.Materialise` has no `GuidanceTemplate` - PR3C must make the mixed SAM_Systems call (as the gate
  harness does) and pass the catalogue descriptors **and** templates to `MaterialisePartODwellingStrategies` (a cooled
  product must be selected against the catalogue in that call); (3) legacy SAM_UI `PartOIteration3GuidanceResolution`
  still uses the product default airflow (could move to the design-duty overload).
- Not in PR3B: SAM_UI cooling toggle (PR3C, do not start without the owner); PR2F reporting review paused (nothing written).
- **Next step:** owner review of the gate result; legacy B0/MG re-acceptance; then PR3C only on the owner's go-ahead.
