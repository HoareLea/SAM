// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// How a <see cref="Solver2DResult"/>'s geometry was arrived at.
    /// <para>
    /// The distinction exists so that a consumer can never mistake geometry the solver gave up on for
    /// geometry it validated. Before this existed, a label the solver placed correctly and a label it
    /// dropped at its anchor - overlapping whatever was already there - were indistinguishable to the
    /// caller, because both came back as a non-null rectangle.
    /// </para>
    /// </summary>
    public enum Solver2DResultType
    {
        /// <summary>
        /// No result type was recorded.
        /// <para>
        /// Present so that a defaulted value cannot read as <see cref="Solved"/>. A result that claims a
        /// successful placement only because nobody set the field is exactly the confusion this enum was
        /// added to remove, so the zero value has to mean "unknown", not "fine". Nothing
        /// <see cref="Solver2D.Solve"/> returns ever carries it.
        /// </para>
        /// </summary>
        Undefined,

        /// <summary>
        /// The solver found a position it accepted under its normal rules: inside the solver area, clear
        /// of every obstacle and of every rectangle already placed, and - where one was given - with its
        /// centre inside <see cref="Solver2DSettings.LimitArea"/>.
        /// <para>
        /// This includes the item's original, un-displaced position where that already satisfied those
        /// rules, and it includes a position found during the reduced sweep the degenerate-layout backstop
        /// falls back to, because such a position is still tested against all of them.
        /// </para>
        /// </summary>
        Solved,

        /// <summary>
        /// The solver ran out of its work budget and returned a deliberate fallback position - the item's
        /// anchor - <b>without testing it</b>. The geometry is a placeholder that keeps the item visible;
        /// it may overlap obstacles, other items, or lie outside the limit area.
        /// <para>
        /// A consumer that cares about overlap must treat this as "not placed" and show it accordingly.
        /// </para>
        /// </summary>
        Fallback,

        /// <summary>
        /// No position satisfied the rules, so no geometry was returned at all -
        /// <see cref="Solver2DResult.Closed2D{T}"/> is null. The consumer decides what to do: the
        /// floor-plan space labels blank the text, and Part F draws the tag at its anchor and flags it.
        /// </summary>
        Unplaced
    }
}
