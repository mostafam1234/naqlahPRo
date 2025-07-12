using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Presentaion.Reponse;
using System.Net;

namespace NAQLAH.Server.MiddleWares
{
    public class ExceptionHandlingMiddleWare
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleWare(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionMessageAsync(context, ex).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionMessageAsync(HttpContext context, Exception ex)
        {
            context.Response.Clear();
            if (ex is FluentValidation.ValidationException)
            {
                var ValidationException = ex as FluentValidation.ValidationException;
                if (ValidationException is null)
                {
                    var globalError = ProblemDetail.CreateProblemDetail("An Error Occur While Tring to Process The Request");
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(globalError));
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var validationErrors = ValidationException.Errors.Select(x => x.ErrorMessage).ToList();
                var faliures = string.Join(",", validationErrors);
                var errorResult = ProblemDetail.CreateProblemDetail(faliures);
                return context.Response.WriteAsync(JsonConvert.SerializeObject(errorResult));
            }
            var result = ProblemDetail.CreateProblemDetail("An Error Occur While Tring to Process The Request");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result));

        }

    }
}
