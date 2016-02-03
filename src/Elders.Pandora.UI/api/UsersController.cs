using Elders.Pandora.UI.Common;
using Elders.Pandora.UI.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;

namespace Elders.Pandora.UI.api
{
    [Authorize]
    public class UsersController : ApiController
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(UsersController));

        [HttpGet]
        public IEnumerable<User> Get()
        {
            var users = Directory.GetFiles(Folders.Users, "*.json", SearchOption.AllDirectories);

            foreach (var user in users)
            {
                User userObject = null;

                try
                {
                    userObject = JsonConvert.DeserializeObject<User>(System.IO.File.ReadAllText(user));
                }
                catch (Exception ex)
                {
                    log.Error(ex);

                    continue;
                }

                if (user != null)
                    yield return userObject;
            }
        }

        public User Get(string id)
        {
            var userFilePath = Path.Combine(Folders.Users, id.ToString(), id.ToString() + ".json");

            if (System.IO.File.Exists(userFilePath))
            {
                var user = JsonConvert.DeserializeObject<User>(System.IO.File.ReadAllText(userFilePath));

                return user;
            }

            return null;
        }

        public void Post(string id, [FromBody]User user)
        {
            try
            {
                var workingDir = Path.Combine(Folders.Users, id.ToString());

                var userFilePath = Path.Combine(workingDir, id.ToString() + ".json");

                if (!System.IO.File.Exists(userFilePath))
                {
                    Directory.CreateDirectory(Path.Combine(workingDir));

                    var serializedUser = JsonConvert.SerializeObject(user, Formatting.Indented);

                    System.IO.File.WriteAllText(userFilePath, serializedUser);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }

        public void Put(string id, [FromBody]User user)
        {
            try
            {
                var workingDir = Path.Combine(Folders.Users, id.ToString());

                var userFilePath = Path.Combine(workingDir, id.ToString() + ".json");

                if (System.IO.File.Exists(userFilePath))
                {
                    var serializedUser = JsonConvert.SerializeObject(user, Formatting.Indented);

                    System.IO.File.WriteAllText(userFilePath, serializedUser);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                throw;
            }
        }
    }
}