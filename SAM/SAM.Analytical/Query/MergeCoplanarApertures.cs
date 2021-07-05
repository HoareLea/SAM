using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergeOverlapApertures(this IEnumerable<Panel> panels, bool validateConstruction = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(panels == null)
            {
                return null;
            }

            return panels.ToList().ConvertAll(x => MergeOverlapApertures(x, validateConstruction, minArea, tolerance));
        }

        public static AdjacencyCluster MergeOverlapApertures(this AdjacencyCluster adjacencyCluster, bool validateConstruction = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if (panels == null)
                return null;

            panels = MergeOverlapApertures(panels, validateConstruction, minArea, tolerance);

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            if (panels != null && panels.Count != 0)
            {
                panels.ForEach(x => result.AddObject(x));
            }

            return result;
        }

        public static AnalyticalModel MergeOverlapApertures(this AnalyticalModel analyticalModel, bool validateConstruction = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
                return null;

            adjacencyCluster = MergeOverlapApertures(adjacencyCluster, validateConstruction, minArea, tolerance);

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        public static Panel MergeOverlapApertures(this Panel panel, bool validateConstruction = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null)
                return null;

            Panel panel_New = new Panel(panel);

            List<Aperture> apertures = panel_New.Apertures;
            if (apertures == null || apertures.Count < 2)
            {
                return panel_New;
            }

            List<List<Aperture>> aperturesList = new List<List<Aperture>>();
            if(validateConstruction)
            {
                Dictionary<string, List<Aperture>> dictionary = new Dictionary<string, List<Aperture>>();
                foreach(Aperture aperture in apertures)
                {
                    ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                    string name = apertureConstruction?.Name;
                    if (name == null)
                        name = string.Empty;

                    if(!dictionary.TryGetValue(name, out List<Aperture> apertures_Temp))
                    {
                        apertures_Temp = new List<Aperture>();
                        dictionary[name] = apertures_Temp;
                    }

                    apertures_Temp.Add(aperture);

                }

                foreach (List<Aperture> apertures_Temp in dictionary.Values)
                    aperturesList.Add(apertures_Temp);
            }
            else
            {
                aperturesList.Add(apertures);
            }


            foreach(List<Aperture> apertures_Temp in aperturesList)
            {
                List<Aperture> apertures_Merged = MergeOverlapApertures(apertures_Temp, minArea, tolerance);
                if(apertures_Merged == null || apertures_Merged.Count == 0)
                {
                    continue;
                }

                panel_New.RemoveApertures();

                foreach(Aperture aperture_New in apertures_Merged)
                {
                    panel_New.AddAperture(aperture_New);
                }
            }

            return panel_New;

        }

        public static List<Aperture> MergeOverlapApertures(this IEnumerable<Aperture> apertures, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(apertures == null)
            {
                return null;
            }


            List<Tuple<Aperture, Face3D, Point3D>> tuples = new List<Tuple<Aperture, Face3D, Point3D>>();
            foreach(Aperture aperture in apertures)
            {
                Face3D face3D = aperture?.GetFace3D();
                if(face3D == null || !face3D.IsValid())
                {
                    continue;
                }

                if(face3D.GetArea() < minArea)
                {
                    continue;
                }

                Point3D internalPoint3D = face3D.InternalPoint3D(tolerance);
                if (internalPoint3D == null || !internalPoint3D.IsValid())
                {
                    continue;
                }

                tuples.Add(new Tuple<Aperture, Face3D, Point3D>(aperture, face3D, internalPoint3D));
            }

            List<Aperture> result = new List<Aperture>();
            while(tuples.Count != 0)
            {
                Plane plane = tuples[0].Item2.GetPlane();

                List<Tuple<Aperture, Face3D, Point3D>> tuples_Coplanar = tuples.FindAll(x => x.Item2.GetPlane().Coplanar(plane, tolerance));
                if(tuples_Coplanar == null || tuples_Coplanar.Count == 0)
                {
                    result.Add(tuples[0].Item1);
                    tuples.RemoveAt(0);
                    continue;
                }

                tuples_Coplanar.ForEach(x => tuples.Remove(x));

                if(tuples_Coplanar.Count == 1)
                {
                    result.Add(tuples_Coplanar[0].Item1);
                    continue;
                }

                List<Face2D> face2Ds_Union = new List<Face2D>();
                foreach(Tuple<Aperture, Face3D, Point3D> tuple in tuples_Coplanar)
                {
                    Face2D face2D = plane.Convert(tuple.Item2);

                    List<Face2D> face2Ds_Intersection = new List<Face2D>() { face2D };
                    foreach(Face2D face2D_Union in face2Ds_Union)
                    {
                        List<Face2D> face2Ds_Intersection_Temp = new List<Face2D>();
                        foreach (Face2D face2D_Intersection in face2Ds_Intersection)
                        {
                            List<Face2D> face2Ds_Difference = face2D_Intersection.Difference(face2D_Union, tolerance);
                            if(face2Ds_Difference != null && face2Ds_Difference.Count != 0)
                            {
                                face2Ds_Intersection_Temp.AddRange(face2Ds_Difference);
                            }
                        }

                        face2Ds_Intersection = face2Ds_Intersection_Temp;
                    }

                    ApertureConstruction apertureConstruction = tuple.Item1.ApertureConstruction;

                    foreach (Face2D face2D_Intersection in face2Ds_Intersection)
                    {
                        if (face2D_Intersection.GetArea() < tolerance)
                        {
                            continue;
                        }

                        Face3D face3D = plane.Convert(face2D_Intersection);

                        Aperture aperture = new Aperture(apertureConstruction, face3D);
                        result.Add(aperture);

                        face2Ds_Union.Add(face2D_Intersection);
                    }

                    face2Ds_Union = face2Ds_Union.Union(tolerance);
                }
            }

            return result;
        }
    }
}