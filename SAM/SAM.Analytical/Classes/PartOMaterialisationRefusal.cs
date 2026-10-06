// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One structured reason a model is not a clean Part O baseline, or a set of dwelling strategies could not
    /// be materialised: a machine-readable <see cref="Reason"/>, the zone it concerns (where it concerns one),
    /// the object it names, and a sentence an engineer can act on.
    /// </summary>
    public class PartOMaterialisationRefusal
    {
        public PartOMaterialisationRefusal(PartOMaterialisationRefusalReason partOMaterialisationRefusalReason, string message, Guid? guid_Zone = null, string subject = null)
        {
            Reason = partOMaterialisationRefusalReason;
            Message = message;
            ZoneGuid = guid_Zone;
            Subject = subject;
        }

        public PartOMaterialisationRefusalReason Reason { get; }

        /// <summary>The zone the refusal concerns, or null where it concerns the model or the set.</summary>
        public Guid? ZoneGuid { get; }

        /// <summary>The object named - a zone, system, unit or space name - or null.</summary>
        public string Subject { get; }

        public string Message { get; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Reason, Message);
        }
    }
}
