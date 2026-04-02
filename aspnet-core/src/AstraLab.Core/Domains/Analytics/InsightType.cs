namespace AstraLab.Core.Domains.Analytics
{
    /// <summary>
    /// Classifies the type of persisted analytics insight content.
    /// </summary>
    public enum InsightType
    {
        /// <summary>
        /// Represents a general summary insight.
        /// </summary>
        Summary = 1,

        /// <summary>
        /// Represents a data-quality-focused insight.
        /// </summary>
        DataQuality = 2,

        /// <summary>
        /// Represents an anomaly or notable-pattern insight.
        /// </summary>
        Pattern = 3,

        /// <summary>
        /// Represents a recommendation or next-step insight.
        /// </summary>
        Recommendation = 4,

        /// <summary>
        /// Represents a machine-learning-result-focused insight.
        /// </summary>
        Experiment = 5
    }
}
