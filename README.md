## JWTAuth
JWT authentication in asp.net web api

## Add the jwt key in the web.config
```<add key="jwtKey" value="cXdlcnR5dWlvcGFzZGZnaGprbHp4Y3Zibm0xMjM0NTY=" />```

## Add the message handler to the request pipeline in the WebApiConfig class
```
   public static void Register(HttpConfiguration config)
   {       
      config.MessageHandlers.Add(new JwtAuthHandler()); //use custom jwt authentication
   }
```   
## Create the Login action method in the AccountController class
    ```
        [HttpPost]
        [Route("oauth/token")]
        public async Task<IHttpActionResult> Login(LoginDto dto)
        {
            try
            {
                //can login with email               
                if (dto.Email != "johndoe@gmail.com" && dto.Password!"johndoe1")
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        Message = "Invalid credentials",
                        Err_Code = ErrorCode.BadEmailOrPassword,
                        Succeeded = false
                    });
                }
                var userDto = new DTO.User
                {
                    Email = dto.Email,
                    Id = 1,
                    UserRoles = string.Join(",", new[]{"Admin","User"});
                };
                object dbUser;
                var token = JwtManager.CreateToken(userDto, out dbUser);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    ```     
# Getting access to a protected resource

        ```
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("user/{id:int}")]
        public IHttpActionResult GetUser(int Id)
        {
           //Get data using some repository pattern with EF
           var user = repo.GetUser(u=>u.Id==Id);
           return Ok(user);
        }
        ```
Note, if your not authorized to call the GetUser endpoint, you would see a 403 response from the server. But if you pass the token return after login in, to the request header with the right credentials, 403 would not occur.

You also don't need to worry about token expiration and re-validation, the JwtManager class already has that in place. If the token becomes invalid, it would bounce back to a 403 response meaning authentication is required.
