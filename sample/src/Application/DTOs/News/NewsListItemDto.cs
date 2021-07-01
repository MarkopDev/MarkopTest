using System;
using Application.DTOs.User;

namespace Application.DTOs.News
{
    public class NewsListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Preview { get; set; }
        public DateTime CreatedDate { get; set; }
        public ShortProfileDto Author { get; set; }
    }
}