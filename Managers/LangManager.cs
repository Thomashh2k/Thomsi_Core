﻿using Headless.Core.Pagination;
using Headless.Core.Payloads;
using Headless.DB.Tables;
using Headless.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headless.Core.Managers
{
    public interface ILangManager
    {
        public Task<Lang> CreateLang(CreateLanuagePL langPL);
        public Task<PaginatedList<Lang>> GetPaginatedLang(int count, int pageIndex, int pageSize);
        public Task<Lang> GetSingleLangById(Guid id);
        public Lang UpdateLang(Lang updatedLang);
    }
    public class LangManager : ILangManager
    {
        private HeadlessDbContext DbContext;
        public LangManager(HeadlessDbContext dbContext)
        {
            DbContext = dbContext;
        }
        public async Task<Lang> CreateLang(CreateLanuagePL langPL)
        {
            Lang newLang = new Lang
            {
                Id = Guid.NewGuid(),
                LanguageIdentifier = langPL.LanguageIdentifier,
                LanguageName = langPL.LanguageName
            };

            DbContext.Languages.Add(newLang);
            await DbContext.SaveChangesAsync();

            return newLang;

        }

        public async Task<PaginatedList<Lang>> GetPaginatedLang(int count, int pageIndex, int pageSize)
        {
            return new PaginatedList<Lang>(DbContext.Languages.ToList(), count, pageIndex, pageSize);
        }

        public async Task<Lang> GetSingleLangById(Guid id) => DbContext.Languages.Find(id);

        public Lang UpdateLang(Lang updatedLang)
        {
            Lang lang = DbContext.Languages.FirstOrDefault(lng => lng.Id == updatedLang.Id);

            lang.LanguageName = (updatedLang.LanguageName != "") ? updatedLang.LanguageName : lang.LanguageName;
            lang.LanguageIdentifier = (updatedLang.LanguageIdentifier != "") ? updatedLang.LanguageIdentifier : lang.LanguageIdentifier;

            DbContext.Languages.Update(lang);

            return lang;
        }
    }
}