using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lr2_Kobzeva.Models; // Namespace ������ User � ApplicationDbContext

var builder = WebApplication.CreateBuilder(args);

// ��������� �������� ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ����������� JWT-��������������
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // �������� �������� ������
            ValidateAudience = true, // �������� ��������� ������
            ValidateLifetime = true, // �������� ����� �������� ������
            ValidateIssuerSigningKey = true, // �������� ������� ������
            ValidIssuer = "PT", // ���������� ������������� �������
            ValidAudience = "APIclients", // ��������� ������
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("superSecretKeyMustBeLoooooong32bitsMore")) // ��������� ����
        };
    });

// ��������� �����������
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();

app.UseRouting();

// ���������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
