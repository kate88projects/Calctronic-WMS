using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using RackingSystem.Models;
using System.Data;
using System.Threading.Tasks;

namespace RackingSystem.Helpers
{
    public class DocFormatHelper
    {
        #region singleton
        private static readonly Lazy<DocFormatHelper> lazy = new Lazy<DocFormatHelper>(() => new DocFormatHelper());
        public static DocFormatHelper Instance { get { return lazy.Value; } }
        private DocFormatHelper() { }
        #endregion

        public async Task<ServiceResponseModel<string>> get_NextDocumentNo(AppDbContext _dbContext, EnumConfiguration configType, DateTime docDate, bool needUpdate)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.success = false;
            string docno = "";
            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == configType.ToString()).FirstOrDefault();
                if (config != null)
                {
                    var docF = _dbContext.DocFormat.Where(x => x.DocFormat_Id == Convert.ToInt64(config.ConfigValue)).FirstOrDefault();
                    if (docF != null)
                    {
                        if (docF.IsResetMonthly)
                        {
                            int nextNo = await get_DocFormatDtl(_dbContext, docF.DocFormat_Id, docDate, needUpdate);
                            docno = get_ExampleDocumentFormat(docF.DocumentFormat, nextNo.ToString(), docDate);
                        }
                        else
                        {
                            int nextNo = docF.NextRoundingNum;
                            docno = get_ExampleDocumentFormat(docF.DocumentFormat, nextNo.ToString(), docDate);
                            if (docno != "")
                            {
                                if (needUpdate)
                                {
                                    docF.NextRoundingNum = nextNo + 1;
                                    _dbContext.DocFormat.Update(docF);
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                //if (dr == null)
                //{
                //    result.message = "Please check on Configuration > Doc Format.";
                //    return result;
                //}
                result.success = true;
                result.data = docno;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                return result;
            }
            return result;
        }

        internal string get_ExampleDocumentFormat(string docFormat, string nextNum, DateTime docDate)
        {
            if (string.IsNullOrEmpty(docFormat)) { return ""; }
            if (string.IsNullOrEmpty(nextNum)) { return ""; }

            int value = 1;
            int.TryParse(nextNum, out value);
            string result = docFormat;
            string formatstring = "";
            string formatstring2 = "";
            string formattedstring = "";
            string dateformatstring = "";
            string dateformatstring2 = "";
            string dateformattedstring = "";

            //Get Format String eg. 000000
            if (docFormat.Contains("<") && docFormat.Contains(">"))
            {
                int start = docFormat.IndexOf("<") + 1;
                int end = docFormat.IndexOf(">", start);
                formatstring = docFormat.Substring(start, end - start);
                formatstring2 = docFormat.Substring(start - 1, end - start + 2);
                formattedstring = value.ToString(formatstring);

                result = result.Replace(formatstring2, formattedstring);
            }
            //Get Date Format String eg. MMyyyy
            if (docFormat.Contains("{") && docFormat.Contains("}"))
            {
                int start = docFormat.IndexOf("{") + 1;
                int end = docFormat.IndexOf("}", start);
                dateformatstring = docFormat.Substring(start, end - start);
                dateformatstring2 = docFormat.Substring(start - 1, end - start + 2);
                dateformattedstring = docDate.ToString(dateformatstring);

                result = result.Replace(dateformatstring2, dateformattedstring);
                result = result.Replace("@", "");
            }

            return result;
        }

        internal async Task<int> get_DocFormatDtl(AppDbContext _dbContext, long id, DateTime docDate, bool needUpdate)
        {
            int nextNo = 0;

            try
            {
                var docFDtl = _dbContext.DocFormatDetail.Where(x => x.DocFormat_Id == id && x.Year == docDate.Year && x.Month == docDate.Month).FirstOrDefault();
                if (docFDtl != null)
                {
                    nextNo = docFDtl.NextRoundingNum;
                    if (needUpdate)
                    {
                        docFDtl.NextRoundingNum = nextNo + 1;
                        _dbContext.DocFormatDetail.Update(docFDtl);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    nextNo = 1;
                    if (needUpdate)
                    {
                        var dtl = new DocFormatDetail();
                        dtl.DocFormat_Id = id;
                        dtl.Year = docDate.Year;
                        dtl.Month = docDate.Month;
                        dtl.NextRoundingNum = 2;
                        _dbContext.DocFormatDetail.Add(dtl);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return nextNo;
        }

    }
}
