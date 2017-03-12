using System;

namespace Common.Utilities
{
    public class Ephemeron
    {
        private readonly Action _callback;

        public Ephemeron(Action callback)
        {
            _callback = callback;
        }

        ~Ephemeron()
        {
            _callback();
        }
    }
}
