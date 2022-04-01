﻿using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading.Tasks;
using TbsCore.Helpers;
using TbsCore.Models.AccModels;
using TbsCore.Parsers;
using static TbsCore.Helpers.Classificator;

namespace TbsCore.Tasks.LowLevel
{
    public class SellOnAuctions : BotTask
    {
        public override async Task<TaskRes> Execute(Account acc)
        {
            if (acc.AccInfo.ServerVersion == ServerVersionEnum.TTwars) return TaskRes.Executed;
            string xPathSellTab = null;

            switch (acc.AccInfo.ServerVersion)
            {
                case ServerVersionEnum.TTwars:
                    xPathSellTab = "//*[@id='content']/div[4]/div[2]/div[3]/a";
                    break;

                case ServerVersionEnum.T4_5:
                    xPathSellTab = "//*[@id='heroAuction']/div[2]/div[2]/div[3]/a";
                    break;
            }
            // enter right tab
            do
            {
                await NavigationHelper.ToHero(acc, NavigationHelper.HeroTab.Auctions);

                var node = acc.Wb.Html.DocumentNode.SelectSingleNode(xPathSellTab);
                if (node == null) continue;
                var element = acc.Wb.Driver.FindElement(By.XPath(node.XPath));
                if (element == null) continue;
                element.Click();

                try
                {
                    await DriverHelper.WaitPageChange(acc, "auction?action=sell");
                }
                catch
                {
                    continue;
                }
                break;
            }
            while (true);

            acc.Wb.UpdateHtml();
            var nodeAllItem = acc.Wb.Html.GetElementbyId("itemsToSale");
            if (nodeAllItem == null) return TaskRes.Executed;

            var nodeItems = nodeAllItem.Descendants("div").Where(x => !x.HasClass("disabled") && x.HasClass("item"));
            if (nodeItems.Count() == 0) return TaskRes.Executed;

            foreach (var nodeItem in nodeItems)
            {
                (var heroItemEnum, int amount) = HeroParser.ParseItemNode(nodeItem);
                if (heroItemEnum == null) continue;
                if (HeroHelper.GetHeroItemCategory(heroItemEnum ?? HeroItemEnum.Others_None_0) == HeroItemCategory.Horse) continue;
                if (HeroHelper.GetHeroItemCategory(heroItemEnum ?? HeroItemEnum.Others_None_0) == HeroItemCategory.Others && amount < 5) continue;

                var nodeParentItem = nodeItem.ParentNode;
                var nodeItemXPath = acc.Wb.Html.DocumentNode.SelectSingleNode($"//*[@id='{nodeParentItem.Id}']/div");
                if (nodeItem.Id != nodeItemXPath.Id) continue;
                var element = acc.Wb.Driver.FindElement(By.XPath(nodeItemXPath.XPath));
                if (element == null) continue;
                element.Click();
                await Task.Delay(600);

                int counter = 3;
                do
                {
                    counter--;
                    acc.Wb.UpdateHtml();
                    var nodeDialog = acc.Wb.Html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("dialogVisible"));
                    if (nodeDialog == null) continue;

                    var button = acc.Wb.Html.DocumentNode.SelectSingleNode("//*[@id='mainLayout']/body/div[1]/div/div/div/div/form/div[6]/button");
                    if (button == null) continue;
                    try
                    {
                        var elementButton = acc.Wb.Driver.FindElement(By.XPath(button.XPath));
                        if (elementButton == null) continue;
                        elementButton.Click();
                    }
                    catch { }
                    break;
                }
                while (counter > 0);
            }

            return TaskRes.Executed;
        }
    }
}