﻿#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using HtmlAgilityPack;
using Tabster.Core;
using Tabster.Core.Plugins;

#endregion

namespace UltimateGuitar
{
    public class UltimateGuitarSearch : ISearchService
    {
        #region Implementation of ISearchService

        public string Name
        {
            get { return "Ultimate Guitar"; }
        }

        public ITabParser Parser
        {
            get { return new UltimateGuitarParser(); }
        }

        public SearchServiceOptions Options
        {
            get { return SearchServiceOptions.None; }
        }

        public Tab[] Search(string artist, string title, TabType? type)
        {
            var results = new List<Tab>();

            var searchString = (artist + " " + title).Trim().Replace(" ", "+");

            var urlString = string.Format("http://www.ultimate-guitar.com/search.php?w=songs&s={0}", searchString);

            if (type.HasValue)
            {
                var typeID = "0";

                switch (type)
                {
                    case TabType.Guitar:
                        typeID = "200";
                        break;
                    case TabType.Chords:
                        typeID = "300";
                        break;
                    case TabType.Bass:
                        typeID = "400";
                        break;
                    case TabType.Drum:
                        typeID = "700";
                        break;
                    case TabType.Ukulele:
                        typeID = "800";
                        break;
                }

                urlString += string.Format("&type={0}", typeID);
            }

            var url = new Uri(urlString);

            string data;

            var client = new WebClient {Proxy = null};
            {
                data = client.DownloadString(url);
            }

            if (data.Length > 0 && !data.Contains("No results"))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                var tresults = doc.DocumentNode.SelectSingleNode("//table[@class='tresults']");

                if (tresults != null)
                {
                    var count = 0;
                    var rows = tresults.SelectNodes("tr");
                    var loopArtist = ""; //store last artist

                    foreach (var row in rows)
                    {
                        //skip first (header) row
                        if (count > 0)
                        {
                            var columns = row.SelectNodes("td");

                            //column indexes
                            var colIndexArtist = 0;
                            var colIndexSong = 1;
                            var colIndexType = 3;

                            var attemptedBreaking = row.InnerHtml.Contains("THIS APP DOESN'T HAVE RIGHTS TO DISPLAY TABS");

                            if (attemptedBreaking)
                            {
                                colIndexArtist += 1;
                                colIndexSong += 1;
                                colIndexType += 1;
                            }

                            var rowArtist = columns[colIndexArtist].InnerText;

                            if ((string.IsNullOrEmpty(loopArtist) || loopArtist != rowArtist) && rowArtist != "&nbsp;")
                            {
                                loopArtist = HttpUtility.HtmlDecode(rowArtist);
                            }

                            var rowType = GetTabType(columns[colIndexType].InnerText);

                            if (rowType.HasValue)
                            {
                                var rowURL = columns[colIndexSong].ChildNodes["a"].Attributes["href"].Value;
                                var rowSong = HttpUtility.HtmlDecode(columns[colIndexSong].ChildNodes["a"].InnerText);

                                if (!type.HasValue || rowType == type)
                                {
                                    var tab = new Tab(loopArtist, rowSong, rowType.Value, null) { Source = new Uri(rowURL) };
                                    results.Add(tab);
                                }
                            }
                        }

                        count++;
                    }
                }
            }

            return results.ToArray();
        }

        public bool SupportsTabType(TabType type)
        {
            switch (type)
            {
                case TabType.Guitar:
                case TabType.Chords:
                case TabType.Bass:
                case TabType.Drum:
                case TabType.Ukulele:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Static Methods

        private static TabType? GetTabType(string str)
        {
            if (str.Equals("tab", StringComparison.InvariantCultureIgnoreCase))
                return TabType.Guitar;
            if (str.Equals("chords", StringComparison.InvariantCultureIgnoreCase))
                return TabType.Chords;
            if (str.Equals("bass", StringComparison.InvariantCultureIgnoreCase))
                return TabType.Bass;
            if (str.Equals("drums", StringComparison.InvariantCultureIgnoreCase))
                return TabType.Drum;
            if (str.Equals("ukulele", StringComparison.InvariantCultureIgnoreCase))
                return TabType.Ukulele;

            return null;
        }

        #endregion
    }
}