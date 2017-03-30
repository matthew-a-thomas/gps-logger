using Common.Utilities;

namespace Common.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IFactoryExtensions
    {
        private class TwoStepFactory<TOptions, TIntermediate, TFinal> : IFactory<TOptions, TFinal>
        {
            private readonly IFactory<TOptions, TIntermediate> _firstFactory;
            private readonly IFactory<TIntermediate, TFinal> _secondFactory;

            public TwoStepFactory(IFactory<TOptions, TIntermediate> firstFactory, IFactory<TIntermediate, TFinal> secondFactory)
            {
                _firstFactory = firstFactory;
                _secondFactory = secondFactory;
            }

            public TFinal Create(TOptions options)
            {
                var intermediate = _firstFactory == null ? default(TIntermediate) : _firstFactory.Create(options);
                var final = _secondFactory == null ? default(TFinal) : _secondFactory.Create(intermediate);
                return final;
            }
        }

        /// <summary>
        /// Creates a factory that uses intermediate factories to transform objects along the way
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="TIntermediate"></typeparam>
        /// <typeparam name="TFinal"></typeparam>
        /// <param name="factory"></param>
        /// <param name="secondFactory"></param>
        /// <returns></returns>
        public static IFactory<TOptions, TFinal> ChainInto<TOptions, TIntermediate, TFinal>(
            this IFactory<TOptions, TIntermediate> factory,
            IFactory<TIntermediate, TFinal> secondFactory
        ) =>
            new TwoStepFactory<TOptions, TIntermediate, TFinal>(factory, secondFactory);
    }
}
