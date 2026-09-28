# PR2F — Space Design Load Summary: real-project review (2026-09-27)

Status: **review complete; PR2F-1 (SAM report content) implemented on this branch, see §8.** Findings are ranked
A (correctness / misleading meaning), B (high-value UX), C (cosmetic), D (future).
Evidence: `documentation/evidence/reporting-pr2f/`.

## 1. What was reviewed

The only real project models on this machine with typed peaks (PR2A contract) are the Part O dwelling
(`000000_SAM_AnalyticalModel-It1a-futureZ1`, Leeds TRY, 9 Spaces) in two variants. Every Space of each was
rendered with the production `Create.SpaceDesignLoadSummary` + `PdfRenderer` (harness:
`evidence/reporting-pr2f/harness/Program.cs.txt`, which also dumps unrounded peaks).

| Set | Model | What it covers |
|---|---|---|
| `open-SI/` (9) | `C:\TasOut\pr2d\open_peaks.sam` (free-running) | heating-dominated (Bathroom_2, Ensuite_5/8), zero peaks, bedroom / kitchen / studio / corridor, 1-page reports |
| `bridge-SI/` (9) | `C:\TasOut\pr2d\bridge_peaks.sam` (DX bridge, It3B) | heating + cooling, cooling-dominated, latent at peak, many component rows, 2-page reports |
| `bridge-IP/` | Studio 1_0 | IP units |
| `not-simulated-SI/` | `nores.sam`, Bathroom_2 | missing results |

Long Space / design-day names and large values are covered by the Phase-2 stress gate
(`reporting-phase2-gate/gate-3-*`); the real names here are short.

Not available on this machine: a non-residential project (office, AHU, mechanical ventilation latent
load), or any model whose `AnalyticalModel` carries `DesignDay` objects.

Scale: the 9 Spaces were replicated (Spaces + their panels, new Guids) to 999 and 4,995 Spaces.

## 2. Component balance (unrounded, `bridge-SI/_balance.csv`, `open-SI/_balance.csv`)

Tas stores a heat balance at the peak hour: **heating load = −Σ sensible components; cooling load =
+Σ sensible components** (latent terms excluded).

| Case (all non-zero peaks, 26 in total) | Max \|residual\| unrounded | Displayed residual |
|---|---|---|
| open, heating DD + annual (6) | 0.002 W | 0 or 1 W |
| bridge, heating DD + annual (14) | 0.007 W | 0 or ±1 W |
| bridge, cooling DD + annual (14) | 0.013 W | 0 or ±1 W |

Examples: Studio 1_0 cooling DD 1,973.468 W vs Σ 1,973.474 W; Bedroom 2_3 heating DD 1,629.660 vs
−1,629.659. Every residual is float32 storage precision; every ±1 W displayed difference is rounding
of the individually rounded rows.

**Recommendation: no balance row, no pass/fail.** A row would always read 0 W for Tas. EnergyPlus shows
"Difference Between Peak and Estimated Sensible Load" because its components are *estimates* (instant +
delayed); Tas's are the balance itself. Balance closure is an engine property, not a contract of
`SpaceLoadPeak`, so the report must not assert it for every engine. (D: if a future engine's components
do not close, the engine should declare it and the report can then show an informational difference.)

## 3. Findings

### A1 — Thermostat "off" sentinels print as set points (−50.0 / 150.0 °C; −58 / 302 °F). Fix now.
- **Evidence.** Design Criteria "Room set point" on all 18 real reports: bridge −50.0 / 150.0 °C; open
  16.0 / 150.0 °C; IP −58.0 / 302.0 °F.
- **Authority.** SAM's own profile library defines them: `SAM_ProfileLibrary.JSON` "No Heating"
  (Thermostat, `[0,23,-50.0]`) and "No Cooling" (`[0,23,150.0]`); `SAM_ProfileLibrary_TM59.JSON` the same
  as Heating / Cooling profiles. SAM_Tas `Query/Sizing.cs` treats a lower limit ≤ −50 as "no heating". The
  humidity rows already use this pattern: 0 % / 100 % = no (de)humidification → not applicable
  (`SpaceDocumentData.cs` `DesignCriteria()`, `notApplicable` hook).
