namespace Elders.Pandora.Api
{
    public static class Folders
    {
        public static string Main
        {
            get
            {
                return ApplicationConfiguration.Get("pandora_folder");
            }
        }

        public static string Users
        {
            get
            {
                return ApplicationConfiguration.Get("pandora_users_folder");
            }
        }

        public static string Projects
        {
            get
            {
                return ApplicationConfiguration.Get("pandora_projects_folder");
            }
        }
    }
}
