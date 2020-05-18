using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Skoohii.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Skoohii.Controllers
{
    public class SkoohiiAwsS3Controller : BasePluginController
    {
        #region Fields

        private readonly SkoohiiAwsS3Settings _skoohiiAwsS3Settings;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public SkoohiiAwsS3Controller(SkoohiiAwsS3Settings skoohiiAwsS3Settings,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _skoohiiAwsS3Settings = skoohiiAwsS3Settings;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                AccessKeyId = _skoohiiAwsS3Settings.AccessKeyId,
                Bucket = _skoohiiAwsS3Settings.Bucket,
                Region= _skoohiiAwsS3Settings.Region,
                SecretAccessKey = _skoohiiAwsS3Settings.SecretAccessKey
            };

            return View("~/Plugins/Nop.Plugin.Misc.Skoohii/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _skoohiiAwsS3Settings.AccessKeyId = model.AccessKeyId;
            _skoohiiAwsS3Settings.Bucket = model.Bucket;
            _skoohiiAwsS3Settings.Region = model.Region;
            _skoohiiAwsS3Settings.SecretAccessKey = model.SecretAccessKey;

            _settingService.SaveSetting(_skoohiiAwsS3Settings);

             _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
        #endregion
    }
}