using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<IOpening> AddOpenings(this BuildingModel buildingModel, IEnumerable<IOpening> openings, double tolerance = Core.Tolerance.Distance)
        {
            if(buildingModel == null || openings == null)
            {
                return null;
            }

            List<IOpening> result = new List<IOpening>();
            foreach(IOpening opening in openings)
            {
                if(buildingModel.TryAddOpening(opening, tolerance))
                {
                    result.Add(opening);
                }
            }

            return result;
        }
    }
}