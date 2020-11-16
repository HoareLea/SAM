using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static PanelType PanelType(this object @object)
        {
            if (@object == null)
                return Analytical.PanelType.Undefined;
            
            if (@object is PanelType)
                return (PanelType)@object;

            if (@object is Geometry.Spatial.Vector3D)
                return PanelType((Geometry.Spatial.Vector3D)@object);

            PanelType result;

            if (@object is Construction)
            {
                string text;
                if (((Construction)@object).TryGetValue(ConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                {
                    result = PanelType(text, false);
                    return result;
                }

                return Analytical.PanelType.Undefined;
            }
                

            if (@object is ApertureConstruction)
            {
                string text;
                if (((ApertureConstruction)@object).TryGetValue(ApertureConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                {
                    result = PanelType(text, false);
                    return result;
                }

                return Analytical.PanelType.Undefined;
            }

            if (@object is string)
            {
                string value = (string)@object;

                if (Enum.TryParse(value, out result))
                    return result;

                value = value.Replace(" ", string.Empty).ToUpper();
                foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
                {
                    string value_Type = panelType.ToString().ToUpper();
                    if (value_Type.Equals(value))
                        return panelType;

                    value_Type = Text(panelType)?.Replace(" ", string.Empty).ToUpper();
                    if (!string.IsNullOrEmpty(value_Type) && value_Type.Equals(value))
                        return panelType;
                }

                return Analytical.PanelType.Undefined;
            }

            if (@object is int)
                return (PanelType)(int)(@object);

            return Analytical.PanelType.Undefined;
        }

        public static PanelType PanelType(Geometry.Spatial.Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return Analytical.PanelType.Undefined;

            double value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldZ);
            if (System.Math.Abs(value) <= tolerance)
                return Analytical.PanelType.Wall;

            if (value < 0)
                return Analytical.PanelType.Floor;

            return Analytical.PanelType.Roof;
        }

        public static PanelType PanelType(string text, bool includeDefaultConstructionNames = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Analytical.PanelType.Undefined;

            PanelType result = Analytical.PanelType.Undefined;
            if (Enum.TryParse(text, out result))
                return result;

            foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
            {
                string value = null;

                if(includeDefaultConstructionNames)
                {
                    value = DefaultConstruction(panelType)?.Name;
                    if (text.Equals(value))
                        return panelType;
                }

                value = Text(panelType);
                if (text.Equals(value))
                    return panelType;
            }

            return Analytical.PanelType.Undefined;
        }
    }
}