using System;
using System.ComponentModel;
using System.Windows.Media;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>A milestone tile. The predicate is evaluated whenever progress changes.</summary>
    public sealed class Achievement : INotifyPropertyChanged
    {
        private bool unlocked;

        public event PropertyChangedEventHandler PropertyChanged;

        public Achievement(string id, string title, string description, string glyph, Func<Game, bool> predicate)
        {
            Id = id;
            Title = title;
            Description = description;
            Glyph = glyph;
            Predicate = predicate;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Glyph { get; private set; }
        public Func<Game, bool> Predicate { get; private set; }
        public bool IsUnlocked { get { return unlocked; } }

        public string StatusText { get { return unlocked ? "UNLOCKED" : "LOCKED"; } }

        public Brush TileBrush
        {
            get
            {
                return unlocked
                    ? new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x9B, 0xAC))
                    : new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x2A, 0x33));
            }
        }

        public Brush StatusBrush
        {
            get
            {
                return unlocked
                    ? new SolidColorBrush(Color.FromArgb(0xFF, 0x7D, 0xFF, 0xD4))
                    : new SolidColorBrush(Color.FromArgb(0xFF, 0x77, 0x88, 0x96));
            }
        }

        public double TileOpacity { get { return unlocked ? 1 : 0.55; } }

        public void Unlock(bool value)
        {
            if (unlocked == value) return;
            unlocked = value;
            OnPropertyChanged("IsUnlocked");
            OnPropertyChanged("StatusText");
            OnPropertyChanged("TileBrush");
            OnPropertyChanged("StatusBrush");
            OnPropertyChanged("TileOpacity");
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
