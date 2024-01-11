using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static List<T> ToSAM<T>(this NetTopologySuite.Geometries.Geometry geometry, double tolerance = Core.Tolerance.Distance) where T : ISAMGeometry2D
        {
            if(geometry == null)
            {
                return null;
            }

            List<T> result = new List<T>();

            if (geometry is GeometryCollection)
            {
                foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in (GeometryCollection)geometry)
                {
                    List<T> sAMGeometry2Ds = ToSAM<T>(geometry_Temp, tolerance);
                    if (sAMGeometry2Ds == null)
                    {
                        continue;
                    }

                    result.AddRange(sAMGeometry2Ds);
                }

            }
            else if (geometry is MultiPoint)
            {
                List<Point2D> point2Ds = ToSAM((MultiPoint)geometry, tolerance);
                if (point2Ds != null)
                {
                    foreach(Point2D point2D in point2Ds)
                    {
                        if(point2D is T)
                        {
                            result.Add((T)(object)point2D);
                        }
                    }
                }
            }
            else
            {
                ISAMGeometry2D sAMGeometry2D = Convert.ToSAM(geometry as dynamic, tolerance);
                if (sAMGeometry2D is T)
                {
                    result.Add((T)sAMGeometry2D);
                }
            }
             
            return result;
        }

        public static List<T> ToSAM<T>(this IEnumerable<string> lines_NTS, double tolerance = Core.Tolerance.MicroDistance) where T: ISAMGeometry2D
        {
            if (lines_NTS == null || lines_NTS.Count() == 0)
                return null;

            List<T> result = new List<T>();

            PrecisionModel precisionModel = new PrecisionModel(1.0 / tolerance);
            //NetTopologySuite.Geometries.GeometryFactory geometryFactory = new NetTopologySuite.Geometries.GeometryFactory(precisionModel);

            // Create a new NtsGeometryServices instance with the new GeometryFactory
            NetTopologySuite.NtsGeometryServices ntsGeometryServices = new NetTopologySuite.NtsGeometryServices(precisionModel);

            WKTReader wKTReader = new WKTReader(ntsGeometryServices);
            foreach (string line_NTS in lines_NTS)
            {
                if (string.IsNullOrWhiteSpace(line_NTS))
                {
                    continue;
                }

                NetTopologySuite.Geometries.Geometry geometry = wKTReader.Read(line_NTS);
                if (geometry == null)
                {
                    continue;
                }

                ToSAM<T>(geometry, tolerance)?.ForEach(x => result.Add(x));
            }

            return result;
        }
    }
}