- **Fix.** Name the two limits once in SAM.Analytical (domain, next to the library profiles), and pass
  them through the existing `ProfileValue(..., notApplicable)` hook for heating (yearly max ≤ −50 °C)
  and cooling (yearly min ≥ 150 °C). Only a thermostat that is off *all year* is affected: the heating
  value is the yearly maximum, the cooling one the yearly minimum, so any real schedule still prints.
  Presentation: "none" with the reason as the note (see §5 decision).
- **Repo / risk.** SAM (`SAM.Analytical`, `SAM.Analytical.Reporting`). Low: one row of both documents;
  Space Assumptions goldens that contain the sentinels change.

### A2 — "Latent components at peak" reads as the latent load; it lists internal latent gains only. Fix now (labels).
- **Evidence.** Bedroom 2_3 annual cooling peak (HOY 5117): report shows Occupancy (latent) 77 W; the TSD
  at the same hour has `latentRemovalLoad` 300.9 W (`reporting-phase2-gate/pr2a/bridge_probe.out.txt`).
  The difference is infiltration / ventilation moisture, which has no component in the contract.
- **Fix.** Relabel the table "Latent gains at the sensible peak hour" and add one note: internal latent
  gains only; the room's latent load is not in the results. No value added, none derived (the latent
  removal load stays a candidate field, authority doc §3).
- **Repo / risk.** SAM reporting. Low: labels; goldens relabel.

### B1 — The "Zero at both peaks" note mixes sensible and latent terms. Fix now.
- **Evidence.** open Bathroom_2: under the *sensible* table (no latent table, all latent zero) the note
  lists "…, Occupancy (latent), Equipment (latent)". bridge: every cooled room's note lists
  "Equipment (latent)" alongside sensible terms.
- **Cause.** One `zeros` list is shared by both `ComponentTable` calls
  (`SpaceDesignLoadSectionBuilders.cs` ~L152–166) and printed once after both tables.
- **Fix.** One list per kind. Sensible zeros → note directly under the sensible table; latent zeros →
  note under the latent table; when every latent term is zero (no latent table), one line
  "Latent gains: zero at both peaks (Occupancy, Equipment)". Missing-at-one-peak terms keep their row;
  exact-zero folding unchanged.
- **Repo / risk.** SAM reporting. Low; goldens change.

### B2 — No outdoor design condition anywhere for the design day. Record; not fixable in reporting (D).
- **Evidence.** Every design-day column prints Outdoor dry bulb / RH "—", and Design Criteria prints
  "—" for both outdoor rows. The outdoor design temperature is the number an engineer checks first.
- **Cause.** (1) The TSD design-day data sets carry no weather results (SAM_Tas `Create/SpaceLoadPeak.cs`),
  so design-day peaks have no outdoor state. (2) These real models carry no `HeatingDesignDays` /
  `CoolingDesignDays` (collector warning "no heating design day in model").
- **Proposal (future, D).** Investigate an authoritative source in SAM_Tas: the TBD design-day climate
  (already readable, `Convert/ToSAM/DesignDay.cs`) or populating the model's design days on import. Do
  not synthesise a value in reporting.

### B3 — Tas design-day names are raw. No change now (C/D).
- **Evidence.** "Leeds_TRY ANN HTG 100% CONDS DB", "Leeds_TRY ANN CLG 0% CONDS DB=>GRad".
- **Investigation.** The string is the TSD `CoolingDesignData.name` / `HeatingDesignData.name` = the TBD
  design-day name. SAM `DesignDay` has `Name`, `Description` and a load type, but the real models hold no
  `DesignDay` objects to join to, and the name's grammar (ANN / 0% / CONDS / DB=>GRad) is Tas's CIBSE
  design-day generator convention, not a documented contract. Parsing it would invent semantics.
- **Recommendation.** Keep the raw name (it is the provenance and is unambiguous inside a "Heating" /
  "Cooling" section). Future (D): when the model carries a `DesignDay` whose `Name` matches, print its
  `Description` if set, with the raw name kept.

### C1 — Irrelevant notes. Fix now.
- The Results note "Peak hour: … HOY 1 = …" prints on Not simulated / all-zero reports where no peak hour
  is shown (`not-simulated-SI`, open Corridor_1). Print it only when a peak hour row is printed.

### C2 — Footer legend "— not available". No change.
- The legend is already conditional on a printed "—" (`Create/Document.cs`). In all 20 real reports a
  "—" is printed (Design Criteria outdoor rows at least; heating Room RH), so the legend is correct. A
  per-page legend would add renderer complexity for no gain.

