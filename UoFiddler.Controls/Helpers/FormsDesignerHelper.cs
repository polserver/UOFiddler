using System.Windows.Forms;

namespace UoFiddler.Controls.Helpers
{
    public static class FormsDesignerHelper
    {
        public static bool IsInDesignMode()
        {
            return Application.ProductName.Contains("Visual Studio");
            //if (Application.ExecutablePath.IndexOf("devenv.exe", StringComparison.OrdinalIgnoreCase) > -1)
            //{
            //    return true;
            //}
            //return false;
        }
    }
}
