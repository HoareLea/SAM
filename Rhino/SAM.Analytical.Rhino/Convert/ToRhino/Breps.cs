using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static List<Brep> ToRhino(this AdjacencyCluster adjacencyCluster, bool cutApertures = false, bool includeFrame = false, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                panels = new List<Panel>();

            List<Brep> result = new List<Brep>();

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces != null && spaces.Count > 0)
            {
                foreach (Space space in spaces)
                {

                    List<Panel> panels_Related = adjacencyCluster.GetRelatedObjects<Panel>(space);
                    if (panels_Related == null || panels_Related.Count == 0)
                        continue;

                    panels.RemoveAll(x => panels_Related.Contains(x));
                    List<Brep> breps = new List<Brep>();
                    foreach (Panel panel in panels_Related)
                    {
                        List<Brep> breps_Panel = panel.ToRhino(cutApertures, tolerance);
                        if (breps_Panel == null)
                        {
                            continue;
                        }

                        breps.AddRange(breps_Panel);

                        List<Aperture> apertures = panel.Apertures;
                        if (apertures != null)
                        {
                            foreach (Aperture aperture in apertures)
                            {
                                aperture.ToRhino(includeFrame)?.ForEach(x => result.Add(x));
                            }
                        }
                    }

                    if (breps == null || breps.Count == 0)
                        continue;

                    Brep[] breps_Join = Brep.JoinBreps(breps, tolerance);

                    if (breps_Join != null)
                        result.AddRange(breps_Join);


                }
            }

            foreach (Panel panel in panels)
            {
                List<Brep> breps = panel.ToRhino(cutApertures, tolerance);
                if (breps == null)
                {
                    continue;
                }

                List<Aperture> apertures = panel.Apertures;
                if(apertures != null)
                {
                    foreach(Aperture aperture in apertures)
                    {
                        aperture.ToRhino(includeFrame)?.ForEach(x => result.Add(x));
                    }
                }

                result.AddRange(breps);
            }

            return result;
        }

        public static List<Brep> ToRhino(this Aperture aperture, bool includeFrame = false)
        {
            if(aperture == null)
            {
                return null;
            }

            List<Brep> result = new List<Brep>();

            if (!includeFrame)
            {
                result.Add(Geometry.Rhino.Convert.ToRhino_Brep(new Face3D(aperture?.GetExternalEdge3D())));
                return result;
            }

            List<Face3D> face3Ds = null;

            face3Ds = aperture.GetFace3Ds(AperturePart.Frame);
            face3Ds?.ForEach(x => result.Add(Geometry.Rhino.Convert.ToRhino_Brep(x)));

            face3Ds = aperture.GetFace3Ds(AperturePart.Pane);
            face3Ds?.ForEach(x => result.Add(Geometry.Rhino.Convert.ToRhino_Brep(x)));

            return result;
        }

        public static List<Brep> ToRhino(this IPanel panel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (panel == null)
            {
                return null;
            }

            List<Face3D> face3Ds = panel is Panel ? ((Panel)panel).GetFace3Ds(cutApertures) : new List<Face3D>() { panel.Face3D };
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Brep> result = new List<Brep>();
            foreach(Face3D face3D in face3Ds)
            {
                Brep brep = Geometry.Rhino.Convert.ToRhino_Brep(face3D, tolerance);
                if(brep == null)
                {
                    continue;
                }

                result.Add(brep);
            }

            return result;
        }
    }
}