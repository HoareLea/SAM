// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class PartFData : SAMObject
    {
        /// <summary>
        /// Default setback operating factor: the setback rate is 30% of the continuous design rate,
        /// i.e. a 70% reduction.
        /// <para>
        /// Neither the 2021 nor the 2026 edition of Approved Document F, Volume 1 specifies a reduced
        /// operating rate for mechanical ventilation with heat recovery. This factor is therefore a SAM
        /// reduced-operation convention, not a regulatory value.
        /// </para>
        /// <para>
        /// Deliberately named "setback" rather than "background": Approved Document F uses "background
        /// ventilator" for a trickle ventilator and "whole dwelling (general) ventilation" for the
        /// continuous requirement, so calling a reduced operating rate a background rate would invite
        /// confusion with both.
        /// </para>
        /// </summary>
        public const double DefaultSetbackFlowRateFactor = 0.3;

        /// <summary>
        /// Default whole dwelling ventilation rate [l/s] where the dwelling has exactly one habitable
        /// room. Approved Document F, Volume 1: Dwellings (2021 edition, England), Table 1.3 note 1
        /// (page 10): "If the dwelling only has one habitable room, a minimum ventilation rate of 13l/s
        /// should be used."
        /// </summary>
        public const double DefaultOneHabitableRoomRate_Lps = 13;

        public Dictionary<string, PartFCategory> PartFCategories { get; set; } = [];

        public Dictionary<int, double> WholeDwellingRates_Lps { get; set; } = [];

        private SpaceSemanticsResolver spaceSemanticsResolver = null;

        private Dictionary<SpaceUse, PartFCategory> dictionary_SpaceUse = null;

        private TextMap textMap_Legacy = null;

        private double setbackFlowRateFactor = DefaultSetbackFlowRateFactor;

        /// <summary>
        /// Additional whole dwelling ventilation rate [l/s] for each bedroom above the highest
        /// tabulated bedroom count. Defaults to 6 l/s per Approved Document F, Volume 1: Dwellings
        /// (2021 edition, England), Table 1.3 note 2 (page 10).
        /// </summary>
        public double IncrementAbove5 { get; set; } = 6;

        /// <summary>
        /// AreaRate [l/(s*m2)] of internal floor area. Defaults to 0.3 l/(s*m2) per Approved
        /// Document F, Volume 1: Dwellings (2021 edition, England), paragraph 1.24a (page 10).
        /// </summary>
        public double AreaRate_LpsPerM2 { get; set; } = 0.3;

        /// <summary>
        /// Whole dwelling ventilation rate [l/s] applied where the dwelling has exactly one habitable
        /// room, per Table 1.3 note 1 (page 10). Defaults to
        /// <see cref="DefaultOneHabitableRoomRate_Lps"/> (13 l/s).
        /// <para>
        /// This is a regulatory requirement, not a SAM convention: where it applies it REPLACES the
        /// Table 1.3 bedroom based rate, so a one habitable room dwelling is sized at 13 l/s rather than
        /// the one bedroom figure of 19 l/s. The final continuous design rate can still be higher, since
        /// the floor area rate and the total of the wet room minimums also apply.
        /// </para>
        /// </summary>
        public double OneHabitableRoomRate_Lps { get; set; } = DefaultOneHabitableRoomRate_Lps;

        /// <summary>
        /// Minimum kitchen extract rate [l/s] for an INTERMITTENT extract system where a cooker hood
        /// extracts to the outside. Approved Document F, Volume 1: Dwellings (2021 edition, England)
        /// Table 1.1 (page 8) and Diagram 1.1 (page 9): 30 l/s.
        /// </summary>
        public double IntermittentKitchenRateWithCookerHood_Lps { get; set; } = 30;

        /// <summary>
        /// Minimum kitchen extract rate [l/s] for an INTERMITTENT extract system where there is no cooker
        /// hood, or the cooker hood does not extract to the outside. Table 1.1 (page 8) and Diagram 1.2
        /// (page 9): 60 l/s.
        /// <para>
        /// Diagram 1.2 note 1 also states that a recirculating cooker hood on its own does not provide a
        /// means of ventilation that complies with Part F, so a recirculating hood does not reduce this
        /// figure and does not satisfy the requirement by itself.
        /// </para>
        /// </summary>
        public double IntermittentKitchenRateWithoutCookerHood_Lps { get; set; } = 60;

        /// <summary>
        /// How continuous extract above the Table 1.2 minimums is shared between the dwelling's extract
        /// terminals.
        /// <para>
        /// Approved Document F prescribes only two things about extract totals: each wet room reaches at
        /// least its Table 1.2 minimum high rate (paragraph 1.70), and the sum of all extract on its
        /// continuous rate is at least the whole dwelling ventilation rate (Table 1.2, continuous rate
        /// column). The split of the surplus between terminals is an engineering strategy, so it is named
        /// on the rule set, recorded on every result, and can be changed.
        /// </para>
        /// </summary>
        public Enums.PartFExtractAllocationStrategy ExtractAllocationStrategy { get; set; } = Enums.PartFExtractAllocationStrategy.MinimumFirstCookingPriority;

        /// <summary>
        /// Reduced-operation factor applied to every continuous design flow rate to obtain the setback
        /// flow rate, so setback = continuous design x factor. Defaults to
        /// <see cref="DefaultSetbackFlowRateFactor"/> (0.30), i.e. the setback rate is 30% of the
        /// continuous design rate.
        /// <para>
        /// An operating mode only. It is applied after every Approved Document F minimum has been
        /// satisfied at the continuous design condition and never reduces or replaces the regulatory
        /// sizing calculation.
        /// </para>
        /// <para>
        /// Only a factor greater than 0 and no greater than 1 is accepted. Zero, a negative value, a
        /// value above 1, NaN and infinity are all rejected and replaced by the documented default
        /// rather than silently producing a setback rate that exceeds the continuous design rate, is
        /// zero, or is not a number. Use <see cref="IsValidSetbackFlowRateFactor(double)"/> to test a
        /// value before assigning it if the caller needs to report the rejection.
        /// </para>
        /// </summary>
        public double SetbackFlowRateFactor
        {
            get
            {
                return setbackFlowRateFactor;
            }

            set
            {
                setbackFlowRateFactor = IsValidSetbackFlowRateFactor(value) ? value : DefaultSetbackFlowRateFactor;
            }
        }

        /// <summary>
        /// True for a setback flow rate factor that is a real number greater than 0 and no greater than
        /// 1. A factor of exactly 1 means the system runs at the continuous design rate with no
        /// reduction; zero is rejected because it would stop the ventilation entirely.
        /// </summary>
        public static bool IsValidSetbackFlowRateFactor(double factor)
        {
            return !double.IsNaN(factor) && !double.IsInfinity(factor) && factor > 0 && factor <= 1;
        }

        public PartFData()
        {
        }

        /// <summary>
        /// Minimum extract rate [l/s] for the room containing the cooking function on a CONTINUOUS
        /// system, i.e. the kitchen high rate of Approved Document F, Volume 1: Dwellings (2021 edition,
        /// England) Table 1.2 (page 10).
        /// <para>
        /// Read from the rule set's own kitchen category so that an edited rule set is honoured, and
        /// falling back to the tabulated 13 l/s where the rule set describes no kitchen. Needed as a
        /// standalone lookup because a studio and an open plan living kitchen also contain the cooking
        /// function and are habitable rooms, so they carry no kitchen category of their own but still need
        /// the kitchen rate for their local kitchen extract terminal.
        /// </para>
        /// </summary>
        public double GetKitchenExtractHighRate_Lps()
        {
            double? result = GetPartFCategory(SpaceUse.Kitchen)?.MinFlowRate_Lps;

            return result is null || double.IsNaN(result.Value) || result.Value <= 0 ? 13 : result.Value;
        }

        /// <summary>
        /// Minimum whole dwelling ventilation rate [l/s] determined by the number of bedrooms.
        /// Approved Document F, Volume 1: Dwellings (2021 edition, England), Table 1.3 (page 10).
        /// </summary>
        /// <param name="count">Number of bedrooms in the dwelling.</param>
        /// <returns>Minimum whole dwelling ventilation rate [l/s], or NaN if no rate table is loaded.</returns>
        public double GetWholeDwellingRates_Lps(int count)
        {
            if(WholeDwellingRates_Lps is null || WholeDwellingRates_Lps.Count == 0)
            {
                return double.NaN;
            }

            if(WholeDwellingRates_Lps.TryGetValue(count, out double result))
            {
                return result;
            }

            int count_Min = WholeDwellingRates_Lps.Keys.Min();
            if(count <= count_Min)
            {
                //Table 1.3 starts at one bedroom and gives no value below it. A dwelling with no room
                //classified as a bedroom is treated as a one bedroom dwelling rather than extrapolated
                //below the table.
                //
                //Table 1.3 note 1 (a dwelling with only one habitable room uses 13 l/s) is NOT applied
                //here: it depends on the habitable ROOM count, not the bedroom count, so it cannot be
                //decided from this argument alone. See GetBedroomOrHabitableRate_Lps, which selects
                //between note 1 and this table.
                return WholeDwellingRates_Lps[count_Min];
            }

            int count_Max = WholeDwellingRates_Lps.Keys.Max();

            //Table 1.3 note 2: for each additional bedroom, add IncrementAbove5 (6 l/s) to the
            //highest tabulated value.
            return WholeDwellingRates_Lps[count_Max] + ((count - count_Max) * IncrementAbove5);
        }

        /// <summary>
        /// The whole dwelling ventilation rate [l/s] set by the dwelling's rooms, selecting between the
        /// two Approved Document F, Volume 1 (2021 edition, England) provisions that can supply it:
        /// <list type="bullet">
        /// <item>Table 1.3 note 1 (page 10): where the dwelling has exactly ONE habitable room, 13 l/s;</item>
        /// <item>Table 1.3 (page 10): otherwise, the rate for the number of bedrooms.</item>
        /// </list>
        /// <para>
        /// Note 1 keys off the habitable ROOM count, not the bedroom count. A studio is one habitable
        /// room and one bedroom equivalent, so a studio with a separate bathroom is a one habitable room
        /// dwelling and takes 13 l/s - not the one bedroom figure of 19 l/s. Adding any second habitable
        /// room (a separate living room, a study) takes the dwelling out of note 1 and back onto the
        /// bedroom table.
        /// </para>
        /// <para>
        /// A bathroom, ensuite, utility room, sanitary accommodation, circulation space, store, plant
        /// room or void is not a habitable room (Appendix A, page 36), so none of them takes a dwelling
        /// out of note 1.
        /// </para>
        /// <para>
        /// This is only one of the applicable minimums. The caller must still take the greatest of this
        /// rate, the floor area rate of paragraph 1.24a, and the total of the Table 1.2 wet room minimums.
        /// </para>
        /// </summary>
        /// <param name="habitableRoomCount">Number of habitable rooms in the dwelling.</param>
        /// <param name="bedroomCount">Number of bedrooms in the dwelling.</param>
        public double GetBedroomOrHabitableRate_Lps(int habitableRoomCount, int bedroomCount)
        {
            if (habitableRoomCount == 1)
            {
                return OneHabitableRoomRate_Lps;
            }

            return GetWholeDwellingRates_Lps(bedroomCount);
        }

        /// <summary>
        /// The shared space use vocabulary this rule set recognises rooms with: the default
        /// SAM_SpaceUseTextMap merged with each category's own <see cref="PartFCategory.Synonyms"/>,
        /// so a project-specific rule set can add its own names without losing the shared ones.
        /// <para>
        /// Built lazily on first use rather than in the constructor, because reading the default text
        /// map goes through ActiveSetting, which itself constructs a PartFData while it is still being
        /// initialised.
        /// </para>
        /// </summary>
        public SpaceSemanticsResolver SpaceSemanticsResolver
        {
            get
            {
                spaceSemanticsResolver ??= CreateResolver();
                return spaceSemanticsResolver;
            }
        }

        private SpaceSemanticsResolver CreateResolver()
        {
            TextMap textMap = Core.Create.TextMap("SpaceUse");

            TextMap textMap_Default = Query.DefaultSpaceUseTextMap();
            if (textMap_Default is not null)
            {
                textMap.AddRange(textMap_Default);
            }

            //Each category's own synonyms are merged in under its SpaceUse, so an existing edited rule
            //set keeps working: its names still classify even where they are absent from the shared
            //vocabulary. A category with no SpaceUse predates the shared layer and cannot be merged.
            foreach (PartFCategory partFCategory in PartFCategories?.Values ?? Enumerable.Empty<PartFCategory>())
            {
                if (partFCategory is null || partFCategory.SpaceUse == SpaceUse.Undefined)
                {
                    continue;
                }

                List<string> synonyms = partFCategory.Synonyms;
                if (synonyms is null || synonyms.Count == 0)
                {
                    continue;
                }

                textMap.Add(partFCategory.SpaceUse.ToString(), [.. synonyms]);
            }

            return new SpaceSemanticsResolver(textMap);
        }

        /// <summary>
        /// The Approved Document F category that applies to a shared space use, or null where this rule
        /// set describes no category for it.
        /// </summary>
        public PartFCategory GetPartFCategory(SpaceUse spaceUse)
        {
            if (spaceUse == SpaceUse.Undefined)
            {
                return null;
            }

            if (dictionary_SpaceUse is null)
            {
                dictionary_SpaceUse = [];
                foreach (PartFCategory partFCategory in PartFCategories?.Values ?? Enumerable.Empty<PartFCategory>())
                {
                    if (partFCategory is null || partFCategory.SpaceUse == SpaceUse.Undefined)
                    {
                        continue;
                    }

                    //First category wins, so a rule set that maps two categories onto one space use
                    //behaves predictably rather than depending on dictionary ordering.
                    if (!dictionary_SpaceUse.ContainsKey(partFCategory.SpaceUse))
                    {
                        dictionary_SpaceUse[partFCategory.SpaceUse] = partFCategory;
                    }
                }
            }

            return dictionary_SpaceUse.TryGetValue(spaceUse, out PartFCategory result) ? result : null;
        }

        /// <summary>
        /// Resolves a space to its Approved Document F category through the shared semantic
        /// classification layer, reporting how the classification was reached.
        /// </summary>
        /// <returns>The category, or null where the space could not be classified.</returns>
        public PartFCategory GetPartFCategory(Space space, out SpaceSemantics spaceSemantics)
        {
            spaceSemantics = SpaceSemanticsResolver?.Resolve(space);

            PartFCategory result = spaceSemantics is null ? null : GetPartFCategory(spaceSemantics.SpaceUse);
            if (result is not null)
            {
                return result;
            }

            //Fall back to matching the space name against the categories that carry no SpaceUse, so a
            //rule set written before the shared vocabulary existed - or one built in code, as the tests
            //and any bespoke project rule set do - still classifies its own rooms. Matching uses the same
            //deterministic whole-token/phrase matcher, so this fallback is not a return to substring
            //scoring.
            //
            //spaceSemantics is deliberately cleared when this path wins: the shared layer did not
            //classify the space, the rule set's own categories did, and reporting a shared classification
            //that was never established would be misleading.
            PartFCategory result_Legacy = GetLegacyPartFCategory(space?.Name);
            if (result_Legacy is not null)
            {
                spaceSemantics = null;
                return result_Legacy;
            }

            return null;
        }

        /// <summary>
        /// Matches a room name against the categories that carry no <see cref="PartFCategory.SpaceUse"/>,
        /// keyed by category name. Returns null where nothing matched or two categories matched equally
        /// well.
        /// </summary>
        private PartFCategory GetLegacyPartFCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (textMap_Legacy is null)
            {
                textMap_Legacy = Core.Create.TextMap("PartFLegacy");

                foreach (PartFCategory partFCategory in PartFCategories?.Values ?? Enumerable.Empty<PartFCategory>())
                {
                    if (partFCategory?.Name is not string name_Category || partFCategory.SpaceUse != SpaceUse.Undefined)
                    {
                        continue;
                    }

                    List<string> synonyms = partFCategory.Synonyms;
                    if (synonyms is null || synonyms.Count == 0)
                    {
                        synonyms = [name_Category];
                    }

                    textMap_Legacy.Add(name_Category, [.. synonyms]);
                }
            }

            string key = textMap_Legacy.SemanticBestTextMapKey(name);
            if (key is null)
            {
                return null;
            }

            return PartFCategories.TryGetValue(key, out PartFCategory result) ? result : null;
        }

        /// <summary>
        /// Resolves a room name to an Approved Document F category through the shared semantic
        /// classification layer.
        /// <para>
        /// Retained for callers that only hold a name. Unlike the previous implementation this no
        /// longer uses TextMap.GetSortedKeys, whose bidirectional substring scoring let a name match on
        /// a shared word fragment - "Server Room" scored against the "living room" and "shower room"
        /// aliases on the token "room" alone. Matching is now whole-token or whole-phrase only, and an
        /// ambiguous name resolves to null rather than to whichever candidate happened to sort first.
        /// </para>
        /// </summary>
        public PartFCategory GetPartFCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            //Resolve through a Space so the one resolution cascade in SpaceSemanticsResolver is used
            //here too, rather than a second, subtly different name-matching path.
            return GetPartFCategory(new Space(name), out SpaceSemantics _);
        }
    }
}
