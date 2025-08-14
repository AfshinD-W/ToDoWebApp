using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSToDo.Models.Entities;
using SSToDo.Models.Enums;

namespace SSToDo.Models.EntityConfigurations
{
    public class TodoTaskConfigurations : IEntityTypeConfiguration<TodoTask>
    {
        public void Configure(EntityTypeBuilder<TodoTask> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(t => t.ProjectId);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(250);

            builder.Property(t => t.Status)
                .HasDefaultValue(TaskStatusEnums.Open);

            builder.Property(t => t.Priority)
                .HasDefaultValue(TaskPriorityEnums.low);

            builder.Property(t => t.StartDate).HasDefaultValueSql("GETUTCDATE()");

            builder.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
