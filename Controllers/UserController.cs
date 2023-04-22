using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using todoapp.Data;
using todoapp.Dtos;
using todoapp.Models;
using todoapp.Helpers;

namespace todoapp.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class UserController : ControllerBase
    {
        DataContextDapper _dapper;
        ReuseableSql _reuseableSql;

        public UserController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _reuseableSql = new ReuseableSql(config);
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
            if (_reuseableSql.UpsertUser(userToUpsert))
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

