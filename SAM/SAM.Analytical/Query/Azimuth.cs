namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Azimuth(this Panel panel)
        {
            if (panel == null)
                return double.NaN;

            return Geometry.Spatial.Query.Azimuth(panel.GetFace3D(), Geometry.Spatial.Vector3D.WorldY());
        }
    }
}