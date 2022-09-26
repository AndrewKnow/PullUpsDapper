
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using System.Collections.Generic;

namespace PullUpsDapper
{
    public interface IUser
    {
        List<User> GetUsers();
        void CreateUser(User user);
        (string lvl, int count, bool program) GetUsersId(long userId);
        void UpdateUser(string lvl, long userId);
        void CreateTrainingProgram(string lvl, long userId);
        List<UserDayResult> DayStatus(long userId);
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
            ConnString = DBConnection.ConnectionString();
            var list = CreateProgram.CreateLvlProgram(lvl, userId);

            for (int i = 0; i < list.Count; i++)
            {
                using (var conn = new NpgsqlConnection(ConnString))
                {
                    TrainingProgram program = list[i];
                    var sqlQuery = @"INSERT INTO ""Pulls"".""UserProgram"" (""userId"", ""week"", ""approach"", ""pulls"", ""date"")  VALUES ('"
                        + program.Id + "', '"
                        + program.Week + "', '"
                        + program.Approach + "', '"
                        + program.Pulls + "', '"
                        + program.Date
                        + "')";
                    conn.Execute(sqlQuery);
                }
            }
        }
        public List<UserDayResult> DayStatus(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            var date = DateTime.Now;
            using (var conn = new NpgsqlConnection(ConnString))
            {
                string sqlQuery = @"SELECT ""UserProgram"".""date"", ""UserProgram"".""approach"" , 
                            ""UserProgram"".""pulls"" FROM  ""Pulls"".""UserProgram""  WHERE ""UserProgram"".""userId"" = " + userId +
                            @" and ""UserProgram"".""date"" = CAST('" + date + "' as Date);";

                var dayProgram = conn.Query<TrainingProgram>(sqlQuery);
                List<UserDayResult> userDayResult = new List<UserDayResult>();

                foreach (var item in dayProgram)
                {
                    userDayResult.Add(new UserDayResult(item.Date, item.Approach, item.Pulls));
                }
                return userDayResult;
            }
        }
    }
}
