namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ConstructionManager DefaultConstructionManager()
        {
            return new ConstructionManager(DefaultApertureConstructionLibrary(), DefaultConstructionLibrary(), DefaultMaterialLibrary());
        }
    }
}