/*

Do not use the tool multiple times per day or you might get flagged by instagram

*/

using System;
using RestSharp;
using Newtonsoft.Json;

namespace Stalkiana_Console
{
    internal class Program
    {
        public static RestClient client = new RestClient("https://www.instagram.com");
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
            Console.Write("\nThis is a tool used for stalking an instagram user\n");
            int minTime = 1000;
            int maxTime = 2000;
            var rand = new Random();
            Console.Write("\nPlease input the username to stalk: ");
            string username = Console.ReadLine()!;
            string input;
            do{
                Console.WriteLine("\n1- Download Profile Picture");
                Console.WriteLine("2- Get Followers/Following\n");
                Console.Write("\nChoose what you want to do: ");
                input = Console.ReadLine()!;
            }while(input != "1" && input != "2");
            if(input == "1"){
                downloadProfileImage(username);
                return;
            }
            Console.WriteLine("\nThis only works on public instagram accounts or in private accounts that you are following\n\n");
            Console.Write("Please input the full instagram cookie: ");
            string cookie = Console.ReadLine()!;
            string followingsFileName = $"{username}/{username}_followings.txt";
            string followersFileName = $"{username}/{username}_followers.txt";
            string resultFileName = $"{username}/result.txt";
            string userId;

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
                Console.WriteLine("\nFirst request completed succesfully");
            }else{ Console.WriteLine($"\nError in request1 (maybe cookie is not correct): {response1.StatusCode}"); return; }

            Thread.Sleep(rand.Next(minTime, maxTime));

            //get the follower and following count of the user
            var request2 = new RestRequest("/api/v1/users/web_profile_info/", Method.Get);
            request2.AddQueryParameter("username", username);
            request2.AddHeader("user-agent", "Instagram 76.0.0.15.395 Android (24/7.0; 640dpi; 1440x2560; samsung; SM-G930F; herolte; samsungexynos8890; en_US; 138226743)");
            var response2 = client.Execute(request2);
            Thread.Sleep(rand.Next(minTime, maxTime));

