namespace RackingSystem.Models.Setting
{
    public class SlotCalculationDTO
    {
        public long SlotCalculation_Id { get; set; }

        public int MaxThickness { get; set; } = 0;

        public int ReserveSlot { get; set; } = 0;
    }
}
