using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HiLoGame.Infrastructure.Data.Configurations
{
    public sealed class GuessLogConfig : IEntityTypeConfiguration<GuessLog>
    {
        public void Configure(EntityTypeBuilder<GuessLog> b)
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.PlayerId).HasMaxLength(64).IsRequired();
            b.Property(x => x.PlayerName).HasMaxLength(128).IsRequired();
        }
    }
}