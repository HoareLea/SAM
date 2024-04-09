using Grasshopper.Kernel.Types;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Query
    {
        public static bool TryGetSAMGeometries<T>(this GH_ObjectWrapper objectWrapper, out List<T> sAMGeometries) where T : ISAMGeometry
        {
            sAMGeometries = null;

            if (objectWrapper == null || objectWrapper.Value == null)
                return false;

            if (objectWrapper.Value is T)
            {
                sAMGeometries = new List<T>() { (T)objectWrapper.Value };
                return true;
            }

            if (objectWrapper.Value is GooSAMGeometry)
            {
                ISAMGeometry sAMGeometry = ((GooSAMGeometry)objectWrapper.Value).Value;
                if (sAMGeometry is T)
                {
                    sAMGeometries = new List<T>() { (T)sAMGeometry };
                    return true;
                }
                else if (typeof(T) == typeof(Face3D))
                {
                    if (sAMGeometry is IFace3DObject)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((IFace3DObject)sAMGeometry).Face3D };
                        return true;
                    }
                    else if (sAMGeometry is Shell)
                    {
                        List<Face3D> face3Ds = ((Shell)sAMGeometry).Face3Ds;
                        if (face3Ds != null)
                        {
                            sAMGeometries = new List<T>(face3Ds.ConvertAll(x => (T)(object)x));
                        }

                        return true;
                    }
                    else if (sAMGeometry is IClosedPlanar3D)
                    {
                        sAMGeometries = new List<T>() { (T)(object)Spatial.Create.Face3D((IClosedPlanar3D)sAMGeometry) };
                        return true;
                    }
                    else if (sAMGeometry is IFace3DObject)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((IFace3DObject)sAMGeometry).Face3D };
                        return true;
                    }
                    else if (sAMGeometry is Mesh3D)
                    {
                        sAMGeometries = ((Mesh3D)sAMGeometry).GetTriangles()?.ConvertAll(x => (T)(object)(new Face3D(x)));
                        return true;
                    }
                }
                else if (typeof(T) == typeof(Point3D))
                {
                    if (sAMGeometry is Point3D)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((Point3D)sAMGeometry) };
                        return true;
                    }
                }
                else if (typeof(T) == typeof(Plane))
                {
                    if (sAMGeometry is Plane)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((Plane)sAMGeometry) };
                        return true;
                    }
                }
            }

            object @object = null;

            if (objectWrapper.Value is IGH_GeometricGoo)
            {
                @object = Convert.ToSAM(objectWrapper.Value as dynamic);
                if (typeof(T) == typeof(Face3D))
                {
                    if (@object is Shell)
                    {
                        @object = ((Shell)@object).Face3Ds;
                    }
                    else if (@object is Mesh3D)
                    {
                        @object = ((Mesh3D)@object).GetTriangles().ConvertAll(x => new Face3D(x));
                    }
                    else if (@object is IClosedPlanar3D)
                    {
                        sAMGeometries = new List<T>() { (T)(object)Spatial.Create.Face3D((IClosedPlanar3D)@object) };
                        return true;
                    }
                    else if (@object is IFace3DObject)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((IFace3DObject)@object).Face3D };
                        return true;
                    }
                    else if (@object is Mesh3D)
                    {
                        sAMGeometries = ((Mesh3D)@object).GetTriangles()?.ConvertAll(x => (T)(object)(new Face3D(x)));
                        return true;
                    }
                    else if (@object is Polycurve3D)
                    {
                        if(Polycurve3D.TryGetPolyline3D((Polycurve3D)@object, out Polyline3D polyline3D))
                        {
                            sAMGeometries = new List<T>() { (T)(object)Spatial.Create.Face3D(polyline3D?.ToPolygon3D()) };
                            return true;
                        }

                        //sAMGeometries = ((Spatial.Mesh3D)@object).GetTriangles()?.ConvertAll(x => (T)(object)(new Spatial.Face3D(x)));
                        //return true;
                    }
                    else if(@object is Polyline3D)
                    {
                        Polyline3D polyline3D = (Polyline3D)@object;
                        if (polyline3D.IsClosed())
                        {
                            List<Point3D> point3Ds = polyline3D.Points;
                            sAMGeometries = new List<T>() { (T)(object)Spatial.Create.Face3D(new Polygon3D(point3Ds)) };
                            return true;
                        }
                    }

                }
                else if(typeof(T) == typeof(Point3D))
                {
                    if (@object is Point3D)
                    {
                        sAMGeometries = new List<T>() { (T)(object)(Point3D)@object };
                        return true;
                    }
                }
                else if (typeof(T) == typeof(Plane))
                {
                    if (@object is Plane)
                    {
                        sAMGeometries = new List<T>() { (T)(object)(Plane)@object };
                        return true;
                    }
                }
                else if(typeof(T) == typeof(ISegmentable3D))
                {
                    if (@object is Polycurve3D)
                    {
                        if (Polycurve3D.TryGetPolyline3D((Polycurve3D)@object, out Polyline3D polyline3D) && polyline3D != null)
                        {
                            sAMGeometries = new List<T>() { (T)(object)polyline3D };
                            return true;
                        }
                    }
                }

                if (@object is IEnumerable)
                {

                    IEnumerable<ISAMGeometry> sAMGeometries_Temp = ((IEnumerable)@object).Cast<ISAMGeometry>();
                    if (sAMGeometries_Temp != null && sAMGeometries_Temp.Count() > 0)
                    {
                        if (typeof(T) == typeof(Face3D))
                        {
                            sAMGeometries = new List<T>();
                            foreach (ISAMGeometry sAMGeometry in sAMGeometries_Temp)
                            {
                                if (sAMGeometry is T)
                                {
                                    sAMGeometries.Add((T)sAMGeometry);
                                }
                                else
                                {
                                    if (sAMGeometry is Shell)
                                    {
                                        sAMGeometries.AddRange(((Shell)sAMGeometry).Face3Ds?.Cast<T>());
                                    }
                                    else if (sAMGeometry is Mesh3D)
                                    {
                                        sAMGeometries.AddRange(((Mesh3D)sAMGeometry).GetTriangles().ConvertAll(x => new Face3D(x)).Cast<T>());
                                    }
                                    else if (sAMGeometry is IClosedPlanar3D)
                                    {
                                        sAMGeometries.Add((T)(object)Spatial.Create.Face3D((IClosedPlanar3D)sAMGeometry));
                                        return true;
                                    }
                                    else if (sAMGeometry is IFace3DObject)
                                    {
                                        sAMGeometries.Add((T)(object)((IFace3DObject)sAMGeometry).Face3D);
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            sAMGeometries = sAMGeometries_Temp.ToList().FindAll(x => x is T).Cast<T>().ToList();
                        }

                        return sAMGeometries.Count > 0;
                    }
                }
                else if (@object is T)
                {
                    sAMGeometries = new List<T>() { (T)@object };
                    return true;
                }
            }

            @object = objectWrapper.Value;
            if (@object is IGH_Goo)
            {
                @object = (@object as dynamic).Value;
            }

            if (@object is IFace3DObject && typeof(T) == typeof(Face3D))
            {
                sAMGeometries = new List<T>() { (T)(object)((IFace3DObject)@object).Face3D };
                return true;
            }

            if(@object is Polycurve3D && typeof(T) == typeof(ISegmentable3D))
            {
                if (Polycurve3D.TryGetPolyline3D((Polycurve3D)@object, out Polyline3D polyline3D) && polyline3D != null)
                {
                    sAMGeometries = new List<T>() { (T)(object)polyline3D };
                    return true;
                }
            }

            return false;

        }

        public static bool TryGetSAMGeometries<T>(this IEnumerable<GH_ObjectWrapper> objectWrappers, out List<T> sAMGeometries) where T : ISAMGeometry
        {
            sAMGeometries = null;

            if(objectWrappers == null)
            {
                return false;
            }

            sAMGeometries = new List<T>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(!TryGetSAMGeometries(objectWrapper, out List<T> sAMGeometries_Temp) || sAMGeometries_Temp == null || sAMGeometries_Temp.Count == 0)
                {
                    continue;
                }

                foreach(T sAMGeometry in sAMGeometries_Temp)
                {
                    sAMGeometries.Add(sAMGeometry);
                }
            }

            return sAMGeometries != null && sAMGeometries.Count > 0;
        }
    }
}