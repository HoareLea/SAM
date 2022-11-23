using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByDischargeCoefficient(this RhinoDoc rhinoDoc, IEnumerable<Aperture> apertures)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_DischargeCoefficient = layerTable[index];
            layer_DischargeCoefficient.Name = "DischargeCoefficient";
            layer_DischargeCoefficient.ParentLayerId = layer_SAM.Id;

            SortedDictionary<double, List<Aperture>> dictionary = new SortedDictionary<double, List<Aperture>>();
            List<Aperture> apertures_NoDischargeCoefficient = new List<Aperture>();
            foreach(Aperture aperture in apertures)
            {
                if (aperture == null || aperture.ApertureType != ApertureType.Window)
                {
                    continue;
                }

                double dischargeCoefficient = 0;
                if(aperture.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties) && openingProperties != null)
                {
                    dischargeCoefficient = openingProperties.GetDischargeCoefficient();
                    if(double.IsNaN(dischargeCoefficient))
                    {
                        dischargeCoefficient = 0;
                    }
                }

                dischargeCoefficient = Core.Query.Round(dischargeCoefficient, Core.Tolerance.MacroDistance);

                if(dischargeCoefficient == 0)
                {
                    apertures_NoDischargeCoefficient.Add(aperture);
                    continue;
                }

                if(!dictionary.TryGetValue(dischargeCoefficient, out List<Aperture> apertures_Temp) || apertures_Temp == null)
                {
                    apertures_Temp = new List<Aperture>();
                    dictionary[dischargeCoefficient] = apertures_Temp;
                }

                apertures_Temp.Add(aperture);
            }

            Color color = Query.Color(ApertureType.Window, AperturePart.Pane, true);

            List<Color> colors = Core.Create.Colors(color, dictionary.Count, -0.5, 0.5);

            int i = 0;
            SortedDictionary<double, Color> dictionary_Color = new SortedDictionary<double, Color>();
            foreach (KeyValuePair<double, List<Aperture>> keyValuePair in dictionary)
            {
                dictionary_Color[keyValuePair.Key] = colors[i];
                i++;
            }

            dictionary[0] = apertures_NoDischargeCoefficient;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<Guid> guids = new List<Guid>();
            foreach (KeyValuePair<double, List<Aperture>> keyValuePair in dictionary)
            {
                double dischargeCoefficient = keyValuePair.Key;

                Color color_Temp = dictionary_Color.ContainsKey(dischargeCoefficient) ? dictionary_Color[dischargeCoefficient] : Query.Color(ApertureType.Window);

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_DischargeCoefficient.Id, dischargeCoefficient.ToString(), color_Temp);

                objectAttributes.LayerIndex = layer.Index;

                foreach(Aperture aperture in keyValuePair.Value)
                {
                    if (BakeGeometry(aperture, rhinoDoc, objectAttributes, out Guid guid))
                    {
                        guids.Add(guid);
                    }
                }
            }
        }
    }
}