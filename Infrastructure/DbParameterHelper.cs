using Dapper;
using System.Reflection;

namespace Infrastructure.Helpers;

public static class DbParameterHelper
{
    public static DynamicParameters ToDbParameters(bool treatDefaultNumericAsNull = true, params object?[] paramObjects)
    {
        var parameters = new DynamicParameters();

        foreach (var obj in paramObjects)
        {
            if (obj == null) continue;

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(obj);

                if (value == null || value is DBNull)
                {
                    value = null;
                }
                else if (value is string s && string.IsNullOrWhiteSpace(s))
                {
                    value = null;
                }
                else if (prop.PropertyType == typeof(DateTime) && (DateTime)value == default)
                {
                    value = null;
                }
                else if (prop.PropertyType == typeof(DateTime?) && value is DateTime dt2 && dt2 == default)
                {
                    value = null;
                }
                else if (treatDefaultNumericAsNull && IsNumericType(prop.PropertyType) && Convert.ToDecimal(value) == 0)
                {
                    value = null;
                }

                parameters.Add("@" + prop.Name, value);
            }
        }

        return parameters;
    }

    private static bool IsNumericType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(byte) ||
               type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(ushort) ||
               type == typeof(int) ||
               type == typeof(uint) ||
               type == typeof(long) ||
               type == typeof(ulong) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(decimal);
    }
}
