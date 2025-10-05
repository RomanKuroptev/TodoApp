using NUnit.Framework;
using System.Net;
using TodoApp.Api.Models;

namespace TodoApp.Api.Tests
{
    [TestFixture]
    public class TodoTests : BaseTest
    {
        [Test]
        public async Task GetAllTodos_ReturnsSuccessStatusCode()
        {
            // Arrange - setup is done in the base class
            
            // Act
            var response = await ApiClient.GetAllTodosAsync(false);
            
            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Count, Is.EqualTo(2)); // Two non-completed todos in test data
        }
        
        [Test]
        public async Task GetCompletedTodos_ReturnsOnlyCompletedTodos()
        {
            // Arrange - first create and complete a todo using the DbContext directly
            var completedTodo = new Models.Todo 
            { 
                Title = "Completed task", 
                IsDone = true,
                DueDate = DateTime.UtcNow.AddDays(-1) 
            };
            DbContext.Todos.Add(completedTodo);
            DbContext.SaveChanges();

            // Act
            var response = await ApiClient.GetAllTodosAsync(true);

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(response.All(t => t.IsDone), Is.True);
        }
        
        [Test]
        public async Task GetHealth_ReturnsOk()
        {
            // Act
            var response = await HttpClient.GetAsync("/health");
            
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Does.Contain("ok"));
        }
        
        [Test]
        public async Task CreateTodo_SavesInDatabase()
        {
            // Arrange
            var newTodo = new TodoCreateDto
            {
                Title = "Test todo from API",
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            // Act - Create todo via API
            var createdTodo = await ApiClient.CreateTodoAsync(newTodo);

            // Assert - Verify the response
            Assert.That(createdTodo, Is.Not.Null);
            Assert.That(createdTodo.Id, Is.GreaterThan(0));
            Assert.That(createdTodo.Title, Is.EqualTo(newTodo.Title));
            Assert.That(createdTodo.IsDone, Is.False);

            // Assert - Verify in database using DbContext
            var todoFromDb = DbContext.Todos.Find(createdTodo.Id);
            Assert.That(todoFromDb, Is.Not.Null);
            Assert.That(todoFromDb.Title, Is.EqualTo(newTodo.Title));
            Assert.That(todoFromDb.IsDone, Is.False);
            Assert.That(todoFromDb.DueDate, Is.Not.Null);
            Assert.That(todoFromDb.DueDate.Value.Date, Is.EqualTo(newTodo.DueDate.Value.Date));
        }
    }
}