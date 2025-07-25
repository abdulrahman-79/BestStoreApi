﻿using BestStoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {            
        }

        public DbSet<Contacts> Contacts { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Subject> Subjects { get; set; }
    }
}
