using System;

namespace PortfolioApi
{
    public class ApiException : Exception
    {
        public static int Unknown = 0;
        public static int NotAuthorized = 1;
        public static int BadHttpStatus = 2;
        public static int EmptyResponse = 3;

        public ApiException(ErrorStage errorStage, string message, int errorCode, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ErrorStage = errorStage;
        }

        public ErrorStage ErrorStage { get; }

        public int ErrorCode { get; set; }

        public string ForDisplay => $"{ErrorCode}: {Message}";

        public override string ToString()
        {
            return $"{ErrorCode}: {base.ToString()}";
        }
    }

    public enum ErrorStage
    {
        Channel,
        Application
    }
}