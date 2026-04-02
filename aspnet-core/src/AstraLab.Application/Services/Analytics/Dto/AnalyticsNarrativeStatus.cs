namespace AstraLab.Services.Analytics.Dto
{
    /// <summary>
    /// Describes the availability state of a generated analytics narrative.
    /// </summary>
    public enum AnalyticsNarrativeStatus
    {
        /// <summary>
        /// A narrative was generated successfully.
        /// </summary>
        Generated = 1,

        /// <summary>
        /// A narrative was not generated because there was not enough meaningful context.
        /// </summary>
        Unavailable = 2,

        /// <summary>
        /// A narrative was attempted but the generation flow failed.
        /// </summary>
        Failed = 3
    }
}
