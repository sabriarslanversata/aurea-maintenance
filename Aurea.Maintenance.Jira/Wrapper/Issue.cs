namespace Aurea.Maintenance.Jira.Wrapper
{
    /// <summary>
    /// An issue represents tasks, software bugs, feature requests or any other type of project
    /// work within the context of JIRA.
    /// </summary>
    public class Issue
    {
        /// <summary>
        /// Gets or sets the the unique global identifier of this <see cref="Issue"/>.
        /// </summary>
        /// <value>
        /// The global identifier string.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the self URI address of this <see cref="Issue"/> resource.
        /// </summary>
        /// <value>
        /// The self URI address string.
        /// </value>
        public string Self { get; set; }

        /// <summary>
        /// Gets or sets the unique golobal key of this <see cref="Issue"/>.
        /// </summary>
        /// <value>
        /// The key string.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the fields that composes this JIRA <see cref="Issue"/>.
        /// </summary>
        /// <value>
        /// The JIRA issue fields list.
        /// </value>
        public IssueFields Fields { get; set; }
    }
}
