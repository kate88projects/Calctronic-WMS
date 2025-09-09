using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Setting
{
    public class GlobalSettingDTO
    {
        public long Configuration_Id { get; set; }

        public string ConfigTitle { get; set; } = "";

        public string ConfigValue { get; set; } = "";
    }
}
