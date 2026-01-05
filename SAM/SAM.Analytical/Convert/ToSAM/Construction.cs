// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostPartitionType hostPartitionType)
        {
            if (hostPartitionType == null)
            {
                return null;
            }

            Construction result = new Construction(hostPartitionType.Guid, hostPartitionType.Name, hostPartitionType?.MaterialLayers?.ConvertAll(x => x.ToSAM()));
            Core.Modify.CopyParameterSets(hostPartitionType, result);
            return result;
        }
    }
}
