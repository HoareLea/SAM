// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b><c>ProfileOpeningProperties</c> carries two different things, and old models must not change
    /// meaning.</b>
    /// <para>
    /// <c>Schedule</c> is the first-class 24-hour binary availability schedule
    /// (<c>DailyAvailabilitySchedule</c>). <c>Profile</c> is the legacy general-valued day profile that
    /// stood in for one before that type existed, and is still what <c>profiles_</c> and
    /// <c>Create.AnalyticalModel</c> construct. Precedence on export is <c>Schedule</c> first, legacy
    /// <c>Profile</c> only when <c>Schedule</c> is null - so a model saved before <c>Schedule</c> existed
    /// behaves exactly as it did.
    /// </para>
    /// </summary>
    public class ProfileOpeningPropertiesTests
    {
        private static bool[] Window(int from, int to)
        {
            bool[] values = new bool[24];
            for (int hour = 0; hour < 24; hour++)
            {
                values[hour] = hour >= from && hour < to;
            }

            return values;
        }

        private static double[] DoubleWindow(int from, int to)
        {
            double[] values = new double[24];
            for (int hour = 0; hour < 24; hour++)
            {
                values[hour] = (hour >= from && hour < to) ? 1 : 0;
            }

            return values;
        }

        // -------------------------------------------------------------------------------------------------
        // Schedule
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void Default_HasNeitherProfileNorSchedule()
        {
            ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(0.6);

            Assert.Null(profileOpeningProperties.Profile);
            Assert.Null(profileOpeningProperties.Schedule);
        }

        [Fact]
        public void ScheduleConstructor_StoresTheSchedule()
        {
            DailyAvailabilitySchedule schedule = new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23));

            ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(0.6, schedule);

            Assert.Null(profileOpeningProperties.Profile);
            Assert.NotNull(profileOpeningProperties.Schedule);
            Assert.Equal("PartO_DayOpen_08_23", profileOpeningProperties.Schedule.Name);
            Assert.True(schedule.ValuesEqual(profileOpeningProperties.Schedule));
        }

        /// <summary>
        /// The getter must hand out a copy, or a caller could mutate an opening's stored schedule by
        /// mutating what it only meant to read.
        /// </summary>
        [Fact]
        public void ScheduleGetter_ReturnsACopy()
        {
            ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(0.6, new DailyAvailabilitySchedule("S", Window(8, 23)));

            bool[] values = profileOpeningProperties.Schedule.GetValues();
            values[8] = false;

            Assert.True(profileOpeningProperties.Schedule[8]);
        }

        [Fact]
        public void CopyConstructor_CopiesScheduleAndProfileIndependently()
        {
            ProfileOpeningProperties original = new ProfileOpeningProperties(
                0.6,
                new Profile("Legacy", ProfileGroup.Ventilation, DoubleWindow(9, 21)),
                new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23)));

            ProfileOpeningProperties copy = new ProfileOpeningProperties(original);

            Assert.NotNull(copy.Schedule);
            Assert.NotNull(copy.Profile);
            Assert.True(original.Schedule.ValuesEqual(copy.Schedule));
            Assert.Equal(original.Profile.Name, copy.Profile.Name);

            //Distinct instances, not shared references.
            Assert.NotSame(original.Schedule, copy.Schedule);
            Assert.NotSame(original.Profile, copy.Profile);
        }

        [Fact]
        public void ConversionConstructor_CarriesScheduleAndProfileAcross()
        {
            ProfileOpeningProperties original = new ProfileOpeningProperties(
                0.6,
                new Profile("Legacy", ProfileGroup.Ventilation, DoubleWindow(9, 21)),
                new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23)));

            ProfileOpeningProperties converted = new ProfileOpeningProperties(original, 0.55);

            Assert.Equal(0.55, converted.GetDischargeCoefficient());
            Assert.True(original.Schedule.ValuesEqual(converted.Schedule));
            Assert.Equal("Legacy", converted.Profile.Name);
        }

        // -------------------------------------------------------------------------------------------------
        // JSON
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void JsonRoundTrip_PreservesSchedule()
        {
            ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(0.6, new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23)))
            {
                Factor = 0.75
            };

            ProfileOpeningProperties reconstructed = RoundTrip.Once(profileOpeningProperties);

            Assert.NotNull(reconstructed.Schedule);
            Assert.Equal("PartO_DayOpen_08_23", reconstructed.Schedule.Name);
            Assert.Equal(profileOpeningProperties.Schedule.Guid, reconstructed.Schedule.Guid);
            Assert.True(profileOpeningProperties.Schedule.ValuesEqual(reconstructed.Schedule));
            Assert.Equal(0.75, reconstructed.Factor);
            Assert.Equal(0.6, reconstructed.GetDischargeCoefficient());
        }

        [Fact]
        public void JsonRoundTrip_PreservesBothCarriersSideBySide()
        {
            ProfileOpeningProperties profileOpeningProperties = new ProfileOpeningProperties(
                0.6,
                new Profile("Legacy", ProfileGroup.Ventilation, DoubleWindow(9, 21)),
                new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23)));

            ProfileOpeningProperties reconstructed = RoundTrip.Once(profileOpeningProperties);

            Assert.NotNull(reconstructed.Profile);
            Assert.NotNull(reconstructed.Schedule);
            Assert.Equal("Legacy", reconstructed.Profile.Name);
            Assert.Equal("PartO_DayOpen_08_23", reconstructed.Schedule.Name);
        }

        /// <summary>
        /// The compatibility case that matters most: a <c>ProfileOpeningProperties</c> serialised before
        /// <c>DailyAvailabilitySchedule</c> existed carries a <c>"Profile"</c> key and no <c>"Schedule"</c>
        /// key. It must deserialise with the legacy profile intact and no schedule invented for it.
        /// </summary>
        [Fact]
        public void LegacyJson_WithProfileAndNoScheduleKey_KeepsProfileAndHasNoSchedule()
        {
            ProfileOpeningProperties legacy = new ProfileOpeningProperties(0.62, new Profile("Legacy_Availability", ProfileGroup.Ventilation, DoubleWindow(8, 23)));

            string json = SAM.Core.Convert.ToString(legacy);

            Assert.DoesNotContain("\"Schedule\"", json);

            ProfileOpeningProperties reconstructed = SAM.Core.Create.IJSAMObject<ProfileOpeningProperties>(json);

            Assert.NotNull(reconstructed);
            Assert.Null(reconstructed.Schedule);
            Assert.NotNull(reconstructed.Profile);
            Assert.Equal("Legacy_Availability", reconstructed.Profile.Name);

            double[] values = reconstructed.Profile.GetDailyValues();
            Assert.Equal(24, values.Length);
            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal((hour >= 8 && hour < 23) ? 1 : 0, values[hour]);
            }
        }

        /// <summary>
        /// Hand-written legacy JSON, exactly as an older SAM release would have written it - no
        /// <c>"Schedule"</c> key at all, and it must not throw or fail deserialization.
        /// </summary>
        [Fact]
        public void HandWrittenLegacyJson_WithNoScheduleKey_DeserialisesWithNoSchedule()
        {
            string legacyJson = @"{
                ""_type"": ""SAM.Analytical.ProfileOpeningProperties"",
                ""DischargeCoefficient"": 0.62,
                ""Factor"": 1.0
            }";

            ProfileOpeningProperties reconstructed = SAM.Core.Create.IJSAMObject<ProfileOpeningProperties>(legacyJson);

            Assert.NotNull(reconstructed);
            Assert.Null(reconstructed.Schedule);
            Assert.Null(reconstructed.Profile);
            Assert.Equal(0.62, reconstructed.GetDischargeCoefficient());
        }
    }
}
