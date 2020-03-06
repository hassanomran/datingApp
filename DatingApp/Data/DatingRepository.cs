﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Helpers;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.photo.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.photo.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }
         public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() >0;
        }

       public async Task<pagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users
                .Include(p => p.photos).OrderByDescending(u =>u.LastActive)
                .AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);
            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if(userParams.Likees)
            {
                
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if (userParams.MinAge !=18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await pagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.Pagesize);
        }
        private async Task<IEnumerable<int>> GetUserLikes(int id,bool likers)
        {
            var user = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);
            if(likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<Like> GetLike(int userId, int recipiented)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipiented);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<pagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var message = _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.photos)
                .Include(u => u.Recipient).ThenInclude(p => p.photos)
                .AsQueryable();
            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    message = message.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    message = message.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    message = message.Where(u => u.RecipientId == messageParams.UserId && u.IsRead == false && u.RecipientDeleted == false );
                    break;
            }
            message = message.OrderByDescending(d => d.MessageSent);

            return await pagedList<Message>.CreateAsync(message,
                messageParams.PageNumber, messageParams.Pagesize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var message = await _context.Messages
               .Include(u => u.Sender).ThenInclude(p => p.photos)
               .Include(u => u.Recipient).ThenInclude(p => p.photos)
               .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId
               || m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId)
               .OrderByDescending(m => m.MessageSent)
               .ToListAsync();
            return message;
        }
    }
}