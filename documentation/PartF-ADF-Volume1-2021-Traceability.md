<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Approved Document F, Volume 1: Dwellings — regulatory traceability

**Source**: Approved Document F — Ventilation, Volume 1: Dwellings, **2021 edition, for use in England**,
in effect from 15 June 2022. Requirement F1: Means of ventilation.

**Scope of this matrix**: every requirement relevant to a **new dwelling** served by **mechanical
ventilation with heat recovery** (paragraphs 1.67 to 1.73). Requirements that belong only to natural
ventilation, to continuous mechanical extract ventilation as a system type, or to Section 3 (work on
existing dwellings) are listed once at the end as out of scope.

**This is an assessment, not a certification.** Software cannot certify compliance with the Building
Regulations. What SAM records is which requirements were calculated, which were verified from the model
geometry, which a person confirmed, and which remain open. Compliance is demonstrated to a building
control body, on the complete design and the built work, by a suitably qualified person.

## How to read the Status column

| Status | Meaning |
|---|---|
| **Calculated** | SAM derives the answer arithmetically from the Approved Document and the model. |
| **Geometry** | SAM reads the answer from the analytical model's geometry or adjacency. |
| **Manual** | The requirement is real but no analytical model contains the answer. Recorded as an open check that a person resolves and signs. Never silently passed. |
| **Commissioning** | Answered by site evidence recorded against the dwelling. |
| **Out of scope** | Not implemented. Stated as a limitation rather than assumed satisfied. |

---

## 1. Extract ventilation

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 1.17 (p8) | Extract ventilation **to the outside** in kitchens, utility rooms, bathrooms and sanitary accommodation | `PartFCalculator.BuildTerminals` creates a `GeneralExtract` terminal for every wet room and a `LocalKitchenExtract` terminal for every cooking space | Check "Extract ventilation provided in every required space"; terminal schedule | Calculated + Manual | That each terminal's duct actually reaches outside air is a construction fact; confirm it |
| 1.17a (p8) | Extract from the room containing the **cooking function** | `PartFTerminalRole.LocalKitchenExtract`, a terminal role of its own, held separately from general wet-room extract | Check "Local kitchen extract from the room containing the cooking function"; local kitchen extract schedule | Calculated | Record the actual arrangement on `SpaceParameter.PartFLocalExtractMethod` |
| 1.18 (p8) | Extract may be intermittent or continuous | `PartFExtractMethod` selects Table 1.1 or Table 1.2 and whether the terminal joins the balanced continuous flow | `PartFVentilationTerminalRequirement.IsInBalancedFlow`, `OperatingMode` | Calculated | — |
| 1.19, Table 1.1 (p8) | Intermittent rates: kitchen 30 (hood to outside) / 60 (no hood or not to outside), utility 30, bathroom 15, sanitary 6 l/s | `PartFData.IntermittentKitchenRateWithCookerHood_Lps` / `…WithoutCookerHood_Lps`; `PartFCategory.IntermittentExtractRate_Lps` in the rule set | Terminal `MinimumRequiredFlowRate_Lps` and `HighFlowRate_Lps` for an intermittent method | Calculated | — |
| 1.20 (p8) | Extract terminals and fans, **not** cooker hoods: as high as practicable, max 400 mm below ceiling | Structured check | Check "Extract terminals installed high in the room" | Manual | SAM models the room, not the grille position on its wall |
| 1.21 (p8) | Cooker hood extracting outside: 650–750 mm above the hob unless the manufacturer specifies otherwise | Structured check, `NotApplicable` where no cooker hood is recorded | Check "Cooker hood height above the hob surface" | Manual | Confirm from the kitchen layout |
| Diagram 1.1 note (p9) | The cooker hood should span at least the full width of the cooker | Not modelled | — | Manual | Confirm from the kitchen layout |
| Diagram 1.2 note 1 (p9) | **A recirculating cooker hood on its own does not comply with Part F** | `PartFExtractMethod.RecirculatingCookerHood` → `Fail`, contributes to no design flow | Terminal status `Fail`; overall status `Fail` | Calculated | — |
| 1.22, Table 1.2 (p10) | Continuous system minimum **high** rates, **per room**: kitchen 13, utility 8, bathroom 8, sanitary 6 l/s | `PartFCategory.MinFlowRate_Lps` from the rule set; a cooking space carries the kitchen figure on its local extract terminal. Assessed room by room on the HIGH rate, never summed into the continuous dwelling rate | Check "Each extract room reaches its Table 1.2 minimum high rate" | Calculated | — |
| Table 1.2 continuous column (p10) | The **total** of all extract on its continuous rate ≥ the whole dwelling rate. A requirement on the total only — **not** on each room, and **not** that the total reach the sum of the per-room high-rate minimums | `AllocateContinuousExtract`: each terminal takes its minimum then the surplus is split by the selected strategy; where the minimums total more than the whole dwelling rate, `AllocateContinuousExtractBelowMinimumTotal` shares the whole dwelling rate pro-rata and each room boosts instead | Check "Total continuous extract reaches the whole dwelling ventilation rate" | Calculated | — |
| Table 1.2 note 1 (p10) | Where a room's continuous rate is already ≥ its own minimum high rate, **no extra ventilation is needed for that room**. Note 1 is per room and is not a licence to size the continuous dwelling rate from the summed minimums | `HighFlowRate_Lps = max(continuous, minimum)`; `HighRateIncreaseRequired` records which terminals must boost | Terminal schedule | Calculated | — |

