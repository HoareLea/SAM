using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
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

        public ArchitecturalModel(string description, Location location, Address address, Terrain terrain)
            : base()
        {
            this.description = description;
            this.location = location?.Clone();
            this.address = address?.Clone();
            this.terrain = terrain?.Clone();
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

        public List<Room> GetRooms(Partition partition)
        {
            return relationCluster?.GetRelatedObjects<Room>(partition);
        }

        public bool Internal(HostPartition hostPartition)
        {
            List<Room> rooms = GetRooms(hostPartition);
            return rooms != null || rooms.Count > 2;
        }
        
        public bool External(HostPartition hostPartition)
        {
            List<Room> rooms = GetRooms(hostPartition);
            return rooms != null && rooms.Count == 1;
        }

        public bool Shade(HostPartition hostPartition)
        {
            List<Room> rooms = GetRooms(hostPartition);
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

        public Shell GetShell(Room room)
        {
            if (relationCluster == null || room == null)
                return null;

            List<object> relatedObjects = relationCluster.GetRelatedObjects(room);
            if (relatedObjects == null || relatedObjects.Count == 0)
                return null;

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(object relatedObject in relatedObjects)
            {
                Partition partition = relatedObject as Partition;
                if(partition == null)
                {
                    continue;
                }

                Face3D face3D = partition.Face3D;
                if(face3D == null)
                {
                    continue;
                }

                face3Ds.Add(face3D);
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            return new Shell(face3Ds);
        }

        public bool Add(Partition partition)
        {
            if(partition == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            return relationCluster.AddObject(partition);
        }

        public bool Add(Room room, IEnumerable<Partition> partitions = null)
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
                foreach(HostPartition partition in partitions)
                {
                    if(relationCluster.AddObject(partition))
                    {
                        relationCluster.AddRelation(room, partition);
                    }

                }
            }

            return result;
        }

        public BoundingBox3D GetBoundingBox3D()
        {
            List<BuildingElement> buildingElements = GetObjects<BuildingElement>();
            if(buildingElements == null || buildingElements.Count == 0)
            {
                return null;
            }

            return new Geometry.Spatial.BoundingBox3D(buildingElements.ConvertAll(x => x.Face3D?.GetBoundingBox()).FindAll(x => x != null));
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
