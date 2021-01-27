using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            return typeof(Panel).IsAssignableFrom(type) || 
                typeof(Space).IsAssignableFrom(type) || 
                typeof(MechanicalSystem).IsAssignableFrom(type) || 
                typeof(SpaceSimulationResult).IsAssignableFrom(type) ||
                typeof(PanelSimulationResult).IsAssignableFrom(type) ||
                typeof(ZoneSimulationResult).IsAssignableFrom(type);
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
                case PanelType.UndergroundCeiling:
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

        public List<Panel> GetExternalPanels(Space space)
        {
            List<Panel> panels = GetPanels(space);
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => External(x));
        }

        public List<Panel> GetInternalPanels(Space space)
        {
            List<Panel> panels = GetPanels(space);
            if (panels == null || panels.Count == 0)
                return null;

            return panels.FindAll(x => Internal(x));
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

        public IEnumerable<InternalCondition> GetInternalConditions()
        {
            List<Space> spaces = GetSpaces();
            if (spaces == null)
                return null;

            Dictionary<Guid, InternalCondition> dictionary = new Dictionary<Guid, InternalCondition>();
            foreach(Space space in spaces)
            {
                InternalCondition internalCondition = space.InternalCondition;
                if (internalCondition != null)
                    dictionary[internalCondition.Guid] = internalCondition;
            }

            return dictionary.Values;
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

        public List<Panel> GetPanels(ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            List<Panel> panels = GetPanels();
            if (panels == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                if (apertures.Find(x => x.SAMTypeGuid == apertureConstruction.Guid) == null)
                    continue;

                result.Add(panel);
            }

            return result;
        }

        public Panel GetPanel(Aperture aperture)
        {
            List<Panel> panels = GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            foreach (Panel panel in panels)
                if (panel.HasAperture(aperture.Guid))
                    return panel;

            return null;
        }

        public List<Space> GetSpaces()
        {
            return GetObjects<Space>();
        }

        public List<Space> GetSpaces(Zone zone)
        {
            if (zone == null)
                return null;

            List<Space> result = new List<Space>();
            foreach(Guid guid in zone)
            {
                Space space = GetObject<Space>(guid);
                if (space != null)
                    result.Add(space);
            }

            return result;
        }

        public List<Aperture> GetApertures()
        {
            List<Panel> panels = GetPanels();
            if (panels == null)
                return null;

            List<Aperture> result = new List<Aperture>();
            foreach (Panel panel in panels)
                if(panel.HasApertures)
                    result.AddRange(panel.Apertures);

            return result;
        }

        public List<Aperture> GetApertures(ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            List<Panel> panels = GetPanels();
            if (panels == null)
                return null;

            List<Aperture> result = new List<Aperture>();
            foreach (Panel panel in panels)
            {
                List<Aperture> apertures_Panel = panel?.GetApertures(apertureConstruction);
                if (apertures_Panel == null || apertures_Panel.Count == 0)
                    continue;

                result.AddRange(apertures_Panel);
            }

            return result;
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

        /// <summary>
        /// Gets spaces for given points
        /// </summary>
        /// <param name="point3Ds">Points to be checked</param>
        /// <param name="spaceLocation"> consider points as space locatioon (if point3D is on shell of adjacent spaces take upper space)</param>
        /// <param name="silverSpacing">Silver spacing tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of List of spaces</returns>
        public List<List<Space>> GetSpaces(IEnumerable<Geometry.Spatial.Point3D> point3Ds, bool spaceLocation = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Space> spaces = GetSpaces();
            if (spaces == null)
                return null;

            List<Tuple<Space, Geometry.Spatial.Shell>> tuples = new List<Tuple<Space, Geometry.Spatial.Shell>>();
            spaces.ForEach(x => tuples.Add(new Tuple<Space, Geometry.Spatial.Shell>(x, this.Shell(x))));
            tuples.RemoveAll(x => x.Item2 == null);


            int count = point3Ds.Count();

            List<List<Space>> result = new List<List<Space>>();
            if (count == 0)
                return result;


            for (int i = 0; i < count; i++)
                result.Add(null);

            Parallel.For(0, count, (int i) =>
            {
                Geometry.Spatial.Point3D point3D = point3Ds.ElementAt(i);
                List<Tuple<Space, Geometry.Spatial.Shell>> tuples_Temp = tuples.FindAll(x => x.Item2.InRange(point3D, tolerance) || x.Item2.Inside(point3D, silverSpacing, tolerance));
                
                //Handling cases where Space Location is on the floor
                if (spaceLocation && tuples_Temp.Count > 1)
                    tuples_Temp = tuples.FindAll(x => x.Item2.InRange(point3D.GetMoved(Geometry.Spatial.Vector3D.WorldZ * silverSpacing) as Geometry.Spatial.Point3D, tolerance));

                result[i] = tuples_Temp?.ConvertAll(x => x.Item1);
            });

            return result;
        }

        public List<Space> GetSpaces(Panel panel)
        {
            return GetRelatedObjects<Space>(panel);
        }

        public List<Zone> GetZones()
        {
            return Groups?.FindAll(x => x is Zone).ConvertAll(x => (Zone)x);
        }

        /// <summary>
        /// Gets spaces for given point
        /// </summary>
        /// <param name="point3D">Poins to be checked</param>
        /// <param name="spaceLocation"> consider point as space locatioon (if point3D is on shell of adjacent spaces take upper space)</param>
        /// <param name="silverSpacing">Silver spacing tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of List of spaces</returns>
        public List<Space> GetSpaces(Geometry.Spatial.Point3D point3D, bool spaceLocation = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            return GetSpaces(new Geometry.Spatial.Point3D[] { point3D }, spaceLocation, silverSpacing, tolerance)?.FirstOrDefault();
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
