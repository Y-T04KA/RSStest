using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RSStest.Models;

namespace RSStest.Data
{
    public class RSStestContext : DbContext
    {
        public RSStestContext (DbContextOptions<RSStestContext> options)
            : base(options)
        {
        }

        public DbSet<RSStest.Models.RSSitem> RSSitem { get; set; } = default!;
    }
}