### C3 — Known IP items (carried): humidity ratio g/kg in IP; "Design load per area" wraps in IP.

### D1 — Peaks on the first simulated hour (engineering observation, not a SAM defect).
- bridge: 7/7 design-day cooling peaks at 00:00–01:00 = TSD index 5089, the first hour of the CDD block
  (days 213–214), solar 0, driven by +1.0–1.4 kW building heat transfer; 7/7 annual heating peaks at
  HOY 1. Both are start-up / pull-down effects of the model's simulation set-up. SAM reports Tas's peak
  faithfully. A "peak on the first simulated hour" advisory would need an engineering basis; not now.

### D2 — Sizing design load 0 W vs simulated peaks (fixture data, known; no change).

### Confirmed, no change
- Headline "Peak sensible load"; latent never added; no total. Clear on every report.
- Time: design day = hour only ("15:00–16:00"), no HOY, no date; full year = "3 Jul 19:00–20:00 (HOY 4412)",
  "1 Jan 00:00–01:00 (HOY 1)". Phase-2 behaviour intact.
- Layout: heating-only rooms 1 page; heated + cooled rooms 2 pages with Cooling moved whole to page 2
  (page 1 about one third empty — acceptable, no split section); no clipping, no orphan headings; footer
  consistent; IP fits.

## 4. Comparison with established load reports (information hierarchy only)

