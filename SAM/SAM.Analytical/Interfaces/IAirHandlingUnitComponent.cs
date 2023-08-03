
using System;

namespace SAM.Analytical
{
    public interface IAirHandlingUnitComponent : IAnalyticalEquipment, IAnalyticalObject
    {
        Guid Guid {get;}
    }
}
