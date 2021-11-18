using Rhino;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Rhino
{
    public static partial class Create
    {
        public static Shell Shell(this global::Rhino.Geometry.Mesh mesh, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (mesh == null)
                return null;

            double unitScale = RhinoMath.UnitScale(UnitSystem.Millimeters, RhinoDoc.ActiveDoc.ModelUnitSystem);

            mesh.Faces.ConvertNonPlanarQuadsToTriangles(unitScale * tolerance_Distance, unitScale * tolerance_Angle, 0);
            mesh.UnifyNormals(false);
            mesh.Ngons.AddPlanarNgons(unitScale * tolerance_Distance, 3, 1, false);

            Mesh3D mesh3D = Rhino.Convert.ToSAM(mesh);
            if(mesh3D == null)
            {
                return null;
            }

            return Geometry.Convert.ToSAM_Shell(mesh3D);
        }
    }
}