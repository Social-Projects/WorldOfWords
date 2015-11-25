using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WorldOfWords.Infrastructure.Data.EF.Factory;
using WorldOfWords.Infrastructure.Data.EF.UnitOfWork;
using WorldOfWords.Infrastructure.Data.EF.Contracts;
using WorldOfWords.Domain.Services;
using WorldOfWords.Domain.Models;
using WorldOfWords.API.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace WorldOfWords.Tests.ServicesTests
{
    [TestFixture]
    public class WordTranslationServiceTest
    {
        private Mock<IUnitOfWorkFactory> _factory;
        private Mock<IWorldOfWordsUow> _uow;
        private Mock<IWordTranslationRepository> _repo;
        private WordTranslationService _service;
        private Mock<IWordTranslationMapper> mapper;
        private Mock<IWordMapper> wordMapper;
        private Mock<ITagMapper> tagMapper;

        [SetUp]
        public void Setup()
        {
            mapper = new Mock<IWordTranslationMapper>();
            wordMapper = new Mock<IWordMapper>();
            tagMapper = new Mock<ITagMapper>();
            _factory = new Mock<IUnitOfWorkFactory>();
            _uow = new Mock<IWorldOfWordsUow>();

            _service = new WordTranslationService(_factory.Object, mapper.Object, wordMapper.Object, tagMapper.Object);
            _repo = new Mock<IWordTranslationRepository>();

            _factory.Setup(f => f.GetUnitOfWork()).Returns(_uow.Object);
            _uow.Setup(u => u.WordTranslationRepository).Returns(_repo.Object);
        }

        public static Mock<IDbSet<T>> GenerateMockDbSet<T>(IQueryable<T> collection) where T : class
        {
            var mockSet = new Mock<IDbSet<T>>();
            mockSet.As<IDbAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<T>(collection.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<WordSuite>(collection.Provider));

            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(collection.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(collection.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(collection.GetEnumerator());

            return mockSet;
        }

        [Test]
        public void GetTopBySearchWord_ReturnsWordTranslationList()
        {
            //Act
            string searchWord = "preword";
            int languageId = 8;

            IQueryable<WordTranslation> expected = new List<WordTranslation>
            {
                new WordTranslation
                {
                    OriginalWord = new Word
                    {
                      Id = 8,
                      LanguageId = languageId,
                      Value = "preword"
                    },
                    TranslationWord = new Word
                    {
                        LanguageId = languageId,
                        Value = "preword",
                        Id = 8
                    }
                }
            }.AsQueryable<WordTranslation>(); ;

            _repo.Setup(w => w.GetAll()).Returns(expected);

            //Act
            var actual = _service.GetTopBySearchWord(searchWord, languageId);

            //Assert
            _factory.Verify(f => f.GetUnitOfWork(), Times.Once);
            _uow.Verify(u => u.WordTranslationRepository, Times.Once);
            _repo.Verify(w => w.GetAll(), Times.Once);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Exists_ReturnsId()
        {
            //Arrange
            int originalWordId = 1;
            int translationWordId = 2;
            int expected = 1;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                new WordTranslation
                {
                    Id = 1,
                    OriginalWordId = originalWordId,
                    TranslationWordId = translationWordId
                }
            }.AsQueryable<WordTranslation>();
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            var actual = _service.Exists(originalWordId, translationWordId);

            //Assert
            Assert.AreEqual(actual, expected);

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }
        [Test]
        public void Exists_ReturnsZero()
        {
            //Arrange
            int originalWordId = 1;
            int translationWordId = 2;
            int expected = 0;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                new WordTranslation
                {
                    OriginalWordId = 2,
                    TranslationWordId = translationWordId
                }
            }.AsQueryable<WordTranslation>();
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            var actual = _service.Exists(originalWordId, translationWordId);

            //Assert
            Assert.AreEqual(actual, expected);

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }
        [Test]
        public void Exists_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => _service.Exists(It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async void GetWordsFromIntervalAsync_ReturnsListOfWordTranslations()
        {
            //Arrange
            int startOfInterval = 0;
            int endOfInterval = 1;
            int originLangId = 1;
            int translLangId = 4;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                new WordTranslation
                {
                    OriginalWordId = 1,
                    OriginalWord = new Word
                    {
                        LanguageId = originLangId
                    },
                    TranslationWord = new Word
                    {
                        LanguageId = translLangId
}
                }
            }.AsQueryable<WordTranslation>();
            List<WordTranslationImportModel> expected = new List<WordTranslationImportModel>
            {
                new WordTranslationImportModel
                {
                    OriginalWordId = 1
                }
            };
            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);
            mapper.Setup(m => m.MapToImportModel(wordTranslations.First())).Returns(expected[0]);

            //Act
            var actual = await _service.GetWordsFromIntervalAsync(startOfInterval, endOfInterval, originLangId, translLangId);

            //Assert
            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Exactly(3));
            _repo.Verify(x => x.GetAll(), Times.Exactly(3));

            CollectionAssert.AreEqual(expected, actual);

        }
        [Test]
        public void GetWordsFromIntervalAsync_ThrowsException()
        {
            //Arrange
            int startOfInterval = 1;
            int startOfIntervalNegative = -1;
            int startOfIntervalBad = 3;
            int endOfInterval = 0;
            int endOfIntervalBad = 3;
            int originLangId = 1;
            int translLangId = 4;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
                                    {
                                        new WordTranslation
                                        {
                                            OriginalWordId = 1,
                                            OriginalWord = new Word
                                            {
                                                LanguageId = originLangId
                                            },
                                            TranslationWord = new Word
                                            {
                                                LanguageId = translLangId
                                            }
                                        },
                                        new WordTranslation
                                        {
                                            OriginalWordId = 2,
                                            OriginalWord = new Word
                                            {
                                                LanguageId = originLangId
                                            },
                                            TranslationWord = new Word
                                            {
                                                LanguageId = translLangId
}
                                        }
                                    }.AsQueryable<WordTranslation>();

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);

            //Act
            //Assert
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsFromIntervalAsync(startOfInterval, endOfInterval, originLangId, translLangId), "Start of interval is bigger than end");
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsFromIntervalAsync(startOfIntervalBad, endOfInterval, originLangId, translLangId), "Start of interval is bigger than end");
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsFromIntervalAsync(startOfIntervalNegative, endOfInterval, originLangId, translLangId), "Start of interval is bigger than end");
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsFromIntervalAsync(startOfInterval, endOfIntervalBad, originLangId, translLangId), "Start of interval is bigger than end");

            _factory.Verify(x => x.GetUnitOfWork(), Times.Exactly(4));
            _uow.Verify(x => x.WordTranslationRepository, Times.Exactly(5));
            _repo.Verify(x => x.GetAll(), Times.Exactly(5));
        }
        [Test]
        public void GetWordsFromIntervalAsync_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(async () => await _service.GetWordsFromIntervalAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async void GetWordsWithSearchValueAsync_ReturnsListOfWordTranslations()
        {
            //Arrange
            int startOfInterval = 0;
            int endOfInterval = 1;
            int originLangId = 1;
            int translLangId = 4;
            string searchVal = "foo";

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                new WordTranslation
                {
                    OriginalWordId = 1,
                    OriginalWord = new Word
                    {
                        Value = "fool",
                        LanguageId = originLangId
                    },
                    TranslationWord = new Word
                    {
                        LanguageId = translLangId
}
                }
            }.AsQueryable<WordTranslation>();
            List<WordTranslationImportModel> expected = new List<WordTranslationImportModel>
            {
                new WordTranslationImportModel
                {
                    OriginalWordId = 1,
                    OriginalWord = "fool"
                }
            };
            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);
            mapper.Setup(m => m.MapToImportModel(wordTranslations.First())).Returns(expected[0]);

            //Act
            var actual = await _service.GetWordsWithSearchValueAsync(searchVal, startOfInterval, endOfInterval, originLangId, translLangId);

            //Assert
            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Exactly(3));
            _repo.Verify(x => x.GetAll(), Times.Exactly(3));

            CollectionAssert.AreEqual(expected, actual);

        }
        [Test]
        public void GetWordsWithSearchValueAsync_ThrowsException()
        {
            //Arrange
            int startOfInterval = 1;
            int startOfIntervalNegative = -1;
            int startOfIntervalBad = 3;
            int endOfInterval = 0;
            int endOfIntervalBad = 3;
            int originLangId = 1;
            int translLangId = 4;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
                                    {
                                        new WordTranslation
                                        {
                                            OriginalWordId = 1,
                                            OriginalWord = new Word
                                            {
                                                LanguageId = originLangId
                                            },
                                            TranslationWord = new Word
                                            {
                                                LanguageId = translLangId
                                            }
                                        },
                                        new WordTranslation
                                        {
                                            OriginalWordId = 2,
                                            OriginalWord = new Word
                                            {
                                                LanguageId = originLangId
                                            },
                                            TranslationWord = new Word
                                            {
                                                LanguageId = translLangId
                                            }
                                        }
                                    }.AsQueryable<WordTranslation>();

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);

            //Act
            //Assert
            Assert.Throws<ArgumentException>(async () =>
               await _service.GetWordsWithSearchValueAsync(It.IsAny<string>(), startOfInterval, endOfInterval, originLangId, translLangId), "Start of interval is bigger than end");
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsWithSearchValueAsync(It.IsAny<string>(), startOfIntervalBad, endOfInterval, originLangId, translLangId), "Start of interval is bigger than end");
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsWithSearchValueAsync(It.IsAny<string>(), startOfIntervalNegative, endOfInterval, originLangId, translLangId), "Start of interval is bigger than end");
            Assert.Throws<ArgumentException>(async () =>
                await _service.GetWordsWithSearchValueAsync(It.IsAny<string>(), startOfInterval, endOfIntervalBad, originLangId, translLangId), "Start of interval is bigger than end");

            _factory.Verify(x => x.GetUnitOfWork(), Times.Exactly(4));
            _uow.Verify(x => x.WordTranslationRepository, Times.Exactly(5));
            _repo.Verify(x => x.GetAll(), Times.Exactly(5));

        }
        [Test]
        public void GetWordsWithSearchValueAsync_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(async () =>
                await _service.GetWordsWithSearchValueAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async void GetAmountOfWordTranslationsByLanguageAsync_ReturnsAmount()
        {
            //Arrange
            int originLangId = 1;
            int translLangId = 4;
            int actual = 1;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
                                    {
                                        new WordTranslation
                                        {
                                            OriginalWordId = 1,
                                            OriginalWord = new Word
                                            {
                                                LanguageId = originLangId
                                            },
                                            TranslationWord = new Word
                                            {
                                                LanguageId = translLangId
                                            }
                                        }
                                    }.AsQueryable<WordTranslation>();

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);

            //Act
            int expected = await _service.GetAmountOfWordTranslationsByLanguageAsync(originLangId, translLangId);

            //Assert
            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);

            Assert.AreEqual(actual, expected);

        }
        [Test]
        public void GetAmountOfWordTranslationsByLanguageAsync_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(async () => await _service.GetAmountOfWordTranslationsByLanguageAsync(It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async void GetAmountOfWordsBySearchValuesAsync_ReturnsAmount()
        {
            //Arrange
            int originLangId = 1;
            int translLangId = 4;
            string searchValue = "foo";
            int actual = 1;

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
                                    {
                                        new WordTranslation
                                        {
                                            OriginalWordId = 1,
                                            OriginalWord = new Word
                                            {
                                                Value = searchValue,
                                                LanguageId = originLangId
                                            },
                                            TranslationWord = new Word
                                            {
                                                LanguageId = translLangId
                                            }
                                        }
                                    }.AsQueryable<WordTranslation>();

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);

            //Act
            int expected = await _service.GetAmountOfWordsBySearchValuesAsync(searchValue, originLangId, translLangId);

            //Assert
            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);

            Assert.AreEqual(actual, expected);

        }
        [Test]
        public void GetAmountOfWordsBySearchValuesAsync_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(async () =>
                await _service.GetAmountOfWordsBySearchValuesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public void GetWordFullInformationAsync_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(async () =>
                await _service.GetWordFullInformationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        [Test]
        public async void GetWordFullInformationStringsAsync_ReturnsWord()
        {
            //Arrange
            string word = "word";
            string description = "word";
            string transl = "слово";
            string synon = "phrase";
            int originLangId = 1;
            int translLangId = 4;

            Word translation = new Word()
            {
                Id = 2,
                Value = transl,
                LanguageId = translLangId
            };
            Word synonim = new Word()
            {
                Id = 3,
                Value = synon,
                LanguageId = originLangId
            };

            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                new WordTranslation
                {
                    OriginalWordId = 1,
                    OriginalWord = new Word
                    {
                        Value = word,
                        Description = description,
                        LanguageId = originLangId
                    },
                    TranslationWord = translation
                },
                new WordTranslation
                {
                    OriginalWordId = 1,
                    OriginalWord = new Word
                    {
                        Value = word,
                        Description = description,
                        LanguageId = originLangId
                    },
                    TranslationWord = synonim
                }
            }.AsQueryable<WordTranslation>();

            WordTranslationFullStringsModel expected = new WordTranslationFullStringsModel
            {
                OriginalWord = word,
                Description = description,
                Translations = new List<string>
                {
                    transl
                },
                Synonims = new List<string>
                {
                    synon
                }
            };

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(x => x.GetAll()).Returns(mockSet.Object);


            //Act
            var actual = await _service.GetWordFullInformationStringsAsync(word, originLangId, translLangId);

            //Assert
            Assert.AreEqual(expected.OriginalWord, actual.OriginalWord);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Transcription, actual.Transcription);
            CollectionAssert.AreEqual(expected.Translations, actual.Translations);
            CollectionAssert.AreEqual(expected.Synonims, actual.Synonims);

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Exactly(3));
            _repo.Verify(x => x.GetAll(), Times.Exactly(3));

        }
        [Test]
        public void GetWordFullInformationStringsAsync_GetAllReturnsNull()
        {
            //Arrange
            IQueryable<WordTranslation> wordTranslations = null;
            _repo.Setup(x => x.GetAll()).Returns(wordTranslations);

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(async () =>
                await _service.GetWordFullInformationStringsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            _factory.Verify(x => x.GetUnitOfWork(), Times.Once);
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }
        
        [Test]
        public async void DeleteAsync_IfDependencyDoesntExists()
        {
            //Arrange
            int originalId = 1;
            int translationId = 2;
            WordTranslation transl = new WordTranslation()
            {
                OriginalWordId = originalId,
                TranslationWordId = translationId
            };
            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                transl

            }.AsQueryable<WordTranslation>();

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(r => r.GetAll()).Returns(mockSet.Object);
            _uow.Setup(x => x.SaveAsync()).ReturnsAsync(1);
            _repo.Setup(r => r.Delete(originalId, translationId)).Verifiable();

            //Act
            var actual = await _service.DeleteAsync(originalId, translationId);

            //Assert
            Assert.IsTrue(actual);

            _factory.Verify(x => x.GetUnitOfWork(), Times.Exactly(2));
            _uow.Verify(x => x.WordTranslationRepository, Times.Exactly(2));
            _repo.Verify(x => x.GetAll(), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once);
            _repo.VerifyAll();          

        }
        [Test]
        public async void DeleteAsync_IfDependencyExists()
        {
            //Arrange
            int originalId = 1;
            int translationId = 2;
            ICollection<WordProgress> progresses = new List<WordProgress>
            {
                new WordProgress
                    {
                        WordTranslationId = originalId
                    }
            };
            WordTranslation transl = new WordTranslation()
            {
                OriginalWordId = originalId,
                TranslationWordId = translationId,
                WordProgresses = progresses
            };
            IQueryable<WordTranslation> wordTranslations = new List<WordTranslation>
            {
                transl

            }.AsQueryable<WordTranslation>();

            var mockSet = GenerateMockDbSet<WordTranslation>(wordTranslations);
            _repo.Setup(r => r.GetAll()).Returns(mockSet.Object);
            _uow.Setup(x => x.SaveAsync()).ReturnsAsync(1);

            //Act
            var actual = await _service.DeleteAsync(originalId, translationId);

            //Assert
            Assert.IsTrue(actual);

            _factory.Verify(x => x.GetUnitOfWork(), Times.Exactly(2));
            _uow.Verify(x => x.WordTranslationRepository, Times.Once);
            _repo.Verify(x => x.GetAll(), Times.Once);
        }

        

    }
}
