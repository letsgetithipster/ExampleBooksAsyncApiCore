using AutoMapper;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BooksApi.Filters
{
    public class BookWithCoversResultFilterAttribute : ResultFilterAttribute
    {
        public override async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {

            var resultFromAction = context.Result as ObjectResult;
            if (resultFromAction?.Value == null
                || resultFromAction.StatusCode < 200
                || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            var (book, bookCovers) = ((Entities.Book,
                IEnumerable<ExternalModels.BookCover>))resultFromAction.Value;

            var mappedBook = Mapper.Map<BookWithCovers>(book);

            resultFromAction.Value = Mapper.Map(bookCovers, mappedBook);

            await next();
        }
    }

}
