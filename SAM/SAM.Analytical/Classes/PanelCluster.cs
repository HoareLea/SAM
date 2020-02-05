using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Architectural;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelCluster : SAMObject
    {
        private Dictionary<PanelType, Construction> dictionary_Constructions;
        private Dictionary<PanelType, Dictionary<Guid, Panel>> dictionary_Panels;

        public PanelCluster(IEnumerable<Panel> panels)
        {
            dictionary_Panels = new Dictionary<PanelType, Dictionary<Guid, Panel>>();


            if (panels != null)
                foreach (Panel panel in panels)
                    Add(panel);
        }

        public bool Add(Panel panel)
        {
            if (panel == null)
                return false;

            Dictionary<Guid, Panel> dictionary;
            if (!dictionary_Panels.TryGetValue(panel.PanelType, out dictionary))
            {
                dictionary = new Dictionary<Guid, Panel>();
                dictionary_Panels[panel.PanelType] = dictionary;
            }

            dictionary[panel.Guid] = panel;
            return true;
        }

        public Panel GetPanel(Guid guid, PanelType? panelType = null)
        {
            if(panelType == null || !panelType.HasValue)
            {
                foreach(KeyValuePair<PanelType, Dictionary<Guid, Panel>> keyValuePair in dictionary_Panels)
                {
                    Panel panel;
                    if (keyValuePair.Value.TryGetValue(guid, out panel))
                        return panel;
                }
            }
            else
            {
                Dictionary<Guid, Panel> dictionary;
                if (!dictionary_Panels.TryGetValue(panelType.Value, out dictionary))
                    return null;

                return dictionary[guid];
            }

            return null;
        }

        public List<Panel> GetPanels(params PanelType[] panelTypes)
        {
            List<PanelType> panelTypeList = GetPanelTypes(panelTypes);

            List<Panel> result = new List<Panel>();
            if (panelTypeList.Count == 0)
                return result;

            foreach(PanelType panelType in panelTypeList)
            {
                Dictionary<Guid, Panel> dictionary;

                if (!dictionary_Panels.TryGetValue(panelType, out dictionary))
                    continue;

                result.AddRange(dictionary.Values);
            }

            return result;
        }

        public List<Level> GetLevels()
        {
            List<Panel> panels = new List<Panel>();
            List<Panel> panels_Temp = new List<Panel>();

            panels_Temp = GetPanels(PanelType.Floor, PanelType.FloorExposed, PanelType.FloorInternal, PanelType.FloorRaised, PanelType.Roof, PanelType.SlabOnGrade, PanelType.UndergroundSlab);
            if (panels_Temp != null && panels_Temp.Count > 0)
                panels.AddRange(panels_Temp);

            if (panels == null | panels.Count == 0)
                panels = GetPanels();

            HashSet<double> elevations = new HashSet<double>();
            foreach(Panel panel in panels)
            {
                Geometry.Spatial.BoundingBox3D boundingBox3D = panel.GetBoundingBox();
                elevations.Add(boundingBox3D.Max.Z);
                elevations.Add(boundingBox3D.Min.Z);
            }

            return elevations.ToList().ConvertAll(x => new Level(x));

        }

        public List<Guid> AssignPanelType(double angle_Start, double angle_End, PanelType panelType)
        {
            Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.Base;

            Dictionary<Guid, Panel> dictionary = null;
            if (!dictionary_Panels.TryGetValue(panelType, out dictionary))
            {
                dictionary = new Dictionary<Guid, Panel>();
                dictionary_Panels[panelType] = dictionary;
            }

            List<Guid> result = new List<Guid>();
            foreach (KeyValuePair<PanelType, Dictionary<Guid, Panel>> keyValuePair_PanelType in dictionary_Panels)
            {
                if (keyValuePair_PanelType.Key == panelType)
                    continue;
                
                foreach(KeyValuePair<Guid, Panel> keyValuePair_Panel in keyValuePair_PanelType.Value)
                {
                    Panel panel = keyValuePair_Panel.Value;
                    Geometry.Spatial.Vector3D vector3D = panel.PlanarBoundary3D.GetNormal();

                    Geometry.Spatial.Vector3D vector3D_Projected = plane.Project(vector3D);
                    double angle = vector3D_Projected.Angle(vector3D);
                    if (angle < angle_Start || angle > angle_End)
                        continue;

                    result.Add(panel.Guid);
                    dictionary_Panels[keyValuePair_PanelType.Key].Remove(panel.Guid);
                    dictionary[panel.Guid] = new Panel(panel, panelType);
                }
            }

            return result;
        }

        public List<Guid> AssignPanelTypes()
        {
            List<Guid> guidList = new List<Guid>();

            List<Guid> guidList_Temp = new List<Guid>();

            guidList_Temp = AssignPanelType(0.2617993878, 1.308996939, PanelType.Wall);
            if (guidList_Temp != null && guidList_Temp.Count > 0)
                guidList.AddRange(guidList_Temp);

            guidList_Temp = AssignPanelType(1.8325957146, 6.0213859194, PanelType.Wall);
            if (guidList_Temp != null && guidList_Temp.Count > 0)
                guidList.AddRange(guidList_Temp);

            guidList_Temp = AssignPanelType(6.0213859194, 0.2617993878, PanelType.Roof);
            if (guidList_Temp != null && guidList_Temp.Count > 0)
                guidList.AddRange(guidList_Temp);

            guidList_Temp = AssignPanelType(1.308996939, 1.8325957146, PanelType.Floor);
            if (guidList_Temp != null && guidList_Temp.Count > 0)
                guidList.AddRange(guidList_Temp);

            return guidList_Temp;
        }

        public List<Guid> AssignDefaultConstruction(params PanelType[] panelTypes)
        {
            List<PanelType> panelTypeList = GetPanelTypes(panelTypes);

            List<Guid> result = new List<Guid>();
            if (panelTypeList.Count == 0)
                return result;

            foreach(PanelType panelType in panelTypeList)
            {
                Construction construction;
                if (!dictionary_Constructions.TryGetValue(panelType, out construction) || construction == null)
                    continue;

                Dictionary<Guid, Panel> dictionary;
                if (!dictionary_Panels.TryGetValue(panelType, out dictionary))
                    continue;

                foreach(Panel panel in dictionary.Values)
                {
                    dictionary[panel.Guid] = new Panel(panel, construction);
                    result.Add(panel.Guid);
                }
                    
            }

            return result;
        }

        public Construction GetDefaultConstruction(PanelType panelType)
        {
            if (dictionary_Constructions == null)
                return null;

            Construction construction;
            dictionary_Constructions.TryGetValue(panelType, out construction);

            return construction;
        }

        public bool SetDefaultConstruction(PanelType panelType, Construction construction = null)
        {
            if (dictionary_Constructions == null)
                dictionary_Constructions = new Dictionary<PanelType, Construction>();

            if (construction == null)
                dictionary_Constructions.Remove(panelType);
            else
                dictionary_Constructions[panelType] = construction;

            return true;
        }


        private static List<PanelType> GetPanelTypes(params PanelType[] panelTypes)
        {
            List<PanelType> result = null;
            if (panelTypes != null)
                result = new List<PanelType>(panelTypes);

            if (result == null)
            {
                result = new List<PanelType>();

                foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
                    result.Add(panelType);
            }

            return result;
        }
    }
}
