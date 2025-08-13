using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSToDo.Models.Entities;

namespace SSToDo.Models.EntityConfigurations
{
    public class ProjectConfigurations : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Unknown project");

            builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
