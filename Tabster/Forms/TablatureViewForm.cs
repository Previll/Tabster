﻿#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tabster.Controls;
using Tabster.Core.Types;
using Tabster.Data;
using Tabster.Properties;
using Tabster.Utilities;
using Tabster.WinForms;
using ToolStripRenderer = Tabster.Controls.ToolStripRenderer;

#endregion

namespace Tabster.Forms
{
    internal partial class TablatureViewForm : Form
    {
        #region Delegates

        public delegate void TabHandler(object sender, ITablatureFile file);

        #endregion

        private static TablatureViewForm _instance;
        private readonly Form _owner;
        private readonly List<TabInstance> _tabInstances = new List<TabInstance>();
        private bool _isFullscreen;
        private FormBorderStyle _previousBorderStyle;
        private FormWindowState _previousWindowState;

        private TablatureViewForm()
        {
            InitializeComponent();

            controlsToolStrip.Renderer = new ToolStripRenderer();
        }

        private TablatureViewForm(Form owner) : this()
        {
            _owner = owner;
        }

        public static TablatureViewForm GetInstance(Form owner)
        {
            if (_instance == null || _instance.IsDisposed)
                _instance = new TablatureViewForm(owner);

            return _instance;
        }

        public event TabHandler TabClosed;
        public event TabHandler TabOpened;

        private TabInstance CreateTabInstance(ITablatureFile file, FileInfo fileInfo)
        {
            var editor = new BasicTablatureTextEditor {Dock = DockStyle.Fill, ReadOnly = false};
            var instance = new TabInstance(file, fileInfo, editor);

            _tabInstances.Add(instance);
            tabControl1.TabPages.Add(instance.Page);

            editor.ContentsModified += editor_ContentsModified;
            editor.TablatureLoaded += editor_TablatureLoaded;
            editor.LoadTablature(file);

            if (TabOpened != null)
                TabOpened(this, file);

            return instance;
        }

        private void editor_ContentsModified(object sender, EventArgs e)
        {
            UpdateInstanceControls(GetInstance((BasicTablatureTextEditor) sender));
        }

        private void editor_TablatureLoaded(object sender, EventArgs e)
        {
            UpdateInstanceControls(GetInstance((BasicTablatureTextEditor) sender));
        }

        private void SelectTabInstance(TabInstance instance)
        {
            tabControl1.SelectedTab = instance.Page;
            instance.Editor.Focus(false);
        }

        private TabInstance GetTabInstance(FileInfo fileInfo)
        {
            return _tabInstances.Find(x => x.FileInfo.FullName.Equals(fileInfo.FullName));
        }

        private TabInstance GetSelectedInstance()
        {
            var selectedTab = tabControl1.SelectedTab;
            return _tabInstances.Find(x => x.Page == selectedTab);
        }

        private void UpdateInstanceControls(TabInstance instance)
        {
            savebtn.Enabled = instance.Editor.Modified;
            instance.Page.ImageIndex = instance.Editor.Modified ? 1 : 0;
        }

