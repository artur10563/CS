using Data.Infrastructure;
using LR6_GraphQL_ProductCatalog.Api.Domain;

namespace LR6_Api.Languages;

public sealed record AddLanguageInput(string Name, string Code);

[MutationType]
public static class LanguageMutations
{
    [Error<ArgumentException>]
    public static async Task<Language> AddLanguage(
        AddLanguageInput input,
        AppDbContext dbContext)
    {
        if (string.IsNullOrEmpty(input.Name) || string.IsNullOrEmpty(input.Code))
        {
            throw new ArgumentException("Language name and code is required");
        }
        
        var language = new Language()
        {
            Name = input.Name,
            Code = input.Code
        };

        dbContext.Languages.Add(language);
        await dbContext.SaveChangesAsync();

        return language;
    }
}