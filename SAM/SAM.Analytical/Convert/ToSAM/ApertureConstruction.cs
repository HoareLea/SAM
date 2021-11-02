using System;

namespace SAM.Analytical
{
    public static partial class Convert
    {  
        public static ApertureConstruction ToSAM(this OpeningType openingType)
        {
            if(openingType == null)
            {
                return null;
            }

            ApertureType apertureType = ApertureType.Undefined;
            if(openingType is WindowType)
            {
                apertureType = ApertureType.Window;
            }
            else if(openingType is DoorType)
            {
                apertureType = ApertureType.Door;
            }

            ApertureConstruction result = new ApertureConstruction(openingType.Guid, openingType.Name, apertureType, openingType?.PaneMaterialLayers?.ConvertAll(x => x.ToSAM()), openingType?.FrameMaterialLayers?.ConvertAll(x => x.ToSAM()));
            return result;
        }
    }
}