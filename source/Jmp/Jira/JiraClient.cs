using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jmp.Jira
{
    public class JiraClient : IJiraClient
    {
        public Issue[] GetIssues(string jiraApiUrl, string jiraUserName, string jiraPassword, string includeLabel)
        {
            var results = new List<Issue>();
            var client = new RestClient(jiraApiUrl);
            client.Authenticator = new HttpBasicAuthenticator(jiraUserName, jiraPassword);
            var startAt = 0;
            while (true)
            {
                var request = new RestRequest("search", Method.GET);
                request.AddParameter("jql", string.Format("labels={0}", includeLabel));
                request.AddParameter("fields", "summary,labels,priority,timetracking");
                request.AddParameter("startAt", startAt);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException(string.Format("Jira response: {0}", response.StatusCode.ToString()));
                }
                var content = response.Content;
                var page = JsonConvert.DeserializeObject<IssueSearchResults>(content);
                results.AddRange(page.Issues);

                if (startAt + page.MaxResults > page.Total)
                {
                    break;
                }
                startAt += page.MaxResults;
            }
            return results.ToArray();
        }
    }
}
