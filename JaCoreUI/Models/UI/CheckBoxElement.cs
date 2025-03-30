using CommunityToolkit.Mvvm.ComponentModel;
using JaCoreUI.Models.UI.Base;

namespace JaCoreUI.Models.UI
{
    /// <summary>
    /// Checkbox UI element
    /// </summary>
    public partial class CheckBoxElement : UIElement
    {
        /// <summary>
        /// Current checked state
        /// </summary>
        [ObservableProperty]
        public partial bool IsChecked { get; set; }

        /// <summary>
        /// Expected checked state for validation
        /// </summary>
        [ObservableProperty]
        public partial bool ExpectedValue { get; set; }

        /// <summary>
        /// Validates the current checked state
        /// </summary>
        public override bool Validate()
        {
            return !IsRequired || IsChecked == ExpectedValue;
        }

        /// <summary>
        /// Creates a deep clone of this checkbox element
        /// </summary>
        public override UIElement Clone()
        {
            return new CheckBoxElement
            {
                Name = Name,
                Description = Description,
                IsRequired = IsRequired,
                IsReadOnly = IsReadOnly,
                Label = Label,
                HelpText = HelpText,
                ExpectedValue = ExpectedValue
            };
        }
    }
}