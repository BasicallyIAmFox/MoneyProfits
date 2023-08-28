using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MagicStorage;
using MonoMod.Cil;
using MonoMod.Utils;
using Terraria;
using Terraria.ModLoader;

namespace MoneyProfits;

internal sealed class ReflectionHolder {
	public Func<Recipe> BetterCrafting_recipetocraft = null;

	public FieldInfo MagicStorage_recipeHeaderZone = null;
	public Func<Recipe> MagicStorage_selectedRecipe = null;

	public void Load() {
		if (MoneyProfits.HasBetterCrafting) {
			LoadBetterCrafting();
		}

		if (MoneyProfits.HasMagicStorage) {
			LoadMagicStorage();
		}
	}

	[JITWhenModsEnabled("BetterCrafting")]
	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LoadBetterCrafting() {
		MonoModHooks.Modify(typeof(BetterCrafting.UI.CraftingUIState).FindMethod("SearchRecipes"), (il) => {
			try {
				MoneyProfits.BetterCrafting_AssignRecipeIndexesToSlots(new ILCursor(il));
			}
			catch {
				MonoModHooks.DumpIL(MoneyProfits.Instance, il);
			}
		});
	}

	[JITWhenModsEnabled("MagicStorage")]
	[MethodImpl(MethodImplOptions.NoInlining)]
	private void LoadMagicStorage() {
		MagicStorage_recipeHeaderZone = typeof(MagicStorage.UI.States.CraftingUIState).GetField("recipeHeaderZone", BindingFlags.Instance | BindingFlags.NonPublic);
		MagicStorage_selectedRecipe = Expression.Lambda<Func<Recipe>>(Expression.Field(null,
			typeof(CraftingGUI).GetField("selectedRecipe", BindingFlags.Static | BindingFlags.NonPublic)
		)).Compile();
	}

	public void Unload() {
		MagicStorage_recipeHeaderZone = null;
		MagicStorage_selectedRecipe = null;
	}
}
