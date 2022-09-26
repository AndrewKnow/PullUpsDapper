
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
        (string lvl, int count, bool program) GetUsersId(long userId);
        void UpdateUser(string lvl, long userId);
        void CreateTrainingProgram(string lvl, long userId);


    }
    public class UserRepository : IUser
    {
        public string ConnString { get; set; }
        public List<User> GetUsers()
        {
            ConnString = DBConnection.ConnectionString();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                return conn.Query<User>(@"SELECT * FROM  ""Pulls"".""Users"" ;").ToList();
            }
        }

        public (string lvl, int count, bool program) GetUsersId(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                string lvl = conn.ExecuteScalar<string>(@"SELECT ""Users"".""level"" FROM  ""Pulls"".""Users""  WHERE ""Users"".""userId"" = " + userId + ";");
                int count = conn.ExecuteScalar<int>(@"SELECT count(*) FROM  ""Pulls"".""Users""  WHERE ""Users"".""userId"" = " + userId + ";");
                bool program = conn.ExecuteScalar<bool>(@"SELECT count(*) FROM  ""Pulls"".""UserProgram""  WHERE ""UserProgram"".""userId"" = " + userId + ";");

                return (lvl, count, program);
            }
        }
        public void CreateUser(User user)
        {
            ConnString = DBConnection.ConnectionString();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                var sqlQuery = @"INSERT INTO ""Pulls"".""Users""  (""userId"", ""name"") VALUES ('" + user.IdUser + "', '" + user.Name + "')";
                conn.Execute(sqlQuery);
            }
        }
        public void UpdateUser(string lvl, long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using (var conn = new NpgsqlConnection(ConnString))
            {
                var sqlQuery = @"UPDATE ""Pulls"".""Users""  SET ""level"" = '" + lvl + @"' WHERE ""Users"".""userId"" = " + userId + ";"; 
                conn.Execute(sqlQuery);
            }
        }
        public void CreateTrainingProgram(string lvl, long userId)
        {
            TrainingProgram.CreateProgram.CreateLvlProgram(lvl, userId);



        //    public class MyObject
        //{
        //    public int A { get; set; }

        //    public string B { get; set; }
        //}
        //И предполагаяprocessList = List<MyObject>, что вы хотели бы сделать это

        //foreach (var item in processList)
        //    {
        //         string processQuery = "INSERT INTO PROCESS_LOGS VALUES (@A, @B)";
        //            connection.Execute(processQuery, item);
        //    }




        }
    }
}
