using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsPersonData
    {
        // ============================
        // GET PERSON BY ID
        // ============================
        public static bool GetByID(int personID,
            ref string firstName,
            ref string lastName,
            ref string dateOfBirth,
            ref string email,
            ref string phone,
            ref string address,
            ref int gender)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT FirstName, LastName, DateOfBirth,
                               Email, Phone, Address, Gender
                        FROM Persons
                        WHERE PersonID = @PersonID
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            firstName = reader["FirstName"].ToString();
                            lastName = reader["LastName"].ToString();
                            dateOfBirth = reader["DateOfBirth"]?.ToString();
                            email = reader["Email"].ToString();
                            phone = reader["Phone"].ToString();
                            address = reader["Address"].ToString();
                            gender = Convert.ToInt32(reader["Gender"]);

                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetByID Person: " + ex.Message);
            }
        }
    }
}