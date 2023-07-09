using System.Collections.Generic;
using Microsoft.Xna.Framework;
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

		public sealed override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (RecipeIndex != -1) {
				tooltips.Add(new TooltipLine(
					MoneyProfits.Instance, "Profit",
					Language.GetTextValue(
						"Mods.MoneyProfits.ProfitText",
						ProfitCacheLoader.Cache[Main.availableRecipe[RecipeIndex]].ToString()
					)) {
					OverrideColor = Color.Gold
				});
			}
		}
	}

	public static MoneyProfits Instance { get; private set; }

	public MoneyProfits() {
		Instance = this;
	}

	public sealed override void Load() {
#if !TML_2022_09
		IL_Main.DrawInventory += AssignRecipeIndexesToWindow;
		IL_Main.HoverOverCraftingItemButton += AssignRecipeIndexesToList;
#else
		IL.Terraria.Main.DrawInventory += AssignRecipeIndexesToWindow;
		IL.Terraria.Main.HoverOverCraftingItemButton += AssignRecipeIndexesToList;
#endif
	}

	public sealed override void Unload() {
#if !TML_2022_09
		IL_Main.DrawInventory -= AssignRecipeIndexesToWindow;
		IL_Main.HoverOverCraftingItemButton -= AssignRecipeIndexesToList;
#else
		IL.Terraria.Main.DrawInventory -= AssignRecipeIndexesToWindow;
		IL.Terraria.Main.HoverOverCraftingItemButton -= AssignRecipeIndexesToList;
#endif

		Instance = null;
	}
}