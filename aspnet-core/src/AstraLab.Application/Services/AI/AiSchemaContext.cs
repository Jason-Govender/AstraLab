namespace AstraLab.Services.AI
{
    /// <summary>
    /// Represents the schema-level summary included in AI context assembly.
    /// </summary>
    public class AiSchemaContext
    {
        /// <summary>
        /// Gets or sets the total number of persisted columns for the selected dataset version.
        /// </summary>
        public int TotalColumnCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a serialized schema payload exists.
        /// </summary>
        public bool HasSchemaJson { get; set; }

        /// <summary>
        /// Gets or sets the compact serialized schema preview.
        /// </summary>
        public string SchemaJsonPreview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the serialized schema preview was truncated.
        /// </summary>
        public bool SchemaJsonWasTruncated { get; set; }
    }
}
