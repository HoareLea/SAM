<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O — mixed dwelling strategies: PR1 (SAM authority + NV/MVHR materialisation)

**Status: implemented, following the approved PR0 specification**
([`PartO-MixedDwellingStrategies-PR0.md`](PartO-MixedDwellingStrategies-PR0.md), §D–§F and the owner decisions at
its top). The PR0 report is unchanged; where implementation settled one of its open gates or refined a rule, that is
listed in §8 below, not written back into PR0.

Scope: SAM only. No SAM_UI grid (PR2), no active-cooling TPD route (PR3), no licensed TAS run, no SAM_Deploy bump
(PR4). The legacy `PreparePartOIteration` path (1a/1b/2/2B) is unchanged.

```text
clean pre-Part-O baseline  +  persisted PartODwellingStrategySet  (+ catalogue)
        |
        v   Modify.MaterialisePartODwellingStrategies   -- one call, deterministic, refuses rather than repairs
mixed analytical model  +  per-zone OverheatingScenarios  +  PartOMaterialisationRecord
```

---

## 1. Public types and APIs

| Kind | Name | Purpose |
|---|---|---|
| class | `PartODwellingStrategy` (`IJSAMObject`, no guid) | Selected intent for one dwelling: `ZoneGuid`, `VentilationMode` (existing `PartOVentilationMode`), `VentilationUnitReference` (null = select from project pool), `ActiveCooling`, `DesignAirFlowBasis`, `DesignFingerprint`. `IsValid`, `CanonicalText()` |
| class | `PartODwellingStrategySet` | One strategy per dwelling, schema `PartODwellingStrategies:v1`, canonical (zone-guid order, no instance guids). `IsValid`, `Conflicts`, `Set/Remove/Strategy` |
| enum | `PartOActiveCooling` | `Undefined`, `None`, `SupplyAirCooling` (recorded, refused in PR1) |
| enum | `PartODesignAirFlowBasis` | `Undefined`, `PartFRequirement`, `RetainedDesign` |
| enum member | `PartOIteration.DwellingIndependent` | Iteration-neutral identity of an assessed common space (appended) |
| parameter | `AnalyticalModelParameter.PartODwellingStrategies` | The persisted strategy set, on the baseline |
| parameter | `AnalyticalModelParameter.PartOMaterialisationRecord` | The record, on the materialised model only |
| class | `PartOMaterialisation` | Result: `AnalyticalModel` (null on any refusal), `Refusals`, `OverheatingScenarios`, `Record`, systems, units, selections, notes, warnings |
| class | `PartOMaterialisationRefusal` + enum `PartOMaterialisationRefusalReason` | Structured refusal: reason code, zone guid, subject, message |
| class | `PartOMaterialisationRecord` | Baseline / strategy / catalogue fingerprints, assessed and common-space zones, zone → system guid map; `IsCurrent(baseline, catalogue, out reason)` |
| Modify | `MaterialisePartODwellingStrategies(baseline, descriptors = null, guids_Zone_Assessed = null)` | The one materialisation |
| Modify | `ApplyPartFVentilationRates(model, mode, IEnumerable<Space> spaces, …)` | Scoped overload; the old signature delegates with `null` (bit-identical) |
| Query | `PartOBaselineFindings(model)`, `IsPartOCleanBaseline(model, out findings)` | Clean-baseline validation (D1) |
| Query | `IsTM59CommunalCorridor(space)` | Exact match of the **assigned** internal condition against `TM59InternalConditionResolver.CommunalCorridorInternalConditionName` |
| Query | `PartODwellingDesignFingerprint(cluster, zone)`, `PartOCatalogueFingerprint(descriptors, projectTest)`, `PartOStrategyFingerprint(strategies, zones)` | Deterministic SHA-256 fingerprints |
| Create | `PartOCommonSpaceOverheatingScenario(zone)`, const `PartOCommonSpaceVentilationStrategy = "UV"` | The neutral corridor scenario |

Internal seams (no public change): `Modify.RealizeBaseMVHRDwelling` is `PrepareBaseMVHR`'s per-dwelling loop body lifted
out unchanged, with two caller-stated options that default to the legacy behaviour (created system id / unit name, and a
unit gate); `AddPartOBaseMVHRSystem` has an internal overload taking the id and name.

