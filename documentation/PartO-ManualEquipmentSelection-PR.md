<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O: hand-picked per-dwelling products as design input (`PartOManualEquipmentSelection`)

**Status (30 Sep 2026): implemented and tested. The PR is open against `sow/2026-Q3` and is NOT merged. It is the SAM
half of finishing SAM_UI PR-4 (SAM-BIM/SAM_UI#150), which consumes it. Merge this first.**

- Branch `feature/parto-manual-equipment-selection-2026-09-30`, from `sow/2026-Q3` `9fbe8353`.
- Architecture: SAM_UI `documentation/PartO-ModelStateArchitecture.md` (approved), plus the owner decision below.

## Problem

In Manual mode an engineer picks each dwelling's product in the Review window. The pick is written onto the air
handling unit the preparation builds (`AirHandlingUnitParameter.VentilationUnitReference`), and that unit is run
output. The pick reached the next case only because the next case was prepared from the previous result. SAM_UI
PR-4 removes that carry-forward (every case is prepared from the design model), so the pick was lost.

**Owner decision (30 Sep 2026):**
- Hand-picked per-dwelling products are **Part O design input**, persisted independently of prepared or run systems.
- A preparation materialises them onto its temporary units.
- They are never copied from a previous result's unit.

## Representation, and why

A new `PartOManualEquipmentSelection` is stored as a new, additive `AnalyticalModelParameter.PartOManualEquipmentSelection`.

- **Keyed by dwelling zone guid**, at most one product each, and never by name.
- **Identities only**: manufacturer, model and reference. There is no instance guid and no capacity.
- **Canonical**: dwellings are written in zone-guid order, so two identical selections write the same bytes.
- **Schema `PartOManualEquipmentSelection:v1`**. An unknown schema loads as `IsValid` false and is not applied.
- **Absent** means no hand-picked product. Nothing is inferred from systems, units or results, and no migration is
  needed.

Existing types were considered and rejected:
- `PartOEquipmentSelection` is documented as *"a candidate constraint, never an assignment"*. It is the mode and the
  pool, and its `Matches` drives preparation reuse. Putting assignments in it would break that contract.
- Mixed Design's `PartODwellingStrategySet` is Mixed Design's authority (route, product, cooling, airflow basis).
  Sharing it would make an Iteration 2 pick a Mixed strategy and vice versa.

The new type follows that set's pattern (zone-guid keyed, canonical, schema-versioned) without sharing its meaning.

## Preparation

`Modify.PreparePartOIteration` gains a **new overload** with a seventh parameter, `PartOManualEquipmentSelection`. All
seven parameters are required. Null keeps every existing behaviour exactly.

**Binary compatibility.** The original six-parameter signature is kept unchanged, as a public overload, defaults
included (`ventilationUnitCapacityDescriptors = null`, `isolate = false`). It delegates to the new overload with no
selection. So a caller compiled against the previous SAM still binds, and every existing source call (4, 5 or 6
arguments) resolves to it. The new overload deliberately has no optional parameters: two overloads with optional
tails would make those short calls ambiguous.

- **Manual authority only**, meaning no catalogue was offered, and only on the MVHR route:
  - each dwelling's built or reused unit is assigned its chosen product through `Modify.AssignVentilationUnit`, the
    same write the Review window's manual table commits, so no airflow moves;
  - a dwelling with no choice keeps its unit as built.
- **With a catalogue**, the automatic rule decides. The selection is not applied, and a note says so.
- **An unreadable schema** is not applied, and a warning says so.
- **The caller decides the iteration.** 1a is also prepared without a catalogue, so only a manual Iteration 2 passes
  the selection. Nothing here reads it off the model or writes it.
- `PartOIterationPreparation.DwellingZoneGuids` (new) gives the dwelling zone each built unit is for, item for item
  with `AirHandlingUnits` (`Guid.Empty` for the zone-less whole-model case). SAM_UI keys the Review table's choices
  by it instead of by name.

## Files

- `SAM/SAM.Analytical/Classes/PartOManualEquipmentSelection.cs` (new)
- `SAM/SAM.Analytical/Enums/Parameter/AnalyticalModelParameter.cs`: `PartOManualEquipmentSelection`, appended last
- `SAM/SAM.Analytical/Modify/PreparePartOIteration.cs`: the original overload kept, the manual-aware overload, and dwelling zones through `DwellingSpaceGroups`
- `SAM/SAM.Analytical/Classes/PartOIterationPreparation.cs`: `DwellingZoneGuids`
- `SAM/SAM.Tests/PartOManualEquipmentSelectionTests.cs` (new, 10 tests)
- `documentation/PartO-ManualEquipmentSelection-PR.md` (this record)

## Evidence

- **`PartOManualEquipmentSelectionTests`: 10/10.**
  - Set, replace, remove and copy-out semantics; nothing that identifies nothing is stored.
  - Canonical round trip through the model's own JSON, and `Matches`.
  - Absent means none, and an unknown schema is invalid.
  - The selection is design input: a design carrying it has no `PartOBaselineFindings`, and `RemovePartORunState`
    keeps it.
  - A manual preparation gives dwelling 1 product A and dwelling 2 product B, found by zone. It runs no selection
    rule and leaves the input model byte-identical.
  - A dwelling with no choice gets no product.
  - 1a is unchanged, and an automatic run selects exactly what it selects without the selection (with a note).
  - An unreadable selection is not applied, with a warning.
  - **The original signature is pinned by reflection**: public static extension method, the same six parameter types,
    names, order and defaults, and it is a separate method from the new overload.
  - **The original overload behaves as the new one with no selection.** With 4 arguments it selects nothing, even
    from a model carrying hand-picked products (it never reads the parameter). With 5 or 6 arguments and a catalogue
    it chooses exactly what the new overload chooses.
- **Full SAM.Tests: 2705/2705.**
- **Mutation checks.**
  - Assignment skipped: the two preparation tests fail, and three SAM_UI journey tests fail.
  - Manual applied under a catalogue: `Without_manual_authority_nothing_changes` fails.
  - The legacy overload's `isolate` default changed: the signature pin fails.

## Risks

- **Zone-less models.** A model with no zones is prepared as one whole-model dwelling. It has no zone identity, so
  no hand-picked product is applied to it.
- **An isolated preparation** reports the same zone guids. Isolation extracts the assessed dwellings with their
  zones.

## Next step

Owner review, then merge before SAM-BIM/SAM_UI#150. SAM_UI CI stays red until this is on `sow/2026-Q3`.
