// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Units;
using System;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Immutable engineering value for a report, with its availability and provenance.
    /// <para>
    /// Availability is derived from which factory built the instance and cannot be set, so an instance can never be
    /// both "available" and "missing". Source, freshness and timestamp exist only on available values; a missing or
    /// not-applicable value carries only its reason (<see cref="Note"/>).
    /// </para>
    /// <para>
    /// <see cref="Available"/> rejects null and non-finite numbers (NaN, infinity, an invalid <see cref="Quantity"/>)
    /// with an <see cref="ArgumentException"/>: passing one is a programming error. A collector that reads an invalid
    /// value from a model reports it with <see cref="NotAvailable"/> instead.
    /// </para>
    /// </summary>
    public sealed class ReportValue<T> : IReportValue
    {
        private readonly T value;

        private ReportValue(Availability availability, T value, ReportValueSource? source, Freshness? freshness, DateTime? sourceTimestamp, string note)
        {
            Availability = availability;
            this.value = value;
            Source = source;
            Freshness = freshness;
            SourceTimestamp = sourceTimestamp;
            Note = note;
        }

        public Availability Availability { get; }

        public bool HasValue => Availability == Availability.Available;

        /// <summary>
        /// The value. Throws <see cref="InvalidOperationException"/> when the value is not available; use
        /// <see cref="TryGetValue"/> when that is expected.
        /// </summary>
        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException(string.Format("Report value is {0}: {1}", Availability, Note));
                }

                return value;
            }
        }

        public ReportValueSource? Source { get; }

        public Freshness? Freshness { get; }

        public DateTime? SourceTimestamp { get; }

        public string Note { get; }

        public bool TryGetValue(out T value)
        {
            value = HasValue ? this.value : default;
            return HasValue;
        }

        /// <summary>
        /// A copy with a different freshness. Only available values have a freshness, so this throws
        /// <see cref="InvalidOperationException"/> on a value that is not available.
        /// </summary>
        public ReportValue<T> WithFreshness(Freshness freshness, string note = null)
        {
            if (!HasValue)
            {
                throw new InvalidOperationException("Only an available report value has a freshness.");
            }

            return new ReportValue<T>(Availability.Available, value, Source, freshness, SourceTimestamp, note ?? Note);
        }

        public static ReportValue<T> Available(T value, ReportValueSource source, Freshness freshness = Reporting.Freshness.Current, DateTime? sourceTimestamp = null, string note = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!IsFinite(value))
            {
                throw new ArgumentException(string.Format("An available report value must be finite and defined, got {0}.", value), nameof(value));
            }

            return new ReportValue<T>(Availability.Available, value, source, freshness, sourceTimestamp, note);
        }

        public static ReportValue<T> NotAvailable(string reason)
        {
            return new ReportValue<T>(Availability.NotAvailable, default, null, null, null, Reason(reason));
        }

        public static ReportValue<T> NotApplicable(string reason)
        {
            return new ReportValue<T>(Availability.NotApplicable, default, null, null, null, Reason(reason));
        }

        public override string ToString()
        {
            return HasValue ? string.Format("{0} ({1}, {2})", value, Source, Freshness) : string.Format("{0}: {1}", Availability, Note);
        }

        private static string Reason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A value that is not available or not applicable needs a reason.", nameof(reason));
            }

            return reason;
        }

        private static bool IsFinite(T value)
        {
            switch (value)
            {
                case double @double:
                    return !double.IsNaN(@double) && !double.IsInfinity(@double);

                case float @float:
                    return !float.IsNaN(@float) && !float.IsInfinity(@float);

                case Quantity quantity:
                    return quantity.IsValid;
            }

            return true;
        }
    }
}
