// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>Query.Simplify</c> - that each field of a simplified TM59 result holds what its name says.
    /// <para>
    /// <b>Why this exists.</b> The simplified results are what <c>TM59AssessmentCalculator.Calculate</c>
    /// returns unless extended results were asked for, so they are what every downstream reader sees - the
    /// Grasshopper outputs, the Part O diagnostic log, and now the TM59 verification report. Their
    /// constructors take a run of same-typed <c>int</c> arguments, which the compiler cannot check, and a
    /// wrong order produces plausible-looking hours under the wrong names rather than an error.
    /// </para>
    /// <para>
    /// The natural-ventilation branch had exactly that defect - see
    /// <see cref="PlainNaturalVentilation_PutsEachHourCountUnderItsOwnName"/> - and it is pinned here rather
    /// than only in the report that found it.
    /// </para>
    /// </summary>
    public class TM59SimplifyTests
    {
        /// <summary>
        /// <b>A real defect, fixed.</b> <c>Simplify</c> passed the summer pair and the annual limit rotated by
        /// one position, so a simplified natural-ventilation result reported
        /// <c>MaxExceedableHours</c> = the summer occupied hours, <c>SummerOccupiedHours</c> = 3% of them, and
        /// <c>MaxExceedableSummerHours</c> = the annual limit. Pass/Fail was unaffected - it is copied from
        /// <c>Pass</c> directly - which is why nothing caught it: the verdict was right and the numbers behind
        /// it were not. The four values below are deliberately all different, so any rotation shows up.
        /// </summary>
        [Fact]
        public void PlainNaturalVentilation_PutsEachHourCountUnderItsOwnName()
        {
            TM59NaturalVentilationExtendedResult tM59NaturalVentilationExtendedResult = NaturalVentilationExtendedResult();

            TM59NaturalVentilationResult tM59NaturalVentilationResult = (TM59NaturalVentilationResult)tM59NaturalVentilationExtendedResult.Simplify();

            //100 occupied hours, 60 of them in summer - so the four figures are 100, 3, 60 and 1.
            Assert.Equal(100, tM59NaturalVentilationExtendedResult.OccupiedHours);
            Assert.Equal(60, tM59NaturalVentilationExtendedResult.GetSummerOccupiedHours());

            Assert.Equal(tM59NaturalVentilationExtendedResult.OccupiedHours, tM59NaturalVentilationResult.OccupiedHours);
            Assert.Equal(tM59NaturalVentilationExtendedResult.MaxExceedableHours, tM59NaturalVentilationResult.MaxExceedableHours);
            Assert.Equal(tM59NaturalVentilationExtendedResult.GetSummerOccupiedHours(), tM59NaturalVentilationResult.SummerOccupiedHours);
            Assert.Equal(tM59NaturalVentilationExtendedResult.GetSummerMaxExceedableHours(), tM59NaturalVentilationResult.MaxExceedableSummerHours);

            //HoursExceedingComfortRange is the SUMMER-restricted count, matching SummerOccupiedHours/
            //MaxExceedableSummerHours beside it and the Criterion 1 verdict this fixture's Pass carries - not
            //the annual GetOccupiedHoursExceedingComfortRange, which TM59 Criterion 1 no longer decides from.
            Assert.Equal(tM59NaturalVentilationExtendedResult.GetSummerOccupiedHoursExceedingComfortRange(), tM59NaturalVentilationResult.HoursExceedingComfortRange);
            Assert.Equal(tM59NaturalVentilationExtendedResult.Pass, tM59NaturalVentilationResult.Pass);
        }

        /// <summary>
        /// The bedroom branch carries its own night-time pair through, and its argument order is correct -
        /// which is what makes the natural-ventilation defect above a rotation in one branch rather than a
        /// shared mistake.
        /// </summary>
        [Fact]
        public void PlainBedroom_CarriesTheNightTimeHoursUnderTheirOwnNames()
        {
            TM59NaturalVentilationBedroomExtendedResult tM59NaturalVentilationBedroomExtendedResult = new("Bedroom", "SAM.Tests", null, TM52BuildingCategory.CategoryII, OccupiedHourIndices(), Temperatures(24), Temperatures(25), Temperatures(27));

            TM59NaturalVentilationBedroomResult tM59NaturalVentilationBedroomResult = (TM59NaturalVentilationBedroomResult)tM59NaturalVentilationBedroomExtendedResult.Simplify();

            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.OccupiedHours, tM59NaturalVentilationBedroomResult.OccupiedHours);
            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.MaxExceedableHours, tM59NaturalVentilationBedroomResult.MaxExceedableHours);
            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.GetSummerOccupiedHours(), tM59NaturalVentilationBedroomResult.SummerOccupiedHours);
            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.GetSummerMaxExceedableHours(), tM59NaturalVentilationBedroomResult.MaxExceedableSummerHours);
            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.GetAnnualNightOccupiedHours(), tM59NaturalVentilationBedroomResult.AnnualNightOccupiedHours);
            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.GetAnnualMaxExceedableNightHours(), tM59NaturalVentilationBedroomResult.MaxExceedableNightHours);
            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.GetNightHoursNumberExceeding26(), tM59NaturalVentilationBedroomResult.NightHoursNumberExceeding26);

            //Not vacuous: this fixture's bedroom really does exceed 26 C at night.
            Assert.True(tM59NaturalVentilationBedroomExtendedResult.GetNightHoursNumberExceeding26() > 0);
        }

        /// <summary>
        /// The night-time verdict survives simplification too, and agrees with the extended branch's.
        /// <para>
        /// <c>Pass</c> on every TM59 result is Criterion 1 alone, so without
        /// <c>TM59NaturalVentilationBedroomResult.Criterion2</c> a caller holding a simplified result had the
        /// night-time hours and the night-time limit but no statement of what they decided - and would have
        /// had to restate the comparison, which is inclusive here and strict everywhere else.
        /// </para>
        /// </summary>
        [Fact]
        public void PlainBedroom_CarriesTheNightTimeCriterionAndNotJustItsHours()
        {
            TM59NaturalVentilationBedroomExtendedResult tM59NaturalVentilationBedroomExtendedResult = new("Bedroom", "SAM.Tests", null, TM52BuildingCategory.CategoryII, OccupiedHourIndices(), Temperatures(24), Temperatures(25), Temperatures(27));

            TM59NaturalVentilationBedroomResult tM59NaturalVentilationBedroomResult = (TM59NaturalVentilationBedroomResult)tM59NaturalVentilationBedroomExtendedResult.Simplify();

            Assert.Equal(tM59NaturalVentilationBedroomExtendedResult.Criterion2, tM59NaturalVentilationBedroomResult.Criterion2);

            //Not vacuous: this fixture's bedroom exceeds 26 C on every night-occupied hour, far past the 1%
            //allowance - so the two agreeing is agreement on a false, not on a default.
            Assert.False(tM59NaturalVentilationBedroomResult.Criterion2);

            //And the inclusive boundary is the direction the extended type states: actual == limit passes.
            Assert.True(new TM59NaturalVentilationBedroomResult("Bedroom", "SAM.Tests", null, TM52BuildingCategory.CategoryII, 3672, 110, 37, 3285, 3672, 110, 32, 32, true).Criterion2);
            Assert.False(new TM59NaturalVentilationBedroomResult("Bedroom", "SAM.Tests", null, TM52BuildingCategory.CategoryII, 3672, 110, 37, 3285, 3672, 110, 32, 33, true).Criterion2);
        }

        private static TM59NaturalVentilationExtendedResult NaturalVentilationExtendedResult()
        {
            return new TM59NaturalVentilationExtendedResult("Living", "SAM.Tests", null, TM52BuildingCategory.CategoryII, OccupiedHourIndices(), Temperatures(24), Temperatures(25), Temperatures(27), TM59SpaceApplication.Living);
        }

        /// <summary>
        /// 100 occupied hours: 40 at the start of the year and 60 inside the summer window, so the annual and
        /// summer figures cannot be confused for one another.
        /// </summary>
        private static HashSet<int> OccupiedHourIndices()
        {
            HashSet<int> result = [];

            for (int i = 0; i < 40; i++)
            {
                result.Add(i);
            }

            for (int i = 0; i < 60; i++)
            {
                result.Add(HourOfYear.SummerStartIndex + i);
            }

            return result;
        }

        private static IndexedDoubles Temperatures(double value)
        {
            IndexedDoubles result = new();

            foreach (int index in OccupiedHourIndices())
            {
                result.Add(index, value);
            }

            return result;
        }
    }
}
