using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class TM59Manager : IJSAMObject
    {
        private TextMap textMap;
        
        public TM59Manager(TextMap textMap)
        { 
            this.textMap = textMap == null ? null : Core.Create.TextMap(textMap);
        }

        public TM59Manager()
        {
            this.textMap = Query.DefaultInternalConditionTextMap_TM59();
        }

        public TM59Manager(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("TextMap"))
            {
                textMap = Core.Create.TextMap(jObject.Value<JObject>("TextMap"));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            if (jObject == null)
            {
                return null;
            }

            if (textMap != null)
            {
                jObject.Add("TextMap", textMap.ToJObject());
            }

            return jObject;
        }

        public bool IsSleeping(Space space)
        {
            return IsSleeping(space, textMap);
        }

        public bool IsSleeping(InternalCondition internalCondition)
        {
            return IsSleeping(internalCondition, textMap);
        }

        public bool IsLiving(Space space)
        {
            return IsLiving(space, textMap);
        }

        public bool IsLiving(InternalCondition internalCondition)
        {
            return IsLiving(internalCondition, textMap);
        }

        public bool IsCooking(Space space)
        {
            return IsCooking(space, textMap);
        }

        public bool IsCooking(InternalCondition internalCondition)
        {
            return IsCooking(internalCondition, textMap);
        }

        public List<TM59SpaceApplication> TM59SpaceApplications(Space space)
        {
            return TM59SpaceApplications(space, textMap);
        }

        public int Occupancy(Space space)
        {
            return Count(space?.Name, textMap);
        }

        public int Occupancy(InternalCondition internalCondition)
        {
            return Count(internalCondition?.Name, textMap);
        }

        public InternalCondition GetInternalCondition(AdjacencyCluster adjacencyCluster, InternalConditionLibrary internalConditionLibrary, Space space, string zoneType)
        {
            Zone zone = adjacencyCluster?.GetZones(space, zoneType)?.FirstOrDefault();
            if (zone == null)
            {
                return null;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces(zone);
            if (spaces == null || spaces.Count == 0)
            {
                return null;
            }

            List<TM59SpaceApplication> applications = TM59SpaceApplications(space, textMap);
            if (spaces.Count == 1 || (applications.Contains(TM59SpaceApplication.Sleeping) && applications.Contains(TM59SpaceApplication.Cooking) && applications.Contains(TM59SpaceApplication.Living)))
            {
                return internalConditionLibrary.GetInternalConditions("Studio").FirstOrDefault();
            }

            int count = 0;

            if (applications.Contains(TM59SpaceApplication.Sleeping))
            {
                count = Count(space?.Name, textMap);
                count = count == 0 ? 1 : count;

                switch (count)
                {
                    case 1:
                        return internalConditionLibrary.GetInternalConditions("Single Bedroom").FirstOrDefault();

                    case 2:
                        return internalConditionLibrary.GetInternalConditions("Double Bedroom").FirstOrDefault();
                }

                return null;
            }

            List<Space> spaces_Sleeping = spaces.FindAll(y => IsSleeping(y, textMap));

            //count = spaces_Sleeping.ConvertAll(y => Count(y?.Name, textMap)).Sum();
            count = spaces_Sleeping.Count();
            count = count == 0 ? 1 : count;
            count = count > 3 ? 3 : count;

            string name = null;
            if (applications.Contains(TM59SpaceApplication.Cooking) && applications.Contains(TM59SpaceApplication.Living))
            {
                name = "Bed Apt. Living Room/Kitchen";
            }
            else if (applications.Contains(TM59SpaceApplication.Cooking))
            {
                name = "Bed Apt. Kitchen";
            }
            else if (applications.Contains(TM59SpaceApplication.Living))
            {
                name = "Bed Apt. Living Room";
            }
            else
            {
                return null;
            }

            name = string.Format("{0} {1}", count, name);
            return internalConditionLibrary.GetInternalConditions(name).FirstOrDefault();
        }

        public static bool IsSleeping(string name, TextMap textMap)
        {
            return Is(name, textMap, "Sleeping");
        }

        public static bool IsSleeping(Space space, TextMap textMap)
        {
            return IsSleeping(space?.Name, textMap);
        }

        public static bool IsSleeping(InternalCondition internalCondition, TextMap textMap)
        {
            return IsSleeping(internalCondition?.Name, textMap);
        }

        public static bool IsLiving(Space space, TextMap textMap)
        {
            return IsLiving(space?.Name, textMap);
        }

        public static bool IsLiving(InternalCondition internalCondition, TextMap textMap)
        {
            return IsLiving(internalCondition?.Name, textMap);
        }

        public static bool IsLiving(string name, TextMap textMap)
        {
            return Is(name, textMap, "Living");
        }

        public static bool IsCooking(Space space, TextMap textMap)
        {
            return IsCooking(space?.Name, textMap);
        }

        public static bool IsCooking(InternalCondition internalCondition, TextMap textMap)
        {
            return IsCooking(internalCondition?.Name, textMap);
        }

        public static bool IsCooking(string name, TextMap textMap)
        {
            return Is(name, textMap, "Cooking");
        }

        public static List<TM59SpaceApplication> TM59SpaceApplications(Space space, TextMap textMap)
        {
            if (space == null || textMap == null)
            {
                return null;
            }

            List<TM59SpaceApplication> result = new List<TM59SpaceApplication>();
            if (IsSleeping(space, textMap))
            {
                result.Add(TM59SpaceApplication.Sleeping);
            }

            if (IsLiving(space, textMap))
            {
                result.Add(TM59SpaceApplication.Living);
            }

            if (IsCooking(space, textMap))
            {
                result.Add(TM59SpaceApplication.Cooking);
            }

            return result;

        }

        private static int Count(string name, TextMap textMap)
        {
            if (string.IsNullOrWhiteSpace(name) || textMap == null)
            {
                return 0;
            }

            HashSet<string> values = textMap.GetSortedKeys(name);
            foreach (string value in values)
            {
                if (Core.Query.TryConvert<int>(value, out int result))
                {
                    return result;
                }
            }

            return 0;
        }

        private static bool Is(string name, TextMap textMap, string key)
        {
            if (string.IsNullOrWhiteSpace(name) || textMap == null || string.IsNullOrEmpty(key))
            {
                return false;
            }

            HashSet<string> values = textMap.GetSortedKeys(name);
            return values != null && values.Contains(key);
        }

    }
}
