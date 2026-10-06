// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <see cref="ReportValue{T}"/> invariants (Rev 3 R3.2): availability comes from the factory, freshness and source
    /// exist only on available values, and no factory can produce a contradictory state.
    /// </summary>
    public class ReportValueTests
    {
        public static IEnumerable<object[]> AllFactories()
        {
            yield return new object[] { ReportValue<Quantity>.Available(new Quantity(1, UnitType.Watt), ReportValueSource.SAM) };
            yield return new object[] { ReportValue<Quantity>.Available(new Quantity(1, UnitType.Watt), ReportValueSource.TBD, Freshness.Unknown, new DateTime(2026, 1, 1), "note") };
            yield return new object[] { ReportValue<Quantity>.Available(new Quantity(1, UnitType.Watt), ReportValueSource.TSD, Freshness.OutOfDate) };
            yield return new object[] { ReportValue<Quantity>.Available(new Quantity(1, UnitType.Watt), ReportValueSource.Derived).WithFreshness(Freshness.OutOfDate) };
            yield return new object[] { ReportValue<Quantity>.NotAvailable("missing") };
            yield return new object[] { ReportValue<Quantity>.NotApplicable("not relevant") };
        }

        [Theory]
        [MemberData(nameof(AllFactories))]
        public void EveryFactory_HasNoContradictoryState(ReportValue<Quantity> reportValue)
        {
            bool available = reportValue.Availability == Availability.Available;

            Assert.Equal(available, reportValue.HasValue);
            Assert.Equal(available, reportValue.TryGetValue(out Quantity quantity));
            Assert.Equal(available, reportValue.Source.HasValue);
            Assert.Equal(available, reportValue.Freshness.HasValue);

            if (available)
            {
                Assert.True(quantity.IsValid);
                Assert.Equal(quantity, reportValue.Value);
            }
            else
            {
                Assert.Null(reportValue.SourceTimestamp);
                Assert.False(string.IsNullOrWhiteSpace(reportValue.Note));
                Assert.Throws<InvalidOperationException>(() => reportValue.Value);
                Assert.Throws<InvalidOperationException>(() => reportValue.WithFreshness(Freshness.Current));
            }
        }

        [Fact]
        public void Available_DefaultsToCurrent()
        {
            ReportValue<string> reportValue = ReportValue<string>.Available("Office", ReportValueSource.SAM);

            Assert.Equal(Freshness.Current, reportValue.Freshness);
            Assert.Equal(ReportValueSource.SAM, reportValue.Source);
            Assert.Equal("Office", reportValue.Value);
        }

        [Fact]
        public void Available_RejectsNullAndNonFiniteValues()
        {
            Assert.ThrowsAny<ArgumentException>(() => ReportValue<string>.Available(null, ReportValueSource.SAM));
            Assert.ThrowsAny<ArgumentException>(() => ReportValue<double>.Available(double.NaN, ReportValueSource.SAM));
            Assert.ThrowsAny<ArgumentException>(() => ReportValue<double>.Available(double.PositiveInfinity, ReportValueSource.SAM));
            Assert.ThrowsAny<ArgumentException>(() => ReportValue<Quantity>.Available(new Quantity(double.NaN, UnitType.Watt), ReportValueSource.SAM));
            Assert.ThrowsAny<ArgumentException>(() => ReportValue<Quantity>.Available(new Quantity(1, UnitType.Undefined), ReportValueSource.SAM));
        }

        [Fact]
        public void NotAvailableAndNotApplicable_RequireAReason()
        {
            Assert.Throws<ArgumentException>(() => ReportValue<Quantity>.NotAvailable(null));
            Assert.Throws<ArgumentException>(() => ReportValue<Quantity>.NotAvailable(" "));
            Assert.Throws<ArgumentException>(() => ReportValue<Quantity>.NotApplicable(string.Empty));
        }

        [Fact]
        public void WithFreshness_ReturnsANewInstance_AndKeepsTheOriginal()
        {
            ReportValue<Quantity> original = ReportValue<Quantity>.Available(new Quantity(779, UnitType.Watt), ReportValueSource.TBD, sourceTimestamp: new DateTime(2026, 9, 19));

            ReportValue<Quantity> changed = original.WithFreshness(Freshness.OutOfDate, "model changed");

            Assert.NotSame(original, changed);
            Assert.Equal(Freshness.Current, original.Freshness);
            Assert.Equal(Freshness.OutOfDate, changed.Freshness);
            Assert.Equal(original.Value, changed.Value);
            Assert.Equal(original.Source, changed.Source);
            Assert.Equal(original.SourceTimestamp, changed.SourceTimestamp);
            Assert.Equal("model changed", changed.Note);
        }

        [Fact]
        public void ReportValue_HasNoPublicSetters()
        {
            foreach (System.Reflection.PropertyInfo propertyInfo in typeof(ReportValue<Quantity>).GetProperties())
            {
                Assert.True(propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic, propertyInfo.Name);
            }

            Assert.All(typeof(ReportValue<Quantity>).GetConstructors(), x => Assert.False(x.IsPublic));
        }
    }
}