## 2. Whole dwelling ventilation

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 1.23 (p10) | Supply air by continuous supply fans or background ventilators | MVHR: continuous supply fans. Background ventilators are excluded by 1.72 | System type on the result | Calculated | — |
| 1.24a (p10) | ≥ 0.3 l/s per m² of internal floor area, **all floors** | `PartFData.AreaRate_LpsPerM2` × `InternalFloorArea_M2` | `AreaBasedRate_Lps`; governing calculation in the report | Calculated | — |
| 1.24b, Table 1.3 (p10) | Minimum by bedroom count: 19/25/31/37/43 l/s for 1–5 bedrooms | `PartFData.GetWholeDwellingRates_Lps` | `BedroomBasedRate_Lps` | Calculated | — |
| Table 1.3 note 1 (p10) | **One habitable room → 13 l/s**, replacing the bedroom rate | `GetBedroomOrHabitableRate_Lps`, keyed off the habitable **room** count | `OneHabitableRoomRuleApplied`; reported as a note | Calculated | — |
| Table 1.3 note 2 (p10) | +6 l/s for each bedroom above 5 | `PartFData.IncrementAbove5` | `BedroomBasedRate_Lps` | Calculated | — |
| 1.24 (p10) | The rate must meet **both** conditions, so the greater applies | `ContinuousDesignSystemRate = max(bedroomOrHabitable, area)` — those two terms and no others | Governing calculation block | Calculated | — |
| Appendix A (p36) | **Habitable room**: not *solely* a kitchen, utility room, bathroom, cellar or sanitary accommodation | `SpaceSemantics.IsHabitable`; a studio and an open-plan living kitchen are habitable | `HabitableRoomCount`, `HabitableRoomNames` | Calculated | — |
| Appendix A (p38) | **Wet room**: produces significant airborne moisture; sanitary accommodation counts as one | `SpaceSemantics.IsWetRoom` | Terminal roles | Calculated | — |

