﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GPGiaitriviet.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class GiaitrivietEntities : DbContext
    {
        public GiaitrivietEntities()
            : base("name=GiaitrivietEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<CodeGP> CodeGPs { get; set; }
        public virtual DbSet<LogController> LogControllers { get; set; }
        public virtual DbSet<GiftExchange> GiftExchanges { get; set; }
        public virtual DbSet<LogMO> LogMOes { get; set; }
        public virtual DbSet<LogMT> LogMTs { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
    }
}
