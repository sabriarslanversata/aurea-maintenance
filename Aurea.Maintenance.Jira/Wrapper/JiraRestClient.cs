namespace Aurea.Maintenance.Jira.Wrapper
{
    using System;
    using System.Text;
    using System.Net;
    using RestSharp;
    using RestSharp.Deserializers;
    using System.Collections.Generic;

    public class JiraRestClient
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _baseUrl;
        private readonly JsonDeserializer _deserializer;

        public JiraRestClient(string baseUrl, string username, string password)
        {
            _username = username;
            _password = password;

            _baseUrl = new Uri(new Uri(baseUrl), "rest/api/2/").ToString();

            _deserializer = new JsonDeserializer();
        }

        public User GetUser(string username)
        {
            try
            {
                var resource = $"user?username={username}";
                var request = CreateRequest(Method.GET, resource);
                var response = ExecuteRequest(request);

                var user = _deserializer.Deserialize<User>(response);

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load user!", ex);
            }
        }

        public Issue GetIssue(string issueKey)
        {
            try
            {
                var resource = $"issue/{issueKey}";
                var request = CreateRequest(Method.GET, resource);
                var response = ExecuteRequest(request);

                return _deserializer.Deserialize<Issue>(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load issue!", ex);
            }
        }

        public Project GetProject(string projectKey)
        {
            try
            {
                var resource = $"project/{projectKey}";
                var request = CreateRequest(Method.GET, resource);
                var response = ExecuteRequest(request);

                return _deserializer.Deserialize<Project>(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load project!", ex);
            }
        }

        public WorklogWrapper GetIssueWorklog(string issueKey)
        {
            try
            {
                var resource = $"issue/{issueKey}/worklog";
                var request = CreateRequest(Method.GET, resource);
                var response = ExecuteRequest(request);

                return _deserializer.Deserialize<WorklogWrapper>(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load issue!", ex);
            }
        }

        public Worklog AddWorklogToIssue(string issueKey, Worklog worklog)
        {
            try
            {
                var resource = $"issue/{issueKey}/worklog";
                var request = CreateRequest(Method.POST, resource);

                request.AddHeader(
                    name: "ContentType",
                    value: "application/json");

                request.AddBody(new
                {
                    comment = worklog.Comment,
                    timeSpentSeconds = worklog.TimeSpentSeconds
                });

                var response = ExecuteRequest(request);

                return _deserializer.Deserialize<Worklog>(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not add worklog!", ex);
            }
        }

        private RestRequest CreateRequest(Method method, String resource)
        {
            var request = new RestRequest
            {
                Method = method,
                Resource = resource,
                RequestFormat = DataFormat.Json
            };

            request.AddHeader(
                name: "Authorization",
                value: $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", _username, _password)))}"
                );

            return request;
        }

        private IRestResponse ExecuteRequest(RestRequest request)
        {
            var client = new RestClient(_baseUrl);

            return client.Execute(request);
        }

        private void AssertStatus(IRestResponse response, HttpStatusCode statusCode)
        {
            if (response.ErrorException != null)
            {
                throw new Exception(
                    message: $"Transport level error: {response.ErrorMessage}",
                    innerException: response.ErrorException);
            }

            if (response.StatusCode != statusCode)
            {
                throw new Exception( $"JIRA returned wrong status: {response.StatusDescription}")
                {
                    Source = response.Content
                };
            }
        }
    }
}
