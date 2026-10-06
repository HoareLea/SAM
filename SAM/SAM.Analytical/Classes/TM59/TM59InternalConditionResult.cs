// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    /// <summary>
    /// Result of resolving a Space to a TM59 InternalCondition via TM59InternalConditionResolver.
    /// Diagnostic is non-null whenever InternalCondition is null (manual review required) or a
    /// noteworthy automatic decision was made (e.g. an area-based bedroom tie-break).
    /// </summary>
    public class TM59InternalConditionResult
    {
        public InternalCondition InternalCondition { get; }
        public TM59SpaceClassification Classification { get; }
        public int Occupancy { get; }
        public int BedroomCount { get; }
        public string Diagnostic { get; }

        public TM59InternalConditionResult(InternalCondition internalCondition, TM59SpaceClassification classification, int occupancy, int bedroomCount, string diagnostic)
        {
            InternalCondition = internalCondition;
            Classification = classification;
            Occupancy = occupancy;
            BedroomCount = bedroomCount;
            Diagnostic = diagnostic;
        }
    }
}
