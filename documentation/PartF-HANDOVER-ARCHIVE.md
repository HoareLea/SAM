<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part F / Part O — handover archive

**This file is historical reference. It is NOT authoritative for the current repository state, the
current test counts, or the next piece of work.**

For anything current, read:

| Question | Authoritative document |
|---|---|
| Which repos, branches, SHAs, PRs; what to do next | [`PartF-HANDOVER.md`](PartF-HANDOVER.md) |
| Approved Document F regulatory / calculation traceability | [`PartF-ADF-Volume1-2021-Traceability.md`](PartF-ADF-Volume1-2021-Traceability.md) |
| Real TAS / Part O / TM59 validation evidence | [`PartO-TAS-VALIDATION.md`](PartO-TAS-VALIDATION.md) |

**What this file is for.** The handover accumulated the whole implementation history of the Part F and
Part O programme: reconciliation incidents, SHA-by-SHA repository histories, step-by-step implementation
diaries, mutation-testing narratives, closed reviewer findings, superseded architecture proposals, closed
defects, old test counts and successive "NEXT" sections. That material is genuinely valuable — it records
*why* decisions were taken and which mistakes were already made and paid for — but it is not needed to
resume work, and mixing it with the live continuation state made the live state hard to trust.

**Read it for reasoning, never for state.** Every SHA, test count, "current", "latest checkpoint", "NEXT"
and "verify this before touching anything" statement below was true when written and is now superseded by
`PartF-HANDOVER.md`. In particular, **do not run the reconciliation commands from the archived §0a** —
their pinned SHAs are frozen at the point of archiving. Use the ones in the active handover.

**Historical conclusions below have not been rewritten**, including the ones later found to be wrong and
corrected in place (for example the `Interop.TBD` temperature-member scan in the archived §11o, and the
step 8 commit message the archived §0b corrects). The corrections are recorded beside them, as they were.

**Archived on 2026-08-19**, from the handover as it stood at SAM `5bee56f0` (last code commit
`3fcfb880`), SAM_Systems `4446bb8`, SAM_UI `fcf0ec8`, SAM_Tas `2750a21`, SAM_Tas_Grasshopper `29c31db`.

**A note on the validation sections.** The archived §11e, §11o, §11t, §11u and §11v below hold the
original chronological validation narratives, kept verbatim as the detailed trace. Their engineering
conclusions are consolidated, non-chronologically, in
[`PartO-TAS-VALIDATION.md`](PartO-TAS-VALIDATION.md) — that document, not this one, is the validation
record to cite.

---

# ARCHIVED CONTENT — verbatim from `PartF-HANDOVER.md` as at 2026-08-19

*Everything from here down is the previous handover, unaltered except for this banner and the heading
immediately below. Section numbers referred to elsewhere as `0a`, `0e`, `11o`, `11p`, `11q`, `11u`,
`11v` and so on are the ones in this archived content.*

## Part F — session handover (archived state, superseded)

**This file is the authoritative cross-laptop continuation state.** Michal works interchangeably on two
laptops, so nothing important may live only in a chat, a terminal history or an unpushed local commit.

**Working sequence, every checkpoint:** implement → test → commit → push → independent review/fixes →
test → commit/push → **update this handover** → commit/push the handover → continue. A checkpoint is not
finished while this file is behind the repositories.

**Starting a session:** read section 0 first, then verify every repo against the branch/SHA/remote state
recorded there. **If the actual state differs, stop and reconcile before changing any code.**

---

## 0. Cross-laptop continuation state

*Last updated at SAM `3fcfb880` + SAM_Tas `2750a21` + SAM_Tas_Grasshopper `29c31db` (SAM_Systems `4446bb8`,
SAM_UI `fcf0ec8` unchanged) — **the TM59 verification `report` output implemented and Iteration 1 BasePassive
report validation CLOSED against the real Flat1 comparison (11v)**. Previously: the first SAM-vs-native-TAS
comparison on real Flat1 output (11u), and before that, Iteration 0 step 9 complete and Iteration 1's
BasePassive vertical slice landed.*

### 0a. Repository state — verify this before touching anything

**FIVE repos are now in the workstream.** `SAM_Systems` joined at step 5b; **`SAM_Tas_Grasshopper` joined at
step 7a**, with Michal's approval, to repoint `Tas.TSDQueryTM59Results` at the step 6 service.

| Repo | Branch | Last CODE commit | HEAD should be | Tree | Cut from |
|---|---|---|---|---|---|
| `SAM` | `feature/partf-terminal-transfer-compliance` | **`3fcfb880`** | that, **plus the handover commit(s) on top** | clean, level | `sow/2026-Q3` @ `34dea440` |
| `SAM_Systems` | `feature/partf-terminal-transfer-compliance` | **`4446bb8`** | exactly `4446bb8` | clean, level | `sow/2026-Q3` @ `d7303c2` |
| `SAM_UI` | `feature/partf-terminal-transfer-compliance` | **`fcf0ec8`** | exactly `fcf0ec8` | clean, level | `sow/2026-Q3` @ `074f3d9` |
| `SAM_Tas` | `feature/partf-terminal-transfer-compliance` | **`2750a21`** | exactly `2750a21` | clean, level | `sow/2026-Q3` @ `3d58bfe` |
| `SAM_Tas_Grasshopper` | `feature/partf-terminal-transfer-compliance` | **`29c31db`** | exactly `29c31db` | clean, level | `sow/2026-Q3` @ `9555aa1` |

**`SAM_UI`'s pin is a MERGE commit, and that is why the check fired on it.** The 2026-08-17 session's
verification loop printed eleven `UNRECORDED CODE:` lines for `SAM_UI` - Grasshopper component files that have
nothing to do with Part F. The reconciliation: `5b617e8` is a merge of `sow/2026-Q3` into the feature branch
whose **first parent is the recorded `ffd8e38`**, and those files came in from the other side (PR #74,
Grasshopper component descriptions). So it was a base sync, not unrecorded feature work. The pin is bumped to
the merge itself. **If the loop fires again on `SAM_UI`, check for a merge before assuming lost work** -
`git log -1 --format='%P' <head>` showing two parents is the tell.

**The step 9 session opened by finding this table WRONG, and the check caught it. Worth keeping as evidence
the invariant earns its keep.** It pinned `SAM`'s last code commit at `f712fd9f`, but `cc5e67f0` — a real code
commit touching `OverheatingScenarioMap.cs` and `PartOResultAssociationTests.cs` — had landed on top of it and
was described in 0b and 11n prose while the table was never bumped. So the loop below printed two
`UNRECORDED CODE:` lines on a repository that was in fact perfectly healthy. **A stale pin and unrecorded work
look identical from here**, which is exactly why the rule is to stop and reconcile rather than assume: the
resolution was to read the prose, confirm the commit was accounted for, and correct the pin. Do the same if it
happens again — and bump this table in the *same* commit that lands code.

**One older wrinkle in `SAM`'s history, recorded rather than rewritten.** The step 7 review-fix commit
`d2b0f971` also carries a partial update of this file, because the handover edit was in progress when it
was staged. Nothing was force-pushed and nothing rewritten. It is historical now.

**2026-08-17 diagnostic-checkpoint session opened by finding two more pins stale — both benign, same
shape as the step 9 incident above.** `SAM_UI` had pushed `fcf0ec8` "Fix Part F assessment window review
findings" (`PartFConfirmationsTests.cs`, `AddVentilationByPartF.cs`, `PartFAirflowViewSettingsWindow.xaml.cs`,
`PartFAssessmentWindow.xaml.cs`) on top of the recorded `5b617e8`, and `SAM_Tas_Grasshopper` had pushed
`5b9fa39` "Remove stale TM59 XML when the scenario map is refused" (a PR #4 review-fix in
`SAMAnalyticalCreateTBDByTM59.cs`, deleting a stale TM59 companion file on a refused conversion) on top of
the recorded `94ac244`. Both were already committed, pushed, and level with origin — no dirty tree, no
divergence, just two review-fix commits that landed without a handover bump. Both pins are corrected above.
`SAM_UI #75` is out of scope for the diagnostic-logging work this checkpoint adds; the correction here is
bookkeeping only, made to keep the invariant honest before touching `SAM_Tas` / `SAM_Tas_Grasshopper` code.

**2026-08-18 Option B session opened by finding `SAM_Tas` stale again, plus a base sync in all five — both
benign, and the check earned its keep for the fourth time.** `SAM_Tas` had pushed `05155e6` "Log both sides
of the zone-guid identity separately, and PartFSpaceData's per-terminal-role rates" on top of the recorded
`d269d772` — the diagnostic-logger follow-up that produced the evidence this checkpoint acts on, committed
and pushed but never recorded here. Separately, **all five** repos printed `AGENTS.md` and
`PROJECT_PROGRESS.md` as unrecorded: those came in from a `sow/2026-Q3` base-sync merge (`SAM_Tas`'s is
`a8690ee`, first parent the recorded pin), not from feature work — the same merge shape §0a already warns
about for `SAM_UI`. Nothing was lost, nothing diverged. `SAM_Tas`'s pin is bumped to this checkpoint's code
commit below; the four documentation-only rows are left as they are, since `AGENTS.md`/`PROJECT_PROGRESS.md`
are not Part F code and re-pinning four repos to record a base sync would obscure the next real firing.

**2026-08-18 TM59 report-validation session — SAM and `SAM_Tas_Grasshopper` pins bumped; `SAM_Systems` and
`SAM_UI` unchanged.** SAM pushed three commits: `07d1c728` (a real, independently-discovered `Query.Simplify`
argument-order bug in the non-bedroom natural-ventilation branch — the summer-hours pair and the annual limit
were rotated by one constructor position; Pass/Fail unaffected), `3a386baf` (the new `TM59AssessmentReport`/
`TM59AssessmentReportFormatter` verification layer plus `Criterion2` on `TM59NaturalVentilationBedroomResult`),
and `3fcfb880` (a further real defect the BasePassive validation pass itself found while comparing the new
`report` output against the real Flat1 run — detail in 11v). `SAM_Tas_Grasshopper` pushed `2bee815` (the new
`report` Grasshopper output, thin over the SAM service) and `29c31db` (CI-only: registered
`TasLogPartODiagnostics`'s ComponentGuid — added in `32f2763` without a baseline entry — in the GUID-identity
check's baseline; the Windows CI job had been failing on every push to this branch since `32f2763`, for a
reason unrelated to any Part F/O/TM59 code). `SAM_Tas_Grasshopper`'s pin is bumped to `29c31db` even though it
is maintenance rather than production code — the same reasoning `SAM_Systems`' `24ed46a` "fix missing
reference" pin already established: 0a's invariant catches any unpinned commit, feature or not. `SAM_Systems`
and `SAM_UI` printed only the same `AGENTS.md`/`PROJECT_PROGRESS.md` base-sync rows already accepted as
benign (see the 2026-08-18 Option B entry above); their pins are unchanged.

**Why a HEAD is not pinned to a SHA.** The commit that updates this file cannot contain its own hash, so
a pinned HEAD would be wrong the moment it landed. The last **code** commit is pinned instead.

**The invariant, and a descendant check is NOT enough.** "HEAD descends from the recorded SHA" also
passes when somebody committed code and never recorded it — exactly the state this file exists to
prevent. So verify all three of:

1. every tree **clean** and every branch **level with its remote**;
2. HEAD **descends from** the recorded last-code SHA;
3. **every change between that SHA and HEAD touches only `documentation/PartF-HANDOVER.md`.**

Anything else — a dirty tree, an unpushed commit, a divergence, or unrecorded code between the
checkpoint and HEAD — means **stop and reconcile before changing any code**.

```bash
for r in SAM SAM_Systems SAM_UI SAM_Tas SAM_Tas_Grasshopper; do echo "=== $r ==="; git -C $r status --porcelain; git -C $r log --oneline -1; git -C $r log --oneline -1 origin/feature/partf-terminal-transfer-compliance; done
```

```bash
while read -r r sha; do git -C "$r" merge-base --is-ancestor "$sha" HEAD && echo "$r: descends from $sha" || echo "$r: DOES NOT CONTAIN $sha - STOP"; git -C "$r" diff --name-only "$sha" HEAD | grep -v '^documentation/PartF-HANDOVER\.md$' | sed "s|^|$r UNRECORDED CODE: |"; done <<'EOF'
SAM 3fcfb880
SAM_Systems 4446bb8
SAM_UI fcf0ec8
SAM_Tas 2750a21
SAM_Tas_Grasshopper 29c31db
EOF
```

That must print exactly **five** `descends from` lines and nothing else. **Any `UNRECORDED CODE:` or
`DOES NOT CONTAIN` line means stop and reconcile.** (An earlier version of this check passed a literal
`PENDING` for a repo that had no SHA yet and swallowed the error, so it silently verified nothing —
the loop above fails loudly instead.)

**All five now have open PRs against `sow/2026-Q3` — opened together, in dependency order, once every
branch was merged current with sow.** `sow/2026-Q3` itself never committed to directly, untouched
everywhere.

