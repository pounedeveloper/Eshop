using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MKCollection.Applications;
using MKCollection.Models;
using MKCollection.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format 'Bearer {your token here}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var base64Secret = "5TzGYeBgtl4C0YtN03uvFcV3BZ9qN5XjYZRmfdrwPjY=";
IdentityModelEventSource.ShowPII = true;
var key = Convert.FromBase64String(base64Secret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddMemoryCache();

var configuration = builder.Configuration;
builder.Services.AddDbContext<MkcollectionContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")).UseLazyLoadingProxies());

RegisterApplications(builder);
builder.Services.AddScoped(typeof(ModelServiceBase<>));

var app = builder.Build();

app.UseStaticFiles();

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//    Path.Combine(Directory.GetCurrentDirectory(), "public")),
//    RequestPath = "/public"
//});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(options =>
{
    options
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod();
});

app.UseMiddleware<JwtAuthorizationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();
app.MapControllers();

app.Run();

static void RegisterApplications(WebApplicationBuilder builder)
{
    _ = builder.Services.AddScoped<DiscountApplication>();
    _ = builder.Services.AddScoped<AttachmentApplication>();
    _ = builder.Services.AddScoped<ReviewApplication>();
    _ = builder.Services.AddScoped<PickerApplication>();
    _ = builder.Services.AddScoped<ProductApplication>();
    _ = builder.Services.AddScoped<InvoiceApplication>();
    _ = builder.Services.AddScoped<CustomerApplication>();
    _ = builder.Services.AddScoped<CustomerAddressApplication>();
    _ = builder.Services.AddScoped<CategoryApplication>();
    _ = builder.Services.AddScoped<CollectionApplication>();
    _ = builder.Services.AddScoped<InvoiceDetailApplication>();
    _ = builder.Services.AddScoped<CustomerAddressApplication>();
    _ = builder.Services.AddScoped<TranslatorApplication>();
    _ = builder.Services.AddScoped<AttachmentApplication>();
    _ = builder.Services.AddScoped<EnumerableValueApplication>();
}
