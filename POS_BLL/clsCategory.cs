using POS_DAL;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_BLL
{
    public class clsCategory
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? IconID { get; set; }

        // ============================
        // Constructor for new category
        // ============================
        public clsCategory()
        {
            _Mode = enMode.AddNew;
            CategoryID = -1;
            Name = "";
            Description = "";
            IconID = -1;
        }

        // ============================
        // Private constructor for loading existing category
        // ============================
        private clsCategory(int categoryID, string name, string description ,int? iconID)
        {
            _Mode = enMode.Update;
            CategoryID = categoryID;
            Name = name;
            Description = description;
            IconID = iconID;
        }

        // ============================
        // Find by ID
        // ============================
        public static clsCategory FindByID(int categoryID)
        {
            string name = string.Empty;
            string description = string.Empty;
            int? iconID = -1;

            bool found = clsCategoryData.GetByID(categoryID, ref name, ref description ,ref iconID);
            if (!found)
                return null;

            return new clsCategory(categoryID, name, description ,iconID);
        }

        // ============================
        // Find by Name (first match)
        // ============================
        public static clsCategory FindByName(string name)
        {
            int categoryID = -1;
            string description = string.Empty;
            int? iconID = -1;

            bool found = clsCategoryData.GetByName(name, ref categoryID, ref description ,ref iconID);
            if (!found)
                return null;

            return new clsCategory(categoryID, name, description ,iconID);
        }

        // ============================
        // Add new category
        // ============================
        bool _AddNew()
        {
            this.CategoryID = clsCategoryData.AddNew(this.Name, this.Description ,this.IconID);
            return this.CategoryID != -1;
        }

        // ============================
        // Update existing category
        // ============================
        bool _Update()
        {
            return clsCategoryData.Update(this.CategoryID, this.Name, this.Description ,this.IconID);
        }

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
            if (_Mode == enMode.AddNew && clsCategoryData.IsCategoryExistByName(Name))
                return false;

            // On update: reject if another category already has this name
            if (_Mode == enMode.Update && clsCategoryData.IsCategoryExistByName(Name, CategoryID))
                return false;

            return true;
        }

        /// <summary>
        ///     Validates then persists the category (insert or update).
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
        // Delete category
        // ============================
        public static bool Delete(int categoryID)
        {
            return clsCategoryData.Delete(categoryID);
        }

        // ============================
        // Get all categories
        // ============================
        public static DataTable GetAll()
        {
            return clsCategoryData.GetAll();
        }

        

        public static bool IsCategoryExistByName(string name)
        {
            return clsCategoryData.IsCategoryExistByName(name);
        }

        public static bool IsCategoryExistByNameExceptID(string name, int exceptCategoryID)
        {
            return clsCategoryData.IsCategoryExistByName(name, exceptCategoryID);
        }

        public static bool GetActiveStatus(int categoryID)
        {
            return clsCategoryData.GetActiveStatus(categoryID);
        }

        public static bool SetActiveStatus(int categoryID, bool isActive)
        {
            return clsCategoryData.SetActiveStatus(categoryID, isActive);
        }

        public static int GetDependencies(int categoryID)
        {
            return clsCategoryData.GetCategoryDependencies(categoryID);
        }

        public static bool CanDelete(int categoryID, out int productsCount)
        {
            productsCount = clsCategoryData.GetCategoryDependencies(categoryID);
            return productsCount == 0;
        }


    }
}
