﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Forums;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Forums;
using Nop.Web.Framework.Models.DataTables;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the forum model factory implementation
    /// </summary>
    public partial class ForumModelFactory : IForumModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ForumModelFactory(IDateTimeHelper dateTimeHelper,
            IForumService forumService,
            ILocalizationService localizationService)
        {
            _dateTimeHelper = dateTimeHelper;
            _forumService = forumService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare forum search model
        /// </summary>
        /// <param name="searchModel">Forum search model</param>
        /// <returns>Forum search model</returns>
        protected virtual ForumSearchModel PrepareForumSearchModel(ForumSearchModel searchModel)
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
        protected virtual DataTablesModel PrepareForumGridModel(ForumGroupSearchModel searchModel)
        {
            //prepare common properties
            var model = new DataTablesModel
            {
                Name = "forumgroup-grid",
                UrlRead = new DataUrl("ForumGroupList", "Forum", null),
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
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
                new ColumnProperty(nameof(ForumGroupModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Fields.Name"),
                    Width = "300"
                },
                new ColumnProperty(nameof(ForumGroupModel.DisplayOrder))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder"),
                    Width = "100"
                },
                new ColumnProperty(nameof(ForumGroupModel.CreatedOn))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn"),
                    Width = "200",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(ForumGroupModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "50",
                    ClassName = StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("EditForumGroup"))
                }
            };

            //prepare common properties for detail table
            var detailModel = new DataTablesModel
            {
                Name = "shipments-grid",
                UrlRead = new DataUrl("ForumList", "Forum", null),
                IsChildTable = true,
                Length = searchModel.PageSize,
                LengthMenu = searchModel.AvailablePageSizes
            };

            //prepare filters to search
            detailModel.Filters = new List<FilterParameter>
            {
                new FilterParameter(nameof(ForumModel.ForumGroupId), nameof(ForumGroupModel.Id), true)
            };

            detailModel.ColumnCollection = new List<ColumnProperty>
            {
                new ColumnProperty(nameof(ForumModel.Name))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.Name"),
                    Width = "300"
                },
                new ColumnProperty(nameof(ForumModel.DisplayOrder))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.DisplayOrder"),
                    Width = "150"
                },
                new ColumnProperty(nameof(ForumModel.CreatedOn))
                {
                    Title = _localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.CreatedOn"),
                    Width = "150",
                    Render = new RenderDate()
                },
                new ColumnProperty(nameof(ForumModel.Id))
                {
                    Title = _localizationService.GetResource("Admin.Common.Edit"),
                    Width = "50",
                    ClassName = StyleColumn.CenterAll,
                    Render = new RenderButtonEdit(new DataUrl("EditForum"))
                }
            };

            model.ChildTable = detailModel;

            return model;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare forum group search model
        /// </summary>
        /// <param name="searchModel">Forum group search model</param>
        /// <returns>Forum group search model</returns>
        public virtual ForumGroupSearchModel PrepareForumGroupSearchModel(ForumGroupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare nested search model
            PrepareForumSearchModel(searchModel.ForumSearch);

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Grid = PrepareForumGridModel(searchModel);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged forum group list model
        /// </summary>
        /// <param name="searchModel">Forum group search model</param>
        /// <returns>Forum group list model</returns>
        public virtual ForumGroupListModel PrepareForumGroupListModel(ForumGroupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get forum groups
            var forumGroups = _forumService.GetAllForumGroups().ToPagedList(searchModel);

            //prepare list model
            var model = new ForumGroupListModel().PrepareToGrid(searchModel, forumGroups, () =>
            {
                return forumGroups.Select(forumGroup =>
                {
                    //fill in model values from the entity
                    var forumGroupModel = forumGroup.ToModel<ForumGroupModel>();

                    //convert dates to the user time
                    forumGroupModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(forumGroup.CreatedOnUtc, DateTimeKind.Utc);

                    return forumGroupModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare forum group model
        /// </summary>
        /// <param name="model">Forum group model</param>
        /// <param name="forumGroup">Forum group</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Forum group model</returns>
        public virtual ForumGroupModel PrepareForumGroupModel(ForumGroupModel model, ForumGroup forumGroup, bool excludeProperties = false)
        {
            //fill in model values from the entity
            if (forumGroup != null)
                model = model ?? forumGroup.ToModel<ForumGroupModel>();

            //set default values for the new model
            if (forumGroup == null)
                model.DisplayOrder = 1;

            return model;
        }

        /// <summary>
        /// Prepare paged forum list model
        /// </summary>
        /// <param name="searchModel">Forum search model</param>
        /// <param name="forumGroup">Forum group</param>
        /// <returns>Forum list model</returns>
        public virtual ForumListModel PrepareForumListModel(ForumSearchModel searchModel, ForumGroup forumGroup)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (forumGroup == null)
                throw new ArgumentNullException(nameof(forumGroup));

            //get forums
            var forums = forumGroup.Forums.ToList().ToPagedList(searchModel);

            //prepare list model
            var model = new ForumListModel().PrepareToGrid(searchModel, forums, () =>
            {
                return forums.Select(forum =>
                {
                    //fill in model values from the entity
                    var forumModel = forum.ToModel<ForumModel>();

                    //convert dates to the user time
                    forumModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(forum.CreatedOnUtc, DateTimeKind.Utc);

                    return forumModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare forum model
        /// </summary>
        /// <param name="model">Forum model</param>
        /// <param name="forum">Forum</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Forum model</returns>
        public virtual ForumModel PrepareForumModel(ForumModel model, Forum forum, bool excludeProperties = false)
        {
            //fill in model values from the entity
            if (forum != null)
                model = model ?? forum.ToModel<ForumModel>();

            //set default values for the new model
            if (forum == null)
                model.DisplayOrder = 1;

            //prepare available forum groups
            foreach (var forumGroup in _forumService.GetAllForumGroups())
            {
                model.ForumGroups.Add(forumGroup.ToModel<ForumGroupModel>());
            }

            return model;
        }

        #endregion
    }
}