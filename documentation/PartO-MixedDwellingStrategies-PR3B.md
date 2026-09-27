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

## 2. PR3B-2 — SAM_Systems

*Not started.*

## 3. PR3B-3 — SAM_Tas

*Not started.*
