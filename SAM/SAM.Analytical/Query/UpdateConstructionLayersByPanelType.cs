// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Fills in the fabric of panels and apertures that <b>have none</b>, from the construction
        /// libraries, keyed by panel type.
        /// <para>
        /// With <paramref name="emptyOnly"/> left at its default this is a <b>gap filler and not a rewrite</b>:
        /// a construction that already has layers is never touched, so a prepared model's own fabric is
        /// preserved exactly as it was authored.
        /// </para>
        /// <para>
        /// Where a library has no candidate for a type, the existing construction is <b>kept</b> and the
        /// panel or aperture is reported through the overload below. It is never replaced by a default, and
        /// nothing is ever marked adiabatic to make the run continue - a surface with no fabric that is
        /// quietly treated as adiabatic is a different thermal case presented as the requested one.
        /// </para>
        /// </summary>
        public static AnalyticalModel UpdateConstructionLayersByPanelType(this AnalyticalModel analyticalModel, ConstructionLibrary constructionLibrary = null, ApertureConstructionLibrary apertureConstructionLibrary = null, MaterialLibrary materialLibrary = null, bool emptyOnly = true)
        {
            return UpdateConstructionLayersByPanelType(analyticalModel, out _, constructionLibrary, apertureConstructionLibrary, materialLibrary, emptyOnly);
        }

        /// <summary>
        /// The same fill, <b>saying what it could not resolve</b>.
        ///
        /// <para><b>Why the report exists</b></para>
        /// <para>
        /// A panel or an aperture that still has no fabric after this runs does not fail loudly further on -
        /// it changes the thermal case silently, and differently for each:
        /// </para>
        /// <list type="bullet">
        /// <item>A <b>panel</b> whose construction has no layers converts to a TBD construction with no
        /// materials, and <see cref="Adiabatic(Construction)"/> reports a zero thickness construction as
        /// adiabatic in its own right - so <c>SAM_Tas Modify.UpdateAdiabatic</c> nulls the surface's link and
        /// the wall is simulated as an adiabatic boundary nobody asked for.</item>
        /// <item>An <b>aperture</b> whose construction has no pane layers has a pane thickness of zero, and
        /// the conversion creates a pane surface only where the thickness is above zero
        /// (<c>SAM_Tas Modify.Update</c>) - so the opening is not represented in the TBD at all. A door the
        /// model says is there simply does not exist in the simulation.</item>
        /// </list>
        /// <para>
        /// Neither announces itself. The caller is given what could not be resolved so it can decide - and
        /// for a Part O run, refuse - rather than simulate a case it did not intend.
        /// </para>
        /// </summary>
        /// <param name="unresolved">
        /// One entry per panel or aperture left with no fabric, naming it. Empty where everything resolved.
        /// Never null.
        /// </param>
        public static AnalyticalModel UpdateConstructionLayersByPanelType(this AnalyticalModel analyticalModel, out List<string> unresolved, ConstructionLibrary constructionLibrary = null, ApertureConstructionLibrary apertureConstructionLibrary = null, MaterialLibrary materialLibrary = null, bool emptyOnly = true)
        {
            unresolved = [];

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if (panels == null)
            {
                return null;
            }

            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            MaterialLibrary materialLibrary_AnalyticalModel = analyticalModel.MaterialLibrary;

            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if (panel == null)
                    continue;

                Construction construction_Old = panel.Construction;

                //Whether this panel is one the fill is FOR. Asked once and reused below, so that "was it
                //missing fabric?" and "did it end up with fabric?" cannot drift apart.
                bool empty_Construction = construction_Old == null || !construction_Old.HasConstructionLayers();

                Construction construction_New = null;
                if (!emptyOnly || empty_Construction)
                {
                    //A library that is not configured, and a library with no candidate for this type, are the
                    //same answer: there is nothing to fill from. Neither is an error here and neither may
                    //throw - GetConstructions answers null rather than an empty list, and FirstOrDefault over
                    //null throws ArgumentNullException, which is how an ordinary model with a door reached
                    //the guided Part O path and crashed it.
                    construction_New = constructionLibrary?.GetConstructions(panel.PanelType)?.FirstOrDefault();
                }

                bool updated = false;

                if (construction_New != null)
                {
                    IEnumerable<IMaterial> materials_Temp = Materials(construction_New, materialLibrary);
                    if (materials_Temp != null)
                    {
                        foreach (IMaterial material in materials_Temp)
                            if (!materialLibrary_AnalyticalModel.Contains(material))
                                materialLibrary_AnalyticalModel.Add(material);
                    }

                    construction_New = new Construction(construction_Old, construction_New.ConstructionLayers);

                    panel = Create.Panel(panel, construction_New);
                    updated = true;
                }
                else if (empty_Construction)
                {
                    //Kept exactly as it is - an unresolved construction is not replaced by a default and is
                    //not made adiabatic. It is reported instead.
                    unresolved.Add(string.Format(
                        "Panel '{0}' ({1}) has no construction layers, and the construction library offers nothing for that panel type.",
                        string.IsNullOrWhiteSpace(panel.Name) ? panel.Guid.ToString() : panel.Name,
                        panel.PanelType));
                }

                if (panel.HasApertures)
                {
                    panel = Create.Panel(panel);
                    foreach (Aperture aperture in panel.Apertures)
                    {
                        Aperture aperture_Old = panel.GetAperture(aperture.Guid);
                        if (aperture_Old == null)
                            continue;

                        ApertureConstruction apertureConstruction_Old = aperture_Old.ApertureConstruction;

                        bool empty_ApertureConstruction = apertureConstruction_Old == null || !apertureConstruction_Old.HasPaneConstructionLayers();

                        ApertureConstruction apertureConstruction_New = null;
                        if (!emptyOnly || empty_ApertureConstruction)
                        {
                            //apertureConstruction_Old is dereferenced for its type, so it has to be there:
                            //the null case above is a genuinely empty aperture and there is nothing to look
                            //a replacement up BY. Same null-safety as the panel above for the rest.
                            apertureConstruction_New = apertureConstruction_Old == null
                                ? null
                                : apertureConstructionLibrary?.GetApertureConstructions(apertureConstruction_Old.ApertureType, panel.PanelType)?.FirstOrDefault();
                        }

                        if (apertureConstruction_New != null)
                        {
                            IEnumerable<IMaterial> materials_Temp = Materials(apertureConstruction_New, materialLibrary);
                            if (materials_Temp != null)
                            {
                                foreach (IMaterial material in materials_Temp)
                                    if (!materialLibrary_AnalyticalModel.Contains(material))
                                        materialLibrary_AnalyticalModel.Add(material);
                            }

                            apertureConstruction_New = new ApertureConstruction(apertureConstruction_Old, apertureConstruction_New.PaneConstructionLayers, apertureConstruction_New.FrameConstructionLayers);

                            Aperture aperture_New = new Aperture(aperture_Old, apertureConstruction_New);

                            if (aperture_New == null)
                                continue;

                            panel.RemoveAperture(aperture_Old.Guid);
                            panel.AddAperture(aperture_New);
                            updated = true;
                        }
                        else if (empty_ApertureConstruction)
                        {
                            unresolved.Add(string.Format(
                                "Aperture '{0}' ({1}) on panel '{2}' has no pane construction layers, and the aperture construction library offers nothing for that aperture type on that panel type.",
                                string.IsNullOrWhiteSpace(aperture_Old.Name) ? aperture_Old.Guid.ToString() : aperture_Old.Name,
                                apertureConstruction_Old == null ? "no aperture construction" : apertureConstruction_Old.ApertureType.ToString(),
                                string.IsNullOrWhiteSpace(panel.Name) ? panel.Guid.ToString() : panel.Name));
                        }
                    }
                }

                if (updated)
                    adjacencyCluster.AddObject(panel);

            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary_AnalyticalModel, analyticalModel.ProfileLibrary);
        }
    }
}
