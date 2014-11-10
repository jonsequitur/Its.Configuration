using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;
using NUnit.Framework;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class SatisfyImportsTests
    {
        [Test]
        public void Can_satisfy_imports_for_public_static_properties()
        {
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic));

            Assert.That(
                SomethingConfigurableAndStatic.ADateProperty,
                Is.EqualTo(DateTime.Parse("9/27/1953")));
        }

        [Test]
        public void Can_satisfy_imports_for_public_static_fields()
        {
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic));

            Assert.That(
                SomethingConfigurableAndStatic.ADateField,
                Is.EqualTo(DateTime.Parse("9/27/1953")));
            Assert.That(
                SomethingConfigurableAndStatic.AnIntField,
                Is.EqualTo(42));
        }

        [Test]
        public void Can_satisfy_imports_for_internal_and_private_static_fields()
        {
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic));

            Assert.That(
                SomethingConfigurableAndStatic.AnInternalStringFieldsAccessorProperty,
                Is.EqualTo(ConfigurationManager.AppSettings.Get("some_string")));
            Assert.That(
                SomethingConfigurableAndStatic.APrivateBoolFieldsAccessorProperty,
                Is.EqualTo(bool.Parse(ConfigurationManager.AppSettings.Get("some_bool"))));
        }

        [Test]
        public void Can_satisfy_imports_for_private_and_internal_fields()
        {
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            var obj = container.GetExportedValue<SomethingWithNonPublicConfigurables>();

            Assert.That(obj.IsCorrectlyConfigured());
        }

        [Test]
        public void Can_satisfy_imports_for_static_properties_that_require_type_conversions()
        {
            var exportProvider = new ConfigurationValueExportProvider(
                new Dictionary<string, object> { { "some_uri", "http://bing.com/" } })
                .RegisterConversion(s => new Uri(s));
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                exportProvider);

            container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStaticHavingSpecialTypes));

            Assert.That(
                SomethingConfigurableAndStaticHavingSpecialTypes.SomeUri,
                Is.Not.Null);
            Assert.That(
                SomethingConfigurableAndStaticHavingSpecialTypes.SomeUri.ToString(),
                Is.EqualTo("http://bing.com/"));
        }

        [Test]
        public void Can_satisfy_static_imports_for_all_types_in_the_application()
        {
            int some_other_int = 4576234;
            var some_uri = "https://msn.com/";
            var settings = new Dictionary<string, object>
            {
                { "some_other_int", some_other_int },
                { "some_uri", some_uri },
                { "some_date", DateTime.Now },
                { "some_bool", true },
                { "some_string", "some value" },
            };

            var container = new CompositionContainer(
                new DeploymentCatalog(),
                new ConfigurationValueExportProvider(settings)
                    .RegisterConversion(s => new Uri(s)));

            container.SatisfyStaticImportsInAppDomain();

            Assert.That(SomethingConfigurableAndStatic.AnIntProperty,
                        Is.EqualTo(some_other_int));
            Assert.That(SomethingConfigurableAndStatic.a_string,
                        Is.EqualTo("some value"));
            Assert.That(SomethingConfigurableAndStaticHavingSpecialTypes.SomeUri.ToString(),
                        Is.EqualTo(some_uri));
            Assert.That(SomethingConfigurableHavingStaticProperties.SomeUri.ToString(),
                        Is.EqualTo(some_uri));
        }

        [Test]
        public void Unsatisfied_non_optional_imports_for_static_properties_throw()
        {
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(new Dictionary<string, object>()));

            Assert.Throws<CompositionException>(() =>
                                                container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic)));
        }

        [Test]
        public void Exceptions_for_missing_imports_for_static_properties_list_the_missing_config_keys()
        {
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(new Dictionary<string, object>()));

            var ex = Assert.Throws<CompositionException>(() =>
                                                         container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic)));

            Assert.That(ex.Message, Is.StringContaining("some_string"));
            Assert.That(ex.Message, Is.StringContaining("some_bool"));
            Assert.That(ex.Message, Is.StringContaining("some_date"));
        }

        [Test]
        public void Unsatisfied_optional_imports_for_static_properties_do_not_cause_exceptions()
        {
            var expected = 1563456;
            SomethingConfigurableAndStatic.AnIntProperty = expected;
            var container = new CompositionContainer(
                new AssemblyCatalog(GetType().Assembly),
                new ConfigurationValueExportProvider(ConfigurationManager.AppSettings.Get));

            container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic));

            Assert.That(
                SomethingConfigurableAndStatic.AnIntProperty,
                Is.EqualTo(expected));
        }

        [Test]
        public void Can_mix_config_sources_in_order_to_merge_Azure_and_web_config_settings()
        {
            // simulate an overriding value for one config key
            GetConfigurationValue getFromAzure = key => key == "some_string" ? "hello from Azure config" : null;
            var assemblyCatalog = new AssemblyCatalog(GetType().Assembly);
            var container = new CompositionContainer(
                assemblyCatalog,
                new ConfigurationValueExportProvider(key =>
                                                     getFromAzure(key) ?? ConfigurationManager.AppSettings[key]));

            var obj = container.GetExportedValue<SomethingConfigurable>();

            // here's the override
            Assert.That(obj.AString, Is.EqualTo("hello from Azure config"));
            // the others fall back to the config file
            Assert.That(obj.ADate, Is.EqualTo(DateTime.Parse(ConfigurationManager.AppSettings["some_date"])));
            Assert.That(obj.AnInt, Is.EqualTo(int.Parse(ConfigurationManager.AppSettings["some_int"])));
            Assert.That(obj.ABool, Is.EqualTo(bool.Parse(ConfigurationManager.AppSettings["some_bool"])));
        }

        [Test]
        public void Properties_using_Allow_Default_can_declare_their_own_defaults_using_an_encapsulation_pattern()
        {
            var container = new CompositionContainer(
                new TypeCatalog(typeof (OrderRepository)),
                new ConfigurationValueExportProvider(new Dictionary<string, object>()));

            Assert.That(
                container.GetExportedValue<OrderRepository>().RetryCount,
                Is.EqualTo(123));
        }

        [Test]
        public void When_conversion_fails_due_to_a_format_error_then_the_error_message_explains_what_happened_and_where()
        {
            var exportProvider = new ConfigurationValueExportProvider(new Dictionary<string, object>
            {
                { "some_int", "some value that totally will not convert to an int" },
                { "some_date", DateTime.Now },
                { "some_string", DateTime.Now.ToString() },
                { "some_bool", false },
            });
            var container = new CompositionContainer(exportProvider);

            var ex = Assert.Throws<CompositionException>(() => container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic)));

            Console.WriteLine(ex.ToString());

            Assert.That(ex.InnerException, Is.InstanceOf<FormatException>());
            Assert.That(ex.Message, Is.StringContaining("some_int"));
            Assert.That(ex.Message, Is.StringContaining("Its.Configuration.Tests.SomethingConfigurableAndStatic.AnIntField"));
        }

        [Test]
        public void When_conversion_fails_due_to_an_invalid_cast_then_the_error_message_explains_what_happened_and_where()
        {
            var exportProvider = new ConfigurationValueExportProvider(new Dictionary<string, object>
            {
                { "some_int", "123" },
                { "some_date", new object() },
                { "some_string", DateTime.Now.ToString() },
                { "some_bool", false },
            });
            var container = new CompositionContainer(exportProvider);

            var ex = Assert.Throws<CompositionException>(() => container.SatisfyStaticImportsOnce(typeof (SomethingConfigurableAndStatic)));

            Assert.That(ex.InnerException, Is.InstanceOf<InvalidCastException>());
            Assert.That(ex.Message, Is.StringContaining("some_date"));
            Assert.That(ex.Message, Is.StringContaining("Its.Configuration.Tests.SomethingConfigurableAndStatic.ADateProperty"));
        }
    }
}