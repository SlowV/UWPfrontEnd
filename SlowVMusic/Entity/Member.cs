using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowVMusic.Entity
{
    class Member
    {
        private long _id;
        private string _firstName;
        private string _lastName;
        private string _avatar;
        private string _phone;
        private string _address;
        private string _introduction;
        private int _gender = 0;
        private string _birthday;
        private string _email;
        private string _password;
        private string _salt;
        private string _createdAt;
        private string _updatedAt;
        private string _status;


        public string firstName { get => _firstName; set => _firstName = value; }
        public string lastName { get => _lastName; set => _lastName = value; }
        public string avatar { get => _avatar; set => _avatar = value; }
        public string phone { get => _phone; set => _phone = value; }
        public string address { get => _address; set => _address = value; }
        public string introduction { get => _introduction; set => _introduction = value; }
        public int gender { get => _gender; set => _gender = value; }
        public string birthday { get => _birthday; set => _birthday = value; }
        public string email { get => _email; set => _email = value; }
        public string password { get => _password; set => _password = value; }
        public long id { get => _id; set => _id = value; }
        public string salt { get => _salt; set => _salt = value; }
        public string createdAt { get => _createdAt; set => _createdAt = value; }
        public string updatedAt { get => _updatedAt; set => _updatedAt = value; }
        public string status { get => _status; set => _status = value; }
    }
}
