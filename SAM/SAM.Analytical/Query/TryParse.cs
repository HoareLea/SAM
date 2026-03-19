// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryParse(string text, out SystemTemplate systemTemplate)
        {
            systemTemplate = null;

            if(string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string[] values = text.Trim().Split(['_'], StringSplitOptions.RemoveEmptyEntries);
            if(values is null || values.Length == 0)
            {
                return false;
            }

            List<Tuple<string, string>> tuples = [];
            foreach(string value in values)
            {
                string trimmedValue = value.Trim();
                int index = trimmedValue.IndexOf(".");

                if (index == -1)
                {
                    tuples.Add(new Tuple<string, string>(null, trimmedValue));
                }
                else
                {
                    // index points to the dot. Substring(0, index) takes everything before it.
                    string prefix = trimmedValue.Substring(0, index).ToUpper().Trim();
                    if(prefix != "V" || prefix != "H" || prefix != "C" || prefix != "PR" || prefix != "CTL" || prefix != "VER")
                    {
                        tuples.Add(new Tuple<string, string>(null, trimmedValue));
                    }
                    else
                    {
                        // index + 1 starts right after the dot.
                        string suffix = trimmedValue.Substring(index + 1).Trim();

                        tuples.Add(new Tuple<string, string>(prefix, suffix));
                    }
                }
            }

            if(tuples is null || tuples.Count == 0)
            {
                return false;
            }

            string ventilation = null;
            string heating = null;
            string cooling = null;
            string plantRoom = null;
            string controls = null;
            string version = null;

            if (tuples.Count == 1)
            {
                if(string.IsNullOrWhiteSpace(tuples[0].Item1) || tuples[0].Item1 == "V")
                {
                    ventilation = tuples[0].Item2;
                }
                else
                {
                    switch (tuples[0].Item1)
                    {
                        case "V":
                            ventilation= tuples[0].Item2;
                            break;

                        case "H":
                            heating = tuples[0].Item2;
                            break;

                        case "C":
                            cooling = tuples[0].Item2;
                            break;

                        case "PR":
                            plantRoom = tuples[0].Item2;
                            break;

                        case "CTL":
                            controls = tuples[0].Item2;
                            break;

                        case "VER":
                            version = tuples[0].Item2;
                            break;
                    }
                }
            }
            else
            {
                int index;

                index = tuples.FindIndex(x => x.Item1 == "V");
                if (index != -1)
                {
                    ventilation = tuples[index].Item2;
                    tuples.RemoveAt(index);
                }
                else
                {
                    index = tuples.FindIndex(x => string.IsNullOrWhiteSpace(x.Item1));
                    if(index != -1)
                    {
                        ventilation = tuples[index].Item2;
                        tuples.RemoveAt(index);
                    }
                }

                index = tuples.FindIndex(x => x.Item1 == "H");
                if (index != -1)
                {
                    heating = tuples[index].Item2;
                    tuples.RemoveAt(index);
                }
                else
                {
                    index = tuples.FindIndex(x => string.IsNullOrWhiteSpace(x.Item1));
                    if (index != -1)
                    {
                        heating = tuples[index].Item2;
                        tuples.RemoveAt(index);
                    }
                }

                index = tuples.FindIndex(x => x.Item1 == "C");
                if (index != -1)
                {
                    cooling = tuples[index].Item2;
                    tuples.RemoveAt(index);
                }
                else
                {
                    index = tuples.FindIndex(x => string.IsNullOrWhiteSpace(x.Item1));
                    if (index != -1)
                    {
                        cooling = tuples[index].Item2;
                        tuples.RemoveAt(index);
                    }
                }

                index = tuples.FindIndex(x => x.Item1 == "PR");
                if (index != -1)
                {
                    plantRoom = tuples[index].Item2;
                    tuples.RemoveAt(index);
                }
                else
                {
                    index = tuples.FindIndex(x => string.IsNullOrWhiteSpace(x.Item1));
                    if (index != -1)
                    {
                        plantRoom = tuples[index].Item2;
                        tuples.RemoveAt(index);
                    }
                }

                index = tuples.FindIndex(x => x.Item1 == "VER");
                if (index != -1)
                {
                    version = tuples[index].Item2;
                    tuples.RemoveAt(index);
                }
                else
                {
                    index = tuples.FindIndex(x => string.IsNullOrWhiteSpace(x.Item1));
                    if (index != -1)
                    {
                        version = tuples[index].Item2;
                        tuples.RemoveAt(index);
                    }
                }

            }

            systemTemplate = new SystemTemplate(ventilation, heating, cooling, plantRoom, controls, version);
            return true;
        }
    }
}
