using ClientDependency.Core;
using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Tinifier.Core.Infrastructure;
using Umbraco.Web;
using Tinifier.Core.Services.Settings;
using Tinifier.Core.Services.Media;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Tinifier.Core.Repository.Common;
using System.Linq;

namespace Tinifier.Core.Controllers
{
    [Tree(PackageConstants.SectionAlias, PackageConstants.TreeAlias, TreeTitle = PackageConstants.TreeTitle, SortOrder = 5)]
    [PluginController(PackageConstants.SectionName)]
    class TinifierTreeController : TreeController
    {
        private readonly IImageService _imageService;
        private readonly ISettingsService _settingsService;
        private readonly IUmbracoDbRepository _umbracoDbRepository;

        public TinifierTreeController(ISettingsService settingsService, IImageService imageService, IUmbracoDbRepository umbracoDbRepository)
        {
            _settingsService = settingsService;
            _imageService = imageService;
            _umbracoDbRepository = umbracoDbRepository;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            var settings = _settingsService.GetSettings();
            var trashedNodes = _umbracoDbRepository.GetTrashedNodes();

            if (id == PackageConstants.FirstNodeId && settings != null && !settings.HideLeftPanel)
            {
                foreach (var timage in _imageService.GetTopOptimizedImages())
                {
                    var treeNode = CreateTreeNode(timage.Id + string.Empty, id, queryStrings, timage.Name, PackageConstants.TreeIcon, false,
                        queryStrings.GetValue<string>(PackageConstants.AppAlias) + PackageConstants.CustomTreeUrl +
                        SolutionExtensions.Base64Encode(timage.Id));

                    if (trashedNodes.Count(idNode => timage.Id.Equals(idNode.ToString())) != 0)
                        treeNode.Name += " (trashed)";

                    treeNode.MenuUrl = null;
                    nodes.Add(treeNode);
                }
            }
            return nodes;
        }


        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {

            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
                // add your menu item actions or custom ActionMenuItems
                menu.Items.Add(new CreateChildEntity(Services.TextService));
                // add refresh menu item (note no dialog)            
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }
            // add a delete action to each individual item
            menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);

            return menu;
            //return null;
        }

        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            root.HasChildren = true;
            root.MenuUrl = null;
            return root;
        }
    }
}
