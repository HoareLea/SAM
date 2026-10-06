// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using SAM.Core;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>UTC-09:00</c> fell through both <c>Query.Double(UTC)</c> and <c>Query.UTC(double)</c> with no
    /// matching case, silently returning <c>NaN</c> / <c>Undefined</c> instead of throwing, so the gap went
    /// unnoticed. Pinned here in both directions, plus a round trip over every named <c>UTC</c> value so a
    /// future gap of the same shape fails loudly instead of silently.
    /// </summary>
    public class UTCTests
    {
        [Fact]
        public void Minus0900_ConvertsToMinusNinePointZero()
        {
            Assert.Equal(-9.0, SAM.Core.UTC.Minus0900.Double());
        }

        [Fact]
        public void MinusNinePointZero_ConvertsToMinus0900()
        {
            Assert.Equal(SAM.Core.UTC.Minus0900, SAM.Core.Query.UTC(-9.0));
        }

        [Fact]
        public void EveryNamedUTCValue_RoundTripsThroughItsDouble()
        {
            foreach (SAM.Core.UTC uTC in Enum.GetValues(typeof(SAM.Core.UTC)))
            {
                if (uTC == SAM.Core.UTC.Undefined)
                {
                    continue;
                }

                double @double = uTC.Double();

                Assert.False(double.IsNaN(@double), string.Format("{0} has no double mapping.", uTC));
                Assert.Equal(uTC, SAM.Core.Query.UTC(@double));
            }
        }
    }
}
