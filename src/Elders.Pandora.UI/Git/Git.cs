using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Elders.Pandora.UI
{
    public class Git
    {
        private readonly Repository repo;
        private readonly string workingDir;
        private readonly string email;
        private readonly string username;
        private readonly string password;

        public Git(string workingDir)
        {
            this.workingDir = workingDir;
            this.email = ConfigurationManager.AppSettings["GitEmail"];
            this.username = ConfigurationManager.AppSettings["GitUsername"];
            this.password = ConfigurationManager.AppSettings["GitPassword"];
            this.repo = new Repository(workingDir);
        }

        public static void Clone(string sourceUrl, string workingDir)
        {
            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = false;
            cloneOptions.Checkout = true;
            Repository.Clone(sourceUrl, workingDir, cloneOptions);
        }

        public void Stage(IEnumerable<string> stageFiles)
        {
            repo.Stage(stageFiles);
        }

        public void Remove(IEnumerable<string> removeFiles)
        {
            repo.Remove(removeFiles);
        }

        public void Commit(string message, string username, string email)
        {
            repo.Commit(message, new Signature(this.username, this.email, DateTimeOffset.Now), new Signature(username, email, DateTimeOffset.Now));
        }

        public void Push()
        {
            var pushOptions = new PushOptions();

            pushOptions.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(
                (_url, _user, _cred) => new UsernamePasswordCredentials()
                {
                    Username = this.username,
                    Password = this.password
                });

            repo.Network.Push(repo.Branches["master"], pushOptions);
        }
    }
}