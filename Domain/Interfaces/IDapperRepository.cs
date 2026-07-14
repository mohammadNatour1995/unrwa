using Dapper;

namespace Domain.Interfaces;

public interface IDapperRepository<T>
{
    Task<int> ExecuteCommand(string spName, object? parameters = null);
    Task<int> ExecuteCommandParamOut(string spName, DynamicParameters parameters);
    Task<T?> ExecuteCommandSingle(string spName, object? parameters = null);
    Task<IList<T>> ExecuteCommandList(string spName, object? parameters = null);
    Task<int> ExecuteCommandListParamOut(string spName, List<DynamicParameters> parametersList);
}
