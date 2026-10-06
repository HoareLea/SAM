// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b><c>Profile.GetDailyValues()</c> - pinned because a TAS-side write used to depend on it silently.</b>
    /// <para>
    /// <c>Modify.SetApertureType</c> (SAM_Tas) wrote a TBD schedule only when
    /// <c>GetDailyValues().Length == 24</c>, and did nothing - after already creating and naming the TBD
    /// schedule - when that was false. A named schedule holding 24 zeros was the visible result. These
    /// tests establish what <c>GetDailyValues()</c> actually returns, so that the failure mode is
    /// attributed to the right place: it is the caller's silent guard that was defective, not this method.
    /// </para>
    /// <para>
    /// What was checked by reading the code: <c>GetDailyValues()</c> is <c>GetValues(new Range&lt;int&gt;(0, 23))</c>,
    /// which appends one value per <c>i</c> in 0..23 and therefore returns exactly 24 values whenever
    /// <c>GetIndexedDoubles()</c> is non-null. The <c>new Range&lt;int&gt;(max, min)</c> argument order inside
    /// <c>GetValues</c> looks reversed but is harmless: <c>Range&lt;T&gt;</c>'s two-value constructor normalises
    /// through <c>Math.Min</c>/<c>Math.Max</c>. There is no off-by-one or inverted-range defect to fix.
    /// </para>
    /// <para>
    /// The one case that does return null is a profile carrying no values at all - and that, not any range
    /// arithmetic, is the condition the old guard swallowed.
    /// </para>
    /// </summary>
    public class ProfileDailyValuesTests
    {
        private static double[] Window(int from, int to)
        {
            double[] values = new double[24];
            for (int hour = 0; hour < 24; hour++)
            {
                values[hour] = (hour >= from && hour < to) ? 1 : 0;
            }

            return values;
        }

        /// <summary>
        /// The direct answer to "does a Profile built from exactly 24 values give 24 values back": yes,
        /// and the same values in the same order.
        /// </summary>
        [Fact]
        public void ProfileFromTwentyFourValues_GetDailyValues_ReturnsTwentyFourIdenticalValues()
        {
            double[] source = Window(8, 23);

            Profile profile = new Profile("PartO_DayOpen_08_23", ProfileGroup.Ventilation, source);

            double[] values = profile.GetDailyValues();

            Assert.NotNull(values);
            Assert.Equal(24, values.Length);
            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(source[hour], values[hour]);
            }
        }

        /// <summary>
        /// Every single-hour pattern survives, including hour 0 and hour 23 - the two an off-by-one or an
        /// inverted range would lose first.
        /// </summary>
        [Fact]
        public void ProfileFromTwentyFourValues_EveryHourIsPreservedIndependently()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                double[] source = new double[24];
                source[hour] = 1;

                double[] values = new Profile(string.Format("Hour{0}", hour), ProfileGroup.Ventilation, source).GetDailyValues();

                Assert.Equal(24, values.Length);
                for (int i = 0; i < 24; i++)
                {
                    Assert.Equal(i == hour ? 1 : 0, values[i]);
                }
            }
        }

        [Fact]
        public void ProfileFromAllZeroValues_StillReturnsTwentyFourValues()
        {
            double[] values = new Profile("AllZero", ProfileGroup.Ventilation, new double[24]).GetDailyValues();

            Assert.NotNull(values);
            Assert.Equal(24, values.Length);
        }

        /// <summary>
        /// <c>Range&lt;int&gt;</c> normalises its two-value constructor, which is why the reversed
        /// <c>new Range&lt;int&gt;(max, min)</c> inside <c>Profile.GetValues</c> is not a defect.
        /// </summary>
        [Fact]
        public void RangeConstructor_NormalisesArgumentOrder()
        {
            Range<int> range = new Range<int>(23, 0);

            Assert.Equal(0, range.Min);
            Assert.Equal(23, range.Max);
        }

        /// <summary>
        /// The one condition that really does produce no daily values: a profile that carries none. This is
        /// what the removed <c>Length == 24</c> guard was actually catching - and silently.
        /// </summary>
        [Fact]
        public void ProfileWithNoValues_GetDailyValues_IsNull()
        {
            Profile profile = new Profile("Empty", ProfileGroup.Ventilation.Text());

            Assert.Null(profile.GetDailyValues());
        }

        /// <summary>
        /// A profile carrying fewer than 24 indices still yields 24 values (indices are bounded/wrapped
        /// rather than left short), so a short profile never trips a length check either - which is why the
        /// new TAS path validates the SOURCE explicitly instead of inferring validity from a length.
        /// </summary>
        [Fact]
        public void ProfileWithFewerThanTwentyFourIndices_StillReturnsTwentyFourValues()
        {
            Profile profile = new Profile("Short", ProfileGroup.Ventilation, new double[] { 1, 0, 1 });

            double[] values = profile.GetDailyValues();

            Assert.NotNull(values);
            Assert.Equal(24, values.Length);
        }
    }
}
