using System;
using System.Collections.Generic;
using LibGit2Sharp;

namespace Elders.Pandora.Api
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
            this.email = ApplicationConfiguration.Get("GitEmail");
            this.username = ApplicationConfiguration.Get("GitUsername");
            this.password = ApplicationConfiguration.Get("GitPassword");
            this.repo = new Repository(workingDir);
        }

        public static void Clone(string sourceUrl, string workingDir)
        {
            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = false;
            cloneOptions.Checkout = true;
            cloneOptions.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler((a, b, c) => new UsernamePasswordCredentials()
            {
                Username = ApplicationConfiguration.Get("GitUsername"),
                Password = ApplicationConfiguration.Get("GitPassword")
            });
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
            repo.Commit(message, new Signature(this.username, this.email, DateTimeOffset.UtcNow), new Signature(username, email, DateTimeOffset.UtcNow));
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

        public void Pull()
        {
            var pullOptions = new PullOptions();

            pullOptions.FetchOptions.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(
                (_url, _user, _cred) => new UsernamePasswordCredentials()
                {
                    Username = this.username,
                    Password = this.password
                });

            repo.Network.Pull(new Signature(this.username, this.email, DateTimeOffset.UtcNow), pullOptions);
        }
    }
}