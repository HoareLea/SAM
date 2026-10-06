<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O — mixed dwelling strategies: PR0 architecture investigation

**Status: PR0 complete and APPROVED by the owner (2026-09-26). No production code was changed.**

### Owner decisions (binding for PR1–PR4)

1. **A clean baseline is mandatory.** Mixed materialisation starts from a clean pre-Part-O analytical model.
   PR1 adds **no** lossy Part O undo or adoption mechanism. `MaterialisePartODwellingStrategies` **fails
   explicitly** if the supplied model is not an acceptable baseline (D1).
2. **Common / corridor spaces are included automatically.** Assessed common spaces (e.g. a
   `TM59_Communal Corridor` internal condition) take part in the final mixed annual run. They are **not**
   dwelling strategy rows. Their scenario and classification are derived from authoritative internal
   condition / zone state (`IsDwelling` and the space's **assigned** internal condition), **never from
   space names**. This closes #12. PR1 must not use `TM59InternalConditionResolver` for it: that resolver
   classifies from `space.Name` (D2.5).
3. **Project-wide constraints live at project level.** Rules such as "all dwellings MVHR" and the allowed
   product pool are project settings (beside `PartOEquipmentSelection`). The dwelling grid consumes them.
   They are **not** duplicated into each `PartODwellingStrategy`.
4. **2B airflow authority stays singular.** No copy of terminal airflow values in `PartODwellingStrategy`.
   The accepted design airflow lives on `VentilationTerminal`. The strategy holds only the
   reference / intent / fingerprint that identifies the accepted design (D3).
5. **Cooling stays gated.** PR1 does not materialise active cooling. It records it and refuses it
   structurally until the PR3 licensed TPD proof settles the final authority (D5).

**Accepted order:**
- PR1 — SAM: per-dwelling authority + deterministic NV/MVHR materialisation.
- PR2 — SAM_UI: scalable dwelling strategy assignment + mixed run.
- PR3 — SAM + SAM_Tas: cooling authority with licensed TAS proof.
- PR4 — real-project TAS acceptance + deploy.

Investigation date
2026-09-26. Evidence base: SAM `sow/2026-Q3` `00db4b85`, SAM_Tas `sow/2026-Q3` `aa00ff9`, SAM_UI `4b773f3e`
(Pass 6), SAM_Systems `2213373`. The production pins (SAM `22f9c743`, SAM_Tas `b32c0808`) differ only in
changes outside Part O preparation.

**Question.** Can one `AnalyticalModel` carry a *different final Part O strategy per dwelling*? For example,
Flat 1 NV, Flat 2 MVHR unit A, Flat 3 MVHR + cooling, Flat 4 MVHR unit B, and a communal corridor. It would be
simulated once in TAS and assessed once against TM59, and that mixed-model result would be the final
engineering authority.

**Short answer.** The *downstream* chain is already per space and per unit: TAS export, a single weather,
result import, and the TM59 criterion per space. The *upstream* chain, preparation, is not composable. Calling
it once per dwelling gives the right answer for MVHR next to MVHR. It damages every NV dwelling in the model.
It cannot turn an MVHR dwelling back to NV. Cooling has no representation on the model at all. The smallest
safe design is an explicit, persisted per-dwelling **intent** in SAM, with **deterministic materialisation from
a clean baseline** in one SAM call. Active cooling should be recorded but gated until the TAS Systems (TPD)
route has licensed proof.

Evidence: `SAM/SAM.Tests/PartOMixedStrategyProofTests.cs` has 13 tests, `[Trait("Category","PR0Investigation")]`,
all passing. They assert **observed** behaviour, including behaviour classified below as a defect. The existing
Part O/Part F suite is 1238/1238 green including them.

---

## A. Current architecture — what each step actually does to the model

| Step (UI) | Call | Model mutation (followed in code, not from UI labels) |
|---|---|---|
| **1b** NV | `Modify.PreparePartOIteration(BaseNaturalVentilation, …)` | **A plain copy; nothing is written** (`Modify/PreparePartOIteration.cs:172-182`). Nothing is stripped. |
| **1a** MVHR | `PreparePartOIteration(BasePassive, zones, route, descriptors: null)` | **Whole model:** `ApplyPartFVentilationRates` (`:197`) replaces every Part-F-sized space's internal condition with a renamed per-space clone under a new guid, zeroes the per-person/area/ACH bases, and writes `SupplyAirFlow`/`ExhaustAirFlow`. **Whole model:** `RealizePartFVentilationTerminals(null)` (`:346`) re-links existing terminals and adds missing ones; it never resizes. **Per dwelling** (loop `:384-574`): `AddPartOBaseMVHRSystem` (reuse via terminal relation; new system and unit `MVHR-NN` with random guids; fixed type guid `5a4b1f2c…`), removal and rebuild of that dwelling's own air movements, transfer air scoped to the dwelling, and a per-node balance refusal. Optional isolation afterwards. |
| **2** | *Same call as 1a* + a catalogue | Per-dwelling `SelectVentilationUnit` writes only `AirHandlingUnitParameter.VentilationUnitReference`, onto a replacement AHU. SAM's `AcousticRestricted` is **not used**: its route is Undefined and its operating mode is refused. The scenario key is identical to 1a's. |
| **2B** | `EvaluateTargetedDesignAirFlows` → `SetSpaceDesignFlowRate` → re-`PreparePartOIteration` (catalogue null) | Same-guid `VentilationTerminal` replacements per space and direction; the re-preparation rebuilds movements. Each round **chains from the previous round's TAS output** (SAM_UI `OptimisePartOTM59.cs:370,458`). |
| **3** | SAM_UI `PartOIteration3Pipeline` (no `PreparePartOIteration`) | Works on copies. It keeps the systems in `PartORun.Guids_VentilationSystem_Prepared`, which is **session-only** (`Query/PartOIteration3SystemScope.cs`). SAM_Systems `MechanicalVentilation` uses the MV or MVRE template **chosen once for the whole materialisation**, with cooling settings **per AHU guid**. SAM_Tas `NoIzamThermalSource` **removes every IZAM and ticV building-wide** (`Create/NoIzamThermalSource.cs:81-86`), then the TPD `SystemVentilationRoute`, then a thermostat bridge over the bound rooms only. TM59 is assessed against **Reference A's BasePassive scenarios** (`RunPartOIteration3.cs:156,852,972`), whose key states no cooling. |

**Ownership.**
- **SAM:** ventilation route; Part F requirement; terminals; systems and units; unit selection; overheating
  scenarios; the TM59 criterion, chosen **per space** through `VentilationStrategyMap`
  (`TMOverheatingCalculator.cs:312-372`).
- **SAM_Tas:** export, simulation and import, all per AHU (`UpdateIZAMs.cs:187-423`), per aperture and per
  space. One weather per TBD (`SAM.Weather.Tas/Modify/UpdateWeatherData.cs:51-104`).
- **SAM_UI:** orchestration. It also currently owns **Iteration 3 cooling resolution**
  (`Query.PartOIteration3CoolingResolution`) and the **prepared-design identity**.
- **SAM_Systems:** the TPD materialisation.

**The four airflow concepts, where they live today.**
- Part F required: `PartFSpaceData.Terminals[].ContinuousDesignFlowRate_Lps`.
- DesignAirFlow: `VentilationTerminal.DesignFlowRate_Lps`. The duty is derived and never stored.
- Selected capacity: the catalogue `VentilationUnitTemplate`. Only its identity is on the AHU.
- Operating airflow: the catalogue `VentilationUnitOperatingStrategy`, or the Iteration 3 CSV.

The invariant `PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow` holds.
A fifth, runtime-adjacent copy of the Part F rate lives on the cloned internal condition. **P5** shows it
disagreeing with the terminal after a 2B raise (terminal 9.02 l/s, internal condition 7.02 l/s).

**Weather.** One `WeatherData` per `PartOSimulationContext`, and so one per TBD. A full year is forced
(SAM_UI `Simulate.cs:206-209`). One simulation therefore has exactly one weather basis by construction. It is
only null-checked, and it is fingerprinted into the model (`SimulationResultProvenance.Fingerprint`, section 5,
model parameters).

---

## B. Composability verdicts (executed evidence)

| # | Concern | Verdict | Evidence |
|---|---|---|---|
| 1 | One call with NV + MVHR routes | **Refused by design** | P0; `Query/PartOVentilationMode.cs:162-183` |
| 2 | MVHR A, then MVHR C, in separate scoped calls | **Already safe** | P2, P5 (A's signature unchanged when C is prepared) |
| 3 | A different product per dwelling | **Safe with constraints.** The product lives per AHU. An automatic call whose scope covers a manually assigned dwelling **silently re-selects it**. | P3 |
| 4 | NV dwelling beside an MVHR dwelling | **Unsafe: isolation.** Preparing *any* MVHR dwelling writes Part F supply/extract onto *every* sized space, including NV and unassessed dwellings. It also creates unconnected terminals there. Their data reaches TAS through `ticV` whenever the NV room's internal condition carries a Ventilation profile that resolves (`SAM_Tas Modify/UpdateInternalCondition.cs:273-302`). In that case a neighbour's Part F rate is simulated as mechanical ventilation in an NV flat. Otherwise it is inert, but the model on disk misstates the NV flat. | P1 (Flat 2 *and* the unassessed Flat 3 both polluted), P11 |
| 5 | Switching a dwelling MVHR → NV | **Unsafe: blocker.** NV preparation succeeds, returns the full MVHR design unchanged, and states an NV scenario. TM59 would assess a mechanically ventilated flat against the NV criterion. | P4 |
| 6 | Order independence (A→B→C vs C→A→B) | **Engineering state equal; identity not.** All values are equal, including the unit movements' control profiles. Generic unit names follow call order (`MVHR-01`/`-02` swap). The name is the system→unit link (`VentilationSystemParameter.SupplyUnitName`), and it also leaks into the unit movement's profile names (`MVHR-01 Humidification`). | P2 |
| 7 | Idempotence of one dwelling | **Already safe** | P6 |
| 8 | A retained 2B design airflow | **Safe with constraints.** It survives preparing other dwellings and re-preparing its own. It must be a **balanced set**: a single raised value is refused, not rescaled. | P5 |
| 9 | Deterministic reconstruction | **Engineering state reproducible; guids not.** Two independent clones of one baseline with the same strategy set give equal signatures. Every generated object has a new guid, so the model fingerprint differs and prior results are never reused. That fails closed. | P10 |
| 10 | Sanitising a materialised model back to its baseline | **Not possible deterministically.** The internal-condition rewrite discards the authored bases and the original identity, and records neither. | P11 |
| 11 | Shared or authored system across dwellings | **Refuse.** SAM never builds shared plant. A legacy shared system is warned about and left alone, and isolation refuses it by name. | P7 |
| 12 | Communal corridor | **Safe for airflow.** Transfer air never crosses the corridor (`PartFTransferAirSpaces`). **Gap for assessment:** no per-dwelling workflow states a corridor scenario, so TM59 has no strategy for it. | P8 |
| 13 | TM59 over a composed model | **Already safe.** The union of per-call scenarios maps each space to its own dwelling's strategy with no conflict. Mixed NV/MVRE/UV TM59 was already covered by `PartOIterationSliceTests:224-262`. | P8 |
| 14 | SAM serialisation | **Already safe** (products, terminals, movements, internal conditions) | P9 |
| 15 | TAS export / import of NV + MVHR | **Safe (code read).** Per AHU plant zone, IZAMs and thermostat limits; per-aperture opening control; import by space name and ZoneGuid. One defect: `PartODiagnosticLog` takes the iteration from `scenarios[0]` (`PartODiagnosticLog.cs:167`). | SAM_Tas code |
| 16 | Weather | **Already safe** (one per simulation) | code |
| 17 | Cooling on selected dwellings | **Unsafe in the model.** It exists only as SAM_UI-resolved per-AHU settings for SAM_Systems. There is nothing on the model and no scenario identity (`ActiveTrimCooling` is refused everywhere). Per-AHU granularity in TPD is already there. | code |
| 18 | NV + cooling | **Must refuse.** The only cooling path is on the MVHR supply. | code |
| 19 | Cooled and non-cooled mechanical dwellings in one simulation | **Unresolved; gated.** Any cooling forces the TPD route on the *whole* building, because the no-IZAM source strips all IZAMs and ticV. Non-cooled MVHR dwellings are then simulated through TPD without their internal transfer-air network, a different representation from their IZAM screening. NV rooms are unbound and free-run in the bridge. Needs licensed TAS proof. | code |
| 20 | The open model as baseline | **Blocker.** Prepare, Simulate and 2B each `SetJSAMObject` the prepared model into the open model. After one run the open model is no longer a baseline (see #10). | SAM_UI code |
| 21 | An authored conditioned unit reused for an MVHR dwelling | **Unsafe: breaks the cooling gate.** When the dwelling's design terminals are already connected to an authored system, `AddPartOBaseMVHRSystem` reuses that system and unit, and **keeps its supply temperatures**. Only the create path sets them to NaN (`Modify/AddPartOBaseMVHRSystem.cs:291-292`). `AddAirMovementObjects` then emits a cooling profile from a finite `SummerSupplyTemperature` (`Modify/AddAirMovementObjects.cs:256-258`). The result is active supply-air cooling under a `BasePassive` scenario that asserts none. This also affects today's legacy 1a path over such a model. | P12 |

---

## C. Concrete blockers

| # | Where | Why it blocks mixed strategies | Repo |
|---|---|---|---|
| C1 | `Query.PartOVentilationMode`, `Modify.PreparePartOIteration` | One route and one iteration per call, by design (#1) | SAM |
| C2 | `Modify.ApplyPartFVentilationRates`, `Modify.RealizePartFVentilationTerminals(null)` from `PrepareBaseMVHR` | Whole model and blind to the route. Pollutes NV and unassessed dwellings, irreversibly (#4, #10). | SAM |
| C3 | No dematerialisation anywhere | A dwelling cannot leave MVHR (#5) | SAM |
| C4 | Cooling authority | Not on the model; not in `OverheatingScenario` identity; resolved in SAM_UI (#17) | SAM (+ SAM_UI move) |
| C5 | `PartORun.Guids_VentilationSystem_Prepared` | The design-under-assessment identity is session-only | SAM (persist), SAM_UI (consume) |
| C6 | Iteration 3 route | Framed as an A/B comparison; template global; no-IZAM building-wide (#19) | SAM_UI, SAM_Tas |
| C7 | `SetJSAMObject` after Prepare/Simulate/2B; 2B round chaining | No stable baseline survives (#20) | SAM_UI |
| C8 | `PartOWorkflowRequest.VentilationStrategies()`, `PartORun` | Fans one word out to every zone; one iteration per run | SAM_UI |
| C9 | `PartODiagnosticLog` | Single iteration per log (minor) | SAM_Tas |
| C10 | `PartOEquipmentSelection` mode | Per call, not per dwelling: an automatic scope overwrites a manual choice (#3) | SAM |
| C11 | `Modify.AddPartOBaseMVHRSystem` reuse path | Keeps an authored unit's supply temperatures, so a reused conditioned unit delivers cooling behind the gate (#21, P12) | SAM |

---

## D. Recommended architecture

```text
clean design baseline (persisted, never materialised into)
  + PartODwellingStrategy set (persisted intent, one per dwelling)
  + catalogue (argument, as today)
        |
        v   SAM  Modify.MaterialisePartODwellingStrategies   -- one call, pure, deterministic
materialised mixed model (run artefact only)  + materialisation record
        |
        v   SAM_Tas  existing IZAM route (no cooling)  |  TPD route (cooling, gated)
one annual TAS simulation, one weather
        |
        v   SAM  TM59 over per-zone scenarios (existing per-space criterion)
final TM59 authority  ->  edit intent of failing dwellings  ->  re-materialise from the SAME baseline
```

### D1. Stable baseline — resolved

**Decision: mixed materialisation starts from a clean pre-Part-O baseline. A materialised model is not
sanitised back to a baseline.** P11 is decisive: `ApplyPartFVentilationRates` zeroes authored per-person,
per-area and ACH bases and replaces shared internal conditions with renamed per-space clones under new guids.
It does this on every sized space in the model, and it keeps no record. No deterministic sanitiser can
restore what was never recorded, and the lost values are the ones `ticV` reads (#4).

**What the baseline is.** The design layer:
- geometry, constructions and openings;
- **authored** internal conditions;
- zones with `IsDwelling`;
- Part F requirements (`PartFSpaceData`);
- **design ventilation terminals**, where the designer (or an accepted 2B outcome, D3) has stated any.

**What the baseline is not.** The Part O materialisation layer:
- Part O MVHR systems (type guid `5a4b1f2c…`) and their units;
- `SpaceAirMovement` and `AirHandlingUnitAirMovement` objects;
- Part-F-applied internal-condition clones;
- overheating scenarios;
- `SimulationResultProvenance`;
- simulation results.

**Rules.**
- Materialisation never writes into the baseline. The materialised model exists only as a run artefact, the
  way Iteration 3's Candidate B already does.
- PR1 adds `Query.PartOMaterialisationState(model)`. It **refuses** any model that carries anything in the
  "is not" list above, both **materialisation** and **run outputs**. Its signals are:
  - *materialisation:* the Part O MVHR type guid, Part O air movements, and Part F rates on internal
    conditions of sized spaces;
  - *run outputs:* an `AnalyticalModelParameter.OverheatingScenarios` value, a `SimulationResultProvenance`,
    or **any object in the cluster, or on the model, deriving from `Core.Result` (`Core.IResult`)**. The
    check is over the base type, not a list. SAM's result types today are `SpaceSimulationResult`,
    `ZoneSimulationResult`, `SurfaceSimulationResult`, `OpeningSimulationResult`, `PartitionSimulationResult`,
    `AdjacencyClusterSimulationResult`, `AnalyticalModelSimulationResult`, `BuildingModelSimulationResult`,
    `TMResult` and its TM52/TM59 subtypes, and `UncloneableResult`, and a later subtype is covered
    automatically. A legacy or imported model that holds only an `AnalyticalModelSimulationResult`, with no
    scenarios or provenance, is refused too.
  - The second group matters on its own. A previously simulated **all-NV** model carries no MVHR object,
    air movement or Part F rate, yet it holds scenarios, provenance and results (and TAS-written design-day
    records). Accepting it would feed a run output back in as input, which is the `DesignDay` /
    `ZoneSimulationResult` accumulation class.
  - Whether the cluster's `DesignDay` records are authored design data or run output is a PR1 rule to pin
    by test. Until it is pinned, the result, provenance and scenario signals already refuse every simulated
    output model.
  - A false positive only refuses, which is the safe direction.
- Existing projects whose open model was already overwritten by a Part O run (1a/1b/2/2B) must reopen their
  pre-Part-O source. This is a migration consequence (G), not a sanitiser.
- **No lossy "adopt" or undo operation** (owner decision 1).

### D2. Materialisation

`Modify.MaterialisePartODwellingStrategies(baseline, strategies, descriptors)`, one call, pure:
1. **Refuse on:** a materialised baseline (D1); a strategy for a non-dwelling zone; a dwelling with no
   strategy while others have one; an authored shared or legacy mechanical system serving a mixed scope (P7);
   NV + cooling; cooling while gated (D5); a retained airflow set that does not resolve or does not balance;
   and, for an MVHR dwelling with `ActiveCooling = None`, a **reused authored unit that states a finite
   summer or winter supply temperature** (#21, P12). Materialisation never clears an authored setpoint;
   that would be a silent engineering change. It refuses and names the unit, so the designer either removes
   the conditioning from the baseline or waits for the cooling authority (PR3).
2. Apply Part F rates and realise terminals **only on the spaces of MVHR dwellings**. This needs scoped
   overloads of the two whole-model calls; the existing whole-model behaviour stays for legacy callers.
3. Run the existing per-dwelling loop for each MVHR dwelling: system, unit, product, movements, transfer air
   and balance. **Unit names are derived from the dwelling**, not from call order (P2).
4. **NV dwellings: materialisation writes nothing and strips nothing.**
   - No Part F internal-condition rates, no generated terminals, and no system, unit or movement.
   - **An NV strategy over authored mechanical design refuses.** If any space of the dwelling is served by
     an authored `VentilationSystem` carrying effective mechanical duty (a connected terminal with a positive
     design flow, as in Iteration 3's `IsEffectiveMechanicalDuty`), or by an air handling unit, NV refuses
     and names the system, even when that system is local to the dwelling. The authored design says
     "mechanical"; the strategy says NV; one of the two is wrong, and removing the system would delete
     authored design data (owner decision 1).
   - A space-related system with no duty (e.g. an `NV`/`UV` template assignment) is metadata. It passes
     through untouched, with a note.
   - A check asserts this, with `AssertNoContinuousMechanicalAirflow` semantics.
   - **Baseline design terminals already in an NV dwelling** (designer-authored; D1 allows them) are copied
     through **unconnected and inert**, and reported by name. NV materialisation never deletes design data.
   - An NV strategy whose `DesignAirFlowBasis` is `RetainedDesign` is a contradiction, so it **refuses**.
5. Scenarios per zone. Each dwelling zone gets its own iteration (`BaseNaturalVentilation` or `BasePassive`).
   Assessed common spaces get a `CommonSpace` scenario **automatically** (owner decision 2). That
   classification is derived from `IsDwelling` / internal-condition state (`PartOClassifyAssessmentZones`),
   never from names (#12).
   - **The identity of a common-space scenario is a PR1 design gate, not settled here.**
   - Today `Create.OverheatingScenarios` gives a common space scope `CommonSpace`, the strategy it is handed
     (`UV` selects the TM59 corridor criterion, `TMOverheatingCalculator.cs:359-361`), and **the call's own
     dwelling iteration**, with that iteration's operating assumptions inside the key.
   - That was harmless when every call had one iteration. In a mixed model it would assert, for example,
     `BasePassive`'s "mechanical ventilation at design rate" about a corridor. SAM itself says common spaces
     are covered by neither base iteration (`Query/PartOVentilationMode.cs:74-80`).
   - PR1 must therefore define an iteration-neutral common-space identity: scope `CommonSpace`, strategy
     `UV` or the corridor criterion, and assumptions that assert nothing about dwelling ventilation. That is a
     **new key**, never a reinterpretation of old ones (G).
   - **The corridor is recognised from the space's assigned internal condition, not through
     `TM59InternalConditionResolver`.** That resolver's `ClassifyCore` classifies from `space.Name`, and its
     `Resolve` uses the result to pick a library condition. It never inspects the condition the space
     actually carries. Using it would drop a correctly assigned communal-corridor condition on an unusually
     named space, and accept a corridor-like name that has no authoritative condition.
     - PR1 either adds a query that reads the **assigned** `InternalCondition` and matches it exactly against
       `TM59InternalConditionResolver.CommunalCorridorInternalConditionName`, or makes that exact-condition
       check inline.
     - Pin it with a regression test on a **non-corridor-named** space that carries the corridor condition,
       and a corridor-named space that does not.
     - The dwelling / common split itself stays `IsDwelling` (`PartOClassifyAssessmentZones`).
6. Stamp a **materialisation record** containing:
   - the baseline fingerprint (reuse `SimulationResultProvenance.Fingerprint` over the baseline);
   - the strategy-set fingerprint;
   - a **catalogue fingerprint over every selection-relevant descriptor field**: reference (manufacturer,
     model, reference), maximum supply, maximum extract and rank, in a stable order. Identities alone would
     miss a capacity or rank correction that changes which unit is selected;
   - the prepared-system guids, which closes C5.

It never takes a previous output as input: `PartOMaterialisationState` refuses a result-, provenance- or
scenario-bearing model (D1). So the DesignDay / ZoneSimulationResult accumulation class cannot arise from it. P10 shows the engineering state is reproducible. Guids are not, and do not need to be, because
identity for staleness is the record plus the model fingerprint.

### D3. Retained 2B design airflow — the least duplicative persistence

**`PartODwellingStrategy` must not persist per-space or per-direction airflow values.** A persisted copy
would be a second authoritative store for `VentilationTerminal.DesignFlowRate_Lps`. The two would disagree
the first time either is edited, just as the internal-condition copy already disagrees with the terminal
after 2B (P5).

- **One store for design airflow: the baseline's design terminals.** The terminal is already the design
  object. `RealizePartFVentilationTerminals` re-links it and never resizes it (P5), so a terminal on the
  baseline survives every materialisation unchanged.
- **Accepting a 2B outcome is an explicit design edit.** The designer accepts it for a dwelling. Nothing is
  accepted automatically, and acceptance is whole-dwelling (P5 shows a partial set is refused). Two steps,
  both on the **baseline**:
  1. **Ensure the dwelling's design terminals exist on the baseline.** A normal clean baseline has Part F
     requirements but no terminals; those exist only on the run copy, and `SetSpaceDesignFlowRate` refuses a
     space with none. Acceptance first calls `RealizePartFVentilationTerminals` scoped to **that dwelling's
     spaces**. It creates one terminal per continuous requirement and writes only terminals, never internal
     conditions or systems, so it does not bring back the lossy rewrite of D1.
  2. **Write the balanced set.** It is matched to the run copy's terminals by requirement lineage
     (`PartFTerminalReference`: space, role, paragraph), never by guid, because the run copy's terminals are
     regenerated. `SetSpaceDesignFlowRate` then writes the set (same-guid replacement). An unmatched or
     ambiguous terminal **refuses**, following the existing lineage rules.
  - The baseline stays acceptable afterwards: D1 allows design terminals, and unconnected terminals are
    inert.
- **The strategy records only intent and a guard:** `DesignAirFlowBasis` ∈ {`PartFRequirement`,
  `RetainedDesign`}, plus a fingerprint of that dwelling's terminal set at the moment it was accepted.
  - `PartFRequirement` with terminals that differ from the requirement **refuses**; it does not reset them
    silently.
  - `RetainedDesign` whose terminals no longer match the fingerprint is **stale** and refuses.
- **Distinguish, never merge:**
  - *selected design intent / retained outcome* = the strategy's `DesignAirFlowBasis` and fingerprint;
  - *materialised terminal airflow* = the terminal values the materialised model carries, copied from the
    baseline;
  - *runtime movements* = derived from those terminals on every materialisation.

### D4. Iteration

Build → annual TAS → TM59 → edit the intent (or accept a 2B design for) the failing dwellings only →
re-materialise **from the same baseline** → rerun. The unchanged dwellings rebuild to the same engineering
state (P2, P10). **Mutating the previous mixed model is rejected**, because of #5, #10 and the accumulation
history.

### D5. Simulation route and cooling (gated)

- **No dwelling cooled: the existing IZAM route** (today's 1a/1b authority, #15). This is PR1's scope.
- **Any dwelling cooled: the TPD route over the whole building.** Every mechanical dwelling needs a selected
  product (MVRE template); cooling settings go per AHU; NV rooms are unbound.
- **The cooled mixed case stays unresolved until licensed TAS proof shows:**
  (a) the NV-room result in the bridge TSD matches the IZAM-route NV result, within an agreed tolerance;
  (b) a non-cooled MVHR dwelling on the TPD route is acceptable as final authority despite losing its
  transfer-air network (the Iteration 3 route check quantifies this);
  (c) a scenario identity for active cooling exists (C4).
- Until then, PR1 **records** `ActiveCooling` on the strategy and **refuses** to materialise it.

### D6. Suggestion (later; policy in SAM)

`Query.PartOSuggestedDwellingStrategy(screening evidence, project constraints)` gives the least intervention
that passes:

```text
Natural  <  MVHR (smallest compliant product)  <  MVHR with a retained raised design airflow  <  MVHR + active cooling
```

- **Project constraints filter it:** allowed modes, the product pool, and "all MVHR" project policies. The
  filter narrows the ordering; it never reorders it.
- **It is not "lowest iteration"**, because 1a and 1b are alternatives.
- **Suggested is derived and never authority.** Only Selected is persisted. The designer may select a
  strategy that is not the suggestion, for example forcing MVHR on a flat that passes naturally, and the
  selected strategy governs.

---

## E. Authority model — recommendation

**An explicit, persisted `PartODwellingStrategy` in SAM is needed.** Existing analytical state alone is not
sufficient:
1. The absence of a system cannot tell "NV chosen" from "not prepared yet".
2. Cooling has no model representation at all.
3. The design-under-assessment identity is session-only.
4. Deriving intent from a materialised model would mean reading a model that P4 and P11 show can misstate the
   building.

Conceptually, persisted as a SAM collection parameter on `AnalyticalModelParameter` beside
`PartOEquipmentSelection`:

```text
PartODwellingStrategy          -- intent, on the BASELINE
  ZoneGuid
  VentilationMode              PartOVentilationMode {NaturalVentilation, MVHR}      (existing enum)
  VentilationUnitReference?    null = select from the project pool (per-dwelling "auto")
  ActiveCooling                {None, SupplyAirCooling}                             (gated, D5)
  DesignAirFlowBasis           {PartFRequirement, RetainedDesign} + terminal-set fingerprint (D3)
```

- **Orthogonal properties, no enum explosion.** "Optimised MVHR" is MVHR + `RetainedDesign`. "MVHR + cooling"
  is MVHR + `SupplyAirCooling`.
- **It is intent only.** It never holds flows, capacities or operating airflows. Those stay on the terminals,
  the catalogue and the operating strategy, so the four-concept invariant is unchanged.
- **A per-dwelling product (or null) replaces the per-call selection mode** for the mixed workflow, which
  closes C10.
- **Materialised state** stays in the analytical objects of the run artefact.
- **Provenance** is the materialisation record (D2.6), plus the existing `SimulationResultProvenance`.
- **Staleness** fails closed at three levels:
  - baseline fingerprint ≠ record → re-materialise;
  - strategy set or catalogue fingerprint (every selection-relevant descriptor field) ≠ record →
    re-materialise;
  - model fingerprint ≠ provenance → re-simulate (existing).

---

## F. PR sequence and acceptance gates

**PR1 — SAM: authority and composable materialisation (NV / MVHR / product / retained design).**
- **Scope:**
  - `PartODwellingStrategy` plus its persistence (JSON round trip, schema identity);
  - a per-zone route query;
  - scoped overloads of `ApplyPartFVentilationRates` and `RealizePartFVentilationTerminals`;
  - `Query.PartOMaterialisationState`;
  - `Modify.MaterialisePartODwellingStrategies`;
  - dwelling-derived unit names;
  - per-zone scenarios, including automatic `CommonSpace` scenarios for assessed common spaces
    (classified from state, not names), with the iteration-neutral common-space identity of D2.5;
  - the materialisation record, including the full catalogue fingerprint;
  - refusals: cooling (recorded, refused), NV + cooling, NV + `RetainedDesign`, NV over authored mechanical duty, a reused conditioned unit under `ActiveCooling = None`, shared systems, a
    materialised or result-bearing baseline, a stale or unbalanced retained design.
- **Tests:** the PR0 proof matrix promoted and inverted to the target behaviour:
  - P1: an NV dwelling stays clean;
  - P4: MVHR→NV on the same baseline gives no mechanical state;
  - P2/P10: order-independent and reproducible, with names now equal;
  - P3: a manual product is kept under an automatic neighbour;
  - P5: the retained design is kept, or refused when stale; accepting 2B on a terminal-less baseline realises that dwelling's terminals first;
  - P11: a materialised input is refused, and so is a simulated all-NV model (scenarios, provenance or results present);
  - P12: a reused conditioned unit under `ActiveCooling = None` is refused, and a materialised movement never carries cooling.
  - Legacy `PreparePartOIteration` tests unchanged.
- **Dependencies:** none.
- **Acceptance:**
  - all green;
  - legacy callers bit-identical (the existing Part O/Part F tests);
  - an NV dwelling has no Part F internal-condition rates, no materialisation-generated or connected
    terminals, and no system, unit or movement after materialisation;
  - its baseline design terminals, if any, are unchanged, unconnected and reported (D2.4);
  - the common-space scenario identity (D2.5) is defined, tested and keyed iteration-neutrally;
  - a changed descriptor capacity or rank makes the materialisation record stale.

**PR2 — SAM_UI: dwelling-strategy assignment and the mixed-model workflow (NV / MVHR / product / retained
design; the IZAM route).**
- **Scope:**
  - **The strategy grid:** one row per dwelling, virtualised (`VirtualizingPanel.IsVirtualizingWhenGrouping`),
    with grouping, search, filters, multi-select bulk assignment and summaries.
  - **Screening columns** from existing homogeneous runs, read-only and labelled as screening.
  - **Suggested vs Selected** shown separately.
  - **The run:** materialise on a copy; the open model stays baseline + intent (fixes C7, C8); one simulation;
    TM59; edit the failing rows; re-run.
  - **Persistence:** sidecar schema `PartORunResume:v3`, carrying the materialisation record for mixed runs.
  - **Accept 2B for a dwelling** as the explicit design edit (D3).
- **Tests:** WPF tests at 500+ dwelling rows (scaling pattern of `PartOEquipmentAssignmentScalingTests`);
  stale refusal; no `SetJSAMObject` of a materialised model.
- **Dependencies:** PR1 merged.
- **Acceptance:** an owner walk-through on a real project; existing Part O runs still reopen.

**PR3 — SAM + SAM_Tas (+ SAM_Systems if needed): cooling authority (gated).**
- **Scope:**
  - scenario identity for active cooling (an `OverheatingScenario` assumptions change — this is an identity
    migration, see G);
  - per-AHU cooling resolution moved into SAM;
  - a standalone TPD route over a mixed model (TPD for cooled AHUs; the representation of non-cooled
    mechanical dwellings decided by the proof);
  - `PartODiagnosticLog` per-scenario iteration (C9).
- **Tests:** unit tests plus **licensed TAS proof** of D5 (a)–(c).
- **Dependencies:** PR1; its UI surface lands in PR2 as a follow-up toggle.
- **Acceptance:** the D5 proof documented in `PartO-TAS-VALIDATION.md` and owner sign-off. Until then,
  cooling stays refused.

**PR4 — real TAS acceptance and deploy.**
- **Scope:** a full-year mixed run on a large project (hundreds of dwellings, ~5,000 spaces); SAM_Deploy pins.
- **Acceptance:**
  - the TM59 per-dwelling outcome matches the selected strategies;
  - no `.sam` growth across re-materialisations;
  - materialisation-time budget met at scale.

*Change from the draft order:* the UI is PR2 and cooling is PR3. Cooling is gated on licensed proof, so it
should not block the NV/MVHR workflow that PR1 already makes safe. The draft put SAM_Tas orchestration second;
the evidence shows SAM_Tas needs **no change** for NV/MVHR mixing (#15).

---

## G. Migration and compatibility

- **Existing saved Part O runs, sidecars (`PartORunResume:v2`) and reopened results:** unchanged. The legacy
  `PreparePartOIteration` path stays.
- **Scenario keys:** unchanged. A mixed model's NV zone keys as `BaseNaturalVentilation` and its MVHR zones
  as `BasePassive`, exactly as in homogeneous runs.
- **Keys are not proof of the same result.** A dwelling's key does not encode its neighbours' strategies, so
  a key match between a screening run and the final run is never evidence that the result is the same.
  Provenance plus the materialisation record decide.
- **No record, no mixed workflow.** A model with no `PartODwellingStrategy` collection is a legacy model:
  - its first mixed run needs a clean baseline;
  - an open model already overwritten by a 1a/2/2B run is refused by `PartOMaterialisationState`, and the
    user reopens the pre-Part-O source. **This is the one user-visible compatibility cost.**
- **Schema:**
  - new collection parameter, versioned;
  - sidecar v3 for mixed runs only;
  - the cooling scenario identity in PR3 is a migration: a new assumption means new keys, and old keys are
    never reinterpreted.

---

## H. Risks and open questions

**Resolved by PR0:**
1. The baseline must be clean; sanitising is rejected (P11, D1).
2. Retained 2B airflow lives only on the baseline terminals; the strategy holds basis + fingerprint (D3).
3. Deterministic reconstruction reproduces the engineering state (P10).
4. Mutating the previous mixed model is rejected (D4).
5. SAM_Tas needs no change for NV/MVHR mixing.
6. Shared systems refuse (P7).
7. Weather is single by construction.

**Genuine blockers (must be closed in the PR stated):**
1. C2/C3, NV pollution and no dematerialisation → PR1.
2. C7, the open model is overwritten → PR2.
3. Cooled mixed operation (D5 a–c) → PR3 licensed proof. **Unresolved until then.**
4. Legacy projects whose open model is already materialised need their pre-Part-O source. **Accepted by the
   owner** (decision 1); no adopt tool.
5. The identity of the automatic common-space scenario (iteration-neutral, D2.5) → PR1 design gate. It must
   be defined before any mixed model is keyed.

**Owner decisions taken 2026-09-26** (see the top of this document): common spaces are included
automatically; project-wide constraints are project settings.

**Optional improvements (not blockers):**
1. `PartODiagnosticLog` single-iteration field (C9).
2. SAM_Tas `UpdateInternalCondition.cs:326,331` writes the setback/factor only when the value `IsNaN`. That
   looks inverted, and it is unrelated to this programme.
3. The duplicate internal-condition copy of the Part F rate (P5) could eventually be derived rather than
   stored.

**Scale.**
- Materialisation is linear per dwelling: the loop already exists, and the fingerprint is streamed for
  ~5,000 spaces (`SimulationResultProvenance.Fingerprint`).
- A PR4 budget should be measured, not assumed. The per-dwelling `PartFTransferAirSpaces` and balance work
  is the part to watch.
- The UI must be dwelling-rows-only. Per-space data is shown only on demand.

---

## Evidence index — `SAM.Tests/PartOMixedStrategyProofTests.cs`

The fixture is Flats 1–2 from `PartOIterationPreparationTests.ModelWithTwoAssessedDwellings` (by reflection,
so the two cannot drift), plus an authored Flat 3 and a communal corridor zone (`IsDwelling=false`) adjacent
to Flats 1 and 2. Every test compares a GUID-insensitive signature labelled by dwelling. The signature
covers systems and their served spaces; units and their products; internal-condition supply and extract;
terminals with flow and connection; every `SpaceAirMovement` with its endpoints, flow and profile; and every
`AirHandlingUnitAirMovement` with its unit relation and its heating, cooling, humidification,
dehumidification and density profiles (name, type, length and value sum). Unit names inside profile names are
normalised to the dwelling they serve.

| Test | Fact pinned |
|---|---|
| P0 | A mixed-route call is refused with no model |
| P1 | MVHR then NV: A intact; B and the unassessed C carry Part F rates and unconnected terminals |
| P2 | Reordering keeps the engineering state, profiles included; unit names, and the profile names built from them, swap with call order |
| P3 | Products stay per dwelling; an automatic scope over a manual dwelling re-selects it |
| P4 | Re-stating MVHR as NV keeps the whole mechanical design under an NV scenario |
| P5 | A retained airflow survives; a single raise is refused as unbalanced; the internal condition disagrees with the terminal |
| P6 | Re-preparing is idempotent |
| P7 | An authored shared system is warned about and left alone; isolation refuses it |
| P8 | Per-call scenarios compose per space; the corridor has no strategy |
| P9 | The composed model round-trips through JSON |
| P10 | Two independent baseline clones give equal engineering state and different guids |
| P11 | MVHR preparation irreversibly rewrites an unassessed dwelling's internal condition |
| P12 | A reused authored unit keeps its 18 °C summer supply, and the rebuilt movement carries a cooling profile |

Run: `dotnet test SAM/SAM.Tests/SAM.Tests.csproj --filter Category=PR0Investigation` → 13/13 passed.
Existing suite: `--filter "FullyQualifiedName~PartO|FullyQualifiedName~PartF"` → 1238/1238 passed.

**The proof tests are disposable.** They pin *today's* behaviour. PR1 should delete or invert them, not keep
them as regression pins.
