using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
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

            List<Panel> panels_FixEdges = panel.FixEdges();
            if (panels_FixEdges == null || panels_FixEdges.Count == 0)
            {
                panels_FixEdges = new List<Panel>() { panel };
            }

            bool result = true;

            foreach(Panel panel_FixEdges in panels_FixEdges)
            {
                Brep brep = panel.ToRhino(cutApertures, tolerance);
                if (brep == null)
                    result = Geometry.Grasshopper.Modify.BakeGeometry(panel.GetFace3D(), rhinoDoc, objectAttributes, out guid);
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

        public static bool BakeGeometry(this Aperture aperture, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (aperture == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, aperture);
            objectAttributes.Name = aperture.Name;

            if (!Geometry.Grasshopper.Modify.BakeGeometry(aperture.GetFace3D(), rhinoDoc, objectAttributes, out guid))
                return false;

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

            if (!Geometry.Grasshopper.Modify.BakeGeometry(space.Location, rhinoDoc, objectAttributes, out guid))
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