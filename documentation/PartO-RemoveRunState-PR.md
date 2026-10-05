<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O: remove the run state from a model (`Modify.RemovePartORunState`)

**Status (30 Sep 2026): implemented and tested. The PR is open against `sow/2026-Q3` and is NOT merged. It is the
library half of SAM_UI "Results > Part O > Remove Results..." (SAM_UI PR on branch
`feature/parto-remove-results-2026-09-30`). Merge this one first: the SAM_UI PR does not build without it.**

- Branch `feature/parto-remove-results-2026-09-30`, from `sow/2026-Q3` `83eb79a3`.
- SAM only. SAM_Tas is unchanged.

## Why

Mixed Design (SAM_UI) starts only from a clean Part O baseline (`Query.PartOBaselineFindings`, owner decision D1).
The owner's real models have all been through Prepare & Run. In the 30 Sep final acceptance the source
`2026-09-29 partOi\000000_SAM_AnalyticalModel.sam` was refused with six findings: scenarios, provenance, 242 results,
2 cluster design days, Part O MVHR systems `MVHR 1-3`, and 8 per-space Part F internal conditions. The only way
forward was to find an older `.sam`.

## What the investigation found (before any code)

| Validator signal | Written by | Reversible? |
|---|---|---|
| `OverheatingScenarios`, `SimulationResultProvenance` | the Part O run | yes: stamped by the run |
| `IResult` objects in the cluster or as a model value | TAS results | yes |
| `DesignDay` objects in the cluster | `SAM_Tas Modify.ReplaceDesignDays` | yes (model-level design-day params are inputs and stay) |
| Part O MVHR system (fixed type guid) | `AddPartOBaseMVHRSystem` | yes, with its unit (named by `SupplyUnitName`/`ExhaustUnitName`), its terminals and the movements `RemoveBaseMVHRAirMovementObjects` scopes |
| per-space Part F internal condition `<IC> - <space>` | `ApplyPartFVentilationRates` | **only on evidence**: the clone has a new guid and the six airflow bases zeroed, and what they held is stored nowhere |

If the Part O air movements are left behind, materialisation refuses the MVHR dwellings with
`AuthoredAirMovementConflict`. So they must go with the system.

## Decisions (owner, 30 Sep 2026)

1. **D1 amended for the provable case.** A per-space Part F condition is restored only when the model proves what it
   replaced:
   - the base condition, by the name the clone was made from, is held in the model: in the cluster (Map IC (TM59)
     leaves its library there) or on a space that was not rewritten;
   - there is one base, or all the candidates agree;
   - the base states no non-zero airflow on any of the eight Part F airflow parameters;
   - the clone agrees with the base on every other parameter the base states.

   The restored condition is the clone with the base's name and the base's airflow state (absent or zero). Extras
   the space's own condition carried, such as Area Per Person, are kept. Anything unproven is left, named in `kept`,
   and the validator still refuses it.
2. **The rules live in SAM, next to the validator**, so SAM_UI duplicates nothing. The Part O MVHR type guid and
   `RemoveBaseMVHRAirMovementObjects` are internal to SAM.

## Change

- `Modify.RemovePartORunState(this AnalyticalModel, out List<string> removed, out List<string> kept)`
  (`SAM.Analytical/Modify/RemovePartORunState.cs`). It returns a copy; the input is not modified.
  - It removes the run output and the Part O MVHR plant listed above.
  - It restores the provable Part F conditions.
  - It keeps a `PartOMaterialisationRecord` or `PartOIsolationContext`, with a reason: rebuild from the baseline, or
    open the source.
  - It also keeps: authored systems and units (a unit that a remaining system names), terminals not connected to a
    Part O system (an accepted 2B design lives there), dwelling strategies, the equipment selection and the weather.
  - It never decides "clean". The caller asks `Query.PartOBaselineFindings`.
