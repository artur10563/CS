using LR6_GraphQL_ProductCatalog.Api.GraphQL.Types;
using LR6_GraphQL_ProductCatalog.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGraphQLServer()
    .AddProjections()
    .AddFiltering()
    .AddQueryType<ProductsQuery>()
    .AddType<LanguageType>()
    .AddType<TranslationType>()
    .AddType<ProductType>()
    ;


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);




