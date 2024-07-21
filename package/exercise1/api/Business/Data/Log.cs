using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data;

[Table("Log")]
public class Log
{
    public long Id { get; set; }

    public string RequestResponseName { get; set; } = string.Empty;

    public string RequestIdentifier { get; set; } = string.Empty;

    public string LogMessage { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public long? ElapsedTimeInMillis { get; set; }
}

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
