using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation; // For password hashing
using Microsoft.IdentityModel.Tokens;
using MAPI.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;


        // Constructor to get the configuration
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        [HttpGet("ValidateToken")]
        public async Task<IActionResult> ValidateToken()
        {
            // Retrieve the token from the Authorization header
            // Retrieve the Authorization header(which is in the form of 'Bearer <token>')
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Authorization header is missing.");
            }

            // Check if the Authorization header starts with 'Bearer ' and extract the token
           
                var token = authorizationHeader.Substring(7).Trim(); 

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Token is missing or invalid.");
                }



                // Validate the token
                
                var user = await ValidateTokenAsync(token);

                if (user != null)
                {
                    return Ok(); // Token is expired, invalid, or user not found
                }
                else
                {
                    return Unauthorized();
                }
                
            
        }


        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] LoginModel login)
        {
            // Step 1: Validate the user credentials (username aind password)
            var user = ValidateUserCredentials(login.Username, login.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Step 2: Generate JWT token
            var token = GenerateJwtToken(user.Username, user);

            // Step 3: Return the JWT token
            return Ok(new { Token = token});
        }



        private async Task<User> ValidateTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();


            var parameters = new TokenValidationParameters
            {
                
                  ValidateIssuer = true,
                  ValidIssuer = "your-issuer",  // Match the 'iss' in the token
                  ValidateAudience = true,
                  ValidAudience = "your-audience",  // Match the 'aud' in the token
                  ValidateLifetime = true,
                  ClockSkew = TimeSpan.FromMinutes(5), // To remove clock skew tolerance
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-very-secure-jwt-secret-key-001")) // The same key used to sign the token
        };

            try
            {
                // Validate the token and get the claims principal
                var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

                // Ensure the token is a valid JWT
                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    

                    // Extract claims from the JWT token
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");
                    var isAdminClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "IsAdmin");
                    var userNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

                    if (userIdClaim != null && isAdminClaim != null && userNameClaim != null)
                    {
                        var userId = int.Parse(userIdClaim.Value);
                        var isAdmin = bool.Parse(isAdminClaim.Value);
                        var userName = userNameClaim.Value;

                       

                        // Check if the user is an admin
                        if (isAdmin)
                        {
                            // Fetch the user from the database or return a mock user
                            var user = GetAdminFromDatabase(userName);

                            return user; // Return the admin user
                        }
                        else
                        {
                          
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Claims not found: UserId or IsAdmin missing.");
                    }
                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                // Token expired error
                Console.WriteLine($"Token expired: {ex.Message}");
            }
            catch (SecurityTokenValidationException ex)
            {
                // Token validation failed error
                Console.WriteLine($"Token validation failed: {ex.Message}");
            }
           
            catch (ArgumentNullException ex)
            {
                // Missing parameters, such as a null token or key
                Console.WriteLine($"ArgumentNullException: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw ex;
               
            
            }

            return null;  // Token is invalid or expired
        }



        // Example method to validate user credentials against a database (replace with actual database logic)
        private User ValidateUserCredentials(string username, string password)
        {
            // Replace this with your actual logic to retrieve the user from the admin table in your database
            // For example, querying a database or using an ORM (Entity Framework)
            var userFromDb = GetAdminFromDatabase(username);

            // Example logic to check if the user exists and the password matches (hashing password for comparison)
            if (username == userFromDb.Username && VerifyPassword(password, userFromDb.PasswordHash))
            {
                return userFromDb;
            }

            return null;
        }

        // Example method to verify the password hash
        private bool VerifyPassword(string password, string storedPasswordHash)
        {
            // Compare password using your hashing algorithm
 
            return storedPasswordHash == password;
        }

        // Example method to retrieve a user from your database (replace with actual database access code)
        private User GetAdminFromDatabase(string username)
        {
            // Replace with actual database query logic
            // For example, using Entity Framework or raw SQL to query your admin table
            return new User
            {
                Id = 1,
                Username = "admin",  // Example username
                IsAdmin = true,      // Example flag to indicate if the user is an admin
                PasswordHash = "admin@123" // Example password (hashed in real applications)
            };
        }

        // Method to generate the JWT token
        private string GenerateJwtToken(string username, User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("UserId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),  // Include the role (Admin if user is an admin)
            new Claim("IsAdmin", user.IsAdmin.ToString())  // Custom admin claim (optional)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


       
       
    }
   

}