## 3. Supply, transfer and internal doors

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 1.67 (p16) | Each habitable room has **mechanical supply** | A `Supply` terminal per habitable room | Check "Mechanical supply to every habitable room" | Calculated | — |
| 1.67 (p16) | Total supply distributed **proportionately to the volume of each habitable room** | `AllocateContinuousSupply` | Check "Supply distributed in proportion to habitable room volume" | Calculated | — |
| 1.68 (p16) | Supply terminals located and directed to avoid draughts | Structured check | Check "Supply terminals located and directed to avoid draughts" | Manual | Confirm from the ventilation layout |
| 1.69 (p16) | Minimum total continuous MVHR rate = the whole dwelling rate | `ContinuousDesignSystemRate_Lps`; supply and extract totals both checked against it | Check "Continuous supply and extract are balanced" | Calculated | — |
| 1.70 (p17) | Each wet room reaches its Table 1.2 minimum **high** rate | See Table 1.2 above. Assessed on `HighFlowRate_Lps`, independently of the whole-dwelling continuous total | Check "Each extract room reaches its Table 1.2 minimum high rate" | Calculated | — |
| 1.71 (p17) | Moist air from wet rooms not recirculated to habitable rooms | Structured check; BS EN 13141-8 Class U4 named as the relevant standard | Check "Moist air from the wet rooms is not recirculated…" | Manual | Confirm from the unit specification |
| 1.72 (p17) | **Background ventilators not installed with MVHR** | Structured check | Check "Background ventilators are not installed with MVHR" | Manual | SAM does not model trickle ventilators; confirm none is specified |
| **1.25 (p10)** | Internal doors provide a minimum free area **equivalent to a 10 mm undercut in a 760 mm wide door** = **7,600 mm²**; undercut **10 mm above a fitted floor finish** or **20 mm above an unfinished floor surface** | `PartFDoorTransferData`; constants `ReferenceDoorWidth_mm`, `ReferenceUndercutHeight_mm`, `UndercutHeightBeforeFloorFinish_mm`, `NominalEquivalentFreeArea_mm2`. **Every** internal door is assessed, not only loaded ones | Door undercut and free area schedule | Calculated (requirement) + Manual (provision) | The provided undercut is an engineering input; an analytical model does not represent the gap under a door leaf |
| 1.25 (p10) | Air flows **through** the dwelling. The requirement is on free AREA; **no flow rate is prescribed for any individual door**, so the per-door l/s are calculated routing and nothing is assessed against them | `PartFAirflowNetwork`: graph of the dwelling's internal separating elements; proportional shortest-path allocation, exact on a tree | Checks "Internal doors allow air to flow through the dwelling" and "Transfer air routes connect the supply spaces to the extract locations"; transfer air schedule | Geometry + Calculated | Where the topology has a loop the split is an engineering decision and is reported as such |

## 4. Purge ventilation

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 1.26 (p11) | Purge ventilation in **each habitable room** | `PartFPurgeAssessor`, one record per habitable room | Purge ventilation assessment | Calculated | — |
| 1.27 (p11) | ≥ **four air changes per hour**, **directly to the outside** | `RequiredAirChangesPerHour = 4`; `RequiredPurgeRate_Lps`; `IsPurgeRouteDirectlyOutside` from the room's external apertures | Purge schedule | Calculated + Geometry | — |
| 1.28 (p11) | Delivered by openings or by a mechanical extract system | `PartFPurgeMethod` | Purge schedule | Manual | Record the method |
| 1.29, Table 1.4 (p11) | Minimum opening areas: hinged/pivot 15–30° → 1/10 of floor area; ≥30°, opening sash, external door → 1/20 | `PartFPurgeVentilationData.Table1_4AreaFraction` | `RequiredOpeningArea_M2` | Calculated | Requires the opening type/angle, which is a product property |
| 1.30 (p11) | Smaller openings only with expert advice | Reported in the failure diagnostic | Purge diagnostic | Manual | — |
| 1.31 (p11) | Hinged/pivot **< 15° is not suitable** for purge | `PartFPurgeOpeningType.HingedOrPivotUnder15Degrees` → `Fail` | Purge status | Calculated | — |
| 1.73 (p17) | MVHR purge: follow 1.26–1.31 | Same assessment | Purge schedule | Calculated | — |
| 0.21 (p4) | **Part O may require a higher purge standard**; the higher applies | `PartOInteractionNote`, reported separately and never folded into the Part F figure | Purge schedule footer; check notes | Manual | Assess Part O separately |
| — | The model's window area is **not** the openable area | `ExternalApertureArea_M2` reported as context only; openable area is an explicit input | Purge diagnostic | Geometry (context) | Enter the openable area |

