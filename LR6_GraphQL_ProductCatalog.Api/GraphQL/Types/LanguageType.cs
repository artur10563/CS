using HotChocolate.Data.Filters;
using LR6_GraphQL_ProductCatalog.Api.Domain;

namespace LR6_GraphQL_ProductCatalog.Api.GraphQL.Types;

public class LanguageType : ObjectType<Language>
{
    protected override void Configure(IObjectTypeDescriptor<Language> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        
        descriptor.Field(l => l.Id);
        descriptor.Field(l => l.Name);
        descriptor.Field(l => l.Code);
    }
}

public class LanguageFilterType : FilterInputType<Language>
{
    protected override void Configure(IFilterInputTypeDescriptor<Language> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        
        descriptor.Field(l => l.Id);
        descriptor.Field(l => l.Code);
        descriptor.Field(l => l.Name);
    }
}