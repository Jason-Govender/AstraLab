namespace AstraLab.Core.Domains.ML
{
    /// <summary>
    /// Represents the supported machine learning workflow categories.
    /// </summary>
    public enum MLTaskType
    {
        /// <summary>
        /// Legacy or unknown workflow type for experiments created before task types were tracked explicitly.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Supervised classification workflow.
        /// </summary>
        Classification = 1,

        /// <summary>
        /// Supervised regression workflow.
        /// </summary>
        Regression = 2,

        /// <summary>
        /// Unsupervised clustering workflow.
        /// </summary>
        Clustering = 3,

        /// <summary>
        /// Basic anomaly detection workflow.
        /// </summary>
        AnomalyDetection = 4
    }
}
