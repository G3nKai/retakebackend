namespace retakebackend.consumer.Messaging;
public record TodoMessage(string EntityType, string Action, int Id, string PayloadJson);
