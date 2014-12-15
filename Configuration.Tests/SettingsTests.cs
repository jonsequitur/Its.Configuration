// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Its.Recipes;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests
{
    public class SettingsTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.Reset();
            Environment.SetEnvironmentVariable("Its.Configuration.Settings.Precedence", null);
            Settings.CertificatePassword = null;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("Its.Configuration.Settings.Precedence", null);
        }

        [Test]
        public void Settings_Get_T_deserializes_its_Value_from_config_source()
        {
            var settings = Settings.Get<LogDbConnection>();

            settings.ConnectionString
                    .Should().Be("Data Source=(localdb)\\v11.0; Integrated Security=True; MultipleActiveResultSets=True");
        }

        [Test]
        public void Settings_Get_deserializes_its_Value_from_config_source()
        {
            dynamic settings = Settings.Get(typeof (LogDbConnection));

            string connectionString = settings.ConnectionString;

            connectionString
                .Should().Be("Data Source=(localdb)\\v11.0; Integrated Security=True; MultipleActiveResultSets=True");
        }

        [Test]
        public void Default_values_can_be_declared_in_the_class_and_are_not_overwritten_when_the_value_is_not_present_in_the_config_source()
        {
            var settings = Settings.Get<LogDbConnection>();

            settings.WriteRetries
                    .Should().Be(3);
        }

        [Test]
        public void Uris_are_supported()
        {
            Settings.For<Widget<Uri>>.GetSerializedSetting = key => new
            {
                TheSetting = new Uri("http://blammo.com")
            }.ToJson();

            var settings = Settings.Get<Widget<Uri>>();

            settings.TheSetting.ToString()
                    .Should().Be("http://blammo.com/");
        }

        [Test]
        public void Parseable_DateTime_formats_are_supported()
        {
            Settings.For<Widget<DateTime>>.GetSerializedSetting = key => new
            {
                TheSetting = "2/20/2013"
            }.ToJson();

            var settings = Settings.Get<Widget<DateTime>>();

            settings.TheSetting
                    .Should().Be(DateTime.Parse("2/20/2013"));
        }

        [Test]
        public void Parseable_DateTimeOffset_formats_are_supported()
        {
            Settings.For<Widget<DateTimeOffset>>.GetSerializedSetting = key => new
            {
                TheSetting = "2/20/2013"
            }.ToJson();

            var settings = Settings.Get<Widget<DateTimeOffset>>();

            settings.TheSetting
                    .Should().Be(DateTimeOffset.Parse("2/20/2013"));
        }

        [Test]
        public void Settings_are_looked_up_by_file_name_by_default_from_json_files_in_the_root_of_the_config_folder()
        {
            Settings.Precedence = null;

            var settings = Settings.Get<EnvironmentSettings>();

            settings.Name.Should().Be("local");
            settings.IsLocal.Should().BeTrue();
            settings.IsTest.Should().BeTrue();
        }

        [Test]
        public void Config_folder_selection_is_case_insensitive()
        {
            Settings.Precedence = new[] { "PRODUCTION" }; // the actual folder name is "production"

            var settings = Settings.Get<EnvironmentSettings>();

            settings.Name.Should().Be("production");
            settings.IsLocal.Should().BeFalse();
            settings.IsTest.Should().BeFalse();
        }

        [Test]
        public void When_an_order_of_precedence_is_specified_then_settings_are_looked_up_by_file_name_from_json_files_in_the_environment_folder_under_the_config_folder()
        {
            Settings.Precedence = new[] { "production" };

            var settings = Settings.Get<EnvironmentSettings>();

            settings.Name.Should().Be("production");
            settings.IsLocal.Should().BeFalse();
            settings.IsTest.Should().BeFalse();
        }

        [Test]
        public void GetFile_uses_stated_precedence()
        {
            var file = Settings.GetFile(f => f.Name == "EnvironmentSettings.json");
            file.DirectoryName.Should().EndWith("test");

            Settings.Reset();

            Settings.Precedence = new[] { "production" };
            file = Settings.GetFile(f => f.Name == "EnvironmentSettings.json");

            file.DirectoryName.Should().EndWith("production");
        }

        [Test]
        public void When_an_order_of_precedence_is_specified_then_each_folder_is_consulted_in_order_and_the_first_hit_wins()
        {
            Settings.Precedence = new[] { "test", "production" };

            var settings = Settings.Get<EnvironmentSettings>();

            settings.Name.Should().Be("test");
            settings.IsLocal.Should().BeFalse();
            settings.IsTest.Should().BeTrue();
        }

        [Test]
        public void When_an_order_of_precedence_is_specified_then_nonexistent_subfolders_are_ignored()
        {
            Settings.Precedence = new[] { "nonexistent", "test" };

            var settings = Settings.Get<EnvironmentSettings>();

            settings.Name.Should().Be("test");
            settings.IsLocal.Should().BeFalse();
            settings.IsTest.Should().BeTrue();
        }

        [Test]
        public void When_an_order_of_precedence_is_specified_then_setting_files_in_the_root_are_used_if_not_found_in_a_subfolder()
        {
            Settings.Precedence = new[] { "production" };

            var settings = Settings.Get<OnlyConfiguredInRootFolder>();

            settings.Value.Should().Be("correct");
        }

        [Test]
        public void Trace_output_indicates_the_resolved_source_for_the_settings()
        {
            string log = "";
            Settings.Precedence = new[] { "nonexistent", "test", "production" };
            var listener = new TestTraceListener();
            listener.OnTrace += (Action<string>) (s => log += s);
            Trace.Listeners.Add(listener);

            using (new AnonymousDisposable(() => Trace.Listeners.Remove(listener)))
            {
                Settings.Get<EnvironmentSettings>();
            }

            log.Should().Contain(string.Format("Resolved setting 'EnvironmentSettings' from settings folder ({0})", Path.Combine(Deployment.Directory, ".config", "test")));
        }

        [Test]
        public void Settings_are_looked_up_from_environment_variables()
        {
            var name = Guid.NewGuid().ToString();
            var json = new EnvironmentVariableTestSettings
            {
                Name = name
            }.ToJson();
            Environment.SetEnvironmentVariable("EnvironmentVariableTestSettings", json);

            var settings = Settings.Get<EnvironmentVariableTestSettings>();

            settings.Name.Should().Be(name);
        }

        [Test]
        public void Settings_Get_returns_a_default_instance_if_no_settings_are_found()
        {
            var settings = Settings.Get<UnconfiguredSettings>();

            settings.Value.Should().Be(new UnconfiguredSettings().Value);
        }

        [Test]
        public void The_first_settings_source_to_return_a_non_null_or_empty_value_is_used()
        {
            Settings.Sources = new[]
            {
                Settings.CreateSource(key => null),
                Settings.CreateSource(key => ""),
                Settings.CreateSource(key => new { WriteRetries = 100 }.ToJson()),
                Settings.CreateSource(key => new { WriteRetries = 200 }.ToJson())
            };

            var settings = Settings.Get<LogDbConnection>();

            settings.WriteRetries.Should().Be(100);
        }

        [Test]
        public void Settings_sources_later_in_the_sequence_than_the_first_to_return_a_value_are_ignored()
        {
            var secondSourceWasCalled = false;
            Settings.Sources = new[]
            {
                Settings.CreateSource(key => new { WriteRetries = 100 }.ToJson()),
                Settings.CreateSource(key =>
                {
                    secondSourceWasCalled = true;
                    return new { WriteRetries = 200 }.ToJson();
                })
            };

            secondSourceWasCalled.Should().BeFalse();
        }

        [Test]
        public void Single_parameter_generic_settings_types_use_a_known_convention_to_look_up_corresponding_settings()
        {
            string requestKey = null;
            Settings.For<Widget<SettingsTests>>.GetSerializedSetting = key =>
            {
                requestKey = key;
                return null;
            };

            Settings.Get<Widget<SettingsTests>>();

            requestKey.Should().Be("Widget(SettingsTests)");
        }

        [Test]
        public void Multiple_parameter_generic_settings_types_use_a_known_convention_to_look_up_corresponding_settings()
        {
            string requestKey = null;
            Settings.For<Dictionary<string, Widget<int>>>.GetSerializedSetting = key =>
            {
                requestKey = key;
                return null;
            };

            Settings.Get<Dictionary<string, Widget<int>>>();

            requestKey.Should().Be("Dictionary(String,Widget(Int32))");
        }

        [Test]
        public void Abstract_settings_indicate_a_type_redirect_via_AppSettings()
        {
            var settings = Settings.Get<AbstractSettings>();

            settings.Value.Should().Be("found me!"); // configured in app.config
        }

        [NUnit.Framework.Ignore("Scenario under development")]
        [Test]
        public void SettingsBase_maps_its_own_properties_from_serialized_values_on_construction()
        {
            // TODO: (SettingsBase_maps_its_own_properties_from_serialized_values_on_construction) 
            Settings.GetSerializedSetting = _ => new
            {
                String = "hello",
                Int = 5
            }.ToJson();

            Assert.Fail("Broken: ctor never completes");

            var settings = new DerivedFromSettingsBase();

            settings.String.Should().Be("hello");
            settings.Int.Should().Be(5);
        }

        [Test]
        public void Configuration_folder_location_can_be_specified()
        {
            Settings.SettingsDirectory = Path.Combine(Deployment.Directory, ".alternateConfig");

            Settings.Get<EnvironmentSettings>().Name.Should().Be("alternate");
        }

        [Test]
        public void Settings_are_only_looked_up_and_deserializd_once_per_type()
        {
            int deserializeCount = 0;
            int lookupCount = 0;

            Settings.Sources = new[]
            {
                Settings.CreateSource(key =>
                {
                    lookupCount++;
                    return "_";
                })
            };

            Settings.Deserialize = (type, json) =>
            {
                deserializeCount++;
                return new object();
            };

            Settings.Get<object>();
            Settings.Get<object>();
            Settings.Get<object>();

            deserializeCount.Should().Be(1);
            lookupCount.Should().Be(1);
        }

        [Test]
        public void Settings_precedence_set_via_an_environment_variable_overrides_app_config()
        {
            // default behavior check...
            var settings = Settings.Get<EnvironmentSettings>();
            settings.Name.Should().Be("test");

            Settings.Reset();

            Environment.SetEnvironmentVariable("Its.Configuration.Settings.Precedence", "production");

            settings = Settings.Get<EnvironmentSettings>();

            settings.Name.Should().Be("production");
        }

        public class Credentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class LogDbConnection
        {
            public LogDbConnection()
            {
                WriteRetries = 3;
            }

            public virtual string ConnectionString { get; set; }
            public virtual int WriteRetries { get; set; }
        }

        public class Widget<T>
        {
            public T TheSetting { get; set; }
        }

        public class EnvironmentVariableTestSettings
        {
            public string Name { get; set; }
        }

        public class UnconfiguredSettings
        {
            public UnconfiguredSettings()
            {
                Value = 42;
            }

            public int Value { get; set; }
        }

        internal class DerivedFromSettingsBase : SettingsBase
        {
            public string String { get; set; }
            public int Int { get; set; }
        }

        internal abstract class SettingsBase
        {
            protected SettingsBase()
            {
                var myType = GetType();
                var serialized = Settings.GetSerializedSetting(myType.Name);

                var mapperType = typeof (MappingExpression.From<>).MakeGenericType(myType);

                var map = mapperType
                    .GetMethod("ToExisting")
                    .MakeGenericMethod(myType).Invoke(null, null);

                var deserialized = Settings.Deserialize(myType, serialized);

                ((dynamic) map)(deserialized, this);
            }
        }

        public class OnlyConfiguredInRootFolder
        {
            public string Value { get; set; }
        }

        public abstract class AbstractSettings
        {
            public string Value { get; set; }
        }

        public class DerivedFromAbstractSettings : AbstractSettings
        {
        }
    
        public class TestTraceListener : TraceListener
        {
            public event Action<string> OnTrace;

            public override void Write(string message)
            {
                WriteLine(message);
            }

            public override void WriteLine(string message)
            {
                var handler = OnTrace;
                if (handler != null)
                {
                    handler( message);
                }
            }


        }
    }

    
}