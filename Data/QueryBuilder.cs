using Dapper;

namespace Latihan_dotnet.Data;

public class QueryBuilder
{
    private string? _table;
    private List<string> _selects = new();
    private Dictionary<string, object> _insertValues = new();
    private Dictionary<string, object> _updateValues = new();
    private List<string> _whereClauses = new();
    private List<string> _joins = new();
    private List<string> _groupBys = new();
    private List<string> _orderBys = new();
    private int? _limit = null;
    private int? _offset = null;

    private DynamicParameters _parameters = new();
    private int _paramCounter = 0;

    public QueryBuilder Table(string table)
    {
        _table = table;
        Reset();
        return this;
    }

    private void Reset()
    {
        _selects.Clear();
        _insertValues.Clear();
        _updateValues.Clear();
        _whereClauses.Clear();
        _joins.Clear();
        _groupBys.Clear();
        _orderBys.Clear();
        _limit = null;
        _offset = null;
        _parameters = new();
        _paramCounter = 0;
    }

    public QueryBuilder Select(params string[] columns)
    {
        _selects.AddRange(columns);
        return this;
    }

    public QueryBuilder Where(string column, string op, object value)
    {
        var paramName = GenerateParamName(column);
        _whereClauses.Add($"{column} {op} @{paramName}");
        _parameters.Add(paramName, value);
        return this;
    }

    public QueryBuilder And(string column, string op, object value)
    {
        var paramName = GenerateParamName(column);
        _whereClauses.Add($"AND {column} {op} @{paramName}");
        _parameters.Add(paramName, value);
        return this;
    }

    public QueryBuilder Or(string column, string op, object value)
    {
        var paramName = GenerateParamName(column);
        _whereClauses.Add($"OR {column} {op} @{paramName}");
        _parameters.Add(paramName, value);
        return this;
    }

    public QueryBuilder WhereIn(string column, IEnumerable<object> values)
    {
        var paramNames = new List<string>();
        foreach (var value in values)
        {
            var paramName = GenerateParamName(column);
            paramNames.Add("@" + paramName);
            _parameters.Add(paramName, value);
        }
        var inClause = $"{column} IN ({string.Join(", ", paramNames)})";
        _whereClauses.Add(inClause);
        return this;
    }

    public QueryBuilder Like(string column, string pattern)
    {
        var paramName = GenerateParamName(column);
        _whereClauses.Add($"{column} LIKE @{paramName}");
        _parameters.Add(paramName, pattern);
        return this;
    }

    public QueryBuilder InnerJoin(string table, string alias, string onClause)
    {
        _joins.Add($"INNER JOIN {table} {alias} ON {onClause}");
        return this;
    }

    public QueryBuilder LeftJoin(string table, string alias, string onClause)
    {
        _joins.Add($"LEFT JOIN {table} {alias} ON {onClause}");
        return this;
    }

    public QueryBuilder GroupBy(params string[] columns)
    {
        _groupBys.AddRange(columns);
        return this;
    }

    public QueryBuilder OrderBy(string column, bool descending = false)
    {
        _orderBys.Add($"{column} {(descending ? "DESC" : "ASC")}");
        return this;
    }

    public QueryBuilder Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryBuilder Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    public QueryBuilder Insert(Dictionary<string, object> values)
    {
        _insertValues = values;
        foreach (var kv in values)
            _parameters.Add(kv.Key, kv.Value);
        return this;
    }

    public QueryBuilder Update(Dictionary<string, object> values)
    {
        _updateValues = values;
        foreach (var kv in values)
            _parameters.Add(kv.Key, kv.Value);
        return this;
    }

    public (string query, DynamicParameters parameters) BuildSelect()
    {
        var columns = _selects.Count > 0 ? string.Join(", ", _selects) : "*";
        var query = $"SELECT {columns} FROM {_table}";

        if (_joins.Count > 0)
            query += " " + string.Join(" ", _joins);

        if (_whereClauses.Count > 0)
            query += " WHERE " + string.Join(" ", _whereClauses);

        if (_groupBys.Count > 0)
            query += " GROUP BY " + string.Join(", ", _groupBys);

        if (_orderBys.Count > 0)
            query += " ORDER BY " + string.Join(", ", _orderBys);

        if (_limit.HasValue)
            query += $" LIMIT {_limit.Value}";

        if (_offset.HasValue)
            query += $" OFFSET {_offset.Value}";

        return (query, _parameters);
    }

    public (string query, DynamicParameters parameters) BuildInsert()
    {
        var columns = string.Join(", ", _insertValues.Keys);
        var values = string.Join(", ", _insertValues.Keys.Select(k => "@" + k));
        var query = $"INSERT INTO {_table} ({columns}) VALUES ({values})";
        return (query, _parameters);
    }

    public (string query, DynamicParameters parameters) BuildUpdate()
    {
        var setClause = string.Join(", ", _updateValues.Keys.Select(k => $"{k} = @{k}"));
        var query = $"UPDATE {_table} SET {setClause}";
        if (_whereClauses.Count > 0)
            query += " WHERE " + string.Join(" ", _whereClauses);
        return (query, _parameters);
    }

    public (string query, DynamicParameters parameters) BuildDelete()
    {
        var query = $"DELETE FROM {_table}";
        if (_whereClauses.Count > 0)
            query += " WHERE " + string.Join(" ", _whereClauses);
        return (query, _parameters);
    }

    private string GenerateParamName(string baseName)
    {
        return $"{baseName}_{_paramCounter++}";
    }
}
