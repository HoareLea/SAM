using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel MergeCoplanarApertures(this AnalyticalModel analyticalModel, out List<Aperture> mergedApertures, out List<Aperture> redundantApertures, bool validateApertureConstruction, double tolerance = Core.Tolerance.Distance)
        {
            redundantApertures = null;
            mergedApertures = null;

            if (analyticalModel == null)
            {
                return null;
            }

            AdjacencyCluster adjacencyCluster = MergeCoplanarApertures(analyticalModel.AdjacencyCluster, out mergedApertures, out redundantApertures, validateApertureConstruction, tolerance);
            if (adjacencyCluster != null && (mergedApertures != null && mergedApertures.Count > 0) || (redundantApertures != null && redundantApertures.Count > 0))
            {
                AnalyticalModel result = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            return new AnalyticalModel(analyticalModel);
        }

        public static AdjacencyCluster MergeCoplanarApertures(this AdjacencyCluster adjacencyCluster, out List<Aperture> mergedApertures, out List<Aperture> redundantApertures, bool validateApertureConstruction, double tolerance = Core.Tolerance.Distance)
        {
            redundantApertures = null;
            mergedApertures = null;

            if (adjacencyCluster == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return new AdjacencyCluster(adjacencyCluster);
            }

            panels = MergeCoplanarApertures(panels, out mergedApertures, out redundantApertures, validateApertureConstruction, tolerance);
            if((mergedApertures != null && mergedApertures.Count > 0) || (redundantApertures != null && redundantApertures.Count > 0))
            {
                AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);
                foreach (Panel panel in panels)
                {
                    result.AddObject(panel);
                }

                return result;
            }

            return new AdjacencyCluster(adjacencyCluster);

        }

        public static List<Panel> MergeCoplanarApertures(this IEnumerable<Panel> panels, out List<Aperture> mergedApertures, out List<Aperture> redundantApertures, bool validateApertureConstruction, double tolerance = Core.Tolerance.Distance)
        {
            mergedApertures = null;
            redundantApertures = null;

            if(panels == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                List<Aperture> mergedApertures_Panel = null;
                List<Aperture> redundantApertures_Panel = null;

                Panel panel_Temp = panel?.MergeCoplanarApertures(out  mergedApertures_Panel, out redundantApertures_Panel, validateApertureConstruction, tolerance);
                result.Add(panel_Temp);

                if(mergedApertures_Panel != null)
                {
                    if(mergedApertures == null)
                    {
                        mergedApertures = new List<Aperture>();
                    }

                    mergedApertures.AddRange(mergedApertures_Panel);
                }

                if (redundantApertures_Panel != null)
                {
                    if (redundantApertures == null)
                    {
                        redundantApertures = new List<Aperture>();
                    }

                    redundantApertures.AddRange(redundantApertures_Panel);
                }
            }

            return result;

        }

        public static Panel MergeCoplanarApertures(this Panel panel, out List<Aperture> mergedApertures, out List<Aperture> redundantApertures, bool validateApertureConstruction, double tolerance = Core.Tolerance.Distance)
        {
            mergedApertures = null;
            redundantApertures = null;

            if (panel == null)
            {
                return null;
            }

            Panel result = Create.Panel(panel);

            List<Aperture> apertures = panel.Apertures;
            if(apertures == null || apertures.Count < 2)
            {
                return result;
            }

            apertures.RemoveAll(x => x == null);

            List<List<Aperture>> apertures_Group = new List<List<Aperture>>();
            if(validateApertureConstruction)
            {
                List<Guid> guids = new List<Guid>();
                foreach(Aperture aperture in apertures)
                {
                    Guid guid = aperture.TypeGuid;
                    int index = guids.IndexOf(guid);
                    if(index == -1)
                    {
                        guids.Add(guid);
                        apertures_Group.Add(new List<Aperture>() { aperture });
                    }
                    else
                    {
                        apertures_Group[index].Add(aperture);
                    }
                }
            }
            else
            {
                apertures_Group.Add(apertures);
            }

            Plane plane = panel.Plane;

            HashSet<Guid> guids_ToBeRemoved = new HashSet<Guid>();
            foreach(List<Aperture> apertures_Temp in apertures_Group)
            {
                if(apertures_Temp.Count < 2)
                {
                    continue;
                }

                Dictionary<Aperture, Face2D> dictionary = new Dictionary<Aperture, Face2D>();
                foreach(Aperture aperture_Temp in apertures_Temp)
                {
                    dictionary[aperture_Temp] = plane.Convert(plane.Project(aperture_Temp.GetFace3D()));
                }

                List<Face2D> face2Ds = Geometry.Planar.Query.Union(dictionary.Values, tolerance);
                if(face2Ds == null || face2Ds.Count == 0)
                {
                    continue;
                }

                foreach(Face2D face2D in face2Ds)
                {
                    BoundingBox2D boundingBox2D = face2D.GetBoundingBox();

                    List<Aperture> apertures_Face2D = new List<Aperture>();
                    foreach(KeyValuePair<Aperture, Face2D> keyValuePair in dictionary)
                    {
                        Point2D point2D = keyValuePair.Value.GetInternalPoint2D(tolerance);
                        if(!boundingBox2D.InRange(point2D, tolerance))
                        {
                            continue;
                        }

                        if(!face2D.Inside(point2D, tolerance))
                        {
                            continue;
                        }

                        apertures_Face2D.Add(keyValuePair.Key);
                    }

                    if(apertures_Face2D.Count <= 1)
                    {
                        continue;
                    }

                    apertures_Face2D.ForEach(x => guids_ToBeRemoved.Add(x.Guid));

                    if(mergedApertures == null)
                    {
                        mergedApertures = new List<Aperture>();
                    }

                    mergedApertures.Add(Create.Aperture(apertures_Face2D[0].ApertureConstruction, plane.Convert(face2D)));
                    apertures_Face2D.RemoveAt(0);

                    if (redundantApertures == null)
                    {
                        redundantApertures = new List<Aperture>();
                    }

                    redundantApertures.AddRange(apertures_Face2D);
                }
            }

            if(guids_ToBeRemoved != null && guids_ToBeRemoved.Count != 0)
            {
                foreach(Guid guid in guids_ToBeRemoved)
                {
                    result.RemoveAperture(guid);
                }
            }

            if(mergedApertures != null && mergedApertures.Count != 0)
            {
                for (int i = mergedApertures.Count - 1; i >= 0; i--)
                {
                    if(!result.AddAperture(mergedApertures[i]))
                    {
                        mergedApertures.RemoveAt(i);
                    }
                }
            }

            return result;
        }
    }
}