using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;

namespace Tinifier.Core.Infrastructure
{
    [Weight(-10)]
    class DashboardExtension
    {
       //public string[] Sections => new[] { "TinifierSettings", "TinifierStatistic" };
       //
       //public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
       //
       //public string Alias => PackageConstants.SectionAlias;
       //
       //public string View => "/App_Plugins/Tinifier/BackOffice/Dashboards/edit.html";
    }
}
