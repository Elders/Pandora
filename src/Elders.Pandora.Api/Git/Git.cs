using LibGit2Sharp;
using System;
using System.Collections.Generic;

namespace Elders.Pandora.Api
{
    public class Git
    {
        private readonly Repository repo;
        private readonly string workingDir;
        private readonly string email;
        private readonly string username;
        private readonly string password;

        public Git(string workingDir, string email, string username, string password)
        {
            this.workingDir = workingDir;
            this.email = email;
            this.username = username;
            this.password = password;
            this.repo = new Repository(workingDir);
        }

        public static void Clone(string sourceUrl, string workingDir)
        {
            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = false;
            cloneOptions.Checkout = true;
            Repository.Clone(sourceUrl, workingDir, cloneOptions);
        }

        public void Commit(IEnumerable<string> files, string message)
        {
            repo.Stage(files);
            repo.Commit(message, new Signature(username, email, DateTimeOffset.Now), new Signature(username, email, DateTimeOffset.Now));
        }

        public void Push()
        {
            var pushOptions = new PushOptions();

            pushOptions.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(
                (_url, _user, _cred) => new UsernamePasswordCredentials()
                {
                    Username = username,
                    Password = password
                });

            repo.Network.Push(repo.Branches["master"], pushOptions);
        }

        private void Go()
        {
            var cloneOptions = new CloneOptions();
            cloneOptions.IsBare = false;
            cloneOptions.Checkout = true;

            var sourceUrl = "https://github.com/ethno2405/Samodiva_old.git";
            var workingDir = @"D:\Projects\Test";

            var r = Repository.Clone(sourceUrl, workingDir, cloneOptions);

            using (var repo = new Repository(workingDir))
            {
                var localBranch = repo.CreateBranch("test-branch", new Signature("Sinstraliz", "blagovest.vp@gmail.com", DateTimeOffset.Now));

                Remote remote = repo.Network.Remotes["origin"];

                repo.Branches.Update(localBranch,
                    b => b.Remote = remote.Name,
                    b => b.UpstreamBranch = localBranch.CanonicalName);

                repo.Checkout(localBranch);


                var pushOptions = new PushOptions();

                pushOptions.CredentialsProvider = new LibGit2Sharp.Handlers.CredentialsHandler(
                    (_url, _user, _cred) => new UsernamePasswordCredentials()
                {
                    Username = "Sinstraliz",
                    Password = "pr0st0passa"
                });

                repo.Network.Push(repo.Branches["test-branch"], pushOptions);
            }
        }
    }
}