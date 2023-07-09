using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace MoneyProfits;

public partial class MoneyProfits {
	// Sorry future me
	private static void AssignRecipeIndexesToWindow(ILContext il) {
		var c = new ILCursor(il);

		try {
			int recipeIndexIndex = -1;

#if !TML_2022_09
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
#else
			/*
			// HoverItem = recipe[availableRecipe[num89]].createItem.Clone();
			IL_2e02: ldsfld class Terraria.Recipe[] Terraria.Main::recipe
			IL_2e07: ldsfld int32[] Terraria.Main::availableRecipe
			IL_2e0c: ldloc.s 155
			IL_2e0e: ldelem.i4
			IL_2e0f: ldelem.ref
			IL_2e10: ldfld class Terraria.Item Terraria.Recipe::createItem
			IL_2e15: callvirt instance class Terraria.Item Terraria.Item::Clone()
			IL_2e1a: stsfld class Terraria.Item Terraria.Main::HoverItem
			 */
#endif
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

			c.Emit(OpCodes.Ldloc, recipeIndexIndex);
			c.EmitDelegate((int recipeIndex) => {
				if (!Main.HoverItem.TryGetGlobalItem<ShowItemProfitability>(out var itemProfitability)) {
					return;
				}

				itemProfitability.RecipeIndex = recipeIndex;
			});
		}
#if TML_2022_09
		catch (System.Exception exception) {
			Instance.Logger.Error($"Failed to patch {il.Body.Method.FullName}. Stack trace: {exception.Message}");
		}
#else
		catch {
			Terraria.ModLoader.MonoModHooks.DumpIL(Instance, il);
		}
#endif
	}

	private static void AssignRecipeIndexesToList(ILContext il) {
		var c = new ILCursor(il);

		try {
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
				if (Main.HoverItem.TryGetGlobalItem<ShowItemProfitability>(out var itemProfitability)) {
					itemProfitability.RecipeIndex = recipeIndex;
				}
			});
		}
#if TML_2022_09
		catch (System.Exception exception) {
			Instance.Logger.Error($"Failed to patch {il.Body.Method.FullName}. Stack trace: {exception.Message}");
		}
#else
		catch {
			Terraria.ModLoader.MonoModHooks.DumpIL(Instance, il);
		}
#endif
	}
}
