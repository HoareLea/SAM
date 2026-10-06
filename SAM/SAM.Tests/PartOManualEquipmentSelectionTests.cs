// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>A hand-picked ventilation unit is Part O design input, not run output.</b>
    /// <para>
    /// Under manual authority the engineer chooses each dwelling's product. That choice used to live only on the air
    /// handling unit a preparation builds - run output - so it reached the next case only when that case was prepared
    /// from the previous result. <see cref="PartOManualEquipmentSelection"/> is its design-model representation, keyed
    /// by dwelling zone guid, and <c>Modify.PreparePartOIteration</c> materialises it onto the units it builds.
    /// </para>
    /// </summary>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartOManualEquipmentSelectionTests
    {
        private static readonly VentilationUnitReference product_A = new("Test Fixture", "MVHR-A", null);

        private static readonly VentilationUnitReference product_B = new("Test Fixture", "MVHR-B", "B-01");

        // ---- The representation ------------------------------------------------------------------------------

        /// <summary>Keyed by zone guid, one product each; set replaces, remove clears, and nothing else is a choice.</summary>
        [Fact]
        public void A_dwelling_has_at_most_one_hand_picked_product()
        {
            Guid guid_1 = Guid.NewGuid();
            Guid guid_2 = Guid.NewGuid();

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();

            Assert.True(partOManualEquipmentSelection.Set(guid_1, product_A));
            Assert.True(partOManualEquipmentSelection.Set(guid_2, product_A));
            Assert.True(partOManualEquipmentSelection.Set(guid_2, product_B));

            Assert.Equal(2, partOManualEquipmentSelection.Count);
            Assert.Equal(0, VentilationUnitReference.Compare(product_A, partOManualEquipmentSelection.Product(guid_1)));
            Assert.Equal(0, VentilationUnitReference.Compare(product_B, partOManualEquipmentSelection.Product(guid_2)));

            Assert.True(partOManualEquipmentSelection.Remove(guid_1));
            Assert.Null(partOManualEquipmentSelection.Product(guid_1));
            Assert.Equal(1, partOManualEquipmentSelection.Count);

            //No dwelling, or a product that identifies nothing, is not a choice.
            Assert.False(partOManualEquipmentSelection.Set(Guid.Empty, product_A));
            Assert.False(partOManualEquipmentSelection.Set(guid_1, new VentilationUnitReference(null, null, null)));
            Assert.False(partOManualEquipmentSelection.Set(guid_1, null));
            Assert.Equal(1, partOManualEquipmentSelection.Count);

            //A copy out: the stored identity cannot be edited through what was returned.
            VentilationUnitReference ventilationUnitReference = partOManualEquipmentSelection.Product(guid_2);
            ventilationUnitReference.Model = "changed";
            Assert.Equal("MVHR-B", partOManualEquipmentSelection.Product(guid_2).Model);
        }

        /// <summary>
        /// Canonical and lossless: two selections made in different orders write the same JSON, it survives the
        /// model's own serialisation, and <see cref="PartOManualEquipmentSelection.Matches"/> agrees.
        /// </summary>
        [Fact]
        public void The_selection_round_trips_canonically_on_the_model()
        {
            Guid guid_1 = new("11111111-1111-1111-1111-111111111111");
            Guid guid_2 = new("22222222-2222-2222-2222-222222222222");

            PartOManualEquipmentSelection selection_Forward = new();
            selection_Forward.Set(guid_1, product_A);
            selection_Forward.Set(guid_2, product_B);

            PartOManualEquipmentSelection selection_Reverse = new();
            selection_Reverse.Set(guid_2, new VentilationUnitReference(product_B));
            selection_Reverse.Set(guid_1, new VentilationUnitReference(product_A));

            Assert.Equal(selection_Forward.ToJsonObject().ToJsonString(), selection_Reverse.ToJsonObject().ToJsonString());
            Assert.True(selection_Forward.Matches(selection_Reverse));

            AnalyticalModel analyticalModel = new("Manual", null, null, null, new AdjacencyCluster(), null, null);
            Assert.True(analyticalModel.SetValue(AnalyticalModelParameter.PartOManualEquipmentSelection, selection_Forward));

            AnalyticalModel analyticalModel_Read = new(analyticalModel.ToJsonObject());
            PartOManualEquipmentSelection selection_Read = analyticalModel_Read.GetValue<PartOManualEquipmentSelection>(AnalyticalModelParameter.PartOManualEquipmentSelection);

            Assert.NotNull(selection_Read);
            Assert.True(selection_Read.IsValid);
            Assert.True(selection_Read.Matches(selection_Forward));
            Assert.Equal(selection_Forward.ToJsonObject().ToJsonString(), selection_Read.ToJsonObject().ToJsonString());

            PartOManualEquipmentSelection selection_Changed = new(selection_Forward);
            selection_Changed.Set(guid_2, product_A);
            Assert.False(selection_Changed.Matches(selection_Forward));
        }

        /// <summary>A model without the parameter has no hand-picked product; a later schema loads as invalid.</summary>
        [Fact]
        public void Absent_is_no_choice_and_an_unknown_schema_is_not_reinterpreted()
        {
            AnalyticalModel analyticalModel = new("Legacy", null, null, null, new AdjacencyCluster(), null, null);
            Assert.Null(analyticalModel.GetValue<PartOManualEquipmentSelection>(AnalyticalModelParameter.PartOManualEquipmentSelection));

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(Guid.NewGuid(), product_A);

            System.Text.Json.Nodes.JsonObject jsonObject = partOManualEquipmentSelection.ToJsonObject();
            jsonObject["Schema"] = "PartOManualEquipmentSelection:v99";

            PartOManualEquipmentSelection partOManualEquipmentSelection_Read = new(jsonObject);
            Assert.False(partOManualEquipmentSelection_Read.IsValid);
        }

        /// <summary>
        /// It is design input: a design carrying it is still a clean Part O baseline, and removing Part O run state
        /// (Remove Results) keeps it.
        /// </summary>
        [Fact]
        public void The_selection_is_design_input_not_run_state()
        {
            AnalyticalModel analyticalModel = TwoDwellingModel(out Zone zone_1, out Zone _);

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(zone_1.Guid, product_A);
            Assert.True(analyticalModel.SetValue(AnalyticalModelParameter.PartOManualEquipmentSelection, partOManualEquipmentSelection));

            Assert.Empty(analyticalModel.PartOBaselineFindings());

            AnalyticalModel analyticalModel_Cleaned = analyticalModel.RemovePartORunState(out List<string> _, out List<string> _);
            PartOManualEquipmentSelection partOManualEquipmentSelection_Kept = analyticalModel_Cleaned.GetValue<PartOManualEquipmentSelection>(AnalyticalModelParameter.PartOManualEquipmentSelection);

            Assert.NotNull(partOManualEquipmentSelection_Kept);
            Assert.True(partOManualEquipmentSelection_Kept.Matches(partOManualEquipmentSelection));
        }

        // ---- The preparation ---------------------------------------------------------------------------------

        /// <summary>
        /// Manual authority (no catalogue): each dwelling's built unit receives exactly its own hand-picked product,
        /// found by zone identity, and the preparation says which unit belongs to which dwelling. The model handed
        /// in - the design - is not changed.
        /// </summary>
        [Fact]
        public void Manual_preparation_materialises_each_dwellings_own_product()
        {
            AnalyticalModel analyticalModel = TwoDwellingModel(out Zone zone_1, out Zone zone_2);
            string json_Before = analyticalModel.ToJsonObject().ToJsonString();

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(zone_1.Guid, product_A);
            partOManualEquipmentSelection.Set(zone_2.Guid, product_B);

            PartOIterationPreparation partOIterationPreparation = Prepare(analyticalModel, null, partOManualEquipmentSelection);

            Assert.Null(partOIterationPreparation.Refusal);
            Assert.Equal(2, partOIterationPreparation.AirHandlingUnits.Count);
            Assert.Equal(partOIterationPreparation.AirHandlingUnits.Count, partOIterationPreparation.DwellingZoneGuids.Count);
            Assert.Equal(new[] { zone_1.Guid, zone_2.Guid }.OrderBy(x => x), partOIterationPreparation.DwellingZoneGuids.OrderBy(x => x));

            //Read off the MODEL the preparation hands back, not off the report of it.
            AdjacencyCluster adjacencyCluster = partOIterationPreparation.AnalyticalModel.AdjacencyCluster;
            Assert.Equal(0, VentilationUnitReference.Compare(product_A, Unit(adjacencyCluster, partOIterationPreparation, zone_1.Guid).SelectedVentilationUnitReference()));
            Assert.Equal(0, VentilationUnitReference.Compare(product_B, Unit(adjacencyCluster, partOIterationPreparation, zone_2.Guid).SelectedVentilationUnitReference()));

            //No automatic rule ran, and the design model handed in is untouched.
            Assert.Empty(partOIterationPreparation.VentilationUnitSelections);
            Assert.Equal(json_Before, analyticalModel.ToJsonObject().ToJsonString());
            Assert.Null(partOIterationPreparation.AnalyticalModel.GetValue<PartOManualEquipmentSelection>(AnalyticalModelParameter.PartOManualEquipmentSelection));
        }

        /// <summary>A dwelling nobody chose for is left exactly as the preparation builds it: no product.</summary>
        [Fact]
        public void A_dwelling_with_no_choice_gets_no_product()
        {
            AnalyticalModel analyticalModel = TwoDwellingModel(out Zone zone_1, out Zone zone_2);

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(zone_1.Guid, product_A);

            PartOIterationPreparation partOIterationPreparation = Prepare(analyticalModel, null, partOManualEquipmentSelection);
            AdjacencyCluster adjacencyCluster = partOIterationPreparation.AnalyticalModel.AdjacencyCluster;

            Assert.Equal(0, VentilationUnitReference.Compare(product_A, Unit(adjacencyCluster, partOIterationPreparation, zone_1.Guid).SelectedVentilationUnitReference()));
            Assert.Null(Unit(adjacencyCluster, partOIterationPreparation, zone_2.Guid).SelectedVentilationUnitReference());
        }

        /// <summary>
        /// Null keeps every existing behaviour: Iteration 1a (no catalogue, no selection) selects nothing, and an
        /// automatic run (a catalogue) chooses exactly what it chose without the selection - a stored manual choice
        /// never overrides the rule, and the preparation says it was not applied.
        /// </summary>
        [Fact]
        public void Without_manual_authority_nothing_changes()
        {
            AnalyticalModel analyticalModel = TwoDwellingModel(out Zone zone_1, out Zone zone_2);

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(zone_1.Guid, product_A);
            partOManualEquipmentSelection.Set(zone_2.Guid, product_B);

            //Iteration 1a.
            PartOIterationPreparation partOIterationPreparation_1a = Prepare(analyticalModel, null, null);
            Assert.All(partOIterationPreparation_1a.AnalyticalModel.AdjacencyCluster.GetObjects<AirHandlingUnit>(), x => Assert.Null(x.SelectedVentilationUnitReference()));

            //Automatic, with and without the manual selection.
            List<VentilationUnitCapacityDescriptor> catalogue = [Descriptor("MVHR-50", 50, 50), Descriptor("MVHR-200", 200, 200)];

            List<string> models_Automatic = SelectedModels(Prepare(analyticalModel, catalogue, null));
            PartOIterationPreparation partOIterationPreparation_Both = Prepare(analyticalModel, catalogue, partOManualEquipmentSelection);

            Assert.Equal(models_Automatic, SelectedModels(partOIterationPreparation_Both));
            Assert.DoesNotContain("MVHR-A", models_Automatic);
            Assert.DoesNotContain("MVHR-B", models_Automatic);
            Assert.Contains(partOIterationPreparation_Both.Notes, x => x.Contains("hand-picked products were not applied", StringComparison.Ordinal));
        }

        /// <summary>A selection of a schema this build does not know is not applied, and the preparation warns.</summary>
        [Fact]
        public void An_unreadable_selection_is_not_applied_and_is_warned_about()
        {
            AnalyticalModel analyticalModel = TwoDwellingModel(out Zone zone_1, out Zone _);

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(zone_1.Guid, product_A);

            System.Text.Json.Nodes.JsonObject jsonObject = partOManualEquipmentSelection.ToJsonObject();
            jsonObject["Schema"] = "PartOManualEquipmentSelection:v99";

            PartOIterationPreparation partOIterationPreparation = Prepare(analyticalModel, null, new PartOManualEquipmentSelection(jsonObject));

            Assert.Null(partOIterationPreparation.Refusal);
            Assert.All(partOIterationPreparation.AnalyticalModel.AdjacencyCluster.GetObjects<AirHandlingUnit>(), x => Assert.Null(x.SelectedVentilationUnitReference()));
            Assert.Contains(partOIterationPreparation.Warnings, x => x.Contains("format this build does not read", StringComparison.Ordinal));
        }

        // ---- The original public API -------------------------------------------------------------------------

        /// <summary>
        /// Binary compatibility: the original six-parameter <c>PreparePartOIteration</c> still exists exactly - public,
        /// static, an extension method, the same parameter types in the same order and the same two defaults - so a
        /// caller compiled against the previous SAM still binds. The manual-aware overload is a separate method.
        /// </summary>
        [Fact]
        public void The_original_PreparePartOIteration_signature_is_unchanged()
        {
            Type[] types = [typeof(AnalyticalModel), typeof(PartOIteration), typeof(IEnumerable<Zone>), typeof(Dictionary<Guid, string>), typeof(IEnumerable<VentilationUnitCapacityDescriptor>), typeof(bool)];

            System.Reflection.MethodInfo methodInfo = typeof(Analytical.Modify).GetMethod(nameof(Analytical.Modify.PreparePartOIteration), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, types, null);

            Assert.NotNull(methodInfo);
            Assert.Equal(typeof(PartOIterationPreparation), methodInfo.ReturnType);
            Assert.True(methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false));

            System.Reflection.ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            Assert.Equal(["analyticalModel", "partOIteration", "zones", "dictionary_VentilationStrategy", "ventilationUnitCapacityDescriptors", "isolate"], parameterInfos.Select(x => x.Name));
            Assert.All(parameterInfos.Take(4), x => Assert.False(x.HasDefaultValue));
            Assert.True(parameterInfos[4].HasDefaultValue);
            Assert.Null(parameterInfos[4].DefaultValue);
            Assert.True(parameterInfos[5].HasDefaultValue);
            Assert.Equal(false, parameterInfos[5].DefaultValue);

            //The manual-aware overload is the only other one, and it is not a replacement of this one.
            System.Reflection.MethodInfo methodInfo_Manual = typeof(Analytical.Modify).GetMethod(nameof(Analytical.Modify.PreparePartOIteration), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, [.. types, typeof(PartOManualEquipmentSelection)], null);
            Assert.NotNull(methodInfo_Manual);
            Assert.NotEqual(methodInfo, methodInfo_Manual);
        }

        /// <summary>
        /// The original overload is the manual-aware one with no selection. It selects nothing without a catalogue
        /// - even from a model that carries hand-picked products, since it never reads the model parameter - and with
        /// a catalogue it chooses exactly what the new overload chooses. The short calls still compile and bind to it.
        /// </summary>
        [Fact]
        public void The_original_overload_behaves_as_the_new_one_with_no_manual_selection()
        {
            AnalyticalModel analyticalModel = TwoDwellingModel(out Zone zone_1, out Zone zone_2);

            PartOManualEquipmentSelection partOManualEquipmentSelection = new();
            partOManualEquipmentSelection.Set(zone_1.Guid, product_A);
            partOManualEquipmentSelection.Set(zone_2.Guid, product_B);
            Assert.True(analyticalModel.SetValue(AnalyticalModelParameter.PartOManualEquipmentSelection, partOManualEquipmentSelection));

            Dictionary<Guid, string> dictionary = analyticalModel.GetZones().ToDictionary(x => x.Guid, x => "MVRE");

            //Four arguments, as the long-standing callers write it: no catalogue, no product.
            PartOIterationPreparation partOIterationPreparation_Legacy = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary);
            Assert.Null(partOIterationPreparation_Legacy.Refusal);
            Assert.All(partOIterationPreparation_Legacy.AnalyticalModel.AdjacencyCluster.GetObjects<AirHandlingUnit>(), x => Assert.Null(x.SelectedVentilationUnitReference()));
            Assert.Equal(2, partOIterationPreparation_Legacy.DwellingZoneGuids.Count);

            //The same, through the new overload with no selection.
            PartOIterationPreparation partOIterationPreparation_New = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary, null, false, null);
            Assert.Equal(SelectedModels(partOIterationPreparation_New).Count, SelectedModels(partOIterationPreparation_Legacy).Count);
            Assert.All(partOIterationPreparation_New.AnalyticalModel.AdjacencyCluster.GetObjects<AirHandlingUnit>(), x => Assert.Null(x.SelectedVentilationUnitReference()));

            //Five and six arguments, with a catalogue: the automatic answer is the same either way.
            List<VentilationUnitCapacityDescriptor> catalogue = [Descriptor("MVHR-50", 50, 50), Descriptor("MVHR-200", 200, 200)];

            List<string> models_Five = SelectedModels(analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary, catalogue));
            List<string> models_Six = SelectedModels(analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary, catalogue, false));
            List<string> models_New = SelectedModels(analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary, catalogue, false, null));

            Assert.Equal(models_New, models_Five);
            Assert.Equal(models_New, models_Six);
            Assert.DoesNotContain(models_New, x => x.EndsWith(": -", StringComparison.Ordinal));
        }

        // -----------------------------------------------------------------------------------------------------

        private static PartOIterationPreparation Prepare(AnalyticalModel analyticalModel, List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, PartOManualEquipmentSelection partOManualEquipmentSelection)
        {
            List<Zone> zones = analyticalModel.GetZones();

            Dictionary<Guid, string> dictionary = [];
            foreach (Zone zone in zones)
            {
                dictionary[zone.Guid] = "MVRE";
            }

            PartOIterationPreparation result = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, zones, dictionary, ventilationUnitCapacityDescriptors, false, partOManualEquipmentSelection);

            Assert.True(result.Refusal is null, result.Refusal);

            return result;
        }

        /// <summary>The unit the preparation built for one dwelling zone, as the prepared model holds it.</summary>
        private static AirHandlingUnit Unit(AdjacencyCluster adjacencyCluster, PartOIterationPreparation partOIterationPreparation, Guid guid_Zone)
        {
            int index = partOIterationPreparation.DwellingZoneGuids.IndexOf(guid_Zone);
            Assert.True(index >= 0, "No unit was reported for the dwelling.");

            AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(partOIterationPreparation.AirHandlingUnits[index].Guid);
            Assert.NotNull(airHandlingUnit);

            //The unit really serves that dwelling's spaces.
            Zone zone = adjacencyCluster.GetObject<Zone>(guid_Zone);
            HashSet<Guid> guids_Space = [.. adjacencyCluster.GetRelatedObjects<Space>(zone).Select(x => x.Guid)];
            List<Space> spaces_Served = adjacencyCluster.GetRelatedObjects<Space>(partOIterationPreparation.VentilationSystems[index]);
            Assert.NotEmpty(spaces_Served);
            Assert.All(spaces_Served, x => Assert.Contains(x.Guid, guids_Space));

            return airHandlingUnit;
        }

        private static List<string> SelectedModels(PartOIterationPreparation partOIterationPreparation)
        {
            List<string> result = [];

            for (int i = 0; i < partOIterationPreparation.AirHandlingUnits.Count; i++)
            {
                AirHandlingUnit airHandlingUnit = partOIterationPreparation.AnalyticalModel.AdjacencyCluster.GetObject<AirHandlingUnit>(partOIterationPreparation.AirHandlingUnits[i].Guid);
                result.Add(string.Format("{0}: {1}", partOIterationPreparation.DwellingZoneGuids[i], airHandlingUnit.SelectedVentilationUnitReference()?.Model ?? "-"));
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        private static VentilationUnitCapacityDescriptor Descriptor(string model, double maximumSupply_Lps, double maximumExtract_Lps)
        {
            return new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", model, null), maximumSupply_Lps, maximumExtract_Lps, 0);
        }

        /// <summary>
        /// Two flats, each its own dwelling zone, sized by the real Part F calculation - the shape
        /// <c>PartOVentilationUnitSelectionTests</c> prepares, so the preparation under test is production's.
        /// </summary>
        private static AnalyticalModel TwoDwellingModel(out Zone zone_1, out Zone zone_2)
        {
            AdjacencyCluster adjacencyCluster = new();

            AddDwelling(adjacencyCluster, 1, 0);
            AddDwelling(adjacencyCluster, 2, 100);

            zone_1 = Zone(adjacencyCluster, 1);
            zone_2 = Zone(adjacencyCluster, 2);

            AnalyticalModel analyticalModel = new("Part O Manual Equipment", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part O Manual Equipment Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;
            Assert.True(partFCalculator.Calculate("Flats"));

            return new AnalyticalModel(analyticalModel, partFCalculator.AdjacencyCluster);
        }

        private static Zone Zone(AdjacencyCluster adjacencyCluster, int index)
        {
            Zone zone = new(string.Format("Flat {0}", index));
            zone.SetValue(ZoneParameter.IsDwelling, true);
            zone.SetValue(ZoneParameter.ZoneCategory, "Flats");

            adjacencyCluster.AddObject(zone);

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                if (space.Name.EndsWith(string.Format(" {0}", index), StringComparison.Ordinal))
                {
                    adjacencyCluster.AddRelation(zone, space);
                }
            }

            return zone;
        }

        private static void AddDwelling(AdjacencyCluster adjacencyCluster, int index, double x)
        {
            double scale = index == 2 ? 2.0 : 1.0;

            Dictionary<string, double> dictionary = new()
            {
                { string.Format("Living Room {0}", index), 30.0 * scale },
                { string.Format("Bedroom {0}", index), 16.0 * scale },
                { string.Format("Kitchen {0}", index), 12.0 },
                { string.Format("Bathroom {0}", index), 6.0 },
            };

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new(keyValuePair.Key);
                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                InternalCondition internalCondition = new(keyValuePair.Key + " IC");
                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, "MVRE");
                space.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space);
            }

            foreach (string name in new[] { "Bedroom", "Kitchen", "Bathroom" })
            {
                Helpers.DwellingPartitions.Partition(adjacencyCluster, string.Format("Living Room {0}", index), string.Format("{0} {1}", name, index), x);

                x += 10;
            }
        }
    }
}
