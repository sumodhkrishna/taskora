using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sumodh.Taskora.Application.Abstractions.Authentication;
using Sumodh.Taskora.Application.Abstractions.Communication;
using Sumodh.Taskora.Application.Abstractions.Identity;
using Sumodh.Taskora.Application.Abstractions.Persistence;
using Sumodh.Taskora.Application.Features.Auth.Commands.Login;
using Sumodh.Taskora.Application.Features.Auth.Commands.Logout;
using Sumodh.Taskora.Application.Features.Auth.Commands.RefreshToken;
using Sumodh.Taskora.Application.Features.Auth.Commands.Register;
using Sumodh.Taskora.Application.Features.Auth.Commands.RequestPasswordReset;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResendEmailVerification;
using Sumodh.Taskora.Application.Features.Auth.Commands.ResetPassword;
using Sumodh.Taskora.Application.Features.Auth.Commands.VerifyEmail;
using Sumodh.Taskora.Application.Features.Todos.Commands;
using Sumodh.Taskora.Application.Features.Todos.Queries;
using Sumodh.Taskora.Application.Features.Users.Queries.GetById;
using Sumodh.Taskora.Application.Features.Users.Queries.GetCurrentUser;
using Sumodh.Taskora.Infra.Authentication;
using Sumodh.Taskora.Infra.Email;
using Sumodh.Taskora.Infra.Persistance;
using Sumodh.Taskora.Infra.Persistance.Repositories;
using Sumodh.Taskora.Infrastructure;
using System.Text;
using System.Threading.RateLimiting;

namespace Sumodh.Taskora
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(JwtOptions.SectionName));
            builder.Services
                .AddOptions<SendGridEmailOptions>()
                .Bind(builder.Configuration.GetSection(SendGridEmailOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
            var jwtOptions = jwtSection.Get<JwtOptions>()
                             ?? throw new InvalidOperationException("Jwt configuration is missing.");

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddHealthChecks();
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("AuthSensitivePolicy", policy =>
                {
                    policy.PermitLimit = 50;
                    policy.Window = TimeSpan.FromMinutes(1);
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("AuthSessionPolicy", policy =>
                {
                    policy.PermitLimit = 150;
                    policy.Window = TimeSpan.FromMinutes(1);
                    policy.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    policy.QueueLimit = 0;
                });
            });

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.Key)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJWTTokenGenerator, JwtTokenGenerator>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ITodoRepository, TodoRepository>();
            builder.Services.AddScoped<IPasswordResetTokenGenerator, PasswordResetTokenGenerator>();
            builder.Services.AddScoped<IEmailVerificationTokenGenerator, EmailVerificationTokenGenerator>();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddScoped<IPasswordResetEmailSender, ConsolePasswordResetEmailSender>();
                builder.Services.AddScoped<IEmailVerificationEmailSender, ConsoleEmailVerificationEmailSender>();
            }
            else
            {
                builder.Services.AddHttpClient<IPasswordResetEmailSender, SendGridPasswordResetEmailSender>(client =>
                {
                    client.BaseAddress = new Uri("https://api.sendgrid.com/v3/");
                });
                builder.Services.AddHttpClient<IEmailVerificationEmailSender, SendGridEmailVerificationEmailSender>(client =>
                {
                    client.BaseAddress = new Uri("https://api.sendgrid.com/v3/");
                });
            }

            builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            builder.Services.AddScoped<RefreshTokenCommandHandler>();
            builder.Services.AddScoped<LogoutCommandHandler>();
            builder.Services.AddScoped<RequestPasswordResetCommandHandler>();
            builder.Services.AddScoped<ResendEmailVerificationCommandHandler>();
            builder.Services.AddScoped<ResetPasswordCommandHandler>();
            builder.Services.AddScoped<VerifyEmailCommandHandler>();
            builder.Services.AddScoped<CreateTodoCommandHandler>();
            builder.Services.AddScoped<RegisterCommandHandler>();
            builder.Services.AddScoped<GetUserByIdQueryHandler>();
            builder.Services.AddScoped<LoginCommandHandler>();
            builder.Services.AddScoped<GetCurrentUserQueryHandler>();
            builder.Services.AddScoped<GetTodoByIdQueryHandler>();
            builder.Services.AddScoped<GetMyTodosQueryHandler>();
            builder.Services.AddScoped<UpdateTodoCommandHandler>();
            builder.Services.AddScoped<DeleteTodoCommandHandler>();
            builder.Services.AddScoped<CompleteTodoCommandHandler>();
            builder.Services.AddScoped<ReopenTodoCommandHandler>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
            }
            app.UseExceptionHandler();
            app.UseCors("FrontendPolicy");
            app.UseHttpsRedirection();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health");
            app.MapControllers();

            app.Run();
        }
    }
}
