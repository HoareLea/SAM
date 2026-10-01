// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Width [m] of an internal door this method creates: the Approved Document F, Volume 1: Dwellings
        /// (2021 edition) paragraph 1.25 reference door width, so a 10mm undercut across a door this wide
        /// provides exactly the 7,600mm2 free area the paragraph requires. There is no other door width
        /// convention anywhere in the Part F workflow - the Approved Document's own reference door is the
        /// source of truth, and no new assumption is introduced.
        /// </summary>
        public const double DefaultTransferAirDoorWidth_M = PartFDoorTransferData.ReferenceDoorWidth_mm / 1000;

        /// <summary>
        /// Height [m] of an internal door this method creates. Approved Document F sets no door height;
        /// 2.1m is the programme's documented default for a created transfer door ("a 760 x 2100 mm internal door"). Geometry only - nothing in the Part F assessment
        /// reads it.
        /// </summary>
        public const double DefaultTransferAirDoorHeight_M = 2.1;

        /// <summary>
        /// Adds the internal transfer-air doors Approved Document F requires but the model does not carry.
        /// <para>
        /// The transfer-air requirement is established by running the SAME <see cref="PartFCalculator"/>
        /// that <c>SAMAnalytical.AddVentilationPropertiesByPartF</c>, <c>SAMAnalytical.CheckPartFCompliance</c>
        /// and SAM_UI's Part F assessment run - the calculation is not duplicated here, only its result is
        /// acted on. The calculator deep-clones the model, re-sizes the dwelling(s) identically and
        /// refreshes every internal door's <see cref="PartFDoorTransferData"/>, carrying the engineer's
        /// recorded inputs (provided undercut, provided free area, transfer device type, flow override)
        /// forward. This method then walks the dwelling transfer-air schedules and, for every route that
        /// has to carry air but has no modelled door
        /// (<see cref="PartFDoorTransferData.IsDoorRepresented"/> false with a real transfer flow), creates
        /// ONE internal door in a shared internal wall between the two spaces and writes the route's
        /// paragraph 1.25 record onto it.
        /// </para>
        /// <para>
        /// <b>Existing suitable doors are never duplicated.</b> A route with a modelled door already carries
        /// its refreshed record from the calculator and is left alone, which is also what makes a second
        /// run of this method a no-op.
        /// </para>
        /// <para>
        /// <b>The door.</b> The door carries the default internal-door construction of the active
        /// <see cref="ApertureConstructionLibrary"/>. Where that library establishes none, the route is
        /// REFUSED rather than given an invented one - a door is a real building element, and
        /// manufacturing a construction merely to let the geometry exist would put into the model a
        /// build-up nothing supports. The door is <see cref="DefaultTransferAirDoorWidth_M"/> wide and
        /// <see cref="DefaultTransferAirDoorHeight_M"/> high, sits on the bottom edge of the wall, and is
        /// centred on the clearest horizontal position of the panel - the panel centre where it is free,
        /// otherwise as close to it as the existing apertures allow.
        /// </para>
        /// <para>
        /// <b>The undercut is a requirement, not a manufactured fact.</b> The created door's record carries
        /// the paragraph 1.25 requirement - a minimum free area of 7,600mm2, equivalent to a 10mm undercut
        /// across the 760mm door width, achieved 10mm above a fitted floor finish or 20mm above an
        /// unfinished floor surface - and its provided undercut stays UNRECORDED, because an analytical
        /// model does not represent the gap under a door leaf and absence of evidence is never compliance.
        /// The route therefore reports <see cref="Enums.PartFComplianceStatus.CannotBeDetermined"/> until
        /// the engineer records what is actually provided, exactly as a hand-modelled door does.
        /// </para>
        /// <para>
        /// <b>Refuse, never guess.</b> No door is created - and the route is returned in
        /// <paramref name="refusals"/> with the reason - where the two spaces share no internal wall,
        /// where none of the shared walls can geometrically take the door, where the candidates cannot be
        /// ranked at all (a space with no valid location), or where no default internal-door construction
        /// is established. Where several shared walls could each take the door, the host panel is resolved
        /// by the selection hierarchy - host validity, geometric relevance, shorter wall, then the stable
        /// first candidate (see
        /// <see cref="AddTransferAirDoor(AdjacencyCluster, PartFDoorTransferData, out string, out string)"/>)
        /// - so a route is never refused merely because two candidate walls are equal. The supplied model
        /// is never modified; an updated copy is returned.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">
        /// A model whose spaces already carry <c>PartFSpaceData</c> - normally the output of
        /// <c>SAMAnalytical.AddVentilationPropertiesByPartF</c>. <b>Not modified</b>: an updated copy is
        /// returned. (The calculation re-derives the same Part F data itself, so an unprocessed model is
        /// accepted too; running the sizing component first remains the intended workflow.)
        /// </param>
        /// <param name="zoneCategoryName">
        /// Zone category containing the dwelling zones, exactly as supplied to
        /// <c>SAMAnalytical.AddVentilationPropertiesByPartF</c>. Null or empty sizes the whole model as one
        /// dwelling. Passing a different category than the sizing run would re-group the dwellings, so the
        /// same value should be supplied.
        /// </param>
        /// <param name="setbackFlowRateFactor">
        /// Setback operating-rate factor, again exactly as supplied upstream. Null uses the rule set's own
        /// factor. Because this method re-runs the sizing, passing a different factor here than upstream
        /// would change the setback rates written on the spaces.
        /// </param>
        /// <param name="doors_Created">The door apertures created, one per resolved route.</param>
        /// <param name="notes">What was done, one sentence each - doors created, existing doors reused.</param>
        /// <param name="refusals">Routes where a transfer path is required but no defensible door could be created, with the reason.</param>
        /// <returns>The updated model, or null where no Part F calculation could run at all.</returns>
        public static AnalyticalModel AddTransferAirDoorsByPartF(this AnalyticalModel analyticalModel, string zoneCategoryName, double? setbackFlowRateFactor, out List<Aperture> doors_Created, out List<string> notes, out List<string> refusals)
        {
            doors_Created = [];
            notes = [];
            refusals = [];

            if (analyticalModel == null)
            {
                refusals.Add("No analytical model was supplied.");
                return null;
            }

            AdjacencyCluster adjacencyCluster_Input = analyticalModel.AdjacencyCluster;
            if (adjacencyCluster_Input == null)
            {
                refusals.Add("The analytical model carries no adjacency cluster.");
                return null;
            }

            PartFCalculator partFCalculator = Query.DefaultPartFCalculator();
            if (partFCalculator == null)
            {
                refusals.Add("Could not load the Part F rule set. Run SAMAnalytical.AddVentilationPropertiesByPartF first and check the Part F resources are available.");
                return null;
            }

            partFCalculator.AdjacencyCluster = adjacencyCluster_Input;

            if (setbackFlowRateFactor != null && setbackFlowRateFactor.HasValue)
            {
                //The property validates and substitutes the documented default for an invalid factor, so an
                //out-of-range value can never poison the setback rates.
                partFCalculator.SetbackFlowRateFactor = setbackFlowRateFactor.Value;
            }

            if (!partFCalculator.Calculate(zoneCategoryName))
            {
                refusals.Add("The Part F calculation did not run, so no transfer-air doors could be resolved.");
                return null;
            }

            //The calculator's own deep clone: every write below lands on the copy, never on the caller's
            //model, and every existing door aperture already carries its refreshed record.
            AdjacencyCluster adjacencyCluster = partFCalculator.AdjacencyCluster;

            int count_Reused = 0;

            foreach (PartFDwellingResult partFDwellingResult in partFCalculator.DwellingResults ?? [])
            {
                List<PartFDoorTransferData> transferPaths = partFDwellingResult?.ComplianceResult?.TransferPaths;
                if (transferPaths == null)
                {
                    continue;
                }

                foreach (PartFDoorTransferData partFDoorTransferData in transferPaths)
                {
                    if (partFDoorTransferData == null || !partFDoorTransferData.RequiresTransferAirPath || !partFDoorTransferData.IsInternalDwellingDoor)
                    {
                        continue;
                    }

                    if (partFDoorTransferData.IsDoorRepresented)
                    {
                        //The calculator has already refreshed this door's record and written it back onto
                        //the aperture; an existing suitable door is never duplicated.
                        count_Reused++;
                        continue;
                    }

                    //A route only earns a door where air actually has to move through it. Paragraph 1.25
                    //puts a requirement on every internal door, but a partition between two rooms with no
                    //transfer flow - two adjacent bedrooms, both supplied - needs no opening manufactured
                    //in it.
                    double flow_Continuous = System.Math.Abs(partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps ?? 0);
                    double flow_High = System.Math.Abs(partFDoorTransferData.HighTransferFlowRate_Lps ?? 0);
                    if (flow_Continuous <= PartFAirflowNetwork.Tolerance_Lps && flow_High <= PartFAirflowNetwork.Tolerance_Lps)
                    {
                        continue;
                    }

                    Aperture aperture = AddTransferAirDoor(adjacencyCluster, partFDoorTransferData, out string refusal, out string note_Selection);
                    if (aperture == null)
                    {
                        refusals.Add(string.Format("{0}{1} to {2}: a transfer path is required ({3:0.##} l/s at the continuous design condition) but no internal door could be created - {4}.",
                            string.IsNullOrWhiteSpace(partFDwellingResult.Name) ? string.Empty : partFDwellingResult.Name + ": ",
                            partFDoorTransferData.UpstreamSpaceName,
                            partFDoorTransferData.DownstreamSpaceName,
                            flow_Continuous,
                            refusal));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(note_Selection))
                    {
                        notes.Add(string.Format("{0}{1} to {2}: {3}",
                            string.IsNullOrWhiteSpace(partFDwellingResult.Name) ? string.Empty : partFDwellingResult.Name + ": ",
                            partFDoorTransferData.UpstreamSpaceName,
                            partFDoorTransferData.DownstreamSpaceName,
                            note_Selection));
                    }

                    //The route record itself is updated and written onto the new aperture through the shared
                    //helper, because Panel.Apertures hands out clones and that is the only path that
                    //persists. ApertureGuid and IsDoorRepresented describe the door now in the model; the
                    //clear width is read back from its geometry exactly as the calculation reads any other
                    //modelled door; Assess re-judges the record, which with nothing provided stays
                    //CannotBeDetermined - created is not compliant.
                    partFDoorTransferData.ApertureGuid = aperture.Guid;
                    partFDoorTransferData.IsDoorRepresented = true;
                    partFDoorTransferData.ClearDoorWidth_mm = PartFAirflowNetwork.ClearDoorWidth_mm(aperture);
                    PartFTransferPathBuilder.Assess(partFDoorTransferData);

                    adjacencyCluster.SetPartFDoorTransferData(aperture.Guid, partFDoorTransferData);

                    //SetPartFDoorTransferData persists the record by creating a REPLACEMENT aperture (Panel.Apertures
                    //hands out clones), so the aperture created above is no longer the one in the model. Re-reading it
                    //means the reported door is the door the model actually carries - with the PartFDoorTransferData
                    //record attached - rather than a detached original that a caller comparing against the returned
                    //model would find bare.
                    Aperture aperture_Persisted = adjacencyCluster.GetAperture(aperture.Guid) ?? aperture;

                    doors_Created.Add(aperture_Persisted);

                    notes.Add(string.Format("{0}{1} to {2}: created a {3:0} mm x {4:0} mm internal door ({5}) in the shared wall and recorded the paragraph 1.25 requirement on it - a minimum free area of {6:0.##} mm2, equivalent to a {7:0.##} mm undercut across the {3:0} mm door width. The provided undercut is not recorded and remains to be confirmed.",
                        string.IsNullOrWhiteSpace(partFDwellingResult.Name) ? string.Empty : partFDwellingResult.Name + ": ",
                        partFDoorTransferData.UpstreamSpaceName,
                        partFDoorTransferData.DownstreamSpaceName,
                        DefaultTransferAirDoorWidth_M * 1000,
                        DefaultTransferAirDoorHeight_M * 1000,
                        aperture.Name,
                        PartFDoorTransferData.NominalEquivalentFreeArea_mm2,
                        PartFDoorTransferData.ReferenceUndercutHeight_mm));
                }
            }

            if (count_Reused != 0)
            {
                notes.Add(string.Format("{0} existing internal door(s) already serve the dwelling transfer paths; their Part F records were refreshed and no duplicates were created.", count_Reused));
            }

            if (doors_Created.Count == 0 && refusals.Count == 0)
            {
                notes.Add("No transfer-air doors were required: every route that carries air already has a modelled door.");
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// Creates the single internal door for one unrepresented transfer route: resolves the internal
        /// wall panels the two spaces share, keeps the ones that can geometrically take the standard
        /// transfer door exactly as they stand, and places the door in the best of them. Null - with the
        /// reason in <paramref name="refusal"/> - where none can, where no candidate can be ranked
        /// (missing/invalid space location), or where no default internal-door construction is
        /// established.
        /// <para>
        /// <b>Where several shared walls could each take the door, the host panel is resolved by the
        /// selection hierarchy: host validity, then geometric relevance, then shorter wall, then the
        /// stable first candidate.</b> First, the wall most directly between the two spaces - the one the
        /// segment joining the two space locations passes through, or of the walls it does not pass
        /// through, the one it passes closest to - wins. Where the geometry ties two candidates (within
        /// <see cref="Core.Tolerance.Distance"/>), the SHORTER valid shared wall is preferred - the wall
        /// the door fits more closely. Only where the tied walls are also the same length (within
        /// <see cref="Core.Tolerance.Distance"/>) is the first of the stable guid-sorted candidates
        /// taken. Room name, panel name, wall area and creation/enumeration order remain arbitrary with
        /// respect to where a door belongs, and none of them is consulted; guid order is the absolute
        /// final deterministic fallback only, never the first arbiter. A route is refused only where the
        /// candidates cannot be ranked at all - one of the two spaces carries no valid location - never
        /// merely because two candidates are equal.
        /// </para>
        /// </summary>
        private static Aperture AddTransferAirDoor(AdjacencyCluster adjacencyCluster, PartFDoorTransferData partFDoorTransferData, out string refusal, out string note_Selection)
        {
            refusal = null;
            note_Selection = null;

            Space space_1 = adjacencyCluster.GetObject<Space>(partFDoorTransferData.UpstreamSpaceGuid);
            Space space_2 = adjacencyCluster.GetObject<Space>(partFDoorTransferData.DownstreamSpaceGuid);
            if (space_1 == null || space_2 == null)
            {
                refusal = "one of the two spaces is no longer in the model";
                return null;
            }

            //Identity, never name: the panels related to BOTH spaces by guid. The wall group filter
            //excludes the floor between two stacked rooms, which the transfer network quite rightly treats
            //as an adjacency but no door can be hung in.
            List<Panel> panels = adjacencyCluster.GetPanels(LogicalOperator.And, space_1, space_2)?
                .FindAll(x => x != null && x.PanelType.PanelGroup() == PanelGroup.Wall);
            if (panels == null || panels.Count == 0)
            {
                refusal = "the two spaces share no internal wall panel - the adjacency the transfer network found is not a wall, or the partition is not related to both spaces";
                return null;
            }

            //Guid order, and ONLY so that the diagnostics below name the same panels in the same order on
            //every run. Nothing is SELECTED by this order: a guid says nothing about where a door belongs,
            //and neither does the order the panels happen to have been created in.
            panels = [.. panels.OrderBy(x => x.Guid)];

            //Which panels could take the standard door as they stand. The test only READS each panel - the
            //placement is computed and offered to the same host check Panel.AddApertures applies - so every
            //candidate of the route is interrogated before any one of them is committed to.
            List<Tuple<Panel, Polygon3D>> candidates = [];
            List<string> reasons = [];
            foreach (Panel panel in panels)
            {
                if (TryTransferAirDoorGeometry(panel, out Polygon3D polygon3D_Candidate, out string reason))
                {
                    candidates.Add(new Tuple<Panel, Polygon3D>(panel, polygon3D_Candidate));
                    continue;
                }

                reasons.Add(string.Format("panel {0}: {1}", panel.Guid, reason));
            }

            if (candidates.Count == 0)
            {
                refusal = string.Format("none of the {0} shared wall panel(s) can take the door ({1})", panels.Count, string.Join("; ", reasons));
                return null;
            }

            Panel panel_Selected;
            Polygon3D polygon3D_Door;

            if (candidates.Count == 1)
            {
                panel_Selected = candidates[0].Item1;
                polygon3D_Door = candidates[0].Item2;
            }
            else
            {
                //More than one wall could take the door: rank the candidates by how directly each one
                //lies between the two spaces and take the single best. A geometric tie is broken by the
                //shorter valid shared wall, and an equal-length tie by the stable guid-sorted order -
                //nothing here reads candidate creation order, name or area.
                //The ranking reads the two space locations. A space carrying no valid location
                //(IsPlaced false - missing or NaN) establishes nothing geometric, so the candidates
                //cannot be ranked from it and the route is refused rather than scored from invalid
                //geometry: no winner is ever manufactured from NaN distances.
                if (!space_1.IsPlaced() || !space_2.IsPlaced())
                {
                    refusal = string.Format("{0} shared wall panels can each take the transfer door and the candidates could not be distinguished geometrically - one of the two spaces carries no valid location, so no wall can be established as the one between them ({1}) - model the door in the intended wall, or resolve the partition so the two spaces share a single wall panel, and run again",
                        candidates.Count,
                        string.Join("; ", candidates.ConvertAll(x => string.Format("panel {0}", x.Item1.Guid))));
                    return null;
                }

                Segment3D segment3D = new(space_1.Location, space_2.Location);

                List<Tuple<Panel, double>> tuples_Score = candidates.ConvertAll(x => new Tuple<Panel, double>(x.Item1, TransferAirDoorPanelScore(x.Item1.GetFace3D(), segment3D)));

                if (tuples_Score.Exists(x => double.IsNaN(x.Item2)))
                {
                    //Defensive only - every candidate passed TryTransferAirDoorGeometry, which already
                    //established valid planar geometry. A panel that cannot be scored cannot be ranked.
                    refusal = string.Format("{0} shared wall panels can each take the transfer door and the candidates could not be distinguished geometrically ({1}) - model the door in the intended wall, or resolve the partition so the two spaces share a single wall panel, and run again",
                        candidates.Count,
                        string.Join("; ", candidates.ConvertAll(x => string.Format("panel {0}", x.Item1.Guid))));
                    return null;
                }

                double score_Min = tuples_Score.Min(x => x.Item2);
                List<Tuple<Panel, double>> tuples_Best = tuples_Score.FindAll(x => System.Math.Abs(x.Item2 - score_Min) <= Core.Tolerance.Distance);

                if (tuples_Best.Count == 1)
                {
                    panel_Selected = tuples_Best[0].Item1;
                }
                else
                {
                    //Geometric tie: prefer the SHORTER valid shared wall - the wall the door physically
                    //fits more closely - and only where those are also the same length, the first of the
                    //stable guid-sorted candidates. Both fallbacks are deterministic and independent of
                    //creation and enumeration order; guid order is never consulted before host validity,
                    //geometric relevance and wall length have all failed to distinguish.
                    double length_Min = double.NaN;
                    foreach (Tuple<Panel, double> tuple_Best in tuples_Best)
                    {
                        double length = WallLength(tuple_Best.Item1);
                        if (double.IsNaN(length))
                        {
                            continue;
                        }

                        if (double.IsNaN(length_Min) || length < length_Min)
                        {
                            length_Min = length;
                        }
                    }

                    List<Tuple<Panel, double>> tuples_Shortest = double.IsNaN(length_Min)
                        ? tuples_Best
                        : tuples_Best.FindAll(x => System.Math.Abs(WallLength(x.Item1) - length_Min) <= Core.Tolerance.Distance);

                    panel_Selected = tuples_Shortest[0].Item1;
                }

                polygon3D_Door = candidates.Find(x => x.Item1.Guid == panel_Selected.Guid).Item2;

                List<Tuple<Panel, double>> tuples_Other = tuples_Score.FindAll(x => x.Item1.Guid != panel_Selected.Guid);

                string note_Reason;
                if (tuples_Best.Count == 1)
                {
                    note_Reason = score_Min <= Core.Tolerance.Distance
                        ? "the direct line between the two spaces passes through it"
                        : string.Format("it lies closest to the direct line between the two spaces ({0:0.###} m away)", score_Min);
                }
                else
                {
                    double length_Selected = WallLength(panel_Selected);
                    if (!double.IsNaN(length_Selected) && tuples_Best.Exists(x => x.Item1.Guid != panel_Selected.Guid && System.Math.Abs(WallLength(x.Item1) - length_Selected) > Core.Tolerance.Distance))
                    {
                        note_Reason = string.Format("the direct line between the two spaces passes the {0} candidate walls equally closely, and it is the shorter of them ({1:0.###} m)", tuples_Best.Count, length_Selected);
                    }
                    else
                    {
                        note_Reason = string.Format("the direct line between the two spaces passes the {0} candidate walls equally closely and they are the same length; panel {1} was selected as the stable first candidate", tuples_Best.Count, panel_Selected.Guid);
                    }
                }

                note_Selection = string.Format("{0} shared wall panels could take the transfer door; it was created in panel {1} because {2}{3}",
                    candidates.Count,
                    panel_Selected.Guid,
                    note_Reason,
                    tuples_Other.Count == 0 ? string.Empty : string.Format(" ({0})", string.Join("; ", tuples_Other.ConvertAll(x => string.Format("panel {0} stands {1:0.###} m from that line", x.Item1.Guid, x.Item2)))));
            }

            //An established construction or nothing. Substituting a manufactured one here would put a door
            //build-up into the model that no library, no specification and no engineer ever established,
            //purely so the geometry could be created - the operation refuses instead.
            ApertureConstruction apertureConstruction = Query.DefaultApertureConstruction(panel_Selected, ApertureType.Door);
            if (apertureConstruction == null)
            {
                refusal = "no default internal door construction could be resolved from the active aperture construction library, and one is not invented here - load an aperture construction library that carries an internal door construction and run again";
                return null;
            }

            //trimGeometry false: the rectangle was built to fit the wall, and trimming a misfit would
            //silently shrink the door below the width the undercut requirement is read against. A door the
            //panel rejects is refused rather than adjusted.
            List<Aperture> apertures = panel_Selected.AddApertures(apertureConstruction, polygon3D_Door, false);
            if (apertures == null || apertures.Count == 0)
            {
                refusal = string.Format("panel {0} rejected the door geometry", panel_Selected.Guid);
                return null;
            }

            //The panel returned by GetPanels is the cluster's own object; AddObject puts it back so the
            //change is persisted, the same path SetPartFDoorTransferData takes.
            adjacencyCluster.AddObject(panel_Selected);

            return apertures[0];
        }

        /// <summary>
        /// Works out where the standard transfer door would sit on one wall panel - on the panel's bottom
        /// edge, as close to the panel's horizontal centre as the existing apertures allow - and reports
        /// whether the panel can take it there. All placement is computed in the panel's own plane, so a
        /// wall of any orientation behaves the same.
        /// <para>
        /// <b>The panel is only read, never modified.</b> That is what lets every shared wall of a route
        /// be tested before one of them is chosen - and what makes "could more than one wall take this
        /// door?" a question this operation can ask at all.
        /// </para>
        /// </summary>
        private static bool TryTransferAirDoorGeometry(Panel panel, out Polygon3D polygon3D, out string refusal)
        {
            polygon3D = null;
            refusal = null;

            Face3D face3D_Panel = panel.GetFace3D();
            Plane plane = face3D_Panel?.GetPlane();
            if (plane == null)
            {
                refusal = "the panel has no valid planar geometry";
                return false;
            }

            Face2D face2D_Panel = plane.Convert(face3D_Panel);
            BoundingBox2D boundingBox2D_Panel = face2D_Panel?.GetBoundingBox();
            if (boundingBox2D_Panel == null)
            {
                refusal = "the panel has no valid planar geometry";
                return false;
            }

            //Which way is up in the panel's plane. A wall has the world vertical lying in its plane; where
            //it does not, the panel is not a wall a door can be hung in.
            Vector2D vector2D_Up = plane.Convert(Vector3D.WorldZ);
            if (vector2D_Up == null || vector2D_Up.Length < 0.5)
            {
                refusal = "the shared panel is not vertical";
                return false;
            }

            bool verticalIsY = System.Math.Abs(vector2D_Up.Y) >= System.Math.Abs(vector2D_Up.X);
            double up = verticalIsY ? vector2D_Up.Y : vector2D_Up.X;

            double hMin = verticalIsY ? boundingBox2D_Panel.Min.X : boundingBox2D_Panel.Min.Y;
            double hMax = verticalIsY ? boundingBox2D_Panel.Max.X : boundingBox2D_Panel.Max.Y;
            double vMin = verticalIsY ? boundingBox2D_Panel.Min.Y : boundingBox2D_Panel.Min.X;
            double vMax = verticalIsY ? boundingBox2D_Panel.Max.Y : boundingBox2D_Panel.Max.X;

            double width = DefaultTransferAirDoorWidth_M;
            double height = DefaultTransferAirDoorHeight_M;

            if (hMax - hMin < width - Core.Tolerance.MacroDistance)
            {
                refusal = string.Format("the wall is {0:0.###} m wide and the {1:0.###} m door does not fit", hMax - hMin, width);
                return false;
            }

            if (vMax - vMin < height - Core.Tolerance.MacroDistance)
            {
                refusal = string.Format("the wall is {0:0.###} m high and the {1:0.###} m door does not fit", vMax - vMin, height);
                return false;
            }

            //The door stands on the bottom edge of the wall. Where the plane's vertical axis points down,
            //the bottom is the axis's maximum, not its minimum.
            double v0 = up >= 0 ? vMin : vMax - height;
            double v1 = v0 + height;

            //Horizontal intervals already taken by the panel's apertures where they cross the door's
            //vertical band, with a millimetre's clearance so the new door never touches an existing
            //opening.
            List<Tuple<double, double>> occupied = [];
            foreach (Aperture aperture_Existing in panel.Apertures ?? [])
            {
                Face3D face3D_Aperture = aperture_Existing?.GetFace3D();
                if (face3D_Aperture == null)
                {
                    continue;
                }

                //Coplanar with the panel by construction - Panel.AddAperture enforces it - so conversion
                //into the panel plane is exact.
                BoundingBox2D boundingBox2D_Aperture = plane.Convert(face3D_Aperture)?.GetBoundingBox();
                if (boundingBox2D_Aperture == null)
                {
                    continue;
                }

                double aVMin = verticalIsY ? boundingBox2D_Aperture.Min.Y : boundingBox2D_Aperture.Min.X;
                double aVMax = verticalIsY ? boundingBox2D_Aperture.Max.Y : boundingBox2D_Aperture.Max.X;
                if (aVMax <= v0 + Core.Tolerance.MacroDistance || aVMin >= v1 - Core.Tolerance.MacroDistance)
                {
                    continue;
                }

                double aHMin = (verticalIsY ? boundingBox2D_Aperture.Min.X : boundingBox2D_Aperture.Min.Y) - Core.Tolerance.MacroDistance;
                double aHMax = (verticalIsY ? boundingBox2D_Aperture.Max.X : boundingBox2D_Aperture.Max.Y) + Core.Tolerance.MacroDistance;

                occupied.Add(new Tuple<double, double>(aHMin, aHMax));
            }

            //The clearest free interval of the wall first - the one whose midpoint is nearest the panel
            //centre - with the door clamped inside it, again as close to the panel centre as fits.
            double centre = (hMin + hMax) / 2;

            double h0 = double.NaN;
            foreach (Tuple<double, double> interval in FreeIntervals(hMin, hMax, occupied).OrderBy(x => System.Math.Abs(((x.Item1 + x.Item2) / 2) - centre)))
            {
                if (interval.Item2 - interval.Item1 < width - Core.Tolerance.MacroDistance)
                {
                    continue;
                }

                double c = System.Math.Max(interval.Item1 + (width / 2), System.Math.Min(centre, interval.Item2 - (width / 2)));

                h0 = c - (width / 2);
                break;
            }

            if (double.IsNaN(h0))
            {
                refusal = "no clear length of the wall remains for the door alongside its existing apertures";
                return false;
            }

            double h1 = h0 + width;

            Point2D Point(double h, double v)
            {
                return verticalIsY ? new Point2D(h, v) : new Point2D(v, h);
            }

            Polygon2D polygon2D = new([Point(h0, v0), Point(h1, v0), Point(h1, v1), Point(h0, v1)]);

            polygon3D = plane.Convert(polygon2D);
            if (polygon3D == null)
            {
                refusal = "the door geometry could not be constructed";
                return false;
            }

            //The same host check Panel.AddApertures makes before it accepts an aperture, applied here
            //without touching the panel: a candidate the panel would reject is not a candidate.
            if (!Query.ApertureHost(panel, polygon3D))
            {
                polygon3D = null;
                refusal = "the panel cannot host the door geometry";
                return false;
            }

            return true;
        }

        /// <summary>
        /// How directly a wall panel lies between two spaces: 0 where the segment joining the two space
        /// locations passes through the panel, otherwise the distance between that segment and the panel.
        /// Lower means the panel separates the spaces more directly, so the smallest score is the wall the
        /// missing internal door belongs in. The panel is only read, never modified.
        /// </summary>
        private static double TransferAirDoorPanelScore(Face3D face3D, Segment3D segment3D)
        {
            if (face3D == null || segment3D == null)
            {
                return double.NaN;
            }

            //Degenerate segment: the two space locations coincide. The score is the distance of that
            //single point from the panel REGION - the perpendicular offset where the point's projection
            //onto the panel plane falls inside the panel, the distance to the panel's edge otherwise -
            //so a point facing the middle of a large wall is not scored by how far the wall's edges are.
            if (segment3D.GetLength() < Core.Tolerance.Distance)
            {
                return DistanceToPanel(face3D, segment3D[0]);
            }

            //The direct line between the spaces passes THROUGH the panel: that panel is the wall the two
            //spaces meet through, and nothing can score lower than 0.
            PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(face3D, segment3D);
            if (planarIntersectionResult != null && planarIntersectionResult.Intersecting)
            {
                return 0;
            }

            Plane plane = face3D.GetPlane();
            if (plane == null)
            {
                return double.NaN;
            }

            //A wall genuinely between two spaces is crossed by the line joining their locations. Where
            //that line runs parallel to the panel plane, the panel does not separate the two locations
            //(they stand at the same perpendicular offset); such a panel can still take the door, so it
            //is scored by how far it stands off the line - the perpendicular offset where the line's
            //projection overlaps the panel, the distance to the panel's edge otherwise.
            if (System.Math.Abs(plane.Normal.DotProduct(segment3D.Direction)) < Core.Tolerance.Distance)
            {
                double distance_Perpendicular = plane.Distance(segment3D[0]);
                if (double.IsNaN(distance_Perpendicular))
                {
                    return double.NaN;
                }

                Segment3D segment3D_Projected = plane.Project(segment3D);
                Face2D face2D = segment3D_Projected == null ? null : plane.Convert(face3D);
                Segment2D segment2D = segment3D_Projected == null ? null : plane.Convert(segment3D_Projected);
                if (face2D != null && segment2D != null)
                {
                    List<ISAMGeometry2D> geometry2Ds = Geometry.Planar.Query.Intersection<ISAMGeometry2D>(face2D, segment2D, Core.Tolerance.Distance);
                    if (geometry2Ds != null && geometry2Ds.Count != 0)
                    {
                        return distance_Perpendicular;
                    }
                }

                return (face3D.GetExternalEdge3D() as ISegmentable3D)?.Distance(segment3D) ?? double.NaN;
            }

            //The line crosses the panel plane at a point outside the panel: the panel stands as far off
            //the direct path as that crossing point stands from the panel.
            Point3D point3D_Intersection = Geometry.Spatial.Create.PlanarIntersectionResult(plane, segment3D)?.GetGeometry3D<Point3D>();
            if (point3D_Intersection == null)
            {
                //The line crosses the panel plane BEYOND the bounded segment: the panel does not stand
                //between the two locations, and the closest the segment comes to it is at its nearer
                //endpoint. That stays a finite, rankable score - such a wall simply loses to any wall
                //the direct line actually reaches.
                return plane.Distance(segment3D[0]) <= plane.Distance(segment3D[1])
                    ? DistanceToPanel(face3D, segment3D[0])
                    : DistanceToPanel(face3D, segment3D[1]);
            }

            return DistanceToPanel(face3D, point3D_Intersection);
        }

        /// <summary>
        /// The distance from a point to a wall panel region: the perpendicular offset where the point's
        /// projection onto the panel plane falls inside the panel, the distance to the panel's edge
        /// otherwise. 0 where the point lies on the panel itself. The panel is only read, never modified.
        /// </summary>
        private static double DistanceToPanel(Face3D face3D, Point3D point3D)
        {
            if (face3D == null || point3D == null)
            {
                return double.NaN;
            }

            Plane plane = face3D.GetPlane();
            if (plane == null)
            {
                return double.NaN;
            }

            double distance_Perpendicular = plane.Distance(point3D);
            if (double.IsNaN(distance_Perpendicular))
            {
                return double.NaN;
            }

            if (distance_Perpendicular <= Core.Tolerance.Distance)
            {
                //On the panel plane: the point is either inside the panel (0) or off to its side (the
                //distance to the edge, which for a point on the plane is the exact region distance).
                return face3D.Inside(point3D) ? 0 : (face3D.GetExternalEdge3D() as ISegmentable3D)?.Distance(point3D) ?? double.NaN;
            }

            //Off the panel plane: where the point faces the panel interior, the perpendicular offset IS
            //the region distance - measuring to the edge instead would score a point facing the middle of
            //a large wall by how far its edges are. Only where the projection falls outside the panel is
            //the nearest region point on the edge.
            Point3D point3D_Projected = plane.Project(point3D);
            if (point3D_Projected != null && face3D.Inside(point3D_Projected))
            {
                return distance_Perpendicular;
            }

            return (face3D.GetExternalEdge3D() as ISegmentable3D)?.Distance(point3D) ?? double.NaN;
        }

        /// <summary>
        /// The horizontal length [m] of a wall panel - the length of its bottom edge, the span the door's
        /// sill sits along. NaN where the panel carries no usable geometry. Used only to break a geometric
        /// tie between candidate walls: the shorter wall is the one the door fits more closely.
        /// </summary>
        private static double WallLength(Panel panel)
        {
            IClosedPlanar3D closedPlanar3D = panel?.GetFace3D()?.GetExternalEdge3D();
            if (closedPlanar3D == null)
            {
                return double.NaN;
            }

            List<Point3D> point3Ds = (closedPlanar3D as ISegmentable3D)?.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
            {
                return double.NaN;
            }

            //The bottom edge of the wall: the edge the door stands on. Its length is the horizontal
            //span of the panel.
            double z_Min = point3Ds.Min(x => x.Z);
            List<Point3D> point3Ds_Bottom = point3Ds.FindAll(x => System.Math.Abs(x.Z - z_Min) <= Core.Tolerance.Distance);

            double result = 0;
            for (int i = 0; i < point3Ds_Bottom.Count; i++)
            {
                for (int j = i + 1; j < point3Ds_Bottom.Count; j++)
                {
                    double distance = point3Ds_Bottom[i].Distance(point3Ds_Bottom[j]);
                    if (distance > result)
                    {
                        result = distance;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The complement of the occupied intervals within [min, max], in order, each wide enough to
        /// notice.
        /// </summary>
        private static List<Tuple<double, double>> FreeIntervals(double min, double max, List<Tuple<double, double>> occupied)
        {
            List<Tuple<double, double>> result = [];

            double start = min;
            foreach (Tuple<double, double> interval in (occupied ?? []).OrderBy(x => x.Item1))
            {
                double end = System.Math.Min(System.Math.Max(interval.Item1, min), max);
                if (end - start > Core.Tolerance.MacroDistance)
                {
                    result.Add(new Tuple<double, double>(start, end));
                }

                start = System.Math.Max(start, System.Math.Min(interval.Item2, max));
            }

            if (max - start > Core.Tolerance.MacroDistance)
            {
                result.Add(new Tuple<double, double>(start, max));
            }

            return result;
        }
    }
}
