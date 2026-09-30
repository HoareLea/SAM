<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O PR-5 (SAM half): `PartOBaselineReference` - what a saved result was derived from

**Status (1 Oct 2026): implemented and tested; [SAM-BIM/SAM#173](https://github.com/SAM-BIM/SAM/pull/173) is open against `sow/2026-Q3` and is NOT merged. Merge it before
[SAM-BIM/SAM_UI#155](https://github.com/SAM-BIM/SAM_UI/pull/155) (same branch name), which stamps and reads it.** Cross-repo record, the
investigation and the wiring: SAM_UI `documentation/PartO-BaselineReference-PR5.md`.

- Branch `feature/parto-pr5-baseline-reference-2026-10-01`, from `sow/2026-Q3` `137c0bcf`. No SAM_Systems, SAM_Tas change.
- Architecture authority: SAM_UI `documentation/PartO-ModelStateArchitecture.md`, step 6 (PR-5). Nothing here contradicts it.

## Problem

A Part O result model keeps its design's `Guid` (every copy constructor preserves it) and, for 1a, 1b and 2, its `Name`. Its only link to a design was
`PartOMaterialisationRecord.Fingerprint_Baseline` - a content hash, Mixed only, which can validate a baseline it is handed but cannot locate
one. So a reopened result could not say which design it came from, which case it was, or whether that design had changed since.

## Representation (additive, schema `v1`)

`AnalyticalModelParameter.PartOBaselineReference` holds a `PartOBaselineReference`, stamped on **result** models only:

| Field | Meaning |
|---|---|
| `Case` | `PartODerivedCase`: Iteration1a, Iteration1b, Iteration2, Iteration2B, Iteration3, MixedDesign |
| `Design` | `PartOModelReference` (Kind Design) - the design model the lineage derives from. Null only for a result derived from a legacy result. |
| `Source` | `PartOModelReference` (Kind Result) - the immediate source **result**, for 2B (the Iteration 2 result) and Iteration 3 (the 1a/2 result). Null otherwise. |

A `PartOModelReference` is **identity first, locator second, name last**: `Guid`, `Fingerprint` (state - `SimulationResultProvenance.Fingerprint` of a
design, or a result's own provenance `Fingerprint_Model`), `Path_Relative` (from the folder the result is written to) then `Path_Absolute`, and `Name`
for display only. A file is accepted only by identity; a name is never compared.

- **Absent means unknown.** A legacy result has no reference and nothing is inferred. An unknown schema loads `IsValid` false and round-trips as read.
- **Carrying it marks a model as a result.** `Query.PartOBaselineFindings` refuses it as a baseline (run output) and `Modify.RemovePartORunState` removes it.
- **Stamped before the provenance record** is constructed, so `SimulationResultProvenance` (which fingerprints every non-excluded parameter) is
  unchanged: no fingerprint semantics, deny list or existing record moved.

## API

- `Create.PartOBaselineReferenceFromDesign(case, design, path_Design, fingerprint = null)` - 1a/1b/2/Mixed. Null for 2B/3, no design or no identity.
- `Create.PartOBaselineReferenceFromResult(case, source, path_Source)` - 2B/3. Requires the source to carry a provenance; inherits the source's own `Design`.
- `Modify.StampPartOBaselineReference(model, reference)` and `Modify.LocatePartOBaselineReference(model, directory_Result, path_Design = null)`
  (completes relative paths once the result folder is known; identity untouched).
- `Query.PartOModelResolution(reference, path_Result)` -> `PartOBaselineResolution` (Resolved / Changed / NotFound / Ambiguous / Unknown).
- `Modify.MaterialisePartODwellingStrategies` stamps the Mixed reference itself (baseline guid, name, and the record's own `Fingerprint_Baseline` - no
  second hash). SAM does not know the baseline's file; SAM_UI adds it as a locator.

**Resolution order:** relative locator, absolute locator, then - only if neither holds the model - up to 16 other `.sam` files beside those places, so a
renamed design is still found. A design is never a file that carries a `SimulationResultProvenance` or scenarios (a result shares its design's guid); a
result must carry a provenance. More than one identical candidate is `Ambiguous` and none is chosen.

## Files

`Enums/Parameter/AnalyticalModelParameter.cs`; new `Enums/PartODerivedCase.cs`, `PartOModelReferenceKind.cs`, `PartOBaselineResolutionStatus.cs`;
new `Classes/PartOModelReference.cs`, `PartOBaselineReference.cs`, `PartOBaselineResolution.cs`; new `Create/PartOBaselineReference.cs`,
`Modify/PartOBaselineReference.cs`, `Query/PartOModelResolution.cs`; changed `Modify/MaterialisePartODwellingStrategies.cs`,
`Modify/RemovePartORunState.cs`, `Query/PartOBaselineFindings.cs`. Tests: new `SAM.Tests/PartOBaselineReferenceTests.cs` (18 tests, 20 cases, real `.sam` files) and `PartODwellingStrategyMaterialisationTests.BaselineReference.cs` (2).

## Validation

- `SAM.Tests` full suite **2782/2782** (22 new tests: 18 in `PartOBaselineReferenceTests`, 20 cases, and 2 in the materialisation partial). One earlier run,
  straight after the SAM_UI suite, failed about ten unrelated `PartOIterationPreparationTests`/materialisation tests; the same binaries passed 2782/2782 on the rerun, and the
  cause (probably shared ActiveSetting state left by the other suite) was not investigated.
- **Mutation checks** (all killed, sources restored and sha1-checked): resolution ignoring the guid; a result accepted as the design; an edited design
  still `Resolved`; the materialiser not stamping; Remove Results keeping the reference; the relative locator never written.

## Risks / not done

- The fallback neighbour scan opens up to 16 `.sam` files synchronously, on the refusal path only.
- An older build opening a new result sees an unknown parameter that it keeps as raw JSON; that it then digests to the same fingerprint was not verified.
- The design's state fingerprint costs in proportion to the project size, once per case start (Mixed reuses the one it already takes).

## Next step

Owner review -> merge this PR -> SAM_UI PR-5 (its CI clones the same-named SAM branch first) -> closeouts in both repos.
