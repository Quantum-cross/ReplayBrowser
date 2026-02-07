using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ReplayBrowser.Data.Models;

[Owned]
public class Objective
{
    [Key]
    public int Id { get; set; }
    
    public required string Task { get; set; }
    public required string Status { get; set; }
    public required string Percent { get; set; }
}