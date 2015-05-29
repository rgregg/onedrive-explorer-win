using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    public static class ErrorExtensionMethods
    {

        public static ODErrorDetail InnerMostErrorDetail(this ODError error)
        {
            ODErrorDetail innerMostError = error.Error;
            while (innerMostError.InnerError != null)
            {
                innerMostError = innerMostError.InnerError;
            }

            return innerMostError;
        }

        public static string Message(this ODError error)
        {
            ODErrorDetail innerMostError = error.Error;
            while (innerMostError.InnerError != null && !string.IsNullOrEmpty(innerMostError.InnerError.Message))
            {
                innerMostError = innerMostError.InnerError;
            }

            return innerMostError.Message;
        }


        public static bool IsErrorCode(this ODError error, string expectedErrorCode)
        {
            ODErrorDetail innerMostError = error.Error;
            while (innerMostError.InnerError != null)
            {
                string errorCode = innerMostError.Code;
                if (errorCode.Equals(expectedErrorCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                
                innerMostError = innerMostError.InnerError;
            }

            return false;
        }


    }
}
