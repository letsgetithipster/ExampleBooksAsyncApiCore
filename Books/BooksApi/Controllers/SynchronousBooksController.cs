using BooksApi.Filters;
using BooksApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BooksApi.Controllers
{
    [Route("api/synchronousbooks")]
    [ApiController]
    public class SynchronousBooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;

        public SynchronousBooksController(IBooksRepository booksRepository)
        {
            this._booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
        }

        [HttpGet]
        [BooksResultFilter]
        public IActionResult GetBooks()
        {
            var bookEntities = _booksRepository.GetBooks();
            return Ok(bookEntities);
        }

        [HttpGet]
        [BookWithCoversResultFilter]
        [Route("{id}", Name = "GetBook")]
        public IActionResult GetBook(Guid id)
        {
            var bookEntity = _booksRepository.GetBook(id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            return Ok(bookEntity);
        }
    }
}
