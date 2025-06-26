using System;
using Library.Repositories.Enums;

namespace Library.Exceptions
{
    public class UnknownDataBaseException : Exception
    {
        public override string Message { get; }
        
        public UnknownDataBaseException(string type) : base()
        {
            Message = $@"База данных ""{type}"" не поддерживается";
        }
    }
}