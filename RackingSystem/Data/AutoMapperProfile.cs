using AutoMapper;
using RackingSystem.Data.GRN;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Slot;
using RackingSystem.Models.GRN;
using RackingSystem.Models.Item;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Reel;
using RackingSystem.Models.Trolley;

namespace RackingSystem.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Slot, SlotListDTO>();
            CreateMap<Item, ItemListDTO>();
            CreateMap<Reel, ReelListDTO>();

            CreateMap<Loader, LoaderDTO>();
            CreateMap<Loader, LoaderListDTO>();
            CreateMap<Trolley, TrolleyListDTO>();

            CreateMap<GRNDetail, GRNDtlListDTO>();
        }
    }
}
