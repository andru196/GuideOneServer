using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuideOneServer.Models;
using Microsoft.EntityFrameworkCore;

namespace GuideOneServer.DBContextModel
{
	public class UsersContext : DbContext
	{
		public DbSet<User> Users { get; set; }
		public UsersContext(DbContextOptions<UsersContext> options) : base(options)
		{

		}
	}
}
