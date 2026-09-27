<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O — target architecture

**This file is the durable record of the Approved Document O architecture.** It is not a progress log and
not PR prose: it states the separation of concepts that the Part O code must keep, the algorithm for each
iteration, and which parts are implemented. Implementation evidence lives in
[`PartO-TAS-VALIDATION.md`](PartO-TAS-VALIDATION.md); repository state lives in
[`PartF-HANDOVER.md`](PartF-HANDOVER.md).

---

## 1. The central separation

```text
regulatory requirement
        |
        v
ventilation route
        |
        v
equipment selection
        |
        v
Part O operating scenario
        |
        v
simulation
        |
        v
result
```

**These are five different concepts and they must not be collapsed into one another.** Every Part O defect
found so far has been a collapse of two adjacent boxes:

| Collapse | What it produced |
|---|---|
| requirement == equipment | A regulatory airflow read off an MVHR unit's capacity |
| route == stale system metadata | `VentilationSystemTypeName` deciding what the simulation is |
| route == "not NV" | An MVHR system invented for a dwelling nobody said had one |
| scenario == ventilation system | `BasePassive` meaning one specific system, so an NV result could only be attributed to a scenario asserting design-rate mechanical ventilation |

### The rule that follows

> **Part F calculates regulatory ventilation requirements.**
>
> It must **not** derive the regulatory airflow as a percentage of an MVHR unit's capacity.
>
> For an MVHR route, the calculated requirement is later used to **select** an appropriate physical MVHR
> unit. The unit's capacity is *equipment capability*; it is never the source of the requirement.

---

## 2. The algorithm

```text
DWELLING / ZONE DATA
        |
        v
PART F REGULATORY REQUIREMENTS
  - whole-dwelling ventilation requirement
  - wet-room extract requirements
  - other Part F requirements where implemented
        |
        v
EXPLICIT PART O VENTILATION ROUTE
  never inferred from stale system metadata
        |
        +-------------------------------------+
        |                                     |
        v                                     v
+-----------------------+          +------------------------+
| Iteration 1a          |          | Iteration 1b           |
| Base MVHR             |          | Base Natural Vent.     |
|                       |          |                        |
| Windows available     |          | Windows available      |
| No acoustic restrict. |          | No acoustic restrict.  |
|                       |          |                        |
| Continuous MVHR       |          | NO central supply      |
| supply/extract        |          | NO continuous MVHR     |
| = Part F requirement  |          | extract                |
|                       |          |                        |
| select minimum        |          | openings/background    |
| compliant MVHR unit   |          | ventilation            |
|                       |          |                        |
|                       |          | intermittent wet-room  |
|                       |          | extract only where its |
|                       |          | operation is truthfully|
|                       |          | characterised          |
+-----------------------+          +------------------------+
        |                                     |
        +------------------+------------------+
                           v
                        RESULT
                           |
                 further mitigation needed
                           |
                           v
+----------------------------------------------+
| Iteration 2                                  |
| Acoustic restriction                         |
| - FullyClosed OR BedroomNightClosed          |
|                                              |
| selected REAL MVHR unit                      |
| summer bypass                                |
| demand/boost capability                      |
| actual available airflow                     |
+----------------------------------------------+
                           |
                           v
                        RESULT
                           |
                   optional larger unit
                           |
                           v
+----------------------------------------------+
| Iteration 3                                  |
| Same acoustic restriction                    |
| selected REAL MVHR unit                      |
| boost airflow                                |
| active cooling / tempering                   |
| supply T from manufacturer performance       |
+----------------------------------------------+
                           |
                           v
                        RESULT
```

### 1a and 1b are alternatives, not steps

**Iteration 1a and Iteration 1b are two alternative BASE configurations of the same dwelling.** They are
not sequential mitigation stages. A dwelling is assessed at 1a *or* 1b according to its ventilation route;
Iteration 2 is the first thing that is genuinely "further mitigation".

Anything that models the Part O stages as a single ordered list — a numbered enum walked in order, a UI
that greys out later steps until earlier ones fail, a report that prints 1a then 1b — has the engineering
wrong.

