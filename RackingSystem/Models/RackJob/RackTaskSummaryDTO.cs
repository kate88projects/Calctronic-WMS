namespace RackingSystem.Models.RackJob
{
    public class RackTaskSummaryDTO
    {
        //jobs that is working now
        public int RunningCount { get; set; } = 0;

        //jobs that finished today 
        public int CompletedTodayCount { get; set; } = 0;
    }
}
