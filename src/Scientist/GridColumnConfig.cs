namespace GitHub
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GitHub.Internals;

    public class GridColumnConfig<T, TClean>
    {
        public string Name { get; }

        public Func<Result<T, TClean>, object> ControlRowSelector { get; }
        public Func<Result<T, TClean>, Observation<T, TClean>, object> ObservationRowSelector { get; }

        public Func<object, string> ColumnFormatter { get; }

        public GridColumnConfig(string name, Func<Result<T, TClean>, object> controlRowSelector, Func<Result<T, TClean>, Observation<T, TClean>, object> observationRowSelector, Func<object, string> columnFormatter)
        {
            this.Name = name;
            this.ColumnFormatter = columnFormatter;
            this.ControlRowSelector = controlRowSelector;
            this.ObservationRowSelector = observationRowSelector;
        }
    }
}
