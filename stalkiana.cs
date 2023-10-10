/*

Do not use the tool multiple times per day (max 10 times) or you might be flagged by instagram

*/

using RestSharp;
using Newtonsoft.Json;

namespace Stalkiana_Console
{
    internal class Program
    {
        static void Main(string[] args)
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
            Console.Write("\nThis tool is used to stalk the follower count of a user\nIt works by keeping track of the list of followers/following accounts and then comparing with the last save.\n");
            Console.WriteLine("\nThis tool only works on public instagram accounts or in private accounts that you are following");
            int minTime = 4000;
            int maxTime = 6000;
            var rand = new Random();
            Console.Write("\nPlease input the username to stalk: ");
            string username = Console.ReadLine()!;
            Console.Write("\nPlease input the full instagram cookie: ");
            string cookie = Console.ReadLine()!;
            Console.Write("\nPlease input the x-ig-app-id: ");
            string app_id = Console.ReadLine()!;
            string followingsFileName = $"{username}_followings.txt";
            string followersFileName = $"{username}_followers.txt";
            string resultFileName = "result.txt";
            var client = new RestClient("https://www.instagram.com");
            string userId;

            int pos;
            int increment = 100;

            int followerCount;
            int followingCount;

            var usersFollowing = new List<string>();
            var usersFollowers = new List<string>();
            var usersFollowingFile = new List<string>();
            var usersFollowersFile = new List<string>();
            var resultLines = new List<string>();


            //search the user and get the ID
            var request1 = new RestRequest("api/v1/web/search/topsearch/", Method.Get);
            request1.AddHeader("cookie", cookie);
            request1.AddQueryParameter("query", username);
            request1.AddQueryParameter("context", "blended");
            request1.AddQueryParameter("include_reel", "false");
            request1.AddQueryParameter("search_surface", "web_top_search");
            var response1 = client.Execute(request1);
            if(response1.IsSuccessful){
                Console.WriteLine("First request completed succesfully");
            }else{ Console.WriteLine("Error in request1 (maybe cookie or app id is not correct)"); return; }

            Thread.Sleep(rand.Next(minTime, maxTime));

            //get the follower and following count of the user
            var request2 = new RestRequest("/api/v1/users/web_profile_info/", Method.Get);
            request2.AddQueryParameter("username", username);
            request2.AddHeader("cookie", cookie);
            request2.AddHeader("x-ig-app-id", app_id);
            var response2 = client.Execute(request2);
            Thread.Sleep(rand.Next(minTime, maxTime));

            if(response2.IsSuccessful){
                Console.WriteLine("Second request completed succesfully\n");
            }else{ Console.WriteLine("Error in request2"); return; }

            if (File.Exists(followingsFileName) && File.Exists(followersFileName))
            {
                usersFollowingFile = File.ReadAllLines(followingsFileName).ToList();
                usersFollowersFile = File.ReadAllLines(followersFileName).ToList();
            }

            try
            {
                dynamic? obj1 = JsonConvert.DeserializeObject(response1.Content!)!;
                dynamic? obj2 = JsonConvert.DeserializeObject(response2.Content!)!;
                userId = obj1.users[0].user.pk!;
                followerCount = obj2.data.user.edge_followed_by.count;
                followingCount = obj2.data.user.edge_follow.count;
                Console.WriteLine($"{username}: {userId}\n");
                Console.WriteLine($"Previous follower count: {usersFollowersFile.Count}, previous following count: {usersFollowingFile.Count}");
                Console.WriteLine($"Current follower count:  {followerCount}, current following count:  {followingCount}\n");
                Thread.Sleep(rand.Next(minTime, maxTime));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                return;
            }

            Console.WriteLine("Getting Following...");

            //get the list of all the following
            for (pos = 0; pos < followingCount; pos += increment)
            {
                var request3 = new RestRequest($"/api/v1/friendships/{userId}/following/");
                request3.AddQueryParameter("count", increment);
                request3.AddQueryParameter("max_id", pos);
                request3.AddHeader("cookie", cookie);
                request3.AddHeader("x-ig-app-id", app_id);
                var response3 = client.Execute(request3);
                if (!response3.IsSuccessStatusCode) { Console.WriteLine("Error in request3 following"); return; }
                dynamic? obj3 = JsonConvert.DeserializeObject(response3.Content!)!;
                int userCount = obj3.users.Count;
                for (int i = 0; i < userCount; i++)
                {
                    usersFollowing.Add((string)obj3.users[i].username);
                }
                Thread.Sleep(rand.Next(minTime, maxTime));
            }

