namespace ArchiveViewerBackend;

/// <summary>Thrown when a text buffer is full but does not yet hold the wanted number of lines.</summary>
public class LineTooLongException(int capacityInChars)
    : InvalidOperationException($"The buffer of {capacityInChars:N0} characters is full but does not hold the wanted number of lines");
