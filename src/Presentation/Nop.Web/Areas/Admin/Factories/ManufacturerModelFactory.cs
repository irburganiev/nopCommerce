﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the manufacturer model factory implementation
    /// </summary>
    public partial class ManufacturerModelFactory : IManufacturerModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public ManufacturerModelFactory(CatalogSettings catalogSettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            IManufacturerService manufacturerService,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IProductService productService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IUrlRecordService urlRecordService)
        {
            _catalogSettings = catalogSettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _manufacturerService = manufacturerService;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _productService = productService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare manufacturer product search model
        /// </summary>
        /// <param name="searchModel">Manufacturer product search model</param>
        /// <param name="manufacturer">Manufacturer</param>
        /// <returns>Manufacturer product search model</returns>
        protected virtual ManufacturerProductSearchModel PrepareManufacturerProductSearchModel(ManufacturerProductSearchModel searchModel,
            Manufacturer manufacturer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            searchModel.ManufacturerId = manufacturer.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareManufacturerGridModel(ManufacturerSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "manufacturers-grid",
                UrlRead = new DataUrl("List", "Manufacturer", null),
                SearchButtonId = "search-manufacturers",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchManufacturerName)),
                new FilterParameter(nameof(searchModel.SearchStoreId))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ManufacturerModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Name")
                },
                new ColumnProperty(nameof(ManufacturerModel.Published))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Published"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderBoolean()
                },
                new ColumnProperty(nameof(ManufacturerModel.DisplayOrder))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.DisplayOrder"),
                    Width = "150"
                },
                new ColumnProperty(nameof(ManufacturerModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("Edit"))
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareAddProductGridModel(AddProductToManufacturerSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "products-grid",
                UrlRead = new DataUrl("ProductAddPopupList", "Manufacturer", null),
                SearchButtonId = "search-products",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchProductName)),
                new FilterParameter(nameof(searchModel.SearchCategoryId)),
                new FilterParameter(nameof(searchModel.SearchManufacturerId)),
                new FilterParameter(nameof(searchModel.SearchStoreId)),
                new FilterParameter(nameof(searchModel.SearchVendorId)),
                new FilterParameter(nameof(searchModel.SearchProductTypeId))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ProductModel.Id))
                {
                    IsMasterCheckBox = true,
                    Render = new RenderCheckBox(nameof(AddProductToManufacturerModel.SelectedProductIds)),
                    ClassName =  StyleColumn.CenterAll,
                    Width = "50",
                },
                new ColumnProperty(nameof(ProductModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Products.Fields.Name")
                },
                new ColumnProperty(nameof(ProductModel.Published))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Products.Fields.Published"),
                    Width = "100",
                    Render = new RenderBoolean()
                }
            };

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare manufacturer search model
        /// </summary>
        /// <param name="searchModel">Manufacturer search model</param>
        /// <returns>Manufacturer search model</returns>
        public virtual ManufacturerSearchModel PrepareManufacturerSearchModel(ManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareManufacturerGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged manufacturer list model
        /// </summary>
        /// <param name="searchModel">Manufacturer search model</param>
        /// <returns>Manufacturer list model</returns>
        public virtual ManufacturerListModel PrepareManufacturerListModel(ManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get manufacturers
            var manufacturers = _manufacturerService.GetAllManufacturers(showHidden: true,
                manufacturerName: searchModel.SearchManufacturerName,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new ManufacturerListModel().PrepareToGrid(searchModel, manufacturers, () =>
            {
                //fill in model values from the entity
                return manufacturers.Select(manufacturer =>
                {
                    var manufacturerModel = manufacturer.ToModel<ManufacturerModel>();
                    manufacturerModel.SeName = _urlRecordService.GetSeName(manufacturer, 0, true, false);

                    return manufacturerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare manufacturer model
        /// </summary>
        /// <param name="model">Manufacturer model</param>
        /// <param name="manufacturer">Manufacturer</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Manufacturer model</returns>
        public virtual ManufacturerModel PrepareManufacturerModel(ManufacturerModel model,
            Manufacturer manufacturer, bool excludeProperties = false)
        {
            Action<ManufacturerLocalizedModel, int> localizedModelConfiguration = null;

            if (manufacturer != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = manufacturer.ToModel<ManufacturerModel>();
                    model.SeName = _urlRecordService.GetSeName(manufacturer, 0, true, false);
                }

                //prepare nested search model
                PrepareManufacturerProductSearchModel(model.ManufacturerProductSearchModel, manufacturer);

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(manufacturer, entity => entity.Name, languageId, false, false);
                    locale.Description = _localizationService.GetLocalized(manufacturer, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = _localizationService.GetLocalized(manufacturer, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = _localizationService.GetLocalized(manufacturer, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = _localizationService.GetLocalized(manufacturer, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = _urlRecordService.GetSeName(manufacturer, languageId, false, false);
                };
            }

            //set default values for the new model
            if (manufacturer == null)
            {
                model.PageSize = _catalogSettings.DefaultManufacturerPageSize;
                model.PageSizeOptions = _catalogSettings.DefaultManufacturerPageSizeOptions;
                model.Published = true;
                model.AllowCustomersToSelectPageSize = true;
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare available manufacturer templates
            _baseAdminModelFactory.PrepareManufacturerTemplates(model.AvailableManufacturerTemplates, false);

            //prepare model discounts
            var availableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
            _discountSupportedModelFactory.PrepareModelDiscounts(model, manufacturer, availableDiscounts, excludeProperties);

            //prepare model customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(model, manufacturer, excludeProperties);

            //prepare model stores
            _storeMappingSupportedModelFactory.PrepareModelStores(model, manufacturer, excludeProperties);

            return model;
        }

        /// <summary>
        /// Prepare paged manufacturer product list model
        /// </summary>
        /// <param name="searchModel">Manufacturer product search model</param>
        /// <param name="manufacturer">Manufacturer</param>
        /// <returns>Manufacturer product list model</returns>
        public virtual ManufacturerProductListModel PrepareManufacturerProductListModel(ManufacturerProductSearchModel searchModel,
            Manufacturer manufacturer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            //get product manufacturers
            var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(showHidden: true,
                manufacturerId: manufacturer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new ManufacturerProductListModel
            {
                Data = productManufacturers.Select(productManufacturer =>
                {
                    //fill in model values from the entity
                    var manufacturerProductModel = productManufacturer.ToModel<ManufacturerProductModel>();

                    //fill in additional values (not existing in the entity)
                    manufacturerProductModel.ProductName = _productService.GetProductById(productManufacturer.ProductId)?.Name;

                    return manufacturerProductModel;
                }),
                Total = productManufacturers.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare product search model to add to the manufacturer
        /// </summary>
        /// <param name="searchModel">Product search model to add to the manufacturer</param>
        /// <returns>Product search model to add to the manufacturer</returns>
        public virtual AddProductToManufacturerSearchModel PrepareAddProductToManufacturerSearchModel(AddProductToManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available manufacturers
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available product types
            _baseAdminModelFactory.PrepareProductTypes(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();
            searchModel.Grid = PrepareAddProductGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product list model to add to the manufacturer
        /// </summary>
        /// <param name="searchModel">Product search model to add to the manufacturer</param>
        /// <returns>Product list model to add to the manufacturer</returns>
        public virtual AddProductToManufacturerListModel PrepareAddProductToManufacturerListModel(AddProductToManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = _productService.SearchProducts(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddProductToManufacturerListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        #endregion
    }
}