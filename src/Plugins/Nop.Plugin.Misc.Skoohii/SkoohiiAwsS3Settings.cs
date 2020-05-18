using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Skoohii
{
    public class SkoohiiAwsS3Settings : ISettings
    {
        public string Region { get; set; }
        public string Bucket { get; set; }
        public string SecretAccessKey { get; set; }
        public string AccessKeyId { get; set; }
    }
}