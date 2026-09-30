namespace GreenGenius.Common.Data.Exceptions;

public class DataNotFoundException(string? message, Exception? cause = null) : Exception(message, cause);
