using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

    public class DiffRow : INotifyPropertyChanged
    {
        public DiffLine Left { get; set; }
        public DiffLine Right { get; set; }
        private InlineAISuggestion _inlineSuggestion;
        public InlineAISuggestion InlineSuggestion
        {
            get => _inlineSuggestion;
            set
            {
                if (_inlineSuggestion != value)
                {
                    if (_inlineSuggestion != null)
                        _inlineSuggestion.PropertyChanged -= InlineSuggestion_PropertyChanged;
                    _inlineSuggestion = value;
                    if (_inlineSuggestion != null)
                        _inlineSuggestion.PropertyChanged += InlineSuggestion_PropertyChanged;
                    OnPropertyChanged(nameof(InlineSuggestion));
                }
            }
        }

        private void InlineSuggestion_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(InlineSuggestion));
        }

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

    public class InlineAISuggestion : INotifyPropertyChanged
    {
        public int LineNumber { get; set; }
        public string FileName { get; set; }
        public string OriginalText { get; set; }
        public string SuggestedText { get; set; }

        private bool _isApplied;
        public bool IsApplied
        {
            get => _isApplied;
            set
            {
                if (_isApplied != value)
                {
                    _isApplied = value;
                    OnPropertyChanged(nameof(IsApplied));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
