using HotChocolate.Data.Filters;
using LR6_GraphQL_ProductCatalog.Api.Domain;

namespace LR6_GraphQL_ProductCatalog.Api.GraphQL.Types;


public class TranslationType : ObjectType<Translations>
{
    protected override void Configure(IObjectTypeDescriptor<Translations> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        
        descriptor.Field(t => t.Id);
        descriptor.Field(t => t.Text);

        descriptor.Field(t => t.Language)
            .Type<LanguageType>();
    }
}


public class TranslationFilterType : FilterInputType<Translations>
{
    protected override void Configure(IFilterInputTypeDescriptor<Translations> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        
        descriptor.Field(t => t.Id);
        descriptor.Field(t => t.Text);
        descriptor.Field(t => t.Language).Type<LanguageFilterType>();
    }
}