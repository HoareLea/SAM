using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

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

        public List<Room> GetRooms(HostBuildingElement hostBuildingElement)
        {
            return relationCluster?.GetRelatedObjects<Room>(hostBuildingElement);
        }

        public bool Internal(HostBuildingElement hostBuildingElement)
        {
            List<Room> rooms = GetRooms(hostBuildingElement);
            return rooms != null || rooms.Count > 2;
        }
        
        public bool External(HostBuildingElement hostBuildingElement)
        {
            List<Room> rooms = GetRooms(hostBuildingElement);
            return rooms != null && rooms.Count == 1;
        }

        public bool Shade(HostBuildingElement hostBuildingElement)
        {
            List<Room> rooms = GetRooms(hostBuildingElement);
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

            List<HostBuildingElement> hostBuildingElements = relationCluster.GetRelatedObjects<HostBuildingElement>(room);
            if (hostBuildingElements == null || hostBuildingElements.Count == 0)
                return null;

            return new Shell(hostBuildingElements.ConvertAll(x => x.Face3D));
        }

        public bool Add(HostBuildingElement hostBuildingElement)
        {
            if(hostBuildingElement == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            return relationCluster.AddObject(hostBuildingElement);
        }

        public bool Add(Room room, List<HostBuildingElement> hostBuildingElements = null)
        {
            if (room == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            bool result = relationCluster.AddObject(room);
            if(hostBuildingElements != null && hostBuildingElements.Count != 0)
            {
                foreach(HostBuildingElement hostBuildingElement in hostBuildingElements)
                {
                    if(relationCluster.AddObject(hostBuildingElement))
                    {
                        relationCluster.AddRelation(room, hostBuildingElement);
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
