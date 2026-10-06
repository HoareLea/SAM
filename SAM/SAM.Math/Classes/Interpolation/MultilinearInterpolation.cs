// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Math
{
    /// <summary>
    /// Multilinear interpolation over a regular grid of any number of dimensions - the N-dimensional
    /// generalisation of <see cref="LinearInterpolation"/> (one axis) and
    /// <see cref="BilinearInterpolation"/> (two).
    /// <para>
    /// <b>Why a third class rather than a third special case.</b> Manufacturer performance data is
    /// routinely tabulated against three or more independent conditions - an air handling unit's leaving
    /// air temperature against outdoor dry bulb, entering dry bulb and airflow, for instance - and a
    /// chain of nested bilinear calls over such a table is both hard to read and easy to get subtly
    /// wrong in the corner weights. The arithmetic below is the same in every dimension, so the
    /// dimension count is data.
    /// </para>
    /// <para>
    /// <b>Deterministic and exact at the nodes.</b> The bracketing fraction of a coordinate that sits
    /// exactly on an axis value is exactly zero or exactly one, so a query at a grid point returns that
    /// point's stored value bit for bit rather than a rounded reconstruction of it. That matters
    /// wherever the grid holds published data: a lookup has to be able to give the published number
    /// back.
    /// </para>
    /// <para>
    /// <b>Arithmetic only - it is deliberately not an <c>IJSAMObject</c>.</b> Nothing serialises an
    /// interpolator: what gets written to a file is the manufacturer table it was built from, and the
    /// assembly owning that file owns its parser. A serialiser here would be a second, untested way to
    /// express the same grid, and a second place for a JSON reading bug to live.
    /// </para>
    /// <para>
    /// <b>Outside the grid it answers <see cref="double.NaN"/>, and that is the default on purpose.</b>
    /// <see cref="Calculate(double[])"/> never extrapolates. Where a caller genuinely wants a value
    /// outside the tabulated range it has to say which behaviour it means -
    /// <see cref="CalculateClamped(double[])"/> holds the edge, and
    /// <see cref="CalculateExtrapolated(double[])"/> continues the edge cell's gradient - so that
    /// "beyond the data" is always a decision somebody made rather than a number that appeared.
    /// </para>
    /// </summary>
    public class MultilinearInterpolation
    {
        /// <summary>
        /// One strictly increasing array of coordinates per dimension. An axis of a single value is legal -
        /// it makes that dimension a constant.
        /// </summary>
        private double[][] axes;

        /// <summary>
        /// The tabulated values, flattened row-major over <see cref="axes"/>: the LAST axis varies
        /// fastest, so for axes of lengths (3, 4, 8) the value at (i, j, k) sits at ((i * 4) + j) * 8 + k.
        /// </summary>
        private double[] values;

        public MultilinearInterpolation()
        {
        }

        public MultilinearInterpolation(MultilinearInterpolation multilinearInterpolation)
        {
            if (multilinearInterpolation == null)
            {
                return;
            }

            if (multilinearInterpolation.axes != null)
            {
                axes = new double[multilinearInterpolation.axes.Length][];
                for (int i = 0; i < multilinearInterpolation.axes.Length; i++)
                {
                    axes[i] = multilinearInterpolation.axes[i] == null ? null : (double[])multilinearInterpolation.axes[i].Clone();
                }
            }

            if (multilinearInterpolation.values != null)
            {
                values = (double[])multilinearInterpolation.values.Clone();
            }
        }

        /// <summary>
        /// Builds a grid from its axes and its flattened values.
        /// </summary>
        /// <param name="axes">One coordinate array per dimension, each strictly increasing.</param>
        /// <param name="values">
        /// The tabulated values flattened row-major - last axis fastest - so its length is the product of
        /// the axis lengths.
        /// </param>
        public MultilinearInterpolation(IEnumerable<IEnumerable<double>> axes, IEnumerable<double> values)
        {
            Load(axes, values);
        }

        /// <summary>
        /// Fills the grid. Returns false, and leaves the instance empty, where what was handed in is not a
        /// usable regular grid - see <see cref="IsValid"/> for what that means and why each condition is
        /// refused rather than repaired.
        /// <para>
        /// Private, so a grid never changes after construction. That is what lets a caller cache one
        /// interpolator and read it from several threads without it being reshaped underneath them.
        /// </para>
        /// </summary>
        private bool Load(IEnumerable<IEnumerable<double>> axes, IEnumerable<double> values)
        {
            this.axes = null;
            this.values = null;

            if (axes == null || values == null)
            {
                return false;
            }

            List<double[]> axes_Temp = new List<double[]>();
            foreach (IEnumerable<double> axis in axes)
            {
                if (axis == null)
                {
                    return false;
                }

                axes_Temp.Add(new List<double>(axis).ToArray());
            }

            List<double> values_Temp = new List<double>(values);

            if (!IsUsableGrid(axes_Temp, values_Temp.Count))
            {
                return false;
            }

            foreach (double value in values_Temp)
            {
                //A hole in the table is refused rather than carried. Interpolating a cell with a missing
                //corner produces NaN for every query touching that cell, which reads exactly like "outside
                //the grid" and would make a gap in the source data indistinguishable from a coordinate
                //nobody tabulated.
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    return false;
                }
            }

            this.axes = axes_Temp.ToArray();
            this.values = values_Temp.ToArray();

            return true;
        }

        /// <summary>
        /// Whether this is a grid a query can be answered from: at least one axis, every axis strictly
        /// increasing and finite, one finite value per grid point and no more.
        /// <para>
        /// Strictly increasing rather than merely sorted, because two axis entries of the same coordinate
        /// give a zero-width cell whose interpolation fraction is a division by zero, and there is no
        /// honest way to choose between the two values sitting on it.
        /// </para>
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (axes == null || values == null)
                {
                    return false;
                }

                return IsUsableGrid(new List<double[]>(axes), values.Length);
            }
        }

        /// <summary>How many independent coordinates a query takes. -1 where nothing is loaded.</summary>
        public int Dimensions
        {
            get
            {
                return axes == null ? -1 : axes.Length;
            }
        }

        /// <summary>How many tabulated values the grid holds. -1 where nothing is loaded.</summary>
        public int Count
        {
            get
            {
                return values == null ? -1 : values.Length;
            }
        }

        /// <summary>The coordinates of one axis. A copy, so a caller cannot reshape the grid through it.</summary>
        public double[] Axis(int index)
        {
            if (axes == null || index < 0 || index >= axes.Length || axes[index] == null)
            {
                return null;
            }

            return (double[])axes[index].Clone();
        }

        /// <summary>The lowest coordinate one axis was tabulated at.</summary>
        public double Minimum(int index)
        {
            if (axes == null || index < 0 || index >= axes.Length || axes[index] == null || axes[index].Length == 0)
            {
                return double.NaN;
            }

            return axes[index][0];
        }

        /// <summary>The highest coordinate one axis was tabulated at.</summary>
        public double Maximum(int index)
        {
            if (axes == null || index < 0 || index >= axes.Length || axes[index] == null || axes[index].Length == 0)
            {
                return double.NaN;
            }

            return axes[index][axes[index].Length - 1];
        }

        /// <summary>
        /// Whether every coordinate falls inside the tabulated range of its own axis. A coordinate exactly
        /// on a boundary is inside it.
        /// </summary>
        public bool InDomain(params double[] coordinates)
        {
            if (!IsValid || coordinates == null || coordinates.Length != axes.Length)
            {
                return false;
            }

            for (int i = 0; i < coordinates.Length; i++)
            {
                if (double.IsNaN(coordinates[i]) || coordinates[i] < Minimum(i) || coordinates[i] > Maximum(i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// One tabulated value, addressed by its position on each axis - the stored number itself, with no
        /// arithmetic performed on it. <see cref="double.NaN"/> where the indices do not address a point.
        /// </summary>
        public double Value(params int[] indices)
        {
            if (axes == null || values == null || indices == null || indices.Length != axes.Length)
            {
                return double.NaN;
            }

            int index = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                if (axes[i] == null || indices[i] < 0 || indices[i] >= axes[i].Length)
                {
                    return double.NaN;
                }

                index = (index * axes[i].Length) + indices[i];
            }

            return index >= 0 && index < values.Length ? values[index] : double.NaN;
        }

        /// <summary>
        /// The interpolated value at a point inside the grid, or <see cref="double.NaN"/> outside it.
        /// <para>
        /// <b>Never extrapolates.</b> See the type remarks - beyond the tabulated range the caller has to
        /// name the behaviour it wants.
        /// </para>
        /// </summary>
        public double Calculate(params double[] coordinates)
        {
            return InDomain(coordinates) ? Interpolate(coordinates) : double.NaN;
        }

        /// <summary>
        /// The interpolated value at a point, with any coordinate outside its axis pulled back to the
        /// nearest tabulated boundary first - so the grid's edge value is held flat beyond the edge.
        /// <para>
        /// The right reading where a table states a saturating behaviour ("100% at 26 degrees and above"),
        /// and the wrong one where a quantity genuinely keeps changing. It is a caller's decision, which is
        /// why it is a separate method.
        /// </para>
        /// </summary>
        public double CalculateClamped(params double[] coordinates)
        {
            if (!IsValid || coordinates == null || coordinates.Length != axes.Length)
            {
                return double.NaN;
            }

            double[] coordinates_Clamped = new double[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
            {
                if (double.IsNaN(coordinates[i]))
                {
                    return double.NaN;
                }

                double minimum = Minimum(i);
                double maximum = Maximum(i);

                coordinates_Clamped[i] = coordinates[i] < minimum ? minimum : (coordinates[i] > maximum ? maximum : coordinates[i]);
            }

            return Interpolate(coordinates_Clamped);
        }

        /// <summary>
        /// The value at a point, continuing the gradient of the outermost cell beyond the tabulated range.
        /// <para>
        /// <b>A compatibility behaviour, not a reading of the data.</b> An extrapolated number is this
        /// class's invention and carries none of the source's authority, so nothing here calls it by
        /// default. An axis of a single coordinate has no gradient to continue and stays constant.
        /// </para>
        /// </summary>
        public double CalculateExtrapolated(params double[] coordinates)
        {
            if (!IsValid || coordinates == null || coordinates.Length != axes.Length)
            {
                return double.NaN;
            }

            for (int i = 0; i < coordinates.Length; i++)
            {
                if (double.IsNaN(coordinates[i]) || double.IsInfinity(coordinates[i]))
                {
                    return double.NaN;
                }
            }

            return Interpolate(coordinates);
        }

        public override string ToString()
        {
            if (!IsValid)
            {
                return "Invalid MultilinearInterpolation";
            }

            List<string> lengths = new List<string>();
            for (int i = 0; i < axes.Length; i++)
            {
                lengths.Add(axes[i].Length.ToString());
            }

            return string.Format("MultilinearInterpolation [{0}]", string.Join(" x ", lengths.ToArray()));
        }

        /// <summary>
        /// The weighted sum over the 2^n corners of the cell each coordinate falls in. Coordinates are
        /// assumed to have been admitted by the caller: a coordinate outside its axis simply produces a
        /// fraction outside [0, 1] and so extrapolates along the edge cell, which is what
        /// <see cref="CalculateExtrapolated(double[])"/> wants and what
        /// <see cref="Calculate(double[])"/> has already excluded.
        /// </summary>
        private double Interpolate(double[] coordinates)
        {
            int dimensions = axes.Length;

            int[] indices_Lower = new int[dimensions];
            double[] fractions = new double[dimensions];

            //Only a varying (Length > 1) axis contributes an upper corner - a singleton axis is a constant
            //with nothing to weight against. Enumerating corners over dimensions rather than over this list
            //is the defect this method used to have: a singleton axis has exactly one real point, but was
            //still given a bit in the corner index, so 24 singleton axes produced 2^24 nominal corners - all
            //but one of them immediately zero-weighted and discarded - for a grid holding a single value.
            List<int> dimensions_Varying = new List<int>();

            for (int i = 0; i < dimensions; i++)
            {
                double[] axis = axes[i];

                if (axis.Length == 1)
                {
                    //One tabulated coordinate is a constant in this dimension - there is no second corner to
                    //weight against and no gradient to continue.
                    indices_Lower[i] = 0;
                    fractions[i] = 0;
                    continue;
                }

                dimensions_Varying.Add(i);

                //The last cell whose lower bound is at or below the coordinate, held inside the grid so that
                //the top boundary uses the final cell rather than falling off the end. A coordinate sitting
                //exactly on an axis value gives a fraction of exactly 0 - or exactly 1 at the very top - so
                //grid points come back exactly.
                int index = 0;
                for (int j = 0; j < axis.Length - 1; j++)
                {
                    if (coordinates[i] >= axis[j])
                    {
                        index = j;
                    }
                    else
                    {
                        break;
                    }
                }

                indices_Lower[i] = index;
                fractions[i] = (coordinates[i] - axis[index]) / (axis[index + 1] - axis[index]);
            }

            double result = 0;

            int dimensionCount_Varying = dimensions_Varying.Count;
            int corners = 1 << dimensionCount_Varying;

            for (int corner = 0; corner < corners; corner++)
            {
                double weight = 1;
                int[] indices = (int[])indices_Lower.Clone();

                for (int bit = 0; bit < dimensionCount_Varying; bit++)
                {
                    int dimension = dimensions_Varying[bit];
                    bool upper = ((corner >> bit) & 1) == 1;

                    if (upper)
                    {
                        indices[dimension] = indices_Lower[dimension] + 1;
                        weight *= fractions[dimension];
                    }
                    else
                    {
                        indices[dimension] = indices_Lower[dimension];
                        weight *= 1 - fractions[dimension];
                    }
                }

                if (weight == 0)
                {
                    //Skipped rather than added: it contributes nothing, and skipping keeps a zero-weight
                    //corner out of the arithmetic entirely.
                    continue;
                }

                result += weight * Value(indices);
            }

            return result;
        }

        private static bool IsUsableGrid(List<double[]> axes, int valueCount)
        {
            //Corners are enumerated as a bit per dimension, so the count has to fit in an int shift. Far
            //beyond any real performance table - three or four conditions - but a shift of 32 or more wraps
            //silently in C# and would enumerate the wrong corners rather than fail.
            if (axes == null || axes.Count == 0 || axes.Count > 24)
            {
                return false;
            }

            long expected = 1;

            foreach (double[] axis in axes)
            {
                if (axis == null || axis.Length == 0)
                {
                    return false;
                }

                for (int i = 0; i < axis.Length; i++)
                {
                    if (double.IsNaN(axis[i]) || double.IsInfinity(axis[i]))
                    {
                        return false;
                    }

                    if (i > 0 && axis[i] <= axis[i - 1])
                    {
                        return false;
                    }
                }

                expected *= axis.Length;

                if (expected > int.MaxValue)
                {
                    return false;
                }
            }

            return expected == valueCount;
        }
    }
}
