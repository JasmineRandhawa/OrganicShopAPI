using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicShopAPI.Utility
{
    public static class ErrorMessages
    {
        public const string LessThenZero = " should be greater than 0.";
        public const string EmptyOrWhiteSpace = " cannot be empty string or white space.";
        public const string InvalidURL = " is not a valid URL.";
        public const string DoesNotExist = " does not exist.";
        public const string NullParameter = " parameter cannot be null.";
    }
}
