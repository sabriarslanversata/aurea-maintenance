namespace Aurea.Maintenance.Jira.Wrapper
{
    using System.Collections.Generic;

    public class Project
    {
        public string Expand { get; set; }

        public string Self { get; set; }

        public string Id { get; set; }

        public string Key { get; set; }

        public string Description { get; set; }

        public User Lead { get; set; }

        public List<Component> Components { get; set; }

        public List<IssueType> IssueTypes { get; set; }

        public string Url { get; set; }

        public string Email { get; set; }

        public AssigneeType AssigneeType { get; set; }

        public List<Version> Versions { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> Roles { get; set; }

        public Dictionary<string, string> AvatarUrls { get; set; }

        public List<string> ProjectKeys { get; set; }

        public ProjectCategory ProjectCategory { get; set; }

        public string ProjectTypeKey { get; set; }
    }

    public class ProjectCategory
    {
        public string Self { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class Version
    {
        public string Expand { get; set; }

        public string Self { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public bool Archived { get; set; }

        public bool Released { get; set; }

        public bool Overdue { get; set; }

        public string UserStartDate { get; set; }

        public string UserReleaseDate { get; set; }

        public string Project { get; set; }

        public int ProjectId { get; set; }

        public string MoveUnfixedIssuesTo { get; set; }

        public List<SimpleLink> Operations { get; set; }

        public List<RemoteEntityLink> RemoteLinks { get; set; }
    }

    public class RemoteEntityLink
    {
        public string Self { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }
    }

    public class SimpleLink
    {
        public string Id { get; set; }

        public string StyleClass { get; set; }

        public string IconClass { get; set; }

        public string Label { get; set; }

        public string Title { get; set; }

        public string Href { get; set; }

        public int Weight { get; set; }
    }

    public enum AssigneeType
    {
        PROJECT_DEFAULT,
        COMPONENT_LEAD,
        PROJECT_LEAD,
        UNASSIGNED
    }

    public class IssueType
    {
        public string Self { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public string IconUrl { get; set; }

        public string Name { get; set; }

        public bool SubTask { get; set; }

        public int AvatarId { get; set; }
    }

    public class Component
    {
        public string Self { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public User Lead { get; set; }

        public string LeadUserName { get; set; }

        public AssigneeType AssigneeType { get; set; }

        public User Assignee { get; set; }

        public AssigneeType RealAssigneeType { get; set; }

        public User RealAssignee { get; set; }

        public bool IsAssigneeTypeValid { get; set; }

        public string Project { get; set; }

        public int ProjectId { get; set; }
    }
}
