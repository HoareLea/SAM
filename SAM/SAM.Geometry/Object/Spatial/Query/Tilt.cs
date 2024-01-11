using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Angle between normal taken from given geometry and XY Plane mesured in degrees 
        /// </summary>
        /// <param name="face3DObject">SAM Face3D Object</param>
        /// <returns>Tilt in degrees</returns>
        public static double Tilt(this IFace3DObject face3DObject)
        {
            if (face3DObject == null)
                return double.NaN;

            return Geometry.Spatial.Query.Tilt(face3DObject.Face3D);
        }
    }
}