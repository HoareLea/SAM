using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class AdjacencyCluster : Core.RelationCluster
    {        
        public AdjacencyCluster()
            : base()
        {

        }
        
        public AdjacencyCluster(AdjacencyCluster adjacencyCluster)
            :base(adjacencyCluster)
        {

        }
        
        public AdjacencyCluster(JObject jObject)
            : base(jObject)
        {

        }
        
        public override bool IsValid(Type type)
        {
            if (!base.IsValid(type))
                return false;

            return typeof(Panel).IsAssignableFrom(type) || typeof(Space).IsAssignableFrom(type);
        }

        public override Core.RelationCluster Clone()
        {
            return new AdjacencyCluster(this);
        }

        public bool Internal(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces != null && spaces.Count > 1;
        }

        public bool External(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces != null && spaces.Count == 1;
        }

        public bool Shade(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces == null || spaces.Count == 0;
        }

        public bool ExposedToSun(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            return Query.ExposedToSun(panel.PanelType);
        }

        public bool Ground(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            if (spaces == null || spaces.Count != 1)
                return false;

            switch (panel.PanelType)
            {
                case PanelType.SlabOnGrade:
                case PanelType.UndergroundSlab:
                case PanelType.UndergroundWall:
                    return true;
                default:
                    return false;
            }
        }

        public List<Panel> GetInternalPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => Internal(x));
        }

        public List<Panel> GetExternalPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => External(x));
        }

        public List<Panel> GetShadingPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => Shade(x));
        }

        public List<Panel> GetExposedToSunPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => ExposedToSun(x));
        }

        public List<Panel> GetGroundPanels()
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => Ground(x));
        }

        public List<Panel> GetPanels()
        {
            return GetObjects<Panel>();
        }

        public List<Panel> GetPanels(Space space)
        {
            return GetRelatedObjects<Panel>(space);
        }

        public List<Panel> GetPanels(Construction construction)
        {
            if (construction == null)
                return null;

            List<Panel> panels = GetPanels();
            if (panels == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                if (panel.SAMTypeGuid != construction.Guid)
                    continue;

                result.Add(panel);
            }

            return result;
        }

        public List<Space> GetSpaces()
        {
            return GetObjects<Space>();
        }

        public List<Construction> GetConstructions()
        {
            List<Panel> panels = GetPanels();
            if (panels == null)
                return null;

            Dictionary<Guid, Construction> dictionary = new Dictionary<Guid, Construction>();
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Guid guid = panel.SAMTypeGuid;
                if (guid == Guid.Empty)
                    continue;

                if (dictionary.ContainsKey(guid))
                    continue;

                Construction construction = panel.Construction;
                if (construction == null)
                    continue;

                dictionary[guid] = construction;
            }

            return dictionary.Values.ToList();
        }

        public List<ApertureConstruction> GetApertureConstructions()
        {
            List<Panel> panels = GetPanels();
            if (panels == null)
                return null;

            Dictionary<Guid, ApertureConstruction> dictionary = new Dictionary<Guid, ApertureConstruction>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                foreach(Aperture aperture in apertures)
                {
                    if (aperture == null)
                        continue;

                    Guid guid = aperture.SAMTypeGuid;
                    if (guid == Guid.Empty)
                        continue;

                    if (dictionary.ContainsKey(guid))
                        continue;

                    ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                    if (apertureConstruction == null)
                        continue;

                    dictionary[guid] = apertureConstruction;

                }
            }

            return dictionary.Values.ToList();
        }

        public List<List<Space>> GetSpaces(IEnumerable<Geometry.Spatial.Point3D> point3Ds, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Space> spaces = GetSpaces();
            if (spaces == null)
                return null;

            List<Tuple<Space, Geometry.Spatial.Shell>> tuples = new List<Tuple<Space, Geometry.Spatial.Shell>>();
            spaces.ForEach(x => tuples.Add(new Tuple<Space, Geometry.Spatial.Shell>(x, this.Shell(x))));
            tuples.RemoveAll(x => x.Item2 == null);

            List<List<Space>> result = new List<List<Space>>();
            foreach(Geometry.Spatial.Point3D point3D in point3Ds)
            {
                List<Tuple<Space, Geometry.Spatial.Shell>> tuples_Temp = tuples.FindAll(x => x.Item2.InRange(point3D, tolerance) || x.Item2.Inside(point3D, silverSpacing, tolerance));                   
                result.Add(tuples_Temp?.ConvertAll(x => x.Item1));
            }

            return result;
        }

        public List<Space> GetSpaces(Panel panel)
        {
            return GetRelatedObjects<Space>(panel);
        }

        public List<Space> GetSpaces(Geometry.Spatial.Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            return GetSpaces(new Geometry.Spatial.Point3D[] { point3D }, silverSpacing, tolerance)?.FirstOrDefault();
        }

        public AdjacencyCluster Filter(IEnumerable<Space> spaces)
        {
            if (spaces == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster();
            foreach (Space space in spaces)
            {
                if (GetGuid(space) == Guid.Empty)
                    continue;

                if (!result.AddObject(space))
                    continue;

                List<object> relatedObjects = GetRelatedObjects(space);
                if (relatedObjects == null)
                    continue;

                foreach(object relatedObject in relatedObjects)
                {
                    if (relatedObject == null)
                        continue;

                    if (!result.AddObject(relatedObject))
                        continue;

                    result.AddRelation(space, relatedObject);
                }
            }

            return result;
        }
    }
}
