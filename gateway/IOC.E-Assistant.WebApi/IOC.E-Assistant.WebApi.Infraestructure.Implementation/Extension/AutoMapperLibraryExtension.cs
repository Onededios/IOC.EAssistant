using AutoMapper;

namespace IOC.E_Assistant.Infraestructure.Implementation.Extension;
public static class AutoMapperLibraryExtension
{
    static AutoMapperLibraryExtension()
    {
        Mapper = new MapperConfiguration(mc =>
        {
            mc.AddProfile<>();
        }).CreateMapper();
    }

    static T MapTo<T>(this object model) where T : class => Mapper.Map<T>(model);
    static IEnumerable<T> MapTo<T>(this IEnumerable<object> source) where T : class => Mapper.Map<T>(model);

    internal static IMapper Mapper { get; }
}
