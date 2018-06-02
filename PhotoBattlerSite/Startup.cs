using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoBattlerSite
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      //services.AddIdentity<ApplicationUser, IdentityRole>()
      // .AddEntityFrameworkStores<ApplicationDbContext>()
      // .AddDefaultTokenProviders();

      services.AddAuthentication().AddTwitter(twitterOptions =>
      {
        twitterOptions.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
        twitterOptions.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      app.UseStaticFiles();
    }
  }
}
