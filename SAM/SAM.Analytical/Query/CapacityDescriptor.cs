// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The selection-facing view of one manufacturer template - its identity, its two maximum airflows
        /// and its rank - or <b>null</b> where the template does not establish a capacity.
        /// <para>
        /// <b>This is the whole seam.</b> Iteration 2's selection kernel reads
        /// <see cref="VentilationUnitCapacityDescriptor"/> and knows nothing about manufacturers,
        /// brochures, performance tables or control curves. A catalogue of real products becomes usable by
        /// it here, in one mapping, and the kernel is unchanged by that - which is the property that lets a
        /// second manufacturer be added by adding a file.
        /// </para>
        /// <para>
        /// <b>Null, not a fallback.</b> A template whose maximum airflows nobody has established is not
        /// offered with a guessed capacity, and specifically is not offered with the largest airflow in its
        /// performance table: that number is a duty point the manufacturer chose to publish at, and reading
        /// it as the fan's limit would size a dwelling's plant against a figure nobody stated.
        /// <see cref="VentilationUnitTemplate.SelectionCapacityRefusal"/> is the sentence explaining it.
        /// </para>
        /// <para>
        /// <b>Only capability crosses.</b> The performance table and the control curve stay on the template
        /// and do not travel into the descriptor - a selection has no business reading them, and Iteration 3
        /// reads them from the template it resolves by identity.
        /// </para>
        /// </summary>
        public static VentilationUnitCapacityDescriptor CapacityDescriptor(this VentilationUnitTemplate ventilationUnitTemplate)
        {
            if (ventilationUnitTemplate is null || !ventilationUnitTemplate.HasSelectionCapacity)
            {
                return null;
            }

            return new VentilationUnitCapacityDescriptor(
                ventilationUnitTemplate.VentilationUnitReference,
                ventilationUnitTemplate.MaximumSupplyFlowRate_Lps,
                ventilationUnitTemplate.MaximumExtractFlowRate_Lps,
                ventilationUnitTemplate.Rank);
        }

        /// <summary>
        /// Every template that can be offered to a selection, in the order they were supplied.
        /// <para>
        /// Templates without an established capacity are left out rather than approximated. They are not
        /// lost: <see cref="UnselectableVentilationUnitTemplates"/> lists them with the reason, and a caller
        /// that reports one alongside the other can say "eleven products, one of which has no stated
        /// capacity" instead of silently offering ten.
        /// </para>
        /// <para>
        /// Order is preserved rather than sorted, because ordering candidates is
        /// <see cref="CapableVentilationUnits"/>'s job and it is deterministic there. Sorting twice, by two
        /// different rules, is how a list starts depending on which one ran last.
        /// </para>
        /// </summary>
        public static List<VentilationUnitCapacityDescriptor> CapacityDescriptors(this IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates)
        {
            List<VentilationUnitCapacityDescriptor> result = [];

            foreach (VentilationUnitTemplate ventilationUnitTemplate in ventilationUnitTemplates ?? [])
            {
                VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = CapacityDescriptor(ventilationUnitTemplate);

                if (ventilationUnitCapacityDescriptor is not null)
                {
                    result.Add(ventilationUnitCapacityDescriptor);
                }
            }

            return result;
        }

        /// <summary>
        /// The templates a selection cannot be offered, each paired with the sentence saying why.
        /// <para>
        /// Exists so that "this catalogue holds a product we cannot yet select" is a reportable fact rather
        /// than a shorter list. A missing product is exactly the kind of absence that looks like nothing at
        /// all until somebody wonders why the unit they specified was never chosen.
        /// </para>
        /// </summary>
        public static List<KeyValuePair<VentilationUnitTemplate, string>> UnselectableVentilationUnitTemplates(this IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates)
        {
            List<KeyValuePair<VentilationUnitTemplate, string>> result = [];

            foreach (VentilationUnitTemplate ventilationUnitTemplate in ventilationUnitTemplates ?? [])
            {
                if (ventilationUnitTemplate is null)
                {
                    continue;
                }

                string reason = ventilationUnitTemplate.SelectionCapacityRefusal;

                if (!string.IsNullOrWhiteSpace(reason))
                {
                    result.Add(new KeyValuePair<VentilationUnitTemplate, string>(ventilationUnitTemplate, reason));
                }
            }

            return result;
        }

        /// <summary>
        /// The template in a catalogue that a selected product identity names, or null where the catalogue
        /// does not hold it.
        /// <para>
        /// The bridge Iteration 3 crosses: a model stores an identity, and the performance data behind that
        /// identity is looked up here rather than copied onto the model. Matched on the identity fields -
        /// see <see cref="VentilationUnitReference.Matches"/> - never on guid, because a template is minted
        /// afresh every time a catalogue is read.
        /// </para>
        /// <para>
        /// <b>Refuses an ambiguous catalogue rather than taking the first match.</b> Two templates sharing
        /// one identity have no single answer to "what did we select", which is the same defect
        /// <see cref="SelectSmallestCapableVentilationUnit"/> refuses a catalogue for. Answering here with
        /// whichever came first would make the performance data depend on the order a directory was read.
        /// </para>
        /// </summary>
        public static VentilationUnitTemplate MatchingVentilationUnitTemplate(this IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates, VentilationUnitReference ventilationUnitReference)
        {
            if (ventilationUnitReference is null || !ventilationUnitReference.IsValid)
            {
                return null;
            }

            VentilationUnitTemplate result = null;

            foreach (VentilationUnitTemplate ventilationUnitTemplate in ventilationUnitTemplates ?? [])
            {
                if (ventilationUnitTemplate?.VentilationUnitReference is null || !ventilationUnitReference.Matches(ventilationUnitTemplate.VentilationUnitReference))
                {
                    continue;
                }

                if (result is not null)
                {
                    return null;
                }

                result = ventilationUnitTemplate;
            }

            return result;
        }
    }
}
