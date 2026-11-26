using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Cqrs;
using WebAPI.Common.ExternalServices;
using WebAPI.Database;
using WebAPI.Features.JsonPlaceholder;

namespace WebAPI;

internal static class DependencyInjection
{
    /// <summary>Every feature: handlers, validators and decorators (by scanning), plus each module's own services.</summary>
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);



        /*
         Scrutor
        مكتبة صغيرة تضيف ميزتين لنظام
        الـ Dependency Injection المدمج في .NET.
        1. Scanning (التسجيل التلقائي) :
             بدلاً من تسجيل كل خدمة يدوياً، يمكننا مسح الأسمبليز للعثور على الخدمات تلقائياً.
         */

        services.Scan(scan => scan.FromAssemblyOf<SchoolDbContext>()
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(IQueryHandler<,>))
                .Where(type => !type.IsGenericTypeDefinition), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)), publicOnly: false)
            .As(type => type.GetInterfaces().Where(service =>
                service.IsGenericType && service.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .WithScopedLifetime());

        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.Command<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationDecorator.Query<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.Command<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.Query<,>));

        /*
        2. Decorate (التغليف)

        هذه الميزة تسمح لك بأن تغلّف كل هاندلر بطبقه اضافيه لوغ او تحقق قبل او بعده ان يصل التنفيذ له

          لـ MediatR
        عادةً هو الذي يوفّر
        الـ pipeline for validation & logging.


        LoggingDecorator
                    └─ ValidationDecorator
                                └─ CreateCourseCommandHandler
         */

        // External services: one shared timeout for all clients.
        services.AddOptions<ExternalServicesOptions>().BindConfiguration(ExternalServicesOptions.SectionName);

        // Modules with their own services (HTTP clients, options, ...).
        services.AddJsonPlaceholderModule();

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services, string connectionString, bool enableSensitiveDataLogging = false)
    {
        services.AddDbContext<SchoolDbContext>(options => options
            .UseSqlite(connectionString)
            .EnableSensitiveDataLogging(enableSensitiveDataLogging));
        services.AddHealthChecks().AddDbContextCheck<SchoolDbContext>("database",
            customTestQuery: async (db, cancellationToken) =>
            {
                await db.Courses.AnyAsync(cancellationToken);
                return true;
            });

        return services;
    }

    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
