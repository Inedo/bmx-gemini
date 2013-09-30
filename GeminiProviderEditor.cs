using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Gemini
{
    /// <summary>
    /// Custom editor for the Gemini issue tracker provider.
    /// </summary>
    internal sealed class GeminiProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtUserName;
        private ValidatingTextBox txtBaseUrl;
        private PasswordTextBox txtPassword;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiProviderEditor"/> class.
        /// </summary>
        public GeminiProviderEditor()
        {
        }

        public override void BindToForm(ProviderBase extension)
        {
            this.EnsureChildControls();

            var provider = (GeminiProvider)extension;
            this.txtUserName.Text = provider.UserName;
            this.txtPassword.Text = provider.Password;
            this.txtBaseUrl.Text = provider.BaseUrl;
        }

        public override ProviderBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new GeminiProvider
            {
                UserName = this.txtUserName.Text,
                Password = this.txtPassword.Text,
                BaseUrl = this.txtBaseUrl.Text
            };
        }

        protected override void CreateChildControls()
        {
            txtUserName = new ValidatingTextBox { Width = 300, Required = true };
            txtBaseUrl = new ValidatingTextBox { Width = 300, Required = true };
            txtPassword = new PasswordTextBox { Width = 270 };

            this.Controls.Add(
                new FormFieldGroup("Gemini Server URL",
                    "The URL of the Gemini server, for example: http://gemini:8080",
                    false,
                    new StandardFormField(
                        "Server URL:",
                        txtBaseUrl)
                    ),
                new FormFieldGroup("Authentication",
                    "Provide a username and password to connect to the Gemini server.",
                    false,
                    new StandardFormField(
                        "User Name:",
                        txtUserName),
                    new StandardFormField(
                        "Password:",
                        txtPassword)
                    )
            );
        }
    }
}
