// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Units;
using System;
using System.Globalization;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Host-supplied settings for one document run: unit system, culture, display preferences, metadata and style.
    /// </summary>
    public sealed class DocumentOptions
    {
        public DocumentOptions()
        {
        }

        public DocumentOptions(DocumentOptions documentOptions)
        {
            if (documentOptions == null)
            {
                return;
            }

            UnitSystem = documentOptions.UnitSystem;
            Culture = documentOptions.Culture;
            SIAirFlow = documentOptions.SIAirFlow;
            Metadata = documentOptions.Metadata;
            Style = documentOptions.Style;
            GeneratedAt = documentOptions.GeneratedAt;
            SoftwareVersion = documentOptions.SoftwareVersion;
        }

        /// <summary>
        /// SI (default) or Imperial.
        /// </summary>
        public UnitStyle UnitSystem { get; set; } = UnitStyle.SI;

        /// <summary>
        /// Culture for decimal and grouping separators and dates. Defaults to en-GB.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.GetCultureInfo("en-GB");

        /// <summary>
        /// SI air flow display unit: L/s (default) or m³/s. Imperial always uses cfm.
        /// </summary>
        public AirFlowDisplay SIAirFlow { get; set; } = AirFlowDisplay.LitersPerSecond;

        /// <summary>
        /// Project details supplied by the host (project name/number, prepared by). May be null.
        /// </summary>
        public DocumentMetadata Metadata { get; set; }

        /// <summary>
        /// Branding. Null means <see cref="DocumentStyle.Default"/>.
        /// </summary>
        public DocumentStyle Style { get; set; }

        /// <summary>
        /// Generation time printed in the footer. Null means now; tests fix it for deterministic snapshots.
        /// </summary>
        public DateTime? GeneratedAt { get; set; }

        /// <summary>
        /// SAM version printed in the footer. Null means the version of the reporting assembly; tests fix it.
        /// </summary>
        public string SoftwareVersion { get; set; }
    }
}
