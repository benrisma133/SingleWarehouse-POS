// clsProduct.cs
using System;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsProduct
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _Mode;

        public int ProductID { get; private set; }
        public int CategoryID { get; set; }
        public int ModelID { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        public clsCategory Category { get; private set; }
        public clsModel Model { get; private set; }

        // ============================
        // CONSTRUCTORS
        // ============================
        public clsProduct()
        {
            _Mode = enMode.AddNew;
            ProductID = -1;
            CategoryID = -1;
            ModelID = -1;
            ProductName = string.Empty;
            Description = string.Empty;
            Price = 0;
            Quantity = 0;
        }

        private clsProduct(
            int productID, int categoryID, int modelID,
            decimal price, string productName, string description, int quantity)
        {
            _Mode = enMode.Update;
            ProductID = productID;
            CategoryID = categoryID;
            ModelID = modelID;
            Price = price;
            ProductName = productName;
            Description = description;
            Quantity = quantity;

            Category = clsCategory.FindByID(categoryID);
            Model = clsModel.FindByID(modelID);
        }

        // ============================
        // FIND
        // ============================

        /// <summary>Returns the product with the given ID, or null if not found.</summary>
        public static clsProduct FindByID(int productID)
        {
            int categoryID = -1, modelID = -1, quantity = 0;
            decimal price = 0;
            string productName = string.Empty, description = string.Empty;

            if (!clsProductData.GetProduct(
                    productID,
                    ref categoryID, ref modelID, ref price,
                    ref productName, ref description,
                    ref quantity))
                return null;

            return new clsProduct(
                productID, categoryID, modelID,
                price, productName, description, quantity);
        }

        // ============================
        // PRIVATE SAVE HELPERS
        // ============================
        private bool _AddNew()
        {
            ProductID = clsProductData.AddNew(
                ProductName, Description, Price,
                CategoryID == -1 ? (int?)null : CategoryID,
                ModelID == -1 ? (int?)null : ModelID,
                Quantity);

            return ProductID != -1;
        }

        private bool _Update()
        {
            clsProductData.Update(
                ProductID, ProductName, Description, Price,
                CategoryID == -1 ? (int?)null : CategoryID,
                ModelID == -1 ? (int?)null : ModelID,
                Quantity);

            return true;
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
            if (string.IsNullOrWhiteSpace(ProductName))
                return false;

            if (Price <= 0)
                return false;

            if (Quantity < 0)
                return false;

            return true;
        }

        // ============================
        // SAVE
        // ============================

        /// <summary>
        /// Validates then persists the product (insert or update).
        /// Returns false if validation fails or the DAL reports no rows affected.
        /// Throws if the DAL encounters a database error.
        /// </summary>
        public bool Save()
        {
            if (!IsValid())
                return false;

            bool success;

            switch (_Mode)
            {
                case enMode.AddNew:
                    success = _AddNew();
                    if (success)
                        _Mode = enMode.Update;
                    break;

                case enMode.Update:
                    success = _Update();
                    break;

                default:
                    return false;
            }

            if (success)
            {
                clsNotify.CheckStock(ProductID, Quantity, 10);
                AppEvents.RaiseStockChanged();
            }

            return success;
        }

        // ============================
        // STATIC OPERATIONS
        // ============================

        /// <summary>Returns all products as a DataTable.</summary>
        public static DataTable GetAll()
        {
            return clsProductData.GetAllProductDetails();
        }

        /// <summary>Deletes the product with the given ID.</summary>
        public static bool Delete(int productID)
        {
            return clsProductData.Delete(productID);
        }

        /// <summary>Returns the current stock quantity for the given product.</summary>
        public static int GetQuantity(int productID)
        {
            return clsProductData.GetQuantity(productID);
        }
    }
}