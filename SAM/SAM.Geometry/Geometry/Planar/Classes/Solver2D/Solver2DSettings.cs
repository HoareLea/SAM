// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public class Solver2DSettings
    {
        /// <summary>
        /// Starting distance between object to move and object around which it moves.
        /// </summary>
        public double StartingDistance { get; set; } = 0;

        /// <summary>
        /// Distance of every shift of moveable object.
        /// </summary>
        public double ShiftDistance { get; set; } = 1;

        /// <summary>
        /// Number of object shift attempts.
        /// </summary>
        public double IterationCount { get; set; } = 10;

        /// <summary>
        /// Area where object may be shifted. Object's center must be inside limit area.
        /// <para>
        /// <b>The name understates the test.</b> Only the rectangle's CENTROID has to lie inside this area
        /// - the rectangle itself may extend well beyond it. The whole-geometry containment test is the
        /// separate solver <c>area</c>, which every candidate must sit entirely inside.
        /// </para>
        /// <para>
        /// This is deliberate and both consumers depend on it. A floor-plan space label and a Part F
        /// terminal tag are text boxes a metre or more wide, and an ensuite, a WC or a slender utility room
        /// frequently cannot contain one; requiring full containment would leave those rooms unlabelled,
        /// whereas a label whose centre is in the room and whose tail overhangs the wall still reads
        /// unambiguously as that room's. Documented rather than renamed or redefined: the meaning is load
        /// bearing for the Mollier chart as well, so changing it here would change three drawings at once.
        /// </para>
        /// <para>
        /// The test is <see cref="IClosed2D.Inside(Point2D, double)"/> and not <c>InRange</c>, so a centroid
        /// exactly on the boundary does not qualify.
        /// </para>
        /// </summary>
        public IClosed2D LimitArea { get; set; } = null;
    }
}
