﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Localization;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Tax;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the tax model factory implementation
    /// </summary>
    public partial class TaxModelFactory : ITaxModelFactory
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ITaxPluginManager _taxPluginManager;

        #endregion

        #region Ctor

        public TaxModelFactory(ILocalizationService localizationService, 
            ITaxCategoryService taxCategoryService,
            ITaxPluginManager taxPluginManager)
        {
            _localizationService = localizationService;
            _taxCategoryService = taxCategoryService;
            _taxPluginManager = taxPluginManager;
        }

        #endregion

        #region Utilities
        
        /// <summary>
        /// Prepare datatables model
        /// </summary>
        /// <param name="searchModel">Search model</param>
        /// <returns>Datatables model</returns>
        protected virtual DataTablesModel PrepareTaxProviderGridModel(TaxProviderSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "tax-providers-grid",
                UrlRead = new DataUrl("Providers", "Tax", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare model columns
            model.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(TaxProviderModel.FriendlyName))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Tax.Providers.Fields.FriendlyName"),
                    Width = "250"
                },
                new ColumnProperty(nameof(TaxProviderModel.SystemName))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Tax.Providers.Fields.SystemName"),
                    Width = "250"
                },
                new ColumnProperty(nameof(TaxProviderModel.IsPrimaryTaxProvider))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Tax.Providers.Fields.IsPrimaryTaxProvider"),
                    Width = "100",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderBoolean()
                },
                new ColumnProperty(nameof(TaxProviderModel.SystemName))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Tax.Providers.Fields.MarkAsPrimaryProvider"),
                    Width = "200",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderCustom("renderColumnSystemName")
                },
                new ColumnProperty(nameof(TaxProviderModel.SystemName))
                {
                    Title = _localizationService.GetResource("Admin.Configuration.Tax.Providers.Configure"),
                    Width = "150",
                    ClassName =  StyleColumn.CenterAll,
                    Render = new RenderCustom("renderColumnConfigure")
                }
            };

            return model;
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Prepare tax configuration model
        /// </summary>
        /// <param name="taxConfigurationModel">Tax configuration model</param>
        /// <returns>Tax configuration model</returns>
        public virtual TaxConfigurationModel PrepareTaxConfigurationModel(TaxConfigurationModel taxConfigurationModel)
        {
            if (taxConfigurationModel == null)
                throw new ArgumentNullException(nameof(taxConfigurationModel));

            //prepare nested search models
            PrepareTaxProviderSearchModel(taxConfigurationModel.TaxProviders);
            PrepareTaxCategorySearchModel(taxConfigurationModel.TaxCategories);

            return taxConfigurationModel;
        }

        /// <summary>
        /// Prepare tax provider search model
        /// </summary>
        /// <param name="searchModel">Tax provider search model</param>
        /// <returns>Tax provider search model</returns>
        public virtual TaxProviderSearchModel PrepareTaxProviderSearchModel(TaxProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareTaxProviderGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged tax provider list model
        /// </summary>
        /// <param name="searchModel">Tax provider search model</param>
        /// <returns>Tax provider list model</returns>
        public virtual TaxProviderListModel PrepareTaxProviderListModel(TaxProviderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get tax providers
            var taxProviders = _taxPluginManager.LoadAllPlugins().ToPagedList(searchModel);

            //prepare grid model
            var model = new TaxProviderListModel().PrepareToGrid(searchModel, taxProviders, () =>
            {
                return taxProviders.Select(provider =>
                {
                    //fill in model values from the entity
                    var taxProviderModel = provider.ToPluginModel<TaxProviderModel>();

                    //fill in additional values (not existing in the entity)
                    taxProviderModel.ConfigurationUrl = provider.GetConfigurationPageUrl();
                    taxProviderModel.IsPrimaryTaxProvider = _taxPluginManager.IsPluginActive(provider);

                    return taxProviderModel;
                });
            });
            return model;
        }

        /// <summary>
        /// Prepare tax category search model
        /// </summary>
        /// <param name="searchModel">Tax category search model</param>
        /// <returns>Tax category search model</returns>
        public virtual TaxCategorySearchModel PrepareTaxCategorySearchModel(TaxCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged tax category list model
        /// </summary>
        /// <param name="searchModel">Tax category search model</param>
        /// <returns>Tax category list model</returns>
        public virtual TaxCategoryListModel PrepareTaxCategoryListModel(TaxCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories().ToPagedList(searchModel);

            //prepare grid model
            var model = new TaxCategoryListModel
            {
                //fill in model values from the entity
                Data = taxCategories.Select(taxCategory => taxCategory.ToModel<TaxCategoryModel>()),
                Total = taxCategories.TotalCount
            };

            return model;
        }

        #endregion
    }
}