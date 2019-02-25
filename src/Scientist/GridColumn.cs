namespace GitHub
{
    using System.Collections.Generic;

    public sealed class GridColumn
    {
        public int Length { get; internal set; }

        public string Header { get; }

        public List<string> Rows { get; } = new List<string>();

        public GridColumn(string columnHeader)
        {
            Header = columnHeader;
        }
    }
}
