namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedEquipmentGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            double gain_1 = CalculatedEquipmentLatentGain(space);

            double gain_2 = CalculatedEquipmentSensibleGain(space);

            if (double.IsNaN(gain_1) && double.IsNaN(gain_2))
                return double.NaN;

            if (double.IsNaN(gain_1))
                return gain_2;

            if (double.IsNaN(gain_2))
                return gain_1;

            return gain_1 + gain_2;
        }
    }
}