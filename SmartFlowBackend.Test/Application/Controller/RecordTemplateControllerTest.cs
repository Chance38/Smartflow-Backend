// using System.Net;
// using System.Text;
// using System.Text.Json;

// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging.Testing;
// using Microsoft.Extensions.Logging;

// using SmartFlowBackend.Application.Controller;
// using SmartFlowBackend.Application.SwaggerSetting;
// using SmartFlowBackend.Infrastructure.Persistence;
// using SmartFlowBackend.Test.Helper;

// using Testcontainers.PostgreSql;
// using SmartFlowBackend.Domain.Contracts;

// namespace SmartFlowBackend.Test.Application.Controller.RecordTemplate;

// public class RecordTemplateControllerTest
// {
//     private PostgreSqlContainer _postgresContainer;
//     private CustomWebApplicationFactory _factory;
//     private HttpClient _client;

//     [OneTimeSetUp]
//     public async Task OneTimeSetUp()
//     {
//         Console.WriteLine("Starting PostgreSQL container...");
//         _postgresContainer = TestHelper.CreatePostgreSqlContainer();
//         await _postgresContainer.StartAsync();
//         Console.WriteLine("PostgreSQL container started.");

//         var postgresUrl = _postgresContainer.GetConnectionString();
//         Environment.SetEnvironmentVariable("ConnectionStrings__Postgres_Connection", postgresUrl);
//     }

//     [OneTimeTearDown]
//     public async Task OneTimeTearDown()
//     {
//         if (_postgresContainer != null)
//             await _postgresContainer.DisposeAsync();
//     }

//     [SetUp]
//     public async Task SetUp()
//     {
//         _factory = new CustomWebApplicationFactory();
//         _client = _factory.CreateClient();

//         var scope = _factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//         db.RecordTemplate.RemoveRange(db.RecordTemplate);
//         await db.SaveChangesAsync();
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         _factory.Dispose();
//         _client.Dispose();
//     }

//     [Test]
//     public async Task AddRecordTemplate_Should_Returns_Ok()
//     {
//         var AddRecordTemplateReq =
//         """
//         {
//             "recordTemplate":{
//                 "recordTemplateName": "Test Template",
//                 "categoryName": "food",
//                 "categoryType": "expense",
//                 "tags": ["tag1", "tag2"],
//                 "amount": 100.0
//             }
//         }
//         """;

//         var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
//             AddRecordTemplateReq,
//             Encoding.UTF8,
//             "application/json"
//         ));

//         Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
//         Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));

//         var scope = _factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//         var recordTemplates = await db.RecordTemplate.FirstOrDefaultAsync(rt =>
//             rt.RecordTemplateName == "Test Template" &&
//             rt.CategoryName == "food" &&
//             rt.CategoryType == Domain.Entities.CategoryType.Expense &&
//             rt.TagNames.Contains("tag1") &&
//             rt.TagNames.Contains("tag2") &&
//             rt.Amount == 100.0
//         );

//         Assert.IsNotNull(recordTemplates, "RecordTemplate should be added to the database");
//     }

//     [Test]
//     public async Task AddRecordTemplate_When_TemplateNameAlreadyExists_Should_Return_BadRequest()
//     {
//         var scope = _factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//         db.RecordTemplate.Add(new Domain.Entities.RecordTemplate
//         {
//             RecordTemplateId = Guid.NewGuid(),
//             RecordTemplateName = "Test Template",
//             CategoryName = "food",
//             CategoryType = Domain.Entities.CategoryType.Expense,
//             TagNames = new List<string> { "tag1", "tag2" },
//             Amount = 100.0f,
//             UserId = TestUser.Id
//         });

//         db.SaveChanges();

//         var AddRecordTemplateReq =
//         """
//         {
//             "recordTemplate":{
//                 "recordTemplateName": "Test Template",
//                 "categoryName": "food",
//                 "categoryType": "expense",
//                 "tags": ["tag1", "tag2"],
//                 "amount": 100.0
//             }
//         }
//         """;

//         var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
//             AddRecordTemplateReq,
//             Encoding.UTF8,
//             "application/json"
//         ));

//         Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
//         Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
//     }

