

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static OpeningType OpeningType(this OpeningType openingType, string name)
        {
            if (openingType == null || name == null)
            {
                return null;
            }

            if(openingType is DoorType)
            {
                return new DoorType((DoorType)openingType, name);
            }

            if (openingType is WindowType)
            {
                return new WindowType((WindowType)openingType, name);
            }

            return null;
        }
    }
}
