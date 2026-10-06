// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>The working-copy ownership rule.</b>
    /// <para>
    /// <c>new AnalyticalModel(analyticalModel)</c> and <see cref="AnalyticalModel.AdjacencyCluster"/> both
    /// rebuild the cluster's dictionaries but store <b>the same</b> <see cref="Space"/>,
    /// <see cref="Panel"/> and relation-cluster object instances - see
    /// <c>RelationCluster(RelationCluster&lt;X&gt;)</c>, which copies
    /// <c>dictionary[key] = keyValuePair.Value</c>. That is deliberate and it is safe for every operation in
    /// this assembly that writes by same-guid REPLACEMENT: <c>Modify.EvaluateTargetedDesignAirFlows</c>
    /// states exactly that rule at its own boundary, and <c>Query.GetObjects</c> handing back live instances
    /// is what makes a replacement cheap.
    /// </para>
    /// <para>
    /// It is <b>not</b> safe for an operation that mutates an object <i>in place</i>. A caller that takes a
    /// copy in order to be free to mutate it - the TAS workflow does, and says so - is not isolated by these
    /// constructors at all, and every in-place write it makes is visible through the model it copied from.
    /// </para>
    /// <para>
    /// So the rule this fixture pins is:
    /// </para>
    /// <para>
    /// <i>A simulation or optimisation working model may be mutated freely, but no caller, retained
    /// last-valid model or previously completed run sharing ancestry with it can observe those mutations
    /// unless that working model is explicitly adopted.</i>
    /// </para>
    /// <para>
    /// <c>new AnalyticalModel(analyticalModel, deepClone: true)</c> is the one authority that establishes it,
    /// and it is the constructor a mutating boundary has to take its copy with. The pair of tests on each
    /// object type below is the point: the shallow test is not an aspiration, it is the documented current
    /// behaviour, and it is there so that a future change which makes the shallow copy deep is noticed
    /// rather than silently paid for on every getter.
    /// </para>
    /// </summary>
    public class AnalyticalModelWorkingCopyTests
    {
        private static readonly Construction construction_Wall = new(Guid.NewGuid(), "Wall");
        private static readonly ApertureConstruction apertureConstruction_Window = new(Guid.NewGuid(), "Window", ApertureType.Window);

        //Stand-ins for the TAS identity parameters, which are SAM.Analytical.Tas enums this assembly cannot
        //see. What is being pinned is the OWNERSHIP of the object whose parameter set is written, not the
        //spelling of the parameter, and a string parameter written in place is the same write
        //SAM.Analytical.Tas.Modify.UpdateIds makes on SpaceParameter.ZoneGuid and
        //PanelParameter.ZoneSurfaceReference_1. The SAM_Tas suite pins the real parameters on the real
        //authority.
        private const string parameterName_SpaceIdentity = "Test Zone Guid";
        private const string parameterName_PanelIdentity = "Test Zone Surface Reference";
        private const string parameterName_ApertureIdentity = "Test Aperture BuildingElement Guid";

        private static Point3D P(double x, double y, double z) => new(x, y, z);

        /// <summary>
        /// One space, one wall panel carrying one window, and the space-to-panel relation - the smallest
        /// model that has all three object shapes the TAS conversion stamps identity onto, including an
        /// aperture held inside a panel rather than standing alone in the cluster.
        /// </summary>
        private static AnalyticalModel Model()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = new("Bedroom 1", P(5, 5, 1.5));

            Face3D face3D_Panel = new(new Polygon3D(new List<Point3D> { P(0, 0, 0), P(10, 0, 0), P(10, 0, 3), P(0, 0, 3) }));
            Panel panel = AnalyticalCreate.Panel(construction_Wall, PanelType.Wall, face3D_Panel);

            Face3D face3D_Aperture = new(new Polygon3D(new List<Point3D> { P(2, 0, 1), P(4, 0, 1), P(4, 0, 2), P(2, 0, 2) }));
            panel.AddAperture(AnalyticalCreate.Aperture(apertureConstruction_Window, face3D_Aperture));

            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(panel);
            adjacencyCluster.AddRelation(space, panel);

            return new AnalyticalModel("Flat1", null, null, null, adjacencyCluster);
        }

        /// <summary>
        /// Exactly what <c>SAM.Analytical.Tas.Modify.UpdateIds</c> does, in the same order and by the same
        /// means: read the live objects out of the cluster, write identity onto them <b>in place</b>, and put
        /// them back with <c>AddObject</c>. Nothing here is a replacement - which is the whole point.
        /// </summary>
        private static void StampTasIdentity(AnalyticalModel analyticalModel, string value)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            foreach (Space space in adjacencyCluster.GetSpaces() ?? new List<Space>())
            {
                space.SetValue(parameterName_SpaceIdentity, value);
                adjacencyCluster.AddObject(space);
            }

            foreach (Panel panel in adjacencyCluster.GetPanels() ?? new List<Panel>())
            {
                panel.SetValue(parameterName_PanelIdentity, value);

                foreach (Aperture aperture in panel.Apertures ?? new List<Aperture>())
                {
                    aperture.SetValue(parameterName_ApertureIdentity, value);

                    //The pairing UpdateIds uses to write an aperture back into its panel.
                    panel.RemoveAperture(aperture.Guid);
                    panel.AddAperture(aperture);
                }

                adjacencyCluster.AddObject(panel);
            }
        }

        private static bool HasValue(AnalyticalModel analyticalModel, string parameterName)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            foreach (Space space in adjacencyCluster.GetSpaces() ?? new List<Space>())
            {
                if (space.TryGetValue(parameterName, out object _))
                {
                    return true;
                }
            }

            foreach (Panel panel in adjacencyCluster.GetPanels() ?? new List<Panel>())
            {
                if (panel.TryGetValue(parameterName, out object _))
                {
                    return true;
                }

                foreach (Aperture aperture in panel.Apertures ?? new List<Aperture>())
                {
                    if (aperture.TryGetValue(parameterName, out object _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // -----------------------------------------------------------------------------------------------
        // The current behaviour, stated so a change to it is visible.
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// The shallow copy shares its objects, so an in-place write on the copy is a write on the source.
        /// This is the defect F1 and F3 both reduce to, reproduced at the real boundary.
        /// </summary>
        [Fact]
        public void ShallowCopy_SharesObjects_SoAnInPlaceWriteReachesTheSource()
        {
            AnalyticalModel analyticalModel_Source = Model();

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source);

            StampTasIdentity(analyticalModel_Working, "zone-1");

            //Not an aspiration - the documented consequence of sharing. If this ever fails, the shallow
            //constructor has started copying and the cost of that has to be understood before it ships.
            Assert.True(HasValue(analyticalModel_Source, parameterName_SpaceIdentity));
            Assert.True(HasValue(analyticalModel_Source, parameterName_PanelIdentity));
            Assert.True(HasValue(analyticalModel_Source, parameterName_ApertureIdentity));
        }

        /// <summary>
        /// <b>The shallow copy DOES isolate the model's own state</b> - its name, its libraries, its
        /// parameters - because those are fields of <see cref="AnalyticalModel"/> itself and the copy is a
        /// new instance. Only the cluster's objects are shared.
        /// <para>
        /// This is the half of the shallow copy that is load-bearing at a boundary which renames a model and
        /// then hands it to something that takes its own working copy. <c>Modify.Simulate</c> is that
        /// boundary: it renames to the project name and passes the model to
        /// <c>Modify.RunPartOSimulation</c>, which takes the one deep copy of the Part O run. Cloning
        /// deeply at both would pay twice for one guarantee - and the second copy is thrown away on
        /// adoption - so the rename relies on exactly what is asserted here.
        /// </para>
        /// </summary>
        [Fact]
        public void ShallowCopy_IsolatesTheModelsOwnStateEvenThoughItSharesTheClusterObjects()
        {
            AnalyticalModel analyticalModel_Source = Model();
            analyticalModel_Source.SetValue(AnalyticalModelParameter.NorthAngle, 1.25);

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source)
            {
                Name = "Flat1-Opt01",
            };

            analyticalModel_Working.SetValue(AnalyticalModelParameter.NorthAngle, 99.0);

            //Renaming and re-parameterising the copy left the source alone.
            Assert.Equal("Flat1", analyticalModel_Source.Name);
            Assert.True(analyticalModel_Source.TryGetValue(AnalyticalModelParameter.NorthAngle, out double northAngle));
            Assert.Equal(1.25, northAngle);

            //Identity is carried over, which is why a rename is not a new model.
            Assert.Equal(analyticalModel_Source.Guid, analyticalModel_Working.Guid);

            //And the cluster objects are still shared - stated here so the two halves of the shallow copy's
            //behaviour are read together rather than one of them being assumed from the other.
            Assert.True(ReferenceEquals(
                analyticalModel_Source.AdjacencyCluster.GetSpaces()[0],
                analyticalModel_Working.AdjacencyCluster.GetSpaces()[0]));
        }

        // -----------------------------------------------------------------------------------------------
        // The rule.
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Test 1.</b> A working model receiving new TAS identity does not change the model it was copied
        /// from - the space, the panel, and the aperture held inside the panel.
        /// </summary>
        [Fact]
        public void DeepCopy_IsolatesEveryStampedObjectFromTheSource()
        {
            AnalyticalModel analyticalModel_Source = Model();

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source, true);

            StampTasIdentity(analyticalModel_Working, "zone-1");

            Assert.False(HasValue(analyticalModel_Source, parameterName_SpaceIdentity));
            Assert.False(HasValue(analyticalModel_Source, parameterName_PanelIdentity));
            Assert.False(HasValue(analyticalModel_Source, parameterName_ApertureIdentity));

            //And the working model really was stamped, so the isolation is not the stamp having failed.
            Assert.True(HasValue(analyticalModel_Working, parameterName_SpaceIdentity));
            Assert.True(HasValue(analyticalModel_Working, parameterName_PanelIdentity));
            Assert.True(HasValue(analyticalModel_Working, parameterName_ApertureIdentity));
        }

        /// <summary>
        /// <b>Test 2, at this level.</b> The persisted provenance record is what a reopened model is paired
        /// with its results by, so the fingerprint - not just the parameter - has to survive a working
        /// model's mutation. A retained last-valid design whose fingerprint moved is exactly the false "the
        /// model has changed since the simulation results were produced from it" this closes.
        /// </summary>
        [Fact]
        public void DeepCopy_LeavesTheSourceFingerprintAndProvenanceIntact()
        {
            AnalyticalModel analyticalModel_Source = Model();

            string fingerprint_Before = SimulationResultProvenance.Fingerprint(analyticalModel_Source);

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source, true);

            StampTasIdentity(analyticalModel_Working, "zone-1");

            Assert.Equal(fingerprint_Before, SimulationResultProvenance.Fingerprint(analyticalModel_Source));

            //The working model's own fingerprint DID move, which is what makes the equality above a
            //statement about isolation rather than about the stamp being a no-op.
            Assert.NotEqual(fingerprint_Before, SimulationResultProvenance.Fingerprint(analyticalModel_Working));
        }

        /// <summary>
        /// The relations survive the deep copy. <c>AddObject</c> replaces by guid and relations are held in
        /// their own guid-keyed dictionary, so a clone carrying the same guid steps into the same relation -
        /// but a deep copy that quietly dropped the space-to-panel relation would break every
        /// <c>GetPanels(space)</c> read the conversion makes, so it is asserted rather than assumed.
        /// </summary>
        [Fact]
        public void DeepCopy_PreservesRelationsAndApertureOwnership()
        {
            AnalyticalModel analyticalModel_Source = Model();

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source, true);

            AdjacencyCluster adjacencyCluster = analyticalModel_Working.AdjacencyCluster;

            List<Space> spaces = adjacencyCluster.GetSpaces();
            Assert.Single(spaces);

            List<Panel> panels = adjacencyCluster.GetPanels(spaces[0]);
            Assert.NotNull(panels);
            Assert.Single(panels);

            //The aperture is still held by the panel that owned it, under its own guid.
            List<Aperture> apertures = panels[0].Apertures;
            Assert.NotNull(apertures);
            Assert.Single(apertures);

            //Same identity, different instance: guids are preserved (relations depend on it) and the object
            //is the working model's own.
            AdjacencyCluster adjacencyCluster_Source = analyticalModel_Source.AdjacencyCluster;
            List<Space> spaces_Source = adjacencyCluster_Source.GetSpaces();

            Assert.Equal(spaces_Source[0].Guid, spaces[0].Guid);
            Assert.False(ReferenceEquals(spaces_Source[0], spaces[0]));

            List<Panel> panels_Source = adjacencyCluster_Source.GetPanels();
            Assert.Equal(panels_Source[0].Guid, panels[0].Guid);
            Assert.False(ReferenceEquals(panels_Source[0], panels[0]));
        }

        /// <summary>
        /// The deep copy carries the model's own non-cluster state across unchanged - the libraries and the
        /// model parameters the conversion reads. A working copy that lost its material library would refuse
        /// at the pre-simulation gate rather than simulate.
        /// </summary>
        [Fact]
        public void DeepCopy_CarriesTheModelsOwnStateAcross()
        {
            AnalyticalModel analyticalModel_Source = Model();
            analyticalModel_Source.SetValue(AnalyticalModelParameter.NorthAngle, 1.25);

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source, true);

            Assert.Equal(analyticalModel_Source.Guid, analyticalModel_Working.Guid);
            Assert.Equal(analyticalModel_Source.Name, analyticalModel_Working.Name);

            Assert.True(analyticalModel_Working.TryGetValue(AnalyticalModelParameter.NorthAngle, out double northAngle));
            Assert.Equal(1.25, northAngle);
        }
    }
}
