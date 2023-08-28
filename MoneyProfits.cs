using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MagicStorage.Common.Systems;
using MagicStorage.UI;
using MagicStorage.UI.States;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MoneyProfits;

public sealed partial class MoneyProfits : Mod {
	// TODO: Consider moving this in its own class/file.
	private sealed class ShowItemProfitability : GlobalItem {
		public int RecipeIndex { get; set; } = -1;

		public sealed override bool InstancePerEntity => true;
		protected sealed override bool CloneNewInstances => false;

		public sealed override void ModifyTooltips(Item item, List<TooltipLine> lines) {
			int index = RecipeIndex;
			if (HasMagicStorage) {
				TryAssignCache_MagicStorage(ref index);
			}

			if (index != -1) {
				lines.Add(new TooltipLine(MoneyProfits.Instance, "Profit",
					Language.GetTextValue(
						"Mods.MoneyProfits.ProfitText",
						Cache[index].ToString())) {
					OverrideColor = Color.Gold
				});
			}
		}

		[JITWhenModsEnabled("MagicStorage")]
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void TryAssignCache_MagicStorage(ref int value) {
			var zone = (NewUISlotZone)Holder.MagicStorage_recipeHeaderZone.GetValue((CraftingUIState)MagicUI.craftingUI);
			var recipe = Holder.MagicStorage_selectedRecipe();

			if (zone != null && recipe != null) {
				value = recipe.RecipeIndex;
			}
		}
	}

	private sealed class ProfitCacheLoader : ModSystem {
		public sealed override void PostSetupRecipes() {
			Cache = Main.recipe.Take(Recipe.numRecipes).Select(x => new ProfitCache(x)).ToArray();
		}

		public sealed override void Unload() {
			if (Cache != null)
				Array.Clear(Cache);
		}
	}

	internal static ReflectionHolder Holder { get; private set; }

	public static MoneyProfits Instance { get; private set; }
	public static ProfitCache[] Cache { get; private set; }

	public static bool HasBetterCrafting { get; private set; }
	public static bool HasMagicStorage { get; private set; }

	public MoneyProfits() {
		Instance = this;
	}

	public sealed override void Load() {
		IL_Main.DrawInventory += (il) => {
			try {
				AssignRecipeIndexesToWindow(new ILCursor(il));
			}
			catch {
				MonoModHooks.DumpIL(Instance, il);
			}
		};
		IL_Main.HoverOverCraftingItemButton += (il) => {
			try {
				AssignRecipeIndexesToList(new ILCursor(il));
			}
			catch {
				MonoModHooks.DumpIL(Instance, il);
			}
		};

		HasBetterCrafting = ModLoader.HasMod("BetterCrafting");
		HasMagicStorage = ModLoader.HasMod("MagicStorage");

		Holder = new();
		Holder.Load();
	}

	public sealed override void Unload() {
		Holder?.Unload();
		Holder = null;

		Instance = null;
	}

	private static void TryAssignRecipeIndex(Item item, int index) {
		if (item.TryGetGlobalItem<ShowItemProfitability>(out var result))
			result.RecipeIndex = index;
	}
}