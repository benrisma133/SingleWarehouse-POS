// clsModel.cs
using POS_DAL;
using System.Data;

namespace POS_BLL
{
    public class clsModel
    {
        // ============================
        // FIELDS
        // ============================
        private enum enMode { AddNew = 1, Update = 2 }
        private enMode _Mode;

        public int ModelID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? SerieID { get; set; }
        public clsSeries SerieInfo { get; private set; }

        // ============================
        // CONSTRUCTORS
        // ============================
        public clsModel()
        {
            _Mode = enMode.AddNew;
            ModelID = -1;
            Name = string.Empty;
            Description = string.Empty;
            SerieID = null;
        }

        private clsModel(int modelID, string name, string description, int? serieID)
        {
            _Mode = enMode.Update;
            ModelID = modelID;
            Name = name;
            Description = description;
            SerieID = serieID;
            SerieInfo = clsSeries.FindByID(serieID ?? -1);
        }

        // ============================
        // FIND
        // ============================

        /// <summary>Returns the model with the given ID, or null if not found.</summary>
        public static clsModel FindByID(int modelID)
        {
            string name = string.Empty;
            string description = string.Empty;
            int? serieID = null;

            if (!clsModelData.GetByID(modelID, ref name, ref description, ref serieID))
                return null;

            return new clsModel(modelID, name, description, serieID);
        }

        /// <summary>Returns the model with the given name, or null if not found.</summary>
        public static clsModel FindByName(string name)
        {
            int modelID = -1;
            string description = string.Empty;
            int? serieID = null;

            if (!clsModelData.GetByName(name, ref modelID, ref description, ref serieID))
                return null;

            return new clsModel(modelID, name, description, serieID);
        }

        // ============================
        // PRIVATE SAVE HELPERS
        // ============================
        private bool _AddNew()
        {
            ModelID = clsModelData.AddNew(Name, Description, SerieID);
            return ModelID != -1;
        }

        private bool _Update()
        {
            return clsModelData.Update(ModelID, Name, Description, SerieID);
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
            if (_Mode == enMode.AddNew && clsModelData.IsModelExistByName(Name))
                return false;

            // On update: reject if another model already has this name
            if (_Mode == enMode.Update && clsModelData.IsModelExistByName(Name, ModelID))
                return false;

            return true;
        }

        // ============================
        // SAVE
        // ============================

        /// <summary>
        /// Validates then persists the model (insert or update).
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

        /// <summary>Deletes the model with the given ID.</summary>
        public static bool Delete(int modelID)
        {
            return clsModelData.Delete(modelID);
        }

        /// <summary>Completely deletes the model and all related data.</summary>
        public static bool DeleteCompletely(int modelID)
        {
            if (modelID == -1)
                return false;

            return clsModelData.DeleteModelCompletely(modelID);
        }

        /// <summary>Returns all models as a DataTable.</summary>
        public static DataTable GetAll()
        {
            return clsModelData.GetAll();
        }

        /// <summary>Returns all distinct models as a DataTable.</summary>
        public static DataTable GetAllDistinct()
        {
            return clsModelData.GetAllDistinct();
        }

        // ============================
        // EXISTENCE CHECKS
        // ============================
        public static bool IsModelExistsByName(string name)
        {
            return clsModelData.IsModelExistByName(name);
        }

        public static bool IsModelExistsByNameExcludingID(string name, int excludeModelID)
        {
            return clsModelData.IsModelExistByName(name, excludeModelID);
        }
    }
}