# SimulationResultProvenance - no absolute path in saved models

PR record (branch `fix/simulation-result-provenance-relative-locator-2026-10-01`, base `sow/2026-Q3`). Small, separate from PR-5 and PR-6.

## Status
Implemented and tested; open for review, not merged.

## Root cause
`SimulationResultProvenance.ToJsonObject` wrote `Path_TSD`, the absolute path of the TSD results file, into every saved `.sam` result model
(Part O 1a/1b/2, Iteration 3 Candidate B, Mixed). A public or shared model therefore carried the user's workstation, OneDrive or company folder.

## Trace (every producer and consumer)
- Producers: the constructor `(AnalyticalModel, path_TSD)`, called by `Modify.RunPartOSimulation`, `Modify.RunPartOIteration3`
  and `Modify.SimulatePartOMaterialisationSystems` (SAM_UI). All three then write the model **beside** the results (`Query.Path_PartORunModel`).
  `FromJsonObject` and the copy constructor also set it.
- Consumers: only inside the class - `IsComplete`, `TryResolvePath_TSD` (its refusal texts and its "beside the model by file name" fallback).
  Nothing else reads the property. `PartORun.RestoreCore` and `PartOIteration3ReviewRefusals` call `TryResolvePath_TSD` with the opened model's path.
  `PartORun.Path_TSD` and the other `Path_TSD` members in SAM_UI/SAM_Tas are different, runtime types and are unchanged.
- Fingerprints exclude this record, so they are unaffected.

## Design
- New persisted `Locator_TSD`: the results file's path relative to the folder of the model file (forward slashes; in practice the file name).
  Never rooted. `Locator(path_TSD, path_Model)` builds it; where no relative form exists (another drive) it is null, so the record is incomplete and refused rather than persisted with a local path.
- `Path_TSD` stays on the type as a **runtime-only hint** (set by the constructor, never serialized). No name or file name is identity: the recorded length and write time still decide.
- Resolution: the hint if present and current, else the locator against the opened model's folder (same length/timestamp check), else the same refusals as before.
  A rooted locator is never followed.
- Backward compatible: a legacy record's `Path_TSD` is still read and still resolves; its file name becomes the locator (what the old fallback used), so the next save writes the locator and drops the path.
- New constructor overload `(model, path_TSD, path_Model)` for a model written elsewhere; the existing two-argument constructor assumes "beside", as every caller does. No caller changed.

## Files
`SAM/SAM.Analytical/Classes/SimulationResultProvenance.cs`; `SAM/SAM.Tests/SimulationResultProvenanceTests.cs`; this record.

## Validation
- SAM.Tests 2792/2792 (23 provenance tests, 5 new: no absolute path emitted, save/reopen by locator, copied folder resolves to the copy with the original deleted,
  legacy absolute path reads/resolves/is dropped on save, missing and rewritten results refuse with the same messages).
- SAM_UI `SAM.Analytical.UI.WPF.Tests` 1569/1569 against this SAM build.
- Diff and record scanned for local, user and OneDrive paths: none.

## Risks
- A model saved elsewhere (Save As into another folder) without its results beside it cannot be located by the relative locator in a later session; it refuses safely with "no longer at ...".
  Before this change it resolved on the same machine through the absolute path. Within the session the runtime hint still works.
- Other Part O sidecars (`PartORunResume`, Iteration 3 records, `.partomixed.json`) may still hold absolute paths; out of scope here.
- How an older build reads a model written by this one (no `Path_TSD`) was not run: it would treat the record as having no results file and refuse safely.
