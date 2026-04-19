using System.Data;

namespace MedHub.Application.Abstractions.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}