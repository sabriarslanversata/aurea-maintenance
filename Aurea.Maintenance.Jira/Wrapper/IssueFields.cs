namespace Aurea.Maintenance.Jira.Wrapper
{
    using System;
    using System.Collections.Generic;

    public class IssueFields
    {
        public IssueResolution Resolution { get; set; }

        public DateTime LastViewed { get; set; }

        public List<IssueLink> IssueLinks { get; set; }

        public User Assignee { get; set; }

        public Project Project { get; set; }

        public List<Worklog> Worklog { get; set; }
    }

    public class Worklog
    {
        public string Self { get; set; }

        public User Author { get; set; }

        public User UpdateAuthor { get; set; }

        public string Comment { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public Visibility Visibility { get; set; }

        public DateTime Started { get; set; }

        public string TimeSpent { get; set; }

        public long TimeSpentSeconds { get; set; }

        public string Id { get; set; }

        public string IssueId { get; set; }
    }

    public class WorklogWrapper
    {
        public long MaxResults { get; set; }
        public long StartAt { get; set; }
        public long Total { get; set; }
        public List<Worklog> Worklogs { get; set; }
    }
}