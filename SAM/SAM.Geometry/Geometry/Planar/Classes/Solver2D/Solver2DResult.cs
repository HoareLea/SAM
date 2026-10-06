// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public class Solver2DResult
    {
        private Solver2DData solver2DData;
        private IClosed2D closed2D;
        private Solver2DResultType solver2DResultType;

        /// <summary>
        /// Records a result whose type is derived from the geometry alone: geometry means
        /// <see cref="Solver2DResultType.Solved"/>, no geometry means
        /// <see cref="Solver2DResultType.Unplaced"/>.
        /// <para>
        /// Kept for callers that construct results themselves. <see cref="Solver2D"/> does not use it -
        /// it cannot, because a fallback position is also non-null geometry and would be reported here as
        /// solved. Prefer the overload that states the type.
        /// </para>
        /// </summary>
        public Solver2DResult(Solver2DData solver2DData, IClosed2D closed2D)
            : this(solver2DData, closed2D, closed2D == null ? Solver2DResultType.Unplaced : Solver2DResultType.Solved)
        {
        }

        public Solver2DResult(Solver2DData solver2DData, IClosed2D closed2D, Solver2DResultType solver2DResultType)
        {
            this.solver2DData = solver2DData;
            this.closed2D = closed2D;
            this.solver2DResultType = solver2DResultType;
        }

        public Solver2DData Solver2DData
        {
            get
            {
                return solver2DData;
            }
        }

        /// <summary>
        /// How this result's geometry was arrived at, so a fallback position can never be read as a
        /// solved one. Set once, when the result is recorded; see <see cref="Solver2DResultType"/> for
        /// what each value guarantees.
        /// </summary>
        public Solver2DResultType ResultType
        {
            get
            {
                return solver2DResultType;
            }
        }

        public T Closed2D<T>() where T : IClosed2D
        {
            return closed2D is T ? (T)closed2D : default;
        }

        public object Tag
        {
            get
            {
                return solver2DData?.Tag;
            }
        }

    }
}
