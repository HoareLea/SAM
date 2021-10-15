using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static OpeningType DefaultOpeningType(this OpeningAnalyticalType openingAnalyticalType)
        {
            switch(openingAnalyticalType)
            {
                case Architectural.OpeningAnalyticalType.Undefined:
                    return null;

                case Architectural.OpeningAnalyticalType.Door:
                    return new DoorType(new System.Guid("f3445b54-23cb-4ff7-b2c9-b3da2f807bfd"), "Default Door");

                case Architectural.OpeningAnalyticalType.Window:
                    return new WindowType(new System.Guid("6890fa89-2a6e-489f-ab57-3a2a7d5c6baf"), "Default Widnow");
            }

            return null;
        }
    }
}