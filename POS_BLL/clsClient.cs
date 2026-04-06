using System;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsClient
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int ClientID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public clsClient()
        {
            _Mode = enMode.AddNew;
            ClientID = -1;
            FirstName = "";
            LastName = "";
            Phone = "";
            Email = "";
        }

        private clsClient(int clientID, string firstName, string lastName, string phone, string email)
        {
            _Mode = enMode.Update;
            ClientID = clientID;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            Email = email;
        }

        public static clsClient FindByID(int clientID)
        {
            string firstName = string.Empty;
            string lastName = string.Empty;
            string phone = string.Empty;
            string email = string.Empty;

            bool found = clsClientData.GetByID(clientID, ref firstName, ref lastName, ref phone, ref email);
            if (!found) return null;

            return new clsClient(clientID, firstName, lastName, phone, email);
        }

        bool _AddNew()
        {
            this.ClientID = clsClientData.AddNew(this.FirstName, this.LastName, this.Phone, this.Email);
            return this.ClientID != -1;
        }

        bool _Update()
        {
            return clsClientData.Update(this.ClientID, this.FirstName, this.LastName, this.Phone, this.Email);
        }

        public bool Save()
        {
            switch (_Mode)
            {
                case enMode.AddNew:
                    if (_AddNew())
                    {
                        _Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _Update();
            }
            return false;
        }

        public static bool Delete(int clientID)
        {
            return clsClientData.Delete(clientID);
        }

        public static DataTable GetAll()
        {
            return clsClientData.GetAll();
        }

        // ============================
        // CHECK IF EMAIL OR PHONE EXISTS
        // ============================
        public static bool IsEmailExist(string email)
        {
            return clsClientData.IsEmailExist(email);
        }

        public static bool IsEmailExist(string email, int ignoreClientID)
        {
            return clsClientData.IsEmailExist(email, ignoreClientID);
        }

        public static bool IsPhoneExist(string phone)
        {
            return clsClientData.IsPhoneExist(phone);
        }

        public static bool IsPhoneExist(string phone, int ignoreClientID)
        {
            return clsClientData.IsPhoneExist(phone, ignoreClientID);
        }
    }
}