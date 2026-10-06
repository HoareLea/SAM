// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b><c>DailyAvailabilitySchedule</c> - SAM's first-class counterpart to a <c>TBD.schedule</c>.</b>
    /// <para>
    /// The object exists because SAM previously had no schedule concept and used a general
    /// <c>Profile</c> as a stand-in for an availability mask. These tests pin the properties the TAS
    /// transfer depends on: exactly 24 hours or nothing, values that cannot be mutated from outside,
    /// value equality that ignores name and guid (which is what makes TAS-side schedule REUSE safe),
    /// and a build-stable signature (which is what makes a generated schedule NAME deterministic).
    /// </para>
    /// </summary>
    public class DailyAvailabilityScheduleTests
    {
        private static bool[] Window(int from, int to)
        {
            bool[] values = new bool[24];
            for (int hour = 0; hour < 24; hour++)
            {
                values[hour] = from <= to ? (hour >= from && hour < to) : (hour >= from || hour < to);
            }

            return values;
        }

        // -------------------------------------------------------------------------------------------------
        // Exactly 24 values
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void HourCount_Is24()
        {
            Assert.Equal(24, DailyAvailabilitySchedule.HourCount);
        }

        [Fact]
        public void TwentyFourValues_AreAccepted()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Test", Window(8, 23));

            Assert.Equal(24, dailySchedule.GetValues().Length);
            Assert.Equal("Test", dailySchedule.Name);
        }

        /// <summary>
        /// A short array is refused at construction rather than padded. Silently padding is exactly how a
        /// schedule ends up written to a TBD as 24 zeros with nothing reporting it.
        /// </summary>
        [Fact]
        public void TooFewValues_AreRejected()
        {
            Assert.Throws<ArgumentException>(() => new DailyAvailabilitySchedule("Test", new bool[23]));
        }

        [Fact]
        public void TooManyValues_AreRejected()
        {
            Assert.Throws<ArgumentException>(() => new DailyAvailabilitySchedule("Test", new bool[25]));
        }

        [Fact]
        public void NullValues_AreRejected()
        {
            Assert.Throws<ArgumentException>(() => new DailyAvailabilitySchedule("Test", (bool[])null));
        }

        [Fact]
        public void IsValid_MatchesWhatTheConstructorAccepts()
        {
            Assert.True(DailyAvailabilitySchedule.IsValid(new bool[24]));
            Assert.False(DailyAvailabilitySchedule.IsValid(new bool[23]));
            Assert.False(DailyAvailabilitySchedule.IsValid(new bool[25]));
            Assert.False(DailyAvailabilitySchedule.IsValid(null));
        }

        /// <summary>The name-only constructor is a valid, all-unavailable day - not an invalid object.</summary>
        [Fact]
        public void NameOnlyConstructor_IsAllUnavailable()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Closed");

            Assert.Equal(24, dailySchedule.GetValues().Length);
            Assert.All(dailySchedule.GetValues(), value => Assert.False(value));
            Assert.Equal("000000", dailySchedule.Signature);
        }

        // -------------------------------------------------------------------------------------------------
        // Index access
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void Indexer_ReadsTheHourlyValue()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Test", Window(8, 23));

            Assert.False(dailySchedule[0]);
            Assert.False(dailySchedule[7]);
            Assert.True(dailySchedule[8]);
            Assert.True(dailySchedule[22]);
            Assert.False(dailySchedule[23]);
        }

        [Fact]
        public void Indexer_OutsideZeroToTwentyThree_Throws()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Test", Window(8, 23));

            Assert.Throws<ArgumentOutOfRangeException>(() => dailySchedule[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => dailySchedule[24]);
        }

        // -------------------------------------------------------------------------------------------------
        // Copies are independent
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The stored array is never handed out by reference. Mutating what GetValues returned must not be
        /// able to change the schedule - the failure mode this rules out is a schedule silently changing
        /// under a caller that only meant to read it.
        /// </summary>
        [Fact]
        public void GetValues_ReturnsACopy_NotTheInternalArray()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Test", Window(8, 23));

            bool[] values = dailySchedule.GetValues();
            values[0] = true;
            values[8] = false;

            Assert.False(dailySchedule[0]);
            Assert.True(dailySchedule[8]);
        }

        /// <summary>The source array a schedule was built from is likewise not adopted by reference.</summary>
        [Fact]
        public void Constructor_DoesNotAdoptTheSourceArray()
        {
            bool[] source = Window(8, 23);
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Test", source);

            source[8] = false;

            Assert.True(dailySchedule[8]);
        }

        [Fact]
        public void CopyConstructor_ProducesAnIndependentValueEqualSchedule()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("Test", Window(8, 23));

            DailyAvailabilitySchedule copy = new DailyAvailabilitySchedule(dailySchedule);

            Assert.True(dailySchedule.ValuesEqual(copy));
            Assert.Equal(dailySchedule.Name, copy.Name);
            Assert.Equal(dailySchedule.Guid, copy.Guid);

            bool[] values = copy.GetValues();
            values[8] = false;
            Assert.True(dailySchedule[8]);
            Assert.True(copy[8]);
        }

        [Fact]
        public void RenamingConstructor_KeepsTheValuesAndTakesTheNewName()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23));

            DailyAvailabilitySchedule renamed = new DailyAvailabilitySchedule("PartO_DayOpen_08_23_00FFFE", dailySchedule);

            Assert.Equal("PartO_DayOpen_08_23_00FFFE", renamed.Name);
            Assert.True(dailySchedule.ValuesEqual(renamed));
        }

        // -------------------------------------------------------------------------------------------------
        // Value comparison
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Name and guid take no part in value equality. This is the whole basis of TAS-side reuse: a Part O
        /// window, an internal door and a security-restricted opening naming the schedule three different
        /// things must still be able to share one TBD schedule.
        /// </summary>
        [Fact]
        public void ValuesEqual_IgnoresNameAndGuid()
        {
            DailyAvailabilitySchedule a = new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23));
            DailyAvailabilitySchedule b = new DailyAvailabilitySchedule("Internal door availability", Window(8, 23));

            Assert.NotEqual(a.Guid, b.Guid);
            Assert.NotEqual(a.Name, b.Name);
            Assert.True(a.ValuesEqual(b));
            Assert.True(b.ValuesEqual(a));
        }

        [Fact]
        public void ValuesEqual_OneDifferingHour_IsNotEqual()
        {
            DailyAvailabilitySchedule a = new DailyAvailabilitySchedule("A", Window(8, 23));

            for (int hour = 0; hour < 24; hour++)
            {
                bool[] values = Window(8, 23);
                values[hour] = !values[hour];

                DailyAvailabilitySchedule b = new DailyAvailabilitySchedule("A", values);

                Assert.False(a.ValuesEqual(b), string.Format("Flipping hour {0} must break value equality.", hour));
                Assert.NotEqual(a.Signature, b.Signature);
            }
        }

        [Fact]
        public void ValuesEqual_Null_IsNotEqual()
        {
            DailyAvailabilitySchedule a = new DailyAvailabilitySchedule("A", Window(8, 23));

            Assert.False(a.ValuesEqual(null));
        }

        // -------------------------------------------------------------------------------------------------
        // Signature and text
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The signature is a fixed 6-hex-digit mask with hour 0 as the most significant bit. It appears in
        /// generated and collision-resolved TAS schedule names, so it must be stable across builds - it is
        /// derived arithmetically, never from GetHashCode.
        /// </summary>
        [Fact]
        public void Signature_IsTheSixHexDigitMask_HourZeroMostSignificant()
        {
            Assert.Equal("000000", new DailyAvailabilitySchedule("None", new bool[24]).Signature);
            Assert.Equal("FFFFFF", new DailyAvailabilitySchedule("All", Enumerable.Repeat(true, 24).ToArray()).Signature);
            Assert.Equal("00FFFE", new DailyAvailabilitySchedule("PartO", Window(8, 23)).Signature);

            bool[] hourZeroOnly = new bool[24];
            hourZeroOnly[0] = true;
            Assert.Equal("800000", new DailyAvailabilitySchedule("Hour0", hourZeroOnly).Signature);

            bool[] hourTwentyThreeOnly = new bool[24];
            hourTwentyThreeOnly[23] = true;
            Assert.Equal("000001", new DailyAvailabilitySchedule("Hour23", hourTwentyThreeOnly).Signature);
        }

        [Fact]
        public void Signature_IsStableAcrossInstancesAndRepeatedReads()
        {
            DailyAvailabilitySchedule a = new DailyAvailabilitySchedule("A", Window(8, 23));
            DailyAvailabilitySchedule b = new DailyAvailabilitySchedule("B", Window(8, 23));

            Assert.Equal("00FFFE", a.Signature);
            Assert.Equal(a.Signature, a.Signature);
            Assert.Equal(a.Signature, b.Signature);
        }

        [Fact]
        public void ValuesText_IsTwentyFourBitsHourZeroFirst()
        {
            Assert.Equal("000000001111111111111110", new DailyAvailabilitySchedule("PartO", Window(8, 23)).ValuesText);
            Assert.Equal("111111000000000000111111", new DailyAvailabilitySchedule("Overnight", Window(18, 6)).ValuesText);
        }

        // -------------------------------------------------------------------------------------------------
        // JSON
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void JsonRoundTrip_PreservesNameGuidAndValues()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23));

            DailyAvailabilitySchedule reconstructed = RoundTrip.Once(dailySchedule);

            Assert.Equal(dailySchedule.Name, reconstructed.Name);
            Assert.Equal(dailySchedule.Guid, reconstructed.Guid);
            Assert.True(dailySchedule.ValuesEqual(reconstructed));
            Assert.Equal(dailySchedule.Signature, reconstructed.Signature);
        }

        [Fact]
        public void JsonRoundTrip_PreservesEveryHourPattern()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                bool[] values = new bool[24];
                values[hour] = true;

                DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule(string.Format("Hour{0}", hour), values);
                DailyAvailabilitySchedule reconstructed = RoundTrip.Once(dailySchedule);

                Assert.True(dailySchedule.ValuesEqual(reconstructed));
            }
        }

        /// <summary>
        /// A wrong-length "Values" array comes from a file, not from a caller, so it fails deserialization
        /// rather than throwing - but it is never padded to 24 hours, because a padded schedule is
        /// indistinguishable from one that really is all-zero.
        /// </summary>
        [Fact]
        public void MalformedJson_WithWrongValueCount_FailsDeserialisationRatherThanPadding()
        {
            string json = @"{
                ""_type"": ""SAM.Analytical.DailyAvailabilitySchedule"",
                ""Name"": ""Broken"",
                ""Guid"": ""8b5b9b8a-0d21-4a3f-9c3a-1f2e3d4c5b6a"",
                ""Values"": [false, false, true]
            }";

            DailyAvailabilitySchedule dailySchedule = SAM.Core.Create.IJSAMObject<DailyAvailabilitySchedule>(json);

            //Deserialization reported failure; nothing pretends the three supplied hours are a valid day.
            Assert.All(dailySchedule.GetValues(), value => Assert.False(value));
            Assert.Equal(24, dailySchedule.GetValues().Length);
        }

        [Fact]
        public void JsonHasExactlyTwentyFourValues()
        {
            DailyAvailabilitySchedule dailySchedule = new DailyAvailabilitySchedule("PartO_DayOpen_08_23", Window(8, 23));

            System.Text.Json.Nodes.JsonObject jsonObject = dailySchedule.ToJsonObject();

            Assert.Equal(24, (jsonObject["Values"] as System.Text.Json.Nodes.JsonArray).Count);
        }

        /// <summary>
        /// A 24-element "Values" array whose elements are not all genuine JSON booleans is malformed file
        /// data, exactly like a wrong-length array. <c>GetValue&lt;bool&gt;()</c> would THROW on a string or
        /// number element - contradicting the documented failed-deserialization contract and taking down
        /// whatever load contained the schedule - so each element must fail deserialization cleanly instead,
        /// with nothing adopted as values.
        /// </summary>
        [Theory]
        [InlineData("\"true\"")]
        [InlineData("\"on\"")]
        [InlineData("1")]
        [InlineData("0.0")]
        [InlineData("null")]
        public void MalformedJson_WithNonBooleanValueElement_FailsDeserialisationRatherThanThrowing(string json_Element)
        {
            string[] elements = new string[24];
            for (int i = 0; i < 24; i++)
            {
                elements[i] = "false";
            }
            elements[13] = json_Element;

            string json = string.Format(@"{{ ""_type"": ""SAM.Analytical.DailyAvailabilitySchedule"", ""Name"": ""Broken"", ""Values"": [ {0} ] }}", string.Join(", ", elements));

            DailyAvailabilitySchedule dailySchedule = SAM.Core.Create.IJSAMObject<DailyAvailabilitySchedule>(json);

            //Deserialization reported failure; no element - including the eleven preceding the malformed one -
            //is adopted, so nothing half-read pretends to be a valid day.
            Assert.All(dailySchedule.GetValues(), value => Assert.False(value));
        }
    }
}