### Naming

Engineering meaning is carried by **semantic names**, never by numbers:

| API (durable) | Grasshopper / UI (presentational) |
|---|---|
| `PartOIteration.BaseMVHR` *(future; today `BasePassive`)* | Iteration 1a |
| `PartOIteration.BaseNaturalVentilation` | Iteration 1b |
| `PartOIteration.AcousticRestricted` | Iteration 2 |
| *(no enum member)* | Iteration 3 — see below |

> **Iteration 3 is not an enum member.** `PartOIteration.ActiveTrimCooling` exists in the enum but drives
> nothing: it has no operating mode (`Query.PartOIterationOperatingMode`), no ventilation mode
> (`Query.PartOIterationVentilationMode`) and no operating assumptions (`Query.PartOOperatingAssumptions`) —
> all three refuse it by name — and `SAM_UI` deliberately does not offer it
> (`PartOVentilationStrategyOption`, pinned by `PartOPresentationTests`). Iteration 3 is orchestrated
> entirely in `SAM_UI` (`PartOIteration3Pipeline` and the A/B action). The enum member is a reserved slot
> whose name predates the design; reading it as "the API for Iteration 3" is wrong.

"1a"/"1b" must never be the only place the engineering meaning is recorded. A number in a public API is a
fact about a document's layout, not about a building.

---

## 3. The explicit Part O ventilation route

### The type

```csharp
namespace SAM.Analytical.Enums;

public enum PartOVentilationMode
{
    Undefined,            // nothing stated -> REFUSE
    NaturalVentilation,   // Iteration 1b
    MVHR,                 // Iteration 1a
}
```

Resolved by `SAM.Analytical.Query.PartOVentilationMode(...)` from what the assessment **states**, never
from what the model's existing objects happen to carry.

### The contract

```text
PartOVentilationMode.NaturalVentilation
    -> no continuous MVHR supply
    -> no continuous MVHR extract
    -> opening / background ventilation remains available
    -> intermittent extract remains a SEPARATE concept
    -> TAS / scenario assessed as natural ventilation

PartOVentilationMode.MVHR
    -> the Part F continuous requirement can be applied
    -> a later physical MVHR selection can satisfy that requirement
    -> TAS / scenario assessed as mechanical

missing / unsupported / ambiguous mode
    -> REFUSE
```

### The rule that was removed, and must not come back

```text
FORBIDDEN:   anything that is not "NV" == mechanical
```

`UV`, an empty string, an unrecognised word and stale model metadata are all **absences of a stated
route**, not statements that the dwelling has an MVHR system. None of them may cause Part F System 4
airflow to be written. They refuse.

`MV` is refused for the same reason and it is worth being explicit about why, because it looks like a
statement and is not one. Part F System 3 (continuous mechanical extract) and System 4 (continuous supply
and extract with heat recovery) are different buildings. What `PartFCalculator` sizes is System 4 — a
supply terminal in every habitable room. Writing that onto a dwelling whose route is stated only as
"mechanical" invents the supply half, which is the same defect as inventing the whole system for an NV
dwelling, one terminal smaller.

### Physical system metadata is evidence, never authority

A `SAM_System`, a `SystemTemplate`, or an `InternalCondition.VentilationSystemTypeName` may *corroborate*
a route and may one day be *validated against* it. In this implementation none of them is read to decide
the route, and — equally important — **none of them is written to force one**. Mutating
`VentilationSystemTypeName` so the simulation takes a chosen branch would put the decision back into the
metadata it was taken out of, and would make the model on disk a lie about the building.

The explicit Part O route is authoritative for **preparation, export and assessment**.

---

## 4. Iteration 1b — Base Natural Ventilation (implemented)

```text
BaseNaturalVentilation

no acoustic restriction imposed by the iteration

authored opening behaviour remains authoritative:
    Unrestricted / fully available
             OR
    NightClosed with authored hours

no central mechanical supply

no continuous MVHR extract

intermittent WC / bathroom / kitchen extract:
    preserve / model ONLY if its actual operation can be
    represented truthfully with existing SAM data
```

