using Avalonia.Data;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.Material;

namespace HASS.Agent.Client.MarkupExtensions;

public class IconExtension
{
    private readonly BindingBase _valueBinding;
    
    public PackIconMaterialKind TrueIcon { get; set; } = PackIconMaterialKind.Circle;
    public PackIconMaterialKind FalseIcon { get; set; } = PackIconMaterialKind.Square;

    public IconExtension(BindingBase valueBindingBase)
    {
        _valueBinding = valueBindingBase;
    }
    
    public object ProvideValue()
    {
        _valueBinding.Converter = new FuncValueConverter<bool, PackIconMaterialKind>(b => b ? TrueIcon : FalseIcon);
        return _valueBinding;
    }
}