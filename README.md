# Stalkiana

A simple command-line tool for interacting with Instagram using the Instagram API. This tool allows you to perform various actions on Instagram, from the command line.

**Note**: Please be aware that using this tool is against Instagram's terms of service. Use it responsibly and at your own risk.

## Features

- Get a list of followings and followers from a user;
- Save and log if the user's following/follower count has changed;

## Prerequisites

- .NET Core SDK installed on your machine;
- Instagram credentials (cookie, app_id);

## Installation

1. Clone this repository to your local machine:

   ```shell
   git clone https://github.com/yourusername/instagram-console-tool.git
   ```

2. Navigate to the project directory:

    ```shell
    cd stalkiana
    ```

3. Run the tool:

    ```shell
    dotnet run stalkiana.cs
    ```

## Information and Usage

This tool requires the instagram cookie and x-ig-app-id in order to work.
If you don't know how to get the cookie and the x-ig-app-id, here are the steps to do it:


1. Launch your browser and log in to Instagram.


2. Go to inspect element (CTRL + SHIFT + C or F12).


3. Go to the network tab and type "graphql" on the filter at the top and click on any of the requests.


4. Go to request headers and search for the cookie, triple click the value (mid=...) and copy it.


5. To get the x-ig-app-id scroll down until you find it (its a number with approximately 15 digits).