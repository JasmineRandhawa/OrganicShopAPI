namespace OrganicShopAPI.DataTransferObjects
{
    public class OrderDto
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Address { get; set; }

        public ShoppingCartDto ShoppingCart { get; set; }

        public string DateCreated { get; set; }
    }
}