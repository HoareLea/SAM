<!-- SPDX-License-Identifier: LGPL-3.0-or-later -->
<!-- Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors -->

# Part O PR-5 (SAM half): `PartOBaselineReference` - what a saved result was derived from

**Status (1 Oct 2026): implemented, tested and revised after the owner's design review. [SAM-BIM/SAM#173](https://github.com/SAM-BIM/SAM/pull/173) is open
against `sow/2026-Q3` and is NOT merged. Merge it before [SAM-BIM/SAM_UI#155](https://github.com/SAM-BIM/SAM_UI/pull/155) (same branch name), which stamps and
reads it.** Cross-repo record, the investigation and the wiring: SAM_UI `documentation/PartO-BaselineReference-PR5.md`.

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

A `PartOModelReference` persists exactly: `Kind`, `Guid`, `Fingerprint`, `Name` (display only) and `Path_Relative` (a locator, from the folder the result is
written to). **No absolute path is persisted** (design review, item 1): a saved result can be shared as a fixture or as evidence, and a workstation, user or
OneDrive path would travel with it. The absolute path a caller starts from is used to compute the relative locator and is not kept; a reference written by a
build that did keep one is read without it and written back without it. Identity and resolution never depend on a path or a name.

- **Absent means unknown.** A legacy result has no reference and nothing is inferred. An unknown schema loads `IsValid` false and round-trips as read.
- **Carrying it marks a model as a result.** `Query.PartOBaselineFindings` refuses it as a baseline (run output) and `Modify.RemovePartORunState` removes it.
- **Stamped before the provenance record** is constructed, so `SimulationResultProvenance` (which fingerprints every non-excluded parameter) is
  unchanged: no fingerprint semantics, deny list or existing record moved.

## The fingerprint invariant (design review, item 3)

A reference records a state fingerprint; stamping must not be able to move it.

- **Design:** `Create.PartOBaselineReferenceFromDesign` fingerprints the design **as handed in** (or takes the one the caller already holds - Mixed reuses its
  record's). The reference is stamped on a different model, the result, so the design is untouched; no reference holds the fingerprint of the model that carries it,
  so there is no recursion (the result's own fingerprint, which includes the reference, is another value).
- **Source result:** `Create.PartOBaselineReferenceFromResult` reads the source's own recorded `Fingerprint_Model`, never recomputes or touches it; it equals what
  the source fingerprints to.
- **Resolution recomputes, for results as for designs.** It first read a result's own provenance record, which is what the file *said of itself* when saved: a result
  whose content was edited afterwards, still carrying its old record, would have read as unchanged. It now fingerprints the model as it is (mutation M10 below).
  `Resolved` = same identity and same state; `Changed` = same identity, different state (design edited, result overwritten by a later run, or edited since saving).
- Tests: `TheDesignFingerprint_IsCapturedFromTheIntendedState_AndStampingCannotPerturbIt` and
  `TheSourceFingerprint_IsCapturedFromTheSourcesRecordedState_AndStampingCannotPerturbIt` (captured from the intended state; stamping does not change it; save/reopen
  keeps it; unchanged resolves; a genuine change reports Changed).

## API

- `Create.PartOBaselineReferenceFromDesign(case, design, path_Design, directory_Result, fingerprint = null)` - 1a/1b/2/Mixed. Null for 2B/3, no design or no identity.
- `Create.PartOBaselineReferenceFromResult(case, source, path_Source, directory_Result)` - 2B/3. Requires the source to carry a provenance; inherits the source's own
  `Design`, **rebased** from the source's folder to `directory_Result`.
- `Modify.StampPartOBaselineReference(model, reference)`; `Modify.LocatePartOBaselineReference(model, directory_Result, path_Design)` (gives a design locator where
  there is none - Mixed - and never replaces one).
- `Query.PartOModelResolution(reference, path_Result, path_Hint = null)` -> `PartOBaselineResolution` (Resolved / Changed / NotFound / Ambiguous / Unknown), and the
  path helpers `PartOBaselineRelativePath` / `PartOBaselineRebasedPath`.
- `Modify.MaterialisePartODwellingStrategies` stamps the Mixed reference itself (baseline guid, name and the record's own `Fingerprint_Baseline`). SAM does not know
  the baseline's file or the result folder; SAM_UI adds the relative locator.

**Resolution order:** the relative locator; the caller's `path_Hint` (a place it knows **now**, never persisted); then - only if neither holds the model - up to 16 other
`.sam` files beside those places, so a renamed design is still found. A design is never a file that carries a `SimulationResultProvenance` or scenarios (a result shares
its design's guid); a result must carry a provenance. More than one identical candidate is `Ambiguous` and none is chosen. No absolute path takes part.

## Files

`Enums/Parameter/AnalyticalModelParameter.cs`; new `Enums/PartODerivedCase.cs`, `PartOModelReferenceKind.cs`, `PartOBaselineResolutionStatus.cs`; new
`Classes/PartOModelReference.cs`, `PartOBaselineReference.cs`, `PartOBaselineResolution.cs`; new `Create/PartOBaselineReference.cs`, `Modify/PartOBaselineReference.cs`,
`Query/PartOModelResolution.cs`; changed `Modify/MaterialisePartODwellingStrategies.cs`, `Modify/RemovePartORunState.cs`, `Query/PartOBaselineFindings.cs`. Tests: new
`SAM.Tests/PartOBaselineReferenceTests.cs` (23 tests, 25 cases, real `.sam` files) and `PartODwellingStrategyMaterialisationTests.BaselineReference.cs` (2).

## Validation

- **`SAM.Tests` 2787/2787** (2782 before this review plus 5 new), in fresh processes - see the PR description for the runs. The 25 PR-5 tests (23 reference tests, 25 cases, plus 2 materialisation tests) also passed on their own.
- **11 SAM-side mutations, all killed** (sources restored and sha1-checked; each run builds and runs the focused tests): resolution ignoring the guid; a result accepted as the
  design; an edited design read as `Resolved`; a source judged by its own record instead of its content; the materialiser not stamping; Remove Results keeping the reference;
  no relative locator recorded; the inherited design locator not rebased (this one **survived** the first time - the tests used folders of equal depth - and is killed since
  the Iteration 3 test uses another depth); an absolute path persisted again; the runtime hint ignored; the design fingerprint taken from the wrong state. SAM_UI-side mutations are in
  the SAM_UI record.
- **The earlier isolated failure** (about ten unrelated `PartOIterationPreparationTests`/materialisation tests failing in one run, then 2782/2782 on the same binaries): it has
  **not reproduced** in any later fresh-process run, and its cause was not identified. One later failure in this review was a procedure error, not a product failure:
  `SAM.sln` does not contain `SAM.Tests`, so the test project kept a stale mutation build of `SAM.Analytical.dll` until it was rebuilt itself.
- **Local paths:** a scan of every file this PR adds or changes for user, OneDrive, company and drive-letter paths finds none (two comments use the word "OneDrive" to explain
  why none is stored). A test saves a real result and inspects the inflated `.sam` payload for the temporary folder, `Path_Absolute` and the user name.
- **Existing, not changed here:** a result's `SimulationResultProvenance.Path_TSD` is itself an absolute path to its results file (its fallback is a same-named file beside
  the model). The reference adds none, but a saved result that is shared as a fixture still carries that one. Whether to change it is a separate decision.

## Risks / not done

- The fallback neighbour scan opens up to 16 `.sam` files synchronously, on the refusal path only.
- An older build opening a new result sees an unknown parameter that it keeps as raw JSON; that it then digests to the same fingerprint was not verified.
- The design's state fingerprint costs in proportion to the project size, once per case start (Mixed reuses the one it already takes); resolution of a source result now fingerprints it too.
- A result whose own file is not known when a derived result is made (`path_Source` null) gives its derived result a source locator of none and no design locator; identity still holds.

## Next step

Owner review -> merge this PR -> SAM_UI#155 (its CI clones the same-named SAM branch first) -> closeouts in both repos.
