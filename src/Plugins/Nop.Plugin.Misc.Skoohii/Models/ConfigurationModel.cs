using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Skoohii.Models
{
    /// <summary>
    /// Represents plugin configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Nop.Plugin.Misc.Skoohii.Region")]
        public string Region { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.Skoohii.Bucket")]
        public string Bucket { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.Skoohii.SecretAccessKey")]
        public string SecretAccessKey { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.Skoohii.AccessKeyId")]
        public string AccessKeyId { get; set; }
    }
}