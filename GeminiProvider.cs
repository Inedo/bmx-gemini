using System;
using System.Linq;
using System.Xml;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Gemini
{
    /// <summary>
    /// CounterSoft Gemini issue tracker provider.
    /// </summary>
    [ProviderProperties(
      "CounterSoft Gemini",
      "Supports Gemini 4.2 and later; requires .NET 3.5 to be installed.")]
    [CustomEditor(typeof(GeminiProviderEditor))]
    public sealed class GeminiProvider : IssueTrackingProviderBase, ICategoryFilterable, IUpdatingProvider
    {
        private const string IssueUrlFormat = "issue/ViewIssue.aspx?id={0}";

        private CounterSoft.Gemini.WebServices.ServiceManager serviceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiProvider"/> class.
        /// </summary>
        public GeminiProvider()
        {
        }

        /// <summary>
        /// Gets or sets the URL of the Gemini server.
        /// </summary>
        [Persistent]
        public string BaseUrl { get; set; }
        /// <summary>
        /// Gets or sets the Gemini user name.
        /// </summary>
        [Persistent]
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the Gemini password.
        /// </summary>
        [Persistent]
        public string Password { get; set; }

        public string[] CategoryIdFilter { get; set; }
        public string[] CategoryTypeNames
        {
            get { return new[] { "Project" }; }
        }

        public bool CanAppendIssueDescriptions
        {
            get { return true; }
        }
        public bool CanChangeIssueStatuses
        {
            get { return true; }
        }
        public bool CanCloseIssues
        {
            get { return true; }
        }

        private CounterSoft.Gemini.WebServices.ServiceManager ServiceManager
        {
            get
            {
                if (this.serviceManager == null)
                    this.serviceManager = new CounterSoft.Gemini.WebServices.ServiceManager(this.BaseUrl, this.UserName, this.Password, string.Empty, false);

                return this.serviceManager;
            }
        }

        public override string GetIssueUrl(Issue issue)
        {
            int id = GetIssueId(issue.IssueId);
            return new Uri(new Uri(this.BaseUrl), string.Format(IssueUrlFormat, id)).ToString();
        }

        public override Issue[] GetIssues(string releaseNumber)
        {
            if (this.CategoryIdFilter == null || this.CategoryIdFilter.Length < 1 || string.IsNullOrEmpty(this.CategoryIdFilter[0]))
                return new Issue[0];

            int projectId;
            if (!int.TryParse(this.CategoryIdFilter[0], out projectId))
                return new Issue[0];

            var filter = CounterSoft.Gemini.Commons.Entity.IssuesFilterEN.CreateProjectFilter(-1, projectId);
            var issues = this.ServiceManager.IssuesService.GetFilteredIssues(filter);

            return issues.Where(i => string.Equals(i.FixedInVersionNumber, releaseNumber, StringComparison.CurrentCultureIgnoreCase))
                         .Select(i => new GeminiIssue(i))
                         .ToArray();
        }
        public override bool IsIssueClosed(Issue issue)
        {
            return ((GeminiIssue)issue).IsClosed;
        }
        public override bool IsAvailable()
        {
            return true;
        }
        public override void ValidateConnection()
        {
            try
            {
                GetCategories();
            }
            catch(Exception ex)
            {
                throw new NotAvailableException(ex.Message, ex);
            }
        }
        public override string ToString()
        {
            return "Connects to the CounterSoft Gemini issue tracking system.";
        }
        public CategoryBase[] GetCategories()
        {
            return this.ServiceManager.ProjectsService.GetProjects().Select(p => new GeminiProject(p)).ToArray();
        }
        public void AppendIssueDescription(string issueId, string textToAppend)
        {
            var issue = this.ServiceManager.IssuesService.GetIssue(GetIssueId(issueId));
            issue.IssueLongDesc += textToAppend;

            try
            {
                this.ServiceManager.IssuesService.UpdateIssue(issue);
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException("Unable to update issue description. IIS may not be configured to allow all Gemini REST verbs.", ex);
            }
        }
        public void ChangeIssueStatus(string issueId, string newStatus)
        {
            var statuses = this.ServiceManager.AdminService.GetIssueStatus();

            CounterSoft.Gemini.Commons.Entity.IssueStatusEN status;

            try
            {
                status = statuses.Where(s => string.Equals(s.Description, newStatus, StringComparison.CurrentCultureIgnoreCase))
                                 .First();
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException("Invalid status.", ex);
            }

            var issue = this.ServiceManager.IssuesService.GetIssue(GetIssueId(issueId));
            issue.IssueStatus = status.StatusID;

            try
            {
                this.ServiceManager.IssuesService.UpdateIssue(issue);
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException("Unable to update issue status. IIS may not be configured to allow all Gemini REST verbs.", ex);
            }
        }
        public void CloseIssue(string issueId)
        {
            ChangeIssueStatus(issueId, "Closed");
        }

        private int GetIssueId(string issueKey)
        {
            int index = issueKey.LastIndexOf('-');
            return int.Parse(issueKey.Substring(index + 1));
        }
    }
}
