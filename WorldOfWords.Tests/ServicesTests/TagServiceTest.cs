//Oleh Krutii
//Reviwer: Harasymovych Yurii
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using WorldOfWords.Infrastructure.Data.EF.Factory;
using WorldOfWords.Infrastructure.Data.EF.UnitOfWork;
using WorldOfWords.Domain.Models;
using WorldOfWords.API.Models;
using WorldOfWords.Infrastructure.Data.EF.Contracts;
using WorldOfWords.Domain.Services;

namespace WorldOfWords.Tests.ServicesTests
{
    [TestFixture]
    class TagServiceTest
    {
        private Mock<IUnitOfWorkFactory> _factory;
        private Mock<IWorldOfWordsUow> _uow;
        private Mock<IRepository<Tag>> _repo;
        private TagService _service;
        Mock<ITagMapper> tagMapper;

        [SetUp]
        public void Setup()
        {
            tagMapper = new Mock<ITagMapper>();
            _factory = new Mock<IUnitOfWorkFactory>();
            _uow = new Mock<IWorldOfWordsUow>();
            _repo = new Mock<IRepository<Tag>>();
            _factory.Setup(f => f.GetUnitOfWork()).Returns(_uow.Object);
            _uow.Setup(u => u.TagRepository).Returns(_repo.Object);
            _service = new TagService(_factory.Object, tagMapper.Object);
        }

        [Test]
        public void Exists_ReturnsTagId_TagExists()
        {
            //Arrange
            string name = "pesik";
            int expected = 8;
            var listik = new List<Tag>
            {
                new Tag 
                {
                    Name = name,
                    Id = expected
                }
            }.AsQueryable<Tag>();
            _repo.Setup(r => r.GetAll()).Returns(listik);

            //Act
             int actual = _service.Exists(name);

            //Assert
             _factory.Verify(f => f.GetUnitOfWork(), Times.Once);
             _uow.Verify(u => u.TagRepository, Times.Once);
             _repo.Verify(r => r.GetAll(), Times.Once);
             Assert.AreEqual(expected, actual);

        }

        [Test]
        public void Exists_ReturnsZero_TagNotExist()
        {
            //Arrange
            string name = "pesik";
            int expected = 0;
            var listik = new List<Tag>
            {
                new Tag 
                {
                    Name = name + "failed",
                    Id = expected
                }
            }.AsQueryable<Tag>();
            _repo.Setup(r => r.GetAll()).Returns(listik);

            //Act
            int actual = _service.Exists(name);

            //Assert
            _factory.Verify(f => f.GetUnitOfWork(), Times.Once);
            _uow.Verify(u => u.TagRepository, Times.Once);
            _repo.Verify(r => r.GetAll(), Times.Once);
            Assert.AreEqual(expected, actual);
            

        }

        //[Test]
        //public void Add_ReturnsTagId()
        //{
        //    //Arrange
        //    _repo.Setup(r => r.Add(It.IsAny<Tag>())).Verifiable();
        //    _uow.Setup(u => u.Save()).Verifiable();
        //    int expected = 8;
        //    Tag tag = new Tag { Id = expected };

        //    //Act
        //    int actual = _service.Add(tag);

        //    //Assert
        //    _uow.VerifyAll();
        //    _factory.Verify(f => f.GetUnitOfWork(), Times.Once);
        //    _uow.Verify(u => u.TagRepository, Times.Once);
        //    Assert.AreEqual(expected, actual);
        //}
    }
}
