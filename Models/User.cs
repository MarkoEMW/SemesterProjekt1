using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
<<<<<<< HEAD
using System.Xml.Linq;
=======
>>>>>>> 2294446d5af047acbbeaeaf021f7e0e97165c055
using static System.Net.Mime.MediaTypeNames;

namespace SemesterProjekt1
{
    public class User
    {
        private int _id;

        private string _username;

        private string _password;

        private Inventory _Inventory;

        private string _bio;

        private string _image;

<<<<<<< HEAD
        private string _name;

=======
>>>>>>> 2294446d5af047acbbeaeaf021f7e0e97165c055
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public Inventory Inventory
        {
            get { return _Inventory; }
            set { _Inventory = value; }
        }

        public string Bio
        {
            get { return _bio; }
            set { _bio = value; }
        }

        public string Image
        {
            get { return _image; }
            set { _image = value; }
        }

<<<<<<< HEAD
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }



=======
>>>>>>> 2294446d5af047acbbeaeaf021f7e0e97165c055
        ~User()
        {
            Console.WriteLine($"User {_username}, {_password} wird zerstört.");
        }

        public User(int id, string username, string password)
        {
            this._id = id;
            this._username = username;
            this._password = password;
            this._Inventory = new Inventory(this._id);
            this._image = string.Empty;
            this._bio = string.Empty;
<<<<<<< HEAD
            this._name = string.Empty;
        }

        [JsonConstructor]
        public User(int id, string username, string password, Inventory inventory, string bio, string image, string name)
=======
        }

        [JsonConstructor]
        public User(int id, string username, string password, Inventory inventory, string bio, string image)
>>>>>>> 2294446d5af047acbbeaeaf021f7e0e97165c055
        {
            this._id = id;
            this._username = username;
            this._password = password;
            this._Inventory = inventory ?? new Inventory(this._id);
            this._image = image ?? string.Empty;
            this._bio = bio ?? string.Empty;
<<<<<<< HEAD
            this._name = name ?? string.Empty;

=======
>>>>>>> 2294446d5af047acbbeaeaf021f7e0e97165c055
        }

        public void GetNextAvailableId(List<User> userlist)
        {
            this.Id = userlist.Any() ? userlist.Max(u => u.Id) + 1 : 1;
        }
    }
}