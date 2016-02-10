using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Jmp.Jira;
using Jmp.Reports;
using Jmp.Web.Models;

namespace Jmp.Web.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            var defaultReportSetup = new ReportSetup()
            {
                Jql = "project=MVPD1",
                ColumnLabelPrefix = "jmp-stream-",
                WeeklyCapacityHoursPerStream = "*: 40",
                IssueFinalStatuses = "DEV COMPLETE, CANCELLED",
                JiraApiUrl = "https://orwell.atlassian.net/rest/api/2/",
                JiraShowIssueUrl = "https://orwell.atlassian.net/browse/",
                JiraSearchIssuesUrl = "https://orwell.atlassian.net/issues/?jql="

            };

            container.RegisterInstance<ReportSetup>(defaultReportSetup, new ContainerControlledLifetimeManager());

            container.RegisterType<IJiraClient, JiraClient>();
            container.RegisterType<IReportService, ReportService>();
        }
    }
}