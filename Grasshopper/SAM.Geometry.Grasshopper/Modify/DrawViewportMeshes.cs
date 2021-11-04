using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Modify
    {
        public static void DrawViewportMeshes(this ISAMGeometry sAMGeometry, GH_PreviewMeshArgs previewMeshArgs, Rhino.Display.DisplayMaterial displayMaterial = null)
        {
            Brep brep = null;

            if (sAMGeometry is Spatial.Face3D)
                brep = ((Spatial.Face3D)sAMGeometry).ToRhino_Brep();
            else if (sAMGeometry is Spatial.Shell)
                brep = ((Spatial.Shell)sAMGeometry).ToRhino();
            else if (sAMGeometry is Spatial.Mesh3D)
                brep = Brep.CreateFromMesh(((Spatial.Mesh3D)sAMGeometry).ToRhino(), true);

            if (brep != null)
                previewMeshArgs.Pipeline.DrawBrepShaded(brep, displayMaterial == null ? previewMeshArgs.Material : displayMaterial);
        }
    }
}