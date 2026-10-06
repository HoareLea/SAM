// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The stable identity of one reusable ventilation unit product - the mechanical ventilation with
    /// heat recovery unit an <see cref="AirHandlingUnit"/> instance has been selected to be.
    /// <para>
    /// <b>Identity only, and no capability.</b> What the product can move is
    /// <see cref="VentilationUnitCapacityDescriptor"/>, which is a catalogue fact belonging to whoever
    /// ships the catalogue. Copying a capacity onto the model would create a second answer that goes
    /// stale the day the catalogue is corrected, and - worse - would put a number meaning "equipment
    /// capability" into the model beside numbers meaning "design duty" and "regulatory requirement".
    /// Those three have to stay separable, so only the identity is stored and the capacity is looked up.
    /// </para>
    /// <para>
    /// <b>Why a reference and not the descriptor.</b> The same arrangement
    /// <see cref="PartFTerminalReference"/> uses: the model holds a durable pointer at something an
    /// authority outside the model owns and recalculates. A reference survives serialization, survives
    /// the catalogue being re-read, and can be re-resolved explicitly and reportably.
    /// </para>
    /// <para>
    /// <b>Two air handling units may hold the same reference.</b> Two dwellings fitted with the same
    /// product is the normal case, and it says nothing whatever about their duties: those come from
    /// their own terminals. Nothing about this class is per-dwelling.
    /// </para>
    /// </summary>
    public class VentilationUnitReference : SAMObject
    {
        public VentilationUnitReference()
        {
        }

        public VentilationUnitReference(string manufacturer, string model, string reference)
            : base(DisplayName(manufacturer, model))
        {
            Manufacturer = manufacturer;
            Model = model;
            Reference = reference;
        }

        public VentilationUnitReference(VentilationUnitReference ventilationUnitReference)
            : base(ventilationUnitReference)
        {
            if (ventilationUnitReference is not null)
            {
                Manufacturer = ventilationUnitReference.Manufacturer;
                Model = ventilationUnitReference.Model;
                Reference = ventilationUnitReference.Reference;
            }
        }

        public VentilationUnitReference(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>Who makes it, or null where a generic unit is being sized rather than a real one.</summary>
        public string Manufacturer { get; set; }

        /// <summary>The product designation - normally the part of the identity a schedule prints.</summary>
        public string Model { get; set; }

        /// <summary>
        /// A catalogue or variant reference distinguishing two entries a manufacturer and a model alone
        /// could not - a size code, a revision. Optional, and absent is a legal state.
        /// </summary>
        public string Reference { get; set; }

        /// <summary>Whether this names a product at all.</summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Model) || !string.IsNullOrWhiteSpace(Manufacturer);
            }
        }

        /// <summary>
        /// Whether two references name the same product, on the three identity fields and ordinally.
        /// <para>
        /// Deliberately not an equality operator and deliberately not guid-based: a reference is minted
        /// fresh every time a catalogue entry is read, so its guid is an instance identity while the
        /// product identity is what is being asked about. This mirrors
        /// <see cref="PartFTerminalReference.Matches"/>, which exists for the same reason.
        /// </para>
        /// </summary>
        public bool Matches(VentilationUnitReference ventilationUnitReference)
        {
            return ventilationUnitReference is not null && Compare(this, ventilationUnitReference) == 0;
        }

        /// <summary>
        /// Orders two references by product identity, field by field and ordinally.
        /// <para>
        /// The tie-break that makes a selection independent of the order catalogue entries arrived in -
        /// the same job <see cref="SystemCapabilityDescriptor.CompareIdentity"/> does for a template, and
        /// for the same reason: a library that enumerated a directory would otherwise let the file system
        /// decide an engineering answer.
        /// </para>
        /// </summary>
        public static int Compare(VentilationUnitReference ventilationUnitReference_1, VentilationUnitReference ventilationUnitReference_2)
        {
            if (ventilationUnitReference_1 is null || ventilationUnitReference_2 is null)
            {
                return (ventilationUnitReference_1 is null ? 0 : 1) - (ventilationUnitReference_2 is null ? 0 : 1);
            }

            int result = CompareText(ventilationUnitReference_1.Manufacturer, ventilationUnitReference_2.Manufacturer);
            if (result != 0)
            {
                return result;
            }

            result = CompareText(ventilationUnitReference_1.Model, ventilationUnitReference_2.Model);

            return result != 0 ? result : CompareText(ventilationUnitReference_1.Reference, ventilationUnitReference_2.Reference);
        }

        public override string ToString()
        {
            string result = DisplayName(Manufacturer, Model);

            return string.IsNullOrWhiteSpace(Reference) ? result : string.Format("{0} ({1})", result, Reference);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            Manufacturer = Text(jsonObject, "Manufacturer");
            Model = Text(jsonObject, "Model");
            Reference = Text(jsonObject, "Reference");

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            SetText(result, "Manufacturer", Manufacturer);
            SetText(result, "Model", Model);
            SetText(result, "Reference", Reference);

            return result;
        }

        private static string DisplayName(string manufacturer, string model)
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                return string.IsNullOrWhiteSpace(model) ? "-" : model;
            }

            return string.IsNullOrWhiteSpace(model) ? manufacturer : string.Format("{0} {1}", manufacturer, model);
        }

        private static int CompareText(string text_1, string text_2)
        {
            return string.CompareOrdinal(text_1 ?? string.Empty, text_2 ?? string.Empty);
        }

        private static string Text(JsonObject jsonObject, string name)
        {
            //Read through ToString rather than GetValue<string>, which throws on a non-string - a
            //catalogue is a hand-edited file and one broken entry must not take a whole model down.
            return jsonObject is not null && jsonObject.ContainsKey(name) ? jsonObject[name]?.ToString() : null;
        }

        private static void SetText(JsonObject jsonObject, string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                jsonObject[name] = value;
            }
        }
    }
}
