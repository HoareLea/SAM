using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static bool DrawViewportWires(this BuildingModel buildingModel, GH_PreviewWireArgs previewWireArgs)
        {
            if(buildingModel == null || previewWireArgs == null)
            {
                return false;
            }

            List<Space> spaces = buildingModel.GetObjects<Space>();
            if (spaces != null)
            {
                foreach (Space space in spaces)
                {
                    Point3d? point3d = Geometry.Rhino.Convert.ToRhino(space?.Location);
                    if (point3d == null || !point3d.HasValue)
                        continue;

                    previewWireArgs.Pipeline.DrawPoint(point3d.Value);
                }
            }

            List<IPartition> partitions = buildingModel.GetObjects<IPartition>();
            if (partitions == null)
            {
                return false;
            }

            Geometry.Spatial.BoundingBox3D boundingBox3D = null;
            if (previewWireArgs.Viewport.IsValidFrustum)
            {
                BoundingBox boundingBox = previewWireArgs.Viewport.GetFrustumBoundingBox();
                boundingBox3D = new Geometry.Spatial.BoundingBox3D(new Geometry.Spatial.Point3D[] { Geometry.Rhino.Convert.ToSAM(boundingBox.Min), Geometry.Rhino.Convert.ToSAM(boundingBox.Max) });
            }

            foreach (IPartition partition in partitions)
            {
                Geometry.Spatial.Face3D face3D = partition?.Face3D;
                if(face3D == null)
                {
                    continue;
                }

                if (boundingBox3D != null)
                {
                    Geometry.Spatial.BoundingBox3D boundingBox3D_Temp = face3D.GetBoundingBox();
                    if (boundingBox3D_Temp != null)
                    {
                        if (!boundingBox3D.Inside(boundingBox3D_Temp) && !boundingBox3D.Intersect(boundingBox3D_Temp))
                            continue;
                    }
                }

                List<Space> spaces_hostPartition = buildingModel.GetSpaces(partition);
                if (spaces != null && spaces.Count > 1)
                    continue;

                Geometry.Grasshopper.Modify.DrawViewportWires(face3D, previewWireArgs, System.Drawing.Color.DarkRed, System.Drawing.Color.BlueViolet);
            }

            return true;
        }

    }
}