<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O PR-2: effective-duty classification of authored ventilation plant

**Status (30 Sep 2026): implemented and tested. The PR is open (SAM-BIM/SAM#172) against `sow/2026-Q3` and is NOT merged. SAM only. SAM_UI
needs no production change.**

- Branch `feature/parto-effective-duty-2026-09-30`, from `sow/2026-Q3` `525a9f3a` (after SAM#170, SAM#171 and their
  closeouts; SAM_UI#150 and SAM_UI#151 are merged too).
- Architecture authority: SAM_UI `documentation/PartO-ModelStateArchitecture.md` (approved 30 Sep 2026), step 3 and
  owner decision 1.

## Problem

The owner's cleaned model refused Mixed Design with `SharedSystem` on `MV 1` and `AHU1`. Both are
`Modify.AddMechanicalSystems` template scaffolding: `MV 1` is related to the rooms of Flats 2 and 3 and names `AHU1`.
Neither has a terminal, an air movement or a product. `MaterialisePartODwellingStrategies.AuthoredMechanicalSystems`
treated an existing unit as plant (`AHU exists → active`), so the scaffold looked like shared plant.

## Domain investigation (before any change)

| Question | Finding |
|---|---|
| AHU stated design airflow | **None exists.** `AirHandlingUnit` stores only supply temperatures and its sections. Its only parameter is `AirHandlingUnitParameter.VentilationUnitReference`. Its airflow is always **derived**: `Query.AirHandlingUnitDesignDuty` (supply/extract from the design terminals of systems whose `SupplyUnitName` is the unit, documented as "Derived, never stored on the unit") and `Query.AirFlow(AirHandlingUnitAirMovement)` (from `SpaceAirMovement`s out of the unit). No converter or Grasshopper component writes an airflow onto a unit. |
| Supply / exhaust | A system binds units by name through `VentilationSystemParameter.SupplyUnitName` and `ExhaustUnitName`, and both are read. Duty is split supply/extract by each terminal's `FlowClassification`. |
| NaN / zero | The Systems scope (PR-1) counts effective duty as a stated, finite, non-zero `DesignFlowRate_Lps`: null, 0, NaN and ±∞ are not. The materialiser used `(DesignFlowRate_Lps ?? 0) > 0`. |
| Product marker | `AirHandlingUnitParameter.VentilationUnitReference`, read through `Query.SelectedVentilationUnitReference`. |
| `SpaceAirMovement` link | `Modify.AddAirMovementObjects` relates each unit movement to the unit AND the space, and names the unit in `From`/`To` (an `ObjectReference`). A unit's plant-zone condition is an `AirHandlingUnitAirMovement` related to the unit. |
| Terminal duty today | The materialiser reads terminals related to the system. The Systems scope uses `IsPartOEffectiveMechanicalDuty`. |
| Real model | `AHU1`: no parameter sets, summer supply 23 °C (the `Create.AirHandlingUnit` default), no relations. Model: 0 terminals, 0 `SpaceAirMovement`, 0 `AirHandlingUnitAirMovement`. |

**Contradiction check.** The agreed rule's "finite stated AHU design airflow" has no representation in SAM. It does not
contradict the rule, because every airflow a unit can have is derived from terminals or movements, and both are
tested. No airflow parameter was invented: that would contradict SAM's documented "derived, never stored" design.
**Owner confirmed (30 Sep 2026):** because SAM stores no AHU airflow, the derived terminal and movement airflow is the
correct reading.

## What this PR adds (additive only)

```csharp
public static PartOAuthoredPlantDuty Query.PartOAuthoredPlantDuty(this AdjacencyCluster, VentilationSystem)
public static PartOAuthoredPlantDuty Query.PartOAuthoredPlantDuty(this AdjacencyCluster, AirHandlingUnit)
```

- `PartOAuthoredPlantDuty` (result): `Guid`, `Name` (system `FullName` / unit name), `UnitNames`, `Evidence`, and
  `IsInert` (true when there is no evidence).
- `PartOMechanicalDutyEvidence`: `Kind`, the stating object (`Guid`, `Name`), its owner (`Guid_Owner`, `Name_Owner`),
  `Value` (l/s, or NaN) and `Message`.
- `Enums.PartOMechanicalDutyEvidenceKind`: `TerminalDesignAirFlow`, `SpaceAirMovement`, `SelectedProduct`.
- `internal Query.IsPartOEffectiveAirMovement(SpaceAirMovement)`.

The public API is shaped for PR-6 ("Systems in this assessment") to list without parsing messages. No existing
signature, refusal reason or refusal message changed.

## The effective-duty definition

A system is **active** when any of these holds on it, or on a unit it names (supply or exhaust):
1. a related design terminal with a finite, non-zero design airflow (PR-1's `IsPartOEffectiveMechanicalDuty`, reused,
   so there is one terminal definition);
2. a `SpaceAirMovement` with a finite, non-zero `AirFlow`, related to the system, related to the unit, or naming the
   unit as `From`/`To`;
3. a selected product on the unit (`VentilationUnitReference`);
4. (1) or (2) on another system naming the same unit. A unit is one piece of plant, so a duty on one of its systems
   makes the unit, and every system on it, active.

Otherwise it is **inert**. The system type, names and unit supply temperatures are not read. A unit's supply condition
(`AirHandlingUnitAirMovement`) is not duty on its own, because it states conditions, not an airflow. The airflow TAS
gives it (`Query.AirFlow`) is summed from the unit's `SpaceAirMovement`s, which (2) already counts.

## How `AuthoredMechanicalSystems` changed

- The per-system gate `!duty && names_Unit.Count == 0` became `PartOAuthoredPlantDuty(system).IsInert`.
  - An inert system with no unit keeps the existing note, word for word.
  - An inert system naming a unit gets a new note: *"… names unit 'AHU1', but neither states any duty … inert template
    metadata: left exactly as authored and not part of this assessment."*
- The shared-unit loop now skips a unit none of whose same-named instances is active.
- `dictionary_ZonesOfUnit` is still built for every named unit. `AuthoredAirMovements` reads it, so the movement rule
  is unchanged.
- Unchanged: `SharedSystem`, `NaturalOverMechanicalDuty` and `UnconnectedAuthoredPlant`, with their messages, for active
  plant; `AuthoredAirMovementConflict`; P12 `ConditionedReusedUnit`; unit reuse through design terminals.

## Decisions and assumptions

- **A unit's supply condition alone is not duty.** This is the owner's decision of 30 Sep 2026: the rule is about
  effective duty, not about a condition object existing. The condition counts only through the finite, non-zero
  movements its `Query.AirFlow` is summed from. Next to an MVHR dwelling, the unchanged movement rule still refuses the
  condition itself (`AuthoredAirMovementConflict`), so no safety refusal is lost. It is just no longer
  `SharedSystem`.
- **A negative terminal airflow is duty.** This is PR-1 parity: a non-zero stated value is duty. The old materialiser
  test (`> 0`) did not count it.
- **±∞ is not duty.** This is PR-1 parity. Such a model cannot be fingerprinted or saved anyway, because JSON cannot
  hold ∞.
- **The classification applies to all three authored-plant refusals.** An inert single-dwelling system that names a
  unit is now noted, not refused `UnconnectedAuthoredPlant` or `NaturalOverMechanicalDuty`. This is the agreed
  decision: inert plant is excluded from the assessment.
- **The PR-1 Systems scope is unchanged.** It still reads terminal duty only. This matters only for authored systems
  outside every assessed dwelling, which the materialiser does not judge. That was already true before this PR (see
  Risks).

## Files

- `SAM/SAM.Analytical/Query/PartOAuthoredPlantDuty.cs` (new: the query)
- `SAM/SAM.Analytical/Classes/PartOAuthoredPlantDuty.cs`, `PartOMechanicalDutyEvidence.cs` (new)
- `SAM/SAM.Analytical/Enums/PartOMechanicalDutyEvidenceKind.cs` (new)
- `SAM/SAM.Analytical/Modify/MaterialisePartODwellingStrategies.cs` (the per-system gate, the unit loop, docs)
- `SAM/SAM.Tests/PartODwellingStrategyMaterialisationTests.EffectiveDuty.cs` (new: 16 tests, 28 cases)
- `documentation/evidence/parto-pr2-effective-duty-2026-09-30/` (replay source, output, folder listing; paths redacted)

## Evidence

- **Fixtures** are production-shaped. `Modify.AddMechanicalSystems` runs over internal conditions naming `NV` (Flat 1),
  `UV` (corridor) and `MV` (Flats 2+3), giving `NV 1`, `UV 1`, `MV 1 → AHU1`. The one-unit-per-flat topology is
  `AddMechanicalSystems` per flat.
- **Required cases:**

  | # | Case | Result |
  |---|---|---|
  | 1 | Inert shared scaffold | passes, inert note, no `SharedSystem`, scaffold kept, PR-1 scope removes NV 1/UV 1/MV 1, baseline JSON unchanged |
  | 2 | Terminal duty | `SharedSystem` on `MV 1` and `AHU1` |
  | 3 | Air movement | refused, for each of: unit→room, unit exhaust, endpoint-only, and on the system |
  | 4 | Unit airflow / supply condition | a finite `Query.AirFlow` (from a unit movement) refuses `SharedSystem`; a supply condition alone is inert (no `SharedSystem`, though the movement rule still refuses it next to an MVHR dwelling) |
  | 5 | Product | refused |
  | 6 | 0 / NaN / ±∞ / null terminal, and 0 / NaN movement | not duty; negative is duty |
  | 7 | One unit per flat | each unit carries its own product and duty; materialises; each flat reuses its own unit |
  | 7b | Two flats on one unit | `SharedSystem` once either system states duty; inert otherwise |
  | 8 | Single-dwelling plant with a product | still `UnconnectedAuthoredPlant` or `NaturalOverMechanicalDuty`; the inert version is noted |
  | 9 | NV/UV | not mechanical, no unit, inert; the NV note is unchanged |
  | 10 | PR-1 regressions | the scope is unchanged (case 1 and the existing `SystemsScope_*` tests) |

- **Focused** (materialisation + Systems scope + isolation): 228/228.
- **Full SAM.Tests: 2760/2760** (2732 before; +28).
- **SAM_UI WPF against this SAM: 1559/1559.** No SAM_UI change. `SAM_UI/build/SAM.Analytical.dll` has the same hash as
  this build.
- **Mutation checks.** All killed, and each file was restored byte-identical (cmp) before the next one:

  | Mutation | Tests failed |
  |---|---|
  | M1: AHU exists ⇒ active | 13 |
  | M2: ignore terminal duty | 9 |
  | M3: ignore space movements | 6 |
  | M4: ignore product | 5 |
  | M5: ignore the unit's airflow (its movements) | 5 |
  | M10: a supply condition alone counts as duty | 3 |
  | M6a: NaN/zero terminal counts as duty | 5 |
  | M6b: NaN/zero movement counts as duty | 2 |
  | M7: genuine shared duty passes | 13 |
  | M8: a unit ignores its other systems | 6 |
  | M9: unit movements by relation only | 1 |

  A clean rerun after the mutations gave 228/228. M1, M5, M6b and M10 were rerun after the supply-condition change.
  The others ran on the first head, and the code they mutate is unchanged.
- **Real model, headless, no TAS, read-only** (`evidence/.../replay-output.txt`): `000000_SAM_AnalyticalModel-Cleaned.sam`,
  SHA256 `F561161F…0B78` before and after, folder listing identical, nothing written, nothing deleted.
  - Classification: `NV 1`, `UV 1`, `MV 1` and `AHU1` are all inert.
  - The owner's selection (Flat 1 natural; Flat 2 Nuaire XBC15, cooling off; Flat 3 Nuaire MRXBOXAB-ECO5-AECV with
    cooling), through the real SAM_UI Check:
    - it materialises on the Systems route with no refusal and 0 `SharedSystem`, and has the inert note for `MV 1`/`AHU1`;
    - Flat 2 gets XBC15; Flat 3 gets MRXBOX and a cooling airflow of 80 l/s;
    - `NV 1`, `UV 1`, `MV 1` (6 rooms) and `AHU1` are kept.
  - PR-1 scope: 2 retained (`MVHR Flat 2`, `MVHR Flat 3`), 3 removed (`MV 1`, `UV 1`, `NV 1`).
  - Production Systems preflight: the SAM_Systems graph is built, with 2 air systems and 1 guidance-cooled unit, and no
    refusal.
- **SAM_Systems production code untouched** (`sow/2026-Q3` `aadb1b1`, clean).

## Owner decisions (30 Sep 2026, on review)

1. **Stated AHU airflow means the derived terminal and movement airflow**, since SAM stores none. No change needed.
2. **A supply condition alone is not effective duty** unless it has a finite, non-zero airflow, that is, through its
   movements. Applied: the standalone `UnitAirMovement` evidence kind was removed.

## Risks / not verified

- No licensed TAS run. That is step 5, the Mixed acceptance gate.
- The Check on the real model reports 6 existing warnings: "Space … is still related to ventilation system 'MV 1' …".
  These come from the unchanged 1a realisation and are correct, but noisy. PR-6 could present them better.
- The PR-1 scope reads terminal duty only. An authored system outside every assessed dwelling whose unit has a product
  or movements, but no terminal duty, is left out of the SAM_Systems input without refusal, exactly as before this PR.
  PR-3 or PR-6 could use `PartOAuthoredPlantDuty` there.

## Next step

Then PR-3 (SAM_Systems D2 scope), and the licensed Mixed acceptance on `-Cleaned.sam` with nothing deleted.
