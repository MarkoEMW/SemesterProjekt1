﻿using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using System;
using System.IO;

using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


// 20 hours already Wasted from HTTPLISTENER -> TCP ANDWiwkndiunwaidon Day 3 Note of this shit 
// 25 and nearly finished 



namespace SemesterProjekt1
{

    enum HttpStatusCode
    {
        OK = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Conflict = 409,
        InternalServerError = 500
    }



    public class UserServiceRequest
    {
        public UserServiceHandler _userServiceHandler = new UserServiceHandler();
        private HTMLGEN _htmlgen = new HTMLGEN(new UserServiceHandler());

        public async Task HandleRequestAsync(StreamReader request, StreamWriter response, string method, string path)
        {
            try
            {


                switch (method)
                {
                    case "GET":
                        await HandleGetRequestAsync(request, response, path);
                        break;
                    case "POST":
                        await HandlePostRequestAsync(request, response, path);
                        break;
                    default:
                        response.WriteLine("HTTP/1.1 405 Method Not Allowed");
                        response.WriteLine("Content-Length: 0");
                        response.WriteLine();
                        response.Flush();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
        }


        private async Task HandleGetRequestAsync(StreamReader request, StreamWriter response, string path1)
        {
            switch (path1)
            {
                case "/":
                    var users = _userServiceHandler.GetAllUsers();
                    string htmlResponse = _htmlgen.GenerateOptionsPage(users.Count);
                    SendResponse(response, htmlResponse, "text/html");
                    break;
                case "/users":
                    var allUsers = _userServiceHandler.GetAllUsers();
                    string jsonResponse = SerializeToJson(allUsers);
                    SendResponse(response, jsonResponse, "application/json");
                    break;
                case string path when path1.StartsWith("/user/"):
                    await HandleGetUserByIdAsync(request, response, path1);
                    break;
                case "/login":
                           _htmlgen.SendLoginPage(response);
                    break;
                case "/lobby":
                   // _htmlgen.SendLobbyPage(request, response);
                    break;
                case "/logout":
                    await HandleLogout(response);
                    break;
                default:
                    response.WriteLine("HTTP/1.1 404 Not Found");
                    response.WriteLine("Content-Length: 0");
                    response.WriteLine();
                    break;
            }
        }

        private async Task HandlePostRequestAsync(StreamReader request, StreamWriter response, string path1)
        {
            switch (path1)
            {
                case "/users":
                    await HandleAddUserAsync(request, response);
                    break;
                case "/login":
                    await HandleLoginAsync(request, response);
                    break;
                case "/sessions":
                    await HandleLoginAsyncCURL(request, response);
                    break;
                case "/openpack":
                  await HandleOpenCardPackAsync(request, response);
                    break;
                case "/inventory":
                          await HandleBuyPacksAsync(request, response);
                    break;
                case "/add-card-to-deck":
                          await HandleAddCardToDeckAsync(request, response);
                    break;
                default:
                    response.WriteLine("HTTP/1.1 404 Not Found");
                    response.WriteLine("Content-Length: 0");
                    response.WriteLine();
                    response.Flush();
                    break;
            }
        }

        private async Task HandleGetUserByIdAsync(StreamReader request, StreamWriter response, string path)
        {
            Console.WriteLine(path);
            string userIdString = path.Substring(6);

            if (int.TryParse(userIdString, out int userId))
            {
                var user = _userServiceHandler.GetUserById(userId);
                if (user != null)
                {
                    string jsonResponse = SerializeToJson(user);
                    SendResponse(response, jsonResponse, "application/json");
                }
                else
                {
                    SendErrorResponse(response, HttpStatusCode.BadRequest);
                }
            }
            else
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest);
            }
        }

