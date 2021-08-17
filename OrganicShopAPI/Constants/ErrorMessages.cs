namespace OrganicShopAPI.Constants
{
    public static class ErrorMessages
    {
        public const string LessThanEqualToZero = " should be greater than 0.";
        public const string EmptyOrWhiteSpace = " cannot be empty string or white space.";
        public const string InvalidURL = " is not a valid URL.";
        public const string InvalidEmail = " is not a valid Email Address.";
        public const string DoesNotExist = " does not exist.";
        public const string ShoppingCartMissing = " shopping cart missing.";
        public const string ShoppingCartItemsMissing = " shopping cart items missing.";
        public const string NullParameter = " parameter cannot be null.";
        public const string PendingItemsInCart = " cannot be decativated since User has some items in the shopping cart.";
        public const string NoRecordFound = "No matching record found";
    }
}
