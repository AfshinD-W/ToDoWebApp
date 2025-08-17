using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSToDo.Models.Entities;
using SSToDo.Models.Enums;

namespace SSToDo.Models.EntityConfigurations
{
    public class ProjectUserInviteConfigurations : IEntityTypeConfiguration<ProjectUserInvite>
    {
        public void Configure(EntityTypeBuilder<ProjectUserInvite> builder)
        {
            builder.HasKey(i => i.Id);

            builder.HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Project)
                .WithMany()
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(i => i.UserId);
            builder.HasIndex(i => i.ProjectId);

            builder.Property(i => i.InviteToken).HasMaxLength(200);

            builder.Property(i => i.Status).HasDefaultValue(TaskStatusEnums.Pending);

            builder.Property(i => i.CreateAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(i => i.ExpiresAt).HasDefaultValueSql("DATEADD(day, 1, GETUTCDATE())");
        }
    }
}