**The iteration imposes nothing on the openings.** `OpeningRestriction` is authored building data and
`PartOOpeningProperties.Schedule` is *derived* from it, so resetting a restriction to match a stage's
assumption deletes that aperture's `PartO_DayOpen_HH_HH` availability schedule from the model that reaches
TAS. Disagreement is reported, never reconciled.

**An intermittent Part F design extract rate must not be turned into a continuous 24/7 extract flow.**
Where SAM carries the rate but no truthful operation schedule or control, the data is preserved and the
runtime behaviour is *not* invented. See §6.

---

## 5. Iteration 1a — Base MVHR (implemented)

```text
Part F regulatory requirement
       |
       v
explicit MVHR route  +  BasePassive
       |
       v
apply continuous supply / extract requirement       Modify.ApplyPartFVentilationRates
       |
       v
realize 0..N design terminals per requirement       Modify.RealizePartFVentilationTerminals
       |
       v
generic ventilation system + air handling unit      Modify.AddPartOBaseMVHRSystem
       |
       v
derive system design duty, check against Part F     Query.ReconcileVentilationSystemDesignDuty
       |
       v
directional air movements, per room per direction   Modify.AddAirMovementObjects
  + the unit's own exhaust, to outside
       |
       v
internal transfer air, routed by the Part F         Modify.AddPartFTransferAirMovements
airflow network over the model's own adjacencies
       |
       v
conservation checked at every node                  Query.AirMovementResidual
       |
       v
TAS inter-zone air movements                        Modify.UpdateIZAMs  (SAM_Tas)
       |
       v
TM59 mechanical criterion

  [ Iteration 2 ] select minimum compliant MVHR unit against that duty.
                  Unit capacity is equipment capability,
                  NOT the source of the Part F requirement.
```

### The four separations this chain keeps

```text
PartFVentilationTerminalRequirement        what Approved Document F requires of a room
        |  realized by 0..N
VentilationTerminal                        what the design puts in the room
        |  connected to
VentilationSystem + AirHandlingUnit         what moves the air
        |  realized as
SpaceAirMovement                           what the simulation moves, this stage
```

- **The requirement is read and never written.** Realization touches no `PartFSpaceData`.
- **`0..N` is the model, never `1`.** One 20 l/s terminal may become two of 10 without changing the
  requirement, the space, the system, the duty or the scenario key. A space's duty is the **sum** of its
  terminals, never the count of them. Realizing one terminal per continuous requirement is the initial
  strategy, not an invariant.
- **The system duty is derived, never stored.** `Query.VentilationSystemDesignDuty` sums the connected
  terminals on demand, so it cannot go stale. It is checked against the Approved Document F requirement the
  spaces carry, and a disagreement at the system total **refuses**.
- **Supply and extract are separate everywhere.** A balanced heat recovery system balances at the *system*:
  a bedroom is supplied and not extracted, a wet room extracted and not supplied, and the air moves between
  them as transfer air. Deriving both directions from one figure — which is what the generic air-movement
  builder did before, having nothing better to read — moves roughly the right total amount of air through
  the wrong rooms.

### The air movements are a network, and it must conserve

TAS will not simulate a building in which **any one zone's** inter-zone air movements do not balance. A
zone that gains air it never loses is refused outright — the EDSL documentation states the rule as *"any
air flow imbalance will be reported as a Max Pressure Exceeded error"*, and SAM sees it only as
`Simulation Failed`. Balance over the building as a whole is **not** enough; it is checked zone by zone.

That is not a formality for this design, it is the design: a balanced heat recovery dwelling balances at
the system, so almost every room is individually out of balance and the air that closes each of them is
transfer air. Two objects carry it, and neither adjusts a design duty to get there:

- **`Modify.AddPartFTransferAirMovements`** routes each space's net — supply less extract — through the
  dwelling using `PartFAirflowNetwork`, the same network Approved Document F paragraph 1.25 is assessed
  over. The connections are the model's own internal adjacencies. Where the network cannot route a space's
  net, the preparation **refuses and names the room**; it does not invent a route, and it does not quietly
  connect the room to outside, which would put untempered outside air into the wet rooms of a heat
  recovery dwelling and flatter the overheating result the assessment turns on.
