// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    /// <summary>
    /// One reusable ventilation unit product offered to a selection, and the most air it can move.
    /// <para>
    /// <b>Capability, and nothing else.</b> The two figures here are what the equipment is able to do.
    /// They are never a duty, never a requirement and never a design airflow - a unit able to move
    /// 150 l/s does not thereby mean the dwelling moves 150 l/s, and nothing in this assembly may write
    /// a capacity into either of the other two. Iteration 2 exists to keep those four quantities apart.
    /// </para>
    /// <para>
    /// <b>Supply and extract are independent, and both must be sufficient.</b> They are held separately
    /// for exactly the reason <c>Query.VentilationSystemDesignDuty</c> reports them separately: a
    /// balanced heat recovery system balances at the system, not room by room, and a product whose two
    /// sides are not the same rating is an ordinary thing. A candidate is compliant only when
    /// <see cref="MaximumSupplyFlowRate_Lps"/> covers the supply duty <b>and</b>
    /// <see cref="MaximumExtractFlowRate_Lps"/> covers the extract duty.
    /// </para>
    /// <para>
    /// <b>The lightweight thing a selection reads, and it is handed in.</b> This is the same seam
    /// <see cref="SystemCapabilityDescriptor"/> established: <c>SAM.Analytical</c> owns the vocabulary
    /// and the selection rule, and which products exist is a fact about whoever is asking. So the core
    /// library carries no manufacturer list, adding a product needs no change here, and choosing a unit
    /// opens no file.
    /// </para>
    /// <para>
    /// <b>Not an <c>IJSAMObject</c>, deliberately</b> - the same decision, and the same reasoning, as
    /// <see cref="SystemCapabilityDescriptor"/>. Nothing needs to serialise a descriptor: the catalogue
    /// is the wire format and the assembly that owns the catalogue parses it. What the <i>model</i>
    /// stores is <see cref="VentilationUnitReference"/>, which does serialise.
    /// </para>
    /// </summary>
    public class VentilationUnitCapacityDescriptor
    {
        private readonly VentilationUnitReference ventilationUnitReference;
        private readonly double maximumSupplyFlowRate_Lps;
        private readonly double maximumExtractFlowRate_Lps;
        private readonly int rank;

        public VentilationUnitCapacityDescriptor(VentilationUnitReference ventilationUnitReference, double maximumSupplyFlowRate_Lps, double maximumExtractFlowRate_Lps, int rank = 0)
        {
            //Copied: VentilationUnitReference is mutable, and a descriptor a selector is reading must not
            //change underneath it.
            this.ventilationUnitReference = ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference);
            this.maximumSupplyFlowRate_Lps = maximumSupplyFlowRate_Lps;
            this.maximumExtractFlowRate_Lps = maximumExtractFlowRate_Lps;
            this.rank = rank;
        }

        public VentilationUnitCapacityDescriptor(VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor)
        {
            if (ventilationUnitCapacityDescriptor is not null)
            {
                ventilationUnitReference = ventilationUnitCapacityDescriptor.ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitCapacityDescriptor.ventilationUnitReference);
                maximumSupplyFlowRate_Lps = ventilationUnitCapacityDescriptor.maximumSupplyFlowRate_Lps;
                maximumExtractFlowRate_Lps = ventilationUnitCapacityDescriptor.maximumExtractFlowRate_Lps;
                rank = ventilationUnitCapacityDescriptor.rank;
            }
        }

        /// <summary>The product's identity. A copy.</summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference);
            }
        }

        /// <summary>The most supply air [l/s] the product can move.</summary>
        public double MaximumSupplyFlowRate_Lps
        {
            get
            {
                return maximumSupplyFlowRate_Lps;
            }
        }

        /// <summary>The most extract air [l/s] the product can move.</summary>
        public double MaximumExtractFlowRate_Lps
        {
            get
            {
                return maximumExtractFlowRate_Lps;
            }
        }

        /// <summary>
        /// Where this product sits in the preference order of whoever supplied it. <b>Lower is
        /// preferred</b>, and it is <b>only ever a tie-break</b>: size decides first, because "the
        /// smallest unit that is big enough" is the engineering rule and it is not a catalogue's to
        /// override. Rank separates two products that are the same size on both sides, where the
        /// catalogue is the only thing that can say which one is meant.
        /// <para>
        /// Supplied, never inferred - the same rule as <see cref="SystemCapabilityDescriptor.Rank"/>.
        /// </para>
        /// </summary>
        public int Rank
        {
            get
            {
                return rank;
            }
        }

        /// <summary>
        /// Whether the descriptor names a product and states a usable capacity on both sides.
        /// <para>
        /// A negative or non-finite capacity is invalid rather than "very small": a unit that moves
        /// negative air is a broken catalogue entry, and letting it through would make it compliant with
        /// nothing and quietly reduce the field of candidates. Zero is valid and simply never sufficient
        /// for a duty above zero.
        /// </para>
        /// </summary>
        public bool IsValid
        {
            get
            {
                return ventilationUnitReference is not null
                    && ventilationUnitReference.IsValid
                    && IsUsable(maximumSupplyFlowRate_Lps)
                    && IsUsable(maximumExtractFlowRate_Lps);
            }
        }

        /// <summary>
        /// Whether this product can move both duties, each side checked against its own capacity.
        /// <para>
        /// <b>Never a "nearest" comparison.</b> A unit is compliant or it is not; an absolute distance
        /// would rank a unit 5 l/s too small above one 10 l/s too big and select equipment that cannot
        /// do the job.
        /// </para>
        /// </summary>
        /// <param name="tolerance_Lps">
        /// The margin in l/s within which a duty counts as met - so a unit rated at exactly the duty is
        /// sufficient rather than failing on a rounding bit. A flow-rate literal rather than a borrowed
        /// distance tolerance, following <c>Query.PartFSystemCapabilityRequirement</c>.
        /// </param>
        public bool IsSufficientFor(double supplyDuty_Lps, double extractDuty_Lps, double tolerance_Lps = 0.001)
        {
            //An unusable tolerance cannot show sufficiency, so it does not. Nor can a duty that is not a
            //real, non-negative quantity of air: a NEGATIVE duty would otherwise be met by every capacity,
            //and this predicate is what gates equipment selection. False is the safe answer to both - see
            //Query.IsValidFlowRateTolerance.
            return Query.IsValidFlowRateTolerance(tolerance_Lps)
                && IsUsable(supplyDuty_Lps)
                && IsUsable(extractDuty_Lps)
                && IsValid
                && maximumSupplyFlowRate_Lps + tolerance_Lps >= supplyDuty_Lps
                && maximumExtractFlowRate_Lps + tolerance_Lps >= extractDuty_Lps;
        }

        /// <summary>
        /// This product as an <b>engineer-facing</b> label: its identity and its two maximum airflows.
        /// <para>
        /// <b>Deliberately without the rank</b>, which <see cref="ToString"/> carries and which this is
        /// here to leave out. <see cref="Rank"/> is an internal catalogue tie-breaker - not an airflow,
        /// not an efficiency, not a performance score and not an engineering output - and a product
        /// picker that prints it invites an engineer to read a preference number as a rating. So the
        /// picker shows this and the diagnostics show <see cref="ToString"/>.
        /// </para>
        /// <para>
        /// The capacity stays <i>in</i> the label, because choosing a unit by hand is choosing a capacity
        /// and the alternative is reading the identity here and the rating three columns away.
        /// </para>
        /// </summary>
        public string Label
        {
            get
            {
                return string.Format(
                    "{0} — {1:0.###} / {2:0.###} l/s",
                    ventilationUnitReference is null ? "-" : ventilationUnitReference.ToString(),
                    maximumSupplyFlowRate_Lps,
                    maximumExtractFlowRate_Lps);
            }
        }

        /// <summary>
        /// The scalar this assembly reads as "how big is it" when choosing the smallest compliant
        /// product: the two sides added.
        /// <para>
        /// <b>Why a sum, and what it is not.</b> Size over two independent axes is a partial order, and
        /// something has to make it total or the answer depends on list order. The sum is the honest
        /// reading of the plant a unit represents - both fans - and it never decides <i>compliance</i>,
        /// which <see cref="IsSufficientFor"/> checks side by side. A unit whose sum is smaller but whose
        /// supply side is too small is not a candidate at all and is never compared.
        /// </para>
        /// </summary>
        public double Size_Lps
        {
            get
            {
                return maximumSupplyFlowRate_Lps + maximumExtractFlowRate_Lps;
            }
        }

        /// <summary>
        /// Orders two descriptors by size, then by the catalogue's rank, then by product identity.
        /// <para>
        /// Deterministic and independent of the order the descriptors arrived in, so a catalogue read
        /// from a directory cannot let the file system choose a dwelling's plant.
        /// </para>
        /// </summary>
        public static int Compare(VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_1, VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_2)
        {
            if (ventilationUnitCapacityDescriptor_1 is null || ventilationUnitCapacityDescriptor_2 is null)
            {
                return (ventilationUnitCapacityDescriptor_1 is null ? 0 : 1) - (ventilationUnitCapacityDescriptor_2 is null ? 0 : 1);
            }

            int result = ventilationUnitCapacityDescriptor_1.Size_Lps.CompareTo(ventilationUnitCapacityDescriptor_2.Size_Lps);
            if (result != 0)
            {
                return result;
            }

            //Two products of the same total size but split differently between the sides - 150/100 and
            //100/150 - are genuinely different equipment, so the supply side separates them before the
            //catalogue's own preference is consulted.
            result = ventilationUnitCapacityDescriptor_1.maximumSupplyFlowRate_Lps.CompareTo(ventilationUnitCapacityDescriptor_2.maximumSupplyFlowRate_Lps);
            if (result != 0)
            {
                return result;
            }

            //CompareTo, not subtraction: ranks come from a file, and int.MaxValue - int.MinValue overflows
            //to a wrong sign, which both mis-orders the list and can make List.Sort throw.
            result = ventilationUnitCapacityDescriptor_1.rank.CompareTo(ventilationUnitCapacityDescriptor_2.rank);

            return result != 0 ? result : CompareIdentity(ventilationUnitCapacityDescriptor_1, ventilationUnitCapacityDescriptor_2);
        }

        /// <summary>Orders two descriptors by product identity alone.</summary>
        public static int CompareIdentity(VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_1, VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_2)
        {
            return VentilationUnitReference.Compare(ventilationUnitCapacityDescriptor_1?.ventilationUnitReference, ventilationUnitCapacityDescriptor_2?.ventilationUnitReference);
        }

        public override string ToString()
        {
            return string.Format(
                "{0} [supply {1:0.###} l/s, extract {2:0.###} l/s] rank {3}",
                ventilationUnitReference is null ? "-" : ventilationUnitReference.ToString(),
                maximumSupplyFlowRate_Lps,
                maximumExtractFlowRate_Lps,
                rank);
        }

        private static bool IsUsable(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
        }
    }
}
