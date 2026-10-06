// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core.Reporting
{
    /// <summary>
    /// A renderer-neutral piece of document content. Blocks hold only formatted values and text: they carry no
    /// engineering units and need no conversion. Every block is produced by a named section builder from typed data.
    /// </summary>
    public abstract class DocumentBlock
    {
        protected DocumentBlock(string id)
        {
            Id = id;
        }

        /// <summary>
        /// Stable identifier within its section, used by tests and snapshots.
        /// </summary>
        public string Id { get; }
    }
}
