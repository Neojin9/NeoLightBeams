using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace NeoLightBeams.Items.Placeable {

    public class FocusedLenses : ModItem {

        public const string FocusedLens = "FocusedLens";
        public const string FocusedBlackLens = "FocusedBlackLens";

        public override bool CloneNewInstances => true;

        private int _placeStyle;

        public FocusedLenses() { }

        public FocusedLenses(int placeStyle) {
            this._placeStyle = placeStyle;
        }

        public override bool Autoload(ref string name) {

            mod.AddItem(FocusedLens, new FocusedLenses(0));
            mod.AddItem(FocusedBlackLens, new FocusedLenses(1));

            return false;

        }

        public override void SetDefaults() {

            item.autoReuse = true;
            item.consumable = true;
            item.createTile = TileType<Tiles.FocusedLens>();
            item.height = 16;
            item.maxStack = 999;
            item.placeStyle = _placeStyle;
            item.rare = 1;
            item.useAnimation = 15;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;

            if (!GetInstance<NeoConfigServer>().DemoMode) {
                item.value = (_placeStyle == 0) ? Item.sellPrice(silver: 10) : Item.sellPrice(gold: 1);
            }

            item.width = 16;

        }

        public override void SetStaticDefaults() {
            
            if (Name == FocusedLens) {
                DisplayName.SetDefault("Focused Lens");
                Tooltip.SetDefault("Shiny.");
            } else if (Name == FocusedBlackLens) {
                DisplayName.SetDefault("Focused Black Lens");
                Tooltip.SetDefault("Glossy.");
            }

        }

        public override void AddRecipes() {

            ModRecipe recipe = new ModRecipe(mod);

            if (GetInstance<NeoConfigServer>().DemoMode) {
                recipe.AddIngredient(ItemID.DirtBlock);
            }
            else {

                int lensType = (_placeStyle == 0 ? ItemID.Lens : ItemID.BlackLens);

                recipe.AddIngredient(ItemID.Glass);
                recipe.AddIngredient(lensType);
                recipe.AddTile(TileID.WorkBenches);

            }

            recipe.SetResult(this);
            recipe.AddRecipe();

        }

    }

}
