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
        public Task<Page> CreatePage(CreatePagePL routePL);
        public Task<PaginatedList<Page>> GetPaginatedPagesForTables(int count, int pageIndex, int pageSize);
        public Task<PaginatedList<Page>> GetPaginatedPages(int count, int pageIndex, int pageSize);
        public Task<PaginatedList<Page>> GetPage(int count, int pageIndex, int pageSize);
    }
    public class PagesManager : IPagesManager
    {
        private HeadlessDbContext DbContext { get; set; }
        public PagesManager(HeadlessDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<Page> CreatePage(CreatePagePL pagePL)
        {
            //FKs Cant be null this needs to be fixed
            Page newPage = new Page
            {
                Id = Guid.NewGuid(),
                Title = pagePL.Title,
                Route = pagePL.Route,
                Body = pagePL.Body,
                LangId = pagePL.LangId != Guid.Empty ? pagePL.LangId : Guid.Empty,
                Lang = DbContext.Languages.First(lng => lng.Id == pagePL.LangId)
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

            return new PaginatedList<Page>(DbContext.Pages.ToList(), count, pageIndex, pageSize);

        }

        public Task<PaginatedList<Page>> GetPaginatedPages(int count, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedList<Page>> GetPage(int count, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
