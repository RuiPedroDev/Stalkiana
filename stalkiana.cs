﻿/*

Do not use the tool multiple times per day or you might get flagged by instagram

*/

using System;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stalkiana_Console
{
    internal class Program
    {
        public static RestClient client = new RestClient("https://www.instagram.com");
        static void displayStartingScreen()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Stalkiana the instagram stalking tool\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"  _________  __           .__    __    .__                         
 /   _____/_/  |_ _____   |  |  |  | __|__|_____     ____  _____   
 \_____  \ \   __\\__  \  |  |  |  |/ /|  |\__  \   /    \ \__  \  
 /        \ |  |   / __ \_|  |__|    < |  | / __ \_|   |  \ / __ \_
/_______  / |__|  (____  /|____/|__|_ \|__|(____  /|___|  /(____  /
        \/             \/            \/         \/      \/      \/ ");
            Console.ResetColor();
            Console.Write("\nThis is a tool used for stalking an Instagram user\n");
        }
        static string getUsername()
        {
            string username;
            do
            {
                Console.Write("\nPlease input the username to stalk: ");
                username = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Username cannot be empty. Please enter a valid username.");
                }
            } while (string.IsNullOrWhiteSpace(username));
            return username;
        }
        static string getOption()
        {
            string option;
            do
            {
                Console.WriteLine("\n1- Download Profile Picture");
                Console.WriteLine("2- Get Followers/Following\n");
                Console.Write("\nChoose what you want to do: ");
                option = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(option))
                {
                    Console.WriteLine("Option cannot be empty. Please enter a valid option (1 or 2).");
                }
            } while (option != "1" && option != "2");
            return option;
        }

        static Dictionary<string, string>? getFollowersList(string userPK, string cookie, int minTime, int maxTime)
        {
            var list = new Dictionary<string, string>();
            bool hasNext = true;
            string? after = null!;
            while (hasNext)
            {
                var request = new RestRequest("/graphql/query/", Method.Get);
                request.AddQueryParameter("query_hash", "c76146de99bb02f6415203be841dd25a");
                request.AddQueryParameter("id", userPK);
                request.AddQueryParameter("include_reel", "true");
                request.AddQueryParameter("fetch_mutual", "true");
                request.AddQueryParameter("first", "50");
                request.AddQueryParameter("after", after);
                request.AddHeader("cookie", cookie);
                var response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    try
                    {
                        dynamic? obj = JsonConvert.DeserializeObject(response.Content!);
                        hasNext = obj!.data.user.edge_followed_by.page_info.has_next_page;
                        after = obj.data.user.edge_followed_by.page_info.end_cursor;
                        foreach (dynamic follower in obj.data.user.edge_followed_by.edges)
                        {
                            list[(string)follower.node.id] = (string)follower.node.username;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Error fetching followings: {e.Message}");
                        return null;
                    }
                    sleepRandom(minTime, maxTime);
                }
                else
                {
                    Console.WriteLine($"Error in fetching followers: {response.StatusCode}");
                    return null;
                }
            }
            return list;
        }

        static Dictionary<string, string>? getFollowingList(string userPK, string cookie, int minTime, int maxTime)
        {
            var list = new Dictionary<string, string>();
            bool hasNext = true;
            string? after = null!;
            while (hasNext)
            {
                var request = new RestRequest("/graphql/query/", Method.Get);
                request.AddQueryParameter("query_hash", "d04b0a864b4b54837c0d870b0e77e076");
                request.AddQueryParameter("id", userPK);
                request.AddQueryParameter("include_reel", "true");
                request.AddQueryParameter("fetch_mutual", "true");
                request.AddQueryParameter("first", "50");
                request.AddQueryParameter("after", after);
                request.AddHeader("cookie", cookie);
                var response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    try
                    {
                        dynamic? obj = JsonConvert.DeserializeObject(response.Content!);
                        hasNext = obj!.data.user.edge_follow.page_info.has_next_page;
                        after = obj.data.user.edge_follow.page_info.end_cursor;
                        foreach (dynamic following in obj.data.user.edge_follow.edges)
                        {
                            list[(string)following.node.id] = (string)following.node.username;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Error fetching followings: {e.Message}");
                        return null;
                    }
                    sleepRandom(minTime, maxTime);
                }
                else
                {
                    Console.WriteLine($"Error fetching followings: {response.StatusCode}");
                    return null;
                }
            }
            return list;
        }

        static string getCookie()
        {
            string cookie;
            do
            {
                Console.Write("Please input the full instagram cookie: ");
                cookie = Console.ReadLine()!;
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    Console.WriteLine("Cookie cannot be empty. Please enter a valid cookie.");
                }
            } while (string.IsNullOrWhiteSpace(cookie));
            return cookie;
        }

        static string? getUserPK(string cookie, string username)
        {
            string userPK;
            var request = new RestRequest("api/v1/web/search/topsearch/", Method.Get);
            request.AddHeader("cookie", cookie);
            request.AddQueryParameter("query", username);
            request.AddQueryParameter("context", "blended");
            request.AddQueryParameter("include_reel", "false");
            request.AddQueryParameter("search_surface", "web_top_search");
            var response1 = client.Execute(request);
            if (response1.IsSuccessful)
            {
                Console.WriteLine("\nRequest to get user PK completed succesfully");
                try
                {
                    dynamic? obj = JsonConvert.DeserializeObject(response1.Content!)!;
                    userPK = obj.users[0].user.pk!;
                    Console.WriteLine($"{username}: {userPK}\n");
                    return userPK;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error: {e.Message}");
                    return null;
                }
            }
            else
            {
                Console.Error.WriteLine($"\nError in request to get user PK (maybe cookie is invalid): {response1.StatusCode}");
                return null;
            }
        }

        static int getFollowingCount(string username)
        {
            var request = new RestRequest("/api/v1/users/web_profile_info/", Method.Get);
            request.AddQueryParameter("username", username);
            request.AddHeader("user-agent", "Instagram 76.0.0.15.395 Android (24/7.0; 640dpi; 1440x2560; samsung; SM-G930F; herolte; samsungexynos8890; en_US; 138226743)");
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                try
                {
                    dynamic? obj = JsonConvert.DeserializeObject(response.Content!)!;
                    Console.WriteLine("Get following count request completed succesfully\n");
                    return obj.data.user.edge_follow.count;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error in get following count request: {e.Message}");
                    return -1;
                }
            }
            else
            {
                Console.Error.WriteLine($"Error in get following count request: {response.StatusCode}");
                return -1;
            }
        }
        static int getFollowerCount(string username)
        {
            var request = new RestRequest("/api/v1/users/web_profile_info/", Method.Get);
            request.AddQueryParameter("username", username);
            request.AddHeader("user-agent", "Instagram 76.0.0.15.395 Android (24/7.0; 640dpi; 1440x2560; samsung; SM-G930F; herolte; samsungexynos8890; en_US; 138226743)");
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                try
                {
                    dynamic? obj = JsonConvert.DeserializeObject(response.Content!)!;
                    Console.WriteLine("Get follower count request completed succesfully\n");
                    return obj.data.user.edge_followed_by.count;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error in get follower count request: {e.Message}");
                    return -1;
                }
            }
            else
            {
                Console.Error.WriteLine($"Error in get follower count request: {response.StatusCode}");
                return -1;
            }
        }

        static Dictionary<string, string>? getDataFromFile(string filename)
        {
            string jsonString = File.ReadAllText(filename);

            try
            {
                var userList = JsonConvert.DeserializeObject<List<JObject>>(jsonString);
                var dictionary = new Dictionary<string, string>();

                foreach (JObject user in userList!)
                {
                    var userPK = user["userPK"]!.ToString();
                    var username = user["username"]!.ToString();
                    dictionary[userPK] = username;
                }

                return dictionary;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        static void sleepRandom(int minTime, int maxTime)
        {
            Random rand = new Random();
            Thread.Sleep(rand.Next(minTime, maxTime));
        }

        static string dictionaryToJsonString(Dictionary<string, string> list)
        {
            var jsonArray = list.Select(kv => new { userPK = kv.Key, username = kv.Value }).ToArray();
            return JsonConvert.SerializeObject(jsonArray);
        }

        static void Main(string[] args)
        {
            var usersFollowing = new Dictionary<string, string>();
            var usersFollowers = new Dictionary<string, string>();

            var usersFollowingFile = new Dictionary<string, string>();
            var usersFollowersFile = new Dictionary<string, string>();
            var resultLines = new List<string>();

            const int minTime = 1000;
            const int maxTime = 2000;
            string username;
            string option;
            string cookie;
            string? userPK;

            int userFollowersCount;
            int userFollowingCount;

            displayStartingScreen();
            username = getUsername();
            option = getOption();

            string followingFileName = $"{username}/{username}_followings.json";
            string followersFileName = $"{username}/{username}_followers.json";
            string resultFileName = $"{username}/result.txt";

            if (option == "1")
            {
                downloadProfileImage(username);
            }

            else if (option == "2")
            {
                Console.WriteLine("\nThis only works on public instagram accounts or on private accounts that you are following\n\n");
                cookie = getCookie();
                sleepRandom(minTime, maxTime);
                userPK = getUserPK(cookie, username);
                sleepRandom(minTime, maxTime);

                if (userPK == null)
                {
                    Console.Error.WriteLine("Something went wrong");
                    return;
                }

                userFollowersCount = getFollowerCount(username);
                sleepRandom(minTime, maxTime);
                userFollowingCount = getFollowingCount(username);

                if (userFollowersCount < 1 || userFollowingCount < 1)
                {
                    Console.Error.WriteLine("Something went wrong");
                    return;
                }

                if (File.Exists(followingFileName) && File.Exists(followersFileName))
                {
                    usersFollowingFile = getDataFromFile(followingFileName);
                    sleepRandom(minTime, maxTime);
                    usersFollowersFile = getDataFromFile(followersFileName);
                }

                Console.WriteLine($"{username}: {userPK}\n");
                Console.WriteLine($"Previous follower count: {usersFollowersFile!.Count}, previous following count: {usersFollowingFile!.Count}");
                Console.WriteLine($"Current follower count:  {userFollowersCount}, current following count:  {userFollowingCount}\n");

                Console.WriteLine("Getting Following...");
                usersFollowing = getFollowingList(userPK, cookie, minTime, maxTime);
                if (usersFollowing == null)
                {
                    Console.Error.WriteLine("Something went wrong");
                    return;
                }

                Console.WriteLine("Getting Followers...");
                usersFollowers = getFollowersList(userPK, cookie, minTime, maxTime);
                if (usersFollowers == null)
                {
                    Console.Error.WriteLine("Something went wrong");
                    return;
                }

                client.Dispose();
                Directory.CreateDirectory(username);

                File.WriteAllText(followersFileName, dictionaryToJsonString(usersFollowers));
                File.WriteAllText(followingFileName, dictionaryToJsonString(usersFollowing));

                Console.WriteLine("\n\nVerifying...\n");


                resultLines.Add($"{DateTime.Now}: Current Follower count: {usersFollowers.Count}, Current Following count: {usersFollowing.Count}");
                Console.WriteLine($"\nCurrent Follower Count: {usersFollowers.Count}, Current Following Count: {usersFollowing.Count}");
                resultLines.Add($"{DateTime.Now}: {username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)}");
                Console.WriteLine($"\n{username} {(usersFollowing.Count < usersFollowingFile.Count ? "stopped" : "started")} following {(usersFollowing.Count < usersFollowingFile.Count ? usersFollowingFile.Count - usersFollowing.Count : usersFollowing.Count - usersFollowingFile.Count)}");

                foreach (var user in usersFollowingFile)
                {
                    if (!usersFollowing.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{username} stopped following {user.Value}");
                        Console.WriteLine($"{username} stopped following {user.Value}");
                    }
                }

                foreach (var user in usersFollowing)
                {
                    if (!usersFollowingFile.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{username} started following {user.Value}");
                        Console.WriteLine($"{username} started following {user.Value}");
                    }
                }

                resultLines.Add($"{DateTime.Now}: {(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");
                Console.WriteLine($"\n{(usersFollowers.Count < usersFollowersFile.Count ? usersFollowersFile.Count - usersFollowers.Count : usersFollowers.Count - usersFollowersFile.Count)} users {(usersFollowers.Count < usersFollowersFile.Count ? "stopped" : "started")} following {username}");

                foreach (var user in usersFollowersFile)
                {
                    if (!usersFollowers.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{user.Value} stopped following {username}");
                        Console.WriteLine($"{user.Value} stopped following {username}");
                    }
                }

                foreach (var user in usersFollowers)
                {
                    if (!usersFollowersFile.ContainsKey(user.Key))
                    {
                        resultLines.Add($"{user.Value} started following {username}");
                        Console.WriteLine($"{user.Value} started following {username}");
                    }
                }

                resultLines.Add($"{DateTime.Now}: Name changes");
                Console.WriteLine("\nName changes");

                foreach (var user in usersFollowersFile)
                {
                    if (usersFollowersFile.ContainsKey(user.Key) && usersFollowers[user.Key] != usersFollowersFile[user.Key])
                    {
                        resultLines.Add($"{user.Value} changed their username to {usersFollowers[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowers[user.Key]}");
                    }
                }

                foreach (var user in usersFollowingFile)
                {
                    if (usersFollowingFile.ContainsKey(user.Key) && usersFollowing[user.Key] != usersFollowingFile[user.Key])
                    {
                        resultLines.Add($"{user.Value} changed their username to {usersFollowing[user.Key]}");
                        Console.WriteLine($"{user.Value} changed their username to {usersFollowing[user.Key]}");
                    }
                }

                File.AppendAllLines(resultFileName, resultLines);
                Console.WriteLine($"\nFinished successfully, results saved in ./{username}/results.txt");
            }
            else
            {
                return;
            }
        }

        static void downloadProfileImage(string username)
        {
            var request1 = new RestRequest("/api/v1/users/web_profile_info/", Method.Get);
            request1.AddQueryParameter("username", username);
            request1.AddHeader("user-agent", "Instagram 76.0.0.15.395 Android (24/7.0; 640dpi; 1440x2560; samsung; SM-G930F; herolte; samsungexynos8890; en_US; 138226743)");
            var response1 = client.Execute(request1);
            if (!response1.IsSuccessful)
            {
                Console.WriteLine($"Error in request1: {response1.StatusCode}");
                return;
            }
            try
            {
                dynamic obj1 = JsonConvert.DeserializeObject(response1.Content!)!;
                string imageUrl = obj1.data.user.profile_pic_url_hd.ToString();
                byte[] fileBytes = client.DownloadData(new RestRequest(imageUrl, Method.Get))!;

                Directory.CreateDirectory(username);
                string? filePath = GenerateNewFileName($"{username}/{username}_profileImage.jpg", fileBytes);
                if (filePath != null)
                {
                    Console.WriteLine($"\nThe profile picture was successfully saved in ./{filePath}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return;
            }
        }

        static string? GenerateNewFileName(string fullFilePath, byte[] newFileContent)
        {
            int counter = 1;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);
            string fileExtension = Path.GetExtension(fullFilePath);
            string directoryPath = Path.GetDirectoryName(fullFilePath)!;

            fullFilePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}({counter}){fileExtension}");

            while (File.Exists(fullFilePath))
            {
                byte[] existingFileContent = File.ReadAllBytes(fullFilePath);

                if (existingFileContent.SequenceEqual(newFileContent))
                {
                    Console.WriteLine("\nThe profile picture is unchanged. No new file created.");
                    return null;
                }

                fullFilePath = Path.Combine(directoryPath, $"{fileNameWithoutExtension}({++counter}){fileExtension}");
            }

            File.WriteAllBytes(fullFilePath, newFileContent);
            return fullFilePath;
        }
    }
}