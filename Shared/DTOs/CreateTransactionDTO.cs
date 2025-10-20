namespace Shared.DTOs;

public record CreateTransactionDTO(string Sender, string Recipient, decimal Amount);