- **The unit's exhaust**, built by `Modify.AddAirMovementObjects` as a `SpaceAirMovement` from the unit to
  a destination of **null**. Null is how outside is said. Its flow is the sum of the extract movements, so
  the unit's zone loses exactly the extract air it gained.

**Conservation is summed per node, never matched per route.** These movements form a directed network: one
unit feeds several rooms, a room may draw from several rooms and pass air on to several more, and flows
split and recombine along the way. No movement has a partner, and a check that looked for one would reject
a correct model. `Query.AirMovementResidual` sums every movement at each node — counting the unit's
outside intake on the same terms the TBD writer derives it — and `Modify.PreparePartOIteration` refuses on
any node that does not come out at zero.

**The network is not built over the served spaces alone.** A `VentilationSystem` relates only to the spaces
carrying a design terminal, but paragraph 1.25's transfer air crosses a space with none — a hall, a
landing, a lobby — on its way between a supplied room and an extracted one. `Query.PartFTransferAirSpaces`
widens the served spaces out to every other space of the *same dwelling*, read from the model's own `Zone`
membership via `Query.PartFDwellingZones` — the same authority `PartFCalculator` sizes with, so a space this
calls part of a dwelling is exactly a space Part F sized as part of it — so a zero-terminal internal hall
stays in scope as a transfer node and is not routed around. It is
not simply every space in the model, either: a communal corridor, stair or landlord area is excluded by the
same widening, so the network can never carry one dwelling's transfer air through a common part into
another dwelling, which Approved Document F forbids. Pinned by
`SAM.Tests/PartFTransferAirDwellingScopeTests.cs`.

### Requirement lineage across a Part F recalculation

`PartFCalculator` mints a new `PartFVentilationTerminalRequirement` on every run, so a design terminal's
`RequirementGuid` is stale the moment Part F is recalculated. `PartFTerminalReference` therefore carries the
requirement's **regulatory identity** beside the guid — space, `PartFTerminalRole`, source paragraph — and
re-linking is explicit in every direction: exactly one match re-links and reports; no match **refuses**;
more than one match **refuses as ambiguous**. Nothing is guessed and nothing is silently repaired.

The role is on the *reference*, not on the terminal. `FlowClassification` stays `Supply`/`Extract`, because
local kitchen extract and general wet-room extract are the same thing physically and differ only under the
Approved Document — and that distinction must not leak into a classification generic MEP work reads.

### The model's own ventilation systems are evidence, not the design

A model routinely arrives carrying the system-template assignment it was built with. The licensed acceptance
model splits its rooms across an `NV` system, an `MV` system and a `UV` system while the assessment states
one MVHR route for the whole dwelling. Iteration 1a **builds its own** system rather than attaching Base
MVHR terminals to one of those — attaching them to a system typed `NV` would be untrue, and choosing between
three would be a guess — and the air-movement realization is **scoped** to the system it built, so no room
served by two systems is ventilated twice. What the model says is reported room by room as a warning and
left exactly as authored. Reconciling it is design work, or Iteration 2's when it selects a real unit.

**Iteration 2** — acoustic restriction, summer bypass, boost, larger-unit selection against the derived
duty — is recorded in §2 and **not implemented**. It extends the topology built here rather than replacing
it. (This is why the Iteration 2 stage of the 2026-09-15 real-project acceptance came out bit-identical to
Iteration 1a: the stage runs, but the behaviour that distinguishes it does not yet exist.)

**Iteration 3** — active cooling against a manufacturer supply-temperature table — **is implemented**, by
PR5B, and is orchestrated in `SAM_UI` rather than here; see `PartO-TAS-VALIDATION.md`. It too extends this
topology rather than replacing it.

---

## 6. Wet-room intermittent extract — what SAM actually has

Investigated 2026-08-26 against `SAM.Analytical`. Four questions, four answers:

**1. What Part F design rate exists?**

Two separate things, and they are not the same thing:

