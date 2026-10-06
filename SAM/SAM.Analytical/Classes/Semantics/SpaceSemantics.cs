// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The shared semantic classification of one space: what the space is, plus the independent
    /// semantic roles that follow from it, plus where the classification came from.
    /// <para>
    /// One enum cannot represent every concept because a room can hold several roles at once - a
    /// studio is simultaneously habitable, bedroom-equivalent and a cooking space - so
    /// <see cref="SpaceUse"/> carries the single primary identity and the boolean properties carry
    /// the independent roles. The roles are derived from the use by one convention table in
    /// <see cref="Create.SpaceSemantics(Analytical.SpaceUse, SpaceSemanticsSource, string, string)"/>
    /// so that Approved Document F, Approved Document O, CIBSE TM59 and SAM_UI cannot drift apart.
    /// </para>
    /// <para>
    /// This class describes the space only. It holds no flow rates, no thresholds and no assessment
    /// criteria - those stay in the standard-specific layer.
    /// </para>
    /// </summary>
    public class SpaceSemantics : SAMObject
    {
        public SpaceSemantics()
        {
        }

        public SpaceSemantics(SpaceSemantics spaceSemantics)
            : base(spaceSemantics)
        {
            if (spaceSemantics is not null)
            {
                SpaceUse = spaceSemantics.SpaceUse;
                Source = spaceSemantics.Source;
                MatchedAlias = spaceSemantics.MatchedAlias;
                Diagnostic = spaceSemantics.Diagnostic;
                IsDwellingSpace = spaceSemantics.IsDwellingSpace;
                IsHabitable = spaceSemantics.IsHabitable;
                IsBedroomEquivalent = spaceSemantics.IsBedroomEquivalent;
                IsLivingSpace = spaceSemantics.IsLivingSpace;
                IsCookingSpace = spaceSemantics.IsCookingSpace;
                IsWetRoom = spaceSemantics.IsWetRoom;
                IsCirculation = spaceSemantics.IsCirculation;
                IsCommunal = spaceSemantics.IsCommunal;
                HasSupplyRole = spaceSemantics.HasSupplyRole;
                HasExtractRole = spaceSemantics.HasExtractRole;
                SpaceUse_Name = spaceSemantics.SpaceUse_Name;
                SpaceUse_InternalCondition = spaceSemantics.SpaceUse_InternalCondition;
                HasSourceConflict = spaceSemantics.HasSourceConflict;
            }
        }

        /// <summary>
        /// Records what each classification source independently resolved to, so both survive even where
        /// only one of them won. Called by <see cref="SpaceSemanticsResolver"/> once it has evaluated both
        /// sources; not part of the convention table, which derives only the semantic flags.
        /// </summary>
        internal void SetSources(SpaceUse spaceUse_Name, SpaceUse spaceUse_InternalCondition, bool hasSourceConflict)
        {
            SpaceUse_Name = spaceUse_Name;
            SpaceUse_InternalCondition = spaceUse_InternalCondition;
            HasSourceConflict = hasSourceConflict;
        }

        public SpaceSemantics(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        internal SpaceSemantics(
            SpaceUse spaceUse,
            SpaceSemanticsSource source,
            string matchedAlias,
            string diagnostic,
            bool isDwellingSpace,
            bool isHabitable,
            bool isBedroomEquivalent,
            bool isLivingSpace,
            bool isCookingSpace,
            bool isWetRoom,
            bool isCirculation,
            bool isCommunal,
            bool hasSupplyRole,
            bool hasExtractRole)
            : base(Core.Query.Description(spaceUse))
        {
            SpaceUse = spaceUse;
            Source = source;
            MatchedAlias = matchedAlias;
            Diagnostic = diagnostic;
            IsDwellingSpace = isDwellingSpace;
            IsHabitable = isHabitable;
            IsBedroomEquivalent = isBedroomEquivalent;
            IsLivingSpace = isLivingSpace;
            IsCookingSpace = isCookingSpace;
            IsWetRoom = isWetRoom;
            IsCirculation = isCirculation;
            IsCommunal = isCommunal;
            HasSupplyRole = hasSupplyRole;
            HasExtractRole = hasExtractRole;
        }

        /// <summary>The primary identity of the space.</summary>
        public SpaceUse SpaceUse { get; private set; } = SpaceUse.Undefined;

        /// <summary>Which resolution source produced this classification, and so why it won.</summary>
        public SpaceSemanticsSource Source { get; private set; } = SpaceSemanticsSource.None;

        /// <summary>
        /// The configured synonym or InternalCondition name that matched, for traceability. Null
        /// where the classification came from an override or nothing matched.
        /// </summary>
        public string MatchedAlias { get; private set; }

        /// <summary>Why the space could not be classified, or a note about a noteworthy choice.</summary>
        public string Diagnostic { get; private set; }

        /// <summary>
        /// The space use the space NAME resolved to, kept even where a higher-priority source won, so
        /// neither source value is lost. <see cref="SpaceUse.Undefined"/> where the name resolved to
        /// nothing.
        /// </summary>
        public SpaceUse SpaceUse_Name { get; private set; } = SpaceUse.Undefined;

        /// <summary>
        /// The space use the space's INTERNAL CONDITION name resolved to, kept even where the name won, so
        /// neither source value is lost. <see cref="SpaceUse.Undefined"/> where there is no internal
        /// condition or it resolved to nothing.
        /// </summary>
        public SpaceUse SpaceUse_InternalCondition { get; private set; } = SpaceUse.Undefined;

        /// <summary>
        /// True where the space name and the internal condition each resolved to a DIFFERENT space use.
        /// The higher-priority source (the name) is used, both values are preserved above, and the
        /// conflict is reported in <see cref="Diagnostic"/> and surfaced in SAM_UI so the engineer can
        /// override it. Neither source is silently overwritten.
        /// </summary>
        public bool HasSourceConflict { get; private set; }

        /// <summary>
        /// False only where the space is positively identified as outside every dwelling - communal
        /// circulation and explicitly non-dwelling spaces. An unclassified space is not assumed to be
        /// outside the dwelling.
        /// </summary>
        public bool IsDwellingSpace { get; private set; }

        /// <summary>
        /// A room used for dwelling purposes but not <i>solely</i> a kitchen, utility room, bathroom,
        /// cellar or sanitary accommodation (Approved Document F, Volume 1, 2021 edition, Appendix A).
        /// An open plan living kitchen is therefore habitable; a room that is solely a kitchen is not.
        /// </summary>
        public bool IsHabitable { get; private set; }

        /// <summary>
        /// Counts as one bedroom for whole dwelling sizing. True for a bedroom and for a studio,
        /// which combines sleeping with living and cooking in one room.
        /// </summary>
        public bool IsBedroomEquivalent { get; private set; }

        /// <summary>Contains the living function.</summary>
        public bool IsLivingSpace { get; private set; }

        /// <summary>
        /// Contains the cooking function, so Approved Document F, Volume 1 (2021 edition) paragraph
        /// 1.17a and Table 1.2 require kitchen extract from it. True for a kitchen, an open plan
        /// living kitchen and a studio.
        /// </summary>
        public bool IsCookingSpace { get; private set; }

        /// <summary>
        /// A room used for domestic activities that produce significant airborne moisture, plus
        /// sanitary accommodation (Approved Document F, Volume 1, 2021 edition, Appendix A).
        /// <para>
        /// Deliberately false for a studio and an open plan living kitchen even though both contain
        /// the cooking function: the SAM design convention treats them as habitable supply spaces, so
        /// their cooking function is carried by <see cref="IsCookingSpace"/> instead. See the Part F
        /// kitchen extract limitation documented on PartFCalculator.
        /// </para>
        /// </summary>
        public bool IsWetRoom { get; private set; }

        /// <summary>Circulation, whether inside a dwelling or shared between dwellings.</summary>
        public bool IsCirculation { get; private set; }

        /// <summary>Shared between dwellings, so outside any one dwelling.</summary>
        public bool IsCommunal { get; private set; }

        /// <summary>
        /// Takes a mechanical supply terminal. Approved Document F, Volume 1 (2021 edition)
        /// paragraph 1.67 requires mechanical supply to each habitable room.
        /// </summary>
        public bool HasSupplyRole { get; private set; }

        /// <summary>
        /// Takes a mechanical extract terminal. Approved Document F, Volume 1 (2021 edition)
        /// paragraph 1.70 requires continuous mechanical extract from each wet room.
        /// </summary>
        public bool HasExtractRole { get; private set; }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject.ContainsKey("SpaceUse"))
            {
                SpaceUse = Core.Query.Enum<SpaceUse>(jsonObject["SpaceUse"]?.GetValue<string>());
            }

            if (jsonObject.ContainsKey("Source"))
            {
                Source = Core.Query.Enum<SpaceSemanticsSource>(jsonObject["Source"]?.GetValue<string>());
            }

            if (jsonObject.ContainsKey("MatchedAlias"))
            {
                MatchedAlias = jsonObject["MatchedAlias"]?.GetValue<string>();
            }

            if (jsonObject.ContainsKey("Diagnostic"))
            {
                Diagnostic = jsonObject["Diagnostic"]?.GetValue<string>();
            }

            if (jsonObject.ContainsKey("SpaceUse_Name"))
            {
                SpaceUse_Name = Core.Query.Enum<SpaceUse>(jsonObject["SpaceUse_Name"]?.GetValue<string>());
            }

            if (jsonObject.ContainsKey("SpaceUse_InternalCondition"))
            {
                SpaceUse_InternalCondition = Core.Query.Enum<SpaceUse>(jsonObject["SpaceUse_InternalCondition"]?.GetValue<string>());
            }

            HasSourceConflict = Boolean(jsonObject, "HasSourceConflict", HasSourceConflict);

            IsDwellingSpace = Boolean(jsonObject, "IsDwellingSpace", IsDwellingSpace);
            IsHabitable = Boolean(jsonObject, "IsHabitable", IsHabitable);
            IsBedroomEquivalent = Boolean(jsonObject, "IsBedroomEquivalent", IsBedroomEquivalent);
            IsLivingSpace = Boolean(jsonObject, "IsLivingSpace", IsLivingSpace);
            IsCookingSpace = Boolean(jsonObject, "IsCookingSpace", IsCookingSpace);
            IsWetRoom = Boolean(jsonObject, "IsWetRoom", IsWetRoom);
            IsCirculation = Boolean(jsonObject, "IsCirculation", IsCirculation);
            IsCommunal = Boolean(jsonObject, "IsCommunal", IsCommunal);
            HasSupplyRole = Boolean(jsonObject, "HasSupplyRole", HasSupplyRole);
            HasExtractRole = Boolean(jsonObject, "HasExtractRole", HasExtractRole);

            return true;
        }

        private static bool Boolean(JsonObject jsonObject, string name, bool @default)
        {
            if (!jsonObject.ContainsKey(name))
            {
                return @default;
            }

            return jsonObject[name]?.GetValue<bool>() ?? @default;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            result["SpaceUse"] = SpaceUse.ToString();
            result["Source"] = Source.ToString();

            if (MatchedAlias is not null)
            {
                result["MatchedAlias"] = MatchedAlias;
            }

            if (Diagnostic is not null)
            {
                result["Diagnostic"] = Diagnostic;
            }

            result["SpaceUse_Name"] = SpaceUse_Name.ToString();
            result["SpaceUse_InternalCondition"] = SpaceUse_InternalCondition.ToString();
            result["HasSourceConflict"] = HasSourceConflict;

            result["IsDwellingSpace"] = IsDwellingSpace;
            result["IsHabitable"] = IsHabitable;
            result["IsBedroomEquivalent"] = IsBedroomEquivalent;
            result["IsLivingSpace"] = IsLivingSpace;
            result["IsCookingSpace"] = IsCookingSpace;
            result["IsWetRoom"] = IsWetRoom;
            result["IsCirculation"] = IsCirculation;
            result["IsCommunal"] = IsCommunal;
            result["HasSupplyRole"] = HasSupplyRole;
            result["HasExtractRole"] = HasExtractRole;

            return result;
        }
    }
}
