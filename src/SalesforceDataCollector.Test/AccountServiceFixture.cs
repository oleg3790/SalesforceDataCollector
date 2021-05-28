using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SalesforceDataCollector.Data;
using SalesforceDataCollector.Data.Models;
using SalesforceDataCollector.Models;
using SalesforceDataCollector.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Test
{
    [TestClass]
    public class AccountServiceFixture
    {
        [TestMethod]
        public async Task Adds_New_Accounts()
        {
            // Arrange
            var newAccounts = new List<Account>()
            {
                new Account { Id = "1", AccountNumber = "Acct1", LastModifiedDate = DateTime.Now, Name = "Test", IsDeleted = true },
                new Account { Id = "3", AccountNumber = "Acct3", LastModifiedDate = DateTime.Now, Name = "Test 3", IsDeleted = true }
            };

            var existingAccounts = new List<AccountDataModel>()
            {
                new AccountDataModel { Id = "1", Created = DateTime.Now, AccountNumber = "Acct1", LastModified = DateTime.Now, Name = "Test", IsDeleted = false },
                new AccountDataModel { Id = "2", Created = DateTime.Now, AccountNumber = "Acct2", LastModified = DateTime.Now, Name = "Test 2", IsDeleted = false }
            }.AsQueryable();

            var existingAccountsMock = GetAccountDataSetMock(existingAccounts);

            var dbContextMock = new Mock<AccountContext>();
            dbContextMock.Setup(c => c.Accounts).Returns(existingAccountsMock.Object);
            dbContextMock.Setup(c => c.AddRangeAsync(It.IsAny<IEnumerable<Account>>(), false)).Returns(Task.CompletedTask);
            dbContextMock.Setup(c => c.SaveChangesAsync(true, CancellationToken.None)).Returns((Task<int>)null);

            var service = BuildService(dbContextMock);

            // Act
            var acctsAdded = await service.AddNewAccountsAsync(newAccounts);

            // Assert
            acctsAdded.Should().Be(1);
        }

        [TestMethod]
        public async Task Updates_Modified_Accounts()
        {
            // Arrange
            var syncDate = DateTime.Now;

            var newAccounts = new List<Account>()
            {
                new Account { Id = "1", AccountNumber = "Updated Acct Num", LastModifiedDate = DateTime.Now.AddDays(1), Name = "Test", IsDeleted = true },
                new Account { Id = "3", AccountNumber = "Acct3", LastModifiedDate = syncDate, Name = "Test 3", IsDeleted = true }
            };

            var existingAccounts = new List<AccountDataModel>()
            {
                new AccountDataModel { Id = "1", Created = DateTime.Now, AccountNumber = "Acct1", LastModified = syncDate, Name = "Test", IsDeleted = false },
                new AccountDataModel { Id = "2", Created = DateTime.Now, AccountNumber = "Acct2", LastModified = syncDate, Name = "Test 2", IsDeleted = false }
            }.AsQueryable();

            var existingAccountsMock = GetAccountDataSetMock(existingAccounts);

            var dbContextMock = new Mock<AccountContext>();
            dbContextMock.Setup(c => c.Accounts).Returns(existingAccountsMock.Object);
            dbContextMock.Setup(c => c.Accounts.FindAsync(It.IsAny<object>())).ReturnsAsync(new AccountDataModel());
            dbContextMock.Setup(c => c.SaveChangesAsync(true, CancellationToken.None)).Returns((Task<int>)null);

            var service = BuildService(dbContextMock);

            // Act
            var acctsAdded = await service.UpdateModifiedAccountsAsync(newAccounts);

            // Assert
            acctsAdded.Should().Be(1);
        }

        [TestMethod]
        public async Task Remove_Missing_Accounts()
        {
            // Arrange
            var syncDate = DateTime.Now;

            var newAccounts = new List<Account>();

            var existingAccounts = new List<AccountDataModel>()
            {
                new AccountDataModel { Id = "1", Created = DateTime.Now, AccountNumber = "Acct1", LastModified = syncDate, Name = "Test", IsDeleted = false },
                new AccountDataModel { Id = "2", Created = DateTime.Now, AccountNumber = "Acct2", LastModified = syncDate, Name = "Test 2", IsDeleted = false }
            }.AsQueryable();

            var existingAccountsMock = GetAccountDataSetMock(existingAccounts);

            var dbContextMock = new Mock<AccountContext>();
            dbContextMock.Setup(c => c.Accounts).Returns(existingAccountsMock.Object);
            dbContextMock.Setup(c => c.RemoveRange(It.IsAny<object>()));
            dbContextMock.Setup(c => c.SaveChangesAsync(true, CancellationToken.None)).Returns((Task<int>)null);

            var service = BuildService(dbContextMock);

            // Act
            var acctsAdded = await service.RemoveMissingAccountsAsync(newAccounts);

            // Assert
            acctsAdded.Should().Be(2);
        }

        private Mock<DbSet<AccountDataModel>> GetAccountDataSetMock(IQueryable<AccountDataModel> accounts)
        {
            var existingAccountsMock = new Mock<DbSet<AccountDataModel>>();
            existingAccountsMock.As<IDbAsyncEnumerable<AccountDataModel>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<AccountDataModel>(accounts.GetEnumerator()));

            existingAccountsMock.As<IQueryable<AccountDataModel>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<AccountDataModel>(accounts.Provider));

            existingAccountsMock.As<IQueryable<AccountDataModel>>().Setup(m => m.Expression).Returns(accounts.Expression);
            existingAccountsMock.As<IQueryable<AccountDataModel>>().Setup(m => m.ElementType).Returns(accounts.ElementType);
            existingAccountsMock.As<IQueryable<AccountDataModel>>().Setup(m => m.GetEnumerator()).Returns(accounts.GetEnumerator());

            return existingAccountsMock;
        }

        private AccountService BuildService(Mock<AccountContext> dbContextMock)
        {
            return new AccountService
            (
                new Mock<ILogger<AccountService>>().Object,
                dbContextMock.Object
            );
        }
    }
}
