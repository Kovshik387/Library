using System.IO;
using Library.Interfaces;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library
{
    public static class Startup
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services,
            IConfiguration configuration = null)
        {
            services.AddTransient<IDictionaryRepository, AccessDictionaryRepository>();
            services.AddTransient<IDictionaryRepository, MsSqlDictionaryRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services,
            IConfiguration configuration = null) =>
            services.AddTransient<IDictionaryService, DictionaryService>();
        
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            
            services.AddSingleton<IConfiguration>(configuration);
            
            return services;
        }
    }
}