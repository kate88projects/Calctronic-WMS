using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.RackJob
{
    public class RackJobDTO
    {
        public long Rack_Id { get; set; }

        public string CurrentJobType { get; set; } = "";

        public long Loader_Id { get; set; } = 0;

        public long Trolley_Id { get; set; } = 0;

        public long RackJobQueue_Id { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public string LoginIP { get; set; } = "";

        public string Json { get; set; } = "";

    }
}
