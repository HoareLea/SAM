# SAM Part O PR1 progress

Base: `sow/2026-Q3`. PR1 merged as SAM-BIM/SAM#176 at `fc2345667fb2726b2364264eda14a6a219250144` on 2026-10-04. Local base updated.

## Completed
Persisted CoolingStatSpaceGuid on dwelling strategies and cooled records; refused missing or out-of-dwelling selections; validated record against selected room. Legacy cooled strategies retain no room and require explicit confirmation. Uncooled canonical strategy text remains unchanged.

## Files changed
PartODwellingStrategy, PartOCooledDwelling, PartOMaterialisationRecord, MaterialisePartODwellingStrategies, refusal enum, focused tests; this progress file.

## Validation
Focused PartODwellingStrategy tests: 145 passed; broader PartO suite: 982 passed; PR Windows build, test and SPDX passed.

## PR2 validation (2026-10-04)
No SAM code changes were needed. Existing SimulationResultProvenance tests (23 passed) cover changed weather, changed model/scenarios, missing or rewritten results, and relative result resolution. PR2 diagnostics merged in SAM_Tas#81 and SAM_UI#192. No physics changed.

## Next step
PR2 is complete. Next task, only when requested: real end-to-end acceptance through SAM_UI → Part O → Prepare & Run → Iteration 3 using the prepared Nuaire sample. Do not start PR3.
