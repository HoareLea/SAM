namespace SAM.Architectural.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this HostBuildingElement hostBuildingElement, bool cutOpenings = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            return Geometry.Grasshopper.Convert.ToRhino_Brep(hostBuildingElement?.Face3D(cutOpenings, tolerance));
        }
    }
}