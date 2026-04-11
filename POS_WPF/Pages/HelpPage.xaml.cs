using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace POS_WPF.Pages
{
    public partial class HelpPage : UserControl
    {
        // ── State ────────────────────────────────────────────────────────────
        private bool _categoriesTopicOpen = false;
        private bool _modelsTopicOpen = false;
        private bool _seriesTopicOpen = false;
        private bool _brandsTopicOpen = false;
        private bool _productsTopicOpen = false;
        private bool _clientsTopicOpen = false;
        private bool _newSaleTopicOpen = false;
        private bool _saleDetailsTopicOpen = false;

        private bool _addOpen = false;
        private bool _updateOpen = false;
        private bool _deleteOpen = false;

        private bool _addModelOpen = false;
        private bool _updateModelOpen = false;
        private bool _deleteModelOpen = false;

        private bool _addSeriesOpen = false;
        private bool _updateSeriesOpen = false;
        private bool _deleteSeriesOpen = false;

        private bool _addBrandOpen = false;
        private bool _updateBrandOpen = false;
        private bool _deleteBrandOpen = false;

        private bool _addProductOpen = false;
        private bool _updateProductOpen = false;
        private bool _deleteProductOpen = false;

        private bool _addClientOpen = false;
        private bool _updateClientOpen = false;
        private bool _deleteClientOpen = false;

        private bool _howToSaleOpen = false;
        private bool _viewSalesOpen = false;
        private bool _deleteSaleOpen = false;

        public HelpPage()
        {
            InitializeComponent();
        }

        // ════════════════════════════════════════════════════════════
        //  TOPIC TOGGLES
        // ════════════════════════════════════════════════════════════

        private void CategoriesTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _categoriesTopicOpen = !_categoriesTopicOpen;
            if (!_categoriesTopicOpen) CollapseSubItems_Categories();
            TogglePanel(CategoriesTopicContent, CategoriesTopicInner,
                        CategoriesTopicArrow, _categoriesTopicOpen);
        }

        private void ModelsTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _modelsTopicOpen = !_modelsTopicOpen;
            if (!_modelsTopicOpen) CollapseSubItems_Models();
            TogglePanel(ModelsTopicContent, ModelsTopicInner,
                        ModelsTopicArrow, _modelsTopicOpen);
        }

        private void SeriesTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _seriesTopicOpen = !_seriesTopicOpen;
            if (!_seriesTopicOpen) CollapseSubItems_Series();
            TogglePanel(SeriesTopicContent, SeriesTopicInner,
                        SeriesTopicArrow, _seriesTopicOpen);
        }

        private void BrandsTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _brandsTopicOpen = !_brandsTopicOpen;
            if (!_brandsTopicOpen) CollapseSubItems_Brands();
            TogglePanel(BrandsTopicContent, BrandsTopicInner,
                        BrandsTopicArrow, _brandsTopicOpen);
        }

        private void ProductsTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _productsTopicOpen = !_productsTopicOpen;
            if (!_productsTopicOpen) CollapseSubItems_Products();
            TogglePanel(ProductsTopicContent, ProductsTopicInner,
                        ProductsTopicArrow, _productsTopicOpen);
        }

        private void ClientsTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _clientsTopicOpen = !_clientsTopicOpen;
            if (!_clientsTopicOpen) CollapseSubItems_Clients();
            TogglePanel(ClientsTopicContent, ClientsTopicInner,
                        ClientsTopicArrow, _clientsTopicOpen);
        }

        private void NewSaleTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _newSaleTopicOpen = !_newSaleTopicOpen;
            if (!_newSaleTopicOpen) CollapseSubItems_NewSale();
            TogglePanel(NewSaleTopicContent, NewSaleTopicInner,
                        NewSaleTopicArrow, _newSaleTopicOpen);
        }

        private void SaleDetailsTopic_Toggle(object sender, MouseButtonEventArgs e)
        {
            _saleDetailsTopicOpen = !_saleDetailsTopicOpen;
            if (!_saleDetailsTopicOpen) CollapseSubItems_SaleDetails();
            TogglePanel(SaleDetailsTopicContent, SaleDetailsTopicInner,
                        SaleDetailsTopicArrow, _saleDetailsTopicOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  CATEGORY SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void AddCategory_Toggle(object sender, MouseButtonEventArgs e)
        {
            _addOpen = !_addOpen;
            TogglePanel(AddCategoryContent, AddCategoryInner, AddArrowRotate, _addOpen);
        }

        private void UpdateCategory_Toggle(object sender, MouseButtonEventArgs e)
        {
            _updateOpen = !_updateOpen;
            TogglePanel(UpdateCategoryContent, UpdateCategoryInner, UpdateArrowRotate, _updateOpen);
        }

        private void DeleteCategory_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteOpen = !_deleteOpen;
            TogglePanel(DeleteCategoryContent, DeleteCategoryInner, DeleteArrowRotate, _deleteOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  MODEL SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void AddModel_Toggle(object sender, MouseButtonEventArgs e)
        {
            _addModelOpen = !_addModelOpen;
            TogglePanel(AddModelContent, AddModelInner, AddModelArrowRotate, _addModelOpen);
        }

        private void UpdateModel_Toggle(object sender, MouseButtonEventArgs e)
        {
            _updateModelOpen = !_updateModelOpen;
            TogglePanel(UpdateModelContent, UpdateModelInner, UpdateModelArrowRotate, _updateModelOpen);
        }

        private void DeleteModel_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteModelOpen = !_deleteModelOpen;
            TogglePanel(DeleteModelContent, DeleteModelInner, DeleteModelArrowRotate, _deleteModelOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  SERIES SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void AddSeries_Toggle(object sender, MouseButtonEventArgs e)
        {
            _addSeriesOpen = !_addSeriesOpen;
            TogglePanel(AddSeriesContent, AddSeriesInner, AddSeriesArrowRotate, _addSeriesOpen);
        }

        private void UpdateSeries_Toggle(object sender, MouseButtonEventArgs e)
        {
            _updateSeriesOpen = !_updateSeriesOpen;
            TogglePanel(UpdateSeriesContent, UpdateSeriesInner, UpdateSeriesArrowRotate, _updateSeriesOpen);
        }

        private void DeleteSeries_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteSeriesOpen = !_deleteSeriesOpen;
            TogglePanel(DeleteSeriesContent, DeleteSeriesInner, DeleteSeriesArrowRotate, _deleteSeriesOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  BRAND SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void AddBrand_Toggle(object sender, MouseButtonEventArgs e)
        {
            _addBrandOpen = !_addBrandOpen;
            TogglePanel(AddBrandContent, AddBrandInner, AddBrandArrowRotate, _addBrandOpen);
        }

        private void UpdateBrand_Toggle(object sender, MouseButtonEventArgs e)
        {
            _updateBrandOpen = !_updateBrandOpen;
            TogglePanel(UpdateBrandContent, UpdateBrandInner, UpdateBrandArrowRotate, _updateBrandOpen);
        }

        private void DeleteBrand_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteBrandOpen = !_deleteBrandOpen;
            TogglePanel(DeleteBrandContent, DeleteBrandInner, DeleteBrandArrowRotate, _deleteBrandOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  PRODUCT SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void AddProduct_Toggle(object sender, MouseButtonEventArgs e)
        {
            _addProductOpen = !_addProductOpen;
            TogglePanel(AddProductContent, AddProductInner, AddProductArrowRotate, _addProductOpen);
        }

        private void UpdateProduct_Toggle(object sender, MouseButtonEventArgs e)
        {
            _updateProductOpen = !_updateProductOpen;
            TogglePanel(UpdateProductContent, UpdateProductInner, UpdateProductArrowRotate, _updateProductOpen);
        }

        private void DeleteProduct_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteProductOpen = !_deleteProductOpen;
            TogglePanel(DeleteProductContent, DeleteProductInner, DeleteProductArrowRotate, _deleteProductOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  CLIENT SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void AddClient_Toggle(object sender, MouseButtonEventArgs e)
        {
            _addClientOpen = !_addClientOpen;
            TogglePanel(AddClientContent, AddClientInner, AddClientArrowRotate, _addClientOpen);
        }

        private void UpdateClient_Toggle(object sender, MouseButtonEventArgs e)
        {
            _updateClientOpen = !_updateClientOpen;
            TogglePanel(UpdateClientContent, UpdateClientInner, UpdateClientArrowRotate, _updateClientOpen);
        }

        private void DeleteClient_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteClientOpen = !_deleteClientOpen;
            TogglePanel(DeleteClientContent, DeleteClientInner, DeleteClientArrowRotate, _deleteClientOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  NEW SALE SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void HowToSale_Toggle(object sender, MouseButtonEventArgs e)
        {
            _howToSaleOpen = !_howToSaleOpen;
            TogglePanel(HowToSaleContent, HowToSaleInner, HowToSaleArrowRotate, _howToSaleOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  SALE DETAILS SUB-ITEMS
        // ════════════════════════════════════════════════════════════

        private void ViewSales_Toggle(object sender, MouseButtonEventArgs e)
        {
            _viewSalesOpen = !_viewSalesOpen;
            TogglePanel(ViewSalesContent, ViewSalesInner, ViewSalesArrowRotate, _viewSalesOpen);
        }

        private void DeleteSale_Toggle(object sender, MouseButtonEventArgs e)
        {
            _deleteSaleOpen = !_deleteSaleOpen;
            TogglePanel(DeleteSaleContent, DeleteSaleInner, DeleteSaleArrowRotate, _deleteSaleOpen);
        }

        // ════════════════════════════════════════════════════════════
        //  CORE TOGGLE
        // ════════════════════════════════════════════════════════════

        private void TogglePanel(
            Border container,
            FrameworkElement inner,
            RotateTransform arrowRotate,
            bool open)
        {
            var duration = new Duration(TimeSpan.FromMilliseconds(320));
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            if (open)
            {
                inner.Opacity = 0;
                container.Height = double.NaN;

                container.Measure(new Size(
                    container.ActualWidth > 0
                        ? container.ActualWidth
                        : double.PositiveInfinity,
                    double.PositiveInfinity));

                container.UpdateLayout();

                double targetH = container.DesiredSize.Height;
                container.Height = 0;

                var heightAnim = new DoubleAnimation(0, targetH, duration)
                { EasingFunction = ease };

                heightAnim.Completed += (s, _) =>
                {
                    container.BeginAnimation(HeightProperty, null);
                    container.Height = double.NaN;
                };

                container.BeginAnimation(HeightProperty, heightAnim);

                var opacityAnim = new DoubleAnimation(0, 1, duration)
                { EasingFunction = ease };
                inner.BeginAnimation(OpacityProperty, opacityAnim);
            }
            else
            {
                double fromH = container.ActualHeight;
                var heightAnim = new DoubleAnimation(fromH, 0, duration)
                { EasingFunction = ease };
                container.BeginAnimation(HeightProperty, heightAnim);

                var opacityAnim = new DoubleAnimation(1, 0, duration)
                { EasingFunction = ease };
                inner.BeginAnimation(OpacityProperty, opacityAnim);
            }

            var rotateAnim = new DoubleAnimation(
                open ? 0 : -180,
                open ? -180 : 0,
                duration)
            { EasingFunction = ease };
            arrowRotate.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
        }

        // ════════════════════════════════════════════════════════════
        //  SILENT COLLAPSE
        // ════════════════════════════════════════════════════════════

        private void SilentCollapse(Border container, FrameworkElement inner, RotateTransform arrow)
        {
            container.BeginAnimation(HeightProperty, null);
            container.Height = 0;
            inner.BeginAnimation(OpacityProperty, null);
            inner.Opacity = 0;
            arrow.BeginAnimation(RotateTransform.AngleProperty, null);
            arrow.Angle = 0;
        }

        private void CollapseSubItems_Categories()
        {
            if (_addOpen) { SilentCollapse(AddCategoryContent, AddCategoryInner, AddArrowRotate); _addOpen = false; }
            if (_updateOpen) { SilentCollapse(UpdateCategoryContent, UpdateCategoryInner, UpdateArrowRotate); _updateOpen = false; }
            if (_deleteOpen) { SilentCollapse(DeleteCategoryContent, DeleteCategoryInner, DeleteArrowRotate); _deleteOpen = false; }
        }

        private void CollapseSubItems_Models()
        {
            if (_addModelOpen) { SilentCollapse(AddModelContent, AddModelInner, AddModelArrowRotate); _addModelOpen = false; }
            if (_updateModelOpen) { SilentCollapse(UpdateModelContent, UpdateModelInner, UpdateModelArrowRotate); _updateModelOpen = false; }
            if (_deleteModelOpen) { SilentCollapse(DeleteModelContent, DeleteModelInner, DeleteModelArrowRotate); _deleteModelOpen = false; }
        }

        private void CollapseSubItems_Series()
        {
            if (_addSeriesOpen) { SilentCollapse(AddSeriesContent, AddSeriesInner, AddSeriesArrowRotate); _addSeriesOpen = false; }
            if (_updateSeriesOpen) { SilentCollapse(UpdateSeriesContent, UpdateSeriesInner, UpdateSeriesArrowRotate); _updateSeriesOpen = false; }
            if (_deleteSeriesOpen) { SilentCollapse(DeleteSeriesContent, DeleteSeriesInner, DeleteSeriesArrowRotate); _deleteSeriesOpen = false; }
        }

        private void CollapseSubItems_Brands()
        {
            if (_addBrandOpen) { SilentCollapse(AddBrandContent, AddBrandInner, AddBrandArrowRotate); _addBrandOpen = false; }
            if (_updateBrandOpen) { SilentCollapse(UpdateBrandContent, UpdateBrandInner, UpdateBrandArrowRotate); _updateBrandOpen = false; }
            if (_deleteBrandOpen) { SilentCollapse(DeleteBrandContent, DeleteBrandInner, DeleteBrandArrowRotate); _deleteBrandOpen = false; }
        }

        private void CollapseSubItems_Products()
        {
            if (_addProductOpen) { SilentCollapse(AddProductContent, AddProductInner, AddProductArrowRotate); _addProductOpen = false; }
            if (_updateProductOpen) { SilentCollapse(UpdateProductContent, UpdateProductInner, UpdateProductArrowRotate); _updateProductOpen = false; }
            if (_deleteProductOpen) { SilentCollapse(DeleteProductContent, DeleteProductInner, DeleteProductArrowRotate); _deleteProductOpen = false; }
        }

        private void CollapseSubItems_Clients()
        {
            if (_addClientOpen) { SilentCollapse(AddClientContent, AddClientInner, AddClientArrowRotate); _addClientOpen = false; }
            if (_updateClientOpen) { SilentCollapse(UpdateClientContent, UpdateClientInner, UpdateClientArrowRotate); _updateClientOpen = false; }
            if (_deleteClientOpen) { SilentCollapse(DeleteClientContent, DeleteClientInner, DeleteClientArrowRotate); _deleteClientOpen = false; }
        }

        private void CollapseSubItems_NewSale()
        {
            if (_howToSaleOpen) { SilentCollapse(HowToSaleContent, HowToSaleInner, HowToSaleArrowRotate); _howToSaleOpen = false; }
        }

        private void CollapseSubItems_SaleDetails()
        {
            if (_viewSalesOpen) { SilentCollapse(ViewSalesContent, ViewSalesInner, ViewSalesArrowRotate); _viewSalesOpen = false; }
            if (_deleteSaleOpen) { SilentCollapse(DeleteSaleContent, DeleteSaleInner, DeleteSaleArrowRotate); _deleteSaleOpen = false; }
        }
    }
}