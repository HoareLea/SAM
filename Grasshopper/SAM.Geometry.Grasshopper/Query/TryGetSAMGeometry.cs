using Grasshopper.Kernel.Types;
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
                else if (typeof(T) == typeof(Spatial.Face3D))
                {
                    if (sAMGeometry is Spatial.IFace3DObject)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((Spatial.IFace3DObject)sAMGeometry).Face3D };
                        return true;
                    }
                    else if (sAMGeometry is Spatial.Shell)
                    {
                        List<Spatial.Face3D> face3Ds = ((Spatial.Shell)sAMGeometry).Face3Ds;
                        if (face3Ds != null)
                        {
                            sAMGeometries = new List<T>(face3Ds.ConvertAll(x => (T)(object)x));
                        }

                        return true;
                    }
                    else if (sAMGeometry is Spatial.IClosedPlanar3D)
                    {
                        sAMGeometries = new List<T>() { (T)(object)Spatial.Create.Face3D((Spatial.IClosedPlanar3D)sAMGeometry) };
                        return true;
                    }
                    else if (sAMGeometry is Spatial.IFace3DObject)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((Spatial.IFace3DObject)sAMGeometry).Face3D };
                        return true;
                    }
                    else if (sAMGeometry is Spatial.Mesh3D)
                    {
                        sAMGeometries = ((Spatial.Mesh3D)sAMGeometry).GetTriangles()?.ConvertAll(x => (T)(object)(new Spatial.Face3D(x)));
                        return true;
                    }
                }
            }

            object @object = null;

            if (objectWrapper.Value is IGH_GeometricGoo)
            {
                @object = Convert.ToSAM(objectWrapper.Value as dynamic);
                if (typeof(T) == typeof(Spatial.Face3D))
                {
                    if (@object is Spatial.Shell)
                    {
                        @object = ((Spatial.Shell)@object).Face3Ds;
                    }
                    else if (@object is Spatial.Mesh3D)
                    {
                        @object = ((Spatial.Mesh3D)@object).GetTriangles().ConvertAll(x => new Spatial.Face3D(x));
                    }
                    else if (@object is Spatial.IClosedPlanar3D)
                    {
                        sAMGeometries = new List<T>() { (T)(object)Spatial.Create.Face3D((Spatial.IClosedPlanar3D)@object) };
                        return true;
                    }
                    else if (@object is Spatial.IFace3DObject)
                    {
                        sAMGeometries = new List<T>() { (T)(object)((Spatial.IFace3DObject)@object).Face3D };
                        return true;
                    }
                    else if (@object is Spatial.Mesh3D)
                    {
                        sAMGeometries = ((Spatial.Mesh3D)@object).GetTriangles()?.ConvertAll(x => (T)(object)(new Spatial.Face3D(x)));
                        return true;
                    }
                }

                if (@object is IEnumerable)
                {

                    IEnumerable<ISAMGeometry> sAMGeometries_Temp = ((IEnumerable)@object).Cast<ISAMGeometry>();
                    if (sAMGeometries_Temp != null && sAMGeometries_Temp.Count() > 0)
                    {
                        if (typeof(T) == typeof(Spatial.Face3D))
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
                                    if (sAMGeometry is Spatial.Shell)
                                    {
                                        sAMGeometries.AddRange(((Spatial.Shell)sAMGeometry).Face3Ds?.Cast<T>());
                                    }
                                    else if (sAMGeometry is Spatial.Mesh3D)
                                    {
                                        sAMGeometries.AddRange(((Spatial.Mesh3D)sAMGeometry).GetTriangles().ConvertAll(x => new Spatial.Face3D(x)).Cast<T>());
                                    }
                                    else if (sAMGeometry is Spatial.IClosedPlanar3D)
                                    {
                                        sAMGeometries.Add((T)(object)Spatial.Create.Face3D((Spatial.IClosedPlanar3D)sAMGeometry));
                                        return true;
                                    }
                                    else if (sAMGeometry is Spatial.IFace3DObject)
                                    {
                                        sAMGeometries.Add((T)(object)((Spatial.IFace3DObject)sAMGeometry).Face3D);
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

            if (@object is Spatial.IFace3DObject && typeof(T) == typeof(Spatial.Face3D))
            {
                sAMGeometries = new List<T>() { (T)(object)((Spatial.IFace3DObject)@object).Face3D };
                return true;
            }

            return false;

        }
    }
}