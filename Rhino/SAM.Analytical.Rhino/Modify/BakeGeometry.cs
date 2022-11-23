using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static bool BakeGeometry(this Panel panel, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            guid = Guid.Empty;

            if (panel == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, panel);
            objectAttributes.Name = panel.Name;

            List<Panel> panels_FixEdges = panel.FixEdges(cutApertures, tolerance);
            if (panels_FixEdges == null || panels_FixEdges.Count == 0)
            {
                panels_FixEdges = new List<Panel>() { panel };
            }

            bool result = true;

            foreach(Panel panel_FixEdges in panels_FixEdges)
            {
                Brep brep = panel.ToRhino(cutApertures, tolerance);
                if (brep == null)
                    result = Geometry.Rhino.Modify.BakeGeometry(panel.GetFace3D(), rhinoDoc, objectAttributes, out guid);
                else
                    guid = rhinoDoc.Objects.AddBrep(brep, objectAttributes);
            }

            if (!result)
                return false;

            GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
            if (geometryBase != null)
            {
                string @string = panel.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                    geometryBase.SetUserString("SAM", @string);
            }

            return true;
        }

        public static bool BakeGeometry(this Aperture aperture, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid, bool includeFrame = false)
        {
            guid = Guid.Empty;

            if (aperture == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, aperture);
            objectAttributes.Name = aperture.Name;

            if(!includeFrame)
            {
                if (!Geometry.Rhino.Modify.BakeGeometry(new Face3D(aperture.GetExternalEdge3D()), rhinoDoc, objectAttributes, out guid))
                {
                    return false;
                }
            }
            else
            {
                List<Face3D> face3Ds = new List<Face3D>();

                List<Face3D> face3Ds_Temp = null;

                face3Ds_Temp = aperture.GetFace3Ds(AperturePart.Frame);
                if(face3Ds_Temp != null)
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }

                face3Ds_Temp = aperture.GetFace3Ds(AperturePart.Pane);
                if (face3Ds_Temp != null)
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }

                if (face3Ds == null || face3Ds.Count == 0)
                {
                    return false;
                }

                if (!Geometry.Rhino.Modify.BakeGeometry(face3Ds, rhinoDoc, objectAttributes, out guid))
                {
                    return false;
                }
            }

            GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
            if (geometryBase != null)
            {
                string @string = aperture.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                    geometryBase.SetUserString("SAM", @string);
            }

            return true;
        }

        public static bool BakeGeometry(this Space space, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (space == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, space);
            objectAttributes.Name = space.Name;

            if (!Geometry.Rhino.Modify.BakeGeometry(space.Location, rhinoDoc, objectAttributes, out guid))
                return false;

            GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
            if (geometryBase != null)
            {
                string @string = space.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                    geometryBase.SetUserString("SAM", @string);
            }

            return true;
        }
    }
}