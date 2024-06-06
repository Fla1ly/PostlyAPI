using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace postly.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class postlyController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _userCollection;
        private readonly IMongoCollection<BsonDocument> _postCollection;
        private readonly IMongoCollection<BsonDocument> _draftCollection;
        private readonly IMongoCollection<BsonDocument> _commentCollection;
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
            _commentCollection = postDatabase.GetCollection<BsonDocument>("comments");
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

            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate a 256-bit (32-byte) key
            var key = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, loginForm.username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }

        [HttpPost("createPost")]
        public IActionResult CreatePost([FromBody] PostDto postDto)
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

        [HttpPost("createDraft")]
        public IActionResult CreateDraft([FromBody] PostDto postDto)
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

        [HttpPost("editBlog/{postId}")]
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

        [HttpPost("createComment/{postId}")]
        public IActionResult CreateComment(string postId, [FromBody] CommentDto commentDto)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return BadRequest(new { message = "Blog ID not provided" });
            }

            commentDto.postId = postId;

            var commentDocument = new BsonDocument
            {
                { "commentId", commentDto.commentId },
                { "made_by", commentDto.made_by },
                { "postId", commentDto.postId },
                { "content", commentDto.content },
                { "likes", commentDto.likes },
                { "date Created", DateTime.Now.ToString("MM-dd-yyyy HH:mm") },
            };

            _commentCollection.InsertOne(commentDocument);

            _logger.LogInformation("New comment created. Made by: {made_by}, BlogId: {blogId}", commentDto.made_by, commentDto.postId);

            return Ok(new { message = "Comment created successfully", made_by = commentDto.made_by, blogId = commentDto.postId });
        }

        [HttpGet("fetchComments/{postId}")]
        public IActionResult FetchComments(string postId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("postId", postId);
            var comments = _commentCollection.Find(filter).ToList();

            var commentList = new List<object>();
            foreach (var comment in comments)
            {
                commentList.Add(new
                {
                    commentId = comment.GetValue("commentId").AsString,
                    made_by = comment.GetValue("made_by").AsString,
                    postId = comment.GetValue("postId").AsString,
                    content = comment.GetValue("content").AsString,
                    likes = comment.GetValue("likes").AsInt32,
                    dateCreated = comment.GetValue("date Created").AsString,
                });
            }

            return Ok(commentList);
        }

        [HttpGet("getBlogCount/{username}")]
        public IActionResult GetBlogCount(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Username not provided" });
            }

            var filter = Builders<BsonDocument>.Filter.Eq("author", username);
            var count = _postCollection.CountDocuments(filter);

            return Ok(new { username = username, blogCount = count });
        }
    }
}
