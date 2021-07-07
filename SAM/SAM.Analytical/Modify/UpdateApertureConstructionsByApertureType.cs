using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static ApertureConstructionLibrary UpdateApertureConstructionsByApertureType(this List<Panel> panels, ApertureConstructionLibrary apertureConstructionLibrary)
        {
            if (panels == null || apertureConstructionLibrary == null)
                return null;

            ApertureConstructionLibrary result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];

                List<Aperture> apertures = panel?.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                bool updated = false;
                foreach(Aperture aperture in apertures)
                {
                    ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                    if(apertureConstruction == null)
                    {
                        continue;
                    }

                    ApertureType apertureType = apertureConstruction.ApertureType;

                    ApertureConstruction apertureConstruction_New = result.GetApertureConstructions(aperture.ApertureType, panel.PanelGroup)?.FirstOrDefault();
                    if (apertureConstruction_New == null)
                    {
                        apertureConstruction_New = apertureConstructionLibrary.GetApertureConstructions(aperture.ApertureType, panel.PanelGroup)?.FirstOrDefault();
                        if (apertureConstruction_New == null)
                            continue;

                        result.Add(new ApertureConstruction(apertureConstruction_New));
                    }

                    if (apertureConstruction_New == null)
                        continue;

                    if(!updated)
                    {
                        updated = true;
                        panel = new Panel(panel);
                    }

                    panel.RemoveAperture(aperture.Guid);
                    Aperture aperture_New = new Aperture(aperture, apertureConstruction_New);
                    panel.AddAperture(aperture_New);
                }

                if (updated)
                    panels[i] = panel;
            }
            return result;
        }
    }
}