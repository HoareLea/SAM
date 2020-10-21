using Grasshopper;
using Grasshopper.Kernel;
using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static GH_Component Copy(this GH_Component gH_Component)
        {
            if (gH_Component == null || gH_Component.Attributes == null)
                return null;

            System.Drawing.PointF location = gH_Component.Attributes.Bounds.Location;

            GH_SAMComponent result = Activator.CreateInstance(gH_Component.GetType()) as GH_SAMComponent;
            if (result == null)
                return null;

            bool instantiated = Instances.ActiveCanvas.InstantiateNewObject(gH_Component.ComponentGuid, location, true);

            if (instantiated)
            {
                result.ExpireSolution(true);
            }

            return result;
        }
    }
}