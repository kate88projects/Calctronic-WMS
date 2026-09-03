namespace RackingSystem.Models.Log
{
    public class PLCAddressResponseDTO
    {
        public int Address { get; set; }

        public string Action { get; set; } = "";

        public int Value { get; set; }

        public string MethodName { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsErr { get; set; } = false;

    }
}
