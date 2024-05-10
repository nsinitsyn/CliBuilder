namespace ShellBuilderCore.Validation;

public enum ValidationErrorCode
{
    NoRegisteredCommands = 0,
    UsingReservedCommandName,
    InputTemplateIsNullOrEmpty,
    DuplicateInputTemplate,
    MissingPropertyInCommandClass
}