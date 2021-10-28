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

        public static IOpening Opening(System.Guid guid, IOpening opening, Face3D face3D, Point3D location)
        {
            if(opening == null || face3D == null || location == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            plane = new Plane(plane, plane.Project(location));

            Face3D face3D_Temp = plane.Convert(plane.Convert(face3D));
            if(face3D_Temp == null)
            {
                return null;
            }

            return Opening(guid, opening, face3D);
        }

        public static IOpening Opening(System.Guid guid, OpeningType openingType, Face3D face3D, Point3D location)
        {
            if (openingType == null || face3D == null || location == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            plane = new Plane(plane, plane.Project(location));

            Face3D face3D_Temp = plane.Convert(plane.Convert(face3D));
            if (face3D_Temp == null)
            {
                return null;
            }

            return Opening(guid, openingType, face3D);
        }
    }
}
