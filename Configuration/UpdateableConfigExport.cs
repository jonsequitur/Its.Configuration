using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace Its.Configuration
{
    internal class UpdateableConfigExport : Export
    {
        // HACK: This field is used as a reference by the base Export class to signal whether an exported value has been calculated yet. Since we might change value of the export, we'll need this to tell the base class to re-initialize.
        private static readonly object _EmptyValue = typeof (Export)
                .GetField("_EmptyValue", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
        // This is the field that needs to be reset to _EmptyValue when Update is called.
        private static readonly FieldInfo exportedValueField = typeof (Export)
                .GetField("_exportedValue", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly Func<object, object> convert;
        private readonly Dictionary<string, Func<string, object>> typeConverters;
        private object value;
      

        public UpdateableConfigExport(string contractName, object initialValue, Func<object, object> convert, Dictionary<string, Func<string, object>> typeConverters) : base(contractName, () => convert(initialValue))
        {
            this.convert = convert;
            this.typeConverters = typeConverters;
        }

        public void Update(string newValue)
        {
            if (value != null)
            {
                // the value could be a T or an IObserver<T>. we need to decide which in order to decide how to update it.
                var type = value.GetType();

                var observerInterface = type.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof (IObserver<>));

                if (observerInterface != null)
                {
                    // if it's an IObserver, we update it by calling OnNext...

                    // get the generic parameter type and find the conversion to that type, e.g. if this Export exports IObservable<bool>, we need to find the export for bool.
                    var observableGenericParam = observerInterface.GetGenericArguments().First();
                    Func<string, object> convertToObservableGenericParamType;
                    if (!typeConverters.TryGetValue(
                        observableGenericParam.Key(),
                        out convertToObservableGenericParamType))
                    {
                        // if a custom one wasn't defined, default to:
                        convertToObservableGenericParamType = s => Convert.ChangeType(s, observableGenericParam);
                    }

                    var correctlyTypedNewValue = convertToObservableGenericParamType(newValue);

                    // call OnNext on the observable
                    ((dynamic) value).OnNext(((dynamic) correctlyTypedNewValue));
                    return;
                }
            }

            // if the value was not an IObserver, we can update it directly

            // update the value directly if this was not an observable
            value = convert(newValue);
            
            // reset the base class so that the cached previous value is not reused.
            exportedValueField.SetValue(this, _EmptyValue);
        }

        protected override object GetExportedValueCore()
        {
            return value ?? (value = base.GetExportedValueCore());
        }

        public UpdateableConfigExport Clone()
        {
            return new UpdateableConfigExport(Definition.ContractName, value, convert, typeConverters);
        }
    }
}