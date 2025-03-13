using GlobalExceptionHandling;
using GlobalExceptionHandling.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseExceptionHandler(options =>
//{
//    options.Run(async context =>
//    {
//        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
//        context.Response.ContentType = "application/json";
//        var exception = context.Features.Get<IExceptionHandlerFeature>();
//        if (exception != null)
//        {
//            var message = $"{exception.Error.Message}";
//            await context.Response.WriteAsync(message).ConfigureAwait(false);
//        }
//    });
//});

//app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseExceptionHandler();


app.MapGet("/", () => { throw new ProductNotFoundException(Guid.NewGuid()); });

app.Run();