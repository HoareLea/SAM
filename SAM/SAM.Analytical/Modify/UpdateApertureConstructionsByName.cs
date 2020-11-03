using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static ApertureConstructionLibrary UpdateApertureConstructionsByName(this List<Panel> panels, ApertureConstructionLibrary apertureConstructionLibrary)
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

                foreach(Aperture aperture in apertures)
                {
                    string name = aperture?.ApertureConstruction?.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    ApertureConstruction apertureConstruction = result.GetApertureConstructions(name, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (apertureConstruction == null)
                    {
                        apertureConstruction = apertureConstructionLibrary.GetApertureConstructions(name, TextComparisonType.Equals, true)?.FirstOrDefault();
                        if (apertureConstruction == null)
                            continue;

                        result.Add(apertureConstruction);
                    }

                    if (apertureConstruction == null)
                        continue;

                    panel.RemoveAperture(aperture.Guid);
                    Aperture aperture_New = new Aperture(aperture, apertureConstruction);
                    panel.AddAperture(aperture_New);
                }
            }
            return result;
        }

        public static ApertureConstructionLibrary UpdateApertureConstructionsByName(this List<Aperture> apertures, ApertureConstructionLibrary apertureConstructionLibrary)
        {
            if (apertures == null || apertureConstructionLibrary == null)
                return null;

            ApertureConstructionLibrary result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);
            for (int i = 0; i < apertures.Count; i++)
            {
                Aperture aperture = apertures[i];

                string name = aperture?.ApertureConstruction?.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                ApertureConstruction apertureConstruction = result.GetApertureConstructions(name, TextComparisonType.Equals, true)?.FirstOrDefault();
                if (apertureConstruction == null)
                {
                    apertureConstruction = apertureConstructionLibrary.GetApertureConstructions(name, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (apertureConstruction == null)
                        continue;

                    result.Add(apertureConstruction);
                }

                if (apertureConstruction == null)
                    continue;

               apertures[i] = new Aperture(aperture, apertureConstruction);
            }

            return result;
        }

        public static ApertureConstructionLibrary UpdateApertureConstructionsByName(
        this List<Aperture> apertures,
        ApertureConstructionLibrary apertureConstructionLibrary,
        DelimitedFileTable delimitedFileTable,
        string columnName_Source,
        string columnName_Template,
        string columnName_Destination = null)
        {
            if (apertures == null || apertureConstructionLibrary == null || delimitedFileTable == null)
                return null;

            int index_Template = delimitedFileTable.GetColumnIndex(columnName_Template);
            if (index_Template == -1)
                return null;

            int index_Source = delimitedFileTable.GetColumnIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Destination = delimitedFileTable.GetColumnIndex(columnName_Destination);

            ApertureConstructionLibrary result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);
            for (int i = 0; i < apertures.Count; i++)
            {
                Aperture aperture = apertures[i];

                string name = aperture?.ApertureConstruction?.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                List<int> indexes = delimitedFileTable.GetRowIndexes(index_Source, name, TextComparisonType.Equals, true);
                if (indexes == null || indexes.Count == 0)
                    continue;

                int index = indexes[0];

                string name_Template = delimitedFileTable.ToString(index, index_Template);
                if (string.IsNullOrWhiteSpace(name_Template))
                    continue;

                string name_Destination = name_Template;
                if (index_Destination != -1)
                {
                    name_Destination = delimitedFileTable.ToString(index, index_Destination);
                    if (string.IsNullOrWhiteSpace(name_Destination))
                        name_Destination = name_Template;
                }

                ApertureConstruction apertureConstruction = result.GetApertureConstructions(name_Destination, TextComparisonType.Equals, true)?.FirstOrDefault();
                if (apertureConstruction == null)
                {
                    ApertureConstruction apertureConstruction_Temp = apertureConstructionLibrary.GetApertureConstructions(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (apertureConstruction_Temp == null)
                        continue;

                    if (name_Destination.Equals(name_Template))
                        apertureConstruction = apertureConstruction_Temp;
                    else
                        apertureConstruction = new ApertureConstruction(apertureConstruction_Temp, name_Destination);

                    result.Add(apertureConstruction);
                }

                if (apertureConstruction == null)
                    continue;

                apertures[i] = new Aperture(aperture, apertureConstruction);
            }
            return result;
        }

        public static ApertureConstructionLibrary UpdateApertureConstructionsByName(
        this List<Panel> panels,
        ApertureConstructionLibrary apertureConstructionLibrary,
        DelimitedFileTable delimitedFileTable,
        string columnName_Source,
        string columnName_Template,
        string columnName_Destination = null)
        {
            if (panels == null || apertureConstructionLibrary == null || delimitedFileTable == null)
                return null;

            int index_Template = delimitedFileTable.GetColumnIndex(columnName_Template);
            if (index_Template == -1)
                return null;

            int index_Source = delimitedFileTable.GetColumnIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Destination = delimitedFileTable.GetColumnIndex(columnName_Destination);

            ApertureConstructionLibrary result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];

                List<Aperture> apertures = panel?.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                foreach (Aperture aperture in apertures)
                {
                    string name = aperture?.ApertureConstruction?.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    List<int> indexes = delimitedFileTable.GetRowIndexes(index_Source, name, TextComparisonType.Equals, true);
                    if (indexes == null || indexes.Count == 0)
                        continue;

                    int index = indexes[0];

                    string name_Template = delimitedFileTable.ToString(index, index_Template);
                    if (string.IsNullOrWhiteSpace(name_Template))
                        continue;

                    string name_Destination = name_Template;
                    if (index_Destination != -1)
                    {
                        name_Destination = delimitedFileTable.ToString(index, index_Destination);
                        if (string.IsNullOrWhiteSpace(name_Destination))
                            name_Destination = name_Template;
                    }

                    ApertureConstruction apertureConstruction = result.GetApertureConstructions(name_Destination, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (apertureConstruction == null)
                    {
                        ApertureConstruction apertureConstruction_Temp = apertureConstructionLibrary.GetApertureConstructions(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
                        if (apertureConstruction_Temp == null)
                            continue;

                        if (name_Destination.Equals(name_Template))
                            apertureConstruction = apertureConstruction_Temp;
                        else
                            apertureConstruction = new ApertureConstruction(apertureConstruction_Temp, name_Destination);

                        result.Add(apertureConstruction);
                    }

                    if (apertureConstruction == null)
                        continue;

                    panel.RemoveAperture(aperture.Guid);
                    Aperture aperture_New = new Aperture(aperture, apertureConstruction);
                    panel.AddAperture(aperture_New);
                }
            }
            return result;
        }

        public static ApertureConstructionLibrary UpdateApertureConstructionsByName(
        this List<ApertureConstruction> apertureConstructions,
        ApertureConstructionLibrary apertureConstructionLibrary,
        DelimitedFileTable delimitedFileTable,
        string columnName_Source,
        string columnName_Template,
        string columnName_Destination = null)
        {
            if (apertureConstructions == null || apertureConstructionLibrary == null || delimitedFileTable == null)
                return null;

            int index_Template = delimitedFileTable.GetColumnIndex(columnName_Template);
            if (index_Template == -1)
                return null;

            int index_Source = delimitedFileTable.GetColumnIndex(columnName_Source);
            if (index_Source == -1)
                return null;

            int index_Destination = delimitedFileTable.GetColumnIndex(columnName_Destination);

            ApertureConstructionLibrary result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);
            for (int i = 0; i < apertureConstructions.Count; i++)
            {
                string name = apertureConstructions[i].Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                List<int> indexes = delimitedFileTable.GetRowIndexes(index_Source, name, TextComparisonType.Equals, true);
                if (indexes == null || indexes.Count == 0)
                    continue;

                int index = indexes[0];

                string name_Template = delimitedFileTable.ToString(index, index_Template);
                if (string.IsNullOrWhiteSpace(name_Template))
                    continue;

                string name_Destination = name_Template;
                if (index_Destination != -1)
                {
                    name_Destination = delimitedFileTable.ToString(index, index_Destination);
                    if (string.IsNullOrWhiteSpace(name_Destination))
                        name_Destination = name_Template;
                }

                ApertureConstruction apertureConstruction = result.GetApertureConstructions(name_Destination, TextComparisonType.Equals, true)?.FirstOrDefault();
                if (apertureConstruction == null)
                {
                    ApertureConstruction apertureConstruction_Temp = apertureConstructionLibrary.GetApertureConstructions(name_Template, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (apertureConstruction_Temp == null)
                        continue;

                    if (name_Destination.Equals(name_Template))
                        apertureConstruction = apertureConstruction_Temp;
                    else
                        apertureConstruction = new ApertureConstruction(apertureConstruction_Temp, name_Destination);

                    result.Add(apertureConstruction);
                }

                apertureConstructions[i] = apertureConstruction;
            }
            return result;
        }
    }
}