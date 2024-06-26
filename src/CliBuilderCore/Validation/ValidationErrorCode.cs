namespace CliBuilderCore.Validation;

public enum ValidationErrorCode
{
    NoRegisteredCommands = 0,
    UsingReservedCommandName,
    InputTemplateIsNullOrEmpty,
    DuplicateInputTemplate,
    MissingPropertyInCommandClass,
    DuplicateParameterName,
    RepeatableOnlyNameParameter,
    RequiredOnlyNameParameter,
    OnlyNameParameterTypeNotBool,
}