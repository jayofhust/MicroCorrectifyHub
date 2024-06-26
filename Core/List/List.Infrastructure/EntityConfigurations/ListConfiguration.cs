using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RecALLDemo.Core.List.Infrastructure.EntityConfigurations;



public class ListConfiguration : IEntityTypeConfiguration<
    Domain.AggregateModels.ListAggregate.List> {
    public void Configure(
        EntityTypeBuilder<Domain.AggregateModels.ListAggregate.List> builder) {
        RelationalEntityTypeBuilderExtensions.ToTable(
            (EntityTypeBuilder)builder, "lists");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .UseHiLo("listseq", ListContext.DefaultSchema);
        builder.Ignore(p => p.DomainEvents);

        builder.Property<string>("_name")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Name").IsRequired();   //只有私有没用公开访问渠道

        builder.Property<int>("_typeId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("TypeId").IsRequired();

        builder.HasOne(p => p.Type).
            WithMany().
            HasForeignKey("_typeId")
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(p => p.UserIdentityGuid).HasField("_userIdentityGuid")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("UserIdentityGuid").IsRequired();  //公开和私有的访问渠道都有

        builder.Property(p => p.IsDeleted).HasField("_isDeleted")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("IsDeleted").IsRequired();
    }
}

