using System.ComponentModel.DataAnnotations;

namespace LR6_GraphQL_ProductCatalog.Api.Domain;

public class Product
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(255)]
    public string Name { get; set; }

    public virtual ICollection<Translations> Translations { get; set; }
}