## 5. System design, noise, controls, installation

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 1.5–1.7 (p6) | Designed and installed to minimise noise; fans not near maximum in normal mode; 30 dB L<sub>Aeq,T</sub> noise-sensitive / 45 dB less sensitive | Structured check quoting the levels | Check "System designed and installed to minimise noise" | Manual | Confirm from the equipment schedule / acoustic assessment |
| 1.8 (p7), 1.75 (p17) | Reasonable access for maintenance: filters, fans, coils, duct cleaning points, plant | Structured check | Check "Reasonable access for maintenance" | Manual | Confirm from the plant layout |
| 1.33, 1.35–1.37 (p12) | Controllable; continuous fans run without intervention; manual high-rate controls local to the spaces served; humidity sensors **not** for sanitary accommodation; automatic controls have manual override | Structured check quoting each condition | Check "Ventilation controls" | Manual | Confirm from the controls specification |
| 1.74–1.83 (pp17–18) | Installation: rigid ducts preferred; flexible only for final connections ≤1.5 m, taut, to BSRIA BG 43/2013; ducts sized for the flow; terminal free area ≥90% of its duct; connections mechanically secured and sealed | Structured check quoting each condition | Check "Installation of the ventilation system" | Manual | Confirm from the ventilation drawings and Appendix C Part 2a |
| 1.32, Table 1.5 (pp11–12) | Performance testing standards (BS EN 13141 parts) | Named in the 1.71 check evidence | Check evidence | Manual | Confirm from product data |

## 6. External pollutants (Section 2)

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 2.1, Table 2.1 (p19) | Section 2 applies where pollutant limits are exceeded or the dwelling is near a significant local source | Named in the check requirement | Check "Outdoor air intake location" | Manual | Site air quality assessment |
| 2.2–2.6 (p20) | Intakes away from pollution sources; high and on the less polluted side near busy urban roads; out of courtyards where practicable | Structured check | Check "Outdoor air intake location" | Manual | Confirm from the facade layout |
| 2.7–2.9 (p20) | Exhaust outlets minimise re-entry; downwind of intakes; not into courtyards, enclosures or architectural screens | Structured check | Check "Exhaust outlet location" | Manual | Confirm from the facade layout |

## 7. Commissioning and information (Section 4, Appendix C)

| ADF reference | Requirement | SAM implementation | Evidence | Status | Remaining action |
|---|---|---|---|---|---|
| 4.1 (p31) | System commissioned; commissioning notice given to the building control body | `PartFCommissioningData.CommissioningNoticeGiven` | Check "System commissioned and commissioning notice given" | Commissioning | — |
| 4.2 (p31) | Air flow rates **measured** in new dwellings; notice of measured rates given | `AirFlowRateNoticeGiven`; measured totals and per-terminal measured rates | Check "Air flow rates measured and notice given" | Commissioning | — |
| 4.3 (p31) | Commissioning sheets include at least Appendix C Part 3 | `PartFCommissioningData` fields mirror Part 3 | Commissioning status block | Commissioning | — |
| 4.7–4.8 (p32) | Ductwork and terminal inspections; system balanced to design flow at each terminal | `InstallationChecks` on the commissioning record | Recorded checks | Commissioning | — |
| 4.9 (p33) | Test all fans, including cooker hoods | Per-terminal `MeasuredContinuousFlowRate_Lps` / `MeasuredHighFlowRate_Lps` | Terminal schedule | Commissioning | — |
| 4.10 (p33) | Calibrated device with proprietary hood, ±5%, UKAS-calibrated within 12 months; transfer devices open, doors and windows closed | `MeasurementEquipment`, `CalibrationDate`; conditions quoted in the check requirement | Check "Air flow rates measured and notice given" | Commissioning | — |
| 4.11 (p34) | Each control function tested and labelled | `InstallationChecks` | Recorded checks | Commissioning | — |
| 4.13–4.17 (p34) | Operating and maintenance information to the building owner, including design flow rates | `OperatingAndMaintenanceInformationIssued` | Check "Operating and maintenance information issued…" | Commissioning | — |
| 4.18–4.19 (p35) | Home User Guide for a new dwelling | `HomeUserGuideIssued` | Check "Home User Guide provided" | Commissioning | — |
| **C2 (p42)** | **Measured rate for each fan ≥ its design value**; otherwise adjust and remeasure | Per-terminal comparison; design values never overwritten by measured ones | Check "Measured air flow rates meet the design air flow rates" | Commissioning | — |

