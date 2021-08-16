using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicShopAPI.Utility
{
    public static class Routes
    {
        public const string Controller = "Api/[controller]";
        public const string All = "All";
        public const string Add = "Add";
        public const string Update = "Update";
        public const string Deactivate = "Deactivate/{Id}";
        public const string Activate = "Activate/{Id}";
        public const string Id = "{Id}";
        public const string UserId = "User/{Id}";
    }
}
