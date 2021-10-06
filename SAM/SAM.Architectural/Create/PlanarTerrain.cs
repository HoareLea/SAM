using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static PlanarTerrain PlanarTerrain(double elevation)
        {
            Plane plane = Geometry.Spatial.Create.Plane(elevation);
            if(plane  == null)
            {
                return null;
            }

            return new PlanarTerrain(plane);
        }
    }
}