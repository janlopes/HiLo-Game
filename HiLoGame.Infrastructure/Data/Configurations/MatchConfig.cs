using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HiLoGame.Infrastructure.Data.Configurations
{
    public sealed class MatchConfig : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.RoomId).HasMaxLength(64).IsRequired();
            b.OwnsMany(x => x.Players, p =>
            {
                p.WithOwner();
                p.Property<int>("MatchId");
                p.Property(x => x.PlayerId).HasMaxLength(64);
                p.Property(x => x.Name).HasMaxLength(128);
            });
            b.HasMany(x => x.Logs).WithOne().HasForeignKey(x => x.MatchId);
        }
    }
}
