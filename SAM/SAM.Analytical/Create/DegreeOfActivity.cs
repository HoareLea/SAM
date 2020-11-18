namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// DegreeOfActivity created base on VDI 2078 2015
        /// </summary>
        /// <param name="activityLevel">ActivityLevel </param>
        /// <param name="name"> Name of DegreeOfActivity</param>
        /// <param name="temperature">Room temperature [C]</param>
        /// <returns></returns>
        public static DegreeOfActivity DegreeOfActivity(this ActivityLevel activityLevel, string name, double temperature)
        {
            if (double.IsNaN(temperature) || activityLevel == ActivityLevel.Undefined)
                return null;

            double senisble = double.NaN;
            double latent = double.NaN;

            if (temperature < 16)
                temperature = 16;

            if (temperature > 28)
                temperature = 28;

            switch(activityLevel)
            {
                case ActivityLevel.First:
                    senisble = 161 - (3.8 * temperature);
                    latent = -61 + (3.8 * temperature);
                    break;
                case ActivityLevel.Second:
                    senisble = 166 - (3.8 * temperature);
                    latent = -41 + (3.8 * temperature);
                    break;
                case ActivityLevel.Third:
                    senisble = 183 - (4.1 * temperature);
                    latent = -13 + (4.1 * temperature);
                    break;
                case ActivityLevel.Fourth:
                    senisble = 263 - (6.6 * temperature);
                    latent = -53 + (6.6 * temperature);
                    break;
                default:
                    return null;
            }

            if (senisble < 0)
                senisble = 0;

            if (latent < 25)
                latent = 25;

            return new DegreeOfActivity(name, senisble, latent);
        }
    }
}