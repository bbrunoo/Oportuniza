using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using System.Security.Claims;

namespace Oportuniza.Infrastructure.Services
{
    public class UserRegistrationFilterAttribute : ActionFilterAttribute
    {
        private readonly IUserRepository _userRepository;

        public UserRegistrationFilterAttribute(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userUniqueId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.HttpContext.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userUniqueId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var identityProvider = context.HttpContext.User.FindFirst("idp")?.Value ?? "AzureAD";

            // Tente encontrar um login existente
            var userLogin = await _userRepository.GetUserLoginAsync(identityProvider, userUniqueId);

            // Se o login não existir, crie o usuário e o login
            if (userLogin == null)
            {
                var email = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var name = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value
                         ?? context.HttpContext.User.FindFirst("given_name")?.Value
                         ?? "Usuário";

                var user = new User
                {
                    Email = email,
                    Name = name,
                    FullName = name,
                    IdentityProvider = identityProvider,
                    IdentityProviderId = userUniqueId
                };

                await _userRepository.AddAsync(user);

                var login = new UserLogin
                {
                    UserId = user.Id, // Use o Id do usuário recém-criado
                    IdentityProvider = identityProvider,
                    ProviderId = userUniqueId
                };

                await _userRepository.AddUserLoginAsync(login);
            }

            // O próximo filtro ou a ação do controller serão executados
            await next();
        }
    }
}