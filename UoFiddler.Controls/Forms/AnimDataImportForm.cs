/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimDataImportForm : Form
    {
        public AnimDataImportForm()
        {
            InitializeComponent();
            comboBoxUpsertAction.SelectedItem = "skip";
        }
    }
}
