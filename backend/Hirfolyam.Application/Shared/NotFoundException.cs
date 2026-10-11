namespace Hirfolyam.Application.Shared;

public sealed class NotFoundException(string message) : Exception(message);