- `PartFCategory.IntermittentExtractRate_Lps` — the Table 1.1 intermittent rates, present in
  `SAM_PartFSpaceRulesUKDwellingsMVHR.json`: **15 l/s**, **15 l/s**, **6 l/s** and **30 l/s** on the four
  categories that carry one.
- The kitchen local-extract terminal built by `PartFCalculator`, which for
  `PartFExtractMethod.CookerHoodExtractingOutside` and `SeparateIntermittentExtract` carries
  `HighFlowRate_Lps` = 30 / 60 l/s from `PartFData`.

**2. Is it currently represented as intermittent?**

Partly.

- The **kitchen** terminal is: `OperatingMode = HighBoost`, `IsInBalancedFlow = false`, and
  `ContinuousDesignFlowRate_Lps` left null — so it is already outside every continuous total and outside
  everything `Modify.ApplyPartFVentilationRates` writes.
- The **bathroom / WC** intermittent rate is **not**. `PartFCategory.IntermittentExtractRate_Lps` is
  parsed from the rules JSON, stored on the category, and **read by nothing** — no calculator, no report,
  no export. It is inert data. What a wet room actually receives from `PartFCalculator` is a *continuous*
  `GeneralExtract` terminal with `IsInBalancedFlow = true`, because the calculator is unconditionally
  System 4 shaped (paragraph 1.67) whatever the dwelling's real strategy.

**3. Does SAM carry a trustworthy runtime schedule or control for it?**

**No.** Nothing on `PartFVentilationTerminalRequirement`, `PartFSpaceData` or `PartFCategory` states when
an intermittent extract runs, for how long, or what triggers it. There is no occupancy link, no humidity
control, no daily availability schedule and no duty cycle. The only operating fact recorded is the enum
value `PartFOperatingMode.HighBoost`, which names a *rate*, not an operation.

**4. Does TAS currently have a truthful write path for it?**

**No — for an *intermittent* extract.** There is now one for a *continuous* one; the two answers are
different and the distinction is the whole point of this section.

`InternalConditionParameter.ExhaustAirFlow` still reaches **no** TBD field. It is read in exactly one place
in the whole repository — `PartODiagnosticLog`, for reporting. The internal-gain export is supply-only:
`SupplyAirFlow` / `SupplyAirFlowPerArea` / `SupplyAirFlowPerPerson` reach the TBD as `freshAirRate` and the
`ticV` factor, plus the `SAMZoneMetadata` decomposition in the zone description.

What Iteration 1a added is a *different* mechanism, not a write path for that parameter. A design extract
terminal becomes a `SpaceAirMovement` from the room **to the air handling unit**, and `Modify.UpdateIZAMs`
writes it as an inter-zone air movement on the unit's own TAS zone, sourced from the room. That direction
has to be expressed that way round: `TBD.IIZAM` has a source zone, target zones and a `fromOutside` flag,
and **no outward direction at all**, so an air movement with an unstated destination is written onto its own
room's zone with neither a source nor outside air behind it and moves nothing. That is what every outward
movement did before Iteration 1a, and it is the one `SAM_Tas` production change the milestone needed.

None of that gives an *intermittent* extract a representation. The rate is continuous, and the movement runs
whenever its profile does.

### The consequence, and it is deliberate

Iteration 1b is accepted as:

```text
natural ventilation through openings
+
zero continuous mechanical ventilation
```

with the intermittent wet-room extract **preserved as data and not modelled as runtime behaviour**. This
is not a gap that blocks the NV/opening workflow: SAM has the rate but not the operation, TAS has neither,
and inventing a schedule would be exactly the failure mode this architecture exists to prevent. It is a
documented future runtime-control item.

Iteration 1a does not change that. It realizes the **continuous** extract a wet room is sized for, because
the calculator is System 4 shaped and that terminal runs at the Approved Document F sizing condition. A
terminal with no `ContinuousDesignFlowRate_Lps` — a cooker hood, a separate intermittent extract fan — is
realized as nothing at all, so no intermittent rate is turned into a 24/7 flow.

---

## 7. What "no mechanical airflow applied" claims, and what it does not

