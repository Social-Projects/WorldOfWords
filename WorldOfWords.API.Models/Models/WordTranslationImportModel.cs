using System.Collections.Generic;

namespace WorldOfWords.API.Models
{
    public class WordTranslationImportModel
    {
        //public WordTranslationImportModel()
        //{
        //    Tags = new List<TagModel>();
        //}

        public string OriginalWord { get; set; }
        public string TranslationWord { get; set; }
        public int OriginalWordId { get; set; }
        public int TranslationWordId { get; set; }
        public string Transcription { get; set; }
        public string Description { get; set; }
        //public List<TagModel> Tags { get; set; }
        public int LanguageId { get; set; }
        public int OwnerId { get; set; }
    }
}
