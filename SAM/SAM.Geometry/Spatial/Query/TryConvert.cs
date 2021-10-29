using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool TryConvert<T>(this ISAMGeometry3D sAMGeometry3D, out List<T> sAMGeometry3Ds) where T : ISAMGeometry3D
        {
            sAMGeometry3Ds = null;
            if(sAMGeometry3D == null)
            {
                return false;
            }

            if(typeof(T) == typeof(Face3D))
            {
                if(sAMGeometry3D is Shell)
                {
                    List<Face3D> face3Ds = ((Shell)sAMGeometry3D).Face3Ds;
                    
                    if(face3Ds != null)
                    {
                        sAMGeometry3Ds = new List<T>();
                        foreach (Face3D face3D in face3Ds)
                        {
                            if(face3D is T)
                            {
                                sAMGeometry3Ds.Add((T)(object)face3D);
                            }
                        }
                        
                    }

                    return true;
                }

                if(sAMGeometry3D is Face3D)
                {
                    sAMGeometry3Ds = new List<T>() { (T)sAMGeometry3D };
                    return true;
                }

                if(sAMGeometry3D is IClosedPlanar3D)
                {
                    sAMGeometry3Ds = new List<T>() { (T)(object)(new Face3D((IClosedPlanar3D)sAMGeometry3D))};
                }

                if(sAMGeometry3D is IFace3DObject)
                {
                    sAMGeometry3Ds = new List<T>() { (T)(object)((IFace3DObject)sAMGeometry3D).Face3D };
                }
            }

            return false;
        }
    }
}