
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
        List<UserDayProgram> DayStatus(long userId);
        string DayResult(long userId, int pulls);
    }
    public class UserRepository : IUser
    {
        
        public string ConnString { get; set; }
        public List<User> GetUsers()
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            return conn.Query<User>(@"SELECT * FROM  ""Pulls"".""Users"" ;").ToList();
        }

        public (string lvl, int count, bool program) GetUsersId(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string lvl = conn.ExecuteScalar<string>(@"SELECT ""Users"".""level"" FROM  ""Pulls"".""Users""  WHERE ""Users"".""userId"" = " + userId + ";");
            int count = conn.ExecuteScalar<int>(@"SELECT count(*) FROM  ""Pulls"".""Users""  WHERE ""Users"".""userId"" = " + userId + ";");
            bool program = conn.ExecuteScalar<bool>(@"SELECT count(*) FROM  ""Pulls"".""UserProgram""  WHERE ""UserProgram"".""userId"" = " + userId + ";");
            conn.Close();
            return (lvl, count, program);
        }
        public void CreateUser(User user)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = @"INSERT INTO ""Pulls"".""Users""  (""userId"", ""name"") VALUES ('" + user.IdUser + "', '" + user.Name + "')";
            conn.Execute(sqlQuery);
            conn.Close();
        }
        public void UpdateUser(string lvl, long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = @"UPDATE ""Pulls"".""Users""  SET ""level"" = '" + lvl + @"' WHERE ""Users"".""userId"" = " + userId + ";";
            conn.Execute(sqlQuery);
            conn.Close();
        }
        public void CreateTrainingProgram(string lvl, long userId)
        {
            ConnString = DBConnection.ConnectionString();
            var list = CreateProgram.CreateLvlProgram(lvl, userId);

            for (int i = 0; i < list.Count; i++)
            {
                using var conn = new NpgsqlConnection(ConnString);
                TrainingProgram program = list[i];
                var sqlQuery = @"INSERT INTO ""Pulls"".""UserProgram"" (""userId"", ""week"", ""approach"", ""pulls"", ""date"")  VALUES ('"
                    + program.Id + "', '"
                    + program.Week + "', '"
                    + program.Approach + "', '"
                    + program.Pulls + "', '"
                    + program.Date
                    + "')";
                conn.Execute(sqlQuery);
                conn.Close();
            }
        }
        public List<UserDayProgram> DayStatus(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            var date = DateTime.Now;
            using var conn = new NpgsqlConnection(ConnString);
            string sqlQuery = @"SELECT ""UserProgram"".""date"", ""UserProgram"".""approach"" , 
                            ""UserProgram"".""pulls"" FROM  ""Pulls"".""UserProgram""  WHERE ""UserProgram"".""userId"" = " + userId +
                        @" and ""UserProgram"".""date"" = CAST('" + date + "' as Date);";

            var dayProgram = conn.Query<UserDayProgram>(sqlQuery);
            List<UserDayProgram> userDayProgram = new List<UserDayProgram>();

            foreach (var item in dayProgram)
            {
                userDayProgram.Add(new UserDayProgram(item.Date, item.Approach, item.Pulls));
            }
            conn.Close();
            return userDayProgram;
        }
        public string DayResult(long userId, int pulls)
        {
            var date = DateTime.Now;
            string checkResult;
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            int check = conn.ExecuteScalar<int>(@"SELECT count(*) FROM   ""Pulls"".""DayResult""   WHERE ""DayResult"".""userId"" = " + userId + " and date = '" + date + "';");

            int sumPullsFromProgram = conn.ExecuteScalar<int>(@"SELECT sum(""pulls"") FROM   ""Pulls"".""UserProgram""   WHERE ""UserProgram"".""date"" = '" + date + 
                @"' and ""UserProgram"".""userId""  = '" + userId + "';");

            if (pulls < sumPullsFromProgram)
            {
                checkResult = "не доделал";
            }    
            else if (pulls > sumPullsFromProgram)
            {
                checkResult = "перевыполнил";
            }
            else
            {
                checkResult = "выполнил";
            }

            if (check == 0)
            {
                var sqlQuery = @"INSERT INTO ""Pulls"".""DayResult""  (""userId"", ""date"", ""pulls"") VALUES ('" + userId + "', '" + date + "', '" + pulls + "')";
                conn.Execute(sqlQuery);
            }
            else
            {
                var sqlQuery = @"UPDATE ""Pulls"".""DayResult"" Set ""pulls"" = '" + pulls + @"' WHERE ""DayResult"".""userId"" = " + userId + " and date = '" + date + "';";
                conn.Execute(sqlQuery);
            }
            UserDayProgram.DayReport = false;
            conn.Close();

            return checkResult;
        }
    }
}
