using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static PanelType PanelType(this object @object)
        {
            if (@object is PanelType)
                return (PanelType)@object;

            PanelType result;
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
                        return result;
                }

                return Analytical.PanelType.Undefined;
            }

            if (@object is int)
                return (PanelType)(int)(@object);

            return Analytical.PanelType.Undefined;
        }

        public static PanelType PanelType(Geometry.Spatial.Vector3D normal)
        {
            if (normal == null)
                return Analytical.PanelType.Undefined;

            double tolerance = 0.1;

            double value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldZ);
            if (value < tolerance && value > -tolerance)
                return Analytical.PanelType.Wall;

            if (value < 0)
                return Analytical.PanelType.Floor;

            return Analytical.PanelType.Roof;
        }

        public static PanelType PanelType(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Analytical.PanelType.Undefined;
            
            foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
            {
                string value = null;

                value = Construction(panelType)?.Name;
                if (text.Equals(value))
                    return panelType;

                value = Text(panelType);
                if (text.Equals(value))
                    return panelType;
            }

            return Analytical.PanelType.Undefined;
        }
    }
}