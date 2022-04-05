﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TbsCore.Helpers;
using TbsCore.Models.AccModels;

using static TbsCore.Helpers.Classificator;

namespace TbsCore.Tasks.LowLevel
{
    public class HeroEquip : BotTask
    {
        public List<(HeroItemEnum, int)> Items { get; set; }

        public override async Task<TaskRes> Execute(Account acc)
        {
            await NavigationHelper.ToHero(acc, NavigationHelper.HeroTab.Attributes);

            foreach (var use in Items)
            {
                var (item, amount) = use;

                var (category, _, _) = HeroHelper.ParseHeroItem(item);
                if (category != HeroItemCategory.Resource || category != HeroItemCategory.Stackable || category != HeroItemCategory.NonStackable)
                {
                    // Check if hero is at home
                    if (acc.Hero.Status != Hero.StatusEnum.Home)
                    {
                        // Wait for hero to come home
                        var nextExecute = acc.Hero.NextHeroSend > acc.Hero.HeroArrival ?
                            acc.Hero.NextHeroSend :
                            acc.Hero.HeroArrival;

                        var in5Min = DateTime.Now.AddMinutes(5);
                        if (nextExecute < in5Min) nextExecute = in5Min;
                        this.NextExecute = nextExecute;
                        return TaskRes.Retry;
                    }
                }

                string script = "var items = document.getElementById('itemsToSale');";

                switch (acc.AccInfo.ServerVersion)
                {
                    case ServerVersionEnum.T4_5:
                        script += $"items.querySelector('div[class$=\"_{(int)item}\"]').click();";
                        break;

                    case ServerVersionEnum.TTwars:
                        script += $"items.querySelector('div[class$=\"_{(int)item} \"]').click();";
                        break;
                }

                await DriverHelper.ExecuteScript(acc, script);

                // No amount specified, meaning we have already equipt the item
                if (amount == 0) continue;
                await Task.Delay(600);
                await DriverHelper.WriteById(acc, "amount", amount);

                await DriverHelper.ClickByClassName(acc, "ok");
                HeroHelper.ParseHeroPage(acc);
            }

            return Done(acc);
        }

        /// <summary>
        /// Refresh the hero page after equipping an item
        /// </summary>
        /// <param name="acc">Account</param>
        /// <returns>TaskRes</returns>

        private TaskRes Done(Account acc)
        {
            return TaskRes.Executed;
        }
    }
}