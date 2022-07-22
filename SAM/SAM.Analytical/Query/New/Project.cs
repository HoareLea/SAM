using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IOpening Project(this IPartition partition, IOpening opening)
        {
            if(partition == null || opening == null)
            {
                return null;
            }

            Plane plane = partition.Face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Face3D face3D = plane.Project(opening.Face3D);
            if(face3D == null || !face3D.IsValid())
            {
                return null;
            }

            return Create.Opening(opening.Guid, opening, face3D);

        }
    }
}