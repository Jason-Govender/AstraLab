using System;

namespace AstraLab.Core.Domains.AI
{
    /// <summary>
    /// Defines the persisted assistant response categories supported by the platform.
    /// </summary>
    public enum AIResponseType
    {
        /// <summary>
        /// A high-level dataset or dataset-version summary response.
        /// </summary>
        Summary = 1,

        /// <summary>
        /// A recommendation response, such as suggested cleaning or transformation actions.
        /// </summary>
        Recommendation = 2,

        /// <summary>
        /// An explanatory response that clarifies a concept, result, or dataset behavior.
        /// </summary>
        Explanation = 3,

        /// <summary>
        /// A generated insight response that highlights notable patterns or findings.
        /// </summary>
        Insight = 4,

        /// <summary>
        /// A question-and-answer response grounded in the selected dataset context.
        /// </summary>
        QuestionAnswer = 5
    }
}
