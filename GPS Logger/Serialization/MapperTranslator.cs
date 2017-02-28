using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace GPS_Logger.Serialization
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

        public TTo Translate(TFrom thing) => _mapper.Map<TTo>(thing);
    }
}