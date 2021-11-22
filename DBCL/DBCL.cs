using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBCLibrary
{
    public class DBC
    {
        private NpgsqlConnection _connection;

        public DBC()
        {
            _connection = new NpgsqlConnection(@"server=localhost;Port=5433;User Id=postgres;Password=123;Database=LogisticsNetworkPrototype");
            _connection.Open();
        }

        ~DBC()
        {
            _connection.Close();
        }

        public void ExecuteQuery(string query, out NpgsqlDataReader answer)
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
                Console.WriteLine($"[{DateTime.Now}] Request execution error: {query}\n{e.Message}");
            }
        }

        public void ExecuteQuery(string query)
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
                Console.WriteLine($"[{DateTime.Now}] Request execution error: {query}\n{e.Message}");
            }
        }

        public void ExecuteQueryScalar(string query, out object answer)
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
                Console.WriteLine($"[{DateTime.Now}] Request execution error: {query}\n{e.Message}");
            }
        }

    }
}
