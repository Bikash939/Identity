using API.Data;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));    
});

//3. be able to inject jwtservice class inside our controller
builder.Services.AddScoped<JWTService>();

// defining our identitycore service
builder.Services.AddIdentityCore<User>(options =>
{
    //password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    //for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})

    .AddRoles<IdentityRole>()   //be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>()   //be able to make of rolemanager
    .AddEntityFrameworkStores<Context>()   //providing our context
    .AddSignInManager<SignInManager<User>>()   //make use of signin manager
    .AddUserManager<UserManager<User>>()   //make use of usermanager to create users
    .AddDefaultTokenProviders();         //be able to create tokens for email confirmation


//be able to authenticate users using jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //validate the token based on the key we have provided inside appsettings.development.json jwt:key
            ValidateIssuerSigningKey = true,
            //the issuer signinkey based on jwt:key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            //the issuer which in here is the api project url we are using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            //validate the issuer (who ever is issuing the jwt)
            ValidateIssuer = true,
            //dont validate audience (angular side)
            ValidateAudience = false
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
        .Where(x => x.Value.Errors.Count() > 0)
        .SelectMany(x => x.Value.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

var app = builder.Build();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// adding useauthentication into our pipeline and this should come before UseAuthorization
//authenticaton verifies the identity of a user or service, and Authorization determines their access rights.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
