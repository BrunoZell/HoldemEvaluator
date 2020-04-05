using System;

namespace HoldemEvaluator
{
    // Todo: Rename NashException
    public class NashException : Exception
    {
        protected virtual string _standardMessage { get; set; } = "An error occurred.";
        private string _message = null;

        public NashException() :
            this(null, null)
        {

        }

        public NashException(string errorMessage)
            : this(errorMessage, null)
        {
        }

        public NashException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
            _message = errorMessage;
        }

        /// <summary>
        /// Exception.Message is overwritten by this to enable a standard error message by class.
        /// This Property returns the message passed in the constructor of the exception. If no error message was passed it will return a standard error message defined in each individual NashException class.
        /// </summary>
        new public string Message => String.IsNullOrEmpty(_message) ? _standardMessage : _message;
    }

    public class ParsingException : NashException
    {
        protected override string _standardMessage { get; set; } = "Error while parsing";
        public ParsingException()
        {

        }

        public ParsingException(string errorMessage)
            : base(errorMessage)
        {
        }

        public ParsingException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }

    public class NotARangeException : ParsingException
    {
        protected override string _standardMessage { get; set; } = "Input is not a valid range";
        public NotARangeException()
        {

        }

        public NotARangeException(string errorMessage)
            : base(errorMessage)
        {
        }

        public NotARangeException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }

    public class NotABoardException : ParsingException
    {
        protected override string _standardMessage { get; set; } = "Input is not a valid board";
        public NotABoardException()
        {

        }

        public NotABoardException(string errorMessage)
            : base(errorMessage)
        {
        }

        public NotABoardException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }

    public class NotACardException : ParsingException
    {
        protected override string _standardMessage { get; set; } = "Input is not a valid card";
        public NotACardException()
        {

        }

        public NotACardException(string errorMessage)
            : base(errorMessage)
        {
        }

        public NotACardException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }

    public class NotACollectionException : ParsingException
    {
        protected override string _standardMessage { get; set; } = "Input is not a valid card collection";
        public NotACollectionException()
        {

        }

        public NotACollectionException(string errorMessage)
            : base(errorMessage)
        {
        }

        public NotACollectionException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }

    public class NoHoleCardsException : ParsingException
    {
        protected override string _standardMessage { get; set; } = "Input is a invalid hole card representation";
        public NoHoleCardsException()
        {

        }

        public NoHoleCardsException(string errorMessage)
            : base(errorMessage)
        {
        }

        public NoHoleCardsException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }
}
