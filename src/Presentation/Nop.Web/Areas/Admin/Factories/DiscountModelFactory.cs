﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Discounts;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the discount model factory implementation
    /// </summary>
    public partial class DiscountModelFactory : IDiscountModelFactory
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountPluginManager _discountPluginManager;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public DiscountModelFactory(CurrencySettings currencySettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IDiscountPluginManager discountPluginManager,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper)
        {
            _currencySettings = currencySettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _discountPluginManager = discountPluginManager;
            _discountService = discountService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare discount usage history search model
        /// </summary>
        /// <param name="searchModel">Discount usage history search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount usage history search model</returns>
        protected virtual DiscountUsageHistorySearchModel PrepareDiscountUsageHistorySearchModel(DiscountUsageHistorySearchModel searchModel,
            Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            searchModel.DiscountId = discount.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareDiscountProductGridModel(DiscountProductSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "products-grid",
                UrlRead = new DataUrl("ProductList", "Discount", new RouteValueDictionary { [nameof(searchModel.DiscountId)] = searchModel.DiscountId }),
                UrlDelete = new DataUrl("ProductDelete", "Discount", new RouteValueDictionary { [nameof(searchModel.DiscountId)] = searchModel.DiscountId }),
                BindColumnNameActionDelete = nameof(DiscountProductModel.ProductId),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(DiscountProductModel.ProductName))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.AppliedToProducts.Product")
                },
                new ColumnProperty(nameof(DiscountProductModel.ProductId))
                {
                    Title = _localizationService.GetResource("Admin.Common.View"),
                    Width = "150",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonView(new DataUrl("~/Admin/Product/Edit/"))
                },
                new ColumnProperty(nameof(DiscountProductModel.ProductId))
                {
                    Title = _localizationService.GetResource("Admin.Common.Delete"),
                    Width = "150",
                    Render = new RenderButtonRemove(_localizationService.GetResource("Admin.Common.Delete")) { Style = StyleButton.Default },
                    ClassName =  StyleColumn.CenterAll
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare discount product search model
        /// </summary>
        /// <param name="searchModel">Discount product search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount product search model</returns>
        protected virtual DiscountProductSearchModel PrepareDiscountProductSearchModel(DiscountProductSearchModel searchModel, Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            searchModel.DiscountId = discount.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareDiscountProductGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareDiscountCategoryGridModel(DiscountCategorySearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "categories-grid",
                UrlRead = new DataUrl("CategoryList", "Discount", new RouteValueDictionary { [nameof(searchModel.DiscountId)] = searchModel.DiscountId }),
                UrlDelete = new DataUrl("CategoryDelete", "Discount", new RouteValueDictionary { [nameof(searchModel.DiscountId)] = searchModel.DiscountId }),
                BindColumnNameActionDelete = nameof(DiscountCategoryModel.CategoryId),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(DiscountCategoryModel.CategoryName))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.AppliedToCategories.Category")
                },
                new ColumnProperty(nameof(DiscountCategoryModel.CategoryId))
                {
                    Title = _localizationService.GetResource("Admin.Common.View"),
                    Width = "150",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonView(new DataUrl("~/Admin/Category/Edit/"))
                },
                new ColumnProperty(nameof(DiscountCategoryModel.CategoryId))
                {
                    Title = _localizationService.GetResource("Admin.Common.Delete"),
                    Width = "150",
                    Render = new RenderButtonRemove(_localizationService.GetResource("Admin.Common.Delete")) { Style = StyleButton.Default },
                    ClassName =  StyleColumn.CenterAll
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare discount category search model
        /// </summary>
        /// <param name="searchModel">Discount category search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount category search model</returns>
        protected virtual DiscountCategorySearchModel PrepareDiscountCategorySearchModel(DiscountCategorySearchModel searchModel, Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            searchModel.DiscountId = discount.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareDiscountCategoryGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareDiscountManufacturerGridModel(DiscountManufacturerSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "manufacturers-grid",
                UrlRead = new DataUrl("ManufacturerList", "Discount", new RouteValueDictionary { [nameof(searchModel.DiscountId)] = searchModel.DiscountId }),
                UrlDelete = new DataUrl("ManufacturerDelete", "Discount", new RouteValueDictionary { [nameof(searchModel.DiscountId)] = searchModel.DiscountId }),
                BindColumnNameActionDelete = nameof(DiscountManufacturerModel.ManufacturerId),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(DiscountManufacturerModel.ManufacturerName))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.AppliedToManufacturers.Manufacturer")
                },
                new ColumnProperty(nameof(DiscountManufacturerModel.ManufacturerId))
                {
                    Title = _localizationService.GetResource("Admin.Common.View"),
                    Width = "150",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderButtonView(new DataUrl("~/Admin/Manufacturer/Edit/"))
                },
                new ColumnProperty(nameof(DiscountManufacturerModel.ManufacturerId))
                {
                    Title = _localizationService.GetResource("Admin.Common.Delete"),
                    Width = "150",
                    Render = new RenderButtonRemove(_localizationService.GetResource("Admin.Common.Delete")) { Style = StyleButton.Default },
                    ClassName =  StyleColumn.CenterAll
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare discount manufacturer search model
        /// </summary>
        /// <param name="searchModel">Discount manufacturer search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount manufacturer search model</returns>
        protected virtual DiscountManufacturerSearchModel PrepareDiscountManufacturerSearchModel(DiscountManufacturerSearchModel searchModel,
            Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            searchModel.DiscountId = discount.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareDiscountManufacturerGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareDiscountGridModel(DiscountSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "discounts-grid",
                UrlRead = new DataUrl("List", "Discount", null),
                SearchButtonId = "search-discounts",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchDiscountTypeId)),
                new FilterParameter(nameof(searchModel.SearchDiscountCouponCode)),
                new FilterParameter(nameof(searchModel.SearchDiscountName)),
                new FilterParameter(nameof(searchModel.SearchStartDate)),
                new FilterParameter(nameof(searchModel.SearchEndDate))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(DiscountModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.Fields.Name"),
                    Width = "350"
                },
                new ColumnProperty(nameof(DiscountModel.DiscountTypeName))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.Fields.DiscountType"),
                    Width = "250"
                },
                new ColumnProperty(nameof(DiscountModel.DiscountAmount))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.Fields.Discount"),
                    Width = "200",
                    Render = new RenderCustom("renderColumnDiscount")
                },
                new ColumnProperty(nameof(DiscountModel.StartDateUtc))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.Fields.StartDate"),
                    Width = "200",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(DiscountModel.EndDateUtc))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.Fields.EndDate"),
                    Width = "200",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(DiscountModel.TimesUsed))
                {
                    Title = _localizationService.GetResource("Admin.Promotions.Discounts.Fields.TimesUsed"),
                    Width = "200"
                },
                new ColumnProperty(nameof(DiscountModel.Id))
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
        protected virtual DataTablesModel PrepareAddProductGridModel(AddProductToDiscountSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "products-grid",
                UrlRead = new DataUrl("ProductAddPopupList", "Discount", null),
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
                    Render = new RenderCheckBox(nameof(AddProductToDiscountModel.SelectedProductIds)),
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

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareAddCategoryGridModel(AddCategoryToDiscountSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "categories-grid",
                UrlRead = new DataUrl("CategoryAddPopupList", "Discount", null),
                SearchButtonId = "search-categories",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchCategoryName))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(CategoryModel.Id))
                {
                    IsMasterCheckBox = true,
                    Render = new RenderCheckBox(nameof(AddCategoryToDiscountModel.SelectedCategoryIds)),
                    ClassName =  StyleColumn.CenterAll,
                    Width = "50",
                },
                new ColumnProperty(nameof(CategoryModel.Breadcrumb))
                {
                    Title = _localizationService.GetResource("")
                },
                new ColumnProperty(nameof(CategoryModel.Published))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Categories.Fields.Published"),
                    Width = "100",
                    Render = new RenderBoolean()
                }
            };

            return model;
        }

        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareAddManufacturerGridModel(AddManufacturerToDiscountSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "manufacturers-grid",
                UrlRead = new DataUrl("ManufacturerAddPopupList", "Discount", null),
                SearchButtonId = "search-manufacturers",
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            model.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(searchModel.SearchManufacturerName))
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ManufacturerModel.Id))
                {
                    IsMasterCheckBox = true,
                    Render = new RenderCheckBox(nameof(AddManufacturerToDiscountModel.SelectedManufacturerIds)),
                    ClassName =  StyleColumn.CenterAll,
                    Width = "50",
                },
                new ColumnProperty(nameof(ManufacturerModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Name")
                },
                new ColumnProperty(nameof(ManufacturerModel.Published))
                {
                    Title = _localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Published"),
                    Width = "100",
                    Render = new RenderBoolean()
                }
            };

            return model;
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Prepare discount search model
        /// </summary>
        /// <param name="searchModel">Discount search model</param>
        /// <returns>Discount search model</returns>
        public virtual DiscountSearchModel PrepareDiscountSearchModel(DiscountSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available discount types
            _baseAdminModelFactory.PrepareDiscountTypes(searchModel.AvailableDiscountTypes);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareDiscountGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged discount list model
        /// </summary>
        /// <param name="searchModel">Discount search model</param>
        /// <returns>Discount list model</returns>
        public virtual DiscountListModel PrepareDiscountListModel(DiscountSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter discounts
            var discountType = searchModel.SearchDiscountTypeId > 0 ? (DiscountType?)searchModel.SearchDiscountTypeId : null;
            var startDateUtc = searchModel.SearchStartDate.HasValue ?
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone) : null;
            var endDateUtc = searchModel.SearchEndDate.HasValue ?
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1) : null;

            //get discounts
            var discounts = _discountService.GetAllDiscounts(showHidden: true,
                discountType: discountType,
                couponCode: searchModel.SearchDiscountCouponCode,
                discountName: searchModel.SearchDiscountName,
                startDateUtc: startDateUtc,
                endDateUtc: endDateUtc).ToPagedList(searchModel);

            //prepare list model
            var model = new DiscountListModel().PrepareToGrid(searchModel, discounts, () =>
            {
                return discounts.Select(discount =>
                {
                    //fill in model values from the entity
                    var discountModel = discount.ToModel<DiscountModel>();

                    //fill in additional values (not existing in the entity)
                    discountModel.DiscountTypeName = _localizationService.GetLocalizedEnum(discount.DiscountType);
                    discountModel.PrimaryStoreCurrencyCode = _currencyService
                        .GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)?.CurrencyCode;
                    discountModel.TimesUsed = _discountService.GetAllDiscountUsageHistory(discount.Id, pageSize: 1).TotalCount;

                    return discountModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare discount model
        /// </summary>
        /// <param name="model">Discount model</param>
        /// <param name="discount">Discount</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Discount model</returns>
        public virtual DiscountModel PrepareDiscountModel(DiscountModel model, Discount discount, bool excludeProperties = false)
        {
            if (discount != null)
            {
                //fill in model values from the entity
                model = model ?? discount.ToModel<DiscountModel>();

                //prepare available discount requirement rules
                var discountRules = _discountPluginManager.LoadAllPlugins();
                foreach (var discountRule in discountRules)
                {
                    model.AvailableDiscountRequirementRules.Add(new SelectListItem
                    {
                        Text = discountRule.PluginDescriptor.FriendlyName,
                        Value = discountRule.PluginDescriptor.SystemName
                    });
                }

                model.AvailableDiscountRequirementRules.Insert(0, new SelectListItem
                {
                    Text = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.AddGroup"),
                    Value = "AddGroup"
                });

                model.AvailableDiscountRequirementRules.Insert(0, new SelectListItem
                {
                    Text = _localizationService.GetResource("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.Select"),
                    Value = string.Empty
                });

                //prepare available requirement groups
                var requirementGroups = discount.DiscountRequirements.Where(requirement => requirement.IsGroup);
                model.AvailableRequirementGroups = requirementGroups.Select(requirement =>
                    new SelectListItem { Value = requirement.Id.ToString(), Text = requirement.DiscountRequirementRuleSystemName }).ToList();

                //prepare nested search models
                PrepareDiscountUsageHistorySearchModel(model.DiscountUsageHistorySearchModel, discount);
                PrepareDiscountProductSearchModel(model.DiscountProductSearchModel, discount);
                PrepareDiscountCategorySearchModel(model.DiscountCategorySearchModel, discount);
                PrepareDiscountManufacturerSearchModel(model.DiscountManufacturerSearchModel, discount);
            }

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            //get URL of discount with coupon code
            if (model.RequiresCouponCode && !string.IsNullOrEmpty(model.CouponCode))
            {
                model.DiscountUrl = QueryHelpers.AddQueryString(_webHelper.GetStoreLocation().TrimEnd('/'),
                    NopDiscountDefaults.DiscountCouponQueryParameter, model.CouponCode);
            }

            //set default values for the new model
            if (discount == null)
                model.LimitationTimes = 1;

            return model;
        }

        /// <summary>
        /// Prepare discount requirement rule models
        /// </summary>
        /// <param name="requirements">Collection of discount requirements</param>
        /// <param name="discount">Discount</param>
        /// <param name="groupInteractionType">Interaction type within the group of requirements</param>
        /// <returns>List of discount requirement rule models</returns>
        public virtual IList<DiscountRequirementRuleModel> PrepareDiscountRequirementRuleModels(ICollection<DiscountRequirement> requirements,
            Discount discount, RequirementGroupInteractionType groupInteractionType)
        {
            if (requirements == null)
                throw new ArgumentNullException(nameof(requirements));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            var lastRequirement = requirements.LastOrDefault();

            return requirements.Select(requirement =>
            {
                //set common properties
                var requirementModel = new DiscountRequirementRuleModel
                {
                    DiscountRequirementId = requirement.Id,
                    ParentId = requirement.ParentId,
                    IsGroup = requirement.IsGroup,
                    RuleName = requirement.DiscountRequirementRuleSystemName,
                    IsLastInGroup = lastRequirement == null || lastRequirement.Id == requirement.Id,
                    InteractionTypeId = (int)groupInteractionType
                };

                var interactionType = requirement.InteractionType ?? RequirementGroupInteractionType.And;
                requirementModel.AvailableInteractionTypes = interactionType.ToSelectList();

                if (requirement.IsGroup)
                {
                    //get child requirements for the group
                    requirementModel
                        .ChildRequirements = PrepareDiscountRequirementRuleModels(requirement.ChildRequirements, discount, interactionType);

                    return requirementModel;
                }

                //or try to get name and configuration URL for the requirement
                var requirementRule = _discountPluginManager.LoadPluginBySystemName(requirement.DiscountRequirementRuleSystemName);
                if (requirementRule == null)
                    return null;

                requirementModel.RuleName = requirementRule.PluginDescriptor.FriendlyName;
                requirementModel
                    .ConfigurationUrl = requirementRule.GetConfigurationUrl(discount.Id, requirement.Id);

                return requirementModel;
            }).ToList();
        }

        /// <summary>
        /// Prepare paged discount usage history list model
        /// </summary>
        /// <param name="searchModel">Discount usage history search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount usage history list model</returns>
        public virtual DiscountUsageHistoryListModel PrepareDiscountUsageHistoryListModel(DiscountUsageHistorySearchModel searchModel,
            Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            //get discount usage history
            var history = _discountService.GetAllDiscountUsageHistory(discountId: discount.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new DiscountUsageHistoryListModel
            {
                Data = history.Select(historyEntry =>
                {
                    //fill in model values from the entity
                    var discountUsageHistoryModel = historyEntry.ToModel<DiscountUsageHistoryModel>();

                    //convert dates to the user time
                    discountUsageHistoryModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(historyEntry.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    var order = _orderService.GetOrderById(historyEntry.OrderId);
                    if (order != null)
                    {
                        discountUsageHistoryModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
                        discountUsageHistoryModel.CustomOrderNumber = order.CustomOrderNumber;
                    }

                    return discountUsageHistoryModel;
                }),
                Total = history.TotalCount
            };

            return model;
        }

        /// <summary>
        /// Prepare paged discount product list model
        /// </summary>
        /// <param name="searchModel">Discount product search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount product list model</returns>
        public virtual DiscountProductListModel PrepareDiscountProductListModel(DiscountProductSearchModel searchModel, Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            //get products with applied discount
            var discountProducts = _discountService.GetProductsWithAppliedDiscount(discountId: discount.Id,
                showHidden: false,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new DiscountProductListModel().PrepareToGrid(searchModel, discountProducts, () =>
            {
                //fill in model values from the entity
                return discountProducts.Select(product =>
                {
                    var discountProductModel = product.ToModel<DiscountProductModel>();
                    discountProductModel.ProductId = product.Id;
                    discountProductModel.ProductName = product.Name;

                    return discountProductModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product search model to add to the discount
        /// </summary>
        /// <param name="searchModel">Product search model to add to the discount</param>
        /// <returns>Product search model to add to the discount</returns>
        public virtual AddProductToDiscountSearchModel PrepareAddProductToDiscountSearchModel(AddProductToDiscountSearchModel searchModel)
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
        /// Prepare paged product list model to add to the discount
        /// </summary>
        /// <param name="searchModel">Product search model to add to the discount</param>
        /// <returns>Product list model to add to the discount</returns>
        public virtual AddProductToDiscountListModel PrepareAddProductToDiscountListModel(AddProductToDiscountSearchModel searchModel)
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
            var model = new AddProductToDiscountListModel().PrepareToGrid(searchModel, products, () =>
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

        /// <summary>
        /// Prepare paged discount category list model
        /// </summary>
        /// <param name="searchModel">Discount category search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount category list model</returns>
        public virtual DiscountCategoryListModel PrepareDiscountCategoryListModel(DiscountCategorySearchModel searchModel, Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            //get categories with applied discount
            var discountCategories = _discountService.GetCategoriesWithAppliedDiscount(discountId: discount.Id,
                showHidden: false,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new DiscountCategoryListModel().PrepareToGrid(searchModel, discountCategories, () =>
            {
                //fill in model values from the entity
                return discountCategories.Select(category =>
                {
                    var discountCategoryModel = category.ToModel<DiscountCategoryModel>();

                    discountCategoryModel.CategoryName = _categoryService.GetFormattedBreadCrumb(category);
                    discountCategoryModel.CategoryId = category.Id;

                    return discountCategoryModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare category search model to add to the discount
        /// </summary>
        /// <param name="searchModel">Category search model to add to the discount</param>
        /// <returns>Category search model to add to the discount</returns>
        public virtual AddCategoryToDiscountSearchModel PrepareAddCategoryToDiscountSearchModel(AddCategoryToDiscountSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetPopupGridPageSize();
            searchModel.Grid = PrepareAddCategoryGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged category list model to add to the discount
        /// </summary>
        /// <param name="searchModel">Category search model to add to the discount</param>
        /// <returns>Category list model to add to the discount</returns>
        public virtual AddCategoryToDiscountListModel PrepareAddCategoryToDiscountListModel(AddCategoryToDiscountSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get categories
            var categories = _categoryService.GetAllCategories(showHidden: true,
                categoryName: searchModel.SearchCategoryName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddCategoryToDiscountListModel().PrepareToGrid(searchModel, categories, () =>
            {
                return categories.Select(category =>
                {
                    //fill in model values from the entity
                    var categoryModel = category.ToModel<CategoryModel>();

                    //fill in additional values (not existing in the entity)
                    categoryModel.Breadcrumb = _categoryService.GetFormattedBreadCrumb(category);
                    categoryModel.SeName = _urlRecordService.GetSeName(category, 0, true, false);

                    return categoryModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged discount manufacturer list model
        /// </summary>
        /// <param name="searchModel">Discount manufacturer search model</param>
        /// <param name="discount">Discount</param>
        /// <returns>Discount manufacturer list model</returns>
        public virtual DiscountManufacturerListModel PrepareDiscountManufacturerListModel(DiscountManufacturerSearchModel searchModel,
            Discount discount)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            //get manufacturers with applied discount
            var discountManufacturers = _discountService.GetManufacturersWithAppliedDiscount(discountId: discount.Id,
                showHidden: false,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new DiscountManufacturerListModel().PrepareToGrid(searchModel, discountManufacturers, () =>
            {
                //fill in model values from the entity
                return discountManufacturers.Select(manufacturer =>
                {
                    var discountManufacturerModel = manufacturer.ToModel<DiscountManufacturerModel>();
                    discountManufacturerModel.ManufacturerId = manufacturer.Id;
                    discountManufacturerModel.ManufacturerName = manufacturer.Name;

                    return discountManufacturerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare manufacturer search model to add to the discount
        /// </summary>
        /// <param name="searchModel">Manufacturer search model to add to the discount</param>
        /// <returns>Manufacturer search model to add to the discount</returns>
        public virtual AddManufacturerToDiscountSearchModel PrepareAddManufacturerToDiscountSearchModel(AddManufacturerToDiscountSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetPopupGridPageSize();
            searchModel.Grid = PrepareAddManufacturerGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged manufacturer list model to add to the discount
        /// </summary>
        /// <param name="searchModel">Manufacturer search model to add to the discount</param>
        /// <returns>Manufacturer list model to add to the discount</returns>
        public virtual AddManufacturerToDiscountListModel PrepareAddManufacturerToDiscountListModel(AddManufacturerToDiscountSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get manufacturers
            var manufacturers = _manufacturerService.GetAllManufacturers(showHidden: true,
                manufacturerName: searchModel.SearchManufacturerName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddManufacturerToDiscountListModel().PrepareToGrid(searchModel, manufacturers, () =>
            {
                return manufacturers.Select(manufacturer =>
                {
                    var manufacturerModel = manufacturer.ToModel<ManufacturerModel>();
                    manufacturerModel.SeName = _urlRecordService.GetSeName(manufacturer, 0, true, false);

                    return manufacturerModel;
                });
            });

            return model;
        }

        #endregion
    }
}