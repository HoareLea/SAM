# SAM Part O PR1 progress

Base: `sow/2026-Q3`. PR1 merged as SAM-BIM/SAM#176 at `fc2345667fb2726b2364264eda14a6a219250144` on 2026-10-04. Local base updated.

## Completed
Persisted CoolingStatSpaceGuid on dwelling strategies and cooled records; refused missing or out-of-dwelling selections; validated record against selected room. Legacy cooled strategies retain no room and require explicit confirmation. Uncooled canonical strategy text remains unchanged.

## Files changed
PartODwellingStrategy, PartOCooledDwelling, PartOMaterialisationRecord, MaterialisePartODwellingStrategies, refusal enum, focused tests; this progress file.

## Validation
Focused PartODwellingStrategy tests: 145 passed; broader PartO suite: 982 passed; PR Windows build, test and SPDX passed.

## Next step
No unresolved PR1 issues. Stop after PR1; do not start PR2 without a new request.
