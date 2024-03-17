using System.ComponentModel.DataAnnotations;
using Domain;
using Domain.Players;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp;

public class UniquePlayerNamesAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is GameState gameState
            && gameState.Players.Select(p => p.Name).Distinct().Count() != gameState.Players.Count)
        {
            return new ValidationResult("Player names must be unique.");
        }

        return ValidationResult.Success;
    }
}