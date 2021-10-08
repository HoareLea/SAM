namespace SAM.Architectural.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this HostPartition hostPartition, bool cutOpenings = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            return Geometry.Grasshopper.Convert.ToRhino_Brep(hostPartition?.Face3D(cutOpenings, tolerance));
        }
    }
}