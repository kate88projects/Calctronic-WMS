namespace RackingSystem.Models.Setting
{
    public class SlotColumnSettingDTO
    {
        public long SlotColumnSetting_Id { get; set; }

        public int ColNo { get; set; } = 0;

        public int EmptyDrawer_IN_Idx { get; set; } = 0;

        public int Reel_IN_Idx { get; set; } = 0;

        public int EmptyDrawer_OUT_Idx { get; set; } = 0;

        public int Reel_OUT_Idx { get; set; } = 0;
    }
}