            Console.WriteLine("Getting Followers...");

            //get list of all the followers
            for (pos = 0; pos < followerCount; pos += increment)
            {
                var request3 = new RestRequest($"/api/v1/friendships/{userId}/followers/");
                request3.AddQueryParameter("count", increment);
                request3.AddQueryParameter("max_id", pos);
                request3.AddQueryParameter("search_surface", "follow_list_page");
                request3.AddHeader("cookie", cookie);
                request3.AddHeader("x-ig-app-id", app_id);
                var response3 = client.Execute(request3);
                if (!response3.IsSuccessStatusCode) { Console.WriteLine("Error in request3 followers"); return; }
                dynamic? obj3 = JsonConvert.DeserializeObject(response3.Content!)!;
                int userCount = obj3.users.Count;
                for (int i = 0; i < userCount; i++)
                {
                    usersFollowers.Add((string)obj3.users[i].username);
                }
                Thread.Sleep(rand.Next(minTime, maxTime));
            }

            Console.WriteLine();
            client.Dispose();

            //save the lists to the file
            File.WriteAllLines(followingsFileName, usersFollowing);
            File.WriteAllLines(followersFileName, usersFollowers);

            Console.WriteLine("Following:\n");
            for (int i = 0; i < usersFollowing.Count; i++)
            {
                Console.WriteLine($"{usersFollowing[i]}");
            }
            Console.WriteLine("\n\nFollowers:\n");
            for (int i = 0; i < usersFollowers.Count; i++)
            {
                Console.WriteLine($"{usersFollowers[i]}");
            }


            Console.WriteLine("\n\nVerifying...\n");


            if (usersFollowingFile.Count == usersFollowing.Count)
            {
                Console.WriteLine($"{username} has the same followings\n");
                resultLines.Add($"{DateTime.Now}: {username} has the same followings");
            }
            else if (usersFollowingFile.Count < usersFollowing.Count)
            {
                Console.WriteLine($"{username} started following {usersFollowing.Count - usersFollowingFile.Count} users\n");
                resultLines.Add($"{DateTime.Now}: {username} started following {usersFollowing.Count - usersFollowingFile.Count} users");
                foreach (string user in usersFollowing)
                {
                    if (!usersFollowingFile.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}, ");
                    }
                }
            }
            else
            {
                Console.WriteLine($"{username} stopped following {usersFollowingFile.Count - usersFollowing.Count} users\n");
                resultLines.Add($"{DateTime.Now}: {username} stopped following {usersFollowingFile.Count - usersFollowing.Count} users");
                foreach (string user in usersFollowingFile)
                {
                    if (!usersFollowing.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}, ");
                    }
                }
            }

            if (usersFollowersFile.Count == usersFollowers.Count)
            {
                Console.WriteLine($"{username} has the same followers\n");
                resultLines.Add($"{DateTime.Now}: {username} has the same followers");
            }
            else if (usersFollowersFile.Count < usersFollowers.Count)
            {
                Console.WriteLine($"{usersFollowers.Count - usersFollowersFile.Count} users started following {username}\n");
                resultLines.Add($"{DateTime.Now}: {usersFollowers.Count - usersFollowersFile.Count} users started following {username}");
                foreach (string user in usersFollowers)
                {
                    if (!usersFollowersFile.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}, ");
                    }
                }
            }
            else
            {
                Console.WriteLine($"{usersFollowersFile.Count - usersFollowers.Count} users stopped following {username}\n");
                resultLines.Add($"{DateTime.Now}: {usersFollowersFile.Count - usersFollowers.Count} users stopped following {username}");
                foreach (string user in usersFollowersFile)
                {
                    if (!usersFollowers.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}, ");
                    }
                }
            }
            File.AppendAllLines(resultFileName, resultLines);
            Console.WriteLine("\nFinished successfully, results saved in ./results.txt");
        }
    }
}