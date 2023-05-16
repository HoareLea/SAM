using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Polygonize;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static ISAMGeometry2D ToSAM(this NetTopologySuite.Geometries.Geometry geometry, double tolerance = Core.Tolerance.Distance)
        {
            return Convert.ToSAM(geometry as dynamic, tolerance);
        }

        public static List<ISAMGeometry2D> ToSAM(this IEnumerable<string> lines_NTS, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (lines_NTS == null || lines_NTS.Count() == 0)
                return null;

            List<ISAMGeometry2D> result = new List<ISAMGeometry2D>();

            PrecisionModel precisionModel = new PrecisionModel(1.0 / tolerance);
            //NetTopologySuite.Geometries.GeometryFactory geometryFactory = new NetTopologySuite.Geometries.GeometryFactory(precisionModel);

            // Create a new NtsGeometryServices instance with the new GeometryFactory
            NetTopologySuite.NtsGeometryServices ntsGeometryServices = new NetTopologySuite.NtsGeometryServices(precisionModel);

            WKTReader wKTReader = new WKTReader(ntsGeometryServices);
            foreach (string line_NTS in lines_NTS)
            {
                if (string.IsNullOrWhiteSpace(line_NTS))
                    continue;

                NetTopologySuite.Geometries.Geometry geometry = wKTReader.Read(line_NTS);
                if (geometry == null)
                    continue;

                ISAMGeometry2D sAMGeometry2D = geometry.ToSAM(tolerance);
                if (sAMGeometry2D == null)
                    continue;

                result.Add(sAMGeometry2D);
            }

            return result;
        }


        //public static List<ISAMGeometry2D> ToSAM(this IEnumerable<string> lines_NTS, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (lines_NTS == null || lines_NTS.Count() == 0)
        //        return null;

        //    List<ISAMGeometry2D> result = new List<ISAMGeometry2D>();

        //    WKTReader wKTReader = new WKTReader(new GeometryFactory(new PrecisionModel(1 / tolerance)));

        //    //NetTopologySuite.NtsGeometryServices ntsGeometryServices = NetTopologySuite.NtsGeometryServices.Instance;
        //    //if(ntsGeometryServices?.DefaultPrecisionModel != null && ntsGeometryServices.DefaultPrecisionModel.Scale != 1.0 / tolerance)
        //    //{
        //    //    ntsGeometryServices.DefaultPrecisionModel.Scale = 1.0 / tolerance;
        //    //}

        //    //WKTReader wKTReader = new WKTReader(ntsGeometryServices);
        //    foreach(string line_NTS in lines_NTS)
        //    {
        //        if (string.IsNullOrWhiteSpace(line_NTS))
        //            continue;

        //        NetTopologySuite.Geometries.Geometry geometry = wKTReader.Read(line_NTS);
        //        if (geometry == null)
        //            continue;

        //        ISAMGeometry2D sAMGeometry2D = geometry.ToSAM(tolerance);
        //        if (sAMGeometry2D == null)
        //            continue;

        //        result.Add(sAMGeometry2D);
        //    }

        //    return result;

        //}
    }
}