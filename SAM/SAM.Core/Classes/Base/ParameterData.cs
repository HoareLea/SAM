// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;

namespace SAM.Core
{
    public class ParameterData : IParameterData
    {
        private ParameterProperties parameterProperties;
        private ParameterValue parameterValue;

        public ParameterData(ParameterProperties parameterProperties, ParameterValue parameterValue)
        {
            this.parameterProperties = parameterProperties;
            this.parameterValue = parameterValue;
        }

        public ParameterProperties ParameterProperties
        {
            get
            {
                return parameterProperties;
            }
        }

        public ParameterValue ParameterValue
        {
            get
            {
                return parameterValue;
            }
        }
    }
}
