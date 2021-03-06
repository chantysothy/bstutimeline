﻿using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Entity;
using site.Data;
using site.Data.Abscract;
using site.Data.Repositories;
using site.Data.Services;
using site.Options;

namespace site
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("privatesettings.json");

            builder.AddEnvironmentVariables();
            Configuration = builder.Build()
                .ReloadOnChanged("appsettings.json")
                .ReloadOnChanged("privatesettings.json");
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var connectionString = Configuration["db:bstu-mssql"];

            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IDataContext>(provider => provider.GetService<DataContext>());
            services.AddScoped<INewsRepository, NewsRepository>();
            services.AddScoped<IFeedNewsService, FeedNewsService>();
            services.AddScoped<IUpcomingNewsService, UpcomingNewsService>();

            services.Configure<FileSystem>(options =>
            {
                options.ThumbsPath = Configuration["fs:thumbs-path"];
                options.ThumbsFilename = Configuration["fs:thumbs-filename"];
            });

            if (Configuration["env"] == "dev")
            {
                //services.AddSingleton<IImageRepository, HttpImageRepository>();
                services.AddSingleton<IImageRepository, DirectImageRepository>();
            }
            else
            {
                services.AddSingleton<IImageRepository, FileImageRepository>();
            }

            services.AddSingleton<IImageCache, ImageCache>();
            services.AddSingleton<IImageService, ImageService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIISPlatformHandler();

            app.UseDefaultFiles(new Microsoft.AspNet.StaticFiles.DefaultFilesOptions { DefaultFileNames = new[] { "index.html" } });
            app.UseStaticFiles();
            
            app.UseMvc();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
