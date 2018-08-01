namespace GitHub
{
    using GitHub.Internals;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class Grid<T, TClean>
    {
        private Dictionary<string, GridColumn> Contents { get; }

        public Grid(IEnumerable<Result<T, TClean>> results, List<GridColumnConfig<T, TClean>> columnConfigs)
        {
            Contents = new Dictionary<string, GridColumn>();

            foreach (var column in columnConfigs)
            {
                Contents.Add(column.Name, new GridColumn(column.Name));
            }

            foreach (var result in results)
            {
                foreach (var column in columnConfigs)
                {
                    Contents[column.Name].Rows.Add(column.ColumnFormatter(column.ControlRowSelector(result)));
                    foreach (var observation in result.Candidates)
                    {
                        Contents[column.Name].Rows.Add(column.ColumnFormatter(column.ObservationRowSelector(result, observation)));
                    }
                }
            }

            foreach (var column in columnConfigs)
            {
                Contents[column.Name].Length = Contents[column.Name].Rows.Max((s) => s.Length);
                Contents[column.Name].Length = (Contents[column.Name].Length < column.Name.Length) ? column.Name.Length : Contents[column.Name].Length;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var key in Contents.Keys)
            {
                builder.Append('|');
                builder.AppendFormat($"{{0,{-Contents[key].Length}}}", Contents[key].Header);
            }

            builder.Append('|');
            builder.Append(Environment.NewLine);

            foreach (var key in Contents.Keys)
            {
                builder.Append('|');
                builder.AppendFormat(string.Empty.PadRight(Contents[key].Length, '-'));
            }

            builder.Append('|');
            builder.Append(Environment.NewLine);

            for (int i = 1; i < Contents.Values.First().Rows.Count; i++)
            {
                foreach (var key in Contents.Keys)
                {
                    builder.Append('|');
                    builder.AppendFormat($"{{0,{Contents[key].Length}}}", Contents[key].Rows[i]);
                }
                builder.Append('|');
                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        public static List<GridColumnConfig<T, TClean>> DefaultColumns { get; } = 
                new List<GridColumnConfig<T, TClean>>()
                {
                    new GridColumnConfig<T, TClean>(
                                                            "Experiment",
                                                            (result) => result.ExperimentName,
                                                            (result, _) => result.ExperimentName,
                                                            (o) => (string)o
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Observation",
                                                            (result) => "Control",
                                                            (_, observation) => observation.Name,
                                                            (o) => (string)o
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Context",
                                                            (result) => Flatten(result.Contexts),
                                                            (result, _) => Flatten(result.Contexts),
                                                            (o) => (string)o
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Control",
                                                            (result) => result.Control.Value,
                                                            (result, _) => result.Control.Value,
                                                            (o) => o.ToString()
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Observed",
                                                            (result) => result.Control.Value,
                                                            (_, observation) => observation.Value,
                                                            (o) => o.ToString()
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Duration",
                                                            (result) => result.Control.Duration,
                                                            (_, observation) => observation.Duration,
                                                            (o) => ((TimeSpan)o).ToScaledString()
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Time Delta",
                                                            (result) => TimeSpan.Zero,
                                                            (result, observation) => observation.Duration - result.Control.Duration,
                                                            (o) => ((TimeSpan)o).ToScaledString()
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Allocated",
                                                            (result) => result.Control.AllocatedBytes,
                                                            (_, observation) => observation.AllocatedBytes,
                                                            (o) => ((long)o).ToByteScaleString()
                                                            ),
                    new GridColumnConfig<T, TClean>(
                                                            "Bytes Delta",
                                                            (result) => 0L,
                                                            (result, observation) => observation.AllocatedBytes - result.Control.AllocatedBytes,
                                                            (o) => ((long)o).ToByteScaleString()
                                                            ),
                };

        private static string Flatten<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            var builder = new StringBuilder();

            foreach (TValue value in dictionary.Values)
            {
                builder.Append(value);
                builder.Append(',');
            }
            return builder.ToString();
        }
    }
}