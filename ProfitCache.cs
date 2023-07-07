using System;
using Terraria;

namespace MoneyProfits;

public readonly struct ProfitCache {
	private readonly string asString;

	public float Percent { get; }

	public ProfitCache(Recipe recipe) {
		ulong itemValue = CalculateValue(recipe.createItem);

		// Calculate ingredients total value.
		ulong ingredientsValue = 0;
		foreach (var ingredient in recipe.requiredItem) {
			ingredientsValue += CalculateValue(ingredient);
		}

		// Perhaps should somehow support recipe groups later...
		// But on other hand, they simply add their "iconic item" as ingredient
		// I really don't know what to do about them, to be honest.

		Percent = CalculatePercent(itemValue, ingredientsValue);

		// Make a string, so we don't have to re-make it every single draw tick.
		asString = $"[c/{ColorFromPercentage(Percent):X6}:{Percent:P0}]";

		static float CalculatePercent(float v1, float v2) {
			float dividend = v1;
			float divisor = 100f;
			if (v2 != 0f) {
				dividend -= v2;
				divisor = Math.Abs(v2);
			}

			return dividend / divisor;
		}

		static ulong CalculateValue(in Item item) {
			// Value and stack here realistically should never be negative, so we don't expect an overflow
			return (ulong)item.value / 5 * (ulong)item.stack;
		}

		static uint ColorFromPercentage(float value) {
			if (value > 0f)
				// Pale green
				return 0x98FB98;
			else if (value < 0f)
				// Crimson
				return 0xDC143C;
			else {
				// Gold
				return 0xFFD700;
			}
		}
	}

	public override readonly string ToString() => asString;
}