## 8. Deliberately out of scope

| ADF reference | Requirement | Why | Status |
|---|---|---|---|
| Section 3 (pp21–27) | Work on existing dwellings | New dwellings only; no new/existing switch exists, by design | Out of scope |
| 1.47–1.59, Table 1.1, Table 1.7 (pp14–15) | Natural ventilation as a **system type**; background ventilator equivalent areas | Only MVHR is sized. Table 1.1 **rates** are still applied to an individual intermittent device such as a cooker hood | Out of scope |
| 1.60–1.66 (p16) | Continuous mechanical extract ventilation as a system type | Only MVHR is sized | Out of scope |
| 1.38–1.41 (pp12–13) | Dwellings with basements | Not implemented | Out of scope |
| 1.42–1.44 (p13) | Ventilating a habitable room through another room; permanent opening ≥ 1/20 of the combined floor area | **Detected and reported** where an internal habitable room is found, but the 1/20 opening is not sized | Out of scope (reported) |
| 1.15–1.16 (p8), 1.64 (p16) | Equivalent area of background ventilators | Excluded from MVHR by 1.72 | Out of scope |
| Appendix B (pp39–41) | Performance-based ventilation criteria | The prescriptive route is implemented, not the performance route | Out of scope |
| Appendix D (pp47–48) | Existing-dwelling checklist | Section 3 | Out of scope |
| ADF 2026 edition | — | Explicitly excluded from this work; requirements from the two editions are never combined | Out of scope |

---

## Notes on deliberate SAM conventions

**1. Table 1.2 is two separate requirements, and they are implemented separately.** Table 1.2 sets:

- a requirement on the **total**: the sum of continuous extract must reach the whole dwelling ventilation
  rate; and
- a requirement on **each room**: every kitchen, utility room, bathroom and sanitary accommodation must
  reach its own applicable minimum **high** rate.

Neither implies the other, and in particular **nothing in the Approved Document requires the continuous
dwelling rate to reach the sum of the per-room high-rate minimums.** Note 1 says only that a room whose
continuous rate already meets its own minimum needs no further increase for that room. Accordingly:

```
WholeDwellingContinuousDesignRate = max(BedroomOrHabitableRate, 0.3 l/s/m² × InternalFloorArea)

and separately, for each extract room:  HighFlowRate ≥ its Table 1.2 minimum
```

An earlier version of this implementation carried a third term — the sum of the Table 1.2 minimums — inside
the governing rate, as a conservative SAM convention. **That term has been removed.** Summing per-room
high-rate minima into the continuous rate systematically oversizes normal continuous operation in any
dwelling with several wet rooms: a 20 m² studio with a kitchen, bathroom, WC and utility room would have
been sized at 35 l/s continuously against a whole dwelling rate of 13 l/s, nearly three times what the
Approved Document asks of it.

