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
        List<UserDayProgram> DayStatus(long userId, string lvl);
        string DayResult(long userId, int pulls);
        void CreateLevelProgram();
        IEnumerable<ForUserReport> UserReport(long userId, string lvl);
        void DeleteUserProgram(long userId);
    }
    public class UserRepository : IUser
    {
        public string? ConnString { get; set; }
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

            string lvl = conn.ExecuteScalar<string>("SELECT users.level FROM  pulls.users  WHERE users.user_id = @user_id;", new { @user_id = userId });
            int count = conn.ExecuteScalar<int>("SELECT count(*) FROM  pulls.users  WHERE users.user_id = @user_id;", new { @user_id = userId });

            conn.Close();
            return (lvl, count);
        }

        public User GetUsersLevel(long userId)
        {
            // для теста
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string query = @"SELECT users.level FROM pulls.users  WHERE users.user_id = @users.user_id;";
            var lvl = conn.QueryFirstOrDefault<User>(query, new { user_id = userId });
            return lvl;
        }

        public void CreateUser(User user)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = @"INSERT INTO pulls.users (user_id, name) VALUES (@user_id, @name)";
            conn.Execute(sqlQuery, new { @user_id = user.IdUser, @name = user.Name });
            conn.Close();
        }

        public void UpdateUser(string lvl, long userId)
        {
            //cnn.Execute("update Table set val = @val where Id = @id", new { val, id = 1 });
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = @"UPDATE pulls.users SET level = '" + lvl + "' WHERE @users.user_id = @user_id;";
            conn.Execute(sqlQuery, new { lvl, @user_id = userId });
            conn.Close();
        }

        public void CreateTrainingProgram(string lvl, long userId)
        {
            ConnString = DBConnection.ConnectionString();
            var result = CreateProgram.CreateUserProgram(lvl, userId);
            using var conn = new NpgsqlConnection(ConnString);
            for (int i = 0; i < result.Count; i++)
            {
                DayResult res = result[i];
                var sqlQuery = @"INSERT INTO pulls.day_result (user_id, week, date, pulls) VALUES (@user_id, @week, @date, @pulls)";
                conn.Execute(sqlQuery, new { @user_id = res.Id, @week = res.Week, @date = res.Date, @pulls = 0 });
            }
            conn.Close();
        }

        public string DayResult(long userId, int pulls)
        {
            var date = DateTime.Now;
            string checkResult = "";
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string sqlQuery;
            sqlQuery = @"Select sum(pulls) From pulls.lvl_user_program WHERE " +
                              "week = (Select week From pulls.day_result WHERE date = CAST(@date as Date) and user_id = @user_id) " +
                              "and level = (Select level From pulls.users Where user_id = @user_id)::text ;";
            int sumPullsFromProgram = conn.ExecuteScalar<int>(sqlQuery, new { @user_id = userId, @date = date });

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

            sqlQuery = @"UPDATE pulls.day_result Set pulls = @pulls WHERE day_result.user_id = @user_id and day_result.date = CAST(@date as Date);";
            conn.Execute(sqlQuery, new { @pulls = pulls, @user_id = userId, @date = date });

            UserDayProgram.DayReport = false;
            conn.Close();

            return checkResult;
        }

        public void CreateLevelProgram() // Функция администратора
        {
            ConnString = DBConnection.ConnectionString();
            var result = CreateProgram.CreareProgramLevel();
            using var conn = new NpgsqlConnection(ConnString);

            conn.Execute("DELETE From pulls.lvl_user_program");

            for (int i = 0; i < result.Count; i++)
            {
                LevelProgram res = result[i];
                var sqlQuery = @"INSERT INTO pulls.lvl_user_program (level, week, approach, pulls) VALUES (@level, @week, @approach, @pulls)";
                conn.Execute(sqlQuery, new { @level = res.Level, @week = res.Week, @approach = res.Approach, @pulls = res.Pulls });
            }
            conn.Close();
        }

        public List<UserDayProgram> DayStatus(long userId, string lvl)
        {
            ConnString = DBConnection.ConnectionString();
            var date = DateTime.Now;
            using var conn = new NpgsqlConnection(ConnString);

            string sqlQuery = "SELECT a.approach , a.pulls" +
                  " FROM  pulls.lvl_user_program a LEFT JOIN pulls.day_result b ON a.week = b.week " +
                  " WHERE a.level = @level::text  and b.date = CAST(@date as Date) and user_id = @user_id;";
            var dayProgram = conn.Query<UserDayProgram>(sqlQuery, new { @user_id = userId, @date = date, @level = lvl });

            List<UserDayProgram> userDayProgram = new List<UserDayProgram>();

            foreach (var item in dayProgram)
            {
                userDayProgram.Add(new UserDayProgram(item.Approach, item.Pulls));
            }
            conn.Close();
            return userDayProgram;
        }

        public IEnumerable<ForUserReport> UserReport(long userId, string lvl)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            {
                var sqlQuery = @"Select plan.*, fact.*, plan.week as Id,fact.week as Id, plan.pullsplan,  fact.pullsfact " +
                                "From " +
                                "(Select week, sum(pulls) * 7 as pullsplan From pulls.lvl_user_program " +
                               @"where level = @level::text " +
                                "Group by week Order by week) as plan " +
                                "Full Join " +
                                "(Select week, sum(pulls) as pullsfact " +
                                "From pulls.day_result Where user_id = @user_id " +
                                "Group by week Order by week) as fact " +
                                "on plan.week = fact.week";

                var lookup = new Dictionary<int, PlanPulls>();
                _ = conn.Query<PlanPulls, FactPulls, PlanPulls>(sqlQuery, (p, f) => {

                    PlanPulls planPulls;
                    if (!lookup.TryGetValue(p.Week, out planPulls))
                    {
                        lookup.Add(p.Week, planPulls = p);
                    }
 
                    if (planPulls.Facts == null)
                        planPulls.Facts = new List<FactPulls>();
                    planPulls.Facts.Add(f);

                    return planPulls;
                    }, new { @user_id = userId, @level = lvl }, splitOn: "Id"
                 ).AsQueryable();
                var resultList = lookup.Values;

                List <ForUserReport> report = new List<ForUserReport>();

                foreach (var plan in resultList)
                {
                    foreach (var fact in plan.Facts)
                    {
                        report.Add(new ForUserReport(plan.Week, plan.PullsPlan, fact.PullsFact));
                    }
                }
                conn.Close();
                return report;
            }
        }

        public void DeleteUserProgram(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            conn.Execute(@"DELETE From pulls.day_result Where day_result.user_id = @user_id;", new { @user_id = userId });
            conn.Execute(@"UPDATE pulls.users SET level = null Where users.user_id = @user_id;", new { @user_id = userId });
            conn.Close();
        }
    }
}
