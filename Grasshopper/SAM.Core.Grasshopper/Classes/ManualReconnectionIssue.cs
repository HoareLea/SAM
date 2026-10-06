// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SAM.Core.Grasshopper
{
    public class ManualReconnectionIssue
    {
        public Guid ComponentInstanceGuid;
        public string ComponentName;
        public string ComponentNickName;
        public float PivotX;
        public float PivotY;
        public bool ReplacementFailed;
        public List<ManualReconnectionWire> Wires = new List<ManualReconnectionWire>();

        public List<string> MissingInputNames
        {
            get
            {
                return GetMissingNames(GH_ParameterSide.Input);
            }
        }

        public List<string> MissingOutputNames
        {
            get
            {
                return GetMissingNames(GH_ParameterSide.Output);
            }
        }

        public string Summary
        {
            get
            {
                if (ReplacementFailed)
                {
                    return "replacement failed";
                }

                int inputCount = Wires == null ? 0 : Wires.FindAll(x => x != null && x.Side == GH_ParameterSide.Input).Count;
                int outputCount = Wires == null ? 0 : Wires.FindAll(x => x != null && x.Side == GH_ParameterSide.Output).Count;

                if (inputCount != 0 && outputCount == 0)
                {
                    return string.Format("{0} missing {1}", inputCount, inputCount == 1 ? "input" : "inputs");
                }

                if (outputCount != 0 && inputCount == 0)
                {
                    return string.Format("{0} missing {1}", outputCount, outputCount == 1 ? "output" : "outputs");
                }

                int count = inputCount + outputCount;
                return string.Format("{0} missing {1}", count, count == 1 ? "wire" : "wires");
            }
        }

        public string ToText(int number)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("[{0:00}] {1}", number, ComponentName);

            if (!string.IsNullOrWhiteSpace(ComponentNickName) && ComponentNickName != ComponentName)
            {
                stringBuilder.AppendFormat(" ({0})", ComponentNickName);
            }

            stringBuilder.AppendFormat(" — {0}", Summary);

            List<string> inputNames = MissingInputNames;
            if (inputNames.Count != 0)
            {
                stringBuilder.AppendFormat(" | Inputs: {0}", string.Join(", ", inputNames));
            }

            List<string> outputNames = MissingOutputNames;
            if (outputNames.Count != 0)
            {
                stringBuilder.AppendFormat(" | Outputs: {0}", string.Join(", ", outputNames));
            }

            stringBuilder.AppendFormat(" | Component ID: {0}", ComponentInstanceGuid);

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} — {1}", ComponentName, Summary);
        }

        private List<string> GetMissingNames(GH_ParameterSide side)
        {
            List<string> result = new List<string>();
            if (Wires == null)
            {
                return result;
            }

            foreach (ManualReconnectionWire wire in Wires)
            {
                if (wire == null || wire.Side != side)
                {
                    continue;
                }

                string name = string.IsNullOrWhiteSpace(wire.ParamName) ? "(unnamed)" : wire.ParamName;
                if (!result.Contains(name))
                {
                    result.Add(name);
                }
            }

            return result;
        }
    }
}
