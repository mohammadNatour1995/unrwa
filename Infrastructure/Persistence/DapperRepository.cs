using Dapper;
using Domain.Interfaces;
using Infrastructure.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.Persistence;

public class DapperRepository<T> : IDapperRepository<T>
{
    private readonly string _connectionString;

    public DapperRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
    }

    public async Task<int> ExecuteCommand(string spName, object? parameters = null)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ExecuteCommandParamOut(string spName, DynamicParameters parameters)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(spName, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<T?> ExecuteCommandSingle(string spName, object? parameters = null)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        var dbParameters = DbParameterHelper.ToDbParameters(true, parameters);
        var data = await connection.QueryAsync<T>(spName, dbParameters, commandType: CommandType.StoredProcedure);
        return data.FirstOrDefault();
    }

    public async Task<IList<T>> ExecuteCommandList(string spName, object? parameters = null)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        var dbParameters = DbParameterHelper.ToDbParameters(true, parameters);
        var data = await connection.QueryAsync<T>(spName, dbParameters, commandType: CommandType.StoredProcedure);
        return data.ToList();
    }

    public async Task<int> ExecuteCommandListParamOut(string spName, List<DynamicParameters> parametersList)
    {
        using IDbConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var totalAffected = 0;
            foreach (var parameters in parametersList)
                totalAffected += await connection.ExecuteAsync(spName, parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

            transaction.Commit();
            return totalAffected;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
