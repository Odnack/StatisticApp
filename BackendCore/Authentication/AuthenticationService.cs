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

        public async Task<LoginOutDto> Login(LoginInDto inDto)
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

                return result;
            }
            catch (ArgumentException ae)
            {
                return null;
            }
        }

        public async Task<int> RegisterUser(RegisterUserDto inData)
        {
            return await RegisterUserNoCredentialCheck(inData);
        }
        public async Task<int> RegisterUserNoCredentialCheck(RegisterUserDto inData)
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

                return newUser.Id;
            }
            catch (ArgumentException ae)
            {
                return -1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        public async Task<List<UserDto>> GetUsers()
        {
            var dbContext = _serviceSource.CreateDbContext();
            var userQuery = dbContext.Users;
            var userDtoQuery = SelectUserDto(userQuery, dbContext);
            return await userDtoQuery.ToListAsync();
        }

        public async Task<UserDto> GetUser(int userId)
        {
            var dbContext = _serviceSource.CreateDbContext();
            var userQuery = dbContext.Users.Where(x => x.Id == userId);
            var userDtoQuery = SelectUserDto(userQuery, dbContext);
            return await userDtoQuery.FirstOrDefaultAsync();
        }
        public IQueryable<UserDto> SelectUserDto(IQueryable<User> query, DatabaseContext dbContext)
        {
            return query
                .Select(dbItem => new UserDto
                {
                    Id = dbItem.Id,
                    Email = dbItem.Email
                });
        }
    }
}