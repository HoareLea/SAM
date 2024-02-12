using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static TMResult Simplify(this TMExtendedResult tMExtendedResult)
        {
            if(tMExtendedResult == null)
            {
                return null;
            }

            if(tMExtendedResult is TM52ExtendedResult)
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
                    tM52ExtendedResult.Pass);;
            }

            if(tMExtendedResult is TM59CorridorExtendedResult)
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
                    tM59CorridorExtendedResult.Pass);
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
                    tM59NaturalVentilationBedroomExtendedResult.GetOccupiedHoursExceedingComfortRange(),
                    tM59NaturalVentilationBedroomExtendedResult.GetAnnualNightOccupiedHours(),
                    tM59NaturalVentilationBedroomExtendedResult.GetAnnualMaxExceedableNightHours(),
                    tM59NaturalVentilationBedroomExtendedResult.GetNightHoursNumberExceeding26(),
                    tM59NaturalVentilationBedroomExtendedResult.Pass);
            }

            if (tMExtendedResult is TM59NaturalVentilationExtendedResult)
            {
                TM59NaturalVentilationExtendedResult tM59NaturalVentilationExtendedResult = (TM59NaturalVentilationExtendedResult)tMExtendedResult;

                return new TM59NaturalVentilationResult(
                    tM59NaturalVentilationExtendedResult.Name,
                    tM59NaturalVentilationExtendedResult.Source,
                    tM59NaturalVentilationExtendedResult.Reference,
                    tM59NaturalVentilationExtendedResult.TM52BuildingCategory,
                    tM59NaturalVentilationExtendedResult.OccupiedHours,
                    tM59NaturalVentilationExtendedResult.MaxExceedableHours,
                    tM59NaturalVentilationExtendedResult.GetOccupiedHoursExceedingComfortRange(),
                    tM59NaturalVentilationExtendedResult.Pass);

            }

            if (tMExtendedResult is TM59MechanicalVentilationExtendedResult)
            {
                TM59MechanicalVentilationExtendedResult tM59MechanicalVentilationExtendedResult = (TM59MechanicalVentilationExtendedResult)tMExtendedResult;

                return new TM59MechanicalVentilationResult(
                    tM59MechanicalVentilationExtendedResult.Name,
                    tM59MechanicalVentilationExtendedResult.Source,
                    tM59MechanicalVentilationExtendedResult.Reference,
                    tM59MechanicalVentilationExtendedResult.TM52BuildingCategory,
                    tM59MechanicalVentilationExtendedResult.OccupiedHours,
                    tM59MechanicalVentilationExtendedResult.MaxExceedableHours,
                    tM59MechanicalVentilationExtendedResult.GetHoursNumberExceeding26(),
                    tM59MechanicalVentilationExtendedResult.Pass);
            }

            throw new System.NotImplementedException();
        }
    }
}