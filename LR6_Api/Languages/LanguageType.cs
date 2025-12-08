using LR6_GraphQL_ProductCatalog.Api.Domain;
using GreenDonut.Data;
using HotChocolate.Execution.Processing;
using LR6_Api.Languages.DataLoaders;

namespace LR6_Api.Languages;

[ObjectType<Language>]
public static partial class LanguageType
{
    static partial void Configure(IObjectTypeDescriptor<Language> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).Type<IdType>();
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Code);
        
        descriptor.Field("intId")
            .Type<NonNullType<IntType>>()
            .Resolve(ctx => ctx.Parent<Language>().Id);
    }

    [BindMember(nameof(Language.Translations))]
    public static async Task<IEnumerable<LR6_GraphQL_ProductCatalog.Api.Domain.Translations>> GetTranslationsAsync(
        [Parent] Language language,
        ITranslationsByLanguageIdDataLoader translationsByLanguageId,
        ISelection selection,
        CancellationToken cancellationToken)
    {
        return await translationsByLanguageId
            .Select(selection)
            .LoadRequiredAsync(language.Id, cancellationToken);
    }
}