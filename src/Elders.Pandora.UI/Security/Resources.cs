using System.Collections.Generic;

namespace Elders.Pandora.UI.Security
{
    public static class Resources
    {
        public const string Admin = "Admin";

        public static class Actions
        {
            public const string Read = "Read";
            public const string Manage = "Manage";
        }
    }

    public static class Roles
    {
        public const string SuperAdmin = "superAdmin";
        public const string Admin = "admin";
        public const string Any = "*";
    }

    public class AuthorizationRules
    {
        public static List<Resource> ResourceRules = new List<Resource>()
            {
                new Resource(Resources.Admin,
                    new ResourceAction(Resources.Actions.Read,Roles.Admin, Roles.SuperAdmin),
                    new ResourceAction(Resources.Actions.Manage, Roles.SuperAdmin))
            };
    }
}