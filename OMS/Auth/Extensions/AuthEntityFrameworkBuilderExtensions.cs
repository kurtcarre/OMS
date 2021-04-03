using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OMS.Auth;
using OMS.Auth.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthEntityFrameworkBuilderExtensions
    {
        public static AuthBuilder AddEntityFrameworkStores<TContext>(this AuthBuilder builder) where TContext : DbContext
        {
            AddStores(builder.Services, builder.UserType, typeof(TContext));
            return builder;
        }

        private static void AddStores(IServiceCollection services, Type userType, Type contextType)
        {
            Type authUserType = FindGenericBaseType(userType, typeof(User<>));
            if (authUserType == null)
                throw new InvalidOperationException("User type is not a user!");

            Type keyType = authUserType.GenericTypeArguments[0];

            Type userStoreType;
            Type authContext = FindGenericBaseType(contextType, typeof(AuthUserContext<,,,>));
            if (authContext == null)
                userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, contextType, keyType);

            else
            {
                userStoreType = typeof(UserStore<,,,,>).MakeGenericType(userType, contextType,
                    authContext.GenericTypeArguments[1],
                    authContext.GenericTypeArguments[2],
                    authContext.GenericTypeArguments[3]);
            }

            services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
        }

        private static Type FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            Type type = currentType;
            while(type != null)
            {
                Type genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                    return type;
                type = type.BaseType;
            }
            return null;
        }
    }
}