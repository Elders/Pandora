using System.Web.Optimization;

namespace Elders.Pandora.UI.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-theme.css",
                    "~/Content/font-awesome.css",
                    "~/Content/sb-admin.css"));

            bundles.Add(new ScriptBundle("~/Scripts").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/jquery-2.1.3.js",
                        "~/Scripts/jquery.metisMenu.js",
                        "~/Scripts/sb-admin.js"));
        }
    }
}