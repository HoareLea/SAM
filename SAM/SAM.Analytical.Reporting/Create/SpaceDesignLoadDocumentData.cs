// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Core.Reporting;
using SAM.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Reporting
{
    public static partial class Create
    {
        /// <summary>
        /// The year an annual peak's hour of the year is placed in to give it a calendar time. Results carry no year,
        /// so it is a fixed non-leap reference (the one SAM uses for its 8760-hour series); show month, day and time
        /// only.
        /// </summary>
        public const int ReferenceYear = 2018;

        /// <summary>
        /// Collects the Space Design Load Summary data of one space: the Phase-1 identity, design criteria and sizing
        /// (collected exactly as for the Space Assumptions document), and the heating and cooling peaks of the stored
        /// simulation results (<see cref="SpaceLoadResultData(DocumentContext, Space, LoadType, string)"/>).
        /// </summary>
        /// <param name="resultSource">
        /// Only results with this source are read (Result.Source). Null reads every source; then more than one result
        /// carrying peaks for a load type is <see cref="LoadResultStatus.Ambiguous"/>.
        /// </param>
        public static SpaceDesignLoadDocumentData SpaceDesignLoadDocumentData(DocumentContext documentContext, Space space, string resultSource = null)
        {
            if (documentContext == null)
            {
                throw new ArgumentNullException(nameof(documentContext));
            }

            if (space == null)
            {
                throw new ArgumentNullException(nameof(space));
            }

            Collector collector = new Collector(documentContext, space);

            return new SpaceDesignLoadDocumentData()
            {
                Identity = collector.Identity(),
                DesignCriteria = collector.DesignCriteria(),
                Sizing = collector.Sizing(collector.Geometry()),
                Heating = SpaceLoadResultData(documentContext, space, LoadType.Heating, resultSource),
                Cooling = SpaceLoadResultData(documentContext, space, LoadType.Cooling, resultSource),
                Provenance = documentContext.Provenance,
            };
        }

        /// <summary>
        /// Reads a space's design-day and full-year peaks of one load type from the typed result contract only:
        /// <see cref="SpaceSimulationResultParameter.DesignDayPeak"/> and <see cref="SpaceSimulationResultParameter.AnnualPeak"/>
        /// on the <see cref="SpaceSimulationResult"/>s related to the space. The legacy values (Load, LoadIndex,
        /// SizingMethod and their -1 sentinels) are never read, whichever engine wrote the result.
        /// <para>
        /// The two peaks stay independent, a missing peak stays unavailable and a zero peak stays a zero; values are
        /// passed through unchanged (component signs included) and nothing is derived. Expected gaps become
        /// NotAvailable / NotApplicable with a reason and a warning in <see cref="DocumentContext.Diagnostics"/>.
        /// </para>
        /// </summary>
        public static SpaceLoadResultData SpaceLoadResultData(DocumentContext documentContext, Space space, LoadType loadType, string resultSource = null)
        {
            if (documentContext == null)
            {
                throw new ArgumentNullException(nameof(documentContext));
            }

            if (space == null)
            {
                throw new ArgumentNullException(nameof(space));
            }

            if (loadType != LoadType.Heating && loadType != LoadType.Cooling)
            {
                throw new ArgumentOutOfRangeException(nameof(loadType), loadType, "Only heating and cooling results have load peaks.");
            }

            return new LoadResultCollector(documentContext, space, loadType).Collect(resultSource);
        }

        /// <summary>
        /// Reads one load type of one space. Kept private so the reading rules (reasons, warnings, state) stay in one place.
        /// </summary>
        private sealed class LoadResultCollector
        {
            private const string InvalidValue = "invalid value in results";
            private const string NoPeakTimestep = "No demand: no peak timestep";
            private const string NotReported = "Not reported by the simulation";

            private readonly DocumentContext documentContext;
            private readonly Space space;
            private readonly LoadType loadType;
            private readonly string what;

            public LoadResultCollector(DocumentContext documentContext, Space space, LoadType loadType)
            {
                this.documentContext = documentContext;
                this.space = space;
                this.loadType = loadType;
                what = loadType == LoadType.Heating ? "heating" : "cooling";
            }

            private string SpaceName => string.IsNullOrWhiteSpace(space.Name) ? space.Guid.ToString() : space.Name;

            public SpaceLoadResultData Collect(string resultSource)
            {
                List<SpaceSimulationResult> spaceSimulationResults = documentContext.AdjacencyCluster?.GetResults<SpaceSimulationResult>(space, resultSource)?
                    .FindAll(x => x != null && x.LoadType() == loadType) ?? new List<SpaceSimulationResult>();

                List<SpaceSimulationResult> withPeaks = spaceSimulationResults.FindAll(HasPeak);

                if (withPeaks.Count == 1)
                {
                    SpaceSimulationResult spaceSimulationResult = withPeaks[0];
                    spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.DesignDayPeak, out SpaceLoadPeak designDay);
                    spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.AnnualPeak, out SpaceLoadPeak annual);

                    return new SpaceLoadResultData()
                    {
                        LoadType = loadType,
                        Status = LoadResultStatus.Available,
                        ResultSource = string.IsNullOrWhiteSpace(spaceSimulationResult.Source)
                            ? ReportValue<string>.NotAvailable("Result has no source")
                            : ReportValue<string>.Available(spaceSimulationResult.Source.Trim(), ReportValueSource.SimulationResult, Freshness.Unknown),
                        ConvertedAt = spaceSimulationResult.DateTime == DateTime.MinValue
                            ? ReportValue<DateTime>.NotAvailable("Result has no time")
                            : ReportValue<DateTime>.Available(spaceSimulationResult.DateTime, ReportValueSource.SimulationResult, Freshness.Unknown, note: "When the results were read into the model"),
                        DesignDay = Peak(designDay, LoadPeakBasis.DesignDay),
                        Annual = Peak(annual, LoadPeakBasis.AnnualSimulation),
                    };
                }

                LoadResultStatus loadResultStatus;
                string reason;
                if (withPeaks.Count > 1)
                {
                    loadResultStatus = LoadResultStatus.Ambiguous;
                    reason = string.Format("{0} {1} results carry peaks; none chosen", withPeaks.Count, what);
                }
                else if (spaceSimulationResults.Count != 0)
                {
                    loadResultStatus = LoadResultStatus.PeaksNotRecorded;
                    reason = string.Format("The {0} results record no design-day or annual peak: re-run the simulation", what);
                }
                else
                {
                    loadResultStatus = LoadResultStatus.NotSimulated;
                    reason = string.Format("No {0} results", what);
                }

                Warning(reason);

                return new SpaceLoadResultData()
                {
                    LoadType = loadType,
                    Status = loadResultStatus,
                    ResultSource = ReportValue<string>.NotAvailable(reason),
                    ConvertedAt = ReportValue<DateTime>.NotAvailable(reason),
                    DesignDay = Unavailable(LoadPeakBasis.DesignDay, reason),
                    Annual = Unavailable(LoadPeakBasis.AnnualSimulation, reason),
                };
            }

            private static bool HasPeak(SpaceSimulationResult spaceSimulationResult)
            {
                return spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.DesignDayPeak, out SpaceLoadPeak designDay) && designDay != null
                    || spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.AnnualPeak, out SpaceLoadPeak annual) && annual != null;
            }

            private SpaceLoadPeakData Peak(SpaceLoadPeak spaceLoadPeak, LoadPeakBasis basis)
            {
                string peak = string.Format("{0} {1} peak", basis == LoadPeakBasis.DesignDay ? "design-day" : "annual", what);

                if (spaceLoadPeak == null)
                {
                    string reason = string.Format("No {0} in the results", peak);
                    Warning(reason);
                    return Unavailable(basis, reason);
                }

                if (spaceLoadPeak.Basis != basis)
                {
                    Warning(string.Format("the {0} is recorded as {1}, an {2}", peak, spaceLoadPeak.Basis, InvalidValue));
                    return Unavailable(basis, InvalidValue);
                }

                double load = spaceLoadPeak.Load;
                if (double.IsNaN(load) || double.IsInfinity(load) || load < 0)
                {
                    Warning(string.Format("the {0} load is {1}, an {2}", peak, load, InvalidValue));
                    return Unavailable(basis, InvalidValue);
                }

                LoadPeakState loadPeakState = load == 0 ? LoadPeakState.Zero : LoadPeakState.Value;

                //A value the peak does not carry: nothing to report at a zero peak (no timestep), unreported at a real one.
                string absent = loadPeakState == LoadPeakState.Zero ? NoPeakTimestep : NotReported;

                ReportValue<string> designDayName = basis != LoadPeakBasis.DesignDay
                    ? ReportValue<string>.NotApplicable("Full-year peak")
                    : string.IsNullOrWhiteSpace(spaceLoadPeak.DesignDayName)
                        ? ReportValue<string>.NotAvailable(NotReported)
                        : ReportValue<string>.Available(spaceLoadPeak.DesignDayName.Trim(), ReportValueSource.SimulationResult, Freshness.Unknown);

                ReportValue<int> hourOfYear;
                ReportValue<DateTime> time;
                if (basis == LoadPeakBasis.DesignDay)
                {
                    //A design day's place in the engine's series is not a date: never build one.
                    hourOfYear = ReportValue<int>.NotApplicable("Design day: no calendar date");
                    time = ReportValue<DateTime>.NotApplicable("Design day: no calendar date");
                }
                else
                {
                    hourOfYear = Hour(spaceLoadPeak.HourOfYear, 8759, peak, "hour of the year", absent);
                    if (!hourOfYear.HasValue)
                    {
                        time = hourOfYear.Availability == Availability.NotApplicable ? ReportValue<DateTime>.NotApplicable(hourOfYear.Note) : ReportValue<DateTime>.NotAvailable(hourOfYear.Note);
                    }
                    else if (spaceLoadPeak.TryGetDateTime(ReferenceYear, out DateTime dateTime))
                    {
                        time = ReportValue<DateTime>.Available(dateTime, ReportValueSource.SimulationResult, Freshness.Unknown, note: "Start of the peak hour");
                    }
                    else
                    {
                        time = ReportValue<DateTime>.NotAvailable(InvalidValue);
                    }
                }

                return new SpaceLoadPeakData()
                {
                    Basis = basis,
                    State = loadPeakState,
                    Load = ReportValue<Quantity>.Available(new Quantity(load, UnitType.Watt), ReportValueSource.SimulationResult, Freshness.Unknown),
                    DesignDayName = designDayName,
                    HourOfDay = Hour(spaceLoadPeak.HourOfDay, 23, peak, "hour of the day", absent),
                    HourOfYear = hourOfYear,
                    Time = time,
                    RoomDryBulbTemperature = Value(spaceLoadPeak.DryBulbTemperature, UnitType.Celsius, peak, "room dry bulb", absent),
                    RoomResultantTemperature = Value(spaceLoadPeak.ResultantTemperature, UnitType.Celsius, peak, "room resultant temperature", absent),
                    RoomRelativeHumidity = Value(spaceLoadPeak.RelativeHumidity, UnitType.Percent, peak, "room relative humidity", absent),
                    RoomHumidityRatio = Value(spaceLoadPeak.HumidityRatio, UnitType.KilogramPerKilogram, peak, "room humidity ratio", absent),
                    OutdoorDryBulbTemperature = Value(spaceLoadPeak.OutdoorDryBulbTemperature, UnitType.Celsius, peak, "outdoor dry bulb", absent),
                    OutdoorRelativeHumidity = Value(spaceLoadPeak.OutdoorRelativeHumidity, UnitType.Percent, peak, "outdoor relative humidity", absent),
                    SensibleComponents = Components(spaceLoadPeak, false),
                    LatentComponents = Components(spaceLoadPeak, true),
                };
            }

            private static SpaceLoadPeakData Unavailable(LoadPeakBasis basis, string reason)
            {
                IReadOnlyList<SpaceLoadPeakComponentData> none = new List<SpaceLoadPeakComponentData>().AsReadOnly();
                ReportValue<Quantity> quantity = ReportValue<Quantity>.NotAvailable(reason);
                ReportValue<int> hour = ReportValue<int>.NotAvailable(reason);

                return new SpaceLoadPeakData()
                {
                    Basis = basis,
                    State = LoadPeakState.Unavailable,
                    Load = quantity,
                    DesignDayName = basis == LoadPeakBasis.DesignDay ? ReportValue<string>.NotAvailable(reason) : ReportValue<string>.NotApplicable("Full-year peak"),
                    HourOfDay = hour,
                    HourOfYear = basis == LoadPeakBasis.DesignDay ? ReportValue<int>.NotApplicable("Design day: no calendar date") : hour,
                    Time = basis == LoadPeakBasis.DesignDay ? ReportValue<DateTime>.NotApplicable("Design day: no calendar date") : ReportValue<DateTime>.NotAvailable(reason),
                    RoomDryBulbTemperature = quantity,
                    RoomResultantTemperature = quantity,
                    RoomRelativeHumidity = quantity,
                    RoomHumidityRatio = quantity,
                    OutdoorDryBulbTemperature = quantity,
                    OutdoorRelativeHumidity = quantity,
                    SensibleComponents = none,
                    LatentComponents = none,
                };
            }

            private ReportValue<int> Hour(int? value, int maximum, string peak, string name, string absent)
            {
                if (value is not int hour)
                {
                    return absent == NoPeakTimestep ? ReportValue<int>.NotApplicable(absent) : ReportValue<int>.NotAvailable(absent);
                }

                if (hour < 0 || hour > maximum)
                {
                    Warning(string.Format("the {0} {1} is {2}, an {3}", peak, name, hour, InvalidValue));
                    return ReportValue<int>.NotAvailable(InvalidValue);
                }

                return ReportValue<int>.Available(hour, ReportValueSource.SimulationResult, Freshness.Unknown);
            }

            private ReportValue<Quantity> Value(double? value, UnitType unitType, string peak, string name, string absent)
            {
                if (value is not double @double)
                {
                    return absent == NoPeakTimestep ? ReportValue<Quantity>.NotApplicable(absent) : ReportValue<Quantity>.NotAvailable(absent);
                }

                if (double.IsNaN(@double) || double.IsInfinity(@double))
                {
                    Warning(string.Format("the {0} {1} is {2}, an {3}", peak, name, @double, InvalidValue));
                    return ReportValue<Quantity>.NotAvailable(InvalidValue);
                }

                return ReportValue<Quantity>.Available(new Quantity(@double, unitType), ReportValueSource.SimulationResult, Freshness.Unknown);
            }

            /// <summary>
            /// The stored terms, sign untouched. Latent terms (<see cref="LoadPeakComponent.OccupancyLatent"/>,
            /// <see cref="LoadPeakComponent.EquipmentLatent"/>) are listed apart from the sensible ones; no term is added.
            /// </summary>
            private static IReadOnlyList<SpaceLoadPeakComponentData> Components(SpaceLoadPeak spaceLoadPeak, bool latent)
            {
                return spaceLoadPeak.Components
                    .Where(x => IsLatent(x.Key) == latent)
                    .OrderBy(x => x.Key)
                    .Select(x => new SpaceLoadPeakComponentData()
                    {
                        Component = x.Key,
                        Value = ReportValue<Quantity>.Available(new Quantity(x.Value, UnitType.Watt), ReportValueSource.SimulationResult, Freshness.Unknown),
                    })
                    .ToList()
                    .AsReadOnly();
            }

            private static bool IsLatent(LoadPeakComponent loadPeakComponent)
            {
                return loadPeakComponent == LoadPeakComponent.OccupancyLatent || loadPeakComponent == LoadPeakComponent.EquipmentLatent;
            }

            private void Warning(string message)
            {
                documentContext.Diagnostics.Add("Space {0}: {1}", LogRecordType.Warning, SpaceName, message.Substring(0, 1).ToLowerInvariant() + message.Substring(1));
            }
        }
    }
}
