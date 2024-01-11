using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Create
    {
        public static List<Shell> Shells<N>(this IEnumerable<N> face3DObjects, IEnumerable<double> elevations, double offset = 0.1, double thinnessRatio = 0.01, double minArea = Core.Tolerance.MacroDistance, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where N: IFace3DObject
        {
            return Geometry.Spatial.Create.Shells(face3DObjects?.ToList().ConvertAll(x => x?.Face3D), elevations, offset, thinnessRatio, minArea, snapTolerance, silverSpacing, tolerance_Angle, tolerance_Distance);
        }

        public static List<Shell> Shells<N>(this IEnumerable<N> face3DObjects, IEnumerable<double> elevations, IEnumerable<double> offsets, IEnumerable<double> auxiliaryElevations = null, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where N: IFace3DObject
        {
            return Geometry.Spatial.Create.Shells(face3DObjects?.ToList().ConvertAll(x => x?.Face3D), elevations, offsets, auxiliaryElevations, snapTolerance, silverSpacing, tolerance_Angle, tolerance_Distance);
        }
    }
}