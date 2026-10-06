// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    public class TM59Result : TMResult
    {
        private HashSet<TM59SpaceApplication> tM59SpaceApplications;

        private int occupiedHours;
        private int maxExceedableHours;

        private bool pass;

        public TM59Result(
            string name,
            string source,
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHours = occupiedHours;
            this.maxExceedableHours = maxExceedableHours;
            this.pass = pass;
            this.tM59SpaceApplications = tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
        }

        public TM59Result(
            Guid guid,
            string name,
            string source,
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(guid, name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHours = occupiedHours;
            this.maxExceedableHours = maxExceedableHours;
            this.pass = pass;
            this.tM59SpaceApplications = tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
        }

        /// <summary>
        /// A copy of this result.
        /// <para>
        /// Declared here rather than left to the base class's own copy constructor because constructors are
        /// NOT inherited: <c>Core.Query.Clone</c> reflects over <c>type.GetConstructors()</c>, which returns
        /// only this type's, so a subclass without one is uncloneable however many its base has. That
        /// mattered silently - <c>AdjacencyCluster.IsValid</c> accepts this type, and the deep-clone
        /// constructor <c>AnalyticalModel(AnalyticalModel, bool)</c> replaces each stored object with its
        /// clone, so a clone that came back null left the ORIGINAL instance in the supposedly owned cluster
        /// and the two models went on sharing it.
        /// </para>
        /// </summary>
        public TM59Result(TM59Result tM59Result)
            : base(tM59Result)
        {
            if (tM59Result != null)
            {
                occupiedHours = tM59Result.occupiedHours;
                maxExceedableHours = tM59Result.maxExceedableHours;
                pass = tM59Result.pass;
                tM59SpaceApplications = tM59Result.tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59Result.tM59SpaceApplications);
            }
        }

        public override int OccupiedHours
        {
            get
            {
                return occupiedHours;
            }
        }

        public override int MaxExceedableHours
        {
            get
            {
                return maxExceedableHours;
            }
        }

        public override bool Pass
        {
            get
            {
                return pass;
            }
        }

        public HashSet<TM59SpaceApplication> TM59SpaceApplications
        {
            get
            {
                return tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
            }
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject["TM59SpaceApplications"] is JsonArray tM59SpaceApplicationsArray)
            {
                tM59SpaceApplications = new HashSet<TM59SpaceApplication>();
                foreach (JsonNode node in tM59SpaceApplicationsArray)
                {
                    TM59SpaceApplication tM59SpaceApplication = Core.Query.Enum<TM59SpaceApplication>(node?.GetValue<string>());
                    tM59SpaceApplications.Add(tM59SpaceApplication);
                }
            }

            if (jsonObject.ContainsKey("OccupiedHours"))
            {
                occupiedHours = jsonObject["OccupiedHours"]?.GetValue<int>() ?? 0;
            }

            if (jsonObject.ContainsKey("MaxExceedableHours"))
            {
                maxExceedableHours = jsonObject["MaxExceedableHours"]?.GetValue<int>() ?? 0;
            }

            if (jsonObject.ContainsKey("Pass"))
            {
                pass = jsonObject["Pass"]?.GetValue<bool>() ?? false;
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result == null)
            {
                return null;
            }

            if (tM59SpaceApplications != null)
            {
                JsonArray tM59SpaceApplicationsArray = new JsonArray();
                foreach (TM59SpaceApplication tM59SpaceApplication in tM59SpaceApplications)
                {
                    tM59SpaceApplicationsArray.Add(tM59SpaceApplication.ToString());
                }

                result["TM59SpaceApplications"] = tM59SpaceApplicationsArray;
            }

            if (occupiedHours != int.MinValue)
            {
                result["OccupiedHours"] = occupiedHours;
            }

            if (maxExceedableHours != int.MinValue)
            {
                result["MaxExceedableHours"] = maxExceedableHours;
            }

            result["Pass"] = pass;

            return result;
        }
    }
}
