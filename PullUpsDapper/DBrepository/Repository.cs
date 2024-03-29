﻿
using Npgsql;
using Dapper;
using PullUpsDapper.Users;
using PullUpsDapper.TrainingProgram;
using Microsoft.VisualBasic;

namespace PullUpsDapper.DBrepository
{
    public interface IUser
    {
        List<User> GetUsers();
        void CreateUser(User user);
        (string lvl, int count) GetUsersId(long userId);
        void UpdateUser(string lvl, long userId);
        void CreateTrainingProgram(string lvl, long userId);
        Task<List<UserDayProgram>> DayStatus(long userId, string lvl);
        string DayResult(long userId, int pulls);
        string DayResultPlus(long userId, int pulls);
        void CreateLevelProgram();
        IEnumerable<ForUserReport> UserReport(long userId, string lvl);
        void DeleteUserProgram(long userId);
        (int fact, int plan) FactPlanToday(long userId, string lvl);
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
            conn.QueryFirstOrDefault<User>(sqlQuery, new { @user_id = user.IdUser, @name = user.Name });
            conn.Close();
        }

        public void UpdateUser(string lvl, long userId)
        {
            //cnn.Execute("update Table set val = @val where Id = @id", new { val, id = 1 });
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            var sqlQuery = @"UPDATE pulls.users SET level = '" + lvl + "' WHERE @users.user_id = @user_id;";
            conn.QueryFirstOrDefault<User>(sqlQuery, new { lvl, @user_id = userId });
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
                // conn.Execute(sqlQuery, new { @user_id = res.Id, @week = res.Week, @date = res.Date, @pulls = 0 });
                conn.QueryFirstOrDefault<DayResult>(sqlQuery, new { @user_id = res.Id, @week = res.Week, @date = res.Date, @pulls = 0 });
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

            sqlQuery = @"UPDATE pulls.day_result Set pulls = @pulls WHERE day_result.user_id = @user_id and day_result.date = CAST(@date as Date);";
            conn.QueryFirstOrDefault<DayResult>(sqlQuery, new { pulls, @user_id = userId, date });

            sqlQuery = @"Select sum(pulls) From pulls.lvl_user_program WHERE " +
                              "week = (Select week From pulls.day_result WHERE date = CAST(@date as Date) and user_id = @user_id) " +
                              "and level = (Select level From pulls.users Where user_id = @user_id)::text ;";
            int sumPullsFromProgram = conn.ExecuteScalar<int>(sqlQuery, new { @user_id = userId, @date = date });

            if (pulls < sumPullsFromProgram && sumPullsFromProgram > 0)
            {
                checkResult = $"не доделал программу за сегодня (осталось 👉🏻 {sumPullsFromProgram - pulls})";
            }
            else if (pulls > sumPullsFromProgram && sumPullsFromProgram > 0)
            {
                checkResult = $"перевыполнил программу за сегодня (сверх плана 🦾 {pulls - sumPullsFromProgram})";
            }
            else if (sumPullsFromProgram > 0)
            {
                checkResult = "выполнил программу за сегодня 💪🏻";
            }

            UserDayProgram.DayReport = false;
            conn.Close();

            return checkResult;
        }
        public string DayResultPlus(long userId, int pulls)
        {
            var date = DateTime.Now;
            string checkResult = "";
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string sqlQuery;

            // 101022.2 тестировать метод + повторения DayResultPlus

            sqlQuery = @"UPDATE pulls.day_result Set pulls = pulls + @pulls WHERE day_result.user_id = @user_id and day_result.date = CAST(@date as Date);";
            conn.QueryFirstOrDefault<DayResult>(sqlQuery, new { pulls, @user_id = userId, date });

            sqlQuery = @"Select sum(pulls) From pulls.lvl_user_program WHERE " +
                              "week = (Select week From pulls.day_result WHERE date = CAST(@date as Date) and user_id = @user_id) " +
                              "and level = (Select level From pulls.users Where user_id = @user_id)::text ;";
            int sumPullsFromProgram = conn.ExecuteScalar<int>(sqlQuery, new { @user_id = userId, @date = date });

            sqlQuery = @"Select pulls From pulls.day_result WHERE date = CAST(@date as Date) and user_id = @user_id;";

            int sumPullsFromResult = conn.ExecuteScalar<int>(sqlQuery, new { @user_id = userId, @date = date });

            if (sumPullsFromResult < sumPullsFromProgram && sumPullsFromProgram > 0)
            {
                checkResult = $"не доделал программу на сегодня (осталось 👉🏻 {sumPullsFromProgram - sumPullsFromResult})";
            }
            else if (sumPullsFromResult > sumPullsFromProgram && sumPullsFromProgram > 0)
            {
                checkResult = $"перевыполнил программу на сегодня (сверх плана 🦾 {sumPullsFromResult - sumPullsFromProgram})";
            }
            else if (sumPullsFromProgram > 0)
            {
                checkResult = $"выполнил программу на сегодня 💪🏻";
            }

            return checkResult;
        }

