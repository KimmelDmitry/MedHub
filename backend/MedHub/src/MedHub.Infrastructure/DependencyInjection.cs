using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Asp.Versioning;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Caching;
using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Email;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Users;
using MedHub.Infrastructure.Authentication;
using MedHub.Infrastructure.Authorization;
using MedHub.Infrastructure.Caching;
using MedHub.Infrastructure.Clock;
using MedHub.Infrastructure.Data;
using MedHub.Infrastructure.Email;
using MedHub.Infrastructure.Repositories;
using Dapper;
using MedHub.Application.Abstractions.Media;
using MedHub.Application.Abstractions.Storage;
using MedHub.Application.Media.Options;
using MedHub.Application.Media.Services;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;
using MedHub.Infrastructure.BackgroundJobs.MediaProcessing;
using MedHub.Infrastructure.BackgroundJobs.Outbox;
using MedHub.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Quartz;
using AuthenticationOptions = MedHub.Infrastructure.Authentication.AuthenticationOptions;
using AuthenticationService = MedHub.Infrastructure.Authentication.AuthenticationService;
using IAuthenticationService = MedHub.Application.Abstractions.Authentication.IAuthenticationService;

namespace MedHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IEmailService, EmailService>();

        AddPersistence(services, configuration);

        AddCaching(services, configuration);

        AddAuthentication(services, configuration);

        AddFileStorage(services, configuration);

        AddAuthorization(services);

        AddHealthChecks(services, configuration);

        AddApiVersioning(services);

        AddBackgroundJobs(services, configuration);

        AddVideoProcessing(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") ??
                               throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<ICheckpointRepository, CheckpointRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<ISqlConnectionFactory>(_ =>
            new SqlConnectionFactory(connectionString));

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));

        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

        services.AddTransient<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<IAuthenticationService, AuthenticationService>((serviceProvider, httpClient) =>
            {
                var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keycloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<IJwtService, JwtService>((serviceProvider, httpClient) =>
        {
            var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

            httpClient.BaseAddress = new Uri(keycloakOptions.TokenUrl);
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddScoped<AuthorizationService>();

        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Cache") ??
                               throw new ArgumentNullException(nameof(configuration));

        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

        services.AddSingleton<ICacheService, CacheService>();
    }

    private static void AddFileStorage(IServiceCollection services, IConfiguration configuration)
    {
        // 1. Регистрация настроек
        services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
    
        // 2. Регистрация клиента Amazon S3
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<FileStorageOptions>>().Value;
    
            var config = new AmazonS3Config
            {
                ServiceURL = options.Endpoint,
                ForcePathStyle = true, 
            
                UseHttp = !options.UseHttps 
            };

            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        });

        // 3. Регистрация провайдера
        services.AddScoped<IVideoStorageProvider, S3VideoStorageProvider>();
    }

    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("Database")!,
                name: "postgres",
                failureStatus: HealthStatus.Degraded)
            .AddRedis(
                configuration.GetConnectionString("Cache")!,
                name: "redis",
                failureStatus: HealthStatus.Degraded);

        
        var keycloakBaseUrl = configuration["Keycloak:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(keycloakBaseUrl))
        {
            healthChecksBuilder.AddUrlGroup(
                new Uri(keycloakBaseUrl),
                HttpMethod.Get,
                name: "keycloak",
                failureStatus: HealthStatus.Degraded);
        }
    }

    private static void AddApiVersioning(IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));

        services.AddQuartz();

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<ProcessOutboxMessagesJobSetup>();
    }
    
    private static void AddVideoProcessing(IServiceCollection services, IConfiguration configuration)
    {
        //  Options
        services.Configure<VideoProcessingOptions>(
            configuration.GetSection(VideoProcessingOptions.SectionName));

        //  Domain services
        services.AddScoped<IVideoProbeAnalyzer, FfprobeVideoAnalyzer>();
        services.AddScoped<IVideoTranscoder, FfmpegVideoTranscoder>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();

        //  Quartz Job
        services.ConfigureOptions<VideoProcessingJobSetup>();
    }
}
