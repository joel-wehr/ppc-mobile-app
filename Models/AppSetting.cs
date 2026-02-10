using SQLite;

namespace powered_parachute.Models
{
    [Table("app_settings")]
    public class AppSetting
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique, NotNull]
        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }
    }
}
