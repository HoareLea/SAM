// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Core
{
    public static partial class Modify
    {
        /// <summary>
        /// Adds <paramref name="parameterSet"/> to <paramref name="parameterSets"/>, or merges it into the set that
        /// already represents it: the same GUID, else the same (non-empty) name. Merged values override existing ones.
        /// </summary>
        /// <remarks>
        /// The name is the identity of an assembly-owned set; the GUID is only a hint. An assembly without an
        /// [assembly: Guid] (SAM.Analytical since 49069d9c) takes its GUID from the per-build module version id
        /// (<see cref="Query.Guid(System.Reflection.Assembly)"/>), so matching by GUID alone appended one more
        /// same-name set per build and left readers on another build with a stale value (SAM#146).
        /// </remarks>
        public static bool Add(this List<ParameterSet> parameterSets, ParameterSet parameterSet)
        {
            if (parameterSets == null || parameterSet == null)
                return false;

            ParameterSet parameterSet_Existing = parameterSets.Find(x => x.Guid.Equals(parameterSet.Guid));
            if (parameterSet_Existing == null && !string.IsNullOrEmpty(parameterSet.Name))
                parameterSet_Existing = parameterSets.Find(x => parameterSet.Name.Equals(x.Name));

            if (parameterSet_Existing == null)
            {
                parameterSets.Add(parameterSet);
                return true;
            }

            return parameterSet_Existing.Copy(parameterSet);
        }

        public static LogRecord Add(this Log log, string format, params object[] values)
        {
            if (log == null || format == null)
                return null;

            return log.Add(format, values);
        }

        public static LogRecord Add(this Log log, string format, LogRecordType logRecordType, params object[] values)
        {
            if (log == null || format == null)
                return null;

            return log.Add(format, logRecordType, values);
        }

        public static bool Add(this JsonObject jsonObject, Tag tag)
        {
            if (jsonObject == null || tag == null)
            {
                return false;
            }

            JsonObject jsonObject_Tag = tag.ToJsonObject();
            if (jsonObject_Tag == null)
            {
                return false;
            }

            jsonObject["Tag"] = jsonObject_Tag.DeepClone();
            return true;
        }

    }
}
