using BooksApi.Contexts;
using BooksApi.Entities;
using BooksApi.ExternalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BooksApi.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        #region private variable and constructor

        private BooksContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BooksRepository> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public BooksRepository(BooksContext context, IHttpClientFactory httpClientFactory, ILogger<BooksRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        public async Task<Book> GetBookAsync(Guid id)
        {
            return await _context.Books.Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            await _context.Database.ExecuteSqlCommandAsync("WAITFOR DELAY '00:00:02';");
            return await _context.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            return await _context.Books.Where(b => bookIds.Contains(b.Id))
                .Include(b => b.Author).ToListAsync();
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // pass through a dummy name
            var response = await httpClient
                                .GetAsync($"http://localhost:54493/api/bookcovers/{coverId}");

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<BookCover>(
                    await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            _cancellationTokenSource = new CancellationTokenSource();

            //create a list of fake bookcovers
            var bookCoverUrls = new[]
            {
                $"http://localhost:54493/api/bookcovers/{bookId}-dummycover1",
                //$"http://localhost:54493/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:54493/api/bookcovers/{bookId}-dummycover2",
                $"http://localhost:54493/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:54493/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:54493/api/bookcovers/{bookId}-dummycover5"
            };            

            //create tasks
            var downloadBookCoverTasksQuery =
                from bookCoverUrl
                in bookCoverUrls
                select DownloadBookCover(httpClient, bookCoverUrl, _cancellationTokenSource.Token);

            //start the tasks
            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();


            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation($"{ex.Message}");
                foreach (var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}.");
                }

                return new List<BookCover>();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{ex.Message}");
                throw;
            }
        }

        private async Task<BookCover> DownloadBookCover(
            HttpClient httpClient, string bookCoverUrl, CancellationToken cancellationToken)
        {
            var response = await httpClient
                   .GetAsync(bookCoverUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonConvert.DeserializeObject<BookCover>(
                    await response.Content.ReadAsStringAsync());

                return bookCover;
            }

            _cancellationTokenSource.Cancel();

            return null;
        }

        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
                throw new ArgumentNullException(nameof(bookToAdd));


            _context.Add(bookToAdd);
        }

        public async Task<bool> SaveChangesAsync()
            => (await _context.SaveChangesAsync() > 0);

        public IEnumerable<Book> GetBooks()
        {
            _context.Database.ExecuteSqlCommand("WAITFOR DELAY '00:00:02';");
            return _context.Books.Include(b => b.Author).ToList();
        }

        public Book GetBook(Guid id)
        {
            return _context.Books.Include(b => b.Author)
                .FirstOrDefault(b => b.Id == id);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_context != null)
                    {
                        _context.Dispose();
                        _context = null;
                    }
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Dispose();
                        _cancellationTokenSource = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
