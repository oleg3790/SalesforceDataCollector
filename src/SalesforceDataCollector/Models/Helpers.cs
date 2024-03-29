﻿using SalesforceDataCollector.Data.Models;
using System;

namespace SalesforceDataCollector.Models
{
    public static class Helpers
    {
        // Cannot use a constructor as EF will complain
        public static AccountDataModel ToDataModel(this Account account)
        {
            return new AccountDataModel
            {
                Id = account.Id,
                Created = DateTime.Now,
                LastModified = account.LastModifiedDate,
                AccountNumber = account.AccountNumber,
                Name = account.Name,
                IsDeleted = account.IsDeleted
            };
        }
    }
}
