using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Serilog;

internal static class Extensions
{
    internal static void LogVersion<T>(this IServiceProvider provider)
    {
        provider
            .GetRequiredService<ILogger<T>>()
            .LogInformation("DirectoryMiner {version:l}", typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
    }



    internal static IConfigurationBuilder AddEmbeddedJsonFile(this IConfigurationBuilder builder, string name)
    {
        return builder
            .AddJsonStream(new EmbeddedFileProvider(Assembly.GetExecutingAssembly()).GetFileInfo(name).CreateReadStream())
            .AddJsonFile(name, true);
    }

    internal static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton(configuration);
    }

    internal static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddLogging(builder => builder
            .AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger(), dispose: true));
    }

    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            // Add services here
            .AddTransient<Main>();
    }
}