Where the sum of the per-room minimums does exceed the whole dwelling rate, the continuous total stays at
the whole dwelling rate and is shared between the extract terminals, and each room reaches its own figure by
boosting to its high rate. `HighRateIncreaseRequired` records every terminal that has to boost. The sum is
still reported, under "Table 1.2 high-rate minimums" in the governing-calculation block, so a reader can see
it and see that it did not govern.

*Separate point, unchanged*: a cooking space carries the Table 1.2 kitchen minimum of 13 l/s on its own
terminal, so it contributes to the reported per-room minimum total and must reach 13 l/s at its high rate.
That is a direct result of modelling an extract that previously was not modelled at all.

**2. The setback rate.** Neither the 2021 nor the 2026 edition specifies a reduced operating rate for MVHR.
`SetbackFlowRateFactor` (0.30 by default) is a **SAM reduced-operation convention, not a regulatory value**.
It is applied only after every regulatory minimum has been established at the continuous design condition,
never reduces or replaces it, and is never checked against Table 1.2. It is called *setback* rather than
*background* because in Approved Document F a background ventilator is a trickle ventilator and "whole
dwelling (general) ventilation" is the continuous requirement.

**3. The extract allocation strategy.** Approved Document F fixes only that each wet room reaches its
Table 1.2 minimum at the high rate (1.70) and that the continuous total reaches the whole dwelling rate
(Table 1.2). The split between terminals is an **engineering strategy**, named on the rule set and recorded
on every result: `MinimumFirstCookingPriority` (default) or `VolumeWeighted` (the pre-terminal SAM
behaviour). A user-defined split is also supported, per terminal.

For the studio reference case the default strategy gives 22 l/s local kitchen extract and 8 l/s bathroom
extract from a 30 l/s dwelling rate. Both already meet their Table 1.2 minimums (22 ≥ 13, 8 ≥ 8), so no
boost is required for that design — but the 22/8 split itself is a SAM allocation strategy, not a value the
Approved Document prescribes.

**4. Transfer air flows through internal doors are calculated routing, not a requirement.** Paragraph 1.25
requires a free **area** through an internal door and prescribes no flow rate for any individual door. The
l/s figures on `PartFDoorTransferData`, in the schematic and in the "Internal transfer air routing
(calculated)" schedule are SAM's airflow-network result, obtained by conserving air across the dwelling.
Nothing is assessed against them and no door passes or fails on their value; the paragraph 1.25 assessment
is entirely on free area.

**5. Required, proposed and provided are three separate records on a terminal.** `RequiredHighFlowRate_Lps`
is what the Approved Document asks of the room. `ProposedExtractMethod` is what SAM generated to carry it,
from the system type and the room. `ProvidedExtractMethod` is what the design actually records, and stays
`NotSpecified` until the model or a person supplies it; `ProvisionStatus` reports that. A cooking space
discovered from the room semantics therefore always gets a required terminal, and the calculator may
propose and size a continuous MVHR kitchen terminal for it, but the compliance result never treats that
terminal as physically provided. This matters most for kitchens, where extract must be to the outside and a
recirculating cooker hood alone is not an acceptable external extract provision (Diagram 1.2 note 1, p9).

**6. A calculated failure is never overridable into a pass.** `PartFComplianceCheck.CalculatedStatus` holds
what SAM calculated and is never rewritten. A person's answer is applied through `ApplyUserResolution`,
which records `UserEvidence`, `AlternativeComplianceMethod`, `OverrideReason`, `ConfirmedBy` and
`ConfirmationDate` — and, against a calculated failure, refuses a pass, sending the check to
`AlternativeSolutionPendingApproval` where an alternative method has been recorded and to
`EngineeringReviewRequired` otherwise. The routes out of a calculated failure are: correct the input and
recalculate; supply the missing measured or geometric evidence; record an alternative compliance method; or
refer the item for engineering or building-control review.
