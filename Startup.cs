using Blazorise;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StellaTheStaffe.Models;
using StellaTheStaffe.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise.Material;
using Blazorise.Icons.Material;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace StellaTheStaffe
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            BsonClassMap.RegisterClassMap<OembedData>();
            BsonClassMap.RegisterClassMap<InstagramData>(a =>
            {
                a.AutoMap();
                a.MapMember(a => a.OembedData);
            });

            services.AddSingleton(Options.Create(new MongoDbOptions(Configuration["MongoDb:UserId"] ?? throw new NullReferenceException("MongoDb user is null"), Configuration["MongoDb:Password"] ?? throw new NullReferenceException("MongoDb password is null"))));
            services.AddSingleton<IMongoClient>(a => {
                var options = a.GetRequiredService<IOptions<MongoDbOptions>>().Value;
                return new MongoClient($"mongodb+srv://{options.User}:{options.Password}@stella-the-staffe.sqozg.mongodb.net/instagramData?retryWrites=true&w=majority");
            });
            services.AddBlazorise().AddMaterialProviders().AddMaterialIcons();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton(Options.Create(new InstagramOptions(Configuration["Instagram:AccessToken"], Configuration["Instagram:ClientId"], Configuration["Instagram:ClientSecret"])));
            services.AddSingleton<PostsService>();
            services.AddSingleton<PostsContext>();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() != Environments.Development) services.AddHostedService<InstagramFetchService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

           

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.ApplicationServices.UseMaterialProviders().UseMaterialIcons();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
