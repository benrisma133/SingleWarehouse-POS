using System;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsProduct
    {
        // ============================
        // PROPERTIES
        // ============================
        public int ProductID { get; private set; }
        public int CategoryID { get; set; }
        public int ModelID { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        public clsCategory Category { get; private set; }
        public clsModel Model { get; private set; }

        private enum enMode { AddNew, Update }
        private enMode _mode;

        // ============================
        // PRIVATE CONSTRUCTOR (Find)
        // ============================
        private clsProduct(
            int productID, int categoryID, int modelID,
            decimal price, string productName, string description, int quantity)
        {
            ProductID = productID;
            CategoryID = categoryID;
            ModelID = modelID;
            Price = price;
            ProductName = productName;
            Description = description;
            Quantity = quantity;

            Category = clsCategory.FindByID(categoryID);
            Model = clsModel.FindByID(modelID);

            _mode = enMode.Update;
        }

        // ============================
        // PUBLIC CONSTRUCTOR (Add)
        // ============================
        public clsProduct()
        {
            ProductID = -1;
            CategoryID = -1;
            ModelID = -1;
            ProductName = string.Empty;
            Description = string.Empty;
            Quantity = 0;
            _mode = enMode.AddNew;
        }

        // ============================
        // SAVE
        // ============================
        private bool _AddNew()
        {
            ProductID = clsProductData.AddNew(
                ProductName, Description, Price,
                CategoryID == -1 ? (int?)null : CategoryID,
                ModelID == -1 ? (int?)null : ModelID,
                Quantity);

            if (ProductID != -1)
                _mode = enMode.Update;

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

        public bool Save()
        {
            bool success = false;

            switch (_mode)
            {
                case enMode.AddNew:
                    success = _AddNew();
                    break;

                case enMode.Update:
                    success = _Update();
                    break;
            }

            if (success)
            {
                AppEvents.RaiseStockChanged();
            }

            return success;
        }

        // ============================
        // FIND BY ID
        // ============================
        public static clsProduct FindByID(int productID)
        {
            int categoryID = -1, modelID = -1, quantity = 0;
            decimal price = 0;
            string productName = string.Empty, description = string.Empty;

            bool found = clsProductData.GetProduct(
                productID,
                ref categoryID, ref modelID, ref price,
                ref productName, ref description,
                ref quantity);

            if (!found) return null;

            return new clsProduct(
                productID, categoryID, modelID,
                price, productName, description, quantity);
        }

        // ============================
        // GET ALL & DELETE
        // ============================
        public static DataTable GetAll()
        {
            return clsProductData.GetAllProductDetails();
        }

        public static bool Delete(int productID)
        {
            clsProductData.DeleteCompletely(productID);
            return true;
        }

        public static int GetQuantity(int productID)
        {
            return clsProductData.GetQuantity(productID);
        }
    }
}