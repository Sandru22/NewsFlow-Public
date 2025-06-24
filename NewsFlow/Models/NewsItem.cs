using NewsFlow.Web;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace NewsFlow.Models
{
    public class NewsItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int NewsId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }

        public string Source { get; set; }
        public DateTime publishedAt { get; set; }

        private int _likes;
        public int Likes
        {
            get => _likes;
            set
            {
                _likes = value;
                OnPropertyChanged(nameof(Likes));
            }
        }

        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                OnPropertyChanged(nameof(IsHighlighted));
            }
        }

        private bool _hasLiked;
        public bool HasLiked
        {
            get => _hasLiked;
            set
            {
                _hasLiked = value;
                OnPropertyChanged(nameof(HasLiked));
            }
        }

        private bool _hasSubscribed;
        public bool HasSubscribed
        {
            get => _hasSubscribed;
            set
            {
                _hasSubscribed = value;
                OnPropertyChanged(nameof(HasSubscribed));
            }
        }
       
        public List<NewsLike> NewsLikes { get; set; } = new List<NewsLike>();

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ImageUrl { get; set; }

        public ICommand OpenNewsCommand { get; }
        public bool IsImageVisible => !string.IsNullOrWhiteSpace(ImageUrl);

        public string Site
        {
            get
            {
                if (Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    var segments = uri.AbsolutePath
                        .Trim('/')
                        .Split('/')
                        .Where(s => !string.Equals(s, "rss", StringComparison.OrdinalIgnoreCase))
                        .Where(s => !string.Equals(s, "feed", StringComparison.OrdinalIgnoreCase))
                        .Where(s => !s.Contains("-"))
                        .Where(s => !string.Equals(s, "stiri", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    if (segments.Length > 0)
                    {
                        return $"{uri.Host}/{string.Join("/", segments)}";
                    }

                    return uri.Host;
                }
                return string.Empty;
            }
        }

        public bool IsSubscribed { get; set; }
    }

    
 
}