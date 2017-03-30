using System;

namespace Common.Utilities
{
    public class Factory<TOptions, TInstance> : IFactory<TOptions, TInstance>
    {
        private readonly Func<TOptions, TInstance> _factory;
        public Factory(Func<TOptions, TInstance> factory)
        {
            _factory = factory;
        }

        public TInstance Create(TOptions options) => _factory(options);
    }
}
