using CSharpFunctionalExtensions;
using Domain.InterFaces;
using MediatR;
using System.Reflection;

namespace Application.Behaviours
{
    public class ResultLocalizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IReadFromResourceFile readFromResourceFile;

        public ResultLocalizationBehavior(IReadFromResourceFile readFromResourceFile)
        {
            this.readFromResourceFile = readFromResourceFile;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();
            return Localize(response);
        }

        private TResponse Localize(TResponse response)
        {
            if (response is null)
                return response;

            if (response is Result result)
            {
                if (result.IsSuccess)
                    return response;

                return (TResponse)(object)Result.Failure(Translate(result.Error));
            }

            var type = response.GetType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var isFailure = (bool)type.GetProperty(nameof(Result.IsFailure))!.GetValue(response)!;
                if (!isFailure)
                    return response;

                var error = (string)type.GetProperty(nameof(Result.Error))!.GetValue(response)!;
                var valueType = type.GetGenericArguments()[0];
                var localized = Translate(error);

                var failureMethod = typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Single(m => m.Name == nameof(Result.Failure)
                                 && m.IsGenericMethodDefinition
                                 && m.GetParameters().Length == 1
                                 && m.GetParameters()[0].ParameterType == typeof(string))
                    .MakeGenericMethod(valueType);

                return (TResponse)failureMethod.Invoke(null, new object[] { localized })!;
            }

            return response;
        }

        private string Translate(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return error;

            return readFromResourceFile.GetLocalizedMessage(error);
        }
    }
}
