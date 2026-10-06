// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Whether an opening is <b>positively</b> restricted, <b>positively</b> unrestricted, or states
        /// availability behaviour that cannot be classified as either.
        /// <para>
        /// <b>The classification is read off what TAS will actually be given</b>, not off the authoring
        /// vocabulary, so it stays true whichever component authored the opening. The one question asked is
        /// the one <c>Openings Restricted</c> asks: does this opening spend any hour of the day unavailable
        /// for overheating ventilation?
        /// </para>
        /// <list type="table">
        /// <listheader><term>What the opening states</term><description>Verdict, and why</description></listheader>
        /// <item>
        /// <term><see cref="PartOOpeningProperties"/>, <see cref="OpeningRestriction.Unrestricted"/></term>
        /// <description><b>Unrestricted.</b> Derives no schedule, so the aperture control carries none.</description>
        /// </item>
        /// <item>
        /// <term><see cref="PartOOpeningProperties"/>, <see cref="OpeningRestriction.NightClosed"/></term>
        /// <description><b>Restricted.</b> Derives the <c>PartO_DayOpen_HH_HH</c> availability schedule.</description>
        /// </item>
        /// <item>
        /// <term><see cref="PartOOpeningProperties"/>, <see cref="OpeningRestriction.AlwaysClosed"/></term>
        /// <description><b>Restricted.</b> Expressed downstream as an opening factor of zero.</description>
        /// </item>
        /// <item>
        /// <term><see cref="ProfileOpeningProperties"/> carrying a <see cref="DailyAvailabilitySchedule"/></term>
        /// <description>
        /// <b>Classified from its 24 values</b> - every hour available is unrestricted, any hour unavailable
        /// is restricted. This is safe precisely because a <see cref="DailyAvailabilitySchedule"/> is binary
        /// and exactly 24 hours long: no rounding, no interpolation, no inference. Note what is NOT done -
        /// the schedule is never reverse-engineered back into a <see cref="OpeningRestriction.NightClosed"/>
        /// window, because nothing needs that and the mapping would not be reliable.
        /// </description>
        /// </item>
        /// <item>
        /// <term><see cref="ProfileOpeningProperties"/> with only a legacy <see cref="Profile"/></term>
        /// <description>
        /// <b>Unknown.</b> A <see cref="Profile"/> is a general, sparse, range-compressible curve of arbitrary
        /// doubles. The TAS write does convert one to schedule values through <c>Convert.ToInt32</c>, but that
        /// is a lossy rounding of values that were never authored as an availability mask - reading a
        /// compliance assumption off it would be an inference, not a fact. Refused as unknown instead.
        /// </description>
        /// </item>
        /// <item>
        /// <term>
        /// <see cref="ProfileOpeningProperties"/> with neither, a plain <see cref="OpeningProperties"/>, or
        /// any other <see cref="IOpeningProperties"/>
        /// </term>
        /// <description>
        /// <b>Unrestricted.</b> Not an assumption: <c>SAM.Analytical.Tas.Query.TryGetOpeningScheduleSource</c>
        /// finds no schedule source in any of these, so the aperture control is written without a schedule and
        /// the opening is available every hour. <b>A new <see cref="IOpeningProperties"/> that CAN carry an
        /// availability schedule must be added to both that query and this one</b>, or it will classify here
        /// as unrestricted while restricting the simulation.
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// <b>Unknown dominates within a <see cref="MultipleOpeningProperties"/>.</b> One pane that cannot be
        /// classified makes the aperture unclassifiable, even where another pane is provably restricted:
        /// "restricted" would then be a claim about the aperture resting on a pane nobody has read.
        /// </para>
        /// </summary>
        /// <param name="aperture">The aperture to classify.</param>
        /// <param name="evidence">
        /// What the opening states, in one clause, for a refusal to quote - or null where there is nothing to
        /// quote (the aperture carries no opening properties at all).
        /// </param>
        /// <returns>
        /// <c>true</c> positively restricted, <c>false</c> positively unrestricted, <c>null</c> either
        /// unclassifiable OR not an operable opening - the two are told apart by <paramref name="evidence"/>,
        /// which is non-null only for the unclassifiable case.
        /// </returns>
        public static bool? PartOOpeningRestricted(this Aperture aperture, out string evidence)
        {
            evidence = null;

            if (aperture == null || !aperture.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties) || openingProperties == null)
            {
                return null;
            }

            return PartOOpeningRestricted(openingProperties, out evidence);
        }

        /// <summary>
        /// Whether these opening properties are positively restricted, positively unrestricted, or
        /// unclassifiable. See <see cref="PartOOpeningRestricted(Aperture, out string)"/> for the table.
        /// </summary>
        public static bool? PartOOpeningRestricted(this IOpeningProperties openingProperties, out string evidence)
        {
            evidence = null;

            if (openingProperties == null)
            {
                return null;
            }

            if (openingProperties is PartOOpeningProperties partOOpeningProperties)
            {
                if (partOOpeningProperties.OpeningRestriction == OpeningRestriction.Unrestricted)
                {
                    return false;
                }

                evidence = string.Format("opening restriction {0}", partOOpeningProperties.OpeningRestriction);

                return true;
            }

            if (openingProperties is ProfileOpeningProperties profileOpeningProperties)
            {
                DailyAvailabilitySchedule dailyAvailabilitySchedule = profileOpeningProperties.Schedule;
                if (dailyAvailabilitySchedule != null)
                {
                    bool[] values = dailyAvailabilitySchedule.GetValues();

                    int count = 0;
                    if (values != null)
                    {
                        foreach (bool value in values)
                        {
                            if (!value)
                            {
                                count++;
                            }
                        }
                    }

                    if (count == 0)
                    {
                        return false;
                    }

                    evidence = string.Format("availability schedule '{0}' unavailable for {1} hour(s) of the day", dailyAvailabilitySchedule.Name, count);

                    return true;
                }

                Profile profile = profileOpeningProperties.Profile;
                if (profile != null)
                {
                    evidence = string.Format("a general-valued opening profile '{0}'", profile.Name);

                    return null;
                }

                //Neither carrier: no schedule source, so the aperture control is written without a schedule.
                return false;
            }

            if (openingProperties is MultipleOpeningProperties multipleOpeningProperties)
            {
                List<ISingleOpeningProperties> singleOpeningProperties = multipleOpeningProperties.SingleOpeningProperties;
                if (singleOpeningProperties == null || singleOpeningProperties.Count == 0)
                {
                    return false;
                }

                bool restricted = false;
                string evidence_Restricted = null;

                foreach (ISingleOpeningProperties singleOpeningProperties_Item in singleOpeningProperties)
                {
                    bool? restricted_Item = PartOOpeningRestricted(singleOpeningProperties_Item, out string evidence_Item);

                    if (!restricted_Item.HasValue && evidence_Item != null)
                    {
                        //Unknown dominates - see the remarks.
                        evidence = evidence_Item;

                        return null;
                    }

                    if (restricted_Item == true && !restricted)
                    {
                        restricted = true;
                        evidence_Restricted = evidence_Item;
                    }
                }

                evidence = restricted ? evidence_Restricted : null;

                return restricted;
            }

            //Anything else states no availability at all, so nothing restricts the opening downstream.
            return false;
        }
    }
}
