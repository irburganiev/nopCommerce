using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.AutumnDiscount.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AutumnDiscount.AltText")]
        public string Text { get; set; }
        public bool Text_OverrideForStore { get; set; }
    }
}