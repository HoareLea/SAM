# Reporting Phase 2 — Space Design Load Summary: result-authority audit and design gate

Status: **investigation + design-definition (26 Sep 2026).** The audit itself changed no product code. Since then,
B0 has been fixed by [SAM#147](https://github.com/SAM-BIM/SAM/pull/147) (merge `00db4b85`, see §3.1), and B1–B5 by
PR2A (PR2A-1 = SAM#153, merge `8a22c82b`; PR2A-2 = SAM_Tas#69, merge `46dacf22`; see §3.2). PR2B (typed report
data + collector, SAM#156, merge `9c3b9d6e`) is described in §7.1; PR2C (document + PDF) in §9.1.

```text
Phase 2 result authority: COMPLETE
B0: FIXED (SAM#147, 00db4b85)
B1–B5: FIXED in PR2A-1 (SAM#153, 8a22c82b) + PR2A-2 (SAM_Tas#69, 46dacf22), merged (§3.2)
B6: VERIFIED read-only on an existing real Tas cooling result (pr3\final\bridge.tsd, §3.2); no licensed run needed
PR2B reporting data + collector: MERGED (SAM#156, §7.1)
PR2C Space Design Load Summary document + PDF: IMPLEMENTED (§9.1); SAM_UI command (PR2D): NOT STARTED
```

Phase 2 must report *persisted* results. The audit found that the persisted per-space load results are
not yet authoritative. Reporting must not work around that, so the reporting layer waits for two small
prerequisite fixes, listed at the end.

## 1. Repository state inspected

| Item | State |
|---|---|
| Phase-1 closeout PRs SAM#144, SAM_UI#123, SAM_Deploy#52 | Open during the audit; **merged afterwards**: SAM#144 `a947c5a3`, SAM_UI#123 `6d2f2d6b`, SAM_Deploy#52 `8fe17f64`. |
| SAM | audited at `sow/2026-Q3` `22f9c743`; this record is based on `a947c5a3` (#144, docs only, no code change). |
| SAM_Tas | `sow/2026-Q3` = `aa00ff91` (origin = local, clean). |
| SAM_UI | `sow/2026-Q3` = `7e7de033`. |
| SAM_OpenStudio | read only, for the cross-engine `LoadIndex` comparison. |

No licensed simulation was run. The only Tas process used was a **read-only TSD reader** on a *copy* of an
existing TSD (`C:\TasOut\final1b\open.tsd`), through the `Interop.TSD` API that SAM_Tas uses
(`openReadOnly`, `GetPeakZoneGains`, `GetHourlyZoneResult`, `GetHourlyBuildingResult`).

## 2. The production result path (traced, not inferred)

`SAM_UI Modify.RunWorkflow` → `SAM_Tas WorkflowCalculator`:

1. `Sizing` — `Query.Sizing(TBD, ExcludePositiveInternalGains = true)` writes the TBD zone `maxHeatingLoad` / `maxCoolingLoad`.
2. `Simulate` — full year plus the TBD design days, into the TSD.
3. `Modify.AddResults(TSD, cluster)` → `Convert.ToSAM_Results(SimulationData)`. This gives one Tas `SpaceSimulationResult` per space and
   `LoadType`, matched to the Space **by name**, plus surface results at the peak index.
4. `Modify.UpdateDesignLoads(TBD, cluster)` writes the TBD `maxHeating/CoolingLoad` twice, and does not stamp either copy with provenance:
   - onto the Space, as `SpaceParameter.DesignHeatingLoad` / `DesignCoolingLoad`;
   - onto the result, as `SpaceSimulationResultParameter.DesignLoad`.
5. The model is saved. Only the Part O workflow (`RunPartOSimulation`, `RunPartOIteration3`) stamps
   `SimulationResultProvenance`. The ordinary `RunWorkflow` path does not.

How `ToSAM_Results` chooses a space's result, for each load type:
- `Load` = **max(design-day peak, annual-simulation peak)**. `SizingMethod` records which one won (`HDD`/`CDD` or
  `Simulation`). The losing peak is **discarded**.
- Room state and components are read at the winner's index from the winner's `ZoneData`.
- Cooling results store: DB, resultant, RH, humidity ratio, solar, lighting, occupancy sensible/latent,
  equipment sensible/latent, inf/vent, air movement, BHT, external conduction (opaque, glazing), aperture flows,
  pollutant.
- Heating results store only: DB, resultant, humidity ratio, inf/vent, air movement, BHT, external conduction
  (opaque, glazing).
- Outdoor T / RH are stored as `DesignDayTemperature` / `DesignDayRelativeHumidity` **only when `Simulation` wins**.
  The code carries a TODO for the design-day case.
- Overheating counts and the min/max dry bulb are copied onto both results.

Freshness and provenance:
- `Result.DateTime` is when the result object was converted. It is persisted and deterministic on reopen.
- There is no link from a result to its TSD except `SimulationResultProvenance`, and that exists on Part O models only.
- Results are removed only by the explicit Remove Results command, so they survive later model edits.

## 3. Blockers (defects on the authoritative path)

| # | Where | Defect | Evidence | Blocks |
|---|---|---|---|---|
| **B0** — **FIXED** (SAM#147, `00db4b85`; §3.1) | SAM.Core parameter sets / `SpaceParameter.DesignHeating/CoolingLoad` | A Space can carry **several `SAM.Analytical` ParameterSets**, each with a `Design Heating Load`. One more is appended per run made with a different build. `TryGetValue` takes the set with the current assembly GUID (`e5c2659a…`), else the first set named `SAM.Analytical`, else the **first** set containing the name. A file with no current-GUID set therefore reads the **oldest** value. | `final1b/open_out.sam` Bathroom_2: sets `cc94e7a1` = 0.0 and `feae3a10` = 1139.87 W; the result's `DesignLoad` = 1139.87 W. The Phase-1 collector (production DLLs) reports **0 W**. Scan of `C:\TasOut`: **51 of 988** spaces-with-results in **17** files read stale. These are exactly the spaces with a non-zero load. | TBD design load, sizing comparison. **Also affected shipped Phase 1.** Root cause and fix: §3.1. |
| **B1** — **FIXED** (PR2A-2; §3.2) | SAM_Tas `Convert/ToSAM/Results.cs`, heating block | When the annual heating peak beats the HDD peak, the code assigns `zoneData_Cooling`, `coolingLoad` and `coolingIndex` instead of the heating variables. The heating result is then labelled `Simulation` but carries the HDD load, state and index, with outdoor T/RH taken at the *annual* index. With no HDD at all, there is no heating result. | Code (present since `ea0b1e8f`). Not triggered in any local fixture, because HDD always won there. | Heating peak, time, conditions, components |
| **B2** — **FIXED** (PR2A-1/2; §3.2) | same, plus `Create.SpaceSimulationResult(ZoneData, index, …)` | A zero peak returns index **0**. Tas hourly arrays are **1-based**, so `GetHourlyZoneResult(0, …)` returns the Tas **−1** "invalid" sentinel. `Load = −1`, every temperature = −1, every component = −1 and `LoadIndex = 0` are then persisted as real numbers. A genuine −1 W component cannot be told apart from the sentinel. | Every free-running space in every local fixture (e.g. Studio 1_0: `Load −1, LoadIndex 0, DB −1 …`). TSD probe: index 0 → all −1 (HDD) or garbage (annual: ext T 65 °C, RH 300 %). | Zero vs missing; every peak field |
| **B3** — **FIXED** (PR2A-1/2; §3.2) | same | The max() discards the loser. The **annual simulated peak is not persisted** whenever the design day wins, and it won in every local fixture. `Load` is therefore *either* a design-day or an annual value. | Bathroom_2: persisted 1139.80 W (HDD) vs annual TSD peak **104.01 W @ 8554**, which is not persisted. | "TSD simulated peak" as its own concept; the TBD-vs-TSD comparison |
| **B4** — **FIXED** (PR2A-1/2; §3.2) | SAM.Analytical `LoadIndex` | The base differs by engine. Tas writes a **1-based** hour of year (verified). OpenStudio writes a **0-based** interval index (`Core.Query.IntervalHourOfYear`). `MaxDryBulbTemperatureIndex` is 0-based in the *same* Tas object. For a design-day winner the index is Tas's internal calendar slot for the design day (the HDD in the fixture occupies hours 1585–1608, i.e. "8 Mar"), not a weather date. | TSD probe: HDD valid only at 1585–1608; annual valid at 1–8760, invalid at 0. OpenStudio `Convert/ToSAM/SimulationResults.cs` + `SAM.Core Query.IntervalHourOfYear`. | Peak time |
| **B5** — **FIXED** (PR2A-2; §3.2) | same | The heating components omit solar, lighting, occupancy and equipment, and the heating result has no RH. For an annual-simulation heating peak those gains can be non-zero, so the stored subset may not close the balance. | Code. The balance does close for HDD, where the internal gains are zero. | Heating breakdown when `Simulation` wins |
| **B6** — **VERIFIED** read-only on a real Tas cooling result (§3.2) | evidence gap | **No persisted cooling peak exists in any local fixture.** Cooling-load composition is therefore unverified: does it include latent, and do the stored terms close? | Fixture scan: `C:\TasOut`, `SAM_daily`, `Nextcloud`, SAM_Validation benchmark — every cooling `Load` is −1 or absent. | Cooling breakdown, cooling time |

### 3.1 B0 closeout (SAM#147, merge `00db4b85`, issue SAM#146)

- **Root cause.** `SAM.Analytical` has had no `[assembly: Guid]` since `49069d9c` (the old value was `fbbd5ce9-…`), so
  its ParameterSet GUID is the per-build MVID. `Modify.Add` matched by GUID only. SAM_Tas `Query.UpdateT3D` adds a
  `SAM.Analytical` zone set on each run, so a run on a new build **appended** a set and wrote that run's design loads
  into it. A reader on another build fell through to the **first** set holding the key, which was the oldest.
- **Fix, central in SAM.Core. No reporting, SAM_UI or SAM_Tas change:**
  - At most one ParameterSet per name on an object. On load, same-name legacy and current sets are merged
    deterministically in stored order, and the **later persisted value wins**.
  - Future `Modify.Add` writes update the existing same-name set rather than appending another.
  - Files without duplicates load unchanged. Explicit 0 stays 0, and a missing value stays missing.
- **Result:**
  - Bathroom_2 now resolves **1139.87 W** (was 0 W), and the Phase-1 Space Assumptions collector reports the same value.
  - Real-fixture rescan (`C:\TasOut`, 198 files, production `Convert.ToSAM`): stale mismatches went from **51 to 0**.
  - The "later wins" rule matched the persisted `SpaceSimulationResult.DesignLoad` in all 229 scanned files.
  - Harness: `evidence/reporting-phase2-gate/harness/pr2a0_rescan.cs.txt`.
- The Phase-1 fix reaches users through a SAM_Deploy SAM-pointer bump.

### 3.2 PR2A closeout — B1–B6 (PR2A-1 SAM#153, PR2A-2 SAM_Tas#69; merged)

Two PRs, merged in this order: **PR2A-1** (SAM, `feature/pr2a1-space-load-peak-2026-09-27`) adds the typed result;
**PR2A-2** (SAM_Tas, `fix/pr2a2-tas-peak-authority-2026-09-27`) fills it. SAM owns the type because the future
reporting collector (SAM.Analytical.Reporting) cannot reference SAM_Tas, and OpenStudio results use the same class.

**Contract (SAM.Analytical).** A `SpaceSimulationResult` (one per space and `LoadType`, as before) carries up to two
`SpaceLoadPeak`s, under the new, appended parameters `SpaceSimulationResultParameter.DesignDayPeak` and `AnnualPeak`:

| `SpaceLoadPeak` member | Meaning |
|---|---|
| `Basis` | `LoadPeakBasis.DesignDay` or `AnnualSimulation`. The two are never merged. |
| `Load` | W, non-negative magnitude, heating and cooling alike. `0` = the simulation ran and there was no demand. |
| `DesignDayName` | Design-day peaks only. |
| `HourOfYear` | **0-based** (0 = 1 Jan 00:00–01:00). Annual peaks only; `null` for a design day and for a zero peak. |
| `HourOfDay` | 0-based (0 = 00:00–01:00). `null` for a zero peak. |
| `TryGetDateTime(year)` | Start of the peak hour. Refuses a design-day peak and a zero peak. |
| `DryBulbTemperature`, `ResultantTemperature`, `RelativeHumidity`, `HumidityRatio` | Room state at the peak (°C, %, kg/kg). |
| `OutdoorDryBulbTemperature`, `OutdoorRelativeHumidity` | From the **results** at the peak. Annual only: the TSD design-day data sets have no building (weather) results, so a design-day peak has none. Never from the model's weather. |
| `Components` (`LoadPeakComponent` → W) | Solar, Lighting, OccupancySensible, EquipmentSensible, InfiltrationVentilation, AirMovement, BuildingHeatTransfer, ExternalConductionOpaque, ExternalConductionGlazing, AirHandlingUnit (sensible); OccupancyLatent, EquipmentLatent. Signed as Tas signs them: **+ gain to room air, − loss**. |

**Availability — no sentinels in the typed result.**
- No `DesignDayPeak` / `AnnualPeak` parameter: unavailable. The engine produced no such peak (e.g. no heating design day
  ran), or the result predates PR2A. A pre-PR2A Tas result has neither; reporting shows it as "re-run".
- `Load = 0` with no `HourOfDay`: a real zero. There is no time, state or component, because Tas reports no peak hour.
- A `null` property or an absent component: not reported. A value of −1 at a real peak hour is kept as −1 (regression test).
- Zero vs missing, concretely (Bathroom_2 / Studio 1_0, `open.tsd`): Studio 1_0 heating DD and annual are
  `load=0, hourOfDay=null`, and the persisted JSON contains no −1. Legacy `Load=-1 LoadIndex=0` is unchanged (below).

**Signs (verified on real Tas output).**
- Heating: `Load = −Σ sensible components`.
- Cooling: `Load = +Σ sensible components`.
- Latent gains are recorded but are outside `Load`. Tas's cooling load is sensible only; Tas reports latent removal
  separately (`latentRemovalLoad`, e.g. 300.9 W at Bedroom 2_3's annual cooling peak), and PR2A does not persist it.
- `AirHandlingUnit` (`AHUGain`) is 0 in every fixture examined. Its role in closure when non-zero is unverified.
- Aperture flows and IZAM channels are **not** in the contract: Tas returns −1 for them at valid peak hours.

**Time.**
- Tas: TSD hourly indices are 1-based (index 0 = no hour; Tas answers it with −1). They are converted once, in SAM_Tas
  `Query.ZeroBasedHourOfYear` / `ZeroBasedHourOfDay`.
- OpenStudio: `Core.Query.IntervalHourOfYear` (interval end → 0-based) already yields SAM's convention. The SAM test
  `AnnualHour_IsZeroBased_AndMapsToTheSameDateForEitherEngine` pins Tas 8554 ≡ OpenStudio "23 Dec 10:00" ≡ SAM 8553.
  The OpenStudio converter does **not** populate the new peaks yet (out of scope). Its results therefore read as
  unavailable in the new contract, and its legacy `LoadIndex` stays 0-based.
- A design-day peak is identified by `Basis` and `DesignDayName` and has only an hour of the day.

**Compatibility.** `Load`, `LoadIndex`, `SizingMethod`, the room/gain parameters, `DesignDayName` and
`DesignDayTemperature`/`RelativeHumidity` remain a projection of the **governing** peak:
- which peak governs: the design day, unless the annual peak is strictly larger (as before);
- `LoadIndex`: still the raw 1-based Tas index;
- zero peak: still the −1 values Tas returns for index 0, with `LoadIndex 0`. The sentinel is confined to these legacy
  values and never reaches the typed peaks;
- `DesignDayTemperature`/`RelativeHumidity`: still the outdoor state at the annual peak, recorded only when it governs.

They are unchanged except in two cases:
- **B1**: an annual heating winner now carries the annual state instead of the design day's.
- A load type with no design day now gets its annual result instead of no result.

Print RDS, the benchmark and every other consumer of those fields (all searched) read what they did. The compatibility
tests pass on the old and the new code alike.

**Status.**

| # | Status | Evidence |
|---|---|---|
| B1 | **FIXED, VERIFIED (synthetic)** | `B1_AnnualHeatingAboveTheDesignDay_GovernsTheHeatingResult`: pre-fix the "Simulation" heating result carried the 50 W design-day load; now 104.01 W at 8554. No real fixture has an annual heating winner. |
| B2 | **FIXED, VERIFIED** | Typed zero vs absent vs genuine −1 tests (SAM + SAM_Tas). Real: Studio 1_0 / Bathroom_2 cooling in `open.tsd` persist `load=0, no hour`. |
| B3 | **FIXED, VERIFIED on real data** | Bathroom_2: DD 1139.796 W and annual 104.010 W both survive TSD → `.sam` copy → reopen. |
| B4 | **FIXED, VERIFIED** | Tas 1608 → DD hour 23, no date; Tas 8554 → hour of year 8553 = 23 Dec 09:00; OpenStudio mapping test. |
| B5 | **FIXED, VERIFIED on real data** | Heating peaks now carry RH, humidity ratio and every term, internal gains included; they close to ≤ 0.002 W. |
| B6 | **VERIFIED on real data (read-only)** | A TSD scan (103 unique TSDs, harness `pr2a/tsd_cooling_scan.*`) found real zone cooling peaks in the TPD "bridge" and Part O Iteration 3 bridge runs. `C:\TasOut\pr3\final\bridge.tsd` (Leeds TRY, PR3 resultant-temperature thermostat bridge) was converted by the production code onto a copy of its design model `pr3\a2\prepared.sam`, persisted and reopened. Every cooling peak closes as `+Σ` within 0.006 W (table below). Caveat: its set points are imposed by the bridge, not a designer's cooled model; the conversion semantics are what is verified. |

**Real fixtures (production `Modify.AddResults` → `Core.Convert.ToFile` to a new `.sam` → reopen; sources unchanged, SHA-256 in `pr2a/source_hashes.txt`).**

| Space / file | Peak | Load W | Time | Room DB / RH | Outdoor | Σ sensible terms | Residual |
|---|---|---|---|---|---|---|---|
| Bathroom_2, `open.tsd` | Heating DD | 1139.796 | design day hour 23, no date | 16.0 °C / 19.6 % | n/a (design day) | −1139.796 | 0.0002 |
| Bathroom_2, `open.tsd` | Heating annual | 104.010 | hour 8553 = 23 Dec 09:00 | 16.0 °C / 35.0 % | −2.3 °C / 100 % | −104.010 | 0.0002 |
| Bathroom_2, `open.tsd` | Cooling DD / annual | 0 / 0 | none | — | — | — | real zero |
| Bedroom 2_3, `bridge.tsd` | Cooling DD | 2070.833 | design day hour 0 | 19.3 °C / 100 % | n/a | +2070.831 | 0.002 |
| Bedroom 2_3, `bridge.tsd` | Cooling annual | 1369.404 | hour 5116 = 2 Aug 04:00 | 17.2 °C / 100 % | 15.2 °C / 94 % | +1369.407 | 0.002 |
| Studio 1_0, `bridge.tsd` | Cooling DD | 1973.468 | design day hour 0 | 19.8 °C / 100 % | n/a | +1973.474 | 0.006 |
| Studio 1_0, `bridge.tsd` | Cooling annual | 1972.137 | hour 4411 = 3 Jul 19:00 | 19.0 °C / 58.3 % | 17.5 °C / 59 % | +1972.134 | 0.003 |

Bathroom_2's persisted DD components are inf/vent −111.737, BHT −1023.256 and opaque −4.803, with every other term 0.
The annual components are −93.374, −3.158 and −7.478. Full dumps: `pr2a/open_pr2a.out.txt` and `pr2a/bridge_pr2a.out.txt`.
The raw channels are in `pr2a/*_probe.out.txt`.

**Provenance / freshness (unchanged, stated).**
- `Result.DateTime` is still the conversion time.
- The ordinary `RunWorkflow` path still stamps no `SimulationResultProvenance`, so the freshness of a non-Part-O result
  stays **Unknown**. The peaks add no freshness claim.
- A redesign (a TSD path/fingerprint per result, or `RunWorkflow` stamping provenance) is recorded for PR2D and is not
  done here.

**Not changed, recorded.**
- The zone-group (`ZoneSimulationResult`) aggregation in `Modify.AddResults` builds transient per-space cooling results
  at the zone-group peak. They are summed and not persisted.
- Latent removal/addition load is not persisted. It is a candidate additive field if the report needs a latent line.
- The annual heating peak can fall on TSD hour 1 (1 Jan 00:00). The bridge fixture shows it; it is reported as-is.

Not a blocker, but recorded:
- The legacy Print RDS formats the cooling `LoadIndex` with `Convert.ToDateTime(index, 2018)` even for a design-day
  winner, so it prints the design day's internal calendar slot as a date.
- `AddResults` replaces results only for spaces matched by name *this* run. A space that gets no result keeps its old one.
- Phase-1 display: Tas's "thermostat disabled" sentinels (e.g. −50 °C heating, 150 °C cooling on free-running
  Part O spaces) print as real set points (see the gate PDFs).
- **SAM#138** stays independent. None of its three items blocks this report.

## 4. Result authority map

"Today" is what is persisted now. Confidence is in the *semantics*, from code plus TSD evidence.

| Item | Source / API | Persistence | Availability semantics today | Freshness / provenance | Sign | Confidence |
|---|---|---|---|---|---|---|
| TBD heating design load | `SpaceParameter.DesignHeatingLoad`. The same TBD value is also on `SpaceSimulationResult.DesignLoad` (Heating). | Space parameter set(s). The result copy is written only when the Simulate step ran. | 0 is ambiguous: an unconditioned space and a stale duplicate set both read 0 (B0). | Unknown. No record binds it to the TBD. | + magnitude, W | Medium: B0 fixed (SAM#147); provenance still Unknown |
| TBD cooling design load | as above (Cooling) | as above | as above | Unknown | + magnitude | Medium (B0 fixed) |
| Sizing multiplier | `SpaceParameter` / model factor (Phase 1) | model | Phase-1 rules (value 0 = not set) | model assumption | dimensionless | High (Phase 1) |
| TSD heating peak | `SpaceSimulationResult` (Tas source, Heating).`Load` + `SizingMethod` | result object | Mixed: design day **or** annual (B3). Zero → −1 (B2). Heating `Simulation` branch wrong (B1). | `Result.DateTime` = converted-at. Part O only: `SimulationResultProvenance.IsCurrent(model)`. | Tas `heatingLoad` ≥ 0 | **Blocked** |
| TSD cooling peak | same (Cooling) | result object | Mixed (B3); zero → −1 (B2); no fixture (B6) | as above | Tas `coolingLoad` ≥ 0 | **Blocked** |
| Heating peak time | `LoadIndex` | result object | Tas 1-based hour of year; a design day's value is a calendar slot, not a date (B4) | as above | n/a | Medium (base verified; hour-ending is Tas convention, not re-verified against weather) |
| Cooling peak time | `LoadIndex` | result object | as above; 0 = no peak (B2) | as above | n/a | Medium / blocked |
| Room temperature at peak | `DryBulbTempearture`, `ResultantTemperature` | result object | −1 sentinel when there is no peak (B2) | as above | °C | Blocked (B1/B2) |
| Outdoor temperature at peak | `DesignDayTemperature` (misnamed: the annual external T at the peak) | result object | Only when `Simulation` wins; never for a design-day winner | as above | °C | **Not reliably available** |
| Humidity at peak | cooling: `RelativeHumidity`, `HumidityRatio`; heating: `HumidityRatio` only | result object | Heating RH absent | as above | %, kg/kg | Partial |
| Component breakdown | gain parameters (list above) | result object | Heating subset (B5); cooling unverified (B6); −1 on zero (B2) | as above | **+ gain to room air, − loss** (verified: heating load = −Σ terms, exact) | Heating HDD: high. Otherwise blocked. |

Freshness states that Phase 2 can genuinely distinguish:
- **Not simulated** — no Tas `SpaceSimulationResult` for the space.
- **Result unavailable / unusable** — a Tas result whose index is ≤ 0 (legacy sentinel). After PR2A: "no peak recorded".
- **Legitimate zero** — only after PR2A, as `Load = 0` with no index.
- **Stale** — only when a `SimulationResultProvenance` record exists and `IsCurrent(model)` is false. Otherwise freshness is **Unknown**.
  Nothing else can prove currency.
- TBD design loads stay **Unknown**, as in Phase 1: provenance binds the TSD, not the TBD.

## 5. Representative evidence — `C:\TasOut\final1b\open_out.sam` + `open.tsd`, space **Bathroom_2**

Leeds TRY, free-running Part O model. Heating and cooling design days: `Leeds_TRY ANN HTG 100% CONDS DB` and
`… CLG 0% CONDS DB=>GRad`.

| Quantity | Persisted (production API) | TSD (read-only reader) |
|---|---|---|
| TBD design heating load | `SpaceParameter.DesignHeatingLoad` → **0.0 W** at audit time (stale set, B0); **1139.87 W** after SAM#147; `SpaceSimulationResult.DesignLoad` → **1139.87 W** | — |
| Heating peak | `Load` 1139.796 W, `SizingMethod` HDD, `LoadIndex` 1608 | HDD `GetPeakZoneGains` = 1139.796 @ 1608 (HDD hours 1585–1608). **Annual** peak 104.010 W @ **8554** (23 Dec 09:00–10:00): not persisted (B3). |
| Room state at heating peak | DB 16.0, resultant 13.87 °C, humidity ratio 0.00221 | HDD @1608 identical. Annual @8554: DB 16.0, resultant 15.93 °C, RH 35.0 %, outdoor −2.3 °C / 100 %. |
| Heating components @ HDD peak | inf/vent −111.737, BHT −1023.256, opaque −4.803, glazing 0, air movement 0 W | identical; **Σ = −1139.796 = −Load** |
| Heating components @ annual peak | not persisted | inf/vent −93.374, BHT −3.158, opaque −7.478 → **Σ = −104.010 = −load** |
| Cooling peak | `Load` **−1**, `LoadIndex` 0, every field −1 (B2) | building and CDD cooling peak = **0 @ index 0** |
| Freshness | `Result.DateTime` 2026-08-27 18:38 +02:00; no `SimulationResultProvenance` | — |
| Outdoor weather | the model's own `WeatherData` is **London TRY**, the TSD was run with **Leeds TRY** | ⇒ outdoor state must come from the TSD/results, never from the model's weather |

## 6. Semantics settled

Time:
- The TSD year has 365 days (`firstDay` 1, `lastDay` 365): no leap day, no DST.
- The hourly index is **1-based**: hour *n* = `(n−1):00–n:00` local standard time, Tas's hour-ending convention. The base was
  verified; the start/end mapping follows Tas's convention and was not re-derived from the weather file.
- A design-day peak has no calendar date. It is shown as "hour *h* of design day" plus the design day's name.
- Display without a year. When converting, use the fixed non-leap mapping (SAM uses 2018).
- Result `DateTime` is the conversion time, not the simulation time.

Signs:
- `Load` is a non-negative magnitude for both heating and cooling. Display it unsigned.
- Components keep Tas's sign: **+ = gain to room air, − = loss.** For heating, Σ terms = −load, verified at both the HDD
  and the annual peak.
- The display must keep the component signs.
- Regression tests to add in PR2A/PR2B: the HDD closure above; zero → 0, not −1; heating with the annual peak winning.

Units:
- Power uses one shared display unit per table (existing `SelectDisplayUnit`: W → kW at 10 kW, Btu/h → kBtu/h).
- Temperatures use °C/°F. RH is %. Humidity ratio uses the existing `HumidityRatio` category.
- Airflow stays `L/s`. No new units are needed.

## 7. Proposed Phase-2 typed contract (for PR2B, in `SAM.Analytical.Reporting`)

It reuses `SpaceIdentityData`, `SpaceDesignCriteriaData`, `SpaceSizingData`, `DocumentProvenance`, `ReportValue<T>`,
`Quantity`, `QuantityFormatter` and the Phase-1 section builders (identity, design criteria, sizing) and footer pattern. The
collector reads only persisted SAM objects. The only engine-specific rule is how to read a Tas index, and it lives in
SAM.Analytical/SAM_Tas (PR2A), not in reporting.

```text
SpaceDesignLoadDocumentData
  Identity        : SpaceIdentityData        (reuse)
  DesignCriteria  : SpaceDesignCriteriaData  (reuse)
  Sizing          : SpaceSizingData          (reuse; TBD design loads + multiplier, Source TBD, Freshness Unknown)
  Results         : SpaceLoadResultsData
  Provenance      : DocumentProvenance       (reuse)

SpaceLoadResultsData
  Status          : LoadResultStatus { NotSimulated, Available, Unusable }  // Unusable = legacy sentinel / pre-PR2A
  ConvertedAt     : ReportValue<DateTime>    // Result.DateTime
  Freshness       : Freshness                // provenance IsCurrent(model) → Current/OutOfDate; no record → Unknown
  HeatingDesignDay, CoolingDesignDay : ReportValue<string>
  Heating, Cooling : PeakLoadPair

PeakLoadPair      { DesignDay : PeakLoadResult, Annual : PeakLoadResult }

PeakLoadResult
  Basis           : PeakBasis { DesignDay, AnnualSimulation }
  Load            : ReportValue<Quantity>    // W, ≥ 0; explicit 0 = no demand; NotAvailable = not recorded
  Time            : ReportValue<PeakTime>
  Condition       : PeakCondition
  Components      : IReadOnlyList<PeakLoadComponent>   // empty ⇒ section notice, never a fabricated balance

PeakTime          { Basis; HourOfDesignDay (1–24) | HourOfYear (1–8760, hour-ending, 365-day, LST) }
PeakCondition     { RoomDryBulb, RoomResultant, RoomRelativeHumidity, RoomHumidityRatio, OutdoorDryBulb, OutdoorRelativeHumidity : ReportValue<Quantity> }
PeakLoadComponent { Term : PeakLoadTerm, Value : ReportValue<Quantity> /* signed, + gain to room air */ }
PeakLoadTerm      { Solar, Lighting, OccupancySensible, EquipmentSensible, InfiltrationVentilation, AirMovement,
                    BuildingHeatTransfer, ExternalConductionOpaque, ExternalConductionGlazing,
                    OccupancyLatent, EquipmentLatent }   // latent listed separately; use only terms the result stores
```

Per-field rules:
- Every result value: Source TSD, Freshness from `SpaceLoadResultsData.Freshness`.
- TBD values: Source TBD, Freshness Unknown.
- Net balance: Source Derived. Show it only when every term of the balance is available.
- No pass/fail and no ratio: design load and peaks are shown side by side only.

### 7.1 As implemented in PR2B (SAM only)

The proposal above was implemented with these differences, made to keep the data layer solver-neutral and free of
derived values:

```text
SpaceDesignLoadDocumentData            Create.SpaceDesignLoadDocumentData(context, space, resultSource = null)
  Identity, DesignCriteria, Sizing     Phase-1 collector, unchanged (Sizing = TBD design loads, Freshness Unknown)
  Heating, Cooling : SpaceLoadResultData   Create.SpaceLoadResultData(context, space, loadType, resultSource)
  Provenance

SpaceLoadResultData
  LoadType; Status : LoadResultStatus { NotSimulated, PeaksNotRecorded, Available, Ambiguous }
  ResultSource : ReportValue<string>   (Result.Source, e.g. "Tas" / "OpenStudio")
  ConvertedAt  : ReportValue<DateTime> (Result.DateTime = conversion time, not simulation time)
  DesignDay, Annual : SpaceLoadPeakData

SpaceLoadPeakData
  Basis : LoadPeakBasis; State : LoadPeakState { Unavailable, Zero, Value }
  Load : ReportValue<Quantity> W
  DesignDayName (NotApplicable on annual); HourOfDay (0-based); HourOfYear (0-based, NotApplicable on a design day)
  Time : ReportValue<DateTime>  annual only, SpaceLoadPeak.TryGetDateTime(Create.ReferenceYear = 2018); show no year
  RoomDryBulb/Resultant/RH/HumidityRatio, OutdoorDryBulb/RH : ReportValue<Quantity>
  SensibleComponents, LatentComponents : SpaceLoadPeakComponentData { Component, Value (signed W) }
```

- **Only** `SpaceSimulationResultParameter.DesignDayPeak` / `AnnualPeak` are read. `Load`, `LoadIndex`,
  `SizingMethod` and every legacy room/gain value are never read (test: conflicting legacy values are ignored).
- `Unusable` became `PeaksNotRecorded` (results exist, no typed peak: re-run). `Ambiguous` (more than one result of
  the load type carries peaks) picks nothing; the caller passes `resultSource`.
- Per peak: missing → `Unavailable` (all values NotAvailable, no components); `Load 0` → `Zero` (Load an available
  0 W; absent time/state NotApplicable, "no peak timestep"); otherwise `Value` (absent values NotAvailable, "not
  reported"). A peak whose `Basis` does not match its slot, a negative/non-finite load, or an out-of-range hour is
  NotAvailable "invalid value in results" with a warning.
- Source: new additive `ReportValueSource.SimulationResult` (not `TSD`, which names an engine). Freshness: Unknown
  for every result value; `SimulationResultProvenance` is Part-O-specific and not consulted (freshness redesign = PR2D).
- Components: exactly the stored terms, sign untouched, latent listed apart. No net balance, no latent removal load,
  no derived quantity. The Derived net-balance row of §7 is left to PR2C if it is wanted, under its own rule.
- `PeakTime`, `PeakCondition`, `PeakLoadTerm` were not introduced: the fields sit on `SpaceLoadPeakData`, and the
  existing `SAM.Analytical.LoadPeakComponent` enum is reused.
- Tests: `SAM/SAM.Tests/SpaceDesignLoadDataTests.cs`.

## 8. Report content

| Included (v1, after PR2A-0 + PR2A) | Deferred | Unavailable today |
|---|---|---|
| Identity; result source + converted-at + freshness; design criteria (reuse) | Outdoor state at a **design-day** peak (needs the design-day weather from the TSD design data or cluster `DesignDay`, per design day) | Annual simulated peak (B3, until PR2A) |
| TBD design load + sizing multiplier (reuse, after B0) | Latent balance and the cooling-load/latent definition (B6) | Any cooling breakdown (B6) |
| Design-day and annual peaks, heating and cooling, side by side | Surface-level results (`SurfaceSimulationResult` at the peak) | Heating RH at peak (until PR2A) |
| Peak time (hour of design day / hour-ending date) | "Model vs results" for non-Part-O models (needs `RunWorkflow` to stamp provenance) | |
| Room DB, resultant, RH; outdoor DB/RH at the annual peak | | |
| Sensible zone heat balance at each peak, Tas terms, signed, with net | | |

## 9. Visual design gate

Files are in `documentation/evidence/reporting-phase2-gate/`:
- The PDFs were rendered by the **production `PdfRenderer`**.
- The `*.json` files are the `Document` dumps from `SAM.Core.Reporting.Convert.ToJson`.
- The scratch harness source is in `harness/`. It loads the real `.sam`, reuses the Phase-1 identity, design-criteria and
  sizing builders, and builds the new sections from generic blocks. It is not production code.

| File | Case | Pages |
|---|---|---|
| `gate-1-normal-SI.pdf` | Bathroom_2. Real persisted values, plus the real TSD annual peak (as PR2A would persist it). Cooling shown as an explicit 0 (the post-PR2A semantics). | **1** |
| `gate-4-normal-IP.pdf` | same, Imperial (core IP works end to end) | **1** |
| `gate-2a-legacy-sentinel-SI.pdf` | Studio 1_0. Legacy −1 results → "re-run" notice; peaks "—" | 1 |
| `gate-2b-not-simulated-SI.pdf` | Corridor_1 with its results removed → "No Tas simulation results" | 1 |
| `gate-3-long-name-large-values-SI.pdf` | **Illustrative** (flagged in the PDF): long name, values ×118, an invented cooling peak/breakdown | **2** — only the net row spills onto page 2 |

Findings:
1. The normal case fits A4 portrait on one page. Keep the Phase-1 visual language: half-width results and
   criteria, full-width loads, conditions and balance.
2. A group header over a **single** column wraps ("Coolin g"). Use a symmetric 4-column layout (Heating DD / Annual,
   Cooling DD / Annual). Fix it in the design, not the renderer.
3. A full cooling breakdown with a 3-line name overflows by about one row. PR2C must either accept a controlled page 2
   (a heat-balance page) or add a *generic* keep-together option to the renderer. It must not squeeze the typography.
4. The gate itself exposed B0 (0 W design load) and the Phase-1 set-point sentinels.

### 9.1 As implemented in PR2C (SAM only)

The production document is `SpaceDocumentDefinitions.SpaceDesignLoadSummary`, built by
`Create.SpaceDesignLoadSummary(context, space, resultSource = null)` from `Create.SpaceDesignLoadDocumentData` (PR2B)
and rendered by the unchanged `PdfRenderer`. The builders read only `SpaceDesignLoadDocumentData`.

```text
identity (header band)  design-criteria | sizing      Phase-1 builders, run on the PR2B data by an adapter (same output)
heating                 Design day: <name>             untitled key/value, kept with the table below
                        [ | Design day | Full year ]   Peak sensible load, Peak hour, Room DB / resultant / RH / humidity ratio,
                                                       Outdoor DB / RH
                        Sensible load components at peak [Component | Design day (W) | Full year (W)], signed as stored
                        Latent components at peak      same, kept apart
                        notes                          terms 0 at both peaks named, not listed; sign convention
cooling                 same
results                 [ | Heating | Cooling ]        Status, Result source, Read into model, Matches current model
                        notes                          peak-hour convention; no acceptance rule
footer                  Phase-1 footer, unchanged
```

- **States.** `NotSimulated` → an Information notice ("… This is not a zero load."); `PeaksNotRecorded` → a Warning
  ("… Re-run the simulation …"); `Ambiguous` → a Warning, no table, nothing chosen: a pick happens only through
  `resultSource`. A peak `Zero` prints `0 W`; `Unavailable` prints `—`; a zero peak's absent hour/state prints `n/a`.
  When neither peak is above zero the section shows the design day, the Peak sensible load row and one note, nothing else.
- **Time.** Design day: `23:00–24:00` from `HourOfDay` (0-based), and the design-day name; never a date and never an
  HOY. Full year: `23 Dec 09:00–10:00 (HOY 8554)` from `Time` and `HourOfYear`; no year (`Create.ReferenceYear` is
  never printed). The HOY is user-facing, 1–8760 (HOY 1 = 1 Jan 00:00–01:00, HOY 8760 = 31 Dec 23:00–24:00): the
  stored 0-based `HourOfYear` + 1, the same normalized value the date comes from, for either engine (it equals Tas's
  own 1-based index, §3.2 *Time*). No hour of the year → no HOY, never estimated. The Results note states the
  convention. (Added after PR2C, on branch `feature/pr2c-annual-peak-hoy-2026-09-27`.)
- **Units.** Shared `QuantityFormatter`. One power unit per load section (both peak loads and every component), so
  W / kW / Btu/h / kBtu/h switch together. Pairs of temperatures/RH share one unit.
- **Components.** Values exactly as stored, signs untouched, sensible and latent in separate tables. A term stored as
  exactly 0 at **both** peaks is named in one note ("Zero at both peaks (not listed): …") instead of a row, the Phase-1
  zero-area fabric rule; a term missing at one peak stays listed (`—` not reported, `n/a` at a zero peak). No total, no
  net balance, no latent removal load, nothing Derived.
- **Provenance.** Source and read-in time from the typed data; "Matches current model" reads the freshness the data
  carries: Unknown → "not recorded", never "yes".
- **No acceptance rule.** Sizing design loads and simulated peaks are only adjacent; no pass/fail, ratio or verdict.
- **Renderer (generic, SAM.Core.Reporting.Pdf):** an untitled key/value block directly followed by a table is placed
  with it as one unit (the existing side-by-side frame with one part; it falls back to page flow when taller than half
  the body). It stops the design-day line being left alone at a page foot. No Phase-1 section has that shape, so
  Phase-1 output is unchanged (its PDF tests pass as before).
- **Layout vs §9 finding 2:** heating and cooling are stacked, each with its own `Design day | Full year` columns,
  rather than one 4-column table: the pairing the PR2C brief asked for. It also removes the single-column group
  header that wrapped ("Coolin g").
- **Public API added:** `SpaceDocumentDefinitions.SpaceDesignLoadSummary`, `Create.SpaceDesignLoadSummary`. The new
  builders, the Phase-1 adapter and the format helpers are internal.

Visual gate (production renderer; PDFs in `documentation/evidence/reporting-phase2-gate/pr2c/`, values from
`SAM.Tests/Helpers/SpaceDesignLoadFixture.cs`: real Tas peaks of Bathroom_2 and Studio 1_0 as SAM_Tas#69 stores them):

| Case | File | Pages |
|---|---|---|
| A Bathroom_2 SI: heating DD 1,140 W / FY 104 W, cooling real zero | `SpaceDesignLoad_A_Bathroom_2_SI.pdf` | **1** |
| A Bathroom_2 IP (3,889 / 355 Btu/h) | `SpaceDesignLoad_A_Bathroom_2_IP.pdf` | **1** |
| B Studio 1_0 SI/IP: heating and cooling both populated | `SpaceDesignLoad_B_Studio_1_0_{SI,IP}.pdf` | 2 (cooling starts page 2 whole) |
| C Not simulated | `SpaceDesignLoad_C_NotSimulated_SI.pdf` | 1 |
| D Peaks not recorded (legacy −1 results) | `SpaceDesignLoad_D_PeaksNotRecorded_SI.pdf` | 1 |
| Ambiguous heating (Tas + OpenStudio) | `SpaceDesignLoad_E_Ambiguous_SI.pdf` | 1 |
| G stress (invented, long names, kW, every term) SI/IP | `SpaceDesignLoad_G_Stress_{SI,IP}.pdf` | 2 |

Findings: no overlap or clipping (checked by `SpaceDesignLoad_NothingEscapesItsColumn` too); Bathroom_2 fits one page
with about 10 mm to spare, so a longer normal case may take a natural second page. Known, not PR2C: the shared
formatter shows humidity ratio in g/kg in IP too (it has no IP humidity-ratio unit); the Phase-1 sizing label "Design
load per area" wraps in IP.

Tests: `SAM.Tests/SpaceDesignLoadSummaryTests.cs` (document, 6 goldens `Golden/SpaceDesignLoad_*.json`) and
`SAM.Tests/PdfRendererTests.SpaceDesignLoad.cs` (every case rendered: pages, A4, values verbatim, nothing escapes; the
renderer lead rule).

## 10. Recommended PR sequence

1. **PR2A-0 — SAM (SAM.Core/SAM.Analytical): stale design-load read (B0).** **DONE:** SAM#147, merge `00db4b85` (§3.1).
   - Open a SAM issue first.
   - Find why duplicate `SAM.Analytical` sets are appended.
   - Make the read/write deterministic: one set per assembly name on load, or read and write the same set.
   - Add a regression test on the 3-set shape.
   - Re-check Phase-1 golden/fixture output. **This corrects shipped Phase 1.**
2. **PR2A — SAM_Tas `Convert.ToSAM_Results` / `Create.SpaceSimulationResult`.** **DONE, open for review** as PR2A-1
   (SAM, typed `SpaceLoadPeak`) + PR2A-2 (SAM_Tas); see §3.2. B6 was verified read-only on an existing TSD, so the
   licensed run below was not needed. The original plan follows:
   - Fix the heating-branch variables (B1).
   - For a zero/absent peak, write `Load = 0` with no index, state or components, never −1 (B2).
   - Persist the design-day **and** annual peaks separately (B3), with an explicit, documented time base (B4). Add
     parameters; leave the legacy `Load`/`LoadIndex`/`SizingMethod` alone for Print RDS and the benchmark.
   - Store the full component set and RH for heating too (B5).
   - Tests: pure peak-selection functions, no COM. Evidence: one short licensed design-day run of a small **cooled** model,
     to create the missing cooling fixture (B6) and check cooling closure and latent.
3. **PR2B — SAM.Analytical.Reporting:** **DONE:** SAM#156, merge `9c3b9d6e` (§7.1: data + collector; the builders and
   SI/IP goldens moved to PR2C). Planned: the typed data above, the collector and builders, the legacy-sentinel
   "re-run" state, SI/IP goldens.
4. **PR2C — design/PDF integration:** **DONE** (§9.1). The original plan follows:
   - the 4-column tables;
   - the page-2 decision (optional generic keep-together in the renderer);
   - a gate re-run on real cooled data.
5. **PR2D — SAM_UI command** (Edit › Reports › Space Design Load Summary). **DONE:** SAM_UI#127, merge `cbe1c076`;
   HOY + "Peak sensible load" terminology SAM#159, merge `6c255ad8`. Optionally stamp
   `SimulationResultProvenance` in `RunWorkflow`, so that "model vs results" becomes known.
6. **PR2E — SAM_Deploy** pointer + installed-product smoke test. **DONE:** SAM_Deploy#55, merge `1506da5f` (pins SAM
   `6c255ad8`, SAM_Tas `fedf34cd`, SAM_UI `cbe1c076`); installer run 218, installed acceptance A/A2/B/C/D PASS.
   **Phase 2: COMPLETE.** Next: PR2F, real-project report review (not started).

Separate follow-ups, not blocking: Phase-1 set-point sentinel display; the Print RDS design-day date; SAM#138.

## 11. Reproduce

- Result-type scan: count and dump `SpaceSimulationResult` in a `.sam` (a zip of one JSON). Scripts are the Python ones in
  `harness/`; `stale_design_load_scan.py <dir>` gives the B0 figure.
- TSD reader (read-only, on a **copy**):
  - `csc -platform:x64 -r:Interop.TSD.dll TsdProbe.cs` (from `SAM_Tas\build`);
  - run `probe.exe <copy.tsd> <zone name> <design-day hours> <annual hours>`.
- Design gate:
  - `gate.csproj` references `SAM\build\SAM*.dll` + PDFsharp-MigraDoc 6.2.0;
  - run `dotnet gate.dll <outDir>`.
