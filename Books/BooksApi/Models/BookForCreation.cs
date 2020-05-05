using System;

namespace BooksApi.Models
{
    public class BookForCreation
    {
        public Guid AuthorID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
