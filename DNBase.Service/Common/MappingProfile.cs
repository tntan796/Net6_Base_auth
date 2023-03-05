using AutoMapper;
using DNBase.ViewModel;
using System.Reflection;
using DNBase.DataLayer.EF.Entities;
using DNBase.DataLayer.EF.Mongo.Entities;

namespace DNBase.Services
{
    public class RequestMappingProfile : AutoMapper.Profile
    {
        public RequestMappingProfile()
        {
            CreateMap<CategoryCreateRequestModel, Category>().IgnoreAllNonExisting();
            CreateMap<CategoryItemCreateRequestModel, CategoryItem>().IgnoreAllNonExisting();
            CreateMap<UserCreateRequestModel, AppUser>().IgnoreAllNonExisting();
            CreateMap<CRUD_MGRequestModel, CRUD_MG>().IgnoreAllNonExisting();
        }
    }

    public class ResponeMappingProfile : AutoMapper.Profile
    {
        public ResponeMappingProfile()
        {
            CreateMap<AppUser, UserRespondModel>().IgnoreAllNonExisting();
        }
    }

    public static class AutoMapperHeper
    {
        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var sourceType = typeof(TSource);
            var destinationProperties = typeof(TDestination).GetProperties(flags);

            foreach (var property in destinationProperties)
            {
                if (sourceType.GetProperty(property.Name, flags) == null)
                {
                    expression.ForMember(property.Name, opt => opt.Ignore());
                }
            }
            return expression;
        }
    }
}