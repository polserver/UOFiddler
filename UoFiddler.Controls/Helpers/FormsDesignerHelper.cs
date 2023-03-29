using System.Windows.Forms;

namespace UoFiddler.Controls.Helpers
{
    public static class FormsDesignerHelper
    {
        public static bool IsInDesignMode()
        {
            return Application.ProductName.Contains("Visual Studio");
        }
    }
}
