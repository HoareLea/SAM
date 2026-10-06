// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>A declared deep clone owns EVERY object in the cluster, or it fails.</b>
    /// <para>
    /// <c>AnalyticalModel(AnalyticalModel, deepClone: true)</c> is the authority the working-model ownership
    /// rule rests on - see <see cref="AnalyticalModelWorkingCopyTests"/>. It delegates to
    /// <c>SAMObjectRelationCluster(..., true)</c>, which replaces each stored object with
    /// <c>Core.Query.Clone</c> of itself.
    /// </para>
    /// <para>
    /// <c>Core.Query.Clone</c> resolves by reflection - an instance <c>Clone()</c>, else a single-argument
    /// constructor accepting the type, else a parameterless one - and returns null when it finds none.
    /// <b>Constructors are not inherited</b>, so a subclass of a type that has a copy constructor does not
    /// have one. Seven types accepted by <c>AdjacencyCluster.IsValid</c> were in exactly that position -
    /// <c>ZoneSimulationResult</c> and the six <c>TM5x</c> results - and for them the null clone was handed
    /// to <c>AddObject</c>, rejected, and the ORIGINAL instance left in the dictionary the shallow base
    /// constructor had already filled. The caller had asked for a copy that owns its objects and silently
    /// did not get one, so an in-place write through the "deep" model reached the model it came from.
    /// </para>
    /// <para>
    /// The first test is the one that matters, because it is <b>exhaustive over the accepted types rather
    /// than over the ones somebody thought to write a fixture for</b>: it enumerates every concrete
    /// <c>IJSAMObject</c> the cluster accepts and asserts <c>Core.Query.Clone</c> can resolve a path for it.
    /// A type added later with no copy support fails here rather than in a Part O run.
    /// </para>
    /// </summary>
    public class AdjacencyClusterDeepCloneTests
    {
        private static readonly Construction construction_Wall = new(Guid.NewGuid(), "Wall");
        private static readonly ApertureConstruction apertureConstruction_Window = new(Guid.NewGuid(), "Window", ApertureType.Window);

        private const string parameterName_Probe = "Test Deep Clone Probe";

        private static Point3D P(double x, double y, double z) => new(x, y, z);

        /// <summary>
        /// Exactly <c>Core.Query.Clone</c>'s resolution rules, asked of a type rather than an instance - so
        /// the audit needs no way to construct one of everything.
        /// </summary>
        private static bool CanClone(Type type)
        {
            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                if (!methodInfo.Name.Equals("Clone"))
                {
                    continue;
                }

                if (methodInfo.ReturnType.IsAssignableFrom(type) && methodInfo.GetParameters().Length == 0)
                {
                    return true;
                }
            }

            foreach (ConstructorInfo constructorInfo in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();

                if (parameterInfos.Length == 0)
                {
                    return true;
                }

                if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Every concrete cluster-storable type this build has.</summary>
        private static List<Type> AcceptedTypes()
        {
            AdjacencyCluster adjacencyCluster = new();

            List<Type> result = [];

            foreach (Assembly assembly in new[] { typeof(AdjacencyCluster).Assembly, typeof(IJSAMObject).Assembly, typeof(Architectural.Level).Assembly })
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                    {
                        continue;
                    }

                    if (typeof(IJSAMObject).IsAssignableFrom(type) && adjacencyCluster.IsValid(type))
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }

        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>The exhaustive one.</b> Every type the cluster will store can be cloned, so a deep copy of any
        /// model this build can produce owns all of it.
        /// </summary>
        [Fact]
        public void EveryTypeTheClusterAccepts_CanBeCloned()
        {
            List<Type> types = AcceptedTypes();

            //A sanity floor: if the reflection ever stops finding types, the audit below would pass by
            //examining nothing.
            Assert.True(types.Count > 40, string.Format("Only {0} accepted types were found; the audit is not looking at the model.", types.Count));

            List<string> uncloneable = [.. types.Where(x => !CanClone(x)).Select(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal)];

            Assert.True(uncloneable.Count == 0, string.Format(
                "{0} type(s) accepted by AdjacencyCluster.IsValid cannot be cloned, so a deep copy would share them with its source. Give each a copy constructor:\n{1}",
                uncloneable.Count,
                string.Join("\n", uncloneable)));
        }

        /// <summary>
        /// The seven that were missing one, named individually so a regression on any of them reads as
        /// itself rather than as a count.
        /// </summary>
        [Theory]
        [InlineData(typeof(ZoneSimulationResult))]
        [InlineData(typeof(TM52Result))]
        [InlineData(typeof(TM59Result))]
        [InlineData(typeof(TM59CorridorResult))]
        [InlineData(typeof(TM59MechanicalVentilationResult))]
        [InlineData(typeof(TM59NaturalVentilationResult))]
        [InlineData(typeof(TM59NaturalVentilationBedroomResult))]
        public void TheResultTypesThatHadNoCopyConstructor_CanBeCloned(Type type)
        {
            Assert.True(CanClone(type), string.Format("{0} has lost its copy constructor.", type.FullName));
        }

        /// <summary>
        /// A cluster carrying one of every category it commonly holds is deep-copied, and <b>every</b> object
        /// in it is the copy's own: a different instance, the same identity, and a parameter written on it
        /// does not reach the source. Walked generically rather than asserted type by type, so an object
        /// added to the fixture is covered without a new assertion.
        /// </summary>
        [Fact]
        public void DeepCopy_OwnsEveryObjectInTheCluster()
        {
            AnalyticalModel analyticalModel_Source = Model();

            AnalyticalModel analyticalModel_Working = new(analyticalModel_Source, true);

            AdjacencyCluster adjacencyCluster_Source = analyticalModel_Source.AdjacencyCluster;
            AdjacencyCluster adjacencyCluster_Working = analyticalModel_Working.AdjacencyCluster;

            List<IJSAMObject> objects_Source = adjacencyCluster_Source.GetObjects();
            List<IJSAMObject> objects_Working = adjacencyCluster_Working.GetObjects();

            Assert.Equal(objects_Source.Count, objects_Working.Count);
            Assert.True(objects_Source.Count >= 7, "The fixture stopped covering the categories it is for.");

            foreach (IJSAMObject object_Working in objects_Working)
            {
                Guid guid = ((SAMObject)object_Working).Guid;

                IJSAMObject object_Source = objects_Source.Find(x => ((SAMObject)x).Guid == guid);

                Assert.True(object_Source != null, string.Format("The deep copy lost '{0}'.", object_Working.GetType().FullName));

                //Same identity - relations are keyed by guid, so a clone that renumbered would orphan them.
                Assert.Equal(object_Source.GetType(), object_Working.GetType());

                //A different instance. This is what was false for the seven result types.
                Assert.False(ReferenceEquals(object_Source, object_Working), string.Format(
                    "The deep copy shares its '{0}' with the source.", object_Working.GetType().FullName));

                //And the separation is real, not just two wrappers over one parameter set.
                ((ParameterizedSAMObject)object_Working).SetValue(parameterName_Probe, "leaked");

                Assert.False(((ParameterizedSAMObject)object_Source).TryGetValue(parameterName_Probe, out object _), string.Format(
                    "A write on the deep copy's '{0}' reached the source.", object_Working.GetType().FullName));
            }
        }

        /// <summary>Relations survive a deep copy of a cluster holding every category.</summary>
        [Fact]
        public void DeepCopy_PreservesRelations()
        {
            AnalyticalModel analyticalModel_Source = Model();

            AdjacencyCluster adjacencyCluster_Working = new AnalyticalModel(analyticalModel_Source, true).AdjacencyCluster;

            List<Space> spaces = adjacencyCluster_Working.GetSpaces();
            Assert.Single(spaces);

            //Space to panel, and zone to space - both directions the conversion reads.
            Assert.Single(adjacencyCluster_Working.GetPanels(spaces[0]));

            List<Zone> zones = adjacencyCluster_Working.GetObjects<Zone>();
            Assert.Single(zones);
            Assert.Single(adjacencyCluster_Working.GetSpaces(zones[0]));
        }

        /// <summary>
        /// <b>A deep clone that cannot be delivered throws rather than quietly sharing.</b>
        /// <para>
        /// This is the guard that makes the audit above a safety net rather than the only defence: a type
        /// added later with no copy support is refused at the copy, naming itself, instead of producing a
        /// model that silently shares one of its objects with its source.
        /// </para>
        /// </summary>
        [Fact]
        public void DeepCopy_ThrowsRatherThanSharing_WhenAnObjectCannotBeCloned()
        {
            AdjacencyCluster adjacencyCluster = new();
            adjacencyCluster.AddObject(new UncloneableResult("Nothing can copy me"));

            AnalyticalModel analyticalModel = new("Flat1", null, null, null, adjacencyCluster);

            InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(
                () => new AnalyticalModel(analyticalModel, true));

            Assert.Contains(typeof(UncloneableResult).FullName, invalidOperationException.Message);

            //The shallow copy is unaffected - only a DECLARED deep copy makes this promise.
            AnalyticalModel analyticalModel_Shallow = new(analyticalModel);
            Assert.Single(analyticalModel_Shallow.AdjacencyCluster.GetObjects<UncloneableResult>());
        }

        /// <summary>
        /// <b>An object with no Guid of its own is replaced by its clone, not joined by it.</b>
        /// <para>
        /// A <c>DesignDay</c> is a <c>WeatherDay</c>, not a <c>SAMObject</c>, so the cluster keys it by a Guid
        /// it generated. The deep copy re-added each clone through <c>AddObject</c>, which could not find that
        /// key again (a clone is not <c>Equals</c> its original) and stored the clone under a NEW key beside
        /// the original - doubling every design day on every deep copy and still sharing the originals. The
        /// Part O 2B live run (26 Sep 2026) grew from 12 design days to 57,342 over eleven rounds this way,
        /// one deep copy per TAS run.
        /// </para>
        /// </summary>
        [Fact]
        public void DeepCopy_OfObjectsWithNoGuid_NeitherDuplicatesNorSharesThem()
        {
            AdjacencyCluster adjacencyCluster = new();

            Zone zone = new("Flat 1");
            adjacencyCluster.AddObject(zone);

            DesignDay designDay_Cooling = new(new DesignDay("Cooling", 2018, 7, 1), LoadType.Cooling);
            DesignDay designDay_Heating = new(new DesignDay("Heating", 2018, 1, 1), LoadType.Heating);
            adjacencyCluster.AddObject(designDay_Cooling);
            adjacencyCluster.AddObject(designDay_Heating);
            adjacencyCluster.AddRelation(zone, designDay_Cooling);

            Guid guid_Cooling = adjacencyCluster.GetGuid(designDay_Cooling);
            Guid guid_Heating = adjacencyCluster.GetGuid(designDay_Heating);

            AnalyticalModel analyticalModel_Source = new("Flat1", null, null, null, adjacencyCluster);

            //Repeated, as every TAS run in an optimisation does to the model the previous run returned.
            AnalyticalModel analyticalModel_Working = analyticalModel_Source;
            for (int i = 0; i < 5; i++)
            {
                analyticalModel_Working = new AnalyticalModel(analyticalModel_Working, true);
            }

            AdjacencyCluster adjacencyCluster_Working = analyticalModel_Working.AdjacencyCluster;

            List<DesignDay> designDays_Working = adjacencyCluster_Working.GetObjects<DesignDay>();
            Assert.Equal(2, designDays_Working.Count);

            //Same keys, so anything related by Guid still finds them.
            Assert.Equal(new HashSet<Guid> { guid_Cooling, guid_Heating }, designDays_Working.ConvertAll(adjacencyCluster_Working.GetGuid).ToHashSet());

            //Owned, not shared.
            DesignDay designDay_Cooling_Working = adjacencyCluster_Working.GetObject<DesignDay>(guid_Cooling);
            Assert.NotNull(designDay_Cooling_Working);
            Assert.False(ReferenceEquals(designDay_Cooling, designDay_Cooling_Working));
            Assert.False(ReferenceEquals(designDay_Heating, adjacencyCluster_Working.GetObject<DesignDay>(guid_Heating)));
            Assert.Equal("Cooling", designDay_Cooling_Working.Name);

            //And the relation follows the copy's own instance.
            List<DesignDay> designDays_Related = adjacencyCluster_Working.GetRelatedObjects<DesignDay>(adjacencyCluster_Working.GetObjects<Zone>()[0]);
            Assert.Single(designDays_Related);
            Assert.True(ReferenceEquals(designDay_Cooling_Working, designDays_Related[0]));

            //The source is untouched.
            Assert.Equal(2, adjacencyCluster.GetObjects<DesignDay>().Count);
            Assert.True(ReferenceEquals(designDay_Cooling, adjacencyCluster.GetObject<DesignDay>(guid_Cooling)));
        }

        // -----------------------------------------------------------------------------------------------

        /// <summary>
        /// One of every category the cluster commonly holds, including the result types the deep clone used
        /// to share.
        /// </summary>
        private static AnalyticalModel Model()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = new("Bedroom 1", P(5, 5, 1.5));

            Face3D face3D_Panel = new(new Polygon3D(new List<Point3D> { P(0, 0, 0), P(10, 0, 0), P(10, 0, 3), P(0, 0, 3) }));
            Panel panel = AnalyticalCreate.Panel(construction_Wall, PanelType.Wall, face3D_Panel);

            Face3D face3D_Aperture = new(new Polygon3D(new List<Point3D> { P(2, 0, 1), P(4, 0, 1), P(4, 0, 2), P(2, 0, 2) }));
            panel.AddAperture(AnalyticalCreate.Aperture(apertureConstruction_Window, face3D_Aperture));

            Zone zone = new("Flat 1");

            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(panel);
            adjacencyCluster.AddObject(zone);
            adjacencyCluster.AddRelation(space, panel);
            adjacencyCluster.AddRelation(zone, space);

            //The definitions.
            adjacencyCluster.AddObject(construction_Wall);
            adjacencyCluster.AddObject(apertureConstruction_Window);

            //The results - the category that was not being cloned.
            adjacencyCluster.AddObject(new ZoneSimulationResult("Flat 1", "test", "zone-1"));
            adjacencyCluster.AddObject(new SpaceSimulationResult("Bedroom 1", "test", "space-1"));
            adjacencyCluster.AddObject(new TM59CorridorResult("Corridor", "test", "space-2", TM52BuildingCategory.CategoryII, 100, 10, 3, true, 8760));
            adjacencyCluster.AddObject(new TM59MechanicalVentilationResult("Bedroom 1", "test", "space-1", TM52BuildingCategory.CategoryII, 100, 10, 4, true, TM59SpaceApplication.Sleeping));

            return new AnalyticalModel("Flat1", null, null, null, adjacencyCluster);
        }

        /// <summary>
        /// A cluster-storable type with no copy support of any kind - <c>Result</c> is accepted by
        /// <c>AdjacencyCluster.IsValid</c>, and this declares no <c>Clone()</c>, no parameterless
        /// constructor and no constructor taking itself. Exactly the shape the seven result types were in.
        /// </summary>
        private sealed class UncloneableResult : Result
        {
            public UncloneableResult(string name)
                : base(name, "test", "uncloneable")
            {

            }
        }
    }
}