        private bool CloseInstance(TabInstance instance, bool closeIfLast)
        {
            var saveBeforeClosing = true;

            if (instance.Modified)
            {
                var result = MessageBox.Show(string.Format(Resources.SaveChangesDialogCaption + Environment.NewLine + instance.File.ToFriendlyString(), Resources.SaveChangesDialogTitle), Resources.Save, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return false;
                if (result == DialogResult.No)
                    saveBeforeClosing = false;
            }

            if (saveBeforeClosing)
            {
                instance.File.Save(instance.FileInfo.FullName);
            }

            tabControl1.TabPages.Remove(instance.Page);
            _tabInstances.Remove(instance);

            if (TabClosed != null)
                TabClosed(this, instance.File);

            if (closeIfLast && tabControl1.TabPages.Count == 0)
            {
                Close();
            }

            return true;
        }

        private void PrintTab(object sender, EventArgs e)
        {
            var instance = GetSelectedInstance();

            if (instance != null)
                instance.Editor.Print();
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instance = GetSelectedInstance();

            if (instance != null)
                CloseInstance(instance, true);
        }

        private void TabbedViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            //remove in reverse order
            for (var i = _tabInstances.Count - 1; i > -1; i--)
            {
                var instance = _tabInstances[i];

                var result = CloseInstance(instance, false);

                if (!result)
                {
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void tabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                for (var i = tabControl1.TabPages.Count - 1; i >= 0; i--)
                {
                    if (tabControl1.GetTabRect(i).Contains(e.Location))
                    {
                        var tabPage = ((TabControl) sender).TabPages[i];

                        var match = _tabInstances.Find(x => x.Page == tabPage);

                        if (match != null)
                        {
                            CloseInstance(match, true);
                        }

                        break;
                    }
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                offToolStripMenuItem.PerformClick();
                Text = string.Format("{0} - {1}", Application.ProductName, tabControl1.SelectedTab.Text);

                savebtn.Enabled = GetSelectedInstance().Modified;
            }
        }

        private void ToggleFullscreen(object sender = null, EventArgs e = null)
        {
            if (_isFullscreen)
            {
                FormBorderStyle = _previousBorderStyle;
                WindowState = _previousWindowState;

                _isFullscreen = false;
                fullscreenbtn.Text = Resources.FullScreen;
            }

            else
            {
                _previousBorderStyle = FormBorderStyle;
                _previousWindowState = WindowState;

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                _isFullscreen = true;
                fullscreenbtn.Text = Resources.Restore;
            }
        }

        private void TabbedViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullscreen();
            }

            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.S)
                {
                    if (savebtn.Enabled)
                        savebtn.PerformClick();
                }

                if (e.KeyCode == Keys.P)
                {
                    printbtn.PerformClick();
                }
            }
        }

        private void SaveTab(object sender, EventArgs e)
        {
            var instance = GetSelectedInstance();

            if (instance != null)
            {
                instance.File.Contents = instance.Editor.Text;
                instance.File.Save(instance.FileInfo.FullName);
                instance.Modified = false;
                savebtn.Enabled = false;
            }
        }

        private TabInstance GetInstance(BasicTablatureTextEditor editor)
        {
            return _tabInstances.FirstOrDefault(instance => instance.Editor == editor);
        }

        #region Public Methods

        public bool IsFileOpen(FileInfo fileInfo)
        {
            return GetTabInstance(fileInfo) != null;
        }

        public void LoadTablature(ITablatureFile file, FileInfo fileInfo)
        {
            var instance = IsFileOpen(fileInfo) ? GetTabInstance(fileInfo) : CreateTabInstance(file, fileInfo);

            if (!Visible)
            {
                if (_owner != null)
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(_owner.Location.X + (_owner.Width - Width)/2,
                        _owner.Location.Y + (_owner.Height - Height)/2);

                    Show(_owner);
                }

                else
                {
                    Show();
                }
            }

            SelectTabInstance(instance);
        }

        #endregion
    }

    internal class TabInstance
    {
        public TabInstance(ITablatureFile file, FileInfo fileInfo, BasicTablatureTextEditor editor = null)
        {
            File = file;

            FileInfo = fileInfo;

            Page = new EllipsizedTabPage {Text = file.ToFriendlyString(), ToolTipText = FileInfo.FullName};

            Editor = editor ?? new BasicTablatureTextEditor {Dock = DockStyle.Fill};
            Editor.Font = TablatureFontManager.GetFont();

            Page.Controls.Add(Editor);

            Editor.LoadTablature(file);
        }

        public TabPage Page { get; private set; }
        public BasicTablatureTextEditor Editor { get; private set; }
        public ITablatureFile File { get; private set; }
        public FileInfo FileInfo { get; private set; }

        public bool Modified
        {
            get { return Editor.Modified; }
            set { Editor.Modified = value; }
        }
    }
}