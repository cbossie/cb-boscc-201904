using Amazon.DynamoDBv2;
using Amazon.Polly;
using Amazon.S3;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using BostonCodeCampModels.Transcribe;
using BostonCodeCampServices.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SiteDynamoLambda.Model;
using System;
using System.Threading.Tasks;

namespace SiteDynamoLambda
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
            // Interface to dynamodb    
            services.AddTransient<ITranscribeDataService, TranscribeDataService>();
            services.AddTransient<IFileService, FileService>();

            //AWS Services
            var awsOptions = Configuration.GetAWSOptions();            
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonPolly>();
            services.AddAWSService<IAmazonS3>();

           var genConfig = new GeneralConfig();
            Configuration.Bind("GeneralConfig", genConfig);
            services.AddSingleton<IGeneralConfig>(genConfig);


            //AWS XRay Config
            AWSSDKHandler.RegisterXRayForAllServices();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add Authentication
            var oidcCfg = new OidcConfig();
            
            Configuration.Bind("CognitoOIDC", oidcCfg);
            var baseUrl = oidcCfg.BaseUrl;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
               
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.ResponseType = oidcCfg.ResponseType;
                options.MetadataAddress = oidcCfg.MetadataAddress;
                options.ClientId = oidcCfg.ClientId;
                options.ClientSecret = oidcCfg.ClientSecret;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"
                    
                };


                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = oidcCfg.LogoutUrl + oidcCfg.ClientId;
                        logoutUri += $"&logout_uri={Uri.EscapeDataString(baseUrl)}";
                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                //AWS XRay Config
                AWSSDKHandler.RegisterXRayForAllServices();
                app.UseXRay("boscc34App");
            }



            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvcWithDefaultRoute();
        }
    }
}
