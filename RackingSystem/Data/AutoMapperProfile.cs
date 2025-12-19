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
using RackingSystem.Data.JO;
using RackingSystem.Models.JO;
using RackingSystem.Models.RackJob;
using RackingSystem.Data.RackJob;

namespace RackingSystem.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Slot, SlotDTO>();
            CreateMap<Slot, SlotListDTO>();
            CreateMap<Item, ItemListDTO>();
            CreateMap<ItemGroup, ItemGroupListDTO>();
            CreateMap<Reel, ReelListDTO>();

            CreateMap<ReelDimension, ReelDimensionDTO>();
            CreateMap<ReelDimension, ReelDimensionListDTO>();
            CreateMap<SlotCalculation, SlotCalculationDTO>();
            CreateMap<SlotCalculation, SlotCalculationListDTO>();
            CreateMap<SlotColumnSetting, SlotColumnSettingDTO>();
            CreateMap<Slot, SlotUsageDTO>();

            CreateMap<Loader, LoaderDTO>();
            CreateMap<Loader, LoaderListDTO>()
            .ForMember(dest => dest.ColList, opt => opt.Ignore())
            .ForMember(dest => dest.ColBalList, opt => opt.Ignore())
            .ForMember(dest => dest.BalancePercentage, opt => opt.Ignore())
            .ForMember(dest => dest.BalanceHeight, opt => opt.Ignore())
            .ForMember(dest => dest.BalancePercentage, opt => opt.Ignore())
            .ForMember(dest => dest.UsagePercentage, opt => opt.Ignore());
            CreateMap<LoaderColumn, LoaderColumnDTO>()
            .ForMember(dest => dest.BalancePercentage, opt => opt.Ignore())
            .ForMember(dest => dest.UsagePercentage, opt => opt.Ignore());
            CreateMap<Trolley, TrolleyListDTO>();
            CreateMap<Trolley, TrolleyDTO>();
            CreateMap<TrolleySlot, TrolleySlotDTO>();
            CreateMap<BOM, BOMListDTO>();
            CreateMap<BOMDetail, BOMDtlDTO>();
            CreateMap<BOMDetail, BOMListReqDTO>();
            CreateMap<GRNDetail, GRNDtlListDTO>();
            CreateMap<GRNDetail, GRNDtlDTO>();
            CreateMap<JobOrder, JOListDTO>();
            CreateMap<JobOrderDetail, JODetailReqDTO>();
            CreateMap<JobOrderDetail, RackJobHubOutDtlDTO>();
            CreateMap<JobOrderEmergency, JOEmergencyReqDTO>();
            CreateMap<JobOrderEmergency, RackJobHubOutEDtlDTO>();
            CreateMap<JobOrderEmergencyDetail, JOEmergencyDetailReqDTO>();
            CreateMap<RackJob.RackJob, RackJobDTO>();

        }
    }
}
