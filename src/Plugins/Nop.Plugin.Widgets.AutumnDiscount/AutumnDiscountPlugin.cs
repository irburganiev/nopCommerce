using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.AutumnDiscount
{
    /// <summary>
    /// Plugin
    /// </summary>
    public class AutumnDiscountPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public AutumnDiscountPlugin(ISettingService settingService,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _webHelper = webHelper;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/WidgetsAutumnDiscount/Configure";
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> {PublicWidgetZones.HomePageTop};
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsAutumnDiscount";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new AutumnDiscountSettings
            {
                Text = "При заказе до 1 декабря скидка на доставку 50%"
            };
            _settingService.SaveSetting(settings);
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<AutumnDiscountSettings>();
            base.Uninstall();
        }
    }
}