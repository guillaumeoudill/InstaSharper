using System;
using InstaSharper.Classes.Models;

namespace InstaBot.Utils
{
    public static class ConsoleUtils
    {
        public static void PrintMedia(string header, InstaMedia media, int maxDescriptionLength)
        {
            Console.WriteLine(
                $"{header} [{media.User.UserName}]: {media.Caption?.Text}, {media.Code}, likes: {media.LikesCount}, multipost: {media.IsMultiPost}, viewcount: {media.ViewCount}, url: {media.Images[0].URI}");
        }
    }
}