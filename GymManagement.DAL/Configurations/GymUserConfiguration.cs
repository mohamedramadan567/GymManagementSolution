using GymManagement.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Configurations
{
    public class GymUserConfiguration<T> : IEntityTypeConfiguration<T> where T : GymUser
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(u => u.Name)
                   .HasColumnType("varchar")
                   .HasMaxLength(50);

            builder.Property(u => u.Email)
                   .HasColumnType("varchar")
                   .HasMaxLength(100);

            builder.Property(u => u.Phone)
                   .HasMaxLength(11);

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Phone).IsUnique();

            builder.ToTable(tb =>
            {
                tb.HasCheckConstraint("CheckEmail", "Email like '_%@_%._%'");
                tb.HasCheckConstraint("CheckPhone", "Phone like 01[0125]________");
            });

            builder.OwnsOne(u => u.Address, address =>
            {
                address.Property(a => a.Street)
                       .HasMaxLength(30);
                address.Property(a => a.City)
                       .HasMaxLength(30);
            });
        }
    }
}
