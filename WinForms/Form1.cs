using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace Notebook
{
    public partial class Form1 : Form
    {
        private readonly NoteManager _manager = new NoteManager();
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupButtons();
            SetupControls();

            _manager.LoadFromFile();
            RefreshList();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            string title = noteTextBox.Text.Trim();
            string content = contentTextBox.Text;
            string category = categoryComboBox.SelectedItem?.ToString() ?? "UnCategorized";

            if (Validation.StringIsNullOrEmpty(title))
            {
                MessageBox.Show("Please enter a note title. ");
                return;
            }

            _manager.Add(title, content, category);
            RefreshList();

            noteTextBox.Clear();
            contentTextBox.Clear();
        }

        private void editBtn_Click(object sender, EventArgs e)
        {
            var selectedNote = GetSelectedNote();

            if (selectedNote == null)
            {
                MessageBox.Show("Please select a note to edit.");
                return;
            }

            string newTitle = string.IsNullOrWhiteSpace(noteTextBox.Text)
                ? selectedNote.Title : noteTextBox.Text.Trim();

            string newContent = string.IsNullOrWhiteSpace(contentTextBox.Text)
                ? selectedNote.Content : contentTextBox.Text.Trim();
            string newCategory = categoryComboBox.SelectedItem?.ToString() ?? "Uncategorized";

            _manager.Edit(selectedNote, newTitle, newContent, newCategory);
            RefreshList(searchTextBox.Text);

            noteTextBox.Clear();
            contentTextBox.Clear();
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            var selectedNote = GetSelectedNote();

            if (selectedNote == null)
            {
                MessageBox.Show("Please select a note to delete.");
                return;
            }

            _manager.Delete(selectedNote);
            RefreshList(searchTextBox.Text);
            noteTextBox.Clear();
            contentTextBox.Clear();
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Reset mygtukas
        private void button1_Click(object sender, EventArgs e)
        {
            searchTextBox.Clear();
            noteTextBox.Clear();
            contentTextBox.Clear();
        }

        private void exportCsvBtn_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "notes.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var notes = _manager.GetAll();
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("Title; Content; Category; Date; Done");

                        foreach (var note in notes)
                        {
                            string content = note.Content.Replace(Environment.NewLine, " ");
                            sw.WriteLine($"\"{note.Title}\";\"{content}\";\"{note.Category}\";\"{note.CreatedAt:yyyy-MM-dd HH:mm}\";{note.IsDone}");
                        }
                    }

                    MessageBox.Show("Notes exported successfully to CSV!");
                }
            }
        }

        private void exportTxtBtn_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text files (*.txt)|*.txt";
                sfd.FileName = "notes.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var notes = _manager.GetAll();
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        foreach (var note in notes)
                        {
                            sw.WriteLine($"Title: {note.Title}");
                            sw.WriteLine($"Content: {note.Content}");
                            sw.WriteLine($"Category:  {note.Category}");
                            sw.WriteLine($"Date: {note.CreatedAt: yyyy-MM-dd HH:mm}");
                            sw.WriteLine($"Done: {note.IsDone}");
                            sw.WriteLine(new string('-', 50));
                        }
                    }

                    MessageBox.Show("Notes exported successfully to TXT!");
                }
            }
        }

        private void SetupButtons()
        {
            SetupButton(addBtn, "images/add.png");
            SetupButton(editBtn, "images/edit.png");
            SetupButton(deleteBtn, "images/delete.png");
        }

        private void SetupButton(Button btn, string imagePath)
        {
            if (File.Exists(imagePath))
            {
                btn.Image = Image.FromFile(imagePath);
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.TextAlign = ContentAlignment.MiddleRight;
            }
        }

        private void SetupControls()
        {
            noteListView.CheckBoxes = true;
            noteListView.FullRowSelect = true;
            noteListView.HideSelection = false;

            themeComboBox.SelectedIndex = 0;

            SetupComboBoxTheme(categoryComboBox);
            SetupComboBoxTheme(filterCategoryComboBox);
            SetupComboBoxTheme(themeComboBox);
            SetupComboBoxTheme(sortComboBox);
        }

        private void noteListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (noteListView.SelectedItems.Count > 0)
            {
                var selected = (Note)noteListView.SelectedItems[0].Tag;
                noteTextBox.Text = selected.Title;
                contentTextBox.Text = selected.Content;
            }
        }

        private void sortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList(searchTextBox.Text);
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            RefreshList(searchTextBox.Text);
        }

        private void filterCategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList(searchTextBox.Text);
        }

        private void noteListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is Note note)
            {
                note.IsDone = e.Item.Checked;

                bool darkMode = themeComboBox.SelectedItem?.ToString() == "Dark";
                ApplyNoteStyle(e.Item, note, darkMode);

                _manager.SaveToFile();
            }
        }

        private void themeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool darkMode = themeComboBox.SelectedItem.ToString() == "Dark";

            // Pagrindinė formos spalva
            this.BackColor = darkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            this.ForeColor = darkMode ? Color.White : Color.Black;

            // Taikome visiems kontrolėms
            ApplyThemeToControls(this, darkMode);

            // Specialiai atnaujinam ListView ir ComboBox
            noteListView.BackColor = darkMode ? Color.FromArgb(40, 40, 40) : Color.White;
            noteListView.ForeColor = darkMode ? Color.White : Color.Black;

            // Priverstinis ListView persipiešimas su nauja tema
            noteListView.Invalidate();
            RefreshList(searchTextBox.Text);
        }

        private void RefreshList(string search = "")
        {
            string category = filterCategoryComboBox.SelectedItem?.ToString();
            string sortOption = sortComboBox.SelectedItem?.ToString();

            var notes = GetFilteredNotes(search, category);
            notes = ApplySorting(notes, sortOption);
            DisplayNotes(notes);
        }

        private List<Note> GetFilteredNotes(string search, string category)
        {
            var notes = _manager.GetAll();

            if (!Validation.StringIsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                notes = notes.Where(x => x.Title.ToLower().Contains(search) ||
                                    x.Content.ToLower().Contains(search)).ToList();
            }

            if (!Validation.StringIsNullOrWhiteSpace(category) && category != "All")
            {
                notes = notes.Where(x => x.Category == category).ToList();
            }

            return notes;
        }

        private List<Note> ApplySorting(List<Note> notes, string sortOption)
        {
            if (Validation.StringIsNullOrWhiteSpace(sortOption) || sortOption == "All")
            {
                return notes;
            }

            switch (sortComboBox.SelectedItem.ToString())
            {
                case "Title (A-Z)":
                    notes = notes.OrderBy(x => x.Title).ToList();
                    break;
                case "Title (Z-A)":
                    notes = notes.OrderByDescending(x => x.Title).ToList();
                    break;
                case "Date (Newest)":
                    notes = notes.OrderByDescending(x => x.CreatedAt).ToList();
                    break;
                case "Date (Oldest)":
                    notes = notes.OrderBy(x => x.CreatedAt).ToList();
                    break;
                case "Category (A-Z)":
                    notes = notes.OrderBy(x => x.Category).ToList();
                    break;
            }

            return notes;
        }

        private void DisplayNotes(List<Note> notes)
        {
            noteListView.Items.Clear();
            bool darkMode = themeComboBox.SelectedItem?.ToString() == "Dark";

            foreach (var note in notes)
            {
                var item = new ListViewItem(note.Title);
                item.SubItems.Add(note.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add(note.Content.Length > 30
                            ? note.Content.Substring(0, 30) + "..."
                            : note.Content);
                item.SubItems.Add(note.Category);
                item.Tag = note;

                item.Checked = note.IsDone;

                ApplyNoteStyle(item, note, darkMode);

                noteListView.Items.Add(item);
            }
        }

        private Note GetSelectedNote()
        {
            if (noteListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return (Note)noteListView.SelectedItems[0].Tag;
        }

        private void ApplyThemeToControls(Control parent, bool darkMode)
        {
            Color darkBack = Color.FromArgb(40, 40, 40);
            Color darkFore = Color.White;
            Color lightBack = Color.White;
            Color lightFore = Color.Black;

            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is TextBox tb)
                {
                    tb.BackColor = darkMode ? darkBack : lightBack;
                    tb.ForeColor = darkMode ? darkFore : lightFore;
                }
                else if (ctrl is ComboBox cb)
                {
                    cb.BackColor = darkMode ? darkBack : lightBack;
                    cb.ForeColor = darkMode ? darkFore : lightFore;
                    cb.FlatStyle = darkMode ? FlatStyle.Flat : FlatStyle.Standard;
                }
                else if (ctrl is ListView lv)
                {
                    lv.BackColor = darkMode ? darkBack : lightBack;
                    lv.ForeColor = darkMode ? darkFore : lightFore;
                }
                else if (ctrl is Button btn)
                {
                    if (darkMode)
                    {
                        btn.BackColor = Color.DimGray;
                        btn.ForeColor = Color.White;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = Color.Black;
                    }
                    else
                    {
                        btn.BackColor = Color.LightGray;
                        btn.ForeColor = Color.Black;
                        btn.FlatStyle = FlatStyle.Standard;
                    }
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = darkMode ? darkFore : lightFore;
                }
                else
                {
                    ctrl.BackColor = darkMode ? darkBack : lightBack;
                    ctrl.ForeColor = darkMode ? darkFore : lightFore;
                }

                // Rekursyviai pritaikom vaikiniams kontrolėms
                if (ctrl.HasChildren)
                {
                    ApplyThemeToControls(ctrl, darkMode);
                }
            }
        }

        private void SetupComboBoxTheme(ComboBox cb)
        {
            cb.DrawMode = DrawMode.OwnerDrawFixed;

            cb.DrawItem += (s, eArgs) =>
            {
                if (eArgs.Index < 0) return;

                string text = cb.Items[eArgs.Index].ToString();
                bool darkMode = themeComboBox.SelectedItem?.ToString() == "Dark";

                Color back = darkMode ? Color.FromArgb(40, 40, 40) : Color.White;
                Color fore = darkMode ? Color.White : Color.Black;

                using (Brush b = new SolidBrush(back))
                {
                    eArgs.Graphics.FillRectangle(b, eArgs.Bounds);
                }


                TextRenderer.DrawText(
                    eArgs.Graphics,
                    text,
                    eArgs.Font,
                    eArgs.Bounds,
                    fore,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            };
        }

        private void ApplyNoteStyle(ListViewItem item, Note note, bool darkMode)
        {
            if (note.IsDone)
            {
                item.ForeColor = Color.Gray;
                item.Font = new Font(item.Font, FontStyle.Strikeout);
            }
            else
            {
                item.Font = new Font(item.Font, FontStyle.Regular);
                item.ForeColor = darkMode ? Color.White : Color.Black;

                switch (note.Category)
                {
                    case "Work": item.ForeColor = Color.DeepSkyBlue; break;
                    case "Personal": item.ForeColor = Color.Green; break;
                    case "School": item.ForeColor = Color.DarkGoldenrod; break;
                    case "Ideas": item.ForeColor = Color.DarkMagenta; break;
                    case "Other": item.ForeColor = darkMode ? Color.LightGray : Color.DimGray; break;
                }
            }
        }
    }
}
