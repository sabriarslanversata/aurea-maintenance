namespace Aurea.Maintenance.Jira
{
    using System;
    using System.Collections.Generic;
    using Aurea.Maintenance.Jira.Wrapper;

    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new JiraRestClient("https://jira.devfactory.com", "kkilicoglu", "xxx");

            var user = client.GetUser("kkilicoglu");

            foreach(var worklog in DefaultWorklogs(user))
            {
                client.AddWorklogToIssue("AESCIS-16741", worklog);
            }

            //"AESCIS-16741"
            Console.ReadLine();
        }

        private static List<Worklog> DefaultWorklogs(User user)
        {
            var logList = new List<Worklog>();

            var defaultComments = new List<string>
            {
                "Find, read and understand Jira ticket as well as understand logs.",
                "Obtain source code based on version.",
                "Review product documentation.",
                "Chat with support.",
                "Chat with client.",
                "Fire up product and environment.",
                "Test and recreate the bug.",
                "Evaluate if regression.",
                "Verify against logs.",
                "Create unit tests.",
                "Fix bug.",
                "Test fix.",
                "Merge changes.",
                "Update technical documentation.",
                "Communicate with product team on closure of ticket."
            };

            foreach (var comment in defaultComments)
            {
                logList.Add(new Worklog
                {
                    Author = user,
                    Created = DateTime.Now,
                    Comment = comment,
                    Started = DateTime.Now,
                    TimeSpentSeconds = 60
                });
            }

            return logList;
        }
    }
}
