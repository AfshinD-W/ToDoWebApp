using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSToDo.Models.Entities;

namespace SSToDo.Models.EntityConfigurations
{
    public class TaskHistoryConfigurations : IEntityTypeConfiguration<TaskHistory>
    {
        public void Configure(EntityTypeBuilder<TaskHistory> builder)
        {
            builder.HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
