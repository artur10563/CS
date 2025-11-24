using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LR6_GraphQL_ProductCatalog.Api.Domain;

public class Translations
{
    [Key] public int Id { get; set; }

    public string Text { get; set; }

    public int LanguageId { get; set; }

    [ForeignKey(nameof(LanguageId))] public virtual Language Language { get; set; }

    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))] public virtual Product Product { get; set; }
}