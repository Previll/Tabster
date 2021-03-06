﻿#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Tabster.Core.Types;
using Tabster.Properties;
using Tabster.Utilities;

#endregion

namespace Tabster.Forms
{
    internal partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();

            lblVersion.Text = string.Format("{0} {1}", Resources.Version, TabsterEnvironment.GetVersion().ToString(TabsterVersionFormatFlags.BuildString | TabsterVersionFormatFlags.CommitShort | TabsterVersionFormatFlags.Truncated));
            lblVersion.LinkArea = TabsterEnvironment.GetVersion().Commit != null ? new LinkArea(lblVersion.Text.Length - TabsterEnvironment.GetVersion().Commit.ToShorthandString().Length, TabsterEnvironment.GetVersion().Commit.ToShorthandString().Length) : new LinkArea(0, 0);

            lblCopyright.Text = BrandingUtilities.GetCopyrightString(Assembly.GetExecutingAssembly());
            txtLicense.Text = Resources.ApplicationLicense;
            txtFontLicense.Text = MonoUtilities.ReadFileText(new[] {Application.StartupPath, "Resources", "SourceCodePro", "SIL OPEN FONT LICENSE.txt"}.Aggregate(Path.Combine));

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            foreach (var pluginHost in Program.GetPluginManager().GetPluginHosts())
            {
                if (pluginHost.Plugin.Guid != Guid.Empty && pluginHost.Enabled)
                {
                    var lvi = new ListViewItem {Text = pluginHost.Plugin.DisplayName ?? Resources.NotAvailableAbbreviation};

                    lvi.SubItems.Add(pluginHost.Plugin.Version != null ? pluginHost.Plugin.Version.ToString() : Resources.NotAvailableAbbreviation);
                    lvi.SubItems.Add(pluginHost.Plugin.Author ?? Resources.NotAvailableAbbreviation);
                    lvi.SubItems.Add(pluginHost.FileInfo.FullName);

                    listPlugins.Items.Add(lvi);
                }
            }

            if (listPlugins.Items.Count > 0)
                listPlugins.AutoResizeColumn(listPlugins.Columns.Count - 1, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.fatcow.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.iconshock.com/");
        }

        private void btnHomepage_Click(object sender, EventArgs e)
        {
            Process.Start("http://tabster.org");
        }

        private void lblVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(string.Format("https://github.com/GetTabster/Tabster/commit/{0}", TabsterEnvironment.GetVersion().Commit));
        }
    }
}