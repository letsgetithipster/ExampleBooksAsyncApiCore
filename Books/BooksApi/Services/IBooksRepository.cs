using BooksApi.ExternalModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BooksApi.Services
{
    public interface IBooksRepository
    {
        IEnumerable<Entities.Book> GetBooks();

        Entities.Book GetBook(Guid id);

        Task<IEnumerable<Entities.Book>> GetBooksAsync();

        Task<IEnumerable<Entities.Book>> GetBooksAsync(IEnumerable<Guid> bookIds);

        Task<BookCover> GetBookCoverAsync(string coverId);

        Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId);

        Task<Entities.Book> GetBookAsync(Guid id);

        void AddBook(Entities.Book bookToAdd);

        Task<bool> SaveChangesAsync();
    }
}