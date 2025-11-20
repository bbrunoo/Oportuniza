//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Oportuniza.Domain.Interfaces;
//using Oportuniza.Domain.Models;
//using System.Security.Claims;

//namespace Oportuniza.Infrastructure.Services
//{
//    public class UserRegistrationFilterAttribute : ActionFilterAttribute
//    {
//        private readonly IUserRepository _userRepository;

//        public UserRegistrationFilterAttribute(IUserRepository userRepository)
//        {
//            _userRepository = userRepository;
//        }

//        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//        {
//            var keycloakId = context.HttpContext.User.FindFirst("sub")?.Value;

//            if (string.IsNullOrEmpty(keycloakId))
//            {
//                context.Result = new UnauthorizedResult();
//                return;
//            }

//            var user = await _userRepository.GetUserByKeycloakIdAsync(keycloakId);

//            if (user == null)
//            {
//                var email = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

//                var name = context.HttpContext.User.FindFirst("given_name")?.Value
//                               ?? context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

//                if (string.IsNullOrEmpty(name))
//                {
//                    name = GenerateNameFromEmail(email);
//                }

//                user = new User
//                {
//                    Id = Guid.Parse(keycloakId),
//                    KeycloakId = keycloakId,
//                    Email = email,
//                    Name = name,
//                    FullName = name,
//                    IsProfileCompleted = false,
//                };

//                await _userRepository.AddAsync(user);
//            }

//            await next();
//        }

//        private string GenerateNameFromEmail(string? email)
//        {
//            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
//            {
//                return "Usuário";
//            }

//            string name = email.Split('@')[0];

//            if (name.Length > 0)
//            {
//                name = char.ToUpper(name[0]) + name.Substring(1);
//            }

//            return name;
//        }
//    }
//}