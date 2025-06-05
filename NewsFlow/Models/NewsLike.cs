using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFlow.Models
{
    public class NewsLike
    {
        public int NewsId { get; set; } // ID-ul știrii la care s-a dat like
        public string UserId { get; set; } // ID-ul utilizatorului care a dat like
        public DateTime LikedAt { get; set; } // Data și ora la care s-a dat like
    }
}
