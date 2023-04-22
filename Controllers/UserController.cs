using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using todoapp.Data;
using todoapp.Dtos;
using todoapp.Models;

namespace todoapp.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class UserController : ControllerBase
    {
        DataContextDapper _dapper;

        public UserController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Users/{userId}")]
        public IEnumerable<User> GetUsers(int userId = 0)
        {
            string sql = @"
                EXEC TodoAppSchema.spUsers_Get
            ";

            string parameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (userId != 0)
            {
                parameters += "@UserId=@UserIdParam";
                sqlParameters.Add("UserIdParam", userId, DbType.Int32);
            }

            if (parameters.Length > 0)
                sql += parameters;

            return _dapper.LoadDataWithParameters<User>(sql, sqlParameters);
        }

        [HttpPost("Users")]
        public IActionResult UpsertUser(User userToUpsert)
        {
            string sql = @"
                EXEC TodoAppSchema.sp_Users_Upsert
                    @FirstName=@FirstNameParam,
                    @LastName=@LastnameParam,
                    @Email=@EmailParam,
                    @Gender=@GenderParam,
                    @Active=@ActiveParam
            ";
            string parameters = "";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@FirstNameParam", userToUpsert.FirstName, DbType.String);
            sqlParameters.Add("@LastnameParam", userToUpsert.LastName, DbType.String);
            sqlParameters.Add("@EmailParam", userToUpsert.Email, DbType.String);
            sqlParameters.Add("@GenderParam", userToUpsert.Gender, DbType.String);
            sqlParameters.Add("@ActiveParam", userToUpsert.Active, DbType.Boolean);


            if (userToUpsert.UserId != 0)
            {
                parameters += ", @UserId=@UserIdParam";
                sqlParameters.Add("@UserIdParam", userToUpsert.UserId, DbType.Int32);
            }

            if (parameters.Length > 0)
            {
                sql += parameters;
            }

            Console.WriteLine(sql);

            if (!_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
                return StatusCode(401, "Failed to upsert user!");

            return Ok();
        }

        [HttpDelete("Users/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string sql = @"TodoAppSchema.spUsers_Delete 
                @UserId=@UserIdParam
            ";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("UserIdParam", userId, DbType.Int32);

            if (!_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
                return StatusCode(400, "Failed to delete user!");

            return Ok();
        }
    }
}

