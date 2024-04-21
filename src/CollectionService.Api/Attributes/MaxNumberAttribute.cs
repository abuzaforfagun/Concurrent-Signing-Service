using System.ComponentModel.DataAnnotations;

namespace CollectionService.Api.Attributes;

public class MaxNumberAttribute : ValidationAttribute
{
    private readonly int _maxValue;

    public MaxNumberAttribute(int maxValue)
    {
        _maxValue = maxValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Null values are considered valid
        }

        if (value is IComparable comparableValue && comparableValue.CompareTo(_maxValue) > 0)
        {
            return new ValidationResult($"The field {validationContext.DisplayName} must be less than or equal to {_maxValue}.");
        }

        return ValidationResult.Success;
    }
}