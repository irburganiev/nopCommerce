﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Affiliates;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the affiliate model factory implementation
    /// </summary>
    public partial class AffiliateModelFactory : IAffiliateModelFactory
    {
        #region Fields

        private readonly IAffiliateService _affiliateService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public AffiliateModelFactory(IAffiliateService affiliateService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPriceFormatter priceFormatter)
        {
            _affiliateService = affiliateService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
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

            //set all address fields as enabled and required
            model.FirstNameEnabled = true;
            model.FirstNameRequired = true;
            model.LastNameEnabled = true;
            model.LastNameRequired = true;
            model.EmailEnabled = true;
            model.EmailRequired = true;
            model.CompanyEnabled = true;
            model.CountryEnabled = true;
            model.CountryRequired = true;
            model.StateProvinceEnabled = true;
            model.CountyEnabled = true;
            model.CountyRequired = true;
            model.CityEnabled = true;
            model.CityRequired = true;
            model.StreetAddressEnabled = true;
            model.StreetAddressRequired = true;
            model.StreetAddress2Enabled = true;
            model.ZipPostalCodeEnabled = true;
            model.ZipPostalCodeRequired = true;
            model.PhoneEnabled = true;
            model.PhoneRequired = true;
            model.FaxEnabled = true;

            //prepare available countries
            _baseAdminModelFactory.PrepareCountries(model.AvailableCountries);

            //prepare available states
            _baseAdminModelFactory.PrepareStatesAndProvinces(model.AvailableStates, model.CountryId);
        }

        /// <summary>
        /// Prepare affiliated order search model
        /// </summary>
        /// <param name="searchModel">Affiliated order search model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Affiliated order search model</returns>
        protected virtual AffiliatedOrderSearchModel PrepareAffiliatedOrderSearchModel(AffiliatedOrderSearchModel searchModel, Affiliate affiliate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            searchModel.AffliateId = affiliate.Id;

            //prepare available order, payment and shipping statuses
            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);
            _baseAdminModelFactory.PreparePaymentStatuses(searchModel.AvailablePaymentStatuses);
            _baseAdminModelFactory.PrepareShippingStatuses(searchModel.AvailableShippingStatuses);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareAffiliatedOrderGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareAffiliatedOrderGridModel(AffiliatedOrderSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "orders-grid",
                UrlRead = new DataUrl("AffiliatedOrderListGrid", "Affiliate", null),
                SearchButtonId = "search-orders",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes,

                //prepare filters to search
                Filters = new List<FilterParameter>
                {
                    new FilterParameter(nameof(searchModel.AffliateId), searchModel.AffliateId),
                    new FilterParameter(nameof(searchModel.StartDate), nameof(AffiliatedOrderSearchModel)),
                    new FilterParameter(nameof(searchModel.EndDate), nameof(AffiliatedOrderSearchModel)),
                    new FilterParameter(nameof(searchModel.OrderStatusId), nameof(AffiliatedOrderSearchModel)),
                    new FilterParameter(nameof(searchModel.PaymentStatusId), nameof(AffiliatedOrderSearchModel)),
                    new FilterParameter(nameof(searchModel.ShippingStatusId), nameof(AffiliatedOrderSearchModel))
                },

                //prepare model columns
                ColumnCollection = new List<ColumnProperty>
                {
                    new ColumnProperty(nameof(AffiliatedOrderModel.CustomOrderNumber))
                    {
                        Title = _localizationService.GetResource("Admin.Affiliates.Orders.CustomOrderNumber"),
                        Width = "200"
                    },
                    new ColumnProperty(nameof(AffiliatedOrderModel.OrderStatus))
                    {
                        Title = _localizationService.GetResource("Admin.Affiliates.Orders.OrderStatus"),
                        Render = new RenderCustom("renderOrderStatus")
                    },
                    new ColumnProperty(nameof(AffiliatedOrderModel.PaymentStatus))
                    {
                        Title = _localizationService.GetResource("Admin.Affiliates.Orders.PaymentStatus")
                    },
                    new ColumnProperty(nameof(AffiliatedOrderModel.ShippingStatus))
                    {
                        Title = _localizationService.GetResource("Admin.Affiliates.Orders.ShippingStatus")
                    },
                    new ColumnProperty(nameof(AffiliatedOrderModel.OrderTotal))
                    {
                        Title = _localizationService.GetResource("Admin.Affiliates.Orders.OrderTotal")
                    },
                    new ColumnProperty(nameof(AffiliatedOrderModel.CreatedOn))
                    {
                        Title = _localizationService.GetResource("Admin.Affiliates.Orders.CreatedOn"),
                        Render = new RenderDate()
                    },
                    new ColumnProperty(nameof(AffiliatedOrderModel.Id))
                    {
                        Title = _localizationService.GetResource("Admin.Common.View"),
                        Width = "150",
                        ClassName = StyleColumn.CenterAll,
                        Render = new RenderButtonView(new DataUrl("~/Admin/Order/Edit/", nameof(AffiliatedOrderModel.Id)))
                    }
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare affiliated customer search model
        /// </summary>
        /// <param name="searchModel">Affiliated customer search model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Affiliated customer search model</returns>
        protected virtual AffiliatedCustomerSearchModel PrepareAffiliatedCustomerSearchModel(AffiliatedCustomerSearchModel searchModel, Affiliate affiliate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            searchModel.AffliateId = affiliate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareAffiliateGridModel(AffiliateSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "affiliates-grid",
                UrlRead = new DataUrl("List", "Affiliate", null),
                SearchButtonId = "search-affiliates",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchFirstName)),
                new FilterParameter(nameof(searchModel.SearchLastName)),
                new FilterParameter(nameof(searchModel.SearchFriendlyUrlName)),
                new FilterParameter(nameof(searchModel.LoadOnlyWithOrders), typeof(bool)),
                new FilterParameter(nameof(searchModel.OrdersCreatedFromUtc)),
                new FilterParameter(nameof(searchModel.OrdersCreatedToUtc))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty($"{nameof(AffiliateModel.Address)}.{nameof(AddressModel.FirstName)}")
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.FirstName"),
                    Width = "200"
                },
                new ColumnProperty($"{nameof(AffiliateModel.Address)}.{nameof(AddressModel.LastName)}")
                {
                    Title = _localizationService.GetResource("Admin.Address.Fields.LastName"),
                    Width = "200"
                },
                new ColumnProperty(nameof(AffiliateModel.Active))
                {
                    Title = _localizationService.GetResource("Admin.Affiliates.Fields.Active"),
                    Width = "100",
                    Render = new RenderBoolean()
                },
                new ColumnProperty(nameof(AffiliateModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("Edit"))
                }
            };

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare affiliate search model
        /// </summary>
        /// <param name="searchModel">Affiliate search model</param>
        /// <returns>Affiliate search model</returns>
        public virtual AffiliateSearchModel PrepareAffiliateSearchModel(AffiliateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareAffiliateGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged affiliate list model
        /// </summary>
        /// <param name="searchModel">Affiliate search model</param>
        /// <returns>Affiliate list model</returns>
        public virtual AffiliateListModel PrepareAffiliateListModel(AffiliateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get affiliates
            var affiliates = _affiliateService.GetAllAffiliates(searchModel.SearchFriendlyUrlName,
                searchModel.SearchFirstName,
                searchModel.SearchLastName,
                searchModel.LoadOnlyWithOrders,
                searchModel.OrdersCreatedFromUtc,
                searchModel.OrdersCreatedToUtc,
                searchModel.Page - 1, searchModel.PageSize, true);

            //prepare list model
            var model = new AffiliateListModel().PrepareToGrid(searchModel, affiliates, () =>
            {
                //fill in model values from the entity
                return affiliates.Select(affiliate =>
                {
                    var affiliateModel = affiliate.ToModel<AffiliateModel>();
                    affiliateModel.Address = affiliate.Address.ToModel<AddressModel>();
                    affiliateModel.Address.CountryName = affiliate.Address.Country?.Name;
                    affiliateModel.Address.StateProvinceName = affiliate.Address.StateProvince?.Name;

                    return affiliateModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare affiliate model
        /// </summary>
        /// <param name="model">Affiliate model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Affiliate model</returns>
        public virtual AffiliateModel PrepareAffiliateModel(AffiliateModel model, Affiliate affiliate, bool excludeProperties = false)
        {
            //fill in model values from the entity
            if (affiliate != null)
            {
                model = model ?? affiliate.ToModel<AffiliateModel>();
                model.Url = _affiliateService.GenerateUrl(affiliate);

                //prepare nested search models
                PrepareAffiliatedOrderSearchModel(model.AffiliatedOrderSearchModel, affiliate);
                PrepareAffiliatedCustomerSearchModel(model.AffiliatedCustomerSearchModel, affiliate);

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.AdminComment = affiliate.AdminComment;
                    model.FriendlyUrlName = affiliate.FriendlyUrlName;
                    model.Active = affiliate.Active;
                    model.Address = affiliate.Address.ToModel(model.Address);
                }
            }

            //prepare address model
            PrepareAddressModel(model.Address, affiliate?.Address);

            return model;
        }

        /// <summary>
        /// Prepare paged affiliated order list model
        /// </summary>
        /// <param name="searchModel">Affiliated order search model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Affiliated order list model</returns>
        public virtual AffiliatedOrderListModel PrepareAffiliatedOrderListModel(AffiliatedOrderSearchModel searchModel, Affiliate affiliate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            //get parameters to filter orders
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            var orderStatusIds = searchModel.OrderStatusId > 0 ? new List<int> { searchModel.OrderStatusId } : null;
            var paymentStatusIds = searchModel.PaymentStatusId > 0 ? new List<int> { searchModel.PaymentStatusId } : null;
            var shippingStatusIds = searchModel.ShippingStatusId > 0 ? new List<int> { searchModel.ShippingStatusId } : null;

            //get orders
            var orders = _orderService.SearchOrders(createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                affiliateId: affiliate.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new AffiliatedOrderListModel().PrepareToGrid(searchModel, orders, () =>
            {
                //fill in model values from the entity
                return orders.Select(order =>
                {
                    var affiliatedOrderModel = order.ToModel<AffiliatedOrderModel>();

                    //fill in additional values (not existing in the entity)
                    affiliatedOrderModel.OrderStatus = _localizationService.GetLocalizedEnum(order.OrderStatus);
                    affiliatedOrderModel.PaymentStatus = _localizationService.GetLocalizedEnum(order.PaymentStatus);
                    affiliatedOrderModel.ShippingStatus = _localizationService.GetLocalizedEnum(order.ShippingStatus);
                    affiliatedOrderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
                    affiliatedOrderModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);

                    return affiliatedOrderModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged affiliated customer list model
        /// </summary>
        /// <param name="searchModel">Affiliated customer search model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Affiliated customer list model</returns>
        public virtual AffiliatedCustomerListModel PrepareAffiliatedCustomerListModel(AffiliatedCustomerSearchModel searchModel,
            Affiliate affiliate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            //get customers
            var customers = _customerService.GetAllCustomers(affiliateId: affiliate.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new AffiliatedCustomerListModel
            {
                //fill in model values from the entity
                Data = customers.Select(customer =>
                {
                    var affiliatedCustomerModel = customer.ToModel<AffiliatedCustomerModel>();
                    affiliatedCustomerModel.Name = customer.Email;

                    return affiliatedCustomerModel;
                }),
                Total = customers.TotalCount
            };

            return model;
        }

        #endregion
    }
}