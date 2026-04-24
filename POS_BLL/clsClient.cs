// clsClient.cs
using System;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsClient
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _Mode;

        public int ClientID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // ============================
        // CONSTRUCTORS
        // ============================
        public clsClient()
        {
            _Mode = enMode.AddNew;
            ClientID = -1;
            FirstName = string.Empty;
            LastName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
        }

        private clsClient(int clientID, string firstName, string lastName,
            string phone, string email)
        {
            _Mode = enMode.Update;
            ClientID = clientID;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            Email = email;
        }

        // ============================
        // FIND
        // ============================

        /// <summary>Returns the client with the given ID, or null if not found.</summary>
        public static clsClient FindByID(int clientID)
        {
            string firstName = string.Empty;
            string lastName = string.Empty;
            string phone = string.Empty;
            string email = string.Empty;

            if (!clsClientData.GetByID(clientID, ref firstName, ref lastName,
                    ref phone, ref email))
                return null;

            return new clsClient(clientID, firstName, lastName, phone, email);
        }

        // ============================
        // PRIVATE SAVE HELPERS
        // ============================
        private bool _AddNew()
        {
            ClientID = clsClientData.AddNew(FirstName, LastName, Phone, Email);
            return ClientID != -1;
        }

        private bool _Update()
        {
            return clsClientData.Update(ClientID, FirstName, LastName, Phone, Email);
        }

        // ============================
        // VALIDATION
        // ============================

        /// <summary>
        /// Returns true if the object is in a valid state to be saved.
        /// Call this before Save() if you want to surface errors in the UI
        /// without relying on exceptions.
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return false;

            if (string.IsNullOrWhiteSpace(LastName))
                return false;

            if (string.IsNullOrWhiteSpace(Phone))
                return false;

            // Phone duplicate check
            if (_Mode == enMode.AddNew && clsClientData.IsPhoneExist(Phone))
                return false;

            if (_Mode == enMode.Update && clsClientData.IsPhoneExist(Phone, ClientID))
                return false;

            // Email is optional — only validate if provided
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (_Mode == enMode.AddNew && clsClientData.IsEmailExist(Email))
                    return false;

                if (_Mode == enMode.Update && clsClientData.IsEmailExist(Email, ClientID))
                    return false;
            }

            return true;
        }

        // ============================
        // SAVE
        // ============================

        /// <summary>
        /// Validates then persists the client (insert or update).
        /// Returns false if validation fails or the DAL reports no rows affected.
        /// Throws if the DAL encounters a database error.
        /// </summary>
        public bool Save()
        {
            if (!IsValid())
                return false;

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

        // ============================
        // STATIC OPERATIONS
        // ============================

        /// <summary>Deletes the client with the given ID.</summary>
        public static bool Delete(int clientID)
        {
            return clsClientData.Delete(clientID);
        }

        /// <summary>Returns all clients as a DataTable.</summary>
        public static DataTable GetAll()
        {
            return clsClientData.GetAll();
        }

        // ============================
        // EXISTENCE CHECKS
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