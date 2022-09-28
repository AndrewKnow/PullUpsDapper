﻿
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
            return conn.Query<User>("SELECT * FROM  pulls.users ;").ToList();
        }

        public (string lvl, int count, bool program) GetUsersId(long userId)
        {
            ConnString = DBConnection.ConnectionString();
            using var conn = new NpgsqlConnection(ConnString);
            string lvl = conn.ExecuteScalar<string>("SELECT users.level FROM  pulls.users  WHERE users.user_id = " + userId + ";");
            int count = conn.ExecuteScalar<int>("SELECT count(*) FROM  pulls.users  WHERE users.user_id = " + userId + ";");
            bool program = conn.ExecuteScalar<bool>("SELECT count(*) FROM  pulls.user_program  WHERE user_program.user_id = " + userId + ";");
            conn.Close();
            return (lvl, count, program);
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
            var (DayResult, UserProgram) = CreateProgram.CreateLvlProgram(lvl, userId);
            using var conn = new NpgsqlConnection(ConnString);
            for (int i = 0; i < UserProgram.Count; i++)
            {  
                TrainingProgram program = UserProgram[i];
                var sqlQuery = "INSERT INTO pulls.user_program (user_id, week, approach, pulls)  VALUES ('"
                    + program.Id + "', '"
                    + program.Week + "', '"
                    + program.Approach + "', '"
                    + program.Pulls + "'"
                    + ")";
                conn.Execute(sqlQuery); 
            }
            for (int i = 0; i < DayResult.Count; i++)
            {
                DayResult dayResult = DayResult[i];
                var sqlQuery = "INSERT INTO pulls.day_result (user_id, week, date, pulls)  VALUES ('"
                    + dayResult.Id + "', '"
                    + dayResult.Week + "', '"
                    + dayResult.Date + "', '"
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
            //string sqlQuery = "SELECT user_program.date, user_program.approach , 
            //                user_program.pulls FROM  pulls.user_program a LEFT JOIN pulls.day_result b ON a.user_id = b.user_id WHERE user_program.user_id = " + userId +
            //            " and user_program.date = CAST('" + date + "' as Date);";

            string sqlQuery = "SELECT b.date, a.approach , a.pulls" +
                              " FROM  pulls.user_program a LEFT JOIN pulls.day_result b ON a.user_id = b.user_id and a.week = b.week " +
                              " WHERE a.user_id = " + userId + " and b.date = CAST('" + date + "' as Date);";

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
            //int check = conn.ExecuteScalar<int>("SELECT count(*) FROM   pulls.day_result WHERE day_result.user_id = " + userId + " and date = '" + date + "';");

            //int sumPullsFromProgram = conn.ExecuteScalar<int>("SELECT sum(pulls) FROM pulls.user_program WHERE user_program.date = '" + date + 
            //    "' and user_program.user_id  = '" + userId + "';");

            int sumPullsFromProgram = conn.ExecuteScalar<int>("Select sum(pulls) From pulls.user_program WHERE " +
                        "week = (Select week From pulls.day_result WHERE date = CAST('" + date +"' as Date));");

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

            //if (check == 0)
            //{
            //    var sqlQuery = "INSERT INTO pulls.day_result  (user_id, date, pulls) VALUES ('" + userId + "', '" + date + "', '" + pulls + "')";
            //    conn.Execute(sqlQuery);
            //}
            //else
            //{
            var sqlQuery = "UPDATE pulls.day_result Set pulls = '" + pulls + "' WHERE day_result.user_id = " + userId + " and date = CAST('" + date + "' as Date));";
            conn.Execute(sqlQuery);
            //}
            UserDayProgram.DayReport = false;
            conn.Close();

            return checkResult;
        }
    }
}
