using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Object.Spatial
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

            if(typeof(T) == typeof(Segment3D))
            {
                if (sAMGeometry3D is Face3D)
                {
                    Face3D face3D = (Face3D)sAMGeometry3D;
                    List<IClosedPlanar3D> closedPlanar3Ds = face3D.GetEdge3Ds();
                    if (closedPlanar3Ds == null)
                    {
                        return true;
                    }

                    sAMGeometry3Ds = new List<T>();

                    foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
                    {
                        if(closedPlanar3D == null)
                        {
                            continue;
                        }

                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if(segmentable3D == null)
                        {
                            return false;
                        }

                        sAMGeometry3Ds.AddRange(segmentable3D.GetSegments().Cast<T>());
                    }

                    return true;
                }

                if(typeof(ISegmentable3D).IsAssignableFrom(sAMGeometry3D.GetType()))
                {
                    ISegmentable3D segmentable3D = sAMGeometry3D as ISegmentable3D;
                    sAMGeometry3Ds.AddRange(segmentable3D.GetSegments().Cast<T>());
                }
            }

            return false;
        }
    }
}