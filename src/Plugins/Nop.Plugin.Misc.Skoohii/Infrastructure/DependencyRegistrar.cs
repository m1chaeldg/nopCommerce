using Autofac;
using Autofac.Builder;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.Skoohii.Services;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.Skoohii.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterDecorator<Aws3PictureService, IPictureService>();
        }

        public int Order => 1;
    }
}
