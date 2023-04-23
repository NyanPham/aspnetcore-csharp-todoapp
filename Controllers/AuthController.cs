using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using todoapp.Data;
using todoapp.Dtos;
using todoapp.Helpers;
using todoapp.Models;

namespace todoapp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class AuthController : ControllerBase
    {
        DataContextDapper _dapper;
        ReuseableSql _reuseableSql;
        AuthHelper _authHelper;

        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
            _reuseableSql = new ReuseableSql(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password != userForRegistration.PasswordConfirm)
                return StatusCode(401, "Passwords do not match!");

            string sqlCheckExisting = @"
                SELECT Email FROM TodoAppSchema.Auth
                    WHERE Email = '" + userForRegistration.Email + "'";

            int exisitingCount = _dapper.ExecuteSqlWithCount(sqlCheckExisting);

            if (exisitingCount > 0)
                return StatusCode(401, "The user with that email already exists!");

            byte[] passwordSalt = new byte[120 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

            string sqlAddAuth = @"
                EXEC TodoAppSchema.spAuth_Register
                    @Email=@EmailParam,
                    @PasswordHash=@PasswordHashParam,
                    @PasswordSalt=@PasswordSaltParam
            ";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@EmailParam", userForRegistration.Email, DbType.String);
            sqlParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);

            Console.WriteLine(sqlParameters.Get<string>("@EmailParam"));

            if (!_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                return StatusCode(401, "Failed to register!");

            User userToAdd = new User()
            {
                Email = userForRegistration.Email,
                FirstName = userForRegistration.FirstName,
                LastName = userForRegistration.LastName,
                Gender = userForRegistration.Gender,
                Active = true,
            };

            if (!_reuseableSql.UpsertUser(userToAdd))
            {
                string sqlRemoveAuth = @"
                    EXEC TodoAppSchema.spAuth_Delete
                        @Email=@EmailParam
                ";

                DynamicParameters removeAuthParameters = new DynamicParameters();
                removeAuthParameters.Add("@EmailParam", userToAdd.Email, DbType.String);
                _dapper.ExecuteSqlWithParameters(sqlRemoveAuth, removeAuthParameters);

                return StatusCode(401, "Failed to add user after register!");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlConfirmation = @"
                EXEC TodoAppSchema.spAuth_UserConfirmation
                    @Email=@ConfirmEmailParam
            ";
            DynamicParameters confirmParameters = new DynamicParameters();
            confirmParameters.Add("@ConfirmEmailParam", userForLogin.Email, DbType.String);

            UserForConfirmationDto userForConfirmation = _dapper.LoadDataSingleWithParameters<UserForConfirmationDto>(sqlConfirmation, confirmParameters);
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userForConfirmation.PasswordHash[i])
                    return StatusCode(400, "Incorrect Password");
            }
            string sqlLoggedEmail = @"
                EXEC TodoAppSchema.spAuth_UserLogin
                    @Email='" + userForLogin + "'";

            int userId = _dapper.LoadDataSingle<int>(sqlLoggedEmail);

            string token = _authHelper.CreateToken(userId);

            return Ok(new Dictionary<string, string>(){
                {"token", token}
            });
        }
    }
}

