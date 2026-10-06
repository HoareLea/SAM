// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// One <b>requested</b> targeted design airflow, resolved to the closest value the dwelling and its
    /// <b>already selected</b> ventilation unit will actually carry - and the model that value produces,
    /// handed over only where one was found.
    /// <para>
    /// <b>This is a clamp, not an optimiser with an objective of its own.</b> An engineer asks for a room
    /// at 40 l/s. Either the dwelling can be designed that way, or it cannot, and the useful answer to the
    /// second case is not "no" - it is "36.4 l/s, and here is what stopped it at that". Answering only
    /// "no" would leave every caller to write this search again.
    /// </para>
    /// <code>
    /// requested 40.0  ->  achieved 36.4   IsRequestSatisfied false
    ///                                     LimitingReason: the selected MVHR-25 cannot carry the duty
    /// requested 22.2  ->  achieved 22.2   IsRequestSatisfied true
    /// </code>
    ///
    /// <para><b>What it never does</b></para>
    /// <para>
    /// It never moves the room PAST what was asked for, in either direction: a request is a ceiling on an
    /// increase and a floor on a reduction, so headroom that was not asked for is still never spent - the
    /// rule <see cref="DwellingDesignAirFlowCandidate.SupplyHeadroom_Lps"/> already states. It never moves
    /// the room the OTHER way from the request: a reduction that cannot be balanced is refused, not turned
    /// into an increase. And it never changes the selected product - see
    /// <see cref="DwellingDesignAirFlowCandidate.VentilationUnitSelectionOutcome"/>. The selected unit is
    /// the constraint being resolved within, and buying a bigger one is
    /// <see cref="Modify.SelectVentilationUnit"/>, called deliberately, on its own.
    /// </para>
    ///
    /// <para><b>Every answer is a real evaluated candidate</b></para>
    /// <para>
    /// <see cref="Candidate"/> is the actual <see cref="Modify.EvaluateTargetedDesignAirFlow"/> result that
    /// produced <see cref="Achieved_Lps"/> - not a value the search inferred and then trusted. So the model
    /// on <see cref="AdjacencyCluster"/> carries every Approved Document F floor, every balancing
    /// consequence and every capacity check that any candidate carries, because it <i>is</i> one. Adopting
    /// it is the commit, exactly as it is for a single candidate; there is deliberately no second call.
    /// </para>
    ///
    /// <para><b>The authority boundary is inherited unchanged</b></para>
    /// <code>
    /// PartFRequiredAirFlow  !=  DesignAirFlow  !=  SelectedEquipmentCapacity  !=  OperatingAirFlow
    /// </code>
    /// <para>
    /// The search moves design airflow only, and only in the one room it was pointed at. The Approved
    /// Document F requirement bounds it and is never written. The selected unit's capacity bounds it and
    /// never becomes a design airflow. Runtime/operating airflow is not touched at all.
    /// </para>
    /// </summary>
    public class DwellingDesignAirFlowResolution
    {
        /// <summary>The design airflow [l/s] that was asked for in the targeted room.</summary>
        public double Requested_Lps { get; internal set; } = double.NaN;

        /// <summary>
        /// The candidate that produced this answer - the one the search settled on where it found a
        /// feasible design, and the refused one that explains why where it did not.
        /// <para>
        /// Null only where the request was rejected before any candidate could be evaluated at all.
        /// </para>
        /// </summary>
        public DwellingDesignAirFlowCandidate Candidate { get; internal set; }

        /// <summary>
        /// The margin [l/s] this answer was resolved to - the same tolerance every Approved Document F
        /// floor, balance and capacity comparison beneath it was made against.
        /// </summary>
        public double Tolerance_Lps { get; internal set; } = double.NaN;

        /// <summary>How many candidates were evaluated to reach this answer.</summary>
        public int Evaluations { get; internal set; }

        /// <summary>
        /// What stopped the search short of <see cref="Requested_Lps"/>: the refusal of the tightest
        /// infeasible candidate it evaluated - the selected unit's capacity, an Approved Document F floor
        /// on the balancing side, or whatever else the engineering refused. Null where the request was met
        /// exactly.
        /// </summary>
        public string LimitingReason { get; internal set; }

        /// <summary>
        /// The design airflow [l/s] the targeted room actually reaches. NaN where nothing was feasible.
        /// <para>
        /// Always at or between the room's existing design airflow and <see cref="Requested_Lps"/>.
        /// </para>
        /// </summary>
        public double Achieved_Lps
        {
            get
            {
                return Candidate is null || !Candidate.IsAccepted ? double.NaN : Candidate.TargetedAdjustment.After_Lps;
            }
        }

        /// <summary>
        /// Whether the request was met exactly, within <see cref="Tolerance_Lps"/>.
        /// <para>
        /// <b>False is a normal, useful answer</b> - it means <see cref="Achieved_Lps"/> is the clamped
        /// value and <see cref="LimitingReason"/> says what clamped it. It is <i>not</i> the same as a
        /// refusal: check <see cref="IsAccepted"/> for that.
        /// </para>
        /// </summary>
        public bool IsRequestSatisfied
        {
            get
            {
                return IsAccepted && System.Math.Abs(Achieved_Lps - Requested_Lps) <= Tolerance_Lps;
            }
        }

        /// <summary>
        /// Whether adopting this answer would actually move the targeted room.
        /// <para>
        /// False where the room cannot be moved towards the request at all - the answer is then the design
        /// as it already stands, which is valid and adoptable but changes nothing.
        /// <see cref="LimitingReason"/> says why.
        /// </para>
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return IsAccepted && System.Math.Abs(Candidate.TargetedAdjustment.After_Lps - Candidate.TargetedAdjustment.Before_Lps) > Tolerance_Lps;
            }
        }

        /// <summary>
        /// Whether a feasible design was found and may be adopted - and therefore whether
        /// <see cref="AdjacencyCluster"/> is there to adopt.
        /// </summary>
        public bool IsAccepted
        {
            get
            {
                return Refusals.Count == 0 && Candidate is not null && Candidate.IsAccepted;
            }
        }

        /// <summary>
        /// The model this answer produces. <b>Null unless a feasible design was found</b>, exactly as
        /// <see cref="DwellingDesignAirFlowCandidate.AdjacencyCluster"/> is. Taking it is the commit.
        /// </summary>
        public AdjacencyCluster AdjacencyCluster
        {
            get
            {
                return IsAccepted ? Candidate.AdjacencyCluster : null;
            }
        }

        /// <summary>The one room the answer moves, and what it moves it from and to.</summary>
        public DesignAirFlowAdjustment TargetedAdjustment
        {
            get
            {
                return IsAccepted ? Candidate.TargetedAdjustment : null;
            }
        }

        /// <summary>The balancing changes that move derives on the opposite side. Never a second target.</summary>
        public List<DesignAirFlowAdjustment> DerivedAdjustments
        {
            get
            {
                return IsAccepted ? Candidate.DerivedAdjustments : [];
            }
        }

        /// <summary>The dwelling's design supply duty before the answer, on the caller's model.</summary>
        public double SupplyDuty_Before_Lps
        {
            get
            {
                return Candidate is null ? double.NaN : Candidate.SupplyDuty_Before_Lps;
            }
        }

        /// <summary>The dwelling's design extract duty before the answer, on the caller's model.</summary>
        public double ExtractDuty_Before_Lps
        {
            get
            {
                return Candidate is null ? double.NaN : Candidate.ExtractDuty_Before_Lps;
            }
        }

        /// <summary>The dwelling's design supply duty the answer would produce.</summary>
        public double SupplyDuty_After_Lps
        {
            get
            {
                return IsAccepted ? Candidate.SupplyDuty_After_Lps : double.NaN;
            }
        }

        /// <summary>The dwelling's design extract duty the answer would produce.</summary>
        public double ExtractDuty_After_Lps
        {
            get
            {
                return IsAccepted ? Candidate.ExtractDuty_After_Lps : double.NaN;
            }
        }

        /// <summary>The dwelling being rebalanced, where one resolved.</summary>
        public VentilationSystem VentilationSystem
        {
            get
            {
                return Candidate?.VentilationSystem;
            }
        }

        /// <summary>The air handling unit serving it, where one resolved.</summary>
        public AirHandlingUnit AirHandlingUnit
        {
            get
            {
                return Candidate?.AirHandlingUnit;
            }
        }

        /// <summary>
        /// The product that unit is <b>currently</b> selected as. Never changed by resolving an airflow.
        /// </summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return Candidate?.VentilationUnitReference;
            }
        }

        /// <summary>What that product can move, where the catalogue offered to the search describes it.</summary>
        public VentilationUnitCapacityDescriptor VentilationUnitCapacityDescriptor
        {
            get
            {
                return Candidate?.VentilationUnitCapacityDescriptor;
            }
        }

        /// <summary>
        /// What the selected product would have left on the supply side had this answer been adopted.
        /// <b>Reported, never spent</b> - the request is the ceiling, not the rating.
        /// </summary>
        public double SupplyHeadroom_Lps
        {
            get
            {
                return Candidate is null ? double.NaN : Candidate.SupplyHeadroom_Lps;
            }
        }

        /// <summary>The same on the extract side. See <see cref="SupplyHeadroom_Lps"/>.</summary>
        public double ExtractHeadroom_Lps
        {
            get
            {
                return Candidate is null ? double.NaN : Candidate.ExtractHeadroom_Lps;
            }
        }

        /// <summary>What the search and the candidate it settled on found worth saying.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Design headroom and similar - legal, and not a reason to reject anything.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>
        /// Why <b>nothing at all</b> was feasible. Empty where an answer was found, including where that
        /// answer fell short of the request - a clamped answer is not a refusal, and
        /// <see cref="LimitingReason"/> carries that instead.
        /// </summary>
        public List<string> Refusals { get; } = [];
    }
}
