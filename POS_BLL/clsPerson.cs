using System;
using POS_DAL;

namespace POS_BLL
{
    public class clsPerson
    {
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Gender { get; set; }

        public clsPerson()
        {
            PersonID = -1;
            FirstName = "";
            LastName = "";
            DateOfBirth = null;
            Email = "";
            Phone = "";
            Address = "";
            Gender = 0;
        }

        protected clsPerson(int personID, string firstName, string lastName,
            string dob, string email, string phone, string address, int gender)
        {
            PersonID = personID;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dob;
            Email = email;
            Phone = phone;
            Address = address;
            Gender = gender;
        }

        public static clsPerson FindByID(int personID)
        {
            string firstName = "", lastName = "", dob = "", email = "", phone = "", address = "";
            int gender = 0;

            bool found = clsPersonData.GetByID(personID,
                ref firstName, ref lastName, ref dob,
                ref email, ref phone, ref address, ref gender);

            if (!found) return null;

            return new clsPerson(personID, firstName, lastName, dob, email, phone, address, gender);
        }
    }
}