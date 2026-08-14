namespace DocumentManager.Core.Models;

public sealed record DocumentInput(
    DocumentType Type,
    string SourcePath,
    bool IsTemporary = false);

