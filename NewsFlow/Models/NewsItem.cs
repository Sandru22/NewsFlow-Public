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

        // Lista de like-uri
        public List<NewsLike> NewsLikes { get; set; } = new List<NewsLike>();

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ImageUrl { get; set; }

        public ICommand OpenNewsCommand { get; }
        public bool IsImageVisible => !string.IsNullOrWhiteSpace(ImageUrl);

        public string Source
        {
            get
            {
                if (Uri.TryCreate(Url, UriKind.Absolute, out var uri))
                {
                    return uri.Host; // ex: www.stiripesurse.ro
                }
                return string.Empty;
            }
        }

        
    }

    
 
}