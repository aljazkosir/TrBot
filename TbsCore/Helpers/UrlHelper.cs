﻿using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using TbsCore.Models.AccModels;

namespace TbsCore.Helpers
{
    public static class UrlHelper
    {
        public static async Task<bool> MainNavigate(Account acc, HtmlDocument html, MainNavigationButton button)
        {
            var nav = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.Id.CompareTo("navigation") == 0);
            if (nav == null) return false;
            var accessKey = (int)button;
            var a = nav.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("accesskey", 8) == accessKey);
            if (a == null) return false;

            acc.Wb.FindElementByXPath($"//a[@accesskey={accessKey}]").Click();
            await DriverHelper.WaitLoaded(acc);
            return true;
        }

        public static async Task<bool> BuildNavigate(Account acc, HtmlDocument html, int index)
        {
            if (index < 19) // dorf1
            {
                var fields = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.Id.CompareTo("resourceFieldContainer") == 0);
                if (fields == null) return false;
                var field = fields.Descendants("a").FirstOrDefault(x => x.HasClass($"buildingSlot{index}"));
                if (field == null) return false;

                acc.Wb.FindElementByXPath($"//a[contains(concat(' ', @class, ' '), ' buildingSlot{index} ')]").Click();
                await DriverHelper.WaitLoaded(acc);
                return true;
            }
            else // dorf2
            {
                var fields = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.Id.CompareTo("villageContent") == 0);
                if (fields == null) return false;
                var field = fields.Descendants("div").FirstOrDefault(x => x.HasClass($"a{index}"));
                if (field == null) return false;
                var g = field.Descendants("g").FirstOrDefault();
                if (g == null) // empty building
                {
                    acc.Wb.FindElementByXPath($"//div[contains(concat(' ', @class, ' '), ' a{index} ')]/*[name()='svg']/*").Click();
                }
                else // there is building
                {
                    acc.Wb.FindElementByXPath($"//div[contains(concat(' ', @class, ' '), ' a{index} ')]/a").Click();
                }

                await DriverHelper.WaitLoaded(acc);
                return true;
            }
        }

        public static async Task<bool> RallyPointNavigate(Account acc, HtmlDocument html, RallyPointTab index)
        {
            var scrolling = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("scrollingContainer"));
            if (scrolling == null) return false;
            var divs = scrolling.Descendants("div").ToList();
            if (divs.Count > 5) return false;
            var div = divs[(int)index];
            if (div == null) return false;
            var a = div.Descendants("a").FirstOrDefault();
            if (a == null) return false;
            var id = a.Id;
            acc.Wb.Driver.FindElementById(id).Click();
            await DriverHelper.WaitLoaded(acc);
            return true;
        }

        public static async Task<bool> MarketPlaceNavigate(Account acc, HtmlDocument html, MarketPlaceTab index)
        {
            var scrolling = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("scrollingContainer"));
            if (scrolling == null) return false;
            var divs = scrolling.Descendants("div").ToList();
            if (divs.Count > 4) return false;
            var div = divs[(int)index];
            if (div == null) return false;
            var a = div.Descendants("a").FirstOrDefault();
            if (a == null) return false;
            acc.Wb.Driver.FindElementById(a.Id).Click();
            await DriverHelper.WaitLoaded(acc);
            return true;
        }

        public static async Task<bool> UpgradeBuilding(Account acc, HtmlDocument html, UpgradeButton type)
        {
            var div = html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass($"section{(int)type}"));
            if (div == null) return false;
            var button = div.Descendants("button").FirstOrDefault();
            if (button == null) return false;
            acc.Wb.Driver.FindElementById(button.Id).Click();
            await DriverHelper.WaitLoaded(acc);
            return true;
        }

        public enum MainNavigationButton
        {
            Resources = 1,
            Buildings,
            Map,
            Statistics,
            Reports,
            Messages,
            DailyQuests
        }

        public enum RallyPointTab
        {
            Managenment = 0,
            Overview,
            SendTroops,
            CombatSimulator,
            Farmlist
        };

        public enum MarketPlaceTab
        {
            Managenment = 0,
            SendResources,
            Buy,
            Offer
        }

        public enum UpgradeButton
        {
            Normal = 1,
            Faster,
        }
    }
}