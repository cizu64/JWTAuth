using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using JWT;

namespace StakersClubAPI.JWT
{
    public class JwtManager
    {
        /// <summary>
        /// Create a Jwt with user information
        /// </summary>
        /// <param name="user">Pass a user object that has Email, Id and UserRoles properties </param>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        public static string CreateToken(dynamic user, out object dbUser)
        {
            //var secret = ConfigurationManager.AppSettings.Get("jwtKey");
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.UtcNow.AddHours(24) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);
            

            var payload = new Dictionary<string, object>
            {
                {"email", user.Email},
                {"userId", user.Id},
                {"role", user.UserRoles},
                {"sub", user.Id},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };

            var secret = ConfigurationManager.AppSettings.Get("jwtKey");
            dbUser = new { user.Email, user.Id };
            var token = JsonWebToken.Encode(payload, secret, JwtHashAlgorithm.HS256);
            return token;
        }

        /// <summary>
        /// Get the userid from the token if the token is not expired
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static int? GetUserIdFromToken(string token)
        {
            try
            {
                string key = WebConfigurationManager.AppSettings.Get("jwtKey");
                var jsonSerializer = new JavaScriptSerializer();
                var decodedToken = JsonWebToken.Decode(token, key);
                var data = jsonSerializer.Deserialize<Dictionary<string, object>>(decodedToken);
                object userId, exp;
                data.TryGetValue("userId", out userId);
                data.TryGetValue("exp", out exp);
                var validTo = FromUnixTime(long.Parse(exp.ToString()));
                if (DateTime.Compare(validTo, DateTime.UtcNow) <= 0)
                {
                    return null;
                }
                return (int)userId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
