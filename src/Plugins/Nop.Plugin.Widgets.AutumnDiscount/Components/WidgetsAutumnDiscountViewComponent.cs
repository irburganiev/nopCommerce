using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.AutumnDiscount.Models;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.AutumnDiscount.Components
{
    public class WidgetsAutumnDiscountViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        public WidgetsAutumnDiscountViewComponent(IStoreContext storeContext, ISettingService settingService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var autumnDiscountSettings =
                _settingService.LoadSetting<AutumnDiscountSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel
            {
                Text = autumnDiscountSettings.Text
            };
            return View("~/Plugins/Widgets.AutumnDiscount/Views/PublicInfo.cshtml", model);
        }
    }
}