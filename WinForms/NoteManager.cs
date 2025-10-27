using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
namespace Notebook
{
    public class NoteManager
    {
        private readonly List<Note> _notes = new List<Note>();

        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Notebook");

        private static readonly string FilePath = Path.Combine(FolderPath, "notes.json");

        public List<Note> GetAll() 
        { 
            return _notes; 
        }

        public void Add(string title, string content, string category)
        {
            var note = new Note
            {
                Title = title,
                Content = content ?? string.Empty,
                Category = category ?? "Uncategorized",
                CreatedAt = DateTime.Now,
            };

            _notes.Add(note);
            SaveToFile();
        }

        public void Delete(Note note)
        { 
            _notes.Remove(note);
            SaveToFile();
        }

        public void Edit(Note note, string newTitle, string newContent, string newCategory)
        { 
            note.Title = newTitle;
            note.Content = newContent;
            note.Category = newCategory;
            SaveToFile();
        }

        public void LoadFromFile()
        {
            if (!File.Exists(FilePath)) return;

            string json = File.ReadAllText(FilePath);
            var loaded = JsonConvert.DeserializeObject<List<Note>>(json);
            if (loaded == null) return;

            foreach (var n in loaded)
            { 
                n.Content = n.Content ?? string.Empty;
            }

            _notes.Clear();
            _notes.AddRange(loaded);
        }

        public void SaveToFile()
        {
            if (!Directory.Exists(FolderPath))
            { 
                Directory.CreateDirectory(FolderPath);
            }

            string json = JsonConvert.SerializeObject(_notes, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
    }
}
