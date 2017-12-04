namespace Aurea.Maintenance.Jira.Wrapper
{
    public class IssueLink
    {
        public string Id { get; set; }

        public string Self { get; set; }

        public IssueLinkType Type { get; set; }

        public IssueReference InwardIssue { get; set; }

        public IssueReference OutwardIssue { get; set; }
    }
}