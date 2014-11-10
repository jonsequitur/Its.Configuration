using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Reactive.Linq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using System.Linq;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class ConfigValueExportProviderTests
    {
        private static readonly Dictionary<string, string> configValues = new Dictionary<string, string>
        {
            { "some_string", "hello" },
            { "some_date", "09/27/1953" },
            { "some_int", "42" },
            { "some_bool", "true" },
        };
      
        [Test]
        public void A_configuration_value_export_can_be_exposed_in_multiple_types()
        {
            var usedKeys = new HashSet<string>();
            var container = new CompositionContainer(
                new TypeCatalog(typeof (SomethingConfigurable)),
                new ConfigurationValueExportProvider(key =>
                {
                    usedKeys.Add(key);
                    return configValues[key];
                }));

            Assert.That(container.GetExportedValue<string>("some_int"), Is.EqualTo("42"));
            Assert.That(container.GetExportedValue<int>("some_int"), Is.EqualTo(42));
            Assert.That(container.GetExportedValue<int?>("some_int"), Is.EqualTo(42));
            Assert.That(usedKeys.Count, Is.EqualTo(1));
        }

        [Test]
        public void Values_are_retrieved_lazily_via_an_injected_function()
        {
            var usedKeys = new HashSet<string>();
            var container = new CompositionContainer(
                new TypeCatalog(typeof (SomethingConfigurable)),
                new ConfigurationValueExportProvider(key =>
                {
                    usedKeys.Add(key);
                    return configValues[key];
                }));

            var obj = container.GetExportedValue<SomethingConfigurable>();

            Assert.That(usedKeys.Count, Is.EqualTo(4));
            Assert.That(obj.IsCorrectlyConfigured());
        }

        [Test]
        public void A_catalog_can_be_created_from_app_config_using_an_ExportProvider()
        {
            var container = new CompositionContainer(
                new TypeCatalog(typeof (SomethingConfigurable)),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            var obj = container.GetExportedValue<SomethingConfigurable>();

            Assert.That(obj.IsCorrectlyConfigured());
        }

        [Test]
        public void Can_satisfy_imports_but_control_instantiation()
        {
            var container = new CompositionContainer(
                new TypeCatalog(typeof (SomethingConfigurable)),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            var obj = new SomethingConfigurable();

            container.SatisfyImportsOnce(obj);

            Assert.That(obj.IsCorrectlyConfigured());
        }

        [Test]
        public void Config_settings_can_be_used_for_multiple_types()
        {
            var config = new Dictionary<string, object>
            {
                { "db-retry-count", "5" },
                { "orders-db-connection-string", "orders!" },
                { "products-db-connection-string", "products!" }
            };
            var container = new CompositionContainer(
                new TypeCatalog(typeof (ProductRepository), typeof (OrderRepository)),
                new ConfigurationValueExportProvider(config));

            var productRepository = container.GetExportedValue<ProductRepository>();
            var orderRepository = container.GetExportedValue<OrderRepository>();

            Assert.That(productRepository.RetryCount, Is.EqualTo(5));
            Assert.That(productRepository.ConnectionString, Is.EqualTo("products!"));
            Assert.That(orderRepository.RetryCount, Is.EqualTo(5));
            Assert.That(orderRepository.ConnectionString, Is.EqualTo("orders!"));
        }

        [Test]
        public void When_a_configuration_value_is_missing_then_an_exception_is_thrown()
        {
            var container = new CompositionContainer(
                new TypeCatalog(typeof (ProductRepository)),
                new ConfigurationValueExportProvider(
                    new Dictionary<string, object>
                    {
                        { "products-db-connection-string", "products!" }
                    }));

            Assert.Throws<ImportCardinalityMismatchException>(() => container.GetExportedValue<ProductRepository>());
        }

        [Test]
        public void Can_specify_custom_type_conversion()
        {
             var container = new CompositionContainer(
                new TypeCatalog(typeof (ImageRepository)),
                new ConfigurationValueExportProvider(
                    new Dictionary<string, object>
                    {
                        { "image-location", @"c:\images" }
                    }).RegisterConversion(s => new DirectoryInfo(s)));

            var repository = container.GetExportedValue<ImageRepository>();

            Assert.That(repository.ImageLocation, Is.Not.Null);
            Assert.That(repository.ImageLocation.ToString(), Is.EqualTo(@"c:\images"));
        }

        [Test]
        public void When_conversion_fails_then_the_exception_is_informative()
        {
            var container = new CompositionContainer(
                new TypeCatalog(typeof (ImageRepository)),
                new ConfigurationValueExportProvider(
                    new Dictionary<string, object>
                    {
                        { "image-location", @"** not a directory **" }
                    }).RegisterConversion(s => new DirectoryInfo(s)));

            var ex = Assert.Throws<CompositionException>(() => container.GetExportedValue<ImageRepository>());

            Assert.That(ex.Message, Is.StringContaining("image-location"));
        }

        [Test]
        public void Conversion_to_Uri_is_supported_by_default()
        {
             var container = new CompositionContainer(
                new TypeCatalog(typeof (ImageRepository)),
                new ConfigurationValueExportProvider(
                    new Dictionary<string, object>
                    {
                        { "cdn-api-uri", "http://cdn.biz/api" }
                    }));

            var repository = container.GetExportedValue<ImageRepository>();

            Assert.That(repository.CdnApiUri, Is.Not.Null);
            Assert.That(repository.CdnApiUri.AbsoluteUri, Is.EqualTo("http://cdn.biz/api"));
        }

        [Test]
        public void Unconverted_config_values_are_cached()
        {
            int count = 0;
            var configSettings = new Dictionary<string, object>
            {
                { "url", "http://cdn.biz/api" }
            };
            var exportProvider = new ConfigurationValueExportProvider(
                k =>
                {
                    count++;
                    return configSettings[k];
                });

            exportProvider.GetExport<string>("url");
            exportProvider.GetExport<string>("url");

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Updated_values_are_applied_to_new_configured_instances()
        {
            var exportProvider = new ConfigurationValueExportProvider(new Dictionary<string, object>
            {
                { "cdn-api-uri", "http://cdn.biz/1" },
            });
            var container = new CompositionContainer(
                new TypeCatalog(typeof (ImageRepository)),
                exportProvider);

            var imageRepository = new ImageRepository();

            container.SatisfyImportsOnce(imageRepository);

            exportProvider.UpdateConfigurationValue("cdn-api-uri", "http://cdn.biz/2");

            container.SatisfyImportsOnce(imageRepository);

            Assert.That(imageRepository.CdnApiUri.AbsoluteUri, Is.EqualTo("http://cdn.biz/2"));
        }

        [Test]
        public void Observables_can_receive_correctly_initialized_observables()
        {
            var exportProvider = new ConfigurationValueExportProvider(s => "false");

            var container = new CompositionContainer(new TypeCatalog(typeof(Reconfigurable)), exportProvider);

            var reconfigurable = container.GetExportedValue<Reconfigurable>();

            Assert.That(reconfigurable.Enabled.First(), Is.False);
        }

        [Test]
        public void Observable_configuration_values_can_receive_updates()
        {
            var exportProvider = new ConfigurationValueExportProvider(s => "false");

            var container = new CompositionContainer(new TypeCatalog(typeof(Reconfigurable)), exportProvider);

            var reconfigurable = container.GetExportedValue<Reconfigurable>();

            exportProvider.UpdateConfigurationValue("enabled", "true");

            Assert.That(reconfigurable.Enabled.First(), Is.True);
        }

        [Test]
        public void Observable_configuration_values_can_receive_updates_when_using_custom_type_conversions()
        {
            var exportProvider = new ConfigurationValueExportProvider(new Dictionary<string, object>
            {
                { "log-file", @"log1.txt" }
            }).RegisterConversion(s => new FileInfo(s));

            var container = new CompositionContainer(new TypeCatalog(typeof(Logger)), exportProvider);

            var logger = container.GetExportedValue<Logger>();

            exportProvider.UpdateConfigurationValue("log-file", "log2.txt");

            Assert.That(logger.LogFile.First().Name, Is.EqualTo(@"log2.txt"));
        }

        [Export]
        private class Reconfigurable
        {
            [Import("enabled")]
            public IObservable<bool> Enabled;
        }

        [Export]
        private class ReconfigurableWithDefaultEnabledImplementation
        {
            [Import("enabled", AllowDefault = true)]
            public IObservable<bool> Enabled = new BehaviorSubject<bool>(false);
        }

        [Export]
        private class Logger
        {
            [Import("log-file")]
            public IObservable<FileInfo> LogFile { get; set; }
        }
    }
}