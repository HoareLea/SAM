// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    public class TM59CorridorResult : TM59Result
    {
        private int hoursExceeding28;

        //int.MinValue - the plain results' unset marker - where a caller built this without stating the
        //real annual series length, so a report reads it as absent rather than as a fabricated 0.
        private int annualHours = int.MinValue;

        public TM59CorridorResult(
            string name,
            string source,
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceeding28,
            bool pass,
            int annualHours = int.MinValue)
            : base(name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, pass, TM59SpaceApplication.Undefined)
        {
            this.hoursExceeding28 = hoursExceeding28;
            this.annualHours = annualHours;
        }

        public TM59CorridorResult(
            Guid guid,
            string name,
            string source,
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceeding28,
            bool pass,
            int annualHours = int.MinValue)
            : base(guid, name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, pass, TM59SpaceApplication.Undefined)
        {
            this.hoursExceeding28 = hoursExceeding28;
            this.annualHours = annualHours;
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
        public TM59CorridorResult(TM59CorridorResult tM59CorridorResult)
            : base(tM59CorridorResult)
        {
            if (tM59CorridorResult != null)
            {
                hoursExceeding28 = tM59CorridorResult.hoursExceeding28;
                annualHours = tM59CorridorResult.annualHours;
            }
        }

        public int HoursExceeding28
        {
            get
            {
                return hoursExceeding28;
            }
        }

        /// <summary>
        /// The number of hours in the annual series this check was evaluated over (typically 8760) - the
        /// real basis behind <c>MaxExceedableHours</c>, read directly off the calculation rather than
        /// reconstructed from it. <c>int.MinValue</c> where a caller never stated it.
        /// </summary>
        public int AnnualHours
        {
            get
            {
                return annualHours;
            }
        }


        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject.ContainsKey("HoursExceeding28"))
            {
                hoursExceeding28 = jsonObject["HoursExceeding28"]?.GetValue<int>() ?? 0;
            }

            if (jsonObject.ContainsKey("AnnualHours"))
            {
                annualHours = jsonObject["AnnualHours"]?.GetValue<int>() ?? int.MinValue;
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

            if (hoursExceeding28 != int.MinValue)
            {
                result["HoursExceeding28"] = hoursExceeding28;
            }

            if (annualHours != int.MinValue)
            {
                result["AnnualHours"] = annualHours;
            }

            return result;
        }
    }
}
