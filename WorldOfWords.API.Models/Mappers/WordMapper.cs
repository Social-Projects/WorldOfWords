﻿using WorldOfWords.API.Models.Models;
using WorldOfWords.Domain.Models;

namespace WorldOfWords.API.Models
{
    public class WordMapper: IWordMapper
    {

        public Word ToDomainModel(WordModel apiModel)
        {
            return new Word()
            {
                Id = apiModel.Id,
                LanguageId = apiModel.LanguageId,
                Value = apiModel.Value,
                Transcription = apiModel.Transcription,
                Description = apiModel.Description
            };
        }

        public WordModel ToApiModel(Word domainModel)
        {
            return new WordModel()
            {
                Id = domainModel.Id,
                LanguageId = domainModel.LanguageId,
                Value = domainModel.Value,
                Transcription = domainModel.Transcription,
                Description = domainModel.Description
            };
        }

        public WordValueModel ToValueModel(Word domainModel)
        {
            return new WordValueModel()
            {
                Id = domainModel.Id,
                Value = domainModel.Value
            };
        }
        public Word ToDomainModel(WordValueModel apiModel)
        {
            return new Word()
            {
                Id = apiModel.Id ?? default(int),
                Value = apiModel.Value
            };
        }
    }
}
