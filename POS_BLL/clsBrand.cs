using System;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsBrand
    {
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _Mode;

        public int BrandID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // ============================
        // CONSTRUCTORS
        // ============================

        public clsBrand()
        {
            _Mode = enMode.AddNew;
            BrandID = -1;
            Name = string.Empty;
            Description = string.Empty;
        }

        private clsBrand(int brandID, string name, string description)
        {
            _Mode = enMode.Update;
            BrandID = brandID;
            Name = name;
            Description = description;
        }

        // ============================
        // FIND
        // ============================

        /// <summary>Returns the brand with the given ID, or null if not found.</summary>
        public static clsBrand FindByID(int brandID)
        {
            string name = string.Empty;
            string description = string.Empty;

            if (!clsBrandsData.GetByID(brandID, ref name, ref description))
                return null;

            return new clsBrand(brandID, name, description);
        }

        /// <summary>Returns the brand with the given name, or null if not found.</summary>
        public static clsBrand FindByName(string name)
        {
            int brandID = -1;
            string description = string.Empty;

            if (!clsBrandsData.GetByName(name, ref brandID, ref description))
                return null;

            return new clsBrand(brandID, name, description);
        }

        // ============================
        // PRIVATE SAVE HELPERS
        // ============================

        private bool _AddNew()
        {
            BrandID = clsBrandsData.AddNew(Name, Description);
            return BrandID != -1;
        }

        private bool _Update()
        {
            return clsBrandsData.Update(BrandID, Name, Description);
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
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // On add: reject if the name is already taken
            if (_Mode == enMode.AddNew && clsBrandsData.IsBrandExistByName(Name))
                return false;

            // On update: reject if another brand already has this name
            if (_Mode == enMode.Update && clsBrandsData.IsBrandExistByName(Name, BrandID))
                return false;

            return true;
        }

        // ============================
        // SAVE
        // ============================

        /// <summary>
        ///     Validates then persists the brand (insert or update).
        ///     Returns false if validation fails or the DAL reports no rows affected.
        ///     Throws if the DAL encounters a database error.
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

        /// <summary>
        /// Deletes the brand with the given ID.
        /// Returns false if the brand is linked to other records (FK constraint).
        /// </summary>
        public static bool Delete(int brandID)
        {
            return clsBrandsData.Delete(brandID);
        }

        /// <summary>Returns all brands as a DataTable.</summary>
        public static DataTable GetAll()
        {
            return clsBrandsData.GetAll();
        }

        /// <summary>
        /// Gets the active status of the brand with the given ID.
        /// </summary>
        /// <param name="brandID">The ID of the brand.</param>
        /// <returns>True if the brand is active, false otherwise.</returns>
        public static bool GetActiveStatus(int brandID)
        {
            return clsBrandsData.GetActiveStatus(brandID);
        }

        /// <summary>
        /// Sets the active status of the brand with the given ID.
        /// </summary>
        /// <param name="brandID">The ID of the brand.</param>
        /// <param name="isActive">The active status to set.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public static bool SetActiveStatus(int brandID, bool isActive)
        {
            return clsBrandsData.SetActiveStatus(brandID, isActive);
        }

        public static bool IsBrandExistByName(string name)
        {
            return clsBrandsData.IsBrandExistByName(name);
        }

        public static bool IsBrandExistByName(string name, int ignoreBrandID)
        {
            return clsBrandsData.IsBrandExistByName(name, ignoreBrandID);
        }

        // ============================
        // CAN DELETE BRAND
        // ============================
        public static bool CanDelete(int brandID, out (int Series, int Models, int Products) deps)
        {
            deps = clsBrandsData.GetBrandDependencies(brandID);

            return deps.Series == 0 && deps.Models == 0 && deps.Products == 0;
        }

    }
}