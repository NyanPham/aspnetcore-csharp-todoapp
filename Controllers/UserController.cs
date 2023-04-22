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

        [HttpGet("Users")]
        public IEnumerable<User> GetUsers()
        {
            string sql = @"
                SELECT 
                    [UserId],
                    [FirstName],
                    [LastName],
                    [Email], 
                    [Gender], 
                    [Active]
                 FROM TodoAppSchema.Users
            ";

            return _dapper.LoadData<User>(sql);
        }

        [HttpGet("Users/{userId}")]
        public User GetUser(int userId)
        {
            string sql = @"
                SELECT 
                    [UserId],
                    [FirstName],
                    [LastName],
                    [Email], 
                    [Gender], 
                    [Active]
                FROM TodoAppSchema.Users
                    WHERE UserId = " + userId.ToString();

            return _dapper.LoadDataSingle<User>(sql);
        }

        [HttpPost("Users")]
        public IActionResult AddUser(UserToAddDto userToAdd)
        {
            string sql = @"
                INSERT INTO TodoAppSchema.Users (
                    [FirstName]
                    ,[LastName]
                    ,[Email]
                    , [Gender]
                    , [Active]
                )   VALUES (" +
                "'" + userToAdd.FirstName +
                "', '" + userToAdd.LastName +
                "', '" + userToAdd.Email +
                "', '" + userToAdd.Gender +
                "', 1)";

            if (!_dapper.ExecuteSql(sql))
                return StatusCode(401, "Failed to add user!");

            return Ok();
        }

        [HttpPut("Users/{userId}")]
        public IActionResult EditUser(int userId, UserToEditDto userToEdit)
        {
            string sql = @"
                UPDATE TodoAppSchema.Users
                    SET FirstName = '" + userToEdit.FirstName +
                        "', LastName = '" + userToEdit.LastName +
                        "', Email = '" + userToEdit.Email +
                        "', Gender = '" + userToEdit.Gender +
                        "', Active = '" + userToEdit.Active +
                    "' WHERE UserId = " + userId.ToString();

            Console.WriteLine(sql);

            if (!_dapper.ExecuteSql(sql))
                return StatusCode(400, "Failed to edit user!");

            return Ok();
        }

        [HttpDelete("Users/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string sql = @"
                DELETE TodoAppSchema.Users
                    WHERE UserId = " + userId.ToString();

            if (!_dapper.ExecuteSql(sql))
                return StatusCode(400, "Failed to delete user!");

            return Ok();
        }
    }
}

