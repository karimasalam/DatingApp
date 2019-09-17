using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            this._context = context;

        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == username);
            if(user == null)
                return null;
            // if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))           
            //     return null;
            
            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            bool isVerified = true;
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var ComputeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                if(ComputeHash.Length != passwordHash.Length)
                  isVerified = false;
                for (int i=0; i<ComputeHash.Length; i++)
                {
                    if(ComputeHash[i] != passwordHash[i])
                    {
                        isVerified = false;
                        break;
                    }
                }
            }
            return isVerified;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] PasswordHash;
            byte[] PasswordSalt;
            CreatePasswordHash(password, out PasswordHash, out PasswordSalt);

            // user.PasswordHash = PasswordHash;
            // user.PasswordSalt = PasswordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(x => x.UserName == username))
                return true;
            else return false;
        }        
    }
}