using System;
using System.Text.RegularExpressions;
using System.Web;
using CounterSoft.Gemini.Commons.Entity;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.Gemini
{
    /// <summary>
    /// Describes a Gemini issue.
    /// </summary>
    [Serializable]
    public sealed class GeminiIssue : Issue
    {
        private static readonly Regex HtmlRemoverRegex = new Regex(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiIssue"/> class.
        /// </summary>
        /// <param name="issue">The issue.</param>
        internal GeminiIssue(IssueEN issue)
            : base(issue.IssueKey, issue.IssueStatusDesc, issue.IssueSummary, issue.IssueLongDesc, issue.FixedInVersionNumber)
        {
            this.IsClosed = issue.ClosedDate != default(DateTime);
            this.IssueDescription = HttpUtility.HtmlDecode(HtmlRemoverRegex.Replace(issue.IssueLongDesc ?? string.Empty, ""));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the issue is closed.
        /// </summary>
        public bool IsClosed { get; private set; }
    }
}
