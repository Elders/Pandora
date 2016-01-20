using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

namespace Elders.Pandora.UI.ViewModels
{
    public class User
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AvatarUrl { get; set; }

        public string Organization { get; set; }

        public SecurityAccess Access { get; set; }

        public static void GiveAccess(string projectName, string applicationName, string cluster, Access access)
        {
            var user = GetUser();

            UpdateUserAccess(user, projectName, applicationName, cluster, access);

            UpdateClaimsPrincipal(user.Access);
        }

        private static User GetUser()
        {
            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Users/" + ClaimsPrincipal.Current.Id();

            var restClient = new RestSharp.RestClient(url);

            var request = new RestSharp.RestRequest();
            request.Method = RestSharp.Method.GET;
            request.RequestFormat = RestSharp.DataFormat.Json;
            request.AddHeader("Content-Type", "application/json;charset=utf-8");
            request.AddHeader("Authorization", "Bearer " + ClaimsPrincipal.Current.Token());

            var result = restClient.Execute(request);

            return JsonConvert.DeserializeObject<User>(result.Content);
        }

        private static void UpdateUserAccess(User user, string projectName, string applicationName, string cluster, Access access)
        {
            user.Access.AddRule(new AccessRules
            {
                Project = projectName,
                Application = applicationName,
                Cluster = cluster,
                Access = access
            });

            var hostName = ApplicationConfiguration.Get("pandora_api_url");
            var url = hostName + "/api/Users/" + ClaimsPrincipal.Current.Id();

            var restClient = new RestSharp.RestClient(url);

            var editRequest = new RestSharp.RestRequest();
            editRequest.Method = RestSharp.Method.PUT;
            editRequest.RequestFormat = RestSharp.DataFormat.Json;
            editRequest.AddHeader("Content-Type", "application/json;charset=utf-8");
            editRequest.AddHeader("Authorization", "Bearer " + ClaimsPrincipal.Current.Token());

            editRequest.AddBody(user);

            var editResult = restClient.Execute(editRequest);
        }

        private static void UpdateClaimsPrincipal(SecurityAccess access)
        {
            var accessClaim = ClaimsPrincipal.Current.Identities.First().Claims.SingleOrDefault(x => x.Type == "SecurityAccess");

            if (accessClaim != null)
                ClaimsPrincipal.Current.Identities.First().RemoveClaim(accessClaim);

            ClaimsPrincipal.Current.Identities.First().AddClaim(new Claim("SecurityAccess", JsonConvert.SerializeObject(access)));
        }
    }

    public class AccessRules
    {
        public string Application { get; set; }

        public string Project { get; set; }

        public Access Access { get; set; }

        public string Cluster { get; set; }
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

        public void AddRule(AccessRules rule)
        {
            var project = this.Projects.SingleOrDefault(x => x.Name == rule.Project);

            if (project == null)
            {
                project = new Project() { Name = rule.Project };

                this.Projects.Add(project);
            }

            var application = project.Applications.SingleOrDefault(x => x.Name == rule.Application);

            if (application == null)
            {
                application = new Application() { Name = rule.Application };

                project.Applications.Add(application);
            }

            if (rule.Cluster == "Defaults")
            {
                if (rule.Access == Access.WriteAccess)
                    application.Access = Access.ReadAcccess | Access.WriteAccess;
                else
                    application.Access = Access.ReadAcccess;
            }
            else
            {
                var cluster = application.Clusters.SingleOrDefault(x => x.Name == rule.Cluster);

                if (cluster == null)
                {
                    cluster = new Cluster() { Name = rule.Cluster };

                    application.Clusters.Add(cluster);
                }

                if (rule.Access == Access.WriteAccess)
                    cluster.Access = Access.ReadAcccess | Access.WriteAccess;
                else
                    cluster.Access = Access.ReadAcccess;
            }
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
        public static bool HasAccess(this SecurityAccess self, string project, string application, string cluster, Access access)
        {
            if (self.Projects.Select(x => x.Name).Contains(project))
            {
                if (self.Projects.SingleOrDefault(x => x.Name == project).Applications.Select(x => x.Name).Contains(application))
                {
                    if (cluster == "Defaults")
                    {
                        return self.Projects.SingleOrDefault(x => x.Name == project).Applications.SingleOrDefault(x => x.Name == application).Access.HasAccess(access);
                    }
                    else if (self.Projects.SingleOrDefault(x => x.Name == project).Applications.SingleOrDefault(x => x.Name == application).Clusters.Select(x => x.Name).Contains(cluster))
                    {
                        var cl = self.Projects.SingleOrDefault(x => x.Name == project).Applications.SingleOrDefault(x => x.Name == application).Clusters.SingleOrDefault(x => x.Name == cluster);

                        return cl.Access.HasAccess(access);
                    }
                }
            }

            return false;
        }

        public static bool HasAccess(this Access self, Access check)
        {
            return (self & check) == check;
        }

        public static string Token(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
                return (self as ClaimsPrincipal).Claims.Where(x => x.Type == "at").FirstOrDefault().Value;
            else
                return string.Empty;
        }

        public static string FullName(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
            {
                var fullname = (self as ClaimsPrincipal).Claims.Where(x => x.Type == "name").FirstOrDefault();

                if (fullname != null && !string.IsNullOrWhiteSpace(fullname.Value))
                    return fullname.Value;
            }
            return self.Email();
        }

        public static string FirstName(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
            {
                var firstname = (self as ClaimsPrincipal).Claims.Where(x => x.Type == "given_name").FirstOrDefault();

                if (firstname != null && !string.IsNullOrWhiteSpace(firstname.Value))
                    return firstname.Value;
            }
            return string.Empty;
        }

        public static string LastName(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
            {
                var lastname = (self as ClaimsPrincipal).Claims.Where(x => x.Type == "family_name").FirstOrDefault();

                if (lastname != null && !string.IsNullOrWhiteSpace(lastname.Value))
                    return lastname.Value;
            }
            return string.Empty;
        }

        public static string Avatar(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
            {
                var fullname = (self as ClaimsPrincipal).Claims.Where(x => x.Type == "picture").FirstOrDefault();

                if (fullname != null && !string.IsNullOrWhiteSpace(fullname.Value))
                    return fullname.Value;
            }
            return self.Email();
        }

        public static string Email(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
            {
                var email = (self as ClaimsPrincipal).Claims.Where(x => x.Type == "email").FirstOrDefault();

                if (email != null)
                    return email.Value;
            }
            return string.Empty;
        }

        public static string Id(this IPrincipal self)
        {
            if (self is ClaimsPrincipal)
            {
                var id = (self as ClaimsPrincipal).Claims.Where(x => x.Type == "sub").FirstOrDefault();

                if (id != null)
                    return id.Value;
            }
            return string.Empty;
        }
    }
}