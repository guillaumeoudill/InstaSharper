﻿using System;
using System.Linq;
using System.Threading.Tasks;
using InstaSharper.API;
using InstaSharper.Classes;
using InstaSharper.Examples.Utils;

namespace InstaSharper.Examples.Samples
{
    internal class DefaultStats : IDemoSample
    {
        /// <summary>
        ///     Config values
        /// </summary>
        private static readonly int _maxDescriptionLength = 20;

        private readonly IInstaApi _instaApi;

        public DefaultStats(IInstaApi instaApi)
        {
            _instaApi = instaApi;
        }

        public async Task DoShow()
        {
            // get currently logged in user
            var currentUser = await _instaApi.GetCurrentUserAsync();
            Console.WriteLine(
                $"Logged in: username - {currentUser.Value.UserName}, full name - {currentUser.Value.FullName}");

            // get followers of user 'currentuser'
            var followers = await _instaApi.GetUserFollowersAsync(currentUser.Value.UserName,
                PaginationParameters.MaxPagesToLoad(5));
            Console.WriteLine($"Count of followers for {currentUser.Value.UserName} : {followers.Value.Count}");

            // get self folling 
            var following = await _instaApi.GetUserFollowingAsync(currentUser.Value.UserName, PaginationParameters.MaxPagesToLoad(5));
            Console.WriteLine($"Count of following [{currentUser.Value.UserName}]:{following.Value.Count}");

            // get self user's media, latest 5 pages
            var currentUserMedia = await _instaApi.GetUserMediaAsync(currentUser.Value.UserName, PaginationParameters.MaxPagesToLoad(5));
            if (currentUserMedia.Succeeded)
            {
                Console.WriteLine($"Media count [{currentUser.Value.UserName}]: {currentUserMedia.Value.Count}");
                foreach (var media in currentUserMedia.Value)
                    ConsoleUtils.PrintMedia("Self media", media, _maxDescriptionLength);
            }

            //get user time line feed, latest 5 pages
            var userFeed = await _instaApi.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(10));
            if (userFeed.Succeeded)
            {
                Console.WriteLine(
                    $"Feed items (in {userFeed.Value.MediaItemsCount} pages) [{currentUser.Value.UserName}]: {userFeed.Value.Medias.Count}");
                foreach (var media in userFeed.Value.Medias)
                    ConsoleUtils.PrintMedia("Feed media", media, _maxDescriptionLength);
                //like first 10 medias from user timeline feed
                foreach (var media in userFeed.Value.Medias.Take(50))
                {
                    var likeResult = await _instaApi.LikeMediaAsync(media.InstaIdentifier);
                    var resultString = likeResult.Value ? "liked" : "not liked";
                    Console.WriteLine($"Media {media.Code} {resultString} {media.Images[0].URI}");
                }
            }



            //// get tag feed, latest 5 pages
            //var tagFeed = await _instaApi.GetTagFeedAsync("quadcopter", PaginationParameters.MaxPagesToLoad(5));
            //if (tagFeed.Succeeded)
            //{
            //    Console.WriteLine(
            //        $"Tag feed items (in {tagFeed.Value.MediaItemsCount} pages) [{currentUser.Value.UserName}]: {tagFeed.Value.Medias.Count}");
            //    foreach (var media in tagFeed.Value.Medias)
            //        ConsoleUtils.PrintMedia("Tag feed", media, _maxDescriptionLength);
            //}
        }
    }
}