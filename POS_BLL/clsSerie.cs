// clsSeries.cs
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsSeries
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _Mode;

        public int SeriesID { get; set; }
        public string Name { get; set; }
        public int BrandID { get; set; }
        public string Description { get; set; }
        public clsBrand BrandInfo { get; private set; }

        // ============================
        // CONSTRUCTORS
        // ============================
        public clsSeries()
        {
            _Mode = enMode.AddNew;
            SeriesID = -1;
            Name = string.Empty;
            Description = string.Empty;
            BrandID = -1;
        }

        private clsSeries(int seriesID, string name, int brandID, string description)
        {
            _Mode = enMode.Update;
            SeriesID = seriesID;
            Name = name;
            BrandID = brandID;
            Description = description;
            BrandInfo = clsBrand.FindByID(brandID);
        }

        // ============================
        // FIND
        // ============================

        /// <summary>Returns the series with the given ID, or null if not found.</summary>
        public static clsSeries FindByID(int seriesID)
        {
            string name = string.Empty;
            string description = string.Empty;
            int brandID = -1;

            if (!clsSeriesData.GetByID(seriesID, ref brandID, ref name, ref description))
                return null;

            return new clsSeries(seriesID, name, brandID, description);
        }

        /// <summary>Returns the series with the given name, or null if not found.</summary>
        public static clsSeries FindByName(string name)
        {
            int seriesID = -1;
            int brandID = -1;
            string description = string.Empty;

            if (!clsSeriesData.GetByName(name, ref seriesID, ref brandID, ref description))
                return null;

            return new clsSeries(seriesID, name, brandID, description);
        }

        // ============================
        // PRIVATE SAVE HELPERS
        // ============================
        private bool _AddNew()
        {
            SeriesID = clsSeriesData.AddNew(BrandID, Name, Description);
            return SeriesID != -1;
        }

        private bool _Update()
        {
            return clsSeriesData.Update(SeriesID, BrandID, Name, Description);
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

            if (BrandID == -1)
                return false;

            // On add: reject if the name is already taken
            if (_Mode == enMode.AddNew && clsSeriesData.IsSeriesExistByName(Name))
                return false;

            // On update: reject if another series already has this name
            if (_Mode == enMode.Update && clsSeriesData.IsSeriesExistByName(Name, SeriesID))
                return false;

            return true;
        }

        // ============================
        // SAVE
        // ============================

        /// <summary>
        /// Validates then persists the series (insert or update).
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

        /// <summary>Deletes the series with the given ID.</summary>
        public static bool Delete(int seriesID)
        {
            return clsSeriesData.Delete(seriesID);
        }

        /// <summary>Returns all series as a DataTable.</summary>
        public static DataTable GetAll()
        {
            return clsSeriesData.GetAll();
        }

        /// <summary>Returns all series for the given brand as a DataTable.</summary>
        public static DataTable GetByBrandID(int brandID)
        {
            return clsSeriesData.GetByBrandID(brandID);
        }

        // ============================
        // EXISTENCE CHECKS
        // ============================
        public static bool IsSeriesExistByName(string name)
        {
            return clsSeriesData.IsSeriesExistByName(name);
        }

        public static bool IsSeriesExistByName(string name, int ignoreSeriesID)
        {
            return clsSeriesData.IsSeriesExistByName(name, ignoreSeriesID);
        }

        public static bool GetActiveStatus(int seriesID)
        {
            return clsSeriesData.GetActiveStatus(seriesID);
        }

        public static bool SetActiveStatus(int seriesID, bool isActive)
        {
            return clsSeriesData.SetActiveStatus(seriesID, isActive);
        }

        public static bool CanDelete(int seriesID, out (int Models, int Products) deps)
        {
            var result = clsSeriesData.GetSeriesDependencies(seriesID);
            deps = (result.ModelsCount, result.ProductsCount);
            return result.ModelsCount == 0 && result.ProductsCount == 0;
        }
    }
}