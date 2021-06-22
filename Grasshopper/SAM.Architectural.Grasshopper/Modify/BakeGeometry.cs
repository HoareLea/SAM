using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public static partial class Modify
    {
        public static bool BakeGeometry(this HostBuildingElement hostBuildingElement, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            guid = Guid.Empty;

            if (hostBuildingElement == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, panel);
            objectAttributes.Name = hostBuildingElement.Name;

            bool result = true;

            Brep brep = hostBuildingElement.ToRhino(cutOpenings, tolerance);
            if (brep == null)
                result = Geometry.Grasshopper.Modify.BakeGeometry(hostBuildingElement.Face3D, rhinoDoc, objectAttributes, out guid);
            else
                guid = rhinoDoc.Objects.AddBrep(brep, objectAttributes);

            if (!result)
                return false;

            GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
            if (geometryBase != null)
            {
                string @string = hostBuildingElement.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                    geometryBase.SetUserString("SAM", @string);
            }

            return true;
        }

        public static bool BakeGeometry(this Opening opening, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (opening == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, aperture);
            objectAttributes.Name = opening.Name;

            if (!Geometry.Grasshopper.Modify.BakeGeometry(opening.Face3D, rhinoDoc, objectAttributes, out guid))
                return false;

            GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
            if (geometryBase != null)
            {
                string @string = opening.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                    geometryBase.SetUserString("SAM", @string);
            }

            return true;
        }

        public static bool BakeGeometry(this Room room, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (room == null || rhinoDoc == null || objectAttributes == null)
                return false;

            //Core.Grasshopper.Modify.SetUserStrings(objectAttributes, space);
            objectAttributes.Name = room.Name;

            if (!Geometry.Grasshopper.Modify.BakeGeometry(room.Location, rhinoDoc, objectAttributes, out guid))
                return false;

            GeometryBase geometryBase = rhinoDoc.Objects.FindGeometry(guid);
            if (geometryBase != null)
            {
                string @string = room.ToJObject()?.ToString();
                if (!string.IsNullOrWhiteSpace(@string))
                    geometryBase.SetUserString("SAM", @string);
            }

            return true;
        }

        public static bool BakeGeometry(this ArchitecturalModel architecturalModel, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (architecturalModel == null || rhinoDoc == null || objectAttributes == null)
                return false;

            List<HostBuildingElement> hostBuildingElements = architecturalModel.GetObjects<HostBuildingElement>();
            if (hostBuildingElements == null || hostBuildingElements.Count == 0)
                return false;

            List<Brep> breps = new List<Brep>();
            foreach (HostBuildingElement hostBuildingElement in hostBuildingElements)
            {
                Brep brep = hostBuildingElement.ToRhino();
                if (brep == null)
                    continue;

                breps.Add(brep);
            }

            if (breps == null || breps.Count == 0)
                return false;

            Brep result = Brep.MergeBreps(breps, Core.Tolerance.MacroDistance); //Tolerance has been changed from Core.Tolerance.Distance
            if (result == null)
                return false;

            obj_guid = rhinoDoc.Objects.AddBrep(result);
            return true;
        }
    }
}