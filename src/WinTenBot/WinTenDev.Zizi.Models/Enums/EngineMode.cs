namespace WinTenDev.Zizi.Models.Enums
{
    /// <summary>
    /// Configure how to Bots running.
    /// </summary>
    public enum EngineMode
    {
        /// <summary>
        /// Bot will running as Polling in Development, and running as WebHook in Production
        /// </summary>
        FollowHost,

        /// <summary>
        /// Bot will running as Polling
        /// </summary>
        Polling,

        /// <summary>
        /// Bot will running as WebHook
        /// </summary>
        WebHook
    }
}