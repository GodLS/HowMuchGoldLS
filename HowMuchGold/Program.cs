using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;

namespace HowMuchGold
{
    class Program
    {
        private static Obj_AI_Hero Player;
        private static Menu Config;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            Config = new Menu("HowMuchGold", "HowMuchGold", true);
            Config.AddItem(new MenuItem("permaShow", "PermaShow + Update Gold").SetValue(true));
            Config.AddItem(new MenuItem("calculationRange", "Calculation Range").SetValue(new Slider(1000, 1000, 2500)));
            Config.AddItem(new MenuItem("drawRange", "Draw Calculation Range").SetValue(true));
            Config.AddItem(new MenuItem("drawMinimap", "Draw Minimap Range").SetValue(true));

            Config.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;

        }

        private static void OnUpdate(EventArgs args)
        {
            var enemies = HeroManager.Enemies;

            foreach (var h in enemies)
            {
                // find out gold for killing them eventually (also jungle camps..)
            }
        }


        private static void OnEndScene(EventArgs args)
        {
            float calculationRange = Config.Item("calculationRange").GetValue<Slider>().Value;

            var heropos = Drawing.WorldToMinimap(Player.Position);

            if (Config.Item("drawMinimap").GetValue<bool>())
            {
                // Eventually draw text on minimap
                Utility.DrawCircle(Player.Position, calculationRange, System.Drawing.Color.White, 1, 30, true);
            }
        }


        private static void OnDraw(EventArgs args)
        {

            float calculationRange = Config.Item("calculationRange").GetValue<Slider>().Value;

            // Shit math
            var goldAmountMelee = 19.8 + (0.2 * (Game.ClockTime / 90));
            var roundedMelee = Math.Round(goldAmountMelee, 0, MidpointRounding.AwayFromZero);
            var goldAmountRange = 14.8 + (0.2 * (Game.ClockTime / 90));
            var roundedRange = Math.Round(goldAmountRange, 0, MidpointRounding.AwayFromZero);
            var goldAmountSiege = 40 + (0.5 * (Game.ClockTime / 90));
            var roundedSiege = Math.Round(goldAmountSiege, 0, MidpointRounding.AwayFromZero);

            // Finding minion count and their type in range
            var meleeMinions = MinionManager.GetMinions(calculationRange, MinionTypes.Melee, MinionTeam.Enemy);
            var siegeMinions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.BaseSkinName.Contains("Siege") && m.IsValidTarget(calculationRange)).ToList();
            var rangeMinions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.BaseSkinName.Contains("Ranged") && m.IsValidTarget(calculationRange)).ToList();

            // More shit math
            var melee = roundedMelee * meleeMinions.Count;
            var range = roundedRange * rangeMinions.Count;
            var siege = roundedSiege * siegeMinions.Count;
            var total = melee + range + siege;

            // Location for text
            var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (Config.Item("permaShow").GetValue<bool>())
            {
                Drawing.DrawText(heropos.X, heropos.Y + 15, Color.White, total + " total gold");
            }

            if (Config.Item("drawRange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, calculationRange, Color.Aqua, 5);
            }
        }
    }
}
