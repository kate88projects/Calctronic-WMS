using AutoMapper;
using RackingSystem.Data.GRN;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Slot;
using RackingSystem.Models.GRN;
using RackingSystem.Models.Item;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Reel;
using RackingSystem.Models.Trolley;
using RackingSystem.Models.Setting;
using RackingSystem.Models.BOM;

namespace RackingSystem.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Slot, SlotListDTO>();
            CreateMap<Item, ItemListDTO>();
            CreateMap<ItemGroup, ItemGroupListDTO>();
            CreateMap<Reel, ReelListDTO>();

            CreateMap<ReelDimension, ReelDimensionDTO>();
            CreateMap<ReelDimension, ReelDimensionListDTO>();
            CreateMap<SlotCalculation, SlotCalculationDTO>();
            CreateMap<SlotCalculation, SlotCalculationListDTO>();
            CreateMap<SlotColumnSetting, SlotColumnSettingDTO>();

            CreateMap<Loader, LoaderDTO>();
            CreateMap<Loader, LoaderListDTO>();
            CreateMap<Trolley, TrolleyListDTO>();
            CreateMap<TrolleySlot, TrolleySlotDTO>();
            CreateMap<BOM, BOMListDTO>();
            CreateMap<BOMDetail, BOMDtlDTO>();
            CreateMap<GRNDetail, GRNDtlListDTO>();
        }
    }
}
