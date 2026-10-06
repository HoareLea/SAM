// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Resolves a Space to a TM59 InternalCondition, deterministically and without requiring a zone
    /// for non-habitable spaces (corridors, bathrooms, risers, ...). Primary entry point is
    /// Resolve(Space, IEnumerable&lt;Space&gt;) - a pure function of the space and the other spaces in
    /// its flat/unit, so it is unit-testable without an AdjacencyCluster. The AdjacencyCluster overload
    /// wraps it and reads zone membership fresh on every call. Per-Space classification is cached, but
    /// self-invalidates if a Space with the same Guid is later seen with a different Name - see
    /// classificationCacheName - since TM59Manager deliberately reuses one resolver instance across
    /// many Resolve calls over a live, mutable model.
    /// </summary>
    public class TM59InternalConditionResolver
    {
        private enum BedroomKeyword { None, Single, Double }

        /// <summary>
        /// The one authoritative TM59 InternalCondition name that means a real communal corridor - the
        /// only positive identification the domain has. A report or any other caller that needs to tell a
        /// communal corridor apart from an ancillary space with no occupied TM59 use (bathroom, hall,
        /// ensuite, ...) compares against this name, never against the Space name.
        /// </summary>
        public const string CommunalCorridorInternalConditionName = "TM59_Communal Corridor (including pipework gains)";

        //"TM59_Bathroom" and "TM59_Internal Corridor" were previously one combined condition,
        //"TM59_Bathroom/internal corridors". They carry identical profiles and gains, so TM59 results are
        //unchanged, but a single condition whose NAME meant two different room uses could not be read
        //back as either one: the shared classification layer resolved that name to Circulation (the
        //longer phrase), so every bathroom and ensuite carrying it was reported as disagreeing with its
        //own space name. Two separately named conditions each mean exactly one thing.
        private static readonly string[] NonHabitableConditionNames = new[]
        {
            "TM59_Bathroom",
            "TM59_Internal Corridor",
            CommunalCorridorInternalConditionName,
            "TM59_Stairs",
            "TM59_Cupboard/riser/lift/void",
            "TM59_Cupboard with HIU",
            "TM59_Riser Communal pipework",
        };

        private const string SingleBedroomConditionName = "Single Bedroom";
        private const string DoubleBedroomConditionName = "Double Bedroom";
        private const string StudioConditionName = "Studio";

        private static readonly string[] BedroomTypeKeys = { SingleBedroomConditionName, DoubleBedroomConditionName };

        private static readonly string[] DirectMatchKeys =
            NonHabitableConditionNames.Concat(BedroomTypeKeys).ToArray();

        private readonly TextMap textMap;
        private readonly InternalConditionLibrary internalConditionLibrary;

        private readonly Dictionary<Guid, TM59SpaceClassification> classificationCache = new Dictionary<Guid, TM59SpaceClassification>();
        private readonly Dictionary<Guid, string> matchedConditionNameCache = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, BedroomKeyword> explicitBedroomKeywordCache = new Dictionary<Guid, BedroomKeyword>();

        // The Name each Guid's cache entries above were computed from - Classify's only input besides
        // the Guid itself. TM59Manager deliberately reuses one resolver instance across many Resolve
        // calls, so a caller that constructs a new Space with the same Guid but a different Name (the
        // only way to "rename" a Space, since Name has no setter) must invalidate the stale entry
        // rather than have it silently returned forever.
        private readonly Dictionary<Guid, string> classificationCacheName = new Dictionary<Guid, string>();

        public TM59InternalConditionResolver(TextMap textMap, InternalConditionLibrary internalConditionLibrary)
        {
            this.textMap = textMap;
            this.internalConditionLibrary = internalConditionLibrary;
        }

        /// <summary>Single-space classification, independent of any flat/zone context.</summary>
        public TM59SpaceClassification Classify(Space space)
        {
            if (space == null)
                return TM59SpaceClassification.Undefined;

            if (classificationCache.TryGetValue(space.Guid, out TM59SpaceClassification cached)
                && classificationCacheName.TryGetValue(space.Guid, out string cachedName)
                && string.Equals(cachedName, space.Name, StringComparison.Ordinal))
            {
                return cached;
            }

            TM59SpaceClassification classification = ClassifyCore(space, out string matchedConditionName, out BedroomKeyword explicitBedroomKeyword);

            classificationCache[space.Guid] = classification;
            matchedConditionNameCache[space.Guid] = matchedConditionName;
            explicitBedroomKeywordCache[space.Guid] = explicitBedroomKeyword;
            classificationCacheName[space.Guid] = space.Name;

            return classification;
        }

        private TM59SpaceClassification ClassifyCore(Space space, out string matchedConditionName, out BedroomKeyword explicitBedroomKeyword)
        {
            matchedConditionName = null;
            explicitBedroomKeyword = BedroomKeyword.None;

            string name = space?.Name;
            if (string.IsNullOrWhiteSpace(name) || textMap == null)
                return TM59SpaceClassification.Undefined;

            // Deliberately NOT TM59Manager.IsSleeping/IsLiving/IsCooking here: those use TextMap.GetSortedKeys,
            // whose substring scoring treats e.g. "room" as matching the "Sleeping" alias "bedroom" (a
            // whole word contains it) - so a plain "Living Room" would fuzzily read as Sleeping. The
            // classifier needs the same whole-token/phrase discipline as the direct-condition matches below.
            // TM59Manager's role methods stay untouched - SAM_Tas depends on their exact current behaviour.
            List<TM59TextMapMatch> sleepingMatches = textMap.TM59TextMapMatches(name, new[] { "Sleeping" });
            List<TM59TextMapMatch> livingMatches = textMap.TM59TextMapMatches(name, new[] { "Living" });
            List<TM59TextMapMatch> cookingMatches = textMap.TM59TextMapMatches(name, new[] { "Cooking" });

            bool sleeping = sleepingMatches.Count > 0;
            bool living = livingMatches.Count > 0;
            bool cooking = cookingMatches.Count > 0;

            int sleepingTokenCount = sleeping ? sleepingMatches[0].TokenCount : 0;
            int livingTokenCount = living ? livingMatches[0].TokenCount : 0;
            int cookingTokenCount = cooking ? cookingMatches[0].TokenCount : 0;

            List<TM59TextMapMatch> bedroomTypeMatches = textMap.TM59TextMapMatches(name, BedroomTypeKeys);
            int bedroomTypeTokenCount = bedroomTypeMatches.Count > 0 ? bedroomTypeMatches[0].TokenCount : 0;

            List<TM59TextMapMatch> nonHabitableMatches = textMap.TM59TextMapMatches(name, NonHabitableConditionNames);
            int nonHabitableTokenCount = nonHabitableMatches.Count > 0 ? nonHabitableMatches[0].TokenCount : 0;

            // A legacy Sleeping alias that is ALSO literally the bedroom-size modifier this name matched
            // (twin/double/dbl - kept in that row only for SAM_Tas's own IsSleeping compatibility, see
            // TM59ResourceTests) is weak, overloaded evidence: on its own it means nothing until paired
            // with an actual bedroom noun. It only counts as genuine, independent Sleeping-role evidence
            // when nothing more specific (a real Living/Cooking noun, or a non-habitable noun) competes
            // for the same tokens - a tie goes against it, not just a loss.
            bool sleepingIsSpentBedroomTypeAlias = sleeping && bedroomTypeMatches.Count > 0
                && string.Equals(sleepingMatches[0].Alias, bedroomTypeMatches[0].Alias, StringComparison.OrdinalIgnoreCase);

            int bestCompetingNonSleepingTokenCount = System.Math.Max(nonHabitableTokenCount, System.Math.Max(livingTokenCount, cookingTokenCount));

            bool sleepingIsGenuine = sleeping && (!sleepingIsSpentBedroomTypeAlias || sleepingTokenCount > bestCompetingNonSleepingTokenCount);

            bool roleExists = sleepingIsGenuine || living || cooking;
            int bestRoleTokenCount = 0;
            if (sleepingIsGenuine) bestRoleTokenCount = System.Math.Max(bestRoleTokenCount, sleepingTokenCount);
            if (living) bestRoleTokenCount = System.Math.Max(bestRoleTokenCount, livingTokenCount);
            if (cooking) bestRoleTokenCount = System.Math.Max(bestRoleTokenCount, cookingTokenCount);

            // Tier 1: explicit bedroom-size keywords (Single/Double Bedroom). These are a REFINEMENT of
            // the Sleeping role specifically, not a competing generic category, so a tie against the RAW
            // Sleeping match it refines (not sleepingIsGenuine - e.g. bare "Twin" is simultaneously a
            // Sleeping alias and a Single Bedroom alias, both 1 token) is resolved in favour of the more
            // specific bedroom-size reading. But it must be STRICTLY more specific than any competing
            // Living/Cooking evidence ("Double Kitchen"/"Master Living Room" must fall through to the
            // Cooking/Living role, not Bedroom) and STRICTLY more specific than any competing non-habitable
            // noun ("Master Bathroom"/"Twin Ensuite" must read as the bathroom condition).
            bool bedroomTypeBeatsNonHabitable = nonHabitableTokenCount == 0 || bedroomTypeTokenCount > nonHabitableTokenCount;
            bool bedroomTypeBeatsLiving = livingTokenCount == 0 || bedroomTypeTokenCount > livingTokenCount;
            bool bedroomTypeBeatsCooking = cookingTokenCount == 0 || bedroomTypeTokenCount > cookingTokenCount;
            bool bedroomTypeBeatsRawSleeping = !sleeping || bedroomTypeTokenCount >= sleepingTokenCount;

            if (bedroomTypeMatches.Count > 0 && bedroomTypeBeatsNonHabitable && bedroomTypeBeatsLiving && bedroomTypeBeatsCooking && bedroomTypeBeatsRawSleeping)
            {
                string bedroomTypeKey = textMap.TM59BestTextMapKey(name, BedroomTypeKeys);
                if (bedroomTypeKey == SingleBedroomConditionName)
                {
                    explicitBedroomKeyword = BedroomKeyword.Single;
                    return TM59SpaceClassification.Bedroom;
                }

                if (bedroomTypeKey == DoubleBedroomConditionName)
                {
                    explicitBedroomKeyword = BedroomKeyword.Double;
                    return TM59SpaceClassification.Bedroom;
                }

                // bedroomTypeKey == null: "single" and "double" keywords tied with each other (e.g. a name
                // that somehow contains both at equal phrase length) - fall through and let the role-based
                // path decide Bedroom with no explicit size, rather than blocking the whole space.
            }

            // Tier 2: the 6 non-habitable conditions. These ARE a competing, generic category, so per
            // "generic keywords must not override Studio/Bedroom/Living Room/Kitchen/.../Bathroom/Ensuite"
            // they may only win when STRICTLY more specific (more tokens) than the best GENUINE habitable
            // role match - a tie (e.g. "Kitchen"=Cooking vs "Store"=Cupboard, both 1 token) favours the
            // role. Using roleExists/bestRoleTokenCount here (built from sleepingIsGenuine, not raw
            // sleeping) means a spent bedroom-size alias that already lost tier 1 above cannot resurrect
            // itself here just to block the non-habitable noun it already conceded to.
            if (nonHabitableMatches.Count > 0 && (!roleExists || nonHabitableMatches[0].TokenCount > bestRoleTokenCount))
            {
                string nonHabitableKey = textMap.TM59BestTextMapKey(name, NonHabitableConditionNames);
                if (nonHabitableKey != null)
                {
                    matchedConditionName = nonHabitableKey;
                    return TM59SpaceClassification.NonHabitable;
                }

                // Ambiguous within the non-habitable tier itself (e.g. "Stair Lobby": "stair" vs "lobby",
                // both 1 token, both 5 characters) at a specificity that already dominates or matches any
                // role evidence - a genuine conflict the matcher must not silently guess at.
                return TM59SpaceClassification.Undefined;
            }

            // Tier 3: habitable role combination (unchanged priority order), using only GENUINE Sleeping
            // evidence - a spent bedroom-size alias that lost tiers 1 and 2 above must not resurrect
            // itself here either, just because this cascade checks Sleeping before Living/Cooking.
            if (sleepingIsGenuine && living && cooking)
                return TM59SpaceClassification.Studio;

            if (sleepingIsGenuine)
                return TM59SpaceClassification.Bedroom;

            if (living && cooking)
                return TM59SpaceClassification.LivingRoomKitchen;

            if (cooking)
                return TM59SpaceClassification.Kitchen;

            if (living)
                return TM59SpaceClassification.LivingRoom;

            return TM59SpaceClassification.Undefined;
        }

        private static bool IsHabitable(TM59SpaceClassification classification)
        {
            switch (classification)
            {
                case TM59SpaceClassification.Bedroom:
                case TM59SpaceClassification.LivingRoom:
                case TM59SpaceClassification.Kitchen:
                case TM59SpaceClassification.LivingRoomKitchen:
                case TM59SpaceClassification.Studio:
                    return true;
                default:
                    return false;
            }
        }

        private static int OccupancyForBedroomCount(int bedroomCount)
        {
            switch (bedroomCount)
            {
                case 1: return 2;
                case 2: return 3;
                case 3: return 4;
                default: return 0;
            }
        }

        /// <summary>True only for a finite, positive Area - never creates, updates, or infers one.</summary>
        private static bool TryGetArea(Space space, out double area)
        {
            area = 0;
            return space != null
                && space.TryGetValue(SpaceParameter.Area, out area)
                && !double.IsNaN(area)
                && !double.IsInfinity(area)
                && area > 0;
        }

        // Every per-area gain parameter TM59 conditions carry - not just Equipment Sensible Gain Per
        // Area. Almost every condition in the library, habitable and non-habitable alike (including
        // every apartment/bedroom size and e.g. TM59_Stairs), also carries a non-zero Lighting Gain Per
        // Area, so a Space with no valid Area needs the same NaN-safety guard for that too.
        private static readonly (InternalConditionParameter Parameter, string Label)[] PerAreaGainParameters =
        {
            (InternalConditionParameter.EquipmentSensibleGainPerArea, "Equipment Sensible Gain Per Area"),
            (InternalConditionParameter.LightingGainPerArea, "Lighting Gain Per Area"),
        };

        /// <summary>
        /// TM59 must never calculate, infer or write Area - it only validates what the model already has.
        /// Any non-zero per-area gain (equipment or lighting) applied to a Space with no valid Area would
        /// otherwise silently produce a NaN (or a fabricated) gain downstream; this surfaces a diagnostic
        /// instead, directing the modeller to the existing SAM commands that actually establish Area.
        /// Checked against every resolved condition, habitable or not.
        /// </summary>
        private static string ValidateAreaForPerAreaGains(Space space, InternalCondition condition)
        {
            if (condition == null || TryGetArea(space, out _))
                return null;

            List<string> nonZeroGainLabels = null;
            foreach ((InternalConditionParameter parameter, string label) in PerAreaGainParameters)
            {
                if (!condition.TryGetValue(parameter, out double gainPerArea) || double.IsNaN(gainPerArea) || gainPerArea == 0)
                    continue;

                (nonZeroGainLabels ??= new List<string>()).Add($"{label} ({gainPerArea:0.##} W/m²)");
            }

            if (nonZeroGainLabels == null)
                return null;

            return $"'{condition.Name}' has a non-zero {string.Join(" and ", nonZeroGainLabels)} but this Space has no valid Area - " +
                   "TM59 does not calculate or write Area. Run SAMAnalytical.Check then SAMAnalytical.CalculateFloorArea before exporting.";
        }

        /// <summary>
        /// Appends the per-area-gain diagnostic (if any) to whatever Resolve already decided, without
        /// disturbing a null InternalCondition (manual-review results are never validated - there is no
        /// condition to check a gain against) or an existing diagnostic (both are shown, space-separated).
        /// </summary>
        private static TM59InternalConditionResult ApplyAreaGainValidation(Space space, TM59InternalConditionResult result)
        {
            if (result?.InternalCondition == null)
                return result;

            string gainDiagnostic = ValidateAreaForPerAreaGains(space, result.InternalCondition);
            if (gainDiagnostic == null)
                return result;

            string diagnostic = string.IsNullOrWhiteSpace(result.Diagnostic) ? gainDiagnostic : result.Diagnostic + " " + gainDiagnostic;
            return new TM59InternalConditionResult(result.InternalCondition, result.Classification, result.Occupancy, result.BedroomCount, diagnostic);
        }

        /// <summary>
        /// Primary, unit-testable entry point. flatSpaces should include the flat's own spaces
        /// (space itself may or may not be included; both are handled). May be null/empty for
        /// zone-less non-habitable spaces.
        /// </summary>
        public TM59InternalConditionResult Resolve(Space space, IEnumerable<Space> flatSpaces)
        {
            return ApplyAreaGainValidation(space, ResolveCore(space, flatSpaces));
        }

        private TM59InternalConditionResult ResolveCore(Space space, IEnumerable<Space> flatSpaces)
        {
            if (space == null)
                return new TM59InternalConditionResult(null, TM59SpaceClassification.Undefined, 0, 0, "Space is null.");

            TM59SpaceClassification classification = Classify(space);

            if (classification == TM59SpaceClassification.NonHabitable)
            {
                string conditionName = matchedConditionNameCache.TryGetValue(space.Guid, out string cachedName) ? cachedName : null;
                InternalCondition nonHabitableCondition = internalConditionLibrary?.GetInternalConditions(conditionName)?.FirstOrDefault();
                string diagnostic = nonHabitableCondition == null
                    ? $"Matched non-habitable condition '{conditionName}' not found in the selected InternalConditionLibrary."
                    : null;

                return new TM59InternalConditionResult(nonHabitableCondition, classification, 0, 0, diagnostic);
            }

            List<Space> flatSpaces_Temp = (flatSpaces ?? Enumerable.Empty<Space>()).Where(x => x != null).ToList();
            if (flatSpaces_Temp.All(x => x.Guid != space.Guid))
                flatSpaces_Temp.Add(space);

            List<Space> bedroomSpaces = flatSpaces_Temp.Where(x => Classify(x) == TM59SpaceClassification.Bedroom).ToList();
            int bedroomCount = bedroomSpaces.Count;

            if (classification == TM59SpaceClassification.Undefined)
                return new TM59InternalConditionResult(null, classification, 0, bedroomCount, "No defensible automatic mapping - assign manually.");

            if (classification == TM59SpaceClassification.Studio)
                return ResolveNamedCondition(StudioConditionName, TM59SpaceClassification.Studio, 2, bedroomCount);

            if (classification == TM59SpaceClassification.LivingRoomKitchen && bedroomCount == 0)
            {
                List<Space> habitableSpaces = flatSpaces_Temp.Where(x => IsHabitable(Classify(x))).ToList();
                bool soleHabitableSpace = habitableSpaces.Count == 1 && habitableSpaces[0].Guid == space.Guid;

                // "No other HABITABLE space" is not the same guarantee as "every other space is
                // confirmed non-habitable" - an unrecognized room name (e.g. "Study", which classifies
                // as Undefined because it matches no TM59 keyword) would otherwise be silently excluded
                // from habitableSpaces and this space auto-promoted to Studio, even though that
                // unresolved room might actually be a separate bedroom the matcher just failed to
                // recognize. Require every other space to be POSITIVELY classified NonHabitable - an
                // Undefined one falls through to the generic "no identifiable bedroom" manual-review
                // diagnostic below instead.
                bool everyOtherSpaceConfirmedNonHabitable = flatSpaces_Temp
                    .Where(x => x.Guid != space.Guid)
                    .All(x => Classify(x) == TM59SpaceClassification.NonHabitable);

                if (soleHabitableSpace && everyOtherSpaceConfirmedNonHabitable)
                    return ResolveNamedCondition(StudioConditionName, TM59SpaceClassification.Studio, 2, 0,
                        "Classified as Studio: sole combined living/kitchen space with no separate bedroom in the flat.");
            }

            if (classification == TM59SpaceClassification.Bedroom)
                return ResolveBedroom(space, bedroomSpaces, bedroomCount);

            // LivingRoom / Kitchen / LivingRoomKitchen (not the zero-bedroom Studio case above)
            if (bedroomCount == 0)
                return new TM59InternalConditionResult(null, classification, 0, 0,
                    "Flat has no identifiable bedroom - cannot determine apartment size. Assign manually.");

            if (bedroomCount > 3)
                return new TM59InternalConditionResult(null, classification, 0, bedroomCount,
                    $"Flat has {bedroomCount} bedrooms; TM59 library defines apartment sizes only up to 3-bed. Assign manually.");

            string suffix;
            switch (classification)
            {
                case TM59SpaceClassification.LivingRoomKitchen:
                    suffix = "Bed Apt. Living Room/Kitchen";
                    break;
                case TM59SpaceClassification.Kitchen:
                    suffix = "Bed Apt. Kitchen";
                    break;
                default:
                    suffix = "Bed Apt. Living Room";
                    break;
            }

            string apartmentConditionName = $"{bedroomCount} {suffix}";
            return ResolveNamedCondition(apartmentConditionName, classification, OccupancyForBedroomCount(bedroomCount), bedroomCount);
        }

        private TM59InternalConditionResult ResolveNamedCondition(string conditionName, TM59SpaceClassification classification, int occupancy, int bedroomCount, string diagnostic = null)
        {
            InternalCondition internalCondition = internalConditionLibrary?.GetInternalConditions(conditionName)?.FirstOrDefault();
            if (internalCondition == null)
                diagnostic = $"Condition '{conditionName}' not found in the selected InternalConditionLibrary.";

            return new TM59InternalConditionResult(internalCondition, classification, occupancy, bedroomCount, diagnostic);
        }

        private TM59InternalConditionResult ResolveBedroom(Space space, List<Space> bedroomSpaces, int bedroomCount)
        {
            bool isDouble;
            string diagnostic = null;

            BedroomKeyword ownKeyword = explicitBedroomKeywordCache.TryGetValue(space.Guid, out BedroomKeyword cachedKeyword) ? cachedKeyword : BedroomKeyword.None;

            if (ownKeyword == BedroomKeyword.Double)
            {
                isDouble = true;
            }
            else if (ownKeyword == BedroomKeyword.Single)
            {
                isDouble = false;
            }
            else if (bedroomCount <= 1)
            {
                // TM59 convention: a flat's sole bedroom is the main/double bedroom.
                isDouble = true;
            }
            else
            {
                List<Space> explicitDouble = bedroomSpaces.Where(x => ExplicitKeyword(x) == BedroomKeyword.Double).ToList();
                List<Space> unspecified = bedroomSpaces.Where(x => ExplicitKeyword(x) == BedroomKeyword.None).ToList();

                if (explicitDouble.Any(x => x.Guid == space.Guid))
                {
                    isDouble = true;
                }
                else if (explicitDouble.Count > 0)
                {
                    // another bedroom already explicitly claims Double; this one defaults to Single.
                    isDouble = false;
                }
                else if (unspecified.Count == 0)
                {
                    // every bedroom explicitly named Single - respect that literally, no forced Double.
                    isDouble = false;
                }
                else
                {
                    int withValidArea = unspecified.Count(x => TryGetArea(x, out _));
                    Space main;

                    if (withValidArea == unspecified.Count)
                    {
                        // every unresolved bedroom has a valid Area - area-based selection is safe.
                        main = unspecified
                            .OrderByDescending(x => { TryGetArea(x, out double a); return a; })
                            .ThenBy(x => x.Name, StringComparer.Ordinal)
                            .ThenBy(x => x.Guid)
                            .First();
                    }
                    else if (withValidArea == 0)
                    {
                        main = unspecified
                            .OrderBy(x => x.Name, StringComparer.Ordinal)
                            .ThenBy(x => x.Guid)
                            .First();
                        diagnostic = "No area data - main/double bedroom chosen by stable name/Guid ordering, not by size.";
                    }
                    else
                    {
                        // Partial: some bedrooms have Area, some don't. A missing Area must never be
                        // treated as zero (that would silently bias the "largest" comparison), so this
                        // falls back to the same stable, area-independent ordering as the no-area case.
                        main = unspecified
                            .OrderBy(x => x.Name, StringComparer.Ordinal)
                            .ThenBy(x => x.Guid)
                            .First();
                        diagnostic = "Partial bedroom area data - some bedrooms in this flat are missing Area, so the " +
                            "main/double bedroom was chosen by stable name/Guid ordering rather than size. Run " +
                            "SAMAnalytical.Check then SAMAnalytical.CalculateFloorArea to complete the area data.";
                    }

                    isDouble = main.Guid == space.Guid;
                }
            }

            string conditionName = isDouble ? DoubleBedroomConditionName : SingleBedroomConditionName;
            InternalCondition internalCondition = internalConditionLibrary?.GetInternalConditions(conditionName)?.FirstOrDefault();
            if (internalCondition == null)
                diagnostic = $"Condition '{conditionName}' not found in the selected InternalConditionLibrary.";

            int occupancy = isDouble ? 2 : 1;
            return new TM59InternalConditionResult(internalCondition, TM59SpaceClassification.Bedroom, occupancy, bedroomCount, diagnostic);
        }

        private BedroomKeyword ExplicitKeyword(Space space)
        {
            Classify(space);
            return explicitBedroomKeywordCache.TryGetValue(space.Guid, out BedroomKeyword keyword) ? keyword : BedroomKeyword.None;
        }

        /// <summary>Convenience wrapper: resolves the flat from the zone of the given category.</summary>
        public TM59InternalConditionResult Resolve(AdjacencyCluster adjacencyCluster, Space space, string zoneCategory)
        {
            if (space == null)
                return new TM59InternalConditionResult(null, TM59SpaceClassification.Undefined, 0, 0, "Space is null.");

            // A missing AdjacencyCluster is treated exactly like "no zone found" below (null-conditional
            // GetZones), NOT as license to resolve the space alone as if it were a one-space flat -
            // otherwise a lone bedroom/living-kitchen would be silently promoted to Double Bedroom/Studio
            // with no zone at all to justify that "sole space in the flat" assumption. Only NonHabitable
            // spaces get the zone-less shortcut, since they need no zone/flat context either way.
            Zone zone = adjacencyCluster?.GetZones(space, zoneCategory)?.FirstOrDefault();
            if (zone == null)
            {
                // Non-habitable spaces need no zone at all (defect: corridors/bathrooms were previously
                // unreachable because GetInternalCondition returned null before any name matching ran).
                if (Classify(space) == TM59SpaceClassification.NonHabitable)
                    return Resolve(space, null);

                return new TM59InternalConditionResult(null, Classify(space), 0, 0,
                    string.IsNullOrWhiteSpace(zoneCategory)
                        ? "Space is not assigned to any zone - select a Zone Type to enable automatic mapping."
                        : $"Space is not assigned to a zone of category '{zoneCategory}'.");
            }

            // Read zone membership fresh every call, deliberately not memoized: TM59Manager reuses one
            // resolver instance across many Resolve calls, and a caller that adds/removes a Space from
            // this zone between two of those calls must see that change reflected immediately, not a
            // stale snapshot from the first call.
            List<Space> flatSpaces = adjacencyCluster.GetSpaces(zone) ?? new List<Space>();

            return Resolve(space, flatSpaces);
        }
    }
}
