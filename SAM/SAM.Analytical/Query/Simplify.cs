// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static TMResult Simplify(this TMExtendedResult tMExtendedResult)
        {
            if (tMExtendedResult == null)
            {
                return null;
            }

            if (tMExtendedResult is TM52ExtendedResult)
            {
                TM52ExtendedResult tM52ExtendedResult = (TM52ExtendedResult)tMExtendedResult;

                List<int> occupiedHourIndicesExceedingAbsoluteLimit = tM52ExtendedResult.GetOccupiedHourIndicesExceedingAbsoluteLimit();

                return new TM52Result(
                    tM52ExtendedResult.Name,
                    tM52ExtendedResult.Source,
                    tM52ExtendedResult.Reference,
                    tM52ExtendedResult.TM52BuildingCategory,
                    tM52ExtendedResult.OccupiedHours,
                    tM52ExtendedResult.MaxExceedableHours,
                    tM52ExtendedResult.GetOccupiedHoursExceedingComfortRange(),
                    tM52ExtendedResult.GetOccupiedDailyWeightedExceedance(),
                    occupiedHourIndicesExceedingAbsoluteLimit == null ? 0 : occupiedHourIndicesExceedingAbsoluteLimit.Count,
                    tM52ExtendedResult.Pass); ;
            }

            if (tMExtendedResult is TM59CorridorExtendedResult)
            {
                TM59CorridorExtendedResult tM59CorridorExtendedResult = (TM59CorridorExtendedResult)tMExtendedResult;

                return new TM59CorridorResult(
                    tM59CorridorExtendedResult.Name,
                    tM59CorridorExtendedResult.Source,
                    tM59CorridorExtendedResult.Reference,
                    tM59CorridorExtendedResult.TM52BuildingCategory,
                    tM59CorridorExtendedResult.OccupiedHours,
                    tM59CorridorExtendedResult.MaxExceedableHours,
                    tM59CorridorExtendedResult.GetHoursNumberExceeding28(),
                    tM59CorridorExtendedResult.Pass,
                    tM59CorridorExtendedResult.GetAnnualHours());
            }

            if (tMExtendedResult is TM59NaturalVentilationBedroomExtendedResult)
            {
                TM59NaturalVentilationBedroomExtendedResult tM59NaturalVentilationBedroomExtendedResult = (TM59NaturalVentilationBedroomExtendedResult)tMExtendedResult;

                return new TM59NaturalVentilationBedroomResult(
                    tM59NaturalVentilationBedroomExtendedResult.Name,
                    tM59NaturalVentilationBedroomExtendedResult.Source,
                    tM59NaturalVentilationBedroomExtendedResult.Reference,
                    tM59NaturalVentilationBedroomExtendedResult.TM52BuildingCategory,
                    tM59NaturalVentilationBedroomExtendedResult.OccupiedHours,
                    tM59NaturalVentilationBedroomExtendedResult.MaxExceedableHours,
                    tM59NaturalVentilationBedroomExtendedResult.GetSummerOccupiedHoursExceedingComfortRange(),
                    tM59NaturalVentilationBedroomExtendedResult.GetAnnualNightOccupiedHours(),
                    tM59NaturalVentilationBedroomExtendedResult.GetSummerOccupiedHours(),
                    tM59NaturalVentilationBedroomExtendedResult.GetSummerMaxExceedableHours(),
                    tM59NaturalVentilationBedroomExtendedResult.GetAnnualMaxExceedableNightHours(),
                    tM59NaturalVentilationBedroomExtendedResult.GetNightHoursNumberExceeding26(),
                    tM59NaturalVentilationBedroomExtendedResult.Pass); ;
            }

            if (tMExtendedResult is TM59NaturalVentilationExtendedResult)
            {
                TM59NaturalVentilationExtendedResult tM59NaturalVentilationExtendedResult = (TM59NaturalVentilationExtendedResult)tMExtendedResult;

                //Constructor order is (occupiedHours, maxExceedableHours, summerOccupiedHours,
                //maxExceedableSummerHours, hoursExceedingComfortRange, pass) - the three middle values were
                //previously passed rotated by one position, so a simplified (non-extended) plain natural-
                //ventilation result reported SummerOccupiedHours/MaxExceedableSummerHours that were not what
                //their names said (Pass/Fail was unaffected, since it is read from Pass directly). Found while
                //building the TM59 verification report, which reads these two fields for its Criterion 1 rows.
                //
                //The TM59 space applications are carried through: the extended result was classified from the
                //internal condition, and dropping that here is what left the report's TM59 Application column
                //"-" on every simplified row.
                //
                //hoursExceedingComfortRange is the SUMMER-restricted count (GetSummerOccupiedHoursExceedingComfortRange),
                //matching SummerOccupiedHours/MaxExceedableSummerHours beside it and the Criterion1 verdict
                //(Pass) baked in below - not the base type's annual GetOccupiedHoursExceedingComfortRange,
                //which would leave this field disagreeing with the other three once flattened.
                return new TM59NaturalVentilationResult(
                    tM59NaturalVentilationExtendedResult.Name,
                    tM59NaturalVentilationExtendedResult.Source,
                    tM59NaturalVentilationExtendedResult.Reference,
                    tM59NaturalVentilationExtendedResult.TM52BuildingCategory,
                    tM59NaturalVentilationExtendedResult.OccupiedHours,
                    tM59NaturalVentilationExtendedResult.MaxExceedableHours,
                    tM59NaturalVentilationExtendedResult.GetSummerOccupiedHours(),
                    tM59NaturalVentilationExtendedResult.GetSummerMaxExceedableHours(),
                    tM59NaturalVentilationExtendedResult.GetSummerOccupiedHoursExceedingComfortRange(),
                    tM59NaturalVentilationExtendedResult.Pass,
                    tM59NaturalVentilationExtendedResult.TM59SpaceApplications?.ToArray());
            }

            if (tMExtendedResult is TM59MechanicalVentilationExtendedResult)
            {
                TM59MechanicalVentilationExtendedResult tM59MechanicalVentilationExtendedResult = (TM59MechanicalVentilationExtendedResult)tMExtendedResult;

                //TM59SpaceApplications carried through, as above: the mechanical criterion does not vary by
                //application, but the classification is still what the space was assessed as, and the report's
                //TM59 Application column reads it.
                return new TM59MechanicalVentilationResult(
                    tM59MechanicalVentilationExtendedResult.Name,
                    tM59MechanicalVentilationExtendedResult.Source,
                    tM59MechanicalVentilationExtendedResult.Reference,
                    tM59MechanicalVentilationExtendedResult.TM52BuildingCategory,
                    tM59MechanicalVentilationExtendedResult.OccupiedHours,
                    tM59MechanicalVentilationExtendedResult.MaxExceedableHours,
                    tM59MechanicalVentilationExtendedResult.GetHoursNumberExceeding26(),
                    tM59MechanicalVentilationExtendedResult.Pass,
                    tM59MechanicalVentilationExtendedResult.TM59SpaceApplications?.ToArray());
            }

            throw new System.NotImplementedException();
        }
    }
}
