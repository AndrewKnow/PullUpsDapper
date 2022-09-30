
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
        (string lvl, int count) GetUsersId(long userId);
        void UpdateUser(string lvl, long userId);
        void CreateTrainingProgram(string lvl, long userId);
        List<UserDayProgram> DayStatus(long userId);
        string DayResult(long userId, int pulls);
        void CreateLevelProgram();

        void DeleteUserProgram(long userId);
    }
    public class UserRepository : IUser
    {
        public string ConnString { get; set; }
        public List<User> GetUsers()
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            return conn.Query<User>("SELECT * FROM  pulls.users ;").ToList();
        }

        public (string lvl, int count) GetUsersId(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string lvl = conn.ExecuteScalar<string>("SELECT users.level FROM  pulls.users  WHERE users.user_id = " + userId + ";");
            int count = conn.ExecuteScalar<int>("SELECT count(*) FROM  pulls.users  WHERE users.user_id = " + userId + ";");
            //bool program = conn.ExecuteScalar<bool>("SELECT count(*) FROM  pulls.lvl_user_program  WHERE lvl_user_program.user_id = " + userId + ";");
            conn.Close();
            return (lvl, count);
        }

        public User GetUsersLevel(long userId)
        {
            // для теста
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string query = "SELECT users.level FROM  pulls.users  WHERE users.user_id = @users.user_id;";
            var lvl = conn.QueryFirstOrDefault<User>(query, new { user_id = userId });

            return lvl;
        }



        public void CreateUser(User user)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = "INSERT INTO pulls.users  (user_id, name) VALUES ('" + user.IdUser + "', '" + user.Name + "')";
            conn.Execute(sqlQuery);
            conn.Close();
        }

        public void UpdateUser(string lvl, long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = "UPDATE pulls.users  SET level = '" + lvl + "' WHERE users.user_id = " + userId + ";";
            conn.Execute(sqlQuery);
            conn.Close();
        }

        public void CreateTrainingProgram(string lvl, long userId)
        {
            ConnString = DBConnection.ConnectionString();
            // var (result, UserProgram) = CreateProgram.CreateUserProgram(lvl, userId);
            var result = CreateProgram.CreateUserProgram(lvl, userId);
            using var conn = new NpgsqlConnection(ConnString);
            //for (int i = 0; i < UserProgram.Count; i++)
            //{  
            //    TrainingProgram program = UserProgram[i];
            //    var sqlQuery = "INSERT INTO pulls.user_program (user_id, week, approach, pulls)  VALUES ('"
            //        + program.Id + "', '"
            //        + program.Week + "', '"
            //        + program.Approach + "', '"
            //        + program.Pulls + "'"
            //        + ")";
            //    conn.Execute(sqlQuery); 
            //}
            for (int i = 0; i < result.Count; i++)
            {
                DayResult res = result[i];
                var sqlQuery = "INSERT INTO pulls.day_result (user_id, week, date, pulls)  VALUES ('"
                    + res.Id + "', '"
                    + res.Week + "', '"
                    + res.Date + "', '"
                    + 0 + "'"
                    + ")";
                conn.Execute(sqlQuery);
            }
            conn.Close();
        }

        public List<UserDayProgram> DayStatus(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            var date = DateTime.Now;
            using var conn = new NpgsqlConnection(ConnString);
            string sqlQuery = "SELECT a.approach , a.pulls" +
                              " FROM  pulls.lvl_user_program a LEFT JOIN pulls.day_result b ON a.week = b.week " +
                              " WHERE a.level = (Select level From pulls.users Where user_id = " + userId + ")::text  and b.date = CAST('" + date + "' as Date);";

            var dayProgram = conn.Query<UserDayProgram>(sqlQuery);
            List<UserDayProgram> userDayProgram = new List<UserDayProgram>();

            foreach (var item in dayProgram)
            {
                userDayProgram.Add(new UserDayProgram(item.Approach, item.Pulls));
            }
            conn.Close();
            return userDayProgram;
        }

        public string DayResult(long userId, int pulls)
        {
            var date = DateTime.Now;
            string checkResult = "";
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string sqlQuery;
            sqlQuery = "Select sum(pulls) From pulls.lvl_user_program WHERE " +
                              "week = (Select week From pulls.day_result WHERE date = CAST('" + date + "' as Date)) " +
                              "and level = (Select level From pulls.users Where user_id = " + userId + ")::text;";
            int sumPullsFromProgram = conn.ExecuteScalar<int>(sqlQuery);

            if (pulls < sumPullsFromProgram && sumPullsFromProgram > 0)
            {
                checkResult = "не доделал";
            }
            else if (pulls > sumPullsFromProgram && sumPullsFromProgram > 0)
            {
                checkResult = "перевыполнил";
            }
            else if (sumPullsFromProgram > 0)
            {
                checkResult = "выполнил";
            }
            sqlQuery = "UPDATE pulls.day_result Set pulls = '" + pulls + "' WHERE day_result.user_id = " + userId + " and date = CAST('" + date + "' as Date);";
            conn.Execute(sqlQuery);
            UserDayProgram.DayReport = false;
            conn.Close();

            return checkResult;
        }

        public void CreateLevelProgram()
        {
            ConnString = DBConnection.ConnectionString();
            var result = CreateProgram.CreareProgramLevel();
            using var conn = new NpgsqlConnection(ConnString);

            conn.Execute("DELETE From pulls.lvl_user_program");

            for (int i = 0; i < result.Count; i++)
            {
                LevelProgram res = result[i];
                var sqlQuery = "INSERT INTO pulls.lvl_user_program (level, week, approach, pulls)  VALUES ('"
                    + res.Level + "', '"
                    + res.Week + "', '"
                    + res.Approach + "', '"
                    + res.Pulls + "'"
                    + ")";
                conn.Execute(sqlQuery);
            }
            conn.Close();
        }
        public void DeleteUserProgram(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            conn.Execute("DELETE From pulls.day_result Where day_result.user_id = " + userId + ";");

            conn.Execute("UPDATE pulls.users SET level = null Where users.user_id = " + userId + ";");
            conn.Close();

        }
    }
}
