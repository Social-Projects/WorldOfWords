using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WorldOfWords.API.Models;
using WorldOfWords.Domain.Services;

namespace WorldofWords.Controllers
{
    [WowAuthorization(AllRoles = new[] { "Teacher", "Student", "Admin" })]
    [RoutePrefix("api/GlobalDictionary")]
    public class GlobalDictionaryController : ApiController
    {
        private readonly IWordTranslationService wordTranslationService;

        public GlobalDictionaryController(IWordTranslationService wordTranslationService)
        {
            this.wordTranslationService = wordTranslationService;
        }

        public async Task<List<WordTranslationImportModel>> Get(int start, int end, int originalLangId, int translationLangId)
        {
            return await wordTranslationService.GetWordsFromIntervalAsync(start, end, originalLangId, translationLangId);
        }

        public async Task<List<WordTranslationImportModel>> GetBySearchValue(string searchValue, int startOfInterval, int endOfInterval, int originalLangId, int translationLangId)
        {
            return await wordTranslationService.GetWordsWithSearchValueAsync(searchValue, startOfInterval, endOfInterval, originalLangId, translationLangId);
        }

        public async Task<int> Get(int originalLangId, int translationLangId)
        {
            return await wordTranslationService.GetAmountOfWordTranslationsByLanguageAsync(originalLangId, translationLangId);
        }

        public async Task<int> GetAmountOfWordsBySearchValue(string searchValue, int originalLangId, int translationLangId)
        {
            return await wordTranslationService.GetAmountOfWordsBySearchValuesAsync(searchValue, originalLangId, translationLangId);
        }

        [HttpGet]
        public async Task<WordTranslationFullModel> GetFullWord(string word, int originalLangId, int translationLangId)
        {
            return await wordTranslationService.GetWordFullInformationAsync(word, originalLangId, translationLangId);
        }

        [HttpGet]
        public async Task<WordTranslationFullStringsModel> GetFullWordStrings(string wordValue, int originalLangId, int translationLangId)
        {
            var model = await wordTranslationService.GetWordFullInformationStringsAsync(wordValue, originalLangId, translationLangId);
            return model;
        }

        
        [Route("GetAmountByTags")]
        public async Task<int> GetAmountOfTags(string searchValue, int originalLangId, int translationLangId)
        {
             
           return await wordTranslationService.GetAmountOfTagsBySearchValuesAsync(searchValue, originalLangId, translationLangId);
          
        }

        [Route("GetWordsByTag")]
        public async Task<List<WordTranslationImportModel>> GetWordsByTagValue(int startOfInterval, int endOfInterval, int originalLangId, string searchValue, int translationLangId)
        {
            return await wordTranslationService.GetWordsWithTagAsync(startOfInterval, endOfInterval, originalLangId, searchValue, translationLangId);
            
        }
    }
}
