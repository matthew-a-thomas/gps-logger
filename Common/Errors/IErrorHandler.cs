using System;

namespace Common.Errors
{
    public interface IErrorHandler
    {
        object Handle(Exception e);
    }
}
