using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Modify
    {
        public static List<IOpening> AddOpenings(this ArchitecturalModel architecturalModel, IEnumerable<IOpening> openings, double tolerance = Core.Tolerance.Distance)
        {
            if(architecturalModel == null || openings == null)
            {
                return null;
            }

            List<IOpening> result = new List<IOpening>();
            foreach(IOpening opening in openings)
            {
                if(architecturalModel.TryAddOpening(opening, tolerance))
                {
                    result.Add(opening);
                }
            }

            return result;
        }
    }
}