        public void CreateLevelProgram() // Функция администратора
        {
            ConnString = DBConnection.ConnectionString();
            var result = CreateProgram.CreareProgramLevel();
            using var conn = new NpgsqlConnection(ConnString);

            conn.QueryFirstOrDefault<LevelProgram>(@"DELETE From pulls.lvl_user_program");
            //await conn.OpenAsync();
            //conn.Execute("DELETE From pulls.lvl_user_program");

            for (int i = 0; i < result.Count; i++)
            {
                LevelProgram res = result[i];
                var sqlQuery = @"INSERT INTO pulls.lvl_user_program (level, week, approach, pulls) VALUES (@level, @week, @approach, @pulls)";
                //conn.Execute(sqlQuery, new { @level = res.Level, @week = res.Week, @approach = res.Approach, @pulls = res.Pulls });
                conn.QueryFirstOrDefault<LevelProgram>(sqlQuery, new { @level = res.Level, @week = res.Week, @approach = res.Approach, @pulls = res.Pulls });
            }

            conn.Close();
        }

        public async Task<List<UserDayProgram>> DayStatus(long userId, string lvl)
        {
            ConnString = DBConnection.ConnectionString();
            var date = DateTime.Now;
            using var conn = new NpgsqlConnection(ConnString);

            string sqlQuery = "SELECT a.approach , a.pulls" +
                  " FROM  pulls.lvl_user_program a LEFT JOIN pulls.day_result b ON a.week = b.week " +
                  " WHERE a.level = @level::text  and b.date = CAST(@date as Date) and user_id = @user_id;";
            var dayProgram = await conn.QueryAsync<UserDayProgram>(sqlQuery, new { @user_id = userId, @date = date, @level = lvl });

            List<UserDayProgram> userDayProgram = new();

            foreach (var item in dayProgram)
            {
                userDayProgram.Add(new UserDayProgram(item.Approach, item.Pulls));
            }
            conn.Close();
            return userDayProgram;
        }

        public (int fact, int plan) FactPlanToday(long userId, string lvl)
        {
            ConnString = DBConnection.ConnectionString();
            var date = DateTime.Now;
            using var conn = new NpgsqlConnection(ConnString);

            string sqlQuery = "SELECT sum(a.pulls)" +
                  " FROM  pulls.lvl_user_program a LEFT JOIN pulls.day_result b ON a.week = b.week " +
                  " WHERE a.level = @level::text  and b.date = CAST(@date as Date) and user_id = @user_id;";

            int plan = conn.ExecuteScalar<int>(sqlQuery, new { @user_id = userId, @date = date, @level = lvl });
            int fact = conn.ExecuteScalar<int>("SELECT pulls FROM  pulls.day_result WHERE day_result.user_id = @user_id and date = CAST(@date as Date);", new { @user_id = userId, @date = date });

            conn.Close();
            return (fact, plan);
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
                _ = conn.Query<PlanPulls, FactPulls, PlanPulls>(sqlQuery, (p, f) =>
                {

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

                List<ForUserReport> report = new List<ForUserReport>();

                foreach (var plan in resultList)
                {
                    foreach (var fact in plan.Facts)
                    {
                        var sqlDateBegin = @"SELECT TO_CHAR(min(date):: DATE, 'dd.mm.yyyy') FROM pulls.day_result  WHERE day_result.user_id = @user_id and day_result.week = @week;";
                        var sqlDateEnd = @"SELECT TO_CHAR(max(date):: DATE, 'dd.mm.yyyy') FROM pulls.day_result  WHERE day_result.user_id = @user_id and day_result.week = @week;";
                        var dateBegin = conn.ExecuteScalar<string>(sqlDateBegin, new { user_id = userId, @week = plan.Week });
                        var dateEnd = conn.ExecuteScalar<string>(sqlDateEnd, new { user_id = userId, @week = plan.Week });

                        report.Add(new ForUserReport(plan.Week, plan.PullsPlan, fact.PullsFact, dateBegin, dateEnd));
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
            // conn.Execute(@"DELETE From pulls.day_result Where day_result.user_id = @user_id;", new { @user_id = userId });
            conn.QueryFirstOrDefault<DayResult>(@"DELETE From pulls.day_result Where day_result.user_id = @user_id;", new { @user_id = userId });


            conn.Execute(@"UPDATE pulls.users SET level = null Where users.user_id = @user_id;", new { @user_id = userId });
            conn.Close();
        }
    }
}
