using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Project_64140855.Startup))]
namespace Project_64140855
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
