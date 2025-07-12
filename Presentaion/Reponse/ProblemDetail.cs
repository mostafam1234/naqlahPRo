using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentaion.Reponse
{
    public class ProblemDetail
    {
        public int statusCode { get; set; }
        public string errorMessage { get; set; }
        public object additioanlData { get; set; }

        public static ProblemDetail CreateProblemDetail(string ErrorMessage)
        {
            return new ProblemDetail
            {
                statusCode = StatusCodes.Status400BadRequest,
                errorMessage = ErrorMessage
            };
        }


        public static ProblemDetail CreateProblemDetail(string ErrorMessage, object additioanlData)
        {
            return new ProblemDetail
            {
                statusCode = StatusCodes.Status400BadRequest,
                errorMessage = ErrorMessage,
                additioanlData = additioanlData
            };
        }
    }
}
