using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static IOpening Opening(this OpeningType openingType, Face3D face3D)
        {
            if(openingType == null || face3D == null || !face3D.IsValid())
            {
                return null;
            }

            if(openingType is WindowType)
            {
                return new Window((WindowType)openingType, face3D);
            }

            if(openingType is DoorType)
            {
                return new Door((DoorType)openingType, face3D);
            }

            return null;
        }
    }
}
