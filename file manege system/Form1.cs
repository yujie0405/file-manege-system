using System;
using System.Windows.Forms;

namespace file_manege_system
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // --- PUBLIC ACCESSORS ---
        public Button SelectButton => btnSelect;
        public Button OpenFolderButton => btnOpenFolder; // New
        public TextBox PathBox => txtPath;
        public ListBox FolderList => lstFolders;
        public ListBox FileList => lstFiles;
        public WebBrowser Preview => previewBrowser;
        public Button ButtonPrev => btnPrev;
        public Button ButtonNext => btnNext;
        public Button DeleteButton => btnDelete;
        public Button AutoFilterButton => btnAutoFilter;
        public Label StatusLabel => lblStatus; // New
    }
}