using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Polly;
using Amazon.S3;
using Amazon.SecretsManager;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SiteDynamoLambda
{
    public class Startup
    {

        IAmazonSecretsManager SecManager { get;  set; }
        AWSOptions AwsOpts { get; set; }

        IGeneralConfig Config { get; set; }
        OidcConfig OpenIdConnectConfig { get; set; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // Configuration
            AwsOpts = Configuration.GetAWSOptions();
            Config = new GeneralConfig();
            Configuration.Bind("GeneralConfig", Config);
            OpenIdConnectConfig = new OidcConfig();
            Configuration.Bind("CognitoOIDC", OpenIdConnectConfig);
            SecManager = new AmazonSecretsManagerClient(AwsOpts.Region);

        }

        private async Task<string> GetOidcSecretKey()
        {
            var result = await SecManager.GetSecretValueAsync(new Amazon.SecretsManager.Model.GetSecretValueRequest()
            {
                SecretId = OpenIdConnectConfig.ClientSecretKey,
                VersionStage = "AWSCURRENT"
        });

            var obj = JObject.Parse(result.SecretString);
            var secret = obj["cognito-secret"]?.ToString();

            return secret;


        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Interface to dynamodb    
            services.AddTransient<ITranscribeDataService, TranscribeDataService>();
            services.AddTransient<IFileService, FileService>();




            //AWS SDK Configuration       
            services.AddDefaultAWSOptions(AwsOpts);
            // Add SDK clients to configuration
            //DynamoDB
            services.AddAWSService<IAmazonDynamoDB>();
           
            //Polly Speech Synthesis
            services.AddAWSService<IAmazonPolly>();

            //S3 Object Storage
            services.AddAWSService<IAmazonS3>();

            //AWS SecretsManager
            services.AddSingleton(SecManager);

            services.AddSingleton<IGeneralConfig>(Config);

            // SDK Secrets Manager
            services.AddAWSService<IAmazonSecretsManager>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add Authentication
 
         

            var baseUrl = OpenIdConnectConfig.BaseUrl;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
               
            })
            .AddCookie()
            .AddOpenIdConnect(async options =>
            {
                options.ResponseType = OpenIdConnectConfig.ResponseType;
                options.MetadataAddress = OpenIdConnectConfig.MetadataAddress;
                options.ClientId = OpenIdConnectConfig.ClientId;
                options.ClientSecret = await GetOidcSecretKey();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
                    ValidateAudience = false
                    
                };


                options.Events = new OpenIdConnectEvents
                {

                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = OpenIdConnectConfig.LogoutUrl + OpenIdConnectConfig.ClientId;
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
