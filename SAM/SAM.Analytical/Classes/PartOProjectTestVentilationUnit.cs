// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One optional <b>project test</b> ventilation unit: a made-up product, stated in this project alone,
    /// that an engineer can size a dwelling against without inventing a manufacturer catalogue entry.
    ///
    /// <para><b>What it is for</b></para>
    /// <para>
    /// The shipped catalogue states real products at real ratings. Asking "what would a 165 l/s unit do
    /// here" against that catalogue means either editing shipped manufacturer data - which is a
    /// transcription of somebody's published document and must not be edited to answer a what-if - or
    /// adding a fictional Nuaire entry, which would then be indistinguishable from real data in every
    /// report the project produces. This type is the third answer: a capacity the project owns, obviously
    /// synthetic, and confined to the project that states it.
    /// </para>
    ///
    /// <para><b>Deliberately not a catalogue editor</b></para>
    /// <para>
    /// A name and two maximum airflows. No performance table, no control curve, no source document, no
    /// second product. Everything a real catalogue entry carries is exactly what a what-if does not have,
    /// and adding fields here is how this becomes a product-management feature nobody asked for.
    /// </para>
    ///
    /// <para><b>Identity is synthetic and says so</b></para>
    /// <para>
    /// <see cref="VentilationUnitReference"/> is derived, never stored: manufacturer
    /// <see cref="Manufacturer"/> - the literal words "Project test" - and model
    /// <see cref="SAMObject.Name"/>. So an assignment made against it reads as "Project test Test unit"
    /// wherever a product identity is printed, and cannot be mistaken for something a manufacturer
    /// supplied. It also means <b>the name is the identity</b>: renaming this product renames what a
    /// dwelling is fitted with, which is why the UI locks the name while any dwelling is assigned to it.
    /// </para>
    ///
    /// <para><b>Capability, and only capability</b></para>
    /// <para>
    /// This is a capability statement, exactly like a catalogue descriptor, and it is read the same way -
    /// through <c>Query.VentilationUnitTemplate</c> and then the one existing
    /// <c>Query.CapacityDescriptor</c> mapping. It is <b>not</b> stored on an air handling unit, which
    /// holds an identity and never a capacity, and <b>not</b> inside
    /// <see cref="PartOEquipmentSelection"/>, which holds identities and never a capacity either. The
    /// three quantities Part O keeps apart stay apart: a Part F requirement, a design airflow, and what
    /// the equipment can move.
    /// </para>
    ///
    /// <para><b>Where it lives, and why on the project</b></para>
    /// <para>
    /// Stamped on the <see cref="AnalyticalModel"/> as
    /// <c>AnalyticalModelParameter.PartOProjectTestVentilationUnit</c>, beside
    /// <see cref="PartOEquipmentSelection"/> and <see cref="PartOIsolationContext"/>. It has to persist
    /// with the project: a manual assignment made against it must still resolve its capacity after the
    /// project is reopened - otherwise a saved dwelling comes back as "capacity unknown" and an Iteration
    /// 2B ceiling is lost - and it must not persist any wider than that, or one project's what-if would
    /// silently size another project's dwellings. An application setting would do exactly that, which is
    /// why this is not one.
    /// </para>
    ///
    /// <para><b>It is not part of "all catalogue products"</b></para>
    /// <para>
    /// <see cref="PartOEquipmentSelectionMode.AutomaticAllProducts"/> means the shipped manufacturer
    /// catalogue and continues to mean exactly that - see
    /// <see cref="PartOEquipmentSelection.CandidateDescriptors(System.Collections.Generic.IEnumerable{VentilationUnitCapacityDescriptor}, System.Collections.Generic.IEnumerable{VentilationUnitCapacityDescriptor})"/>,
    /// which is where that rule is written down. A project test product takes part only where the
    /// engineer has said so: ticked into a selected pool, or picked by hand.
    /// </para>
    /// </summary>
    public class PartOProjectTestVentilationUnit : SAMObject
    {
        /// <summary>
        /// The manufacturer field every project test product identifies itself by. Not a manufacturer -
        /// that is the point. Held as a constant because it is an identity, and an identity that varied
        /// by locale or by caller would orphan the assignments made against it.
        /// </summary>
        public const string Manufacturer = "Project test";

        /// <summary>
        /// The catalogue rank a project test product is offered at.
        /// <para>
        /// Deliberately far beyond any shipped rank, so a test product of the same size as a real one
        /// <b>loses</b> the tie rather than making it ambiguous: <c>Query.CapableVentilationUnits</c>
        /// refuses two products that are the same size AND the same rank, and orders by rank before
        /// identity otherwise. A what-if must never turn a working selection into a refusal, and must
        /// never quietly outrank real equipment.
        /// </para>
        /// </summary>
        internal const int Rank = 1000000;

        /// <summary>
        /// What a project test product states as its source. <c>VentilationUnitTemplate.IsValid</c>
        /// requires one, and this is the honest answer: nobody published these figures.
        /// </summary>
        internal const string Source = "Project test product stated in this project. Not manufacturer data and not traceable to a published document.";

        public PartOProjectTestVentilationUnit()
        {
        }

        public PartOProjectTestVentilationUnit(string name, double maximumSupplyFlowRate_Lps, double maximumExtractFlowRate_Lps)
            : base(name)
        {
            MaximumSupplyFlowRate_Lps = maximumSupplyFlowRate_Lps;
            MaximumExtractFlowRate_Lps = maximumExtractFlowRate_Lps;
        }

        public PartOProjectTestVentilationUnit(PartOProjectTestVentilationUnit partOProjectTestVentilationUnit)
            : base(partOProjectTestVentilationUnit)
        {
            if (partOProjectTestVentilationUnit is not null)
            {
                MaximumSupplyFlowRate_Lps = partOProjectTestVentilationUnit.MaximumSupplyFlowRate_Lps;
                MaximumExtractFlowRate_Lps = partOProjectTestVentilationUnit.MaximumExtractFlowRate_Lps;
            }
        }

        public PartOProjectTestVentilationUnit(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>
        /// The most supply air [l/s] this test unit is to be treated as able to move.
        /// <para>
        /// <see cref="double.NaN"/> until stated, following <c>VentilationUnitTemplate</c>: unresolved is
        /// a state and not a zero. A test product without both figures states no capability and is not
        /// offered to anything - see <see cref="Refusal"/>.
        /// </para>
        /// </summary>
        public double MaximumSupplyFlowRate_Lps { get; set; } = double.NaN;

        /// <summary>
        /// The most extract air [l/s] this test unit is to be treated as able to move. Independent of
        /// <see cref="MaximumSupplyFlowRate_Lps"/>, because a differently rated pair of sides is ordinary
        /// equipment and one figure covering both would approve a duty one side cannot move.
        /// </summary>
        public double MaximumExtractFlowRate_Lps { get; set; } = double.NaN;

        /// <summary>Whether this states a usable test product at all.</summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Name)
                    && IsUsableCapacity(MaximumSupplyFlowRate_Lps)
                    && IsUsableCapacity(MaximumExtractFlowRate_Lps);
            }
        }

        /// <summary>
        /// The identity a dwelling assigned this product stores, derived from <see cref="Manufacturer"/>
        /// and <see cref="SAMObject.Name"/>. Null where nothing usable is stated, so an unusable test
        /// product cannot be assigned to anything.
        /// </summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return IsValid ? new VentilationUnitReference(Manufacturer, Name, null) : null;
            }
        }

        /// <summary>
        /// Why this test product is not usable, or null where it is. One sentence, so a dialog can say
        /// what is wrong rather than merely disabling something.
        /// </summary>
        public string Refusal
        {
            get
            {
                if (IsValid)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(Name))
                {
                    return "A project test product needs a name: the name is what a dwelling assigned to it stores, so an unnamed one identifies nothing.";
                }

                bool supply = !IsUsableCapacity(MaximumSupplyFlowRate_Lps);
                bool extract = !IsUsableCapacity(MaximumExtractFlowRate_Lps);

                string sides = supply && extract
                    ? "a maximum supply and a maximum extract airflow"
                    : (supply ? "a maximum supply airflow" : "a maximum extract airflow");

                return string.Format(
                    "The project test product '{0}' needs {1} stated as a positive, finite number of litres per second before anything can be sized against it.",
                    Name,
                    sides);
            }
        }

        /// <summary>
        /// Whether two statements describe the same test product - the same identity <b>and</b> the same
        /// two capacities.
        /// <para>
        /// Capacity is part of it, unlike <see cref="PartOEquipmentSelection.Matches"/> where capacities
        /// are not held at all. A prepared Part O run records the capability lookup it was prepared
        /// against, and re-rating a test product from 165 to 175 l/s changes what an Iteration 2B ceiling
        /// is - so a preparation made under the old rating must not be reused for the new one. Nothing
        /// would fail; the answer would just be about different equipment than the screen claimed.
        /// </para>
        /// </summary>
        /// <param name="partOProjectTestVentilationUnit">The other statement. Null matches nothing.</param>
        public bool Matches(PartOProjectTestVentilationUnit partOProjectTestVentilationUnit)
        {
            if (partOProjectTestVentilationUnit is null)
            {
                return false;
            }

            //Ordinal, and on the identity field rather than on the guid: a test product read back from a
            //project file is a different instance stating the same product.
            if (!string.Equals(partOProjectTestVentilationUnit.Name, Name, System.StringComparison.Ordinal))
            {
                return false;
            }

            //Equals, so that two unresolved (NaN) capacities compare as the same unresolved state rather
            //than as unequal - which == would say, and which would refuse reuse forever.
            return partOProjectTestVentilationUnit.MaximumSupplyFlowRate_Lps.Equals(MaximumSupplyFlowRate_Lps)
                && partOProjectTestVentilationUnit.MaximumExtractFlowRate_Lps.Equals(MaximumExtractFlowRate_Lps);
        }

        public override string ToString()
        {
            return IsValid
                ? string.Format("{0} {1} [supply {2:0.###} l/s, extract {3:0.###} l/s] - project test", Manufacturer, Name, MaximumSupplyFlowRate_Lps, MaximumExtractFlowRate_Lps)
                : string.Format("{0} - project test, unusable", string.IsNullOrWhiteSpace(Name) ? "-" : Name);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            MaximumSupplyFlowRate_Lps = PerformanceJson.Value(jsonObject, "MaximumSupplyFlowRate_Lps");
            MaximumExtractFlowRate_Lps = PerformanceJson.Value(jsonObject, "MaximumExtractFlowRate_Lps");

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return null;
            }

            //Omitted rather than written as null where unresolved - PerformanceJson reads absent and
            //unusable back as the same NaN, so one state survives the round trip as one state.
            PerformanceJson.SetValue(result, "MaximumSupplyFlowRate_Lps", MaximumSupplyFlowRate_Lps);
            PerformanceJson.SetValue(result, "MaximumExtractFlowRate_Lps", MaximumExtractFlowRate_Lps);

            return result;
        }

        /// <summary>
        /// A capacity has to be a real, finite, positive quantity of air.
        /// <para>
        /// Zero is refused here, unlike on a manufacturer template where it is a legal rating that is
        /// simply never sufficient. A project test product exists to be sized against, and one stated at
        /// zero is a half-finished entry rather than a statement about a fan.
        /// </para>
        /// </summary>
        private static bool IsUsableCapacity(double value_Lps)
        {
            return !double.IsNaN(value_Lps) && !double.IsInfinity(value_Lps) && value_Lps > 0;
        }
    }
}
