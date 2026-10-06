// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// Scheduled Opening Properties
    /// <para>
    /// <b>Two carriers, two different roles - not two ways of saying the same thing.</b>
    /// </para>
    /// <para>
    /// <see cref="Schedule"/> is a <see cref="DailyAvailabilitySchedule"/>: 24 hourly binary availability values,
    /// SAM's first-class counterpart to a <c>TBD.schedule</c>. This is what new code should state, and
    /// what the TAS aperture-control write uses.
    /// </para>
    /// <para>
    /// <see cref="Profile"/> is the legacy carrier and predates <see cref="DailyAvailabilitySchedule"/> existing.
    /// It is a general <see cref="Analytical.Profile"/> - arbitrary doubles, sparse, range-compressible -
    /// and was used as a stand-in for schedule data because there was nothing else. It stays, because
    /// it is persisted under the JSON key <c>"Profile"</c> in saved models, is what
    /// <c>SAMAnalytical.AddOpeningProperties</c> / <c>AddOpeningPropertiesByPartO</c>'s
    /// <c>profiles_</c> input and <c>Create.AnalyticalModel</c> construct, and is read by
    /// <c>Query.SingleOpeningProperties</c>. It is also the honest place to keep a general-valued day
    /// curve read back from a user-authored TAS model, which a binary <see cref="DailyAvailabilitySchedule"/>
    /// could not represent without changing it.
    /// </para>
    /// <para>
    /// <b>Precedence, where both are present:</b> <see cref="Schedule"/> wins. It is the explicit,
    /// first-class statement; <see cref="Profile"/> is consulted only when <see cref="Schedule"/> is
    /// null, which is exactly the behaviour every model saved before <see cref="Schedule"/> existed
    /// gets. They are not refused as a contradictory pair: a TBD -> SAM read legitimately produces
    /// both from one TAS schedule, so that a re-export reuses the same schedule while the general
    /// values remain recorded.
    /// </para>
    /// </summary>
    public class ProfileOpeningProperties : OpeningProperties
    {
        private Profile profile;
        private DailyAvailabilitySchedule schedule;

        public ProfileOpeningProperties()
        {

        }

        public ProfileOpeningProperties(double dischargeCoefficient)
            : base(dischargeCoefficient)
        {

        }
        public ProfileOpeningProperties(System.Text.Json.Nodes.JsonObject jsonObject)

            : base(jsonObject)

        {

        }

        public ProfileOpeningProperties(double dischargeCoefficient, Profile profile)
            : base(dischargeCoefficient)
        {
            this.profile = profile == null ? null : new Profile(profile);
        }

        /// <summary>
        /// Opening properties stated by a first-class availability <see cref="DailyAvailabilitySchedule"/>.
        /// </summary>
        public ProfileOpeningProperties(double dischargeCoefficient, DailyAvailabilitySchedule schedule)
            : base(dischargeCoefficient)
        {
            this.schedule = schedule == null ? null : new DailyAvailabilitySchedule(schedule);
        }

        /// <summary>
        /// Opening properties carrying both roles - see the type's own remarks for what each means and
        /// which one governs.
        /// </summary>
        public ProfileOpeningProperties(double dischargeCoefficient, Profile profile, DailyAvailabilitySchedule schedule)
            : base(dischargeCoefficient)
        {
            this.profile = profile == null ? null : new Profile(profile);
            this.schedule = schedule == null ? null : new DailyAvailabilitySchedule(schedule);
        }

        public ProfileOpeningProperties(ProfileOpeningProperties profileOpeningProperties)
            : base(profileOpeningProperties)
        {
            profile = profileOpeningProperties.profile == null ? null : new Profile(profileOpeningProperties.profile);
            schedule = profileOpeningProperties.schedule == null ? null : new DailyAvailabilitySchedule(profileOpeningProperties.schedule);
        }

        public ProfileOpeningProperties(IOpeningProperties openingProperties, double dischargeCoefficient)
            : base(openingProperties, dischargeCoefficient)
        {
            if (openingProperties is ProfileOpeningProperties)
            {
                profile = ((ProfileOpeningProperties)openingProperties).profile == null ? null : new Profile(((ProfileOpeningProperties)openingProperties).profile);
                schedule = ((ProfileOpeningProperties)openingProperties).schedule == null ? null : new DailyAvailabilitySchedule(((ProfileOpeningProperties)openingProperties).schedule);
            }
        }

        /// <summary>
        /// The legacy general-valued day profile, or null. See the type's own remarks: this is not the
        /// availability schedule, it is what stood in for one before <see cref="DailyAvailabilitySchedule"/>
        /// existed, and it still governs when <see cref="Schedule"/> is null.
        /// </summary>
        public Profile Profile
        {
            get
            {
                return profile == null ? null : new Profile(profile);
            }
        }

        /// <summary>
        /// The 24-hour binary availability schedule, or null when this opening states none. A copy -
        /// the stored schedule cannot be mutated through the returned value.
        /// </summary>
        public DailyAvailabilitySchedule Schedule
        {
            get
            {
                return schedule == null ? null : new DailyAvailabilitySchedule(schedule);
            }
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject["Profile"] is JsonObject profileJson)
            {
                profile = Core.Query.IJSAMObject<Profile>(profileJson as JsonObject);
            }

            //Absent on any ProfileOpeningProperties serialised before DailyAvailabilitySchedule existed - such a
            //model keeps behaving exactly as it did, through Profile above.
            if (jsonObject["Schedule"] is JsonObject scheduleJson)
            {
                schedule = Core.Query.IJSAMObject<DailyAvailabilitySchedule>(scheduleJson as JsonObject);
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject == null)
            {
                return null;
            }

            if (profile?.ToJsonObject() is JsonObject profileJson)
            {
                jsonObject["Profile"] = profileJson.DeepClone();
            }

            if (schedule?.ToJsonObject() is JsonObject scheduleJson)
            {
                jsonObject["Schedule"] = scheduleJson.DeepClone();
            }

            return jsonObject;
        }
    }
}