## 2. Clean-baseline validation (D1)

`Query.PartOBaselineFindings` lists every reason a model is not a clean baseline; empty means clean. Nothing is cleaned,
undone or adopted.

- **Materialisation** (`MaterialisedBaseline`): a system of the Part O MVHR type guid; a sized space whose internal
  condition is the `ApplyPartFVentilationRates` clone (`<condition> - <space>` name **and** a supply/extract airflow); a
  `PartOMaterialisationRecord`; a `PartOIsolationContext`.
- **Unresolvable air movements** (`UnresolvedAirMovement`): a `SpaceAirMovement` with no source, or a stated source or
  destination the model does not contain. SAM_Tas `Modify.UpdateIZAMs` resolves endpoints by `ObjectReference`; it drops a
  movement whose source is missing and sends the air of one whose destination is missing **to outside**, so the model
  would simulate an airflow it does not state. Air movements as such are **not** refused here — see §2a.
- **Run output** (`RunOutputBaseline`): `OverheatingScenarios`; `SimulationResultProvenance`; any model-level parameter
  value that is an `IResult`; any cluster object whose stored type is assignable to `IResult` (base type, so a later
  result class is covered); `DesignDay` records **in the cluster**.
- **DesignDay rule (PR0 design gate, pinned):** the model-level `HeatingDesignDays` / `CoolingDesignDays` are design
  inputs (weather-derived or an engineer's override) and are accepted. `DesignDay` objects in the adjacency cluster are
  written there by the TAS workflow after a run (`SAM_Tas Modify.ReplaceDesignDays`, from `WorkflowCalculator`), so they
  are run output and refuse.
- Design ventilation terminals, authored internal conditions, Part F requirements, project settings and the strategy set
  itself are baseline data.

## 2a. Air movements — the engineering rule (PR1 review)

**Evidence that authored baselines legitimately carry air movements.** SAM creates movements in three places:
`Modify.AddAirMovementObjects` (from any ventilation system that names a unit; also behind the Grasshopper
`SAMAnalytical.CreateIZAM`), `Modify.AddPartFTransferAirMovements` (Part O transfer air), and the unit exhaust. On top of
that, engineers author them directly: `SAMAnalytical.CreateIZAMBySpaces` builds unit→space and **space→space** inter-zone
air movements, and `SAMAnalytical.CreateIZAMBySetPoint` builds a unit's plant-zone `AirHandlingUnitAirMovement` with
heating, cooling and humidity setpoints. SAM_Tas and SAM_UI create none. A homogeneous Iteration 1b preparation is a plain
copy, so it carries such movements to TAS unchanged. A blanket refusal would therefore reject real design data. It is
replaced by a rule based on where each movement's air goes, decided against the selected strategies
(`AuthoredAirMovements`):

| The movement… | Outcome | Why |
|---|---|---|
| reaches an **MVHR** dwelling (an endpoint or related space, or the unit an effective system serving it names) | refused, `AuthoredAirMovementConflict` | `RealizeBaseMVHRDwelling` → `RemoveBaseMVHRAirMovementObjects` deletes every movement related to the dwelling's rooms and unit, then rebuilds and balances the network from the design terminals. Keeping the authored movement is impossible without deleting authored data or ventilating the dwelling twice. |
| exchanges air between a **Natural** dwelling and a unit or outside | refused, `NaturalOverMechanicalDuty` | prescribed continuous mechanical supply or extract, which the NV scenario key states the dwelling does not have |
| transfers air **space→space** into or out of a Natural dwelling (no MVHR dwelling touched) | carried through unchanged, with a warning | it states no mechanical route; Iteration 1b carries it identically |
| touches no assessed dwelling | carried through unchanged | outside the strategies' authority |
| is a unit's `AirHandlingUnitAirMovement` whose unit serves an MVHR dwelling | refused, `AuthoredAirMovementConflict` | the rebuild regenerates the unit's plant-zone conditions and would delete the authored ones (for example authored cooling setpoints) |

An invariant then proves that every baseline movement survived unchanged (flow and endpoints), and the Natural-dwelling
cleanliness check ignores those baseline movements and looks only for generated ones. Part-O-generated movements never
reach this rule: they come with the Part O MVHR system, which the baseline check refuses.

## 3. Persistence

- `PartODwellingStrategySet` is stored as `AnalyticalModelParameter.PartODwellingStrategies`, JSON `Schema =
  "PartODwellingStrategies:v1"`. Strategies are written in zone-guid order with the product as its identity fields only
  (no guid, no type tag), so logically identical sets write identical bytes and the baseline model fingerprint does not
  move when a UI rebuilds the set.
- **Absent = legacy.** A model without the parameter is refused by the materialiser with `NoStrategies` and is otherwise
  untouched; the legacy path never reads it. Nothing is migrated or inferred from systems, scenarios, report text,
  filenames or scenario labels.
- **Fail closed on read:** an unknown schema, a zone stated twice (kept and written back as a conflict, never resolved to
  either), an unknown enum name, or a stated-but-empty product reference make the set or strategy invalid and refused.
  "No strategy" (no entry), "Natural" and "MVHR" are distinct; an entry with `VentilationMode = Undefined` is invalid.
- No airflow value is persisted in a strategy (D4). `RetainedDesign` holds only `DesignFingerprint`.

## 4. Materialisation

One call, pure (the baseline is not modified), returning a model only when **nothing** refused.

1. **Baseline** findings (§2).
2. **Strategies:** set present and valid; every strategy's zone exists and is a dwelling (`Query.PartFDwellingZones`);
   every assessed dwelling has a valid strategy. Scope: `guids_Zone_Assessed = null` means every dwelling; a dwelling
   outside a stated scope is *unassessed* — untouched, no scenario.
3. **Contradictions / gates:** `NaturalWithCooling`, `CoolingGated` (any `SupplyAirCooling`), `NaturalWithRetainedDesign`,
   `NaturalWithVentilationUnit`; `OverlappingZones`.
4. **Common spaces** (always by the **assigned** internal condition, never names):
   - a marked non-dwelling zone with **no** space assigned the TM59 communal corridor condition → no common-space TM59
     scenario is needed (noted);
   - a zone **all** of whose spaces are assigned it → automatic corridor assessment;
   - a zone **mixing** corridor and other spaces → refused, `CommonSpaceUnclassifiable`, naming both sides. An
     `OverheatingScenario` is zone-scoped (`ZoneGuid`; `OverheatingScenarioMap` applies it to every space of the zone) and
     nothing states a criterion per space. So the zone can be neither assessed (a non-corridor room would fall under the
     corridor criterion) nor dropped (an assessed corridor would disappear);
   - a corridor-assigned space in a **dwelling zone**, or in **no** dwelling or common zone → refused,
     `CommonSpaceUnclassifiable`. An assessed corridor is never silently omitted.
5. **Authored plant** (baseline): a system is *effective* if it has a connected terminal with positive design flow or
   names a unit the model contains; otherwise it is template metadata (noted, untouched). Effective systems / units that
   touch an assessed dwelling and span more than one zone (or unzoned spaces) refuse `SharedSystem`; over a Natural
   dwelling refuse `NaturalOverMechanicalDuty`; in an MVHR dwelling but connected to none of its terminals refuse
   `UnconnectedAuthoredPlant`. A **unit** is shared when the systems naming it - every system that serves spaces, assessed or not - together reach an assessed dwelling and any other zone or unzoned rooms; it refuses `SharedSystem` (review finding: an assessed dwelling sharing a unit with an unassessed neighbour or a corridor). Shared plant is never split or mutated.
6. **Part F, scoped:** `ApplyPartFVentilationRates(ContinuousDesign, spaces of MVHR dwellings)` and
   `RealizePartFVentilationTerminals(spaces of MVHR dwellings)`. Natural and unassessed dwellings receive nothing (P1/P11
   fixed at the seam).
7. **Design basis per MVHR dwelling:** `PartFRequirement` — every terminal must realise a requirement and each continuous
   requirement's terminal sum must equal it, else `DesignDiffersFromRequirement` (never reset silently). `RetainedDesign`
   — the baseline must carry terminals, and the dwelling's fingerprint after realisation must equal the strategy's, else
   `RetainedDesignStale`. The retained airflow is read from `VentilationTerminal.DesignFlowRate_Lps` only.
8. **MVHR design per dwelling**, in (zone name, zone guid) order: `RealizeBaseMVHRDwelling` (system, unit, ticV gate,
   duty, movements, transfer air, balance — Iteration 1a's code). The created system id is the dwelling label and the unit
   `MVHR <label>`, unique against every space and unit name, disambiguated `" (n)"` in canonical order. **P12 gate:** a
   reused unit stating a finite summer or winter supply temperature refuses `ConditionedReusedUnit` (never cleared).
9. **Product:** explicit reference → must be in the offered catalogue (`VentilationUnitUnresolved`), permitted by the
   project's `PartOEquipmentSelection` (`VentilationUnitNotAllowed`) and able to serve the duty (selection over that one
   product; `VentilationUnitSelection`). **Owner decision (PR1 review): an undersized manual product is a structured
   refusal, never a valid materialisation.** Pinned by `ManuallySelectedUndersizedProduct_…`. Null reference with a catalogue → the project's automatic candidate set
   (`CandidateDescriptors`, project test product included as in Iteration 2); manual project mode → generic unit + warning.
   No catalogue → generic units (Iteration 1a). Each dwelling selects against its own duty only.
10. **Natural invariant:** after materialisation each Natural dwelling must still have its baseline internal conditions
    (same guid), the same terminals, none newly connected, no Part O system and no air movement reaching it — else
    `Invariant`.
11. **Scenarios:** Natural → `BaseNaturalVentilation` / `NV`; MVHR → `BasePassive` / `MVHR` (the same keys homogeneous
    runs use, via `Create.OverheatingScenarios`); assessed corridor → `CommonSpace` / `DwellingIndependent` / `UV` / no
    assumptions. SAM's per-space TM59 criterion is unchanged.
12. **Record** stamped on the model (§6). The model also keeps the strategy set it was built from.

**Final-materialisation invariant: the whole clean building, never isolated (owner decision, PR1 review).** The mixed
model is the building-level authority for one annual run, so it holds every space of the baseline. It stamps no
`PartOIsolationContext`, and an already isolated model is refused as a baseline. This is intentional, not a missing
feature. Isolation derives a smaller thermal model from a prepared one and so cannot be the authority it was derived
from. If a later workflow needs an isolated run, it isolates the **materialised union once**, downstream of this call.
Because materialisation always rebuilds from the baseline, a dwelling's strategy is edited and the model rebuilt; the
previous mixed model is never mutated (PR0 D4).

## 5. Determinism

Canonical processing order and dwelling-derived names make the engineering state a function of (baseline, strategies,
catalogue). Pinned: two independent JSON clones give equal GUID-insensitive signatures **and** equal system/unit names;
reversed strategy and scope order give the same result; a dwelling materialised alone gets the same design and names as
when materialised with others. Generated objects take fresh guids, so `SimulationResultProvenance.Fingerprint` of two
rebuilds differs: a rebuilt model always needs a fresh TAS simulation.

## 6. Fingerprints and staleness

`PartOMaterialisationRecord`: `Fingerprint_Baseline` = `SimulationResultProvenance.Fingerprint(baseline)` (the existing
model digest); `Fingerprint_Strategies` over the assessed strategies' canonical text plus the assessed zone set;
`Fingerprint_Catalogue` over every selection-relevant descriptor field — manufacturer, model, reference, maximum supply,
maximum extract, rank, validity — plus the project test product, order-insensitive, `null` distinct from empty.
`IsCurrent(baseline, catalogue, out reason)` fails closed and names the half that moved. Levels:
baseline/strategies/catalogue ≠ record → re-materialise; model ≠ provenance → re-simulate (existing).

## 7. Tests

`SAM/SAM.Tests/PartODwellingStrategyMaterialisationTests.cs` (50 tests) — the PR0 proof matrix promoted and inverted. The
disposable PR0 proof tests (`PartOMixedStrategyProofTests.cs`, P0–P12) are removed; they remain at `444d2db3`.

| PR0 fact | PR1 pin |
|---|---|
| P1 / P11 NV and unassessed dwellings polluted, authored basis zeroed | `MvhrDwelling_LeavesNaturalAndUnassessedDwellingsExactlyAsTheBaselineHasThem`, `ScopedPartFRates_…` |
| P2 / P10 names follow call order; guids differ | `SameBaselineAndStrategies_OnIndependentClones_…`, `StrategyAndScopeOrder_DoNotChangeTheResult` |
| P3 automatic call re-selects a manual product | `DifferentProducts_StayPerDwelling_…` |
| P4 MVHR→NV keeps the design | `NaturalDwelling_OnTheSameBaseline_CarriesNoMechanicalState` |
| P5 retained design / unbalanced raise | `RetainedBalancedDesign_…`, `RetainedDesign_IsNotChangedByAnotherDwellingsStrategy`, `RetainedDesign_WhoseTerminalsMoved_IsStale_…`, `UnbalancedBaselineDesign_IsRefused_NotRescaled` |
| P7 shared system | `AuthoredSystemWithDuty_StraddlingTwoDwellings_IsRefused_…`, `AuthoredUnit_SharedWithASystemOutsideTheAssessedDwellings_IsRefused`, `AuthoredSystemWithoutDuty_IsTemplateMetadata_…` |
| P8 corridor has no strategy | `CommunalCorridor_IsIncludedAutomatically_WithAnIterationNeutralScenario`, `CorridorClassification_ReadsTheAssignedInternalCondition_NeverTheName`, `CommonSpaceZone_Mixing…`, `DwellingIndependent_IsACommonSpaceIdentityOnly` |
| P9 serialisation | `StrategySet_RoundTripsThroughTheModelJson_Canonically`, `StrategySet_OfAnUnknownSchema_…` |
| P11 / result-bearing baseline | `LegacyPreparedModel_IsRefused_AsMaterialised`, `MaterialisedOutput_IsNotABaseline`, `ModelWith{Scenarios,Provenance,SimulationResults}_…`, `DesignDay_…` |
| P12 conditioned reused unit | `ReusedConditionedUnit_IsRefused_RatherThanLeakingCooling` |
| air movements (§2a, review) | `AuthoredTransferMovement_IntoANaturalDwelling_IsCarriedThroughUnchanged`, `AuthoredMovement_IntoAnMvhrDwelling_IsRefused_…`, `AuthoredExtractToOutside_FromANaturalDwelling_…`, `AuthoredMovement_OnlyInAnUnassessedDwelling_…`, `AuthoredUnitPlantZoneMovement_…`, `AirMovement_WithAnEndpointTheModelDoesNotContain_…` |
| mixed / orphan corridors (§4.4, review) | `CommonSpaceZone_MixingCorridorAndOtherSpaces_IsRefusedAsAmbiguous_ByAssignedConditionNotName`, `CorridorAssignedSpace_OutsideAWholeCorridorCommonZone_IsNeverSilentlyOmitted` |
| owner decisions (review) | `ManuallySelectedUndersizedProduct_IsAStructuredRefusal_NeverAMaterialisation`, `Materialisation_NeverIsolates_AndAnIsolatedModelIsNotABaseline` |
| D5 cooling | `ActiveCooling_IsRecordedButRefused`, `Natural_WithRetainedDesign_WithCooling_OrWithAProduct_IsRefused` |
| catalogue | `CatalogueFingerprint_CoversEverySelectionRelevantField_AndNotOrder`, `MaterialisationRecord_IsCurrent_…` |

Results (2026-09-27, Release, after the PR1 review pass): new class 50/50;
`FullyQualifiedName~PartO|FullyQualifiedName~PartF` 1275/1275 (PR0's 1238 − 13 removed proofs + 50);
`PartOIterationPreparationTests` 86/86; `PartOBaseMVHRTests` 34/34; `OverheatingScenario|TM59|VentilationStrategyMap`
256/256; full `SAM.Tests` 2535/2535; `SAM.sln` Release 0 errors.

One existing test changed: `OverheatingScenarioTests.PartOIteration_HasNoFoundationStageMember` pins the exact enum
membership and now includes the appended `DwellingIndependent`.

## 8. PR0 assumptions refined or settled by implementation

1. **Common-space identity (PR0 design gate H5), reviewed.** The identity is `PartOAssessmentScope.CommonSpace` +
   `PartOIteration.DwellingIndependent` + strategy `UV` + the empty assumption set. An orthogonal identity with no
   pseudo-iteration was considered and rejected as larger and riskier:
   - The key (`OverheatingScenario:v1`) already hashes the iteration **name** in a fixed position. Removing the iteration
     from common-space keys, or adding a field, changes the derivation, which means a new identity schema and a
     migration of every stored key (PR0 G).
   - Using `Undefined` would collide with an unreadable persisted iteration, which also loads as `Undefined`.
   - A new `PartOAssessmentScope` member cannot help: the scope already says `CommonSpace`, and what has to be stated is
     the absence of a dwelling stage.

   An appended member keeps every existing key byte-identical. **Evidence that it is not treated as a runnable
   iteration** (search of SAM, SAM_Tas and SAM_UI for switches, `Enum.GetValues` and `.Iteration` reads):
   - `PartOIterationVentilationMode` returns `Undefined` with a refusal, and `PartOIterationOperatingMode` refuses it;
   - `PreparePartOIteration` therefore refuses it;
   - `PartOOperatingAssumptions` is the empty set, so `PartOIterationOpeningCompatibility` asserts nothing;
   - `Create.OverheatingScenarios` refuses it for a dwelling;
   - no UI enumerates the enum. SAM_UI's `PartOIteration3MethodText` reads the iteration of a **run option** (1a/1b/2),
     never of a scenario;
   - the TM59 map ignores the iteration.

   The one misleading surface is SAM_Tas `PartODiagnosticLog`, which labels a whole run with `scenarios[0].Iteration`.
   That is the known single-iteration blocker **C9** (PR3 scope); it already mislabels any mixed run. Pinned by
   `DwellingIndependent_IsACommonSpaceIdentityOnly`.
2. **Missing strategies (D2.1):** implemented with an explicit assessed scope. Inside the scope every dwelling must have a
   strategy; outside it a dwelling is unassessed and untouched — never read as Natural.
3. **Which common spaces are assessed:** see §4.4. Mixed zones and corridor spaces outside a whole-corridor common zone
   refuse; no assessed corridor is silently omitted.
4. **Baseline air movements:** PR0 named "Part O air movements". After review, the rule is §2a: movements are authored
   design data, and only unresolvable movements (baseline) and strategy-incompatible movements (materialisation) refuse.
5. **Result detection:** the cluster answers no interface query (`GetObjects<IResult>()` is null), so detection walks the
   stored types and tests each against `IResult` — the PR0 intent (base type, not a list) unchanged.
6. **Explicit product sufficiency:** PR0 did not say. PR1 refuses an explicit product that cannot serve the dwelling's
   duty, where the legacy manual assignment only flags it. **Confirmed by the owner.**
7. **Retained design:** the terminal count is taken on the baseline and the fingerprint compared after scoped
   realisation, so a requirement that gained no baseline terminal makes the design stale.
8. **Isolation:** never performed; an isolated model is refused as a baseline. **Confirmed by the owner** as a
   final-materialisation invariant (§4).

## 9. Residual risks

- `Fingerprint_Baseline` includes project-setting objects that are `SAMObject`s with instance guids
  (`PartOEquipmentSelection`, `PartOProjectTestVentilationUnit`); re-creating one with equal content makes the record
  stale — a false staleness, fail-closed.
- A baseline carrying run-written design days is refused, and so is an authored air movement that conflicts with an MVHR
  dwelling. The user must reopen the pre-Part-O source or edit the movement (accepted migration cost, PR0 G).
- The per-space internal-condition copy of the Part F rate (PR0 P5) is still written on MVHR spaces; it is pre-existing,
  inert while ticV is refused, and not a new store.
- No licensed TAS run of a mixed model yet, and no scale measurement (PR4).

## 10. Next step

Review and merge this SAM PR into `sow/2026-Q3`. Then **PR2 (SAM_UI)**: the scalable dwelling strategy grid; materialise
on a copy (the open model stays baseline + intent); one simulation; TM59; the explicit "accept 2B for a dwelling" design
edit onto the baseline's terminals (lineage-matched, PR0 D3), recording `RetainedDesign` + `PartODwellingDesignFingerprint`;
sidecar `PartORunResume:v3` carrying the record.

## 11. Follow-up: accepting a dwelling's design (`Modify.AcceptPartODwellingDesign`, 27 Sep 2026)

The PR0 D3 "accept 2B for a dwelling" edit, added for SAM_UI PR2's **Accept optimised airflow…** after the owner's
live review showed a clean baseline could not reach a retained design (it carries no terminals).

`PartODwellingDesignAcceptance AcceptPartODwellingDesign(this AnalyticalModel baseline, Guid zone, AnalyticalModel source, double tolerance_Lps = 0.001)`
returns the baseline with ONE dwelling's design terminals at the source's design airflow (a new model; neither input is
modified), the dwelling's `PartODwellingDesignFingerprint`, and the per-space/direction changes - or refusals and no model.
Existing operations only:

1. **Lineage** - each source terminal in the dwelling realises exactly one continuous requirement of the baseline space
   (`PartFTerminalReference.Matches`: room, role, source paragraph - never a guid, the source's terminals are
   regenerated), and every continuous requirement is realised in the source (whole-dwelling; a partial design refuses).
   Untraceable, ambiguous or airflow-less terminals refuse. The zone must be a baseline dwelling and hold the same spaces
   in the source.
2. **Terminals** - `RealizePartFVentilationTerminals` scoped to the dwelling's spaces (terminals only, no ICs/systems).
3. **Airflow** - per space and direction, the source total via `SetSpaceDesignFlowRate` (the control 2B varies, with the
   Approved Document F floor; its refusal refuses).

Refused: a baseline that is not clean (a simulated/materialised model is never patched), a non-dwelling zone, a source
lacking the dwelling, and a baseline space carrying a designer-added terminal that realises no requirement
(`SetSpaceDesignFlowRate` would spread the accepted total onto it) - generalised after review: every existing baseline terminal must realise exactly one continuous requirement in its own direction; also refused: a source terminal whose direction contradicts its requirement and an unusable tolerance. `Changes`/`Notes` are published only when every write succeeded, and `After_Lps` is the persisted value (Part F floor snapping). A baseline terminal with an unusable duty (negative, NaN, none) refuses before any write, so the equal-total fast path cannot bypass the setter's validation. A space shared with another owning zone (a dwelling or a classified non-dwelling zone such as a corridor) refuses - the materialiser's OverlappingZones scope; every space/direction goes through SetSpaceDesignFlowRate (no equal-total shortcut), so the floor and duty rules always apply. A terminal related to more than one space (baseline or source), or to a space outside the dwelling, refuses. Every persisted difference is reported in `Changes`, even within the tolerance. Baseline-terminal rules apply only where a write happens (a written space: every lineage-tagged terminal, and untagged ones in the written direction), so an untouched manual fan does not block acceptance. Nothing is written to a `PartODwellingStrategy`; the caller records `RetainedDesign` + the returned
fingerprint, and `MaterialisePartODwellingStrategies` reads the airflow from the terminals (one authority). Balance is the
materialiser's (`MechanicalDesign`), not restated. Other dwellings, the corridor and the source are untouched (pinned).

Tests: `PartODwellingStrategyMaterialisationTests.Acceptance.cs` (25) - written onto the baseline for that dwelling only;
materialised as retained beside NV + MVHR; idempotent; requirement design changes nothing; refusals for non-clean
baseline, non-dwelling / foreign source, partial source, untraceable terminal, below-floor design. Real-data check (SAM_UI
investigation harness): the 26 Sep live Iteration 2B round `-Opt10` accepted onto a clean derivative of the example model
for each of the three flats (e.g. Flat 3: Bedroom 63→143, Kitchen 55→95, Ensuite 8→48 l/s), fingerprint equal to the
hand-composed seam that ran through real TAS.
