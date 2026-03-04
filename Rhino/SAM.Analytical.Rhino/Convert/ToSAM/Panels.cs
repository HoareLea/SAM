// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Rhino;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static List<Panel> ToSAM_Panels(this global::Rhino.Geometry.Brep brep, double tolerance = Core.Tolerance.Distance)
        {
            if(brep is null)
            {
                return null;
            }

            Panel panel = Query.AnalyticalObject<Panel>(brep);

            List<Face3D> face3Ds = brep.ToSAM(true, true, tolerance)?.FindAll(x => x is Face3D).ConvertAll(x => (Face3D)x);
            if(face3Ds is null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();

            for (int i = 0; i < face3Ds.Count; i++)
            {
                Panel panel_Temp = panel;


                if (panel_Temp == null)
                {
                    panel_Temp = Create.Panel(null, PanelType.Undefined, face3Ds[0]);
                }
                else
                {
                    panel_Temp = Create.Panel(i == 0 ? panel.Guid : System.Guid.NewGuid(), panel, face3Ds[0]);
                }

                PanelType panelType = panel_Temp.PanelType;
                if (panelType == PanelType.Undefined)
                {
                    Vector3D normal = panel_Temp.Normal;
                    if (normal == null)
                    {
                        continue;
                    }

                    PanelType panelType_New = Analytical.Query.PanelType(normal);
                    if (panelType_New != panelType)
                    {
                        panel_Temp = Create.Panel(panel_Temp, panelType_New);
                    }
                }

                if (panel_Temp.Construction == null)
                {
                    Construction construction_Temp = Analytical.Query.DefaultConstruction(panel_Temp.PanelType);
                    if (construction_Temp != null)
                    {
                        panel_Temp = Create.Panel(panel_Temp, construction_Temp);
                    }
                }

                result.Add(panel_Temp);
            }

            return result;
        }
    }
}
