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

            List<IJSAMObject> jSAMObjects = JSAMObjects<IJSAMObject>(gH_Structure);
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