using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public class ArchitecturalModel : SAMModel, IArchitecturalObject
    {
        private string description;
        private Location location;
        private Address address;
        private Terrain terrain;
        private RelationCluster relationCluster;
        private MaterialLibrary materialLibrary;

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

        public ArchitecturalModel(string description, Location location, Address address, Terrain terrain, MaterialLibrary materialLibrary)
            : base()
        {
            this.description = description;
            this.location = location?.Clone();
            this.address = address?.Clone();
            this.terrain = terrain?.Clone();
            this.materialLibrary = materialLibrary.Clone();
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

        public List<T> GetObjects<T>()
        {
            List<string> typeNames = relationCluster?.GetTypeNames(typeof(T));
            if(typeNames == null || typeNames.Count == 0)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(string typeName in typeNames)
            {
                List<T> objects = relationCluster.GetObjects<T>(typeName);
                if (objects != null && objects.Count != 0)
                {
                    result.AddRange(objects);
                }
            }

            return result;
        }

        public List<T> GetObjects<T>(params Func<T, bool>[] functions)
        {
            if(functions == null)
            {
                return null;
            }

            List<T> result = GetObjects<T>();
            if (result == null)
            {
                return null;
            }

            for (int i = result.Count - 1; i >= 0; i--)
            {
                bool remove = false;
                foreach(Func<T, bool> function in functions)
                {
                    if(!function(result[i]))
                    {
                        remove = true;
                        break;
                    }
                }

                if(remove)
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }

        public List<T> GetRelatedObjects<T>(object @object)
        {
            if(@object == null)
            {
                return null;
            }

            return relationCluster?.GetRelatedObjects<T>(@object);
        }

        public List<object> GetRelatedObjects(object @object, Type type = null)
        {
            if (@object == null)
            {
                return null;
            }

            if(type == null)
            {
                return relationCluster?.GetRelatedObjects(@object);
            }

            return relationCluster?.GetRelatedObjects(@object, type);
        }

        public bool RemoveObject(object @object)
        {
            if(@object == null || relationCluster == null)
            {
                return false;
            }

            Guid guid = relationCluster.GetGuid(@object);
            if(guid == Guid.Empty)
            {
                return false;
            }

            return relationCluster.RemoveObject(@object.GetType(), guid);
        }

        public bool AddRelation(object object_1, object object_2)
        {
            if(object_1 == null || object_2 == null || relationCluster == null)
            {
                return false;
            }

            return relationCluster.AddRelation(object_1, object_2);
        }

        public bool AddMaterial(IMaterial material)
        {
            if(material == null)
            {
                return false;
            }

            if(materialLibrary == null)
            {
                materialLibrary = new MaterialLibrary(string.Empty);
            }

            return materialLibrary.Add(material);
        }

        public IMaterial GetMaterial(string name)
        {
            if(materialLibrary == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            return materialLibrary.GetObject<IMaterial>(name);
        }

        public IMaterial GetMaterial(MaterialLayer materialLayer)
        {
            if(materialLayer == null || materialLibrary == null)
            {
                return null;
            }

            return materialLayer.Material(materialLibrary);
        }

        public List<IMaterial> GetMaterials(HostPartitionType hostPartitionType)
        {
            if(hostPartitionType == null)
            {
                return null;
            }

            return hostPartitionType.Materials(materialLibrary);
        }

        public List<T> GetHostPartitionTypes<T>() where T: HostPartitionType
        {
            List<IHostPartition> hostPartitions = GetPartitions<IHostPartition>();
            if(hostPartitions == null || hostPartitions.Count == 0)
            {
                return null;
            }

            Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            foreach(IHostPartition hostPartition in hostPartitions)
            {
                T hostPartitionType = hostPartition?.Type() as T;
                if(hostPartitionType == null)
                {
                    continue;
                }

                dictionary[hostPartitionType.Guid] = hostPartitionType;
            }

            return dictionary.Values.ToList();
        }

        public List<T> GetOpeningTypes<T>() where T: OpeningType
        {
            List<IHostPartition> hostPartitions = GetPartitions<IHostPartition>();
            if (hostPartitions == null || hostPartitions.Count == 0)
            {
                return null;
            }

            Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
            foreach (IHostPartition hostPartition in hostPartitions)
            {
                List<IOpening> openings = hostPartition?.Openings;
                if(openings == null || openings.Count == 0)
                {
                    continue;
                }

                foreach(IOpening opening in openings)
                {
                    T openingType = opening.Type() as T;
                    if(openingType == null)
                    {
                        continue;
                    }

                    dictionary[openingType.Guid] = openingType;
                }
            }

            return dictionary.Values.ToList();
        }

        public MaterialType GetMaterialType(HostPartitionType hostPartitionType)
        {
            return Query.MaterialType(hostPartitionType?.MaterialLayers, materialLibrary);
        }

        public MaterialType GetMaterialType(IHostPartition hostPartition)
        {
            if(hostPartition == null)
            {
                return MaterialType.Undefined;
            }

            return GetMaterialType(hostPartition.Type());
        }
        
        public List<Room> GetRooms(IPartition partition)
        {
            return relationCluster?.GetRelatedObjects<Room>(partition);
        }

        public bool Internal(IPartition partition)
        {
            List<Room> rooms = GetRooms(partition);
            return rooms != null && rooms.Count > 1;
        }
        
        public bool External(IPartition partition)
        {
            List<Room> rooms = GetRooms(partition);
            return rooms != null && rooms.Count == 1;
        }

        public bool Shade(IPartition partition)
        {
            List<Room> rooms = GetRooms(partition);
            return rooms == null || rooms.Count == 0;
        }

        public List<Room> GetRooms()
        {
            return relationCluster?.GetObjects<Room>();
        }

        public List<Shell> GetShells()
        {
            List<Room> rooms = GetRooms();
            if(rooms == null)
            {
                return null;
            }

            List<Shell> result = new List<Shell>();
            foreach(Room room in rooms)
            {
                Shell shell = GetShell(room);
                if(shell != null)
                {
                    result.Add(shell);
                }
            }

            return result;
        }

        public List<IPartition> GetPartitions(Room room)
        {
            if (relationCluster == null || room == null)
            {
                return null;
            }

            List<object> relatedObjects = relationCluster.GetRelatedObjects(room);
            if (relatedObjects == null || relatedObjects.Count == 0)
            {
                return null;
            }

            List<IPartition> result = new List<IPartition>();
            foreach (object relatedObject in relatedObjects)
            {
                IPartition partition = relatedObject as IPartition;
                if (partition == null)
                {
                    continue;
                }

                Face3D face3D = partition.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                result.Add(partition);
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

        public Shell GetShell(Room room)
        {
            List<IPartition> partitions = GetPartitions(room);
            if (partitions == null || partitions.Count == 0)
            {
                return null;
            }

            return new Shell(partitions.ConvertAll(x => x.Face3D));
        }

        public bool Add(IPartition partition)
        {
            if(partition == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            return relationCluster.AddObject(partition);
        }

        public bool Add(Room room, IEnumerable<IPartition> partitions = null)
        {
            if (room == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            bool result = relationCluster.AddObject(room);
            if(partitions != null && partitions.Count() != 0)
            {
                foreach(IPartition partition in partitions)
                {
                    if(relationCluster.AddObject(partition))
                    {
                        relationCluster.AddRelation(room, partition);
                    }

                }
            }

            return result;
        }

        public bool Contains(ISAMObject sAMObject)
        {
            if(relationCluster == null)
            {
                return false;
            }

            return relationCluster.Contains(sAMObject);
        }

        public BoundingBox3D GetBoundingBox3D()
        {
            List<IBoundable3DObject> boundable3DObjects = GetObjects<IBoundable3DObject>();
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

            return jObject;
        }
    }
}
