using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D SplitEdges(this Face3D face3D_Split, Face3D face3D_Splitting, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D_Split == null || face3D_Splitting == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Split = face3D_Split.GetBoundingBox();
            if (boundingBox3D_Split == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Splitting = face3D_Splitting.GetBoundingBox();
            if (boundingBox3D_Splitting == null)
            {
                return null;
            }

            if (!boundingBox3D_Split.InRange(boundingBox3D_Splitting, tolerance))
            {
                return new Face3D(face3D_Split);
            }

            List<IClosedPlanar3D> edges_Splitting = face3D_Splitting.GetEdge3Ds();
            if (edges_Splitting == null || edges_Splitting.Count == 0)
            {
                return new Face3D(face3D_Split);
            }

            List<Segment3D> segment3Ds_Splitting = new List<Segment3D>();
            foreach (IClosedPlanar3D edge_Splitting in edges_Splitting)
            {
                if (!(edge_Splitting is ISegmentable3D))
                {
                    throw new System.NotImplementedException();
                }

                segment3Ds_Splitting.AddRange(((ISegmentable3D)edge_Splitting).GetSegments());
            }

            Plane plane = face3D_Split.GetPlane();

            bool updated = false;

            //ExternalEdge
            IClosedPlanar3D externalEdge = face3D_Split.GetExternalEdge3D();
            if (externalEdge == null)
            {
                return null;
            }

            ISegmentable3D externalEdge_Segmentable3D = externalEdge as ISegmentable3D;
            if (externalEdge_Segmentable3D == null)
            {
                throw new System.NotImplementedException();
            }

            List<Segment3D> segment3Ds_Temp = new List<Segment3D>(segment3Ds_Splitting);
            segment3Ds_Temp.AddRange(externalEdge_Segmentable3D.GetSegments());

            segment3Ds_Temp = segment3Ds_Temp.Split();
            if (segment3Ds_Temp != null && segment3Ds_Temp.Count != 0)
            {
                Polygon3D polygon3D = new Polygon3D(plane, externalEdge_Segmentable3D.GetPoints().ConvertAll(x => plane.Convert(x)));

                List<Point3D> point3Ds_Polygon3D = polygon3D.GetPoints();

                List<Point3D> point3Ds = new List<Point3D>();
                foreach (Segment3D segment3D_Temp in segment3Ds_Temp)
                {
                    List<Point3D> point3Ds_Temp = segment3D_Temp?.GetPoints();
                    if(point3Ds_Temp == null || point3Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach(Point3D point3D in point3Ds_Temp)
                    {
                        if (point3D == null || !polygon3D.On(point3D, tolerance))
                        {
                            continue;
                        }
                        
                        if (point3Ds_Polygon3D.Find(x => x.Distance(point3D) < tolerance) != null)
                        {
                            continue;
                        }

                        if (point3Ds.Find(x => x.Distance(point3D) < tolerance) != null)
                        {
                            continue;
                        }

                        point3Ds.Add(point3D);
                    }
                }

                if (point3Ds != null && point3Ds.Count > 0)
                {
                    point3Ds.ForEach(x => polygon3D.InsertClosest(x, tolerance));

                    externalEdge = polygon3D;
                    updated = true;
                }
            }


            //InternalEdges
            List<IClosedPlanar3D> internalEdges = face3D_Split.GetInternalEdge3Ds();
            if (internalEdges != null && internalEdges.Count != 0)
            {
                for (int i = 0; i < internalEdges.Count; i++)
                {
                    IClosedPlanar3D internalEdge = internalEdges[i];
                    if (externalEdge == null)
                    {
                        continue;
                    }

                    ISegmentable3D internalEdge_Segmentable3D = externalEdge as ISegmentable3D;
                    if (internalEdge_Segmentable3D == null)
                    {
                        throw new System.NotImplementedException();
                    }

                    segment3Ds_Temp = new List<Segment3D>(segment3Ds_Splitting);
                    segment3Ds_Temp.AddRange(internalEdge_Segmentable3D.GetSegments());

                    segment3Ds_Temp = segment3Ds_Temp.Split();
                    if (segment3Ds_Temp != null && segment3Ds_Temp.Count != 0)
                    {
                        Polygon3D polygon3D = new Polygon3D(plane, internalEdge_Segmentable3D.GetPoints().ConvertAll(x => plane.Convert(x)));

                        List<Point3D> point3Ds_Polygon3D = polygon3D.GetPoints();

                        List<Point3D> point3Ds = new List<Point3D>();
                        foreach (Segment3D segment3D_Temp in segment3Ds_Temp)
                        {
                            List<Point3D> point3Ds_Temp = segment3D_Temp?.GetPoints();
                            if (point3Ds_Temp == null || point3Ds_Temp.Count == 0)
                            {
                                continue;
                            }

                            foreach (Point3D point3D in point3Ds_Temp)
                            {
                                if (point3D == null || !polygon3D.On(point3D, tolerance))
                                {
                                    continue;
                                }

                                if (point3Ds_Polygon3D.Find(x => x.Distance(point3D) < tolerance) != null)
                                {
                                    continue;
                                }

                                if (point3Ds.Find(x => x.Distance(point3D) < tolerance) != null)
                                {
                                    continue;
                                }

                                point3Ds.Add(point3D);
                            }
                        }

                        if (point3Ds != null && point3Ds.Count > 0)
                        {
                            point3Ds.ForEach(x => polygon3D.InsertClosest(x, tolerance));

                            internalEdges[i] = polygon3D;
                            updated = true;
                        }
                    }
                }
            }

            if(updated)
            {
                return plane.Convert(Planar.Create.Face2D(plane.Convert(externalEdge), internalEdges?.ConvertAll(x => plane.Convert(x))));
            }

            return new Face3D(face3D_Split);
        }
    }
}