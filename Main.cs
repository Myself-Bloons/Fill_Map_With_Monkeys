using System.Threading.Tasks;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using Il2CppInterop.Runtime;
using UnityEngine;
using Vector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;
using Vector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

[assembly: MelonInfo(typeof(FillMapWithMonkeys.Main), FillMapWithMonkeys.ModHelperData.Name, FillMapWithMonkeys.ModHelperData.Version, FillMapWithMonkeys.ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace FillMapWithMonkeys;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    private static MelonLogger.Instance Logger = null!;

    public override void OnInitialize()
    {
        Logger = LoggerInstance;
    }

    private static readonly ModSettingHotkey FillMapHotkey = new(KeyCode.BackQuote, HotkeyModifier.Ctrl)
    { description = "Fill the map with the currently selected tower" };

    public override void OnUpdate()
    {
        if (FillMapHotkey.JustPressed())
        {
            try { FillMap(); }
            catch (System.Exception ex) { Logger.Error($"FillMap error: {ex}"); }
        }
    }

    private static void FillMap()
    {
        if (ShopMenu.instance == null || ShopMenu.instance.selectedButton == null)
        {
            Logger.Warning("No tower selected in the shop."); 
            return;
        }

        var selected = ShopMenu.instance.selectedButton;
        var towerModel = selected.TowerModel.Duplicate();
        towerModel.skinName = string.Empty;

        var game = InGame.instance;
        if (game == null || game.bridge == null)
        {
            Logger.Warning("Not in-game yet."); 
            return;
        }

        for (int x = 0; x < Constants.worldWidth; x++)
        {
            for (int y = 0; y < Constants.worldHeight; y++)
            {
                var position = new Vector3(Constants.worldXMin + x, Constants.worldZMin + y);

                var map = game.GetMap();
                if (map == null) continue;

                var areaAtPoint = map.GetAreaAtPoint(position.ToVector2());

                if (!game.GetUnityToSimulation().Simulation.Map.CanPlace(position.ToVector2(), towerModel))
                    continue;

                game.GetTowerManager().CreateTower(
                    towerModel,
                    position,
                    game.bridge.MyPlayerNumber,
                    areaAtPoint?.GetAreaID() ?? ObjectId.FromData(1),
                    ObjectId.FromData(uint.MaxValue),
                    null,
                    false,
                    false,
                    0,
                    false
                );
            }
        }

        Logger.Msg("Done filling map.");
    }
}