| Repo | PR |
|---|---|
| `SAM` | [SAM-BIM/SAM#73](https://github.com/SAM-BIM/SAM/pull/73) |
| `SAM_Systems` | [SAM-BIM/SAM_Systems#14](https://github.com/SAM-BIM/SAM_Systems/pull/14) |
| `SAM_Tas` | [SAM-BIM/SAM_Tas#29](https://github.com/SAM-BIM/SAM_Tas/pull/29) |
| `SAM_Tas_Grasshopper` | [SAM-BIM/SAM_Tas_Grasshopper#4](https://github.com/SAM-BIM/SAM_Tas_Grasshopper/pull/4) |
| `SAM_UI` | [SAM-BIM/SAM_UI#75](https://github.com/SAM-BIM/SAM_UI/pull/75) |

**Before opening them:** each branch was checked for drift against `sow/2026-Q3` (only `SAM` and `SAM_UI`
had any — 20 and 2 commits behind respectively), file-overlap was confirmed zero on both sides with
`git merge-tree`, and `sow/2026-Q3` was merged into `SAM` and `SAM_UI` with the full suite re-run green
after (`SAM.Tests` 1207/1207, `SAM.Analytical.UI.WPF.Tests` 180/180) before pushing. See 11r for a CI-only
test failure this surfaced and its fix — nothing in the merge itself was at fault.

### 0b. Latest checkpoint — what it implemented

**2026-08-18 — TM59 verification `report` output implemented, and Iteration 1 BasePassive report validation
CLOSED against the real Flat1 comparison. Detail in 11v.**

- SAM `07d1c728`, `3a386baf`, `3fcfb880` → the `TM59AssessmentReport`/`TM59AssessmentReportFormatter`
  verification layer over `TM59AssessmentResult` (`TM59ComplianceStatus` Pass/Fail/NotApplicable,
  `TM59RiskStatus` Acceptable/SignificantRisk, Criterion 1/2 stated separately, Actual/Limit/Margin,
  unassessed spaces, legend), a real `Query.Simplify` argument-order bug found and fixed ahead of it, and a
  further real defect the validation pass itself found: Criterion 1's report Limit read the annual
  `MaxExceedableHours` instead of `MaxExceedableSummerHours`.
- SAM_Tas_Grasshopper `2bee815` → `Tas.TSDQueryTM59Results` (now `1.0.9`) gets the new `report` output,
  appended last, thin over the SAM service. `29c31db` → unrelated CI-only fix: registered
  `TasLogPartODiagnostics`'s ComponentGuid in the GUID-identity baseline it had been missing from since
  `32f2763`.
- **Validated against the real Flat1 BasePassive run 11u already used** — the actual `.tsd`, the real SAM
  diagnostic JSONL and TAS's own "Domestic Overheating (CIBSE TM59)" report, parsed directly rather than
  taken from this handover's summary of them. All applicable occupied-space TM59 criteria match TAS's report
  exactly; the full-year >28 C / corridor-style bucket disagrees on `Corridor_1` by the same already-
  investigated design (11u), and correctly does not fail the occupied-space assessment.
- `SAM.Tests` **1230/1230** (Release, was 1229). `SAM.Analytical.Tas.TM59.Tests` **91/91**, unaffected.

**2026-08-18 — first SAM BasePassive vs native TAS comparison, on real Flat1 output. Detail in 11u.**

- SAM_Tas `2750a21` → `PartODiagnosticLog` now also logs `summerOccupiedHours`/`maxExceedableSummerHours`
  for `natural`/`naturalBedroom` rows, read straight off `TM59NaturalVentilationResult.SummerOccupiedHours`/
  `MaxExceedableSummerHours` (plain) or derived via `GetSummerOccupiedHours()`/`GetSummerMaxExceedableHours()`
  (extended) — the exact methods `Query.Simplify` (`SAM.Analytical`) already uses for the same purpose. 2 new
  tests, 91/91 `SAM.Analytical.Tas.TM59.Tests` in Debug **and** Release.
- **Why:** comparing a rerun of Flat1 BasePassive against TAS's own "Domestic Overheating (CIBSE TM59)"
  report for the same `.tsd` found the log's existing `occupiedHours`/`maxExceedableHours` pair is the
  whole-year annual basis, not TAS's day-criterion basis ("Occupied Summer Hours" / "Max. Exceedable
  Hours") — for `Studio 1_0` these read 8760/262 in the log against TAS's 3672/110, even though the actual
  exceedance count and Pass/Fail matched exactly. A log-completeness gap, not a TM59 defect — full detail
  and the space-by-space comparison table are in 11u.
- **Also investigated and closed, no code change:** SAM logged 2 hours exceeding 28°C for a zero-occupancy
  bathroom where TAS's report showed 0. Confirmed apples-to-oranges, not a defect — SAM's corridor/ancillary
  check deliberately runs over the full 8760-hour year regardless of occupancy (`MaxExceedableHours = 8760 ×
  3% ≈ 262` is the tell), while TAS's canned report skips the check entirely for a room it buckets as
  "Other". See 11u.

**2026-08-18 — the §11o identity defect FIXED and independently confirmed live, Option B CLOSED. Detail in 11t.**

- SAM_Tas `d76be13` → `Convert.ToSAM(TSD.ZoneData)` stamps `SpaceParameter.ZoneGuid` from
  `zoneData.zoneGUID` explicitly, mirroring `Convert.ToSAM(TBD.zone)`. One guarded `SetValue` plus
  `TsdZoneIdentityStampTests` (5 new). 89/89 `SAM.Analytical.Tas.TM59.Tests` in Debug **and** Release.
- **Root cause confirmed in source, and it is more specific than "the generic mapper is broken".** The
  `TypeMap` entry exists, but `SpaceParameter.ZoneGuid`'s registered name is `"Zone Guid"` — *with a space* —
  and `SAM.Core.Create.ParameterSet` reads the source property using that SAM-side name instead of the
  TAS-side `"zoneGUID"`, then stores under the TAS-side name. Inverted both ways; the space defeats even a
  case-insensitive match. Full trace in 11t.
- **The generic `Create.ParameterSet` / `TypeMap` defect is NOT fixed and is now a named deferred item in
  §0e.** It is the same defect behind `ZoneNumber`, `Description`, `Volume` and `FloorArea`.
- **Manual Flat1 BasePassive Grasshopper acceptance run — DONE, and it is clean.** Michal rebuilt and reran
  after this checkpoint's fix was installed to `%APPDATA%\SAM`. Evidence: `Flat1.BasePassive.partO.20260818-122737.jsonl`
  (run `c4aa16f2-990c-41b6-ae8f-c78eedd14294`, `runTimestampUtc 2026-08-18T12:27:37Z`) shows all 9 spaces at
  `identityMode: "zoneGuid"` with `designZoneGuidRaw` equal to `simulatedZoneGuidRaw` on every space (e.g.
  `{04860158-BDB1-4295-9EC6-DE80B409774D}` both sides — braces and casing identical, so the format-mismatch
  risk 11t called out did not materialise), `simulationSpaceMapIsComplete: true`,
  `overheatingScenarioMapIsComplete: true`, `unassociatedCount: 0`, `workflowSuccessful: true`,
  `tM59Successful: true`. TM59 outputs unchanged from the pre-fix baseline: 8 spaces pass, `Corridor_1` still
  fails at `hoursExceeding28: 337` vs `maxExceedableHours: 262`. The checkpoint is closed on this evidence.

**2026-08-17/18 — Part O → TAS → TM59 diagnostic logging, diagnostic-only. No compliance path touched.**

- SAM_Tas `d269d772` → `SAM.Analytical.Tas.TM59/Classes/PartODiagnosticLog.cs` (new, COM-free) +
  `PartODiagnosticLogTests.cs` (new, 11 tests). Composes `SimulationSpaceMap`, `OverheatingScenarioMap`,
  `Query.SimulationSpaceKey`, `PartFSpaceData`/`TMResult`/`OverheatingScenario`'s own `ToJsonObject()` into one
  JSON Lines summary per Part O run, plus an optional hourly companion. Calls no Part F/Part O/TM59
  calculation and edits no existing file.
- SAM_Tas_Grasshopper `32f2763` → `Tas.LogPartODiagnostics` (new component,
  `SAM.Analytical.Grasshopper.Tas/Component/TasLogPartODiagnostics.cs`), a thin wrapper that taps wires
  already on the Part O canvas, runs no simulation, mutates no model.
- **The round-trip question the plan opened with is resolved, in source, before any component code was
  written**: `Modify.RunWorkflow` → `WorkflowCalculator.Calculate` takes a copy of the `AnalyticalModel` and
  only ever mutates `Space` objects in place (`UpdateIds` touches `ZoneGuid` only; `UpdateDesignLoads` touches
  design-load values only) or reconstructs via `new Space(space)`/`new AdjacencyCluster(adjacencyCluster)`,
  both of which explicitly carry `InternalCondition` forward and deep-clone the parameter set
  `PartFSpaceData` lives in. **`PartFSpaceData` and the Part-F-applied `InternalCondition` survive the
  workflow unchanged** — so the logger takes **one** `AnalyticalModel_Design` input (the same wire that feeds
  `Tas.TSDQueryTM59Results._analyticalModel`), not a pre/post-workflow pair. Pinned as a test.
- **A pre-existing identity-provenance gap is recorded, not fixed, per the plan's explicit instruction not
  to touch it.** `Modify.UpdateIds` (`SAM_Tas/SAM.Analytical.Tas/Modify/UpdateIds.cs`) strips
  `SpaceParameter.ZoneGuid` from every space (line 22) before the loop that would otherwise read it (line
  56), so in practice every space's `ZoneGuid` is (re)assigned by a TAS zone **name** match during that
  workflow step, not read from a pre-existing guid. A `zoneGuid` identity mode is real — the key matches on
  both sides by the time the logger reads it — but does not, on its own, prove the chain one layer up was
  ever free of a name match. The logger records this as `zoneGuidProvenance: "assignedDuringWorkflow"` plus
  an explanatory note on every `run` record, so a reader of the log is not misled. This needs Michal's own
  decision (preserve the guid through the strip, or refuse an ambiguous name match) and is explicitly **not**
  addressed here.
- Two correctness bugs caught and fixed before this landed, neither in the plan: (1) the TM59 *extended*
  result types are not subtypes of their plain siblings — an unqualified `is TM59MechanicalVentilationResult`
  would have silently reported `criterion: null` for every row once hourly logging (`_extended_ = true`) was
  in use; (2) a result with no governing scenario was initially still being folded into a `space` row via a
  union with result-referenced spaces, contradicting the plan's own test 3. Both fixed; see the SAM_Tas
  commit body for detail.
- Not yet done: the plan's manual-acceptance steps on a live Grasshopper/Rhino/TAS canvas (wire the
  component, confirm Part F numbers match the GH panel on the real Flat1 BasePassive run, confirm
  `identityMode` on live data, confirm TM59 outputs unchanged connected vs disconnected). This session's
  environment has no Rhino/TAS install to run that canvas — flagged rather than claimed. **Whoever picks this
  up next should run that canvas before treating this checkpoint as fully closed.**
- SAM.Analytical built (`dotnet build`, Debug + Release). `SAM_Tas.sln` and `SAM_Tas_Grasshopper.sln` built
  with **VS Framework MSBuild** (Debug; `SAM_Tas.sln` also Release) — 0 errors, 0 warnings on the new files.
  `SAM.Analytical.Tas.TM59.Tests`: **81 passed, 0 failed** in both Debug and Release (11 new, 70 pre-existing,
  no regressions).

**Iteration 0 step 9 — the TSD-simple vs TPD-full preparation boundary, plus Iteration 1's BasePassive
vertical slice. Detail in 11o (step 9) and 11p (BasePassive).**

- SAM_Tas `418f508` + SAM_Tas_Grasshopper `697c798` → **step 9**. `ResultantTemperatureTransfer`,
  `ResultantTemperaturePreparation` and `ApproximateResultantTemperatureMap`; the legacy MRT+zone-temperature
  synthesis moved out of the Grasshopper component into `SAM_Tas`; `Tas.TPDQueryTM59Results` (1.0.2) reduced to
  read → prepare → assess with its third inline copy of the TM59 recipe deleted, and its three name matches
  replaced by the step 8 identity path; `Tas.CalculateResultantTemperatureFromTPD` (1.0.2) now reports why it
  refused. **The intended supply-temperature/airflow transfer is refused, not approximated — the limitation and
  its evidence are in 11o, and it is the most important thing in this checkpoint.**
- SAM `2e8c513f` → **Iteration 1, the REAL BasePassive slice (11q)**. Michal corrected the milestone: a scenario
  attached to an existing simulation is not the deliverable. `Modify.ApplyPartFVentilationRates` carries the Part
  F sized airflows onto the internal conditions the simulation actually reads — **they never reached it before**,
  because `Query.CalculatedSupplyAirFlow` had no knowledge of `PartFSpaceData` — and
  `SAMAnalytical.PreparePartOIteration` drives the whole slice with the applied rates inspectable. The Part F
  calculation itself is **reused unchanged**, not reimplemented.
- SAM `5f3ad8fb` → **Iteration 1, BasePassive scenario identity (11p)**, on Michal's earlier re-prioritisation.
  `Query.PartOOperatingAssumptions`, `Create.OverheatingScenarios`, and
  `SAMAnalytical.CreateOverheatingScenarios` — **the component that was missing**: three components accepted
  `overheatingScenarios_` and nothing created one, so the scenario-authoritative path could not be driven from
  Grasshopper at all. **Stating a stage still does not make a simulation obey it; see the scope boundary in 11p.**

**Previous checkpoint — Iteration 0 step 8 — identity-based association, complete and independently reviewed.
Detail in 11n.**

- SAM `d7abd48a` + SAM_Tas `1aba5eec` + SAM_Tas_Grasshopper `f8ca646c` → the original step 8
  implementation: `SimulationSpaceMap` applied to restore, selection and result association; TAS stable
  keys; and the normal `Tas.TSDQueryTM59Results` path stopped matching by name.
- SAM `f712fd9f` + SAM_Tas `7f3dfda` + SAM_Tas_Grasshopper `b8dae4b` → independent-review fixes:
  fail-safe bidirectional collision handling, resolved-only whole-model/zone selection, transactional
  scenario ownership, both existing TAS workflows wired, and acceptance coverage for three flats each
  containing a room named exactly `Bedroom 2`, plus the communal corridor.
- The new scenario inputs are **optional and appended after existing Grasshopper inputs**, preserving saved
  component port indices and the two existing production workflows. When scenarios are supplied they are
  authoritative; when absent the documented legacy fallback remains available. Verified in source: the
  `overheatingScenarios_` parameter is the last input registered in both
  `TasTSDQueryTM59Results` and `SAMAnalyticalCreateTBDByTM59`, and the latter keeps the two-argument
  `ToXml` on the no-scenario branch.

**One commit message overstates what was outstanding, and cannot be rewritten.** SAM `cc5e67f0` says a
fresh independent review of step 8 "is still owed". That was wrong when written: the review had completed
and its fixes are in SAM `f712fd9f`, recorded by `f2629967`. The agent *I* had spawned to re-review died on
a session limit, and I mistook that for the review itself not having happened. Nothing was force-pushed;
**this file is the correcting record**, which is what it is for. Step 8 has been reviewed.

**Previous checkpoint — Iteration 0 step 7, scenario-authoritative ventilation strategy. Detail in 11l.**

- SAM `193968ff` + SAM_Tas `f6e32b4` + SAM_Tas_Grasshopper `047c583` → **7a**, the behaviour-preserving
  repoint. `TM59AssessmentCalculator.SourceFallback` so provenance survives it,
  `Create.TM59AssessmentCalculator` keeping TAS's two series keys and provenance in TAS's assembly, and
  `Tas.TSDQueryTM59Results` calling the service. **The TM59 recipe now exists once.**
- SAM `f6772519` → **7b**, `VentilationStrategyMap` + `VentilationStrategySelection`. The criterion is
  stated by the scenario, and refused where nothing states it.
- SAM_Tas `b03f02b` → **7c**, the TM59 **XML export** takes the strategy from the scenario too, through
  `Space.ToTM59`'s existing `systemType` seam.
- SAM `d2b0f971` + SAM_Tas `121035c` → **review fixes**, including two real defects: an unrecognised
  strategy silently assessed as mechanical, and the export still dropping unexportable rooms silently.
  See 0d.

**Previous checkpoint — Iteration 0 step 5 (5a, 5b, 5c) and step 6.**

- SAM `b23bef3b` → **5a**: the analytical half — `SystemCapability`, `SystemCapabilityRequirement`,
  `SystemCapabilityDescriptor`, `SystemCapabilitySelection`, `Query.PartFSystemCapabilityRequirement`,
  `Query.CapableSystems`, `Query.SelectPreferredCapableSystem`.
- SAM `c5112e4f` → preference taken out of `SAM.Analytical` (Michal's correction), handover invariant
  strengthened.
- SAM_Systems `c6a441d` → **5b**: `CapabilityIndex.JSON` beside the ten templates,
  `Query.SystemCapabilityDescriptors`, `Query.SystemEnergyCentreResource`,
  `Query.SystemEnergyCentre(SystemTemplate)`, and the conformance test project.
- SAM `b52aff65` + SAM_Systems `e99f311` → review fixes, including a **real misselection** (see 0d).
- SAM_Systems `895a86d` → **5c**: `Application` becomes an eligibility constraint, rank recorded as
  provisional declared policy, commercial `Boost` recorded as unverified, CI executes the suite.
- SAM_Systems `bff125f` → 5c review fixes, including a **CI assembly-version restamp** and an unverified
  capability being credited (see 11j).

- SAM `d327496a` → **step 6**: `TM59AssessmentCalculator` / `TM59AssessmentResult` — the
  `Tas.TSDQueryTM59Results` recipe lifted into `SAM.Analytical`, behaviour unchanged.

Detail in **11h** (5a), **11i** (5b), **11j** (5c) and **11k** (6).

### 0c. Tests and builds run, with counts

| Suite | Result | How |
|---|---|---|
| `SAM/SAM.Tests` | **1208 passed, 0 failed** (was 1185; +11 `PartOIterationSliceTests`, +11 `PartFAirflowApplicationTests`, +1 `SimulationSpaceMapTests`) — verified in Release, matching CI | `dotnet test SAM/SAM/SAM.Tests/SAM.Tests.csproj -c Release` |
| `SAM_Tas/SAM.Analytical.Tas.TM59.Tests` | **89 passed, 0 failed** in Debug **and** Release (65 → 81 `PartODiagnosticLogTests` → 84 → +5 `TsdZoneIdentityStampTests`) | `dotnet test SAM_Tas/SAM_Tas/SAM.Analytical.Tas.TM59.Tests/…csproj` — **build SAM and `SAM_Tas.sln` (VS Framework MSBuild, per config) first**; this project links the prebuilt `build/` DLLs, so `dotnet test` alone silently tests the previous build |
| `SAM_Tas.sln` | 0 errors under VS Framework MSBuild | `MSBuild.exe SAM_Tas.sln -restore -p:Configuration=Debug` |
| `SAM.Analytical.Grasshopper.Tas.TPD` | 0 errors under VS Framework MSBuild | step 9's component changes |
| `SAM.Analytical.Grasshopper` (SAM) | 0 `error CS` under VS Framework MSBuild | the new scenario component. **Its post-build step fails with `MSB3073` copying to `%APPDATA%\SAM`** — the known PostBuild APPDATA trap, pre-existing and unrelated; pass `-p:PostBuildEvent=` to compile only |
| `SAM_Tas_Grasshopper/SAM.Analytical.Grasshopper.Tas` | 0 `error CS` under **VS Framework MSBuild** | no test project in that repo; equivalence is pinned in `SAM.Tests` |
| `SAM_Systems/SAM.Analytical.Systems.Tests` | **34 passed, 0 failed** | unchanged by step 7 |
| `SAM_Systems/SAM.Analytical.Systems.Mollier.Tests` | **123 passed, 0 failed** | unchanged by step 7 |
| `SAM_Systems.sln` | 0 `error` under VS MSBuild | the test project is in the solution AND executed by CI |
| `SAM_UI`, `SAM_Mollier` | **unchanged since their last run** — see section 2 | not re-run; nothing in them changed during step 8 |

**Step 8 mutation checks all behaved correctly** (the named test failed under the mutation, then passed
after restore):

1. restoring simulated/design conditions by name instead of identity → the mandatory duplicate-`Bedroom 2`
   regressions fail;
2. bypassing the whole-model resolved-identity filter →
   `TwoCandidateMappingsForOneDesignIdentity_RefuseExplicitly` fails;
3. treating `spaces == null` as whole-model even when zones were supplied →
   `Zones_ContributeTheirSpacesByIdentityWithoutDuplicating` fails;
4. publishing ventilation claims while scenario ownership was still being accumulated →
   `TwoScenariosOverOneSpace_RefuseExplicitly` fails.

**The earlier five step 7 mutation checks also remain recorded and green after restore:**

1. making a refusal fall back to `SystemTypeName` → fails **4** tests;
2. removing the strategy normalisation → fails `UV_StillRoutesToTheCorridorCriterion`;
3. returning the `Building` regardless of refusals → fails `AnUnsettledStrategy_RefusesTheWholeExport`;
4. disabling the closed ventilation vocabulary → fails **5** tests;
5. disabling the export's dropped-zone guard → fails `ASpaceThatCannotBeExported_RefusesRatherThanVanishing`
   and `ANullManager_RefusesRatherThanExportingNothing`.

**A trap worth remembering, hit during mutation 5.** The `SAM_Tas` test project references
`SAM/build/SAM.Analytical.dll`, so a mutation left in `SAM.Analytical` and not rebuilt away silently
poisons the *next* suite you run. It looked like a third test failing for the export guard; it was the
previous mutation still in the built DLL. **Rebuild SAM before every SAM_Tas run**, mutation testing
included.

### 0d. Architectural decisions made in this checkpoint (step 8)

1. **Association is identity-first at every boundary.** `SimulationSpaceMap` now controls design-condition
   restore, requested-space/zone selection, scenario projection and result attribution. Names are only the
   map's unique fallback; no assessment code independently joins by name.
2. **Ambiguity invalidates both directions.** If two simulated spaces claim one design identity, neither
   forward mapping survives and the reverse mapping is refused. Keeping either candidate usable would let
   the ambiguous pair receive a design internal condition or scenario.
3. **Whole model means every resolvable simulated space, not every simulated space.** Ambiguous or unresolved
   spaces are excluded and reported. A whole-model request cannot undo an earlier identity refusal.
4. **A zones-only request is scoped to those zones.** `spaces == null` is not whole-model when `zones` is
   supplied; zone-contributed spaces are resolved and de-duplicated by identity.
5. **Scenario ownership is transactional.** A second scenario claiming an already-owned design space
   invalidates both scenarios for that space and rolls back the first scenario's complete stored selection.
   The live `VentilationStrategyMap` is built only after ownership is settled, so a refused scenario leaves
   no stale strategy claim behind.
6. **The normal TAS route uses TAS zone identity.** `Tas.TSDQueryTM59Results` builds a
   `SimulationSpaceMap` from each TAS zone guid. Unique-name fallback remains only where a stable key cannot
   be recovered, and duplicate names refuse rather than cross flats.
7. **The official TAS print route uses design identity.** `SAMAnalytical.CreateTBDByTM59` starts from the
   design spaces themselves, so its map is identity. The resulting XML/TBD/TSD continues into the existing
   TAS TM59 tabs and print workflow; no substitute user workflow was introduced.
8. **Both scenario inputs are optional and appended.** Existing Grasshopper input indices and saved
   definitions are preserved. Supplied scenarios are authoritative and use the new identity architecture;
   absent scenarios retain the previously documented fallback path.
9. **Assessment and export keep different refusal contracts.** Assessment returns the valid partial result
   set plus refusals. Official XML export refuses the whole document when any requested scenario association
   is incomplete, because TAS cannot display the omitted room as a refusal.
10. **The communal corridor remains common space.** It is mapped and assessed with its own scenario; it is
    never attributed to one of the three dwellings.

### 0d-prev7. Architectural decisions from the step 7 checkpoint

1. **The scenario is authoritative, and a refusal never falls back.** Where a `VentilationStrategyMap` is
   supplied, none of the three derivations in 11d is consulted — not as a seed, not as a tie-break. Falling
   through on refusal would restore the defect invisibly, at the one input where nothing was said.
2. **Three refusals, three sentences.** "No scenario covers this space", "the scenario states no strategy",
   "two scenarios disagree" and now "the strategy stated is not one I have a criterion for" are different
   mistakes with different fixes. Collapsing them would send somebody looking in the wrong place.
3. **The recognised ventilation vocabulary is CLOSED** — `NV MV MVRE UV EOL EOC CAV VAV DISP`. The criterion
   selection reads `UV` as corridor, `NV` as natural and *everything else* as mechanical, and an open default
   in the authoritative path is the step 7 defect pointing the other way: a scenario stating `"Natural"`, or
   `"N-V"`, or `"MVHR"` — **a name that does not exist, because `MVRE` is SAM's heat-recovery ventilation** —
   was assessed mechanically and reported as a result. Found by review. **The set is declared policy, like
   `SAM_Systems`' rank, and a project with a custom system-type library needs it extended.**
4. **The vocabulary is NOT read from `Query.DefaultSystemTypeLibrary()`.** That comes from `ActiveSetting` and
   can be absent, and making the authoritative path depend on the same installed resource the *defective*
   derivation used would trade one silent failure for another. A list of names is vocabulary and belongs in
   `SAM.Analytical`, which already names `UV` and `NV` in `Query.IsMechanicalVentilation`.
5. **The assessment drops a refused space; the export refuses the whole document.** Asymmetric on purpose. A
   result list is naturally partial and `TM59AssessmentResult` reports the gaps. A TM59 XML is configuration
   for the external TAS TM59 tool, which cannot be told a room is missing — it would assess what it was given
   and produce a complete-looking answer for an incomplete building.
6. **"Cannot be exported" counts as a refusal too.** `Space.ToTM59` returns null for a space with no internal
   condition, and for a null `TM59Manager`. Found by review: those were dropped silently and the completeness
   gate still passed, so a three-space building shipped two zones as a success.
7. **A null return always carries a reason.** Found by review: with a map supplied and nothing to export, the
   export returned null with an *empty* refusal list, so the documented contract lied.
8. **Provenance and routing never touch the criterion.** Not `Source`, not TSD-versus-TPD, not which engine
   wrote the numbers. Tested on both routes.
9. **The map is live and held by reference; the scenario is copied.** A map is built up scenario by scenario
   and is not an identity, so it is not copied in — the opposite of `OverheatingScenario`, deliberately, and
   now stated and tested rather than left to be discovered.
10. **Refusals are reported, not thrown**, so one unstated dwelling does not cost every other dwelling in the
    building its assessment. And they are a **copy** on both types — a reporting layer that de-duplicates in
    place was able to erase the record of which dwellings went unassessed.
11. **`Calculate_TM52` deliberately does not clear the TM59 refusals.** TM52 selects no criterion, so
    clearing them would let a TM52 run erase a TM59 run's record.

### 0d-prev5. Architectural decisions from the step 5 checkpoint

1. **`SAM.Analytical` owns the capability vocabulary + the Part F requirement rule + suitability +
   preference-by-supplied-rank. `SAM_Systems` owns the capability VALUES and the rank.** Michal's
   decision. Same cut as TAS: analytical states intent, the specialised assembly owns implementation.
2. **`SAM.Analytical` must NOT infer preference from capabilities.** An earlier revision chose the
   system with the fewest capabilities. That is a policy about a particular library, not something that
   follows from Part F — a capability a system happens to have may cost nothing to specify. Preference
   is now `SystemCapabilityDescriptor.Rank`, supplied by the catalogue and never derived.
   `Query.CapableSystems` (suitability) is separate from `Query.SelectPreferredCapableSystem` (choice).
3. **`SystemCapability.MechanicalSupply` exists because of a real misselection the review found.** A
   balanced dwelling — paragraph 1.67, a supply terminal in every habitable room — was met by
   `Local Extract Only`, because extract-only does run continuously and can boost. The overheating
   simulation would have run a system with no supply and no heat recovery against a building with both.
   `TotalSupply_Lps` is the indicator: those terminals carry `IsInBalancedFlow`.
4. **`HeatRecovery` is a capability, not an identity.** `MVRE` remains SAM's heat-recovery ventilation.
5. **Part F requires continuous ventilation, mechanical supply where supply was sized, and boost —
   never summer bypass or heat recovery.** Those are Part O mitigation a scenario states.
6. **Refuse, never approximate, and refuse rather than guess.** No capable system ⇒ refusal naming what
   was missing. Two *different* suitable systems at the same rank ⇒ refusal (the catalogue has not said
   which is preferred). One system listed twice ⇒ not ambiguous, it is selected.
7. **A malformed capability index disables selection rather than reordering it.** A missing or mistyped
   `Rank`, or a present-but-unusable ventilation identity, refuses the whole index.
8. **`Application` is an ELIGIBILITY constraint owned by `SAM_Systems`, not documentation.** A Domestic
   request is never offered a commercial template, and rank is no longer what keeps one out — see 11j.
   The constraint parameter is **not optional**, so the commercial-inclusive call cannot be written by
   omission.
9. **The eligibility guarantee is scoped to capability selection.** The older
   `DefaultSystemEnergyCentres`/`Create.SystemEnergyCentre` path bypasses the index entirely and is
   unguarded — recorded, pinned by a test, not changed.
10. **An unverified capability is never credited.** Every capability an index entry lists under
   `UnverifiedDeclarations` must be `false`, asserted.
11. **`SystemCapabilityDescriptor` is not an `IJSAMObject`.** It had a second, incompatible JSON shape
   from the one the catalogue actually uses, and its own reader turned a real index entry into a
   confident empty descriptor. The index is the wire format.

### 0e. Explicitly deferred

- **`SAM.Core.Create.ParameterSet` / `TypeMap` is BROKEN for every TAS-imported parameter whose SAM-side
  name differs from its TAS-side property name — a separate SAM infrastructure task, deliberately not fixed
  in the Option B checkpoint.** `ParameterSet` reads the source property by the **SAM-side** name and stores
  the result under the **TAS-side** name, i.e. inverted in both directions (11t has the line-by-line trace).
  Any registration whose two names differ therefore silently produces nothing — `SpaceParameter.ZoneGuid`
  (`"Zone Guid"` vs `"zoneGUID"`) was the one that mattered for identity and is now stamped explicitly
  instead; `ZoneNumber`, `Description`, `Volume` and `FloorArea` are in the same position and are **not**
  fixed. **Fixing the shared helper changes behaviour across many TAS-imported parameters at once**, which is
  why it was cut out of the identity repair. Note the trap for whoever takes it: correcting the direction
  will start populating parameters that are absent today, so anything downstream that currently reads a
  default because the value never arrived will change answer.

- ~~CI runs no tests in `SAM_Systems`~~ — **CLOSED in 5c.** `build.yml` now executes
  `SAM.Analytical.Systems.Tests` after the ordered Rebuild. **Not yet demonstrated on a real run**: the
  workflow triggers on `master`/`main`/`sow/**` only, so it will first fire when a PR to `sow/2026-Q3`
  is opened. Other SAM repos still have the original gap.
- **The domestic `Rank` order is PROVISIONAL and not confirmed** — `NV 10, EOL 20, EOC 30, MV 40,
  MVRE 50`. It makes selection deterministic and claims nothing about engineering preference. Michal to
  confirm or replace. Commercial ranks now order commercial-to-commercial selection only.
- **`ContinuousVentilation`, `MechanicalSupply` and `Boost` are DECLARED** from each system type's
  meaning, not read from the templates — nothing in the files marks any of them. Each index entry says
  so in `EvidenceFromTemplate`. Michal to confirm the values, particularly `CAV`/`DISP` boost = false.
- ~~`Application` is documentation only~~ — **CLOSED in 5c**, it is now an eligibility constraint (11j).
- ~~The `Tas.TSDQueryTM59Results` Grasshopper component still holds its own copy of the step 6 recipe.~~
  **CLOSED in step 7a.** `SAM_Tas_Grasshopper` is the fifth repo and the component calls the service.
- ~~`TasTPDQueryTM59Results` still holds a THIRD inline copy of the recipe.~~ **CLOSED in step 9** (11o). The
  recipe now exists once; the component calls the service. **New deferred item in its place: the TPD-full route
  cannot transfer supply air temperature into a TBD copy at all** — it is refused rather than approximated, and
  the supported transfer (the first pass's achieved zone temperature into the thermostat limits) is an
  approximation. See 11o; this needs Michal's decision, and possibly EDSL's.
- **`PartOIteration` still has no effect on the SIMULATION INPUTS.** BasePassive can be stated and assessed
  (11p), but nothing writes aperture control or ventilation rates into a TBD, so a stated stage is not a
  modelled stage. `AcousticRestricted` is therefore key-distinct but behaviourally identical to BasePassive on
  the input side, and `ActiveTrimCooling` refuses outright.
- **The four Part O assumption VALUES are declared policy** and need Michal's confirmation (11p). Their
  **names** are permanent — they are in `OverheatingScenario.Key`.
- ~~No production caller can supply a `VentilationStrategyMap`.~~ **CLOSED in step 8.** Both existing
  Grasshopper entry points accept optional `OverheatingScenario` inputs and construct the identity-aware
  scenario/ventilation maps. Supplying scenarios takes the authoritative path; omitting them preserves the
  existing workflow and its documented legacy derivations. The thin headless runner remains step 10; it is
  not a replacement for either production acceptance path.
- **The closed ventilation vocabulary is declared policy and needs Michal's confirmation** — the nine
  identities in `VentilationStrategyMap`. A project shipping a custom `SAM_SystemTypeLibrary` would have its
  extra identities refused.
- **The `Create.SystemEnergyCentre` / `DefaultSystemEnergyCentres` path is UNGUARDED** and is the one with
  production callers (four Grasshopper components). It derives a template from a file NAME and matches a
  space's `VentilationSystemTypeName`, so a space carrying `"VAV"` still resolves `VAV.json` in a dwelling
  model. Pinned by a test, not fixed — **needs its own change and regression**.
- **The commercial `Boost` values are unverified declarations**, all `false` so they refuse rather than
  mis-assess. Not to be relied on until confirmed; nothing in Part F work turns on them, since commercial
  templates are ineligible for a dwelling.
- **No shipped template models a summer bypass**, so Iteration 2 needs one; until then a bypass
  requirement is refused. Verified against all ten files.
- **No template self-identifies its ventilation type.** The identity↔file link is only the index's
  `Resource` string, so swapping two templates' contents is undetectable.
- `SystemTemplate.FromJsonObject` whitespace asymmetry — 11f item 7. Worked around at both boundaries
  that matter; the shared path is still inconsistent for other consumers.
- Everything in 11f items 1–5.

### 0f. The precise next task

**The Option B checkpoint is closed.** The Flat1 BasePassive Grasshopper acceptance run confirmed the
identity now resolves by guid on all 9 spaces, with TM59 outputs unchanged — evidence in 0b and 11t.

**The SAM-vs-native-TAS validation Michal flagged next has started — one comparison run done, on the same
Flat1 BasePassive data.** 8 of 9 spaces agree with TAS's own report on every number that governs pass/fail;
the 9th (`Corridor_1`) disagrees by design (SAM's own communal-corridor criterion, which TAS's canned report
has no equivalent for — already anticipated in 11t's acceptance criteria, not a new finding). One log gap
found and closed (11u), one apparent discrepancy investigated and found not to be a defect (11u). **Not yet
done:** the other TM59 criteria/scenarios this one flat happens not to exercise, and a second model to widen
the sample beyond one flat.

**Two things need Michal, and neither is code:**

1. **Confirm the four BasePassive/AcousticRestricted assumption VALUES in 11p.** The names are permanent and
   cannot be changed without re-keying every scenario; the values are declared policy read off
   `PartOIteration`'s own stage definitions, not from Approved Document O.
2. **Decide what to do about the TPD-full limitation in 11o.** The intended supply-temperature transfer is
   impossible in the TBD object model. The options are: accept the achieved-zone-temperature transfer as the
   documented approximation (what the code does now), raise it with EDSL, or change the route's shape.

**Then, in order:**

3. **Confirm which Approved Document F condition `AcousticRestricted` simulates at** (11q). Its assumptions say
   boost is *available*, not continuous; running a whole cooling season at the Table 1.2 high rate is a much more
   favourable claim. Refused until decided rather than guessed.
4. **Wire the ventilation SYSTEM selection** — the remaining half of 11q. Step 5's capability selection has **no
   production caller anywhere**, and nothing calls `Modify.AddVentilationSystem` from Part F, so the *selected
   system* is still not represented on the model. Note the constraint: descriptors come from `SAM_Systems`, which
   `SAM.Analytical` deliberately does not reference (11h), so a caller must supply them.
5. **Apply a stage to aperture control** — so BasePassive's "openings operated without restriction" is enforced
   on the model rather than only stated. This is what would make `AcousticRestricted` mean anything.
6. **Step 10** — the thin headless TAS runner, last.

**The BasePassive slice as it now stands, for manual testing:**
`AnalyticalModel` → `SAMAnalytical.AddVentilationPropertiesByPartF` (or `CheckPartFCompliance`) →
`SAMAnalytical.PreparePartOIteration` (`BasePassive`) → inspect `supply m3/s` / `extract m3/s` / `partF l/s` →
`To gbXML` → `SAMAnalytical.WorkflowgbXML` (`Simulation=true`) → `Tas.TSDQueryTM59Results` with
`overheatingScenarios_` connected.

`TM59AssessmentCalculator` must remain engine-neutral and must never learn the words TSD or TPD.

### 0g. Environment needed to continue

- **TAS-facing projects need VS Framework MSBuild**, not the dotnet CLI (MSB4803 / `ResolveComReference`):
  `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.
  `SAM.Analytical`, `SAM.Tests`, `SAM.Analytical.Systems` and its test project build fine under `dotnet`.
- Build **SAM before SAM_Systems / SAM_UI / SAM_Tas** — they reference its built DLLs from `SAM\build\`.
- `SAM.Analytical.Systems` has **no `<LangVersion>`**, so it compiles as **C# 7.3** — no collection
  expressions, no target-typed `new`. Its test project is C# 12.
- The conformance test reads the repository's own resources by walking up for the ancestor holding both
  `files/resources/…/SystemEnergyCentre` **and** `SAM_Systems/SAM.Analytical.Systems`. It therefore only
  runs from a source checkout, and it deliberately does **not** accept `build_tests/net8.0/`, where the
  Mollier tests copy two templates.
- Section 9 has the rest of the gotchas.

---

## 1. Where the work is

**COMMITTED AND PUSHED in all FIVE repos. Not merged, no PR opened.** All working trees are clean and
level with their remotes. Do not merge, squash, force-push, or open a PR unless Michal asks.

All five repos are on `feature/partf-terminal-transfer-compliance`. **SAM_Tas's, SAM_Systems's and
SAM_Tas_Grasshopper's branches are new and need PRs like the other two.** `sow/2026-Q3` was never committed to directly and is
untouched everywhere (SAM_Tas verified at `3d58bfe` local and remote).

**Current code heads — SAM `8de00cc7`+handover, SAM_Systems `24ed46a`, SAM_UI `ffd8e38`, SAM_Tas
`134dc1d`, SAM_Tas_Grasshopper `94ac244`. All pushed and verified. See section 0a, which is
authoritative.**

Note for review: `ffd8e38` (SAM_UI) is the Part F dwelling-scope/cache work and is a *different review
topic* from the two Iteration 0 commits in SAM/SAM_Tas — worth looking at separately.

- **SAM**: `feature/partf-terminal-transfer-compliance`, on `sow/2026-Q3` @ `34dea440`.
  - `cd54a62b` shared `Solver2D` hardening + `Solver2DTests`
  - `ae921be4` the Part F analytical body (correction pass, new classes/enums/tests, 2 GH components, rule set, docs)
  - `92c685e0`, `6cefcc08`, `0959b256`, `6148b554` handover
  - `b0e72a21` `AdjacencyCluster.PartFDwellingZoneCategories()`
  - `c5ca006e` the dwelling-selection policy given a single home (see 6b)
  - `cdd36e36` record the pushed checkpoint SHAs in this handover
  - `17a881e8` handover after the saved-view correctness fixes
  - `060feeda` **Part O scope + `SimulationSpaceMap`** (see 11c)
  - `9cbf308a` **`TMOverheatingCalculator` extraction** (see 11c)
  - `2e2362f7` doc terminology
  - `52698867` record the Iteration 0 state in this handover
  - `7faf964c` **`OverheatingScenario` + derived deterministic key** (step 4, see 11c)
  - `02e99582` **step 4 hardened after an independent review** (see 11c)
  - `5d4274a5` handover after step 4
  - `884a2f54` **step 4.1 canonicalisation** — assumption names normalised before sorting, JSON primitives through the typed canonicaliser (see 11c)
  - `342dfb26` handover after step 4.1
  - `b23bef3b` **step 5a — analytical half of system capability selection** (see 11h)
  - `b4d5aaa4`, `27ef1730` handover: section 0 and the repo-state invariant
  - `c5112e4f` **preference taken out of `SAM.Analytical`** + the stronger invariant (Michal's review)
  - `b52aff65` **`MechanicalSupply` + selection hardening** (review of 5a/5b, see 11h)
  - `f712fd9f` **step 8 independent-review fixes**
  - `cc5e67f0` **step 8 further review**: same-key scenarios are one answer, proof 10 made behavioural (11n)
  - `5f3ad8fb` **Iteration 1 — BasePassive stated and assessable** (11p)
  - `2e8c513f` **Iteration 1 — Part F airflows reach the simulation** (11q)
  - merge of `sow/2026-Q3` (43 files, zero overlap with this branch)
  - `5565d96c` **CI-only TM59 test fix** (11r)
  - `0e33ed27` **TMOverheatingCalculator null-library NRE fix** (11r)
  - `0fa247fd` **SAM.Tests made independent of an installed SAM** (11r)
  - `8de00cc7` **Codex review findings addressed on PR #73** (11s) — last CODE commit, pushed; HEAD is the
    handover commit on top
- **SAM_UI**: `feature/partf-terminal-transfer-compliance`, on `sow/2026-Q3` @ `074f3d9`.
  - `e787105` shared 2D-view infrastructure (`FloorPlan2DControl.Overlay`/`Plane`/`WorldToScreen`/`ViewChanged`,
    `AdjacencyCluster.SpaceSectionFace2Ds`, label-solver diagnostic reading `ResultType`)
  - `8782c40` Part F presentation checkpoint (window, overlay, view settings, placement adapter, annotation
    identity, all tests). One commit on purpose: the correction-pass fixes, the persistence types and the
    placement adapter are mutually dependent and the window is a single new file containing all three.
  - `efaed83` Part F airflow on the normal saved 2D views (the one renderer)
  - `150ea63` the new-view preset
  - `31d96cc` test-doc follow-up — last pushed commit
  - `ffd8e38` **dwelling scope + cache safety** (sections 6b, 6c) — `PartFDwellingScope`,
    `PartFAssessmentCache`, `PartFAssessmentCacheTests` new; the preset, the view settings, the dialog, the
    scope-gate in `AnalyticalWindow.PartF.cs` and the button text changed
  - **HEAD = `ffd8e38`**, pushed
- **SAM_Tas**: `feature/partf-terminal-transfer-compliance` (**new this session**), off `sow/2026-Q3` @ `3d58bfe`.
  - `5e38c94` `OverheatingCalculator` reduced to a compatibility wrapper + 3 equivalence tests (see 11c)
  - `d56f679` doc terminology
  - `1aba5eec` original step 8 TAS identity mapping
  - `7f3dfda` step 8 review acceptance coverage for both TAS paths
  - `418f508` **step 9 — the TSD-simple vs TPD-full preparation boundary** (11o)
  - `134dc1d` **step 9 independent-review fixes** (11o)
  - **HEAD = `134dc1d`**, pushed
- **SAM_Systems**: `bff125f` unchanged by step 8; `24ed46a` **"fix missing reference"** — Michal's own local build
  fix, adding the `SAM.Architectural` reference `SAM.Analytical.Grasshopper.Systems.csproj` was missing.
  Unrelated to Part F/O; recorded here only because §0a's invariant caught it as unpinned. **HEAD = `24ed46a`**,
  pushed.
- **SAM_Tas_Grasshopper**: `f8ca646c` original step 8 result association; `b8dae4b` review wiring for
  both existing workflows; `697c798` **step 9 — the TPD query component made thin over the common
  assessment** (11o); `94ac244` step 9 review fix; **HEAD = `94ac244`**, pushed.

## 2. Validation state (all green at handover)

| Suite | Result |
|---|---|
| `SAM/SAM.Tests` | **1185 passed, 0 failed** |
| `SAM_UI/WPF/SAM.Analytical.UI.WPF.Tests` | **180 passed, 0 failed** (123 + 21 placement + 7 identity + 6 whole-floor + 17 preset/scope + 6 assessment cache) |
| `SAM_Systems/SAM.Analytical.Systems.Mollier.Tests` | **123 passed, 0 failed** |
| `SAM_Systems/SAM.Analytical.Systems.Tests` | **34 passed, 0 failed** — capability index conformance + eligibility |
| `SAM_Mollier/SAM.Core.Mollier.Tests` | **22 passed, 0 failed** |
| `SAM.Core.Mollier.UI.WPF` | 0 `error CS` — builds unchanged against the hardened engine |
| `SAM.Analytical.Grasshopper` | 0 `error CS` under VS MSBuild |
| `SAM_Tas/SAM.Analytical.Tas.TM59.Tests` | **46 passed, 0 failed** |
| `SAM_Tas_Grasshopper/SAM.Analytical.Grasshopper.Tas` | 0 `error CS` under VS Framework MSBuild — no test project in that repo |
| SPDX | present on every changed `.cs` |

`dotnet build` on the Grasshopper project exits non-zero on a post-build `%APPDATA%` copy (no Rhino
installed). **Check for `error CS` only.** Same for MSB4803 on SAM_Tas_Grasshopper.

## 3. What is DONE

### 3a. Regulatory correction pass (reviewed and approved, 8/8 items)

1. **The continuous design rate no longer includes the sum of Table 1.2 high rates.**
   `ContinuousDesignSystemRate_Lps = max(BedroomOrHabitableRate, 0.3 × InternalFloorArea)` and nothing
   else. New `AllocateContinuousExtractBelowMinimumTotal` handles the case where the per-room minima
   total *more* than that: share the whole-dwelling rate pro-rata, each room boosts to its own figure.
2. Per-room check renamed **"Each extract room reaches its Table 1.2 minimum high rate"**; the
   whole-dwelling check states the per-room sum does not raise it.
3. **Required / proposed / provided split** on the terminal: `RequiredHighFlowRate_Lps`,
   `ProposedExtractMethod`, `ProvidedExtractMethod` (new `PartFExtractMethod.NotSpecified`),
   `ProvisionStatus`. An unrecorded kitchen terminal is sized but never counted as provided.
4. **Calculated failures cannot be overridden.** `CalculatedStatus` + `FinalAssessmentStatus` +
   `UserEvidence` / `AlternativeComplianceMethod` / `OverrideReason` / `ConfirmedBy` /
   `ConfirmationDate`, and `ApplyUserResolution` redirects a claimed pass to
   `AlternativeSolutionPendingApproval` or `EngineeringReviewRequired`. `IsCalculatedFailureOverstated`
   catches a `Status` assigned from outside that method; `Resolve()` keeps the dwelling at **Fail**
   unless an alternative method was recorded.
5. Door l/s relabelled **calculated routing** everywhere (schedule "INTERNAL TRANSFER AIR ROUTING
   (CALCULATED)", columns "Calculated …").
6. Report + `PartF-ADF-Volume1-2021-Traceability.md` corrected; the old third governing term is
   recorded as removed.
7. Tests updated/renamed where they asserted the removed rule.
8. Six follow-up fixes: floor-plan selection bug, schematic/plan obeying `OpeningStatus`, supply
   high/boost wording, "Sizing method" rename, allocation-note rewrite, label placement.

### 3b. Floor-plan overlay (replaces the old node diagram, which is DELETED)

- `FloorPlan2DControl` gained the one minimal shared hook: screen-space `Overlay`, `Plane`,
  `WorldToScreen`, `ViewChanged`, and hit-test exclusion.
- `AdjacencyCluster.SpaceSectionFace2Ds` exposes the existing cached section.
- `PartFFloorPlanOverlay` + `PartFOverlayMark` (WPF-free, tested): real coordinates, space marks at
  the section outline's **internal point** (not centroid — that falls outside an L-shaped room),
  transfer marks on the modelled door or the longest cut segment of the shared partition.
- Terminal marks are a **point + direction** in the model. ~~drawn as a short screen-space stub~~ —
  **superseded by 5e**: a terminal is now drawn as a white tag and nothing else. The `Direction` is still on
  the mark and unused by the renderer; only TRA marks span real distance.
- `PartFTransferOpeningStatus` (derived, separate from `PartFTransferRouteStatus`): a
  `MissingTransferOpening` route gets no span — a dashed cross on the partition, a `?` in the label
  text, caption "No modelled transfer opening identified".
- `PartFSelectionResolver` — guid-only, searches the whole selection.

### 3c. Persistence spike (proven, section A of the last addendum)

A second parameter enum from the analytical layer **can** be associated with `ViewSettings`, and the
whole chain round-trips through `UIGeometrySettings` → `AnalyticalModel` → JSON.

New in `SAM.Analytical.UI`: `AnalyticalViewSettingsParameter`, `PartFAirflowViewSettings`,
`PartFAnnotationOverride`, `PartFAnnotationType`, `PartFDwellingFilter`, `PartFViewJson`.

**The correction that spike produced, and it was not theoretical:** naming the enum
`ViewSettingsParameter` (mirroring the `AnalyticalModelParameter` precedent) **shadows**
`SAM.Geometry.UI.ViewSettingsParameter` in every file importing both namespaces and broke **seven**
existing `.Group` / `.UseDefaultName` call sites. Hence `AnalyticalViewSettingsParameter`. The reason
is recorded on the type — do not "tidy" it back.

## 4. Decisions — do NOT silently revisit

1. Minimum-first cooking-priority is the default extract allocation; `VolumeWeighted` retained.
2. Table 1.2 is **two** requirements: total continuous ≥ whole-dwelling rate, and each room ≥ its own
   **high**-rate minimum. The per-room sum never raises the continuous rate.
3. The transfer network is built from internal separating **panels**, not doors.
4. A confirmation can resolve an undecidable check; it can **never** overturn a calculated failure.
5. Absence of evidence is never compliance. The output is an assessment, never a certificate.
6. Model owns engineering values; **view owns presentation only**. No flow rate, status or terminal
   may ever be stored in view settings — there is a reflection test asserting this.
7. Manual label positions are **world-plane `Point2D`**, never pixels; `ViewGuid` implicit; **not** keyed by
   operating mode. ~~Keyed on the annotated object's own guid~~ — **superseded, see 5d**: the object's own
   guid is regenerated by every calculation, so the key is DERIVED via `PartFAnnotationKey`.
8. A view with no Part F parameter reopens with the overlay **OFF**.
9. Stale overrides are **ignored**, never pruned on load.

## 5. DONE: Solver2D adopted as the Part F tag engine (task B)

Delivered as two separate changes, in this order, so a Mollier regression is attributable.

### 5a. Shared `Solver2D` hardening (`SAM.Geometry`, additive)

- **Both null paths guarded.** `obstacles2D` null was a real `NullReferenceException` on the first
  candidate — `new Solver2D(area, null)` crashed. The other one **could not actually throw**: `InRange`
  is a null-guarded extension method, so an earlier unplaced item was silently *skipped*, which is what
  the new explicit `continue` does. Both are now explicit and tested. Michal's review said "appears
  capable of dereferencing"; the audit says it skips instead. No behaviour change from that one.
- **`Solver2DResultType { Undefined, Solved, Fallback, Unplaced }`** on `Solver2DResult.ResultType`.
  `Undefined` was added to the three approved members so a defaulted value cannot read as `Solved`;
  `Solve()` never returns it. The old 2-arg `Solver2DResult` ctor is kept and derives the type from the
  geometry — `Solve()` deliberately does not use it, because a fallback rectangle would come out `Solved`.
- **Ordering is total**: priority, then insertion index, by sorting an index list. `List.Sort` stability
  is no longer relied on, and `Solve()` no longer re-sorts the field, so a second solve of one instance
  matches the first.
- **The 10 s wall clock is GONE.** Replaced by a deterministic work budget — `Solver2D.WorkBudget`,
  default `DefaultWorkBudget = 500000` geometric comparisons, with `Solver2D.WorkUnits` exposing the last
  solve's cost. Calibrated by measurement, not guessed: healthy 5 000 space labels = **9 900** units /
  0.4 s; 2 000 = 3 960 (linear); 400 labels collapsed onto one anchor at `IterationCount` 100 = **620 263**
  units / 14.6 s. So it sits >10× above the healthy case and bites into the degenerate one at roughly the
  point in time the stopwatch used to. Time per unit is not constant, so the equivalence is approximate by
  construction — that is the price of a machine-independent layout.
- `LimitArea` semantics **unchanged**, documented at length instead: centroid-only, `Inside` not
  `InRange`, and why all three consumers need the loose meaning.
- 15 tests in `SAM/SAM.Tests/Solver2DTests.cs`, including a Mollier-shaped case (point labels at default
  priority + circle obstacles + polyline-anchored curve labels at priorities 2–4) — the only cover the
  `Polyline2D` branch has, since Mollier's own adapter sits above OxyPlot.

**Mollier behaviour difference observed, and it is real.** Mollier leaves `Priority` at `int.MinValue`
for every point/process/zone label and sets 1–6 for curve labels, so it has large equal-priority groups.
Above 16 items `List.Sort` was reordering them, so those labels **were** nondeterministic between
redraws; they now follow chart-series order. Positions may therefore differ from before — consistently.
Charts with ≤16 in every tie group are unaffected (insertion sort is stable in practice).
Also noted, not changed: `Solver2DDatas_CurveNames` iterates a `Dictionary`, which is deterministic for
identical input but is the kind of incidental order the adapter layer should not lean on.

### 5b. `PartFTagPlacement` adapter (`SAM.Analytical.UI`, user-interface-free)

- `PartFTagPriority` — the named policy: TRA(1) → KEX(2) → EX(3) → SUP(4) → net(5) → diagnostics(6),
  applied only in `PartFTagPlacement.Priority(mark)`. **An unresolved transfer keeps TRA**, asserted.
- `PartFTagPlacementItem` (anchor, size in **metres**, `LimitArea`, priority, key) →
  `PartFTagPlacementResult` (rectangle, `ResultType` passed straight through, `IsUserPositioned`,
  `IsOverlapPossible`, `Leader2D()`).
- Placement order inside the adapter is priority → annotation type → object guid → supplied index, so a
  set handed over through a `HashSet` still lays out identically. Tested.
- **Manual tags in as obstacles**, stale overrides ignored not pruned. `PartFAnnotationOverride.Position2D`
  is now documented as the label's **centre** (a corner would slide when the text changes).
- `Rectangle2D` on a result is **never null**: an `Unplaced` Part F tag is drawn at its anchor and flagged,
  because a rate that vanishes off a compliance drawing is worse than one that overlaps. The floor-plan
  space labels take the opposite decision on the same engine (they blank the text) — that is defensible
  for a room name and not for a rate.
- Leader built at the consumer, `Segment2D` from anchor → nearest point on the tag; the anchor is cloned
  so a renderer cannot move a terminal by drawing a line. `Point2D` is mutable — tested.
- New `PartFOverlayMark.AnnotationGuid` / `.AnnotationType`: the **terminal**'s guid for a terminal mark
  and the **route**'s for a transfer mark (not the aperture — the common case has no modelled door).
- Window: the temporary `Place(Rect, List<Rect>)` pixel-nudging loop and `SpaceNameRects` are **deleted**;
  a reflection test asserts the window declares exactly one `Place`, parameterless. Solve runs on
  dwelling/level/mode/toggle/scale change only — `FloorPlan_ViewChanged` re-solves on a **scale** change
  and a pan merely re-transforms; `loading_FloorPlan` suppresses the solve while the plan is rebuilt.
- Space-name obstacles are now **read from the loaded `GeometryObjectModel`'s `Text3DObject` positions**
  rather than assumed to sit at the space anchor — those labels are themselves solved and can be metres
  away. Only the band's size is still assumed (120×20 px).
- 15 tests in `SAM_UI/WPF/SAM.Analytical.UI.WPF.Tests/PartFTagPlacementTests.cs`, including one end-to-end
  on a real three-room flat with the shipped rule set.
- Also fixed, enabled by the enum: `GeometryObjectModel`'s `FloorPlan.LabelSolver.Placement` diagnostic
  counted a budget fallback as "placed". It now reports solved / over budget / unplaced and work units.

### 5c. Annotation scale, NOT viewport zoom (reviewed and required)

Michal rejected an earlier revision where a zoom re-solved the layout: *"I do not want ordinary navigation
to keep changing an engineering drawing's annotation layout."* The distinction is now explicit.

- `PartFAirflowViewSettings.AnnotationScale` is a **real drawing scale denominator** (1:50 by default), saved
  with the view, machine-independent. It replaced a never-consumed text-size multiplier of the same name.
- `PartFTagPlacement.PixelsPerMetre(scale)` = 96 / 0.0254 / scale is the ONLY conversion from measured text
  to plane units. The viewport transform appears nowhere in the placement path.
- Pan **and zoom** transform and redraw only. Re-solve on: model/dwelling/level, visible annotation types,
  dwelling filter, operating mode, manual position, `AnnotationScale`, reset/auto-arrange.
- Tags are DRAWN at the size the annotation scale implies at the current zoom (text scales with the view), so
  the drawn box always equals the solved box. Fixing the text at its measured pixel size would let tags
  visibly collide at every zoom except one. Consequence to accept: zoomed far out, Part F text is small.
- Proved structurally: `Place_NeverReadsTheViewTransform` walks the compiled IL of `Place()` and everything it
  reaches, with a **positive control** (it must find `PartFTagPlacement.Solve` and `FloorPlan2DControl.Plane`)
  so it cannot pass vacuously, then asserts `WorldToScreen` is unreachable. Plus
  `Place_IsCalledOnlyByTheAgreedInputChanges` locks the caller set to exactly five methods.

### 5d. Stable annotation identity (reviewed and required)

`PartFVentilationTerminalRequirement` and `PartFDoorTransferData` are `SAMObject`s the calculator builds with
`Guid.NewGuid()`, so **their own guids last exactly one calculation**. Keyed on those — which is what handover
decision 7 originally said — every label an engineer had tidied would jump back on the next recalculation.
`RouteAndTerminalOwnGuids_AreNotStableAcrossRecalculation` documents it.

`PartFAnnotationKey` derives the key from persistent model identities (RFC-4122-style name-based guid, SHA-256,
version 8): terminal = space + role; transfer = **aperture** where the model has one, else the two space guids
**canonically ordered** so the key survives the route being reported the other way round. Nothing is persisted
— the key is recomputed. `ManualTransferPosition_SurvivesSaveReopenAndRecalculation` runs the real calculator
twice over one model, for a door-backed route AND a partition-only route.

**This supersedes decision 7 below**: the key is derived, not the terminal's own guid.

### 5e. Tag language (reviewed 2026-08-07 from the screenshot)

Michal asked for Revit-style tags rather than airflow markers, and the screenshot showed why.

- A terminal is a **tag and nothing else** — the directional arrow is gone. SAM knows the room requires
  extract or supply; it does not know where the grille is, so an arrow at a point SAM chose itself asserted a
  direction of air movement at an unestablished location.
- White (`0xF2`) fill, neutral grey hairline border, 3 px padding, text in the air type's colour, no `▶`.
  Reads "SUP 63.0 l/s ✓".
- **No leader for a terminal tag**, automatic or manual: nothing real to lead to. `HasPhysicalAnchor(mark)` is
  the single predicate suppressing it, so a future terminal object with a coordinate gets its leader back. A
  transfer tag DOES get one — a door or partition is a real location.
- The tag is drawn unconditionally now; "Values" chooses between `SUP 63.0 l/s` and `SUP`. It used to be gated
  behind the value toggles, which was safe only while an arrow remained underneath.
- Room names and door labels are protected obstacles, from the **measured** bounds of the text the plan drew
  (`TextObstacle2Ds`, using the plan's own `Query.Width` and `TextAppearance.Height`), not a 120×20 px band
  around the space anchor as before — those labels are themselves solved and can sit metres from the anchor.
- Fixed: `TRA 63.0 l/s ? ?` — the route's own trailing `?` and the cannot-be-determined symbol were both
  appended.
- Checkbox labels are now "Terminal tags" / "Transfer routes".

**Screenshot still owed** (blocked on Michal running the app) before the main 2D View work starts.

### 5f. Known limits, deliberately left

- Tag sizes are fixed on screen, so a zoom re-solves. At a very zoomed-out scale a tag is large in plane
  units, may not fit its room, and comes back `Unplaced` → drawn at its anchor. That is CAD behaviour and
  it is reported, not hidden.
- `PartFTagPlacement` is a pure function and the window owns the "when"; there is no cache keyed on a
  signature yet. Add one if a large plan shows a lag on a zoom — `GeometryObjectModel.labelSolveCache` is
  the precedent.
- `Overrides()` in the window returns an empty list until the drag-to-move UI (section 6) exists. The
  obstacle path is built and tested, so it works the day the UI writes an override.

## 5g. Superseded — original brief for task B

**There is nothing to extract.** The shared engine already exists at
`SAM/SAM/SAM.Geometry/Geometry/Planar/Classes/Solver2D/`. SAM_Mollier is a **consumer**
(`SAM.Core.Mollier.UI.WPF/Create/Solver2DData.cs`, `Modify/AddLabels.cs`). Do **not** create
`AnnotationPlacementEngine`.

Michal's `Solver2D-review.md` was verified line by line against the code — **every claim accurate**:

- `solver2DDatas.Sort(...)` on `Priority` is **unstable** → equal-priority tags reorder between
  redraws. A saved drawing must redraw identically. Fix: priority, then insertion order.
- A 10 s wall-clock budget silently places remaining tags **at their anchor, overlapping**, and the
  caller cannot tell. Fix: `Solver2DResultType { Solved, Fallback, Unplaced }` on `Solver2DResult`.
- `LimitArea` tests the **centroid** only. Keep that (option A) for floor-plan tags — a small ensuite
  cannot contain the whole text box.
- **Latent bug**: `solver2DResults.Find(x => x.Closed2D<Rectangle2D>().InRange(...))` has no null
  guard; an earlier unplaceable item throws. Only on the **≤256-item non-grid path** — i.e. exactly
  Mollier charts and small floor plans.
- **Latent bug**: `obstacles2D.Find(...)` assumes the list is non-null.

**Do NOT add `LeaderLine` to `Solver2DResult`** — this supersedes an earlier proposal of mine that was
wrong. A leader is `anchor + solved rectangle → nearest point → Segment2D`, derivable at the consumer.

**Manual tags become obstacles, not omissions.** Mollier only *excludes* a user-positioned label from
the solve, so an automatic tag can land on top of one somebody placed deliberately:

```
automatic tag   -> Solver2DData
manual tag      -> obstacle
model geometry  -> obstacle
```

`IsUserPositioned` stays in the view layer (`PartFAnnotationOverride`), never in `SAM.Geometry`.

**Order of work** — steps 1–2 touch shared `SAM.Geometry` used by Mollier, so do them as their own
small change with the Mollier chart re-verified BEFORE the Part F adapter, so a regression is
attributable:

1. Harden the two null paths in `Solver2D`.
2. Add `Solver2DResultType`; make ordering stable. Additive only; Mollier regressions must stay green.
3. Thin `PartFTagPlacement` adapter in `SAM.Analytical.UI`: `Solver2DData` per tag with priority
   (TRA → KEX → EX → SUP → net → diagnostics), manual overrides + space outlines as obstacles, leader
   built at the consumer.
4. **Delete the temporary `Place(...)` loop** in `PartFAssessmentWindow.FloorPlan.cs`.
5. Tests: automatic separation, a manual tag never overlapped by an automatic one, deterministic
   repeat solve, `Fallback` surfaced.

Caching: `Solver2D` must NOT run on every pan/zoom `ViewChanged`. Solve on model/mode/filter/toggle/
scale/manual-position change and on auto-arrange; pan and zoom only re-transform and redraw.

## 6. DONE: Part F in the NORMAL saved 2D View

SAM_UI `efaed83`, on the remote.

- **One renderer.** Everything the drawing consists of is now `PartFAirflowRenderer`
  (`WPF/SAM.Analytical.UI.WPF/Controls/`), used by the assessment window AND the saved view, driven entirely
  by `PartFAirflowViewSettings`. `PartFAssessmentWindow` has **no placement and no drawing code at all** — a
  test asserts it declares neither `Place` nor `DrawMarks`.
- The structural zoom tests now target the renderer, so they cover both consumers: callers of
  `PartFAirflowRenderer.Place` are pinned to `{Load, Refresh, set_ViewSettings}`.
- `ViewportControl.FloorPlan2D` exposes the 2D plan (null in 3D or on the legacy orthographic path).
- `AnalyticalWindow.PartF.cs` attaches a renderer per view tab from the view's `PartFAirflow` parameter, and
  re-reads the assessment through the same `PartFCalculator` the command uses, cached per model instance +
  scope. A view with no parameter is left without one — never given a disabled setting. ~~The window holds
  the cache~~ — **superseded by 6b**: the cache and the scope gate are `PartFAssessmentCache`.
- `PartFAirflowViewSettings.ZoneCategoryName` scopes which dwellings a drawing reports on, so reopening
  reproduces the assessment instead of asking again. ~~Empty means the whole model is one dwelling~~ —
  **superseded by 6b**: empty is UNDECIDED, and whole-model is `PartFDwellingScope.WholeModel`.
- **Part F Airflow dialog** on the 2D view settings (`PartFAirflowViewSettingsWindow`) — how it is turned on.
- **Whole-floor isolation is now tested** (`PartFWholeFloorTests`, 6 tests) on two flats separated by a
  communal corridor with every partition real and shared: every dwelling assessed, no TRA between dwellings,
  every mark inside its own dwelling, the corridor annotated with nothing, the floor exactly the sum of its
  dwellings with distinct annotation keys, and no tag overlap ACROSS dwellings (one solve, separate
  assessments).

### 6b. New-view preset (SAM `b0e72a21` + `c5ca006e`, SAM_UI `150ea63`)

Choosing `Color Scheme → Element → PartF Data` on a **new** view now initialises a usable Part F drawing:
annotation on, all layers, 1:50, continuous design, all dwellings on the level. Before this, the obvious way
to ask for a Part F drawing produced coloured fills and no airflow, with no hint that nine more options sat
behind another dialog.

- **The dwelling category is resolved, never hard-coded.** `AdjacencyCluster.PartFDwellingZoneCategories()`
  (SAM). One category ⇒ auto-selected. Several ⇒ **left unset for the user** (which flats a drawing reports on
  is an engineering decision). None ⇒ whole-house mode.
- **The dwelling-selection policy exists exactly ONCE**, in `Query.PartFDwellingZones(IEnumerable<Zone>)`:
  where no zone carries `IsDwelling` every zone counts (the legacy behaviour, because the parameter postdates
  the models); otherwise only an explicit `true` does. `PartFCalculator.SelectDwellingZones` now **decides**
  through that rule and keeps only its own reporting, taking its warning/remark lists from
  `Query.PartFClassifyDwellingZones`. Its warning texts are byte-identical and all 1046 SAM tests pass
  unchanged. **There is no duplicated rule and no technical debt here** - an earlier revision restated the
  policy in the categories query, and Michal was right to object.
  `DwellingCategories_AgreeWithTheCalculator` is kept as the end-to-end integration lock.
- **The four scope cases are now distinguished (review correction, 2026-08-07).** A null `ZoneCategoryName`
  used to mean three different things, and `PartFCalculator.Calculate(string)` reads null as whole-model
  single-house mode - so a new view of a block with several possible categories was enabled, unscoped, and
  drew the whole building as ONE dwelling while waiting to be told which flats it was about. Fixed at the
  **view/preset scope only; the calculator is untouched.**
  - New `PartFDwellingScope { Undefined, WholeModel, ZoneCategory }` on `PartFAirflowViewSettings`, and the
    single predicate `HasDwellingScope`. **A blank category is no longer a decision.**
  - **`Enabled` is intent; `HasDwellingScope` is the safety mechanism.** `Enabled` says this view wants Part
    F annotation; `HasDwellingScope` says whether SAM knows enough to calculate any. The preset therefore
    leaves the annotation **ON in every case** and only the scope undecided, so selecting the dwellings is
    the single remaining action and the drawing appears without a second trip back to re-enable anything.
    An earlier revision switched it off instead; Michal was right that the presentation toggle is the wrong
    place for a correctness gate. `ScopeSelection_IsTheOnlyRemainingAction` pins the behaviour.
  - Preset scope: one category ⇒ that category; no zones at all ⇒ `WholeModel`; several categories ⇒
    `Undefined`; zones but no dwelling among them ⇒ `Undefined` (**never** a silent fall back to
    whole-house - that would report a block as one dwelling because `IsDwelling` was never set).
  - **`PartFAssessmentCache`** (new, in `SAM.Analytical.UI`) is the extracted saved-view assessment AND the
    gate: an undecided scope is not calculated at all, and `null` reaches the calculator only from an
    explicit `WholeModel`. `AnalyticalWindow.PartF.cs` now holds one of these instead of three fields.
  - Dialog: the editable category combo became an explicit list - "Not chosen", "Whole model as one
    dwelling", then each **dwelling** category the model actually holds - with a message saying why nothing
    is drawn. The button reads `Part F Airflow: choose the dwellings...`.
  - A view saved before the scope existed is read from its category: named ⇒ `ZoneCategory`, blank ⇒
    `Undefined`. The safe direction.
- **`IsNewViewSettings`** on `AnalyticalTwoDimensionalViewSettingsControl` is the only gate. Set by the two
  creation paths only — `AnalyticalWindow`'s New Section View and `BatchCreateViewsControl`.
- Never applied to an existing view, never re-applied where settings exist, never on duplicate, never resets a
  manual position. Four tests, one per promise. The preset does NOT switch the annotation off again if the
  colour scheme is later changed — the two are independent by design.
- 17 tests in `PartFAirflowPresetTests` (one per scope case, both round trips, and
  `TwoCategories_NeverProduceAWholeModelAssessment` driving the real cache), plus
  `PartFPlanModel.ZoneWithoutIsDwelling` for the legacy case.

### 6c. The saved-view cache invariant, proved (review correction, 2026-08-07)

`PartFAssessmentCache` is the one place an engineering value is held between draws, so the claim "an edit
cannot be answered from a stale assessment" is now a test rather than a comment. **No revision framework was
added** — the invariant was already there and only needed asserting:

- `UIAnalyticalModel_HandsOutANewModelOnEveryRead` — `UIJSAMObject.JSAMObject` deep-clones on every read, so
  an instance key cannot survive an edit. One line, and it is the whole argument.
- `ModelEdit_CannotReuseTheCachedAssessment` — end to end through the real `UIAnalyticalModel`, cache and
  calculator: two dwellings assessed and the cache proved WARM, a dwelling zone removed, one dwelling after.
- `SameModelAndScope_IsAnsweredFromTheCache` first, so none of the above passes vacuously.
- Verified by mutation: reverting the gate and re-keying the cache on `AnalyticalModel.Guid` (which the clone
  preserves) fails 5 of these tests. Keying on the guid is the tempting wrong answer — it is now covered.
- 6 tests in `PartFAssessmentCacheTests`.

### 6a. Backlog, explicitly NOT to be started yet

Michal's optional productivity command, after the 2D view work settles:
`Create proposed Part F transfer door...` — a **user-invoked** model edit, never something the assessment
creates silently. For a simple unique shared partition it would propose a 760 × 2100 mm internal door,
`SIM_INT_SLD`, a 10 mm finished-floor undercut and 7 600 mm² transfer free area, with a preview and
confirmation before the model changes. **Creating the aperture must not by itself make paragraph 1.25 pass**:
transfer compliance comes from the recorded undercut/free area, so accepting the proposal has to record the
design provision explicitly.

## 7. NEXT: the remaining saved-view work (tasks 15–20)

**This is the real target, and Michal has said so twice.** The assessment window's Airflow tab is for checking
and debugging Part F and must not become the final drawing interface. The goal is:

> normal saved 2D Section/Floor Plan View + `PartF Data` colour scheme + optional Part F annotation overlay

so that a view called e.g. `Level 0 [0m] Part F` can be created and reopened with its Part F presentation
intact. Everything in section 5 was built to be reused there unchanged: `PartFFloorPlanOverlay` and
`PartFTagPlacement` are user-interface-free, and the shared control already carries the overlay hook.

Part F Airflow section in View Settings (separate from the existing `PartF Data` colour scheme, which
is a `ValueAppearanceSettings` and stays untouched); whole-floor rendering as N independent overlays,
one per dwelling result; drag-to-move labels with leaders and reset/auto-arrange; door properties in
the normal property workflow; plan selection resolving to the underlying object.

**Whole-floor isolation needs an explicit regression test** — Michal does not want it resting on the
per-dwelling architecture as an inference: all dwellings render, no TRA between dwelling results,
communal corridor gets nothing, filtering one dwelling removes the others' marks, and per-dwelling
rendering matches the combined render.

## 8. Screenshots still owed (blocked on Michal running the app)

Whole Level 0 with all flats; Flat 1 only; Flat 2 only; Continuous; Setback; colour scheme + overlay
together; before/after auto placement; one manually moved KEX tag with leader; view after
close/reopen proving persistence.

## 9. Environment gotchas

- `Panel.Apertures` returns **clones**; go through `AdjacencyCluster.SetPartFDoorTransferData(...)`.
- Inside `SAM.Analytical.UI.WPF`, unqualified `Window` binds to `SAM.Analytical.Window` — WPF windows
  must declare `: System.Windows.Window`.
- `CS1566 ... g.resources` → delete `WPF/SAM.Analytical.UI.WPF/obj` and rebuild.
- After a compile error, `dotnet test` can run a **stale** test assembly. Use
  `dotnet build --no-incremental` then `dotnet test --no-build` when a fix "does nothing".
- Use `System.Math.*` — bare `Math` collides with the `SAM.Math` namespace.
- `SAM.Analytical` multi-targets below `Dictionary.TryAdd`; use `ContainsKey` + indexer.
- Heredocs through the Bash tool mangle backslashes — use the Edit tool for C# string literals
  containing `\r\n` or `\n`.
- The ADF PDF is read with `pdftotext -layout` (Git for Windows).

## 11. Part O / Iteration programme — Iteration 0 IN PROGRESS

The work has been reframed as iterations. **Only Iteration 0 is being built.**

| | |
|---|---|
| **Iteration 0** | Foundation: dwelling scope → Part F → system selection → scenario → TM59 → result association. **CURRENT** |
| Iteration 1 | Base passive / unrestricted openings, MVRE at Part F continuous. TSD route. NOT STARTED |
| Iteration 2 | Acoustic restriction + boost + summer bypass. TSD route. NOT STARTED |
| Iteration 3 | CoolBreeze-class active trim cooling. Full `SystemEnergyCentre` → TAS HVAC → **TPD** route. NOT STARTED |

Iteration 0 must not make that routing impossible; it must not implement any of it.

### 11a. Architecture reconnaissance — what already exists (REUSE, do not rebuild)

The real TAS run proved execution and result association already work. **Do not build a new simulation
pipeline.**

- **Execution**: `WorkflowCalculator` / `WorkflowSettings` (SAM_Tas), driven from `Modify.Simulate`
  (SAM_UI) — the "Convert to Tas TBD" dialog. Works today.
- **Overheating engine**: `SAM.Analytical.TMOverheatingCalculator` (extracted this session — see 11c).
- **The TSD → TM59 recipe already exists**, in the Grasshopper component
  `TasTSDQueryTM59Results`: TSD → `Convert.ToSAM(path, TSDConversionSettings{ResultantTemperature,
  OccupantSensibleGain, ConvertZones})` → restore design `InternalCondition`s → **select spaces from a
  dwelling Zone via `GetRelatedObjects<Space>(zone)`** → `Calculate_TM59` → split Mechanical / Natural /
  Corridor. **Lift this, do not rewrite it.**
- **Results**: `Result` base + `Space/Zone/AdjacencyCluster/AnalyticalModelSimulationResult`, associated
  through `AdjacencyCluster` relations. The real run wrote 18 space + 8 zone + 440 surface results.
- **System templates**: `SAM_Systems/files/resources/Analytical/Systems/SystemEnergyCentre/*.json`
  (1–1.8 MB each: plant rooms, energy sources, ~100 `SystemType`s, 2D schematic), loaded by
  `Analytical.Systems.Query.SystemEnergyCentre(path)` / `DefaultSystemEnergyCentreDirectory()`.
- **`PartOOpeningProperties`** already exists, and the real model's 20 apertures already carry it.

### 11b. The vocabulary — settled, do not re-litigate

`SAM_SystemTypeLibrary.JSON`'s `VentilationSystemType` ids are **the same identifiers as the
SystemEnergyCentre template filenames**, and `SystemTemplate` (Ventilation/Heating/Cooling/PlantRoom/
Controls/Version) is the existing engine-neutral identity matching names like `NV 1:RAD 1:UC 1`.

```
NV    Natural ventilation
MV    Mechanical ventilation, NO heat recovery   (Air Supply Method = Outside)
MVRE  Mechanical ventilation WITH heat recovery  ← SAM's MVHR
UV    Unconditioned  → routes to TM59CorridorExtendedResult
```

**`MVRE` is MVHR — verified, not assumed.** Its template holds one air-side exchanger:
`SensibleEfficiency 0.7`, `LatentEfficiency 0`, and **no mixing or recirculation component anywhere**.
`MV.json` has no exchanger at all. So the difference between them is exactly heat recovery, and `RE`
behaves as REcovery despite the library description saying "Recirculation".
**Do NOT add an `MVHR` identity** — it would split an established concept. Summer bypass and boost are
**operating states of MVRE**, not new system types.

### 11c. DONE this session (all pushed)

| Repo | SHA | What |
|---|---|---|
| SAM | `060feeda` | `Query.PartOClassifyAssessmentZones` + `SimulationSpaceMap` + 16 tests |
| SAM | `9cbf308a` | `TMOverheatingCalculator` extraction + 5 tests |
| SAM | `2e2362f7` | doc terminology |
| SAM | `7faf964c` | **`OverheatingScenario` + derived key** (step 4) + 24 tests |
| SAM | `02e99582` | **step 4 hardened after independent review** → 35 tests |
| SAM | `884a2f54` | **step 4.1 canonicalisation** (Michal's review) → 40 tests |
| SAM | `b23bef3b` | **step 5a analytical capability selection** → 17 tests |
| SAM | `c5112e4f` | preference out of `SAM.Analytical`; stronger handover invariant |
| SAM | `b52aff65` | **`MechanicalSupply`** + selection hardening → 21 tests |
| SAM_Systems | `c6a441d` | **step 5b `CapabilityIndex.JSON`** + conformance test → 9 tests |
| SAM_Systems | `e99f311` | malformed-index refusal + structural no-template proof → 20 tests |
| SAM_Tas | `5e38c94` | `OverheatingCalculator` → compatibility wrapper + 3 equivalence tests |
| SAM_Tas | `d56f679` | doc terminology |
| SAM | `193968ff` | **step 7a** `TM59AssessmentCalculator.SourceFallback` — provenance survives the repoint |
| SAM_Tas | `f6e32b4` | **step 7a** `Create.TM59AssessmentCalculator` — TAS's series keys + provenance stay in SAM_Tas |
| SAM_Tas_Grasshopper | `047c583` | **step 7a** `Tas.TSDQueryTM59Results` repointed; the recipe now exists once |
| SAM | `f6772519` | **step 7b** `VentilationStrategyMap` + `VentilationStrategySelection` → 16 tests |
| SAM_Tas | `b03f02b` | **step 7c** scenario-authoritative TM59 XML export → 9 tests |
| SAM | `d2b0f971` | step 7 review fixes — closed vocabulary, refusal copies, falsifiable controls → 1173 |
| SAM_Tas | `121035c` | step 7 review fixes — no silently dropped rooms, null returns carry a reason → 42 |
| SAM | `d7abd48a` | **step 8** identity association through `SimulationSpaceMap` |
| SAM_Tas | `1aba5eec` | **step 8** TAS stable-key mapping and print-route scenario seam |
| SAM_Tas_Grasshopper | `f8ca646c` | **step 8** `Tas.TSDQueryTM59Results` stops matching by name |
| SAM | `f712fd9f` | step 8 review fixes — fail-safe mapping, scoped selection, transactional scenarios → 1183 |
| SAM_Tas | `7f3dfda` | step 8 — both TAS acceptance paths proved by identity → 46 |
| SAM_Tas_Grasshopper | `b8dae4b` | step 8 — scenarios wired into both existing TAS workflows |
| SAM | `cc5e67f0` | step 8 further review — repeated scenario is not a collision, proof 10 made behavioural → 1185 |
| SAM_Tas | `7f3dfda` | step 8 review acceptance coverage for both TAS production paths → 46 |
| SAM_Tas_Grasshopper | `b8dae4b` | step 8 review wiring in both existing Grasshopper workflows |

- **Part O scope** (`PartOClassifyAssessmentZones`): dwellings vs common space, dwelling half
  **delegated to `Query.PartFDwellingZones`** (single source of truth). The corridor is *returned* as
  common space, never dropped, never attributed to a dwelling.
- **`SimulationSpaceMap`**: resolves a simulation-derived space back to the design space. Stable
  engine key first, unique name as fallback, **refuses on ambiguity**. The key is a
  `Func<Space,string>` so `SAM.Analytical` stays engine-free. Verified on the real run: **9 spaces → 9
  distinct TAS zone guids**, matching the DomOv XML — unique per SPACE, not per zone.
- **`OverheatingScenario` (step 4, DONE)**: a Part O assessment stated as engineering intent - scope,
  design zone guid, iteration, the existing `SystemTemplate` identity, and an
  `OverheatingOperatingAssumptions` bag - with a **derived** key. SHA-256 over a namespace and the
  components, first 16 bytes stamped version 8 + RFC 4122 variant, mirroring `PartFAnnotationKey`.
  UTF-8, **length-prefixed** (concatenation is ambiguous), NFC-normalised, enums hashed **by name**,
  behind the marker `OverheatingScenario:v1`. **Not** `Core.Query.ComputeHash` - it is ASCII and
  collides. Not in the key: name, `Source`, anything engine-shaped, simulation output, view settings,
  creation time. `Key` is `Guid.Empty` where `IsValid` is false, held after first derivation and
  dropped by `FromJsonObject` - the only path that writes identity-defining state.
  `Key_IsStableAcrossBuilds` pins the exact guid; regenerate it **only** with a schema bump.
  - **No `Iteration0` member.** `PartOIteration { Undefined, BasePassive, AcousticRestricted,
    ActiveTrimCooling }` - the foundation stage is a stage of this codebase, not an operating scenario
    of a building, and a scenario built during it states `Undefined`, which is true.
  - `VentilationStrategy` / `HasVentilationStrategy` read the existing `SystemTemplate.Ventilation`
    (`NV`/`MV`/`MVRE`/`UV`). **No second vocabulary, no `MVHR`.** Step 7's consumer must **refuse**
    where `HasVentilationStrategy` is false, never fall back to the zone-name lookup it replaces.
  - **Canonicalisation is at the boundary, not at the hash** (step 4.1). An assumption **name** is
    NFC-normalised *before* it enters the ordinal `SortedDictionary`, because the assumptions are hashed
    in name order and ordinal order runs over raw code units — composed `é` (U+00E9) sorts **after**
    `f`, the decomposed form sorts **before** it. Normalising only inside `Append` left canonically
    identical assumptions hashed in different **orders**. Proved by mutation.
  - **One canonicaliser per type, whichever door a value came through.** A JSON primitive goes through
    the same path as the typed setter that would have written it: `{"SummerBypass": false}` stores
    `False`, not `false`; a JSON number goes through `Text(double)`. A JSON **string** stays text (so
    `"21.0"` is not `21`). An object or array is **refused**, not flattened — there is no canonical form
    for arbitrary JSON and property order alone would decide the key.
  - An **invalid** scenario is equal only to itself. Its key is empty, and treating "no identity" as a
    shared identity collapsed every half-filled scenario in a user interface into one `HashSet` entry.
- **TM overheating extraction**: `SAM.Analytical.TMOverheatingCalculator` owns TM52 + TM59 + comfort
  helpers; `SAM.Analytical.Tas.OverheatingCalculator` is a delegating wrapper with its public API
  intact (no Grasshopper/UI migration). The two series keys are **instance** properties
  (`ResultantTemperatureSeriesKey`, `OccupancySensibleGainSeriesKey`) defaulted to the analytical
  vocabulary; the wrapper supplies TAS's. Equivalence tests run under `dotnet test` **without TAS COM**.

### 11d. ~~THE OPEN DEFECT~~ — CLOSED in step 7. Ventilation strategy had THREE conflicting derivations

**Fixed in step 7 (11l).** Where a `VentilationStrategyMap` is supplied, the scenario states the strategy and
**all three derivations below are bypassed**; where nothing states it, the space is **refused**. All three
remain reachable for callers that supply no scenario, documented as **superseded**, and derivation #3's
`"NV"` default is the one that made an MVRE dwelling assess as naturally ventilated. Kept below because it
is the record of what the defect was and which code still contains it.



The real run exposed `Nat Vent` / `Mech Vent` mixed across three flats of one building. Root cause:

| # | Where | Decides by | Feeds |
|---|---|---|---|
| 1 | `Convert/ToTM59/Zone.cs:27` | space's `InternalCondition.VentilationSystemTypeName` | TM59 XML |
| 2 | `Convert/ToTM59/Building.cs:26` | any related `VentilationSystem` that `IsMechanicalVentilation()`; overrides #1 | TM59 XML |
| 3 | `TMOverheatingCalculator.SystemTypeName` | internal condition → **else the zone's NAME looked up in `DefaultSystemTypeLibrary()`** → else `"NV"` | **the engineering result** |

**#3 picks the TM59 criterion**, and its middle step is a string match of a zone name (`"Flat 1"`)
against a library — which misses, defaulting an MVHR dwelling to the natural-ventilation criterion.
`Space.ToTM59` already accepts a `systemType` override, so the seam exists. **The scenario must become
authoritative.** All three were ported verbatim and commented; fixing them is Step 7.

Also found: **`Zone Category` does nothing for Domestic Overheating.** In `Modify.Simulate` it is
consumed only by the `createSAP` branch, so the TM59 export covers the whole model — which is why the
communal corridor appeared in the run's DomOv XML as an ordinary room.

### 11e. Real TAS validation assets (on this machine)

- TAS **9.5.7.0** at `C:\Program Files\Environmental Design Solutions Ltd\Tas`.
- Michal's run output: `<user>\Documents\SAM_daily\2027-08-03-HVAC\` —
  `000000_SAM_AnalyticalModel.{tbd,tsd,json}`, `.timing.csv` (78 s total), and
  `Report XMLs\…DomOv.xml`.
- Model: `SAM_zoningAM_v2zonesisDomestic.sam` — **Zone Category `Flats` → Flat 1 / Flat 2 / Flat 3
  (`IsDwelling` true) + Corridor (false)**, 9 spaces. The canonical Iteration 0 fixture shape.
- Weather: DSY from `C:\Users\Public\Documents\Tas Data\Databases\CIBSE Weather 2021.twd`, readable via
  `SAM.Weather.Tas.Convert.ToSAM_WeatherDatas(path)`.
- **The post-run model carries NO `TM59*Result`** — the overheating output is only the DomOv XML, which
  is *configuration for the external TAS TM59 tool*, not an assessment. That gap is Iteration 0's job.

### 11f. Follow-ups deliberately NOT fixed (each needs its own change + regression)

1. **`"Occupant Sensible Gain"` (TAS) vs `"Occupancy Sensible Gain"` (analytical)** — same quantity,
   two spellings. Reading the wrong one is silent. Do not add a second enum member; do not migrate data.
2. **`MVRE` description** says "with Recirculation" but the template is heat recovery. Descriptions
   participate in lookups, so this is behavioural.
3. **`MVRE` `Air Supply Method = Total`** vs `MV`'s `Outside` — verify before Iteration 2 relies on it.
4. **No weather data + valid series → `NullReferenceException`**, thrown inside
   `SAM.Weather.Query.RunningMeanDryBulbTemperatures`. Characterized by
   `NoWeatherData_ThrowsToday_PreExistingBehaviourNotAContract`. The fragility is in the weather layer.
5. **Missing series → silent no-assessment**, pinned not endorsed. Needs a diagnostic.
6. **`Core.Query.ComputeHash` uses `Encoding.ASCII`** — non-ASCII names collide. Fine as a checksum,
   **unsafe for derived identity**. Use UTF-8 for the scenario key. *(Done for the scenario key in
   `7faf964c`; `ComputeHash` itself is untouched and still ASCII.)*
7. **`SystemTemplate`'s setters strip spaces; its copy and JSON constructors do not.** `"MV RE"` means
   `MVRE` through `new SystemTemplate(...)` and `MV RE` through `FromJsonObject`. `OverheatingScenario`
   normalises at its own boundary (`Normalized`), and `SAM_Systems`' capability-index reader rebuilds
   identities through the setters for the same reason - so both places that matter are safe, but the
   shared serialisation path is still inconsistent for every other consumer. Fixing it means routing
   `SystemTemplate.FromJsonObject` through the setters, which changes a path `SAM_Systems` lookups use
   — **needs its own change and regression**, not a drive-by.
8. **`"R"` / `"G17"` are not runtime-stable** — both changed meaning in .NET Core 3.0, so a double
   formatted for identity differs between a .NET Framework host (Revit, gbXML) and .NET 8.
   `OverheatingOperatingAssumptions.Text(double)` is fixed to nine decimal places for that reason. Any
   *other* derived identity in SAM that formats a double has the same exposure.

### 11g. NEXT — Iteration 0 remaining steps, in this order

4. ~~**`OverheatingScenario`**~~ — **DONE**, `7faf964c` + `02e99582`. See 11c.
5. **DONE — 5a, 5b, 5c.** 5a in `SAM.Analytical` (11h), 5b the `SAM_Systems` catalogue (11i), 5c
   eligibility and CI closure (11j). Concrete template selection is enabled: a Part F assessment resolves
   to a real `SystemEnergyCentre`, and a commercial template is never offered for a dwelling.
6. ~~Lift the `TasTSDQueryTM59Results` recipe into a testable service.~~ **DONE** (11k), and **fully
   closed in 7a** — the component now calls it, so the recipe exists once.
7. ~~**Make the scenario authoritative** over ventilation strategy (11d).~~ **DONE and reviewed** — see
   **11l**. 11d is closed.
8. ~~Result association to design dwelling / common space **by identity, not name**.~~ **DONE and
   independently reviewed** — see **11n**. The mandatory three-flat duplicate-`Bedroom 2` regression covers
   both existing TAS production paths.
9. ~~Preserve the TSD-simple vs TPD-full routing boundary.~~ **DONE — see 11o.** The two-pass route is named,
   its refusals are explicit, mechanism B's preparation moved into `SAM_Tas` and its component is thin. **The
   intended supply-temperature/airflow transfer proved impossible in the TBD object model and is refused
   rather than approximated — 11o records the evidence.** Michal's three decisions in 11m are resolved.
10. Thin headless TAS runner, **last**.

**Iteration 1 has started ahead of step 10, on Michal's instruction** — BasePassive is stated and assessable
(11p). Its remaining half is applying a stage to the **simulation inputs**, which 11p deliberately does not do.

### 11h. Step 5a — system capability selection, analytical half (DONE, `b23bef3b` + `c5112e4f` + `b52aff65`)

**The boundary, decided by Michal and replacing an earlier recommendation of mine.** A declared catalog
in `SAM.Analytical` would have put the names of ten `SAM_Systems` resource files inside the core
library. The split is instead the one already drawn for TAS:

```
SAM.Analytical                              │  SAM_Systems  (11i)
  SystemCapability (vocabulary)             │    CapabilityIndex.JSON beside the templates
  PartFSystemCapabilityRequirement (rule)   │    capability VALUES + Rank for the ten templates
  CapableSystems (suitability)              │    chosen SystemTemplate -> concrete SystemEnergyCentre
  SelectPreferredCapableSystem (choice)     │    conformance test opens the real 1.8 MB files
  — owns NO values and NO preference        │
```

- **`SystemCapability`** — `ContinuousVentilation`, `MechanicalSupply`, `Boost`, `SummerBypass`,
  `HeatRecovery`. Capabilities, **not equipment**. `MVRE` stays the heat-recovery identity.
- **`MechanicalSupply` was added after a review found a real misselection.** A balanced dwelling —
  paragraph 1.67, a supply terminal in every habitable room — was met by `Local Extract Only`, because
  extract-only does run continuously and can boost. The overheating simulation would have run a system
  with no supply and no heat recovery against a building that has both.
- **`Query.PartFSystemCapabilityRequirement(PartFDwellingResult)`** — continuous ventilation; mechanical
  supply when `TotalSupply_Lps`/`TotalHighSupply_Lps` is non-zero (those terminals carry
  `IsInBalancedFlow`); boost when `TotalHighExtract_Lps` exceeds `ContinuousDesignSystemRate_Lps`
  (intermittent extract excluded — it is not part of the balanced system). **It never asks for summer
  bypass or heat recovery**, swept over a grid of dwellings.
- **`Query.CapableSystems`** — suitability only, which is all this assembly can honestly decide.
  Returned in the supplied order so a caller with a different policy has the whole suitable set.
- **`Query.SelectPreferredCapableSystem`** — takes the first. **Preference is
  `SystemCapabilityDescriptor.Rank`, supplied by the catalogue and never derived.** An earlier revision
  ranked by fewest capabilities; Michal rejected that as a policy about a particular library rather than
  something following from Part F. Ranks are compared with `CompareTo`, not subtracted (overflow put the
  lowest-ranked system last). The sort breaks a final tie on insertion order, which is the only thing
  that makes sorting an index list less unstable than sorting the list.
- **Refusals**: nothing capable ⇒ names what was missing; every capability present but never together on
  one system ⇒ says that instead; two **different** suitable systems at the lowest rank ⇒ refused, the
  catalogue has not said which is preferred; one system listed **twice** ⇒ not ambiguous, selected.
- **`SystemCapabilityDescriptor` is not an `IJSAMObject`** — it had a second JSON shape incompatible with
  the catalogue's, and its own reader turned a real index entry into a confident empty descriptor.
- Two structural locks, both mutation-verified: no member of the selection types names a file, path,
  directory or energy centre; no static member of `SAM.Analytical` hands out descriptors.
- 21 tests in `SAM.Tests/SystemCapabilitySelectionTests.cs`. The descriptors there are a **test fixture
  standing in for the `SAM_Systems` catalogue, not a copy of it**.

### 11i. Step 5b — the SAM_Systems capability catalogue (DONE, `c6a441d` + `e99f311`)

`files/resources/Analytical/Systems/SystemEnergyCentre/CapabilityIndex.JSON`, beside the ten shipped
templates. Read at runtime; **the templates are not**. Keyed on the existing `SystemTemplate` identity —
ventilation part only, because these are ventilation templates and heating/cooling/plant/controls are
chosen elsewhere. No new system or equipment identity, no `MVHR`.

| | `CV` | `Supply` | `Boost` | `Bypass` | `HR` | Rank | Application |
|---|---|---|---|---|---|---|---|
| `NV` Natural Ventilation | yes | | | | | 10 | Domestic |
| `EOL` Local Extract Only | yes | | yes | | | 20 | Domestic |
| `EOC` Central Extract Only | yes | | yes | | | 30 | Domestic |
| `MV` Supply And Extract | yes | yes | yes | | | 40 | Domestic |
| `MVRE` — SAM's MVHR | yes | yes | yes | | yes | 50 | Domestic |
| `CAV` Constant Air Volume | yes | yes | | | yes | 60 | Commercial |
| `VAV` Variable Air Volume | yes | yes | yes | | yes | 70 | Commercial |
| `DISP` Displacement | yes | yes | | | yes | 80 | Commercial |
| `UV` Unconditioned | | | | | | 90 | Any |

`Plantroom-Only.json` is listed under `NonVentilationResources` — not a candidate, recorded so every
shipped resource is accounted for.

- **`Query.SystemCapabilityDescriptors(directory)`** reads the index and nothing else. **A malformed
  index refuses entirely**: a missing or mistyped `Rank`, or a ventilation identity that is present but
  unusable. The `Rank` case is why — the review changed `"Rank"` to `"rank"` on VAV and every dwelling
  requirement then silently selected a commercial VAV unit in place of a dwelling extract fan, because
  a defaulted 0 sorts first. An **absent** ventilation key still means "not a ventilation template".
- Identities are rebuilt through `SystemTemplate`'s property setters, so an index entry reading `"M V"`
  matches a caller's constructor-built `"MV"` (11f item 7).
- **`Query.SystemEnergyCentreResource`** matches on the ventilation identity and refuses on ambiguity, a
  blank resource, or any resource containing a separator or `..`.
- **`Query.SystemEnergyCentre(SystemTemplate)`** is **the one point at which a template is opened at
  runtime**, once, after the choice, for the system chosen.
- **What the conformance test can and cannot prove.** `HeatRecovery` is checked against every shipped
  file — an exchanger with a sensible efficiency **above zero** — and re-establishes the MVRE fact every
  Part O heat-recovery decision rests on: one exchanger, 0.7 sensible, 0 latent, no recirculation, and
  `MV` is the same system without it. `SummerBypass` is checked as **absent from all ten** (ignoring
  `BypassFactor`, a cooling-coil parameter that false-positives a naive search).
  `ContinuousVentilation`, `MechanicalSupply` and `Boost` **cannot be read off a template at all** and
  are declared from the system type's meaning; each entry's `EvidenceFromTemplate` says which values are
  evidence, and a test asserts it says so honestly rather than presenting declaration as measurement.
- **"No template is opened" is proved structurally**, not by a stopwatch: the index is copied alone into
  an empty directory and every requirement Part F can produce still gives the same answer — and
  resolution is asserted to fail there, so the test cannot pass by reading nothing.
- 34 tests in `SAM_Systems/SAM.Analytical.Systems.Tests`, in `SAM_Systems.sln`, **and executed by CI** —
  see 11j.

### 11j. Step 5c — eligibility and CI closure (DONE, SAM_Systems `895a86d` + `bff125f`)

**`Application` is an eligibility constraint, not documentation.** It was a comment, and `Rank` was doing
its job — so `CapableSystems` reported `CAV`, `VAV` and `DISP` as **suitable** for an Approved Document F
dwelling requirement and only their ranks kept them out of the answer. One edited number could have put a
variable-air-volume unit in a flat.

- `SystemApplication { Undefined, Domestic, Commercial, Any }` lives in **`SAM_Systems`**. Which of *these*
  templates is a dwelling system is a fact about this repository's resources, on the same footing as their
  capabilities. `SAM.Analytical` contains no reference to it and never learns the words.
- `Query.SystemCapabilityDescriptors(directory, systemApplication)` filters **before** an entry becomes a
  descriptor. **The parameter is not optional** — defaulting it made the commercial-inclusive call the one
  you get by writing nothing.
- **Two roles, asymmetrical.** As a *classification* on an entry, `Any` means "suits either". As a
  *request*, `Any` and `Undefined` both mean **no constraint** — before that, asking for `Any` returned only
  `UV`, the one template that can never be selected.
- `CommercialTemplates_AreExcludedByEligibilityAndNotByRank` **inverts every rank in the index** so
  commercial sorts first, asserts that premise, and asserts a dwelling still never sees one.
- A missing, unrecognised, numeric or comma-listed `Application` **refuses the whole index**. The last two
  matter: `Enum.TryParse` accepts `"3"` and `"Domestic,Commercial"`, and since the members number
  `Domestic = 1, Commercial = 2, Any = 3` both OR to `Any` — the most permissive value — with
  `Enum.IsDefined` unable to object. Applications are matched against the three legal names, ordinally.

**The guarantee is scoped, and the limit is recorded rather than left to be discovered.** Eligibility holds
at the descriptor boundary. It does **not** hold across the repository, because an older, live path —
`Query.DefaultSystemEnergyCentres` → `Create.SystemEnergyCentre` — derives a `SystemTemplate` from the
**file name** and matches it against each space's `VentilationSystemTypeName`, consulting neither the index
nor `Application`. **Four Grasshopper components reach it**, and a space carrying `"VAV"` still resolves
`VAV.json` there. Nothing in step 5 changed it; `ResolutionIsIdentityDriven_AndDoesNotApplyEligibility`
pins the behaviour so a future fix has something to change. **Guarding that path is separate work.**

**Rank is declared library policy and nothing else** — not derived, not a claim of engineering minimality.
The domestic order (`NV 10, EOL 20, EOC 30, MV 40, MVRE 50`) is recorded in the index as **PROVISIONAL AND
NOT CONFIRMED**: it makes selection deterministic, nothing more. Ranks need only be distinct **within** an
application, which is the only set they order.

**Unverified declarations are recorded and fail-safe.** `CAV`, `VAV` and `DISP` carry
`"UnverifiedDeclarations": [ "Boost" ]`, and a test asserts **every capability named there is false** — a
review caught `VAV` declaring `Boost: true` while listing it as unverified, which is crediting a capability
nobody confirmed. The name-derived justifications are gone: `"constant volume"`, `"displacement regime"`
and `"Variable Air Volume → can vary"` are all inference from a label. A non-commercial entry carrying an
unverified declaration is now **asserted** impossible, not merely commented.

**CI executes the suite.** `build.yml` runs `SAM.Analytical.Systems.Tests` after the ordered Rebuild, with
`/p:SAMVersion` — **which matters**: three SAM_Systems projects write to the same `build\` folder the
artifact step publishes, and a test build without it recompiles them at the `1.0.0.0` local-dev fallback
and overwrites the CI-stamped DLLs, shipping version-inconsistent assemblies. Verified: the test build now
leaves `2026.3.999.0` in place where it previously left `1.0.0.0`. A failing test no longer costs the build
artifacts either. **Not yet demonstrated on a real run** — the workflow triggers on `master`/`main`/`sow/**`
only, so it first fires when a PR to `sow/2026-Q3` is opened.

### 11k. Step 6 — the TM59 assessment recipe, extracted (DONE, SAM `d327496a`)

`SAM.Analytical.TM59AssessmentCalculator` + `TM59AssessmentResult`. **Moved, not redesigned.**

The `Tas.TSDQueryTM59Results` Grasshopper component held the only working statement of the sequence,
inside a `SolveInstance` interleaved with parameter plumbing — so nothing could call it, nothing could
test it, and the step 10 headless runner would have had to restate it and drift.

```
TSD file ──(TAS)──> analyticalModel_TSD          <- stays in SAM.Analytical.Tas
                          │
                          ├─ RestoreDesignInternalConditions(design)   <- engine-free
                          ├─ Spaces(spaces, zones)                     <- engine-free
                          └─ Calculate(spaces, extended)               <- engine-free
                                 └─> TM59AssessmentResult
                                       Spaces, MechanicalVentilationResults,
                                       NaturalVentilationResults, CorridorResults,
                                       Max/MinIndoorComfortTemperatures
```

Only the read needs TAS, so the recipe runs under plain `dotnet test` — the same split
`TMOverheatingCalculator` already makes.

**Two things deliberately preserved during step 6 so the extraction was falsifiable; both are now closed:**

1. ~~Spaces are matched by NAME in restore and selection.~~ Closed by `SimulationSpaceMap` in **step 8**
   (11n), after step 6 had pinned the old behaviour. Every flat's `Bedroom 2` regression proves it.
2. ~~The criterion comes from `TMOverheatingCalculator`'s zone-name/default derivation.~~ Closed for
   scenario-aware calls in **step 7** (11l); legacy calls without scenarios deliberately retain it.

One incidental asymmetry removed with no behaviour change: the component null-guarded two of its three
result splits and not the third. `FindAll` never returns null, so the guard never did anything; the three
are now one generic `Split<T>`.

**9 tests**, and the one that makes this an extraction rather than a rewrite is
`TheService_MatchesTheComponentsOwnSequence` — the component's sequence is **inlined verbatim** from its
`SolveInstance` and its output compared with the service's: spaces, all three criterion lists, and both
comfort limit series. Deliberately not factored; it is a transcript. Also pinned: the silent
no-assessment when the series key is the analytical spelling against a TAS-written model, and that
**no spaces and no zones means the whole model** — which is why the real run exported a communal corridor
into a domestic overheating assessment as an ordinary room.

~~**NOT DONE** — the Grasshopper component still holds its own copy.~~ **CLOSED in step 7a** — see 11l.
`SAM_Tas_Grasshopper` is now the **fifth repo** in the workstream and `TasTSDQueryTM59Results` calls the
service.

### 11l. Step 7 — the scenario is authoritative over ventilation strategy (DONE)

Three commits, in this order, because 7a had to be behaviour-preserving **before** the semantics changed:

| Repo | SHA | What |
|---|---|---|
| SAM | `193968ff` | **7a** — `TM59AssessmentCalculator.SourceFallback`, so the repoint keeps TAS's provenance |
| SAM_Tas | `f6e32b4` | **7a** — `Create.TM59AssessmentCalculator`, TAS's two series keys + provenance in TAS's assembly |
| SAM_Tas_Grasshopper | `047c583` | **7a** — `TasTSDQueryTM59Results` repointed; ~90 lines of `SolveInstance` become four calls |
| SAM | `f6772519` | **7b** — `VentilationStrategyMap` + `VentilationStrategySelection`; the criterion is stated, not derived |
| SAM_Tas | `b03f02b` | **7c** — the TM59 **XML export** takes the strategy from the scenario too |

**7a, and why `SourceFallback` had to exist first.** The component reached the recipe through
`SAM.Analytical.Tas.OverheatingCalculator`, which supplies three values that are TAS's and not the
assessment's: the two series keys the TSD conversion writes, and the assembly name a result reports as its
`Source` when the model is unnamed. The service exposed the keys but not the fallback, so repointing would
have silently changed a published result's provenance from `SAM.Analytical.Tas` to `SAM.Analytical` as a
side effect of a refactor. `Create.TM59AssessmentCalculator` keeps all three in `SAM_Tas` rather than
restating them at the call site — a second place to keep them right is the drift the extraction existed to
stop. **One behaviour difference, and it is a refusal replacing a crash:** where the TSD read produced
nothing the old code dereferenced null, and the component now reports `"Invalid data"`.

**7b — what the map replaces.** All three derivations in 11d, and it replaces them rather than seeding them.
`VentilationStrategyMap` is keyed on `Space.Guid`, **never on a name**, and it does no matching of its own,
so it cannot quietly reintroduce matching by name. At the step 7 checkpoint, resolving a simulation space
to its design space remained step 8's job; it is now closed in 11n.

- **Refusal never falls through.** A space no scenario covers, a scenario that states no strategy, and two
  scenarios stating different strategies are three refusals with **three different sentences**, because they
  are three different mistakes with three different fixes. A refused space produces **no result** and its
  reason lands in `TM59AssessmentResult.VentilationStrategyRefusals`. Falling back on refusal would restore
  the exact defect the map removes, invisibly, at the one input where nothing was said.
- **Reported, not thrown**, so one unstated dwelling does not cost every other dwelling its assessment.
- **Nothing is inferred from provenance** — not `Source`, not TSD-versus-TPD, not which engine wrote the
  numbers.
- **What a strategy MEANS is unchanged**: `UV` → corridor, `NV` → natural, anything else → mechanical, the
  vocabulary the criterion selection already had. Step 7 changed *which* strategy applies.
- **Scenario identity untouched.** `VentilationStrategy` / `HasVentilationStrategy` already existed and are
  only read. `Key_IsStableAcrossBuilds` still pins the same guid.
- **Left unsupplied, nothing changes.** `SystemTypeName` is documented as **superseded**, not deleted — the
  Grasshopper and user-interface callers have no scenario to state yet, and removing their behaviour without
  giving them a way to state the right one would be a regression.

**7c — the export refuses the WHOLE document, which is deliberately different from the assessment.** The
assessment drops a refused space and reports it; a TM59 XML is configuration for the external TAS TM59 tool,
which has no way of being told a room is missing — it would assess what it was given and produce a
complete-looking answer for an incomplete building. So `ToTM59(…, VentilationStrategyMap, out refusals)`
returns **null** if any space is refused, having visited every space first so one unstated dwelling does not
hide the others' reasons. The strategy goes in through `Space.ToTM59`'s existing `systemType` parameter —
the seam that was already there — and a non-`Undefined` value means that method's internal-condition
fallback is never reached. The two-argument overload is untouched and a null map delegates to it.

**Tests: SAM 1173 (was 1137), SAM_Tas TM59.Tests 42 (was 25).** Every override test carries a **control**
that runs the same model without the map and shows the old derivation reaching the opposite answer —
without those, a passing test would prove only that the map agreed with a derivation that was already
right. Five mutations run, all caught and restored — listed in 0c, along with the build-order trap one of
them exposed.

**What the independent review found, and it was not cosmetic** (fixed in SAM `d2b0f971` + SAM_Tas
`121035c`):

1. **An unrecognised strategy was silently assessed as MECHANICAL** — the step 7 defect pointing the other
   way, reachable from the *authoritative* path. `"Natural"`, `"Mixed Mode"`, `"N-V"`, or `"MVHR"` (a name
   that does not exist, because `MVRE` is SAM's heat-recovery ventilation) all became mechanical results with
   no refusal. The vocabulary is now **closed**; see 0d items 3 and 4.
2. **The export still dropped rooms silently.** `Space.ToTM59` returns null for a space with no internal
   condition and for a null `TM59Manager`; those were omitted, no refusal was recorded, and the completeness
   gate still passed — a three-space building shipped two zones as a success, which is the one failure the
   external TAS tool could never notice.
3. **A null export return could carry an empty reason list**, so the documented contract lied.
4. **`TM59AssessmentResult.VentilationStrategyRefusals` was caller-mutable** while the property it is copied
   from was not — a reporting layer de-duplicating in place could erase the record of which dwellings went
   unassessed.
5. **Two tests were not falsifiable.** The zone-name control used zones named `"NV"`/`"NV Wing"` against an
   MVRE scenario and asserted `"Natural"` — which the `"NV"` default says anyway, so it would have passed
   with the entire zone-name lookup deleted. It now uses `MVRE` and `MVR` against an `NV` scenario, where the
   control reads *mechanical* and the assertion flips. **And the direction of the `StartsWith` step was
   documented backwards**: the comparison asks whether the *library entry's* name starts with the zone name,
   so a zone must be a **prefix** of an entry — `"NV Wing"` matches nothing and went through the default,
   while `"MVR"` genuinely exercises it. Separately, the test claiming to pin derivation #2 in the assessment
   could not: `TMOverheatingCalculator` never consults a `VentilationSystem`, so its control came from
   derivation #1. Renamed to what it proves — that an attached mechanical system is **inert** there —
   with derivation #2 pinned on the export side, where it lives.
6. **The normalisation comment was wrong and `.Trim()` was dead code.** `OverheatingScenario.Normalized`
   already rebuilds the `SystemTemplate` through setters that strip **every** space, so `"MV RE"` and
   `" uv "` cannot reach the map; only `.ToUpper()` does work, and the test claiming to pin whitespace
   normalisation was vacuous.

The review confirmed **no behaviour regression in the 7a repoint**, having read the old `SolveInstance`
against the new one line by line, and confirmed `OverheatingScenario`'s identity is untouched.

**The step 6 equivalence test still matters and its doc comment now says why.** The component calls the
service, so the two agree by construction and the component can no longer disagree with itself. What
`TheService_MatchesTheComponentsOwnSequence` pins is the only falsifiable thing left: that the service
still does what the component *used* to do. Delete it and the last statement of the original behaviour goes
with it.

**Historical step 7 stopping point, closed by step 8.** At this checkpoint no production caller supplied a
map: `Convert.ToXml(AnalyticalModel, …)` — reached from `ToTBD`, which wrote the real run's DomOv XML — still
called the two-argument `ToTM59`. Step 8 now adds an optional scenario input to both existing Grasshopper
entry points and routes supplied scenarios through identity-aware maps; omission deliberately retains this
legacy path. See 11n.

### 11m. Step 9 — the TSD-simple vs TPD-full boundary (Michal's clarification, AUTHORITATIVE)

**The two-pass TPD route is a deliberate TAS compatibility workaround. It is NOT duplication. Do not remove
it, simplify it, or "consolidate" it into the TSD path.** It is expensive on purpose.

**Why it exists.** The TPD route does not give the `ResultantTemperature` series TM59 needs, so the workflow
deliberately runs **two** simulations.

**The intended TPD-full sequence, as stated by Michal:**

1. Simulate the actual system.
2. Read the resulting **supply air temperature** and **supply airflow**.
3. Inject those values into a **copy** of the TBD.
4. Simulate that modified TBD again.
5. Read the resulting TSD.
6. Take the TM59 `ResultantTemperature` from that second simulation.
7. Pass the prepared analytical result model into the common `TM59AssessmentCalculator`.

**Constraints, all of them binding:**

- The first simulation's supply temperature and airflow are **engineering inputs** to the second run, **not
  provenance**. They are therefore not something a scenario states and not something step 7's
  `VentilationStrategyMap` touches.
- **Always modify a COPY of the TBD**, never the original design model.
- **Preserve current TPD-full behaviour exactly** until TAS exposes an equivalent native resultant
  temperature through the TPD workflow.
- **Documentation and regression coverage must explain why the two-pass route exists**, so a future refactor
  cannot delete it as duplicate work. That coverage is owed and does not exist yet.
- When TAS does expose it natively, the workaround may be retired **behind the preparation boundary** without
  changing `TM59AssessmentCalculator`.

**The boundary step 9 must draw.** Preparation differs; assessment does not.

```
TSD-simple:  TSD ───────────────────────────────────────────► analytical result model ─┐
                                                                                       ├─► TM59AssessmentCalculator
TPD-full:    TPD ─► pass 1 ─► TBD COPY ─► pass 2 ─► TSD ────► analytical result model ─┘
```

`TM59AssessmentCalculator` **must not care** which side prepared the model, and must not learn the words
TSD or TPD.

**What the code does TODAY — read this before touching anything, because it is NOT the sequence above.**
Two separate TPD-side mechanisms exist and **only one of them is the two-pass route**:

| | Where | What it actually does |
|---|---|---|
| **A** | `SAM_Tas/…/SAM.Analytical.Tas.TPD/Modify/CalculateResultantTemperature.cs`, driven by the component `Tas.CalculateResultantTemperatureFromTPD` (`SAM_Tas_Grasshopper/…/SAM.Analytical.Grasshopper.Tas.TPD/`) | **The two-pass route.** Reads an already-simulated TPD (`Simulate = false`, `IncludeComponentResults = true`), takes each space's `SpaceDataType.ZoneTemperature`, **copies** the TBD to `<name>_TPDThermostat.tbd`, writes that series into every zone internal condition's thermostat **upper- and lower-limit profiles** as yearly profiles (`factor 1`), **simulates again** to `<name>_TPDThermostat.tsd`, and returns both paths |
| **B** | `SAM_Tas_Grasshopper/…/SAM.Analytical.Grasshopper.Tas.TPD/Component/TasTPDQueryTM59Results.cs` | **Not the two-pass route.** Reads the TSD beside the TPD via `ToSAM_SpaceSystemResults(path_TPD, out path_TSD)` and synthesises `ResultantTemperature` as the arithmetic **mean of the TSD's `MeanRadiantTemperature` and the TPD's `ZoneTemperature`**, then runs its own inline copy of the TM59 recipe |

**The three discrepancies are now RESOLVED — Michal decided them at the start of the step 9 session, and
step 9 implemented the answers. Kept here as the record of what was open and what was decided.**

1. ~~Whether the intended sequence describes A's evolution or a different mechanism.~~ **A is authoritative
   and the intended transfer was to be supply air temperature + supply airflow.** Implemented as far as TAS
   allows, which turned out to be **not at all** on the write side — see 11o, which records the limitation and
   the evidence. `UpdateSpaceAirflows` / `UpdateFanAirflows` were checked and are **not** the required
   transfer: both write **scalar design sizing values into the TPD**, not hourly series into a TBD copy.
2. ~~Which of A and B is the authoritative TPD-full path.~~ **A is authoritative; B is a legacy/approximate
   compatibility route.** A must never fall back to B.
3. ~~Where B's preparation boundary settles.~~ **Inside `SAM.Analytical.Tas.TPD`**, as
   `ApproximateResultantTemperatureMap`, with the component reduced to read → prepare → assess.

Steps 7 and 8 changed **nothing** in either mechanism, by design. Step 9 changed both.

### 11n. Step 8 — identity-based result association (DONE and independently reviewed)

**Goal.** Associate simulation-derived spaces, design spaces, scenarios and results by identity through
`SimulationSpaceMap`, never by a direct name join. The mandatory regression is the real failure shape:
three flats each containing a room named exactly `Bedroom 2`, plus a communal corridor.

**Original implementation:** SAM `d7abd48a`, SAM_Tas `1aba5eec`, SAM_Tas_Grasshopper `f8ca646c`.
It applied the existing stable-key/unique-name/refuse map to internal-condition restore, requested-space and
zone selection, TAS zone identity and the normal TSD result component. The original mutation replacing the
identity restore with name matching failed three tests including the mandatory duplicate-bedroom case.

**Both existing production acceptance paths are preserved and now expose the same architecture:**

1. **Normal Grasshopper results:** `To gbXML` → `SAMAnalytical.WorkflowgbXML` (`Simulation=true`) → TSD →
   `Tas.TSDQueryTM59Results`. Version 1.0.8 appends optional `overheatingScenarios_`, constructs the
   `OverheatingScenarioMap` with the calculator's TAS-keyed `SimulationSpaceMap`, installs its authoritative
   `VentilationStrategyMap`, and reports identity/scenario/criterion refusals as Grasshopper warnings.
2. **Official TAS print:** `SAMAnalytical.CreateTBDByTM59` → TM59 XML/TBD/TSD → TAS UI, where spaces remain
   preconfigured and the user moves through the existing TAS TM59 tabs and prints. Version 1.0.3 appends the
   same optional scenario input, uses a design-identity `SimulationSpaceMap`, and calls the scenario-aware
   XML overload. An incomplete association refuses the whole XML. No replacement print workflow was added.

Appending the optional inputs, rather than inserting them among existing ports, preserves saved Grasshopper
definitions. With no scenarios both components retain their previous acceptance path and fallback behaviour;
with scenarios, the identity/scenario path is authoritative.

**Independent review findings and fixes:** SAM `f712fd9f`, SAM_Tas `7f3dfda`,
SAM_Tas_Grasshopper `b8dae4b`.

1. A whole-model request included unresolved/ambiguous simulated spaces after restore had correctly refused
   them, so those spaces could still produce results. Whole-model selection now includes only resolved map
   entries and reports exclusions.
2. A two-candidates-to-one-design collision removed the reverse mapping but left both forward mappings
   usable, allowing both candidates to receive design state. The collision now invalidates both directions.
3. `Spaces(null, zones)` selected the whole model instead of the supplied zones. Whole-model now means both
   `spaces` and `zones` are null.
4. A scenario ownership collision marked the shared lookup ambiguous but retained the first scenario's full
   stored selection and live ventilation claim. The first scenario is rolled back as a whole, and the
   ventilation map is built only after all ownership conflicts settle.
5. The official print component had only the legacy XML call and the normal results component had no
   scenario input. Both were wired at their existing seams as described above.
6. The new optional inputs were initially inserted mid-list, which would have shifted saved component port
   indices; review moved both to the end. Review also prevented a successful SAP conversion from overwriting
   a failed/refused TM59 status while still allowing the SAP attempt to run.

**A further review pass, SAM `cc5e67f0`**, on top of the above:

7. **The same assessment stated twice was treated as a collision.** Two scenarios are the same assessment
   exactly when they derive the same `Key`, and the rule this codebase already follows -
   `VentilationStrategyMap` for a repeated strategy, `Query.SelectPreferredCapableSystem` for one system
   listed twice - is that one answer said twice is still one answer. A caller assembling its scenario list
   from two overlapping sources was refused AND had the scenario struck out. Caught before anything is
   written, because re-walking the spaces and then storing the result would have overwritten the first
   pass's record of them with an empty list. A scenario struck out by a real collision is absent from that
   record, so it cannot be resurrected this way.
8. **Proof 10 was only a reflection scan over member names**, which proves something about the vocabulary
   and nothing about the implementation. It now first varies the simulated model's **name** - what a result
   reports as its `Source` - and the stamped provenance fallback, and asserts every association comes out
   identical, with the results demonstrably carrying the different provenance so it cannot pass by the two
   models being the same. The scan is kept as a guard against a future member reaching for a file name when
   an identity is missing.

Two more mutations, caught and restored: disabling the same-key guard fails
`TheSameScenarioTwice_IsNotACollision`; building the strategy map inside `Add()` instead of the second pass
fails `TwoScenariosOverOneSpace_RefuseExplicitly` and `ThreeScenariosOverOneSpace_AllRefuse`.

**Final validation:** SAM **1185 passed, 0 failed**; SAM_Tas TM59.Tests **46 passed, 0 failed**; the
SAM_Tas_Grasshopper project built under VS Framework MSBuild with **0 errors**. The TAS suite covers both
paths with three exact duplicate `Bedroom 2` names plus the corridor. Three additional review mutations
proved the resolved-only whole-model filter, zones-only scope and transactional scenario ownership; all were
restored before the final full runs.

**Step 9 boundary preserved.** Neither
`SAM.Analytical.Tas.TPD/Modify/CalculateResultantTemperature.cs` nor
`SAM.Analytical.Grasshopper.Tas.TPD/Component/TasTPDQueryTM59Results.cs` changed. Mechanism A remains the
two-pass TBD/TSD thermostat-profile workaround; mechanism B remains the separate one-pass MRT + TPD zone
temperature synthesis with its inline recipe. The discrepancies in 11m are recorded accurately and require
Michal's decisions before step 9 begins.

### 11o. Step 9 — the TSD-simple vs TPD-full boundary (DONE)

**Implemented:** SAM_Tas `418f508`, SAM_Tas_Grasshopper `697c798`.

**The rule the code now holds: preparation differs; assessment does not.** Three routes reach TM59, each
preparing the required hourly `ResultantTemperature` differently, and all three then run the identical
engine-neutral `TM59AssessmentCalculator`, which still never sees the words TSD, TPD or TAS.

```
TSD-simple:  TSD ─────────────────────────────────────────────────► model ─┐
TPD-full:    TPD ─► pass 1 ─► TBD COPY ─► pass 2 ─► TSD ─────────► model ─┼─► TM59AssessmentCalculator
TPD-approx:  TPD + companion TSD ─► synthesised (MRT + ZT) / 2 ──► model ─┘
```

**New types, all in `SAM.Analytical.Tas.TPD` and all free of TAS COM types** — which is what makes the
boundary testable without an installed TAS, and is the reason the new suite runs under `dotnet test`:

- `ResultantTemperatureTransfer` — names what crosses between the two passes, so the route's engineering
  content is reviewable instead of implicit in one method body.
- `ResultantTemperaturePreparation` — the TPD-full route's decisions: paths, transfer, refusals.
  `TryBeginSecondPass` is the whole "may we proceed, and on which file" step, so the invariants can be
  proved: **a refusal copies nothing and leaves the design TBD byte-identical**, and where the route
  proceeds it proceeds **on a copy**.
- `ApproximateResultantTemperatureMap` — mechanism B's preparation, moved out of the Grasshopper component.

#### THE LIMITATION — the intended transfer cannot be performed, and is refused rather than approximated

**Michal's intended sequence was: read the first pass's supply air temperature and supply airflow, inject
both into the TBD copy, simulate. TAS cannot do this, and the blockage is specifically on the WRITE side.**

- **The read half is available and already happening.** `Convert.ToSAM_SpaceSystemResults` calls
  `ToSAM_SpaceSystemResult(null, start, end)`, which with no explicit list asks the `SystemZone` for **every**
  `SpaceDataType` — and that enum already contains `SupplyAirTemperature = 3` and `FlowRate = 1` alongside
  `ZoneTemperature = 9`. The numeric values are the TPD result-data-type codes, confirmed by the sibling
  enums (`FanDataType`, `AirJunctionDataType`) sharing codes 1–8 for the standard duct/air result set. So the
  supply conditions are in hand.
- **The write half has nowhere to go.** Reflected over the whole `Interop.TBD` assembly (143 types): the
  **only** temperature-valued member anywhere in it is `IWeatherYear.groundTemperature`. A TBD zone, its
  internal condition, its thermostat and its internal gain expose **no per-zone supply air temperature of any
  kind**. That is by design — TBD introduces ventilation air at outside or adjacent-zone conditions, and
  conditioned supply air is a TPD concept. It is exactly why TAS keeps the two models apart.
- **Injecting the airflow alone would be worse than partial progress.** TBD's ventilation profile
  (`Profiles.ticV`, which SAM already reads as `InternalConditionParameter.SupplyAirFlow`) does accept an
  hourly series, so the airflow half is mechanically writable. But air introduced **without its temperature**
  enters at outside conditions, which states a system that does not exist. And once the thermostat pins the
  zone air temperature — which is what the supported transfer does — an injected flow moves only the plant
  load the second pass is not being used for, leaving `ResultantTemperature` **untouched**. The two halves
  are not independent, and half of this transfer is not a fraction of the answer.
- **`UpdateSpaceAirflows` / `UpdateFanAirflows` are not it.** Both operate on the **TPD** (`TPDDoc`) and set
  **scalar design sizing values** (`FlowRate.Value`, `FreshAir.Value`, `tpdSizedVariableValue`). They are
  neither hourly nor TBD-side.

**Therefore `ResultantTemperatureTransfer.SupplyAirTemperatureAndAirflow` is refused with the reason stated,
and the supported transfer remains `ZoneTemperatureToThermostatLimits` — now named as the approximation it
is.** That transfer is not laziness: `ResultantTemperature` is a function of air temperature and mean radiant
temperature, so pinning the air temperature to the first pass's **achieved** value leaves TBD to compute only
the radiant half, which is the half TPD cannot give. It closes the loop; the intended transfer cannot.

**If a future TAS release exposes a per-zone supply air temperature on the TBD side**, the one place that has
to change is `ResultantTemperaturePreparation.TransferRefusal`.

#### Behaviour changes, all deliberate and all previously silent

1. **A TPD carrying no usable first-pass results now refuses** rather than copying the TBD and simulating a
   copy identical to the design model — which would return a plain TBD answer while the workflow reported it
   as systems-aware. The one failure on that route that produced a *plausible* number rather than an error.
2. **A TPD series shorter than the radiant series refuses** rather than being wrapped to fill the year.
   `GetValues(range, bounded: true)` **wraps** the available indices, so the returned length proves nothing —
   the check is against `GetMinIndex`/`GetMaxIndex`.
3. **Mechanism B stopped matching by name**, in three places: TPD-result attribution, design internal-condition
   restore, and requested-space/zone selection. All three took the first same-named candidate, so in a block of
   flats one dwelling's system results and another dwelling's internal condition could be reported against a
   third dwelling's room. Identity is decided **once for the whole model**, not per space — a stamped space
   with no matching result must not fall through to the name rule and collect a same-named sibling's results.
4. **Refusals are reported** as Grasshopper warnings on both TPD components. `CalculateResultantTemperatureFromTPD`
   previously returned `false` with nothing said, so a missing companion TBD was indistinguishable from a
   failed simulation.

#### Independent review, and the one finding that matters most

**Review fixes:** SAM_Tas `134dc1d`, SAM_Tas_Grasshopper `94ac244`. Nine confirmed defects and
four risks were raised; all valid ones are fixed. Three deserve recording here.

**1. THE OPEN RISK — the two identities have never been shown to be the same string, and if they are not, the
approximate route returns NOTHING for a block of flats.** Mechanism B correlates a space to a TPD result by
matching `SpaceParameter.ZoneGuid` (which `ActiveSetting` maps to `TSD.ZoneData.zoneGUID`) against
`SystemSpaceResult.Reference`. That reference comes from `(systemZone as dynamic).GUID` — and **`ISystemZone`
declares no `GUID`**, verified by reflection, so it binds to `ISystemComponent.GUID`: **the TPD component's own
guid.** Nothing establishes that a TPD component guid equals a TBD/TSD zone guid, and step 8's "verified on the
real run" evidence covers only the TSD route. If they differ, identity mode never engages, the whole model falls
to name mode, and three rooms all called `Bedroom 2` are **entirely refused** — safe, but useless, and a
functional regression against the old (mis-attributing) behaviour.

**What was done about it:** the risk is documented on the class, the fall back to names now emits **one
model-wide diagnostic naming the cause** rather than only N baffling per-room messages, and a test
(`WhereTheTwoIdentityNamespacesDisagree_ItSaysSoOnceAndFallsToNames`) gives the two sides **different** strings
so the assumption cannot be silently re-encoded in the fixture — which is exactly what the original tests did.

**The probable fix, NOT applied:** `IZoneLoad.GUID`. `ITSDData` — the object carrying `TBDPath` and `TSDPath` —
exposes `GetZoneLoadForGuid(string)`, so a zone load is addressable by the guid the TBD/TSD side uses. Moving
`ToSAM_SpaceSystemResult` onto `zoneLoad.GUID` is therefore the likely correction, but `Reference` also
correlates a `SystemSpaceResult` to its `SystemSpace`/`SystemZone` in the energy-centre model and is persisted
in JSON. **Verify against Michal's real TPD (`2027-08-03-HVAC`, 11e) before changing it.**

**2. A claim in the step 9 commit message is WRONG and this is the correcting record.** The commit says the only
temperature-valued member in `Interop.TBD` is `IWeatherYear.groundTemperature`. It is not: `IControls` has
frost-protection, authority and night-setback temperatures, `IEmitter` has outside-temperature cut-offs, and
`ISurfaceOutputSpec` has `dryBulbTemp`. The original scan matched `Temperature` case-sensitively and missed
every member spelled `Temp`. **The load-bearing conclusion survives and was re-verified member by member: no
TBD type exposes a per-zone supply air temperature, and none of those members is a zone supply condition.** The
code comments and the test were corrected; the commit message cannot be.

**3. A silent scope change on `Tas.TPDQueryTM59Results`, deliberate but previously unrecorded.** Supplying
**only zones** in `_spaces_` used to assess the **whole model** (the old code treated `spaces == null` as
whole-model and then *added* the zones' spaces). It now returns only the zones' spaces, because
`TM59AssessmentCalculator.Spaces` means whole-model when **both** arguments are null — step 8 review finding 3,
now inherited by this component too. Correct, but existing definitions wired zones-only will return fewer rows.

**Also fixed:** malformed paths refuse instead of throwing `ArgumentNullException` out of a Grasshopper port;
the length guard now checks **alignment and count** rather than index span, so a TPD simulated over a
sub-period refuses instead of being silently **rotated** to fill the year; two spaces claiming one identity are
refused in both directions, matching `SimulationSpaceMap`'s policy; `ApproximateResultantTemperatureMap` gained
`IsSupported` and the component checks it rather than dereferencing a null model; a broken XML `cref`; redundant
deep clones of 8760-element parameter sets; and a null JSON element now refuses instead of throwing in a cast.

**Recorded, not fixed — `Modify.CalculateResultantTemperature` still zero-fills.** The authoritative route reads
`GetValues(new Range<int>(0, 8759))` **unbounded**, so a TPD simulated over part of the year yields `0.0` for the
rest, and those zeros go into **both** thermostat limit profiles of the TBD copy — the second pass then runs
against a 0 °C setpoint outside the simulated period. Pre-existing and left alone under "preserve behaviour
exactly", but note the irony: **mechanism A, the authoritative route, lacks the very protection step 9 added to
mechanism B.** Worth its own change and regression.

#### Proofs (19 new tests, `PreparationBoundaryTests`)

Original TBD unchanged on refusal · second pass operates on a copy · payload follows the first pass's own
results · supply conditions readable but neither half transferred · the intended transfer refused with its
reason · unstated transfer refused · a failed TPD-full run produces nothing assessable and never falls back ·
the synthesis is the mean · **the common calculator cannot synthesise a resultant temperature itself**
(handed a radiant series alone it produces no assessment — the behavioural proof that B's arithmetic is not
in `SAM.Analytical`) · B ties each `Bedroom 2` to its own TPD result · without identity B refuses rather than
crossing flats · TSD-simple assesses without the two-pass preparation · the two TPD routes prepare
distinguishably (A carries 24.0 through unchanged, B yields 22.0 from the same inputs) · both routes converge
on the same assessment · step 7 ventilation authority intact on the approximate route.

### 11p. Iteration 1 — BasePassive, the first usable vertical slice (DONE)

**Implemented:** SAM `5f3ad8fb`. **Michal re-prioritised this mid-session**: the earliest usable milestone is
a complete BasePassive slice through the **existing TSD-simple workflow**, producing TM59 results he can test
by hand, and TPD-full/ActiveTrimCooling must not delay it.

**What was actually missing, and it was not what the name suggested.** `PartOIteration` was **identity-only** —
its own documentation says no member of it causes any behaviour anywhere — and, more importantly, **no
component anywhere created an `OverheatingScenario`.** Three components accepted `overheatingScenarios_`
(`Tas.TSDQueryTM59Results`, `Tas.TPDQueryTM59Results`, `SAMAnalytical.CreateTBDByTM59`) and nothing produced
one, so the whole scenario-authoritative path from step 7/8 **could not be driven from Grasshopper at all**.
That, not the enum, was the blocker to manual testing.

- `Query.PartOOperatingAssumptions(PartOIteration, out string refusal)` — the single place a stage's
  assumptions are written down. Four names: `Openings Restricted`, `Mechanical Ventilation At Design Rate`,
  `Boost Available`, `Summer Bypass Available`. **BasePassive** and **AcousticRestricted** are stated;
  **ActiveTrimCooling refuses** (what it assumes about a cooling provision is unsettled, and guessing would
  put an unreviewed assumption inside a permanent identity); **Undefined returns an empty set**, which is the
  honest answer rather than a refusal.
- `Create.OverheatingScenarios(zones, iteration, strategyByZoneGuid, out refusals)` — the set for one stage.
  Scope comes from `Query.PartOClassifyAssessmentZones` so it cannot drift from what Part F sizes. **One
  stricter rule on top:** an **unmarked** zone beside marked ones is **refused**, not called a common space —
  that split is right for classifying a marked-up model but would assess an unmarked bedroom against the
  corridor criterion. A model that marks nothing keeps the documented legacy behaviour. A zone stating no
  ventilation strategy is refused, never defaulted.
- `SAMAnalytical.CreateOverheatingScenarios` (1.0.0) — the missing component. Zones + iteration + strategies
  → scenarios, with refusals as warnings and on an output.

**THE SCOPE BOUNDARY, and it must not be misread.** Stating a stage **does not make a simulation obey it**.
Nothing in this slice writes aperture control profiles or ventilation rates into a TBD, so a BasePassive
scenario asserts the model was **assessed** as base provision, not that it was **built** that way — and
`Openings Restricted` / `Mechanical Ventilation At Design Rate` are properties of the **simulation inputs**.
Applying a stage to the export is separate, unwritten work. **Modelling the stage you state remains the
modeller's job today.**

**The four assumption NAMES are permanent** — they participate in `OverheatingScenario.Key`, so renaming one
re-keys every scenario that states it and orphans every result already attributed. **Their VALUES are declared
policy** read off `PartOIteration`'s own stage definitions; nothing in Approved Document O was parsed, and
**Michal needs to confirm them.** Note `AcousticRestricted` states a summer bypass that **no shipped template
can satisfy** (0e) — that refusal belongs to capability selection, not here, because what a scenario *assumes*
and what a system can *do* are different statements and conflating them would hide the mismatch.

**How to test it by hand:** `SAMAnalytical.CreateOverheatingScenarios` (zones from the dwelling category,
`_partOIteration` = `BasePassive`, one strategy per zone or one for all) → `overheatingScenarios` into
`Tas.TSDQueryTM59Results`'s appended `overheatingScenarios_` input → TM59 results, with any refusal shown as
a component warning.

**Deferred, on Michal's instruction:** `AcousticRestricted` is *stated* and key-distinct but has no
input-side behaviour either; `ActiveTrimCooling` refuses.

### 11q. Iteration 1 - the REAL BasePassive slice: Part F airflows reach the simulation (DONE)

**Implemented:** SAM `2e8c513f`. **Michal corrected the milestone**: a scenario attached to an existing
simulation is not the deliverable. The slice must be

```
AnalyticalModel -> Part F calculation/terminal transfer -> Part-F-PREPARED AnalyticalModel
              -> BasePassive preparation -> existing TSD simulation -> TM59AssessmentCalculator
```

with the calculated Part F airflows **inspectable on the model before simulation**.

#### What already existed and is reused unchanged - do NOT write another airflow recipe

- `Query.DefaultPartFCalculator()` loads `PartFCalculator` with the shipped rule set
  (`SAM_PartFSpaceRulesUKDwellingsMVHR.json`).
- `partFCalculator.AdjacencyCluster = ...; Calculate(zoneCategoryName)` sizes one house, or every dwelling zone
  in a category independently.
- **It writes its answer onto the model** - `PartFCalculator.cs:1286`,
  `space.SetValue(SpaceParameter.PartFSpaceData, ...)`.
- `PartFSpaceData` holds the authoritative **terminal collection** (`PartFVentilationTerminalRequirement`) with
  `ContinuousDesignFlowRate_Lps` / `HighFlowRate_Lps` / `SetbackFlowRate_Lps` /
  `MinimumRequiredFlowRate_Lps`, plus space roll-ups `ContinuousSupplyFlowRate_Lps`,
  `ContinuousExtractFlowRate_Lps`, `NetContinuousFlowRate_Lps`. Dwelling level: `FinalSystemRate_Lps`,
  `SetbackSystemRate_Lps`, `DwellingResults`.
- Driven already by `SAMAnalytical.AddVentilationPropertiesByPartF` (sizing) and
  `SAMAnalytical.CheckPartFCompliance` (sizes identically, then transfer-air network, door undercuts, purge,
  overall status - the terminal-transfer workflow).

#### The gap that was closed

**`Query.CalculatedSupplyAirFlow` - what the TAS export reads - sums four `InternalCondition` parameters in
m3/s and had never looked at `PartFSpaceData`.** Part F sizing (l/s, on the space) and simulation airflow
(m3/s, on the internal condition) were two disconnected representations, so a Part-F-sized model simulated
with whatever its internal conditions happened to say, and the Part F numbers were **reporting only**.

`Modify.ApplyPartFVentilationRates(model, PartFOperatingMode, out refusals, out notes)` is the bridge. **Two
hazards make it more than a unit conversion, and Michal decided both:**

1. **The summing hazard.** Writing a Part F rate beside an existing per-area or per-person rate would **add**
   to it and over-ventilate the room, silently. **Decision: Part F becomes authoritative** - the other three
   supply bases and their three exhaust twins are cleared, and every space where something was displaced is
   named in `notes`.
2. **The shared internal condition hazard.** Part F rates are per room; internal conditions are routinely
   shared between rooms, so writing in place would let one bedroom's rate overwrite another's - the normal case
   in a block of flats. **Decision: a per-space clone**, fresh guid, named after the space, disambiguated where
   two rooms share a name (three flats all have a `Bedroom 2`).

Also: a wet room gets its extract and an **explicit zero supply**, because under the balanced MVHR arrangement
Part F sizes its make-up air is transfer air through the internal door, and a stale supply rate would model a
room ventilated twice. `MeasuredCommissioning` is **refused** - measured rates are site evidence and must never
be driven into a design simulation.

`Query.PartOIterationOperatingMode(PartOIteration, out refusal)` is the join between the two documents.
**BasePassive -> `PartFOperatingMode.ContinuousDesign`**, which is a restatement of what the iteration says,
not a judgement. **`AcousticRestricted` is REFUSED**: its assumptions say boost is *available*, not continuous,
and simulating a whole cooling season at the Table 1.2 high rate is a materially **more favourable** claim than
making boost available to a control strategy - an engineering decision, not a mapping. Returns nullable rather
than an `Undefined` enum member, which would have renumbered every persisted `PartFOperatingMode`.

`SAMAnalytical.PreparePartOIteration` (1.0.0) drives the slice: Part-F-sized model + iteration + strategies ->
applied model + scenarios + **the applied supply read back through the query the SIMULATION uses** (not the
value that was written) alongside the Part F l/s it came from, plus notes and refusals.

#### Still NOT done - reported, not implied

1. **No ventilation SYSTEM object is selected or applied.** Step 5's capability selection
   (`Query.PartFSystemCapabilityRequirement`, `Query.CapableSystems`, `Query.SelectPreferredCapableSystem`)
   exists and `SAM_Systems` holds the catalogue, but **grep finds no production caller anywhere** - only
   comments and tests - and `Modify.AddVentilationSystem` is never called from Part F. Note the constraint:
   selection needs `SystemCapabilityDescriptor`s from `SAM_Systems`, which `SAM.Analytical` deliberately does
   not reference (11h), so the descriptors must be passed in by the caller.
2. **Nothing touches aperture control**, so BasePassive's "openings operated without restriction" is still not
   enforced on the model.

#### Proof (11 new tests, `PartFAirflowApplicationTests`)

End to end on the **real** calculator with the shipped rule set - nothing stubbed - asserting the applied rate
equals the Part F rate as read back through `CalculatedSupplyAirFlow`; a wet room gets extract and zero supply;
an existing per-area rate is cleared and reported rather than added to; two rooms sharing one internal
condition end up with the rates their own volumes earned; the supplied model is unmodified; measured rates
refuse; an unsized model refuses naming what to run first; a sized space with no internal condition refuses;
and the three iteration mappings.

**The non-mutation test caught a real aliasing bug:** `AdjacencyCluster`'s clone **shares its `Space` objects**
with the model it came from, so assigning an internal condition in place reached back through the clone and
changed the caller's model. Fixed by writing onto `new Space(space)`, which keeps the guid so `AddObject`
replaces rather than duplicating. **Remember this for any `Modify` that claims to return a copy.**

### 11r. Two ActiveSetting-dependent failures, found by CI after opening the PRs (DONE)

**Implemented:** SAM `5565d96c`.

**Context.** Michal asked to merge `sow/2026-Q3` into all five feature branches and open PRs (see 0a for the
merges, all clean, zero file overlap, confirmed by `git merge-tree` before merging). PR SAM-BIM/SAM#73's
`test (Release)` check failed with `Assert.NotNull() Failure: Value is null` on
`TM59AssessmentCalculatorTests`, `TMOverheatingCalculatorTests`, `VentilationStrategyTests`,
`PartOResultAssociationTests` and `PartOIterationSliceTests` - none of which reproduced locally, in either
Debug or Release.

**Root cause.** `TMOverheatingCalculator.Calculate_TM59` refuses (returns null) when its `TextMap` is null.
None of the five files set `TextMap` explicitly, so all five rely on
`Query.DefaultInternalConditionTextMap_TM59()`, which checks `ActiveSetting.Setting` first and falls back to
`Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly())` - which resolves a directory from
**`Assembly.CodeBase`, an API obsolete since .NET 5** and not reliably populated under the `dotnet test` host.
On a developer machine with a real SAM install this is masked entirely: a persisted `%APPDATA%\SAM\settings`
file satisfies `ActiveSetting.Setting` before either fallback is ever reached - exactly why it passed on
every machine that had ever run a real SAM install, and only failed on a clean CI runner.

**Proven, not assumed.** This machine's own `%APPDATA%\SAM\settings` was moved aside; all 73 affected tests
then failed exactly as CI did; the fix below was applied; all 73 passed with the settings file still absent;
the file was restored and the full suite re-run (**1207 passed, both Debug and Release**) to confirm no
regression with it back in place.

**Fix.** `TM59TextMapModuleInitializer` (`[ModuleInitializer]`) seeds `SAM.Analytical.ActiveSetting.Setting`
once per process, reading the TM59 TextMap resource from **`AppContext.BaseDirectory`** - the one path .NET
Core guarantees is populated and correct under every hosting scenario, which `Assembly.CodeBase` is not.
`SAM.Tests.csproj` copies the resource locally, mirroring the pattern `SAM_Tas`'s own TM59 test project
already uses for the identical reason (see that project's `TESTING.md`). Fixed **once for the whole
assembly** rather than in five separate test files, since none of them set `TextMap` themselves - they were
written expecting the production default, and that is what this seeds.

**A SECOND, genuinely different failure followed — and it was a production defect, not a test problem.**
`5565d96c` let the TM59 tests reach real logic instead of refusing early on a null `TextMap`, and they then
threw `NullReferenceException` from `TMOverheatingCalculator.SystemTypeName`. That method dereferenced
`Query.DefaultSystemTypeLibrary()` in two loops **without a guard**, and that library also comes from
`ActiveSetting` - so on a clean runner the legacy ventilation-strategy derivation threw out of the middle of
an assessment instead of falling through to the `"NV"` default it documents for "nothing says otherwise".
Fixed in `0e33ed27` by skipping the zone-name lookup when the library is absent, which reaches that same
default. Behaviour is unchanged wherever a library exists. `DefaultSystemTypeLibrary()` has exactly one other
mention in the codebase - `VentilationStrategyMap`'s documentation, which already states it "can be absent"
and does not call it - so this was the only instance.

**The pattern behind both failures is the same, and it is worth internalising:** anything reached through
`ActiveSetting` is null on a machine without a populated `%APPDATA%\SAM`, and *every* developer machine has
one. Two separate defects hid behind that for as long as the code was only ever run locally. Verified both
fixes by moving this machine's own settings file aside and running the full Release suite with it absent
(**1207 passed, 0 failed**), then restoring it.

**A THIRD round followed, because my verification was wrong — and that is the most useful thing in this
section.** After the two fixes above, CI failed again on a *different* set: `PartFAirflowApplicationTests` (7,
needing `PartFData`) and `VentilationStrategyTests` (4, needing `DefaultSystemTypeLibrary`). Those were the
very tests flagged as at-risk when the null-library guard went in, and then wrongly cleared.

**Why the verification was worthless.** "Clean environment" was simulated by moving `%APPDATA%\SAM\settings`
aside — but `%APPDATA%\SAM\resources` was left in place, so `ActiveSetting.GetDefault()` still loaded every
library from disk and the suite passed. It proved nothing about a runner with no SAM install.

**The real shape of the problem.** This was never one missing resource. **Every** library the suite reaches
through `ActiveSetting` is absent on a clean runner, because `GetDefault()` finds its resources via
`Core.Query.ResourcesDirectory` → `Assembly.CodeBase`. Fixed properly in `0fa247fd`:
`SAMResourcesModuleInitializer` seeds `ActiveSetting` from **this repository's own** resource files, copied
into the test output and read through `AppContext.BaseDirectory`, in `GetDefault()`'s own dependency order
(`PartFData` last — it resolves its space-use vocabulary against the SpaceUse text map). Seeded
unconditionally from the repo rather than the install, so a test run means the same thing on a laptop and on
CI.

**How to verify this class of thing properly — use this, not a file move.** Point the whole profile at an
empty directory, so no SAM install is visible at all:

```powershell
$iso = Join-Path $env:TEMP ("sam_clean_" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force (Join-Path $iso "AppData\Roaming") | Out-Null
$env:APPDATA = Join-Path $iso "AppData\Roaming"; $env:LOCALAPPDATA = Join-Path $iso "AppData\Local"; $env:USERPROFILE = $iso
Test-Path (Join-Path $env:APPDATA "SAM")   # must print False
dotnet test SAM\SAM\SAM.Tests\SAM.Tests.csproj -c Release --no-build
```

Under that, the suite passes **1207/1207**; with the real profile restored it passes 1207/1207 in both Debug
and Release. **Also run Release, not just Debug** — CI runs Release only, and the first two rounds were
Debug-only locally.

**If a new test needs another `ActiveSetting` default**, copy its resource in `SAM.Tests.csproj` *and* seed it
in `SAMResourcesModuleInitializer`. Do not assume a clean runner behaves like a developer machine.

**Worth remembering:** `Assembly.CodeBase`-based path discovery (`Core.Query.ResourcesDirectory`,
`Query.DefaultPath`, `Core.Query.ExecutingAssemblyDirectory`) is fragile under any hosting scenario other
than a real installed SAM. Any new test that instantiates something depending on an `ActiveSetting` default
should either set the dependency explicitly or add itself to this initializer's coverage - do not assume a
clean CI runner behaves like a developer machine with `%APPDATA%\SAM` populated.

### 11s. Codex/Copilot review findings on all five open PRs (SAM + SAM_Systems + SAM_Tas DONE, two repos left)

**Read this section first if resuming mid-review.** Once all five PRs (§0a) went CLEAN, `chatgpt-codex-connector`
left inline findings on all five. Per the standing rule (§10, "address every valid finding regardless of
authorship — never skip one as pre-existing/not my work"), every finding across all five repos needs
verifying against the real code and fixing if valid — **most of what Codex found predates this session's work
entirely** (the Part F correction pass, step 5/7/8, Solver2D hardening), and that is not a reason to skip it.

**SAM (`#73`) is DONE — 9 of 12 fixed in `8de00cc7`, 1 reverted, 2 carried forward. Full detail in that commit
message; the two open items:**

1. **`PartFAirflowNetwork.Solve` — REVERTED, needs fresh thinking, not a retry of the same approach.** Codex's
   finding is real in the abstract (an imbalanced component's supply gets fully apportioned across demand
   that doesn't match it), but the obvious fix — refuse a component whose net flow isn't ~zero — **broke
   `HighRateTransfer_IsSolvedSeparatelyFromTheContinuousCondition`**, an existing, intentional regression
   test. That failure is evidence, not an obstacle to work around: supply and extract are allocated
   **dwelling-wide**, while a "component" here is a physically disconnected sub-graph connected by doors, so
   an individual component legitimately does not have to sum to zero on its own — the proportional
   apportionment already handles that reasonably. **Do not require per-component balance.** If there is a
   real fix here, it has to distinguish "a component that doesn't balance because dwelling-wide allocation
   doesn't split per sub-graph" (normal) from whatever genuinely pathological shape Codex's `+20/-10` example
   was gesturing at — and it isn't obvious those are even distinguishable from inside `Solve` without more
   context than it currently has. Re-derive from scratch; do not just loosen the tolerance on the reverted
   approach.
2. **`PartFSchematic.cs:266` (`AppendBranches`) — confirmed real, not fixed.** The branch filter only follows
   `x.UpstreamSpaceGuid == guid && Rate(x, mode) > Tolerance`. At `HighBoost`, a transfer record's direction
   can reverse relative to how it was oriented at the continuous condition (per-room high-rate minima
   distribute differently from the continuous volume-weighted split), giving `Rate(x, HighBoost) < 0` — the
   filter drops it, and nothing checks the OTHER direction (`x.DownstreamSpaceGuid == guid && Rate(x, mode) <
   -Tolerance`, meaning flow now runs the other way), so the route silently vanishes from the high-rate
   schematic. **The fix needs to gather candidates from both directions and continue the recursion to whichever
   end is now downstream**, which touches the branch-list construction, `RouteText`, and the visited-set
   bookkeping together — under-scoped to rush. Visualization only (does not affect any PASS/FAIL verdict), so
   lower urgency than a wrong compliance answer, but still a real defect per the standing rule.

**SAM_Systems (`#14`) is DONE — `4446bb8`.** `SystemCapabilityDescriptors` applied the `Application`
eligibility filter with a `continue` as soon as it had read the entry's application, which skipped the
ventilation-text check, `SystemTemplate.IsValid` and the `Rank` refusal along with the entry. So the SAME
half-edited catalogue was accepted for one application and refused for another. **The entry it reopened the
hole on is the exact one the reader's own `Rank` comment records as the original defect**: a case typo on
VAV's `"Rank"`. VAV is `Commercial`, so after eligibility became a filter that typo refused an `Any` or
`Commercial` request and was silently dropped from a `Domestic` one — and Approved Document F selection is
precisely the `Domestic` caller. The existing `MalformedIndex_ProducesNoDescriptors` breaks EOL, a *domestic*
entry, so it kept passing throughout. Eligibility now filters the survivors: `IsEligible` is the last thing
before `result.Add`. Tests: `BrokenEntryOutsideTheRequestedApplication_StillRefusesTheIndex` (VAV broken five
ways × all four request kinds; all five cases fail without the reordering) and
`ValidatingEveryEntry_DoesNotChangeWhatIsOffered` (6 domestic / 4 commercial unchanged, so the fix was not
bought by leaking commercial templates into a dwelling answer). 40/40 + 123/123 Release.

**SAM_Tas (`#29`) is DONE — `332736c`.** Both findings valid and fixed.

1. `ToTM59` guarded `spaces == null` while its own refusal message said "there is no model, **or it holds no
   spaces**". An empty list fell through, the loop had nothing to visit, nothing was refused, the completeness
   gate saw a clean run, and `ToXml` wrote a **zero-zone document and returned true**. Reachability had to be
   established rather than assumed, and it holds: an untouched `AdjacencyCluster` has no `Space` type
   registered so `GetSpaces()` returns *null* (the half that already refused), but `RemoveObject` deletes the
   guid and **leaves the type's bucket**, so a model whose spaces were all deleted returns an *empty* list.
2. `ApproximateResultantTemperatureMap` — the comment on the cast claimed to guard "a null or non-numeric
   element" and guarded only the null; a string or bool threw `InvalidOperationException` out of a Grasshopper
   component and **aborted the whole preparation map** instead of excluding the one space.

   **The fix is deliberately NOT just `TryGetValue<double>`, and this was measured, not assumed.** A
   `JsonValue` built in-process from an `int`, `float` or `decimal` reports `JsonValueKind.Number` and still
   fails BOTH `TryGetValue<double>` and `GetValue<double>` — the framework converts numeric types only for
   `JsonElement`-backed values, which is what *parsing* produces. A parsed `5` reads as `5.0`; an in-process
   `5` throws. So a strict guard would have turned a legitimate whole-number series into a refusal. `TryGetDouble`
   decides on numeric *kind*, then reads, falling back to the node's JSON text parsed **invariantly** (a decimal
   comma would otherwise read "20.5" as 205). `AWholeNumberRadiantValue_IsRead` pins that half.

   70/70 Release. All five new tests were first run against the pre-fix binary and failed there exactly as
   reported.

> **GOTCHA that cost a full wrong test run, worth reading before touching SAM_Tas tests.**
> `SAM.Analytical.Tas.TM59.Tests` references **prebuilt DLLs from `build/`** on purpose — a `ProjectReference`
> would drag in the TAS COM interop. So `dotnet test` alone **silently tests the OLD code**: the first run
> "failed" with the pre-fix message text and pre-fix stack traces while the source was already fixed. Build
> `SAM.Analytical.Tas.TPD` and `SAM.Analytical.Tas.TM59` **with VS MSBuild first** — `dotnet build` fails both
> with `MSB4803` on `ResolveComReference`:
> ```bash
> "/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" SAM_Tas/SAM.Analytical.Tas.TPD/SAM.Analytical.Tas.TPD.csproj -p:Configuration=Release -v:m -nologo
> ```
> (Silver lining: that accidental run is the cleanest possible proof the tests catch the defects.)

**STILL TO DO — `SAM_Tas_Grasshopper` (`#4`, 1 finding) and `SAM_UI` (`#75`, 3 findings, two P1).** Verbatim
below so no re-query is needed; re-fetch is one command per repo if wanted:

```bash
gh api "repos/SAM-BIM/<repo>/pulls/<number>/comments" --jq '.[] | "path=\(.path) line=\(.line // .original_line)\n\(.body)\n===="'
```
(SAM_Tas_Grasshopper #4, SAM_UI #75.)

- **SAM_Tas_Grasshopper** — `Grasshopper/SAM.Analytical.Grasshopper.Tas/Component/SAMAnalyticalCreateTBDByTM59.cs:234`
  (P2): rerunning at a path with an existing TM59 companion file, when the new scenario map is incomplete,
  skips `ToXml` but leaves the stale XML in place **after rewriting the TBD** — so TAS opens ventilation
  strategies that do not correspond to the current scenarios. Codex's suggestion: delete or invalidate the
  existing companion XML on refusal, or validate the scenario map *before* producing the TBD. **Note the
  interaction with the `SAM_Tas` fix just landed**: `ToXml` now also refuses an empty space list, so there is
  one more refusal path that must not leave stale XML behind.
- **SAM_UI** — three findings, two P1:
  - `WPF/SAM.Analytical.UI.WPF/Windows/PartFAssessmentWindow.xaml.cs:345` (**P1**) — switching dwellings in a
    multi-dwelling assessment discards unsaved edits, because `Load()` clears and recreates all three row
    collections without calling their `Apply()` first, and `Button_OK_Click` applies only the CURRENTLY
    selected dwelling's rows. Commit the current grids before loading the next dwelling, or retain per-dwelling
    row state.
  - `WPF/SAM.Analytical.UI.WPF/Modify/AddVentilationByPartF.cs:193` (**P1**) — unchecking a previously
    confirmed check restores its calculated final status via `PartFCheckRow.Apply()`, but the record still
    reaches `PersistConfirmations`, which unconditionally writes `UserConfirmed` back into the commissioning
    data. The next calculation therefore reinstates a confirmation the user explicitly withdrew. Reset or
    remove the persisted confirmation status **while retaining any supporting notes**.
  - `WPF/SAM.Analytical.UI.WPF/Windows/PartFAirflowViewSettingsWindow.xaml.cs:86` (P2) — the loop offers every
    model zone as a selectable dwelling regardless of the chosen assessment scope; saving one sets
    `SelectedDwelling`, but the renderer filters only the results calculated for the selected category, so the
    saved view **draws nothing**. Rebuild the list from the dwelling zones belonging to the selected scope, and
    update it when that scope changes.

**`SAM_UI` is the WPF repo, so expect the two P1s to need a real look at row lifecycle rather than a local
patch** — and note there is no headless test for a WPF window, so check what `SAM.Analytical.UI.WPF.Tests`
already covers (it has a `Helpers/PartFPlanModel.cs`, so some of this is testable without a window) before
concluding a fix cannot be regression-tested.

**Process for the next session, matching what worked this session:** verify each finding by reading the
actual current code (don't take Codex's claim on trust — one SAM finding's example numbers didn't match
reality, though its conclusion still held); implement the fix; add or extend a regression test in that repo's
existing test project where one exists; **run the full affected test suite in Release before considering a
fix done** — the SAM airflow-network revert exists precisely because a fix that looked right broke a real,
passing test; commit with SPDX; push to the SAME branch (`feature/partf-terminal-transfer-compliance`), which
updates the existing open PR automatically — no new PR needed; re-check that repo's CI goes green; update this
section.

## 10. Standing instructions

- **Two-laptop continuity rule, mandatory.** This file is the authoritative continuation state. After
  every completed checkpoint, update it **in the same working session**, then commit and push it. Never
  finish a checkpoint with the handover behind the repositories, and never leave important reasoning
  only in a chat, a terminal history or an unpushed local commit. Before starting work in a new session,
  read section 0 and verify every repo against the recorded branch/SHA/remote state; **if the actual
  state differs, stop and reconcile before changing code.**
- **Working sequence:** implement → test → commit → push → independent review/fixes → test →
  commit/push → update this handover → commit/push handover → continue. Never force-push; never squash
  or rebase published commits; do not open a PR unless asked.
- Work on `feature/partf-terminal-transfer-compliance` in **all five** repos — SAM_Tas's,
  SAM_Systems's and SAM_Tas_Grasshopper's are new and need PRs like the others. **Never commit to
  `sow/2026-Q3` directly.**
- SAM first (SAM_UI and SAM_Tas CI dep-clone it), PRs against `sow/2026-Q3`, CI green **and** the Codex
  inline comments read.
- Attribution line `Generated by Michal Dengusiak and CodeClaude` on every commit and PR body.
- SPDX header on every changed `.cs`.
- Use the SAM implementation-summary style for the final response.

### 11t. The §11o identity defect — root cause and the Option B repair (DONE and confirmed live, SAM_Tas `d76be13`)

**What was actually wrong.** `Convert.ToSAM(TSD.ZoneData)` never stamped `SpaceParameter.ZoneGuid`. It left
it to the generic `Create.ParameterSet_Space` → `SAM.Core.Create.ParameterSet` → `TypeMap` path. The mapping
**is** registered — `SAM.Analytical.Tas/Manager/ActiveSetting.cs:167`:

```csharp
typeMap.Add(SpaceParameter.ZoneGuid, typeof(TSD.ZoneData), "zoneGUID");
```

but it cannot fire, and the reason is exact. `TypeMap.Add(Enum, Type, string)` stores the enum's
**`ParameterProperties` name**, and `SpaceParameter.ZoneGuid` is declared
`[ParameterProperties("Zone Guid", "Zone Guid")]` — **with a space**. The tuple is therefore
`(Space, ZoneData, "Zone Guid", "zoneGUID")`. `SAM.Core.Create.ParameterSet` then does, per name:

```csharp
string name_destination = typeMap.GetName(typeName_1, typeName_2, name);   // "zoneGUID"
if (!Query.TryGetValue(@object /* the ZoneData */, name /* "Zone Guid" */, out value))
    continue;                                                              // always taken
result.Add(name_destination, value as dynamic);                            // would store under "zoneGUID"
```

It reads the property off the **TSD object** using the **SAM-side** name, and would store the result under
the **TAS-side** name — inverted in both directions. `TSD.ZoneData` has no member called `"Zone Guid"`, and
the space defeats even a case-insensitive match (`"ZoneGuid"` vs `"zoneGUID"` would have collided
case-insensitively; `"Zone Guid"` cannot). So the parameter is silently never added, and
`Query.SimulationSpaceKey` returns null for every simulated space. **This is a real defect in the shared
helper, not a missing registration** — see §0e, where it is deferred as its own infrastructure task.

**Why `zoneData.zoneGUID` is the correct source and no linked-TBD lookup was invented.** Michal's domain
statement — a TSD is linked back to its TBD, and the stable zone guid lives on the TAS zone in the TBD — is
corroborated by the code, and the codebase **already depends on the equality in production**:

| Side | Built at | From |
|---|---|---|
| TSD | `Convert/ToSAM/PanelSimulationResults.cs:19` | `new ZoneSurfaceReference(surfaceData.surfaceNumber, zoneData.zoneGUID)` |
| TBD | `Modify/UpdateIds.cs:88` | `new ZoneSurfaceReference(zoneSurface.number, zone.GUID)` |

and `Modify/AddResults.cs:120` equates the two (`zoneGuid.Equals(zoneSurfaceReference.ZoneGuid)`, where
`zoneGuid` is the design space's `ZoneGuid`) to attach surface results to panels. That long-standing path
only works if TSD `zoneData.zoneGUID` **is** the TBD `zone.GUID`. The link is also explicit:
`Convert.ToSAM_AnalyticalModel` stamps `AnalyticalModelParameter.Path_TBD` from
`SimulationData.buildingPath` (`Convert/ToSAM/AnalyticalModel.cs:108`). **The name is not used as identity
anywhere in this fix** — it remains only `SimulationSpaceMap`'s documented fallback.

**The design side is still name-assigned, and that has NOT changed.** `Modify.UpdateIds` still strips
`ZoneGuid` from every space (line 22) before the loop that would read it (line 56), so the guid lookup always
misses and the TBD zone is found by **name** (line 59) before `zone.GUID` is stamped (line 65). The
`zoneGuidProvenance: "assignedDuringWorkflow"` note the logger emits therefore remains accurate. Option B
repairs the *simulated* side only; the design side's name-match provenance is a separate decision Michal
still owes (preserve the guid through the strip, or refuse an ambiguous name match).

**The fix.** One guarded stamp, mirroring the TBD converter in the same file:

```csharp
if (!string.IsNullOrWhiteSpace(zoneData.zoneGUID))
{
    space.SetValue(SpaceParameter.ZoneGuid, zoneData.zoneGUID);
}
```

Blank stays blank — a zone with no stated guid carries no identity and still resolves through the
unique-name fallback. No fallback name and no synthetic guid is invented. Nothing in `SAM.Core`,
`TypeMap`, `ActiveSetting`, Part F sizing, Part O preparation, the TM59 criteria, `SimulationSpaceMap`,
`OverheatingScenarioMap` or the diagnostic logger was touched; the diff is that one file plus a new test file.

**The one way this can make things worse, and it is worth understanding before the acceptance run.**
`SimulationSpaceMap` refuses a **stated** key that matches no design space as `Unresolved` — it deliberately
does *not* fall through to the name (`SimulationSpaceMap.cs:88-96`, and the reasoning there is sound: a
stated-but-disagreeing key means a different simulation batch). So if the two guid strings ever differ in
**format** — braces, casing — the live run flips from "9 resolved by unique name" to "9 unresolved", which is
worse than today. The static evidence above says they are the same string; it could not be executed here,
because that needs TAS COM. **The failure is loud, not silent**, and `05155e6` already logs
`designZoneGuidRaw` and `simulatedZoneGuidRaw` separately and uncoalesced, so one look at the log
distinguishes "matched" from "both populated but different" from "simulated still blank".

**Tests — `SAM.Analytical.Tas.TM59.Tests/TsdZoneIdentityStampTests.cs`, 5 new, 89/89 in Debug and Release.**
The stamp is what `Query.SimulationSpaceKey` reads back; a stamped simulated space resolves by key **even
when renamed**; two rooms named exactly `"Bedroom 2"` resolve by guid where their unstamped equivalents are
refused — the case the unique-name fallback structurally cannot serve, and the reason "the fallback was
succeeding" was never good enough; an absent guid still resolves by unique name; a blank guid is not a stated
identity.

**What these tests do NOT do, stated plainly: they do not call the converter, so they will not fail if the
stamp is reverted.** `TSD.ZoneData` is COM interop. It *is* an interface (41 members) that a managed fake
could implement, but `Convert.ToSAM` reaches `ActiveSetting.Setting`, whose `GetDefault()` references
`typeof(TAS3D.Zone)`, `typeof(TBD.zone)` and `typeof(TSD.ZoneData)` — so exercising the converter needs the
whole TAS interop surface loaded inside a `net8.0` project built specifically to exclude it (see that
project's own csproj comment). That was judged not worth breaking the COM-free boundary for. **The converter
line itself is covered by the manual acceptance run below, and by nothing else.**

**Manual acceptance — DONE, and every criterion passed.** Michal reran the existing Flat1 BasePassive
Grasshopper test after `%APPDATA%\SAM` picked up the fix (refreshed by a Debug `SAM_Tas_Grasshopper.sln`
build in the implementing session, whose post-build copies `$(TargetDir)*.dll` there — without that the
canvas would have exercised the old assembly and reported `uniqueName`, looking exactly like a failed fix).
Evidence is `Flat1.BasePassive.partO.20260818-122737.jsonl` (run `c4aa16f2-990c-41b6-ae8f-c78eedd14294`,
written 2026-08-18 14:27 local / 12:27:37 UTC, i.e. after the rebuild):

- `simulatedZoneGuidRaw` populated for all 9 spaces, and equal to `designZoneGuidRaw` on every one — same
  string, braces and casing included (e.g. `{04860158-BDB1-4295-9EC6-DE80B409774D}` both sides), so the
  format-mismatch risk above did not occur;
- `identityMode: "zoneGuid"` for all 9;
- `simulationSpaceMapIsComplete: true`, `overheatingScenarioMapIsComplete: true`, `unassociatedCount: 0`;
- `workflowSuccessful: true`, `tM59Successful: true`;
- TM59 results unchanged from the pre-fix baseline: 8 spaces pass, `Corridor_1` fails with
  `hoursExceeding28: 337` against `maxExceedableHours: 262` — the same fail shape recorded before this fix.

Checked directly against the log, not just the run's own summary line — the per-space records
(`designSpaceName`/`simulatedSpaceName`/`designZoneGuidRaw`/`simulatedZoneGuidRaw`/`identityMode`) were read
one by one, not only the aggregate `run` record.

**Related, still open and NOT addressed here:** the TPD route's identity match (11o review finding 1) — the
TPD result reference and the TBD/TSD zone guid have still never been shown to be the same string. The
evidence gathered here is about **TSD** only.

### 11u. First SAM BasePassive vs native TAS comparison (Flat1, DONE for this one run — SAM_Tas `2750a21`)

**What this is.** With Option B closed, 0f's next task was to prove the actual TM59 numbers match TAS, not
just that the identity chain resolves. Michal reran Flat1 BasePassive in TAS itself and pulled the native
"Domestic Overheating (CIBSE TM59)" report (`Report XMLs\Domestic Overheating (CIBSE TM59).xlsx`) for the
same `.tsd` the matching SAM diagnostic-log run (`Flat1.BasePassive.partO.20260818-134420.jsonl`) was built
from. Both were compared space by space.

**Result: 8 of 9 spaces agree with TAS on every number that governs pass/fail.**

| Space | Criterion | TAS | SAM | Agreement |
|---|---|---|---|---|
| Bedroom 2_3, Bedroom 2_6 | mechanical | occ./max/exceeding-26/Pass | identical | exact |
| Kitchen_4, Kitchen_7 | mechanical | occ./max/exceeding-26/Pass | identical | exact |
| Studio 1_0 | naturalBedroom | day 37, night 3285/32/11, Pass | identical on all four | exact (see log gap below) |
| Ensuite_5, Ensuite_8 | corridor | 0/0/0, Pass | 0 hours >28°C, Pass | exact |
| Bathroom_2 | corridor | 0/0/0, Pass | 2 hours >28°C, Pass | investigated, not a defect (below) |
| Corridor_1 | corridor | 0/0/0, Pass (TAS report) | 337 vs 262, **Fail** | **by design**, not a new finding |

`Corridor_1`'s disagreement is the one 11t's own acceptance criteria already named — SAM deliberately runs
its own communal-corridor overheating criterion (0d point 10), which TAS's canned report template has no
equivalent for: a zero-occupancy circulation space is simply bucketed "Other" and trivially reported as
0/0/0/Pass there.

**Finding 1 — a real gap, fixed in `2750a21`.** `Studio 1_0`'s actual TM59 numbers (day exceedance 37, night
3285/32/11, Pass) matched TAS exactly, but the log's `occupiedHours`/`maxExceedableHours` showed `8760`/`262`
— the whole-year annual pair also carried on `TM59NaturalVentilationResult` — while TAS's report states the
day criterion against `3672`/`110` ("Occupied Summer Hours" / "Max. Exceedable Hours"). The correct pair
(`SummerOccupiedHours`/`MaxExceedableSummerHours`) was on the result object all along and is exactly what
`Query.Simplify` (`SAM.Analytical`) already reads when building a plain result from an extended one — this
log simply never read it. Not a TM59 defect: the assessment used the right basis throughout, only the log
displayed a misleading pair alongside it. `PartODiagnosticLog.SetCriterionSpecificFields` now logs
`summerOccupiedHours`/`maxExceedableSummerHours` for `natural`/`naturalBedroom` rows (plain branch: direct
properties; extended branch: `GetSummerOccupiedHours()`/`GetSummerMaxExceedableHours()`, the same methods
`Query.Simplify` uses). 2 new tests in `PartODiagnosticLogTests.cs`; 91/91 in Debug and Release.

**Finding 2 — investigated, confirmed NOT a defect, no code change.** `Bathroom_2` (0 occupied hours, an
ancillary room) logged 2 hours exceeding 28°C where TAS's report showed 0. Traced to
`TM59CorridorExtendedResult.GetHoursNumberExceeding28()` (`SAM.Analytical`), which scans the resultant
temperature over the **whole 8760-hour year**, not gated on occupancy — confirmed deliberate by
`TM59CorridorExtendedResult.MaxExceedableHours` being overridden to `(wholeYearHourCount) × 3% ≈ 262`, not
`OccupiedHours × 3%` the way the base type computes it for occupied criteria. TAS's own canned report
evidently skips its check entirely for a room it classifies "Other" (reporting 0/0/0 as "not run", not "0
measured") — SAM_Tas has no access to TAS's report-generation logic to confirm this beyond the pattern
itself. Also confirmed in source: **"corridor" is the catch-all in `TMOverheatingCalculator.Calculate_TM59`**
— any space whose `TM59Manager.TM59SpaceApplications` comes back empty (not matched as sleeping/living/
cooking) routes here, not only actual corridors; a bathroom qualifies the same way an actual corridor does.
Nothing changed: SAM's occupancy-independent ancillary-space check is intentional and arguably the more
conservative behaviour, not a bug to match TAS's silence on.

**Not yet done — 0f is explicit about the remaining gap.** This is one flat, one iteration
(`BasePassive`), one `.tsd`. It exercises `mechanical`, `naturalBedroom` and `corridor` but not plain
`natural` (non-bedroom), and covers none of `AcousticRestricted`/`ActiveTrimCooling`. A second model and the
other iterations are still needed before "SAM BasePassive matches native TAS" can be claimed generally
rather than for this one run.

### 11v. TM59 verification report — implementation, GUID baseline fix, and Iteration 1 report validation

**What this is.** A new `TM59AssessmentReport`/`TM59AssessmentReportFormatter` layer over
`TM59AssessmentResult` (SAM `07d1c728`, `3a386baf`, `3fcfb880`), exposed as `Tas.TSDQueryTM59Results`'s new
`report` output (SAM_Tas_Grasshopper `2bee815`), plus an unrelated CI fix found in passing
(SAM_Tas_Grasshopper `29c31db`). Then a validation pass compared the actual `report` text against the same
real Flat1 BasePassive run 11u already validated, using the real evidence files directly rather than this
handover's own summary of them — and found and fixed one further defect in the report itself.

**Three SAM commits:**

- `07d1c728` — a real, independently-discovered `Query.Simplify` bug: the non-bedroom natural-ventilation
  branch passed the summer-hours pair and the annual limit rotated by one constructor position, so every
  simplified (non-extended) result reported `SummerOccupiedHours`/`MaxExceedableSummerHours` under swapped
  meanings. Pass/Fail was unaffected (derived from `HoursExceedingComfortRange` directly). Isolated with its
  own regression test, ahead of the reporting work and in its own commit on purpose.
- `3a386baf` — the report itself: `TM59ComplianceStatus` (Pass/Fail/NotApplicable) and `TM59RiskStatus`
  (Acceptable/SignificantRisk) as separate concepts, Criterion 1/2 stated separately, mechanical results,
  Actual/Limit/Margin, unassessed spaces, and a legend. `TM59NaturalVentilationBedroomResult` gained
  `Criterion2` (Pass on a TM59 result is Criterion 1 alone; without this a simplified bedroom result carried
  the night-time numbers but no verdict to report against them).
- `3fcfb880` — **found during the validation pass below, not before it.** Criterion 1's Limit read the base
  type's annual `MaxExceedableHours`, not `MaxExceedableSummerHours` — the basis TAS's own report actually
  states this criterion on, paired with "Occupied Summer Hours". For `Studio 1_0` this showed `262` where
  TAS's real figure is `110`. Existing tests missed it because every one of them happened to use the same
  number (110) for both the annual and summer test values, so the wrong field and the right one were
  indistinguishable by coincidence. Fixed; the natural-ventilation test fixtures now use a deliberately
  wrong-looking annual placeholder (`999`) so that coincidence cannot recur, and a new test pins the real
  Studio 1_0 figures directly. Mutation-tested: reverting the fix fails 5 tests, including the new one.

**One SAM_Tas_Grasshopper commit, unrelated to the report:** `29c31db` registers
`TasLogPartODiagnostics`'s ComponentGuid (`fb234a4c-...`, added in `32f2763` without a baseline entry) in
`.github/scripts/guid-baseline.json` — the "Check TAS Grasshopper GUID identity" CI job had been failing on
every push to this branch since `32f2763`, unrelated to any Part F/O/TM59 code. Regenerated via the script's
own `-UpdateBaseline` and diffed to confirm it changed exactly one line, at its correct sorted position, with
nothing else touched. Windows CI is green again.

**The ancillary-vs-corridor limitation, restated because it shapes the report's wording.**
`TMOverheatingCalculator.Calculate_TM59` buckets a space into `CorridorResults` when it has no TM59 space
application at all, **or** when its stated strategy is `"UV"` — which `SAM_Systems`' own capability index
describes as "Unconditioned. Provides nothing," not "communal corridor." Nothing on the resulting
`TM59CorridorResult` records which reason applied. `Space`, `InternalCondition`, `TM59Manager` and
`TM59SpaceClassification` were all checked for an authoritative distinction; none exists. The report names
the section `FULL-YEAR >28 C / CORRIDOR-STYLE RESULTS` (never "COMMUNAL CORRIDORS"), prints a caveat beside
the table, and every row is `ComplianceStatus.NotApplicable` — the report never claims a bathroom, hall or
ensuite is a confirmed communal corridor.

**Iteration 1 BasePassive report validation — CLOSED, evidence gathered from source.** The real files
(`Flat1.BasePassive.partO.20260818-134420.jsonl` and TAS's own `Domestic Overheating (CIBSE TM59).xlsx`, both
in `...\SAM_daily\2026-07-15 PartO\Simulation\`) were parsed directly — the xlsx via its raw OOXML, the JSONL
via its own space records — and the actual `TM59AssessmentReport` text was built from the real numbers.

**All applicable occupied-space TM59 criteria in the BasePassive case match the TAS native report exactly:**

| Space | Check | TAS | `report` (post-`3fcfb880`) |
|---|---|---|---|
| Studio 1_0 | Criterion 1 | 37 / 110 | 37 / 110, margin +73 |
| Studio 1_0 | Criterion 2 | 11 / 32 | 11 / 32, margin +21 |
| Bedroom 2_3 | >26 C | 131 / 262 | 131 / 262, margin +131 |
| Kitchen_4 | >26 C | 135 / 142 | 135 / 142, margin +7 |
| Bedroom 2_6 | >26 C | 129 / 262 | 129 / 262, margin +133 |
| Kitchen_7 | >26 C | 129 / 142 | 129 / 142, margin +13 |

**Bathroom/ensuite/corridor-style differences are the already-investigated assessment-scope/classification
distinction, not a new discrepancy:** TAS reports `Bathroom_2` (2/262), `Ensuite_5` and `Ensuite_8` (0/262
each) under whichever ventilation-type table matches their system, as "Other" with no equivalent criterion
applied. SAM's existing full-year >28 C bucket evaluates them independently of that table placement, and
produces the same real hour counts either way. For `Corridor_1` (337/262, margin −75): this exceeds the
threshold, so the report states `RiskStatus = SignificantRisk` — this does **not** fail the occupied-space
TM59 assessment, and the current domain cannot prove that every result in this bucket is a physical communal
corridor (the paragraph above explains why). `OCCUPIED SPACE ASSESSMENT: PASS` throughout, including with
`Corridor_1` at `SignificantRisk` — confirmed both in the rendered text and by
`ASignificantRiskCorridor_DoesNotFailTheOccupiedSpaceAssessment`.

**Wording discipline, on purpose.** The report states `TM59 occupied-space assessment: PASS`, never "Part O
compliant" — passing temperatures do not establish that every Approved Document O modelling assumption behind
the simulation was applied, and BasePassive report validation has not proved that either.

**Association was complete on this run.** All 9 assessment spaces produced a result: `unassociatedCount: 0`,
`refusalCount: 0` on the real run record. The report's "SPACES NOT ASSESSED" section correctly states "Every
space the assessment covered produced a result" — nothing was silently dropped. (The mechanism for a real gap
— a space present but unresolved — is separately unit-tested, since this run does not exercise it live.)

**Validation counts:** `SAM.Tests` **1230/1230** passed (Release, was 1207 at PR-open, 1229 before this
session's fix). `SAM.Analytical.Tas.TM59.Tests` **91/91** passed (SAM_Tas, unaffected). `SAM.Analytical`,
`SAM_Tas.sln` and `SAM.Analytical.Grasshopper.Tas` all build with 0 errors.

**What this closes, and what it explicitly does not.** The `report` output is now verified against the real
Flat1 BasePassive TAS comparison and matches on every number, with one real defect found and fixed in the
process — **Iteration 1 — BasePassive report validation: CLOSED.** This is narrower than "Iteration 1 testing
generally": 11u's still-open gap stands exactly as it did — one flat, one iteration, no plain non-bedroom
`natural` space exercised, no second model tried. Widening the comparison, and Iterations 2/3, remain future
work and were **not** started in this pass.

---

## Paste this into the new session

Continue the Approved Document F / Part O work in the SAM-BIM workspace.

**First: read `SAM/documentation/PartF-HANDOVER.md` in full** - especially **section 11**, which is the
Iteration 0/1 state, the architecture reconnaissance, and the follow-ups deliberately not fixed. It also
holds the decisions that must not be silently revisited and the environment gotchas.

Then in **all five** repos - SAM, **SAM_Systems**, SAM_UI, SAM_Tas and **SAM_Tas_Grasshopper** - run the
two verification commands in **section 0a**, which check clean/level, descent from the recorded last-code
SHA, and that nothing but the handover changed since it. The second must print exactly **five**
`descends from` lines and nothing else.

Confirm each is on `feature/partf-terminal-transfer-compliance`, the tree is **clean**, and local matches
remote at the SHAs in **section 0a**, which is authoritative. Nothing should be outstanding.

### YOUR ACTUAL NEXT TASK — widen the SAM-vs-native-TAS comparison (Iteration 1), then Iterations 2/3

**Where this stands.** The §11o identity defect is fixed and independently confirmed live (11t, SAM_Tas
`d76be13` — Option B, CLOSED). A first SAM-vs-native-TAS comparison then ran on that same fixed identity
chain (11u, SAM_Tas `2750a21`): Flat1 BasePassive, one `.tsd`, compared space-by-space against TAS's own
"Domestic Overheating (CIBSE TM59)" report. **8 of 9 spaces agreed with TAS on every number that governs
pass/fail; the 9th (`Corridor_1`) disagrees by design**, not a defect (SAM runs its own communal-corridor
criterion TAS's canned report has no equivalent for — already anticipated in 11t). Two things the comparison
surfaced were resolved in the same checkpoint: a log-completeness gap (`summerOccupiedHours`/
`maxExceedableSummerHours` now logged for natural-criterion rows) and an investigated non-defect (a
zero-occupancy bathroom's corridor check runs the whole year regardless of occupancy, deliberately).

**Continuing Iteration 1 testing means widening that one comparison, not repeating it.** 11u names what
Flat1 didn't exercise: a plain `natural` (non-bedroom) criterion, and only one model. Rerun against a
different flat/model, or a model with a non-bedroom naturally-ventilated room, and diff against its own TAS
report the same way. If a new discrepancy turns up, investigate it the way 11u's two findings were
investigated before concluding it's a bug — the corridor "discrepancy" that looked like one on first read
turned out to be intentional, occupancy-independent behaviour in `TM59CorridorExtendedResult`.

**Before testing Iteration 2 (`AcousticRestricted`) or Iteration 3 (`ActiveTrimCooling`), read this — it
will look like a bug if you don't.** `Query.PartOIterationOperatingMode` (11q) currently **refuses both**:
`AcousticRestricted` because whether its stated "boost available" assumption means continuous or on-demand
provision is an unresolved engineering question (11q, 0f item 3), and `ActiveTrimCooling` because what it
assumes about a cooling provision is unsettled (11p/0e). So running either through
`SAMAnalytical.PreparePartOIteration` today produces a **refusal**, not a completed run with different
numbers from BasePassive - that refusal firing correctly is itself worth confirming on a live canvas, but it
is not the same exercise as the Flat1 BasePassive comparison. Meaningful Iteration 2/3 testing is gated on
Michal's answers below, not on more code.

**What actually unblocks Iteration 2/3:** 0f's numbered list, specifically items 1 (confirm the four
BasePassive/AcousticRestricted assumption values, 11p) and 3 (confirm which Approved Document F condition
`AcousticRestricted` simulates at, 11q) - the latter is the direct unblock for
`PartOIterationOperatingMode`'s refusal. Neither is code; both need Michal's decision, not a guess.

**Read section 0 first and verify the repositories against it before changing any code.**

### Also outstanding, in parallel, not blocking Iteration 1/2/3 testing: finish section 11s

**Three repos' Codex/Copilot review findings are fixed (SAM, SAM_Systems, SAM_Tas); two are not:**
`SAM_Tas_Grasshopper` #4 (1 finding, a stale-TM59-XML-on-refusal case) and `SAM_UI` #75 (3 findings, two P1
- a dwelling-switch data-loss bug and a withdrawn-confirmation bug in the WPF assessment window). Transcribed
verbatim with file, line and rationale in **section 11s**, so nothing needs re-querying. This predates the
Option B/comparison work and was not touched by it - pick it up whenever it's convenient, it does not gate
the testing above.

### Decisions still waiting on Michal - do not decide these yourself, ask

1. The provisional domestic `Rank` order (`NV 10, EOL 20, EOC 30, MV 40, MVRE 50`).
2. The unverified commercial `Boost` declarations (all `false`, `CAV`/`VAV`/`DISP`).
3. Whether `Application` should also gate the older unguarded `Create.SystemEnergyCentre` /
   `DefaultSystemEnergyCentres` path - the one that HAS production callers (four Grasshopper components),
   where a space carrying `"VAV"` still resolves `VAV.json`.
4. The `SAM_Systems` CI test step is implemented and locally verified but has **never run** - the workflow
   triggers on `master`/`main`/`sow` only, so it first fires on a PR to `sow/2026-Q3`.
5. The closed ventilation vocabulary in `VentilationStrategyMap`
   (`NV MV MVRE UV EOL EOC CAV VAV DISP`). Declared policy; a custom `SAM_SystemTypeLibrary` with extra
   identities would be refused.
6. The three TPD discrepancies in 11m, which must not be guessed at.
7. **The four BasePassive/AcousticRestricted assumption VALUES (11p)** and **which Approved Document F
   condition `AcousticRestricted` simulates at (11q)** - see above, these two gate Iteration 2 testing.
8. Preserve `ZoneGuid` through `Modify.UpdateIds`'s strip, or refuse an ambiguous name match instead (11t) -
   the design side is still name-assigned; Option B repaired the simulated side only.

### Rules established under review. Each has a test; do not undo any

1. **`Query.PartFDwellingZones` is the single source of truth for what a dwelling is.** Part O scope
   delegates to it and only names the remainder as common space.
2. **The corridor is assessed, never attributed to a dwelling.** `UV` -> `TM59CorridorExtendedResult`.
3. **Never attribute a result by NAME.** `SimulationSpaceMap`: stable key, unique name, then refuse.
   The unique-name fallback exists only inside that map; callers never perform a second name join. Every flat
   has a `Bedroom 2`. A collision invalidates both mapping directions, whole-model selection excludes every
   unresolved space, and a zones-only request remains scoped to those zones.
3a. **Scenario ownership is transactional.** A collision rolls back both scenarios and publishes no stale
    ventilation claim. Assessment reports partial results; official TM59 XML refuses the whole incomplete
    document.
4. **`MVRE` is SAM's MVHR** - verified from the template (0.7 sensible / 0 latent exchanger, no
   recirculation component). Never add an `MVHR` identity. Boost and summer bypass are **operating
   states**, not system types.
5. **`SAM.Analytical` never references `SAM.Analytical.Tas`.** The engineering calculation is
   engine-free; TAS owns conversion, series keys and provenance.
5a. **A scenario key is derived, never generated, never stored.** UTF-8, length-prefixed, NFC-normalised,
   enums by name, behind the marker `OverheatingScenario:v1`. `Key_IsStableAcrossBuilds` pins the exact
   guid - regenerate it ONLY with a deliberate schema bump. There is **no `Iteration0` enum member**: the
   foundation stage states `Undefined`.
6. **`Source` is provenance only** - no part in scenario, equipment, dwelling or result identity.
7. **Series keys are instance state**, defaulted to the analytical vocabulary. Do not reconcile
   `Occupant`/`Occupancy` here.
8. **Behaviour preserved, not improved** in the extraction: the silent no-assessment on a missing series and
   the no-weather throw are still pinned, not endorsed. The `"NV"` default and the zone-name lookup are
   **superseded as of step 7** — bypassed whenever a scenario states a strategy, reachable only for callers
   that state none.
8a. **The scenario is authoritative over ventilation strategy, and a refusal NEVER falls back** (11l). Three
   distinct refusals — uncovered space, scenario states nothing, scenarios disagree — plus a fourth for a
   strategy outside the **closed** vocabulary, because "anything that is not NV or UV is mechanical" is the
   same defect pointing the other way. The assessment drops a refused space and reports it; the **export
   refuses the whole document**, because the external TAS TM59 tool cannot be told a room is missing.
8b. **The two-pass TPD route is a required TAS workaround, not duplication** (11m). Never remove or simplify
   it; always modify a **copy** of the TBD; both routes converge on `TM59AssessmentCalculator`, which must
   not learn the words TSD or TPD.
9. **ONE renderer, ONE placement path** for Part F presentation (sections 5-6).
10. **Model owns engineering data; view owns presentation only.** Reflection test asserts it.

### Working practice

- **test -> commit -> push -> independent review -> continue**, at every architectural checkpoint. Do not
  leave a completed green checkpoint local-only. Never force-push, never squash/rebase published commits,
  never commit to `sow/2026-Q3`, no PRs unless asked.
- Attribution line `Generated by Michal Dengusiak and CodeClaude` plus the `Co-Authored-By` trailer.
- SPDX header on every changed `.cs`.
- **TAS-facing projects need VS Framework MSBuild**, not the dotnet CLI (MSB4803 / `ResolveComReference`):
  `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.
  Build SAM before SAM_UI/SAM_Tas - they reference its built DLLs.
- Section 9 has the rest of the gotchas. Use the SAM implementation-summary style for the final response.
