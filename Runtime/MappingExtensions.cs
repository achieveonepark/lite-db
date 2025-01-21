using System;

namespace Achieve.Database
{
    public static class MappingExtensions
    {
        public static TInfo ToInfo<TInfo, TData>(this TData data)
            where TInfo : IMappableFrom<TData>, new()
        {
            var info = new TInfo();
            info.MapFrom(data);
            return info;
        }
        
        public static TInfo ToInfo<TInfo, TData>(
            this TData data,
            Func<TData, TInfo> factory) where TInfo : IMappableFrom<TData>
        {
            var info = factory(data);
            info.MapFrom(data);
            return info;
        }
    }
}