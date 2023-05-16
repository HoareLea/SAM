using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    /// <summary>
    /// Provides methods to modify geometric objects.
    /// </summary>
    public static partial class Modify
    {
        /// <summary>
        /// Draws viewport meshes for the specified SAM geometry object.
        /// </summary>
        /// <param name="sAMGeometry">The SAM geometry to draw.</param>
        /// <param name="previewMeshArgs">The arguments for the preview mesh.</param>
        /// <param name="displayMaterial">
        /// The material to use for the display. If this is null, the method uses the material from the preview mesh arguments.
        /// </param>
        public static void DrawViewportMeshes(this ISAMGeometry sAMGeometry, GH_PreviewMeshArgs previewMeshArgs, global::Rhino.Display.DisplayMaterial displayMaterial = null)
        {
            Brep brep = null;

            // Convert the SAM geometry to a Rhino Brep.
            if (sAMGeometry is Face3D)
            {
                brep = Rhino.Convert.ToRhino_Brep((Face3D)sAMGeometry);
            }
            else if (sAMGeometry is Shell)
            {
                brep = Rhino.Convert.ToRhino((Shell)sAMGeometry);
            }
            else if (sAMGeometry is Mesh3D)
            {
                brep = Brep.CreateFromMesh(Rhino.Convert.ToRhino((Mesh3D)sAMGeometry), true);
            }
            else if (sAMGeometry is Planar.Face2D)
            {
                brep = Rhino.Convert.ToRhino_Brep(Spatial.Query.Convert(Spatial.Plane.WorldXY, (Planar.Face2D)sAMGeometry));
            }

            // Draw the Brep using the specified material.
            if (brep != null)
            {
                previewMeshArgs.Pipeline.DrawBrepShaded(brep, displayMaterial == null ? previewMeshArgs.Material : displayMaterial);
            }
        }
    }
}
