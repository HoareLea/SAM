// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One day's 24 hourly binary availability values - SAM's first-class counterpart to a TAS
    /// <c>TBD.schedule</c>.
    /// <para>
    /// <b>Why this is not a <see cref="Profile"/>.</b> TAS exposes two distinct concepts on an aperture
    /// control: a <c>TBD.profile</c> (type / function / factor / setbackValue) and the
    /// <c>TBD.schedule</c> it points at. SAM previously had no object for the second, so a
    /// <see cref="Profile"/> - a general, sparse, range-compressible curve of arbitrary doubles - was
    /// used as a stand-in for an availability mask. This object exists so that the schedule concept is
    /// stated rather than approximated: exactly 24 hours, exactly on/off, no ranges, no nesting.
    /// </para>
    /// <para>
    /// <b>Binary on purpose.</b> A <c>TBD.schedule</c>'s COM accessor is integer-typed
    /// (<c>set_values(int, int)</c>), but the mechanism it drives is binary: the schedule selects
    /// between the profile's own value/function (hour on) and its <c>setbackValue</c> (hour off).
    /// Nothing SAM ships writes a non-binary aperture schedule. A general-valued day curve read back
    /// from a user-authored TAS model is still representable - as the legacy
    /// <see cref="ProfileOpeningProperties.Profile"/>, not as a <c>DailyAvailabilitySchedule</c>.
    /// </para>
    /// <para>
    /// <b>Deliberately narrow.</b> There is no weekly, yearly or calendar schedule here, and no
    /// schedule library. This is one day pattern, which is the whole of what a <c>TBD.schedule</c> is.
    /// </para>
    /// <para>
    /// <b>Why not simply "DailySchedule".</b> <c>SAM.Analytical.Systems.DailySchedule</c> already exists
    /// and means something else - a named collection of <c>ScheduleDay</c>s, where <c>ScheduleDay</c> is
    /// its 24-value object. That assembly sits downstream of this one and cannot be referenced from here,
    /// so this is not a duplicate abstraction; but taking the same name for a different concept in a
    /// closely related API would invite exactly the confusion this type exists to remove. "Availability"
    /// is also what the object actually is - a binary selector, usable by a window, an internal door, a
    /// security restriction or an acoustic restriction alike.
    /// </para>
    /// </summary>
    public class DailyAvailabilitySchedule : SAMObject, IAnalyticalObject
    {
        /// <summary>
        /// The number of hourly values a <see cref="DailyAvailabilitySchedule"/> has - always exactly this many,
        /// matching the 24 indices <c>TBD.schedule</c> is read and written at.
        /// </summary>
        public const int HourCount = 24;

        /// <summary>
        /// Never null and always <see cref="HourCount"/> long. Private, and never handed out by
        /// reference - <see cref="GetValues"/> returns a copy - so a schedule cannot be mutated
        /// through a value another object is holding.
        /// </summary>
        private bool[] values = new bool[HourCount];

        /// <summary>
        /// An unnamed schedule that is unavailable for all 24 hours.
        /// </summary>
        public DailyAvailabilitySchedule()
            : base()
        {

        }

        /// <summary>
        /// A named schedule that is unavailable for all 24 hours.
        /// </summary>
        public DailyAvailabilitySchedule(string name)
            : base(name)
        {

        }

        /// <summary>
        /// A named schedule from exactly <see cref="HourCount"/> hourly values, hour 0 first.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="values"/> is null, or does not contain exactly <see cref="HourCount"/>
        /// values. A schedule with the wrong number of hours has no meaning against
        /// <c>TBD.schedule</c>'s fixed 24 indices, and silently padding or truncating one is how a
        /// schedule ends up written as 24 zeros - so this is refused at construction rather than
        /// tolerated.
        /// </exception>
        public DailyAvailabilitySchedule(string name, IEnumerable<bool> values)
            : base(name)
        {
            this.values = Values(values);
        }

        /// <summary>
        /// A copy. The copied schedule keeps the original's <see cref="SAMObject.Guid"/> and
        /// <see cref="SAMObject.Name"/> - normal SAM copy-constructor behaviour - and gets its own
        /// values array, so mutating one cannot affect the other.
        /// </summary>
        public DailyAvailabilitySchedule(DailyAvailabilitySchedule dailySchedule)
            : base(dailySchedule)
        {
            if (dailySchedule?.values != null)
            {
                values = dailySchedule.values.ToArray();
            }
        }

        /// <summary>
        /// The same 24 values under a different name. Used where a schedule must be created under a
        /// collision-safe name without its values changing.
        /// </summary>
        public DailyAvailabilitySchedule(string name, DailyAvailabilitySchedule dailySchedule)
            : base(name, dailySchedule)
        {
            if (dailySchedule?.values != null)
            {
                values = dailySchedule.values.ToArray();
            }
        }

        public DailyAvailabilitySchedule(JsonObject jsonObject)
            : base(jsonObject)
        {

        }

        /// <summary>
        /// The value for one hour of the day. <paramref name="hour"/> is 0-based, so 0 is 00:00-01:00
        /// and 23 is 23:00-24:00 - the same indexing <c>TBD.schedule</c> uses.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hour"/> is not 0-23.</exception>
        public bool this[int hour]
        {
            get
            {
                if (hour < 0 || hour >= HourCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(hour), string.Format("A DailyAvailabilitySchedule has hours 0-{0}; {1} was requested.", HourCount - 1, hour));
                }

                return values[hour];
            }
        }

        /// <summary>
        /// A copy of all <see cref="HourCount"/> values, hour 0 first. Never null. Mutating the
        /// returned array does not affect this schedule.
        /// </summary>
        public bool[] GetValues()
        {
            return values.ToArray();
        }

        /// <summary>
        /// A deterministic fingerprint of the 24 values, independent of name, guid and build: six
        /// uppercase hexadecimal digits of the 24-bit mask, hour 0 as the most significant bit. The
        /// default Part O availability window 08:00-23:00 is therefore <c>00FFFE</c>.
        /// <para>
        /// Two schedules have the same signature if and only if they have the same values, so it is
        /// safe to use as an identity in a generated name.
        /// </para>
        /// </summary>
        public string Signature
        {
            get
            {
                int mask = 0;
                for (int hour = 0; hour < HourCount; hour++)
                {
                    if (values[hour])
                    {
                        mask |= 1 << (HourCount - 1 - hour);
                    }
                }

                return mask.ToString("X6");
            }
        }

        /// <summary>
        /// The 24 values as a string of '0'/'1', hour 0 first - for diagnostics and refusal messages,
        /// where the actual pattern is what a reader needs to see.
        /// </summary>
        public string ValuesText
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(HourCount);
                for (int hour = 0; hour < HourCount; hour++)
                {
                    stringBuilder.Append(values[hour] ? '1' : '0');
                }

                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Whether this schedule has the same 24 values as <paramref name="dailySchedule"/>. Name and
        /// guid take no part: two schedules with the same values are behaviourally the same schedule,
        /// which is what makes TAS-side reuse safe.
        /// </summary>
        public bool ValuesEqual(DailyAvailabilitySchedule dailySchedule)
        {
            if (dailySchedule == null)
            {
                return false;
            }

            for (int hour = 0; hour < HourCount; hour++)
            {
                if (values[hour] != dailySchedule.values[hour])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether <paramref name="values"/> is a usable set of hourly values - exactly
        /// <see cref="HourCount"/> of them.
        /// </summary>
        public static bool IsValid(IEnumerable<bool> values)
        {
            return values != null && values.Count() == HourCount;
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            //A malformed or wrong-length "Values" array comes from a file, not from a caller, so it is
            //reported as a failed deserialization rather than thrown - but it is never quietly padded
            //to 24 hours, because that is indistinguishable from a schedule that really is all-zero.
            //Each element must be a genuine JSON boolean: GetValue<bool>() would THROW on a string or
            //number element, contradicting the failed-deserialization contract, and TryGetValue<bool>
            //rejects non-boolean/null elements because JsonValue only converts its own backing kind.
            if (!(jsonObject["Values"] is JsonArray jsonArray) || jsonArray.Count != HourCount)
            {
                return false;
            }

            bool[] values_Temp = new bool[HourCount];
            for (int hour = 0; hour < HourCount; hour++)
            {
                if (!(jsonArray[hour] is JsonValue jsonValue) || !jsonValue.TryGetValue<bool>(out bool value))
                {
                    return false;
                }

                values_Temp[hour] = value;
            }

            values = values_Temp;
            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject == null)
            {
                return null;
            }

            JsonArray jsonArray = new JsonArray();
            for (int hour = 0; hour < HourCount; hour++)
            {
                jsonArray.Add(values[hour]);
            }

            jsonObject["Values"] = jsonArray;

            return jsonObject;
        }

        private static bool[] Values(IEnumerable<bool> values)
        {
            if (!IsValid(values))
            {
                throw new ArgumentException(string.Format("A DailyAvailabilitySchedule needs exactly {0} hourly values; {1} were supplied.", HourCount, values == null ? "no values" : values.Count().ToString()), nameof(values));
            }

            return values.ToArray();
        }
    }
}
