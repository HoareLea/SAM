<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O / TAS validation

**This is an engineering validation record, not a diary.** It states what has been validated against real
TAS output, what that validation proves, which defects it exposed, and what remains unvalidated. The chronological session narratives it was consolidated from are not part of this repository.

Companion document: [`PartF-ADF-Volume1-2021-Traceability.md`](PartF-ADF-Volume1-2021-Traceability.md) for Approved Document F
regulatory and calculation traceability.

---

## Purpose and scope

**The question this record answers:** when SAM assesses a real TAS simulation against CIBSE TM59, does it
produce the same numbers TAS's own Domestic Overheating report produces for the same `.tsd`, and does each
result reach the space it belongs to?

**What is deliberately NOT claimed anywhere in this document.**

- **A TM59 pass is not Part O compliance.** Passing temperatures do not establish that every Approved
  Document O modelling assumption behind the simulation was applied. The report layer states
  `TM59 occupied-space assessment: PASS`, never "Part O compliant", on purpose.
- **Agreement with TAS is not correctness.** It establishes that two independent implementations of the
  TM59 arithmetic agree on the same input series. Where SAM deliberately assesses something TAS's canned
  report does not, agreement is not available and is not expected — see
  [Known scope/classification differences](#known-scopeclassification-differences).
- **One flat is not a model set.** See [Remaining validation work](#remaining-validation-work).

### Validation assets

| | |
|---|---|
| TAS | **9.5.7.0**, `C:\Program Files\Environmental Design Solutions Ltd\Tas` |
| Model | `SAM_zoningAM_v2zonesisDomestic.sam` — Zone Category `Flats` → Flat 1 / Flat 2 / Flat 3 (`IsDwelling` true) + Corridor (`false`), **9 spaces**. The canonical fixture shape: three dwellings each containing a room named exactly `Bedroom 2`, plus a communal corridor. |
| Weather | DSY, `C:\Users\Public\Documents\Tas Data\Databases\CIBSE Weather 2021.twd`, readable via `SAM.Weather.Tas.Convert.ToSAM_WeatherDatas(path)` |
| Flat1 BasePassive evidence | `…\SAM_daily\2026-07-15 PartO\Simulation\` — SAM diagnostic logs `Flat1.BasePassive.partO.20260818-122737.jsonl` (run `c4aa16f2-990c-41b6-ae8f-c78eedd14294`) and `…-134420.jsonl`, alongside TAS's own `Domestic Overheating (CIBSE TM59).xlsx` |
| Earlier full run | `…\SAM_daily\2027-08-03-HVAC\` — `000000_SAM_AnalyticalModel.{tbd,tsd,json}`, `.timing.csv` (78 s), `Report XMLs\…DomOv.xml`. **Also the only real TPD on this machine**, and therefore the asset the TPD identity question must be settled against. |

Both comparisons below were made by parsing the real evidence files directly — the `.xlsx` via its raw
OOXML, the JSONL via its own space records — not from any prose summary of them.

---

## Validation routes

Three routes reach TM59. **Preparation differs; assessment does not** — all three converge on the
engine-neutral `TM59AssessmentCalculator`, which never sees the words TSD, TPD or TAS.

```
TSD-simple:  TSD ─────────────────────────────────────────────────► model ─┐
TPD-full:    TPD ─► pass 1 ─► TBD COPY ─► pass 2 ─► TSD ─────────► model ─┼─► TM59AssessmentCalculator
TPD-approx:  TPD + companion TSD ─► synthesised (MRT + ZT) / 2 ──► model ─┘
```

| Route | Validated against real TAS output? |
|---|---|
| **TSD-simple** | **Yes** — Flat1 BasePassive, one `.tsd`, space by space. This is the whole of the evidence below. |
| **TPD-full** | **No.** Its intended transfer is refused rather than approximated — see [TPD/TBD route limitation](#tpdtbd-route-limitation). |
| **TPD-approx** | **No.** Unit-tested only, and it carries an unresolved identity risk (same section). |

---

## Flat1 BasePassive validation

### Simulation identity

**Result: identity resolves by TAS zone guid on every space, and nothing was silently dropped.**

Read one by one from the per-space records of `Flat1.BasePassive.partO.20260818-122737.jsonl`, not only
from the aggregate `run` record:

- `identityMode: "zoneGuid"` on **all 9** spaces;
- `simulatedZoneGuidRaw` populated on all 9 and **equal to `designZoneGuidRaw`** on every one — the same
  string including braces and casing (e.g. `{04860158-BDB1-4295-9EC6-DE80B409774D}` both sides), so the
  format-mismatch risk that would have flipped the run from "9 resolved by unique name" to "9 unresolved"
  did not materialise;
- `simulationSpaceMapIsComplete: true`, `overheatingScenarioMapIsComplete: true`,
  **`unassociatedCount: 0`**, `refusalCount: 0`;
- `workflowSuccessful: true`, `tM59Successful: true`.

**Why this matters more than it looks.** `SimulationSpaceMap` resolves stable key first, unique name as
fallback, and refuses on ambiguity. Three flats each holding a room named exactly `Bedroom 2` is the shape
the unique-name fallback structurally cannot serve, so "the fallback was succeeding" was never good enough
— identity had to be shown resolving on the key itself. It now does.

**Two identity caveats that this evidence does NOT cover.**

1. **The design side is still name-assigned.** `Modify.UpdateIds` strips `SpaceParameter.ZoneGuid` from
   every space before the loop that would read it, so the TBD zone is found by **name** and `zone.GUID` is
   then stamped. The diagnostic log records this honestly as
   `zoneGuidProvenance: "assignedDuringWorkflow"`. A `zoneGuid` identity mode is real — both sides match
   on the key by the time anything reads it — but it does not prove the chain one layer up was ever free
   of a name match. Whether to preserve the guid through the strip or refuse an ambiguous name match is an
   open decision.
2. **This is TSD evidence only.** Nothing here says anything about the TPD route's identity.

### SAM vs native TAS TM59 comparison

The engineer reran Flat1 BasePassive in TAS itself and pulled the native "Domestic Overheating (CIBSE TM59)"
report for the same `.tsd` the SAM diagnostic run was built from. Compared space by space.

**8 of 9 spaces agree with TAS on every number that governs pass/fail.** The 9th, `Corridor_1`, disagrees
by design and not by defect — see [Known scope/classification differences](#known-scopeclassification-differences).

| Space | Criterion | TAS | SAM | Agreement |
|---|---|---|---|---|
| `Bedroom 2_3`, `Bedroom 2_6` | mechanical | occupied / max / exceeding-26 / Pass | identical | exact |
| `Kitchen_4`, `Kitchen_7` | mechanical | occupied / max / exceeding-26 / Pass | identical | exact |
| `Studio 1_0` | naturalBedroom | day 37; night 3285 / 32 / 11; Pass | identical on all four | exact |
| `Ensuite_5`, `Ensuite_8` | corridor-style | 0 / 0 / 0, Pass | 0 hours >28 °C, Pass | exact |
| `Bathroom_2` | corridor-style | 0 / 0 / 0, Pass | 2 hours >28 °C, Pass | scope difference, investigated, **not a defect** |
| `Corridor_1` | corridor-style | 0 / 0 / 0, Pass (TAS "Other" bucket) | 337 vs 262, SignificantRisk | **by design** |

### Occupied-space criteria

The report layer's own rendered text was then rebuilt from the real numbers and compared against TAS
figure by figure. **Every applicable occupied-space TM59 criterion matches the TAS native report exactly:**

| Space | Check | TAS | SAM `report` |
|---|---|---|---|
| `Studio 1_0` | Criterion 1 | 37 / 110 | 37 / 110, margin +73 |
| `Studio 1_0` | Criterion 2 | 11 / 32 | 11 / 32, margin +21 |
| `Bedroom 2_3` | >26 °C | 131 / 262 | 131 / 262, margin +131 |
| `Kitchen_4` | >26 °C | 135 / 142 | 135 / 142, margin +7 |
| `Bedroom 2_6` | >26 °C | 129 / 262 | 129 / 262, margin +133 |
| `Kitchen_7` | >26 °C | 129 / 142 | 129 / 142, margin +13 |

`OCCUPIED SPACE ASSESSMENT: PASS` throughout — **including** with `Corridor_1` at `SignificantRisk`, which
is asserted behaviourally by `ASignificantRiskCorridor_DoesNotFailTheOccupiedSpaceAssessment` and is a
deliberate separation of two concepts: `TM59ComplianceStatus` (Pass / Fail / NotApplicable) and
`TM59RiskStatus` (Acceptable / SignificantRisk).

**Association was complete on this run.** All 9 assessment spaces produced a result, so the report's
"SPACES NOT ASSESSED" section correctly reads "Every space the assessment covered produced a result".
The mechanism for a real gap — a space present but unresolved — is unit-tested, because this run does not
exercise it live.

### Full-year >28 °C / corridor-style results

SAM evaluates an occupancy-independent full-year >28 °C check on spaces that carry no occupied TM59
criterion. The distinguishing tell is the limit: `MaxExceedableHours` is overridden to
`8760 × 3% ≈ 262`, not `OccupiedHours × 3%` as the base type computes it for occupied criteria.

Real figures on this run: `Bathroom_2` 2 / 262, `Ensuite_5` and `Ensuite_8` 0 / 262 each, `Corridor_1`
337 / 262 (margin −75, `RiskStatus = SignificantRisk`).

**`Corridor_1` exceeding its threshold does not fail the occupied-space TM59 assessment**, and the report
does not claim it does.

### Known scope/classification differences

These are differences in **what is assessed**, not disagreements about arithmetic. None is a defect and
none was "fixed" to match TAS.

1. **TAS's canned report has no communal-corridor criterion.** A zero-occupancy circulation space is
   bucketed "Other" and reported 0/0/0/Pass — reporting "not run", not "0 measured". SAM deliberately runs
   its own criterion there. `Corridor_1`'s disagreement is entirely this.
2. **SAM's ancillary check is occupancy-independent, by design.**
   `TM59CorridorExtendedResult.GetHoursNumberExceeding28()` scans the whole 8760-hour year and is not
   gated on occupancy — confirmed deliberate by the `MaxExceedableHours` override above. That is why
   `Bathroom_2` (0 occupied hours) logs 2 hours >28 °C where TAS shows 0. Traced, confirmed apples-to-
   oranges, **no code change**: the occupancy-independent check is arguably the more conservative
   behaviour, not a bug to match TAS's silence on.
3. **"Corridor" is the catch-all bucket at the calculation level — resolved at the report level by
   InternalCondition.** `TMOverheatingCalculator.Calculate_TM59` still routes a space to `CorridorResults`
   when its `TM59Manager.TM59SpaceApplications` comes back empty **or** when its stated strategy is `UV` —
   which `SAM_Systems`' capability index describes as "Unconditioned. Provides nothing," not "communal
   corridor" — so the calculation-level bucket still catches a bathroom the same way it catches an actual
   corridor. **What changed:** `TM59AssessmentReport` now partitions that bucket itself, matching each
   result's space (by `Reference`/Guid, never by name) against its restored/resolved `InternalCondition`.
   Only `TM59_Communal Corridor (including pipework gains)` —
   `TM59InternalConditionResolver.CommunalCorridorInternalConditionName` — is reported under
   `COMMUNAL CORRIDOR RISK` and counted towards `CorridorRiskStatus`; everything else in the bucket (a
   bathroom, hall, ensuite, or any space with no `InternalCondition` to check) is reported under
   `SUPPLEMENTARY >28 C CHECKS` instead — real engineering information that never contributes to
   `CorridorRiskStatus` and is never presented as a mandatory communal-corridor criterion. `Corridor_1` in
   this validation run carries exactly that condition, so it is the one row `COMMUNAL CORRIDOR RISK`
   shows; `Bathroom_2` carries `TM59_Bathroom` and reports under `SUPPLEMENTARY >28 C CHECKS` instead. The
   old `FULL-YEAR >28 C / CORRIDOR-STYLE RESULTS` heading and its ambiguity disclaimer are retired along
   with the ambiguity they existed to flag.
4. **`Kitchen_4` / `Kitchen_7` reported `TM59 Application: Sleeping, Cooking`, not `Cooking` alone — traced
   to a token collision and FIXED.** Both are multi-bedroom-apartment kitchens, so
   `TM59InternalConditionResolver` names their `InternalCondition` `"1 Bed Apt. Kitchen"` — the apartment's
   bedroom **count** is part of the condition's own name. `TM59Manager.TM59SpaceApplications
   (InternalCondition, TextMap)` classified that whole name through `TextMap.GetSortedKeys`, which checks
   it word by word against every TM59 keyword — and "Bed" is, independently, one of the literal "Sleeping"
   keywords. The same token that stated the apartment's bedroom count was read as evidence that *this* room
   is used for sleeping.

   **The fix.** `TM59Manager.RoleMatchName` strips a leading `"N Bed Apt."` qualifier from an
   InternalCondition's name before it is matched against the Sleeping/Living/Cooking keyword lists —
   applied only to the InternalCondition-based `IsSleeping`/`IsLiving`/`IsCooking` overloads (and therefore
   `TM59SpaceApplications(InternalCondition, TextMap)`, which calls them). Space-name classification
   (`IsSleeping(Space, TextMap)` and its siblings) is untouched, so `SAM_Tas`'s `RoomUse.cs`, `ToSAP.cs` and
   the legacy `OverheatingCalculator.cs` — which call the same InternalCondition-based overloads directly —
   now also classify a multi-bedroom apartment kitchen as `Cooking` alone, correctly. `Kitchen_4`/`Kitchen_7`
   now report `TM59 Application: Cooking`. Pinned by `TM59SpaceApplicationClassificationTests` (`SAM.Tests`),
   including that a plain `"Double Bedroom"`/`"Single Bedroom"` condition (no apartment-size prefix) and raw
   Space-name classification are both unaffected.

   **The mechanical `>26 °C` numbers are unchanged** — `Sleeping` only ever affected which
   natural-ventilation result type a space would be routed to, never the mechanical criterion Kitchen_4/
   Kitchen_7 are actually assessed under, and the "SAM vs native TAS TM59 comparison" table above still
   matches TAS exactly (135/142 and 129/142, unchanged).

   **CLOSED — the related "room"/"bedroom" substring collision.** `GetSortedKeys` (the primitive the fix
   above still delegated to) does `value.Contains(token) || token.Contains(value)`, so a bare apartment
   `"N Bed Apt. Living Room"` condition (no `/Kitchen` suffix) also misread Sleeping — "room" is its own
   token there, and `"bedroom".Contains("room")` is true. **Fixed** by routing InternalCondition role
   matching through `Query.TM59TextMapMatches` instead (`TM59Manager.IsRole`) — the same deterministic,
   whole-token/whole-phrase matcher `TM59InternalConditionResolver` already uses for Space classification,
   reused rather than special-cased for "Living Room". It requires an alias's tokens to appear as an exact,
   contiguous sequence in the name, so "room" can never match the alias "bedroom" as a substring.
   `"N Bed Apt. Living Room"` now classifies as `Living` alone, confirmed end to end through
   `TMOverheatingCalculator.Calculate_TM59` — the space is *not* routed to the bedroom result type, so
   Criterion 2 correctly reads N/A while Criterion 1 still applies
   (`TM59AssessmentCalculatorTests.ApartmentLivingRoomCondition_IsNotRoutedAsABedroom_SoCriterion2IsNotApplicable`).
   Pinned by `TM59SpaceApplicationClassificationTests`, which also confirms `"N Bed Apt. Kitchen"`,
   `"N Bed Apt. Living Room/Kitchen"`, `"Double Bedroom"`/`"Single Bedroom"`, `"Studio"` and raw Space-name
   classification are all unchanged by this second fix.

### Report structure (refined)

The verification report (`TM59AssessmentReport` / `TM59AssessmentReportFormatter`) was reworked for
presentation, auditability and correct TM59 semantics — the validated arithmetic above is unchanged:

- Heading is now `CIBSE TM59:2017 OVERHEATING ASSESSMENT`, with an explicit
  `Part O modelling assumptions: NOT VERIFIED BY THIS RESULT REPORT` line near the header — this report
  assesses simulated temperatures against TM59:2017 only; it never claims Part O compliance.
- `NATURAL VENTILATION` is one row per space (grouped by `Reference`/Guid, never by name), showing
  Criterion 1 and Criterion 2 side by side with an `Overall` verdict; a new `ASSESSMENT HOURS` table
  states the exact `Occupied Summer Hours` / `Annual Night Occupied Hours` basis each space was assessed
  against, read directly off the result (`TM59NaturalVentilationResult.SummerOccupiedHours` /
  `TM59NaturalVentilationBedroomResult.AnnualNightOccupiedHours`, or their extended-result equivalents),
  never reconstructed from a rounded `Limit`.
- `MECHANICAL VENTILATION` gained an `Internal Condition` column and an `Annual Occupied Hours` column (the
  one basis this criterion has, already on `TMResult.OccupiedHours`); the ambiguous `Use` column is now
  headed `TM59 Application`.
- The former single `FULL-YEAR >28 C / CORRIDOR-STYLE RESULTS` section is split into `COMMUNAL CORRIDOR
  RISK` (positively identified corridors only, contributing to `CorridorRiskStatus`) and
  `SUPPLEMENTARY >28 C CHECKS - INFORMATION ONLY` (everything else the same calculation produced,
  explicitly labelled as information only rather than a mandatory corridor criterion) — see item 3 above.
  Both gained an `Annual Hours` column (`TM59CorridorResult.AnnualHours`, the real annual series length,
  not `Limit` divided back through its exceedance factor).
- A new `ASSESSMENT BASIS` section states the fixed TM59:2017 assessment periods plus the genuinely
  data-derived figures: the real annual series length
  (`TM59AssessmentReport.AnnualHours`, read via the shared `TMExtendedResult.GetAnnualHours()` — never
  assumed to be 8760), and — only when that series confirms a standard 365- or 366-day year — the calendar
  "clock hours" behind the natural-ventilation study period (`1 May - 30 Sep`, always 153 days/3672 hours
  regardless of leap year, since the range never includes February) and the bedroom night-time window
  (`22:00-07:00`, 3285 hours for 365 days or 3294 for 366). A non-standard or partial series prints the
  descriptive text without a clock-hours figure, rather than a misleading one. These are calendar constants
  of the period definitions themselves, not per-space occupancy data — the space-specific `Occupied Summer
  Hours` / `Annual Night Occupied Hours` / `Annual Occupied Hours` in `ASSESSMENT HOURS` and
  `MECHANICAL VENTILATION` are unaffected and remain read directly off each result. Descriptive only; it
  recomputes nothing.
- The legend gained a `TM59 Application` block explaining `Sleeping`/`Living`/`Cooking` as assessment
  roles (a Studio carries several at once) rather than the complete architectural room type. The values
  listed are enumerated from the real `TM59SpaceApplication` enum rather than an independently typed-out
  list, so a future application added to the enum appears automatically; `Undefined` is deliberately
  excluded, since `TM59AssessmentReport`'s own `Use` method never emits it. `Actual`'s wording was adjusted
  to "hours exceeding the stated temperature/comfort threshold" — more accurate now that
  `SUPPLEMENTARY >28 C CHECKS - INFORMATION ONLY` is explicitly not a mandatory TM59 criterion. The
  assessment-period prose the legend used to carry now lives in `ASSESSMENT BASIS` instead, and the
  SAM-vs-TAS validation history that used to live there is this document.
- Weather-data identity (e.g. the DSY file the run used) was investigated for `ASSESSMENT BASIS` and left
  out: `TM59AssessmentResult`/`TM59AssessmentReport` carry no weather-data reference at all today (`Source`
  is the model/TSD path only), so stating it would mean adding new plumbing through the calculator and
  result types rather than reading something already reliably available — out of scope for this pass.

---

## TPD/TBD route limitation

**The intended TPD-full transfer cannot be performed, and is refused rather than approximated.** The
intended sequence was: read the first pass's supply air temperature and supply airflow, inject both into a
TBD copy, simulate, take `ResultantTemperature` from the second pass. The blockage is specifically on the
**write** side.

- **The read half is available and already happening.** `Convert.ToSAM_SpaceSystemResults` asks the
  `SystemZone` for every `SpaceDataType`, and that enum already contains `SupplyAirTemperature = 3` and
  `FlowRate = 1` alongside `ZoneTemperature = 9`. The supply conditions are in hand.
- **The write half has nowhere to go.** Reflected over the whole `Interop.TBD` assembly (143 types): **no
  TBD type exposes a per-zone supply air temperature of any kind** — not the zone, not its internal
  condition, not the thermostat, not the internal gain. That is by design: TBD introduces ventilation air
  at outside or adjacent-zone conditions, and conditioned supply air is a TPD concept. It is exactly why
  TAS keeps the two models apart.

  *Correcting record, kept because it shows how the conclusion was tested:* an earlier version of this
  claim said the only temperature-valued member in `Interop.TBD` is `IWeatherYear.groundTemperature`.
  **That was wrong** — the scan matched `Temperature` case-sensitively and missed every member spelled
  `Temp` (`IControls` frost-protection / authority / night-setback, `IEmitter` outside-temperature
  cut-offs, `ISurfaceOutputSpec.dryBulbTemp`). The load-bearing conclusion was re-verified member by
  member and survives: none of those is a zone supply condition.
- **Injecting the airflow alone would be worse than partial progress.** TBD's ventilation profile does
  accept an hourly series, so the airflow half is mechanically writable — but air introduced *without its
  temperature* enters at outside conditions, stating a system that does not exist. And once the thermostat
  pins the zone air temperature (which is what the supported transfer does), an injected flow moves only
  plant load and leaves `ResultantTemperature` untouched. Half of this transfer is not a fraction of the
  answer.
- **`UpdateSpaceAirflows` / `UpdateFanAirflows` are not it.** Both operate on the TPD and set scalar
  design sizing values. Neither hourly nor TBD-side.

**So `ResultantTemperatureTransfer.SupplyAirTemperatureAndAirflow` is refused with the reason stated, and
the supported transfer is `ZoneTemperatureToThermostatLimits`, named as the approximation it is.** That
approximation is not laziness: `ResultantTemperature` is a function of air and mean radiant temperature, so
pinning air temperature to the first pass's *achieved* value leaves TBD to compute only the radiant half —
the half TPD cannot give. It closes the loop; the intended transfer cannot.

**If a future TAS release exposes a per-zone supply air temperature on the TBD side, the one place that
has to change is `ResultantTemperaturePreparation.TransferRefusal`.**

### Open risk on the TPD-approx route — the two identity namespaces have never been shown to match

The approximate route correlates a space to a TPD result by matching `SpaceParameter.ZoneGuid` (mapped
from `TSD.ZoneData.zoneGUID`) against `SystemSpaceResult.Reference`. That reference comes from
`(systemZone as dynamic).GUID` — and **`ISystemZone` declares no `GUID`**, verified by reflection, so it
binds to `ISystemComponent.GUID`: **the TPD component's own guid**. Nothing establishes that a TPD
component guid equals a TBD/TSD zone guid, and the identity evidence in this document covers **TSD only**.

**If they differ**, identity mode never engages, the whole model falls to name mode, and three rooms all
called `Bedroom 2` are entirely refused — safe, but useless.

**What was done about it:** the risk is documented on the class; the fall back to names emits **one
model-wide diagnostic naming the cause** rather than N baffling per-room messages; and
`WhereTheTwoIdentityNamespacesDisagree_ItSaysSoOnceAndFallsToNames` gives the two sides deliberately
different strings so the assumption cannot be silently re-encoded in a fixture — which is what the
original tests did.

**The probable fix, NOT applied:** `IZoneLoad.GUID`. `ITSDData` exposes `GetZoneLoadForGuid(string)`, so a
zone load is addressable by the guid the TBD/TSD side uses. But `Reference` also correlates a
`SystemSpaceResult` to its `SystemSpace`/`SystemZone` in the energy-centre model and is persisted in JSON.
**Verify against the real TPD in `…\SAM_daily\2027-08-03-HVAC\` before changing it.**

### Also recorded, not fixed

`Modify.CalculateResultantTemperature` (the authoritative two-pass route) reads
`GetValues(new Range<int>(0, 8759))` **unbounded**, so a TPD simulated over part of the year yields `0.0`
for the rest, and those zeros go into **both** thermostat limit profiles of the TBD copy — the second pass
then runs against a 0 °C setpoint outside the simulated period. Pre-existing, left alone under "preserve
behaviour exactly". Note the irony: the authoritative route lacks the alignment/count protection that was
added to the approximate one.

---

## Validation defects found and fixed

Every one of these was found **by** validating against real TAS output, and each is the reason this record
exists rather than a green test suite being taken as sufficient.

| # | Defect | Fixed in | How validation exposed it |
|---|---|---|---|
| 1 | **Criterion 1's report `Limit` read the annual `MaxExceedableHours`, not `MaxExceedableSummerHours`** — the basis TAS's own report states this criterion on, paired with "Occupied Summer Hours". For `Studio 1_0` it showed `262` where TAS's real figure is `110`. | SAM `3fcfb880` | Only visible against a real report. **Every existing test happened to use `110` for both the annual and the summer value**, so the wrong field and the right one were indistinguishable by coincidence. Fixtures now carry a deliberately wrong-looking annual placeholder (`999`) so that coincidence cannot recur, plus a test pinning the real `Studio 1_0` figures. Mutation-tested: reverting fails 5 tests. |
| 2 | **`Query.Simplify` argument-order bug** in the non-bedroom natural-ventilation branch — the summer-hours pair and the annual limit were rotated by one constructor position, so every simplified (non-extended) result reported `SummerOccupiedHours` / `MaxExceedableSummerHours` under swapped meanings. Pass/Fail was unaffected (derived from `HoursExceedingComfortRange` directly). | SAM `07d1c728` | Found independently while establishing which pair TAS states its criteria on — i.e. by the same question that produced defect 1. Isolated with its own regression test, in its own commit ahead of the reporting work. |
| 3 | **Diagnostic log completeness** — `occupiedHours` / `maxExceedableHours` logged the whole-year annual pair while TAS states the day criterion against `3672` / `110`. For `Studio 1_0` the log read 8760/262 against TAS's 3672/110 **even though the exceedance count and Pass/Fail matched exactly**. Not a TM59 defect: the assessment used the right basis throughout, the log displayed a misleading pair beside it. | SAM_Tas `2750a21` | Directly, by diffing the log against TAS's report. `summerOccupiedHours` / `maxExceedableSummerHours` are now logged for `natural` / `naturalBedroom` rows, read the same way `Query.Simplify` already reads them. 2 new tests. |
| 4 | **`Convert.ToSAM(TSD.ZoneData)` never stamped `SpaceParameter.ZoneGuid`**, so `Query.SimulationSpaceKey` returned null for every simulated space and identity silently fell to the unique-name fallback. | SAM_Tas `d76be13` | Exposed by the diagnostic logging built to answer "does identity actually resolve on the key?" — the honest answer was no. Root cause is one layer down and **still open**: `SAM.Core.Create.ParameterSet` / `TypeMap` reads the source property by the **SAM-side** name and stores under the **TAS-side** name, inverted both ways, and `SpaceParameter.ZoneGuid` is registered as `"Zone Guid"` — *with a space* — against `"zoneGUID"`. The space defeats even a case-insensitive match. Repaired by one guarded explicit stamp; the shared helper deliberately untouched. |
| 5 | **A simplified bedroom result carried the night-time numbers but no verdict to report against them** — `Pass` on a TM59 result is Criterion 1 alone. `TM59NaturalVentilationBedroomResult` gained `Criterion2`. | SAM `3a386baf` | Surfaced while building a report that has to state Criterion 1 and Criterion 2 separately, because that is how TAS states them. |

**One test-coverage limit, stated plainly.** The `TsdZoneIdentityStampTests` for defect 4 do **not** call
the converter, so they will not fail if the stamp is reverted: `TSD.ZoneData` is COM interop and
`Convert.ToSAM` reaches `ActiveSetting.Setting`, whose `GetDefault()` references `typeof(TAS3D.Zone)`,
`typeof(TBD.zone)` and `typeof(TSD.ZoneData)` — exercising it needs the whole TAS interop surface loaded
inside a `net8.0` project built specifically to exclude it. Judged not worth breaking the COM-free
boundary for. **That converter line is covered by the manual acceptance run above and by nothing else.**

---

## Known open defect — natural-ventilation Criterion 1 Pass/Fail uses the wrong hour basis (found 2026-09-16, fixed 2026-09-16)

**Update (2026-09-16, later).** This defect is now fixed and regression-covered — see § *TM59 Criterion 1
(natural ventilation) — annual-vs-summer hour-basis defect, found and fixed (2026-09-16)* below for the fix,
the regression, and the full test result. The write-up immediately below is preserved unchanged as the
historical record of the defect as found during the 3-B4 rerun, before the fix existed; read its "NOT fixed" /
"open" language as describing that point in time, not the current state.

Historical text (as found, before the fix): it was found during independent review of
the 3-B4 rerun above, is confirmed by direct source inspection (not assumed from the review that raised it),
and is unrelated to the recirculation clamp — it does not affect this session's own B4/B0/1a results (see
*Consequence* below), but it is a live, untested gap in TM59:2017 Criterion 1 itself and must be resolved or
conclusively disproved before the natural-ventilation route's Pass/Fail can be trusted on a real project.

**The defect.** `TMExtendedResult.Criterion1` (`SAM.Analytical\Classes\Result\TM\TMExtendedResult.cs:240-263`)
compares `GetOccupiedHoursExceedingComfortRange()` against `MaxExceedableHours` — both computed from the
**full-year** `OccupiedHourIndices` the result was constructed with (`MaxExceedableHours` at `:50-56` is
`OccupiedHours * 0.03`, and `OccupiedHours` is `occupiedHourIndices.Count` with no date filter).
`TM59NaturalVentilationExtendedResult` and `TM59NaturalVentilationBedroomExtendedResult` do not override
`Criterion1`, `MaxExceedableHours` or the comfort-range hour set, so they inherit this full-year
computation — even though `TM59NaturalVentilationExtendedResult` itself already carries the **correct**
summer-restricted equivalents, `GetSummerOccupiedHourIndices()` / `GetSummerMaxExceedableHours()`
(`TM59NaturalVentilationExtendedResult.cs:36-64`, filtering to `HourOfYear.SummerStartIndex..SummerEndIndex`
— May-September, matching TAS).

`TM59AssessmentReport.NaturalVentilationChecksFor` (`TM59AssessmentReport.cs:256-285`) knows this: its own
comment at `:266-270` states *"Limit is deliberately the SUMMER basis... not the base type's annual
MaxExceedableHours"*, and `actual_Criterion1`/`limit_Criterion1`/`basisHours_Criterion1` are all correctly
read from the summer-restricted getters for **display**. But the `ComplianceStatus` on the same line
(`:285`) is `tMResult.Pass ? Pass : Fail` — `Pass` is `Criterion1`, the **unfixed, full-year** computation.
The report can therefore display a small, correct, summer-based `Actual`/`Limit` pair while deciding
PASS/FAIL from a different, full-year pair entirely — the two are never required to agree.

**Why no test caught it.** `TM59SimplifyTests.PlainNaturalVentilation_PutsEachHourCountUnderItsOwnName`
constructs a real `TM59NaturalVentilationExtendedResult` (100 occupied hours: 40 outside summer, 60 inside,
all at a uniformly-exceeding temperature) and asserts `extended.Pass == simplified.Pass` — but every one of
the 100 hours exceeds by construction, so the full-year and summer-only verdicts are both `Fail` regardless
of which basis decides it; the fixture cannot distinguish them. `TM59AssessmentReportTests`' natural-
ventilation fixtures (`NaturalVentilation(...)`, `Bedroom(...)` helpers, `:891-899`) build the **plain**
`TM59NaturalVentilationResult`/`TM59NaturalVentilationBedroomResult` types directly, with `pass:` injected as
an explicit constructor argument — they test the report's formatting of an already-decided verdict, never
the extended type's own `Criterion1` computation. No fixture anywhere sets up occupied hours where the
full-year and summer exceedance ratios cross the 3% line on opposite sides of each other.

**Consequence, classified as instructed.**
- **This session's B4/B0/1a rerun and its deltas are unaffected.** Every one of the real project's 8
  assessed rooms is on the **mechanical** route (`TM59MechanicalVentilationExtendedResult` /
  `MechanicalVentilationCheckFor`), whose `>26 °C hours` criterion is correctly annual-basis by design (see
  `ASSESSMENT BASIS` in the report: *"Mechanical ventilation check: Full year"*) and reads the *same* basis
  for both `Actual`/`Limit` and `Pass` — no display/decision split exists on that route. The natural-
  ventilation code path this defect lives in is not exercised anywhere in the 3-B0/3-B4 comparison.
- **A B0-vs-B4 (or any A-vs-B) *relative* comparison stays valid even where the natural-ventilation route
  IS exercised**, because both sides of such a comparison are decided by the same (currently wrong) authority
  applied identically — a relative delta cancels a systematic basis error that an absolute verdict cannot.
- **Absolute natural-ventilation PASS/FAIL may be wrong** wherever this route is exercised. The two natural-
  ventilation acceptances already on record — Flat1 BasePassive's `Studio 1_0` (37/110, this document's
  §*Flat1 BasePassive validation*) and the Iteration 1b NV-OPEN/NV-NIGHT A/B (0–4 exceeding hours against
  110–262-style limits, §*Iteration 1b*) — both pass by very large margins under *either* basis, so their
  recorded verdicts are very unlikely to flip, but this has not been checked hour-by-hour and the calculation
  itself is confirmed wrong regardless of whether it happens to change any verdict recorded so far. A project
  whose natural-ventilation exceedances concentrate differently across the year (e.g. outside May-September
  but still within occupied annual hours) could see this decide the wrong way.

**Smallest correct remediation, NOT applied.** Override `Criterion1` (or equivalently `MaxExceedableHours`
and the comfort-range hour set it feeds) in `TM59NaturalVentilationExtendedResult` to compute both the
exceedance count and the limit from `GetSummerOccupiedHourIndices()` / `GetSummerMaxExceedableHours()` —
the same getters the report already trusts for display — rather than from the inherited full-year basis.
`TM59NaturalVentilationBedroomExtendedResult` needs no separate change: it does not override `Criterion1`
either, so it would inherit the fix. This mirrors, rather than reopens, the design decision already visible
in the report layer; it does not touch Criterion 2 (night-time, correctly annual by TM59:2017's own
definition) or the mechanical or corridor routes, which have no analogous split. **Not implemented here** —
per instruction, this calculation is reported, not silently changed, as part of a B4 rerun whose own results
it does not affect.

## Current confidence / limitations

**What is established.**

1. On the **TSD-simple** route, for **one** real flat at **BasePassive**, SAM's TM59 numbers match TAS's
   own Domestic Overheating report **exactly** on every occupied-space criterion the case exercises.
2. On that run, result association is by **TAS zone guid** on every space, with `unassociatedCount: 0` and
   no refusals — so no result reached the wrong room and none vanished.
3. The differences that remain are **scope and classification** differences, each traced to a named,
   deliberate design decision, not to arithmetic.
4. The validation exercise itself found **five real defects** that the test suites did not, two of which
   (defects 1 and 2) were invisible to any test that did not have a real TAS report to compare against.

**What is NOT established.**

- **Nothing about Part O compliance.** This is TM59 arithmetic and result association only.
- **Nothing about a second model.** One flat, one iteration, one `.tsd`.
- **Nothing about the plain `natural` (non-bedroom) criterion** — Flat1 exercises `mechanical`,
  `naturalBedroom` and `corridor-style` and never that one. Note that defect 2 lived in exactly that
  branch, which is the branch with the least real-output coverage.
- **Nothing about `AcousticRestricted` or `ActiveTrimCooling`.** Both are refused by
  `Query.PartOIterationOperatingMode` today, pending engineering decisions, so neither can produce numbers
  to compare.
- **Nothing about either TPD route.** No real TPD has been run through them, and the TPD-approx identity
  namespaces have never been shown to agree.
- **Nothing about the design-side identity provenance.** Still name-assigned.
- **Nothing live about the refusal path.** `unassociatedCount: 0` on every real run means the
  space-present-but-unresolved gap is unit-tested only.
- **Natural-ventilation Criterion 1 Pass/Fail used the wrong (full-year, not summer) hour basis** — see
  § *Known open defect* above for the historical finding, now fixed and regression-covered (§ *TM59
  Criterion 1 (natural ventilation) — annual-vs-summer hour-basis defect, found and fixed (2026-09-16)*
  below).

---

## Natural-ventilation Part O workflow — end-to-end licensed acceptance (2026-08-25)

> **Superseded in its mechanics, not in its findings.** This run stated the strategy as the string
> `"NV"` at `PartOIteration.BasePassive`. That pairing now REFUSES: the route is explicit
> (`PartOVentilationMode`) and Iteration 1b is `BaseNaturalVentilation`. Everything this section
> establishes still holds and is re-established, in both opening cases, by the 2026-08-26 A/B
> acceptance below. Reproduce that one.

**What this section establishes:** one naturally ventilated dwelling can now be carried from an authored SAM
model to a TM59 natural-ventilation result without SAM inventing a mechanical ventilation system for it, and
with the authored opening operation reaching the TAS aperture control intact.

**What it does NOT establish.** Zero continuous mechanical supply and extract means **SAM has not invented an
MVHR/MVRE system**. It does **not** mean the dwelling's natural-ventilation Approved Document F design has
been sized: System 1 background/trickle ventilator sizing and purge provision are not calculated anywhere in
this path. Do not report this as "Part F NV sizing". A TM59 pass is still not Part O compliance, for all the
reasons stated at the top of this document.

### The defect this closes

`PartFCalculator` is unconditionally System 4 shaped. Paragraph 1.67 gives **every** habitable room a
mechanical supply terminal and every wet room a continuous extract terminal, and nothing in
`SAM.Analytical/Classes/PartF/` takes a ventilation strategy as an input.
`SAMAnalytical.PreparePartOIteration` then called `Modify.ApplyPartFVentilationRates` **unconditionally, and
before `_ventilationStrategies` was read at all**, writing those rates onto `InternalConditionParameter.
SupplyAirFlow` / `ExhaustAirFlow` — the values `Query.CalculatedSupplyAirFlow` and the TAS export read.

A naturally ventilated dwelling was therefore simulated with mechanical supply and extract it does not have,
and the run reported `successful = true`. That is worse than a refusal: nothing in the result says the system
was invented. The second failure mode was the mirror image — an NV dwelling never run through a Part F
component hit `count == 0` in `ApplyPartFVentilationRates`, got `null` back, and was refused with "run a Part
F component first" for airflow it does not need.

### The gate

`Query.PartOPartFAirflowApplication` reads the strategy the assessed zones state — the **same** dictionary
`Create.OverheatingScenarios` is given, so the airflow decision and the TM59 criterion can never read
different answers — and returns one of three answers, which `Modify.PreparePartOIteration` acts on **before**
anything is applied:

| Assessed zones | Answer | Behaviour |
|---|---|---|
| every zone states `NV` | `SkipNaturalVentilation` | nothing applied; absence of `PartFSpaceData` is not an error; a note and a warning state what was not applied and why |
| no zone states `NV` (`MV`, `MVRE`, `UV`, or no zones at all) | `Apply` | unchanged — the behaviour this preparation has always had |
| `NV` beside anything else, silence included | `RefuseMixed` | **no model returned**, no scenarios stated, every zone named beside its strategy |

Only `NV` skips. `UV` is not mechanical either, but it selects the corridor criterion rather than the
natural-ventilation one and has never been the subject of this gate, so it keeps the existing behaviour.

The mixed case refuses rather than warning — unlike the opening-restriction disagreement, which only
mislabels which assumption a true result was obtained under. Here continuing would put continuous mechanical
supply and extract into the naturally ventilated half of the building, or strip the mechanically ventilated
half of the rates it is sized for. `ApplyPartFVentilationRates` is whole-model, so a per-zone application is
the real fix and is a separate change.

### Acceptance run

| | |
|---|---|
| Base model | `C:\TasOut\v40\A0.sam` — the 9-space TM59 residential model (Flat 1 / Flat 2 / Flat 3 / Corridor, 20 apertures each already carrying an **Unrestricted** `PartOOpeningProperties`) |
| Authored | Part F sized with the shipped rule set (8 of 9 spaces sized), then **one** aperture — `SIM_EXT_GLZ` on `Bedroom 2_3`, guid `956264f2` — re-authored `NightClosed`, hours 08–23, exactly as `SAMAnalytical.AddOpeningPropertiesByPartO` builds it. The other 19 openings are left Unrestricted and act as the control. |
| Prepared | `Modify.PreparePartOIteration(BasePassive, all zones, "NV")` — the production path |
| Weather | `C:\Users\Public\Documents\Tas Data\Databases\CIBSE Weather 2021.twd` |
| Run | `AnalyticalModel -> ToGbXML -> WorkflowCalculator`, `Sizing = true`, `Simulate = true`, days 1..365 |
| Produced | `nv.tbd` (414 KB), `nv.tsd` (4.8 MB) |
| Control | the identical model prepared as `MVRE`, sizing only, into `mv.tbd` |

**`build_tests/Fixtures/original_v1.sam` was examined and rejected as the fixture.** It is a 3-storey,
5-per-floor office massing: 15 spaces named `Floor_0_Zone_NORTH` …, **0 `Zone` objects, 0 `InternalCondition`
objects, and 0 of its 66 apertures carry any `OpeningProperties`**. It can state neither a dwelling, nor a
ventilation strategy, nor a restriction without being rebuilt rather than adapted, and it sits in a build
**output** directory rather than a curated fixture library.

#### Preparation, before TAS

```
PREPARE|airflowApplication=SkipNaturalVentilation|successful=True|openingCompatibility=Incompatible|scenarios=4
SCENARIO|Flat 1 - BasePassive - NV|strategy=NV|scope=Dwelling
SCENARIO|Corridor - BasePassive - NV|strategy=NV|scope=CommonSpace
SCENARIO|Flat 2 - BasePassive - NV|strategy=NV|scope=Dwelling
SCENARIO|Flat 3 - BasePassive - NV|strategy=NV|scope=Dwelling
APERTURE|956264f2-…|restriction=NightClosed|from=8|to=23|schedule=PartO_DayOpen_08_23|values=000000001111111111111110
```

Every space came out with `supplyAirFlowStated=False`, `exhaustAirFlowStated=False`. The Part F sizing it
declined to use is still on the model — `Bedroom 2_3` carries `continuousSupply_lps=45.5`, `Studio 1_0`
`32.5 / 44`, and so on for all eight sized spaces — so this is a decision not to apply it, not an absence of
anything to apply.

Prepared as `MVRE` instead, the same model gives `airflowApplication=Apply` and eight spaces with
per-space cloned internal conditions carrying the Part F rates (`Bedroom 2_3`: `calculatedSupply_m3s=0.0455`).

#### The opening profile in the produced TBD

Read back through Tas's own 0-based accessors, independently of the code that wrote it:

```
APERTURETYPE|element=Windows: SIM_EXT_GLZ_E67C225E -pane|name=Opening Cd0.411 F1 S00FFFE|Cd=0.410591
             |profileType=ticFunctionProfile
             |function=zdwno,0,19.00,21.00,99.00
             |factor=1
             |schedule=PartO_DayOpen_08_23
             |values=000000001111111111111110
```

The two other shared aperture definitions in the same TBD carry the same function and **no schedule** — the
19 Unrestricted openings — so the schedule reached exactly the one opening that asked for it. The workflow's
own note agrees: *"1 opening(s) requested an availability schedule, 1 of those read a schedule back off the
TBD profile."*

The 24 values are read SAM-hour-ordered (SAM hour `h` is TBD slot `h + 1`, because TBD's 24-slot hourly
indexed properties are 1-based hour-**ending**), so the string is directly comparable with
`DailyAvailabilitySchedule.ValuesText`.

#### No continuous mechanical airflow in the produced TBD

`SAMZoneMetadata` is written into the TBD zone description, because TAS has no field for SAM's airflow
decomposition. That is where "was a Part F continuous mechanical supply carried onto this zone" is readable
straight out of the file. Same zone, same model, two preparations:

| | NV run (`nv.tbd`) | MVRE control (`mv.tbd`) |
|---|---|---|
| `Bedroom 2_3` description | `{"ventilation":{"profile":false},"native":{"freshAirRate":8}}` | `{"ventilation":{"flow":0.0455,"flowPerArea":0,"flowPerPerson":0,"airChangesPerHour":0,"profile":false},"native":{"freshAirRate":0}}` |
| every zone `freshAirRate` | `8` l/s/p — the authored value, untouched | `0` — cleared by the Part F application (only the unsized `Corridor_1` kept its `8`) |

In the NV run the zone carries **no supply-air statement at all**. In the control it carries the Part F
0.0455 m³/s and its authored per-person outside-air rate has been zeroed.

`ticV.factor` is `1` ACH in both, and is not evidence either way: `"profile":false` — no SAM Ventilation
profile is assigned on this model, so TBD Building Simulator mechanical ventilation is not activated in
either run and `Query.VentilationAirChangesPerHour`'s value is never written. See
`Modify.UpdateInternalCondition` for why the presence of airflow data must not switch it on by itself.

#### TM59, from the produced TSD

`Tas.TSDQueryTM59Results`' own sequence — `Convert.ToSAM(tsd)`, `Create.TM59AssessmentCalculator`,
`OverheatingScenarioMap` → `VentilationStrategyMap`, `RestoreDesignInternalConditions`, `Calculate`:

```
COUNTS|naturalVentilation=5|mechanicalVentilation=0|corridor=4
NATURALVENTILATION|Studio 1_0        |type=TM59NaturalVentilationBedroomExtendedResult
NATURALVENTILATION|Bedroom 2_3       |type=TM59NaturalVentilationBedroomExtendedResult
NATURALVENTILATION|Living Kitchen_4  |type=TM59NaturalVentilationExtendedResult
NATURALVENTILATION|Bedroom 2_6       |type=TM59NaturalVentilationBedroomExtendedResult
NATURALVENTILATION|Kitchen_7         |type=TM59NaturalVentilationExtendedResult
CORRIDOR|Corridor_1, Bathroom_2, Ensuite_5, Ensuite_8|type=TM59CorridorExtendedResult
```

No ventilation-strategy refusals, no association refusals. **Zero mechanical results** — the NV route was
taken for every occupied space. The four corridor-criterion results are the communal corridor plus the three
wet rooms, which carry no TM59 space application; that is the documented pre-existing classification, not
something this change introduced. Pass/fail is not the point and is not claimed as compliance: what is
proven is that the correct NV model, with the correct opening operation, was simulated and assessed through
the natural-ventilation route.

**One trap worth recording.** The design model handed to the TM59 assessment must be the **workflow output**
model, not the pre-workflow one. `SimulationSpaceMap` resolves on the `ZoneGuid` that `Modify.UpdateIds`
stamps during the workflow; passing the pre-workflow model produced *"does not resolve to exactly one
simulated space"* for every space and zero results. Pre-existing identity behaviour, unchanged here.

### What is still not covered

- **Mixed NV + mechanical models refuse.** Per-zone airflow application is not implemented; a building with
  naturally and mechanically ventilated zones side by side must be prepared as separate iterations.
- **System 1 sizing.** Background/trickle ventilator and purge provision for a naturally ventilated dwelling
  are not calculated. `PartFCalculator` remains System 4 shaped for every strategy.
- **Wet-room intermittent extract.** Untouched, and confirmed already outside the balanced continuous totals
  (`PartFCalculator` sets `IsInBalancedFlow = false` and leaves `ContinuousDesignFlowRate_Lps` null for
  `CookerHoodExtractingOutside` and `SeparateIntermittentExtract`), so it was never part of what is applied.
- **`OverheatingScenario:v2`.** The stage still asserts `Openings Restricted`, so a NightClosed opening under
  `BasePassive` is still reported as `Incompatible` and still only warned about. Deliberately deferred.

---

## Iteration 1b / Base Natural Ventilation — licensed A/B acceptance (2026-08-26)

**The milestone:** *Iteration 1b / Base Natural Ventilation is proven end to end from an explicitly
prepared SAM dwelling, through authored opening behaviour, TAS simulation and comparable Part O / TM59
results, without inventing an MVHR system.*

The 2026-08-25 acceptance below proved one NV case. This one proves **two cases off one dwelling**, so a
result difference is attributable to the authored opening availability and to nothing else — and it runs
them through the explicit `PartOVentilationMode` route and `PartOIteration.BaseNaturalVentilation` rather
than the string gate and `BasePassive`. See [`PartO-ARCHITECTURE.md`](PartO-ARCHITECTURE.md) for the design
this validates.

### The two cases

| | NV-OPEN | NV-NIGHT |
|---|---|---|
| Base model | `C:\TasOut\v40\A0.sam` — 9-space TM59 residential, Flat 1/2/3 + Corridor | same |
| Part O route (stated) | `NV` → `PartOVentilationMode.NaturalVentilation` | same |
| Iteration | `BaseNaturalVentilation` | same |
| Authored opening | `Unrestricted` | `NightClosed`, openingHour 08, closingHour 23 |
| Aperture authored on | `SIM_EXT_GLZ` of `Bedroom 2_3`, guid `956264f2-…` | same aperture, same guid |
| Weather | `CIBSE Weather 2021.twd` | same |
| Run | `Sizing = true`, `Simulate = true`, days 1..365 | same |

Both cases are authored through **one code path** — the same `PartOOpeningProperties` constructor on the
same aperture, given the same 08/23 hours, differing only in the `OpeningRestriction`. Authoring one case
and leaving the other alone would have made this a comparison of two authoring routes rather than of two
opening availabilities.

Harness: `C:\TasOut\inv`, modes `partoauthor <in> <out> <report> NV "Bedroom 2_3" <OPEN|NIGHT>` →
`togbxml` → `workflowsim` → `partodump` → `partotm59` → `tsdcompare`. Outputs in `C:\TasOut\p1b`.

### Preparation, before TAS

Both cases:

```text
PREPARE|ventilationMode=NaturalVentilation|airflowApplication=SkipNaturalVentilation|successful=True|scenarios=4
SCENARIO|Flat 1 - BaseNaturalVentilation - NV   |scope=Dwelling   |key=6dfe14cb-6cf3-8798-a61a-d337dbc40878
SCENARIO|Corridor - BaseNaturalVentilation - NV |scope=CommonSpace|key=37242d48-94e8-8f66-9949-1746c7e5de04
SCENARIO|Flat 2 - BaseNaturalVentilation - NV   |scope=Dwelling   |key=1d7b7a5b-ca5b-8830-8a15-1904b8b99463
SCENARIO|Flat 3 - BaseNaturalVentilation - NV   |scope=Dwelling   |key=ae358ba0-3ad0-8637-a233-6bd199998654
```

Real Part F sizing ran first and sized 8 spaces on both, so there **was** something a mechanical
preparation would have applied. `SkipNaturalVentilation` is a decision not to use it, not an absence of
anything to use.

Diffing the two authoring reports gives exactly four differences, all of them the one aperture and its
consequences:

- `openingCompatibility` `Compatible` → `Incompatible`;
- two `NOTE` lines and one `WARNING` reporting that 1 of the model's 20 operable openings restricts while
  the stage assumes unrestricted operation — **reported, nothing changed**;
- `APERTURE|956264f2-…|restriction=Unrestricted|schedule=-` →
  `restriction=NightClosed|from=8|to=23|schedule=PartO_DayOpen_08_23|values=000000001111111111111110`.

Every Part F rate, every space's airflow state and all four scenario keys are byte-identical. **The
scenario keys being equal is correct, not an oversight**: opening behaviour is a property of the model, not
of the stage, so both cases are the same assessment of the same zones at the same iteration.

### Geometry reaching TAS

Both gbXML exports are 303 200 bytes and identical apart from the per-run `BuildingStorey` GUID and the
export timestamp. Opening availability does not travel through gbXML — it is applied to the TBD afterwards
by `Modify.SetApertureType` — so identical geometry is the expected result and confirms the two runs
simulate the same building.

### The opening profile in the produced TBDs

**NV-OPEN** — 2 shared aperture types across the model's 20 openings, neither carrying an availability
schedule:

```text
APERTURETYPE|element=Windows: SIM_EXT_GLZ -pane          |Cd=0.410591|profileType=ticFunctionProfile|function=zdwno,0,19.00,21.00,99.00|factor=1|schedule=-|values=-
APERTURETYPE|element=Windows: SIM_EXT_GLZ_AAF00869 -pane |Cd=0.477327|profileType=ticFunctionProfile|function=zdwno,0,19.00,21.00,99.00|factor=1|schedule=-|values=-
```

**NV-NIGHT** — the same two, plus one:

```text
APERTURETYPE|element=Windows: SIM_EXT_GLZ_E67C225E -pane |Cd=0.410591|profileType=ticFunctionProfile|function=zdwno,0,19.00,21.00,99.00|factor=1|schedule=PartO_DayOpen_08_23|values=000000001111111111111110
```

which is the required shape exactly: `profileType = ticFunctionProfile`,
`function = zdwno,0,19.00,21.00,99.00`, `schedule = PartO_DayOpen_08_23`,
`values = 000000001111111111111110`. Read out of the TBD through Tas's own 0-based accessors, so it
observes the file independently of the code that wrote it.

**The two TBD dumps differ by exactly that one line.** Every zone, internal condition, fresh-air rate,
ventilation profile factor and zone description is identical.

### No invented continuous MVHR supply or extract, in either case

The evidence a Part F continuous mechanical supply was carried onto a zone is the `SAMZoneMetadata`
decomposition in the TBD zone description, because TAS has no field of its own for it:

```text
NV-OPEN  / NV-NIGHT, every zone:
   DESC|[LevelName]=Level 0; [SAM_META_V1]={"ventilation":{"profile":false},"native":{"freshAirRate":8}}

CONTROL — the MVRE run of the same model (2026-08-25):
   DESC|[LevelName]=Level 0; [SAM_META_V1]={"ventilation":{"flow":0.0455,"flowPerArea":0,"flowPerPerson":0,"airChangesPerHour":0,"profile":false},"native":{"freshAirRate":0}}
```

Count of `"flow"` keys: **NV-OPEN 0, NV-NIGHT 0, MVRE control 8.** No `SupplyAirFlow`,
`SupplyAirFlowPerArea`, `SupplyAirFlowPerPerson` or `SupplyAirChangesPerHour` was stated on any zone in
either NV case.

`freshAirRate = 8` l/s/person is the **model's own authored TM59 outside-air rate**, preserved untouched —
and the control shows what applying the Part F rates does to it: the mechanical run drives it to 0 and
replaces it with `flow = 0.0455 m³/s`. Both NV cases keep the authored value.

Continuous mechanical extract: `InternalConditionParameter.ExhaustAirFlow` was never stated on any space
(`exhaustAirFlowStated=False` for all nine, both cases), and in any event `SAM.Analytical.Tas` has no TBD
write path for exhaust at all — see [`PartO-ARCHITECTURE.md`](PartO-ARCHITECTURE.md) §6.

### The two simulations genuinely differ

The TM59 verdicts agree (below), so this had to be checked rather than assumed: *"the two cases agree"* and
*"the opening schedule did nothing"* are very different findings. `tsdcompare` reads the same
resultant-temperature series the TM59 assessment reads, out of both TSDs:

| Space | Differing hours / 8760 | Max delta (K) | Mean open → night (°C) |
|---|---:|---:|---|
| **Bedroom 2_3** ← the authored space | 2 334 | **0.674** | 18.9723 → 18.9736 |
| Studio 1_0 | 4 633 | 0.142 | 18.8813 → 18.8813 |
| Kitchen_7 | 643 | 0.121 | 18.6995 → 18.6996 |
| Living Kitchen_4 | 2 137 | 0.069 | 19.0336 → 19.0340 |
| Bathroom_2 | 4 550 | 0.056 | 19.2985 → 19.2987 |
| Bedroom 2_6 | 443 | 0.040 | 18.9433 → 18.9433 |
| Ensuite_5 | 1 335 | 0.012 | 19.3607 → 19.3610 |
| Corridor_1 | 385 | 0.008 | 22.4341 → 22.4342 |
| Ensuite_8 | 230 | 0.006 | 19.2725 → 19.2726 |
| **Total** | **16 690 / 78 840** | | |

The largest single-hour difference in the whole model, by a factor of nearly five, is on **the one space
whose window was restricted**, and the night case is very slightly warmer there — the expected direction
for losing night purge. Every other space shows a small knock-on through inter-zone air movement. The
effect is small because only 1 of the flat's 20 openings was restricted and the `zdwno` function opens a
window only above 19–21 °C in any case.

### TM59 results

Both cases, from their own TSD, through `Tas.TSDQueryTM59Results`' sequence:

```text
COUNTS|naturalVentilation=5|mechanicalVentilation=0|corridor=4
```

No ventilation-strategy refusals, no association refusals, no map refusals, **zero mechanical results** —
the NV route was taken for every occupied space in both cases.

| | NV-OPEN | NV-NIGHT |
|---|---|---|
| Ventilation route | NaturalVentilation | NaturalVentilation |
| Iteration | BaseNaturalVentilation | BaseNaturalVentilation |
| Opening restriction | Unrestricted | NightClosed 08–23 |
| Mechanical supply | 0 | 0 |
| Continuous MVHR extract | 0 | 0 |
| NV results / mechanical / corridor | 5 / 0 / 4 | 5 / 0 / 4 |
| Bedrooms assessed (`…BedroomExtendedResult`) | Studio 1_0, Bedroom 2_3, Bedroom 2_6 | same three |
| Living spaces (`TM59NaturalVentilationExtendedResult`) | Living Kitchen_4, Kitchen_7 | same two |
| Corridor-criterion spaces | Corridor_1, Bathroom_2, Ensuite_5, Ensuite_8 | same four |
| Outcome | every space passes | every space passes |

Key metrics, `OccupiedHoursExceedingComfortRange` against `MaxExceedableHours` (criterion 1) and
`NightHoursNumberExceeding26` against `AnnualMaxExceedableNightHours` (criterion 2):

| Space | Criterion 1, open | Criterion 1, night | Criterion 2, open | Criterion 2, night |
|---|---|---|---|---|
| Studio 1_0 | 4 / 262 | 4 / 262 | 0 / 32 | 0 / 32 |
| Bedroom 2_3 | 0 / 262 | 0 / 262 | 0 / 32 | 0 / 32 |
| Bedroom 2_6 | 0 / 262 | 0 / 262 | 0 / 32 | 0 / 32 |
| Living Kitchen_4 | 2 / 142 | 2 / 142 | n/a | n/a |
| Kitchen_7 | 0 / 142 | 0 / 142 | n/a | n/a |

**The two cases produce the same verdicts, and that is a truthful result rather than a null one.** This
dwelling is nowhere near its comfort limits under Leeds TRY — 0–4 exceeding hours against allowances of
110–262 — so a 0.67 K night-time change on one room does not move any criterion. No expected numerical
delta was manufactured; what is claimed is what the acceptance was defined to claim: the two simulations
are identical except for the authored opening availability, both are assessed as Natural Ventilation, and
the result pipeline returns truthful comparable results for both.

### What changed between the two cases, in full

1. The `OpeningRestriction` on one aperture, and the `PartO_DayOpen_08_23` schedule derived from it.
2. The opening-compatibility report about (1) — a warning, acted on by nothing.
3. One additional TBD `ApertureType` carrying that schedule.
4. Consequently, the simulated hourly temperatures.

Nothing else. Same base model, same Part F sizing, same route, same iteration, same scenario keys, same
weather, same simulation period, same geometry, same internal conditions, same zone ventilation.

### What this acceptance does not cover

- **Iteration 1a (`BaseMVHR`) equipment selection.** The Part F requirement is applied on the MVHR route,
  but no physical MVHR unit is selected against it. Not implemented.
- **Iterations 2 and 3.** Acoustic restriction, summer bypass, boost, active cooling, manufacturer supply
  temperature and larger-unit selection are recorded in the architecture and not implemented.
- **Mixed-route models refuse.** Per-zone airflow application is not implemented.
- **System 1 sizing.** Background/trickle ventilator and purge provision for a naturally ventilated
  dwelling are calculated nowhere. `PartFCalculator` remains System 4 shaped for every route.
- **Wet-room intermittent extract runtime behaviour.** Preserved as data, deliberately not modelled — SAM
  has the Table 1.1 rate but no operating schedule, and TAS has no exhaust write path. Architecture §6.
- **A case that actually fails.** Both cases pass comfortably on this model and this weather, so the
  result pipeline has been shown to report a pass truthfully and has not been shown to report a fail.
- **`OverheatingScenario:v2`.** The stage still asserts `Openings Restricted`, so NV-NIGHT is still
  reported `Incompatible` and still only warned about, and the two cases still share a scenario key.

---

## Remaining validation work

In rough order of value:

1. **A second real model, and specifically one containing a plain non-bedroom naturally-ventilated room.**
   Rerun and diff against its own TAS Domestic Overheating report the same way. This closes the largest
   gap and covers the least-covered criterion branch.
2. **Investigate before concluding.** If a new discrepancy appears, treat it the way the two Flat1
   findings were treated: both looked like defects on first read and one of them was not. The corridor
   "discrepancy" turned out to be intentional occupancy-independent behaviour.
3. **Settle the TPD-approx identity question against the real TPD** in `…\SAM_daily\2027-08-03-HVAC\` —
   is `SystemSpaceResult.Reference` the same string as the TBD/TSD zone guid? If not, `IZoneLoad.GUID` is
   the probable correction, with the JSON-persistence consequence checked first.
4. **Validate `AcousticRestricted` / `ActiveTrimCooling`** once their operating assumptions are confirmed.
   Until then, the only thing worth confirming on a live canvas is that
   `Query.PartOIterationOperatingMode`'s **refusal fires correctly** — which is not the same exercise as
   the BasePassive comparison and must not be mistaken for it.
5. **Exercise the unresolved-space refusal path live**, so "nothing was silently dropped" rests on
   observed behaviour rather than on unit tests plus a run that never triggered it.
6. **Decide the design-side `ZoneGuid` provenance** — preserve the guid through `Modify.UpdateIds`'s
   strip, or refuse an ambiguous name match — so the whole identity chain, not just its simulated half,
   is free of a name join.
7. **CLOSED — the "N Bed Apt." token collision and the related "room"/"bedroom" substring collision in
   `TM59Manager`'s Sleeping/Living/Cooking matching are both fixed**, for Kitchen, Living Room and Living
   Room/Kitchen apartment conditions (`TM59Manager.RoleMatchName` and `TM59Manager.IsRole`, the latter
   routing InternalCondition role matching through `Query.TM59TextMapMatches` instead of
   `TextMap.GetSortedKeys` — see [Known scope/classification differences](#known-scopeclassification-differences)
   item 4). A second real model containing a plain non-bedroom naturally-ventilated Living Room (item 1
   above) remains valuable as live-data confirmation, but is no longer needed to close this defect - it is
   confirmed end to end against a synthetic fixture by
   `TM59AssessmentCalculatorTests.ApartmentLivingRoomCondition_IsNotRoutedAsABedroom_SoCriterion2IsNotApplicable`
   and pinned by `TM59SpaceApplicationClassificationTests`.

---

## Iteration 1a / Base MVHR — licensed acceptance (2026-08-26): PARTIAL

> **Superseded.** The block recorded here was resolved on 2026-08-27 — see
> [Iteration 1a / Base MVHR — the block resolved](#iteration-1a--base-mvhr--the-block-resolved-2026-08-27)
> below. The cause was not the inter-zone air movement record this section suspected; it was conservation.
> The evidence of the design chain in this section stands.

**Read this section's verdict first: the design chain is proven end to end into the TBD, and the annual
simulation of that TBD is BLOCKED.** The block is not in the Iteration 1a chain; it is a pre-existing
property of `SAM.Analytical.Tas.Modify.UpdateIZAMs`, and the evidence below isolates it to one line of the
topology.

| | Value |
|---|---|
| Base model | `C:\TasOut\v40\A0.sam` — the same 9-space TM59 residential model the Iteration 1b acceptance used |
| Weather | `C:\Users\Public\Documents\Tas Data\Databases\CIBSE Weather 2021.twd` |
| Outputs | `C:\TasOut\p1a` |
| A | `partoauthor … MVRE "Bedroom 2_3" OPEN` → `PreparePartOIteration(BasePassive)` |
| B | `partoauthor … NV "Bedroom 2_3" OPEN` → `PreparePartOIteration(BaseNaturalVentilation)` — the accepted 1b baseline |

### What the preparation produced — the directional realization

Ten design terminals across eight sized spaces, one generic system, one generic unit, and a system duty
that agrees with the Approved Document F requirement:

```text
DESIGN|terminals=10|system=MVHR 1|ahu=MVHR-01|supplyDuty_lps=156|extractDuty_lps=156

SPACEDUTY|Bathroom_2        |supply_lps=-    |extract_lps=8
SPACEDUTY|Bedroom 2_3       |supply_lps=45.5 |extract_lps=-
SPACEDUTY|Bedroom 2_6       |supply_lps=45.5 |extract_lps=-
SPACEDUTY|Ensuite_5         |supply_lps=-    |extract_lps=8
SPACEDUTY|Ensuite_8         |supply_lps=-    |extract_lps=8
SPACEDUTY|Kitchen_7         |supply_lps=-    |extract_lps=44
SPACEDUTY|Living Kitchen_4  |supply_lps=32.5 |extract_lps=44
SPACEDUTY|Studio 1_0        |supply_lps=32.5 |extract_lps=44
```

**A supplied room is not extracted and an extracted room is not supplied.** 45.5 + 45.5 + 32.5 + 32.5 = 156
l/s supply; 8 + 8 + 8 + 44 + 44 + 44 = 156 l/s extract. The system balances; the rooms do not, and the two
rooms that carry both roles — the studio and the open plan living kitchen — carry both because Approved
Document F gives them both. Before this milestone every direction was derived from the space's supply
figure, so each of those six wet rooms would have been *supplied* and each bedroom *extracted*.

### What reached the TBD — read back through Tas's own accessors

`partoizam` walks `Building.GetIZAM(i)`, `IZAM.GetSourceZone()` and `IZAM.GetTargetZone(j)`, so it observes
the file independently of the code that wrote it:

```text
ZONES|10|… , MVHR-01, …                       <- the unit has its own TAS zone
IZAMS|11

IZAM|IZAM MVHR-01 FROM OUTSIDE  |fromOutside=-1|source=-              |targets=MVHR-01       |airFlow_m3s=0.156
IZAM|IZAM MVHR-01 TO Bedroom 2_3|fromOutside=0 |source=MVHR-01        |targets=Bedroom 2_3   |airFlow_m3s=0.0455
IZAM|IZAM MVHR-01 TO Bedroom 2_6|fromOutside=0 |source=MVHR-01        |targets=Bedroom 2_6   |airFlow_m3s=0.0455
IZAM|IZAM MVHR-01 TO Living Kitchen_4         |source=MVHR-01         |targets=Living Kitchen_4|airFlow_m3s=0.0325
IZAM|IZAM MVHR-01 TO Studio 1_0               |source=MVHR-01         |targets=Studio 1_0    |airFlow_m3s=0.0325
IZAM|IZAM Bathroom_2 TO MVHR-01 |fromOutside=0 |source=Bathroom_2     |targets=MVHR-01       |airFlow_m3s=0.008
IZAM|IZAM Ensuite_5 TO MVHR-01                |source=Ensuite_5       |targets=MVHR-01       |airFlow_m3s=0.008
IZAM|IZAM Ensuite_8 TO MVHR-01                |source=Ensuite_8       |targets=MVHR-01       |airFlow_m3s=0.008
IZAM|IZAM Kitchen_7 TO MVHR-01                |source=Kitchen_7       |targets=MVHR-01       |airFlow_m3s=0.044
IZAM|IZAM Living Kitchen_4 TO MVHR-01         |source=Living Kitchen_4|targets=MVHR-01       |airFlow_m3s=0.044
IZAM|IZAM Studio 1_0 TO MVHR-01               |source=Studio 1_0      |targets=MVHR-01       |airFlow_m3s=0.044
```

Every airflow equals that room's design-terminal duty ÷ 1000, the unit draws its whole 156 l/s supply duty
from outside, and no room carries a movement in a direction it has no terminal for. **The design chain is
proven into the file.**

`freshAirRate` is 0 on every sized zone, as it was in the 2026-08-25 MVRE control — the Part F application
clears the per-person basis. What is different is that the air the cleared basis used to represent is now
actually delivered, as inter-zone air movements, rather than being absent from the simulation entirely.

### What is blocked — the annual simulation

`Simulation Failed`, at every simulated day range including 1..2. Four causes were ruled out by experiment,
each on the same built file:

| Experiment | Result |
|---|---|
| Same prepared model, `AddIZAMs = false` | **Simulates** — full year, 4.8 MB TSD |
| Unit states no supply temperature, so no NaN thermostat setpoint is written | Still fails |
| Unit's zone excluded from sizing (`tbdNoSizing`) | Still fails |
| Day range 1..2 instead of 1..365 | Still fails |
| **Unit's zone present, every IZAM removed** | **Simulates** — full year, 5.0 MB TSD |
| Only the outside-air IZAM removed | Still fails |

So the unit's zone is fine and **the zone-to-zone inter-zone air movements are what TAS refuses**. The
supply half alone fails, so it is not the extract direction this milestone added.

**Why this was never seen before.** `Modify.AddAirMovementObjects(AnalyticalModel)` reads
`AnalyticalModel.AdjacencyCluster`, which returns a **copy** — so the objects and relations it creates have
never reached a caller's model, and `Modify.UpdateIZAMs` has therefore never had anything to write. No SAM
model has previously carried a `SpaceAirMovement` into a workflow. Iteration 1a is the first work to
exercise that path, and it has found it non-functional. The two overloads are documented accordingly and
the in-place `AdjacencyCluster` overload is what the Part O preparation calls.

**Next step for this block**, in order of expected value: compare the IZAM records SAM writes against one
TAS itself authors in the Building Simulator on the same file — profile type, day-type assignment, and
whether the flow belongs in `profile.factor` (where `Modify.Update` puts it, leaving `profile.value` at 1)
or in `profile.value`. Every SAM-written IZAM here is `ticValueProfile`, `value = 1`, `factor = the flow`,
and is assigned the calendar day types only, `HDD`/`CDD` removed.

### Regression: Iteration 1b is untouched

`partoauthor … NV … OPEN` on the same base model reports `DESIGN|terminals=0|system=-|ahu=-`, no air
movements, `airflowApplication=SkipNaturalVentilation`, four scenarios — the Iteration 1b behaviour
recorded above, unchanged. The natural ventilation route builds none of the Base MVHR topology, so nothing
in that path can reach `UpdateIZAMs`.

### Harness additions

`partoizam <tbd> <out.txt>` — every zone's volume, floor area, surface count, sizing flags and internal
conditions, and every inter-zone air movement with its source, targets and flow.
`partosim <tbd> <tsd> <dayFrom> <dayTo> [stripIZAMsContaining]` — simulate an already-built file, optionally
removing matching air movements first; `INV_NOSIZE_ZONE` clears one zone's sizing flags and `INV_NO_IZAM=1`
turns the workflow's IZAM step off. These are the bisect tools the table above was produced with.

### TM59 takes the mechanical route

Evidenced on the IZAM-free simulation of the **1a-prepared** model, because the criterion is selected from
the scenario and not from the air movements:

```text
1b (NV) : COUNTS|naturalVentilation=5|mechanicalVentilation=0|corridor=4
1a (MVRE): COUNTS|naturalVentilation=0|mechanicalVentilation=5|corridor=4

MECHANICALVENTILATION|Studio 1_0      |type=TM59MechanicalVentilationExtendedResult|pass=True
MECHANICALVENTILATION|Bedroom 2_3     |type=TM59MechanicalVentilationExtendedResult|pass=True
MECHANICALVENTILATION|Bedroom 2_6     |type=TM59MechanicalVentilationExtendedResult|pass=True
MECHANICALVENTILATION|Living Kitchen_4|type=TM59MechanicalVentilationExtendedResult|pass=True
MECHANICALVENTILATION|Kitchen_7       |type=TM59MechanicalVentilationExtendedResult|pass=True
```

All five assessed dwelling spaces flip from the natural-ventilation criterion to the mechanical one, the
four corridor results are unchanged, and **no space was refused**. The 1b column is the Iteration 1b
acceptance figure, unchanged.

### The finding that justifies the whole milestone

`tsdcompare` over the 1b run and the IZAM-free 1a run, on the same resultant-temperature series the
assessment reads:

```text
TOTAL|values=78840|differing=0
```

**Zero.** Not one of 78 840 hourly temperatures differs between a dwelling prepared as Base MVHR and the
same dwelling prepared as naturally ventilated — once the inter-zone air movements are taken away. The
Approved Document F rate written onto the internal conditions changes nothing thermally: TAS's
`freshAirRate` is the Outside Air field and does not itself supply the zone, and no SAM Ventilation profile
is assigned so no `ticV` rate is written either.

So before Iteration 1a, **`MVHR + BasePassive` and `NV + BaseNaturalVentilation` produced numerically
identical simulations of the same building** — the mechanical route was a thermal no-op that reported
success, while the cleared per-person basis quietly removed the authored 8 l/s/person outside-air rate. The
inter-zone air movements are the only thing that makes the two routes differ, which is precisely what the
block above prevents from being demonstrated.

---

## Iteration 1a / Base MVHR — the block resolved (2026-08-27)

**The PARTIAL section above is superseded from its "Next step for this block" paragraph onward.** The
diagnosis in it was wrong in an instructive way: the `profile.factor` / `profile.value` question it named
turned out not to matter at all, and the cause was not in the inter-zone air movement record.

### The root cause

**TAS refuses to simulate a TBD in which any one zone's inter-zone air movements do not balance.** The
EDSL documentation states it as *"any air flow imbalance will be reported as a 'Max Pressure Exceeded'
error"*, and the EDSL FAQ is explicit that an inter-zone air movement into a zone needs a second one taking
the same rate out again. SAM never sees that wording, because
`SAM.Analytical.Tas.Modify.Simulate` returns **true** regardless — it only waits for the TSD to unlock —
so a failed run reports success and the only evidence is `Simulation Failed` in
`<tbd-basename>_error_log.txt`.

Every room of a balanced heat recovery dwelling is individually out of balance, because the design
balances at the system. Iteration 1a was therefore refused on every room at once.

### The experiments, on `C:\TasOut\izam`

| Experiment | Result |
|---|---|
| `example.tbd` from `Tas Data\Sample Projects`, a **TAS-authored** file with three inter-zone air movements | **Simulates** — the control |
| The same file with all three stripped and re-written through `Building.AddIZAM` | **Simulates** — so the COM-created record is not at fault |
| The 1a file with the flow moved from `profile.factor` to `profile.value`, TAS's own convention | Still fails |
| …additionally with `profile.units` and the profile names cleared to match TAS's | Still fails |
| ONE movement, outside → the unit's zone, on a file that simulates | **Fails** |
| ONE movement, outside → an ordinary room | **Fails** |
| ONE movement, room → the unit's zone | **Fails** |
| ONE movement, room → outside | **Fails** |
| One movement, all four calendar day types including `HDD` | **Fails** |
| ONE movement in **and** one out of the same room | **Simulates** |
| The whole 1a topology plus the unit's exhaust — balanced over the **building**, not room by room | **Fails** |
| The whole 1a topology plus the exhaust plus per-room balancing | **Simulates**, full year |

So it is not the record, not the topology, not the day types and not the unit's zone. It is conservation,
**per zone**, and building-wide balance is not enough.

### The TAS-native representation

`TBD.IIZAM` exposes a source zone, target zones and `fromOutside` — and **no outward flag of any kind**. A
late-bound probe against the live COM object confirms the UI's "To Outside" field is not reachable through
automation under that name or any obvious variant. The three shapes are:

| Meaning | Assigned zone | `SetSourceZone` | `fromOutside` |
|---|---|---|---|
| outside → zone | the zone | none | `1` (reads back `-1`) |
| zone A → zone B | B | A | `0` |
| **zone → outside** | the zone | **none** | `0` |

The third is not inferred from member names: `example.tbd`'s TAS-authored *"From Atrium to Outside"* has
exactly that shape, and re-creating it through `Building.AddIZAM` keeps that file's atrium balanced and
simulating — which it could not be if the movement were inert.

### `profile.factor` versus `profile.value`

**Both work.** TAS-authored files put the flow in `value` and leave `factor` at 1; SAM puts it in `factor`
and leaves `value` at 1; the same balanced topology simulates either way. No production change was made,
and the earlier suspicion is closed.

Two real defects were found while ruling this out, neither introduced by Part O and neither fixed here:
the stored flow is read by TAS as a **mass** flow in kg/s (`profile.units` defaults to `kg/s`) while SAM
writes a volume flow in m³/s, and `Modify.Simulate` cannot report a failed simulation.

### Supply temperature is not required

The Base MVHR unit states **no** supply temperature in either season, so no thermostat setpoint is written
for its zone, and the balanced model simulates a full year. Nothing needed to be restored, and no 23 °C
tempering was introduced.

### The production fix

| File | Change |
|---|---|
| `Modify/AddPartFTransferAirMovements.cs` (new) | Routes each space's net (supply less extract) through `PartFAirflowNetwork` — the same network Approved Document F paragraph 1.25 is assessed over — and writes one `SpaceAirMovement` per loaded internal connection. Refuses, naming the rooms, where a net cannot be routed. |
| `Query/AirMovementResidual.cs` (new) | Net at every node, summed over every movement touching it, with the unit's outside intake counted the way the TBD writer derives it. |
| `Modify/AddAirMovementObjects.cs` | Adds the unit's exhaust: a movement from the unit to a destination of `null`, sized from the extract movements. Nothing is added where there is no extract, so the no-terminal branch is unchanged. |
| `Modify/PreparePartOIteration.cs` | Calls the transfer step, then refuses on any node that does not balance. |
| `Query/AirFlow.cs` | The unit's intake counts only what it delivers somewhere, so the exhaust is not counted as intake as well. |
| `SAM_Tas Modify/UpdateIZAMs.cs` | Writes the unit's outward movements as inter-zone air movements on its zone with no source zone and `fromOutside = 0`. The `To`-endpoint change of `4f70f08d` is **kept**: it is what makes a room → unit extract an air movement on the unit's zone, and it matches the TAS-authored control. |

Fixtures that were bags of loose rooms — no internal separating elements at all — now carry partitions.
That is not a workaround: a dwelling whose bedroom and bathroom share no element genuinely cannot move
transfer air, and the preparation is right to refuse it.

### The licensed acceptance — Iteration 1a vs Iteration 1b (2026-08-27)

Outputs in `C:\TasOut\p1a2`. Same base model, weather, gains and period as the Iteration 1b acceptance:
`C:\TasOut\v40\A0.sam`, `CIBSE Weather 2021.twd`, days 1..365.

| | A — Iteration 1a | B — Iteration 1b |
|---|---|---|
| Authored | `partoauthor … MVRE "Bedroom 2_3" OPEN` | `partoauthor … NV "Bedroom 2_3" OPEN` |
| Iteration | `BasePassive` | `BaseNaturalVentilation` |
| Simulation | **Full year, 5 608 411 byte TSD, no error log** | Full year, 4 817 517 byte TSD |

**The unit's zone exists and its connections work.** Twenty inter-zone air movements read back through
Tas's own accessors — one intake, four supply, six extract, one exhaust, eight transfer:

```text
IZAM MVHR-01 FROM OUTSIDE      |fromOutside=-1|source=-              |targets=MVHR-01     |0.156
IZAM MVHR-01 TO Bedroom 2_3    |fromOutside=0 |source=MVHR-01        |targets=Bedroom 2_3 |0.0455
IZAM Bathroom_2 TO MVHR-01     |fromOutside=0 |source=Bathroom_2     |targets=MVHR-01     |0.008
IZAM Bedroom 2_3 TO Bathroom_2 |fromOutside=0 |source=Bedroom 2_3    |targets=Bathroom_2  |0.008
IZAM MVHR-01 TO OUTSIDE        |fromOutside=0 |source=-              |targets=MVHR-01     |0.156
```

Every supply movement equals that room's Supply terminal duty ÷ 1000 and every extract movement its
Extract terminal duty ÷ 1000; **no bedroom is extracted and no wet room is supplied**. The system totals
balance at **156 l/s supply and 156 l/s extract**, and the intake and exhaust carry exactly those.

**Every zone conserves**, read off the file rather than asserted:

| Zone | In | Out |
|---|---|---|
| MVHR-01 | 0.156 outside + 0.156 extract | 0.156 supply + 0.156 exhaust |
| Bedroom 2_3 | 0.0455 supply | 0.008 + 0.026 + 0.0115 transfer |
| Bedroom 2_6 | 0.0455 supply + 0.0105 transfer | 0.004 + 0.052 transfer |
| Studio 1_0 | 0.0325 supply + 0.0115 transfer | 0.044 extract |
| Living Kitchen_4 | 0.0325 supply + 0.026 transfer | 0.0105 + 0.004 transfer + 0.044 extract |
| Kitchen_7 | 0.052 transfer | 0.008 transfer + 0.044 extract |
| Bathroom_2 | 0.008 transfer | 0.008 extract |
| Ensuite_5 | 0.004 + 0.004 transfer | 0.008 extract |
| Ensuite_8 | 0.008 transfer | 0.008 extract |

Note the shape: `Bedroom 2_3` divides its supply three ways, `Ensuite_5` draws from two rooms, and
`Living Kitchen_4` receives transfer air, passes transfer air on and extracts, all at once. Flows split and
recombine, and no movement has a matching partner — which is why conservation is summed at each zone.

**The mechanical ventilation is not the Outside Air field.** Every sized space reads
`freshAirRate_lsp=0`; only `Corridor_1`, which is outside the dwelling and carries no terminal, keeps its
authored 8 l/s/person.

**TM59 takes the mechanical route, with no strategy refusals:**

```text
1a (MVRE): COUNTS|naturalVentilation=0|mechanicalVentilation=5|corridor=4
1b (NV)  : COUNTS|naturalVentilation=5|mechanicalVentilation=0|corridor=4
```

The unit's zone is reported as `ASSOCREFUSAL|Simulated space 'MVHR-01' does not resolve to exactly one
design space` — correct, and required: the unit's zone is plant, not a room, and must not be assessed as
one.

### The number that decides it

`tsdcompare` over the two runs, on the same resultant-temperature series the assessment reads:

```text
TOTAL|values=78840|differing=78835
```

**78 835 of 78 840.** On 2026-08-26 the same comparison, with the inter-zone air movements absent, gave
`differing=0` — the mechanical route was a thermal no-op that reported success. Iteration 1a now changes
the simulated building: the dwelling runs 0.4–0.9 K cooler in the mean in every room, up to 3.1 K at an
individual hour.

`MISSING|MVHR-01` in the comparison is expected — the unit's zone exists in the 1a file and not in the 1b
one.

### Regression: the Iteration 1b OPEN / NIGHT A/B is unchanged

The accepted 2026-08-26 natural-ventilation A/B, re-run on the same base model and weather after the
production change:

```text
RESULT|1b OPEN |SIMULATED
RESULT|1b NIGHT|SIMULATED
TOTAL|values=78840|differing=16690
```

**16 690** — the same count as the accepted run. The natural ventilation route builds no design terminals,
no system and no air movements, so nothing on it reaches the transfer-air step or the balance check.

### One interop note

`TBD.profile.schedule`, read off an inter-zone air movement's profile, **hangs the process** — no
exception, no timeout, both the harness and the out-of-process `TBD.exe` sitting at idle. `factor`,
`value`, `type`, `units`, `function`, `setbackValue`, `hourlyValues` and `GetExtremeValue` all read
normally. Nothing in SAM reads it, and the diagnosis did not need it.

A TBD is a binary document with no XML or text serialization reachable from the automation interface, so
every field above is read through the COM accessors rather than off the file.

---

## Iteration 1a / Base MVHR — the two magnitude and scope corrections (2026-08-27)

The section above stands: the topology, the balance rule and the acceptance it records are unchanged. Two
things in it were wrong in ways that did not stop a simulation, and it named both as deferred defects.
Both are corrected here.

### Correction 1 — the transfer network was solved over the served spaces, not over the dwelling

`Modify.AddPartFTransferAirMovements` took its space set from the `VentilationSystem` → `Space` relation.
That relation is right about what it says: it holds the spaces that carry a **design ventilation
terminal**, and an air handling unit that moves no air into a hall does not serve that hall. It is the
wrong answer to a different question.

Approved Document F sizes no terminal for circulation, so the internal hall of a real flat is a
zero-terminal space — and paragraph 1.25's transfer air crosses it. A bedroom supplied with 20 l/s passes
its air into the hall, and the hall divides it between the bathroom and the kitchen that are extracted:

```text
AHU → Bedroom A     20        Bedroom:  20 in = 20 out
Bedroom A → Hall    20        Hall:     20 in = 8 + 12 out
Hall → Bathroom      8        Bathroom:  8 in =  8 out
Hall → Kitchen      12        Kitchen:  12 in = 12 out
Bathroom → AHU       8
Kitchen  → AHU      12
```

Solved over the served spaces alone the hall is not in the graph, the middle of every route through it is
deleted, and the preparation **refuses** — *"The design airflow of Bathroom, Bedroom 1, Kitchen cannot
reach anywhere it could come from or go to"* — on a dwelling that is modelled perfectly correctly. That is
the failure `PartFTransferAirDwellingScopeTests` reproduces with the fix removed: 5 of its 6 tests fail.

**The boundary is asked for, never guessed at.** The fix is not "add every space in the model": a communal
corridor belongs to no dwelling, and letting the solver route through one would carry a flat's supply air
into the common parts, or use the corridor as a shortcut between two flats that share nothing but a wall.

`Query.PartFTransferAirSpaces` (new) settles it from the **existing** authority —
`Query.PartFDwellingZones`, which is the single source of the dwelling-selection policy and what
`PartFCalculator` itself sizes with. The scope is the served spaces plus every other space of the
**dwelling zones those served spaces belong to**; membership is the model's own `Zone` → `Space` relation,
and nothing is inferred from geometry. A zone marked `Is Dwelling = No` — a communal corridor, a stair, a
landlord area — contributes nothing. A model with no zones at all is one dwelling, which is exactly what
`PartFCalculator.Calculate()` does with a zone-less model, and a note says so.

Two consequences, both deliberate:

- **The system relation is unchanged.** The hall is still not related to the `VentilationSystem`, still
  carries no terminal, and is still not claimed as mechanically served. It is a transfer node, and being a
  transfer node is not being served.
- **The balance check and the stale-movement removal now run over the dwelling too.** A transfer movement
  is related to the space it *arrives* in, so the one arriving in a zero-terminal hall is related to that
  hall alone: checking only the served spaces would pass a model TAS refuses, and removing only the served
  spaces' movements would write a second transfer beside the first on the next preparation.

### Correction 2 — a TBD inter-zone air movement is a mass flow, and SAM was writing a volume flow

The previous section recorded this as a defect found and not fixed. The EDSL Building Simulator
documentation states the Inter-Zone Air Movement flow rate as a time-varying **mass flow rate**, and its
Inter-Zone Air Movement table gives the unit explicitly as **kg/s**. A licensed TBD agrees: read back
through Tas's own accessors, the profile SAM writes reports `units=kg/s`.

SAM is volumetric all the way down — Approved Document F sizes a terminal in l/s and
`SpaceAirMovement.AirFlow` carries m³/s — and neither type says which it is. So the m³/s number went
straight into the kg/s field. Nothing failed: the model still balanced, still simulated and still produced
a full year of results, for a dwelling ventilated about 21% below its design.

```text
Approved Document F duty      45.5 l/s
SpaceAirMovement.AirFlow      0.0455 m3/s        (unchanged - SAM stays volumetric)
TBD IZAM profile              0.055055 kg/s      (0.0455 x 1.21)
```

**Where the conversion is.** In `SAM_Tas`, at the one seam where a SAM movement becomes a TBD profile:
`Modify.UpdateIZAMProfile`, which every inter-zone air movement `Modify.UpdateIZAMs` writes now goes
through — outside → unit, unit → space, space → space transfer, space → unit extract, and the unit's
exhaust. Nothing in `SAM` changed, and no Part F requirement or design terminal duty is restated in kg/s.

**The density is SAM's own.** `Query.IZAMAirDensity_KgPerM3` is `Core.FluidProperty.Air.Density`, which is
**1.210 kg/m³** — the value `Modify.AddAirMovementObjects` already writes as an air handling unit's density
profile and `SAMAnalyticalCreateIZAMBySetPoint` already offers as its default. A second constant was
deliberately *not* minted to obtain the 1.204 kg/m³ of dry air at 20 °C and sea level: the two would then
disagree by half a percent about the same air, for no reason anybody could later reconstruct.

**One density for the whole network, and that is not an approximation of convenience.** Air expands as it
warms, so a physically exact conversion would use each movement's own air temperature — and the movements
would then no longer balance by mass at any node, which is precisely what TAS refuses. Scaling every term
of every node's sum by the same factor turns a balanced volumetric network into a balanced mass network
exactly.

### The corrected licensed acceptance (2026-08-27)

Outputs in `C:\TasOut\p1a3`. Same base model, weather, gains and period as before: `C:\TasOut\v40\A0.sam`,
`CIBSE Weather 2021.twd`, days 1..365.

| | A — Iteration 1a | B — Iteration 1b |
|---|---|---|
| Authored | `partoauthor … MVRE "Bedroom 2_3" OPEN` | `partoauthor … NV "Bedroom 2_3" OPEN` |
| Iteration | `BasePassive` | `BaseNaturalVentilation` |
| Simulation | **Full year, 5 594 029 byte TSD, no error log** | Full year, 4 816 394 byte TSD |

**The dwelling scope, reported by the preparation itself:**

```text
NOTE|The dwelling transfer air is routed over the 8 space(s) the system serves. No further internal
     space belongs to the dwelling(s) they are in, so nothing was added to the network.
```

A0's three dwelling zones — `Flat 1`, `Flat 2`, `Flat 3` — contain only rooms that Approved Document F
sized, so this model has no zero-terminal internal space to add. Its one zero-terminal space,
`Corridor_1`, is in the `Corridor` zone, which the model marks `Is Dwelling = No`; it is a TAS zone in the
file and **carries no inter-zone air movement at all**. The hall case is proven separately — by
`PartFTransferAirDwellingScopeTests`, and on this same licensed model by the `INV_HALLDEMO` probe below.

**Every flow is now written in kg/s, and converts back to the design duty:**

```text
IZAM MVHR-01 FROM OUTSIDE     |fromOutside=-1|source=-             |massFlow_kgs=0.18876 |= 0.156  m3/s = 156.0 l/s
IZAM MVHR-01 TO Bedroom 2_3   |source=MVHR-01|targets=Bedroom 2_3  |massFlow_kgs=0.055055|= 0.0455 m3/s =  45.5 l/s
IZAM MVHR-01 TO Studio 1_0    |source=MVHR-01|targets=Studio 1_0   |massFlow_kgs=0.039325|= 0.0325 m3/s =  32.5 l/s
IZAM Studio 1_0 TO MVHR-01    |source=Studio 1_0|targets=MVHR-01   |massFlow_kgs=0.05324 |= 0.044  m3/s =  44.0 l/s
IZAM Bedroom 2_3 TO Bathroom_2|source=Bedroom 2_3|targets=Bathroom_2|massFlow_kgs=0.00968|= 0.008  m3/s =   8.0 l/s
IZAM MVHR-01 TO OUTSIDE       |fromOutside=0 |source=-             |massFlow_kgs=0.18876 |= 0.156  m3/s = 156.0 l/s
                                                                    profileUnits=kg/s  (TAS's own declaration)
```

Twenty movements — one intake, four supply, six extract, one exhaust, eight transfer. Every supply
movement converts back to that room's Supply terminal duty and every extract to its Extract duty; **no
bedroom is extracted and no wet room is supplied**; the system totals are still **156 l/s supply and
156 l/s extract** volumetrically, which is 0.18876 kg/s each way.

**Every node conserves MASS**, read off the file rather than asserted — summed over every movement
touching the zone, never paired route against route:

```text
NODE|Bathroom_2      |in_kgs=0.00968 |out_kgs=0.00968 |residual_kgs=0
NODE|Bedroom 2_3     |in_kgs=0.055055|out_kgs=0.055055|residual_kgs=0
NODE|Bedroom 2_6     |in_kgs=0.06776 |out_kgs=0.06776 |residual_kgs=0
NODE|Ensuite_5       |in_kgs=0.00968 |out_kgs=0.00968 |residual_kgs=0
NODE|Ensuite_8       |in_kgs=0.00968 |out_kgs=0.00968 |residual_kgs=0
NODE|Kitchen_7       |in_kgs=0.06292 |out_kgs=0.06292 |residual_kgs=-0
NODE|Living Kitchen_4|in_kgs=0.070785|out_kgs=0.070785|residual_kgs=-0
NODE|MVHR-01         |in_kgs=0.37752 |out_kgs=0.37752 |residual_kgs=0
NODE|Studio 1_0      |in_kgs=0.05324 |out_kgs=0.05324 |residual_kgs=-0
CONSERVATION|nodes=9|density_kgm3=1.21|maxResidual_kgs=0|maxResidual_lps=0.000005
```

Nine nodes; `Corridor_1` is a tenth zone and is absent, which is the scope boundary holding. The largest
residual anywhere is 5 × 10⁻⁶ l/s, which is the single-precision rounding of the TBD profile field.
`Bedroom 2_3` still divides its supply three ways, `Ensuite_5` still draws from two rooms, and
`Living Kitchen_4` still receives transfer air, passes transfer air on and extracts at once.

**Unchanged from the accepted run**: every sized space reads `freshAirRate_lsp=0` (only `Corridor_1`, which
is outside the dwelling, keeps its authored 8 l/s/person), and TM59 takes the mechanical route with no
strategy refusals:

```text
1a (MVRE): COUNTS|naturalVentilation=0|mechanicalVentilation=5|corridor=4
1b (NV)  : COUNTS|naturalVentilation=5|mechanicalVentilation=0|corridor=4
```

### The zero-terminal transfer node, on the licensed model (`INV_HALLDEMO` probe)

A0.sam has no zero-terminal space *inside* a dwelling, so the acceptance run above cannot show one. Its one
zero-terminal space, `Corridor_1`, is in a zone the model marks `Is Dwelling = No` — which is exactly why
the acceptance run routes nothing through it. The `INV_HALLDEMO` harness probe relates `Corridor_1` to the
`Flat 1` dwelling zone and changes nothing else, so both halves of the scope rule can be read off the same
licensed model and the same production code. **A probe, not part of the acceptance**: it alters what the
model says about the building. Sizing only — the question is what is written into the TBD.

```text
hall_out (stock A0)
NOTE|The dwelling transfer air is routed over the 8 space(s) the system serves. No further internal
     space belongs to the dwelling(s) they are in, so nothing was added to the network.
     -> Corridor_1 is a zone in the TBD and carries NO inter-zone air movement at all.

hall_in (Corridor_1 related to the Flat 1 dwelling zone)
NOTE|The dwelling transfer air is routed over the 8 space(s) the system serves and 1 further internal
     space(s) of the same dwelling(s) (Flat 1, Flat 2, Flat 3): Corridor_1. These carry no design
     ventilation terminal and are NOT served by the system - they are the rooms the dwelling's
     transfer air passes through.
NOTE|Ventilation system 'MVHR 1' serves 8 space(s) through 10 design terminal(s).
```

`serves 8 space(s)` — unchanged. The corridor joined the **network** and nothing else: no design terminal,
no relation to the ventilation system, no claim that the unit ventilates it. And it carries the dwelling's
transfer air, arriving from two rooms and dividing five ways:

```text
IZAM Bedroom 2_3 TO Corridor_1 |source=Bedroom 2_3|massFlow_kgs=0.0363   |= 30.00 l/s
IZAM Bedroom 2_6 TO Corridor_1 |source=Bedroom 2_6|massFlow_kgs=0.016638 |= 13.75 l/s
IZAM Corridor_1 TO Bathroom_2  |source=Corridor_1 |massFlow_kgs=0.00484  |=  4.00 l/s
IZAM Corridor_1 TO Ensuite_5   |source=Corridor_1 |massFlow_kgs=0.00484  |=  4.00 l/s
IZAM Corridor_1 TO Ensuite_8   |source=Corridor_1 |massFlow_kgs=0.00968  |=  8.00 l/s
IZAM Corridor_1 TO Kitchen_7   |source=Corridor_1 |massFlow_kgs=0.02662  |= 22.00 l/s
IZAM Corridor_1 TO Studio 1_0  |source=Corridor_1 |massFlow_kgs=0.006957 |=  5.75 l/s

NODE|Corridor_1|in_kgs=0.052938|out_kgs=0.052938|residual_kgs=-0|in_lps=43.75|out_lps=43.750001
CONSERVATION|nodes=10|density_kgm3=1.21|maxResidual_kgs=0|maxResidual_lps=0.000005
```

43.75 l/s in, 43.75 l/s out, at a room with no terminal of its own — the `20 -> 8 + 12` hall of the diagram
above, at the scale of a real plan and written into a real TBD. Ten conserving nodes rather than nine, and
the whole-system totals are untouched at 156 l/s each way, because a transfer node changes where the air
goes and never how much of it there is.

### The number

```text
TOTAL|values=78840|differing=78838
```

**78 838 of 78 840**, against 78 835 before the density correction. The count was never the point — the
gate is `differing > 0`, and correcting the physical magnitude of every inter-zone air movement is
entitled to move it. The dwelling now runs 0.2–1.0 K cooler in the mean than the naturally ventilated
alternative, up to 3.0 K at an individual hour.

### Regression: the Iteration 1b OPEN / NIGHT A/B is unchanged

```text
RESULT|1b OPEN |SIMULATED
RESULT|1b NIGHT|SIMULATED
TOTAL|values=78840|differing=16690
```

**16 690** — the accepted count. Neither correction can reach the natural ventilation route: it builds no
design terminals, no system and no air movements, so it never reaches the transfer-air scope or the
inter-zone air movement writer.

### Still deliberately not fixed

`SAM.Analytical.Tas.Modify.Simulate` still reports a refused simulation as a success. It is recorded in
the section above and is not this change's to fix.

The legacy `Create.IZAM` / `Modify.UpdateIZAMsBySpaceParameter` route, which builds an inter-zone air
movement from `SAM_IZAM_*` space parameters a modeller authors by hand, is **not** converted. Those values
are whatever the modeller typed, in whatever unit they meant; converting them would silently rescale
existing models. Only the Part O runtime realization, which knows its own values are m³/s, is converted.

---

## Iteration 1a / Base MVHR — the extract route, and the heat recovery nobody asked for (2026-08-27)

The accepted topology routed each room's extract back to the unit and exhausted it from there:

```text
Outside        -> MVHR-01
extract rooms  -> MVHR-01
MVHR-01        -> supply rooms
MVHR-01        -> Outside
```

It balances, it simulates, and it is wrong for this iteration. **`MVHR-01` is a TAS zone, and a TAS zone
is well mixed.** Giving it the outside intake and the whole extract duty at once mixes the two streams at
its air node, and the supply it then delivers to the rooms leaves at the *mixed* temperature. That is a
sensible heat exchanger. Iteration 1a is deliberately a generic base MVHR **airflow route** with no
manufacturer heat-recovery performance, no bypass, no tempering and no supply-temperature setpoint — so
the exchanger was neither specified nor switchable, and nothing in the model said it was there.

This section is the licensed A/B that established it, and the minimal correction that followed.

### The harness — rebuilt, because the recorded one is not on this machine

`C:\TasOut\inv\Inv.exe` and `C:\TasOut\v40\A0.sam` do not exist here; `C:\TasOut` does not exist at all.
The earlier SAM_Tas run notes anticipated this and said so: *"Both are outside the repo and will not exist
on another machine — rebuild from the `run-tas` skill if the licensed chain needs re-running."* The
harness below is that rebuild, against the same TAS **9.5.7.0** and the same
`AnalyticalModel -> ToGbXML -> WorkflowCalculator` route, with the same one-document-cycle-per-process
rule and short output paths.

### The base model — a RECONSTRUCTION, and where it differs

The base model is rebuilt from the production regression model
`…\SAM_daily\2027-08-03-HVAC\SAM_zoningAM_v2zonesisDomestic.sam` — the same 9 spaces, the same
Flat 1 / Flat 2 / Flat 3 / `Corridor` (`Is Dwelling = No`) zoning, and the same 20 apertures already
carrying an **Unrestricted** `PartOOpeningProperties` with `Function = zdwno,0,19.00,21.00,99.00` — with
`Kitchen_4` renamed **`Living Kitchen_4`**, which is what the accepted acceptance records that space as.

It reproduces the accepted run's **156 l/s supply / 156 l/s extract** system duty exactly, its gbXML is
303 203 bytes against the accepted 303 200, and its movement census has the accepted shape (one intake,
four supply, six extract, one exhaust). It is **not** identical: the Approved Document F allocation
differs room by room (`Bedroom 2_3` 36.75 l/s here against 45.5 l/s recorded) and it routes nine transfer
movements rather than eight. Every number below is therefore internally consistent and comparable
*within* this section, and must not be compared line for line with the sections above.

| | |
|---|---|
| TAS | **9.5.7.0**, GUI closed, no `TBD`/`TSD` process running |
| Weather | `C:\Users\Public\Documents\Tas Data\Databases\CIBSE Weather 2021.twd` |
| Run | `Sizing = true`, `Simulate = true`, days 1..365 |
| Harness | `C:\TasOut\harness` (`PO.exe prep / mkb / togbxml / workflow / dumptbd / dumptsd / tsdcompare / probe / simulate`), outputs in `C:\TasOut\po` |
| Isolation | **one** gbXML generated and fed to both sides, so only the air movement network differs |
| Success criterion | a real multi-megabyte `.tsd` **and** no `*.log` beside the `.tbd` — never `Modify.Simulate`'s return value, which reports a refused simulation as a success |

### The two topologies

**A — the accepted implementation**, straight off `Modify.PreparePartOIteration(BasePassive)`.

**B — direct exhaust.** Built by rewriting A's *prepared model* and nothing else: each `room -> unit`
extract re-issued with the same guid, name, profile and flow and a `To` of **null**, and the unit's
exhaust deleted. The Part F transfer network, the design terminals, the dwelling scope and the duties are
untouched. **No production code was changed to produce B.**

```text
A: Outside -> MVHR-01 -> supply rooms      B: Outside -> MVHR-01 -> supply rooms
   rooms   -> transfer network -> rooms       rooms   -> transfer network -> rooms
   extract rooms -> MVHR-01 -> Outside        extract rooms -> Outside
```

### Node conservation, read off the licensed files

Summed at each zone over every inter-zone air movement touching it, never paired route against route.

| Zone | A in / out [kg/s] | B in / out [kg/s] |
|---|---|---|
| `MVHR-01` | 0.37752 / 0.37752 | 0.18876 / 0.18876 |
| `Bedroom 2_3` | 0.0534288 / 0.0534288 | same |
| `Bedroom 2_6` | 0.08188976 / 0.08188976 | same |
| `Living Kitchen_4` | 0.07623 / 0.07623 | same |
| `Kitchen_7` | 0.07623 / 0.07623 | same |
| `Studio 1_0` | 0.0363 / 0.0363 | same |
| `Bathroom_2`, `Ensuite_5`, `Ensuite_8` | 0.00968 / 0.00968 each | same |

```text
A: IZAMCOUNT|21   CONSERVATION|nodes=9|density_kgm3=1.21|maxResidual_lps=0.00000539
B: IZAMCOUNT|20   CONSERVATION|nodes=9|density_kgm3=1.21|maxResidual_lps=0.00000385
```

`MVHR-01` is the only node that changes: **312 l/s each way in A** (156 outside + 156 extract in;
156 supply + 156 exhaust out) against **156 l/s each way in B** (156 outside in, 156 supply out), which is
the expected Scenario-B balance stated exactly. `Corridor_1` is a tenth zone and carries no inter-zone air
movement in either. The largest residual anywhere is 5.4e-6 l/s, the single-precision rounding of the TBD
profile field. Terminal truth is unchanged: bedrooms supply-only, wet rooms extract-only, `Corridor_1`
with no terminal and no movement, no terminal invented anywhere.

### The thermal evidence

Full year, 8 760 hours, both sides.

| | A | B | A - B |
|---|---|---|---|
| `MVHR-01` dry bulb, mean | **13.79 C** | **10.00 C** | **+3.79 K** |
| `MVHR-01` dry bulb, min | 3.82 C | -5.05 C | +8.87 K |
| `MVHR-01` Air Movement Gain, mean | **+755 W** (max +1785 W, +6 613 kWh/yr) | **0 W exactly** | +755 W |
| `Studio 1_0` dry bulb, mean | 17.63 | 16.88 | +0.75 K |
| `Bedroom 2_3` | 17.57 | 16.71 | +0.86 K |
| `Living Kitchen_4` | 17.42 | 16.58 | +0.84 K |
| `Bedroom 2_6` | 17.21 | 16.07 | +1.14 K |
| `Kitchen_7` | 17.75 | 17.18 | +0.57 K |
| `Bathroom_2` / `Ensuite_5` / `Ensuite_8` | 18.60 / 18.47 / 18.66 | 18.31 / 18.13 / 18.41 | +0.29 / +0.34 / +0.25 K |

**B's unit zone tracks outside air** — 10.0 C in the mean, down to -5.0 C. **A's sits 3.79 K above it and
never falls below 3.8 C**, because 156 l/s of dwelling extract at ~17.6 C is arriving in it. Mixing
0.156 kg/s at 10.0 C with 0.156 kg/s at 17.6 C gives 13.8 C. A's measured mean is **13.79 C**. The unit's
zone is behaving as a perfectly mixed, **~50%-effective** sensible heat exchanger.

**`Air Movement Gain` on `MVHR-01` is the cleanest single tell.** TAS books air arriving from another
*zone* as Air Movement Gain and air arriving from outside as infiltration/ventilation gain. In B nothing
arrives at the unit from a zone, and the series is **0 in every one of the 8 760 hours** — mean, min and
max all zero. In A it averages +755 W and peaks at +1 785 W. That gain is the recovered heat, measured.

**And it reaches the rooms.** Every supplied room's own Air Movement Gain is far less negative in A than
in B — `Bedroom 2_6` -263 W against -465 W in the mean — because the air arriving from the unit is 3.8 K
warmer. This is the direct evidence that extract air entering `MVHR-01` thermally affects the air
subsequently supplied from it.

```text
tsdcompare A vs B, resultant temperature, the series the assessment reads:
TOTAL|values=87600|differing=87599
ZONE|MVHR-01         |differing=8760|meanDiff=3.503 |maxAbsDiff=8.2903
ZONE|Bedroom 2_6     |differing=8760|meanDiff=1.0142|maxAbsDiff=2.7171
ZONE|Bedroom 2_3     |differing=8760|meanDiff=0.7763|maxAbsDiff=2.8634
ZONE|Living Kitchen_4|differing=8760|meanDiff=0.777 |maxAbsDiff=1.5819
```

**87 599 of 87 600.** Not a rounding difference — a different building.

### Answer

**Yes. `MVHR-01` is a well-mixed TAS thermal zone, and the accepted extract -> unit route introduced
unintended thermal coupling.** Scenario B is adopted.

### What TAS's `Value = 1.0` means — settled by experiment, not by reading

`Modify.UpdateIZAMs` writes each movement through `Modify.UpdateIZAMProfile`, which lands in
`Modify.Update(profile, Profile, factor)`. For a one-value SAM profile that sets
`profile_TBD.type = ticValueProfile`, `profile_TBD.value = <the SAM profile's own value>` and
`profile_TBD.factor = <the mass flow>`. The SAM ventilation profile of a Part O air movement is a flat
1.0 — the movement runs at 100% of its design flow every hour — so the field the Building Simulator shows
as **Value** is 1.0 and the engineering magnitude sits in **Factor**.

One representative movement, all the way through, on this model:

```text
Approved Document F requirement   Bedroom 2_3 Supply          36.75 l/s
VentilationTerminal design duty                               36.75 l/s
SpaceAirMovement.AirFlow                                      0.03675 m3/s      (SAM stays volumetric)
TBD  IZAM MVHR-01 TO Bedroom 2_3  profileType=ticValueProfile
                                  value   = 1
                                  factor  = 0.0444675
                                  units   = kg/s              (TAS's own declaration)
                                  GetExtremeValue(true) = 0.0444675
0.0444675 / 1.210                                             = 0.03675 m3/s = 36.75 l/s
```

**Which of the two does TAS actually read?** Three full-year simulations of the same finished TBD, with
`factor x value` held constant and the split between them changed:

| Probe | factor | value | vs control |
|---|---|---|---|
| control — the same TBD, re-simulated | 0.18876 (intake) | 1 | `differing=0` of 87 600 |
| **swap** — TAS's own convention | 1 | 0.18876 | **`differing=0`** |
| **split** | 0.75504 | 0.25 | **`differing=0`** |

The control establishes a noise floor of exactly zero, so any difference would have been real. There is
none. **TAS's effective inter-zone mass flow is `factor x value`**: the two fields are one product and the
split between them carries no physical meaning.

**This is Case 1, and nothing is changed.** The displayed `1.0` is the schedule term — the fraction of
design flow the movement runs at, which for a continuous Part O movement is unity in every hour — and the
0.0444675 kg/s is the magnitude, in the same profile, in the field TAS multiplies it by. The physical
airflow is neither hidden nor weakened, and moving it into `value` would produce a byte-for-byte
equivalent simulation while breaking the schedule/magnitude separation the rest of the library uses.

### Where the correction belongs — the layer, not just the topology

A first implementation made this change **in SAM**: `Modify.AddAirMovementObjects` was altered to give the
extract movement a `To` of null and `AddAirHandlingUnitExhaust` was deleted. It produced the right TAS
result and it is **withdrawn**, because it is the wrong layer.

The SAM model is not a TAS input. It is the engineering statement of the design, and for a balanced MVHR
unit that statement is:

```text
SAM PHYSICAL MODEL  (unchanged, and it is correct)

Outside        -> AHU
AHU            -> Supply spaces
Supply spaces  -> Part F transfer network -> Extract spaces
Extract spaces -> AHU
AHU            -> Outside
```

The extract air really does pass through the unit. That is what an MVHR unit *is*, it is what
`Query.AirFlow` and the balance refusal read, and it is what Iteration 2 will need when a real unit with
real heat recovery is selected. Deleting it from SAM to satisfy an export would have made the model less
true in order to make one consumer of it happier.

**The limitation is TAS's, so the compensation belongs at the TAS boundary.** `Modify.UpdateIZAMs`
represents an air handling unit as a *thermal zone*, and a TAS thermal zone is one well-mixed air node: it
has no way to let a supply airstream and an extract airstream pass through the same box without meeting.
`TBD.IIZAM` offers a source zone, target zones and a `fromOutside` flag and nothing else. So the export
states the same duty in the only vocabulary TAS has:

```text
TAS ITERATION 1a REPRESENTATION  (what SAM_Tas writes)

Outside        -> MVHR TAS node
MVHR TAS node  -> Supply spaces
Supply spaces  -> Part F transfer network -> Extract spaces
Extract spaces -> Outside
```

That is, at the boundary and nowhere else:

```text
SAM:  room -> AHU  and  AHU -> Outside        becomes         TAS:  room -> Outside
SAM:  Outside -> AHU  and  AHU -> room        stays as        TAS:  Outside -> MVHR -> room
```

Every room still loses exactly its design extract, the unit still draws and delivers exactly its design
supply, every node still conserves, and the two airstreams never meet.

### The production change

**`SAM_Tas/SAM.Analytical.Tas/Query/DesignTerminalExtractFlattening.cs`** (new) and two call sites in
**`SAM_Tas/SAM.Analytical.Tas/Modify/UpdateIZAMs.cs`**. **`SAM` carries no code change at all.**

The query answers two questions over the model being exported: which extract movements to write as leaving
from the room, and which units therefore lose their exhaust. `Modify.UpdateIZAMs` then

- drops the `To` of a flattened movement before it resolves endpoints, so it lands on the **room's** zone
  with no source zone and `fromOutside = 0` — the shape TAS itself authors for a zone discharging to
  outside — and is named `IZAM <room> TO OUTSIDE`, which is also the name queued for removal, so a
  re-export replaces it cleanly rather than duplicating it;
- skips that unit's outward movement, because an exhaust beside a flattened extract would take the same
  air out of the building twice.

**The scope, and why generic MEP cannot reach it.** A movement qualifies only where its source is a
`Space` carrying design `VentilationTerminal`s **and** its destination is an `AirHandlingUnit`. That reads
no name — not `MVHR`, not the system type — and it is not a heuristic: it is the same authority
`Modify.AddAirMovementObjects` uses to choose its **design-terminal branch**, which is the only code in
SAM that routes a room's air *into* a unit at all. The generic branch, which every model without design
terminals reaches, already writes each space's outward movement straight to outside and gives its unit
nothing to receive, so it cannot match and is not changed. Design terminals are realized only on the
Approved Document O MVHR route. The legacy `Create.IZAM` / `Modify.UpdateIZAMsBySpaceParameter` route
never reaches this method.

Terminal duties, Part F requirements, transfer-air routing, dwelling scope, `VentilationSystem`
membership, the m3/s to kg/s conversion and its `Core.FluidProperty.Air.Density` = 1.210 kg/m3, and the
`factor` / `value` representation are all untouched.

### The licensed acceptance of the changed production path

**Run settings changed:** these runs use `Sizing = false, Simulate = true`, days 1..365. A Part O
assessment reads free-running hourly temperatures, not plant sizes. The A/B sections above were run with
`Sizing = true`, so absolute figures are not comparable across that boundary — which is why Scenario B was
**re-simulated under the new setting** below rather than compared against its recorded `.tsd`. Both sides
of every gate below were run the same way.

`PO.exe prep A0.sam -> P.sam` on the production path, then the full year:

```text
PREPARE|ventilationMode=MVHR|airflowApplication=Apply|successful=True|openingCompatibility=Compatible|scenarios=4
DUTY|supply_lps=156|extract_lps=156
REFUSAL lines: 0
gbXML 303 202 bytes; TBD 425 335 bytes; TSD 5 538 170 bytes, days 1..365, no *.log beside the TBD
IZAMCOUNT|20   CONSERVATION|nodes=9|density_kgm3=1.21|maxResidual_lps=0.00000385
NODE|MVHR-01|in_kgs=0.18876|out_kgs=0.18876|residual_kgs=0|in_lps=156|out_lps=156
```

**The SAM model says 312 l/s each way at `MVHR-01`; the exported TBD says 156.** That difference *is* the
correction, and both are right in their own layer:

```text
SAMNODE|MVHR-01|in_lps=312|out_lps=312|residual_lps=0     <- the physical model: 156 outside + 156 extract
NODE   |MVHR-01|in_lps=156|out_lps=156|residual_kgs=0     <- the TAS export:     156 outside, 156 supply
```

The exported inter-zone air movement census is exactly the intended shape — one intake, four supplies, six
room-to-outside extracts, nine transfers, and **no unit exhaust**:

```text
IZAM MVHR-01 FROM OUTSIDE   fromOutside=-1  source=-         targets=MVHR-01    0.18876 kg/s = 156 l/s
IZAM MVHR-01 TO <room>      fromOutside=0   source=MVHR-01   targets=<room>     x4, sum 0.18876 kg/s
IZAM <room> TO OUTSIDE      fromOutside=0   source=-         targets=<room>     x6, sum 0.18876 kg/s
IZAM <room> TO <room>       fromOutside=0   source=<room>    targets=<room>     x9, transfer air
(no IZAM MVHR-01 TO OUTSIDE)
```

Every movement carries `profileType=ticValueProfile`, `value=1`, the mass flow in `factor`, and TAS's own
`units=kg/s` — the settled convention, unchanged.

**The production path reproduces the validated Scenario B exactly:**

```text
tsdcompare P (production, corrected) vs B (hand-built Scenario B), both Sizing=false
TOTAL|values=87600|differing=0
every one of the 10 zones: differing=0, meanDiff=0, maxAbsDiff=0
```

Zero of 87 600. The change realizes precisely the topology the A/B validated, and nothing else.

**And the thermal signature holds:**

```text
ZONESTAT|MVHR-01|airMovementGain[mean=0;min=0;max=0;sum_kWh=0]
ZONESTAT|MVHR-01|dryBulb[mean=9.9993;min=-5.0477;max=29.1252]
```

`Air Movement Gain` on the unit's zone is **0 W in every one of the 8 760 hours**, against Scenario A's
+755 W mean and +1 785 W peak, and the zone tracks outside air at 10.0 C rather than sitting 3.79 K above
it. Nothing arrives at the unit from a zone, so there is nothing for it to recover.

### Regression

```text
1b OPEN, PRE-change binary vs POST-change binary   TOTAL|values=78840|differing=0
   (same prepared model, same gbXML, only SAM.Analytical.Tas.dll swapped)

1a (production, corrected) vs 1b OPEN              TOTAL|values=78840|differing=78839   (gate: > 0)
                                                   MISSING|MVHR-01 - expected, 1b has no unit zone
```

The 1b gate is measured **directly, as a binary A/B on this machine**, rather than inferred from an
absolute count: one prepared 1b model and one gbXML were fed to the pre-change and post-change
`SAM.Analytical.Tas.dll` in turn, and the two full years are bit-identical across all 78 840 values. The
mechanism is visible in the files too — the 1b prepared model reports `SAMCONSERVATION|nodes=0` (no
`SpaceAirMovement`s at all) and its TBD contains `IZAMCOUNT|0`, so the changed code is never reached on
the natural ventilation route.

`SAM.Tests` **1471/1471** (was 1470; +1 pinning all four legs of the physical MVHR route).
`SAM.Analytical.Tas.TM59.Tests` **645/645** (was 642; +3 pinning the boundary flattening, its scope, and
that a model without design terminals flattens nothing).

### Reconstructed-laptop limitations — what this machine can and cannot prove

This work was done on a **rebuilt** laptop. `C:\TasOut\v40\A0.sam` and the original `Inv.exe` are not
here; the harness and the base model are the reconstruction described earlier in this section, which
reproduces the accepted 156 l/s / 156 l/s duty and movement census but **not** the accepted run's
room-by-room Approved Document F allocation.

So, precisely:

- **Proved here.** The production path reproduces validated Scenario B (`differing = 0`); the unit's Air
  Movement Gain is zero all year; every node conserves; the 1b route is bit-identical across the change;
  1a and 1b differ.
- **Not proved here, and not claimed.** The historic absolute **`1b OPEN/NIGHT = 16690`** acceptance
  figure. That number belongs to the original `A0.sam`, and this model is not it — the reconstructed
  pairing gives a different absolute count. **It still requires final confirmation on the original laptop
  against the original `C:\TasOut\v40\A0.sam`.** Nothing in this section should be read as having
  discharged that gate.

### What this does NOT do

No manufacturer MVHR logic, no heat recovery, no bypass, no tempering, no supply-temperature setpoint, no
restored 23 C. **The SAM physical airflow topology is unchanged** — all four legs of the MVHR route are
still stated on the model, and `SAM.Tests` pins them. Iteration 1b is untouched, measured. The legacy
`Create.IZAM` / `Modify.UpdateIZAMsBySpaceParameter` route is untouched. Generic MEP exports are untouched,
because the flattening is gated on design ventilation terminals that only the Part O route realizes.

**Iteration 1a's supply air is outside air**, which is what a base configuration stating no heat recovery
means. A later iteration modelling a real unit will add recovery explicitly — where it can be seen,
parameterised and switched off — and it will need the SAM model's `extract room -> AHU` leg, which is why
that leg stays.

---

## Original A0 Final Acceptance (2026-08-27)

Run on the original laptop, against the original licensed fixture and harness the reconstructed laptop did
not have: `C:\TasOut\v40\A0.sam` and `C:\TasOut\inv\Inv.exe`. This closes the one gate the reconstructed
laptop's section above explicitly left open — the historic **`1b OPEN/NIGHT = 16690`** figure — and repeats
the corrected-topology production acceptance on the real fixture rather than the reconstruction.

```text
Original fixture:        C:\TasOut\v40\A0.sam
SAM SHA:                 3be1f645e9dcb5ed23dbeedbc0fec18bfbfc5471
SAM_Tas SHA:              e228098e4697078ebaf11f8a27e968f82e55dbaf
TAS:                     9.5.7.0
Sizing:                  false
Simulation:              full year (days 1..365)

SAM.Tests:                1471/1471
SAM.Analytical.Tas.TM59.Tests: 645/645

supply/extract:          156/156 l/s
MVHR node:               in=155.999998 l/s | out=155.999998 l/s  (was the accidental 312/312)
max conservation residual: -0.000004 l/s (nodes=9, density=1.21 kg/m3)
MVHR Air Movement Gain:  0 W in every one of the 8760 hours (sum=0 kWh)
MVHR mean dry bulb:      9.9993 C (min -5.0477, max 29.1252)
strategy refusals:       0 (only the expected ASSOCREFUSAL on MVHR-01 itself, which is plant, not a room)
TSD size:                5 546 109 bytes (TBD 426 523 bytes, gbXML 303 200 bytes, no error log beside the TBD)

1a vs 1b:                78840/78840 differing (both runs Sizing=false, same base model/weather/period)
1b OPEN/NIGHT:           16690/78840  <- exact match to the historic accepted figure
```

**The harness needed one change to run this, not production code.** `C:\TasOut\inv`'s `Pipeline.Workflow`
hardcoded `Sizing = true`; a `workflowfinal` mode was added alongside the existing `workflowsim` (left
unchanged at `Sizing = true`, which is what the 16690 historic gate below still needs) so the corrected
Iteration 1a acceptance could run at `Sizing = false` as this section requires. A second addition,
`partozonestat`, reads the TSD's per-zone `Air Movement Gain` / `Dry Bulb Temperature` series the same way
`TsdCompare` already read `Resultant Temperature`, to produce the `ZONESTAT` evidence below. Neither
`SAM` nor `SAM_Tas` production code changed for this acceptance.

**Topology, read back from the real `.tbd`, confirms the corrected shape exactly**: 19 inter-zone air
movements — one intake, four supplies, six room-to-outside extracts, eight Part F transfers — and **no**
`extract-space -> MVHR` movement and **no** `MVHR -> Outside` exhaust:

```text
IZAM MVHR-01 FROM OUTSIDE  fromOutside=-1  source=-         targets=MVHR-01     0.18876 kg/s = 156 l/s
IZAM MVHR-01 TO <room>     fromOutside=0   source=MVHR-01   targets=<room>      x4, sum 0.18876 kg/s
IZAM <room> TO OUTSIDE     fromOutside=0   source=-         targets=<room>      x6, sum 0.18876 kg/s
IZAM <room> TO <room>      fromOutside=0   source=<room>    targets=<room>      x8, transfer air
(no IZAM MVHR-01 TO OUTSIDE)

NODE|MVHR-01|in_kgs=0.18876|out_kgs=0.18876|residual_kgs=0|in_lps=155.999998|out_lps=155.999998
```

The eight-transfer count (against the reconstructed laptop's nine) is not a discrepancy: that section's
`P`/`B` figures were run on a different, reconstructed fixture, and A0's own eight-transfer topology matches
this section's own earlier `Iteration 1a vs Iteration 1b (2026-08-27)` table above, room for room.

**The hall probe still works, unchanged**, and shows both sides of the dwelling-scope boundary on the real
fixture: `INV_HALLDEMO="Corridor_1|Flat 1"` relates the model's own zero-terminal space into a dwelling zone
and it immediately carries seven transfer-air movements and no design terminal of its own; the unmodified
model — where `Corridor_1` sits in a zone marked `Is Dwelling = No` — carries zero. Bedroom/habitable spaces
and wet rooms keep their proper one-directional duties in both cases (`SPACE|Bedroom 2_3|...` supply-only,
`SPACE|Bathroom_2|...` extract-only, from `partoauthor`'s own report).

**TM59 route**: Iteration 1a takes the mechanical route (`COUNTS|naturalVentilation=0|mechanicalVentilation=5|corridor=4`);
Iteration 1b OPEN and NIGHT both take the natural route (`COUNTS|naturalVentilation=5|mechanicalVentilation=0|corridor=4`).
Zero strategy refusals in every run. No 23 C MVHR supply setpoint, no heat-recovery efficiency, no cooling,
no tempering, no summer bypass, no manufacturer MVHR behaviour — confirmed off `partodump`'s zone profiles
(`freshAirRate_lsp=0` on every sized space, `ticVfactor_ach=1` unchanged, no SAM Ventilation profile
assigned).

**The historic gate is closed.** `tsdcompare` between a freshly authored-and-simulated `1b OPEN` and
`1b NIGHT`, both at the original `Sizing = true` / `Simulate = true`, days 1..365 recipe the 2026-08-26
acceptance used, gives exactly `TOTAL|values=78840|differing=16690` on the original `A0.sam` — the one
figure the reconstructed laptop could not prove, now confirmed unchanged by the production topology
correction.

---

## `A0.sam` retired as the Base MVHR whole-model acceptance fixture (2026-08-28)

**A second-round review of SAM#77** found that `PrepareBaseMVHR` combined every assessed dwelling onto
one shared generic system/AHU, even though `PartFCalculator` sizes each dwelling zone independently. Fixed:
`Modify.PrepareBaseMVHR` now partitions the assessed scope into one group per dwelling zone
(`Query.PartFDwellingZones` + the zone→space relation) and builds or reuses one system per dwelling, never
one shared system across independent dwellings.

**Re-running the Original A0 Final Acceptance above under this fix no longer produces a result at all.**
That acceptance assessed the WHOLE model — all four zones, confirmed from the harness source
(`C:\TasOut\inv\PartO.cs`'s `partoauthor` mode calls `analyticalModel.GetZones()`, every zone, not a
subset) — and under the fix that whole-model run now refuses. So does every one of A0's three flats
assessed alone:

| dwelling | own net design-duty imbalance | assessed alone |
|---|---|---|
| Flat 1 (`Studio 1_0`, `Bathroom_2`) | -19.5 l/s | refuses: `Studio 1_0` and `Bathroom_2` are both net sinks with no internal source |
| Flat 2 (`Bedroom 2_3`, `Living Kitchen_4`, `Ensuite_5`) | +26 l/s | refuses: `The air movements do not balance: Ensuite_5 gains 10.667 l/s, Living Kitchen_4 gains 15.333 l/s.` |
| Flat 3 (`Bedroom 2_6`, `Kitchen_7`, `Ensuite_8`) | -6.5 l/s | refuses: `Ensuite_8 loses 1 l/s, Kitchen_7 loses 5.5 l/s` |

The three imbalances sum to **exactly zero** only across the whole building — not a coincidence. It is the
fingerprint of the exact defect the fix closes: the old single-shared-system code let one flat's transfer
air surplus balance a different flat's deficit through whatever adjacency happened to connect them, so the
**"156/156 l/s, MVHR Air Movement Gain 0 W, max residual -0.000004 l/s"** result this document recorded
above was resting on invalid cross-dwelling transfer, not on `A0.sam` genuinely being three independent,
individually-balanced Base MVHR dwellings.

**Confirmed this is a fixture defect, not a code defect, two ways:**

1. **Adjacency is not the cause.** `zonespaces` (a new `C:\TasOut\inv` diagnostic mode) confirms Flat 2's
   own three rooms are directly wall-connected to each other (`Bedroom 2_3 <-> Living Kitchen_4 <->
   Ensuite_5`), with no need to route through `Corridor_1` or another flat — a self-contained transfer
   route exists in principle. `INV_ZONE_ONLY=Flat 2` (a new diagnostic-only harness hook restricting the
   `zones_` argument `partoauthor` passes into `Modify.PreparePartOIteration`) still refuses: `Bedroom 2_3`
   alone (45.5 l/s supply, no extract) needs to shed more air internally than the other two rooms in its
   OWN flat can sink (19.5 l/s of combined extract capacity) — a 26 l/s deficit intrinsic to that flat's
   own Part F sizing, independent of any code scope.
2. **Every flat fails the same way**, each for its own named-room reason
   (`Modify.RefuseUnbalancedAirMovement`'s diagnostic names the exact rooms and residuals every time) —
   there is no single dwelling that passes while others fail, which would point at a scoping bug. All
   three fail, for three different room-level reasons, and their totals cancel only in aggregate.

**Disposition.** `A0.sam`'s own Part F terminal data was never authored (or re-sized) to balance
per-dwelling, only in aggregate across the whole building — a fact the pre-fix bug hid. Closing that bug
is correct, and production logic was **not** changed to make `A0.sam` pass; doing so would mean either
re-authoring the fixture's per-flat duties (design work on the fixture, out of scope for this PR) or
letting transfer air cross dwelling boundaries again (reintroducing the defect). `A0.sam` therefore retires
as the licensed acceptance fixture for Base MVHR's per-dwelling behaviour. The multi-dwelling seam is
instead pinned by [SAM-BIM/SAM#77](https://github.com/SAM-BIM/SAM/pull/77)'s own automated `SAM.Tests`, on
a real two-dwelling fixture built and independently balanced for exactly this purpose
(`ModelWithTwoAssessedDwellings` in `PartOIterationPreparationTests.cs`, exercised by
`TwoAssessedDwellings_EachGetsItsOwnVentilationSystemAndAirHandlingUnit` and three sibling tests).

**Iteration 1b is untouched and re-confirmed live on the same fixture**: `1b OPEN/NIGHT` still reproduces
the historic **16690/78840** figure exactly, because Iteration 1b (`BaseNaturalVentilation`) never reaches
`PrepareBaseMVHR` — only the MVHR route does.

Harness additions for this investigation (diagnostic-only, no production code touched): `ZoneSpaces.cs`
(`zonespaces` mode) and `INV_ZONE_ONLY` in `PartO.cs`'s `Author` method. Evidence in `C:\TasOut\p1a4\`
(whole-model refusal, adjacency dump, 1b regression) and `C:\TasOut\single\` (Flat 1/Flat 2 isolated
re-runs).

## Part O real-project acceptance — 1a / 1b / 2 / 2B / 3-B0 / 3-B4 (2026-09-15, closed out 2026-09-16)

The first end-to-end licensed acceptance of the whole Part O iteration set on a **real project** rather than
a fixture: `SAM_zoningAM-CIBSEfutureZ1.sam` (SHA-256 `a7e09a25…`), 3 dwellings, 8 assessed rooms, on its own
embedded `Z1_DSY1_2050s_HIGH90_CIBSE_v1.1` weather. Full evidence, every `.tsd`, the per-stage extracts and
the harness sources are preserved outside the repositories under
`C:\TasOut\parto-final-real-project\` (`evidence-draft.md` is the record).

Built from `sow/2026-Q3` at SAM `b4a1283f`, SAM_Systems `05ca0c18`, SAM_Tas `ac85b5c3`, SAM_UI `9f515c4c` —
each confirmed on 2026-09-16 (with `git ls-remote`) to be the **current tip** of its repo's remote branch, so
the **code revision** is reconstructible from the repositories alone. **The run is not**: the source model,
the harness, every `.tsd`, the per-stage extracts, the evidence record and the acceptance binaries
themselves exist only under `C:\TasOut\parto-final-real-project\`, outside every repository and on one
machine - deliberately, since no TAS document or manufacturer table is ever checked in. Reproducing or
auditing this evidence needs that folder as well as the commits.

### Verdicts

| stage | outcome | detail |
|---|---|---|
| 1a Base MVHR | **FAIL** | 6 of 8 rooms |
| 1b Base NV, original source | **REFUSED** (by design) | overlapping humidity limits, `Create\Log.cs:1154`; pinned by `PartOHumidistatTests.cs:72` |
| 1b Base NV, repaired source | **FAIL** | Criterion 2 in `Bathroom_2`, `Ensuite_5`, `Ensuite_8` |
| 2 Acoustic restricted | **FAIL** | bit-identical to 1a - acoustic restriction / bypass / boost are **not implemented**, so the stage runs without the behaviour that defines it |
| 2B | **FAIL** | 10 rounds plus the capacity envelope; every room lowered, none converted |
| 3-B0 Parity | **FAIL** | 4 of 8 rooms; both ensuites cross to Pass |
| 3-B4 SelectedProduct | **REFUSED**, then **FAIL** on rerun | refused at the flow ceiling on 2026-09-15; SAM_Tas#60's clamp merged 2026-09-16, rerun the same day: refusal gone, Candidate B exists, **FAIL** on 3 of 8 rooms. See [3-B4 rerun on the merged clamp](#3-b4-rerun-on-the-merged-clamp-2026-09-16) below. |

Nothing passes. The model is a weather-driven failing case by construction — the canonical PR4/PR5B
acceptance runs the same geometry on `Leeds_TRY` and passes.

### Iteration 2 is bit-identical to 1a, and so is 1b

Both were measured across all 8760 hours of the series TM59 itself read, joined on the design space guid.
Iteration 2's equality with 1a is expected, and worth stating plainly: acoustic restriction, summer bypass and boost are **not implemented** (`PartO-ARCHITECTURE.md` §5 and its status table), so the Iteration 2 stage runs the Iteration 1a case. It is a placeholder stage, not a null result. **Iteration 1b's identity is a defect finding**, and it is exact rather
than merely small: comparing the raw resultant-temperature series value by value, all **78 840** readings
(**9** spaces × 8760 h - the 8 assessed dwelling rooms plus `Corridor_1`, which TM59 does not assess) are identical strings — max |ΔRT| is `0.0` K, not `0.000` after rounding.

The cause is in `SAM.Analytical\Modify\PreparePartOIteration.cs:172-182`. The
`SkipNaturalVentilation` branch is a **pure no-op copy**:

- it clears nothing — `RemoveBaseMVHRAirMovementObjects` (`:767`) has exactly one call site (`:479`), inside
  `PrepareBaseMVHR`, i.e. the MVHR route only;
- it refuses nothing about authored mechanical objects — the only refusals on the 1b path are route
  resolution, iteration↔route disagreement, and isolation.

So a model authored with mechanical systems keeps them: the `VentilationSystem`s, the AHUs, the 17
`SpaceAirMovement`s and 3 `AirHandlingUnitAirMovement`s all survive into the export, and `SAM_Tas`
`Modify.UpdateIZAMs` writes them as IZAMs. The TM59 *criterion* is still correct — `TM59\Convert\ToTM59\
Building.cs:145-150` takes the route from the `OverheatingScenario`, not from the model's objects — so 1b is
assessed under the adaptive criteria. But the **simulation underneath that criterion is the 1a run**: it is
moving mechanical air.

Two places state the opposite, neither of them enforcing it:

- the preparation preview (`Query\PartOPartFAirflowApplication.cs:53`) says "no mechanical system was
  **invented**", which is true and carefully worded, but a reader would not learn that 17 authored air
  movements are about to be simulated;
- `SAM_UI\…\Query\PartOIteration3Eligibility.cs` asserts in prose that "Iteration 1b has no mechanical
  system to materialise at all" — an assumption the preparation never checks.

No test pins removal, and the gap shows in the test name: `PartOIterationPreparationTests.cs:629`
`NVDwelling_InventsNoContinuousMechanicalSupplyOrExtract` asserts nothing is *invented*; it says nothing
about what was already there.

**Recommendation (not applied — frozen 1b behaviour is unchanged pending a decision).** The smallest
fail-closed change is a **refusal**, not a clear: 1b should refuse a model that carries authored mechanical
air movement, naming the objects. Clearing would mutate authored data on a path whose contract is "leaves the
model as authored"; refusing keeps that contract and makes the user's next action explicit. The legacy
transposed humidity pair should **stay** a refusal for the same reason — repairing it automatically rewrites
authored data on a guess about intent.

### 3-B4 refused at the flow ceiling, and the tolerance is the thing to look at

Production refused with: *"Recirculation cooling of air system `dd8a4594…` carried a recirculation airflow
outside 36..120 l/s in 1 hour(s)."* — MVHR-02, **hour 4927 (25 July 07:00)**, Q = `120.05027770996094` l/s
against `120 + 0.05`, over by **0.00028 l/s**. A harness replay on the same production route reproduced the
refusal exactly.

The bound is `Create\RecirculationCoolingResult.cs:137`, using
`RecirculationCoolingTolerance_Flow_Lps = 0.05` (declared at `:17` — note the doubled `SAM_Tas\SAM_Tas\`
path; a search from the wrong folder level misses the file).

**What calibrated 0.05.** The constant's own doc comment cites "the native solver holds both to within 0.03
l/s over a full year". That measurement is the canonical PR5B acceptance on `Leeds_TRY`
(`SAM_UI\documentation\evidence\PR5B-PRODUCTION-ACCEPTANCE.md:138-152`), and the comment is accurate for it.
But the constant is used for **two different quantities**, which that run bounded very differently:

| quantity | canonical (Leeds TRY) | this project (DSY 2050s) |
|---|---|---|
| ventilation flow departing from its design | max 0.0263 l/s | 0.0023 / 0.0261 / 0.0299 l/s |
| recirculation flow standing outside the law's range | max **0.013** l/s | 0.027 / **0.0503** / 0.001 l/s |

The ventilation quantity is essentially unchanged between the two runs and sits comfortably inside 0.05. The
**range-overshoot quantity grew about fourfold** and consumed the whole margin. The single constant was
calibrated against the gentler of the two.

**The mechanism, from the replay's own hourly series.** Across 3 units × 8760 h = 26 280 hours, only **5
hours** exceed 120 l/s at all. Every one of them is the *first* hour the control law saturates:

| unit | hour | previous hour | the hour | next hour | overshoot |
|---|---|---|---|---|---|
| MVHR-02 | 4572 | Q 118.729, T_mix 25.940 | Q 120.000145, T_mix 26.016 | Q 120.000000 | 0.000145 |
| MVHR-01 | 5148 | Q 106.450, T_mix 25.355 | Q 120.000366, T_mix 26.102 | Q 119.999985 | 0.000366 |
| MVHR-01 | 4837 | Q 99.495, T_mix 25.024 | Q 120.000587, T_mix 26.040 | Q 108.863 | 0.000587 |
| MVHR-03 | 4859 | Q 92.780, T_mix 24.704 | Q 120.026695, T_mix 26.008 | Q 120.000000 | 0.026695 |
| MVHR-02 | **4927** | Q 76.162, T_mix 23.912 | Q **120.050278**, T_mix 26.010 | Q 120.000000 | **0.050278** |

Once saturated the value is exactly `120.000000` — 388 to 494 hours of it per unit. The overshoot appears
only on the ramp hour, and **its size rises monotonically with the size of the approach jump** (1.3 → 13.6 →
20.5 → 27 → 44 l/s gives 0.00014 → 0.00037 → 0.00059 → 0.0267 → 0.0503). That is a controller overshoot
signature, not single-precision noise: it is deterministic in character, but **its magnitude is not bounded
by anything structural** — a steeper transient produces a larger excursion.

Everything else in the B4 branches held: zero heating, zero cooling below the 22 °C gate, coil at the clamped
table to 2e-6 K, ventilation within 0.03 l/s of design.

**Consequence for the fix.** Widening the tolerance to some number above 0.0503 has no principled stopping
point and would mask a recurring artefact that can grow. Clamping the reported flow at the law's ceiling
addresses the actual mechanism — the law *was* saturated (T_mix 26.010 °C → 120.000 l/s), so 120.050 is a
solver excursion on the ramp, not a commanded flow. Recording Iteration 3 as NOT YET ACCEPTED and keeping the
refusal is also defensible: the contract failed closed on a real project, which is the designed behaviour.
**Decision taken (2026-09-16): clamp at the law's ceiling** - SAM_Tas
[#60](https://github.com/SAM-BIM/SAM_Tas/pull/60). A flow just outside the range is reported AT the range and
counted rather than refused, because the declared control cannot command a flow outside its own range; the
clamp runs before the hour's duty, range, law and table coordinates are taken. The bound is a new
`RecirculationCoolingClamp_Lps = 0.1` l/s - about twice the largest measured excursion, and recorded in its
doc comment as a **measured and reviewable** bound rather than a derived one, since nothing structural bounds
a controller overshoot. Every clamped hour is carried on the result (`Count_Clamped`,
`MaximumClampedExcursion_Lps`) so the margin in use stays visible, and beyond the bound the flow is left as
TAS answered it and still refuses. `RecirculationCoolingTolerance_Flow_Lps` keeps only the
ventilation-deviation duty it was actually measured for.

The Iteration 1b finding above is **unchanged and still investigation-only** - no production change was made
for it.

### 3-B4 rerun on the merged clamp (2026-09-16)

**The question.** Does the merged SAM_Tas clamp remove the previous 3-B4 refusal and allow a genuine
Candidate B TM59 result to be produced? This section is that rerun, not a new investigation — the mechanism
was already fully characterised above; this only exercises the fix.

**Provenance.** All four repos rebuilt Release in dependency order SAM → SAM_Systems → SAM_Tas → SAM_UI at
their current `sow/2026-Q3` tips (SAM `192d069a`, SAM_Systems `05ca0c18`, SAM_Tas `96f8ba79`, SAM_UI
`9f515c4c` — SAM#118 and SAM_Tas#60 both merged). `SAM_UI\build\SAM.Analytical.Tas.TPD.dll` confirmed
refreshed and containing `RecirculationCoolingClamp` before the run. Iteration 3-B4 rerun through the same
external PowerShell UI-Automation harness (`stage.ps1 -Stage B4`, outside every repository - it drives the
native `SAM Analytical.exe` via Windows UI Automation, not a UI-automation framework inside the
repositories) on the same preserved real project
(SHA-256 `a7e09a25ae29c7dbb4c690d747a96fcd9f110ca27fb4dc2abe816755368d7e4b`), the same embedded
`Z1_DSY1_2050s_HIGH90_CIBSE_v1.1` weather, full year 1–365, the same 3 dwellings / 8 assessed rooms,
Iteration 3 (A/B) against Reference A = Iteration 1a in the same session, Selected-product cooling module.
Evidence at `C:\TasOut\parto-final-real-project\06-Iteration3-B4\` (the pre-fix refused run preserved
alongside it under `refused-2026-09-15\`).

**A. The previous refusal is gone.** The 2026-09-15 run refused with *"Recirculation cooling of air system
`dd8a4594…` carried a recirculation airflow outside 36..120 l/s in 1 hour(s)"* (MVHR-02, hour 4927). The
rerun's ledger and UI text contain no refusal of any kind.

**B. Ledger completion.** All 15 named stages report `COMPLETED` (Input, Reference A, Reference A TM59,
System scope, Equipment resolution, Materialisation, Thermal source, Systems conversion, Systems simulation,
Zone temperature, Resultant temperature, Candidate B TM59, Reconciliation, Comparison, Persistence) — the
same full ledger 3-B0 always completed. The refused run stopped at *Zone temperature* with 9 stages
completed, 1 refused, 5 never run.

**C. Candidate B TM59 result.** Exists for the first time.
`000000_SAM_AnalyticalModel-It1a-futureZ1-It3B4-Bridge-TM59.txt`: **TM59 OCCUPIED-SPACE ASSESSMENT: FAIL**.
3 of 8 rooms fail the mechanical `>26 C hours` criterion — `Studio 1_0` (344/262), `Kitchen_4` (203/142),
`Kitchen_7` (198/142) — the other 5 (`Bathroom_2`, `Bedroom 2_3`, `Ensuite_5`, `Bedroom 2_6`, `Ensuite_8`)
pass.

**D. Final outcome.** Reference A Fail; Candidate B Fail — the expected result stated in the pre-rerun
record: 3-B0 already failed 4 of 8 rooms, and the clamp fixes a reporting refusal at the control-law
boundary, not the building's overheating. **This FAIL is a valid acceptance result, not a defect.** No
production code was changed to obtain it, and none should be — the model is a weather-driven failing case by
construction (see above).

**Deltas.** 8 rooms, 70 080 hourly values each comparison, from `extract/comparison.md`
(`--delta 3-B0:3-B4`, `--delta 1a:3-B4`):

| room | 3-B0 → 3-B4 mean ΔRT (K) | 3-B0 → 3-B4 max \|ΔRT\| (K) | 1a → 3-B4 mean ΔRT (K) | 1a → 3-B4 max \|ΔRT\| (K) |
|---|---|---|---|---|
| Bathroom_2 | −0.783 | 4.12 | −0.562 | 3.49 |
| Bedroom 2_3 | −0.001 | 1.02 | +0.224 | 2.71 |
| Bedroom 2_6 | +0.004 | 0.96 | +0.212 | 2.70 |
| Ensuite_5 | −0.108 | 3.19 | −0.505 | 3.16 |
| Ensuite_8 | −0.202 | 3.45 | −0.947 | 3.50 |
| Kitchen_4 | −0.072 | 0.95 | +0.406 | 2.63 |
| Kitchen_7 | −0.083 | 0.94 | +0.297 | 2.21 |
| Studio 1_0 | −0.013 | 1.29 | +0.020 | 3.91 |

3-B0 → 3-B4 is the comparison that isolates what the selected-product cooling module changed relative to
the Parity foundation control (both already on the merged clamp) — small (≤3.45 K max, most rooms <1.3 K),
consistent with Selected-product cooling only engaging the same recirculation branch Parity already grounds.

**`Count_Clamped` and `MaximumClampedExcursion_Lps` are not surfaced anywhere in the production route's own
report.** Confirmed by inspecting this rerun's own `Iteration3-Review.json`: it carries `Count_OutOfRange`
(0 for all three units) but no `Count_Clamped` or `MaximumClampedExcursion_Lps` field at all - the production
report layer never reads them, so they exist on `RecirculationCoolingResult` but are not aggregated or
persisted anywhere production writes to. The only way to see them is to call the production reader directly,
which is what the read-only replay below does (no persistence code was added to production, or anywhere
else, to obtain this).

**Coil replay (`prod B4 resolve=model stop=gen coolev=1`, same static no-IZAM fixture the 2026-09-15
investigation used).** For the air system that carried the historical refusal (`dd8a4594-…`, MVHR-02 in
today's naming): `Count_Clamped = 2`, `MaximumClampedExcursion_Lps = 0.050278` — magnitude matches the
investigation's measured `0.0503` almost exactly (count is 2, not the single hour named in the refusal
message; a second, smaller near-ceiling excursion this year was below the printing cutoff of the original
hour-by-hour table and is not itself surprising given the mechanism is weather-driven and stochastic in
timing).

**A genuine, unrelated observation from the same replay, recorded rather than acted on.** One of the other
two air systems (`10fda386-…`) showed `Count_Clamped = 6015` — the great majority of its "below the cooling
gate" hours — with `MaximumClampedExcursion_Lps = 0.000587`. This is not the ramp-overshoot mechanism this
clamp was built for: the magnitude is ~170× smaller than the `0.1` l/s clamp bound and consistent with
floating-point-scale noise at the `36` l/s floor (the pre-clamp `cooling.py` characterisation already showed
thousands of hours legitimately sitting at-or-near that floor for every unit; this branch's specific
arithmetic evidently lands fractionally under `36.0` rather than exactly on it). It does not change any
TM59 result, any refusal, or this rerun's outcome, and **no production code was changed for it** — recorded
here per the stop-condition this rerun was run under, as something to characterise further before treating
`Count_Clamped` as a proxy for "genuine near-boundary events" without also checking excursion magnitude.

**Harness provenance note.** The preserved read-only evidence-extraction harness
(`C:\TasOut\parto-final-real-project\h\p0.csproj` and `tool\tool.csproj`) had three machine-specific paths
hardcoded from the machine the 2026-09-15 acceptance ran on (`<user>\...`), plus a stale
prebuilt `p0.exe` whose `deps.json` predated the `SAM.Analytical.UI.WPF` reference in its own `Program.cs` /
`Prod.cs`. Both were repaired in place (paths repointed to this machine, `p0.exe` rebuilt) — this is harness
plumbing only, explicitly marked "NOT production code" in its own source, not a change to anything under
acceptance.

### Notes for whoever repeats this

- **Provenance travels by commit, not by hash.** `build\binaries.txt`'s hashes are SHA-256 truncated to 16
  hex characters, confirmed by re-hashing the acceptance binaries preserved in
  `h\bin\Release\net8.0-windows` — those preserved binaries *are* the acceptance binaries, bit for bit, so a
  replay should run on them. But the build path is embedded in the assembly, so rebuilding the identical
  commits on another machine gives different bytes (the build is byte-reproducible only *per machine*).
  Verify by commit and by the preserved binaries; never by re-hashing a local rebuild.
- **Identify the source model by its SHA-256, not by a path** — it is filed under a different folder name on
  each machine, and is absent from the second one except as the read-only `00-source` copy.
- Iteration 3 can only be paired against **1a**, in the **same session**, over the **full year 1–365**
  (`SAM_UI\…\Query\PartOIteration3Eligibility.cs`). A reopened project may review an existing pairing, never
  run a new one.
- Re-preparing a 1a-prepared model at 1b carries 1a's MVHR objects along. **1b must start from the clean
  authored `.sam`**, which is what this run did.

---

## TM59 Criterion 1 (natural ventilation) — annual-vs-summer hour-basis defect, found and fixed (2026-09-16)

A follow-up to the 3-B4 rerun recorded elsewhere (SAM#119, open at the time of this fix): independent review
identified that natural-ventilation Criterion 1 Pass/Fail was decided from the full-YEAR occupied-hours basis
instead of TM59:2017's own May-September basis. Confirmed by direct source inspection, fixed with the
smallest change that closes it, and verified not to affect the mechanical-route 3-B4 result above.

**The defect.** `TMExtendedResult.Criterion1` (`SAM.Analytical\Classes\Result\TM\TMExtendedResult.cs:240-263`)
compares `GetOccupiedHoursExceedingComfortRange()` against `MaxExceedableHours` (`OccupiedHours * 0.03`,
`:50-56`) — both computed from the full-year `OccupiedHourIndices` the result was constructed with, no date
filter. `TM59NaturalVentilationExtendedResult` and `TM59NaturalVentilationBedroomExtendedResult` never
overrode `Criterion1`, so they inherited this full-year computation — even though
`TM59NaturalVentilationExtendedResult` already carried the correct summer-restricted denominator,
`GetSummerOccupiedHours()` / `GetSummerMaxExceedableHours()` (`TM59NaturalVentilationExtendedResult.cs:36-64`,
filtering to `HourOfYear.SummerStartIndex..SummerEndIndex` — May-September, matching TAS).

`TM59AssessmentReport.NaturalVentilationChecksFor` (`TM59AssessmentReport.cs:256-299`) already trusted that
summer-restricted denominator for its displayed `Limit`, but read the **annual**, unfiltered
`GetOccupiedHoursExceedingComfortRange()` for its displayed `Actual`, and `tMResult.Pass` (the unfixed,
full-year `Criterion1`) for `ComplianceStatus` — a report row could show three different bases (annual
Actual, summer Limit, annual verdict) that were never required to agree with one another.

**The fix (`SAM.Analytical`).**
- `TM59NaturalVentilationExtendedResult` gained `GetSummerOccupiedHourIndicesExceedingComfortRange()` /
  `GetSummerOccupiedHoursExceedingComfortRange()` (the existing `GetOccupiedHourIndicesExceedingComfortRange()`
  restricted to `GetSummerOccupiedHourIndices()`), and now **overrides** `Criterion1` to decide Pass/Fail from
  these summer-restricted figures — mirroring the base implementation's own comparison (0 exceeding hours
  passes trivially; otherwise strictly `<`, never `<=`) on the summer basis instead of the annual one.
  `TM59NaturalVentilationBedroomExtendedResult` needed no separate change: it does not override `Criterion1`
  either, so it inherits the fix.
- `TM59AssessmentReport.NaturalVentilationChecksFor`'s `actual_Criterion1` now reads the same new summer
  getter instead of the annual one, so Actual, Limit and the verdict all read from one authority.
- `Query.Simplify`'s natural-ventilation and bedroom branches now populate the flattened plain result's
  `HoursExceedingComfortRange` from the same summer getter, so a caller using
  `TM59AssessmentCalculator.Calculate(extended: false)` — the default — sees the identical,
  internally-consistent figures the extended branch does, not a stale annual copy surviving flattening.

**Regression, proving the precise defect.**
`SAM.Tests\TM59NaturalVentilationCriterion1SeasonalBasisTests.Criterion1_IsDecidedBySummerOccupiedHours_NotAnnual`
(3 cases) constructs a fixture with 600 non-summer + 100 summer occupied hours (annual limit 21, summer limit
3 — the two limits can never coincide) and moves exceedance hours between the two periods:
- 4 summer exceedances, 0 non-summer: old code passes (4 < 21); the fix correctly fails (4 < 3 is false) — a
  real May-September failure the annual basis hid.
- 2 summer + 20 non-summer exceedances: old code fails (22 < 21 is false); the fix correctly passes (2 < 3) —
  a room wrongly failed by exceedances TM59 was never meant to count.
- Exactly 3 summer exceedances (equal to the summer limit): old code passes (3 < 21); the fix correctly fails
  (3 < 3 is false), preserving the implementation's existing strict comparison.

All three fail against the pre-fix code and pass against the fix.

**Verified not to affect the 3-B4 real-project result above.** `TMOverheatingCalculator.Calculate_TM59`
(`TMOverheatingCalculator.cs:288-383`) branches per space onto either `TM59NaturalVentilationExtendedResult` /
`TM59NaturalVentilationBedroomExtendedResult` (natural route) or `TM59MechanicalVentilationExtendedResult`
(mechanical route) — the two never share a `Criterion1` implementation.
`TM59MechanicalVentilationExtendedResult.Criterion1` (`TM59MechanicalVentilationExtendedResult.cs:63-69`) is
its own, separate override (`GetHoursNumberExceeding26() < MaxExceedableHours`, both annual by TM59:2017's
own design for the mechanical `>26°C` check) — untouched by this fix. Every one of the 3-B4 real project's 8
assessed rooms is on that mechanical route, so this fix changes none of its reported numbers. **No licensed
rerun was required or performed for this fix.**

**Test results.** `SAM.Tests` rebuilt explicitly (not `--no-build`) and run in full: **2154 passed, 0 failed,
0 skipped** — no regression anywhere in the suite, including `TM59AssessmentReportTests`,
`TM59SimplifyTests` (one assertion updated to read the new summer getter, matching the three figures already
beside it — not reopening the rotation-order defect that test otherwise pins) and `TMOverheatingCalculatorTests`.

**A related instance found but NOT fixed here, out of this fix's scope — since fixed (`SAM_Tas`#62).**
`SAM_Tas\SAM.Analytical.Tas.TM59\Classes\PartODiagnosticLog.cs:646` and `:655` (`SetCriterionSpecificFields`)
logged `hoursExceedingComfortRange` for the extended natural/bedroom branches from the same annual
`GetOccupiedHoursExceedingComfortRange()` this fix moved away from, beside the already-summer
`summerOccupiedHours` / `maxExceedableSummerHours` fields on the same record — the identical defect pattern,
in a diagnostic evidence log rather than the assessment or report. It never fed any Pass/Fail decision.
Left unfixed here because it is a separate repo (`SAM_Tas`) with its own prebuilt-DLL/TPD build chain,
outside this fix's stated scope; flagged for a dedicated follow-up, and fixed the same day in
[SAM_Tas#62](https://github.com/SAM-BIM/SAM_Tas/pull/62) once this fix's `GetSummerOccupiedHoursExceedingComfortRange()`
was available in a merged `SAM.Analytical.dll`.

---

## B4 close/reopen Review acceptance — PASS 7/7 (2026-09-16), SAM#111 final closeout

**The question.** Does the persisted real-project B4 (selected-product cooling) pairing reopen correctly in
a fresh SAM process, on the current merged binaries, reproducing the same engineering result without
rerunning the ~20-minute annual simulation? This is the last outstanding SAM#111 acceptance gate.

**Pairing.** Reference A vs Candidate B `-It3B4`, behaviour mode `SelectedProductCooling`, 3 cooling
modules, 3 air systems, 8 assessed rooms, 14 ventilation legs — the same pairing recorded under § *3-B4
rerun on the merged clamp* above.

**Result — 7/7 acceptance criteria PASS:**

1. **Restored, not rerun.** `IsRestored = true`; Review reported the reopened persisted pairing; no
   behaviour-mode picker appeared.
2. **Engineering results reproduced exactly.** Reference A FAIL; Candidate B FAIL; identical comparison
   statistics; identical TM59 outcomes; a deep comparison of `Record` and `Comparison` produced zero
   engineering violations.
3. **Ledger survived.** 15/15 stages present and `COMPLETED`.
4. **No refusal.**
5. **No TAS simulation during Review.** Only TSD result-reader processes were observed; no new
   TBD/TPD/TAS3D/TasConv process appeared; the pre-existing TAS-process baseline was empty.
6. **Catalogue provenance revalidated.** Schema `VentilationUnitCatalogue:v1`; expected SHA matched disk.
7. Review completed in seconds, not the ~20-minute simulation the original run required.

No production code was changed to obtain this result.

**A separate, non-blocking finding from the same reopen — [SAM#122](https://github.com/SAM-BIM/SAM/issues/122).**
The in-session Review that produced the original B4 pairing recorded 52 run-time `Notes`. The persisted
`PartOIteration3Record` does not store them, so the restored Review reconstructs only 1 note and overwrites
the standard `Iteration3-Review.txt`/`.json` archive path with the thinner reconstruction (original review
text ≈37,811 characters; restored ≈26,377 characters). Calculations, pairing identity, the ledger, TM59
outcomes and comparison statistics are unaffected — only the archived narrative text is thinner after a
reopen. This is an evidence/history-preservation defect, tracked separately, and did **not** fail this
reopen acceptance gate.

**Programme conclusion.** This was the last gate SAM#111 required. Combined with the human decision to move
the remaining manufacturer-ventilation behaviour (certified heat recovery, fan SFP/power, B1/B2,
manufacturer bypass/B3 — blocked on external evidence, not on anything left to build here) to
[SAM#123](https://github.com/SAM-BIM/SAM/issues/123), SAM#111's own closure rule is satisfied and the issue
is closed. SAM#111 closed after the cross-repository reconciliation.
