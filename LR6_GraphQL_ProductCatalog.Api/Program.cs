using LR6_GraphQL_ProductCatalog.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// builder.Services
//     .AddGraphQLServer()
//     .AddProjections()
//     .AddFiltering()
//     .AddQueryType<ProductsQueries>()
//     .AddType<LanguageType>()
//     .AddType<TranslationType>()
//     // .AddType<ProductType>()
//     ;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddGraphQLServer()
    
    .AddGlobalObjectIdentification()
    .AddMutationConventions()
    .AddDbContextCursorPagingProvider()
    .AddPagingArguments()
    .AddFiltering()
    .AddSorting()
    .AddGraphQLTypes();

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);



