namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Adiabatic(this Panel panel)
        {
            if(panel  == null)
            {
                return false;
            }

            if(panel.PanelType == Analytical.PanelType.Air)
            {
                return false;
            }

            if (Adiabatic(panel.Construction))
            {
                return true;
            }

            if (!panel.TryGetValue(PanelParameter.Adiabatic, out bool result))
            {
                return false;
            }

            return result;
        }

        public static bool Adiabatic(this Construction construction)
        {
            if (construction == null)
            {
                return false;
            }

            double thickness = construction.GetThickness();

            return thickness == 0 || double.IsNaN(thickness);
        }
    }
}