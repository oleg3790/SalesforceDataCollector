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
using System.Threading;
using System.Threading.Tasks;

namespace SalesforceDataCollector.Test
{
    [TestClass]
    public class AccountServiceFixture
    {
        [TestMethod]
        public async void Adds_New_Accounts()
        {
            // Arrange
            var newAccounts = new List<Account>()
            {
                new Account { Id = "1", AccountNumber = "Acct1", LastModifiedDate = DateTime.Now, Name = "Test", IsDeleted = true },
                new Account { Id = "3", AccountNumber = "Acct3", LastModifiedDate = DateTime.Now, Name = "Test 3", IsDeleted = true }
            };

            var existingAccounts = new Mock<DbSet<AccountDataModel>>();
            existingAccounts.As<IEnumerable<AccountDataModel>>().Setup(x => x).Returns(
                new List<AccountDataModel>()
                {
                    new AccountDataModel { Id = "1", Created = DateTime.Now, AccountNumber = "Acct1", LastModified = DateTime.Now, Name = "Test", IsDeleted = false },
                    new AccountDataModel { Id = "2", Created = DateTime.Now, AccountNumber = "Acct2", LastModified = DateTime.Now, Name = "Test 2", IsDeleted = false }
                }
            );

            var dbContextMock = new Mock<AccountContext>();
            dbContextMock.Setup(c => c.AddRangeAsync(It.IsAny<IEnumerable<Account>>(), false)).Returns(Task.CompletedTask);
            dbContextMock.SetupGet(c => c.Accounts).Returns(existingAccounts.Object);
            dbContextMock.Setup(c => c.SaveChangesAsync(true, CancellationToken.None)).Returns(new Mock<Task<int>>().Object);

            var service = BuildService(dbContextMock);

            // Act
            var acctsAdded = await service.AddNewAccountsAsync(newAccounts);

            // Assert
            acctsAdded.Should().Be(1);
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
