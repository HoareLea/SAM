// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Query
    {
        /// <summary>
        /// Tries to extract or convert SAM geometries of type <typeparamref name="T"/> from a Grasshopper object wrapper.
        /// </summary>
        /// <typeparam name="T">The requested SAM geometry type.</typeparam>
        /// <param name="objectWrapper">The Grasshopper object wrapper containing the geometry input.</param>
        /// <param name="sAMGeometries">When this method returns, contains the converted SAM geometries if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if one or more valid SAM geometries were retrieved or converted; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetSAMGeometries<T>(this GH_ObjectWrapper objectWrapper, out List<T> sAMGeometries) where T : ISAMGeometry
        {
            sAMGeometries = null;

            if (objectWrapper == null || objectWrapper.Value == null)
            {
                return false;
            }

            List<T> result = new List<T>();
            ProcessValue(objectWrapper.Value, result);

            if (result.Count > 0)
            {
                sAMGeometries = result;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to extract or convert SAM geometries of type <typeparamref name="T"/> from a collection of Grasshopper object wrappers.
        /// </summary>
        /// <typeparam name="T">The requested SAM geometry type.</typeparam>
        /// <param name="objectWrappers">The collection of Grasshopper object wrappers containing geometry inputs.</param>
        /// <param name="sAMGeometries">When this method returns, contains the converted SAM geometries if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if one or more valid SAM geometries were retrieved or converted; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetSAMGeometries<T>(this IEnumerable<GH_ObjectWrapper> objectWrappers, out List<T> sAMGeometries) where T : ISAMGeometry
        {
            sAMGeometries = null;

            if (objectWrappers == null)
            {
                return false;
            }

            List<T> result = new List<T>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if (objectWrapper == null || objectWrapper.Value == null)
                {
                    continue;
                }

                ProcessValue(objectWrapper.Value, result);
            }

            if (result.Count > 0)
            {
                sAMGeometries = result;
                return true;
            }

            return false;
        }

        private static void ProcessValue<T>(object value, List<T> result) where T : ISAMGeometry
        {
            if (value == null)
            {
                return;
            }

            if (value is GooSAMGeometry gooSAMGeometry)
            {
                value = gooSAMGeometry.Value;
                if (value == null)
                {
                    return;
                }
            }

            if (value is T directValue)
            {
                result.Add(directValue);
                return;
            }

            if (value is IGH_GeometricGoo geometricGoo)
            {
                object convertedGoo = ConvertGeometricGooToSAM(geometricGoo);
                if (convertedGoo != null)
                {
                    ProcessValue(convertedGoo, result);
                    return;
                }
            }

            if (value is IGH_Goo ghGoo)
            {
                object scriptVar = ghGoo.ScriptVariable();
                if (scriptVar != null && scriptVar != value)
                {
                    ProcessValue(scriptVar, result);
                    return;
                }
            }

            object rhinoConverted = ConvertRhinoGeometryToSAM(value);
            if (rhinoConverted != null)
            {
                ProcessValue(rhinoConverted, result);
                return;
            }

            Type targetType = typeof(T);

            // Special handling when T is Shell and value is a collection of faces (e.g. from an open Brep conversion)
            if (typeof(Shell).IsAssignableFrom(targetType) && value is IEnumerable faceCollection && !(value is string))
            {
                List<Face3D> faces = new List<Face3D>();
                foreach (object item in faceCollection)
                {
                    if (item is Face3D f)
                    {
                        faces.Add(f);
                    }
                    else if (item is IFace3DObject fObj && fObj.Face3D != null)
                    {
                        faces.Add(fObj.Face3D);
                    }
                }

                if (faces.Count > 0)
                {
                    Shell shell = new Shell(faces);
                    if (shell is T tShell)
                    {
                        result.Add(tShell);
                        return;
                    }
                }
            }

            // Note: SAM geometry objects do not implement IEnumerable.
            // The IEnumerable branch below processes collection inputs (e.g. List<ISAMGeometry>, GH_Structure, etc.).
            if (value is IEnumerable enumerable && !(value is string))
            {
                foreach (object item in enumerable)
                {
                    ProcessValue(item, result);
                }
                return;
            }

            ProcessSAMGeometryConversion(value, result);
        }

        private static object ConvertGeometricGooToSAM(IGH_GeometricGoo geometricGoo)
        {
            if (geometricGoo == null)
            {
                return null;
            }

            if (geometricGoo is GH_Curve ghCurve)
            {
                return ghCurve.ToSAM(true);
            }

            if (geometricGoo is GH_Surface ghSurface)
            {
                return ghSurface.ToSAM(true);
            }

            if (geometricGoo is GH_Brep ghBrep)
            {
                return ghBrep.ToSAM(true);
            }

            if (geometricGoo is GH_Mesh ghMesh)
            {
                return ghMesh.ToSAM();
            }

            if (geometricGoo is GH_Extrusion ghExtrusion)
            {
                return ghExtrusion.ToSAM_Shell();
            }

            if (geometricGoo is GH_Point ghPoint)
            {
                return ghPoint.ToSAM();
            }

            if (geometricGoo is GH_Plane ghPlane)
            {
                return ghPlane.ToSAM();
            }

            if (geometricGoo is GH_Vector ghVector)
            {
                return ghVector.ToSAM();
            }

            if (geometricGoo is GH_Line ghLine)
            {
                return ghLine.ToSAM();
            }

            if (geometricGoo is GH_Rectangle ghRectangle)
            {
                return ghRectangle.ToSAM();
            }

            if (geometricGoo is GH_Circle ghCircle)
            {
                return ghCircle.ToSAM();
            }

            return geometricGoo.ToSAM(true);
        }

        private static object ConvertRhinoGeometryToSAM(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is global::Rhino.Geometry.Point3d pt)
            {
                return Rhino.Convert.ToSAM(pt);
            }

            if (value is global::Rhino.Geometry.Plane plane)
            {
                return Rhino.Convert.ToSAM(plane);
            }

            if (value is global::Rhino.Geometry.Vector3d vector)
            {
                return Rhino.Convert.ToSAM(vector);
            }

            if (value is global::Rhino.Geometry.Line line)
            {
                return Rhino.Convert.ToSAM(line);
            }

            if (value is global::Rhino.Geometry.Polyline polyline)
            {
                return Rhino.Convert.ToSAM(polyline);
            }

            if (value is global::Rhino.Geometry.Rectangle3d rect)
            {
                return Rhino.Convert.ToSAM(rect);
            }

            if (value is global::Rhino.Geometry.Curve curve)
            {
                return Rhino.Convert.ToSAM(curve);
            }

            if (value is global::Rhino.Geometry.Brep brep)
            {
                return Rhino.Convert.ToSAM(brep);
            }

            if (value is global::Rhino.Geometry.Mesh mesh)
            {
                return Rhino.Convert.ToSAM(mesh);
            }

            if (value is global::Rhino.Geometry.Extrusion extrusion)
            {
                global::Rhino.Geometry.Brep extBrep = extrusion.ToBrep(true);
                if (extBrep != null)
                {
                    return Rhino.Convert.ToSAM(extBrep);
                }
            }

            return null;
        }

        private static List<IClosedPlanar3D> GetEdges(object value)
        {
            if (value is Face3D face3D)
            {
                return face3D.GetEdge3Ds();
            }

            if (value is IFace3DObject face3DObject && face3DObject.Face3D != null)
            {
                return face3DObject.Face3D.GetEdge3Ds();
            }

            if (value is Shell shell && shell.Face3Ds != null)
            {
                List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
                foreach (Face3D face in shell.Face3Ds)
                {
                    if (face != null)
                    {
                        List<IClosedPlanar3D> edges = face.GetEdge3Ds();
                        if (edges != null)
                        {
                            result.AddRange(edges);
                        }
                    }
                }
                return result;
            }

            if (value is IClosedPlanar3D closedPlanar)
            {
                return new List<IClosedPlanar3D> { closedPlanar };
            }

            return null;
        }

        private static void ProcessSAMGeometryConversion<T>(object value, List<T> result) where T : ISAMGeometry
        {
            if (value == null)
            {
                return;
            }

            Type targetType = typeof(T);

            // 1. Face3D
            if (typeof(Face3D).IsAssignableFrom(targetType))
            {
                if (value is IFace3DObject face3DObject && face3DObject.Face3D is T faceFromObj)
                {
                    result.Add(faceFromObj);
                    return;
                }

                if (value is Shell shell && shell.Face3Ds != null)
                {
                    foreach (Face3D face in shell.Face3Ds)
                    {
                        if (face is T tFace)
                        {
                            result.Add(tFace);
                        }
                    }
                    return;
                }

                if (value is Mesh3D mesh3D)
                {
                    List<Triangle3D> triangles = mesh3D.GetTriangles();
                    if (triangles != null)
                    {
                        foreach (Triangle3D triangle in triangles)
                        {
                            Face3D triFace = new Face3D(triangle);
                            if (triFace is T tFace)
                            {
                                result.Add(tFace);
                            }
                        }
                    }
                    return;
                }

                if (value is IClosedPlanar3D closedPlanar3D)
                {
                    Face3D face3D = Spatial.Create.Face3D(closedPlanar3D);
                    if (face3D is T tFace)
                    {
                        result.Add(tFace);
                    }
                    return;
                }

                if (value is Polycurve3D polycurve3D)
                {
                    if (Polycurve3D.TryGetPolyline3D(polycurve3D, out Polyline3D polyline3D) && polyline3D != null && polyline3D.IsClosed())
                    {
                        Face3D face3D = Spatial.Create.Face3D(polyline3D.ToPolygon3D());
                        if (face3D is T tFace)
                        {
                            result.Add(tFace);
                        }
                    }
                    return;
                }

                if (value is Polyline3D polyline && polyline.IsClosed())
                {
                    Face3D face3D = Spatial.Create.Face3D(polyline.ToPolygon3D());
                    if (face3D is T tFace)
                    {
                        result.Add(tFace);
                    }
                    return;
                }
            }

            // 2. Polyline3D
            if (typeof(Polyline3D).IsAssignableFrom(targetType))
            {
                if (value is Polycurve3D polycurve3D)
                {
                    if (Polycurve3D.TryGetPolyline3D(polycurve3D, out Polyline3D polyline3D) && polyline3D is T tPolyline)
                    {
                        result.Add(tPolyline);
                        return;
                    }
                }

                if (value is Polygon3D polygon3D)
                {
                    List<Point3D> points = polygon3D.GetPoints();
                    if (points != null && points.Count > 0)
                    {
                        Polyline3D polyline = new Polyline3D(points, true);
                        if (polyline is T tPolyline)
                        {
                            result.Add(tPolyline);
                        }
                    }
                    return;
                }

                List<IClosedPlanar3D> edge3Ds = GetEdges(value);
                if (edge3Ds != null)
                {
                    foreach (IClosedPlanar3D edge in edge3Ds)
                    {
                        if (edge is Polygon3D poly3D)
                        {
                            List<Point3D> points = poly3D.GetPoints();
                            if (points != null && points.Count > 0)
                            {
                                Polyline3D polyline = new Polyline3D(points, true);
                                if (polyline is T tPolyline)
                                {
                                    result.Add(tPolyline);
                                }
                            }
                        }
                        else if (edge is Polyline3D polyline3D && polyline3D is T tPolyline)
                        {
                            result.Add(tPolyline);
                        }
                    }
                    return;
                }
            }

            // 3. Polygon3D
            if (typeof(Polygon3D).IsAssignableFrom(targetType))
            {
                if (value is Polyline3D polyline3D && polyline3D.IsClosed())
                {
                    Polygon3D polygon3D = polyline3D.ToPolygon3D();
                    if (polygon3D is T tPolygon)
                    {
                        result.Add(tPolygon);
                        return;
                    }
                }

                if (value is Polycurve3D polycurve3D)
                {
                    if (Polycurve3D.TryGetPolyline3D(polycurve3D, out Polyline3D polyline) && polyline != null && polyline.IsClosed())
                    {
                        Polygon3D polygon3D = polyline.ToPolygon3D();
                        if (polygon3D is T tPolygon)
                        {
                            result.Add(tPolygon);
                            return;
                        }
                    }
                }

                if (value is Face3D face3D)
                {
                    if (face3D.GetExternalEdge3D() is Polygon3D extPolygon && extPolygon is T tPolygon)
                    {
                        result.Add(tPolygon);
                        return;
                    }
                }

                if (value is IFace3DObject face3DObject && face3DObject.Face3D != null)
                {
                    if (face3DObject.Face3D.GetExternalEdge3D() is Polygon3D extPolygon && extPolygon is T tPolygon)
                    {
                        result.Add(tPolygon);
                        return;
                    }
                }
            }

            // 4. Segment3D
            if (typeof(Segment3D).IsAssignableFrom(targetType))
            {
                if (value is ISegmentable3D segmentable3D)
                {
                    List<Segment3D> segments = segmentable3D.GetSegments();
                    if (segments != null)
                    {
                        foreach (Segment3D segment in segments)
                        {
                            if (segment is T tSegment)
                            {
                                result.Add(tSegment);
                            }
                        }
                    }
                    return;
                }

                List<IClosedPlanar3D> edge3Ds = GetEdges(value);
                if (edge3Ds != null)
                {
                    foreach (IClosedPlanar3D edge in edge3Ds)
                    {
                        if (edge is ISegmentable3D segable)
                        {
                            List<Segment3D> segments = segable.GetSegments();
                            if (segments != null)
                            {
                                foreach (Segment3D segment in segments)
                                {
                                    if (segment is T tSegment)
                                    {
                                        result.Add(tSegment);
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
            }

            // 5. Shell
            if (typeof(Shell).IsAssignableFrom(targetType))
            {
                if (value is Face3D face3D)
                {
                    Shell shell = new Shell(new List<Face3D> { face3D });
                    if (shell is T tShell)
                    {
                        result.Add(tShell);
                        return;
                    }
                }

                if (value is IFace3DObject face3DObject && face3DObject.Face3D != null)
                {
                    Shell shell = new Shell(new List<Face3D> { face3DObject.Face3D });
                    if (shell is T tShell)
                    {
                        result.Add(tShell);
                        return;
                    }
                }
            }

            // 6. Plane
            if (typeof(Plane).IsAssignableFrom(targetType))
            {
                if (value is Face3D face3D)
                {
                    Plane plane = face3D.GetPlane();
                    if (plane is T tPlane)
                    {
                        result.Add(tPlane);
                        return;
                    }
                }

                if (value is IPlanar3D planar3D)
                {
                    Plane plane = planar3D.GetPlane();
                    if (plane is T tPlane)
                    {
                        result.Add(tPlane);
                        return;
                    }
                }

                if (value is IFace3DObject face3DObject && face3DObject.Face3D != null)
                {
                    Plane plane = face3DObject.Face3D.GetPlane();
                    if (plane is T tPlane)
                    {
                        result.Add(tPlane);
                        return;
                    }
                }
            }

            // 7. Point3D
            if (typeof(Point3D).IsAssignableFrom(targetType))
            {
                if (value is Spatial.Plane plane)
                {
                    Point3D origin = plane.Origin;
                    if (origin is T tPoint)
                    {
                        result.Add(tPoint);
                        return;
                    }
                }
            }

            // 8. ISegmentable3D (Interface target)
            if (typeof(ISegmentable3D).IsAssignableFrom(targetType))
            {
                if (value is Polycurve3D polycurve3D)
                {
                    if (Polycurve3D.TryGetPolyline3D(polycurve3D, out Polyline3D polyline3D) && polyline3D is T tPolyline)
                    {
                        result.Add(tPolyline);
                        return;
                    }
                }

                List<IClosedPlanar3D> edge3Ds = GetEdges(value);
                if (edge3Ds != null)
                {
                    foreach (IClosedPlanar3D edge in edge3Ds)
                    {
                        if (edge is ISegmentable3D segmentable3D && segmentable3D is T tSegEdge)
                        {
                            result.Add(tSegEdge);
                        }
                    }
                    return;
                }
            }

            // 9. IClosedPlanar3D (Interface target)
            if (typeof(IClosedPlanar3D).IsAssignableFrom(targetType))
            {
                if (value is Face3D face3D)
                {
                    if (face3D.GetExternalEdge3D() is IClosedPlanar3D edge && edge is T tEdge)
                    {
                        result.Add(tEdge);
                        return;
                    }
                }

                if (value is IFace3DObject face3DObject && face3DObject.Face3D != null)
                {
                    if (face3DObject.Face3D.GetExternalEdge3D() is IClosedPlanar3D edge && edge is T tEdge)
                    {
                        result.Add(tEdge);
                        return;
                    }
                }
            }
        }
    }
}