References: Carrier HAP (zone/space design load reports), Trane TRACE 3D Plus (room checksums), IES VE
ApacheLoads, EnergyPlus / DesignBuilder "Zone Component Load Summary" ([EnergyPlus engineering reference](https://bigladdersoftware.com/epx/docs/9-6/engineering-reference/zone-component-loads-summary.html)).

| Item | Typical in those tools | SAM today | Verdict |
|---|---|---|---|
| Peak sensible load + peak time | yes | yes, design day and full year side by side | good; the DD / annual split is clearer than most |
| Outdoor condition at peak | always (DB, WB/RH) | annual only; design day "—" | **genuinely missing** (B2) — needs a source |
| Room condition at peak | yes | DB, resultant, RH, humidity ratio | good |
| Sensible components | yes | yes, signed, one unit | good |
| Latent components / latent load | latent per component + total latent | internal latent gains only | label it honestly (A2); latent load is a candidate field |
| Grand total (sensible + latent) | yes | no | **do not copy** — no authoritative total |
| % of total, W/m², checksums | yes | design load per area only | do not add derived ratios now |
| Supply airflow / supply temperature | yes (system sizing) | not in the contract | do not copy |
| Balance / difference row | EnergyPlus only (estimated components) | none | not needed for Tas (§2) |
| Result source / provenance | brief | source, read-in time, currency | good |

## 5. Decisions needed before implementation

1. Text for an all-year-off thermostat: **"none"** (recommended, like "not set"), "off", or the generic
   "n/a". Same text for the humidity rows (today "n/a") for consistency?
2. Latent table title: "Latent gains at the sensible peak hour" (recommended).
3. Batch UX shape (§6).

## 6. Multi-Space / All-Spaces export

**Measured** (bridge model replicated; SI; this VM):

| Spaces | Current per-Space `DocumentContext` | One shared context |
|---|---|---|
| 999 | 18 ms / Space | 6.1 ms / Space (total 6 s) |
| 4,995 | 35 ms / Space (context alone 19 ms) | **4.5 ms / Space (total 23 s)**, managed peak 435 MB (the model) |

`Create.DocumentContext` deep-clones the `AdjacencyCluster` (`AnalyticalModel.AdjacencyCluster` getter),
so per-Space contexts are O(model) each (≈ 2 min at 5,000 Spaces). PDF rendering is ≈ 55 % of the shared
path; each PDF is ≈ 50 kB and is written as it is produced, so memory stays flat.

**Proposal**
- Single Space: unchanged (Save As).
- New command "Export Space reports…" (Reports group; also on the multi-Space context menu, pre-set to
  the selection): one small window — report(s) [Space Assumptions] [Space Design Load Summary];
  Spaces (•) selected (n) / ( ) all in model (N); output folder (chosen once). One workflow for both
  reports through the existing `SpaceReportPdf` definitions; SAM_UI stays orchestration.
- Sequential, on a background task, over one model snapshot, with a progress bar and Cancel between
  Spaces. **Not parallel**: PDFsharp's font resolver is process-global (`GlobalFontSettings`) and
  sequential is already ≈ 25 s for 5,000 Spaces.
- One `DocumentContext` per report type per batch, with a fresh diagnostics log per document. Needs a
  small SAM API (e.g. a context that shares the model snapshot but starts a new `Log`), so SAM_UI does
  not slice a shared log.
- File names: the existing `SpaceReportPdfFileName` (sanitised, truncated). Names that collide
  (case-insensitive, after sanitising / truncation) all get " [first 8 hex of Guid]" — deterministic and
  stable when other Spaces are added. Existing files: one prompt ("N files exist — overwrite?").
- One Space failing never stops the batch; at the end one summary (created / failed / cancelled) with
  "Open folder", and an `export-log.txt` in the folder listing failures and document notes.
- Combined PDF: deferred (assess only if a workflow need appears).

## 7. Proposed PR split

1. **SAM — PR2F-1 report content** (this branch; this review is its first commit): A1, A2, B1, C1 +
   the batch `DocumentContext` API; goldens; tests; before/after PDFs of the same 20 reports.
2. **SAM_UI — PR2F-2 batch export** (§6), pinned to the SAM merge.
3. **SAM_Deploy — PR2F-3** bump + installed acceptance.

B2 (design-day outdoor condition) and B3 (friendly design-day label) are separate future SAM_Tas / SAM
investigations.

## 8. PR2F-1 implementation (2026-09-28)

Owner decisions (§5): off-all-year text **"none"** for thermostats and humidity; latent title **"Latent gains at
the sensible peak hour"**; batch UX as §6 (SAM_UI, after this PR). Unchanged by decision: raw Tas design-day
names, no balance row, no synthesised design-day outdoor condition, footer legend.

| Item | Change |
|---|---|
| A1 | `SAM.Analytical.Query.NoHeatingSetPoint` (−50 °C) / `NoCoolingSetPoint` (150 °C) + `IsHeatingOff` / `IsCoolingOff` (new `Query/ThermostatOff.cs`). The collector passes them through the existing `notApplicable` hook (yearly max / min, so a schedule that ever controls still prints). Design Criteria shows every off control (heating, cooling, humidification, dehumidification) as **"none"** with the reason as the note; not the "n/a" marker, so no legend entry. |
| A1 regression found on the real models | Every bridge room and the open corridor have both thermostats off, no humidity profile and no design days: the "all missing" rule then collapsed Design Criteria to "No set point profiles and no design days in model" (false). An off control is now a known state that keeps the table. Test `DesignCriteria_ControlsOff_NothingElseKnown_KeepsTheTable`. |
| A2 | Latent table title "Latent gains at the sensible peak hour" + note: internal gains only; the room's latent load (also infiltration / ventilation moisture) is not in the results and not derived. |
| B1 | Sensible and latent zeros folded separately. Sensible note under the sensible table (id `-components-zero`, sensible terms only); latent note under the latent table (`-latent-zero`). With no latent table, one sentence "Latent gains: zero at both peaks (Occupancy, Equipment)." appended to the sensible note's block (a separate block pushed the one-page heating-only Bathroom fixture to 2 pages). |
| C1 | Results "Peak hour … HOY" note only when a section prints a Peak hour row (status Available and a peak above zero). |
| Batch API | `DocumentContext.WithNewDiagnostics()`: shares the model snapshot, cluster, profile library, options, formatter and provenance; starts an empty log. Real bridge model: 9 documents, 18 diagnostics all in per-document logs, shared log 0; 16.9 vs 32.9 ms/Space (`after-pr2f1/batch-timing.txt`). |

**Validation.** `SAM.Tests` Debug 2681/2681 (2668 baseline + 13 new); `SAM.sln` Release 0 errors. 7 goldens
changed as intended (Space Design Load ×6; Space Assumptions goldens unchanged: their fixture has real set points).
Real-project re-render (`evidence/reporting-pr2f/after-pr2f1/`, same 20 reports): page counts unchanged (open 9×1;
bridge 1×1 + 8×2; IP 2; Not simulated 1); no −50 / 150 / −58 / 302 sentinel left (off thermostats print "none"; open rooms keep e.g. "16.0 °C | none");
peak-hour note gone from Not simulated and all-zero Corridor_1; visual review of Bathroom_2, Studio 1_0 (SI, IP),
Kitchen_4, Corridor_1, Not simulated: no clipping, no orphan headings.
