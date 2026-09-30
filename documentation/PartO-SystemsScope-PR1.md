<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O PR-1 (SAM half): the shared Part O Systems materialisation scope

**Status (30 Sep 2026): implemented and tested. The PR is open (SAM-BIM/SAM#171) against `sow/2026-Q3` and is NOT merged. SAM_UI PR-1
(SAM-BIM/SAM_UI#151) consumes the new public API, so merge this one first.**

- Branch `feature/parto-systems-scope-2026-09-30`, from `sow/2026-Q3` `ffb61972` (after SAM#170 and its closeout).
- Architecture authority: SAM_UI `documentation/PartO-ModelStateArchitecture.md` (approved 30 Sep 2026), step 2.
- SAM_UI record: `SAM_UI/documentation/PartO-SystemsScope-PR1.md`.

## Problem

On the TAS Systems route (any cooled Mixed dwelling), SAM_UI handed SAM_Systems the whole materialised cluster.
SAM_Systems requires every ventilation system it is handed to name an air handling unit. The
`Modify.AddMechanicalSystems` template's `NV 1` / `UV 1` never do (they are non-mechanical by construction), so a
correct design was refused: "Ventilation system 'UV' names no air handling unit". Iteration 3 avoided this with an
identity-based scope that lived in SAM_UI WPF (`PartOIteration3SystemScope`). Mixed never adopted it.

## What this PR adds (additive only)

```csharp
public static PartOSystemsMaterialisationScope Query.PartOSystemsMaterialisationScope(
    AdjacencyCluster adjacencyCluster,
    IEnumerable<Guid> guids_VentilationSystem_Built,
    IEnumerable<Guid> guids_Space_Dwelling)
```

- `PartOSystemsMaterialisationScope` (result): `AdjacencyCluster` (a shallow working copy, or null), `Guids_Retained`,
  `Guids_Removed` (both in guid order), `Exclusions`, `Refusals`, `Notes`, `Refusal`, `IsScoped`.
- `PartOSystemsScopeExclusion`: a system left out, with guid, `Name`, `FullName`, terminal count and message.
- `PartOSystemsScopeRefusal`: `Reason` plus the system, terminal, flow classification, stated airflow and space it
  concerns, and a message.
- `Enums.PartOSystemsScopeRefusalReason`: `NoModel`, `NoIdentities`, `IdentityNotOnModel`, `DutyServesNoSpace`,
  `DutyInsideDwellingScope`, `DutyOutsideDwellingScope`, `NotRemovable`.
- `internal Query.IsPartOEffectiveMechanicalDuty(VentilationTerminal)`.

No existing signature changed. Constructors of the result types are internal.

## The rule (Iteration 3's, moved unchanged)

1. Keep exactly the systems Part O built, by guid. Never by name.
2. For every other `VentilationSystem`: if none of its terminals states effective duty, leave it out of the working
   copy and note it. Effective duty = a stated, finite, non-zero `DesignFlowRate_Lps` (null, 0, NaN and ±∞ are not;
   the flow classification and the system type are not read).
3. If any does, refuse, once per room it serves: inside the assessed dwellings = a second design; outside = the
   no-IZAM source would silently remove it model-wide; no room at all = fail closed.
4. Only `VentilationSystem`s are read. Cooling and heating systems (`AHU 1`, `FCU 1`, `RAD 1` …) are never
   consumed or removed.
5. Fail closed: any refusal yields no working copy, no identities, no notes (structural, in the constructor).
6. The supplied cluster is never modified.

Messages are route-neutral engineer wording and use `FullName` (`NV 1`). Iteration 3 keeps its own established
wording by formatting the structured fields in SAM_UI; it no longer decides anything.

## Decisions and assumptions

- **Name.** `PartOSystemsMaterialisationScope`, as the approved plan suggested; it matches SAM's
  `Query.PartO*` + result-class convention (`PartOMaterialisation`, `PartOMaterialisationRefusal`).
- **Structured refusals** rather than strings, so Iteration 3 can keep byte-identical text and PR-6 can list systems
  without parsing messages.
- **`Name` and `FullName` both kept.** Iteration 3's persisted text used `Name` (the type name, `MV`); the engineer
  sees `FullName` (`MV 1`).
- **Not changed:** `MaterialisePartODwellingStrategies.AuthoredMechanicalSystems` (PR-2). The `MV 1`/`AHU1`
  `SharedSystem` false positive remains, on purpose.

## Files

- `SAM/SAM.Analytical/Query/PartOSystemsMaterialisationScope.cs` (query + effective-duty definition)
- `SAM/SAM.Analytical/Classes/PartOSystemsMaterialisationScope.cs`, `PartOSystemsScopeExclusion.cs`,
  `PartOSystemsScopeRefusal.cs`
- `SAM/SAM.Analytical/Enums/PartOSystemsScopeRefusalReason.cs`
- `SAM/SAM.Tests/PartOSystemsMaterialisationScopeTests.cs` (23 test cases)
- `SAM/SAM.Tests/PartODwellingStrategyMaterialisationTests.SystemsScope.cs` (4 tests on the real Mixed materialiser)

## Evidence

- **Focused:** 27/27 (`PartOSystemsMaterialisationScopeTests` + `SystemsScope_*`).
- **Full SAM.Tests:** 2732/2732 (2705 before this PR; +27).
- **Production-shaped fixture:** a real `Modify.AddMechanicalSystems` scaffold (`NV 1`, `UV 1` with no unit;
  `MV 1` + `AHU1`; cooling `AHU 1`/`FCU 1`; heating `RAD 1`) beside a cooled Mixed dwelling on the Systems route.
  The scope retains exactly `Record.VentilationSystemGuids`, leaves the three scaffold systems out with notes, and
  every retained system names a resolvable unit.
- **Mutation checks (all killed, all reverted clean):** S1 retain by name instead of guid (2 fail); S2 duty never
  refuses (9 fail); S3 infinite airflow counted as duty (2 fail); S4 constructor fail-closed removed (1 fail, after
  adding `A_refused_result_never_carries_a_working_copy`); S5 filter NV/UV by name (4 fail).
- **Iteration 3 equivalence** and the owner's real model: see the SAM_UI record.

## Risks / not verified

- No licensed TAS run (not required for PR-1).
- SAM_Systems still scans every ventilation system it is handed (PR-3). This PR only makes sure the caller hands it the
  right ones.

## Next step

Owner review → merge this PR → rebase/revalidate SAM_UI PR-1 against merged SAM → merge SAM_UI PR-1 → closeouts.
Then PR-2 (effective-duty classification in `AuthoredMechanicalSystems`) and PR-3 (SAM_Systems D2 scope).
