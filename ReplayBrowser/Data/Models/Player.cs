using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReplayBrowser.Models.Ingested;

namespace ReplayBrowser.Data.Models;

public class Player : IEntityTypeConfiguration<Player>
{
    [JsonIgnore]
    public int Id { get; set; }

    public List<string> AntagPrototypes { get; set; } = null!;
    public List<string> JobPrototypes { get; set; } = null!;
    public required string PlayerIcName { get; set; }
    public bool Antag { get; set; }

    [JsonIgnore]
    public ReplayParticipant Participant { get; set; } = null!;
    [JsonIgnore]
    public int ParticipantId { get; set; }

    public JobDepartment? EffectiveJob { get; set; }
    public int? EffectiveJobId { get; set; }
    
    public List<Objective> Objectives { get; set; } = null!;

    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasIndex(p => p.PlayerIcName);
        builder.HasIndex(p => p.ParticipantId);
        builder.OwnsMany(
            p => p.Objectives,
            a =>
            {
                a.WithOwner().HasForeignKey("PlayerId");
                a.Property<int>("Id");
                a.HasKey("Id");
            });
    }

    public static Player FromYaml(YamlPlayer player)
    {
        return new Player {
            PlayerIcName = player.PlayerIcName,

            JobPrototypes = player.JobPrototypes,
            AntagPrototypes = player.AntagPrototypes,

            Antag = player.Antag
        };
    }

    public void RedactInformation(bool wasGdpr = false)
    {
        if (wasGdpr)
        {
            PlayerIcName = "Removed by GDPR request";
        }
        else
        {
            PlayerIcName = "Redacted";
        }
    }

    public record Objective(string Task, string Status, string Pct);
    
    public Player ParseObjectives(string? roundEndText_)
    {
        if (roundEndText_ is not { } roundEndText)
        {
            return this;
        }
        
        var pattern = @"^- (?<task>[^|]*)\| \[[^\]]*\](?<status>[^\[]*)\[[^\]]*\] \((?<pct>[^)]*)\)";
        Objectives = roundEndText
            .Split('\n')
            .TakeWhile(l => !l.StartsWith("Cards"))
            .SkipWhile(l => !(l.Contains(PlayerIcName) && l.Contains("who had the following objective")))
            .SkipWhile(l => !l.StartsWith("- "))
            .TakeWhile(l => l.StartsWith("- "))
            .Select(l => Regex.Match(l, pattern))
            .Where(m => m.Success)
            .Select(m => new Objective(m.Groups["task"].Value, m.Groups["status"].Value, m.Groups["pct"].Value))
            .ToList();
        
        return this;
    }
}