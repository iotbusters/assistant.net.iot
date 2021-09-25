namespace Assistant.Net.Scheduler.Contracts.Enums
{
    /// <summary>
    ///     Automation job type strategy.
    /// </summary>
    public enum JobType
    {
        /// <summary>
        ///     Delegates to next job in the automation.
        /// </summary>
        Nothing,

        /// <summary>
        ///     Executes predefined custom action due to parameters.
        /// </summary>
        Custom,

        /// <summary>
        ///     Produces an event due to parameters.
        /// </summary>
        Event,

        /// <summary>
        ///     Verifies condition due to parameters if next job delegation is allowed.
        /// </summary>
        Condition
    }
}
