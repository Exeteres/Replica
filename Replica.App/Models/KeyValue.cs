using System.ComponentModel.DataAnnotations;

namespace Replica.App.Models
{
    public class KeyValue
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }

        public static void Set(string key, string value)
        {
            var db = new RepoContext();
            var val = new KeyValue { Key = key, Value = value };
            if (Get(key) == null) db.Add(val);
            else db.Update(val);
            db.SaveChanges();
        }

        public static string Get(string key)
        {
            var db = new RepoContext();
            return db.Values.Find(key)?.Value;
        }
    }
}