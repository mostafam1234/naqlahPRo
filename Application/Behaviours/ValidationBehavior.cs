using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Behaviours
{
   public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> 
   {
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) 
    {
           
            if (!_validators.Any())
            {
                return await next();
            }
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));



            var failures = validationResults
                   .SelectMany(result => result.Errors)
                   .Where(f => f != null)
                   .ToList();

            var distincFailures = failures.DistinctBy(x=>x.ErrorMessage).ToList();


            if (failures.Any()) 
            {
                throw new FluentValidation.ValidationException(distincFailures);
            }

        return await next();
    }
   }

}
