using System.Collections.Generic;

namespace Replica.App
{
    internal class Database
    {
        public string Type { get; set; } = "sqlite";
        public string Connection { get; set; } = "Data Source=data/db";
    }

    internal class AppOptions
    {
        public List<string> SU { get; set; }
        public bool Public { get; set; }
        public Database Database { get; set; } = new Database();
        public bool Skip { get; set; } = true;
        public bool Changelog { get; set; }
        public string LogLevel { get; set; } = "Information";
    }
}