using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Core;
using System.Collections.Generic;
using System.Diagnostics;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static Process ToUI(this global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure)
        {
            if(gH_Structure == null)
            {
                return null;
            }

            string path = Query.AnalyticalUIPath();
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            List<IAnalyticalObject> analyticalObjects = Core.Grasshopper.Query.JSAMObjects<IAnalyticalObject>(gH_Structure);
            if (analyticalObjects == null)
            {
                return null;
            }

            AnalyticalModel analyticalModel = Analytical.Convert.ToSAM_AnalyticalModel(analyticalObjects);
            if (analyticalModel == null)
            {
                return null;
            }

            string temporaryPath = System.IO.Path.GetTempFileName();

            Core.Convert.ToFile(analyticalModel, temporaryPath);

            StartupOptions startupOptions = new StartupOptions()
            {
                Path = temporaryPath,
                TemporaryFile = true
            };

            Process result = new Process();
            result.StartInfo.FileName = path;
            result.StartInfo.Arguments = startupOptions.ToString();
            result.Start();

            return result;
        }
    }
}