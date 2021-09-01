namespace Assistant.Net.Scheduler.Api.Enums
{
    /// <summary>
    ///     Automation job triggered by event type.
    /// </summary>
    public enum JobTriggerType
    {

        /// <summary>
        ///     A previous job from current automation is completed.
        /// </summary>
        None,

        /// <summary>
        ///     A job has been triggered manually.
        /// </summary>
        EventTrigger,

        /// <summary>
        ///     The timer notified scheduled time had come.
        /// </summary>
        TimerTrigger
    }
}