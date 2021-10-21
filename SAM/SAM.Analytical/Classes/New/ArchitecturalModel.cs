using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Architectural;

namespace SAM.Analytical
{
    public class ArchitecturalModel : SAMModel, IAnalyticalObject
    {
        private string description;
        private Location location;
        private Address address;
        private Terrain terrain;
        private RelationCluster relationCluster;
        private MaterialLibrary materialLibrary;
        private ProfileLibrary profileLibrary;

        public ArchitecturalModel(JObject jObject)
            : base(jObject)
        {

        }

        public ArchitecturalModel(ArchitecturalModel architecturalModel)
            : base(architecturalModel)
        {
            if (architecturalModel == null)
            {
                return;
            }

            description = architecturalModel.description;
            location = architecturalModel.location?.Clone();
            address = architecturalModel.address?.Clone();
            terrain = architecturalModel.terrain?.Clone();
            materialLibrary = architecturalModel.materialLibrary?.Clone();

            relationCluster = architecturalModel.relationCluster?.Clone();

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

        public ArchitecturalModel(string description, Location location, Address address, Terrain terrain, MaterialLibrary materialLibrary, ProfileLibrary profileLibrary)
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

            return relationCluster.AddRelation(jSAMObject_1.Clone(), jSAMObject_2.Clone());
        }

        public IMaterial GetMaterial(string name)
        {
            if(materialLibrary == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            return materialLibrary.GetObject<IMaterial>(name)?.Clone();
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

        public bool HasMaterial(string name)
        {
            if (materialLibrary == null || string.IsNullOrEmpty(name))
            {
                return false;
            }

            return materialLibrary.GetObject<IMaterial>(name) != null;
        }

        public IMaterial GetMaterial(MaterialLayer materialLayer)
        {
            if(materialLayer == null || materialLibrary == null)
            {
                return null;
            }

            return materialLayer.Material(materialLibrary)?.Clone();
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

        public bool Internal(IPartition partition)
        {
            List<Space> spaces = relationCluster?.GetRelatedObjects<Space>(partition);
            return spaces != null && spaces.Count > 1;
        }
        
        public bool External(IPartition partition)
        {
            List<Space> spaces = relationCluster?.GetRelatedObjects<Space>(partition);
            return spaces != null && spaces.Count == 1;
        }

        public bool Shade(IPartition partition)
        {
            List<Space> spaces = relationCluster?.GetRelatedObjects<Space>(partition);
            return spaces == null || spaces.Count == 0;
        }

        public bool Underground(IPartition partition)
        {
            if(terrain == null || partition == null)
            {
                return false;
            }

            return terrain.Below(partition);
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

        public double GetArea(Space space, double offset, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
        {
            Shell shell = GetShell(space);
            if (shell == null)
            {
                return double.NaN;
            }

            return shell.Area(offset, tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }

        public List<IPartition> GetPartitions(Space space)
        {
            return GetRelatedObjects<IPartition>(space);
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

        public List<T> GetPartitions<T>() where T : IHostPartition
        {
            return GetObjects<T>();
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

            return dictionary.Values.ToList();
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

                List<IOpening> openings = hostPartition.Openings;
                if(openings != null)
                {
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

                        relationCluster.AddRelation(opening, openingType_Temp);
                    }
                }
            }

            return relationCluster.AddObject(partition_Temp);
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
