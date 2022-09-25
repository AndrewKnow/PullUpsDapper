
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;


namespace PullUpsDapper
{
    public interface IUser
    {
        List<User> GetUsers();
        void CreateUser(User user);
        int GetUsersId(long userId);

    }
    public class UserRepository : IUser
    {
        private static readonly string _host = "localhost";
        private static readonly string _user = "postgres";
        private static readonly string _dbName = "PullUps";
        private static string _password = "";
        private static readonly string _port = "5432";
        public static string ConnString { get; set; }
        public static void File()
        {
            _password = Password.DB();
            ConnString = string.Format
            (
              "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
              _host,
              _user,
              _dbName,
              _port,
              _password);
        }
        public List<User> GetUsers()
        {
            File();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                return conn.Query<User>(@"SELECT * FROM  ""Pulls"".""Users"" ;").ToList();
            }
        }

        public int GetUsersId(long userId)
        {
            File();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                int count = conn.ExecuteScalar<int>(@"SELECT count(*) FROM  ""Pulls"".""Users""  WHERE ""Users"".""userId"" = " + userId + ";");
                return count;
                //conn.Query<User>(@"SELECT count(*) FROM  ""Pulls"".""Users""  WHERE ""Users"".""userId"" = @userId", new { userId }).FirstOrDefault();

            }
        }
        public void CreateUser(User user)
        {
            File();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                var sqlQuery = @"INSERT INTO ""Pulls"".""Users""  (""userId"", ""name"") VALUES ('" + user.IdUser + "', '" + user.Name + "')";
                conn.Execute(sqlQuery, user);
            }
        }

    }
}
