using Tinifier.Core.Infrastructure;
using Tinifier.Core.Models.Db;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Tinifier.Core.Application
{
    public class DbComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components()
                .Append<DbStartup>();
        }

        public class DbStartup : IComponent
        {
            private readonly IScopeProvider scopeProvider;
            private readonly IMigrationBuilder migrationBuilder;
            private readonly IKeyValueService keyValueService;
            private readonly ILogger logger;

            public DbStartup(
                IScopeProvider scopeProvider,
                IMigrationBuilder migrationBuilder,
                IKeyValueService keyValueService,
                ILogger logger)
            {
                this.scopeProvider = scopeProvider;
                this.migrationBuilder = migrationBuilder;
                this.keyValueService = keyValueService;
                this.logger = logger;
            }

            public void Initialize()
            {
                Upgrader upgrader = new Upgrader(new MyMigrationPlan());
                upgrader.Execute(scopeProvider, migrationBuilder, keyValueService, logger);
            }

            public void Terminate() { }

            public class MyMigrationPlan : MigrationPlan
            {
                public MyMigrationPlan() : base(PackageConstants.SectionName)
                {
                    var res = From(string.Empty)
                         .To<MigrationCreateTables>("tinifier-migration");
                }
            }

            public class MigrationCreateTables : MigrationBase
            {
                public MigrationCreateTables(IMigrationContext context)
                    : base(context)
                { }

                public override void Migrate()
                {
                    if (!TableExists(PackageConstants.DbSettingsTable))
                        Create.Table<TSetting>().Do();

                    if (!TableExists(PackageConstants.DbHistoryTable))
                        Create.Table<TinyPNGResponseHistory>().Do();

                    if (!TableExists(PackageConstants.DbStatisticTable))
                        Create.Table<TImageStatistic>().Do();

                    if (!TableExists(PackageConstants.DbStateTable))
                        Create.Table<TState>().Do();

                    if (!TableExists(PackageConstants.MediaHistoryTable))
                        Create.Table<TinifierMediaHistory>().Do();

                    if (!TableExists(PackageConstants.DbTImageCropperInfoTable))
                        Create.Table<TImageCropperInfo>().Do();

                    if (!TableExists(PackageConstants.DbTFileSystemProviderSettings))
                        Create.Table<TFileSystemProviderSettings>().Do();

                    if (!TableExists(PackageConstants.DbTinifierImageHistoryTable))
                        Create.Table<TinifierImagesHistory>().Do();
                }
            }
        }
    }
}