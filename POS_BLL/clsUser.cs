using System;
using POS_DAL;

namespace POS_BLL
{
    public class clsUser : clsPerson
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public string Username { get; set; }
        public string PasswordHash { get; set; }

        // ============================
        // CONSTRUCTORS
        // ============================
        public clsUser()
        {
            _Mode = enMode.AddNew;

            PersonID = -1;
            Username = "";
            PasswordHash = "";
        }

        private clsUser(int personID,
            string firstName, string lastName, string dob,
            string email, string phone, string address, int gender,
            string username, string passwordHash)
            : base(personID, firstName, lastName, dob, email, phone, address, gender)
        {
            _Mode = enMode.Update;

            Username = username;
            PasswordHash = passwordHash;
        }

        // ============================
        // FIND (LOGIN)
        // ============================
        public static clsUser FindByUsernameAndPassword(string username, string passwordHash)
        {
            int personID = clsUserData.GetPersonIDByUsernameAndPassword(username, passwordHash);

            if (personID == -1)
                return null;

            clsPerson person = clsPerson.FindByID(personID);
            if (person == null)
                return null;

            return new clsUser(
                person.PersonID,
                person.FirstName,
                person.LastName,
                person.DateOfBirth,
                person.Email,
                person.Phone,
                person.Address,
                person.Gender,
                username,
                passwordHash
            );
        }

        // ============================
        // ADD NEW
        // ============================
        private bool _AddNew()
        {
            this.PersonID = clsUserData.Add(
                this.FirstName,
                this.LastName,
                this.DateOfBirth,
                this.Email,
                this.Phone,
                this.Address,
                this.Gender,
                this.Username,
                this.PasswordHash
            );

            return this.PersonID != -1;
        }

        // ============================
        // UPDATE
        // ============================
        private bool _Update()
        {
            return clsUserData.Update(
                this.PersonID,
                this.FirstName,
                this.LastName,
                this.DateOfBirth,
                this.Email,
                this.Phone,
                this.Address,
                this.Gender,
                this.Username,
                this.PasswordHash
            );
        }

        // ============================
        // SAVE
        // ============================
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

        // ============================
        // CHECK USERNAME EXISTS
        // ============================
        public static bool IsUsernameExist(string username)
        {
            return clsUserData.IsUsernameExist(username);
        }

        public static bool IsUsernameExist(string username, int ignorePersonID)
        {
            return clsUserData.IsUsernameExist(username, ignorePersonID);
        }

        // ============================
        // CREATE DEFAULT ADMIN (FIRST RUN)
        // ============================
        public static void EnsureDefaultAdmin()
        {
            try
            {
                // if any user exists → do nothing
                if (clsUserData.IsUsernameExist("admin"))
                    return;

                clsUser user = new clsUser();

                // 🔐 User Info
                user.Username = "admin";
                user.PasswordHash = "admin"; // (later you can hash it)

                // 👤 Personal Info (NOT EMPTY)
                user.FirstName = "System";
                user.LastName = "Administrator";
                user.DateOfBirth = "2000-01-01";
                user.Email = "admin@system.local";
                user.Phone = "0600000000";
                user.Address = "Default Address";
                user.Gender = 0; // Male

                user.Save();
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating default admin: " + ex.Message);
            }
        }

        // ============================
        // CHANGE PASSWORD
        // ============================

        /// <summary>
        /// Verifies the old password hash then updates to the new one.
        /// You must pass already-hashed values — hash them yourself
        /// before calling this method (e.g. using your existing hash utility).
        /// Syncs PasswordHash on this object on success.
        /// </summary>
        public bool ChangePassword(string oldPasswordHash, string newPasswordHash)
        {
            bool success = clsUserData.ChangePassword(
                this.PersonID,
                oldPasswordHash,
                newPasswordHash);

            if (success)
                this.PasswordHash = newPasswordHash;

            return success;
        }
    }
}