using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static IGH_GeometricGoo ToGrasshopper(this IGeometry geometry)
        {
            if (geometry is Polygon3D)
                return ((Polygon3D)geometry).ToGrasshopper();
            
            if (geometry is Polyline3D)
                return ((Polyline3D)geometry).ToGrasshopper();
            
            if (geometry is Point3D)
                return ((Point3D)geometry).ToGrasshopper();
            
            if (geometry is Segment3D)
                return ((Segment3D)geometry).ToGrasshopper();

            if (geometry is Face)
                return ((Face)geometry).ToGrasshopper();

            if (geometry is Surface)
                return ((Surface)geometry).ToGrasshopper();

            if (geometry is Planar.Polygon2D)
                return ((Planar.Polygon2D)geometry).ToGrasshopper();
            
            if (geometry is Planar.Point2D)
                return ((Planar.Point2D)geometry).ToGrasshopper();

            if (geometry is Planar.Segment2D)
                return ((Planar.Segment2D)geometry).ToGrasshopper();

            return (geometry as dynamic).ToGrasshopper();
        }
    }
}
