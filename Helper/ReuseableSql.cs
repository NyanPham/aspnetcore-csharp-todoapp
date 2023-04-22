using System.Data;
using Dapper;
using todoapp.Data;
using todoapp.Models;

namespace todoapp.Helpers
{
    public class ReuseableSql
    {
        private readonly DataContextDapper _dapper;

        public ReuseableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(User userToAdd)
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
            sqlParameters.Add("@FirstNameParam", userToAdd.FirstName, DbType.String);
            sqlParameters.Add("@LastnameParam", userToAdd.LastName, DbType.String);
            sqlParameters.Add("@EmailParam", userToAdd.Email, DbType.String);
            sqlParameters.Add("@GenderParam", userToAdd.Gender, DbType.String);
            sqlParameters.Add("@ActiveParam", userToAdd.Active, DbType.Boolean);

            if (userToAdd.UserId != 0)
            {
                parameters += ", @UserId=@UserIdParam";
                sqlParameters.Add("@UserIdParam", userToAdd.UserId, DbType.Int32);
            }

            if (parameters.Length > 0)
            {
                sql += parameters;
            }

            return _dapper.ExecuteSqlWithParameters(sql, sqlParameters);
        }
    }
}