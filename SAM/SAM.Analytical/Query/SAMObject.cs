using System;
using System.Collections.Generic;
using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static T SAMObject<T>(this AnalyticalModel analyticalModel, Guid guid) where T: ISAMObject
        {
            ISAMObject jSAMObject = SAMObject(analyticalModel, typeof(T), guid);
            if (jSAMObject == null)
                return default;

            return (T)jSAMObject;
        }

        public static T SAMObject<T>(this AdjacencyCluster adjacencyCluster, Guid guid) where T : ISAMObject
        {
            ISAMObject jSAMObject = SAMObject(adjacencyCluster, typeof(T), guid);
            if (jSAMObject == null)
                return default;

            return (T)jSAMObject;
        }

        public static T SAMObject<T>(this ProfileLibrary profileLibrary, Guid guid)where T : ISAMObject
        {
            ISAMObject jSAMObject = SAMObject(profileLibrary, guid);
            if (jSAMObject == null)
                return default;

            return (T)jSAMObject;
        }

        public static ISAMObject SAMObject(this AnalyticalModel analyticalModel, Type type, Guid guid)
        {
            if (analyticalModel == null || type == null)
                return null;

            if(typeof(Profile).IsAssignableFrom(type))
                return analyticalModel.ProfileLibrary.SAMObject(guid);

            if (typeof(IMaterial).IsAssignableFrom(type))
                return analyticalModel.MaterialLibrary?.GetMaterials()?.Find(x => x.Guid == guid);

            if (typeof(Space).IsAssignableFrom(type))
                return analyticalModel.AdjacencyCluster?.SAMObject(type, guid);

            if (typeof(Aperture).IsAssignableFrom(type))
                return analyticalModel.AdjacencyCluster?.SAMObject(type, guid);

            if (typeof(ApertureConstruction).IsAssignableFrom(type))
                return analyticalModel.AdjacencyCluster?.SAMObject(type, guid);

            if (typeof(Panel).IsAssignableFrom(type))
                return analyticalModel.AdjacencyCluster?.SAMObject(type, guid);

            if (typeof(Construction).IsAssignableFrom(type))
                return analyticalModel.AdjacencyCluster?.SAMObject(type, guid);

            if (typeof(InternalCondition).IsAssignableFrom(type))
                return analyticalModel.AdjacencyCluster?.SAMObject(type, guid);

            if(typeof(Location).IsAssignableFrom(type))
            {
                Location location = analyticalModel.Location;
                if (location != null)
                    return location.Guid.Equals(guid) ? location : null;
            }

            if (typeof(Address).IsAssignableFrom(type))
            {
                Address address = analyticalModel.Address;
                if (address != null)
                    return address.Guid.Equals(guid) ? address : null;
            }

            return null;
        }

        public static ISAMObject SAMObject(this AdjacencyCluster adjacencyCluster, Type type, Guid guid)
        {
            if (adjacencyCluster == null || type == null)
                return null;

            if (typeof(Space).IsAssignableFrom(type))
                return adjacencyCluster.GetObject<Space>(guid);

            if (typeof(Aperture).IsAssignableFrom(type))
                return adjacencyCluster.GetApertures()?.Find(x => x != null && x.Guid.Equals(guid));

            if (typeof(ApertureConstruction).IsAssignableFrom(type))
                return adjacencyCluster.GetApertures()?.Find(x => x != null && x.SAMTypeGuid.Equals(guid))?.ApertureConstruction;

            if (typeof(Panel).IsAssignableFrom(type))
                return adjacencyCluster.GetObject<Panel>(guid);

            if (typeof(Construction).IsAssignableFrom(type))
                return adjacencyCluster.GetConstructions().Find(x => x != null && x.Guid.Equals(guid));

            if (typeof(InternalCondition).IsAssignableFrom(type))
            {
                List<Space> spaces = adjacencyCluster.GetSpaces();
                if (spaces != null)
                {
                    foreach (Space space in spaces)
                    {
                        InternalCondition internalCondition = space.InternalCondition;
                        if (internalCondition != null && internalCondition.Guid.Equals(guid))
                            return internalCondition;
                    }
                }

                return null;
            }

            return null;
        }

        public static ISAMObject SAMObject(this ProfileLibrary profileLibrary, Guid guid)
        {
            if (profileLibrary == null)
                return null;

            List<IJSAMObject> jSAMObjects = profileLibrary.GetObjects();
            if (jSAMObjects == null || jSAMObjects.Count == 0)
                return null;

            foreach (IJSAMObject jSAMObject in jSAMObjects)
            {
                if (jSAMObject == null)
                    continue;
                
                ISAMObject sAMObject = jSAMObject as ISAMObject;

                if (sAMObject != null && sAMObject.Guid.Equals(guid))
                    return sAMObject;
            }

            return null;
        }
    }
}