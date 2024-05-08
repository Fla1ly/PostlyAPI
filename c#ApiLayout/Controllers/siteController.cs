using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace postly.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class postlyController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _userCollection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<postlyController> _logger;


        public postlyController(IConfiguration configuration, IMongoClient mongoClient, ILogger<postlyController> logger)
        {
            _configuration = configuration;

            var client = mongoClient;
            var userDatabase = client.GetDatabase("userDB");
            _userCollection = userDatabase.GetCollection<BsonDocument>("users");
            _logger = logger;
        }

        [HttpPost("userRegistration")]
        public IActionResult dtoEndpoint([FromBody] UserDto userForm)
        {
            string hashedPassword = PasswordHasher.HashPassword(userForm.password);

            var userDocument = new BsonDocument
            {
                { "username", userForm.username },
                { "email", userForm.email },
                { "password", hashedPassword },
                { "userID", userForm.userId },
                { "Date Created", DateTime.Now.ToString("MM-dd-yyyy HH:mm")},
            };

            _userCollection.InsertOne(userDocument);

            _logger.LogInformation("New user created. username: {username}, email: {email}, password: {password}", userForm.username, userForm.email, hashedPassword);

            return Ok(new { message = "created user", username = userForm.username, email = userForm.email });
        }
    }
}
