using System;
using Terraria;
using Terraria.ModLoader;

namespace MoneyProfits;

public sealed class ProfitCacheLoader : ModSystem {
	public static ProfitCache[] Cache { get; private set; }

	public override void PostSetupRecipes() {
		Cache = new ProfitCache[Recipe.numRecipes];

		for (int i = 0; i < Recipe.numRecipes; i++) {
			Cache[i] = new(Main.recipe[i]);
		}
	}

	public sealed override void Unload() => Array.Clear(Cache);
}
