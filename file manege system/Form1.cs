using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace file_manege_system
{
    public partial class Form1 : Form
    {
        // --- 1. CONFIGURATION: Define your supported formats here ---
        private readonly HashSet<string> AllowedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif",
            ".webp", ".tiff", ".tif", ".ico", ".svg"
        };

        private readonly HashSet<string> AllowedVideos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".avi", ".mov", ".wmv", ".mkv",
            ".webm", ".m4v", ".flv", ".3gp"
        };

        // --- Data Variables ---
        private string currentRootPath = "";
        private List<string> currentFiles = new List<string>();

        public Form1()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            this.KeyPreview = true;
            this.AllowDrop = true;
            this.KeyDown += HandleKeyDown;
            this.DragEnter += HandleDragEnter;
            this.DragDrop += HandleDragDrop;

            btnSelect.Click += HandleSelectPath;
            btnOpenFolder.Click += HandleOpenFolder;
            btnAutoFilter.Click += HandleAutoFilter;
            btnDelete.Click += HandleDeleteFile;
            btnNext.Click += (s, e) => NavigateFile(1);
            btnPrev.Click += (s, e) => NavigateFile(-1);

            lstFolders.SelectedIndexChanged += HandleFolderChange;
            lstFiles.SelectedIndexChanged += HandleFileChange;
        }

        // --- LOGIC ---

        private void LoadFiles(string path)
        {
            lstFiles.Items.Clear();
            currentFiles.Clear();
            try
            {
                var allFiles = Directory.GetFiles(path);

                // Filter using our new lists
                var filteredFiles = allFiles.Where(f =>
                {
                    string ext = Path.GetExtension(f);
                    return AllowedImages.Contains(ext) || AllowedVideos.Contains(ext);
                }).ToList();

                foreach (var file in filteredFiles)
                {
                    currentFiles.Add(file);
                    lstFiles.Items.Add(Path.GetFileName(file));
                }

                lblStatus.Text = $"Total: {currentFiles.Count} items";
            }
            catch { }
        }

        private void HandleFileChange(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex == -1) return;

            string filePath = currentFiles[lstFiles.SelectedIndex];
            string ext = Path.GetExtension(filePath); // No need for ToLower() because HashSet handles it
            string fileUri = new Uri(filePath).AbsoluteUri;

            lblStatus.Text = $"Viewing: {lstFiles.SelectedIndex + 1} / {currentFiles.Count}";
            string bg = "#e0e0e0";
            string html;

            // Check if it is a VIDEO
            if (AllowedVideos.Contains(ext))
            {
                html = $@"<html><head><meta http-equiv='X-UA-Compatible' content='IE=Edge' />
                <style>body {{ background-color: {bg}; margin: 0; display: flex; justify-content: center; align-items: center; height: 100vh; overflow: hidden; }} video {{ max-width: 95%; max-height: 95%; }}</style>
                </head><body>
                    <video controls autoplay muted>
                        <source src='{fileUri}' type='video/mp4'>
                        Your browser does not support this video format.
                    </video>
                </body></html>";
            }
            // Otherwise it is an IMAGE
            else
            {
                html = $@"<html><head><meta http-equiv='X-UA-Compatible' content='IE=Edge' />
                <style>body {{ background-color: {bg}; margin: 0; display: flex; justify-content: center; align-items: center; height: 100vh; overflow: hidden; }} img {{ max-width: 95%; max-height: 95%; object-fit: contain; }}</style>
                </head><body><img src='{fileUri}' /></body></html>";
            }
            previewBrowser.DocumentText = html;
        }

        private void HandleAutoFilter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentRootPath)) { MessageBox.Show("Select a folder first."); return; }

            try
            {
                string imgPath = Path.Combine(currentRootPath, "image");
                string vidPath = Path.Combine(currentRootPath, "video");

                if (!Directory.Exists(imgPath)) Directory.CreateDirectory(imgPath);
                if (!Directory.Exists(vidPath)) Directory.CreateDirectory(vidPath);

                var files = Directory.GetFiles(currentRootPath);
                int count = 0;

                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file);
                    string fileName = Path.GetFileName(file);
                    string destFolder = null;

                    // Clean logic using HashSets
                    if (AllowedImages.Contains(ext)) destFolder = imgPath;
                    else if (AllowedVideos.Contains(ext)) destFolder = vidPath;

                    if (destFolder != null)
                    {
                        string destPath = Path.Combine(destFolder, fileName);
                        if (!File.Exists(destPath))
                        {
                            File.Move(file, destPath);
                            count++;
                        }
                    }
                }

                MessageBox.Show($"Filtered {count} files successfully!");
                LoadFolder(currentRootPath);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // --- Standard Helpers (No changes needed below here) ---
        private void LoadFolder(string path)
        {
            currentRootPath = path;
            txtPath.Text = currentRootPath;
            LoadSubFolders(currentRootPath);
            LoadFiles(currentRootPath);
        }

        private void LoadSubFolders(string path)
        {
            lstFolders.Items.Clear();
            try
            {
                lstFolders.Items.Add(".. (Root)");
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs) lstFolders.Items.Add(Path.GetFileName(dir));
                if (lstFolders.Items.Count > 0) lstFolders.SelectedIndex = 0;
            }
            catch { }
        }

        private void HandleFolderChange(object sender, EventArgs e)
        {
            if (lstFolders.SelectedIndex == -1) return;
            string selectedName = lstFolders.SelectedItem.ToString();
            string fullPath = (selectedName == ".. (Root)") ? currentRootPath : Path.Combine(currentRootPath, selectedName);
            LoadFiles(fullPath);
        }

        private void HandleSelectPath(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK) LoadFolder(fbd.SelectedPath);
            }
        }

        private void HandleOpenFolder(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentRootPath) && Directory.Exists(currentRootPath))
                Process.Start("explorer.exe", currentRootPath);
        }

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Directory.Exists(files[0])) LoadFolder(files[0]);
        }

        private void NavigateFile(int direction)
        {
            if (lstFiles.Items.Count == 0) return;
            int newIndex = lstFiles.SelectedIndex + direction;
            if (newIndex >= lstFiles.Items.Count) newIndex = 0;
            if (newIndex < 0) newIndex = lstFiles.Items.Count - 1;
            lstFiles.SelectedIndex = newIndex;
        }

        private void NavigateFolder(int direction)
        {
            if (lstFolders.Items.Count == 0) return;
            int newIndex = lstFolders.SelectedIndex + direction;
            if (newIndex >= 0 && newIndex < lstFolders.Items.Count) lstFolders.SelectedIndex = newIndex;
        }

        private async void HandleDeleteFile(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndex == -1) return;
            string filePath = currentFiles[lstFiles.SelectedIndex];
            if (MessageBox.Show($"Delete '{Path.GetFileName(filePath)}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                previewBrowser.Navigate("about:blank");
                await Task.Delay(200);
                try
                {
                    File.Delete(filePath);
                    LoadFiles(currentRootPath); // Simple refresh
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (txtPath.Focused) return;
            switch (e.KeyCode)
            {
                case Keys.Left: NavigateFile(-1); e.Handled = true; break;
                case Keys.Right: NavigateFile(1); e.Handled = true; break;
                case Keys.Up: NavigateFolder(-1); e.Handled = true; break;
                case Keys.Down: NavigateFolder(1); e.Handled = true; break;
                case Keys.Delete: HandleDeleteFile(sender, e); e.Handled = true; break;
            }
        }
    }
}