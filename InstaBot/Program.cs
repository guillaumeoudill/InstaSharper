using InstaBot.Utils;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using InstaSharper.Logger;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace InstaBot
{
    class Program
    {

        /// <summary>
        ///     Config values
        /// </summary>
        private static readonly int _maxDescriptionLength = 20;
        private const string DefaultTags = "#friders #fridersmtb #mtb #mtblife #mtbride #mtbiker #mtbiking #mtbike #mtbporn #mtbbike #mtblovers #mtbaddict #mtbworld #vtt #freeridemtb #fun #bike #mtbdh #dhmtb #cool #coolmtb #mountainbike #mtbpassion #adrenaline";

        /// <summary>
        ///     Api instance (one instance per Instagram user)
        /// </summary>
        private static IInstaApi _instaApi;

        static void Main(string[] args)
        {

            var result = Task.Run(MainAsync).GetAwaiter().GetResult();
            if (result)
                return;
            Console.ReadKey();
        }

        public static async Task<bool> MainAsync()
        {
            try
            {
                Console.WriteLine("Starting demo of InstaSharper project");
                // create user session data and provide login details
                var userSession = new UserSessionData
                {
                    UserName = "fridersdev",
                    Password = Environment.GetEnvironmentVariable("instaapiuserpassword")
            };

                var delay = RequestDelay.FromSeconds(2, 2);
                // create new InstaApi instance using Builder
                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions)) // use logger for requests and debug messages
                    .SetRequestDelay(delay)
                    .Build();

                const string stateFile = "state.bin";
                try
                {
                    if (File.Exists(stateFile))
                    {
                        Console.WriteLine("Loading state from file");
                        using (var fs = File.OpenRead(stateFile))
                        {
                            _instaApi.LoadStateDataFromStream(fs);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (!_instaApi.IsUserAuthenticated)
                {
                    // login
                    Console.WriteLine($"Logging in as {userSession.UserName}");
                    delay.Disable();
                    var logInResult = await _instaApi.LoginAsync();
                    delay.Enable();
                    if (!logInResult.Succeeded)
                    {
                        Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        return false;
                    }
                }
                var state = _instaApi.GetStateDataAsStream();
                using (var fileStream = File.Create(stateFile))
                {
                    state.Seek(0, SeekOrigin.Begin);
                    state.CopyTo(fileStream);
                }

                // get currently logged in user
                var currentUser = await _instaApi.GetCurrentUserAsync();
                Console.WriteLine(
                    $"Logged in: username - {currentUser.Value.UserName}, full name - {currentUser.Value.FullName}");

                //Get top post of the day

                var tag = "mtblife";
                var result = await _instaApi.GetTagFeedAsync(tag, PaginationParameters.MaxPagesToLoad(5));
                var tagFeed = result.Value;
                var anyMediaDuplicate = tagFeed.Medias.GroupBy(x => x.Code).Any(g => g.Count() > 1);
                var anyStoryDuplicate = tagFeed.Stories.GroupBy(x => x.Id).Any(g => g.Count() > 1);

                var likedMedias = tagFeed.Medias.Where(m => m.LikesCount > 0).OrderByDescending(m => m.LikesCount);
                var topPostofTheDay = tagFeed.Medias.Where(m => m.LikesCount > 0).OrderByDescending(m => m.LikesCount).First();

                var imageuri = GetImageUri(topPostofTheDay);
                var accountid = topPostofTheDay.User;

                var filePath = @"imageoftheday.jpg";

                //Download image
                using (var httpClient = new HttpClient())
                using (var contentStream = await httpClient.GetStreamAsync(imageuri))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, true))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }

                //Upload image test
                var mediaImage = new InstaImage
                {
                    Height = 1080,
                    Width = 1080,
                    URI = new Uri(Path.GetFullPath(filePath), UriKind.Absolute).LocalPath
                };

                string caption = $"#{tag} daily - post by @{topPostofTheDay.User.UserName} " +
                    $"\r\n______________________________" +
                    $"\r\n {DefaultTags}";

                var resultpost = await _instaApi.UploadPhotoAsync(mediaImage, caption);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // perform that if user needs to logged out
                // var logoutResult = Task.Run(() => _instaApi.LogoutAsync()).GetAwaiter().GetResult();
                // if (logoutResult.Succeeded) Console.WriteLine("Logout succeed");
            }
            return false;
        }

        private static string GetImageUri(InstaMedia topPostofTheDay)
        {
            if (topPostofTheDay.Images.Count > 0)
            {
                return topPostofTheDay.Images[0].URI;
            }
            if (topPostofTheDay.Carousel.Count > 0)
            {
                return topPostofTheDay.Carousel[0].Images[0].URI;
            }
            return null;
        }


    }
}



