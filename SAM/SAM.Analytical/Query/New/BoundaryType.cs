using SAM.Architectural;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static BoundaryType? BoundaryType(this BuildingModel buildingModel, IPartition partition)
        {
            if(partition == null)
            {
                return null;
            }

            if(partition is IHostPartition)
            {
                if(Adiabatic((IHostPartition)partition))
                {
                    return Analytical.BoundaryType.Adiabatic;
                }
            }

            if(buildingModel.Shade(partition))
            {
                return Analytical.BoundaryType.Shade;
            }

            ITerrain terrain = buildingModel.Terrain;
            if(terrain != null)
            {
                if (terrain.Below(partition.Face3D) || terrain.On(partition.Face3D))
                {
                    return Analytical.BoundaryType.Ground;
                }
            }

            if(buildingModel.External(partition))
            {
                return Analytical.BoundaryType.Exposed;
            }

            if (buildingModel.Internal(partition))
            {
                return Analytical.BoundaryType.Linked;
            }

            return null;
        }
    }
}