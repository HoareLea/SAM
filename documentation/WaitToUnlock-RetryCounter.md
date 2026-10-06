# Query.WaitToUnlock - retry counter never advanced

PR record (branch `fix/wait-to-unlock-retry-counter-2026-10-01`, base `sow/2026-Q3`). Narrow; independent of Part O and of the TPD `Loading TSD data` stall.

## Status
Implemented and tested; open for review, not merged.

## Root cause
`WaitToUnlock(path, waitTime = 1000, count = 10)` looped `while (i <= count)` but never incremented `i`, so a file that stayed locked waited forever instead of returning `false` after the configured attempts.

## Fix
One line: `i++` after the sleep. Contract unchanged: `true` once the file is unlocked; `false` for a blank or missing path; `false` when still locked after the attempts.
The `<=` bound is untouched, so `count` is the number of retries after the first check (count + 1 checks in total), and `count = 0` still checks once.

## Callers (all SAM-BIM repos, `git grep`)
No caller depends on an infinite wait; the default 10 x 1 s budget now applies.
- SAM_Tas `Modify.Simulate(TBDDocument, ...)` returns the result as its success flag.
- SAM_Tas `Modify.Simulate` (evidence overload) and `ThermostatBridge`: result ignored ("a wait, not a verdict"); success decided by `SimulationEvidence`.
- SAM_Tas `CalculateResultantTemperature`: `finished` only gates a `Save()`.
- SAM_UI `PrintAirHandlingUnitsByTemplate`: deletes a temp file only when it returns true.

## Files
`SAM/SAM.Core/Query/WaitToUnlock.cs`; `SAM/SAM.Tests/QueryWaitToUnlockTests.cs` (new); this record.

## Validation
- Fail-first, before the fix: the new persistently-locked test FAILED (no return within the 10 s bound; 1 failed / 6 passed of 7).
- After the fix: `QueryWaitToUnlockTests` 7/7; full `SAM.Tests` 2799/2799, 0 failed, 0 skipped.

## Risks
- A TSD file that TAS holds locked for longer than about 11 s after `simulate` returns now yields `false` / proceeds instead of blocking. The Tas callers already treat it as a wait only, except `Simulate(TBDDocument, ...)` and `CalculateResultantTemperature`, which would report not-finished / skip the `Save()`. Not observed with real TAS in this PR.
