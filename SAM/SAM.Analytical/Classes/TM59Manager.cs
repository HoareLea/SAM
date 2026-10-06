// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SAM.Analytical
{
    public class TM59Manager : IJSAMObject
    {
        private TextMap textMap;
        private TM59InternalConditionResolver resolver;
        private InternalConditionLibrary resolverLibrary;

        public TM59Manager(TextMap textMap)
        {
            this.textMap = textMap == null ? Query.DefaultInternalConditionTextMap_TM59() : Core.Create.TextMap(textMap);
        }

        public TM59Manager()
        {
            this.textMap = Query.DefaultInternalConditionTextMap_TM59();
        }
        public TM59Manager(System.Text.Json.Nodes.JsonObject jsonObject)

        {

            FromJsonObject(jsonObject);

        }
        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
            {
                return false;
            }

            if (jsonObject["TextMap"] is JsonObject textMapJson)
            {
                textMap = Core.Create.TextMap((JsonObject)textMapJson.DeepClone());
            }

            return true;
        }
        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (textMap?.ToJsonObject() is JsonObject textMapJson)
            {
                jsonObject["TextMap"] = textMapJson.DeepClone();
            }

            return jsonObject;
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
        public List<TM59SpaceApplication> TM59SpaceApplications(InternalCondition internalCondition)
        {
            return TM59SpaceApplications(internalCondition, textMap);
        }

        public int Occupancy(Space space)
        {
            return Count(space?.Name, textMap);
        }

        public int Occupancy(InternalCondition internalCondition)
        {
            return Count(internalCondition?.Name, textMap);
        }

        /// <summary>
        /// Builds (and caches) a TM59InternalConditionResolver for the given library, sharing this
        /// manager's TextMap. The resolver is deterministic and zone-independent for non-habitable
        /// spaces (corridors, bathrooms, risers, ...) - see GetInternalConditionResult.
        /// </summary>
        public TM59InternalConditionResolver Resolver(InternalConditionLibrary internalConditionLibrary)
        {
            if (resolver == null || !ReferenceEquals(resolverLibrary, internalConditionLibrary))
            {
                resolver = new TM59InternalConditionResolver(textMap, internalConditionLibrary);
                resolverLibrary = internalConditionLibrary;
            }

            return resolver;
        }

        /// <summary>
        /// Resolves a Space to a TM59 InternalCondition and returns the full result, including
        /// classification, occupancy and a Diagnostic explaining any manual-review outcome.
        /// </summary>
        public TM59InternalConditionResult GetInternalConditionResult(AdjacencyCluster adjacencyCluster, InternalConditionLibrary internalConditionLibrary, Space space, string zoneType)
        {
            return Resolver(internalConditionLibrary).Resolve(adjacencyCluster, space, zoneType);
        }

        /// <summary>Occupancy (people) for a resolved TM59 InternalConditionResult, per the TM59 occupancy convention.</summary>
        public static int TM59Occupancy(TM59InternalConditionResult result)
        {
            return result?.Occupancy ?? 0;
        }

        private static readonly Dictionary<string, int> tm59OccupancyByConditionName = new Dictionary<string, int>
        {
            ["Studio"] = 2,
            ["1 Bed Apt. Living Room/Kitchen"] = 2,
            ["1 Bed Apt. Living Room"] = 2,
            ["1 Bed Apt. Kitchen"] = 2,
            ["2 Bed Apt. Living Room/Kitchen"] = 3,
            ["2 Bed Apt. Living Room"] = 3,
            ["2 Bed Apt. Kitchen"] = 3,
            ["3 Bed Apt. Living Room/Kitchen"] = 4,
            ["3 Bed Apt. Living Room"] = 4,
            ["3 Bed Apt. Kitchen"] = 4,
            ["Double Bedroom"] = 2,
            ["Single Bedroom"] = 1,
        };

        /// <summary>
        /// Occupancy (people) for a TM59 InternalCondition, looked up by its name against the
        /// documented TM59 occupancy table - independent of any live resolver/flat context, for
        /// callers (e.g. the UI, after the dialog has closed) that only have the final condition.
        /// Returns 0 for non-habitable conditions or any name not in the table.
        /// </summary>
        public static int TM59Occupancy(InternalCondition internalCondition)
        {
            if (internalCondition?.Name == null)
            {
                return 0;
            }

            return tm59OccupancyByConditionName.TryGetValue(internalCondition.Name, out int occupancy) ? occupancy : 0;
        }

        public InternalCondition GetInternalCondition(AdjacencyCluster adjacencyCluster, InternalConditionLibrary internalConditionLibrary, Space space, string zoneType)
        {
            return GetInternalConditionResult(adjacencyCluster, internalConditionLibrary, space, zoneType)?.InternalCondition;
        }

        public static bool IsSleeping(string name, TextMap textMap)
        {
            return Is(name, textMap, "Sleeping");
        }

        public static bool IsSleeping(Space space, TextMap textMap)
        {
            return IsSleeping(space?.Name, textMap);
        }

        /// <summary>
        /// The apartment-size qualifier SAM's own <c>TM59InternalConditionResolver</c> prefixes onto a
        /// multi-bedroom apartment's Kitchen/Living Room condition name - e.g. <c>"1 Bed Apt. Kitchen"</c>.
        /// It states how many bedrooms the APARTMENT has, not what this room is used for, and "Bed" is
        /// coincidentally also a literal Sleeping keyword - see
        /// <see cref="TM59SpaceApplicationClassificationTests"/> in <c>SAM.Tests</c> for the traced defect
        /// this strips.
        /// </summary>
        private static readonly Regex ApartmentBedroomCountPrefix = new(@"^\s*\d+\s+Bed\s+Apt\.?\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The name used to match Sleeping/Living/Cooking role keywords from an InternalCondition - its own
        /// name with a leading apartment bedroom-count qualifier removed, where present. Space-name
        /// classification (<see cref="IsSleeping(Space, TextMap)"/> and its Living/Cooking siblings) is
        /// untouched: this normalisation applies only to the InternalCondition-based overloads, since the
        /// apartment-size prefix is exclusively an InternalCondition naming convention, never a Space name.
        /// </summary>
        private static string RoleMatchName(InternalCondition internalCondition)
        {
            string name = internalCondition?.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return ApartmentBedroomCountPrefix.Replace(name, string.Empty);
        }

        /// <summary>
        /// Whole-token InternalCondition role matching, reusing
        /// <see cref="Query.TM59TextMapMatches(TextMap, string, IEnumerable{string})"/> - the same matcher
        /// <see cref="TM59InternalConditionResolver"/> uses for Space classification - instead of
        /// <see cref="TextMap.GetSortedKeys(string, bool)"/>'s substring search. <c>GetSortedKeys</c> does
        /// <c>value.Contains(token) || token.Contains(value)</c>, so the "Sleeping" alias "bedroom" matches
        /// the bare token "room" (e.g. in "N Bed Apt. Living Room") even though "room" is not "bedroom".
        /// <c>TM59TextMapMatches</c> requires an exact, contiguous, whole-token sequence match instead, so
        /// "room" never matches the alias "bedroom". Scoped to InternalCondition classification only -
        /// <see cref="Is(string, TextMap, string)"/> (Space-name classification) is untouched.
        /// </summary>
        private static bool IsRole(string name, TextMap textMap, string key)
        {
            if (string.IsNullOrWhiteSpace(name) || textMap == null || string.IsNullOrEmpty(key))
            {
                return false;
            }

            return textMap.TM59TextMapMatches(name, [key]).Count > 0;
        }

        public static bool IsSleeping(InternalCondition internalCondition, TextMap textMap)
        {
            return IsRole(RoleMatchName(internalCondition), textMap, "Sleeping");
        }

        public static bool IsLiving(Space space, TextMap textMap)
        {
            return IsLiving(space?.Name, textMap);
        }

        public static bool IsLiving(InternalCondition internalCondition, TextMap textMap)
        {
            return IsRole(RoleMatchName(internalCondition), textMap, "Living");
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
            return IsRole(RoleMatchName(internalCondition), textMap, "Cooking");
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

        public static List<TM59SpaceApplication> TM59SpaceApplications(InternalCondition internalCondition, TextMap textMap)
        {
            if (internalCondition == null || textMap == null)
            {
                return null;
            }

            List<TM59SpaceApplication> result = new List<TM59SpaceApplication>();
            if (IsSleeping(internalCondition, textMap))
            {
                result.Add(TM59SpaceApplication.Sleeping);
            }

            if (IsLiving(internalCondition, textMap))
            {
                result.Add(TM59SpaceApplication.Living);
            }

            if (IsCooking(internalCondition, textMap))
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
                if (Core.Query.TryConvert(value, out int result))
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
