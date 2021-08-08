namespace GitHub
{
    /// <summary>
    /// Defines the stage during the experiment's life cycle
    /// that an exception was thrown.
    /// </summary>
    public enum Operation
    {
        Clean,
        Compare,
        Enabled,
        Ignore,
        Publish,
        RunIf
    }
}