**It claims:** SAM has **not** invented an MVHR or MVRE system for a dwelling nobody said had one.

**It does not claim:** that the dwelling's natural-ventilation Part F design has been sized. System 1
background/trickle ventilator provision and purge ventilation are calculated **nowhere** in SAM.
`PartFCalculator` takes no ventilation-strategy input at all and remains System 4 shaped for every route.

**Never report the NV result as "Part F NV sizing".** This wording is pinned by a test.

---

## 8. Result identity

`OverheatingScenario.Key` is derived from the assessment scope, the zone guid, the iteration, the system
template and the operating assumptions. It is a permanent identity: two engineers stating the same
assessment get the same guid, and a scenario reloaded from JSON is recognisably the same one.

Two consequences follow:

- **The iteration name is inside the key.** Renaming `BasePassive` to `BaseMVHR` re-keys every assessment
  ever attributed to it. That rename is a migration, not an edit, and it is not done here.
- **An iteration's operating assumptions are inside the key.** `BasePassive` asserts
  `Mechanical Ventilation At Design Rate = True`. Attributing a natural-ventilation result to it would
  therefore mint a permanent identity that states something false about the building. That — not style —
  is why `BaseNaturalVentilation` had to be added rather than reused.

`OverheatingScenario:v2` — making opening behaviour a property of the model rather than of the stage — is
still deferred. Nothing here depends on the old assumption that `BasePassive` means one specific
ventilation system; that dependency is what `PartOVentilationMode` removes.

---

## 9. Implementation status

| Concept | Status |
|---|---|
| Explicit `PartOVentilationMode` route | **Implemented** |
| Route refuses missing / unknown / ambiguous / mixed | **Implemented** |
| Iteration 1b `BaseNaturalVentilation` | **Implemented**, licensed acceptance in `PartO-TAS-VALIDATION.md` |
| Authored opening behaviour preserved through preparation | **Implemented** |
| Iteration 1a `BasePassive` — design terminals, generic system, design duty, directional runtime | **Implemented** — §5 |
| Design terminal `0..N` per requirement, per space, per direction | **Implemented** — §5 |
| Requirement lineage re-linked explicitly across a Part F recalculation | **Implemented** — §5 |
| Internal transfer air, routed by the Part F airflow network | **Implemented** — §5 |
| Air handling unit exhaust to outside | **Implemented** — §5 |
| Conservation refused per zone, summed over every movement | **Implemented** — §5 |
| Transfer air through a space with no design terminal (an internal hall) | **Implemented** — §5, `Query.PartFTransferAirSpaces` |
| MVHR unit **selection** against the derived duty | Not implemented — Iteration 2, §5 |
| Design terminal physical placement (`Location`) | Seam present, unused — §5 |
| Reconciling the model's own ventilation systems with the stated route | Not implemented — reported only, §5 |
| Iteration 2 acoustic restriction / bypass / boost | Not implemented — §2 |
| Iteration 3 active cooling / manufacturer performance | **Implemented** (PR5B) — orchestrated in `SAM_UI`; licensed real-project acceptance 2026-09-15, see `PartO-TAS-VALIDATION.md` |
| Per-zone (mixed NV + mechanical) airflow application | `PreparePartOIteration` still refuses a mixed call (legacy, unchanged). Investigated (PR0, 2026-09-26): [`PartO-MixedDwellingStrategies-PR0.md`](PartO-MixedDwellingStrategies-PR0.md) |
| Mixed dwelling strategies — persisted per-dwelling authority + one deterministic NV/MVHR materialisation | **Implemented in SAM (PR1)** — `PartODwellingStrategySet`, `Modify.MaterialisePartODwellingStrategies`, `Query.PartOBaselineFindings`; [`PartO-MixedDwellingStrategies-PR1.md`](PartO-MixedDwellingStrategies-PR1.md). Active cooling recorded and refused (PR3); SAM_UI grid not yet (PR2) |
| System 1 background ventilator / purge sizing | Not implemented anywhere — §7 |
| Intermittent wet-room extract runtime control | Not implemented — §6 |
| `OverheatingScenario:v2` | Deferred — §8 |