- `Query/PartOBaselineFindings.cs`:
  - The doc names the cleaner.
  - Two finding messages now point at it instead of saying "never cleaned back" or "not reversible".
  - `IsPartFAppliedInternalCondition` is now `internal`, for reuse.
  - New internal `PartFAppliedInternalConditionBaseName`: the inverse of `ApplyPartFVentilationRates.UniqueName`.
  - New internal `IsResultValue`, shared with `ModelResults`.
- `Modify/PreparePartOIteration.cs`: `RemoveBaseMVHRAirMovementObjects` accepts a null unit (a Part O system whose
  unit is already gone). The preparation's behaviour is unchanged.

## Files

- `SAM/SAM.Analytical/Modify/RemovePartORunState.cs` (new)
- `SAM/SAM.Analytical/Query/PartOBaselineFindings.cs`
- `SAM/SAM.Analytical/Modify/PreparePartOIteration.cs`
- `SAM/SAM.Tests/PartODwellingStrategyMaterialisationTests.RemoveRunState.cs` (new, 7 tests)
- `documentation/PartO-RemoveRunState-PR.md` (this record)

## Evidence

- **Build.** `SAM.Analytical` builds with 0 errors.
- **Full `SAM.Tests`: 2695/2695.** It includes the 7 new tests:
  - Prepared + run → cleaned is a clean baseline. The input is byte-identical, the design inputs survive, and the
    conditions are restored by name with no airflow.
  - **Mixed materialisation of the cleaned copy equals that of the original baseline:** the same `Signature` and the
    same `Names` (NV / MVHR / MVHR).
  - A clean baseline comes back unchanged (no removed or kept lines, identical JSON).
  - No held base condition → kept; the validator's only finding is the Part F condition.
  - A base stating a non-zero airflow → not restored.
  - A condition edited after preparation → not restored ("not proven").
  - An authored transfer movement and an accepted 2B design survive; the model is identical.
  - A materialised mixed model keeps its record and is still refused.
- **Mutation checks** (each reverted afterwards; the file is byte-identical and rebuilt):
  - No Part F restore: 4 tests fail.
  - No air-movement removal: 2 tests fail. The main one fails because materialisation refuses the MVHR flats.
  - No evidence rule (restore everything): 3 tests fail.
- **Native smoke in SAM_UI: PASS.** The real app on a copy of the owner's model reached Mixed Design "Baseline:
  clean". See SAM_UI `documentation/evidence/parto-remove-results-2026-09-30/SMOKE.md`.
- **Real model (headless, a copy; source SHA-256 `25DD56B6…` unchanged before and after).**
  - Findings: 6 → 0, and still 0 after save and reopen.
  - Removed: 4 scenarios, the provenance, 242 results, 2 design days, `MVHR 1-3`, `MVHR-01..03`, 9 terminals and
    20 movements.
  - Restored: 8 conditions (`Studio`, `TM59_Bathroom`, `Double Bedroom`, `1 Bed Apt. Kitchen`), each keeping its
    Area Per Person.
  - Kept: authored `NV 1`/`UV 1`/`MV 1`/`AHU1`, 4 heating + 3 cooling systems, 50 panels, 4 zones,
    4 constructions, Part F on 8 spaces, weather and the equipment selection.
  - Time: 45 ms.
  - Log: SAM_UI `documentation/evidence/parto-remove-results-2026-09-30/real-model-clean.txt`.

## Risks / not verified

- Residual risk of the evidence rule: if a space's own condition carried a per-space airflow basis that its held base
  did not, `ApplyPartFVentilationRates` zeroed it, and the restore returns the base's state (absent). Nothing in a
  prepared model records it.
- A model whose conditions are all per-space authored, with no held copy, is never restored. Its cleaned copy still
  FAILs, by design.
- Air movements are removed over the Part O dwelling's rooms and unit: the scope `RemoveBaseMVHRAirMovementObjects`
  clears before every re-preparation. The preparation had already removed any authored movement on those rooms, so
  only a movement added to them AFTER preparation is lost. It is counted in the removed line.
- Terminals realised whole-model for dwellings outside the assessed set, and connected to no Part O system, are kept
  (they cannot be told apart from an accepted design). The validator accepts them.
