using System;
using System.Collections.Generic;
using System.Diagnostics; // Needed for Process.Start
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace file_manege_system
{
    static class Program
    {
        static Form1 view;
        static string currentRootPath = "";
        static List<string> currentFiles = new List<string>();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            view = new Form1();

            // --- 1. 键盘监听 (Scheme 1) ---
            view.KeyPreview = true;
            view.KeyDown += HandleKeyDown;

            // --- 2. 拖拽功能 (Scheme 4) ---
            view.DragEnter += HandleDragEnter;
            view.DragDrop += HandleDragDrop;

            // --- BINDINGS ---
            view.SelectButton.Click += HandleSelectPath;
            view.OpenFolderButton.Click += HandleOpenFolder; // Scheme 2
            view.FolderList.SelectedIndexChanged += HandleFolderChange;
            view.FileList.SelectedIndexChanged += HandleFileChange;
            view.AutoFilterButton.Click += HandleAutoFilter;

            view.ButtonNext.Click += (s, e) => NavigateFile(1);
            view.ButtonPrev.Click += (s, e) => NavigateFile(-1);
            view.DeleteButton.Click += HandleDeleteFile;

            Application.Run(view);
        }

        // --- SCHEME 4: 拖拽逻辑 ---
        static void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        static void HandleDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && Directory.Exists(files[0]))
            {
                currentRootPath = files[0];
                view.PathBox.Text = currentRootPath;
                LoadFolders(currentRootPath);
            }
        }

        // --- SCHEME 2: 打开文件夹 ---
        static void HandleOpenFolder(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentRootPath) && Directory.Exists(currentRootPath))
            {
                Process.Start("explorer.exe", currentRootPath);
            }
        }

        // --- SCHEME 1: 键盘逻辑 (新增 Up/Down) ---
        static void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (view.PathBox.Focused) return;

            switch (e.KeyCode)
            {
                // 左右键控制文件 (Files)
                case Keys.Left: NavigateFile(-1); e.Handled = true; break;
                case Keys.Right: NavigateFile(1); e.Handled = true; break;

                // 上下键控制文件夹 (Folders) - NEW
                case Keys.Up: NavigateFolder(-1); e.Handled = true; break;
                case Keys.Down: NavigateFolder(1); e.Handled = true; break;

                case Keys.Delete: HandleDeleteFile(sender, e); e.Handled = true; break;
            }
        }

        static void NavigateFolder(int direction)
        {
            if (view.FolderList.Items.Count == 0) return;
            int newIndex = view.FolderList.SelectedIndex + direction;
            if (newIndex >= 0 && newIndex < view.FolderList.Items.Count)
            {
                view.FolderList.SelectedIndex = newIndex;
            }
        }

        static void HandleDeleteFile(object sender, EventArgs e)
        {
            if (view.FileList.SelectedIndex == -1) return;

            string filePath = currentFiles[view.FileList.SelectedIndex];
            string fileName = Path.GetFileName(filePath);

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete '{fileName}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    view.Preview.Navigate("about:blank");
                    Application.DoEvents();
                    File.Delete(filePath);

                    string selectedFolder = view.FolderList.SelectedItem?.ToString();
                    string fullPath = (selectedFolder == ".. (Root)") ? currentRootPath : Path.Combine(currentRootPath, selectedFolder);

                    LoadFiles(fullPath);
                    view.Preview.DocumentText = "";
                    view.StatusLabel.Text = "Deleted";
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        static void HandleAutoFilter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentRootPath))
            {
                MessageBox.Show("Please select a folder first.");
                return;
            }

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
                    string ext = Path.GetExtension(file).ToLower();
                    string fileName = Path.GetFileName(file);

                    if (ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".bmp")
                    {
                        string dest = Path.Combine(imgPath, fileName);
                        if (!File.Exists(dest)) File.Move(file, dest);
                        count++;
                    }
                    else if (ext == ".mp4" || ext == ".avi" || ext == ".mov")
                    {
                        string dest = Path.Combine(vidPath, fileName);
                        if (!File.Exists(dest)) File.Move(file, dest);
                        count++;
                    }
                }

                MessageBox.Show($"Filtered {count} files successfully!");
                LoadFolders(currentRootPath);
                LoadFiles(currentRootPath);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        static void HandleSelectPath(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    currentRootPath = fbd.SelectedPath;
                    view.PathBox.Text = currentRootPath;
                    LoadFolders(currentRootPath);
                }
            }
        }

        static void LoadFolders(string path)
        {
            view.FolderList.Items.Clear();
            view.FileList.Items.Clear();
            try
            {
                view.FolderList.Items.Add(".. (Root)");
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    view.FolderList.Items.Add(Path.GetFileName(dir));
                }
                if (view.FolderList.Items.Count > 0) view.FolderList.SelectedIndex = 0;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        static void HandleFolderChange(object sender, EventArgs e)
        {
            if (view.FolderList.SelectedIndex == -1) return;
            string selectedName = view.FolderList.SelectedItem.ToString();
            string fullPath = (selectedName == ".. (Root)") ? currentRootPath : Path.Combine(currentRootPath, selectedName);
            LoadFiles(fullPath);
        }

        // --- SCHEME 3: 更新状态信息 ---
        static void LoadFiles(string path)
        {
            view.FileList.Items.Clear();
            currentFiles.Clear();
            try
            {
                var allFiles = Directory.GetFiles(path);
                var filteredFiles = allFiles.Where(f =>
                {
                    string ext = Path.GetExtension(f).ToLower();
                    return ext == ".mp4" || ext == ".jpg" || ext == ".png" || ext == ".jpeg";
                }).ToList();

                foreach (var file in filteredFiles)
                {
                    currentFiles.Add(file);
                    view.FileList.Items.Add(Path.GetFileName(file));
                }

                view.StatusLabel.Text = $"Total: {currentFiles.Count} items";
            }
            catch { }
        }

        static void HandleFileChange(object sender, EventArgs e)
        {
            if (view.FileList.SelectedIndex == -1) return;

            string filePath = currentFiles[view.FileList.SelectedIndex];
            string ext = Path.GetExtension(filePath).ToLower();
            string fileUri = new Uri(filePath).AbsoluteUri;

            // Update Status Info (Scheme 3)
            view.StatusLabel.Text = $"Viewing: {view.FileList.SelectedIndex + 1} / {currentFiles.Count}";

            string bg = "#e0e0e0";

            string html;
            if (ext == ".mp4")
            {
                html = $@"<html><head><meta http-equiv='X-UA-Compatible' content='IE=Edge' />
                <style>body {{ background-color: {bg}; margin: 0; display: flex; justify-content: center; align-items: center; height: 100vh; overflow: hidden; font-family: sans-serif; }} video {{ max-width: 95%; max-height: 95%; }}</style>
                </head><body><video controls autoplay muted><source src='{fileUri}' type='video/mp4'></video></body></html>";
            }
            else
            {
                html = $@"<html><head><meta http-equiv='X-UA-Compatible' content='IE=Edge' />
                <style>body {{ background-color: {bg}; margin: 0; display: flex; justify-content: center; align-items: center; height: 100vh; overflow: hidden; font-family: sans-serif; }} img {{ max-width: 95%; max-height: 95%; object-fit: contain; }}</style>
                </head><body><img src='{fileUri}' /></body></html>";
            }
            view.Preview.DocumentText = html;
        }

        static void NavigateFile(int direction)
        {
            if (view.FileList.Items.Count == 0) return;
            int newIndex = view.FileList.SelectedIndex + direction;
            if (newIndex >= view.FileList.Items.Count) newIndex = 0;
            if (newIndex < 0) newIndex = view.FileList.Items.Count - 1;
            view.FileList.SelectedIndex = newIndex;
        }
    }
}