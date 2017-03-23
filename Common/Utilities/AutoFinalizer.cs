using System;
using System.Runtime.InteropServices;

namespace Common.Utilities
{
    /// <summary>
    /// Owns an action and keeps it alive outside the garbage collector until finalized
    /// </summary>
    /// <remarks>
    /// Just keep a reference to this in your class
    /// When you're GC'd this should be marked for finalize.
    /// Upon finalization, this will invoke the method that's been kept alive.
    /// If then there are no remaining references to the object, it will be GC'd on a later cycle
    /// </remarks>
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
