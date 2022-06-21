using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool SaveAs(this global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure)
        {
            if (gH_Structure == null)
            {
                return false;
            }

            List<IJSAMObject> jSAMObjects = new List<IJSAMObject>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is IGH_Goo)
                {
                    IGH_Goo gH_Goo = (IGH_Goo)variable;

                    IJSAMObject jSAMObject = null;
                    try
                    {
                        jSAMObject = (gH_Goo as dynamic).Value as IJSAMObject;
                    }
                    catch
                    {
                        jSAMObject = null;
                    }

                    if (jSAMObject == null)
                    {
                        continue;
                    }

                    jSAMObjects.Add(jSAMObject);
                }
            }

            if (jSAMObjects == null || jSAMObjects.Count == 0)
            {
                return false;
            }

            string path = null;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }
                path = saveFileDialog.FileName;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return Core.Convert.ToFile(jSAMObjects, path);
        }
    }
}