namespace Elders.Pandora.Box
{
    public static class NameBuilder
    {
        public static string GetFileName(string applicationName, string clusterName, string machineName)
        {
            string theName = (applicationName + "@@" + clusterName + "^" + machineName);
            return theName;
        }

        public static string GetSettingName(string applicationName, string clusterName, string machineName, string settingKeyName)
        {
            string theName = applicationName + "@@" + clusterName + "^" + machineName + "~~" + settingKeyName;
            return theName;
        }
    }
}
