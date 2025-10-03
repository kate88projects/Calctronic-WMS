using RackingSystem.Models.Loader;
using RackingSystem.Models;
using RackingSystem.General;

namespace RackingSystem.Services.LoaderServices
{
    public interface ILoaderService
    {
        public Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList();
        public Task<ServiceResponseModel<LoaderDTO>> SaveLoader(LoaderDTO req);
        public Task<ServiceResponseModel<LoaderDTO>> DeleteLoader(LoaderDTO req);

        public Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList_ReadyToLoad();
        public Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo(string req, bool checkStatus, EnumLoaderStatus loaderStatus1, EnumLoaderStatus? loaderStatus2);
        public Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList_PendingToUnLoad();
        public Task<ServiceResponseModel<List<LoaderColumnDTO>>> GetLoaderColumn(int loaderId);
    }
}
