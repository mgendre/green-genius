namespace GreenGenius.Common.Data.Exceptions;

public class DataNotFoundException(string? message, Exception? cause = null) : KeyNotFoundException(message, cause);
