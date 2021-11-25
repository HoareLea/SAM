using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Architectural;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class BuildingModel : SAMModel, IAnalyticalObject
    {
        private string description;
        private Location location;
        private Address address;
        private Terrain terrain;
        private RelationCluster relationCluster;
        private MaterialLibrary materialLibrary;
        private ProfileLibrary profileLibrary;

        public BuildingModel(JObject jObject)
            : base(jObject)
        {

        }

        public BuildingModel(BuildingModel buildingModel)
            : base(buildingModel)
        {
            if (buildingModel == null)
            {
                return;
            }

            description = buildingModel.description;
            location = buildingModel.location?.Clone();
            address = buildingModel.address?.Clone();
            terrain = buildingModel.terrain?.Clone();
            materialLibrary = buildingModel.materialLibrary?.Clone();

            relationCluster = buildingModel.relationCluster?.Clone();

            List<object> objects = relationCluster?.GetObjects();
            if (objects != null)
            {
                foreach (object @object in objects)
                {
                    if(@object is IJSAMObject)
                    {
                        relationCluster.AddObject(((IJSAMObject)@object).Clone());
                    }
                }
            }


        }

        public BuildingModel(string description, Location location, Address address, Terrain terrain, MaterialLibrary materialLibrary, ProfileLibrary profileLibrary)
            : base()
        {
            this.description = description;
            this.location = location?.Clone();
            this.address = address?.Clone();
            this.terrain = terrain?.Clone();
            if(materialLibrary != null)
            {
                this.materialLibrary = new MaterialLibrary(materialLibrary);
            }

            if (profileLibrary != null)
            {
                this.profileLibrary = new ProfileLibrary(profileLibrary);
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        public Terrain Terrain
        {
            get
            {
                return terrain?.Clone();
            }
        }

        public Location Location
        {
            get
            {
                return location?.Clone();
            }
            set
            {
                if(value != null)
                {
                    location = value;
                }
            }
        }

        public Address Address
        {
            get
            {
                return address?.Clone();
            }
            set
            {
                if (value != null)
                {
                    address = value;
                }
            }
        }

        public List<T> GetObjects<T>() where T : IJSAMObject
        {
            if(typeof(T).IsAssignableFrom(typeof(IMaterial)))
            {
                List<IMaterial> materials = materialLibrary?.GetObjects<IMaterial>();
                if(materials == null)
                {
                    return null;
                }

                List<T> result = new List<T>();
                foreach(IMaterial material in materials)
                {
                    if(material is T)
                    {
                        result.Add((T)material);
                    }
                }

                return result;
            }

            if (typeof(T).IsAssignableFrom(typeof(Profile)))
            {
                List<Profile> profiles = profileLibrary?.GetObjects<Profile>();
                if (profiles == null)
                {
                    return null;
                }

                List<T> result = new List<T>();
                foreach (Profile profile in profiles)
                {
                    if (profile is T)
                    {
                        result.Add((T)(object)profile);
                    }
                }

                return result;
            }

            return relationCluster?.GetObjects<T>()?.ConvertAll(x => Core.Query.Clone(x));

            //List<string> typeNames = relationCluster?.GetTypeNames(typeof(T));
            //if(typeNames == null || typeNames.Count == 0)
            //{
            //    return null;
            //}

            //List<T> result = new List<T>();
            //foreach(string typeName in typeNames)
            //{
            //    List<T> objects = relationCluster.GetObjects<T>(typeName);
            //    if (objects != null && objects.Count != 0)
            //    {
            //        result.AddRange(objects);
            //    }
            //}

            //return result;
        }

        public List<T> GetObjects<T>(params Func<T, bool>[] functions) where T: IJSAMObject
        {
            if (typeof(T).IsAssignableFrom(typeof(IMaterial)))
            {
                List<IMaterial> materials = materialLibrary?.GetObjects<IMaterial>();
                if (materials == null)
                {
                    return null;
                }

                List<T> all = new List<T>();
                foreach (IMaterial material in materials)
                {
                    if (material is T)
                    {
                        all.Add((T)material);
                    }
                }

                all.Filter(out List<T> @in, out List<T> @out, functions);

                return @in;
            }

            if (typeof(T).IsAssignableFrom(typeof(Profile)))
            {
                List<Profile> profiles = profileLibrary?.GetObjects<Profile>();
                if (profiles == null)
                {
                    return null;
                }

                List<T> all = new List<T>();
                foreach (Profile profile in profiles)
                {
                    if (profile is T)
                    {
                        all.Add((T)(object)profile);
                    }
                }

                all.Filter(out List<T> @in, out List<T> @out, functions);

                return @in;
            }

            return relationCluster?.GetObjects(functions)?.ConvertAll(x => Core.Query.Clone(x));
        }

        public T GetObject<T>(Guid guid) where T : IJSAMObject
        {
            if(relationCluster == null)
            {
                return default;
            }

            T @object = relationCluster.GetObject<T>(guid);
            if(@object == null)
            {
                return default;
            }

            return @object.Clone();
        }

        public List<T> GetRelatedObjects<T>(IJSAMObject jSAMObject) where T: IJSAMObject
        {
            if(jSAMObject == null)
            {
                return null;
            }

            return relationCluster?.GetRelatedObjects<T>(jSAMObject)?.ConvertAll(x => Core.Query.Clone(x));
        }

        public List<IJSAMObject> GetRelatedObjects(IJSAMObject jSAMObject, Type type = null)
        {
            if (jSAMObject == null)
            {
                return null;
            }

            List<object> objects = null;
            if(type == null)
            {
                objects = relationCluster?.GetRelatedObjects(jSAMObject);
            }
            else
            {
                objects = relationCluster?.GetRelatedObjects(jSAMObject, type);
            }
            if(objects == null)
            {
                return null;
            }

            List<IJSAMObject> result = new List<IJSAMObject>();
            foreach(object @object in objects)
            {
                IJSAMObject jSAMObject_Temp = Core.Query.Clone(@object as IJSAMObject);
                if(jSAMObject_Temp == null)
                {
                    continue;
                }

                result.Add(jSAMObject_Temp);
            }

            return result;
        }

        public bool RemoveObject(IJSAMObject jSAMObject)
        {
            if(jSAMObject == null || relationCluster == null)
            {
                return false;
            }

            if(jSAMObject is IMaterial)
            {
                if(materialLibrary == null)
                {
                    return false;
                }

                return materialLibrary.Remove((IMaterial)jSAMObject);
            }

            if (jSAMObject is Profile)
            {
                if (profileLibrary == null)
                {
                    return false;
                }

                return profileLibrary.Remove((Profile)jSAMObject);
            }

            if(relationCluster == null)
            {
                return false;
            }

            if(jSAMObject is IOpening)
            {
                List<IHostPartition> hostPartitions = relationCluster?.GetObjects<IHostPartition>();
                if(hostPartitions != null)
                {
                    IOpening opening = (IOpening)jSAMObject;

                    foreach(IHostPartition hostPartition in hostPartitions)
                    {
                        if(hostPartition.HasOpening(opening.Guid))
                        {
                            hostPartition.RemoveOpening(opening.Guid);
                            List<IOpening> openings = hostPartition.GetOpenings();
                            if(openings == null || openings.Count == 0)
                            {
                                relationCluster.RemoveRelation(hostPartition, opening.Type());
                                return true;
                            }

                            bool removeRelation = true;
                            OpeningType openingType = opening.Type();
                            foreach(IOpening opening_Temp in openings)
                            {
                                OpeningType openingType_Temp = opening_Temp?.Type();
                                if(openingType_Temp == null)
                                {
                                    continue;
                                }

                                if(openingType.Guid == openingType_Temp.Guid)
                                {
                                    removeRelation = false;
                                    break;
                                }
                            }

                            if(removeRelation)
                            {
                                relationCluster.RemoveRelation(hostPartition, openingType);
                            }

                            return true;
                        }
                    }
                }
            }
            else if(jSAMObject is InternalCondition)
            {
                InternalCondition internalCondition = (InternalCondition)jSAMObject;

                List<Space> spaces = relationCluster?.GetObjects<Space>();
                if(spaces != null)
                {
                    foreach(Space space in spaces)
                    {
                        InternalCondition internalCondition_Temp = space?.InternalCondition;
                        if(internalCondition_Temp == null)
                        {
                            continue;
                        }

                        if(internalCondition_Temp.Guid == internalCondition.Guid)
                        {
                            space.InternalCondition = null;
                        }
                    }
                }
            }
            else if(jSAMObject is OpeningType)
            {
                OpeningType openingType = (OpeningType)jSAMObject;

                List<IHostPartition> hostPartitions = relationCluster?.GetRelatedObjects<IHostPartition>(jSAMObject);
                if(hostPartitions != null && hostPartitions.Count != 0)
                {
                    foreach(IHostPartition hostPartition in hostPartitions)
                    {
                        List<IOpening> openings = hostPartition?.GetOpenings();
                        if(openings != null && openings.Count != 0)
                        {
                            foreach(IOpening opening in openings)
                            {
                                OpeningType openingType_Temp = opening?.Type();
                                if(openingType == null)
                                {
                                    continue;
                                }

                                if(openingType.Guid == openingType.Guid)
                                {
                                    hostPartition.RemoveOpening(opening.Guid);
                                }
                            }
                        }
                    }
                }
            }
            else if (jSAMObject is HostPartitionType)
            {
                HostPartitionType hostPartitionType = (HostPartitionType)jSAMObject;
                List<IHostPartition> hostPartitions = relationCluster?.GetRelatedObjects<IHostPartition>(hostPartitionType);
                if(hostPartitions != null)
                {
                    foreach(IHostPartition hostPartition in hostPartitions)
                    {
                        relationCluster.RemoveObject(hostPartition.GetType(), hostPartition.Guid);
                    }
                }
            }
            else if (jSAMObject is MechanicalSystemType)
            {
                MechanicalSystemType mechanicalSystemType = (MechanicalSystemType)jSAMObject;
                List<MechanicalSystem> mechanicalSystems = relationCluster?.GetRelatedObjects<MechanicalSystem>(mechanicalSystemType);
                if (mechanicalSystems != null)
                {
                    foreach (MechanicalSystem mechanicalSystem in mechanicalSystems)
                    {
                        relationCluster.RemoveObject(mechanicalSystem.GetType(), mechanicalSystem.Guid);
                    }
                }
            }

            Guid guid = relationCluster.GetGuid(jSAMObject);
            if(guid == Guid.Empty)
            {
                return false;
            }

            return relationCluster.RemoveObject(jSAMObject.GetType(), guid);
        }

        public bool AddRelation(IJSAMObject jSAMObject_1, IJSAMObject jSAMObject_2)
        {
            if(jSAMObject_1 == null || jSAMObject_2 == null || relationCluster == null)
            {
                return false;
            }

            //return relationCluster.AddRelation(jSAMObject_1.Clone(), jSAMObject_2.Clone());
            return relationCluster.AddRelation(jSAMObject_1, jSAMObject_2);
        }

        public IMaterial GetMaterial(string name)
        {
            if(materialLibrary == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            return materialLibrary.GetObject<IMaterial>(name)?.Clone();
        }

        public T GetMaterial<T>(string name) where T: IMaterial
        {
            IMaterial material = GetMaterial(name);
            if(material == null)
            {
                return default;
            }

            if(material is T)
            {
                return (T)material;
            }

            return default;
        }

        public Zone GetZone(string name, ZoneType zoneType)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            List<Zone> zones = relationCluster?.GetObjects<Zone>();
            if(zones == null || zones.Count == 0)
            {
                return null;
            }

            string zoneTypeText = zoneType.Text();

            foreach (Zone zone in zones)
            {
                if (!zone.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory))
                    continue;

                if (zoneTypeText.Equals(zoneCategory))
                    return zone.Clone();
            }

            return null;
        }

        public List<Zone> GetZones()
        {
            return GetObjects<Zone>();
        }

        public Profile GetProfile(ProfileType profileType, string name, bool includeProfileGroup = false)
        {
            if(profileLibrary == null)
            {
                return null;
            }

            return profileLibrary.GetProfile(name, profileType, includeProfileGroup);
        }

        public Profile GetProfile(InternalCondition internalCondition, ProfileType profileType, bool includeProfileGroup = false)
        {
            return internalCondition?.GetProfile(profileType, profileLibrary, includeProfileGroup);
        }

        public Profile GetProfile(Space space, ProfileType profileType, bool includeProfileGroup = false)
        {
            return GetProfile(space?.InternalCondition, profileType, includeProfileGroup);
        }

        public Dictionary<ProfileType, Profile> GetProfileDictionary(Space space)
        {
            if(profileLibrary == null)
            {
                return null;
            }

            InternalCondition internalCondition = space?.InternalCondition;
            if(internalCondition == null)
            {
                return null;
            }

            return internalCondition.GetProfileDictionary(profileLibrary);
        }

        public bool HasMaterial(string name)
        {
            if (materialLibrary == null || string.IsNullOrEmpty(name))
            {
                return false;
            }

            return materialLibrary.GetObject<IMaterial>(name) != null;
        }

        public bool HasMaterial(HostPartitionType hostPartitionType, MaterialType materialType)
        {
            if(hostPartitionType == null || materialLibrary == null)
            {
                return false;
            }

            return Query.HasMaterial(hostPartitionType, materialLibrary, materialType);
        }

        public bool HasMaterial(OpeningType openingType, MaterialType materialType)
        {
            if (openingType == null || materialLibrary == null)
            {
                return false;
            }

            return Query.HasMaterial(openingType, materialLibrary, materialType);
        }

        public IMaterial GetMaterial(MaterialLayer materialLayer)
        {
            if(materialLayer == null || materialLibrary == null)
            {
                return null;
            }

            return materialLayer.Material(materialLibrary)?.Clone();
        }

        public T GetMaterial<T>(MaterialLayer materialLayer) where T: IMaterial
        {
            IMaterial material = GetMaterial(materialLayer);
            if(material == null)
            {
                return default;
            }

            if(material is T)
            {
                return (T)material;
            }

            return default;
        }

        public List<IMaterial> GetMaterials(HostPartitionType hostPartitionType)
        {
            if(hostPartitionType == null)
            {
                return null;
            }

            return hostPartitionType.Materials(materialLibrary)?.ConvertAll(x => x.Clone());
        }

        public List<T> GetHostPartitionTypes<T>() where T: HostPartitionType
        {
            return GetObjects<T>();

            //List<IHostPartition> hostPartitions = GetPartitions<IHostPartition>();
            //if(hostPartitions == null || hostPartitions.Count == 0)
            //{
            //    return null;
            //}

            //Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            //foreach(IHostPartition hostPartition in hostPartitions)
            //{
            //    T hostPartitionType = hostPartition?.Type() as T;
            //    if(hostPartitionType == null)
            //    {
            //        continue;
            //    }

            //    dictionary[hostPartitionType.Guid] = hostPartitionType;
            //}

            //return dictionary.Values.ToList();
        }

        public List<HostPartitionType> GetHostPartitionTypes()
        {
            return GetObjects<HostPartitionType>();
        }

        public List<T> GetHostPartitionTypes<T>(string name, TextComparisonType textComparisonType = TextComparisonType.Equals) where T : HostPartitionType
        {
            return GetObjects((T hostPartitionType) => !string.IsNullOrWhiteSpace(hostPartitionType?.Name) && Core.Query.Compare(name, hostPartitionType?.Name, textComparisonType));
        }

        public List<T> GetOpeningTypes<T>() where T: OpeningType
        {
            return GetObjects<T>();

            //List<IHostPartition> hostPartitions = GetPartitions<IHostPartition>();
            //if (hostPartitions == null || hostPartitions.Count == 0)
            //{
            //    return null;
            //}

            //Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            //foreach (IHostPartition hostPartition in hostPartitions)
            //{
            //    List<IOpening> openings = hostPartition?.Openings;
            //    if(openings == null || openings.Count == 0)
            //    {
            //        continue;
            //    }

            //    foreach(IOpening opening in openings)
            //    {
            //        T openingType = opening.Type() as T;
            //        if(openingType == null)
            //        {
            //            continue;
            //        }

            //        dictionary[openingType.Guid] = openingType;
            //    }
            //}

            //return dictionary.Values.ToList();
        }

        public List<T> GetOpeningTypes<T>(string name, TextComparisonType textComparisonType = TextComparisonType.Equals) where T : OpeningType
        {
            return GetObjects((T openingType) => !string.IsNullOrWhiteSpace(openingType?.Name) && Core.Query.Compare(name, openingType?.Name, textComparisonType));
        }

        public List<OpeningType> GetOpeningTypes()
        {
            return GetObjects<OpeningType>();
        }

        public MaterialType GetMaterialType(HostPartitionType hostPartitionType)
        {
            return Architectural.Query.MaterialType(hostPartitionType?.MaterialLayers, materialLibrary);
        }

        public MaterialType GetMaterialType(IHostPartition hostPartition)
        {
            if(hostPartition == null)
            {
                return MaterialType.Undefined;
            }

            return GetMaterialType(hostPartition.Type());
        }

        public MaterialType GetMaterialType(MaterialLayer materialLayer)
        {
            if(materialLayer == null || materialLibrary == null)
            {
                return MaterialType.Undefined;
            }

            IMaterial material = GetMaterial(materialLayer);
            if(material == null)
            {
                return MaterialType.Undefined;
            }

            return material.MaterialType();
        }

        public MaterialType GetMaterialType(IEnumerable<MaterialLayer> materialLayers)
        {
            return Architectural.Query.MaterialType(materialLayers, materialLibrary);
        }

        public List<Space> GetSpaces(IPartition partition)
        {
            return GetRelatedObjects<Space>(partition);
        }

        public List<Space> GetSpaces(Zone zone)
        {
            return GetRelatedObjects<Space>(zone);
        }

        public List<Space> GetSpaces()
        {
            return GetObjects<Space>();
        }

        public List<T> GetMaterials<T>() where T: IMaterial
        {
            List<IMaterial> materials = materialLibrary?.GetMaterials();
            if(materials == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(IMaterial material in materials)
            {
                if(material is T)
                {
                    result.Add((T)material.Clone());
                }
            }

            return result;
        }

        public List<IMaterial> GetMaterials()
        {
            return GetMaterials<IMaterial>();
        }

        public List<string> GetMissingMaterialNames()
        {
            HashSet<string> materialNames = new HashSet<string>();

            if(relationCluster != null)
            {
                List<HostPartitionType> hostPartitionTypes = relationCluster.GetObjects<HostPartitionType>();
                if(hostPartitionTypes != null)
                {
                    foreach(HostPartitionType hostPartitionType in hostPartitionTypes)
                    {
                        List<MaterialLayer> materialLayers = hostPartitionType?.MaterialLayers;
                        if(materialLayers == null || materialLayers.Count == 0)
                        {
                            continue;
                        }

                        foreach(MaterialLayer materialLayer in materialLayers)
                        {
                            IMaterial material = GetMaterial(materialLayer);
                            if(material != null)
                            {
                                continue;
                            }

                            materialNames.Add(materialLayer.Name);
                        }
                    }
                }

                List<OpeningType> openingTypes = relationCluster.GetObjects<OpeningType>();
                if(openingTypes != null)
                {
                    foreach(OpeningType openingType in openingTypes)
                    {
                        List<MaterialLayer> materialLayers = null;

                        materialLayers = openingType?.PaneMaterialLayers;
                        if(materialLayers != null)
                        {
                            foreach (MaterialLayer materialLayer in materialLayers)
                            {
                                IMaterial material = GetMaterial(materialLayer);
                                if (material != null)
                                {
                                    continue;
                                }

                                materialNames.Add(materialLayer.Name);
                            }
                        }

                        materialLayers = openingType?.FrameMaterialLayers;
                        if (materialLayers != null)
                        {
                            foreach (MaterialLayer materialLayer in materialLayers)
                            {
                                IMaterial material = GetMaterial(materialLayer);
                                if (material != null)
                                {
                                    continue;
                                }

                                materialNames.Add(materialLayer.Name);
                            }
                        }
                    }
                }
            }

            return materialNames.ToList();
        }

        public bool Transparent(IPartition partition)
        {
            if (partition == null || materialLibrary == null)
            {
                return false;
            }

            if (partition is AirPartition)
            {
                return true;
            }

            if (partition is IHostPartition)
            {
                return Query.Transparent(((IHostPartition)partition).Type(), materialLibrary);
            }

            return false;
        }

        public bool Transparent(HostPartitionType hostPartitionType)
        {
            if (hostPartitionType == null || materialLibrary == null)
            {
                return false;
            }

            return Query.Transparent(hostPartitionType, materialLibrary);
        }

        public List<IPartition> GetTransparentPartitions()
        {
            return GetObjects((IPartition partition) => Transparent(partition));
        }

        public bool Internal(IPartition partition)
        {
            List<Space> spaces = relationCluster?.GetRelatedObjects<Space>(partition);
            return spaces != null && spaces.Count > 1;
        }

        public bool Internal(IOpening opening)
        {
            IHostPartition hostPartition = GetHostPartition<IHostPartition>(opening);
            if (hostPartition == null)
            {
                return false;
            }

            return Internal(hostPartition);
        }

        public List<IPartition> GetInternalPartitions()
        {
            return GetObjects((IPartition partition) => Internal(partition));
        }

        public bool External(IPartition partition)
        {
            List<Space> spaces = relationCluster?.GetRelatedObjects<Space>(partition);
            return spaces != null && spaces.Count == 1;
        }

        public bool External(IOpening opening)
        {
            IHostPartition hostPartition = GetHostPartition<IHostPartition>(opening);
            if(hostPartition == null)
            {
                return false;
            }

            return External(hostPartition);
        }

        public List<IPartition> GetExternalPartitions()
        {
            return GetObjects((IPartition partition) => External(partition));
        }

        public bool Shade(IPartition partition)
        {
            List<Space> spaces = relationCluster?.GetRelatedObjects<Space>(partition);
            return spaces == null || spaces.Count == 0;
        }

        public bool Shade(IOpening opening)
        {
            IHostPartition hostPartition = GetHostPartition<IHostPartition>(opening);
            if (hostPartition == null)
            {
                return false;
            }

            return Shade(hostPartition);
        }

        public List<IPartition> GetShadePartitions()
        {
            return GetObjects((IPartition partition) => Shade(partition));
        }

        public bool Underground(IPartition partition)
        {
            if(terrain == null || partition == null)
            {
                return false;
            }

            return terrain.Below(partition);
        }

        public bool Underground(IOpening opening)
        {
            IHostPartition hostPartition = GetHostPartition<IHostPartition>(opening);
            if (hostPartition == null)
            {
                return false;
            }

            return Underground(hostPartition);
        }

        public List<IPartition> GetUndergroundPartitions()
        {
            return GetObjects((IPartition partition) => Underground(partition));
        }

        public bool ExposedToSun(IPartition partition)
        {
            if(partition == null)
            {
                return false;
            }

            if(!External(partition))
            {
                return false;
            }

            if(Underground(partition))
            {
                return false;
            }

            if(terrain.On(partition))
            {
                return false;
            }

            return true;
        }

        public bool ExposedToSun(IOpening opening)
        {
            IHostPartition hostPartition = GetHostPartition<IHostPartition>(opening);
            if (hostPartition == null)
            {
                return false;
            }

            return ExposedToSun(hostPartition);
        }

        public List<IPartition> GetExposedToSunPartitions()
        {
            return GetObjects((IPartition partition) => ExposedToSun(partition));
        }

        public List<Shell> GetShells()
        {
            List<Space> spaces = relationCluster?.GetObjects<Space>();
            if(spaces == null)
            {
                return null;
            }

            List<Shell> result = new List<Shell>();
            foreach(Space space in spaces)
            {
                Shell shell = GetShell(space);
                if(shell != null)
                {
                    result.Add(shell);
                }
            }

            return result;
        }

        public List<Shell> GetShells(Zone zone, bool union = true)
        {
            if(zone == null || relationCluster == null)
            {
                return null;
            }

            List<Space> spaces = GetSpaces(zone);
            if(spaces == null)
            {
                return null;
            }

            List<Shell> shells = new List<Shell>();
            foreach(Space space in spaces)
            {
                Shell shell = GetShell(space);
                if(shell == null)
                {
                    continue;
                }

                shells.Add(shell);
            }

            if(!union)
            {
                return shells;
            }

            return shells.Union();
        }

        public Shell GetShell(Space space)
        {
            List<IPartition> partitions = GetPartitions(space);
            if (partitions == null || partitions.Count == 0)
            {
                return null;
            }

            return new Shell(partitions.ConvertAll(x => x.Face3D));
        }

        public double GetVolume(Space space, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            Shell shell = GetShell(space);
            if(shell == null)
            {
                return double.NaN;
            }

            return shell.Volume(silverSpacing, tolerance);
        }

        public double GetVolume(Zone zone, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            List<Space> spaces = GetSpaces(zone);
            if (spaces == null || spaces.Count == 0)
            {
                return double.NaN;
            }

            List<double> volumes = Enumerable.Repeat(0.0, spaces.Count).ToList();
            Parallel.For(0, spaces.Count, (int i) =>
            {
                Shell shell = GetShell(spaces[i]);
                if (shell == null)
                {
                    return;
                }

                double volume = shell.Volume(silverSpacing, tolerance);
                if (double.IsNaN(volume))
                {
                    return;
                }

                volumes[i] = volume;
            });

            return volumes.Sum();
        }

        public double GetArea(Space space, double offset, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
        {
            Shell shell = GetShell(space);
            if (shell == null)
            {
                return double.NaN;
            }

            return shell.Area(offset, tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }

        public double GetArea(Zone zone, double offset, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
        {
            List<Space> spaces = GetSpaces(zone);
            if(spaces == null || spaces.Count == 0)
            {
                return double.NaN;
            }

            List<double> areas = Enumerable.Repeat(0.0, spaces.Count).ToList();
            Parallel.For(0, spaces.Count, (int i) => 
            {
                Shell shell = GetShell(spaces[i]);
                if (shell == null)
                {
                    return;
                }

                double area = shell.Area(offset, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if(double.IsNaN(area))
                {
                    return;
                }

                areas[i] = area;
            });

            return areas.Sum();
        }

        public List<IPartition> GetPartitions(Space space)
        {
            return GetRelatedObjects<IPartition>(space);
        }

        public List<T> GetPartitions<T>(Space space) where T : IPartition
        {
            return GetRelatedObjects<T>(space);
        }

        public List<IPartition> GetPartitions(Zone zone)
        {
            if(zone == null || relationCluster == null)
            {
                return null;
            }

            List<Space> spaces = GetSpaces(zone);
            if(spaces == null)
            {
                return null;
            }

            Dictionary<Guid, IPartition> dictionary = new Dictionary<Guid, IPartition>();
            foreach(Space space in spaces)
            {
                List<IPartition> partitions = GetPartitions(space);
                if(partitions == null || partitions.Count == 0)
                {
                    continue;
                }

                foreach(IPartition partition in partitions)
                {
                    if(partition == null)
                    {
                        continue;
                    }

                    dictionary[partition.Guid] = partition;
                }
            }

            return dictionary.Values.ToList();
        }

        public List<IHostPartition> GetHostPartitions(HostPartitionType hostPartitionType)
        {
            return GetRelatedObjects<IHostPartition>(hostPartitionType);
        }

        public T GetHostPartition<T>(Guid openingGuid) where T : IHostPartition
        {
            if (openingGuid == Guid.Empty || relationCluster == null)
            {
                return default;
            }

            Func<T, bool> function = new Func<T, bool>((T hostPartition) =>
            {
                if (hostPartition == null)
                {
                    return false;
                }

                return hostPartition.HasOpening(openingGuid);
            });

            T result = relationCluster.GetObject(function);
            if (result == null)
            {
                return default;
            }

            return result.Clone();
        }

        public T GetHostPartition<T>(IOpening opening) where T: IHostPartition
        {
            if (opening == null || relationCluster == null)
            {
                return default;
            }

            return GetHostPartition<T>(opening.Guid);
        }

        public IHostPartition GetHostPartition(IOpening opening)
        {
            return GetHostPartition<IHostPartition>(opening);
        }

        public List<T> GetPartitions<T>(Zone zone) where T: IPartition
        {
            List<IPartition> partitions = GetPartitions(zone);
            if(partitions == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(IPartition partition in partitions)
            {
                if(partition is T)
                {
                    result.Add((T)partition);
                }

            }

            return result;
        }

        public List<IPartition> GetPartitions()
        {
            return GetObjects<IPartition>();
        }

        public List<T> GetPartitions<T>() where T : IPartition
        {
            return GetObjects<T>();
        }

        public List<T> GetOpenings<T>() where T : IOpening
        {
            List<IHostPartition> hostPartitions = relationCluster?.GetObjects<IHostPartition>();
            if(hostPartitions == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(IHostPartition hostPartition in hostPartitions)
            {
                List<T> openings = hostPartition.GetOpenings<T>();
                if(openings == null || openings.Count == 0)
                {
                    continue;
                }

                openings.ForEach(x => result.Add(x.Clone()));
            }

           return result;
        }

        public List<IOpening> GetOpenings()
        {
            return GetOpenings<IOpening>();
        }

        public List<T> GetMechanicalSystems<T>() where T : MechanicalSystem
        {
            return GetObjects<T>();
        }

        public List<IOpening> GetOpenings(OpeningType openingType)
        {
            if(relationCluster == null || openingType == null)
            {
                return null;
            }

            List<IHostPartition> hostPartitions = relationCluster.GetObjects<IHostPartition>();
            if(hostPartitions == null)
            {
                return null;
            }

            List<IOpening> openings = null;

            List <IOpening> result = new List<IOpening>();
            foreach(IHostPartition hostPartition in hostPartitions)
            {
                openings = hostPartition.GetOpenings();
                if(openings == null || openings.Count == 0)
                {
                    continue;
                }

                foreach(IOpening opening in openings)
                {
                    OpeningType openingType_Temp = opening?.Type();
                    if(openingType_Temp == null)
                    {
                        continue;
                    }

                    if(openingType_Temp.Guid == openingType.Guid)
                    {
                        result.Add(opening);
                    }
                }
            }

            openings = GetObjects<IOpening>();
            if (openings != null && openings.Count > 0)
            {
                result.AddRange(openings);
            }

            return result;
        }

        public List<InternalCondition> GetInternalConditions()
        {
            List<Space> spaces = GetSpaces();
            if(spaces == null)
            {
                return null;
            }

            Dictionary<Guid, InternalCondition> dictionary = new Dictionary<Guid, InternalCondition>();
            foreach(Space space in spaces)
            {
                InternalCondition internalCondition = space?.InternalCondition;
                if(internalCondition == null)
                {
                    continue;
                }

                dictionary[space.Guid] = internalCondition;
            }

            List<InternalCondition> internalConditions = GetObjects<InternalCondition>();
            if(internalConditions != null && internalConditions.Count > 0)
            {
                internalConditions.ForEach(x => dictionary[x.Guid] = x);
            }

            return dictionary.Values.ToList();
        }

        public List<Result> GetResults()
        {
            return GetObjects<Result>();
        }

        public List<T> GetResults<T>() where T: Result
        {
            return GetObjects<T>();
        }

        public List<T> GetResults<T>(IJSAMObject jSAMObject, string source = null) where T : Result
        {
            List<T> result = GetRelatedObjects<T>(jSAMObject);
            if (result == null)
            {
                return result;
            }

            if (source != null)
            {
                result.RemoveAll(x => x.Source == null || x.Source != source);
            }

            return result;
        }

        public List<SpaceSimulationResult> GetSpaceSimulationResults(Space space)
        {
            if(space == null || relationCluster == null)
            {
                return null;
            }

            return relationCluster.GetRelatedObjects<SpaceSimulationResult>(space)?.ConvertAll(x => new SpaceSimulationResult(x));
        }

        public List<PartitionSimulationResult> GetPartitionSimulationResults(IPartition partition)
        {
            if (partition == null || relationCluster == null)
            {
                return null;
            }

            return relationCluster.GetRelatedObjects<PartitionSimulationResult>(partition)?.ConvertAll(x => new PartitionSimulationResult(x));
        }

        public List<OpeningSimulationResult> GetOpeningSimulationResults(IOpening opening)
        {
            if (opening == null || relationCluster == null)
            {
                return null;
            }

            List<OpeningSimulationResult> result = relationCluster.GetRelatedObjects<OpeningSimulationResult>(opening)?.ConvertAll(x => new OpeningSimulationResult(x));

            IHostPartition hostPartition = GetHostPartition(opening);
            if(hostPartition != null)
            {
                List<OpeningSimulationResult> openingSimulationResults_HostPartition = relationCluster.GetRelatedObjects<OpeningSimulationResult>(hostPartition)?.ConvertAll(x => new OpeningSimulationResult(x));
                if(openingSimulationResults_HostPartition != null)
                {
                    if (result == null)
                    {
                        result = new List<OpeningSimulationResult>();
                    }

                    result.AddRange(openingSimulationResults_HostPartition);
                }
            }

            return result;
        }

        public List<BuildingModelSimulationResult> GetBuildingModelSimulationResults()
        {
            return GetObjects<BuildingModelSimulationResult>();
        }

        public bool Add(IPartition partition)
        {
            if(partition == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            IPartition partition_Temp = partition.Clone();

            List<HostPartitionType> hostPartitionTypes = relationCluster.GetRelatedObjects<HostPartitionType>(partition_Temp);
            hostPartitionTypes?.ForEach(x => relationCluster.RemoveRelation(partition_Temp, x));

            List<OpeningType> openingTypes = relationCluster.GetRelatedObjects<OpeningType>(partition_Temp);
            openingTypes?.ForEach(x => relationCluster.RemoveRelation(partition_Temp, x));

            if (!relationCluster.AddObject(partition_Temp))
            {
                return false;
            }

            if(partition_Temp is IHostPartition)
            {
                IHostPartition hostPartition = (IHostPartition)partition_Temp;

                HostPartitionType hostPartitionType = hostPartition.Type();
                if(hostPartitionType != null)
                {
                    HostPartitionType hostPartitionType_Temp = relationCluster.GetObject<HostPartitionType>(hostPartitionType.Guid);
                    if(hostPartitionType_Temp == null)
                    {
                        relationCluster.AddObject(hostPartitionType);
                        hostPartitionType_Temp = hostPartitionType;
                    }
                    else
                    {
                        hostPartition.Type(hostPartitionType_Temp);
                    }

                    relationCluster.AddRelation(partition_Temp, hostPartitionType_Temp);
                }

                List<IOpening> openings = hostPartition.GetOpenings();
                if(openings != null)
                {
                    Dictionary<Guid, OpeningType> dictionary = new Dictionary<Guid, OpeningType>();
                    foreach(IOpening opening in openings)
                    {
                        OpeningType openingType = opening?.Type();
                        if(openingType == null)
                        {
                            continue;
                        }

                        OpeningType openingType_Temp = relationCluster.GetObject<OpeningType>(openingType.Guid);
                        if(openingType_Temp == null)
                        {
                            relationCluster.AddObject(openingType);
                            openingType_Temp = openingType;
                        }
                        else
                        {
                            opening.Type(openingType_Temp);
                            hostPartition.AddOpening(opening);
                        }

                        relationCluster.AddRelation(partition_Temp, openingType_Temp);
                    }
                }
            }

            return true;
        }

        public bool Add(Space space, IEnumerable<IPartition> partitions = null)
        {
            if (space == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            Space space_Temp = new Space(space);

            InternalCondition internalCondition = space_Temp.InternalCondition;
            if(internalCondition != null)
            {
                List<InternalCondition> internalConditions = GetInternalConditions();
                if(internalConditions != null)
                {
                    List<Guid> guids = internalConditions.ConvertAll(x => x.Guid);
                    Guid guid = internalCondition.Guid;
                    while(guids.Contains(guid))
                    {
                        guid = Guid.NewGuid();
                    }

                    if(internalCondition.Guid != guid)
                    {
                        space_Temp.InternalCondition = new InternalCondition(guid, internalCondition);
                    }
                }
            }

            bool result = relationCluster.AddObject(space_Temp);
            if(!result)
            {
                return result;
            }

            if(partitions != null && partitions.Count() != 0)
            {
                foreach(IPartition partition in partitions)
                {
                    if(Add(partition))
                    {
                        relationCluster.AddRelation(space_Temp, partition);
                    }
                }
            }

            return result;
        }

        public bool Add(IMaterial material)
        {
            if (material == null)
            {
                return false;
            }

            if (materialLibrary == null)
            {
                materialLibrary = new MaterialLibrary(string.Empty);
            }

            return materialLibrary.Add(material.Clone());
        }

        public bool Add(Profile profile)
        {
            if (profile == null)
            {
                return false;
            }

            if (profileLibrary == null)
            {
                profileLibrary = new ProfileLibrary(string.Empty);
            }

            return profileLibrary.Add(profile.Clone());
        }

        public bool Add(IOpening opening, double tolerance = Tolerance.Distance)
        {
            if(opening == null)
            {
                return false;
            }

            bool valid = false;
            IHostPartition hostPartition_Opening = null;
            List<IHostPartition> hostPartitions_Opening = new List<IHostPartition>();

            List<IHostPartition> hostPartitions = relationCluster?.GetObjects<IHostPartition>();
            if(hostPartitions != null && hostPartitions.Count == 0)
            {
                foreach(IHostPartition hostPartition in hostPartitions)
                {
                    if (hostPartition.HasOpening(opening.Guid))
                    {
                        hostPartition_Opening = hostPartition;
                        if (Query.IsValid(hostPartition, opening, tolerance))
                        {
                            valid = true;
                            break;
                        }
                    }

                    if (Query.IsValid(hostPartition, opening, tolerance))
                    {
                        hostPartitions_Opening.Add(hostPartition);
                    }
                }
            }

            opening = opening.Clone();

            OpeningType openingType = opening.Type();
            if(openingType != null)
            {
                OpeningType openingType_Temp = relationCluster.GetObject<OpeningType>(openingType.Guid);
                if (openingType_Temp == null)
                {
                    relationCluster.AddObject(openingType);
                    openingType_Temp = openingType;
                }
                else
                {
                    opening.Type(openingType_Temp);
                }
            }

            if (valid)
            {
                hostPartition_Opening.AddOpening(opening);
            }

            if(hostPartitions_Opening == null || hostPartitions_Opening.Count == 0)
            {
                relationCluster.AddObject(opening);
                relationCluster.AddRelation(opening, openingType);
                return true;
            }

            if(hostPartition_Opening != null)
            {
                hostPartition_Opening.RemoveOpening(opening.Guid);
                relationCluster.RemoveRelation(hostPartition_Opening, openingType);
            }

            if(hostPartitions_Opening.Count > 0 )
            {
                Point3D point3D = opening.Face3D.InternalPoint3D();
                if(point3D != null)
                {
                    hostPartitions_Opening.Sort((x, y) => x.Face3D.Distance(point3D, tolerance).CompareTo(y.Face3D.Distance(point3D, tolerance)));
                }
            }

            List<IOpening> openings_Add = hostPartitions_Opening[0].AddOpening(opening, tolerance);
            if (openings_Add != null && openings_Add.Count != 0)
            {
                relationCluster.AddRelation(hostPartitions_Opening[0], openingType);
                return true;
            }

            return false;
        }

        public bool Add(Zone zone, IEnumerable<Space> spaces = null)
        {
            if(zone == null)
            {
                return false;
            }

            Zone zone_Temp = zone.Clone();

            bool result = relationCluster.AddObject(zone_Temp);
            if(!result)
            {
                return result;
            }
            
            if(spaces != null && spaces.Count() != 0)
            {
                foreach(Space space in spaces)
                {
                    if(space == null)
                    {
                        continue;
                    }

                    Space space_Temp = relationCluster.GetObject<Space>(space.Guid);
                    if(space_Temp == null)
                    {
                        continue;
                    }

                    relationCluster.AddRelation(zone_Temp, space_Temp);
                }
            }

            return result;

        }

        public bool Add(InternalCondition internalCondition)
        {
            if(internalCondition == null)
            {
                return false;
            }

            InternalCondition internalCondition_Temp = new InternalCondition(internalCondition);

            bool exists = false;
            List<Space> spaces = relationCluster?.GetObjects<Space>();
            if(spaces != null && spaces.Count != 0)
            {
                foreach(Space space in spaces)
                {
                    InternalCondition internalCondition_Space = space?.InternalCondition;
                    if(internalCondition_Space == null)
                    {
                        continue;
                    }

                    if(internalCondition_Temp.Guid == internalCondition_Space.Guid)
                    {
                        space.InternalCondition = internalCondition_Temp;
                        exists = true;
                    }
                }
            }

            if(exists)
            {
                return true;
            }

            return relationCluster.AddObject(internalCondition_Temp);
        }

        public bool Add(HostPartitionType hostPartitionType)
        {
            if(hostPartitionType != null)
            {
                return false;
            }

            HostPartitionType hostPartitionType_Temp = hostPartitionType.Clone();

            if(relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            relationCluster.AddObject(hostPartitionType_Temp);

            List<IHostPartition> hostPartitions = relationCluster.GetRelatedObjects<IHostPartition>(hostPartitionType_Temp);
            if(hostPartitions != null)
            {
                foreach(IHostPartition hostPartition in hostPartitions)
                {
                    hostPartition.Type(hostPartitionType_Temp);
                }
            }

            return true;
        }

        public bool Add(OpeningType openingType)
        {
            if (openingType != null)
            {
                return false;
            }

            OpeningType openingType_Temp = openingType.Clone();

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            relationCluster.AddObject(openingType_Temp);

            List<IHostPartition> hostPartitions = relationCluster.GetRelatedObjects<IHostPartition>(openingType_Temp);
            if (hostPartitions != null)
            {
                foreach (IHostPartition hostPartition in hostPartitions)
                {
                    List<IOpening> openings = hostPartition?.GetOpenings();
                    if(openings != null && openings.Count != 0)
                    {
                        foreach(IOpening opening in openings)
                        {
                            OpeningType openingType_Opening = opening?.Type();
                            if(openingType_Opening == null)
                            {
                                continue;
                            }

                            if(openingType_Opening.Guid == openingType_Temp.Guid)
                            {
                                opening.Type(openingType_Temp);
                                hostPartition.AddOpening(opening);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool Add(MechanicalSystemType mechanicalSystemType)
        {
            if (mechanicalSystemType != null)
            {
                return false;
            }

            MechanicalSystemType mechanicalSystemType_Temp = mechanicalSystemType.Clone();

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            relationCluster.AddObject(mechanicalSystemType_Temp);

            List<MechanicalSystem> mechanicalSystems = relationCluster.GetRelatedObjects<MechanicalSystem>(mechanicalSystemType_Temp);
            if (mechanicalSystems != null)
            {
                foreach (MechanicalSystem mechanicalSystem in mechanicalSystems)
                {
                    mechanicalSystem.Type(mechanicalSystemType_Temp);
                }
            }

            return true;
        }

        public bool Add(BuildingModelSimulationResult buildingModelSimulationResult)
        {
            if (buildingModelSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            BuildingModelSimulationResult buildingModelSimulationResult_Temp = new BuildingModelSimulationResult(buildingModelSimulationResult);

            return relationCluster.AddObject(buildingModelSimulationResult_Temp);
        }

        public bool Add(MechanicalSystem mechanicalSystem, IEnumerable<Space> spaces = null)
        {
            if(mechanicalSystem == null)
            {
                return false;
            }

            if(relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            MechanicalSystem machanicalSystem_Temp = mechanicalSystem.Clone();

            MechanicalSystemType mechanicalSystemType = machanicalSystem_Temp.Type;
            if(mechanicalSystemType != null)
            {
                MechanicalSystemType mechanicalSystemType_Temp = relationCluster.GetObject<MechanicalSystemType>(mechanicalSystemType.Guid);
                if(mechanicalSystemType_Temp == null)
                {
                    relationCluster.AddObject(mechanicalSystemType);
                    mechanicalSystemType_Temp = mechanicalSystemType;
                }
                else
                {
                    machanicalSystem_Temp.Type(mechanicalSystemType_Temp);
                }

                relationCluster.AddRelation(machanicalSystem_Temp, machanicalSystem_Temp);
            }

            bool result = relationCluster.AddObject(machanicalSystem_Temp);
            if(!result)
            {
                return result;
            }

            if(spaces != null)
            {
                foreach(Space space in spaces)
                {
                    if (Add(space))
                    {
                        relationCluster.AddRelation(machanicalSystem_Temp, space);
                    }
                }
            }

            return result;
        }

        public bool Add(SpaceSimulationResult spaceSimulationResult, Space space = null)
        {
            if(spaceSimulationResult == null)
            {
                return false;
            }

            if(relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            bool result = relationCluster.AddObject(spaceSimulationResult);
            if(!result)
            {
                return result;
            }

            if(space != null)
            {
                if(Add(space))
                {
                    relationCluster.AddRelation(spaceSimulationResult, space);
                }
            }

            return result;
        }

        public bool Add(SpaceSimulationResult spaceSimulationResult, Guid spaceGuid)
        {
            if (spaceSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            bool result = relationCluster.AddObject(spaceSimulationResult);
            if (!result)
            {
                return result;
            }

            if (spaceGuid != Guid.Empty)
            {
                Space space = relationCluster.GetObject<Space>(spaceGuid);
                if (space != null)
                {
                    relationCluster.AddRelation(spaceSimulationResult, space);
                }
            }

            return result;
        }

        public bool Add(PartitionSimulationResult partitionSimulationResult, IPartition partition = null)
        {
            if(partitionSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            PartitionSimulationResult partitionSimulationResult_Temp = new PartitionSimulationResult(partitionSimulationResult);

            bool result = relationCluster.AddObject(partitionSimulationResult_Temp);
            if(!result)
            {
                return result;
            }

            if(partition != null)
            {
                if(Add(partition))
                {
                    relationCluster.AddRelation(partitionSimulationResult, partition);
                }
            }

            return result;
        }
       
        public bool Add(PartitionSimulationResult partitionSimulationResult, Guid partitionGuid)
        {
            if (partitionSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            PartitionSimulationResult partitionSimulationResult_Temp = new PartitionSimulationResult(partitionSimulationResult);

            bool result = relationCluster.AddObject(partitionSimulationResult_Temp);
            if (!result)
            {
                return result;
            }

            if (partitionGuid != Guid.Empty)
            {
                IPartition partition = relationCluster.GetObject<IPartition>(partitionGuid);
                if (partition != null)
                {
                    relationCluster.AddRelation(partitionSimulationResult, partition);
                }
            }

            return result;
        }

        public bool Add(OpeningSimulationResult openingSimulationResult, IOpening opening = null, double tolerance = Tolerance.Distance)
        {
            if (openingSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            OpeningSimulationResult openingSimulationResult_Temp = new OpeningSimulationResult(openingSimulationResult);

            bool result = relationCluster.AddObject(openingSimulationResult_Temp);
            if (!result)
            {
                return result;
            }

            if (opening != null)
            {
                if (Add(opening, tolerance))
                {
                    IOpening opening_Temp = relationCluster.GetObject<IOpening>(opening.Guid);
                    if(opening_Temp != null)
                    {
                        relationCluster.AddRelation(openingSimulationResult, opening_Temp);
                    }
                    else
                    {
                        IHostPartition hostPartition = GetHostPartition(opening);
                        if (hostPartition != null)
                        {
                            relationCluster.AddRelation(openingSimulationResult, opening);
                        }
                    }
                }
            }

            return result;
        }

        public bool Add(OpeningSimulationResult openingSimulationResult, Guid openingGuid)
        {
            if (openingSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            OpeningSimulationResult openingSimulationResult_Temp = new OpeningSimulationResult(openingSimulationResult);

            bool result = relationCluster.AddObject(openingSimulationResult_Temp);
            if (!result)
            {
                return result;
            }

            if (openingGuid != Guid.Empty)
            {
                IOpening opening_Temp = relationCluster.GetObject<IOpening>(openingGuid);
                if (opening_Temp != null)
                {
                    relationCluster.AddRelation(openingSimulationResult, opening_Temp);
                }
                else
                {
                    IHostPartition hostPartition = GetHostPartition<IHostPartition>(openingGuid);
                    if (hostPartition != null)
                    {
                        relationCluster.AddRelation(openingSimulationResult, hostPartition);
                    }
                }
            }

            return result;
        }

        public bool Add(ZoneSimulationResult zoneSimulationResult, Zone zone = null)
        {
            if (zoneSimulationResult == null)
            {
                return false;
            }

            if (relationCluster == null)
            {
                relationCluster = new RelationCluster();
            }

            bool result = relationCluster.AddObject(zoneSimulationResult);
            if (!result)
            {
                return result;
            }

            if(zone != null)
            {
                if(relationCluster.AddObject(zone))
                {
                    relationCluster.AddRelation(zoneSimulationResult, zone);
                }
            }

            return true;
        }

        public bool Contains(ISAMObject sAMObject)
        {
            if(sAMObject is Profile)
            {
                if(profileLibrary == null)
                {
                    return false;
                }

                return profileLibrary.Contains((Profile)sAMObject);
            }
            
            if(sAMObject is IMaterial)
            {
                if(materialLibrary == null)
                {
                    return false;
                }

                return materialLibrary.Contains((IMaterial)sAMObject);
            }

            if(relationCluster == null)
            {
                return false;
            }

            if(sAMObject is IOpening)
            {
                List<IHostPartition> hostPartitions = relationCluster.GetObjects<IHostPartition>();
                if(hostPartitions != null && hostPartitions.Count != 0)
                {
                    if(hostPartitions.Find(x => x.HasOpening(sAMObject.Guid)) != null)
                    {
                        return true;
                    }
                }
            }

            if(sAMObject is InternalCondition)
            {
                List<Space> spaces = relationCluster.GetObjects<Space>();
                if (spaces != null && spaces.Count != 0)
                {
                    foreach(Space space in spaces)
                    {
                        InternalCondition internalCondition = space?.InternalCondition;
                        if(internalCondition == null)
                        {
                            continue;
                        }

                        if(internalCondition.Guid == sAMObject.Guid)
                        {
                            return true; 
                        }
                    }
                }
            }

            return relationCluster.Contains(sAMObject);
        }

        public BoundingBox3D GetBoundingBox3D()
        {
            List<IBoundable3DObject> boundable3DObjects = relationCluster?.GetObjects<IBoundable3DObject>();
            if(boundable3DObjects == null || boundable3DObjects.Count == 0)
            {
                return null;
            }

            return new BoundingBox3D(boundable3DObjects.ConvertAll(x => x.GetBoundingBox()).FindAll(x => x != null));
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

            if (jObject.ContainsKey("RelationCluster"))
                relationCluster = new RelationCluster(jObject.Value<JObject>("RelationCluster"));

            if (jObject.ContainsKey("Terrain"))
                terrain = Core.Create.IJSAMObject<Terrain>(jObject.Value<JObject>("Terrain"));

            if (jObject.ContainsKey("MaterialLibrary"))
                materialLibrary = Core.Create.IJSAMObject<MaterialLibrary>(jObject.Value<JObject>("MaterialLibrary"));

            if (jObject.ContainsKey("ProfileLibrary"))
                profileLibrary = Core.Create.IJSAMObject<ProfileLibrary>(jObject.Value<JObject>("ProfileLibrary"));

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

            if (relationCluster != null)
                jObject.Add("RelationCluster", relationCluster.ToJObject());

            if (terrain != null)
                jObject.Add("Terrain", terrain.ToJObject());

            if (materialLibrary != null)
                jObject.Add("MaterialLibrary", materialLibrary.ToJObject());

            if (profileLibrary != null)
                jObject.Add("ProfileLibrary", profileLibrary.ToJObject());

            return jObject;
        }

        public void Transform(Transform3D transform3D)
        {
            List<ISAMGeometry3DObject> sAMGeometry3DObjects = relationCluster?.GetObjects<ISAMGeometry3DObject>();
            if(sAMGeometry3DObjects == null || sAMGeometry3DObjects.Count == 0)
            {
                return;
            }

            foreach(ISAMGeometry3DObject sAMGeometry3DObject in sAMGeometry3DObjects)
            {
                if(sAMGeometry3DObject == null)
                {
                    continue;
                }

                sAMGeometry3DObject.Transform(transform3D);
            }
            
        }
    }
}
