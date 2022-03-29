namespace Statistic
{
    public class AppSettings
    {
        public Logging Logging { get; set; }
        public Database Database { get; set; }
    }
    public class Logging
    {
        public string FileName { get; set; }
        public LogLevel MinLevel { get; set; }
    }
    public class Database
    {
        public string ConnectionString { get; set; }
        public DatabaseType Type { get; set; }
        public string DataFolder { get; set; }
    }
    public enum LogLevel
    {
        Information,
        Debug,
        Warning,
        Error,
        Fatal
    }

    public enum DatabaseType
    {
        Postgres
    }
}