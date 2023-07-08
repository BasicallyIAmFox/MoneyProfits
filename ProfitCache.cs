using System;
using Terraria;
using Terraria.Localization;

namespace MoneyProfits;

public readonly struct ProfitCache {
	private const ulong ZeroMinusOne = unchecked(0ul - 1ul);

	private readonly string asString;
#if DEBUG
	private readonly ulong lower;
	private readonly ulong itemValue;
#endif

	public ProfitCache(Recipe recipe) {
		// Calculate item value.
#if !DEBUG
		ulong
#endif
		itemValue = CalculateValue(recipe.createItem);

		// Calculate ingredients total value.
		ulong ingredientsValue = 0;
		foreach (var ingredient in recipe.requiredItem) {
			ingredientsValue += CalculateValue(ingredient);
		}

		// Perhaps should somehow support recipe groups later...
		// But on other hand, they simply add their "iconic item" as ingredient
		// I really don't know what to do about them, to be honest.

#if !DEBUG
		ulong
#endif
		lower = itemValue - ingredientsValue;

		// Make a string, so we don't have to re-make it every single draw tick.
		asString = GetString(lower, itemValue);

		static ulong CalculateValue(in Item item) {
			// Value and stack here realistically should never be negative, so we don't expect an overflow
			return (ulong)item.value / 5 * (ulong)item.stack;
		}
	}

	public override readonly string ToString() {
#if DEBUG
		return GetString(lower, itemValue);
#else
		return asString;
#endif
	}

	private static string GetString(ulong lower, ulong itemValue) {
		ulong upper = (lower > itemValue) ? ZeroMinusOne : 0UL;
		long lowerAsLong = unchecked((long)lower);

		return string.Format("[c/{0}:{1}{2}]",
			ColorFromCoinValue((upper, lower)).ToString("X6"),

			lowerAsLong < 0 ? "-" : (lowerAsLong == 0 ? "" : "+"),
			OrIfEmpty(CoinsToName(Math.Abs(lowerAsLong)), "0 Copper").ToString()
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

			bool IsLeftGreaterThanZero() {
				if (IsNegative() == false) {
					return value.lower > 0;
				}

				return false;
			}

			bool IsLeftLesserThanZero() {
				if (IsNegative() == false) {
					return value.lower < 0;
				}

				return IsNegative();
			}
		}

		static ReadOnlySpan<char> OrIfEmpty(in ReadOnlySpan<char> value, in ReadOnlySpan<char> other) {
			if (value.IsEmpty)
				return other;
			return value;
		}
	}

	// The only reason this method exists is because PopupText.ValueToName in 1.4.3 takes 32-bit integer instead of 64-bit.
	private static string CoinsToName(long coinValue) {
		int platinum = 0;
		int gold = 0;
		int silver = 0;
		int copper = 0;

		while (coinValue > 0) {
			if (coinValue >= 1000000) {
				coinValue -= 1000000;
				platinum++;
			}
			else if (coinValue >= 10000) {
				coinValue -= 10000;
				gold++;
			}
			else if (coinValue >= 100) {
				coinValue -= 100;
				silver++;
			}
			else if (coinValue >= 1) {
				coinValue--;
				copper++;
			}
		}

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