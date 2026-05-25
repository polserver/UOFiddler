/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Windows.Forms;

namespace UoFiddler.Controls.Helpers
{
    public static class TabPageNavigator
    {
        /// <summary>
        /// Walks the control's parent chain and, if it sits inside a TabControl,
        /// activates the owning TabPage. No-op when the control is not hosted in
        /// a TabPage (e.g., undocked into a standalone form, or used outside the
        /// main TabPanel).
        /// </summary>
        public static void ActivateOwningTabPage(Control control)
        {
            Control current = control;
            while (current != null)
            {
                if (current.Parent is TabControl outerTabControl && current is TabPage outerTabPage)
                {
                    if (outerTabControl.SelectedTab != outerTabPage)
                    {
                        outerTabControl.SelectedTab = outerTabPage;
                    }
                    return;
                }
                current = current.Parent;
            }
        }
    }
}
