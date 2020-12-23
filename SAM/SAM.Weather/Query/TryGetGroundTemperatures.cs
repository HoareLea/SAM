using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static bool TryGetGroundTemperatures(IEnumerable<string> lines, int index, out List<GroundTemperature> groundTemperatures)
        {
            groundTemperatures = null;

            if (lines == null)
                return false;

            if (index < 0 || index >= lines.Count())
                return false;

            string line = lines.ElementAt(index);
            if (string.IsNullOrWhiteSpace(line) || !TryGetValue(line, "GROUND TEMPERATURES", out line))
                return false;

            if (line == null)
                return false;

            string[] values = line.Split(',');
            if (values == null || values.Length < 1)
                return false;

            int count = -1;
            if (!int.TryParse(values[0], out count))
                count = -1;

            if (count == -1)
                return true;

            values = values.ToList().GetRange(1, values.Length - 1).ToArray();

            groundTemperatures = new List<GroundTemperature>();
            for (int i = 0; i <= count - 1; i++)
            {
                if (values.Length < 16)
                    break;

                double depth = double.NaN;
                if (!double.TryParse(values[0], out depth))
                    depth = double.NaN;

                double conductivity = double.NaN;
                if (!double.TryParse(values[1], out conductivity))
                    conductivity = double.NaN;

                double density = double.NaN;
                if (!double.TryParse(values[2], out density))
                    density = double.NaN;

                double specificHeat = double.NaN;
                if (!double.TryParse(values[3], out specificHeat))
                    specificHeat = double.NaN;

                double temperature_1 = double.NaN;
                if (!double.TryParse(values[4], out temperature_1))
                    temperature_1 = double.NaN;

                double temperature_2 = double.NaN;
                if (!double.TryParse(values[5], out temperature_2))
                    temperature_2 = double.NaN;

                double temperature_3 = double.NaN;
                if (!double.TryParse(values[6], out temperature_3))
                    temperature_3 = double.NaN;

                double temperature_4 = double.NaN;
                if (!double.TryParse(values[7], out temperature_4))
                    temperature_4 = double.NaN;

                double temperature_5 = double.NaN;
                if (!double.TryParse(values[8], out temperature_5))
                    temperature_5 = double.NaN;

                double temperature_6 = double.NaN;
                if (!double.TryParse(values[9], out temperature_6))
                    temperature_6 = double.NaN;

                double temperature_7 = double.NaN;
                if (!double.TryParse(values[10], out temperature_7))
                    temperature_7 = double.NaN;

                double temperature_8 = double.NaN;
                if (!double.TryParse(values[11], out temperature_8))
                    temperature_8 = double.NaN;

                double temperature_9 = double.NaN;
                if (!double.TryParse(values[12], out temperature_9))
                    temperature_9 = double.NaN;

                double temperature_10 = double.NaN;
                if (!double.TryParse(values[13], out temperature_10))
                    temperature_10 = double.NaN;

                double temperature_11 = double.NaN;
                if (!double.TryParse(values[14], out temperature_11))
                    temperature_11 = double.NaN;

                double temperature_12 = double.NaN;
                if (!double.TryParse(values[15], out temperature_12))
                    temperature_12 = double.NaN;

                groundTemperatures.Add(new GroundTemperature(
                    depth,
                    conductivity,
                    density,
                    specificHeat,
                    temperature_1,
                    temperature_2,
                    temperature_3,
                    temperature_4,
                    temperature_5,
                    temperature_6,
                    temperature_7,
                    temperature_8,
                    temperature_9,
                    temperature_10,
                    temperature_11,
                    temperature_12));

                values = values.ToList().GetRange(16, values.Length - 16).ToArray();
            }

            return true;
        }
    }
}