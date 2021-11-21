using Rhino;

namespace SAM.Geometry.Rhino
{
    public static partial class Query
    {
        public static double UnitScale()
        {
            return UnitScale(RhinoDoc.ActiveDoc);
        }

        public static double UnitScale(RhinoDoc rhinoDoc)
        {
            if(rhinoDoc == null)
            {
                return double.NaN;
            }

            return RhinoMath.UnitScale(UnitSystem.Meters, rhinoDoc.ModelUnitSystem);
        }
    }
}