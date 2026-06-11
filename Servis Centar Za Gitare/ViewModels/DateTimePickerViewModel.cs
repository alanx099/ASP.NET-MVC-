using System;
using System.Globalization;

namespace Servis_Centar_Za_Gitare.ViewModels
{
    public class DateTimePickerViewModel
    {
        public DateTimePickerViewModel(
            string name,
            string label,
            DateTime? value = null,
            bool required = true,
            int? maxFutureDays = null,
            string? maxFutureMessage = null,
            string? notBeforeFieldName = null,
            string? notBeforeMessage = null)
        {
            Name = name;
            Label = label;
            Value = value.HasValue ? FormatDateTime(value.Value) : string.Empty;
            Required = required;
            MaxFutureDays = maxFutureDays;
            MaxFutureMessage = maxFutureMessage;
            NotBeforeFieldName = notBeforeFieldName;
            NotBeforeMessage = notBeforeMessage;
        }

        public DateTimePickerViewModel(
            string name,
            string label,
            string? value,
            bool required = true,
            int? maxFutureDays = null,
            string? maxFutureMessage = null,
            string? notBeforeFieldName = null,
            string? notBeforeMessage = null)
        {
            Name = name;
            Label = label;
            Value = NormalizeStringValue(value);
            Required = required;
            MaxFutureDays = maxFutureDays;
            MaxFutureMessage = maxFutureMessage;
            NotBeforeFieldName = notBeforeFieldName;
            NotBeforeMessage = notBeforeMessage;
        }

        public string Name { get; }

        public string Label { get; }

        public string Value { get; }

        public bool Required { get; }

        public int? MaxFutureDays { get; }

        public string? MaxFutureMessage { get; }

        public string? NotBeforeFieldName { get; }

        public string? NotBeforeMessage { get; }

        public string Id => Name.Replace(".", "_");

        public string? NotBeforeFieldId => NotBeforeFieldName?.Replace(".", "_");

        private static string FormatDateTime(DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static string NormalizeStringValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed) ||
                DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
            {
                return FormatDateTime(parsed);
            }

            return value;
        }
    }

    public class AutocompleteComboViewModel
    {
        public AutocompleteComboViewModel(string name, string label, string endpoint, long? value = null, string? displayValue = null, string? describedBy = null)
        {
            Name = name;
            Label = label;
            Endpoint = endpoint;
            Value = value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            DisplayValue = displayValue ?? string.Empty;
            DescribedBy = describedBy ?? name + "-error";
        }

        public string Name { get; }
        public string Label { get; }
        public string Endpoint { get; }
        public string Value { get; }
        public string DisplayValue { get; }
        public string DescribedBy { get; }
        public string? CustomerField { get; set; }
        public string? GuitarField { get; set; }
        public string? RepairTypeField { get; set; }
        public string Placeholder { get; set; } = "Search...";
        public string Id => Name.Replace(".", "_");
    }
}
