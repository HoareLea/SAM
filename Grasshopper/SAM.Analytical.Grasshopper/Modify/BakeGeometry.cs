using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<Panel> panels = new List<Panel>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooPanel)
                {
                    panels.Add(((GooPanel)variable).Value);
                }
                else if (variable is GooAdjacencyCluster)
                {
                    List<Panel> panels_Temp = ((GooAdjacencyCluster)variable).Value?.GetPanels();
                    if (panels_Temp != null && panels_Temp.Count != 0)
                        panels.AddRange(panels_Temp);
                }
                else if (variable is GooAnalyticalModel)
                {
                    List<Panel> panels_Temp = ((GooAnalyticalModel)variable).Value?.AdjacencyCluster.GetPanels();
                    if (panels_Temp != null && panels_Temp.Count != 0)
                        panels.AddRange(panels_Temp);
                }
            }

            if (panels == null || panels.Count == 0)
                return;

            List<Brep> breps = new List<Brep>();
            for (int i = 0; i < panels.Count; i++)
                breps.Add(null);

            Parallel.For(0, panels.Count, (int i) => {

                Panel panel = panels[i];
                Brep brep = panel.ToRhino(cutApertures, tolerance);
                Guid guid = Guid.Empty;

                if (brep == null)
                    Geometry.Grasshopper.Modify.BakeGeometry(panel.GetFace3D(), rhinoDoc, objectAttributes, out guid);
                else
                    guid = rhinoDoc.Objects.AddBrep(brep);
            });
        }

        public static bool BakeGeometry(this Panel panel, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            guid = Guid.Empty;

            if (panel == null || rhinoDoc == null || objectAttributes == null)
                return false;

            Brep brep = panel.ToRhino(cutApertures, tolerance);
            if (brep == null)
                return Geometry.Grasshopper.Modify.BakeGeometry(panel.GetFace3D(), rhinoDoc, objectAttributes, out guid);

            guid = rhinoDoc.Objects.AddBrep(brep);
            return true;
        }

        public static bool BakeGeometry(this Aperture aperture, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (aperture == null || rhinoDoc == null || objectAttributes == null)
                return false;

            return Geometry.Grasshopper.Modify.BakeGeometry(aperture.GetFace3D(), rhinoDoc, objectAttributes, out guid);
        }
    }
}