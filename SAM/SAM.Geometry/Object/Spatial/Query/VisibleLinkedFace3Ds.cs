using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static List<LinkedFace3D> VisibleLinkedFace3Ds<T>(this IEnumerable<T> face3DObjects, Vector3D viewDirection, double tolerance_Area = Tolerance.MacroDistance, double tolerance_Snap = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance) where T : IFace3DObject
        {
            ViewField(face3DObjects, viewDirection, out List<LinkedFace3D> linkedFace3D_Hidden, out List<LinkedFace3D> linkedFace3Ds_Visible, false, true, tolerance_Area, tolerance_Snap, tolerance_Angle, tolerance_Distance);

            return linkedFace3Ds_Visible;
        }
    }
}