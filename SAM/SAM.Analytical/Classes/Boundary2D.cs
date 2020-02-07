using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public class Boundary2D : SAMObject
    {
        private Edge2DLoop edge2DLoop;
        private List<Edge2DLoop> internalEdge2DLoops;

        public Boundary2D(Face face)
            : base()
        {
            edge2DLoop = new Edge2DLoop(face.Boundary);
        }

        public Boundary2D(IClosedPlanar3D closedPlanar3D)
        {
            edge2DLoop = new Edge2DLoop(closedPlanar3D);
        }

        public Boundary2D(JObject jObject)
            : base(jObject)
        {

        }

        public Edge2DLoop Edge2DLoop
        {
            get
            {
                return new Edge2DLoop(edge2DLoop);
            }
        }

        public List<Edge2DLoop> InternalEdge2DLoops
        {
            get
            {
                if (internalEdge2DLoops == null)
                    return null;

                return internalEdge2DLoops.ConvertAll(x => new Edge2DLoop(x));
            }
        }

        public Edge3DLoop GetEdge3DLoop(Plane plane)
        {
            return new Edge3DLoop(plane, edge2DLoop);
        }

        public List<Edge3DLoop> GetInternalEdge3DLoops(Plane plane)
        {
            if (internalEdge2DLoops == null)
                return null;

            return internalEdge2DLoops.ConvertAll(x => new Edge3DLoop(plane, x));
        }

        public List<IClosedPlanar3D> GetInternalClosedPlanar3Ds(Plane plane)
        {
            if (internalEdge2DLoops == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (Edge2DLoop edge2DLoop in internalEdge2DLoops)
            {
                Polygon3D polygon3D = new Polygon3D(edge2DLoop.Edge2Ds.ConvertAll(x => ((ICurve3D)plane.Convert(x.Curve2D)).GetStart()));
                result.Add(polygon3D);
            }

            return result;
        }

        public Face GetFace(Plane plane)
        {
            return new Face(plane, edge2DLoop.GetClosed2D());
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            edge2DLoop = new Edge2DLoop(jObject.Value<JObject>("Edge2DLoop"));
            if (jObject.ContainsKey("InternalEdge2DLoops"))
                internalEdge2DLoops = Core.Create.IJSAMObjects<Edge2DLoop>(jObject.Value<JArray>("InternalEdge2DLoops"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            jObject.Add("Edge2DLoop", edge2DLoop.ToJObject());
            if (internalEdge2DLoops != null)
                jObject.Add("InternalEdge2DLoops", Core.Create.JArray(internalEdge2DLoops));
            
            return jObject;
        }


        public static bool TryGetBoundary2D(List<Edge2DLoop> faces, out PlanarBoundary3D planarBoundary3D)
        {

            throw new NotImplementedException();
            //planarBoundary3D = null;

            //if (faces == null || faces.Count() == 0)
            //    return false;

            //Face face_Max = null;
            //double area_Max = double.MinValue;
            //foreach (Face face in faces)
            //{
            //    double area = face.GetArea();
            //    if (area > area_Max)
            //    {
            //        area_Max = area;
            //        face_Max = face;
            //    }

            //}

            //if (face_Max == null)
            //    return false;

            //planarBoundary3D = new PlanarBoundary3D(face_Max);
            //foreach (Face face in faces)
            //{
            //    if (face == face_Max)
            //        continue;

            //    if (face_Max.Inside(face))
            //    {
            //        if (planarBoundary3D.internalEdge2DLoops == null)
            //            planarBoundary3D.internalEdge2DLoops = new List<Edge2DLoop>();

            //        planarBoundary3D.internalEdge2DLoops.Add(new Edge2DLoop(face));
            //    }
            //}

            //return true;
        }

        public static bool TryGetBoundary2Ds(List<Edge2DLoop> faces, out List<PlanarBoundary3D> planarBoundary3Ds)
        {
            throw new NotImplementedException();


            //planarBoundary3Ds = null;

            //if (faces == null || faces.Count() == 0)
            //    return false;

            //planarBoundary3Ds = new List<PlanarBoundary3D>();


            //if (faces.Count() == 1)
            //{
            //    planarBoundary3Ds.Add(new PlanarBoundary3D(faces.First()));
            //    return true;
            //}

            //List<Face> faceList = faces.ToList();
            //faceList.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            //while(faceList.Count > 0)
            //{
            //    List<Face> faceList_ToRemove = new List<Face>();

            //    Face face = faceList.First();
            //    PlanarBoundary3D planarBoundary3D = new PlanarBoundary3D(face);
            //    planarBoundary3Ds.Add(planarBoundary3D);
            //    faceList_ToRemove.Add(face);

            //    Geometry.Orientation orientation = Geometry.Orientation.Undefined;
            //    if(face.Boundary is Geometry.Planar.Polygon2D)
            //        orientation = ((Geometry.Planar.Polygon2D)face.Boundary).GetOrientation();

            //    foreach (Face face_Internal in faces)
            //    {
            //        if (face_Internal == face)
            //            continue;

            //        if (!face.Inside(face_Internal))
            //            continue;

            //        if (planarBoundary3D.internalEdge2DLoops == null)
            //            planarBoundary3D.internalEdge2DLoops = new List<Edge2DLoop>();

            //        Geometry.Planar.IClosed2D closed2D = planarBoundary3D.plane.Convert(face_Internal.ToClosedPlanar3D());
            //        if(orientation != Geometry.Orientation.Undefined)
            //        {
            //            if (closed2D is Geometry.Planar.Polygon2D)
            //            {
            //                Geometry.Planar.Polygon2D polygon2D = (Geometry.Planar.Polygon2D)closed2D;
            //                Geometry.Orientation orientation_Internal = polygon2D.GetOrientation();
            //                if (orientation == orientation_Internal)
            //                    polygon2D.Reverse();
            //            }
            //        }

            //        planarBoundary3D.internalEdge2DLoops.Add(new Edge2DLoop(new Face(planarBoundary3D.plane, closed2D)));
            //        faceList_ToRemove.Add(face_Internal);
            //    }

            //    faceList_ToRemove.ForEach(x => faceList.Remove(x));
            //}

            //return true;
        }
    }
}
