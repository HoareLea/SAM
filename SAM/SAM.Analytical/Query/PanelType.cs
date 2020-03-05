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
                return(PanelType)(int)(@object);

            return Analytical.PanelType.Undefined;
        }

        public static PanelType PanelType(Geometry.Spatial.Vector3D normal)
        {
            if (normal == null)
                return Analytical.PanelType.Undefined;

            if (normal.InRange(Geometry.Spatial.Vector3D.BaseZ, 0.261799))
                return Analytical.PanelType.Roof;

            if (normal.InRange(new Geometry.Spatial.Vector3D(0,0,-1), 0.261799))
                return Analytical.PanelType.Floor;

            return Analytical.PanelType.Wall;
        }
    }
}
