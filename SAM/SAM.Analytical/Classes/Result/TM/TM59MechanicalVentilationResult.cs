// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    public class TM59MechanicalVentilationResult : TM59Result
    {
        private int hoursExceeding26;

        public TM59MechanicalVentilationResult(
            string name,
            string source,
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceeding26,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceeding26 = hoursExceeding26;
        }

        public TM59MechanicalVentilationResult(
            Guid guid,
            string name,
            string source,
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceeding26,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(guid, name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceeding26 = hoursExceeding26;
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
        public TM59MechanicalVentilationResult(TM59MechanicalVentilationResult tM59MechanicalVentilationResult)
            : base(tM59MechanicalVentilationResult)
        {
            if (tM59MechanicalVentilationResult != null)
            {
                hoursExceeding26 = tM59MechanicalVentilationResult.hoursExceeding26;
            }
        }

        public int HoursExceeding26
        {
            get
            {
                return hoursExceeding26;
            }
        }


        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject.ContainsKey("HoursExceeding26"))
            {
                hoursExceeding26 = jsonObject["HoursExceeding26"]?.GetValue<int>() ?? 0;
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

            if (hoursExceeding26 != int.MinValue)
            {
                result["HoursExceeding26"] = hoursExceeding26;
            }

            return result;
        }
    }
}
