using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulations
{
    sealed class Connection
    {
        private NpgsqlConnection _connection;

        internal Connection()
        {
            _connection = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
            _connection.Open();
        }

        ~Connection()
        {
            _connection.Close();
        }

        internal void ExecuteQuery(string query, out NpgsqlDataReader answer)
        {
            answer = null;

            try
            {
                var command = new NpgsqlCommand();
                command.Connection = _connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = query;
                answer = command.ExecuteReader();
                command.Dispose();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now}] Ошибка выполнения запроса: {query}\n{e.Message}");
            }
        }

        internal void ExecuteQuery(string query)
        {
            try
            {
                var command = new NpgsqlCommand();
                command.Connection = _connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = query;
                command.ExecuteReader();
                command.Dispose();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now}] Ошибка выполнения запроса: {query}\n{e.Message}");
            }
        }

        internal void ExecuteQueryScalar(string query, out object answer)
        {
            answer = null;

            try
            {
                var command = new NpgsqlCommand();
                command.Connection = _connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = query;
                answer = command.ExecuteScalar();
                command.Dispose();

            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now}] Ошибка выполнения запроса: {query}\n{e.Message}");
            }
        }
    }
}
