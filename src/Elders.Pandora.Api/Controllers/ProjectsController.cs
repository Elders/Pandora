using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Elders.Pandora.Box;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Routing;
using Newtonsoft.Json;
using RestSharp.Extensions.MonoHttp;

namespace Elders.Pandora.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class ProjectsController : Controller
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ProjectsController));

        [HttpGet]
        public List<string> Get()
        {
            var projects = Directory.GetDirectories(Folders.Projects);

            return projects.Where(x => x != ".git").ToList();
        }

        [HttpPost("{projectName}/{gitUrlBlob}/{gitUsernameBlob}/{gitPasswordBlob}")]
        public void Post(string projectName, byte[] gitUrlBlob, byte[] gitUsernameBlob, byte[] gitPasswordBlob)
        {
            var gitUrl = HttpUtility.UrlDecode(gitUrlBlob, Encoding.UTF8);
            var gitUsername = Encoding.UTF8.GetString(gitUsernameBlob);
            var gitPassword = Encoding.UTF8.GetString(gitPasswordBlob);
            var workingDir = Path.Combine(Folders.Projects, projectName);

            if (Directory.Exists(workingDir) == false)
            {
                Directory.CreateDirectory(workingDir);

                try
                {
                    Git.Clone(gitUrl, workingDir, gitUsername, gitPassword);
                }
                catch (Exception ex)
                {
                    log.Fatal(ex);
                    throw;
                }
            }
        }

        [HttpDelete("{projectName}")]
        public void Delete(string projectName)
        {
            var workingDir = Path.Combine(Folders.Projects, projectName);

            var project = Directory.Exists(workingDir);

            if (project)
            {
                Directory.Delete(workingDir);
            }
        }

        [HttpPost("{projectName}")]
        public void Update(string projectName)
        {
            try
            {
                var projectPath = Path.Combine(Folders.Projects, projectName);

                var git = new Git(projectPath);
                git.Pull();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }
    }
}