            if(response2.IsSuccessful){
                Console.WriteLine("Second request completed succesfully\n");
            }else{ Console.WriteLine($"Error in request2: {response2.StatusCode}"); return; }

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
                Thread.Sleep(rand.Next(minTime, maxTime)); //This lines makes it harder for instagram to flag the bot requests
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                return;
            }

            Console.WriteLine("Getting Following...");

            //get the list of all the following
            bool has_next = true;
            string? after = null!;
            while(has_next){
                var request3 = new RestRequest("/graphql/query/", Method.Get);
                request3.AddQueryParameter("query_hash", "d04b0a864b4b54837c0d870b0e77e076");
                request3.AddQueryParameter("id", userId);
                request3.AddQueryParameter("include_reel", "true");
                request3.AddQueryParameter("fetch_mutual", "true");
                request3.AddQueryParameter("first", "50");
                request3.AddQueryParameter("after", after);
                request3.AddHeader("cookie", cookie);
                var response3 = client.Execute(request3);
                if (response3.IsSuccessful)
                {
                    dynamic? obj3 = JsonConvert.DeserializeObject(response3.Content!);
                    has_next = obj3!.data.user.edge_follow.page_info.has_next_page;
                    after = obj3.data.user.edge_follow.page_info.end_cursor;
                    foreach (dynamic following in obj3.data.user.edge_follow.edges)
                    {
                        usersFollowing.Add((string)following.node.username);
                    }
                }
                else
                {
                    Console.WriteLine($"Error in fetching followings: {response3.StatusCode}");
                    return;
                }
                Thread.Sleep(rand.Next(minTime, maxTime)); //This lines makes it harder for instagram to flag the bot requests
            }

            Console.WriteLine("Getting Followers...");

            //get list of all the followers
            has_next = true;
            after = null!;
            while(has_next){
                var request3 = new RestRequest("/graphql/query/", Method.Get);
                request3.AddQueryParameter("query_hash", "c76146de99bb02f6415203be841dd25a");
                request3.AddQueryParameter("id", userId);
                request3.AddQueryParameter("include_reel", "true");
                request3.AddQueryParameter("fetch_mutual", "true");
                request3.AddQueryParameter("first", "50");
                request3.AddQueryParameter("after", after);
                request3.AddHeader("cookie", cookie);
                var response3 = client.Execute(request3);
                if (response3.IsSuccessful)
                {
                    dynamic? obj3 = JsonConvert.DeserializeObject(response3.Content!);
                    has_next = obj3!.data.user.edge_followed_by.page_info.has_next_page;
                    after = obj3.data.user.edge_followed_by.page_info.end_cursor;
                    foreach (dynamic follower in obj3.data.user.edge_followed_by.edges)
                    {
                        usersFollowers.Add((string)follower.node.username);
                    }
                }
                else
                {
                    Console.WriteLine($"Error in fetching followers: {response3.StatusCode}");
                    return;
                }
                Thread.Sleep(rand.Next(minTime, maxTime)); //This lines makes it harder for instagram to flag the bot requests
            }

            Console.WriteLine();
            client.Dispose();   

            Directory.CreateDirectory(username);
            
            //save the lists to the file
            File.WriteAllLines(followingsFileName, usersFollowing);
            File.WriteAllLines(followersFileName, usersFollowers);

            Console.WriteLine("\n\nVerifying...\n");


            if (usersFollowingFile.Count == usersFollowing.Count)
            {
                Console.WriteLine($"{username} has the same number of followings");
                resultLines.Add($"{DateTime.Now}: {username} has the same number of followings");

                var notList1 = usersFollowing.Except(usersFollowingFile);
                var notList2 = usersFollowingFile.Except(usersFollowing);

                if(notList1.Count() != notList2.Count()){
                    Console.WriteLine("Something went wrong.");
                    return;
                }

                for(int i = 0; i < notList1.Count(); i++){
                    Console.WriteLine($"{username} stopped following {notList2.ElementAt(i)} and started following {notList1.ElementAt(i)},");
                    resultLines.Add($"{username} stopped following {notList2.ElementAt(i)} and started following {notList1.ElementAt(i)},");
                }
            }
            else if (usersFollowingFile.Count < usersFollowing.Count)
            {
                Console.WriteLine($"{username} started following {usersFollowing.Count - usersFollowingFile.Count} users");
                resultLines.Add($"{DateTime.Now}: {username} started following {usersFollowing.Count - usersFollowingFile.Count} users");
                foreach (string user in usersFollowing)
                {
                    if (!usersFollowingFile.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}");
                    }
                }
            }
            else if(usersFollowingFile.Count > usersFollowing.Count)
            {
                Console.WriteLine($"{username} stopped following {usersFollowingFile.Count - usersFollowing.Count} users");
                resultLines.Add($"{DateTime.Now}: {username} stopped following {usersFollowingFile.Count - usersFollowing.Count} users");
                foreach (string user in usersFollowingFile)
                {
                    if (!usersFollowing.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}");
                    }
                }
            }else{
                Console.WriteLine("Something went wrong.");
                return;
            }

            if (usersFollowersFile.Count == usersFollowers.Count)
            {
                Console.WriteLine($"\n{username} has the same number of followers");
                resultLines.Add($"{DateTime.Now}: {username} has the same number of followers");

                var notList1 = usersFollowers.Except(usersFollowersFile);
                var notList2 = usersFollowersFile.Except(usersFollowers);

                if(notList1.Count() != notList2.Count()){
                    Console.WriteLine("Something went wrong.");
                    return;
                }

                for(int i = 0; i < notList1.Count(); i++){
                    Console.WriteLine($"{notList2.ElementAt(i)} stopped following {username} and {notList1.ElementAt(i)} started following {username},");
                    resultLines.Add($"{notList2.ElementAt(i)} stopped following {username} and {notList1.ElementAt(i)} started following {username},");
                }
            }
            else if (usersFollowersFile.Count < usersFollowers.Count)
            {
                Console.WriteLine($"\n{usersFollowers.Count - usersFollowersFile.Count} users started following {username}");
                resultLines.Add($"{DateTime.Now}: {usersFollowers.Count - usersFollowersFile.Count} users started following {username}");
                foreach (string user in usersFollowers)
                {
                    if (!usersFollowersFile.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}");
                    }
                }
            }
            else if(usersFollowersFile.Count > usersFollowers.Count)
            {
                Console.WriteLine($"\n{usersFollowersFile.Count - usersFollowers.Count} users stopped following {username}");
                resultLines.Add($"{DateTime.Now}: {usersFollowersFile.Count - usersFollowers.Count} users stopped following {username}");
                foreach (string user in usersFollowersFile)
                {
                    if (!usersFollowers.Contains(user))
                    {
                        Console.WriteLine(user);
                        resultLines.Add($"{user}");
                    }
                }
            }else{
                Console.WriteLine("Something went wrong.");
                return;
            }
            File.AppendAllLines(resultFileName, resultLines);
            Console.WriteLine($"\nFinished successfully, results saved in ./{username}/results.txt");
        }
        static void downloadProfileImage(string username){
            var request1 = new RestRequest("/api/v1/users/web_profile_info/", Method.Get);
            request1.AddQueryParameter("username", username);
            request1.AddHeader("user-agent", "Instagram 76.0.0.15.395 Android (24/7.0; 640dpi; 1440x2560; samsung; SM-G930F; herolte; samsungexynos8890; en_US; 138226743)");
            var response1 = client.Execute(request1);

            if(response1.IsSuccessful){
                Console.WriteLine("\nFirst request completed succesfully\n");
            }else{ Console.WriteLine($"Error in request1: {response1.StatusCode}"); return; }

            dynamic? obj1 = JsonConvert.DeserializeObject(response1.Content!)!;
            RestClient restClient = new RestClient();
            var request2 = new RestRequest(obj1.data.user.profile_pic_url_hd.ToString(), Method.Get);
            var fileBytes = restClient.DownloadData(request2);
            Directory.CreateDirectory(username);
            File.WriteAllBytes($"{username}/{username}_profileImage.jpg", fileBytes!);
            if(fileBytes != null && File.Exists($"{username}/{username}_profileImage.jpg")){
                Console.WriteLine($"The profile picture was succesfully saved in ./{username}/{username}_profileImage.jpg");
            }else{
                Console.WriteLine("There was an error getting the profile picture");
            }
            return;
        }
    }
}