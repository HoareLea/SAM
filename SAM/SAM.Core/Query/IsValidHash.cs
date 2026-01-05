// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Compares Hash with given string value. Returns true if the same
        /// </summary>
        /// <param name="value">Text</param>
        /// <param name="hash">Hash</param>
        /// <returns>Is Valid Hash</returns>
        public static bool IsValidHash(string value, string hash)
        {
            if (value == null && hash == null)
            {
                return true;
            }

            if (value == string.Empty && hash == string.Empty)
            {
                return true;
            }

            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(hash))
            {
                return false;
            }

            return hash.Equals(ComputeHash(value));
        }
    }
}
