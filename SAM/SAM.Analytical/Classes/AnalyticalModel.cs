using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class AnalyticalModel : SAMModel, IAnalyticalObject
    {
        private string description;
        private Location location;
        private Address address;
        private AdjacencyCluster adjacencyCluster;
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

        public Location Location
        {
            get
            {
                if (location == null)
                    return null;

                return new Location(location);
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
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

        public AdjacencyCluster AdjacencyCluster
        {
            get
            {
                if (adjacencyCluster == null)
                    return null;
                
                return new AdjacencyCluster(adjacencyCluster);
            }
        }

        public List<Guid> Remove(Type type, IEnumerable<Guid> guids)
        {
            if (type == null || guids == null)
                return null;
            
            if (typeof(Space).IsAssignableFrom(type) || typeof(Panel).IsAssignableFrom(type) || typeof(Aperture).IsAssignableFrom(type) || typeof(Result).IsAssignableFrom(type))
                return adjacencyCluster.Remove(type, guids);

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

        public bool AddSpace(Space space, IEnumerable<Panel> panels)
        {
            if (space == null || panels == null)
                return false;

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddSpace(space, panels);
        }

        public bool AddMaterial(IMaterial material)
        {
            if (material == null)
                return false;

            if (materialLibrary == null)
                materialLibrary = new MaterialLibrary("Default Material Libarary");

            return materialLibrary.Add(material.Clone());
        }

        public bool AddProfile(Profile profile)
        {
            if (profile == null)
                return false;

            if (profileLibrary == null)
                profileLibrary = new ProfileLibrary("Default Profile Libarary");

            return profileLibrary.Add(profile.Clone());
        }

        public bool AddPanel(Panel panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddObject(panel);
        }

        public bool AddResult<T>(IResult result, Guid guid)where T: SAMObject
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

            return  adjacencyCluster.AddObject(analyticalModelSimulationResult_Temp);
        }

        public List<Space> GetSpaces()
        {
            return adjacencyCluster?.GetSpaces()?.ConvertAll(x => new Space(x));
        }

        public List<Panel> GetPanels()
        {
            return adjacencyCluster?.GetPanels()?.ConvertAll(x => new Panel(x));
        }

        public List<Zone> GetZones()
        {
            return adjacencyCluster?.GetZones();
        }

        public List<T> GetResults<T>(IJSAMObject jSAMObject) where T : Result
        {
            if (jSAMObject == null)
            {
                return null;
            }

            return adjacencyCluster.GetRelatedObjects<T>(jSAMObject)?.ConvertAll(x => x?.Clone());
        }

        public List<AnalyticalModelSimulationResult> GetAnalyticalModelSimulationResults()
        {
            return adjacencyCluster?.GetObjects<AnalyticalModelSimulationResult>()?.ConvertAll(x => x?.Clone());
        }

        public List<T> GetRelatedObjects<T>(IJSAMObject jSAMObject) where T : IJSAMObject
        {
            if (jSAMObject == null)
            {
                return null;
            }

            return adjacencyCluster?.GetRelatedObjects<T>(jSAMObject)?.ConvertAll(x => Core.Query.Clone(x));
        }

        public List<Geometry.Spatial.Shell> GetShells()
        {
            return adjacencyCluster?.GetShells();
        }

        public IEnumerable<InternalCondition> GetInternalConditions()
        {
            return adjacencyCluster?.GetInternalConditions();
        }

        public void OffsetAperturesOnEdge(double distance, double tolerance = Tolerance.Distance)
        {
            adjacencyCluster?.OffsetAperturesOnEdge(distance, tolerance);
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
