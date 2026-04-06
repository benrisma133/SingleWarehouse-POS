using POS_DAL;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_BLL
{
    public class clsModel
    {
        enum enMode { AddNew = 1, Update = 2 }
        enMode _Mode;

        public int ModelID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int? SerieID { get; set; } // Optional: if you want to link model to a serie
        clsSeries SerieInfo { get; set; } // Optional: if you want to link model to a serie

        // ============================
        // Constructor for new model
        // ============================
        public clsModel()
        {
            _Mode = enMode.AddNew;
            ModelID = -1;
            Name = "";
            Description = "";
            SerieID = -1;
        }

        // ============================
        // Private constructor for loading existing model
        // ============================
        private clsModel(int modelID, string name, string description ,int? serieID)
        {
            _Mode = enMode.Update;
            ModelID = modelID;
            Name = name;
            Description = description;
            SerieID = serieID;
            SerieInfo = clsSeries.FindByID(serieID ?? -1); // Load serie info if needed
        }

        // ============================
        // Find by ID
        // ============================
        public static clsModel FindByID(int modelID)
        {
            string name = "";
            string description = "";
            int? serieID = -1;

            if (clsModelData.GetByID(modelID, ref name, ref description ,ref serieID))
                return new clsModel(modelID, name, description ,serieID);

            return null;
        }

        // ============================
        // Find by Name
        // ============================
        public static clsModel FindByName(string name)
        {
            int modelID = -1;
            string description = "";
            int? serieID = -1;

            if (clsModelData.GetByName(name, ref modelID, ref description ,ref serieID))
                return new clsModel(modelID, name, description , serieID);

            return null;
        }

        // ============================
        // Add new model
        // ============================
        bool _AddNew()
        {
            this.ModelID = clsModelData.AddNew(this.Name, this.Description ,this.SerieID);
            return this.ModelID != -1;
        }

        // ============================
        // Update existing model
        // ============================
        bool _Update()
        {
            return clsModelData.Update(this.ModelID, this.Name, this.Description ,this.SerieID);
        }

        // ============================
        // Save (AddNew or Update)
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
        // Delete model
        // ============================
        public static bool Delete(int modelID)
        {
            return clsModelData.Delete(modelID);
        }

        // ============================
        // Get all models
        // ============================
        public static DataTable GetAll()
        {
            return clsModelData.GetAll();
        }

        public static bool DeleteCompletely(int ModelID)
        {
            if (ModelID == -1)
                return false;

            return clsModelData.DeleteModelCompletely(ModelID);
        }

        public static DataTable GetAllDistinct()
        {
            return clsModelData.GetAllDistinct();
        }


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
