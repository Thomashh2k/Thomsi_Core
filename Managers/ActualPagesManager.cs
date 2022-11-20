using Headless.Core.Pagination;
using Headless.Core.Payloads;
using Headless.DB;
using Headless.DB.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headless.Core.Managers
{
    public interface IActualPagesManager
    {
        public Task<ActualPage> CreateActualPage(Guid PageID, PagePL routePL);
        //public Task<PaginatedList<ActualPage>> GetPaginatedPagesForTables(int count, int pageIndex, int pageSize);
        //public Task<PaginatedList<ActualPage>> GetPaginatedPages(int count, int pageIndex, int pageSize);
        public Task<ActualPage> GetActualPage(Guid pageId, Guid langId);
        public Task<ActualPage> UpdateActualPage(Guid pageId, Guid langId, PagePL updatedPage);
        public Task<bool> DeleteActualPages(Guid id);
    }
    public class ActualPagesManager : IActualPagesManager
    {
        private HeadlessDbContext DbContext { get; set; }
        public ActualPagesManager(HeadlessDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<ActualPage> CreateActualPage(Guid PageID, PagePL pagePL)
        {
            ActualPage actualPage = new ActualPage
            {
                Body = pagePL.Body,
                LangId = pagePL.LangId != Guid.Empty ? pagePL.LangId : Guid.Empty,
                PageID = PageID
            };
            DbContext.ActualPage.Add(actualPage);
            await DbContext.SaveChangesAsync();

            return actualPage;
        }

        ////This API route is only to show the pages on tables without useless data.
        //public async Task<PaginatedList<Page>> GetPaginatedPagesForTables(int count, int pageIndex, int pageSize)
        //{
        //    List<Page> Pages = DbContext.Pages.ToList();
        //    return new PaginatedList<Page>(Pages, count, pageIndex, pageSize);

        //}

        //public Task<PaginatedList<Page>> GetPaginatedPages(int count, int pageIndex, int pageSize)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<ActualPage> GetActualPage(Guid pageId, Guid langId)
        {

            return await DbContext.ActualPage.FirstOrDefaultAsync(ap => ap.LangId == langId && ap.PageID == pageId);
        }

        public async Task<ActualPage> UpdateActualPage(Guid pageId, Guid langId, PagePL updatedPage)
        {
            ActualPage page = await GetActualPage(pageId, langId);

            page.Body = (updatedPage.Body != "") ? updatedPage.Body : page.Body;

            DbContext.ActualPage.Update(page);
            await DbContext.SaveChangesAsync();

            return page;
        }

        public async Task<bool> DeleteActualPages(Guid id)
        {
            try
            {
                ActualPage[] pages = DbContext.ActualPage.Where(ap => ap.Id == id).ToArray();
                if (pages != null)
                {
                    DbContext.ActualPage.RemoveRange(pages);
                    await DbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
