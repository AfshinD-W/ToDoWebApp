using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSToDo.Models.Entities;

namespace SSToDo.Models.EntityConfigurations
{
    public class ProjectUserConfigurations : IEntityTypeConfiguration<ProjectUser>
    {
        public void Configure(EntityTypeBuilder<ProjectUser> builder)
        {
            builder.HasKey(pu => new { pu.ProjectId, pu.UserId });

            builder.HasOne(pu => pu.User)
                .WithMany(u => u.ProjectUsers)
                .HasForeignKey(u => u.UserId);

            builder.HasOne(pu => pu.Project)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(p => p.ProjectId);

            builder.Property(p => p.IsAdmin).HasDefaultValue(false);
        }
    }
}
