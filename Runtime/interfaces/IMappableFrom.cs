namespace Achieve.Database
{
    public interface IMappableFrom<TData>
    {
        void MapFrom(TData data);
    }
}