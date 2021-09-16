namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Space ReplaceNameSpecialCharacters(this Space space, string language)
        {
            if (space == null || string.IsNullOrWhiteSpace(language))
                return null;

            string name = space.Name;
            if (string.IsNullOrWhiteSpace(name))
                return space;

            string name_Temp = Core.Query.ReplaceSpecialCharacters(name, language);
            if (name.Equals(name_Temp))
                return space;

            return new Space(space, name_Temp, space.Location);
        }

        public static InternalCondition ReplaceNameSpecialCharacters(this InternalCondition internalCondition, string language)
        {
            if (internalCondition == null || string.IsNullOrWhiteSpace(language))
                return null;

            string name = internalCondition.Name;
            if (string.IsNullOrWhiteSpace(name))
                return internalCondition;

            string name_Temp = Core.Query.ReplaceSpecialCharacters(name, language);
            if (name.Equals(name_Temp))
                return internalCondition;

            return new InternalCondition(name_Temp, internalCondition);
        }

        public static Panel ReplaceNameSpecialCharacters(this Panel panel, string language)
        {
            if (panel == null || string.IsNullOrWhiteSpace(language))
                return null;

            Construction construction_Old = panel.Construction;
            Construction construction_New = null;
            if (construction_Old != null)
                construction_New = ReplaceNameSpecialCharacters(construction_Old, language);

            string name = Core.Query.ReplaceSpecialCharacters(panel.Name, language);
            if (construction_New?.Name == construction_Old?.Name && name == panel.Name)
                return panel;


            return new Panel(name, panel, construction_New);
        }

        public static Construction ReplaceNameSpecialCharacters(this Construction construction, string language)
        {
            if (construction == null || string.IsNullOrWhiteSpace(language))
                return null;

            string name = construction.Name;
            if (string.IsNullOrWhiteSpace(name))
                return construction;

            string name_Temp = Core.Query.ReplaceSpecialCharacters(name, language);
            if (name.Equals(name_Temp))
                return construction;

            return new Construction(construction.Guid, construction, name_Temp);
        }

        public static ApertureConstruction ReplaceNameSpecialCharacters(this ApertureConstruction apertureConstruction, string language)
        {
            if (apertureConstruction == null || string.IsNullOrWhiteSpace(language))
                return null;

            string name = apertureConstruction.Name;
            if (string.IsNullOrWhiteSpace(name))
                return apertureConstruction;

            string name_Temp = Core.Query.ReplaceSpecialCharacters(name, language);
            if (name.Equals(name_Temp))
                return apertureConstruction;

            return new ApertureConstruction(apertureConstruction.Guid, apertureConstruction, name_Temp);
        }

        public static Aperture ReplaceNameSpecialCharacters(Aperture aperture, string language)
        {
            if (aperture == null || string.IsNullOrWhiteSpace(language))
                return null;

            ApertureConstruction apertureConstruction_Old = aperture.ApertureConstruction;
            ApertureConstruction apertureConstruction_New = null;
            if (apertureConstruction_Old != null)
                apertureConstruction_New = ReplaceNameSpecialCharacters(apertureConstruction_Old, language);

            string name = Core.Query.ReplaceSpecialCharacters(aperture.Name, language);
            if (apertureConstruction_New?.Name == apertureConstruction_Old.Name && name == aperture.Name)
                return aperture;


            return new Aperture(name, aperture, apertureConstruction_New);
        }
    }
}