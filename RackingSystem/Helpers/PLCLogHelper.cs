using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Log;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using System.Drawing;
using System.Threading.Tasks;

namespace RackingSystem.Helpers
{
    public class PLCLogHelper
    {
        #region singleton
        private static readonly Lazy<PLCLogHelper> lazy = new Lazy<PLCLogHelper>(() => new PLCLogHelper());
        public static PLCLogHelper Instance { get { return lazy.Value; } }
        private PLCLogHelper() { }
        #endregion

        public void InsertPLCLoaderLog(AppDbContext _dbContext, long id, string evt, string r1, string r2, bool isErr)
        {
            var logExist = isErr ? null : _dbContext.PLCLoaderLog.Where(x => x.Loader_Id == id && x.EventName == evt && x.Remark1 == r1 && x.Remark2 == r2).FirstOrDefault();
            if (logExist != null)
            {
                logExist.CreatedDate = DateTime.Now;
            }
            else
            {
                PLCLoaderLog log = new PLCLoaderLog()
                {
                    Loader_Id = id,
                    EventName = evt,
                    Remark1 = r1,
                    Remark2 = r2,
                    CreatedDate = DateTime.Now,
                    IsErr = isErr,
                };
                _dbContext.PLCLoaderLog.Add(log);
            }
            _dbContext.SaveChanges();

        }

        public void InsertPLCHubInLog(AppDbContext _dbContext, long id, string evt, string r1, string r2, bool isErr)
        {
            var logExist = isErr ? null : _dbContext.PLCHubInLog.Where(x => x.Loader_Id == id && x.EventName == evt && x.Remark1 == r1 && x.Remark2 == r2).FirstOrDefault();
            if (logExist != null)
            {
                logExist.CreatedDate = DateTime.Now;
            }
            else
            {
                PLCHubInLog log = new PLCHubInLog()
                {
                    Loader_Id = id,
                    EventName = evt,
                    Remark1 = r1,
                    Remark2 = r2,
                    CreatedDate = DateTime.Now,
                    IsErr = isErr,
                };
                _dbContext.PLCHubInLog.Add(log);
            }
            _dbContext.SaveChanges();

        }

        public void InsertPLCHubOutLog(AppDbContext _dbContext, long id, string evt, string r1, string r2, bool isErr)
        {
            var logExist = isErr ? null : _dbContext.PLCHubOutLog.Where(x => x.Loader_Id == id && x.EventName == evt && x.Remark1 == r1 && x.Remark2 == r2).FirstOrDefault();
            if (logExist != null)
            {
                logExist.CreatedDate = DateTime.Now;
            }
            else
            {
                PLCHubOutLog log = new PLCHubOutLog()
                {
                    Loader_Id = id,
                    EventName = evt,
                    Remark1 = r1,
                    Remark2 = r2,
                    CreatedDate = DateTime.Now,
                    IsErr = isErr,
                };
                _dbContext.PLCHubOutLog.Add(log);
            }
            _dbContext.SaveChanges();

        }

        public void InsertPLCTrolleyLog(AppDbContext _dbContext, long id, string evt, string r1, string r2, bool isErr)
        {
            var logExist = isErr ? null : _dbContext.PLCTrolleyLog.Where(x => x.Loader_Id == id && x.EventName == evt && x.Remark1 == r1 && x.Remark2 == r2).FirstOrDefault();
            if (logExist != null)
            {
                logExist.CreatedDate = DateTime.Now;
            }
            else
            {
                PLCTrolleyLog log = new PLCTrolleyLog()
                {
                    Loader_Id = id,
                    EventName = evt,
                    Remark1 = r1,
                    Remark2 = r2,
                    CreatedDate = DateTime.Now,
                    IsErr = isErr,
                };
                _dbContext.PLCTrolleyLog.Add(log);
            }
            _dbContext.SaveChanges();

        }

        public async Task DeleteLog(AppDbContext _dbContext)
        {
            try
            {
                var sqlSP = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"EXEC {GeneralStatic.SP_Log_Delete} ");
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

        }

    }
}
