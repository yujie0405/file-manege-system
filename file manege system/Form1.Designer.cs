using System.Drawing;
using System.Windows.Forms;

namespace file_manege_system
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Controls
        private TextBox txtPath;
        private Button btnSelect;
        private Button btnOpenFolder; // Scheme 2: 打开文件夹按钮
        private ListBox lstFolders;
        private ListBox lstFiles;
        private WebBrowser previewBrowser;
        private Button btnPrev;
        private Button btnDelete;
        private Button btnNext;
        private Button btnAutoFilter;
        private Label lblStatus; // Scheme 3: 状态信息

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // --- THEME COLORS ---
            Color DarkHeader = Color.FromArgb(32, 33, 36);
            Color AccentColor = Color.FromArgb(138, 180, 248);
            Color SidebarColor = Color.LavenderBlush;
            Color ListColor = Color.White;
            Color TextColor = Color.FromArgb(60, 64, 67);
            Color FilterButtonColor = Color.FromArgb(104, 81, 255);
            Color DeleteColor = Color.FromArgb(220, 53, 69);

            this.SuspendLayout();

            // 1. FORM SETUP (Scheme 4: 开启拖拽)
            this.Text = "Media Organizer Pro";
            this.Size = new Size(1350, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(60, 60, 60);
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            this.AllowDrop = true; // [关键] 允许拖拽文件夹进来

            // 2. HEADER
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = DarkHeader, Padding = new Padding(20, 25, 20, 15) };

            btnSelect = new Button
            {
                Text = "Select",
                Size = new Size(100, 40),
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(255, 82, 82),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSelect.FlatAppearance.BorderSize = 0;

            // [Scheme 2] 打开文件夹按钮 (放在 Location 左边或右边，这里放左边Label旁)
            btnOpenFolder = new Button
            {
                Text = "📂", // 文件夹图标
                Size = new Size(40, 40),
                Dock = DockStyle.Left, // 放在左侧
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnOpenFolder.FlatAppearance.BorderSize = 0;

            Label lblPath = new Label
            {
                Text = "Location:",
                AutoSize = true,
                Dock = DockStyle.Left,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 10, 10, 0)
            };

            Panel searchContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 20, 0) };

            Panel txtWrapper = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(224, 224, 224)
            };

            txtPath = new TextBox
            {
                Multiline = false,
                ReadOnly = true,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(224, 224, 224),
                ForeColor = Color.Black,
                Location = new Point(10, 10),
                Width = 600,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            txtWrapper.Controls.Add(txtPath);
            searchContainer.Controls.Add(txtWrapper);

            headerPanel.Controls.Add(searchContainer);
            headerPanel.Controls.Add(btnSelect);
            headerPanel.Controls.Add(lblPath);
            headerPanel.Controls.Add(btnOpenFolder); // 添加打开按钮

            btnSelect.SendToBack();
            btnOpenFolder.SendToBack();
            lblPath.SendToBack();
            searchContainer.BringToFront();
            this.Controls.Add(headerPanel);

            // 3. MAIN LAYOUT
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(20, 100, 20, 80)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Folders
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Gap
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F)); // Files
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); // Preview
            this.Controls.Add(mainLayout);

            // --- COL 1: FOLDERS (PINK) ---
            Panel pnlFolders = new Panel { Dock = DockStyle.Fill, BackColor = SidebarColor };
            lstFolders = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = SidebarColor,
                ItemHeight = 28,
                Font = new Font("Segoe UI Semibold", 10F),
                ForeColor = Color.Black,
                IntegralHeight = false,
                HorizontalScrollbar = true
            };
            pnlFolders.Controls.Add(lstFolders);
            Panel pnlFolderCard = CreateCard(pnlFolders);
            mainLayout.Controls.Add(pnlFolderCard, 0, 0);

            // --- COL 2: AUTO FILTER BUTTON ---
            btnAutoFilter = new Button
            {
                Text = "Auto Filter",
                Size = new Size(100, 40),
                BackColor = FilterButtonColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.None
            };
            btnAutoFilter.FlatAppearance.BorderSize = 0;
            mainLayout.Controls.Add(btnAutoFilter, 1, 0);

            // --- COL 3: FILES ---
            Panel pnlFiles = new Panel { Dock = DockStyle.Fill, BackColor = ListColor };
            lstFiles = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = ListColor,
                ItemHeight = 28,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextColor,
                IntegralHeight = false,
                HorizontalScrollbar = true
            };
            pnlFiles.Controls.Add(lstFiles);
            Panel pnlFileCard = CreateCard(pnlFiles);
            mainLayout.Controls.Add(pnlFileCard, 2, 0);

            // --- COL 4: PREVIEW ---
            Panel pnlPreview = new Panel { Dock = DockStyle.Fill, BackColor = Color.Gainsboro };
            Label lblPreviewTitle = new Label
            {
                Text = "Image / Video Preview",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Black
            };
            previewBrowser = new WebBrowser { Dock = DockStyle.Fill, ScrollBarsEnabled = false };

            // Controls Layout (Updated for Status Label)
            TableLayoutPanel pnlControls = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 70, // 加高一点放 Status
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2
            };

            // Buttons Row
            btnPrev = CreateFlatButton("<", Color.Gainsboro);
            btnDelete = CreateFlatButton("Delete", DeleteColor);
            btnDelete.ForeColor = Color.White;
            btnNext = CreateFlatButton(">", Color.Gainsboro);
            btnPrev.BackColor = Color.LightGray; btnPrev.ForeColor = Color.Black;
            btnNext.BackColor = Color.LightGray; btnNext.ForeColor = Color.Black;

            FlowLayoutPanel flowBtns = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.None, BackColor = Color.Transparent };
            flowBtns.Controls.Add(btnPrev);
            flowBtns.Controls.Add(btnDelete);
            flowBtns.Controls.Add(btnNext);

            // [Scheme 3] Status Label Row
            lblStatus = new Label
            {
                Text = "Ready",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 9F)
            };

            pnlControls.Controls.Add(flowBtns, 0, 0);
            pnlControls.Controls.Add(lblStatus, 0, 1); // Add status to second row

            Panel innerPreview = new Panel { Dock = DockStyle.Fill };
            innerPreview.Controls.Add(previewBrowser);
            innerPreview.Controls.Add(pnlControls);
            innerPreview.Controls.Add(lblPreviewTitle);
            mainLayout.Controls.Add(innerPreview, 3, 0);

            this.ResumeLayout(false);
        }

        private Panel CreateCard(Control content)
        {
            Panel card = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Color.White };
            card.Controls.Add(content);
            return card;
        }

        private Button CreateFlatButton(string text, Color bg)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(80, 30),
                Margin = new Padding(5),
                BackColor = bg,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}