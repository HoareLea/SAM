using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public static partial class Modify
    {
        public static bool DrawViewportWires(this ArchitecturalModel architecturalModel, GH_PreviewWireArgs previewWireArgs)
        {
            if(architecturalModel == null || previewWireArgs == null)
            {
                return false;
            }

            List<Room> rooms = architecturalModel.GetObjects<Room>();
            if (rooms != null)
            {
                foreach (Room room in rooms)
                {
                    Point3d? point3d = Geometry.Grasshopper.Convert.ToRhino(room?.Location);
                    if (point3d == null || !point3d.HasValue)
                        continue;

                    previewWireArgs.Pipeline.DrawPoint(point3d.Value);
                }
            }

            List<IPartition> partitions = architecturalModel.GetObjects<IPartition>();
            if (partitions == null)
            {
                return false;
            }

            Geometry.Spatial.BoundingBox3D boundingBox3D = null;
            if (previewWireArgs.Viewport.IsValidFrustum)
            {
                BoundingBox boundingBox = previewWireArgs.Viewport.GetFrustumBoundingBox();
                boundingBox3D = new Geometry.Spatial.BoundingBox3D(new Geometry.Spatial.Point3D[] { boundingBox.Min.ToSAM(), boundingBox.Max.ToSAM() });
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

                List<Room> rooms_hostPartition = architecturalModel.GetRooms(partition);
                if (rooms != null && rooms.Count > 1)
                    continue;

                Geometry.Grasshopper.Modify.DrawViewportWires(face3D, previewWireArgs, System.Drawing.Color.DarkRed, System.Drawing.Color.BlueViolet);
            }

            return true;
        }

    }
}