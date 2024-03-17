using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Domain;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UnoEngine;
using WebApp.Pages;

namespace WebApp;

[AttributeUsage(AttributeTargets.Property)]
public class EnoughCardsAttribute : ValidationAttribute
{
    private readonly string _playerCountProperty;

    public EnoughCardsAttribute(string playerCountProperty)
    {
        _playerCountProperty = playerCountProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var playerCountProperty = validationContext.ObjectType.GetProperty(_playerCountProperty);

        if (playerCountProperty == null)
        {
            return new ValidationResult($"Unknown property: {_playerCountProperty}");
        }

        var playerCountValue = (int)(playerCountProperty.GetValue(validationContext.ObjectInstance)
                                     ?? throw new InvalidOperationException());

        if (value is not bool[] cardsInGame) return ValidationResult.Success;
        var configuration = CreateModel.GetConfig(cardsInGame);

        return !Engine.IsValidConfiguration(configuration, playerCountValue)
            ? new ValidationResult("There are not enough cards in the deck to start the game!")
            : ValidationResult.Success;
    }
}