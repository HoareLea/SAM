// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static ParameterSet ParameterSet(this IEnumerable<ParameterSet> parameterSets, string name)
        {
            if (name == null || parameterSets == null)
                return null;

            foreach (ParameterSet parameterSet in parameterSets)
            {
                if (name.Equals(parameterSet.Name))
                    return parameterSet;
            }

            return null;
        }

        public static ParameterSet ParameterSet(this IEnumerable<ParameterSet> parameterSets, Assembly assembly)
        {
            ParameterSet parameterSet = ParameterSet(parameterSets, Guid(assembly));
            if (parameterSet == null)
                parameterSet = ParameterSet(parameterSets, Name(assembly));

            return parameterSet;
        }

        public static ParameterSet ParameterSet(this IEnumerable<ParameterSet> parameterSets, Guid guid)
        {
            if (parameterSets == null)
                return null;

            foreach (ParameterSet parameterSet in parameterSets)
            {
                if (guid.Equals(parameterSet.Guid))
                    return parameterSet;
            }

            return null;
        }
    }
}
