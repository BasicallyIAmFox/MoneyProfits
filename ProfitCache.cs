using System;
using Terraria;
using Terraria.Localization;

namespace MoneyProfits;

public readonly struct ProfitCache {
	private const ulong ZeroMinusOne = unchecked(0ul - 1ul);

	private readonly string asString;

	public ProfitCache(Recipe recipe) {
		// Calculate item value.
		ulong itemValue = CalculateValue(recipe.createItem);

		// Calculate ingredients total value.
		ulong ingredientsValue = 0;
		foreach (var ingredient in recipe.requiredItem) {
			// Perhaps should somehow support recipe groups later...
			// But on other hand, they simply add their "iconic item" as ingredient
			// I really don't know what to do about them, to be honest.
			ingredientsValue += CalculateValue(ingredient);
		}

		ulong lower = itemValue - ingredientsValue;

		// Make a string, so we don't have to re-make it every single draw tick.
		asString = GetString(lower, itemValue);

		static ulong CalculateValue(in Item item) {
			// Value and stack here realistically should never be negative, so we don't expect an overflow
			return (ulong)item.value / 5 * (ulong)item.stack;
		}
	}

	public override readonly string ToString() => asString;

	private static string GetString(ulong lower, ulong itemValue) {
		ulong upper = (lower > itemValue) ? ZeroMinusOne : 0UL;
		long lowerAsLong = unchecked((long)lower);

		return string.Format("[c/{0}:{1}{2}]",
			ColorFromCoinValue((upper, lower)).ToString("X6"),

			lowerAsLong < 0 ? "-" : (lowerAsLong == 0 ? "" : "+"),
			OrIfEmpty(EfficientCoinsToName(Math.Abs(lowerAsLong)), $"0 {Language.GetTextValue("Currency.Copper")}").ToString()
		);

		static uint ColorFromCoinValue((ulong upper, ulong lower) value) {
			// value > 0
			if (IsLeftGreaterThanZero())
				// Pale green
				return 0x98FB98;
			// value < 0
			else if (IsLeftLesserThanZero())
				// Crimson
				return 0xDC143C;
			// value == 0
			else {
				// Gold
				return 0xFFD700;
			}

			bool IsNegative() => (long)value.upper < 0;

			bool IsLeftGreaterThanZero() => !IsNegative() && value.lower > 0;

			bool IsLeftLesserThanZero() => IsNegative() || value.lower < 0;
		}

		static ReadOnlySpan<char> OrIfEmpty(in ReadOnlySpan<char> input, in ReadOnlySpan<char> value) => input.IsEmpty ? value : input;
	}

	private static string EfficientCoinsToName(long coinValue) {
		int platinum = 0;
		int gold = 0;
		int silver = 0;
		int copper = 0;

		coinValue -= (platinum	+= (int)(coinValue / 1000000)) * 1000000;
		coinValue -= (gold		+= (int)(coinValue / 10000	)) * 10000;
		coinValue -= (silver	+= (int)(coinValue / 100	)) * 100;
		copper += (int)coinValue;

		string text = string.Empty;
		if (platinum > 0) {
			text += platinum + string.Format(" {0} ", Language.GetTextValue("Currency.Platinum"));
		}

		if (gold > 0) {
			text += gold + string.Format(" {0} ", Language.GetTextValue("Currency.Gold"));
		}

		if (silver > 0) {
			text += silver + string.Format(" {0} ", Language.GetTextValue("Currency.Silver"));
		}

		if (copper > 0) {
			text += copper + string.Format(" {0} ", Language.GetTextValue("Currency.Copper"));
		}

		if (text.Length > 1) {
			text = text[..^1];
		}

		return text;
	}
}