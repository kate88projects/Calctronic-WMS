namespace RackingSystem.Models
{
    public class ServiceResponseModel<T>
    {
        public bool success { get; set; } = false;
        public string errMessage { get; set; } = "";
        public string errStackTrace { get; set; } = "";
        public T data { get; set; }
        public int totalRecords { get; set; } = 0;
        public List<string> errList { get; set; } = new List<string>();
    }
}
