using System;
using System.Collections.Generic;
using System.Text;

namespace Bar.Domain.Errors
{
    public static class Errors
    {
        public static class Generic
        {
            public const string NullCommand = "You must provide a non-null command.";

            public const string NullQuery = "You must provide a non-null request.";
        }

        public static class Tab
        {
            public static string AlreadyExists(Guid tabId) => $"Tab {tabId} already exists.";

            public static string NotOpen(Guid tabId) => $"Tab {tabId} is not open.";

            public static string NotFound(Guid tabId) => $"No tab with an id of {tabId} was found.";

            public const string InvalidClientName = "You must provide a valid client name.";

            public const string TriedToPayLessThanTheBill = "You cannot pay less than what you've consumed.";

            public const string TriedToServeUnorderedBeverages = "You cannot serve beverages that haven't been ordered.";

            public const string InvalidId = "You must provide a valid tab id.";
        }

        public static class Beverage
        {
            public const string MustAddAtLeastOneBeverage = "You must include at least one beverage.";

            public static string AlreadyExist(params int[] menuNumbers) => $"Beverages with menu numbers {string.Join(", ", menuNumbers)} already exist.";

            public static string NotFound(int menuNumber) => $"No beverage was found for menu number '{menuNumber}'.";
        }
    }
}
