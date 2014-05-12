using Elders.Pandora.IdentityAndAccess.UserConfiguration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elders.Pandora.IdentityAndAccess.IdentityManager.ViewModels
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public SecurityAccess Access { get; set; }
    }

    public static class UserBuilder
    {
        public static User ToUserViewModel(this PandoraUser usr)
        {
            if (usr == null)
                return null;

            var user = new User();
            user.Id = Guid.Parse(usr.Id);
            user.Email = usr.Email;

            var firstName = usr.Claims.SingleOrDefault(x => x.ClaimType == "given_name");

            if (firstName != null && !string.IsNullOrWhiteSpace(firstName.ClaimValue))
            {
                user.FirstName = firstName.ClaimValue;
            }

            var lastName = usr.Claims.SingleOrDefault(x => x.ClaimType == "family_name");

            if (lastName != null && !string.IsNullOrWhiteSpace(lastName.ClaimValue))
            {
                user.LastName = lastName.ClaimValue;
            }

            var fullname = usr.Claims.SingleOrDefault(x => x.ClaimType == "name");

            if (fullname == null || string.IsNullOrWhiteSpace(fullname.ClaimValue))
            {
                user.FullName = user.FirstName + " " + user.LastName;
            }
            else
                user.FullName = fullname.ClaimValue;

            var securityAccess = usr.Claims.SingleOrDefault(x => x.ClaimType == "SecurityAccess");

            if (securityAccess == null || string.IsNullOrWhiteSpace(securityAccess.ClaimValue))
                user.Access = new SecurityAccess();
            else
            {
                user.Access = JsonConvert.DeserializeObject<SecurityAccess>(securityAccess.ClaimValue);
            }

            return user;
        }
    }

    public class SecurityAccess
    {
        public SecurityAccess()
        {
            this.Projects = new List<Project>();
        }

        public List<Project> Projects { get; set; }

        public override string ToString()
        {
            return "SecurityAccess";
        }
    }

    public class Project
    {
        public Project()
        {
            this.Applications = new List<Application>();
        }

        public string Name { get; set; }

        public List<Application> Applications { get; set; }
    }

    public class Application
    {
        public Application()
        {
            this.Clusters = new List<Cluster>();
        }

        public string Name { get; set; }

        public List<Cluster> Clusters { get; set; }

        public Access Access { get; set; }
    }

    public class Cluster
    {
        public string Name { get; set; }

        public Access Access { get; set; }
    }

    [Flags]
    public enum Access
    {
        WriteAccess = 2,
        ReadAcccess = 4
    }

    public static class AccessExtensions
    {
        public static bool HasAccess(this Access slef, Access check)
        {
            return (slef & check) == check;
        }
    }
}