using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OMS.Auth
{
    public class AuthBuilder
    {
        public AuthBuilder(Type user, IServiceCollection services)
        {
            UserType = user;
            Services = services;
        }

        public Type UserType { get; private set; }
        public IServiceCollection Services { get; private set; }

        private AuthBuilder AddScoped(Type serviceType, Type concreteType)
        {
            Services.AddScoped(serviceType, concreteType);
            return this;
        }

        public virtual AuthBuilder AddUserValidator<TValidator>() where TValidator : class
            => AddScoped(typeof(IUserValidator<>).MakeGenericType(UserType), typeof(TValidator));

        public virtual AuthBuilder AddClaimsPrincipalFactory<TFactory>() where TFactory : class
            => AddScoped(typeof(IUserClaimsPrincipalFactory<>).MakeGenericType(UserType), typeof(TFactory));

        public virtual AuthBuilder AddErrorDescriber<TDescriber>() where TDescriber : AuthErrorDescriber
        {
            Services.AddScoped<AuthErrorDescriber, TDescriber>();
            return this;
        }

        public virtual AuthBuilder AddPasswordValidator<TValidator>() where TValidator : class
            => AddScoped(typeof(IPasswordValidator<>).MakeGenericType(UserType), typeof(TValidator));

        public virtual AuthBuilder AddUserStore<TStore>() where TStore : class
            => AddScoped(typeof(IUserStore<>).MakeGenericType(UserType), typeof(TStore));

        public virtual AuthBuilder AddUserManager<TUserManager>() where TUserManager : class
        {
            Type userManagerType = typeof(UserManager<>).MakeGenericType(UserType);
            Type customType = typeof(TUserManager);
            if (!userManagerType.IsAssignableFrom(customType))
                throw new InvalidOperationException("Invalid user manager! Type of: "+customType.Name);

            if (userManagerType != customType)
                Services.AddScoped(customType, services => services.GetRequiredService(userManagerType));

            return AddScoped(userManagerType, customType);
        }

        public virtual AuthBuilder AddPersonalDataProtection<TProtector, TKeyRing>()
            where TProtector : class,ILookupProtector
            where TKeyRing : class,ILookupProtectorKeyRing
        {
            Services.AddSingleton<IPersonalDataProtector, DefaultPersonalDataProtector>();
            Services.AddSingleton<ILookupProtector, TProtector>();
            Services.AddSingleton<ILookupProtectorKeyRing, TKeyRing>();
            return this;
        }

        public virtual AuthBuilder AddUserConfirmation<TUserConfirmation>() where TUserConfirmation : class
            => AddScoped(typeof(IUserConfirmation<>).MakeGenericType(UserType), typeof(TUserConfirmation));
    }
}