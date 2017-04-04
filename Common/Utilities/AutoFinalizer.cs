using System;
using System.Runtime.InteropServices;

namespace Common.Utilities
{
    /// <summary>
    /// Owns an action and keeps it alive outside the garbage collector until finalized
    /// </summary>
    public class AutoFinalizer
    {
        private GCHandle _handle;

        /// <summary>
        /// Creates a new AutoFinalizer which invokes the given method when this AutoFinalizer is finalized
        /// </summary>
        public AutoFinalizer(Action finalization)
        {
            if (finalization == null) throw new ArgumentNullException();

            _handle = GCHandle.Alloc(finalization);
        }

        ~AutoFinalizer()
        {
            try
            {
                var finalization = _handle.Target as Action;
                if (finalization != null)
                    try
                    {
                        finalization();
                    }
                    finally
                    {
                        _handle.Free();
                    }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception) { }
        }
    }
}
