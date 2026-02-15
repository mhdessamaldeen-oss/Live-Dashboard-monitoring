namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : Exception
{
    public string Rule { get; }

    public BusinessRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }
}
