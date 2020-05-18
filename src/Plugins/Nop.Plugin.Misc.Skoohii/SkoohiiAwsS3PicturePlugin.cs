using System.Collections.Generic;
using Nop.Core;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Skoohii
{
    /// <summary>
    /// Represents method for the authentication with Facebook account
    /// </summary>
    public class SkoohiiAwsS3PicturePlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public SkoohiiAwsS3PicturePlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FacebookAuthentication/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new SkoohiiAwsS3Settings());

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Nop.Plugin.Misc.Skoohii.Region"] = "AWS S3 Region",
                ["Nop.Plugin.Misc.Skoohii.Region.Hint"] = "Enter your AWS S3 region.",

                ["Nop.Plugin.Misc.Skoohii.Bucket"] = "AWS S3 Bucket",
                ["Nop.Plugin.Misc.Skoohii.Bucket.Hint"] = "Enter your AWS S3 bucket path.",

                ["Nop.Plugin.Misc.Skoohii.SecretAccessKey"] = "AWS S3 Secret Access Key",
                ["Nop.Plugin.Misc.Skoohii.SecretAccessKey.Hint"] = "Enter your secret access key.",

                ["Nop.Plugin.Misc.Skoohii.AccessKeyId"] = "AWS S3 Access ID",
                ["Nop.Plugin.Misc.Skoohii.AccessKeyId.Hint"] = "Enter your access key.",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SkoohiiAwsS3Settings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Nop.Plugin.Misc.Skoohii");

            base.Uninstall();
        }

        #endregion
    }
}