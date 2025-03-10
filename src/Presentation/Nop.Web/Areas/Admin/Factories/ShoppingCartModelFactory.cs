﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.ShoppingCart;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the shopping cart model factory implementation
    /// </summary>
    public partial class ShoppingCartModelFactory : IShoppingCartModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;

        #endregion

        #region Ctor

        public ShoppingCartModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IShoppingCartService shoppingCartService,
            IStoreService storeService,
            ITaxService taxService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _shoppingCartService = shoppingCartService;
            _storeService = storeService;
            _taxService = taxService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare shopping cart item search model
        /// </summary>
        /// <param name="searchModel">Shopping cart item search model</param>
        /// <returns>Shopping cart item search model</returns>
        protected virtual ShoppingCartItemSearchModel PrepareShoppingCartItemSearchModel(ShoppingCartItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareShoppingCartGridModel(ShoppingCartSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "carts-grid",
                UrlRead = new DataUrl("CurrentCarts", "ShoppingCart", null),
                SearchButtonId = "search-shopping-carts",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.ShoppingCartType)),
                new FilterParameter(nameof(searchModel.StartDate)),
                new FilterParameter(nameof(searchModel.EndDate)),
                new FilterParameter(nameof(searchModel.StoreId)),
                new FilterParameter(nameof(searchModel.BillingCountryId)),
                new FilterParameter(nameof(searchModel.ProductId))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(null)
                {
                    Render = new RenderChildCaret(),
                    Width = "5",
                    Searchable = false,
                    ClassName =  StyleColumn.ChildControl
                },
                new ColumnProperty(nameof(ShoppingCartModel.CustomerEmail))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.Customer"),
                    Render = new RenderLink(new DataUrl("~/Admin/Customer/Edit/", nameof(ShoppingCartModel.CustomerId)))
                },
                new ColumnProperty(nameof(ShoppingCartModel.TotalItems))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.TotalItems"),
                    Width = "150"
                }
            };

            //prepare common properties for detail table
            var detailModel = new DataTablesModel
            {
                Name = "carts-grid",
                UrlRead = new DataUrl("GetCartDetails", "ShoppingCart", null),
                UrlDelete = new DataUrl("DeleteItem", "ShoppingCart", null),
                IsChildTable = true,
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            detailModel.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(ShoppingCartItemSearchModel.CustomerId), nameof(ShoppingCartItemSearchModel.CustomerId), true),
                new FilterParameter(nameof(ShoppingCartItemSearchModel.ShoppingCartType)),
                new FilterParameter(nameof(ShoppingCartItemSearchModel.StartDate)),
                new FilterParameter(nameof(ShoppingCartItemSearchModel.EndDate)),
                new FilterParameter(nameof(ShoppingCartItemSearchModel.StoreId)),
                new FilterParameter(nameof(ShoppingCartItemSearchModel.BillingCountryId)),
                new FilterParameter(nameof(ShoppingCartItemSearchModel.ProductId))
            };

            var stores = _storeService.GetAllStores();

            detailModel.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ShoppingCartItemModel.ProductName))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.Product"),
                    Width = "400",
                    Render = new RenderCustom("renderProductName")
                },
                new ColumnProperty(nameof(ShoppingCartItemModel.Quantity))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.Quantity"),
                    Width = "150"
                },
                new ColumnProperty(nameof(ShoppingCartItemModel.UnitPrice))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.UnitPrice"),
                    Width = "150"
                },
                new ColumnProperty(nameof(ShoppingCartItemModel.Total))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.Total"),
                    Width = "150"
                },
                new ColumnProperty(nameof(ShoppingCartItemModel.Store))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.Store"),
                    Width = "150",
                    Visible = stores.Count > 1
                },
                new ColumnProperty(nameof(ShoppingCartItemModel.UpdatedOn))
                {
                    Title = _localizationService.GetResource("Admin.CurrentCarts.UpdatedOn"),
                    Width = "150",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(ShoppingCartItemModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Delete"),
                    Width = "100",
                    Render = new RenderButtonRemove(_localizationService.GetResource("Admin.Common.Delete")) { Style = StyleButton.Default },
                    ClassName =  StyleColumn.CenterAll
                }
            };

            model.ChildTable = detailModel;

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare shopping cart search model
        /// </summary>
        /// <param name="searchModel">Shopping cart search model</param>
        /// <returns>Shopping cart search model</returns>
        public virtual ShoppingCartSearchModel PrepareShoppingCartSearchModel(ShoppingCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available shopping cart types
            _baseAdminModelFactory.PrepareShoppingCartTypes(searchModel.AvailableShoppingCartTypes, false);

            //set default search values
            searchModel.ShoppingCartType = ShoppingCartType.ShoppingCart;

            //prepare available billing countries
            searchModel.AvailableCountries = _countryService.GetAllCountriesForBilling(showHidden: true)
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare nested search model
            PrepareShoppingCartItemSearchModel(searchModel.ShoppingCartItemSearchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareShoppingCartGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged shopping cart list model
        /// </summary>
        /// <param name="searchModel">Shopping cart search model</param>
        /// <returns>Shopping cart list model</returns>
        public virtual ShoppingCartListModel PrepareShoppingCartListModel(ShoppingCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get customers with shopping carts
            var customers = _customerService.GetCustomersWithShoppingCarts(searchModel.ShoppingCartType,
                storeId: searchModel.StoreId,
                productId: searchModel.ProductId,
                createdFromUtc: searchModel.StartDate,
                createdToUtc: searchModel.EndDate,
                countryId: searchModel.BillingCountryId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new ShoppingCartListModel().PrepareToGrid(searchModel, customers, () =>
            {
                return customers.Select(customer =>
                {
                    //fill in model values from the entity
                    var shoppingCartModel = new ShoppingCartModel
                    {
                        CustomerId = customer.Id
                    };

                    //fill in additional values (not existing in the entity)
                    shoppingCartModel.CustomerEmail = customer.IsRegistered()
                        ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    shoppingCartModel.TotalItems = _shoppingCartService.GetShoppingCart(customer, searchModel.ShoppingCartType,
                        searchModel.StoreId, searchModel.ProductId, searchModel.StartDate, searchModel.EndDate).Sum(item => item.Quantity);

                    return shoppingCartModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged shopping cart item list model
        /// </summary>
        /// <param name="searchModel">Shopping cart item search model</param>
        /// <param name="customer">Customer</param>
        /// <returns>Shopping cart item list model</returns>
        public virtual ShoppingCartItemListModel PrepareShoppingCartItemListModel(ShoppingCartItemSearchModel searchModel, Customer customer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //get shopping cart items
            var items = _shoppingCartService.GetShoppingCart(customer, searchModel.ShoppingCartType,
                searchModel.StoreId, searchModel.ProductId, searchModel.StartDate, searchModel.EndDate).ToPagedList(searchModel);

            //prepare list model
            var model = new ShoppingCartItemListModel().PrepareToGrid(searchModel, items, () =>
            {
                return items.Select(item =>
                {
                    //fill in model values from the entity
                    var itemModel = item.ToModel<ShoppingCartItemModel>();

                    //convert dates to the user time
                    itemModel.UpdatedOn = _dateTimeHelper.ConvertToUserTime(item.UpdatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    itemModel.Store = _storeService.GetStoreById(item.StoreId)?.Name ?? "Deleted";
                    itemModel.AttributeInfo = _productAttributeFormatter.FormatAttributes(item.Product, item.AttributesXml, item.Customer);
                    var unitPrice = _priceCalculationService.GetUnitPrice(item);
                    itemModel.UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(item.Product, unitPrice, out var _));
                    var subTotal = _priceCalculationService.GetSubTotal(item);
                    itemModel.Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(item.Product, subTotal, out _));

                    //set product name since it does not survive mapping
                    itemModel.ProductName = item?.Product?.Name;

                    return itemModel;
                });
            });

            return model;
        }

        #endregion
    }
}