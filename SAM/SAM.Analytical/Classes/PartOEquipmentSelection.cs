// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// A project's Approved Document O <b>equipment preselection</b>: how ventilation units are to be
    /// chosen (<see cref="Mode"/>) and, where the engineer has narrowed it, which products are permitted
    /// to be chosen from (<see cref="AllowedVentilationUnitReferences"/>).
    ///
    /// <para><b>A candidate constraint, never an assignment</b></para>
    /// <para>
    /// <c>AllowedVentilationUnitReferences != SelectedVentilationUnitReference</c>. Nothing here says any
    /// dwelling is fitted with anything. What a dwelling is fitted with is
    /// <c>AirHandlingUnitParameter.VentilationUnitReference</c> on that dwelling's own air handling unit,
    /// which is the engineering fact and the only thing an optimisation, a report or a reopened project
    /// reads. This object only bounds what an automatic selection was allowed to offer, and what a manual
    /// picker normally lists.
    /// </para>
    ///
    /// <para><b>Identities only - no capacities</b></para>
    /// <para>
    /// The pool holds <see cref="VentilationUnitReference"/> values and never a maximum airflow. Copying
    /// capacities in here would put a number meaning "equipment capability" into a saved project beside
    /// numbers meaning "design duty" and "regulatory requirement", and it would go stale the day the
    /// catalogue is corrected. <see cref="VentilationUnitReference"/> says why at length; the capacity is
    /// always looked up, through <see cref="CandidateDescriptors"/> and
    /// <see cref="AllowedDescriptors"/>.
    /// </para>
    ///
    /// <para><b>Why this is project configuration and not model engineering</b></para>
    /// <para>
    /// It is stamped on the <see cref="AnalyticalModel"/> as
    /// <c>AnalyticalModelParameter.PartOEquipmentSelection</c> - the project's metadata, beside
    /// <see cref="PartOIsolationContext"/>, which is Part O workflow state recorded exactly the same way
    /// and for the same reason. It is deliberately <b>not</b> on a space, a system or an air handling
    /// unit: none of those is where a project-wide procurement preference belongs. It is equally
    /// deliberately not in <c>ActiveSetting</c>, which is process-global and would carry one project's
    /// pool into the next.
    /// </para>
    ///
    /// <para><b>Absent means the historic default</b></para>
    /// <para>
    /// A model that carries no such parameter reads as <see cref="PartOEquipmentSelectionMode.AutomaticAllProducts"/>
    /// with an empty pool, which is exactly what Iteration 2 did before this type existed. There is no
    /// migration.
    /// </para>
    /// </summary>
    public class PartOEquipmentSelection : SAMObject
    {
        private readonly List<VentilationUnitReference> ventilationUnitReferences_Allowed = [];

        public PartOEquipmentSelection()
        {
        }

        /// <param name="partOEquipmentSelectionMode">The selection authority - see <see cref="Mode"/>.</param>
        /// <param name="ventilationUnitReferences_Allowed">
        /// The permitted products, by identity. Null or empty is "nothing has been narrowed", which
        /// <see cref="PartOEquipmentSelectionMode.AutomaticSelectedPool"/> and
        /// <see cref="PartOEquipmentSelectionMode.ManualPerDwelling"/> read differently and correctly -
        /// see those members.
        /// </param>
        public PartOEquipmentSelection(PartOEquipmentSelectionMode partOEquipmentSelectionMode, IEnumerable<VentilationUnitReference> ventilationUnitReferences_Allowed = null)
        {
            Mode = partOEquipmentSelectionMode;

            foreach (VentilationUnitReference ventilationUnitReference in ventilationUnitReferences_Allowed ?? [])
            {
                //Copied on the way in, and again on the way out. A pool that handed out the caller's own
                //objects would let a product be renamed through a list nobody thought was writable.
                if (ventilationUnitReference is not null && ventilationUnitReference.IsValid)
                {
                    this.ventilationUnitReferences_Allowed.Add(new VentilationUnitReference(ventilationUnitReference));
                }
            }
        }

        public PartOEquipmentSelection(PartOEquipmentSelection partOEquipmentSelection)
            : base(partOEquipmentSelection)
        {
            if (partOEquipmentSelection is not null)
            {
                Mode = partOEquipmentSelection.Mode;

                foreach (VentilationUnitReference ventilationUnitReference in partOEquipmentSelection.ventilationUnitReferences_Allowed)
                {
                    ventilationUnitReferences_Allowed.Add(new VentilationUnitReference(ventilationUnitReference));
                }
            }
        }

        public PartOEquipmentSelection(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>The selection authority. See <see cref="PartOEquipmentSelectionMode"/>.</summary>
        public PartOEquipmentSelectionMode Mode { get; set; } = PartOEquipmentSelectionMode.AutomaticAllProducts;

        /// <summary>
        /// The permitted products, by identity, as a copy. Order is the engineer's and carries no meaning:
        /// <see cref="Query.CapableVentilationUnits"/> orders candidates by capacity and rank, so a pool
        /// listed the other way round selects identically.
        /// </summary>
        public List<VentilationUnitReference> AllowedVentilationUnitReferences => ventilationUnitReferences_Allowed.ConvertAll(x => new VentilationUnitReference(x));

        /// <summary>Whether the engineer has narrowed the catalogue at all.</summary>
        public bool HasAllowedVentilationUnitReferences => ventilationUnitReferences_Allowed.Count != 0;

        /// <summary>
        /// Whether an automatic selection rule is to be run at all. False for
        /// <see cref="PartOEquipmentSelectionMode.ManualPerDwelling"/>, where the engineer is the
        /// authority and preparation is given no catalogue to select from.
        /// </summary>
        public bool IsAutomatic => Mode != PartOEquipmentSelectionMode.ManualPerDwelling;

        /// <summary>Whether <paramref name="ventilationUnitReference"/> is one of the permitted products.</summary>
        public bool IsAllowed(VentilationUnitReference ventilationUnitReference)
        {
            if (ventilationUnitReference is null)
            {
                return false;
            }

            //An un-narrowed pool permits everything - see AllowedDescriptors for the one mode where that
            //is not so, and why.
            if (!HasAllowedVentilationUnitReferences)
            {
                return Mode != PartOEquipmentSelectionMode.AutomaticSelectedPool;
            }

            return ventilationUnitReferences_Allowed.Find(x => x.Matches(ventilationUnitReference)) is not null;
        }

        /// <summary>
        /// Whether two preselections say the same thing - the same authority over the same permitted
        /// products, whatever order they were listed in.
        ///
        /// <para><b>Why this exists</b></para>
        /// <para>
        /// A prepared Part O run records the preselection it was prepared under, and Prepare &amp; Run may
        /// reuse that preparation instead of repeating it. Reuse has to be refused when the engineer has
        /// since changed the mode or the pool, or the run would be simulated with the products the OLD
        /// configuration chose while the dialog reported the new one. That is a silent wrong answer, so the
        /// comparison belongs with the type rather than being restated by whoever needs it.
        /// </para>
        /// <para>
        /// <b>Order carries no meaning</b>, here or in selection: a pool ticked in the other order is the
        /// same pool, and comparing lists positionally would refuse reuse for nothing.
        /// </para>
        /// </summary>
        /// <param name="partOEquipmentSelection">The other statement. Null matches nothing.</param>
        public bool Matches(PartOEquipmentSelection partOEquipmentSelection)
        {
            if (partOEquipmentSelection is null || partOEquipmentSelection.Mode != Mode)
            {
                return false;
            }

            if (partOEquipmentSelection.ventilationUnitReferences_Allowed.Count != ventilationUnitReferences_Allowed.Count)
            {
                return false;
            }

            foreach (VentilationUnitReference ventilationUnitReference in ventilationUnitReferences_Allowed)
            {
                if (partOEquipmentSelection.ventilationUnitReferences_Allowed.Find(x => x.Matches(ventilationUnitReference)) is null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The candidate set an <b>automatic</b> selection is to run over, drawn from the catalogue handed
        /// in - never from anything stored here.
        /// <para>
        /// <see cref="PartOEquipmentSelectionMode.AutomaticAllProducts"/> is the whole catalogue;
        /// <see cref="PartOEquipmentSelectionMode.AutomaticSelectedPool"/> is the catalogue narrowed to the
        /// pool, <b>which may be empty</b> - <see cref="Query.SelectSmallestCapableVentilationUnit"/> then
        /// refuses by name, and this method never widens an empty pool back to the catalogue;
        /// <see cref="PartOEquipmentSelectionMode.ManualPerDwelling"/> is <c>null</c>, meaning "run no
        /// rule", which is exactly what <c>Modify.PreparePartOIteration</c> reads a null catalogue as.
        /// </para>
        /// </summary>
        /// <param name="ventilationUnitCapacityDescriptors">The selectable catalogue.</param>
        /// <returns>The candidates, or null where no rule is to be run.</returns>
        public List<VentilationUnitCapacityDescriptor> CandidateDescriptors(IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            if (Mode == PartOEquipmentSelectionMode.ManualPerDwelling)
            {
                return null;
            }

            List<VentilationUnitCapacityDescriptor> result = [];

            foreach (VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor in ventilationUnitCapacityDescriptors ?? [])
            {
                if (ventilationUnitCapacityDescriptor is null)
                {
                    continue;
                }

                if (Mode == PartOEquipmentSelectionMode.AutomaticAllProducts || IsInPool(ventilationUnitCapacityDescriptor.VentilationUnitReference))
                {
                    result.Add(ventilationUnitCapacityDescriptor);
                }
            }

            return result;
        }

        /// <summary>
        /// The products a <b>manual</b> per-dwelling picker normally offers, and the set a suggestion for
        /// an insufficient assignment is drawn from.
        /// <para>
        /// This differs from <see cref="CandidateDescriptors"/> in one place only, and deliberately. Under
        /// <see cref="PartOEquipmentSelectionMode.ManualPerDwelling"/> an empty pool means the engineer has
        /// made no preselection, not that nothing is permitted, so the whole catalogue is offered - an
        /// empty picker would make the mode unusable, and no selection rule is being run for it to bias.
        /// Under <see cref="PartOEquipmentSelectionMode.AutomaticSelectedPool"/> an empty pool stays empty,
        /// because there the engineer <i>did</i> say "only what I permit".
        /// </para>
        /// <para>
        /// A product already assigned to a dwelling but no longer in the pool is <b>not</b> added here.
        /// That belongs to the dwelling, not to the project, so the per-dwelling assignment model adds it
        /// to that one row's candidates and flags it. This method knows nothing about dwellings.
        /// </para>
        /// </summary>
        public List<VentilationUnitCapacityDescriptor> AllowedDescriptors(IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            List<VentilationUnitCapacityDescriptor> result = [];

            bool all = Mode == PartOEquipmentSelectionMode.AutomaticAllProducts
                || (Mode == PartOEquipmentSelectionMode.ManualPerDwelling && !HasAllowedVentilationUnitReferences);

            foreach (VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor in ventilationUnitCapacityDescriptors ?? [])
            {
                if (ventilationUnitCapacityDescriptor is null)
                {
                    continue;
                }

                if (all || IsInPool(ventilationUnitCapacityDescriptor.VentilationUnitReference))
                {
                    result.Add(ventilationUnitCapacityDescriptor);
                }
            }

            return result;
        }

        /// <summary>
        /// The candidate set an <b>automatic</b> selection is to run over, where the project also states a
        /// project test product.
        ///
        /// <para><b>This overload is where one rule lives, and it is the whole of the test product's automatic semantics</b></para>
        /// <para>
        /// <see cref="PartOEquipmentSelectionMode.AutomaticAllProducts"/> means <b>the shipped
        /// manufacturer catalogue and nothing else</b>. A project test product is a what-if somebody
        /// typed, and letting it into "all catalogue products" would silently change what that mode has
        /// always meant: a project would select a made-up unit because a test capacity happened to be left
        /// enabled, and every historic answer would move. So under that mode the test descriptors are not
        /// offered at all, and the behaviour is identical to what it was before this feature existed.
        /// </para>
        /// <para>
        /// Under <see cref="PartOEquipmentSelectionMode.AutomaticSelectedPool"/> they are offered, and then
        /// the pool decides: the engineer has to have ticked the test product for it to be a candidate,
        /// which is exactly the explicit act this feature requires. Under
        /// <see cref="PartOEquipmentSelectionMode.ManualPerDwelling"/> no rule runs and the answer is
        /// null, as ever.
        /// </para>
        /// <para>
        /// <b>Nothing is stored.</b> Both sequences are parameters, as in the one-argument overload - this
        /// type holds identities and never a capacity, and a project test product is a capacity.
        /// </para>
        /// </summary>
        /// <param name="ventilationUnitCapacityDescriptors">The shipped manufacturer catalogue.</param>
        /// <param name="ventilationUnitCapacityDescriptors_ProjectTest">
        /// What the project's own test product contributes - normally none or one, from
        /// <c>Query.CapacityDescriptors(PartOProjectTestVentilationUnit)</c>.
        /// </param>
        /// <returns>The candidates, or null where no rule is to be run.</returns>
        public List<VentilationUnitCapacityDescriptor> CandidateDescriptors(IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest)
        {
            //The one place the rule is written: an all-products selection is offered the catalogue alone.
            return CandidateDescriptors(Mode == PartOEquipmentSelectionMode.AutomaticAllProducts
                ? ventilationUnitCapacityDescriptors
                : Offered(ventilationUnitCapacityDescriptors, ventilationUnitCapacityDescriptors_ProjectTest));
        }

        /// <summary>
        /// The products a <b>manual</b> per-dwelling picker offers, where the project also states a project
        /// test product - which it may pick, because picking by hand is the engineer's decision and a test
        /// capacity they have stated is one of the things they may decide on.
        /// <para>
        /// The pool still applies in the pooled mode, and an un-narrowed manual pool still offers
        /// everything - see the one-argument overload, which decides both. This adds only whether the test
        /// product is among the things being offered, and under
        /// <see cref="PartOEquipmentSelectionMode.AutomaticAllProducts"/> it is not: there the rows of the
        /// table are an automatic rule's results, and the rule was never offered it.
        /// </para>
        /// </summary>
        public List<VentilationUnitCapacityDescriptor> AllowedDescriptors(IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest)
        {
            return AllowedDescriptors(Mode == PartOEquipmentSelectionMode.AutomaticAllProducts
                ? ventilationUnitCapacityDescriptors
                : Offered(ventilationUnitCapacityDescriptors, ventilationUnitCapacityDescriptors_ProjectTest));
        }

        /// <summary>
        /// The two sequences as one, catalogue first. A concatenation and nothing more - the decision
        /// about whether the second one takes part at all is made by the callers above, once each, so
        /// that the rule is readable where it is stated rather than hidden in a helper.
        /// </summary>
        private static List<VentilationUnitCapacityDescriptor> Offered(IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest)
        {
            List<VentilationUnitCapacityDescriptor> result = [.. ventilationUnitCapacityDescriptors ?? []];

            result.AddRange(ventilationUnitCapacityDescriptors_ProjectTest ?? []);

            return result;
        }

        /// <summary>
        /// The same configuration with <see cref="Mode"/> set to
        /// <see cref="PartOEquipmentSelectionMode.ManualPerDwelling"/> and the pool preserved exactly.
        /// <para>
        /// This is the whole of "Convert to Manual" as far as the project configuration is concerned: a
        /// change of authority. No dwelling's assignment is read, recalculated or written - the identities
        /// already on the air handling units are preserved by not being touched.
        /// </para>
        /// </summary>
        public PartOEquipmentSelection Manual()
        {
            return new PartOEquipmentSelection(PartOEquipmentSelectionMode.ManualPerDwelling, ventilationUnitReferences_Allowed);
        }

        public override string ToString()
        {
            string mode = Core.Query.Description(Mode);

            return HasAllowedVentilationUnitReferences
                ? string.Format("{0} ({1} permitted product(s))", mode, ventilationUnitReferences_Allowed.Count)
                : mode;
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            ventilationUnitReferences_Allowed.Clear();

            //An unreadable or unrecognised mode is the historic default rather than a refusal: this is
            //project configuration, and a project whose preference cannot be read has no preference.
            Mode = jsonObject["Mode"] is not null && System.Enum.TryParse(jsonObject["Mode"].ToString(), out PartOEquipmentSelectionMode partOEquipmentSelectionMode)
                ? partOEquipmentSelectionMode
                : PartOEquipmentSelectionMode.AutomaticAllProducts;

            if (jsonObject["AllowedVentilationUnitReferences"] is JsonArray jsonArray)
            {
                foreach (JsonNode jsonNode in jsonArray)
                {
                    if (jsonNode is not JsonObject jsonObject_VentilationUnitReference)
                    {
                        continue;
                    }

                    VentilationUnitReference ventilationUnitReference = new(jsonObject_VentilationUnitReference);

                    if (ventilationUnitReference.IsValid)
                    {
                        ventilationUnitReferences_Allowed.Add(ventilationUnitReference);
                    }
                }
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return null;
            }

            result["Mode"] = Mode.ToString();

            JsonArray jsonArray = [];
            foreach (VentilationUnitReference ventilationUnitReference in ventilationUnitReferences_Allowed)
            {
                JsonObject jsonObject = ventilationUnitReference.ToJsonObject();
                if (jsonObject is not null)
                {
                    jsonArray.Add(jsonObject);
                }
            }

            result["AllowedVentilationUnitReferences"] = jsonArray;

            return result;
        }

        private bool IsInPool(VentilationUnitReference ventilationUnitReference)
        {
            return ventilationUnitReference is not null && ventilationUnitReferences_Allowed.Find(x => x.Matches(ventilationUnitReference)) is not null;
        }
    }
}
