using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class TypeConversionTests
    {
        [Test]
        public void Nullable_byte_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<byte>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_byte_import_properties_can_be_set_to_non_null()
        {
            const byte b = 23;
            var result = GetExportedValue<byte>("23");
            Assert.That(result.Property, Is.EqualTo(b));
        }

        [Test]
        public void Nullable_bool_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<bool>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_bool_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<bool>("true");
            Assert.That(result.Property, Is.EqualTo(true));
        }

        [Test]
        public void Nullable_DateTime_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<DateTime>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_DateTime_import_properties_can_be_set_to_non_null()
        {
            string dateString = "12/12/12";
            var result = GetExportedValue<DateTime>(dateString);

            Assert.That(result.Property, Is.EqualTo(DateTime.Parse(dateString)));
        }

        [Test]
        public void Nullable_decimal_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<decimal>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_decimal_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<decimal>(decimal.MaxValue.ToString());
            Assert.That(result.Property, Is.EqualTo(decimal.MaxValue));
        }

        [Test]
        public void Nullable_double_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<double>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_double_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<double>("123.321");
            Assert.That(result.Property, Is.EqualTo(123.321));
        }

        [Test]
        public void Nullable_single_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<Single>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_single_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<Single>("123456789");
            Assert.That(result.Property, Is.EqualTo(123456789.0f));
        }

        [Test]
        public void Nullable_Int16_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<Int16>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_Int16_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<Int16>("15");
            Assert.That(result.Property, Is.EqualTo(15));
        }

        [Test]
        public void Nullable_Int32_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<Int32>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_Int32_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<Int32>("15");
            Assert.That(result.Property, Is.EqualTo(15));
        }

        [Test]
        public void Nullable_Int64_import_properties_can_be_set_to_null()
        {
            var result = GetExportedValue<Int64>();
            Assert.IsNull(result.NullableProperty);
        }

        [Test]
        public void Nullable_Int64_import_properties_can_be_set_to_non_null()
        {
            var result = GetExportedValue<Int64>(Int64.MaxValue.ToString());
            Assert.That(result.Property, Is.EqualTo(Int64.MaxValue));
        }

        [Test]
        public void Non_nullable_enum_members_can_be_set_to_a_value_based_on_name()
        {
            var result = GetExportedValue<SomeEnum>("Value1");
            Assert.That(result.Property == SomeEnum.Value1);
        }

        [Test]
        public void Nullable_enum_members_can_be_set_to_a_value_based_on_name()
        {
            var result = GetExportedValue<SomeEnum>("Value2");
            Assert.That(result.NullableProperty.Value == SomeEnum.Value2);
        }

        [Test]
        public void Nullable_enum_members_can_be_set_to_null()
        {
            var result = GetExportedValue<SomeEnum>();
            Assert.That(result.NullableProperty == null);
        }

        private static Type<T> GetExportedValue<T>(string property = null) where T : struct
        {
            var configSettings = new Dictionary<string, object>();
            if (property != null)
            {
                configSettings.Add("property", property);
            }
            var container = new CompositionContainer(
                new TypeCatalog(typeof (Type<T>)),
                new ConfigurationValueExportProvider(
                    configSettings));
            return container.GetExportedValue<Type<T>>();
        }

        [Export]
        public class Type<T> where T : struct
        {
            [Import("property", AllowDefault = true)]
            public T Property { get; set; }

            [Import("property", AllowDefault = true)]
            public T? NullableProperty { get; set; }
        }
    }
}