namespace retakebackend.Dtos;

public record TodoItemDto(int Id, string Title, bool IsCompleted, int TodoListId);
