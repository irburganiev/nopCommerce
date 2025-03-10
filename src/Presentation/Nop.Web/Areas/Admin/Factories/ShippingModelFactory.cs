﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Shipping.Pickup;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Directory;
using Nop.Web.Areas.Admin.Models.Shipping;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the shipping model factory implementation
    /// </summary>
    public partial class ShippingModelFactory : IShippingModelFactory
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPickupPluginManager _pickupPluginManager;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IShippingService _shippingService;

        #endregion

        #region Ctor

        public ShippingModelFactory(IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            IDateRangeService dateRangeService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IPickupPluginManager pickupPluginManager,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService)
        {
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _dateRangeService = dateRangeService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _pickupPluginManager = pickupPluginManager;
            _shippingPluginManager = shippingPluginManager;
            _shippingService = shippingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        protected virtual void PrepareAddressModel(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //set some of address fields as enabled and required
            model.CountryEnabled = true;
            model.CountryRequired = true;
            model.StateProvinceEnabled = true;
            model.CountyEnabled = true;
            model.CityEnabled = true;
            model.StreetAddressEnabled = true;
            model.ZipPostalCodeEnabled = true;
            model.ZipPostalCodeRequired = true;
            model.PhoneEnabled = true;

            //prepare available countries
            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            //prepare available states
            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);
        }

        /// <summary>
        /// Prepare delivery date search model
        /// </summary>
        /// <param name="searchModel">Delivery date search model</param>
        /// <returns>Delivery date search model</returns>
        protected virtual DeliveryDateSearchModel PrepareDeliveryDateSearchModel(DeliveryDateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareDeliveryDateGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare product availability range search model
        /// </summary>
        /// <param name="searchModel">Product availability range search model</param>
        /// <returns>Product availability range search model</returns>
        protected virtual ProductAvailabilityRangeSearchModel PrepareProductAvailabilityRangeSearchModel(ProductAvailabilityRangeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareProductAvailabilityRangeGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareWarehouseGridModel(WarehouseSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "warehouse-grid",
                UrlRead = new DataUrl("Warehouses", "Shipping", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };
            
            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(WarehouseModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.Warehouses.Fields.Name")
                },
                
                new ColumnProperty(nameof(WarehouseModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("EditWarehouse"))
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareDeliveryDateGridModel(DeliveryDateSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "deliverydate-grid",
                UrlRead = new DataUrl("DeliveryDates", "Shipping", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };
            
            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(DeliveryDateModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Fields.Name")
                },
                new ColumnProperty(nameof(DeliveryDateModel.DisplayOrder))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.DeliveryDates.Fields.DisplayOrder"),
                    Width = "150"
                },
                new ColumnProperty(nameof(DeliveryDateModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("EditDeliveryDate"))
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareProductAvailabilityRangeGridModel(ProductAvailabilityRangeSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "productavailabilityrange-grid",
                UrlRead = new DataUrl("ProductAvailabilityRanges", "Shipping", null),
                SearchButtonId = "search-categories",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ProductAvailabilityRangeModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.ProductAvailabilityRanges.Fields.Name")
                },
                new ColumnProperty(nameof(ProductAvailabilityRangeModel.DisplayOrder))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.ProductAvailabilityRanges.Fields.DisplayOrder"),
                    Width = "150"
                },
                new ColumnProperty(nameof(ProductAvailabilityRangeModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("EditProductAvailabilityRange"))
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareShippingMethodGridModel(ShippingMethodSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "shippingmethod-grid",
                UrlRead = new DataUrl("Methods", "Shipping", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ShippingMethodModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.Name"),
                    Width = "200"
                },
                new ColumnProperty(nameof(ShippingMethodModel.Description))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.Description"),
                    Width = "400"
                },
                new ColumnProperty(nameof(ShippingMethodModel.DisplayOrder))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.DisplayOrder"),
                    Width = "100"
                },
                new ColumnProperty(nameof(ShippingMethodModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("EditMethod"))
                }
            };

            return model;
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Prepare shipping provider search model
        /// </summary>
        /// <param name="searchModel">Shipping provider search model</param>
        /// <returns>Shipping provider search model</returns>
        public virtual ShippingProviderSearchModel PrepareShippingProviderSearchModel(ShippingProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged shipping provider list model
        /// </summary>
        /// <param name="searchModel">Shipping provider search model</param>
        /// <returns>Shipping provider list model</returns>
        public virtual ShippingProviderListModel PrepareShippingProviderListModel(ShippingProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get shipping providers
            var shippingProviders = _shippingPluginManager.LoadAllPlugins().ToPagedList(searchModel);

            //prepare grid model
            var model = new ShippingProviderListModel
            {
                Data = shippingProviders.Select(provider =>
                {
                    //fill in model values from the entity
                    var shippingProviderModel = provider.ToPluginModel<ShippingProviderModel>();

                    //fill in additional values (not existing in the entity)
                    shippingProviderModel.IsActive = _shippingPluginManager.IsPluginActive(provider);
                    shippingProviderModel.ConfigurationUrl = provider.GetConfigurationPageUrl();
                    shippingProviderModel.LogoUrl = _shippingPluginManager.GetPluginLogoUrl(provider);

                    return shippingProviderModel;
                }),
                Total = shippingProviders.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare pickup point provider search model
        /// </summary>
        /// <param name="searchModel">Pickup point provider search model</param>
        /// <returns>Pickup point provider search model</returns>
        public virtual PickupPointProviderSearchModel PreparePickupPointProviderSearchModel(PickupPointProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged pickup point provider list model
        /// </summary>
        /// <param name="searchModel">Pickup point provider search model</param>
        /// <returns>Pickup point provider list model</returns>
        public virtual PickupPointProviderListModel PreparePickupPointProviderListModel(PickupPointProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get pickup point providers
            var pickupPointProviders = _pickupPluginManager.LoadAllPlugins().ToPagedList(searchModel);

            //prepare grid model
            var model = new PickupPointProviderListModel
            {
                Data = pickupPointProviders.Select(provider =>
                {
                    //fill in model values from the entity
                    var pickupPointProviderModel = provider.ToPluginModel<PickupPointProviderModel>();

                    //fill in additional values (not existing in the entity)
                    pickupPointProviderModel.IsActive = _pickupPluginManager.IsPluginActive(provider);
                    pickupPointProviderModel.ConfigurationUrl = provider.GetConfigurationPageUrl();
                    pickupPointProviderModel.LogoUrl = _pickupPluginManager.GetPluginLogoUrl(provider);

                    return pickupPointProviderModel;
                }),
                Total = pickupPointProviders.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare shipping method search model
        /// </summary>
        /// <param name="searchModel">Shipping method search model</param>
        /// <returns>Shipping method search model</returns>
        public virtual ShippingMethodSearchModel PrepareShippingMethodSearchModel(ShippingMethodSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareShippingMethodGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged shipping method list model
        /// </summary>
        /// <param name="searchModel">Shipping method search model</param>
        /// <returns>Shipping method list model</returns>
        public virtual ShippingMethodListModel PrepareShippingMethodListModel(ShippingMethodSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get shipping methods
            var shippingMethods = _shippingService.GetAllShippingMethods().ToPagedList(searchModel);

            //prepare grid model
            var model = new ShippingMethodListModel().PrepareToGrid(searchModel, shippingMethods, () =>
            {
                return shippingMethods.Select(method => method.ToModel<ShippingMethodModel>());
            });

            return model;
        }

        /// <summary>
        /// Prepare shipping method model
        /// </summary>
        /// <param name="model">Shipping method model</param>
        /// <param name="shippingMethod">Shipping method</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Shipping method model</returns>
        public virtual ShippingMethodModel PrepareShippingMethodModel(ShippingMethodModel model,
            ShippingMethod shippingMethod, bool excludeProperties = false)
        {
            Action<ShippingMethodLocalizedModel, int> localizedModelConfiguration = null;

            if (shippingMethod != null)
            {
                //fill in model values from the entity
                model = model ?? shippingMethod.ToModel<ShippingMethodModel>();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(shippingMethod, entity => entity.Name, languageId, false, false);
                    locale.Description = _localizationService.GetLocalized(shippingMethod, entity => entity.Description, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare dates and ranges search model
        /// </summary>
        /// <param name="searchModel">Dates and ranges search model</param>
        /// <returns>Dates and ranges search model</returns>
        public virtual DatesRangesSearchModel PrepareDatesRangesSearchModel(DatesRangesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare nested search models
            PrepareDeliveryDateSearchModel(searchModel.DeliveryDateSearchModel);
            PrepareProductAvailabilityRangeSearchModel(searchModel.ProductAvailabilityRangeSearchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged delivery date list model
        /// </summary>
        /// <param name="searchModel">Delivery date search model</param>
        /// <returns>Delivery date list model</returns>
        public virtual DeliveryDateListModel PrepareDeliveryDateListModel(DeliveryDateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get delivery dates
            var deliveryDates = _dateRangeService.GetAllDeliveryDates().ToPagedList(searchModel);

            //prepare grid model
            var model = new DeliveryDateListModel().PrepareToGrid(searchModel, deliveryDates, () =>
            {
                //fill in model values from the entity
                return deliveryDates.Select(date => date.ToModel<DeliveryDateModel>());
            });

            return model;
        }

        /// <summary>
        /// Prepare delivery date model
        /// </summary>
        /// <param name="model">Delivery date model</param>
        /// <param name="deliveryDate">Delivery date</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Delivery date model</returns>
        public virtual DeliveryDateModel PrepareDeliveryDateModel(DeliveryDateModel model, DeliveryDate deliveryDate, bool excludeProperties = false)
        {
            Action<DeliveryDateLocalizedModel, int> localizedModelConfiguration = null;

            if (deliveryDate != null)
            {
                //fill in model values from the entity
                model = model ?? deliveryDate.ToModel<DeliveryDateModel>();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(deliveryDate, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare paged product availability range list model
        /// </summary>
        /// <param name="searchModel">Product availability range search model</param>
        /// <returns>Product availability range list model</returns>
        public virtual ProductAvailabilityRangeListModel PrepareProductAvailabilityRangeListModel(ProductAvailabilityRangeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get product availability ranges
            var productAvailabilityRanges = _dateRangeService.GetAllProductAvailabilityRanges().ToPagedList(searchModel);

            //prepare grid model
            var model = new ProductAvailabilityRangeListModel().PrepareToGrid(searchModel, productAvailabilityRanges, () =>
            {
                //fill in model values from the entity
                return productAvailabilityRanges.Select(range => range.ToModel<ProductAvailabilityRangeModel>());
            });

            return model;
        }

        /// <summary>
        /// Prepare product availability range model
        /// </summary>
        /// <param name="model">Product availability range model</param>
        /// <param name="productAvailabilityRange">Product availability range</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Product availability range model</returns>
        public virtual ProductAvailabilityRangeModel PrepareProductAvailabilityRangeModel(ProductAvailabilityRangeModel model,
            ProductAvailabilityRange productAvailabilityRange, bool excludeProperties = false)
        {
            Action<ProductAvailabilityRangeLocalizedModel, int> localizedModelConfiguration = null;

            if (productAvailabilityRange != null)
            {
                //fill in model values from the entity
                model = model ?? productAvailabilityRange.ToModel<ProductAvailabilityRangeModel>();

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(productAvailabilityRange, entity => entity.Name, languageId, false, false);
                };
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare warehouse search model
        /// </summary>
        /// <param name="searchModel">Warehouse search model</param>
        /// <returns>Warehouse search model</returns>
        public virtual WarehouseSearchModel PrepareWarehouseSearchModel(WarehouseSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareWarehouseGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged warehouse list model
        /// </summary>
        /// <param name="searchModel">Warehouse search model</param>
        /// <returns>Warehouse list model</returns>
        public virtual WarehouseListModel PrepareWarehouseListModel(WarehouseSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get warehouses
            var warehouses = _shippingService.GetAllWarehouses().ToPagedList(searchModel);

            //prepare list model
            var model = new WarehouseListModel().PrepareToGrid(searchModel, warehouses, () =>
            {
                //fill in model values from the entity
                return warehouses.Select(warehouse => warehouse.ToModel<WarehouseModel>());
            });

            return model;
        }

        /// <summary>
        /// Prepare warehouse model
        /// </summary>
        /// <param name="model">Warehouse model</param>
        /// <param name="warehouse">Warehouse</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Warehouse model</returns>
        public virtual WarehouseModel PrepareWarehouseModel(WarehouseModel model, Warehouse warehouse, bool excludeProperties = false)
        {
            if (warehouse != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = warehouse.ToModel<WarehouseModel>();
                }
            }

            //prepare address model
            var address = _addressService.GetAddressById(warehouse?.AddressId ?? 0);
            if (!excludeProperties && address != null)
                model.Address = address.ToModel(model.Address);
            PrepareAddressModel(model.Address, address);

            return model;
        }

        /// <summary>
        /// Prepare shipping method restriction model
        /// </summary>
        /// <param name="model">Shipping method restriction model</param>
        /// <returns>Shipping method restriction model</returns>
        public virtual ShippingMethodRestrictionModel PrepareShippingMethodRestrictionModel(ShippingMethodRestrictionModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var countries = _countryService.GetAllCountries(showHidden: true);
            model.AvailableCountries = countries.Select(country =>
            {
                var countryModel = country.ToModel<CountryModel>();
                countryModel.NumberOfStates = country.StateProvinces?.Count ?? 0;

                return countryModel;
            }).ToList();

            foreach (var shippingMethod in _shippingService.GetAllShippingMethods())
            {
                model.AvailableShippingMethods.Add(shippingMethod.ToModel<ShippingMethodModel>());
                foreach (var country in countries)
                {
                    if (!model.Restricted.ContainsKey(country.Id))
                        model.Restricted[country.Id] = new Dictionary<int, bool>();

                    model.Restricted[country.Id][shippingMethod.Id] = _shippingService.CountryRestrictionExists(shippingMethod, country.Id);
                }
            }

            return model;
        }

        #endregion
    }
}