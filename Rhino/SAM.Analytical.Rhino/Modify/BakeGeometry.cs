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
        public static bool BakeGeometry(this IPanel panel, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out List<Guid> guids, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            guids = null;

            if (panel == null || rhinoDoc == null || objectAttributes == null)
            {
                return false;
            }

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, panel);
            objectAttributes.Name = panel is Panel ? ((Panel)panel).Name : panel.GetType().Name;

            //List<Panel> panels_FixEdges = panel.FixEdges(cutApertures, tolerance);
            List<IPanel> panels_FixEdges = panel.FixEdges(false, tolerance);
            if (panels_FixEdges == null || panels_FixEdges.Count == 0)
            {
                panels_FixEdges = new List<IPanel>() { panel };
            }

            bool result = true;

            guids = new List<Guid>();

            foreach (Panel panel_FixEdges in panels_FixEdges)
            {
                List<Brep> breps = panel_FixEdges.ToRhino(cutApertures, tolerance);
                if (breps == null || breps.Count == 0)
                {
                    result = Geometry.Rhino.Modify.BakeGeometry(panel_FixEdges.Face3D, rhinoDoc, objectAttributes, out Guid guid);
                    if (result)
                    {
                        guids.Add(guid);
                    }
                }
                else
                {
                    foreach (Brep brep in breps)
                    {
                        Guid guid = rhinoDoc.Objects.AddBrep(brep, objectAttributes);
                        if (guid != Guid.Empty)
                        {
                            guids.Add(guid);
                        }
                    }

                }
            }

            if (!result)
            {
                return false;
            }

            if (guids != null && guids.Count != 0)
            {
                string @string = panel.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                {
                    foreach (Guid guid in guids)
                    {
                        GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
                        if (geometryBase != null)
                        {

                            geometryBase.SetUserString("SAM", @string);
                        }
                    }

                }

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

        public static bool BakeGeometry(this ISpace space, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
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