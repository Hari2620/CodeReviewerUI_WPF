using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReviewerApp.Models
{
    public class DiffLine : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        public DiffType Type { get; set; } // Added, Removed, Unchanged, Modified
        public int? LineNumber { get; set; }
        public ObservableCollection<DiffWord> Words { get; set; } = new ObservableCollection<DiffWord>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class DiffWord
    {
        public string Text { get; set; }
        public DiffType Type { get; set; }
    }

    public class DiffRow: INotifyPropertyChanged
    {
        public DiffLine Left { get; set; }
        public DiffLine Right { get; set; }
        public InlineAISuggestion InlineSuggestion { get; set; }

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (_isPopupOpen != value)
                {
                    _isPopupOpen = value;
                    OnPropertyChanged(nameof(IsPopupOpen));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private bool _showAISuggestionPopup;
        public bool ShowAISuggestionPopup
        {
            get => _showAISuggestionPopup;
            set
            {
                if (_showAISuggestionPopup != value)
                {
                    _showAISuggestionPopup = value;
                    OnPropertyChanged(nameof(ShowAISuggestionPopup));
                }
            }
        }
    }

    public enum DiffType
    {
        Unchanged,
        Added,
        Removed,
        Imaginary,
        Modified, // Optionally for inline changes
        Collapsed
    }
    public class InlineAISuggestion
    {
        public int LineNumber { get; set; } // The line index in RightDiffLines
        public string OriginalText { get; set; }
        public string SuggestedText { get; set; }
        public bool IsApplied { get; set; }
    }
}
