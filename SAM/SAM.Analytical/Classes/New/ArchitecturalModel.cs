﻿using Newtonsoft.Json.Linq;
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
        
        public List<Room> GetRooms(IPartition partition)
        {
            return GetRelatedObjects<Room>(partition);
        }

        public List<Room> GetRooms(Zone zone)
        {
            return GetRelatedObjects<Room>(zone);
        }

        public List<Room> GetRooms()
        {
            return GetObjects<Room>();
        }

        public bool Internal(IPartition partition)
        {
            List<Room> rooms = relationCluster?.GetRelatedObjects<Room>(partition);
            return rooms != null && rooms.Count > 1;
        }
        
        public bool External(IPartition partition)
        {
            List<Room> rooms = relationCluster?.GetRelatedObjects<Room>(partition);
            return rooms != null && rooms.Count == 1;
        }

        public bool Shade(IPartition partition)
        {
            List<Room> rooms = relationCluster?.GetRelatedObjects<Room>(partition);
            return rooms == null || rooms.Count == 0;
        }

        public List<Shell> GetShells()
        {
            List<Room> rooms = relationCluster?.GetObjects<Room>();
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
            return GetRelatedObjects<IPartition>(room);
        }

        public List<IPartition> GetPartitions(Zone zone)
        {
            if(zone == null || relationCluster == null)
            {
                return null;
            }

            List<Room> rooms = GetRooms(zone);
            if(rooms == null)
            {
                return null;
            }

            Dictionary<Guid, IPartition> dictionary = new Dictionary<Guid, IPartition>();
            foreach(Room room in rooms)
            {
                List<IPartition> partitions = GetPartitions(room);
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

            return relationCluster.AddObject(partition.Clone());
        }

        public bool Add(Room room, IEnumerable<IPartition> partitions = null)
        {
            if (room == null)
            {
                return false;
            }

            if (relationCluster == null)
                relationCluster = new RelationCluster();

            Room room_Temp = room.Clone();

            bool result = relationCluster.AddObject(room_Temp);
            if(partitions != null && partitions.Count() != 0)
            {
                foreach(IPartition partition in partitions)
                {
                    IPartition partition_Temp = partition.Clone();
                    if (relationCluster.AddObject(partition_Temp))
                    {
                        relationCluster.AddRelation(room_Temp, partition_Temp);
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

        public bool Add(Zone zone, IEnumerable<Room> rooms = null)
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
            
            if(rooms != null && rooms.Count() != 0)
            {
                foreach(Room room in rooms)
                {
                    Room room_Temp = room.Clone();
                    if(relationCluster.AddObject(room_Temp))
                    {
                        relationCluster.AddRelation(zone_Temp, room_Temp);
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