using AutoMapper;
using BooksApi.Filters;
using BooksApi.Models;
using BooksApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BooksApi.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BooksController(IBooksRepository booksRepository, IMapper mapper)
        {
            this._booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [BooksResultFilter]
        public async Task<IActionResult> GetBooks()
        {
            var bookEntities = await _booksRepository.GetBooksAsync();
            return Ok(bookEntities);
        }

        [HttpGet]
        [BookWithCoversResultFilter]
        [Route("{id}", Name = "GetBook")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var bookEntity = await _booksRepository.GetBookAsync(id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            var bookCovers = await _booksRepository.GetBookCoversAsync(id);           

            return Ok((bookEntity, bookCovers));
        }


        [HttpPost]
        [BookResultFilter]
        public async Task<IActionResult> CreateBook([FromBody]BookForCreation book)
        {
            var bookEntity = _mapper.Map<Entities.Book>(book);
            _booksRepository.AddBook(bookEntity);

            await _booksRepository.SaveChangesAsync();

            await _booksRepository.GetBookAsync(bookEntity.Id);

            return CreatedAtRoute("GetBook", new { id = bookEntity.Id }, bookEntity);
        }
    }
}
