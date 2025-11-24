using System.ComponentModel.DataAnnotations;

namespace LR6_GraphQL_ProductCatalog.Api.Domain;

public class Language
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(255)]
    public string Name { get; set; }
    
    [MaxLength(10)] public string Code { get; set; }

    public virtual ICollection<Translations> Translations { get; set; }
}