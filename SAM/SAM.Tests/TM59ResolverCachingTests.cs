// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// TM59Manager deliberately reuses one TM59InternalConditionResolver instance across many Resolve
    /// calls over a live, mutable model - these lock that its per-Space and per-Zone caching does not
    /// go stale when the model changes between two calls on the same resolver.
    /// </summary>
    public class TM59ResolverCachingTests
    {
        [Fact]
        public void Classify_Reflects_A_Renamed_Space_With_The_Same_Guid_Not_A_Stale_Cached_Result()
        {
            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();
            Guid guid = Guid.NewGuid();

            Space bedroom = new Space(guid, "Bedroom 1", null);
            Assert.Equal(TM59SpaceClassification.Bedroom, resolver.Classify(bedroom));

            // Space.Name has no setter - a caller "renames" a Space by constructing a new instance
            // that reuses the same Guid, exactly as some SAM callers do when applying edits.
            Space renamed = new Space(guid, "Corridor 1", null);
            Assert.Equal(TM59SpaceClassification.NonHabitable, resolver.Classify(renamed));
        }

        [Fact]
        public void Resolve_Reflects_A_Space_Added_To_A_Zone_After_An_Earlier_Resolve_On_The_Same_Resolver()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Space bedroom1 = new Space(Guid.NewGuid(), "Bedroom 1", null);
            Space bedroom2 = new Space(Guid.NewGuid(), "Bedroom 2", null);
            adjacencyCluster.AddObject(bedroom1);
            adjacencyCluster.AddObject(bedroom2);
            adjacencyCluster.UpdateZone("Flat 1", "Flats", bedroom1, bedroom2);

            TM59InternalConditionResolver resolver = TM59TestData.NewResolver();

            TM59InternalConditionResult firstResult = resolver.Resolve(adjacencyCluster, bedroom1, "Flats");
            Assert.Equal(2, firstResult.BedroomCount);

            // Add a third bedroom to the SAME zone after the resolver has already resolved against it -
            // a memoized flat-spaces snapshot would still report 2 here.
            Space bedroom3 = new Space(Guid.NewGuid(), "Bedroom 3", null);
            adjacencyCluster.AddObject(bedroom3);
            adjacencyCluster.UpdateZone("Flat 1", "Flats", bedroom3);

            TM59InternalConditionResult secondResult = resolver.Resolve(adjacencyCluster, bedroom1, "Flats");
            Assert.Equal(3, secondResult.BedroomCount);
        }
    }
}
