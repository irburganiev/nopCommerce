using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.AutumnDiscount.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.AutumnDiscount.Controllers
{
    [Area(AreaNames.Admin)]
    public class WidgetsAutumnDiscountController : BasePluginController
    {
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public WidgetsAutumnDiscountController(IStoreContext storeContext,
            IPermissionService permissionService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService)
        {
            _storeContext = storeContext;
            _permissionService = permissionService;
            _settingService = settingService;
            _localizationService = localizationService;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var autumnDiscountSettings = _settingService.LoadSetting<AutumnDiscountSettings>(storeScope);
            var model = new ConfigurationModel
            {
                Text = autumnDiscountSettings.Text,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.Text_OverrideForStore =
                    _settingService.SettingExists(autumnDiscountSettings, x => x.Text, storeScope);
            }

            return View("~/Plugins/Widgets.AutumnDiscount/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var autumnDiscountSettings = _settingService.LoadSetting<AutumnDiscountSettings>(storeScope);

            autumnDiscountSettings.Text = model.Text;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(autumnDiscountSettings, x => x.Text,
                model.Text_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }
    }
}