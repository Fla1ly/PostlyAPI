using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace postly.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class postlyController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _userCollection;
        private readonly IMongoCollection<BsonDocument> _postCollection;
        private readonly IMongoCollection<BsonDocument> _draftCollection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<postlyController> _logger;


        public postlyController(IConfiguration configuration, IMongoClient mongoClient, ILogger<postlyController> logger)
        {
            _configuration = configuration;

            var client = mongoClient;
            var userDatabase = client.GetDatabase("userDB");
            var postDatabase = client.GetDatabase("BlogDB");
            _userCollection = userDatabase.GetCollection<BsonDocument>("users");
            _postCollection = postDatabase.GetCollection<BsonDocument>("blogs");
            _draftCollection = postDatabase.GetCollection<BsonDocument>("drafts");
            _logger = logger;
        }

        [HttpPost("userRegistration")]
        public IActionResult Register([FromBody] UserDto userForm)
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

        [HttpPost("userLogin")]
        public IActionResult Login([FromBody] LoginDto loginForm)
        {
            var user = _userCollection.Find(Builders<BsonDocument>.Filter.Eq("username", loginForm.username)).FirstOrDefault();
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            string storedHashedPassword = user["password"].AsString;
            if (!PasswordHasher.VerifyPassword(loginForm.password, storedHashedPassword))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("createPost")]
        public IActionResult CreatePost([FromBody] PostDto postDto )
        {
            {
                var postDocument = new BsonDocument
                {
                    { "postId", postDto.postId },
                    { "author", postDto.author },
                    { "category", postDto.category },
                    { "title", postDto.title },
                    { "subtitle", postDto.subtitle },
                    { "description", postDto.description },
                    { "date Created", DateTime.Now.ToString("MM-dd-yyyy HH:mm") },
                    { "status", postDto.status },
                    { "visibility", postDto.visibility },
                };

                _postCollection.InsertOne(postDocument);

                _logger.LogInformation("New blog post created. Title: {title}, Description: {description}", postDto.title, postDto.description);

                return Ok(new { message = "Blog post created successfully", title = postDto.title, description = postDto.description });
            }
        }

        [HttpPost("createDraft")]
        public IActionResult CreateDraft([FromBody] PostDto postDto)
        {
            {
                var draftDocument = new BsonDocument
                {
                    { "postId", postDto.postId },
                    { "author", postDto.author },
                    { "category", postDto.category },
                    { "title", postDto.title },
                    { "subtitle", postDto.subtitle },
                    { "description", postDto.description },
                    { "date Created", DateTime.Now.ToString("MM-dd-yyyy HH:mm") },
                    { "status", postDto.status },
                    { "visibility", postDto.visibility },
                };

                _draftCollection.InsertOne(draftDocument);

                _logger.LogInformation("New draft post created. Title: {title}, Description: {description}", postDto.title, postDto.description);

                return Ok(new { message = "Blog draft created successfully", title = postDto.title, description = postDto.description });
            }
        }

        [HttpGet("fetchBlogs")]
        public IActionResult FetchBlogs()
        {
            var blogs = _postCollection.Find(new BsonDocument()).ToList();

            _logger.LogInformation("Blogs retrieved");

            var blogList = new List<object>();
            foreach (var blog in blogs)
            {
                blogList.Add(new
                {
                    id = blog.GetValue("postId").AsString,
                    author = blog.GetValue("author").AsString,
                    category = blog.GetValue("category").AsString,
                    title = blog.GetValue("title").AsString,
                    subtitle = blog.GetValue("subtitle").AsString,
                    description = blog.GetValue("description").AsString,
                    dateCreated = blog.GetValue("date Created").AsString,
                });
            }
            return Ok(blogList);
        }

        [HttpGet("fetchBlog/{postId}")]
        public IActionResult FetchBlog(string postId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("postId", postId);
            var blog = _postCollection.Find(filter).FirstOrDefault();

            if (blog == null)
            {
                return NotFound(new { message = "Blog not found" });
            }

            var blogDetails = new
            {
                Author = blog.GetValue("author").AsString,
                Category = blog.GetValue("category").AsString,
                createdDate = blog.GetValue("date Created").AsString,
                Title = blog.GetValue("title").AsString,
                Subtitle = blog.GetValue("subtitle").AsString,
                Description = blog.GetValue("description").AsString,
                DateCreated = blog.GetValue("date Created").AsString,
            };

            _logger.LogInformation("Blog post retrieved. PostId: {postId}", postId);

            return Ok(blogDetails);
        }

        [HttpGet("fetchUserBlogs/{username}")]
        public IActionResult FetchUserBlogs(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Username not provided" });
            }

            var filter = Builders<BsonDocument>.Filter.Eq("author", username);
            var blogs = _postCollection.Find(filter).ToList();

            var blogList = new List<object>();
            foreach (var blog in blogs)
            {
                blogList.Add(new
                {
                    id = blog.GetValue("postId").AsString,
                    author = blog.GetValue("author").AsString,
                    category = blog.GetValue("category").AsString,
                    title = blog.GetValue("title").AsString,
                    subtitle = blog.GetValue("subtitle").AsString,
                    description = blog.GetValue("description").AsString,
                    dateCreated = blog.GetValue("date Created").AsString,
                    status = blog.GetValue("status").AsString,
                    visibility = blog.GetValue("visibility").AsString,
                });
            }

            return Ok(blogList);
        }

        [HttpPut("editBlog/{postId}")]
        public IActionResult EditBlog(string postId, [FromBody] UpdatePostDto updatePostDto)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("postId", postId);
            var update = Builders<BsonDocument>.Update
                .Set("title", updatePostDto.title)
                .Set("subtitle", updatePostDto.subtitle)
                .Set("description", updatePostDto.description)
                .Set("category", updatePostDto.category)
                .Set("visibility", updatePostDto.visibility);

            var result = _postCollection.UpdateOne(filter, update);

            if (result.MatchedCount == 0)
            {
                return NotFound(new { message = "Blog post not found" });
            }

            _logger.LogInformation("Blog post updated. PostId: {postId}", postId);
            return Ok(new { message = "Blog post updated successfully" });
        }
    }
}
