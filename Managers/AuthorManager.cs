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
    public interface IAuthorManager
    {
        public Task<Author> CreateAuthor();

    }
    public class AuthorManager
    {
        private HeadlessDbContext DbContext;
        public AuthorManager(HeadlessDbContext dbContext)
        {
            DbContext = dbContext;
        }

        //public async Task<Author> CreateAuthor(AuthorPL data)
        //{

        //}

        //public async Task<Author> UpdateAuthor(Guid id, AuthorPL data)
        //{

        //}

        //public async Task<Author> GetAuthorByID(Guid id)
        //{

        //}

        //public async Task<Author> DeleteAuthor(Guid id)
        //{

        //}

        //public async Task<Author> DeactivateAuthor(Guid id)
        //{

        //}

        //public async Task<Author> ActivateAuthor(Guid id)
        //{

        //}
    }
}
