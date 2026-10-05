namespace Claims.Domain;

public sealed class DomainValidationException(string message) : Exception(message);
