using postly.Utilities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace postly.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class postlyController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _testCollection;
        private readonly IConfiguration _configuration;

        public postlyController(IConfiguration configuration, IMongoClient mongoClient)
        {
            _configuration = configuration;

            var client = mongoClient;
            var userDatabase = client.GetDatabase("DBtest");
            _testCollection = userDatabase.GetCollection<BsonDocument>("users");
        }

        [HttpPost("testDtoEndpoint")]
        public IActionResult dtoEndpoint([FromBody] UserDto userForm)
        {
            string username = userForm.username;
            Log.LogEvent(_testCollection, username);
            return Ok(username);
        }
    }
}
