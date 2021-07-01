using System;
using Application.DTOs.User;

namespace Application.DTOs.News
{
    public class NewsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Preview { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public ShortProfileDto Author { get; set; }
    }
}