//     [Test]
//     public async Task AddRecordTemplate_When_AmountIsZero_And_CategoryNameIsNull_And_TagsIsEmpty_Should_Return_BadRequest()
//     {
//         var scope = _factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//         var AddRecordTemplateReq =
//         """
//         {
//             "recordTemplate":{
//                 "recordTemplateName": "Test Template",
//                 "categoryType": "expense",
//                 "amount": 0.0
//             }
//         }
//         """;

//         var resp = await _client.PostAsync("/smartflow/v1/record-template", new StringContent(
//             AddRecordTemplateReq,
//             Encoding.UTF8,
//             "application/json"
//         ));

//         Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
//         Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
//     }

//     [Test]
//     public async Task GetAllRecordTemplates_Should_Returns_Ok_With_RecordTemplates()
//     {
//         var scope = _factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//         db.RecordTemplate.AddRange(new List<Domain.Entities.RecordTemplate>
//         {
//             new Domain.Entities.RecordTemplate
//             {
//                 RecordTemplateId = Guid.NewGuid(),
//                 RecordTemplateName = "Template 1",
//                 CategoryName = "food",
//                 CategoryType = Domain.Entities.CategoryType.Expense,
//                 TagNames = new List<string> { "tag1", "tag2" },
//                 Amount = 50.0f,
//                 UserId = TestUser.Id
//             },
//             new Domain.Entities.RecordTemplate
//             {
//                 RecordTemplateId = Guid.NewGuid(),
//                 RecordTemplateName = "Template 2",
//                 CategoryName = "salary",
//                 CategoryType = Domain.Entities.CategoryType.Income,
//                 TagNames = new List<string> { "tag3" },
//                 Amount = 1500.0f,
//                 UserId = TestUser.Id
//             }
//         });

//         await db.SaveChangesAsync();
//         var resp = await _client.GetAsync("/smartflow/v1/record-templates");

//         Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
//         Assert.That(resp.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
//     }

//     [Test]
//     public async Task DeleteRecordTemplate_Should_Returns_Ok_And_Remove_RecordTemplate()
//     {
//         var scope = _factory.Services.CreateScope();
//         var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//         db.RecordTemplate.Add(new Domain.Entities.RecordTemplate
//         {
//             RecordTemplateId = Guid.NewGuid(),
//             RecordTemplateName = "Template To Delete",
//             CategoryName = "food",
//             CategoryType = Domain.Entities.CategoryType.Expense,
//             TagNames = new List<string> { "tag1", "tag2" },
//             Amount = 100.0f,
//             UserId = TestUser.Id
//         });

//         await db.SaveChangesAsync();

//         var DeleteRecordTemplateReq =
//         """
//         {
//             "recordTemplateName": "Template To Delete"
//         }
//         """;

//         var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/record-template")
//         {
//             Content = new StringContent(DeleteRecordTemplateReq, Encoding.UTF8, "application/json")
//         };

//         var response = await _client.SendAsync(req);
//         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

//         var deletedTemplate = await db.RecordTemplate.FirstOrDefaultAsync(rt => rt.RecordTemplateName == "Template To Delete");
//         Assert.IsNull(deletedTemplate, "RecordTemplate should be deleted from the database");
//     }

//     [Test]
//     public async Task DeleteRecordTemplate_When_TemplateNameIsNotExist_Should_Return_Ok()
//     {
//         var DeleteRecordTemplateReq =
//         """
//         {
//             "recordTemplateName": "Template To Delete"
//         }
//         """;

//         var req = new HttpRequestMessage(HttpMethod.Delete, "smartflow/v1/record-template")
//         {
//             Content = new StringContent(DeleteRecordTemplateReq, Encoding.UTF8, "application/json")
//         };

//         var response = await _client.SendAsync(req);
//         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
//     }
// }

// public class CustomWebApplicationFactory : WebApplicationFactory<Program>
// {
//     public FakeLogger<RecordTemplateController> fakeLogger = null!;

//     protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
//     {
//         builder.ConfigureServices(services =>
//         {
//             fakeLogger = new FakeLogger<RecordTemplateController>();
//             services.AddSingleton<ILogger<RecordTemplateController>>(fakeLogger);

//             var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresDbContext>));
//             if (descriptor != null) services.Remove(descriptor);

//             var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres_Connection")!;
//             services.AddDbContext<PostgresDbContext>(options => options.UseNpgsql(conn));
//         });
//     }
// }