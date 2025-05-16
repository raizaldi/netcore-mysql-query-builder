

namespace Latihan_dotnet.Data;

using Dapper;
using System.Linq.Expressions;
using System.Reflection;

public class QueryBuilder<T>
{

    private readonly Koneksi _koneksi;

    private readonly string? _tableName;
    private readonly string? _alias;
    private List<string> _selects = new();
    private List<string> _whereClauses = new();
    private Dictionary<string, object> _insertValues = new();
    private Dictionary<string, object> _updateValues = new();
    private List<string> _joins = new();
    private List<string> _groupBys = new();
    private List<string> _orderBys = new();
    private int? _limit = null;
    private int? _offset = null;
    private DynamicParameters _parameters = new();
    private int _paramCounter = 0;

    public QueryBuilder(Koneksi koneksi, string alias = null)
    {
        _koneksi = koneksi;
        _tableName = typeof(T).Name;
        _alias = alias;
    }

    private string GenerateParam(string name) => $"{name}_{_paramCounter++}";

    public QueryBuilder<T> Select<TSelect>(Expression<Func<T, TSelect>> selector)
    {
        if (selector.Body is NewExpression newExpr)
        {
            foreach (var arg in newExpr.Members)
            {
                _selects.Add($"{_alias}.{arg.Name}");
            }
        }
        return this;
    }

    public QueryBuilder<T> InnerJoin<TJoin>(string alias, string onClause)
    {
        var joinTable = typeof(TJoin).Name;
        _joins.Add($"INNER JOIN {joinTable} {alias} ON {onClause}");
        return this;
    }

    public QueryBuilder<T> LeftJoin<TJoin>(string alias, string onClause)
    {
        var joinTable = typeof(TJoin).Name;
        _joins.Add($"LEFT JOIN {joinTable} {alias} ON {onClause}");
        return this;
    }

    public QueryBuilder<T> GroupBy(params string[] columns)
    {
        _groupBys.AddRange(columns.Select(c => $"{_alias}.{c}"));
        return this;
    }

    public QueryBuilder<T> OrderBy(string column, bool descending = false)
    {
        _orderBys.Add($"{_alias}.{column} {(descending ? "DESC" : "ASC")}");
        return this;
    }


    public QueryBuilder<T> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryBuilder<T> Offset(int offset)
    {
        _offset = offset;
        return this;
    }


    public QueryBuilder<T> Insert(T obj)
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            var val = prop.GetValue(obj);
            if (val != null && !IsKey(prop))
            {
                _insertValues[prop.Name] = val;
                _parameters.Add(prop.Name, val);
            }
        }
        return this;
    }

    public QueryBuilder<T> Update(T obj)
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            var val = prop.GetValue(obj);
            if (val != null && !IsKey(prop))
            {
                _updateValues[prop.Name] = val;
                _parameters.Add(prop.Name, val);
            }
        }
        return this;
    }

    public QueryBuilder<T> Where(string column, string op, object value)
    {
        var paramName = GenerateParam(column);
        _whereClauses.Add($"{_alias}.{column} {op} @{paramName}");
        _parameters.Add(paramName, value);
        return this;
    }

    public async Task<IEnumerable<T>> BuildSelect()
    {
        var cols = _selects.Count > 0 ? string.Join(", ", _selects) : $"{_alias}.*";
        var sql = $"SELECT {cols} FROM {_tableName} {_alias}";
        if (_whereClauses.Count > 0)
            sql += " WHERE " + string.Join(" AND ", _whereClauses);

        if (_groupBys.Count > 0)
            sql += " GROUP BY " + string.Join(", ", _groupBys);

        if (_orderBys.Count > 0)
            sql += " ORDER BY " + string.Join(", ", _orderBys);

        if (_limit.HasValue)
            sql += $" LIMIT {_limit.Value}";

        if (_offset.HasValue)
            sql += $" OFFSET {_offset.Value}";

        var result = await _koneksi.SqlDynamicQuery<T>(sql, _parameters);
        return result;
    }

    public (string query, DynamicParameters parameters) BuildInsert()
    {
        var columns = string.Join(", ", _insertValues.Keys);
        var values = string.Join(", ", _insertValues.Keys.Select(k => "@" + k));
        var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";
        return (sql, _parameters);
    }

    public (string query, DynamicParameters parameters) BuildUpdate()
    {
        var setClause = string.Join(", ", _updateValues.Keys.Select(k => $"{k} = @{k}"));
        var sql = $"UPDATE {_tableName} SET {setClause}";
        if (_whereClauses.Count > 0)
            sql += " WHERE " + string.Join(" AND ", _whereClauses);
        return (sql, _parameters);
    }

    public (string query, DynamicParameters parameters) BuildDelete()
    {
        var sql = $"DELETE FROM {_tableName}";
        if (_whereClauses.Count > 0)
            sql += " WHERE " + string.Join(" AND ", _whereClauses);
        return (sql, _parameters);
    }

    private bool IsKey(PropertyInfo prop)
    {
        return prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase);
    }
}
