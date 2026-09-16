using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using VDCD.Business;
using VDCD.Business.Helper;
using VDCD.DataAccess;
using VDCD.Middleware;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("DefaultConnection")
	)
);
builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration["Redis:ConnectionString"];
	options.InstanceName = "VDCD_HOTEL:";
});
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()      // Cho phép mọi nguồn gốc
			  .AllowAnyMethod()      // Cho phép mọi method (GET, POST, PUT, DELETE,...)
			  .AllowAnyHeader();     // Cho phép mọi headers
	});
});
builder.Services.AddHttpClient();
// Read configs
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

var keycloakServer = builder.Configuration["Keycloak:ServerUrl"];
var keycloakRealm = builder.Configuration["Keycloak:Realm"];
var keycloakClientId = builder.Configuration["Keycloak:ClientId"];

// Configure authentication: support both Local and Keycloak tokens
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = "Bearer";
	options.DefaultChallengeScheme = "Bearer";
})
.AddPolicyScheme("Bearer", "Authorization Bearer or Keycloak", options =>
{
	options.ForwardDefaultSelector = context =>
	{
		var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
		if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			return "Local";

		var token = authHeader.Substring("Bearer ".Length).Trim();
		try
		{
			var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
			var iss = jwt.Issuer ?? string.Empty;

			if (!string.IsNullOrEmpty(iss) && iss.Contains("/realms/"))
				return "Keycloak";
		}
		catch
		{
		}

		return "Local";
	};
})
.AddJwtBearer("Local", options =>
{
	if (!string.IsNullOrEmpty(jwtKey))
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,
			ValidateAudience = true,
			ValidAudience = jwtAudience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
	}
})
.AddJwtBearer("Keycloak", options =>
{
	if (!string.IsNullOrEmpty(keycloakServer) && !string.IsNullOrEmpty(keycloakRealm))
	{
		options.Authority = $"{keycloakServer.TrimEnd('/')}/realms/{keycloakRealm}";
		if (!string.IsNullOrEmpty(keycloakClientId))
		{
			options.Audience = keycloakClientId;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidAudience = keycloakClientId,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};
		}

		options.RequireHttpsMetadata = !options.Authority.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
	}
});


// Hoặc cấu hình cho riêng endpoint upload
builder.WebHost.ConfigureKestrel(options =>
{
	options.Limits.MaxRequestBodySize = null; 
	//options.Limits.MaxRequestBodySize = 21474836480; 
});

builder.Services.Configure<FormOptions>(options =>
{
	options.ValueLengthLimit = int.MaxValue;
	options.MultipartBodyLengthLimit = int.MaxValue; // Không giới hạn
	options.MemoryBufferThreshold = int.MaxValue;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Vdcd Hotel API",
		Version = "v1",
		Description = "API for Vdcd Hotel application"
	});

	// Cấu hình cho file upload
	options.MapType<IFormFile>(() => new OpenApiSchema
	{
		Type = "string",
		Format = "binary"
	});

	// Thêm operation filter để hỗ trợ file upload
	options.OperationFilter<FileUploadOperationFilter>();

	// Include comments từ XML (nếu có)
	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	if (File.Exists(xmlPath))
	{
		options.IncludeXmlComments(xmlPath);
	}

	// Cấu hình Swagger để hỗ trợ JWT
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter 'Bearer {token}'"
	});

	var bearerScheme = new OpenApiSecurityScheme
	{
		Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
		In = ParameterLocation.Header
	};

	options.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{ bearerScheme, Array.Empty<string>() }
	});
});

builder.Services.AddBusinessServices();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "wwwroot")),
	RequestPath = "",
	ContentTypeProvider = new FileExtensionContentTypeProvider(),
	ServeUnknownFileTypes = true,
	OnPrepareResponse = ctx =>
	{
		ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
		ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
	}
});
app.UseRouting();
app.UseCors("AllowAll");  // Dùng policy AllowAll
app.Use(async (context, next) =>
{
	Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
	try
	{
		await next();
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Lỗi: {ex.Message}");
		throw;
	}
});

/*if (app.Environment.IsDevelopment())
{*/
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vdcd Hotel API V1");
	});
/*}*/
app.Use(async (context, next) =>
{
	context.Response.Headers.Remove("X-Frame-Options");
	context.Response.Headers.Append("X-Frame-Options", "ALLOWALL");
	context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors *");
	await next();
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Tạo thư mục Uploads nếu chưa có
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
	Directory.CreateDirectory(uploadsPath);
}

app.Run();

// SỬA LẠI FILTER NÀY
public class FileUploadOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		// Lấy tất cả parameters của method
		var parameters = context.MethodInfo.GetParameters();

		// Kiểm tra xem có parameter nào là IFormFile không
		var hasFileParameter = parameters.Any(p => p.ParameterType == typeof(IFormFile));

		if (hasFileParameter)
		{
			// Xóa tất cả parameters khỏi operation (vì chúng ta sẽ đưa vào request body)
			operation.Parameters = operation.Parameters
				?.Where(p => !parameters.Any(param => param.Name == p.Name))
				.ToList();

			// Tạo schema cho multipart/form-data
			var schema = new OpenApiSchema
			{
				Type = "object",
				Properties = new Dictionary<string, OpenApiSchema>(),
				Required = new HashSet<string>()
			};

			// Thêm các parameter vào schema
			foreach (var param in parameters)
			{
				var schemaProperty = new OpenApiSchema();

				if (param.ParameterType == typeof(IFormFile))
				{
					schemaProperty.Type = "string";
					schemaProperty.Format = "binary";
				}
				else if (param.ParameterType == typeof(long?) || param.ParameterType == typeof(long))
				{
					schemaProperty.Type = "integer";
					schemaProperty.Format = "int64";
				}
				else if (param.ParameterType == typeof(int?) || param.ParameterType == typeof(int))
				{
					schemaProperty.Type = "integer";
					schemaProperty.Format = "int32";
				}
				else if (param.ParameterType == typeof(bool?) || param.ParameterType == typeof(bool))
				{
					schemaProperty.Type = "boolean";
				}
				else if (param.ParameterType == typeof(DateTime?) || param.ParameterType == typeof(DateTime))
				{
					schemaProperty.Type = "string";
					schemaProperty.Format = "date-time";
				}
				else
				{
					schemaProperty.Type = "string";
				}

				schema.Properties.Add(param.Name, schemaProperty);

				// Kiểm tra required (nếu parameter không phải nullable)
				if (!IsNullable(param.ParameterType))
				{
					schema.Required.Add(param.Name);
				}
			}

			// Tạo request body mới
			operation.RequestBody = new OpenApiRequestBody
			{
				Content = new Dictionary<string, OpenApiMediaType>
				{
					["multipart/form-data"] = new OpenApiMediaType
					{
						Schema = schema
					}
				},
				Required = true
			};
		}
	}

	private bool IsNullable(Type type)
	{
		if (!type.IsValueType) return true; // reference types are nullable
		return Nullable.GetUnderlyingType(type) != null; // value types are nullable if Nullable<T>
	}
}