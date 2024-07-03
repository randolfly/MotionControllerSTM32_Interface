using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace MotionInterface.Util;

public static class FolderPicker
{
    public static string DisplayFolderPicker() {
        string selectedFolder = string.Empty;

        FolderBrowserDialog folderBrowserDialog = new ();
        DialogResult result = folderBrowserDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            selectedFolder = folderBrowserDialog.SelectedPath;
        }

        return selectedFolder;
    }
}