        private async Task HandleAddUserAsync(StreamReader request, StreamWriter response)
        {

            string requestBody = await ReadRequestBodyAsync(request, response);


            var user = DeserializeUser(requestBody);
            if (user != null && (IsValidInput(user.Username) && IsValidInput(user.Password)))
            {
                var existingUser = _userServiceHandler.GetUserByName(user.Username);
                if (existingUser == null)
                {
                    _userServiceHandler.AddUser(user);
                    SendResponse(response, "User created successfully", "application/text", HttpStatusCode.Created);
                }
                else
                {
                    SendErrorResponse(response, HttpStatusCode.Conflict, "User already exists");
                }
            }
            else
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "Invalid user data");
            }
        }

        private async Task HandleLoginAsync(StreamReader request, StreamWriter response)
        {
            var user = await IsIdentiyYesUser(request, response);
            if (user != null)
            {
                string token = $"{user.Username}-mtcgToken";
                var inventory = user.Inventory;
                string responseContent = $"Login successful. Token: {token}";

                if (inventory != null)
                {
                    string inventoryHtml = _htmlgen.GenerateInventoryHtml(inventory);
                    responseContent += "\n" + inventoryHtml; // Combine token and inventory HTML in one response
                }

                response.WriteLine("HTTP/1.1 200 OK");
                response.WriteLine($"Set-Cookie: authToken={token}; Path=/;");
                response.WriteLine($"Set-Cookie: userData=username={user.Username}&password={user.Password}&userid={user.Id};");
                response.WriteLine("Content-Type: text/html");
                response.WriteLine($"Content-Length: {responseContent.Length}");
                response.WriteLine();
                response.WriteLine(responseContent);
                response.Flush();
            }

       
            else
            {
                SendErrorResponse(response, HttpStatusCode.Unauthorized, "Invalid username or password");
            }
        }
        

        private void SendErrorResponse(StreamWriter writer, HttpStatusCode statusCode, string message)
        {
            writer.WriteLine($"HTTP/1.1 {(int)statusCode} {statusCode}");
            writer.WriteLine("Content-Type: text/plain");
            writer.WriteLine("Content-Length: " + message.Length);
            writer.WriteLine();
            writer.Write(message);
            writer.Flush();
        }



        private async Task HandleLoginAsyncCURL(StreamReader reader, StreamWriter writer)
        {
            if (!writer.BaseStream.CanWrite)
            { Console.WriteLine("Error"); }



            //////////////////////////////////////////////
            try
            {



                var authenticatedUser = await IsIdentiyYesUser(reader, writer);
                if (authenticatedUser != null)
                {
                    string token = $"{authenticatedUser.Username}-mtcgToken";
                    string response = $"HTTP/1.1 200 OK\r\nSet-Cookie: authToken={token}\r\nContent-Type: text/html\r\nContent-Length: {token.Length}\r\n\r\n{token}";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    Console.WriteLine("Yes");
                    await writer.BaseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    await writer.BaseStream.FlushAsync();
                    Console.WriteLine("Yes");
                }
                else
                {
                    SendErrorResponse(writer, HttpStatusCode.BadRequest);
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"I/O error during login: {ioEx.Message}");
                SendErrorResponse(writer, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                SendErrorResponse(writer, HttpStatusCode.InternalServerError);
            }
        }


        private async Task HandleLogout(StreamWriter writer)
        {
            try
            {
                // Set expired cookie headers to clear them
                writer.WriteLine("HTTP/1.1 200 OK");
                writer.WriteLine("Set-Cookie: userData=; Expires=Thu, 01 Jan 1970 00:00:00 GMT");
                writer.WriteLine("Content-Type: text/plain");
                writer.WriteLine();
                writer.WriteLine("Logged out successfully.");
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                writer.WriteLine("HTTP/1.1 500 Internal Server Error");
                writer.WriteLine("Content-Length: 0");
                writer.WriteLine();
                writer.Flush();
            }
        }

        
        private async Task HandleOpenCardPackAsync(StreamReader reader, StreamWriter writer)
        {


            var user = await IsIdentiyYesUserCookie(reader, writer);

                if (user != null)
                {
                    if (user.Inventory.CardPacks.Count > 0)
                    {
                        for (int i = user.Inventory.CardPacks.Count - 1; i >= 0; i--)
                        {
                            user.Inventory.OpenCardPack(user.Inventory.CardPacks[i]);
                            user.Inventory.CardPacks.RemoveAt(i);
                        }
                    }

                    string jsonResponse = SerializeToJson(user.Inventory.OwnedCards);
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine($"Content-Length: {jsonResponse.Length}");
                    writer.WriteLine();
                    writer.WriteLine(jsonResponse);
                    writer.Flush();

            }
                else
                {
                SendErrorResponse(writer, HttpStatusCode.Unauthorized, "Invalid at HandleOpenCard");
            }
            }





        private async Task HandleBuyPacksAsync(StreamReader reader, StreamWriter writer)
        {
            var user = await IsIdentiyYesUserCookie(reader, writer);
            string requestBodyString = await ReadRequestBodyAsync(reader, writer);

            if (user != null)
            {
                string[] parameters = requestBodyString.Split('&');
                int amount = 0;

                foreach (var param in parameters)
                {
                    string[] keyValue = param.Split('=');
                    if (keyValue[0] == "Amount" && int.TryParse(keyValue[1], out amount))
                    {
                        break;
                    }
                }

                if (amount > 0)
                {
                    user.Inventory.AddCardPack(new CardPack(user.Id), amount);
                    _userServiceHandler.UpdateUser(user.Id, user);
                    string jsonResponse = SerializeToJson(new { message = "Packs bought successfully" });
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine("Content-Type: application/json");
                    writer.WriteLine($"Content-Length: {jsonResponse.Length}");
                    writer.WriteLine();
                    writer.WriteLine(jsonResponse);
                    writer.Flush();
                }
                else
                {
                    SendErrorResponse(writer, HttpStatusCode.BadRequest, "Invalid amount.");
                }
            }
            else
            {
                SendErrorResponse(writer, HttpStatusCode.Unauthorized, "Invalid username or password.");
            }
        }

     

async Task HandleAddCardToDeckAsync(StreamReader reader, StreamWriter writer)
        {
            try
            {
                var user = await IsIdentiyYesUserCookie(reader, writer);
                string requestBodyString = await ReadRequestBodyAsync(reader, writer);

                if (user != null)
                {

              
               var  parameters = requestBodyString.Split('&');
                                    List<int> cardIndices = new List<int>();
                                  

                                    foreach (var param in parameters)
                                    {
                                        string[] keyValue = param.Split('=');
                                        if (keyValue[0] == "cardIndices" && int.TryParse(keyValue[1], out int index))
                                        {
                                            cardIndices.Add(index);
                                        }
                                    }

                    if (cardIndices != null)
                    {
                        int[] cardPositions = cardIndices.Select(index => index).ToArray();
                        _userServiceHandler.AddCardToDeck(user.Id, user.Username, user.Password, cardPositions);
                        SendResponse(writer, "Cards added to deck successfully.", "text/html");
                    }
                    else
                    {
                        SendErrorResponse(writer, HttpStatusCode.BadRequest, "Invalid card positions.");
                    }
                }
                else
                {
                    SendErrorResponse(writer, HttpStatusCode.BadRequest, "Invalid user credentials.");
                }
            }
            catch (Exception ex)
            {
                SendErrorResponse(writer, HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        private void SendResponse(StreamWriter writer, string content, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            writer.WriteLine($"HTTP/1.1 {(int)statusCode} {statusCode}");
            writer.WriteLine("Content-Type: " + contentType);
            writer.WriteLine("Content-Length: " + content.Length);
            writer.WriteLine();
            writer.Write(content);
            writer.Flush();

        }


        private void SendResponseWeb(StreamWriter writer, string content, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
        {

            writer.Write(content);
            writer.Flush(); // Ensure the content is flushed to the stream
        }






        private User DeserializeUser(string json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<User>(json, options);
        }

        private string SerializeToJson(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }


        private bool IsValidInput(string input)
        {
            // Überprüfen Sie auf schädliche Zeichen oder Muster
            string[] blackList = { "'", "\"", "--", ";", "/*", "*/", "xp_" };
            foreach (var item in blackList)
            {
                if (input.Contains(item))
                {
                    return false;
                }
            }

            // Überprüfen Sie die Länge der Eingabe
            if (input.Length > 20) // Beispielgrenze, anpassen nach Bedarf
            {
                return false;
            }

            return true;
        }
        private bool IsJson(string input)
        {

            input = input.Trim();
            Console.WriteLine(input);
            return input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]");
        }

        private async Task<User?> IsIdentiyYesUser(StreamReader reader, StreamWriter writer)
        {
            if (!writer.BaseStream.CanWrite)
            {
                Console.WriteLine("Error");
                return null;
            }

            string requestBodyString = await ReadRequestBodyAsync(reader, writer);

            string? username = null;
            string? password = null;

            if (IsJson(requestBodyString))
            {
                try
                {
                    var user1 = JsonSerializer.Deserialize<User>(requestBodyString);
                    if (user1 != null)
                    {
                        username = user1.Username;
                        password = user1.Password;
                        Console.WriteLine($"Deserialized User - Username: {username}, Password: {password}");
                    }
                    else
                    {
                        Console.WriteLine("Deserialized user is null.");
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                    SendErrorResponse(writer, HttpStatusCode.BadRequest);
                    return null;
                }
            }
            else
            {
                var formData = System.Web.HttpUtility.ParseQueryString(requestBodyString);
                username = formData["Username"];
                password = formData["Password"];
            }

            if (username == null || password == null)
            {
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return null;
            }

            var authenticatedUser = _userServiceHandler.AuthenticateUser(username, password);

            if (authenticatedUser != null)
                return authenticatedUser;
            else
            {
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return null;
            }
        }


        private async Task<User?> IsIdentiyYesUserCookie(StreamReader reader, StreamWriter writer)
        {
            if (!writer.BaseStream.CanWrite)
            {
                Console.WriteLine("Error");
                return null;
            }

            string requestBodyString = await ReadRequestBodyCookieAsync(reader, writer);

            string? username = null;
            string? password = null;

            if (IsJson(requestBodyString))
            {
                try
                {
                    var user1 = JsonSerializer.Deserialize<User>(requestBodyString);
                    if (user1 != null)
                    {
                        username = user1.Username;
                        password = user1.Password;
                        Console.WriteLine($"Deserialized User - Username: {username}, Password: {password}");
                    }
                    else
                    {
                        Console.WriteLine("Deserialized user is null.");
                    }
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                    SendErrorResponse(writer, HttpStatusCode.BadRequest);
                    return null;
                }
            }
            else
            {
                var formData = System.Web.HttpUtility.ParseQueryString(requestBodyString);
                username = formData["Username"];
                password = formData["Password"];
            }

            if (username == null || password == null)
            {
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return null;
            }

            var authenticatedUser = _userServiceHandler.AuthenticateUser(username, password);

            if (authenticatedUser != null)
                return authenticatedUser;
            else
            {
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return null;
            }
        }



        private async Task<string> ReadRequestBodyAsync(StreamReader reader, StreamWriter writer)
        {
            string? requestLine = await reader.ReadLineAsync();
            if (requestLine == null)
            {
                Console.WriteLine("No request line received.");
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return string.Empty;
            }

            Console.WriteLine($"Request line: {requestLine}");

            int contentLength = 0;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null && line != "")
            {
                Console.WriteLine($"Header: {line}");
                if (line.StartsWith("Content-Length:"))
                {
                    var parts = line.Split(':');
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out contentLength))
                    {
                        Console.WriteLine($"Content-Length: {contentLength}");
                    }
                }
            }

            if (contentLength <= 0)
            {
                Console.WriteLine("Content-Length is invalid or missing.");
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return string.Empty;
            }

            var requestBody = new char[contentLength];
            int totalBytesRead = 0;

            while (totalBytesRead < contentLength)
            {
                int bytesRead = await reader.ReadAsync(requestBody, totalBytesRead, contentLength - totalBytesRead);
                if (bytesRead == 0)
                {
                    Console.WriteLine("End of stream reached before content length was fulfilled.");
                    SendErrorResponse(writer, HttpStatusCode.BadRequest, "Incomplete request body.");
                    return string.Empty;
                }
                totalBytesRead += bytesRead;
            }

            string requestBodyString = new string(requestBody);
            Console.WriteLine($"Received request body: {requestBodyString}");
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();

            return requestBodyString;
        }

        private async Task<string> ReadRequestBodyCookieAsync(StreamReader reader, StreamWriter writer)
        {
            string requestLine = await reader.ReadLineAsync();
            if (requestLine == null)
            {
                Console.WriteLine("No request line received.");
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return string.Empty;
            }
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"Request line: {requestLine}");
     

            string? line;
            string? userDataCookie = null;
            while ((line = await reader.ReadLineAsync()) != null && line != "")
            {
                if (line.StartsWith("Cookie:"))
                {
                    Console.WriteLine($"Request line: {line}");
                    var cookies = line.Substring("Cookie:".Length).Trim().Split(';');

                    foreach (var cookie in cookies)
                    {
                        var trimmedCookie = cookie.Trim();
                        // Check if the cookie starts with "userData=" to capture everything after it
                        if (trimmedCookie.StartsWith("userData="))
                        {
                            // Capture everything after "userData="
                            userDataCookie = trimmedCookie.Substring("userData=".Length);
                            break;
                        }
                    }
                }
            }

            if (userDataCookie == null)
            {
                Console.WriteLine("userData cookie not found.");
                SendErrorResponse(writer, HttpStatusCode.BadRequest);
                return string.Empty;
            }

            Console.WriteLine($"userData cookie: {userDataCookie}");

            // Reset the reader to the beginning of the stream
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();
            Console.ResetColor();
            return userDataCookie;
        }





        private void SendErrorResponse(StreamWriter writer, HttpStatusCode statusCode)
        {
            writer.WriteLine($"HTTP/1.1  {statusCode}");
            writer.WriteLine("Content-Length: 0");
            writer.Flush();
        }

       











    }
}