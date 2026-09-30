// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The ventilation unit product an engineer has chosen <b>by hand</b> for each dwelling, persisted on the
    /// design model as <c>AnalyticalModelParameter.PartOManualEquipmentSelection</c> - Part O design input, read
    /// by <c>Modify.PreparePartOIteration</c> under manual authority.
    ///
    /// <para><b>Why it exists (the model-state architecture)</b></para>
    /// <para>
    /// A dwelling's product is written onto the air handling unit a preparation builds
    /// (<c>AirHandlingUnitParameter.VentilationUnitReference</c>), and that unit is run output: it is never on
    /// the design model. A hand-picked product therefore had no design-model representation and reached the next
    /// case only when that case was prepared from the previous result. This is that representation: the
    /// engineer's choice, keyed by dwelling, from which every preparation materialises its temporary units.
    /// </para>
    ///
    /// <para><b>Not the project preselection</b></para>
    /// <para>
    /// <see cref="PartOEquipmentSelection"/> states how products are chosen and which are permitted - a candidate
    /// constraint, never an assignment. This is the assignment, and only the manual one: it is authoritative only
    /// under <c>PartOEquipmentSelectionMode.ManualPerDwelling</c>, and nothing an automatic rule selects is ever
    /// stored here.
    /// </para>
    ///
    /// <para><b>Keyed by dwelling zone guid, at most one product each</b></para>
    /// <para>
    /// The dwelling is the zone's identity, never its name: two dwellings may be called the same, and a renamed
    /// flat is the same flat. <see cref="Set"/> replaces a dwelling's product and <see cref="Remove"/> clears it.
    /// </para>
    ///
    /// <para><b>Identities only, canonical</b></para>
    /// <para>
    /// A product is persisted by its manufacturer, model and reference alone - no instance guid and no capacity
    /// (see <see cref="VentilationUnitReference"/>) - and dwellings are written in zone-guid order, so two
    /// logically identical selections write the same bytes whatever order they were made in.
    /// </para>
    ///
    /// <para><b>Absent means none, and a later schema is not reinterpreted</b></para>
    /// <para>
    /// A model without the parameter has no hand-picked product: nothing is inferred from its systems, units or
    /// earlier results. A selection of a schema this build does not know loads as <see cref="IsValid"/> false and
    /// is not applied.
    /// </para>
    /// </summary>
    public class PartOManualEquipmentSelection : IJSAMObject, IAnalyticalObject
    {
        /// <summary>The persisted schema this build reads and writes.</summary>
        public const string Schema = "PartOManualEquipmentSelection:v1";

        private readonly SortedDictionary<Guid, VentilationUnitReference> dictionary = [];

        public PartOManualEquipmentSelection()
        {
        }

        public PartOManualEquipmentSelection(PartOManualEquipmentSelection partOManualEquipmentSelection)
        {
            if (partOManualEquipmentSelection is not null)
            {
                foreach (KeyValuePair<Guid, VentilationUnitReference> keyValuePair in partOManualEquipmentSelection.dictionary)
                {
                    dictionary[keyValuePair.Key] = Identity(keyValuePair.Value);
                }

                SchemaRead = partOManualEquipmentSelection.SchemaRead;
            }
        }

        public PartOManualEquipmentSelection(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>The schema the selection was read with - <see cref="Schema"/> unless it was loaded from something else.</summary>
        public string SchemaRead { get; private set; } = Schema;

        /// <summary>Whether the selection may be applied at all: a schema this build knows.</summary>
        public bool IsValid => SchemaRead == Schema;

        /// <summary>How many dwellings carry a hand-picked product.</summary>
        public int Count => dictionary.Count;

        /// <summary>The dwelling zones that carry a hand-picked product, in guid order.</summary>
        public List<Guid> ZoneGuids => [.. dictionary.Keys];

        /// <summary>
        /// The product chosen for a dwelling, as a copy of its identity, or null where none was chosen.
        /// </summary>
        public VentilationUnitReference Product(Guid guid_Zone)
        {
            return dictionary.TryGetValue(guid_Zone, out VentilationUnitReference result) ? Identity(result) : null;
        }

        /// <summary>
        /// Chooses (or replaces) a dwelling's product. A dwelling with no identity, or a product that identifies
        /// nothing, is not a choice and is refused.
        /// </summary>
        /// <returns>Whether the choice was recorded.</returns>
        public bool Set(Guid guid_Zone, VentilationUnitReference ventilationUnitReference)
        {
            if (guid_Zone == Guid.Empty || ventilationUnitReference is null || !ventilationUnitReference.IsValid)
            {
                return false;
            }

            dictionary[guid_Zone] = Identity(ventilationUnitReference);

            return true;
        }

        /// <summary>Clears a dwelling's product - it then has no hand-picked product.</summary>
        public bool Remove(Guid guid_Zone) => dictionary.Remove(guid_Zone);

        /// <summary>Whether two selections choose the same product for the same dwellings.</summary>
        public bool Matches(PartOManualEquipmentSelection partOManualEquipmentSelection)
        {
            if (partOManualEquipmentSelection is null || partOManualEquipmentSelection.SchemaRead != SchemaRead || partOManualEquipmentSelection.dictionary.Count != dictionary.Count)
            {
                return false;
            }

            foreach (KeyValuePair<Guid, VentilationUnitReference> keyValuePair in dictionary)
            {
                if (!partOManualEquipmentSelection.dictionary.TryGetValue(keyValuePair.Key, out VentilationUnitReference ventilationUnitReference) || Analytical.VentilationUnitReference.Compare(keyValuePair.Value, ventilationUnitReference) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            dictionary.Clear();

            if (jsonObject is null)
            {
                return false;
            }

            SchemaRead = jsonObject["Schema"] is JsonValue jsonValue && jsonValue.TryGetValue(out string schema) ? schema : null;

            if (jsonObject["Dwellings"] is JsonArray jsonArray)
            {
                foreach (JsonNode jsonNode in jsonArray)
                {
                    if (jsonNode is not JsonObject jsonObject_Dwelling)
                    {
                        continue;
                    }

                    if (jsonObject_Dwelling["ZoneGuid"] is not JsonValue jsonValue_Zone || !jsonValue_Zone.TryGetValue(out string text_Zone) || !Guid.TryParse(text_Zone, out Guid guid_Zone) || guid_Zone == Guid.Empty)
                    {
                        continue;
                    }

                    VentilationUnitReference ventilationUnitReference = new(
                        Text(jsonObject_Dwelling, "Manufacturer"),
                        Text(jsonObject_Dwelling, "Model"),
                        Text(jsonObject_Dwelling, "Reference"));

                    //The first product stated for a dwelling is kept; a product that identifies nothing is no choice.
                    if (ventilationUnitReference.IsValid && !dictionary.ContainsKey(guid_Zone))
                    {
                        dictionary[guid_Zone] = ventilationUnitReference;
                    }
                }
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonArray jsonArray = [];
            foreach (KeyValuePair<Guid, VentilationUnitReference> keyValuePair in dictionary)
            {
                JsonObject jsonObject_Dwelling = new()
                {
                    ["ZoneGuid"] = keyValuePair.Key.ToString(),
                    ["Manufacturer"] = keyValuePair.Value.Manufacturer,
                    ["Model"] = keyValuePair.Value.Model,
                };

                if (keyValuePair.Value.Reference is not null)
                {
                    jsonObject_Dwelling["Reference"] = keyValuePair.Value.Reference;
                }

                jsonArray.Add(jsonObject_Dwelling);
            }

            return new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Schema"] = SchemaRead ?? Schema,
                ["Dwellings"] = jsonArray,
            };
        }

        public override string ToString()
        {
            return string.Format("{0} dwelling(s) with a hand-picked ventilation unit", dictionary.Count);
        }

        /// <summary>A product's identity fields alone - no instance guid - so the selection serialises canonically.</summary>
        private static VentilationUnitReference Identity(VentilationUnitReference ventilationUnitReference)
        {
            return new VentilationUnitReference(ventilationUnitReference.Manufacturer, ventilationUnitReference.Model, ventilationUnitReference.Reference);
        }

        private static string Text(JsonObject jsonObject, string name)
        {
            return jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out string result) ? result : null;
        }
    }
}
