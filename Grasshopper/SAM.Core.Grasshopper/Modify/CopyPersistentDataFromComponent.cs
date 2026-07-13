// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        internal static void CopyPersistentDataFromComponent(GH_SAMComponent gH_SAMComponent_From, GH_SAMComponent gH_SAMComponent_To)
        {
            if (gH_SAMComponent_From == null || gH_SAMComponent_To == null)
            {
                return;
            }

            List<IGH_Param> oldInputs = gH_SAMComponent_From.Params?.Input;
            List<IGH_Param> newInputs = gH_SAMComponent_To.Params?.Input;

            if (oldInputs == null || newInputs == null)
            {
                return;
            }

            Dictionary<string, IGH_Param> newInputDict = new Dictionary<string, IGH_Param>();
            foreach (IGH_Param param in newInputs)
            {
                if (param != null)
                {
                    newInputDict[param.Name] = param;
                }
            }

            foreach (IGH_Param oldParam in oldInputs)
            {
                if (oldParam == null)
                {
                    continue;
                }

                if (!newInputDict.TryGetValue(oldParam.Name, out IGH_Param newParam))
                {
                    continue;
                }

                CopyPersistentData(oldParam, newParam);
            }
        }
    }
}
