using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClientMvc
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = "https://localhost:15554";
                    config.ClientId = "client_id_mvc";
                    config.ClientSecret = "client_secret_mvc";
                    config.SaveTokens = true;

                    config.ResponseType = "code";
                    config.Scope.Add("OrdersAPI");
                    config.Scope.Add("offline_access");
                    config.GetClaimsFromUserInfoEndpoint = true;
                    config.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, ClaimTypes.DateOfBirth);
                });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("HasDateOfBirth", builder =>
                {
                    builder.RequireClaim(ClaimTypes.DateOfBirth);
                });
                //config.AddPolicy("OlderThan", builder =>
                //{
                //    builder.AddRequirements(new OlderThanRequirement(10));
                //});
            });
            services.AddSingleton<IAuthorizationHandler, OlderThanRequirementHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            services.AddControllersWithViews();
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name:"Default", pattern:"{controller=Site}/{action=Index}/{id?}");
            });
        }
    }

    public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;

        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            _options = options.Value;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var policyExists = await base.GetPolicyAsync(policyName);

            if (policyExists == null)
            {
                policyExists = new AuthorizationPolicyBuilder()
                    .AddRequirements(new OlderThanRequirement(10))
                    .Build();

                _options.AddPolicy(policyName, policyExists);
            }

            return policyExists;
        }
    }

    public class OlderThanRequirement : IAuthorizationRequirement
    {
        public int Years { get; }

        public OlderThanRequirement(int years)
        {
            Years = years;
        }
    }

    public class OlderThanRequirementHandler : AuthorizationHandler<OlderThanRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OlderThanRequirement requirement)
        {
            var hasClaim = context.User.HasClaim(x => x.Type == ClaimTypes.DateOfBirth);

            if (!hasClaim)
            {
                return Task.CompletedTask;
            }

            var dateOfBirth = context.User.FindFirst(x => x.Type == ClaimTypes.DateOfBirth).Value;
            var date = DateTime.Parse(dateOfBirth, new CultureInfo("ru-RU"));
            var canEnter = DateTime.Now.Year - date.Year >= 10;

            if (canEnter)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
