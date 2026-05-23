namespace retakebackend.Dtos;

public record CreateTodoItemDto(string Title, bool IsCompleted, int TodoListId);
