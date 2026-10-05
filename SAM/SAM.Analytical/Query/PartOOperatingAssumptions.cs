// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The name under which base-provision opening behaviour is stated. <c>false</c> means openings are
        /// operated without restriction.
        /// <para>
        /// <b>These four names are identity-defining and permanent.</b> They participate in
        /// <c>OverheatingScenario.Key</c>, so renaming one re-keys every scenario that states it and orphans
        /// every result already attributed to those scenarios. Treat them as a published contract.
        /// </para>
        /// </summary>
        public const string OpeningsRestricted = "Openings Restricted";

        /// <summary>Whether mechanical ventilation runs continuously at its design rate. Identity-defining.</summary>
        public const string MechanicalVentilationAtDesignRate = "Mechanical Ventilation At Design Rate";

        /// <summary>Whether the mechanical system's boost state is available. Identity-defining.</summary>
        public const string BoostAvailable = "Boost Available";

        /// <summary>Whether the mechanical system's summer bypass state is available. Identity-defining.</summary>
        public const string SummerBypassAvailable = "Summer Bypass Available";

        /// <summary>The active cooling provision the dwelling is assessed with. Identity-defining.</summary>
        public const string ActiveCooling = "Active Cooling";

        /// <summary>
        /// The <see cref="ActiveCooling"/> value of the one cooling provision Part O materialises: the selected MVHR
        /// product's supply-air cooling, operated to its manufacturer's modelling guidance (cooling-stat, elevated
        /// airflow while cooling, exchanger-then-coil supply law). Identity-defining.
        /// </summary>
        public const string ActiveCooling_SupplyAirManufacturerGuidance = "Supply Air - Manufacturer Guidance";

        /// <summary>
        /// The canonical operating assumptions for an Approved Document O mitigation stage.
        /// <para>
        /// <b>Why this exists.</b> <see cref="PartOIteration"/> has always been identity-only - naming a stage
        /// was not stating it, so every caller had to hand-populate the assumptions that make the stage what it
        /// is. Two engineers stating "base passive" therefore derived two different scenario keys, which defeats
        /// the whole purpose of a derived key. This is the single place a stage's assumptions are written down,
        /// so a stage means one thing everywhere.
        /// </para>
        /// <para>
        /// <b>Still identity, not execution.</b> These assumptions say what was assumed; they do not make a
        /// simulation obey them. In particular, <see cref="OpeningsRestricted"/> and
        /// <see cref="MechanicalVentilationAtDesignRate"/> are properties of the <b>simulation inputs</b> - the
        /// aperture control profiles and ventilation rates in the TBD - and nothing here writes those. A
        /// BasePassive scenario states base provision and is assessed as base provision; making the model
        /// actually operate that way is the modeller's job today, and applying it to the export is separate,
        /// unwritten work. <b>Do not read a stage's presence here as a claim that the simulation enforced it.</b>
        /// </para>
        /// <para>
        /// <b>Declared policy, and it needs the engineer's confirmation.</b> The values below are read off
        /// <see cref="PartOIteration"/>'s own definitions of the stages; nothing in Approved Document O was
        /// parsed to produce them, and no shipped system template marks boost or summer bypass (a known,
        /// deferred gap). <see cref="PartOIteration.AcousticRestricted"/> may therefore state a
        /// summer bypass that no template can satisfy - that refusal belongs to capability selection, not here,
        /// because what a scenario <i>assumes</i> and what a system can <i>do</i> are two different statements
        /// and conflating them would hide the mismatch.
        /// </para>
        /// </summary>
        /// <param name="partOIteration">The mitigation stage.</param>
        /// <param name="refusal">
        /// Why no assumptions could be stated, or null on success. A stage whose assumptions are not written yet
        /// <b>refuses</b> rather than returning an empty set, because an empty set is a valid statement
        /// ("nothing assumed") and would silently derive a key as though the stage had been characterised.
        /// </param>
        /// <returns>
        /// The assumptions, or null where <paramref name="refusal"/> is set.
        /// <see cref="PartOIteration.Undefined"/> returns an empty set, which is the honest answer: no stage is
        /// stated, so nothing is assumed about one.
        /// </returns>
        public static OverheatingOperatingAssumptions PartOOperatingAssumptions(this PartOIteration partOIteration, out string refusal)
        {
            refusal = null;

            OverheatingOperatingAssumptions result = new();

            switch (partOIteration)
            {
                case PartOIteration.Undefined:
                    //Nothing stated. Not a refusal: a scenario built to exercise the machinery rather than to
                    //assess a building against a stage is entitled to say so.
                    return result;

                case PartOIteration.DwellingIndependent:
                    //The neutral identity of an assessed common space. Deliberately the empty set, and
                    //deliberately a member of its own rather than Undefined (see the enum): a corridor in a
                    //mixed model is ventilated by neither dwelling route, so it states no opening or
                    //mechanical assumption that either route would put in the key. Only valid at the
                    //CommonSpace scope - Create.OverheatingScenarios refuses it for a dwelling.
                    return result;

                case PartOIteration.BasePassive:
                    //"Openings operated without restriction, mechanical ventilation at its design continuous
                    //rate. Nothing has been added to mitigate overheating."
                    result.Set(OpeningsRestricted, false);
                    result.Set(MechanicalVentilationAtDesignRate, true);
                    result.Set(BoostAvailable, false);
                    result.Set(SummerBypassAvailable, false);

                    return result;

                case PartOIteration.BaseNaturalVentilation:
                    //Iteration 1b. The same base provision as BasePassive on the opening side, and its
                    //opposite on the mechanical side: there is no continuous mechanical supply or extract
                    //to run at a design rate, because the route says the dwelling has none.
                    //
                    //MechanicalVentilationAtDesignRate = FALSE is the whole reason this member exists. It
                    //is part of the derived scenario key, so it is not a display detail: it is the sentence
                    //every result attributed to this iteration permanently asserts about the building.
                    result.Set(OpeningsRestricted, false);
                    result.Set(MechanicalVentilationAtDesignRate, false);
                    result.Set(BoostAvailable, false);
                    result.Set(SummerBypassAvailable, false);

                    return result;

                case PartOIteration.AcousticRestricted:
                    //"Openings restricted for noise, with the mechanical system's boost and summer bypass states
                    //available to compensate."
                    result.Set(OpeningsRestricted, true);
                    result.Set(MechanicalVentilationAtDesignRate, true);
                    result.Set(BoostAvailable, true);
                    result.Set(SummerBypassAvailable, true);

                    return result;

                case PartOIteration.ActiveTrimCooling:
                    //An MVHR dwelling with the selected product's supply-air cooling (mixed strategies PR3, owner
                    //decision 27 Sep 2026). Each value states what the manufacturer-guidance Systems route actually
                    //simulates (SAM_Tas GroundGuidanceCooling), verified there rather than chosen to fill a key:
                    //- openings are operated as authored - the route changes no aperture;
                    //- the unit runs its design airflow continuously while the cooling-stat is satisfied, and the
                    //  product's elevated airflow only while it calls - so "at design rate" is the background state,
                    //  and the elevated airflow is part of the cooling provision, not a boost state of its own;
                    //- the unit's own bypass decision is modelled, independent of the cooling-stat;
                    //- the cooling provision is the product's manufacturer-guidance supply-air cooling.
                    result.Set(OpeningsRestricted, false);
                    result.Set(MechanicalVentilationAtDesignRate, true);
                    result.Set(BoostAvailable, false);
                    result.Set(SummerBypassAvailable, true);
                    result.Set(ActiveCooling, ActiveCooling_SupplyAirManufacturerGuidance);

                    return result;

                default:
                    refusal = string.Format("The operating assumptions for the '{0}' iteration are not known, so a scenario cannot state that stage.", partOIteration);

                    return null;
            }
        }
    }
}
