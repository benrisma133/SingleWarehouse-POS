using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using POS_DAL;

namespace POS_BLL
{
    // ── Cart item (UI ↔ BLL contract) ─────────────────────────────────────────────
    public class clsCartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int MaxStock { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    // ── Sale entity ───────────────────────────────────────────────────────────────
    public class clsSale
    {
        // ── Modes ─────────────────────────────────────────────────────────────────
        private enum enMode { AddNew, Update }
        private enMode _mode;

        // ── Properties ────────────────────────────────────────────────────────────
        public int SaleID { get; private set; }
        public int ClientID { get; set; }
        public DateTime SaleDate { get; private set; }
        public decimal TotalPrice { get; private set; }
        public List<clsCartItem> Items { get; set; } = new List<clsCartItem>();

        // ── Public constructor (new sale) ─────────────────────────────────────────
        public clsSale()
        {
            SaleID = -1;
            ClientID = -1;
            SaleDate = DateTime.Now;
            _mode = enMode.AddNew;
        }

        // ── Private constructor (loaded from DB) ──────────────────────────────────
        private clsSale(int saleID, int clientID, DateTime saleDate, decimal totalPrice)
        {
            SaleID = saleID;
            ClientID = clientID;
            SaleDate = saleDate;
            TotalPrice = totalPrice;
            _mode = enMode.Update;
        }

        // ── Save ──────────────────────────────────────────────────────────────────
        private bool _AddNew()
        {
            if (ClientID <= 0 || Items == null || Items.Count == 0)
                return false;

            TotalPrice = Items.Sum(i => i.Total);

            // Map cart items to the tuple the DAL expects
            var lines = Items.Select(i =>
                (i.ProductID, i.ProductName, i.Quantity, i.UnitPrice));

            // Throws InvalidOperationException on stock shortage — let it bubble up
            int newID = clsSalesData.AddSale(ClientID, TotalPrice, lines);

            if (newID == -1) return false;

            SaleID = newID;
            SaleDate = DateTime.Now;
            _mode = enMode.Update;
            return true;
        }

        // Sales are immutable once confirmed (no _Update needed for now).
        // If you add an edit-sale feature later, implement it here.

        public bool Save()
        {
            switch (_mode)
            {
                case enMode.AddNew: return _AddNew();
                default: return false;
            }
        }

        // ── Static helpers ────────────────────────────────────────────────────────

        public static DataTable GetAll()
            => clsSalesData.GetAll();

        public static DataTable GetSaleDetails(int saleID)
            => clsSalesData.GetSaleDetails(saleID);

        public static bool Delete(int saleID)
            => clsSalesData.Delete(saleID);

        public static DataTable GetAvailableProducts(string filter = "")
            => clsSalesData.GetAvailableProducts(filter);

        public static DataTable GetClients()
            => clsSalesData.GetClients();

        public static int GetProductStock(int productID)
            => clsSalesData.GetProductStock(productID);
    }
}