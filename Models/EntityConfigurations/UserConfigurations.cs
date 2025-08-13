using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSToDo.Models.Entities;

namespace SSToDo.Models.EntityConfigurations
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Unknown user");

            builder.Property(u => u.Email)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.ConfirmedEmail)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.Password)
                .IsRequired();

            builder.Property(u => u.ImagePath)
                .HasDefaultValue("Y:\\logo.png");
        }
    }
}
