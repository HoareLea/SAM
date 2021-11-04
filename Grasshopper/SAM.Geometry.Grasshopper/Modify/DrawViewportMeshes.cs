using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Modify
    {
        public static void DrawViewportMeshes(this ISAMGeometry sAMGeometry, GH_PreviewMeshArgs previewMeshArgs, global::Rhino.Display.DisplayMaterial displayMaterial = null)
        {
            Brep brep = null;

            if (sAMGeometry is Spatial.Face3D)
                brep = (Rhino.Convert.ToRhino_Brep((Spatial.Face3D)sAMGeometry));
            else if (sAMGeometry is Spatial.Shell)
                brep = Rhino.Convert.ToRhino(((Spatial.Shell)sAMGeometry));
            else if (sAMGeometry is Spatial.Mesh3D)
                brep = Brep.CreateFromMesh(Rhino.Convert.ToRhino(((Spatial.Mesh3D)sAMGeometry)), true);

            if (brep != null)
                previewMeshArgs.Pipeline.DrawBrepShaded(brep, displayMaterial == null ? previewMeshArgs.Material : displayMaterial);
        }
    }
}