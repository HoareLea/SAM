using Newtonsoft.Json.Linq;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class AdjacencyCluster : Core.RelationCluster, IAnalyticalObject
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
                typeof(Zone).IsAssignableFrom(type) ||
                typeof(MechanicalSystem).IsAssignableFrom(type) ||
                typeof(AdjacencyClusterSimulationResult).IsAssignableFrom(type) ||
                typeof(SpaceSimulationResult).IsAssignableFrom(type) ||
                typeof(SurfaceSimulationResult).IsAssignableFrom(type) ||
                typeof(ZoneSimulationResult).IsAssignableFrom(type) ||
                typeof(InternalCondition).IsAssignableFrom(type) ||
                typeof(Construction).IsAssignableFrom(type) ||
                typeof(ApertureConstruction).IsAssignableFrom(type) ||
                typeof(MechanicalSystemType).IsAssignableFrom(type) ||
                typeof(Core.ISAMInstance).IsAssignableFrom(type) ||
                typeof(Core.ISAMType).IsAssignableFrom(type) ||
                typeof(Core.Result).IsAssignableFrom(type) ||
                typeof(IAnalyticalEquipment).IsAssignableFrom(type) ||
                typeof(DesignDay).IsAssignableFrom(type);
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

        public List<Panel> GetPanels(Core.LogicalOperator logicalOperator, params Space[] spaces)
        {
            if(spaces == null)
            {
                return null;
            }

            return GetRelatedObjects<Panel>(logicalOperator, spaces.ToList().ConvertAll(x => x.Guid).ToArray());
        }

        public IEnumerable<InternalCondition> GetInternalConditions()
        {
            return GetInternalConditions(true, true);
        }

        public IEnumerable<InternalCondition> GetInternalConditions(bool spaces = true, bool templates = true)
        {
            if(!spaces && !templates)
            {
                return null;
            }


            List<InternalCondition> result = null;

            if(spaces)
            {
                List<Space> spaces_Temp = GetSpaces();
                if (spaces_Temp != null)
                {
                    Dictionary<Guid, InternalCondition> dictionary = new Dictionary<Guid, InternalCondition>();
                    foreach (Space space in spaces_Temp)
                    {
                        InternalCondition internalCondition = space.InternalCondition;
                        if (internalCondition != null)
                            dictionary[internalCondition.Guid] = internalCondition;
                    }

                    if (dictionary?.Values != null)
                    {
                        result = new List<InternalCondition>(dictionary.Values);
                    }
                }
            }

            if(templates)
            {
                List<InternalCondition> internalConditions = GetObjects<InternalCondition>();
                if (internalConditions != null)
                {
                    if(result == null)
                    {
                        result = new List<InternalCondition>();
                    }

                    result.AddRange(internalConditions);
                }
            }

            return result;
        }

        public List<T> GetResults<T>(Core.IJSAMObject jSAMObject, string source = null) where T: Core.Result
        {
            List<T> result = GetRelatedObjects<T>(jSAMObject);
            if(result == null)
            {
                return result;
            }

            if(source != null)
            {
                result.RemoveAll(x => x.Source == null || x.Source != source);
            }

            return result;
        }

        public List<T> GetResults<T>(string source = null) where T: Core.Result
        {
            List<T> result = GetObjects<T>();
            if(result == null)
            {
                return result;
            }

            if(source != null)
            {
                result.RemoveAll(x => x.Source != source);
            }

            return result;
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

                if (panel.TypeGuid != construction.Guid)
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

                if (apertures.Find(x => x.TypeGuid == apertureConstruction.Guid) == null)
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

        public List<T> GetMechanicalSystems<T>() where T: MechanicalSystem
        {
            return GetObjects<T>();
        }

        public List<MechanicalSystem> GetMechanicalSystems()
        {
            return GetObjects<MechanicalSystem>();
        }

        public List<MechanicalSystemType> GetMechanicalSystemTypes()
        {
            return GetMechanicalSystemTypes<MechanicalSystemType>();
        }

        public List<T> GetMechanicalSystemTypes<T>() where T: MechanicalSystemType
        {
            Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();

            List<MechanicalSystem> mechanicalSystems = GetMechanicalSystems<MechanicalSystem>();
            if(mechanicalSystems != null)
            {
                foreach (MechanicalSystem mechanicalSystem in mechanicalSystems)
                {
                    T mechanicalSystemType = mechanicalSystem?.Type as T;
                    if (mechanicalSystemType == null)
                    {
                        continue;
                    }

                    dictionary[mechanicalSystemType.Guid] = mechanicalSystemType;
                }
            }

            List<T> mechanicalSystemTypes = GetObjects<T>();
            if (mechanicalSystemTypes != null)
            {
                foreach(T mechanicalSystemType in mechanicalSystemTypes)
                {
                    if(dictionary.ContainsKey(mechanicalSystemType.Guid))
                    {
                        continue;
                    }

                    dictionary[mechanicalSystemType.Guid] = mechanicalSystemType;
                }
            }

            return dictionary.Values.ToList();
        }

        public List<Space> GetSpaces()
        {
            return GetObjects<Space>();
        }

        public List<Space> GetSpaces(Zone zone)
        {
            if (zone == null)
                return null;

            List<Space> result = GetRelatedObjects<Space>(zone);

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

        public Aperture GetAperture(Guid guid)
        {
            Aperture result = GetObject<Aperture>(guid);
            if(result != null)
            {
                return result;
            }

            List<Panel> panels = GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                if(panel.HasAperture(guid))
                {
                    return panel.GetAperture(guid);
                }
            }

            return null;
        }

        public List<Construction> GetConstructions()
        {
            List<Construction> result = null;

            List<Panel> panels = GetPanels();
            if (panels != null)
            {
                Dictionary<Guid, Construction> dictionary = new Dictionary<Guid, Construction>();
                foreach (Panel panel in panels)
                {
                    if (panel == null)
                        continue;

                    Guid guid = panel.TypeGuid;
                    if (guid == Guid.Empty)
                        continue;

                    if (dictionary.ContainsKey(guid))
                        continue;

                    Construction construction = panel.Construction;
                    if (construction == null)
                        continue;

                    dictionary[guid] = construction;
                }

                if(dictionary != null && dictionary.Count != 0)
                {
                    result = new List<Construction>();
                    if (dictionary.Values != null)
                    {
                        result.AddRange(dictionary.Values);
                    }
                }
            }

            List<Construction> constructions = GetObjects<Construction>();
            if(constructions != null)
            {
                if(result == null)
                {
                    result = new List<Construction>();
                }

                result.AddRange(constructions);
            }

            return result;
        }

        public List<ApertureConstruction> GetApertureConstructions()
        {
            List<ApertureConstruction> result = null;

            List<Panel> panels = GetPanels();
            if (panels != null)
            {
                Dictionary<Guid, ApertureConstruction> dictionary = new Dictionary<Guid, ApertureConstruction>();
                foreach (Panel panel in panels)
                {
                    if (panel == null)
                        continue;

                    List<Aperture> apertures = panel.Apertures;
                    if (apertures == null || apertures.Count == 0)
                        continue;

                    foreach (Aperture aperture in apertures)
                    {
                        if (aperture == null)
                            continue;

                        Guid guid = aperture.TypeGuid;
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

                if(dictionary != null && dictionary.Count != 0)
                {
                    result = new List<ApertureConstruction>();
                    if (dictionary.Values != null)
                    {
                        result.AddRange(dictionary.Values);
                    }
                }
            }

            List<ApertureConstruction> apertureConstructions = GetObjects<ApertureConstruction>();
            if (apertureConstructions != null)
            {
                if(result == null)
                {
                    result = new List<ApertureConstruction>();
                }

                result.AddRange(apertureConstructions);
            }

            return result;
        }

        /// <summary>
        /// Gets spaces for given points
        /// </summary>
        /// <param name="point3Ds">Points to be checked</param>
        /// <param name="spaceLocation"> consider points as space locatioon (if point3D is on shell of adjacent spaces take upper space)</param>
        /// <param name="silverSpacing">Silver spacing tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of List of spaces</returns>
        public List<List<Space>> GetSpaces(IEnumerable<Point3D> point3Ds, bool spaceLocation = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Space> spaces = GetSpaces();
            if (spaces == null)
                return null;

            List<Tuple<Space, Shell>> tuples = new List<Tuple<Space, Shell>>();
            spaces.ForEach(x => tuples.Add(new Tuple<Space, Shell>(x, this.Shell(x))));
            tuples.RemoveAll(x => x.Item2 == null || !x.Item2.IsClosed(silverSpacing));


            int count = point3Ds.Count();

            List<List<Space>> result = new List<List<Space>>();
            if (count == 0)
                return result;


            for (int i = 0; i < count; i++)
                result.Add(null);

            Parallel.For(0, count, (int i) =>
            {
                Point3D point3D = point3Ds.ElementAt(i);
                List<Tuple<Space, Shell>> tuples_Temp = tuples.FindAll(x => x.Item2.InRange(point3D, tolerance) || x.Item2.Inside(point3D, silverSpacing, tolerance));
                
                //Handling cases where Space Location is on the floor
                if (spaceLocation && tuples_Temp.Count > 1)
                    tuples_Temp = tuples.FindAll(x => x.Item2.InRange(point3D.GetMoved(Vector3D.WorldZ * silverSpacing) as Point3D, tolerance));

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
            return GetObjects<Zone>();
        }

        public List<Zone> GetZones(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            List<Zone> result = GetObjects<Zone>();
            return result?.FindAll(x => x?.Name == name);
        }

        public List<Zone> GetZones(Space space, string zoneCategory = null)
        {
            List<Zone> zones = GetRelatedObjects<Zone>(space);
            if(zones == null || zones.Count == 0 || zoneCategory == null)
            {
                return zones;
            }

            List<Zone> result = new List<Zone>();
            foreach(Zone zone in zones)
            {
                if(!zone.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory_Zone) || zoneCategory_Zone == null)
                {
                    continue;
                }

                if(zoneCategory.Equals(zoneCategory_Zone))
                {
                    result.Add(zone);
                }
            }

            return result;
        }

        public HashSet<string> GetZoneCategories()
        {
            List<Zone> zones = GetZones();
            if(zones == null || zones.Count == 0)
            {
                return null;
            }

            HashSet<string> result = new HashSet<string>();
            foreach(Zone zone in zones)
            {
                if(zone != null && zone.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory) && zoneCategory != null)
                {
                    result.Add(zoneCategory);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets spaces for given point
        /// </summary>
        /// <param name="point3D">Poins to be checked</param>
        /// <param name="spaceLocation"> consider point as space locatioon (if point3D is on shell of adjacent spaces take upper space)</param>
        /// <param name="silverSpacing">Silver spacing tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of List of spaces</returns>
        public List<Space> GetSpaces(Point3D point3D, bool spaceLocation = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return null;

            return GetSpaces(new Point3D[] { point3D }, spaceLocation, silverSpacing, tolerance)?.FirstOrDefault();
        }

        public AdjacencyCluster Filter(IEnumerable<Space> spaces, bool setAdiabatic = true)
        {
            if (spaces == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster();
            foreach (Space space in spaces)
            {
                if (GetGuid(space) == Guid.Empty)
                    continue;

                if (!result.AddObject(new Space(space)))
                    continue;

                List<object> relatedObjects = GetRelatedObjects(space);
                if (relatedObjects == null)
                    continue;

                foreach(object relatedObject in relatedObjects)
                {
                    if (relatedObject == null)
                        continue;

                    object relatedObject_Temp = relatedObject;

                    if(relatedObject_Temp is Core.IJSAMObject)
                    {
                        relatedObject_Temp = Core.Query.Clone((Core.IJSAMObject)relatedObject_Temp);
                    }

                    if (!result.AddObject(relatedObject_Temp))
                        continue;

                    result.AddRelation(space, relatedObject_Temp);
                }
            }

            if(setAdiabatic)
            {
                List<Panel> panels = result.GetPanels();
                if(panels != null)
                {
                    for (int i=0; i < panels.Count;i++)
                    {
                        Panel panel = panels[i];

                        if (panel == null)
                        {
                            continue;
                        }

                        PanelType panelType = panel.PanelType;
                        if (panelType == PanelType.Air)
                        {
                            if (!result.External(panel))
                            {
                                continue;
                            }

                            if (Internal(panel))
                            {
                                continue;
                            }

                            panelType = Query.PanelType(panel.Normal);
                            if (panelType == PanelType.Undefined)
                            {
                                continue;
                            }

                            Construction construction = Query.DefaultConstruction(panelType);
                            PanelType panelType_Construction = construction.PanelType();
                            if(panelType_Construction != PanelType.Undefined)
                            {
                                panelType = panelType_Construction;
                            }

                            panel = new Panel(panel, construction);
                            panel = new Panel(panel, panelType);
                        }
                        else
                        {
                            if (!result.External(panel))
                            {
                                continue;
                            }

                            if (!Internal(panel))
                            {
                                continue;
                            }
                        }

                        panel.SetValue(PanelParameter.Adiabatic, true);
                        result.AddObject(panel);
                    }
                }
            }

            return result;
        }

        public void Transform(Transform3D transform3D)
        {
            List<Panel> panels = GetPanels();
            if(panels != null)
            {
                foreach(Panel panel in panels)
                {
                    Panel panel_New = new Panel(panel);
                    panel_New.Transform(transform3D);
                    AddObject(panel_New);
                }
            }

            List<Space> spaces = GetSpaces();
            if (spaces != null)
            {
                foreach (Space space in spaces)
                {
                    Space space_New = new Space(space);
                    space_New.Transform(transform3D);
                    AddObject(space_New);
                }
            }
        }

        public List<Shell> GetShells()
        {
            List<Space> spaces = GetSpaces();
            if (spaces == null)
                return null;

            return spaces.ConvertAll(x => Query.Shell(this, x));
        }

        public override int GetIndex(object @object)
        {
            int result = base.GetIndex(@object);

            if (result == -1)
            {
                if (@object is Aperture)
                {
                    List<Panel> panels = GetPanels();
                    if (panels == null || panels.Count == 0)
                    {
                        return -1;
                    }

                    result = base.Count<Aperture>();
                    if (result != 0)
                    {
                        result--;
                    }

                    Aperture aperture = (Aperture)@object;

                    foreach (Panel panel in panels)
                    {
                        List<Aperture> apertures = panel?.Apertures;
                        if (apertures == null && apertures.Count == 0)
                        {
                            continue;
                        }

                        foreach(Aperture aperture_Temp in apertures)
                        {
                            if(aperture_Temp != null && aperture.Guid.Equals(aperture_Temp.Guid))
                            {
                                return result;
                            }

                            result++;
                        }
                    }

                }
            }

            return result;
        }

        public override bool TryGetObject<T>(int index, T @object)
        {
            if(base.TryGetObject(index, @object))
            {
                return true;
            }

            if (@object is Aperture)
            {
                List<Panel> panels = GetPanels();
                if (panels == null || panels.Count == 0)
                {
                    return false;
                }

                int count = base.Count<Aperture>();
                if (count != 0)
                {
                    count--;
                }

                foreach (Panel panel in panels)
                {
                    List<Aperture> apertures = panel?.Apertures;
                    if (apertures == null && apertures.Count == 0)
                    {
                        continue;
                    }

                    foreach (Aperture aperture_Temp in apertures)
                    {
                        if(count == index)
                        {
                            @object = (T)(object)aperture_Temp;
                            return true;
                        }

                        count++;
                    }
                }

            }

            return false;
        }

        public override int Count<T>()
        {
            int result = base.Count<T>();

            if(typeof(Aperture).IsAssignableFrom(typeof(T)))
            {
                List<Panel> panels = GetPanels();
                if (panels != null && panels.Count != 0)
                {
                    foreach (Panel panel in panels)
                    {
                        List<Aperture> apertures = panel?.Apertures;
                        if (apertures == null && apertures.Count == 0)
                        {
                            continue;
                        }

                        result += apertures.Count;
                    }
                }
            }

            return result;
        }
    }
}
