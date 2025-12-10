using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Log;
using RackingSystem.Data.Maintenances;
using System.Drawing;

namespace RackingSystem.Helpers
{
    public class PLCLogHelper
    {
        #region singleton
        private static readonly Lazy<PLCLogHelper> lazy = new Lazy<PLCLogHelper>(() => new PLCLogHelper());
        public static PLCLogHelper Instance { get { return lazy.Value; } }
        private PLCLogHelper() { }
        #endregion

        public void InsertPLCLoaderLog(AppDbContext _dbContext, long id, string evt, string r1, string r2)
        {


            PLCLoaderLog log = new PLCLoaderLog()
            {
                Loader_Id = id,
                EventName = evt,
                Remark1 = r1,
                Remark2 = r2,
                CreatedDate = DateTime.Now,
            };
            _dbContext.PLCLoaderLog.Add(log);
            _dbContext.SaveChanges();

        }

        public void InsertPLCHubInLog(AppDbContext _dbContext, long id, string evt, string r1, string r2)
        {


            PLCHubInLog log = new PLCHubInLog()
            {
                Loader_Id = id,
                EventName = evt,
                Remark1 = r1,
                Remark2 = r2,
                CreatedDate = DateTime.Now,
            };
            _dbContext.PLCHubInLog.Add(log);
            _dbContext.SaveChanges();

        }
    }
}
