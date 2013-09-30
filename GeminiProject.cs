using System;
using CounterSoft.Gemini.Commons.Entity;
using Inedo.BuildMaster.Extensibility.Providers.IssueTracking;

namespace Inedo.BuildMasterExtensions.Gemini
{
    /// <summary>
    /// Describes a Gemini project.
    /// </summary>
    [Serializable]
    public sealed class GeminiProject : CategoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiProject"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        internal GeminiProject(ProjectEN project)
            : base(project.ProjectID.ToString(), project.ProjectName, null)
        {
        }
    }
}
