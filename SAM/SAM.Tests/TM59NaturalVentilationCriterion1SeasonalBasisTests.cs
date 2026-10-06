// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// TM59:2017 Criterion 1 for naturally-ventilated spaces is a May-September (summer) hours test: both the
    /// exceedance count and the 3% limit are counted only over occupied hours between 1 May and 30 September.
    /// <para>
    /// <b>The defect these tests pin.</b> <see cref="TMExtendedResult.Criterion1"/> - the implementation
    /// <see cref="TM59NaturalVentilationExtendedResult"/> inherited unchanged - computes both the exceedance
    /// count and the 3% limit from the FULL YEAR of occupied hours, not the summer subset, even though
    /// <see cref="TM59NaturalVentilationExtendedResult"/> already carries the correct summer-restricted
    /// occupied-hour count (<c>GetSummerOccupiedHours()</c> / <c>GetSummerMaxExceedableHours()</c>) - the same
    /// figures <c>TM59AssessmentReport</c> already trusts for its displayed Limit, just not for the verdict
    /// that decides Pass/Fail. Hours occupied outside May-September therefore dilute or pad the 3% allowance
    /// a room is actually held to, in either direction.
    /// </para>
    /// </summary>
    public class TM59NaturalVentilationCriterion1SeasonalBasisTests
    {
        /// <summary>
        /// Every case uses the same occupied-hour shape - 600 non-summer occupied hours (annual limit 21)
        /// alongside 100 summer occupied hours (summer limit 3) - so the annual and summer limits can never
        /// be confused for one another (21 vs 3), and only where the exceedance hours actually fall can move
        /// the verdict.
        /// </summary>
        [Theory]
        //A real May-September failure - 4 of the 100 summer hours exceed, over the 3-hour summer limit - is
        //hidden by the annual limit: 4 of 700 annual occupied hours is nowhere near the 21-hour annual figure.
        [InlineData(0, 4, false)]
        //The reverse: 20 non-summer exceedances TM59 was never meant to count push the annual total (22) over
        //the annual limit (21), wrongly failing a room whose actual May-September performance (2 of 100,
        //comfortably under the 3-hour limit) was fine.
        [InlineData(20, 2, true)]
        //Boundary: an exceedance count exactly equal to the summer limit (3) still fails - the implementation's
        //existing comparison is strict ("<"), not "<=", and the fix must preserve that direction rather than
        //just changing which hour-basis feeds it.
        [InlineData(0, 3, false)]
        public void Criterion1_IsDecidedBySummerOccupiedHours_NotAnnual(int nonSummerExceeding, int summerExceeding, bool expectedPass)
        {
            TM59NaturalVentilationExtendedResult tM59NaturalVentilationExtendedResult = Result(nonSummerOccupied: 600, summerOccupied: 100, nonSummerExceeding: nonSummerExceeding, summerExceeding: summerExceeding);

            //The two limits this fixture can never confuse for one another.
            Assert.Equal(21, tM59NaturalVentilationExtendedResult.MaxExceedableHours);
            Assert.Equal(3, tM59NaturalVentilationExtendedResult.GetSummerMaxExceedableHours());

            Assert.Equal(expectedPass, tM59NaturalVentilationExtendedResult.Criterion1);
            Assert.Equal(expectedPass, tM59NaturalVentilationExtendedResult.Pass);
        }

        /// <summary>
        /// 600 non-summer occupied hours starting at hour 0 (nowhere near <see cref="HourOfYear.SummerStartIndex"/>)
        /// plus <paramref name="summerOccupied"/> occupied hours starting at <see cref="HourOfYear.SummerStartIndex"/>.
        /// The first <paramref name="nonSummerExceeding"/>/<paramref name="summerExceeding"/> hours of each
        /// block exceed the comfort range (operative 26 against a max acceptable of 25 - a difference of
        /// exactly 1, the implementation's own exceedance threshold); the rest sit at the boundary (0
        /// difference) and do not.
        /// </summary>
        private static TM59NaturalVentilationExtendedResult Result(int nonSummerOccupied, int summerOccupied, int nonSummerExceeding, int summerExceeding)
        {
            HashSet<int> occupiedHourIndices = [];
            IndexedDoubles minAcceptableTemperatures = new();
            IndexedDoubles maxAcceptableTemperatures = new();
            IndexedDoubles operativeTemperatures = new();

            for (int i = 0; i < nonSummerOccupied; i++)
            {
                occupiedHourIndices.Add(i);
                minAcceptableTemperatures.Add(i, 23);
                maxAcceptableTemperatures.Add(i, 25);
                operativeTemperatures.Add(i, i < nonSummerExceeding ? 26 : 25);
            }

            for (int i = 0; i < summerOccupied; i++)
            {
                int index = HourOfYear.SummerStartIndex + i;
                occupiedHourIndices.Add(index);
                minAcceptableTemperatures.Add(index, 23);
                maxAcceptableTemperatures.Add(index, 25);
                operativeTemperatures.Add(index, i < summerExceeding ? 26 : 25);
            }

            return new TM59NaturalVentilationExtendedResult("Living", "SAM.Tests", null, TM52BuildingCategory.CategoryII, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, TM59SpaceApplication.Living);
        }
    }
}
