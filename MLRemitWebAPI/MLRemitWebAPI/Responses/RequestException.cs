using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PayNearMe.Models
{
    public class RequestException : Exception
    {
        private string message;
        private int status;
        public int StatusCode { get { return status; } }
        public new string Message { get { return message; } }

        public RequestException(string message, int statusCode)
        {
            status = statusCode;
            this.message = message;
        }


    }
}
