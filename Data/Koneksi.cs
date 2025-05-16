using Dapper;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Latihan_dotnet.Data;


public class Koneksi : DbContext
{
    private readonly DbConnection _koneksi;

    public Koneksi(DbContextOptions<Koneksi> options) : base(options)
    {
        _koneksi = this.Database.GetDbConnection();
    }

    private DbTransaction? Transaction
    {
        get
        {
            var current = this.Database.CurrentTransaction;
            if (current != null)
            {
                return current.GetDbTransaction();
            }

            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<int> ExecuteSqlCommand(string commandName, DynamicParameters parameters)
    {
        return await _koneksi.ExecuteAsync(commandName, parameters, transaction: this.Transaction, commandType: CommandType.Text);
    }



    public async Task<IEnumerable<T>> SqlDynamicQuery<T>(string commandName, DynamicParameters? parameters)
    {
        parameters ??= new DynamicParameters();
        var result = await _koneksi.QueryAsync<T>(commandName, parameters, transaction: this.Transaction, commandType: CommandType.Text);
        return result;
    }
}

