using Bogus;
using MedHub.Application.Abstractions.Data;
using Dapper;

namespace MedHub.Api.Extensions;

public static class SeedDataExtensions
{
    public static void SeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var sqlConnectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        using var connection = sqlConnectionFactory.CreateConnection();

        var faker = new Faker();

        List<object> apartments = new();
        
        string sql = String.Empty;
        
        connection.Execute(sql, apartments);
    }
}
