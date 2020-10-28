using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string FileName_DefaultMaterialLibrary = "FileName_DefaultMaterialLibrary";
            public const string Library_DefaultMaterialLibrary = "Library_DefaultMaterialLibrary";
            
            public const string FileName_DefaultConstructionLibrary = "FileName_DefaultConstructionLibrary";
            public const string Library_DefaultConstructionLibrary = "Library_DefaultConstructionLibrary";

            public const string FileName_DefaultGasMaterialLibrary = "FileName_DefaultGasMaterialLibrary";
            public const string Library_DefaultGasMaterialLibrary = "Library_DefaultGasMaterialLibrary";

            public const string FileName_DefaultApertureConstructionLibrary = "FileName_DefaultApertureConstructionLibrary";
            public const string Library_DefaultApertureConstructionLibrary = "Library_DefaultApertureConstructionLibrary";
        }

        private static Setting setting = Load();

        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
                setting = GetDefault();

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting result = new Setting(Assembly.GetExecutingAssembly());

            //File Names
            result.Add(Name.FileName_DefaultMaterialLibrary, "SAM_MaterialLibrary.JSON");
            result.Add(Name.FileName_DefaultConstructionLibrary, "SAM_ConstructionLibrary.JSON");
            result.Add(Name.FileName_DefaultGasMaterialLibrary, "SAM_GasMaterialLibrary.JSON");
            result.Add(Name.FileName_DefaultApertureConstructionLibrary, "SAM_ApertureConstructionLibrary.JSON");

            string path = null;

            path = Query.DefaultConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultConstructionLibrary, Core.Create.IJSAMObject<ConstructionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultMaterialLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultGasMaterialLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultGasMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultApertureConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultApertureConstructionLibrary, Core.Create.IJSAMObject<ApertureConstructionLibrary>(System.IO.File.ReadAllText(path)));

            return result;
        }
    }
}