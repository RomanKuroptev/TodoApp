using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TodoApp.Api.Data;

namespace TodoApp.Api.Tests
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected TestClient ApiClient { get; private set; }
        protected HttpClient HttpClient { get; private set; }
        protected TodoAppWebApplicationFactory Factory { get; private set; }
        protected TodoDbContext DbContext { get; private set; }
        protected IServiceScope ServiceScope { get; private set; }
        private const string BaseUrl = "http://localhost";

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            // Create a WebApplicationFactory
            Factory = new TodoAppWebApplicationFactory();

            // Create an HttpClient using the factory
            HttpClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Create the API client using the HttpClient
            ApiClient = new TestClient(BaseUrl, HttpClient);

            // Create a new scope for the DbContext
            ServiceScope = Factory.Services.CreateScope();
            
            // Get access to the DbContext for direct database operations if needed
            DbContext = ServiceScope.ServiceProvider.GetRequiredService<TodoDbContext>();
        }
        
        [SetUp]
        public virtual void SetUp()
        {
            // This method runs before each test
        }

        [TearDown]
        public virtual void TearDown()
        {
            // Clear the database after each test to ensure test isolation
            DbContext.Todos.RemoveRange(DbContext.Todos);
            DbContext.SaveChanges();
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            // Dispose in reverse order of creation
            DbContext?.Dispose();
            ServiceScope?.Dispose();
            HttpClient?.Dispose();
            Factory?.Dispose();
        }
    }
}