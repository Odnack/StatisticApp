using System;
using Api.Api.Authentication;
using Api.Api.Authentication.Dto;
using System.Threading.Tasks;
using BackendCore.Helpers;
using BackendCore.SharedInfrastructure;
using DbLayer.Context;
using DbLayer.Tables.Public;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace BackendCore.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ServiceSource _serviceSource;

        private int CurrUserId
        {
            get => _serviceSource.UserId;
            set => _serviceSource.UserId = value;
        }

        public AuthenticationService(ServiceSource serviceSource)
        {
            _serviceSource = serviceSource;
        }
        private async Task CheckUserEmail(DatabaseContext dbContext, string email)
        {
            bool exists = await dbContext.Users.AnyAsync(item => item.Email == email);
            if (exists)
                throw new ArgumentException(email);
        }
        private void UserFromDto(RegisterUserDto inData, bool newUser, User outUser)
        {
            outUser.Email = inData.Email;
            outUser.Password = SecurePasswordHasher.Hash(inData.Password);
        }
        public async Task<User> RegisterUserNoCredentialCheck(RegisterUserDto inData)
        {
            try
            {
                //no validation here due to base users generated on initial migration

                var dbContext = _serviceSource.CreateDbContext();

                await CheckUserEmail(dbContext, inData.Email);

                var newUser = new User();
                UserFromDto(inData, true, newUser);

                newUser = dbContext.Users.Add(newUser).Entity;

                await dbContext.SaveChangesAsync();

                inData.Id = newUser.Id;

                Log.Information("User '{0}' registered.", inData.Email);

                return newUser;
            }
            catch (ArgumentException ae)
            {
                return null;
            }
        }

        public async Task<IActionResult> SignUp(RegisterUserDto inData)
        {
            var result = await RegisterUserNoCredentialCheck(inData);
            if (result == null)
                return new NotFoundResult();
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> LogIn(LoginInDto inDto)
        {
            var dbContext = _serviceSource.CreateDbContext();
            var user = await dbContext.Users
                .FirstOrDefaultAsync(userItem => userItem.Email == inDto.Email);
            try
            {
                if (user == null)
                {
                    Log.Logger.Information("Email for user {0} failed. Email not found.", inDto.Email);
                    throw new ArgumentException("Email not found");
                }

                if (!SecurePasswordHasher.Verify(inDto.Password, user.Password))
                {
                    Log.Logger.Information("Login for user {0} failed. Wrong password.", inDto.Email);
                    throw new ArgumentException("Wrong password");
                }

                CurrUserId = user.Id;

                LoginOutDto result = new LoginOutDto
                {
                    UserId = user.Id,
                    Email = user.Email
                };

                Log.Logger.Information("User {0} logged in.", inDto.Email);

                return new OkObjectResult(result);
            }
            catch (ArgumentException ae)
            {
                return new NotFoundResult();
            }
        }
        public async Task<List<UserDto>> GetUsers()
        {
            var dbContext = _serviceSource.CreateDbContext();
            var userQuery = dbContext.Users;
            var userDtoQuery = SelectUserDto(userQuery, dbContext);
            return await userDtoQuery.ToListAsync();
        }
        public IQueryable<UserDto> SelectUserDto(IQueryable<User> query, DatabaseContext dbContext)
        {
            return query
                .Select(dbItem => new UserDto
                {
                    Id = dbItem.Id,
                    Email = dbItem.Email
                })
                .OrderBy(item => item.Email);
        }
    }
}