using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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
