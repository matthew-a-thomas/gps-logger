using System.Threading.Tasks;
using AutoMapper;

namespace Common.Serialization
{
    public class MapperTranslator<TFrom, TTo> : ITranslator<TFrom, TTo> where TTo : new()
    {
        private readonly IMapper _mapper;

        public MapperTranslator()
        {
            _mapper = new MapperConfiguration(c =>
            {
                c.AllowNullCollections = true;
                c.CreateMissingTypeMaps = true;
            }).CreateMapper();
        }

        public async Task<TTo> TranslateAsync(TFrom thing) => await Task.Run(() => _mapper.Map<TTo>(thing));
    }
}