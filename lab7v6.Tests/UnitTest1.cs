using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using lab7v6;

namespace lab7v6.Tests
{
    public class InMemoryRepositoryTests
    {
        private InMemoryRepository<Employee> CreateRepository()
        {
            return new InMemoryRepository<Employee>();
        }

       
        [Fact]
        public void Add_ValidItem_ShouldAddItemToRepository()
        {
            var repo = CreateRepository();
            var emp = new Employee(1, "Тарас", "Шевченко", "Team Lead");

            repo.Add(emp);

            var result = repo.GetById(1);
            Assert.NotNull(result);
            Assert.Equal("Тарас", result.FirstName);
            Assert.Single(repo.GetAll());
        }

        [Fact]
        public void Add_DuplicateId_ShouldThrowInvalidOperationException()
        {
            var repo = CreateRepository();
            repo.Add(new Employee(1, "Ольга", "Бойко", "Designer"));

            Assert.Throws<InvalidOperationException>(() =>
                repo.Add(new Employee(1, "Павло", "Кравченко", "DevOps")));
        }

        [Fact]
        public void Add_NullItem_ShouldThrowArgumentNullException()
        {
            var repo = CreateRepository();
            Assert.Throws<ArgumentNullException>(() => repo.Add(null!));
        }

       
        [Fact]
        public void GetById_ExistingId_ShouldReturnCorrectItem()
        {
            var repo = CreateRepository();
            repo.Add(new Employee(10, "Андрій", "Мельник", "Frontend Developer"));

            var result = repo.GetById(10);

            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Андрій", result.FirstName);
        }

        [Fact]
        public void GetById_NonExistingId_ShouldReturnNull()
        {
            var repo = CreateRepository();
            var result = repo.GetById(999);
            Assert.Null(result);
        }

   
        [Fact]
        public void GetAll_WhenEmpty_ShouldReturnEmptyCollection()
        {
            var repo = CreateRepository();
            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void GetAll_MultipleItems_ShouldReturnAllItems()
        {
            var repo = CreateRepository();
            repo.Add(new Employee(1, "Олена", "Ткач", "HR Manager"));
            repo.Add(new Employee(2, "Дмитро", "Сергієнко", "Backend Developer"));

            Assert.Equal(2, repo.GetAll().Count());
        }

  
        [Fact]
        public void Update_ExistingItem_ShouldUpdatePropertiesSuccessfully()
        {
            var repo = CreateRepository();
            repo.Add(new Employee(1, "Артем", "Мороз", "Junior Developer"));

            var updatedEmp = new Employee(1, "Артем", "Мороз", "Middle Developer");
            repo.Update(updatedEmp);

            var result = repo.GetById(1);
            Assert.NotNull(result);
            Assert.Equal("Middle Developer", result.Position);
        }

        [Fact]
        public void Update_NonExistingItem_ShouldThrowKeyNotFoundException()
        {
            var repo = CreateRepository();
            var nonExistingEmp = new Employee(99, "Неіснуючий", "Юзер", "None");

            Assert.Throws<KeyNotFoundException>(() => repo.Update(nonExistingEmp));
        }

        // --- ТЕСТИ МЕТОДУ Delete ---
        [Fact]
        public void Delete_ExistingItem_ShouldRemoveItemFromRepository()
        {
            var repo = CreateRepository();
            repo.Add(new Employee(1, "Вікторія", "Лисенко", "Business Analyst"));

            repo.Delete(1);

            Assert.Null(repo.GetById(1));
            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void Delete_NonExistingItem_ShouldThrowKeyNotFoundException()
        {
            var repo = CreateRepository();
            Assert.Throws<KeyNotFoundException>(() => repo.Delete(999));
        }
    }
}