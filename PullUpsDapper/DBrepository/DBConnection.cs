namespace PullUpsDapper.DBrepository
{
    public static class DBConnection
    {
        private static readonly string _host = "localhost";
        private static readonly string _user = "postgres";
        private static readonly string _dbName = "PullUps";
        private static readonly string _port = "5432";
        public static string ConnectionString()
        {
            string password = Password.DB();
            string connString = string.Format
            (
              "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
              _host,
              _user,
              _dbName,
              _port,
              password);
            return connString;
        }
    }
}
