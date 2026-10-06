// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Builds the <b>derived</b> model that simulates only <paramref name="spaces"/> as thermal zones,
        /// keeping the thermal boundaries and the solar context the selection needs - or refuses, saying
        /// why.
        ///
        /// <para><b>The geometry is not decided here</b></para>
        /// <para>
        /// The extraction and the whole panel classification are
        /// <see cref="AdjacencyCluster.Filter(IEnumerable{Space}, bool)"/>'s - the production authority the
        /// <c>SAMAnalytical.FilterBySpaces</c> component has always called, which already returns a new
        /// cluster (the source is never touched), carries space guids across unchanged, and classifies every
        /// panel by comparing what it is adjacent to in the DERIVED cluster against what it was adjacent to
        /// in the SOURCE:
        /// </para>
        /// <list type="bullet">
        /// <item><b>selected to selected</b> - two spaces in the derived cluster, so it is left as the ordinary internal partition it is.</item>
        /// <item><b>selected to excluded</b> - one space in the derived cluster but two in the source, so it is the isolation cut and is marked <c>PanelParameter.Adiabatic</c> and <c>PanelParameter.IsolationCut</c>. An air boundary on the cut is additionally re-typed from its normal, because it has nothing left to open onto.</item>
        /// <item><b>selected to outside</b> - one space in both, so it is genuinely external and is left exactly as it is: its type, construction, apertures, orientation and exposure all unchanged.</item>
        /// </list>
        /// <para>
        /// This method adds only what <c>Filter</c> has no way to know about, because it is a general
        /// geometric filter and these are questions about a <i>simulation</i>: the plant graph, the airflow
        /// network, the apertures on the cut, and the surrounding geometry that still casts shade.
        /// </para>
        ///
        /// <para><b>Fail closed on shared plant and on airflow crossing the cut</b></para>
        /// <para>
        /// Checked <b>before</b> anything is built. A ventilation system or air handling unit that also
        /// serves excluded spaces cannot be carried into an isolated model without either simulating the
        /// spaces it was supposed to leave out, or quietly keeping a whole unit's duty while dropping the
        /// branches that justified it. Proportionally splitting shared central plant is a different
        /// engineering problem and is deliberately not attempted here; the selection is refused instead. The
        /// dedicated per-dwelling MVHR the Part O workflow builds is unaffected - it serves exactly one
        /// dwelling, so it is never shared.
        /// </para>
        ///
        /// <para><b>The source model is never modified.</b></para>
        /// </summary>
        /// <param name="adjacencyCluster">The whole building. Read only.</param>
        /// <param name="spaces">The spaces to isolate. Matched by guid - never by name.</param>
        /// <returns>The isolated cluster, or the refusals. Never both.</returns>
        public static SpaceIsolation IsolateSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces)
        {
            List<string> refusals = [];
            List<string> notes = [];

            if (adjacencyCluster is null)
            {
                refusals.Add("There is no model to isolate a selection from.");

                return new SpaceIsolation(null, refusals, notes, 0, 0, 0);
            }

            // ---- The selection, by identity ------------------------------------------------------------
            //
            // Resolved against the source cluster rather than trusting the instances handed in, so a caller
            // holding a stale copy of a space cannot select something the model no longer contains. The set
            // is the membership authority for every test below: one hash lookup per question, never a scan
            // of the model per selected space, which is what keeps this linear on a 5,000 space building.

            HashSet<Guid> guids_Selected = [];
            List<Space> spaces_Selected = [];

            foreach (Space space in spaces ?? [])
            {
                if (space is null)
                {
                    continue;
                }

                Space space_Source = adjacencyCluster.GetObject<Space>(space.Guid);
                if (space_Source is null)
                {
                    continue;
                }

                if (guids_Selected.Add(space_Source.Guid))
                {
                    spaces_Selected.Add(space_Source);
                }
            }

            if (spaces_Selected.Count == 0)
            {
                refusals.Add("None of the selected dwellings resolved to a space in this model, so there is nothing to simulate in isolation.");

                return new SpaceIsolation(null, refusals, notes, 0, 0, 0);
            }

            // ---- Fail closed BEFORE building anything --------------------------------------------------

            refusals.AddRange(Refusals_VentilationScope(adjacencyCluster, guids_Selected));
            refusals.AddRange(Refusals_AirMovementScope(adjacencyCluster, guids_Selected));

            if (refusals.Count != 0)
            {
                return new SpaceIsolation(null, refusals, notes, 0, 0, 0);
            }

            // ---- The extraction: the existing authority, unchanged -------------------------------------

            AdjacencyCluster result = adjacencyCluster.Filter(spaces_Selected, true);
            if (result is null)
            {
                refusals.Add("The selected dwellings could not be extracted from the model.");

                return new SpaceIsolation(null, refusals, notes, 0, 0, 0);
            }

            // ---- The plant the selection depends on -----------------------------------------------------
            //
            // An air handling unit is related to no space at all - a ventilation system names it in
            // VentilationSystemParameter.SupplyUnitName - so nothing about it is reachable from a space and
            // Filter cannot carry it. Without this the isolated model would have systems with no unit, and
            // every duty and equipment selection downstream would silently read as absent.
            //
            // BEFORE RestoreRelations, and that order is load-bearing: this adds the plant OBJECTS and
            // nothing else, so they have to be in the derived cluster by the time the one authority on
            // relations walks it. See CarryAirHandlingUnits for what went wrong when the two were split
            // the other way round.

            notes.AddRange(CarryAirHandlingUnits(adjacencyCluster, result));

            // ---- Relations Filter does not rebuild ------------------------------------------------------
            //
            // Filter relates each carried object to the SPACE it was carried for, which is all a geometric
            // filter needs. A simulation needs the rest of the graph too - a terminal to its system, an air
            // movement to its unit - so every source relation BETWEEN TWO CARRIED OBJECTS is restored. Only
            // carried objects are walked, so this costs the isolated model and not the building.

            RestoreRelations(adjacencyCluster, result);

            // ---- The apertures on the cut ---------------------------------------------------------------

            int count_Adiabatic = 0;
            int count_ApertureRemoved = 0;

            foreach (Panel panel in result.GetPanels() ?? [])
            {
                //The isolation cut is the ADJACENCY CHANGE this run made - two spaces in the source and
                //one in the derived cluster - and is asked of the two clusters directly, never of a flag.
                //
                //Not Query.Adiabatic, which reports any zero thickness construction as adiabatic in its
                //own right; and, less obviously, not PanelParameter.Adiabatic either. That flag is not
                //this run's evidence: Convert.ToSAM sets it when a TBD says a surface is adiabatic and
                //when a gbXML surface names an adjacent space the file does not contain, and the
                //SAMAnalyticalSetAdiabatic component sets it because a person asked for it. Filter
                //carries a panel's parameters into the derived cluster with it, so every one of those
                //arrives here already flagged - and reading the flag would take a wall that was adiabatic
                //in the whole building for a cut, strip the apertures it is entitled to keep, and count
                //it in the disclosure note as an interface to a space it does not touch.
                //
                //Asked this way round it also stays right where the two coincide: a source surface that
                //was already adiabatic AND divides a selected space from an excluded one IS a cut, and
                //does lose its apertures, because in the derived model it has nothing left to open onto.
                if (panel is null || !result.External(panel))
                {
                    continue;
                }

                //A cut THIS run made - or one the model was handed already carrying.
                //
                //The second case is not only re-isolation. AdjacencyCluster.Filter is a public API and
                //SAMAnalytical.FilterBySpaces calls it on its own: it marks the cut but strips nothing,
                //because removing a person's openings is not what a geometric filter is for. A model
                //arriving from that component therefore has cuts that still carry their doors, and on the
                //second question below no adjacency comparison can find them - they are external on both
                //sides of it. Counting such a panel and then leaving its door on it would be the worst of
                //the three outcomes: the disclosure would say the interface was treated as adiabatic while
                //the derived model still opened onto the omitted space. A carried cut gets the same
                //treatment as a fresh one, and where its apertures are already gone the work below simply
                //finds none.
                //
                //The carried case is read from PanelParameter.IsolationCut, which Filter wrote when it made
                //the cut, and NOT from PanelParameter.Adiabatic: that one an import or a person sets too,
                //and reading it would take an authored adiabatic external wall for an interface to a space
                //it does not touch, strip the openings it is entitled to keep, and count it in the note.
                if (!adjacencyCluster.Internal(panel) && !Query.IsolationCut(panel))
                {
                    continue;
                }

                count_Adiabatic++;

                List<Aperture> apertures = panel.Apertures;
                if (apertures is null || apertures.Count == 0)
                {
                    continue;
                }

                // The cut stands for an omitted conditioned neighbour, and TAS represents that by nulling
                // the surface's link (SAM_Tas Modify.UpdateAdiabatic sets tbdNullLink). An aperture left on
                // it has nothing to open onto: in the derived model the panel has one adjacent space, so the
                // conversion would export the opening as an EXTERNAL window and TAS would give the flat a
                // door to outside, with solar gain and outside air behind it, where a corridor used to be.
                // That is a fabricated boundary, so the aperture is removed from the DERIVED panel. The
                // source model still has its door.
                count_ApertureRemoved += apertures.Count;

                Panel panel_Cut = Create.Panel(panel);
                panel_Cut.RemoveApertures();

                result.AddObject(panel_Cut);
            }

            if (count_ApertureRemoved != 0)
            {
                notes.Add(string.Format("{0} aperture(s) on the isolation cut were removed from the derived model, because an opening onto an omitted space would be simulated as an opening to outside. The source model is unchanged.", count_ApertureRemoved));
            }

            // ---- The surrounding building, as shade and nothing else -----------------------------------

            int count_Shade = AddShadingContext(adjacencyCluster, result);

            notes.Add(string.Format(
                "Thermal model scope: ISOLATED. {0} space(s) simulated; {1} interface(s) to excluded spaces treated as adiabatic; {2} excluded external surface(s) retained as shading context.",
                spaces_Selected.Count,
                count_Adiabatic,
                count_Shade));

            return new SpaceIsolation(result, refusals, notes, count_Adiabatic, count_Shade, count_ApertureRemoved);
        }

        /// <summary>
        /// The same isolation over a whole <see cref="AnalyticalModel"/>, so the derived model keeps the
        /// material library, the profile library, the location and the model level parameters the
        /// simulation reads. The source model is never modified.
        /// </summary>
        public static SpaceIsolation IsolateSpaces(this AnalyticalModel analyticalModel, IEnumerable<Space> spaces)
        {
            if (analyticalModel is null)
            {
                return new SpaceIsolation(null, ["There is no model to isolate a selection from."], [], 0, 0, 0);
            }

            return IsolateSpaces(analyticalModel.AdjacencyCluster, spaces);
        }

        /// <summary>
        /// Every ventilation system and air handling unit that serves a selected space, checked for whether
        /// it <b>also</b> serves an excluded one.
        /// <para>
        /// One pass over the systems, each asking its own related spaces - the set decides membership, so no
        /// part of this scans the model per selected space.
        /// </para>
        /// </summary>
        private static List<string> Refusals_VentilationScope(AdjacencyCluster adjacencyCluster, HashSet<Guid> guids_Selected)
        {
            List<string> result = [];

            //Unit name -> whether any system on it serves a selected space, and the excluded spaces it also
            //serves. Built in the same pass as the per-system check so the units cost nothing extra.
            Dictionary<string, bool> dictionary_Selected = [];
            Dictionary<string, List<string>> dictionary_Excluded = [];

            foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetObjects<VentilationSystem>() ?? [])
            {
                if (ventilationSystem is null)
                {
                    continue;
                }

                //A system that only restates rooms an in-design-chain system already serves cannot straddle
                //anything, so it is not asked to. See IsRedundant_VentilationSystem for why "bare" is not
                //the criterion and why undetailed shared plant still refuses.
                if (IsRedundant_VentilationSystem(adjacencyCluster, ventilationSystem))
                {
                    continue;
                }

                bool serves_Selected = false;
                List<string> names_Excluded = [];

                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
                {
                    if (space is null)
                    {
                        continue;
                    }

                    if (guids_Selected.Contains(space.Guid))
                    {
                        serves_Selected = true;
                    }
                    else
                    {
                        names_Excluded.Add(space.Name);
                    }
                }

                if (serves_Selected && names_Excluded.Count != 0)
                {
                    result.Add(string.Format(
                        "The selected dwellings cannot be simulated in isolation because ventilation system '{0}' also serves {1} space(s) outside the isolation scope, including '{2}'. Select those dwellings as well, or run the whole building.",
                        ventilationSystem.FullName ?? ventilationSystem.Name,
                        names_Excluded.Count,
                        names_Excluded[0]));
                }

                foreach (string name_Unit in UnitNames(ventilationSystem))
                {
                    dictionary_Selected[name_Unit] = dictionary_Selected.TryGetValue(name_Unit, out bool selected) ? selected || serves_Selected : serves_Selected;

                    if (!dictionary_Excluded.TryGetValue(name_Unit, out List<string> names))
                    {
                        names = [];
                        dictionary_Excluded[name_Unit] = names;
                    }

                    names.AddRange(names_Excluded);
                }
            }

            //A unit whose systems are individually in scope but which, taken together, straddles the cut -
            //one unit serving one selected dwelling and one excluded one through two systems. The
            //per-system check above cannot see that; this does.
            foreach (KeyValuePair<string, bool> keyValuePair in dictionary_Selected)
            {
                if (!keyValuePair.Value)
                {
                    continue;
                }

                if (!dictionary_Excluded.TryGetValue(keyValuePair.Key, out List<string> names_Excluded) || names_Excluded.Count == 0)
                {
                    continue;
                }

                string refusal = string.Format(
                    "The selected dwellings cannot be simulated in isolation because air handling unit '{0}' also serves {1} space(s) outside the isolation scope, including '{2}'. Splitting a shared unit's duty is not something this can decide - select those dwellings as well, or run the whole building.",
                    keyValuePair.Key,
                    names_Excluded.Count,
                    names_Excluded[0]);

                if (!result.Contains(refusal))
                {
                    result.Add(refusal);
                }
            }

            return result;
        }

        /// <summary>
        /// Any air movement that would survive into the isolated model pointing at a space that is no longer
        /// there - a transfer path from an excluded room into a selected one, or the reverse.
        /// <para>
        /// Refused rather than dropped. A transfer path is part of how the dwelling was shown to meet
        /// Approved Document F; silently removing one would change the ventilation design while reporting
        /// the same compliance. For the dedicated per-dwelling MVHR the Part O workflow builds, every path
        /// is within one dwelling and this never fires.
        /// </para>
        /// </summary>
        private static List<string> Refusals_AirMovementScope(AdjacencyCluster adjacencyCluster, HashSet<Guid> guids_Selected)
        {
            List<string> result = [];

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                if (spaceAirMovement is null)
                {
                    continue;
                }

                bool touches_Selected = false;
                List<string> names_Excluded = [];

                foreach (Space space in Spaces_AirMovement(adjacencyCluster, spaceAirMovement))
                {
                    if (guids_Selected.Contains(space.Guid))
                    {
                        touches_Selected = true;
                    }
                    else if (!names_Excluded.Contains(space.Name))
                    {
                        names_Excluded.Add(space.Name);
                    }
                }

                if (touches_Selected && names_Excluded.Count != 0)
                {
                    result.Add(string.Format(
                        "The selected dwellings cannot be simulated in isolation because air movement '{0}' crosses the isolation boundary - it connects a selected space to '{1}', which is outside the scope. Select that dwelling as well, or run the whole building.",
                        spaceAirMovement.Name,
                        names_Excluded[0]));
                }
            }

            return result;
        }

        /// <summary>
        /// Both spaces an air movement actually connects - <b>read from its <c>From</c> and <c>To</c>
        /// references as well as from its relations</b>.
        ///
        /// <para><b>Why the relations alone are not the answer</b></para>
        /// <para>
        /// A transfer air movement is deliberately related to <b>one</b> space, the one the air arrives in:
        /// <c>Modify.AddPartFTransferAirMovements</c> relates it to the downstream space only, because
        /// relating it to both would have the TBD writer walk it twice and write the dwelling two identical
        /// inter-zone air movements. The upstream space exists on the object as a <c>From</c> reference and
        /// nowhere else.
        /// </para>
        /// <para>
        /// So asking the relation graph what a movement connects gives a truthful answer for a supply or an
        /// extract - <c>Modify.AddAirMovementObjects</c> relates those to both the unit and the room - and
        /// half an answer for a transfer. Isolating on half an answer let two cases through silently:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <b>excluded -> selected.</b> Related only to the selected space, so nothing looked excluded and
        /// nothing refused. Carried into the derived model with a <c>From</c> pointing at a space that is
        /// not in it - air arriving from a room that does not exist.
        /// </item>
        /// <item>
        /// <b>selected -> excluded.</b> Related only to the excluded space, so it did not look as though it
        /// touched the selection at all. Not refused and not carried: the selected room passes air on to a
        /// room that is not there, and the dwelling's transfer air quietly comes up short.
        /// </item>
        /// </list>
        /// <para>
        /// A reference that resolves to something other than a space - an air handling unit - is not an
        /// endpoint this asks about, and neither is an absent one, which is how "outside" is said. Those are
        /// the plant side, and <see cref="Refusals_VentilationScope"/> and
        /// <see cref="CarryAirHandlingUnits"/> are what decide about them.
        /// </para>
        /// </summary>
        private static List<Space> Spaces_AirMovement(AdjacencyCluster adjacencyCluster, SpaceAirMovement spaceAirMovement)
        {
            List<Space> result = [];

            HashSet<Guid> guids = [];

            foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(spaceAirMovement) ?? [])
            {
                if (space is not null && guids.Add(space.Guid))
                {
                    result.Add(space);
                }
            }

            foreach (string reference in new string[] { spaceAirMovement.From, spaceAirMovement.To })
            {
                if (string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(reference);
                if (objectReference is null)
                {
                    continue;
                }

                if (adjacencyCluster.GetObject(objectReference) is not Space space)
                {
                    continue;
                }

                if (guids.Add(space.Guid))
                {
                    result.Add(space);
                }
            }

            return result;
        }

        /// <summary>
        /// Restores every source relation whose <b>both</b> ends were carried into the derived cluster.
        /// Walks only what was carried, so an isolated flat costs the flat.
        ///
        /// <para><b>Enumerated with the untyped <c>GetObjects()</c>, deliberately</b></para>
        /// <para>
        /// <c>GetObjects&lt;IJSAMObject&gt;()</c> answers <b>null</b> on an
        /// <see cref="AdjacencyCluster"/> however much it holds, so this method used to return at its
        /// first line and restore nothing at all. <c>RelationCluster.GetObjects(Type)</c> gates on
        /// <c>IsValid(type)</c>, and <c>AdjacencyCluster.IsValid</c> asks whether the type is assignable
        /// TO one of the analytical families it admits - which <c>IJSAMObject</c>, being broader than all
        /// of them, is not. That gate is right for adding an object and wrong for asking what is in there,
        /// so the ungated overload is the one to ask.
        /// </para>
        /// <para>
        /// The visible consequence: the unit's supply air movement was never related back to its unit, so
        /// <c>Query.AirFlow</c> found nothing to size the unit's intake from, so
        /// <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> wrote no "IZAM &lt;unit&gt; FROM OUTSIDE", so the
        /// generated plant zone delivered the dwelling's supply while gaining nothing - and TAS refuses to
        /// simulate a zone whose air movements do not balance.
        /// </para>
        /// </summary>
        private static void RestoreRelations(AdjacencyCluster adjacencyCluster, AdjacencyCluster result)
        {
            List<IJSAMObject> objects = result.GetObjects();
            if (objects is null)
            {
                return;
            }

            HashSet<Guid> guids_Carried = [];
            foreach (IJSAMObject @object in objects)
            {
                Guid guid = result.GetGuid(@object);
                if (guid != Guid.Empty)
                {
                    guids_Carried.Add(guid);
                }
            }

            foreach (IJSAMObject @object in objects)
            {
                Guid guid = result.GetGuid(@object);
                if (guid == Guid.Empty)
                {
                    continue;
                }

                foreach (IJSAMObject object_Related in adjacencyCluster.GetRelatedObjects(guid) ?? [])
                {
                    Guid guid_Related = adjacencyCluster.GetGuid(object_Related);
                    if (guid_Related == Guid.Empty || !guids_Carried.Contains(guid_Related))
                    {
                        continue;
                    }

                    result.AddRelation(guid, guid_Related);
                }
            }
        }

        /// <summary>
        /// Carries in the air handling units the derived model's ventilation systems name, and the plant
        /// side objects that belong to them. A unit reaches its systems by NAME rather than by relation,
        /// which is why nothing space-shaped can find it.
        ///
        /// <para><b>Objects only. Relations are <see cref="RestoreRelations"/>'s job.</b></para>
        /// <para>
        /// This used to restore the unit's own relations here, guarded on
        /// <c>result.GetObject&lt;IJSAMObject&gt;(guid)</c> being non-null - a guard that can never be
        /// satisfied, because <c>AdjacencyCluster.IsValid(Type)</c> rejects <c>IJSAMObject</c> and
        /// <c>RelationCluster.GetObject(Type, Guid)</c> gates on it, so the lookup answers null for every
        /// guid the cluster holds. The unit reached the derived model related to nothing but its own
        /// <see cref="AirHandlingUnitAirMovement"/>; <c>Query.AirFlow</c> had no supply movement to read;
        /// <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> wrote no intake IZAM; and TAS refused the model.
        /// </para>
        /// <para>
        /// So the two are split by responsibility and ordered: this adds objects, and RestoreRelations -
        /// which runs after it, over the whole carried set - is the single authority on relations. One
        /// mechanism, one place to be right.
        /// </para>
        /// </summary>
        private static List<string> CarryAirHandlingUnits(AdjacencyCluster adjacencyCluster, AdjacencyCluster result)
        {
            List<string> notes = [];

            HashSet<string> names_Unit = [];
            foreach (VentilationSystem ventilationSystem in result.GetObjects<VentilationSystem>() ?? [])
            {
                foreach (string name_Unit in UnitNames(ventilationSystem))
                {
                    names_Unit.Add(name_Unit);
                }
            }

            if (names_Unit.Count == 0)
            {
                return notes;
            }

            int count = 0;
            int count_PlantAirMovement = 0;

            foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [])
            {
                if (airHandlingUnit is null || string.IsNullOrWhiteSpace(airHandlingUnit.Name) || !names_Unit.Contains(airHandlingUnit.Name))
                {
                    continue;
                }

                AirHandlingUnit airHandlingUnit_Result = Core.Query.Clone(airHandlingUnit);
                if (!result.AddObject(airHandlingUnit_Result))
                {
                    continue;
                }

                count++;

                //Everything on the unit's side of the model that no space can reach, so that
                //RestoreRelations then finds both ends of the unit's relations already carried.
                foreach (IJSAMObject object_Related in adjacencyCluster.GetRelatedObjects(airHandlingUnit) ?? [])
                {
                    if (object_Related is null)
                    {
                        continue;
                    }

                    Guid guid_Related = adjacencyCluster.GetGuid(object_Related);
                    if (guid_Related == Guid.Empty)
                    {
                        continue;
                    }

                    //Already carried, by Filter or by an earlier unit. Asked with GetTypeName, which is
                    //the cluster's own type-agnostic "do you hold this guid" - NOT GetObject<T>, whose
                    //type gate is what broke this in the first place.
                    if (!string.IsNullOrEmpty(result.GetTypeName(guid_Related)))
                    {
                        continue;
                    }

                    //The unit's supply condition profiles, which become the humidistat and thermostat on
                    //its generated TAS plant zone. Related to the unit and to nothing else, ever.
                    if (object_Related is AirHandlingUnitAirMovement)
                    {
                        result.AddObject(Core.Query.Clone(object_Related));

                        continue;
                    }

                    //The unit's own PLANT SIDE air movements - its exhaust, its intake - which hang off the
                    //unit rather than off a room.
                    //
                    //Filter cannot carry these: they are related to no space at all, so nothing reachable
                    //from the selection finds them. And one that touches an EXCLUDED space cannot arrive
                    //here, because Refusals_AirMovementScope already refused the whole isolation over it -
                    //reading both of the movement's ends, its From and To references included, not only its
                    //relations. What is left is plant belonging to a unit this model is carrying.
                    //
                    //Carried even where this particular dwelling turns out not to need it:
                    //Modify.AddAirHandlingUnitExhaust exists because a unit that gains the extract duty and
                    //never loses it is a zone TAS refuses to simulate, so dropping the exhaust silently
                    //turns a balanced unit into an unbalanced one.
                    if (object_Related is SpaceAirMovement spaceAirMovement)
                    {
                        List<Space> spaces_Related = adjacencyCluster.GetRelatedObjects<Space>(spaceAirMovement);
                        if (spaces_Related is not null && spaces_Related.Count != 0)
                        {
                            continue;
                        }

                        result.AddObject(Core.Query.Clone<SpaceAirMovement>(spaceAirMovement));
                        count_PlantAirMovement++;
                    }
                }
            }

            if (count != 0)
            {
                notes.Add(string.Format("{0} air handling unit(s) serving only the selected dwellings were carried into the isolated model.", count));
            }

            if (count_PlantAirMovement != 0)
            {
                notes.Add(string.Format("{0} plant side air movement(s) belonging to those units - the unit's own intake and exhaust, which no room reaches - were carried with them.", count_PlantAirMovement));
            }

            return notes;
        }

        /// <summary>
        /// Adds the excluded building's <b>external</b> surfaces to the derived model as shading geometry -
        /// and nothing else.
        ///
        /// <para><b>What qualifies, using the definitions that already exist</b></para>
        /// <para>
        /// A source panel that was not carried is shading context when it is exposed to the sun
        /// (<see cref="Query.ExposedToSun(PanelType)"/> - the same test the gbXML export uses to decide the
        /// surface's own solar exposure) <b>and</b> it has at most one adjacent space, which is what
        /// <see cref="AdjacencyCluster.External(Panel)"/> means. So an excluded façade, roof or exposed
        /// floor comes across, and an existing shade in the source model - which has no adjacent space at
        /// all - keeps coming across, exactly as it did before.
        /// </para>
        /// <para>
        /// <b>An internal partition never does.</b> It has two adjacent spaces and is not exposed to the
        /// sun, and turning every panel of every excluded dwelling into a shade would build a huge model
        /// of surfaces buried inside a building where no sun reaches, which is both meaningless and the
        /// thing that would undo the speed this feature exists for.
        /// </para>
        /// <para>
        /// <b>No proximity culling.</b> Retaining a legitimate external surface that turns out to shade
        /// nothing costs a little conversion; guessing wrong about which surfaces matter changes the solar
        /// result. The reduction this feature delivers comes from not simulating thousands of thermal
        /// zones, not from pruning shade.
        /// </para>
        /// </summary>
        private static int AddShadingContext(AdjacencyCluster adjacencyCluster, AdjacencyCluster result)
        {
            int count = 0;

            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                if (panel is null)
                {
                    continue;
                }

                //Already in the isolated model as a real boundary. Adding it again as shade would double
                //the surface and shade the flat with a copy of its own wall.
                if (result.GetObject<Panel>(panel.Guid) is not null)
                {
                    continue;
                }

                if (!Query.ExposedToSun(panel.PanelType))
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel);
                if (spaces is not null && spaces.Count > 1)
                {
                    continue;
                }

                //Stated as a shade rather than left as a spaceless external wall: the conversion decides
                //what a surface is from its type, and a wall belonging to no space is not a thing the TBD
                //model has a place for. Apertures go with it - a window in a shade is not a window.
                Panel panel_Shade = panel.PanelType == PanelType.Shade ? Create.Panel(panel) : Create.Panel(panel, PanelType.Shade);
                panel_Shade.RemoveApertures();

                if (result.AddObject(panel_Shade))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// <b>Whether a ventilation system is redundant metadata beside the plant that actually serves its
        /// rooms</b> - so that isolating a dwelling is not refused on account of it.
        ///
        /// <para><b>The case</b></para>
        /// <para>
        /// A building drawn with one estate-wide <c>MV</c> system - a single <see cref="VentilationSystem"/>
        /// related to every flat's rooms, naming a central unit that was never detailed - refused every
        /// per-dwelling isolation. <see cref="Refusals_VentilationScope"/> saw a system related to spaces in
        /// two dwellings and read it as shared plant. In the models this arises from it is not: each of
        /// those rooms is ALSO on a per-dwelling MVHR carrying that room's design terminals, and it is that
        /// system the Part O preparation builds, sums a duty from, and realizes air movements for. The
        /// estate-wide one contributes nothing to the Approved Document F requirement, nothing to a design
        /// airflow, nothing to a duty and nothing to the inter-zone air movements the TBD carries.
        /// </para>
        ///
        /// <para><b>Bare is NOT the criterion, and must not be</b></para>
        /// <para>
        /// A system carrying no terminal and no air movement is not automatically ignorable: on a model
        /// where the design has not been built yet EVERY system looks like that, and a genuinely shared
        /// central system somebody has still to detail is exactly what this refusal exists to catch.
        /// <c>PartOIsolationTests.SharedVentilationSystem_Refuses</c> and
        /// <c>SharedAirHandlingUnit_AcrossTwoSystems_Refuses</c> pin that, and they pass unchanged.
        /// </para>
        ///
        /// <para><b>Redundancy is the criterion</b></para>
        /// <para>
        /// A system is ignored only where <b>every room it claims is already served by a system that IS in
        /// the design chain</b>. Then it is not plant awaiting detail - it is a second, weaker statement
        /// about rooms whose air is already accounted for, and taking a dwelling out of the scope removes
        /// nothing anything reads. Where even one of its rooms has no such system, it is the only thing
        /// standing for that room's ventilation and the refusal stands.
        /// </para>
        /// <para>
        /// "In the design chain" is <see cref="IsDesignChain_VentilationSystem"/>, which is
        /// <c>SAM.Analytical.UI.WPF.Query.PartOIterationAirHandlingUnits</c>' own test for a run's equipment
        /// - connected to a design <see cref="VentilationTerminal"/> - widened to anything else that moves
        /// air or has had a product chosen, so that nothing real is ever called redundant.
        /// </para>
        ///
        /// <para><b>Nothing is decided about the model</b></para>
        /// <para>
        /// No duty is split, no capacity allocated, nothing written back. An ignored system keeps its
        /// relations and is carried into the derived model by <c>Filter</c> exactly as before, related to
        /// whichever of its spaces were retained. The single effect is that it no longer refuses.
        /// </para>
        /// </summary>
        private static bool IsRedundant_VentilationSystem(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem)
        {
            if (adjacencyCluster is null || ventilationSystem is null)
            {
                return false;
            }

            //Itself in the design chain: real plant, never redundant.
            if (IsDesignChain_VentilationSystem(adjacencyCluster, ventilationSystem))
            {
                return false;
            }

            List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem);
            if (spaces is null || spaces.Count == 0)
            {
                //It claims no room, so it cannot straddle the cut through one either.
                return true;
            }

            foreach (Space space in spaces)
            {
                if (space is null)
                {
                    continue;
                }

                bool covered = false;

                foreach (VentilationSystem ventilationSystem_Other in adjacencyCluster.GetRelatedObjects<VentilationSystem>(space) ?? [])
                {
                    if (ventilationSystem_Other is null || ventilationSystem_Other.Guid == ventilationSystem.Guid)
                    {
                        continue;
                    }

                    if (IsDesignChain_VentilationSystem(adjacencyCluster, ventilationSystem_Other))
                    {
                        covered = true;

                        break;
                    }
                }

                if (!covered)
                {
                    //This room's ventilation is stated by this system and by nothing else in the design
                    //chain. Fail closed.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// <b>Whether a ventilation system is part of the design the simulation reads.</b>
        /// <para>
        /// True where any of these holds: it carries a design <see cref="VentilationTerminal"/> - the
        /// relation only the Part O preparation creates, and the test
        /// <c>Query.PartOIterationAirHandlingUnits</c> uses to decide a run's equipment; it carries a
        /// <see cref="SpaceAirMovement"/>, so it moves air between rooms; or a unit it names is itself real
        /// plant, per <see cref="IsDesignChain_AirHandlingUnit"/>.
        /// </para>
        /// </summary>
        private static bool IsDesignChain_VentilationSystem(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem)
        {
            if (adjacencyCluster is null || ventilationSystem is null)
            {
                return false;
            }

            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem);
            if (ventilationTerminals is not null && ventilationTerminals.Count != 0)
            {
                return true;
            }

            List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(ventilationSystem);
            if (spaceAirMovements is not null && spaceAirMovements.Count != 0)
            {
                return true;
            }

            foreach (string name_Unit in UnitNames(ventilationSystem))
            {
                foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>() ?? [])
                {
                    if (airHandlingUnit is null || !string.Equals(airHandlingUnit.Name, name_Unit, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (IsDesignChain_AirHandlingUnit(adjacencyCluster, airHandlingUnit))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// <b>Whether an air handling unit is more than a name.</b> Any one of these is enough: an
        /// <see cref="AirHandlingUnitAirMovement"/> - its own supply condition, which becomes its generated
        /// TAS plant zone; a <see cref="SpaceAirMovement"/>, air it actually moves to or from a room; a
        /// <see cref="VentilationTerminal"/>, so it is in the design chain; or
        /// <c>AirHandlingUnitParameter.VentilationUnitReference</c>, a product chosen against it and
        /// therefore a capacity somebody selected.
        /// </summary>
        private static bool IsDesignChain_AirHandlingUnit(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit)
        {
            if (adjacencyCluster is null || airHandlingUnit is null)
            {
                return false;
            }

            //Asked through the same query the duty and the equipment table read it with, so a product
            //selected here is a product there.
            if (Query.SelectedVentilationUnitReference(airHandlingUnit) is not null)
            {
                return true;
            }

            foreach (IJSAMObject object_Related in adjacencyCluster.GetRelatedObjects(airHandlingUnit) ?? [])
            {
                if (object_Related is AirHandlingUnitAirMovement || object_Related is SpaceAirMovement || object_Related is VentilationTerminal)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> UnitNames(VentilationSystem ventilationSystem)
        {
            List<string> result = [];

            if (ventilationSystem is null)
            {
                return result;
            }

            if (ventilationSystem.TryGetValue(VentilationSystemParameter.SupplyUnitName, out string name_Supply) && !string.IsNullOrWhiteSpace(name_Supply))
            {
                result.Add(name_Supply);
            }

            if (ventilationSystem.TryGetValue(VentilationSystemParameter.ExhaustUnitName, out string name_Exhaust) && !string.IsNullOrWhiteSpace(name_Exhaust) && !result.Contains(name_Exhaust))
            {
                result.Add(name_Exhaust);
            }

            return result;
        }
    }
}
