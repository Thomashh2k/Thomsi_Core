using Headless.Core.Pagination;
using Headless.Core.Payloads;
using Headless.DB;
using Headless.DB.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headless.Core.Managers
{
    public interface IPagesManager
    {
        public Task<Page> CreatePage(PagePL routePL);
        public Task<PaginatedList<Page>> GetPaginatedPagesForTables(int count, int pageIndex, int pageSize);
        public Task<PaginatedList<Page>> GetPaginatedPages(int count, int pageIndex, int pageSize);
        public Task<Page> GetPage(Guid id);
        public Task<Page> UpdatePage(Guid id, Page updatedPage);
        public Task<bool> DeletePage(Guid id);
    }
    public class PagesManager : IPagesManager
    {
        private HeadlessDbContext DbContext { get; set; }
        public PagesManager(HeadlessDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<Page> CreatePage(PagePL pagePL)
        {
            Page newPage = new Page
            {
                Id = Guid.NewGuid(),
                Title = pagePL.Title,
                Route = pagePL.Route,
                Body = pagePL.Body,
                LangId = pagePL.LangId != Guid.Empty ? pagePL.LangId : Guid.Empty,
                Lang = DbContext.Languages.Find(pagePL.LangId)
            };

            DbContext.Pages.Add(newPage);
            await DbContext.SaveChangesAsync();

            return newPage;
        }

        //This API route is only to show the pages on tables without useless data.
        public async Task<PaginatedList<Page>> GetPaginatedPagesForTables(int count, int pageIndex, int pageSize)
        {
            List<Page> Pages = DbContext.Pages.ToList();

            for(var i = 0; i < Pages.Count; i++)
            {
                Pages[i].Body = "";
                Lang? pageLang = await DbContext.Languages.FindAsync(Pages[i].LangId);
                Pages[i].Lang = pageLang;
            }

            return new PaginatedList<Page>(Pages, count, pageIndex, pageSize);

        }

        public Task<PaginatedList<Page>> GetPaginatedPages(int count, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<Page> GetPage(Guid id) => await DbContext.Pages.FindAsync(id);

        public async Task<Page> UpdatePage(Guid id, Page updatedPage)
        {
            Page page = DbContext.Pages.Find(id);

            page.Title = (updatedPage.Title != "") ? updatedPage.Title : page.Title;
            page.Route = (updatedPage.Route != "") ? updatedPage.Route : page.Route;
            page.Body = (updatedPage.Body != "") ? updatedPage.Body : page.Body;
            page.LangId = (updatedPage.LangId != Guid.Empty) ? updatedPage.LangId : page.LangId;

            DbContext.Pages.Update(page);
            await DbContext.SaveChangesAsync();

            return page;
        }

        public async Task<bool> DeletePage(Guid id)
        {
            try
            {
                Page page = DbContext.Pages.Find(id);
                if (page != null)
                {
                    DbContext.Pages.Remove(page);
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
