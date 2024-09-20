﻿using System;
using System.Collections.Generic;
using System.Net;

public class UserServiceHandler
{
    public List<User> _users;
    public DatabaseHandler _databaseHandler;

    public UserServiceHandler()
    {
        _databaseHandler = new DatabaseHandler();
        _users = _databaseHandler.LoadUser();

        // Initialize with default users if the list is empty
        if (_users.Count == 0)
        {
            InitializeDefaultUsers();
            _databaseHandler.SaveUsers(_users);
        }
    }

    private void InitializeDefaultUsers()
    {
        _users = new List<User>
        {
            new User(1,"Ender","123"),
            new User(2, "John Doe", "test"),
            new User(3, "Jane Smith", "test"),
        };
    }

    public List<User> GetAllUsers()
    {
        return _users;
    }

    public User GetUserById(int id)
    {
        return _users.Find(p => p.Id == id);
    }

    public void AddUser(User user)
    {
        _users.Add(user);
        _databaseHandler.SaveUsers(_users);
    }

    public void UpdateUser(int id, User updatedUser)
    {
        var user = GetUserById(id);
        if (user != null)
        {
            user.Name = updatedUser.Name;
            user.Password = updatedUser.Password;
            _databaseHandler.SaveUsers(_users);
        }
    }

    public void DeleteUser(int id)
    {
        var user = GetUserById(id);
        if (user != null)
        {
            _users.Remove(user);
            _databaseHandler.SaveUsers(_users);
        }
    }

    public User AuthenticateUser(string username, string password)
    {
        return _users.Find(u => u.Name == username && u.Password == password);
    }

    public User BuyPacks(int Userid, int amount, string username, string password)
    {
        var user = GetUserById(Userid);
        if (user != null && AuthenticateUser(username, password) != null)
        {
            user.Inventory.AddCardPack(new CardPack(Userid), amount);
            _databaseHandler.SaveUsers(_users);
        }
        return user;
    }



    public User OpenCardPack(int userId, string username, string password)
    {
        var user = _users.Find(p => p.Id == userId && p.Name == username && p.Password == password);
        if (user != null && AuthenticateUser(username, password) != null)
        {
            if (user.Inventory.CardPacks.Count > 0)
            {
                var cardPack = user.Inventory.CardPacks[0];
                user.Inventory.OpenCardPack(cardPack);
                user.Inventory.CardPacks.RemoveAt(0);
                _databaseHandler.SaveUsers(_users);
            }
        }
        return user;
    }


    private void SendResponse(HttpListenerResponse response, string content, string contentType)
{
    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
    response.ContentLength64 = buffer.Length;
    response.ContentType = contentType;
    response.OutputStream.Write(buffer, 0, buffer.Length);
    response.OutputStream.Close();
}

}