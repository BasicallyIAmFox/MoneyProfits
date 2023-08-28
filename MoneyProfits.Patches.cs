using System.Diagnostics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace MoneyProfits;

public partial class MoneyProfits {
	internal static void BetterCrafting_AssignRecipeIndexesToSlots(ILCursor c) {
		MatchCustomItemSlot(out int recipeIndex);
		c.Index += 3;
		c.Emit(OpCodes.Ldloc, recipeIndex);
		c.EmitDelegate((Item item, Recipe recipe) => {
			item = item.Clone();
			TryAssignRecipeIndex(item, recipe.RecipeIndex);
			return item;
		});

		void MatchCustomItemSlot(out int recipeIndex) {
			int rIndex = -1;

			/*
			// CustomItemSlot val = new CustomItemSlot(recipe.createItem, 0.75f);
			IL_011c: ldloc.3
			IL_011d: ldfld class [tModLoader]Terraria.Recipe BetterCrafting.UI.CraftingUIState/'<>c__DisplayClass12_0'::recipe
			IL_0122: ldfld class [tModLoader]Terraria.Item [tModLoader]Terraria.Recipe::createItem
			IL_0127: ldc.r4 0.75
			IL_012c: newobj instance void [SimpleUILib]SimpleUILib.CustomItemSlot::.ctor(class [tModLoader]Terraria.Item, float32)
			IL_0131: stloc.s 4
			 */
			c.GotoNext(
				i => i.MatchLdloc(out rIndex),
				i => i.MatchLdfld(out _),
				i => i.MatchLdfld<Recipe>(nameof(Recipe.createItem)),
				i => i.MatchLdcR4(out _),
				i => i.MatchNewobj(out _),
				i => i.MatchStloc(out _)
			);

			Debug.Assert(rIndex != -1);

			recipeIndex = rIndex;
		}
	}

	private static void AssignRecipeIndexesToWindow(ILCursor c) {
		int recipeIndexIndex = -1;

		/*
		// HoverItem = recipe[availableRecipe[num89]].createItem.Clone();
		IL_2c9e: ldsfld class Terraria.Recipe[] Terraria.Main::recipe
		IL_2ca3: ldsfld int32[] Terraria.Main::availableRecipe
		IL_2ca8: ldloc.s 156
		IL_2caa: ldelem.i4
		IL_2cab: ldelem.ref
		IL_2cac: ldfld class Terraria.Item Terraria.Recipe::createItem
		IL_2cb1: callvirt instance class Terraria.Item Terraria.Item::Clone()
		IL_2cb6: stsfld class Terraria.Item Terraria.Main::HoverItem
		 */
		c.GotoNext(MoveType.After,
			i => i.MatchLdsfld<Main>(nameof(Main.recipe)),
			i => i.MatchLdsfld<Main>(nameof(Main.availableRecipe)),
			i => i.MatchLdloc(out recipeIndexIndex),
			i => i.MatchLdelemI4(),
			i => i.MatchLdelemRef(),
			i => i.MatchLdfld<Recipe>(nameof(Recipe.createItem)),
			i => i.MatchCallvirt<Item>(nameof(Item.Clone)),
			i => i.MatchStsfld<Main>(nameof(Main.HoverItem))
		);

		Debug.Assert(recipeIndexIndex != -1);

		c.Emit(OpCodes.Ldloc, recipeIndexIndex);
		c.EmitDelegate((int recipeIndex) => {
			if (!Main.HoverItem.TryGetGlobalItem<ShowItemProfitability>(out var item))
				return;
			item.RecipeIndex = Main.availableRecipe[recipeIndex];
		});
	}

	private static void AssignRecipeIndexesToList(ILCursor c) {
		/*
		// HoverItem = recipe.createItem.Clone();
		IL_015b: ldloc.0
		IL_015c: ldfld class Terraria.Item Terraria.Recipe::createItem
		IL_0161: callvirt instance class Terraria.Item Terraria.Item::Clone()
		IL_0166: stsfld class Terraria.Item Terraria.Main::HoverItem
		 */
		c.GotoNext(MoveType.After,
			i => i.MatchLdloc(out _),
			i => i.MatchLdfld<Recipe>(nameof(Recipe.createItem)),
			i => i.MatchCallvirt<Item>(nameof(Item.Clone)),
			i => i.MatchStsfld<Main>(nameof(Main.HoverItem))
		);

		c.Emit(OpCodes.Ldarg, 0);
		c.EmitDelegate((int recipeIndex) => {
			if (!Main.HoverItem.TryGetGlobalItem<ShowItemProfitability>(out var item))
				return;
			item.RecipeIndex = Main.availableRecipe[recipeIndex];
		});
	}
}
