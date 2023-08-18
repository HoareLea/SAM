using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class AnalyticalModel : SAMModel, IAnalyticalObject
    {
        private Address address;
        private AdjacencyCluster adjacencyCluster;
        private string description;
        private Location location;
        private MaterialLibrary materialLibrary;
        private ProfileLibrary profileLibrary;

        public AnalyticalModel(string name, string description, Location location, Address address, AdjacencyCluster adjacencyCluster)
            : base(name)
        {
            this.description = description;

            if (location != null)
                this.location = new Location(location);

            if (address != null)
                this.address = new Address(address);

            if (adjacencyCluster != null)
                this.adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
        }

        public AnalyticalModel(string name, string description, Location location, Address address, AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary, ProfileLibrary profileLibrary)
                : base(name)
        {
            this.description = description;

            if (location != null)
                this.location = new Location(location);

            if (address != null)
                this.address = new Address(address);

            if (adjacencyCluster != null)
                this.adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

            if (materialLibrary != null)
                this.materialLibrary = new MaterialLibrary(materialLibrary);

            if (profileLibrary != null)
                this.profileLibrary = new ProfileLibrary(profileLibrary);
        }

        public AnalyticalModel(Guid guid, string name)
            : base(guid, name)
        {
        }

        public AnalyticalModel(JObject jObject)
            : base(jObject)
        {
        }

        public AnalyticalModel(AnalyticalModel analyticalModel)
            : base(analyticalModel)
        {
            if (analyticalModel == null)
                return;

            description = analyticalModel.description;
            
            if (analyticalModel.location != null)
                location = new Location(analyticalModel.location);

            if (analyticalModel.address != null)
                address = new Address(analyticalModel.address);

            if (analyticalModel.adjacencyCluster != null)
                adjacencyCluster = new AdjacencyCluster(analyticalModel.adjacencyCluster);

            if (analyticalModel.materialLibrary != null)
                materialLibrary = new MaterialLibrary(analyticalModel.materialLibrary);

            if (analyticalModel.profileLibrary != null)
                profileLibrary = new ProfileLibrary(analyticalModel.profileLibrary);
        }

        public AnalyticalModel(AnalyticalModel analyticalModel, Location location)
            : base(analyticalModel)
        {
            if (analyticalModel == null)
                return;

            description = analyticalModel.description;

            if (location != null)
                this.location = new Location(location);

            if (analyticalModel.address != null)
                address = new Address(analyticalModel.address);

            if (analyticalModel.adjacencyCluster != null)
                adjacencyCluster = new AdjacencyCluster(analyticalModel.adjacencyCluster);

            if (analyticalModel.materialLibrary != null)
                materialLibrary = new MaterialLibrary(analyticalModel.materialLibrary);

            if (analyticalModel.profileLibrary != null)
                profileLibrary = new ProfileLibrary(analyticalModel.profileLibrary);
        }

        public AnalyticalModel(AnalyticalModel analyticalModel, AdjacencyCluster adjacencyCluster)
            : base(analyticalModel)
        {
            if (analyticalModel == null)
                return;

            description = analyticalModel.description;

            if (analyticalModel.location != null)
                location = new Location(analyticalModel.location);

            if (analyticalModel.address != null)
                address = new Address(analyticalModel.address);

            if (analyticalModel.materialLibrary != null)
                materialLibrary = new MaterialLibrary(analyticalModel.materialLibrary);

            if (analyticalModel.profileLibrary != null)
                profileLibrary = new ProfileLibrary(analyticalModel.profileLibrary);

            if (adjacencyCluster != null)
                this.adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
        }

        public AnalyticalModel(AnalyticalModel analyticalModel, AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary, ProfileLibrary profileLibrary)
            : base(analyticalModel)
        {
            if (analyticalModel == null)
                return;

            description = analyticalModel.description;

            if (analyticalModel.location != null)
                location = new Location(analyticalModel.location);

            if (analyticalModel.address != null)
                address = new Address(analyticalModel.address);

            if (profileLibrary != null)
                this.profileLibrary = new ProfileLibrary(profileLibrary);

            if (materialLibrary != null)
                this.materialLibrary = new MaterialLibrary(materialLibrary);

            if (adjacencyCluster != null)
                this.adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
        }

        public Address Address
        {
            get
            {
                if (address == null)
                    return null;

                return new Address(address);
            }
        }

        public AdjacencyCluster AdjacencyCluster
        {
            get
            {
                if (adjacencyCluster == null)
                    return null;

                return new AdjacencyCluster(adjacencyCluster);
            }
        }

        public ConstructionLibrary ConstructionLibrary
        {
            get
            {
                List<Construction> constructions = adjacencyCluster?.GetConstructions();
                if (constructions == null)
                {
                    return null;
                }

                ConstructionLibrary result = new ConstructionLibrary(Name);
                constructions.ForEach(x => result.Add(x));
                return result;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public Location Location
        {
            get
            {
                if (location == null)
                    return null;

                return new Location(location);
            }
        }
        
        public MaterialLibrary MaterialLibrary
        {
            get
            {
                if (materialLibrary == null)
                    return null;

                return new MaterialLibrary(materialLibrary);
            }
        }

        public ProfileLibrary ProfileLibrary
        {
            get
            {
                if (profileLibrary == null)
                    return null;

                return new ProfileLibrary(profileLibrary);
            }
        }
        
        public bool AddInternalCondition(InternalCondition internalCondition)
        {
            if (internalCondition == null)
            {
                return false;
            }

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddObject(new InternalCondition(internalCondition));
        }

        public bool AddMaterial(IMaterial material)
        {
            if (material == null)
                return false;

            if (materialLibrary == null)
                materialLibrary = new MaterialLibrary("Default Material Libarary");

            return materialLibrary.Add(material.Clone());
        }

        public bool AddMechanicalSystemType(MechanicalSystemType mechanicalSystemType)
        {
            if (mechanicalSystemType == null)
                return false;

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddObject(mechanicalSystemType);
        }

        public bool AddPanel(Panel panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddObject(new Panel(panel));
        }

        public bool AddProfile(Profile profile, bool @override = true)
        {
            if (profile == null)
            {
                return false;
            }

            if (profileLibrary == null)
            {
                profileLibrary = new ProfileLibrary("Default Profile Libarary");
            }

            if (!@override && profileLibrary.Contains(profile))
            {
                return false;
            }

            return profileLibrary.Add(profile.Clone());
        }

        public bool AddResult<T>(IResult result, Guid guid) where T : SAMObject
        {
            IResult result_Temp = result?.Clone();
            if (result_Temp == null)
            {
                return false;
            }

            if (adjacencyCluster == null)
            {
                adjacencyCluster = new AdjacencyCluster();
            }

            bool added = adjacencyCluster.AddObject(result_Temp);
            if (!added)
            {
                return false;
            }

            if (guid != Guid.Empty)
            {
                T @object = adjacencyCluster.GetObject<T>(guid);
                if (@object != null)
                {
                    adjacencyCluster.AddRelation(result_Temp, @object);
                }
            }

            return true;
        }

        public bool AddResult(AnalyticalModelSimulationResult analyticalModelSimulationResult)
        {
            AnalyticalModelSimulationResult analyticalModelSimulationResult_Temp = analyticalModelSimulationResult?.Clone();
            if (analyticalModelSimulationResult_Temp == null)
            {
                return false;
            }

            if (adjacencyCluster == null)
            {
                adjacencyCluster = new AdjacencyCluster();
            }

            return adjacencyCluster.AddObject(analyticalModelSimulationResult_Temp);
        }

        public bool AddSpace(Space space, IEnumerable<Panel> panels = null)
        {
            if (space == null)
            {
                return false;
            }


            if (adjacencyCluster == null)
            {
                adjacencyCluster = new AdjacencyCluster();
            }

            return adjacencyCluster.AddSpace(space, panels);
        }

        public bool AddZone(Zone zone)
        {
            if (zone == null)
            {
                return false;
            }

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddObject(new Zone(zone));
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Description"))
                description = jObject.Value<string>("Description");

            if (jObject.ContainsKey("Location"))
                location = new Location(jObject.Value<JObject>("Location"));

            if (jObject.ContainsKey("Address"))
                address = new Address(jObject.Value<JObject>("Address"));

            if (jObject.ContainsKey("AdjacencyCluster"))
                adjacencyCluster = new AdjacencyCluster(jObject.Value<JObject>("AdjacencyCluster"));

            if (jObject.ContainsKey("MaterialLibrary"))
                materialLibrary = new MaterialLibrary(jObject.Value<JObject>("MaterialLibrary"));

            if (jObject.ContainsKey("ProfileLibrary"))
                profileLibrary = new ProfileLibrary(jObject.Value<JObject>("ProfileLibrary"));

            return true;
        }

        public List<AnalyticalModelSimulationResult> GetAnalyticalModelSimulationResults()
        {
            return adjacencyCluster?.GetObjects<AnalyticalModelSimulationResult>()?.ConvertAll(x => x?.Clone());
        }

        public List<Aperture> GetApertures(Func<Aperture, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster?.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Aperture> result = new List<Aperture>();
            foreach (Panel panel in panels)
            {
                List<Aperture> apertures = panel?.Apertures;
                if (apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                apertures = apertures.FindAll(x => func.Invoke(x));
                if (apertures == null || apertures.Count == 0)
                {
                    continue;
                }
                result.AddRange(apertures.ConvertAll(x => new Aperture(x)));
            }

            return result;
        }

        public List<IAnalyticalEquipment> GetEquipment()
        {
            return adjacencyCluster?.GetObjects<IAnalyticalEquipment>(); ;
        }

        public IEnumerable<InternalCondition> GetInternalConditions()
        {
            return adjacencyCluster?.GetInternalConditions();
        }

        public List<T> GetMechanicalSystems<T>() where T : MechanicalSystem
        {
            return adjacencyCluster?.GetMechanicalSystems<T>()?.ConvertAll(x => Core.Query.Clone(x));
        }

        public List<MechanicalSystem> GetMechanicalSystems()
        {
            return GetMechanicalSystems<MechanicalSystem>();
        }

        public List<T> GetMechanicalSystemTypes<T>() where T : MechanicalSystemType
        {
            return adjacencyCluster?.GetMechanicalSystemTypes<T>()?.ConvertAll(x => Core.Query.Clone(x));
        }

        public List<MechanicalSystemType> GetMechanicalSystemTypes()
        {
            return GetMechanicalSystemTypes<MechanicalSystemType>();
        }

        public List<Panel> GetPanels()
        {
            return adjacencyCluster?.GetPanels()?.ConvertAll(x => new Panel(x));
        }

        public List<Panel> GetPanels(Func<Panel, bool> func)
        {
            if (func == null)
            {
                return null;
            }

            return adjacencyCluster?.GetPanels()?.FindAll(x => func.Invoke(x)).ConvertAll(x => new Panel(x));
        }

        public List<T> GetRelatedObjects<T>(IJSAMObject jSAMObject) where T : IJSAMObject
        {
            if (jSAMObject == null)
            {
                return null;
            }

            return adjacencyCluster?.GetRelatedObjects<T>(jSAMObject)?.ConvertAll(x => Core.Query.Clone(x));
        }

        public List<T> GetResults<T>(IJSAMObject jSAMObject) where T : Result
        {
            if (jSAMObject == null)
            {
                return null;
            }

            return adjacencyCluster.GetRelatedObjects<T>(jSAMObject)?.ConvertAll(x => x?.Clone());
        }

        public List<T> GetResults<T>(string source = null) where T : Result
        {
            return adjacencyCluster.GetResults<T>(source);
        }

        public List<Geometry.Spatial.Shell> GetShells()
        {
            return adjacencyCluster?.GetShells();
        }

        public List<Space> GetSpaces()
        {
            return adjacencyCluster?.GetSpaces()?.ConvertAll(x => new Space(x));
        }

        public HashSet<string> GetZoneCategories()
        {
            return adjacencyCluster.GetZoneCategories();
        }

        public List<Zone> GetZones()
        {
            return adjacencyCluster?.GetZones();
        }

        public Range<double> GetElevationRange()
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            Range<double> result = null;
            foreach(Panel panel in panels)
            {
                Range<double> elevationRange = panel?.GetElevationRange();
                if(elevationRange == null)
                {
                    continue;
                }

                if (result == null)
                {
                    result = elevationRange;
                }
                else
                {
                    result.Add(elevationRange);
                }
            }

            return result;
        }

        public bool HasMaterial(IMaterial material)
        {
            if (material == null || materialLibrary == null)
                return false;

            return materialLibrary.GetMaterial(material.Name) != null;
        }

        public bool Normalize(bool includeApertures = true, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (adjacencyCluster == null)
            {
                return false;
            }

            return adjacencyCluster.Normalize(includeApertures, orientation, edgeOrientationMethod, tolerance_Angle, tolerance_Distance);
        }

        public void OffsetAperturesOnEdge(double distance, double tolerance = Tolerance.Distance)
        {
            adjacencyCluster?.OffsetAperturesOnEdge(distance, tolerance);
        }

        public List<Guid> Remove(Type type, IEnumerable<Guid> guids)
        {
            if (type == null || guids == null)
                return null;
            
            if (typeof(Space).IsAssignableFrom(type) || typeof(Panel).IsAssignableFrom(type) || typeof(Aperture).IsAssignableFrom(type) || typeof(Result).IsAssignableFrom(type) || typeof(Group).IsAssignableFrom(type) || typeof(MechanicalSystem).IsAssignableFrom(type))
            {
                return adjacencyCluster.Remove(type, guids);

            }

            return null;
        }

        public List<Guid> Remove(IEnumerable<SAMObject> sAMObjects)
        {
            if (sAMObjects == null)
                return null;

            Dictionary<Type, List<SAMObject>> dictionary = Core.Query.TypeDictionary(sAMObjects);

            List<Guid> result = new List<Guid>();
            foreach(KeyValuePair<Type, List<SAMObject>> keyValuePair in dictionary)
            {
                List<Guid> guids = Remove(keyValuePair.Key, keyValuePair.Value.ConvertAll(x => x.Guid));
                if (guids != null && guids.Count > 0)
                    result.AddRange(guids);
            }

            return result;
        }
        
        public List<Aperture> ReplaceApertureConstruction(IEnumerable<Guid> guids, ApertureConstruction apertureConstruction)
        {
            return adjacencyCluster?.ReplaceApertureConstruction(guids, apertureConstruction);
        }

        public List<Panel> ReplaceConstruction(IEnumerable<Guid> guids, Construction construction, ApertureConstruction apertureConstruction = null, double offset = 0)
        {
            return adjacencyCluster?.ReplaceConstruction(guids, construction, apertureConstruction, offset);
        }
        
        public List<Panel> ReplaceTransparentPanels(double offset = 0)
        {
            List<Panel> result = adjacencyCluster?.ReplaceTransparentPanels(materialLibrary, offset);
            if(result != null && result.Count > 0)
            {
                IEnumerable<IMaterial> materials = Query.Materials(result, ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary));
                if (materials != null)
                    foreach (IMaterial material in materials)
                        AddMaterial(material);
            }
            return result;
        }
        
        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (description != null)
                jObject.Add("Description", description);

            if (location != null)
                jObject.Add("Location", location.ToJObject());

            if (address != null)
                jObject.Add("Address", address.ToJObject());

            if (adjacencyCluster != null)
                jObject.Add("AdjacencyCluster", adjacencyCluster.ToJObject());

            if (materialLibrary != null)
                jObject.Add("MaterialLibrary", materialLibrary.ToJObject());

            if (profileLibrary != null)
                jObject.Add("ProfileLibrary", profileLibrary.ToJObject());

            return jObject;
        }
        
        public void Transform(Geometry.Spatial.Transform3D transform3D)
        {
            if (adjacencyCluster != null)
                adjacencyCluster.Transform(transform3D);
        }
    }
}
