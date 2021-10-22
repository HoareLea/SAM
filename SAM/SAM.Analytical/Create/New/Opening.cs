using SAM.Geometry.Spatial;

namespace SAM.Analytical
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

        public static IOpening Opening(System.Guid guid, OpeningType openingType, Face3D face3D)
        {
            if (openingType == null || face3D == null || !face3D.IsValid())
            {
                return null;
            }

            if (openingType is WindowType)
            {
                return new Window(guid, (WindowType)openingType, face3D);
            }

            if (openingType is DoorType)
            {
                return new Door(guid, (DoorType)openingType, face3D);
            }

            return null;
        }

        public static IOpening Opening(System.Guid guid, IOpening opening, Face3D face3D)
        {
            if (opening == null || face3D == null || !face3D.IsValid())
            {
                return null;
            }

            if (opening is Window)
            {
                return new Window(guid, (Window)opening, face3D);
            }

            if (opening is Door)
            {
                return new Door(guid, (Door)opening, face3D);
            }

            return null;
        }
    }
}
